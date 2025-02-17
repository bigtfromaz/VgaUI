using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System.Diagnostics;

//using MongoDB.Driver;
using System.Globalization;
using System.Text;
using VgaUI.Shared;

public class RoundResults
{
    #region Properties
    [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
    //        [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string? DocID { get; set; } = System.Guid.NewGuid().ToString();
    [BsonElement]
    public string CourseName { get; set; } = "Unknown";
    [BsonElement]
    public string CombinedUniqueKey
    {
        get
        {
            return $"{DateOfPlay:yyyy-MM-dd}_{CourseName}";
        }
        set { ; }
    }
    [BsonElement]
    public DateOnly DateOfPlay { get; set; } = DateOnly.FromDateTime(DateTime.Now);

    public string RoundReport { get; set; } = "Report not yet generated";
    [BsonElement]
    public int NumCTPs { get; set; }
    [BsonElement]
    public bool IsOfficial { get; set; } = false;

    public int GetNumCTPs()
    {
        int accumulator = 0;
        foreach (var item in Players)
        {
            accumulator += item.CtpHoleNumbers.Count;
        }
        NumCTPs = accumulator;
        return accumulator;
    }


    [BsonElement]
    public PurseSettings CurrentPurseSettings { get; set; } = new();
    [BsonElement]
    public List<PlayerResult> Players { get; set; } = [];
    #endregion

    #region Recalculation Properties & Methods
    [BsonElement]
    public int TotalLowNetPot { get; set; }
    public int GetTotalLowNetPot()
    {
        TotalLowNetPot = Players.Where(item => item.IsInLowNet).Sum(amt => (int)CurrentPurseSettings.LowNetContributionAmount);
        return TotalLowNetPot;
    }

    [BsonElement]
    public int TotalSkinParticipants { get; set; } = 0;
    public int GetTotalSkinParticipants()
    {
        return TotalSkinParticipants = Players.Where(player => player.IsInSkins).Count();
    }

    [BsonElement]
    public int TotalLowNetParticipants { get; set; } = 0;
    public int GetTotalLowNetParticipants()
    {
        return TotalLowNetParticipants = Players.Where(player => player.IsInLowNet).Count();
    }

    [BsonElement]
    public int TotalSkinPot { get; set; }
    public int GetTotalSkinPot()
    {
        return TotalSkinPot = Players.Where(player => player.IsInSkins).Sum(amt => (int)CurrentPurseSettings.BirdieContributionAmount);
        //return TotalSkinPot = Players.Where(player => player.IsInSkins && !player.IsNoShow).Sum(amt => (int)CurrentPurseSettings.BirdieContributionAmount);
    }

    [BsonElement]
    public int TotalNumSkins { get; set; }
    public int GetTotalNumSkins()
    {
        return TotalNumSkins = Players.Where(player => player.IsInSkins && player.NumSkins > 0).Sum(player => player.NumSkins);
    }

    [BsonElement]
    public decimal PayPerSkin { get; set; }
    public decimal GetPayPerSkin()
    {
        return PayPerSkin = (decimal)GetTotalSkinPot() / GetTotalNumSkins();
    }

    [BsonElement]
    public int TotalSkinPaid { get; set; }
    public int GetTotalSkinPaid()
    {
        return TotalSkinPaid = Players.Where(player => player.IsInSkins).Sum(amt => (int)amt.SkinsPurse);
    }


    /* Guests */

    [BsonElement]
    [BsonIgnoreIfNull]
    public int NumGuests { get => Guests.Count;  }

    [BsonElement]
    public List<PlayerResult> Guests { get; set; } = [];
    public List<PlayerResult> GetGuests()
    {
        return Guests = Players.Where(item => item.IsGuest && !item.IsNoShow).ToList();
    }

    [BsonElement]
    public int GuestFeesToClub { get; set; }
    public int GetGuestFeesToClub()
    {
        return GuestFeesToClub = Guests.Count * (int)CurrentPurseSettings.GuestFeeToClub;

        //return GuestFeesToClub = Players.Where(item => item.IsGuest && !item.IsNoShow).Sum(item => (int)CurrentPurseSettings.GuestFeeToClub);
    }


    /* Members */

    [BsonElement]
    [BsonIgnoreIfNull]
    public List<PlayerResult> Members { get; set; } = [];
    public List<PlayerResult> GetMembers()
    {
        return Members = Players.Where(item => !item.IsGuest && !item.IsNoShow).ToList();
    }
    [BsonElement]
    [BsonIgnoreIfNull]
    public int NumMembers
    {
        get => Members.Count();
    }

    [BsonElement]
    [BsonIgnoreIfNull]
    public int MemberFeesToClub { get; set; } = 0;
    public int GetMemberFeesToClub()
    {
        GetMembers();
        return MemberFeesToClub = Members.Count * (int)CurrentPurseSettings.RoundFeeToClub;
        //return MemberFeesToClub = Players.Where(item => !item.IsGuest && !item.IsNoShow).Sum(item => (int)CurrentPurseSettings.RoundFeeToClub);
    }

    [BsonElement]
    public int LeftOverSkin { get; set; }
    public int GetLeftoverSkin()
    {
        return LeftOverSkin = GetTotalSkinPot() - GetTotalSkinPaid();
    }

    [BsonElement]
    public int TotalCtpPot { get; set; }
    public int GetTotalCtpPot()
    {
        return TotalCtpPot = (int)(GetCTPParticipants() * CurrentPurseSettings.CTPContributionAmount);
    }

    [BsonElement]
    [BsonIgnoreIfNull]
    public int TotalRoundFeeToClub { get; set; } = 0;
    public int GetTotalRoundFeeToClub()
    {
        return TotalRoundFeeToClub = Players.Where(item => !item.IsNoShow && !item.IsGuest).Sum(item => (int)CurrentPurseSettings.RoundFeeToClub);
        // return TotalRoundFeeToClub = (int)(GetCTPParticipants() * CurrentPurseSettings.RoundFeeToClub);
    }
    [BsonElement]
    public int CTPParticipants { get; set; }
    public int GetCTPParticipants()
    {
        return CTPParticipants = Players.Where(player => player.IsInCTPs).Sum(amt => 1);
    }

    [BsonElement]
    public int PayPerCTP { get; set; }
    public int GetPayPerCTP()
    {
        if (GetNumCTPs() > 0)
            return PayPerCTP = (int)(GetTotalCtpPot() / GetNumCTPs());
        else return 0;
    }

    [BsonElement]
    public int LeftoverCTP { get; set; }

    public int GetLeftoverCTP()
    {
        return LeftoverCTP = (GetTotalCtpPot() - (GetNumCTPs() * GetPayPerCTP()));
    }

    [BsonElement]
    public int TotalCollected { get; set; }
    public int GetTotalCollected()
    {
        var TotalCtp = GetTotalCtpPot();
        var TotalLowNet = GetTotalLowNetPot();
        var TotalSkin = GetTotalSkinPot();
        var GuestFees = GetGuestFeesToClub();
        var RoundFees = GetTotalRoundFeeToClub();
        return TotalCollected = (GetTotalCtpPot() + GetTotalLowNetPot() + GetTotalSkinPot() + GetGuestFeesToClub() + GetTotalRoundFeeToClub());
    }

    [BsonElement]
    public int PaidLowNet { get; set; }
    public int GetPaidLowNet()
    {
        return PaidLowNet = Players.Sum(playerResult => (int)playerResult.LowNetPurse);
    }

    [BsonElement]
    public int LeftoverLowNet { get; set; }
    public int GetLeftoverLowNet()
    {
        return LeftoverLowNet = GetTotalLowNetPot() - GetPaidLowNet();
    }
    public int TotalPursePaid => TotalCtpPot + TotalLowNetPot + TotalSkinPot - LeftOverSkin - LeftoverCTP - LeftoverLowNet;
    public int PaidCTPs => TotalCtpPot - LeftoverCTP;
    public int PaidSkins => TotalSkinPot - LeftOverSkin;

    [BsonElement]
    public int TotalToClub { get; set; }
    public int GetTotalToClub()
    {
        return TotalToClub = GetLeftoverSkin() + GetLeftoverCTP() + GetLeftoverLowNet() + GetGuestFeesToClub() + GetMemberFeesToClub();
    }

    [BsonIgnore]
    public IEnumerable<int> Flights => Players.Where(flight => flight.FlightNumber > 0).Select(flight => flight.FlightNumber).Distinct().Order(); // Guests are in flight 0

    #endregion
    [BsonIgnore]
    public List<PlayerResult> PlayersByName => Players.OrderBy(row => row.Player).ToList();


    #region methods
    public void StorePlayer(PlayerResult result) => Players.Add(result);
    public List<PlayerResult> GetFligthPlayers(int FlightNumber) => Players.Where(item => item.FlightNumber == FlightNumber && !item.IsGuest).ToList();
    public PlayerResult GetPlayerByName(string name) => Players.FirstOrDefault(row => row.Player == name) 
        ?? throw new Exception($"The player Named {name} does not exists in All Players. Has someone altered the spreadsheet?");
    public void SetSkinPurses()
    {
        GetTotalSkinParticipants();
        List<PlayerResult> winners = Players.Where(player => player.IsInSkins && player.NumSkins > 0).ToList();
        decimal totalSkinPot = (decimal)GetTotalSkinPot();
        int numSkins = GetTotalNumSkins();
        var payPerSkin = numSkins > 0 ? totalSkinPot / numSkins : 0;

        int amtToPayout = 0;
        foreach (PlayerResult player in winners)
        {
            player.SkinsPurse = (int)(player.NumSkins * payPerSkin);
            player.SkinsDetails = $"{player.SkinsDetailsGG} at {Helpers.Money(GetPayPerSkin())} each";
            amtToPayout += (int) player.SkinsPurse;
        }
    }
    public int GetFlightTotalPaid(int FlightNumber)
    {
        Console.WriteLine($"Flight {FlightNumber} Total Paid: {Players.Where(player => player.FlightNumber == FlightNumber).Sum(player => (int)player.TotalPurse)}");
        int result = Players.Where(player => !player.IsNoShow && player.FlightNumber == FlightNumber).Sum(player => (int)player.LowNetPurse);
        foreach (var item in Players)
        {
            if (item.FlightNumber == FlightNumber)
            {
                if (item.LowNetPurse > 0)
                Console.WriteLine($"{item.Player} Flight: {item.FlightNumber} Position: {item.FlightPosition} Amount: {item.LowNetPurse}");
            }
        }
        return result;
    }

    int GetEligiblePlayerCount(int FlightNumber) => Players.Where(player => !player.IsNoShow && player.FlightNumber == FlightNumber).Count();
    /// <summary>
    /// Loads Low Net winnings Players' Purse column
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void SetLowNetPurses()
    {
        int totalLowNet = GetTotalLowNetPot();
        int allPlayerCount = 0;
        GetTotalLowNetParticipants();
        foreach (int flight in Flights)
        {
            int flightPot;
            int allocated = 0;
            int flightPlayerCount = 0;
            List<PlayerResult> flightPlayers = GetFligthPlayers(flight);

            flightPot = (int) totalLowNet / Flights.Count();

            List<double> payoutRatios = [];

            if (flightPlayers.Count > CurrentPurseSettings.LargeFlightThreshold)
            {
                payoutRatios = CurrentPurseSettings.LargeFlightPlace;
            }
            else
            {
                payoutRatios = CurrentPurseSettings.SmallFlightPlace;
            }
            int rowNum = 0;
            foreach (PlayerResult playerResult in flightPlayers)
            {
                playerResult.LowNetPurse = 0;
                int payoutOffset = flightPlayerCount;
                flightPlayerCount++;
                allPlayerCount++;
                if (payoutOffset < payoutRatios.Count)
                {
                    // We have a winner!
                    playerResult.LowNetPurse = (int)(payoutRatios[payoutOffset] * flightPot);
                    playerResult.Details = $"{Helpers.AddOrdinal(flightPlayerCount)} Place Flight {flight}: {Helpers.Money(playerResult.LowNetPurse)}";
                    allocated += (int)playerResult.LowNetPurse;
                }
                else
                {
                    // This is the last player to be paid
                    if ((flightPot - allocated) > 0)
                    {
                        playerResult.LowNetPurse = flightPot - allocated;
                        playerResult.Details = $"{Helpers.AddOrdinal(flightPlayerCount)} Place Flight {flight}: {Helpers.Money(playerResult.LowNetPurse)}";
                        allocated += (int)playerResult.LowNetPurse;
                    }
                }
                if (rowNum < CurrentPurseSettings.PointsPerPlace.Count)
                {
                    playerResult.Points = CurrentPurseSettings.PointsPerPlace[rowNum];
                }
                rowNum++;
            }

            ReallocateTies(flightPlayers);
            if (flightPot != allocated) throw new Exception($"Bug. The amount allocated, {allocated} does not match the flight pool amount, {flightPot}");
        }
    }

    public void ReallocateTies(List<PlayerResult> flightPlayers)
    {
        // Group players by their FlightPosition and filter out those who are tied
        var tiedGroups = flightPlayers
            .Where(p => p.FlightTie)
            .GroupBy(p => p.FlightPosition)
            .Where(g => g.Count() > 1);

        foreach (var group in tiedGroups)
        {
            // Calculate the total LowNetPurse for the tied players
            decimal totalPurse = group.Sum(p => p.LowNetPurse);
            decimal totalPoints = group.Sum(p => p.Points);

            // Calculate the equal share for each player and truncate to two decimal places
            decimal equalPurse = Math.Truncate((totalPurse / group.Count()) * 100) / 100;
            decimal equalPoints = Math.Round(totalPoints / group.Count());

            // Allocate the equal share to each player
            foreach (var player in group)
            {
                player.LowNetPurse = (int)equalPurse;
                player.Points = (int)equalPoints;
            }
        }
    }

    public void SetLowNetPursesBasedOnPlayerCount()
    {
        int allPlayerCount = 0;
        foreach (int flight in Flights)
        {
            int flightPot;
            int allocated = 0;
            int flightPlayerCount = 0;
            List<PlayerResult> flightPlayers = GetFligthPlayers(flight);

            flightPot = (int)(GetEligiblePlayerCount(flight) * CurrentPurseSettings.LowNetContributionAmount);

            List<double> payoutRatios = [];

            if (flightPlayers.Count > CurrentPurseSettings.LargeFlightThreshold)
            {
                payoutRatios = CurrentPurseSettings.LargeFlightPlace;
            }
            else
            {
                payoutRatios = CurrentPurseSettings.SmallFlightPlace;
            }
            foreach (PlayerResult playerResult in flightPlayers)
            {
                int payoutOffset = flightPlayerCount;
                flightPlayerCount++;
                allPlayerCount++;
                if (payoutOffset < payoutRatios.Count)
                {
                    // We have a winner!
                    playerResult.LowNetPurse = (int)(payoutRatios[payoutOffset] * flightPot);
                    playerResult.Details = $"{Helpers.AddOrdinal(flightPlayerCount)} Place Flight {flight}: {Helpers.Money(playerResult.TotalPurse)}";
                    allocated += (int)playerResult.LowNetPurse;
                }
                else
                {
                    // This is the last player to be paid
                    if ((flightPot - allocated) > 0)
                    {
                        playerResult.LowNetPurse = flightPot - allocated;
                        playerResult.Details = $"{Helpers.AddOrdinal(flightPlayerCount)} Place Flight {flight}: {Helpers.Money(playerResult.TotalPurse)}";
                        allocated += (int)playerResult.LowNetPurse;
                    }
                }
            }

            if (flightPot != allocated) throw new Exception($"Bug. The amount allocated, {allocated} does not match the flight pool amount, {flightPot}");
        }
    }
    public void RecordCTPs()
    {
        foreach (var item in Players)
        {
            if (item.CtpHoleNumbers.Count > 0)
            {
                item.CtpPurse = item.CtpHoleNumbers.Count * GetPayPerCTP();
                string strHoles = string.Empty;
                int idx = 0;
                foreach (var holeNumber in item.CtpHoleNumbers)
                {
                    idx++;
                    if (idx > 1) strHoles += ",";
                    strHoles += holeNumber.ToString();
                }
                item.CtpPurse = item.CtpHoleNumbers.Count * GetPayPerCTP();
                int numCTPs = item.CtpHoleNumbers.Count;
                item.CtpDetails = $"{numCTPs} on {Helpers.Plurality(numCTPs, "Hole")} {strHoles}";
                //item.CtpDetails = $"{numCTPs} on {Helpers.Plurality(numCTPs, "Hole")} {strHoles}";
                //item.CtpDetails = $"{numCTPs} {Plurality(numCTPs, "CTP")} at {Money(GetPayPerCTP())} {Plurality(numCTPs, "Hole")} ({strHoles})";
                //item.CtpDetails = $"{numCTPs} {Plurality(numCTPs, "CTP")} at {Money(PayPerCTP)}: {Money(item.CtpPurse)} {Plurality(numCTPs, "Hole")} ({strHoles})";

            }
            else
            {
                item.CtpDetails = string.Empty;
                item.CtpPurse = 0;
            }
        }


    }
    public List<PlayerResult> GetDisbursementsByTotal()
    {
        return [.. Players.Where(a => a.TotalPurse > 0).OrderByDescending(x => x.TotalPurse)];
    }

    public string CalculateResults()
    {
        if (IsOfficial)
            return RoundReport;
        if (Players.Count == 0) return "No data";
        SetLowNetPurses();
        RecordCTPs();
        SetSkinPurses();
        GetGuests();
        GetMembers();
        GetTotalCollected();
        GetLeftoverSkin();
        GetLeftoverCTP();
        GetLeftoverLowNet();
        GetGuestFeesToClub();
        GetMemberFeesToClub();
        GetTotalToClub();
        RoundReport =
            $"\nRound name: {CourseName} - {DateOfPlay}\n\n" +
            $"Total Collected: {Helpers.Money(TotalCollected)}\n" +
            $"Total Low Net Pot: {Helpers.Money(TotalLowNetPot)}\n" +
            $"Total Low Net Participants: {TotalLowNetParticipants}\n" +
            $"Total Skin Pot: {Helpers.Money(TotalSkinPot)}\n" +
            $"Total Skin Participants: {TotalSkinParticipants}\n" +
            $"Total Number of Skins: {TotalNumSkins}\n" +
            $"Pay Per Skin: {Helpers.Money(PayPerSkin)}\n" +
            $"Total Skin Paid: {Helpers.Money(TotalSkinPaid)}\n" +
            $"Total CTP Pot: {Helpers.Money(TotalCtpPot)}\n" +
            $"Total Round Fee to Club: {Helpers.Money(TotalRoundFeeToClub)}\n" +
            $"Total CTP Participants: {CTPParticipants}\n" +
            $"Pay Per CTP: {Helpers.Money(PayPerCTP)}\n" +
            $"Total Collected: {Helpers.Money(TotalCollected)}\n" +
            $"Paid Low Net: {Helpers.Money(PaidLowNet)}\n" +
            $"Leftover Low Net: {Helpers.Money(LeftoverLowNet)}\n" +
            $"Guest Fees to Club: {Helpers.Money(GuestFeesToClub)}\n" +
            $"Member Fees to Club: {Helpers.Money(MemberFeesToClub)}\n" +
            $"Leftover Skin: {Helpers.Money(LeftOverSkin)}\n" +
            $"Leftover CTP: {Helpers.Money(LeftoverCTP)}\n" +
            $"Total To Club: {Helpers.Money(TotalToClub)}\n";

    Console.WriteLine($"{RoundReport}");
        return RoundReport;
    }

    /// <summary>
    /// A generic for setting a property only if IsOfficial is false
    /// </summary>
    /// <remarks>
    /// Sample use: SetIfNotOfficial(ref flightNumber, value); 
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    /// <param name="field"></param>
    /// <param name="value"></param>
    private void SetIfNotOfficial<T>(ref T field, T value)
    {
        if (!IsOfficial)
        {
            field = value;
        }
    }
}
#endregion
