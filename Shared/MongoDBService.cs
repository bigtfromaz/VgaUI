namespace VgaUI.Shared;

using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Microsoft.Extensions.Logging;

public class MongoDBService
{

    private readonly IMongoCollection<PurseSettings> _PurseSettingsCollection;
    private readonly IMongoCollection<RoundResults> _RoundResultsCollection;
    private readonly ILogger<MongoDBService> _logger;
    //private readonly JsonSerializerOptions serializerOptions = new()

    //{
    //    IncludeFields = true,
    //    WriteIndented = true
    //};

    public MongoDBService(IOptions<MongoDBSettings> mongoDBSettings, ILogger<MongoDBService> logger)
    {
        MongoClient client = new(mongoDBSettings.Value.ConnectionURI);
        IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
        _PurseSettingsCollection = database.GetCollection<PurseSettings>(mongoDBSettings.Value.PurseCollectionName);
        _RoundResultsCollection = database.GetCollection<RoundResults>(mongoDBSettings.Value.RoundResultsCollectionName);
        _logger = logger;
    }
    #region PurseSettings
    public async Task<PurseSettings> GetPurseSettingsAsync(string name)
    {
        var filter = Builders<PurseSettings>.Filter.Eq("Name", name);
        IAsyncCursor<PurseSettings> settingsBack;
        long numSettingsNamedDefault = _PurseSettingsCollection.CountDocuments<PurseSettings>(x => x.Name == name);
        switch (numSettingsNamedDefault)
        {
            case 1:
                settingsBack = await _PurseSettingsCollection.FindAsync(x => x.Name == name);
                return settingsBack.FirstOrDefault();
            case 0:
                await _PurseSettingsCollection.InsertOneAsync(new PurseSettings());
                settingsBack = await _PurseSettingsCollection.FindAsync(x => x.Name == name);
                return settingsBack.FirstOrDefault();
            default:
                throw new Exception($"The number of PurseSettings having the name \"{name}\" should be 0 or 1.  There are {numSettingsNamedDefault}.");
        }
    }
    public async Task StorePurseSettingsAsync(PurseSettings purseSettings)
    {
        long numSettingsNamedDefault = _PurseSettingsCollection.CountDocuments<PurseSettings>(x => x.Name == purseSettings.Name);
        if (numSettingsNamedDefault > 0)
        { // Delete all settings objects whose name matches the one we're about to store
            var filter = Builders<PurseSettings>.Filter.Eq(settings => settings.Name, purseSettings.Name);
            await _PurseSettingsCollection.DeleteManyAsync(filter);
        }
        await _PurseSettingsCollection.InsertOneAsync(new PurseSettings());
    }
    public async Task DeletePurseSettingsAsync(string id)
    {
        await _PurseSettingsCollection.DeleteOneAsync(id);
    }
    #endregion
    #region Rounds
    public async Task<RoundResults> GetRoundResultsAsync(string id)
    {
        var filter = Builders<RoundResults>.Filter.Eq("Id", id);
        IAsyncCursor<RoundResults> settingsBack;
        settingsBack = await _RoundResultsCollection.FindAsync(x => x.Id == id);
        //            var x = BsonExtensionMethods.ToJson(settingsBack.FirstOrDefaultAsync());  
        var response = settingsBack.FirstOrDefault();
        return response;
    }
    public async Task<RoundResults> GetLastRoundResultsAsync()
    {
        string id = _RoundResultsCollection.AsQueryable().Max(x => x.Id) ?? throw new Exception($"Error finding the latest RoundResults ID. Is the database empty?");
        //            RoundResults? xxx = await _RoundResultsCollection.AsQueryable().Where(x => x.Id == id).FirstOrDefaultAsync();
        var filter = Builders<RoundResults>.Filter.Eq("Id", id);
        var xxx = await _RoundResultsCollection.FindAsync<RoundResults>(filter);
        return xxx.FirstOrDefault();
    }
    public async Task<RoundResults> AddOrReplaceRoundResultsAsync(RoundResults roundResults)
    {
        try
        {
            string? idFromDB = await GetID(roundResults.CourseName, roundResults.DateOfPlay);

            if (idFromDB == null && roundResults.Id == null) // If the course and date are not in the DB then insert it
            {
#pragma warning disable CA2254 // Template should be a static expression
                _logger.LogInformation($"Inserting RoundResult Course: {roundResults.CourseName} {roundResults.DateOfPlay}");
                await _RoundResultsCollection.InsertOneAsync(roundResults);
                _logger.LogInformation($"Inserted RoundResult Id: {roundResults.Id}");
            }
            else // The course and date are in the database
            {
                if (roundResults.Id == null)
                {
                    roundResults.Id = idFromDB;
                    if (roundResults.Id != idFromDB)
                    {
                        throw new Exception($"The document you submitted with ID={roundResults.Id ?? "null"} has the same course name and date as this ID:{idFromDB ?? "null"}");
                    }
                }

                _logger.LogInformation($"Replacing RoundResult Id: {roundResults.Id}");
                await _RoundResultsCollection.ReplaceOneAsync<RoundResults>(x => x.Id == roundResults.Id, roundResults);
                _logger.LogInformation($"Replaced RoundResult Id: {roundResults.Id}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error Inserting RoundResult Id: {roundResults.Id} Message is: {ex.Message} \n Stack Trace is: {ex.StackTrace}");
            throw;
        }

        return roundResults;
    }

    public async Task<string?> GetID(string CourseName, DateOnly DateOfPlay)
    {
        try
        {
            _logger.LogInformation($"Looking for Course: {CourseName} {DateOfPlay}");

            string combinedUniqueKey = Helpers.GetCombinedUniqueKey(DateOfPlay, CourseName);

            var filter = Builders<RoundResults>.Filter.Eq("CombinedUniqueKey", combinedUniqueKey);
            //                       & Builders<RoundResults>.Filter.Eq("DateOfPlay", DateOfPlay);

            string? id = null;
            IAsyncCursor<RoundResults> cursor = await _RoundResultsCollection.FindAsync<RoundResults>(filter);
            var resultsList = await cursor.ToListAsync();
            if (resultsList.Count > 0)
            {
                id = resultsList[0].Id;
            }
            return id;
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"GetID Error looking for Course: {CourseName} {DateOfPlay}; Message is: {ex.Message} \n Stack Trace is: {ex.StackTrace}");
            throw;
        }

    }
    public async Task DeleteRoundResultsAsync(string id)
    {
        var filter = Builders<RoundResults>.Filter.Eq(document => document.Id, id);
        await _RoundResultsCollection.DeleteOneAsync(filter);
    }
    public async Task<List<RoundInfo>> GetRoundList(int limit = 20)
    {
        List<RoundInfo> result = await _RoundResultsCollection.AsQueryable()
            .OrderByDescending(doc => doc.CombinedUniqueKey)
            .Select(doc => new RoundInfo
            {
                Id = doc.Id,
                CourseName = doc.CourseName,
                DateOfPlay = doc.DateOfPlay
            })
            .Take(limit)
            .ToListAsync<RoundInfo>();
        return result;
    }
    public async Task<List<RoundInfo>> GetRoundListOfficial(int limit = 20)
    {
        List<RoundInfo> result = await _RoundResultsCollection.AsQueryable()
            .OrderByDescending(doc => doc.CombinedUniqueKey)
            .Where(doc => doc.IsOfficial == true)
            .Select(doc => new RoundInfo
            {
                Id = doc.Id,
                CourseName = doc.CourseName,
                DateOfPlay = doc.DateOfPlay
            })
            .Take(limit)
            .ToListAsync<RoundInfo>();
        return result;
    }
    #endregion
    #region stats
    public async Task<List<RoundResults>> GetRoundResultsRangeAsync(DateOnly from, DateOnly to)
    {
        List<RoundResults> result = await _RoundResultsCollection.AsQueryable()
            .OrderByDescending(doc => doc.CombinedUniqueKey)
            .Where(dates => dates.DateOfPlay >= from && dates.DateOfPlay <= to)
            .ToListAsync<RoundResults>();
        return result;
    }
    public async Task<List<RoundResults>> GetAnnualRoundResultsAsync(int year)
    {
        DateOnly firstDayOfYear = new(year, 1, 1);
        DateOnly lastDayOfYear = new(year, 12, 31);
        return await GetRoundResultsRangeAsync(firstDayOfYear, lastDayOfYear);
    }

    public async Task<List<PlayerStats>> GetPlayerStatsAsync(int year)
    {
        DateOnly firstDayOfYear = new(year, 1, 1);
        DateOnly lastDayOfYear = new(year, 12, 31);
        var stats = await Task.Run(() => new VGAStats(_RoundResultsCollection, firstDayOfYear, lastDayOfYear));

        return stats.ByPlayer;
    }


    #endregion

    /*
*/
}