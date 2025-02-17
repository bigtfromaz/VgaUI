namespace DBUtils
{
    using MongoDB.Bson;
    using MongoDB.Driver;
    using System;
    
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = args[0];
            var dbname = args[1];
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(dbname);
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>("RoundResults");

          
            IMongoCollection<RoundResults> resultsCollection = database.GetCollection<RoundResults>("RoundResults");
            int year = 2024;
            DateOnly firstDayOfYear = new(year, 1, 1);
            DateOnly lastDayOfYear = new(year, 12, 31);
            List<PlayerResultWithRound> details = GetPlayerDetails(resultsCollection, firstDayOfYear, lastDayOfYear);
            string lastPlayer = string.Empty;

            var saturdayResults = details
                .Where(d => d.DateOfPlay.DayOfWeek == DayOfWeek.Saturday)
                .GroupBy(d => new { d.DateOfPlay.DayOfWeek, d.Result.Player })
                .Select(g => new
                {
                    DayOfWeek = g.Key.DayOfWeek,
                    Player = g.Key.Player,
                    TotalPoints = g.Sum(d => d.Result.Points)
                })
                .OrderBy(g => g.DayOfWeek)
                .ThenByDescending(g => g.TotalPoints);

            var wednesdayResults = details
                .Where(d => d.DateOfPlay.DayOfWeek == DayOfWeek.Wednesday)
                .GroupBy(d => new { d.DateOfPlay.DayOfWeek, d.Result.Player })
                .Select(g => new
                {
                    DayOfWeek = g.Key.DayOfWeek,
                    Player = g.Key.Player,
                    TotalPoints = g.Sum(d => d.Result.Points)
                })
                .OrderBy(g => g.DayOfWeek)
                .ThenByDescending(g => g.TotalPoints);



            //.ToList();
            Console.WriteLine($"===== Saturday Big Bogs ======\n");
            foreach (var g in saturdayResults)
            {
                Console.WriteLine($"{g.DayOfWeek,-10} {g.Player,-20} {g.TotalPoints,-5}");
            }

            Console.WriteLine($"\n===== Wednesday Big Bogs ======\n");
            foreach (var g in wednesdayResults)
            {
                Console.WriteLine($"{g.DayOfWeek,-10} {g.Player,-20} {g.TotalPoints,-5}");
            }

            Console.WriteLine($"\n===== Big Bogs Player Details ======\n");
            foreach (PlayerResultWithRound result in details)
            {
                var r = result.Result;
                string day = result.DateOfPlay.DayOfWeek.ToString();

                if (r.LowNetPurse > 0)
                {
                    if (result.Result.Player != lastPlayer)
                    {
                        Console.WriteLine("-------------------------------------------------------------------------------------------");
                    }
                    Console.WriteLine($"{r.Player, -20} Date: {day, -10} {result.DateOfPlay, -10} Course: {result.CourseName,-25} Points: {r.Points,-5}");
                    lastPlayer = result.Result.Player;
                }
            }


        }
        static void PublishEverthing(IMongoCollection<BsonDocument> collection)
        {
            var update = Builders<BsonDocument>.Update.Set("IsOfficial", true);

            try
            {
                var result = collection.UpdateMany(new BsonDocument(), update);
                Console.WriteLine(result.ModifiedCount + " document(s) updated");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: " + ex.Message);
            }
        }
        static List<PlayerResultWithRound> GetPlayerDetails(IMongoCollection<RoundResults> collection, DateOnly from, DateOnly to)
        {
            var stats = new VGAStats(collection, from, to);
            
            List<PlayerResultWithRound> details = stats.PlayerStatsList.OrderBy(p => p.Result.Player).ToList();

        

            //List<RoundResults> result =  collection.AsQueryable()
            //    .OrderByDescending(doc => doc.CombinedUniqueKey)
            //    .Where(dates => dates.DateOfPlay >= from && dates.DateOfPlay <= to)
            //    .ToList<RoundResults>();

            

            return details;
        }
    }
}