using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;

//using MongoDB.Driver;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Collections.ObjectModel;
using VgaUI.Shared;

public class VGAStats
{
    public readonly IMongoCollection<RoundResults> _RoundResultsCollection;
    public readonly List<PlayerResultWithRound> _playerStatsList;
    //static async method that behave like a constructor       
    //async public static Task<VGAStats> VGAStatsFactory(IMongoCollection<RoundResults> roundResultsCollection, DateOnly from, DateOnly to)
    //{
    //    return new VGAStats(roundResultsCollection, from, to);
    //}

    public VGAStats(IMongoCollection<RoundResults> roundResultsCollection, DateOnly from, DateOnly to)
    {
        _RoundResultsCollection = roundResultsCollection;

        List<RoundResults> rounds = (List<RoundResults>)
        [
            .. _RoundResultsCollection.AsQueryable()
                               .Where(dates => dates.DateOfPlay >= from && dates.DateOfPlay <= to)
,
            ];

        _playerStatsList =
        [
            .. _RoundResultsCollection.AsQueryable()
                                .Where(dates => dates.DateOfPlay >= from && dates.DateOfPlay <= to)
                                 .SelectMany(round => round.Players.Select(player => new PlayerResultWithRound { Result = player, CourseName = round.CourseName, DateOfPlay = round.DateOfPlay })
                                 ),
            ];



    }

    public List<PlayerStats> ByPlayer => _playerStatsList
           .Where(player => player.Result.TotalGross < 200 && player.Result.IsInLowNet)
           .GroupBy(playerResult => new { playerResult.Result.Player }) // Group by Player
           .Select(playerStats => new PlayerStats
           {
               Player = playerStats.Key.Player,
               AverageGross = playerStats.Average(playerResult => playerResult.Result.TotalGross),
               AverageNet = playerStats.Average(playerResult => playerResult.Result.TotalNet),
               LowGross = playerStats.Min(playerResult => playerResult.Result.TotalGross),
               LowNet = playerStats.Min(playerResult => playerResult.Result.TotalNet),
               TotalPoints = playerStats.Sum(playerResult => playerResult.Result.Points),
               WednesdayPoints = playerStats.Sum(playerResult => playerResult.WednesdayPoints),
               SaturdayPoints = playerStats.Sum(playerResult => playerResult.SaturdayPoints),
               TimesPlayed = playerStats.Sum(one => 1),
               LowNetPurse = playerStats.Sum(playerResult => (int)playerResult.Result.LowNetPurse),
               CtpPurse = playerStats.Sum(playerResult => (int)playerResult.Result.CtpPurse),
               SkinsPurse = playerStats.Sum(playerResult => (int)playerResult.Result.SkinsPurse),
           }).ToList();


    public List<PlayerStats> PlayerStatsWednesday => _playerStatsList
           .Where(player => player.Result.TotalGross < 200 && player.Result.IsInLowNet && player.DateOfPlay.DayOfWeek == DayOfWeek.Wednesday)
           .GroupBy(playerResult => new { playerResult.Result.Player }) // Group by Player
           .Select(playerStats => new PlayerStats
           {
               Player = playerStats.Key.Player,
               AverageGross = playerStats.Average(playerResult => playerResult.Result.TotalGross),
               AverageNet = playerStats.Average(playerResult => playerResult.Result.TotalNet),
               LowGross = playerStats.Min(playerResult => playerResult.Result.TotalGross),
               LowNet = playerStats.Min(playerResult => playerResult.Result.TotalNet),
               TotalPoints = playerStats.Sum(playerResult => playerResult.Result.Points),
               TimesPlayed = playerStats.Sum(one => 1),
               LowNetPurse = playerStats.Sum(playerResult => (int)playerResult.Result.LowNetPurse),
               CtpPurse = playerStats.Sum(playerResult => (int)playerResult.Result.CtpPurse),
               SkinsPurse = playerStats.Sum(playerResult => (int)playerResult.Result.SkinsPurse),
           }).ToList();

    public List<PlayerStats> PlayerStatsSaturday => _playerStatsList
           .Where(player => player.Result.TotalGross < 200 && player.Result.IsInLowNet && player.DateOfPlay.DayOfWeek == DayOfWeek.Saturday)
           .GroupBy(playerResult => new { playerResult.Result.Player }) // Group by Player
           .Select(playerStats => new PlayerStats
           {
               Player = playerStats.Key.Player,
               AverageGross = playerStats.Average(playerResult => playerResult.Result.TotalGross),
               AverageNet = playerStats.Average(playerResult => playerResult.Result.TotalNet),
               LowGross = playerStats.Min(playerResult => playerResult.Result.TotalGross),
               LowNet = playerStats.Min(playerResult => playerResult.Result.TotalNet),
               TotalPoints = playerStats.Sum(playerResult => playerResult.Result.Points),
               TimesPlayed = playerStats.Sum(one => 1),
               LowNetPurse = playerStats.Sum(playerResult => (int)playerResult.Result.LowNetPurse),
               CtpPurse = playerStats.Sum(playerResult => (int)playerResult.Result.CtpPurse),
               SkinsPurse = playerStats.Sum(playerResult => (int)playerResult.Result.SkinsPurse),
           }).ToList();

    public List<PlayerResultWithRound> PlayerStatsList => _playerStatsList;

    public IEnumerable<PlayerResultWithRound> WednesdayPlayerResults => _playerStatsList.Where(player => player.IsWednesday);
    public IEnumerable<PlayerResultWithRound> SaturdayPlayerResults => _playerStatsList.Where(player => player.IsSaturday);


}