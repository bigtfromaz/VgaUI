using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using VgaUI.Shared;

public class PlayerResult
{
    private bool _isGuest = true;
    private bool isNoShow = false;
    private bool _isInSkins = false;

    public int AllPlayersPosition { get; set; } = 0;
    public bool AllPlayersTie { get; set; } = false;
    public int FlightNumber { get; set; } = 0;
    public int FlightPosition { get; set; } = 0;
    public bool FlightTie { get; set; } = false;
    public bool IsGuest
    {
        get => _isGuest;
        set
        {
            _isGuest = value;
            if (value == true)
            {
                IsInLowNet = false;
                _isInSkins = false;
                IsInCTPs = true;
            }
            else
            {
                IsInLowNet = true;
                _isInSkins = true;
                IsInCTPs = true;
            }
        }
    }
    public bool IsDNF { get; set; } = false;
    public bool IsPaid { get; set; } = false;
    public bool IsNoShow
    {
        get => isNoShow;
        set
        {
            if (value == true)
            {
                IsInLowNet = false;
                IsInSkins = false;
                IsInCTPs = false;
            }
            else
            {
                if (IsGuest)
                {
                    IsInLowNet = false;
                    _isInSkins = false;
                    IsInCTPs = true;
                }
                else
                {
                    IsInLowNet = true;
                    _isInSkins = true;
                    IsInCTPs = true;
                }
            }
            isNoShow = value;

        }
    }
    public bool IsInCTPs { get; set; } = true;
    public bool IsInSkins
    {
        get => _isInSkins;
        set
        {
            if (value == true || NumSkins == 0)
            {
                _isInSkins = value; // Can always set to true
            }
            else if (NumSkins > 0)
            {
                _isInSkins = true;
            }
            else
            { 
                _isInSkins = false; 
            }
        }
    }
    public bool IsInLowNet { get; set; } = false;
    public string Player { get; set; } = "";
    public int TotalGross { get; set; } = 0;
    public string ToBaselineParNet { get; set; } = "unknown";
    public int PlayingHandicap { get; set; } = 0;
    public int TotalNet { get; set; } = 0;
    public int Points { get; set; } = 0;
    public List<int> CtpHoleNumbers { get; set; } = [];
    public string CtpDetails { get; set; } = string.Empty;
    public int NumCTPs
    {
        get
        {
            return CtpHoleNumbers.Count;
        }
    }
   public decimal PlayerEntryFee(PurseSettings purseSettings)
    {
        decimal totalFee = 0m;
        if (IsNoShow)
            return 0m;

        if (IsGuest)
            // Guest fee is the sum of the guest fee, the round fee, and the CTP contribution
            //totalFee = purseSettings.GuestFeeToClub + purseSettings.RoundFeeToClub + purseSettings.CTPContributionAmount;
            // Guest fee is the sum of the guest fee, and the CTP contribution
            totalFee = purseSettings.GuestFeeToClub + purseSettings.CTPContributionAmount;
        else
        {
            totalFee = purseSettings.RoundFeeToClub;
            if (IsInCTPs)
                totalFee += purseSettings.CTPContributionAmount;

            if (IsInLowNet)
                totalFee += purseSettings.LowNetContributionAmount;

            if (IsInSkins)
                totalFee += purseSettings.BirdieContributionAmount;
        }

        return totalFee;
    }    
    public string HolesAsString
    {
        get
        {
            string strHoles = string.Empty;
            int idx = 0;
            foreach (var holeNumber in CtpHoleNumbers)
            {
                idx++;
                if (idx > 1) strHoles += ",";
                strHoles += holeNumber.ToString();
            }

            return strHoles;
            //return $"{Helpers.Plurality(NumCTPs, "Hole")} {strHoles}";
        }
    }

    [BsonIgnore]
    public string CtpEntryString
    {
        get 
        {
            string strWork = string.Empty;
            foreach (int holeNumber in CtpHoleNumbers)
            {
                if (strWork.Length == 0) { strWork = $"{holeNumber}"; }
                else { strWork += $", {holeNumber}"; }
            }
            return strWork; 
        }
        set 
        {

            string[] holes = value.Split(",");
            CtpHoleNumbers = [];
            int idx = 0;
            foreach (string strHole in holes) 
            {

                if (int.TryParse(strHole, out int holeNumber))
                {
                    CtpHoleNumbers.Add(holeNumber);
                }
                else
                {
                    // Bad string, blank it out
                    CtpHoleNumbers = [];
                }
                idx++;
            }
        }
    }
    public int NumSkins { get; set; }
    [BsonRepresentation(MongoDB.Bson.BsonType.Decimal128)]
    public decimal LowNetPurse { get; set; } = 0;
    [BsonRepresentation(MongoDB.Bson.BsonType.Decimal128)]
    public decimal SkinsPurse { get; set; } = 0;
    [BsonRepresentation(MongoDB.Bson.BsonType.Decimal128)]
    public decimal CtpPurse { get; set; } = 0;
    public string SkinsDetailsGG { get; set; } = string.Empty;
    public string SkinsDetails { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    [BsonIgnore]
    public string DisplayDetails => $"{Details} - {SkinsDetailsGG} - {CtpDetails}";
    public string LowNetCellDisplay
    {
        get
        {
            if (LowNetPurse > 0)
                return $"{Helpers.Money(LowNetPurse)} {Helpers.AddOrdinal(FlightPosition)} - flight {FlightNumber} ";
            else
                return "-";
        }
    }

    [BsonIgnore]
    [BsonRepresentation(MongoDB.Bson.BsonType.Decimal128)]
    public decimal TotalPurse => LowNetPurse + CtpPurse + SkinsPurse;
}
public class PlayerResultWithRound
{
    public PlayerResultWithRound()
    {

    }
    public PlayerResultWithRound(PlayerResult Result, string CourseName, DateOnly DateOfPlay)
    {
        this.Result = Result;
        this.CourseName = CourseName;
        this.DateOfPlay = DateOfPlay;
    }
    public DateOnly DateOfPlay = DateOnly.FromDateTime(DateTime.Now);
    public string CourseName = string.Empty;
    public PlayerResult Result { get; set; } = new();
    public bool IsWednesday => DateOfPlay.DayOfWeek == DayOfWeek.Wednesday;
    public bool IsSaturday => DateOfPlay.DayOfWeek == DayOfWeek.Saturday;

    [BsonIgnore]
    public int WednesdayPoints
    {
        get
        {
            if (IsWednesday)
            {
                return Result.Points;
            }
            else return 0;
        }
    }
    [BsonIgnore]
    public int SaturdayPoints
    {
        get
        {
            if (IsSaturday)
            {
                return Result.Points;
            }
            else return 0;
        }
    }

}