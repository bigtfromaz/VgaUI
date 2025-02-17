using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using System.Text;

public class PurseSettings
{
    [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
    public string? Id { get; set; }
    [BsonElement]
    public string? DocID { get; set;  } = System.Guid.NewGuid().ToString();
    [BsonElement]
    public string Name { get; set; } = "Default";
    [BsonElement]
    public List<int> PointsPerPlace = new() { 5, 3, 1 };
    [BsonElement]
    public decimal CTPContributionAmount { get; set; } = 1;
    [BsonElement]
    public decimal LowNetContributionAmount { get; set; } = 8;
    [BsonElement]
    public decimal BirdieContributionAmount { get; set; } = 5;
    [BsonElement]
    public decimal GuestFeeToClub { get; set; } = 4;
    [BsonElement]
    public decimal RoundFeeToClub { get; set; } = 0;
    [BsonElement]
    public int LargeFlightThreshold { get; set; } = 30;
    [BsonElement]
    public List<double> LargeFlightPlace = new() { .5, .3 };
    [BsonElement]
    public List<double> SmallFlightPlace = new() { .5, .3 };
    public static PurseSettings GetOrCreateDefaultPurseSettingsMongoDB(IMongoCollection<PurseSettings> collection, PurseSettings _settings)
    {
        PurseSettings settingsBack;
//        if (dbClient == null) throw new Exception($"Mongo DB Client cannot be null when calling GetOrCreateDefaultPurseSettingsMongoDB");
        //IMongoCollection<PurseSettings> collection = vgadb.GetCollection<PurseSettings>("PurseSettings");
        long numSettingsNamedDefault = collection.CountDocuments<PurseSettings>(x => x.Name == _settings.Name);
        //var deleteResult = collection.DeleteMany<PurseSettings>(x => x.Name == _settings.Name);
        //Console.WriteLine($"Deleted {deleteResult.DeletedCount} PurseSettings where Name = {_settings.Name}");
        switch (numSettingsNamedDefault)
        {
            case 1:
                settingsBack = collection.Find<PurseSettings>(x => x.Name == _settings.Name).FirstOrDefault();
                return settingsBack;
            case 0:
                collection.InsertOne(_settings);
                settingsBack = collection.Find<PurseSettings>(x => x.Name == _settings.Name).FirstOrDefault();
                return settingsBack;
            default:
                throw new Exception($"The number of PurseSettings having the name \"{_settings.Name}\" should be 0 or 1.  There are {numSettingsNamedDefault}.");
        }
    }
}
