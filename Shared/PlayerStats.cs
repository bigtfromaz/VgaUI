using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PlayerStats
{
    public DateOnly DateOfPlay { get; set; }
    public string CourseName { get; set; } = "";
    public string? Player { get; set; }
    public double AverageGross { get; set; }
    public double AverageNet { get; set; }
    public int LowGross { get; set; }
    public int LowNet { get; set; }
    public int TimesPlayed { get; set; }
    public int TotalPoints { get; set; }
    public int WednesdayPoints { get; set; }
    public int SaturdayPoints { get; set; }
    public int LowNetPurse { get; set; }
    public int CtpPurse { get; set; }
    public int SkinsPurse { get; set; }
    public int TotalPurse => LowNetPurse + CtpPurse + SkinsPurse;
}