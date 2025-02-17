using ExcelInterface;
using Microsoft.Extensions.Configuration;
using System;
using System.Runtime;
using System.Text.Json;
using MongoDB.Driver;
using VgaUI.Shared;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System.Diagnostics;
using ZstdSharp.Unsafe;

namespace Tests
{
    [TestClass]
    public class ExcelTests
    {
        private const string ONEFLIGHT = ".\\Spreadsheets\\GCU Incomplete Leaderboard.xls";
        private const string TWOFLIGHTS = ".\\Spreadsheets\\Estrella Complete Leaderboard.xls";
        private const string UNIONHILLS = ".\\Spreadsheets\\UnionHills.xls";
        private const string WIGWAM = ".\\Spreadsheets\\Wigwam.xls";
        private const string DOBSON = ".\\Spreadsheets\\Dobson Ranch.xls";
        private const string TIE = ".\\Spreadsheets\\VGA Aguila Tie.xls";
        private readonly JsonSerializerOptions serializerOptions = new()
        {
            IncludeFields = true,
            WriteIndented = true
        };
        private readonly PurseSettings _settings;
        private readonly IConfigurationRoot config;
        private readonly MongoClient dbClient = new();
        private readonly IMongoDatabase vgadb;
        public ExcelTests()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            _settings = new();
            config = new ConfigurationBuilder().AddUserSecrets<ExcelTests>().Build();

            if ((dbClient = new MongoClient(config.GetSection("mongoConnectionString").Value)) != null)
            {
                vgadb = dbClient.GetDatabase("VGADB") ?? throw new Exception($"Error getting the VGADB database. Wrong connection string?");
                //var dblist = dbClient.ListDatabases().ToList();
                _settings = GetOrCreateDefaultPurseSettingsMongoDB();
            }
            else throw new Exception($"Could not locate a connection string?");
        }

        public void CreateDefaultSettings()
        {
            var settings = new PurseSettings();
            settings.LargeFlightPlace.Add(.5);
            settings.LargeFlightPlace.Add(.3);
            settings.SmallFlightPlace.Add(.5);
            settings.SmallFlightPlace.Add(.3);
            string jsonString = JsonSerializer.Serialize(settings, serializerOptions);
            Console.WriteLine(jsonString);
        }
        [TestMethod]
        public void ShowDefaultSettings()
        {
            PurseSettings settings = new();
            string jsonString = JsonSerializer.Serialize(settings, serializerOptions);
            Console.WriteLine(jsonString);
        }
        public PurseSettings GetOrCreateDefaultPurseSettingsMongoDB()
        {
            PurseSettings settingsBack;
            if (dbClient == null) throw new Exception($"Mongo DB Client cannot be null when calling GetOrCreateDefaultPurseSettingsMongoDB");
            IMongoCollection<PurseSettings> collection = vgadb.GetCollection<PurseSettings>("PurseSettings");
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
        [TestMethod]
        public void LoadRosterSpreadsheet()
        {
            VGA.ExcelInterface spreadSheet;
            spreadSheet = new VGA.ExcelInterface(_settings);
            spreadSheet.LeaderBoardData.CurrentPurseSettings = _settings;
//            spreadSheet.LeaderBoardData.SetNumCTPs(6);
            spreadSheet.LoadRoster(".\\Spreadsheets\\VGA Sample Master Roster.xlsx");

            string jsonString = JsonSerializer.Serialize(spreadSheet._members, serializerOptions);
            Console.WriteLine(jsonString);
        }

        [TestMethod]
        public void LoadLeaderboard1Flight()
        {
            VGA.ExcelInterface spreadSheet;
            spreadSheet = new VGA.ExcelInterface(_settings);
            spreadSheet.LeaderBoardData.CurrentPurseSettings = _settings;
            //spreadSheet.LeaderBoardData.SetNumCTPs(4);
            spreadSheet.LoadLeaderboard(ONEFLIGHT);

            Console.WriteLine($"ResultsSummary\n==============\n{spreadSheet.LeaderBoardData.CalculateResults()}");

            string jsonString = JsonSerializer.Serialize(spreadSheet.LeaderBoardData, serializerOptions);
            Console.WriteLine(jsonString);
        }

        [TestMethod]
        public void LoadLeaderboard2Flights()
        {
            VGA.ExcelInterface spreadSheet;
            spreadSheet = new VGA.ExcelInterface(_settings);
            spreadSheet.LeaderBoardData.CurrentPurseSettings = _settings;
//            spreadSheet.LeaderBoardData.SetNumCTPs(4);
            spreadSheet.LoadLeaderboard(TWOFLIGHTS);

            Console.WriteLine($"ResultsSummary\n==============\n{spreadSheet.LeaderBoardData.CalculateResults()}");

            string jsonString = JsonSerializer.Serialize(spreadSheet.LeaderBoardData, serializerOptions);
            Console.WriteLine(jsonString);
        }
        [TestMethod]
        public void UnionHills()
        {
            VGA.ExcelInterface spreadSheet;
            spreadSheet = new VGA.ExcelInterface(_settings);
            spreadSheet.LeaderBoardData.CurrentPurseSettings = _settings;
//            spreadSheet.LeaderBoardData.SetNumCTPs(4);


            spreadSheet.LoadLeaderboard(UNIONHILLS);

            Console.WriteLine($"ResultsSummary\n==============\n{spreadSheet.LeaderBoardData.CalculateResults()}");

            string jsonString = JsonSerializer.Serialize(spreadSheet.LeaderBoardData, serializerOptions);
            Console.WriteLine(jsonString);
        }
        [TestMethod]
        public void Wigwam()
        {
            VGA.ExcelInterface spreadSheet;
            spreadSheet = new VGA.ExcelInterface(_settings);
            spreadSheet.LeaderBoardData.CurrentPurseSettings = _settings;
            //            spreadSheet.LeaderBoardData.SetNumCTPs(6);


            spreadSheet.LoadLeaderboard(WIGWAM);
            spreadSheet.LeaderBoardData.CourseName = "Wigwam";
            spreadSheet.LeaderBoardData.DateOfPlay = DateOnly.FromDateTime(DateTime.Now);

            //Console.WriteLine($"ResultsSummary\n==============\n{spreadSheet.LeaderBoardData.CalculateResults()}");

            IMongoCollection<RoundResults> resultsCollection = vgadb.GetCollection<RoundResults>("RoundResults");
            _ = spreadSheet.LeaderBoardData.CalculateResults();
            resultsCollection.InsertOne(spreadSheet.LeaderBoardData);
            var id = spreadSheet.LeaderBoardData.Id;

            var docID = spreadSheet.LeaderBoardData.DocID ?? throw new Exception("Bad DocID returned from DB");

            var backCollection = vgadb.GetCollection<RoundResults>("RoundResults");

            //var filter = Builders<LeaderBoardData>.Filter.Eq("DocID", docID);
            var filter = Builders<RoundResults>.Filter.Eq("Id", id);

            var resultsBack = backCollection.Find(filter).FirstOrDefault();
            _ = resultsBack.CalculateResults();

            var resultsAsLeaderBoardData = resultsBack;

            string json = BsonExtensionMethods.ToJson<RoundResults>(resultsBack, new MongoDB.Bson.IO.JsonWriterSettings
            {
                Indent = true

            });

            Console.WriteLine(json);

        }
        [TestMethod]
        public void Tie()
        {
            VGA.ExcelInterface spreadSheet;
            spreadSheet = new VGA.ExcelInterface(_settings);
            spreadSheet.LeaderBoardData.CurrentPurseSettings = _settings;
            //            spreadSheet.LeaderBoardData.SetNumCTPs(6);


            try
            {
                spreadSheet.LoadLeaderboard(TIE);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading the {TIE} spreadsheet: {ex.Message}");
                throw;
            }
            spreadSheet.LeaderBoardData.CourseName = "Wigwam Tie";
            spreadSheet.LeaderBoardData.DateOfPlay = DateOnly.FromDateTime(DateTime.Now);
            //write spreadSheet.Leaderboard to the console in JSON format

            _ = spreadSheet.LeaderBoardData.CalculateResults();
            Console.WriteLine(JsonSerializer.Serialize(spreadSheet.LeaderBoardData, serializerOptions));
        }
        [TestMethod]
        public void DobsonRanch()
        {
            VGA.ExcelInterface spreadSheet;
            spreadSheet = new VGA.ExcelInterface(_settings);
            spreadSheet.LeaderBoardData.CurrentPurseSettings = _settings;
//            spreadSheet.LeaderBoardData.SetNumCTPs(6);


            spreadSheet.LoadLeaderboard(DOBSON);
            spreadSheet.LeaderBoardData.CourseName = "Dobson Ranch";
            spreadSheet.LeaderBoardData.DateOfPlay = DateOnly.FromDateTime(DateTime.Now);

            //Console.WriteLine($"ResultsSummary\n==============\n{spreadSheet.LeaderBoardData.CalculateResults()}");

            IMongoCollection<RoundResults> resultsCollection = vgadb.GetCollection<RoundResults>("RoundResults");
            _ = spreadSheet.LeaderBoardData.CalculateResults();
            resultsCollection.InsertOne(spreadSheet.LeaderBoardData);
            var id = spreadSheet.LeaderBoardData.Id;

            var docID = spreadSheet.LeaderBoardData.DocID ?? throw new Exception("Bad DocID returned from DB");

            var backCollection = vgadb.GetCollection<RoundResults>("RoundResults");

            //var filter = Builders<LeaderBoardData>.Filter.Eq("DocID", docID);
            var filter = Builders<RoundResults>.Filter.Eq("Id", id);

            var resultsBack = backCollection.Find(filter).FirstOrDefault();
            _ = resultsBack.CalculateResults();

            var resultsAsLeaderBoardData = resultsBack;

            string json = BsonExtensionMethods.ToJson<RoundResults>(resultsBack, new MongoDB.Bson.IO.JsonWriterSettings
            {
                Indent = true

            });

            Console.WriteLine(json);

        }
        [TestMethod]
        public void Sandbox()
        {
            var backCollection = vgadb.GetCollection<BsonDocument>("RoundResults");
            var typedCollection = vgadb.GetCollection<RoundResults>("RoundResults");


            // Get first document
            BsonDocument resultsBack = backCollection.Find(new BsonDocument()).FirstOrDefault();

            List<RoundInfo> result = typedCollection.AsQueryable()
                    .OrderByDescending(doc => doc.CombinedUniqueKey)
                    .Select(doc => new RoundInfo
                    {
                        Id = doc.Id,
                        CourseName = doc.CourseName,
                        DateOfPlay = doc.DateOfPlay
                    })
                    .Take(20)
                    .ToList<RoundInfo>();

            IAsyncCursor<BsonDocument> indexes = backCollection.Indexes.List();

            foreach (var item in indexes.ToList())
            {
                Debug.WriteLine(item.ToString()); // or perform any operations with the index information
            };


            string id = (string)resultsBack["_id"];

            string json = BsonExtensionMethods.ToJson<BsonDocument>(resultsBack,
                new MongoDB.Bson.IO.JsonWriterSettings
                {
                    Indent = true
                });

            File.WriteAllText("D:\\temp\\round.json", json);

            //var objectIdToDelete = ObjectId.Parse(id);
            //var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
            //backCollection.DeleteOneAsync(filter);
            //backCollection.DeleteManyAsync(Builders<BsonDocument>.Filter.Empty);

        }
        [TestMethod]
        public void PutOneBack()
        {
            Debug.WriteLine(DateOnly.FromDateTime(DateTime.Now).ToString());
            var backCollection = vgadb.GetCollection<BsonDocument>("RoundResults");
            IAsyncCursor<BsonDocument> indexes = backCollection.Indexes.List();

            foreach (var item in indexes.ToList())
            {
                Debug.WriteLine(item.ToString()); // or perform any operations with the index information
            };
            // Read the document
            BsonDocument resultsBack = BsonDocument.Parse(File.ReadAllText("D:\\temp\\BearCreek.json"));

            backCollection.InsertOne(resultsBack);
        }
        [TestMethod]
        public void CreateIndexes()
        {
            var backCollection = vgadb.GetCollection<RoundResults>("RoundResults");
            IAsyncCursor<BsonDocument> indexes = backCollection.Indexes.List();


            foreach (BsonDocument? item in indexes.ToList())
            {
                Debug.WriteLine(item.ToString()); // or perform any operations with the index information
                if ((string)item["name"] != "_id_") backCollection.Indexes.DropOne((string)item["name"]);
            };


            //backCollection.DeleteMany(Builders<RoundResults>.Filter.Empty);

            {
                var indexKeysDefinition = Builders<RoundResults>.IndexKeys.Descending(model => model.CombinedUniqueKey);

                var indexModel = new CreateIndexModel<RoundResults>(
                    indexKeysDefinition,
                    new CreateIndexOptions { Name = "CombinedUniqueKey_index", Unique = true }
                );
                backCollection.Indexes.CreateOne(indexModel);
            }

            {
                var indexKeysDefinition = Builders<RoundResults>.IndexKeys.Descending(model => model.DocID);

                var indexModel = new CreateIndexModel<RoundResults>(
                    indexKeysDefinition,
                    new CreateIndexOptions { Name = "DocID_index", Unique = true }
                );

                backCollection.Indexes.CreateOne(indexModel);
            }

            {
                var indexKeysDefinition = Builders<RoundResults>.IndexKeys.Descending(model => model.CourseName);

                var indexModel = new CreateIndexModel<RoundResults>(
                    indexKeysDefinition,
                    new CreateIndexOptions { Name = "CourseName_index", Unique = false }
                );

                backCollection.Indexes.CreateOne(indexModel);
            }

            {
                var indexKeysDefinition = Builders<RoundResults>.IndexKeys.Descending("Players.Player");

                var indexModel = new CreateIndexModel<RoundResults>(
                    indexKeysDefinition,
                    new CreateIndexOptions { Name = "Players_index", Unique = false }
                );

                backCollection.Indexes.CreateOne(indexModel);
            }

        }
    }
}