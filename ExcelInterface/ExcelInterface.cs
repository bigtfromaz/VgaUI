using ExcelDataReader;
using ExcelInterface;
using System.Data;
using System.Text;
// NuGet Dependency at https://github.com/ExcelDataReader/ExcelDataReader 
namespace VGA
{
    public class ExcelInterface
    {
        private readonly Dictionary<string, int> _playerColumnIndexes = new();
        private readonly Dictionary<string, int> _skinsColumnIndexes = new();
        private readonly Dictionary<string, int> _allPlayerColumnIndexes = new();
        public List<MemberData> _members = new();

        // Column Headings as constants
        private const string LOW_NET_TABLE_NAME_Opt1 = "low net player v. flight...";
        private const string LOW_NET_TABLE_NAME_Opt2 = "low net  player v. field...";
        private const string SKINS_TABLE_NAME = "basic skins player v. fi...";
        private const string SKINS = "Skins";
        private const string DETAILS = "Details";
        private const string ALL_GOLFERS_SCORES = "all golfers scores";
        private const string POSITION = "Pos.";
        private const string PLAYER = "Player";
        private const string TO_BASELINE_PAR_NET = "To Baseline Par Net";
        private const string TO_PAR_NET = "To Par Net";
        private const string PLAYING_HANDICAP = "Playing Handicap";
        private const string TOTAL_GROSS = "Total Gross";
        private const string TOTAL_NET = "Total Net";
        private const string PURSE = "Purse";
        private const string POINTS = "Points";
        private readonly RoundResults _leaderBoardData;
        private PurseSettings _purseSettings;

        public RoundResults LeaderBoardData
        {
            get { return _leaderBoardData; }
            //set { _flights = value; }
        }

        public ExcelInterface(PurseSettings settings)
        {
            _leaderBoardData = new();
            _leaderBoardData.CurrentPurseSettings = _purseSettings = settings;
        }
        public void LoadLeaderboard(string filePath)
        {
            using FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            MemoryStream ms = new();
            stream.CopyToAsync(ms);
            LoadLeaderboard(ms);
        }
        public RoundResults LoadLeaderboard(MemoryStream stream)
        {
            DataSet result;
            try
            {
                // Auto-detect format, supports:
                //  - Binary Excel files (2.0-2003 format; *.xls)
                //  - OpenXml Excel files (2007 format; *.xlsx, *.xlsb)
                using IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream);
                result = reader.AsDataSet();

            }
            catch (Exception ex)
            {
                StringBuilder sb = new();
                sb.AppendLine($"Ubable to open and read the spreadsheet! Is {stream} the correct name?");
                sb.AppendLine(ex.Message);
                throw new ArgumentException(sb.ToString(), ex);
            }

            DataTable? dtLowNetTournament = dtLowNetTournament = result.Tables[LOW_NET_TABLE_NAME_Opt1] ?? result.Tables[LOW_NET_TABLE_NAME_Opt2];
            if (dtLowNetTournament == null)
            {
                StringBuilder sb = new();
                sb.AppendLine($"This appears to be the wrong spreadsheet!:");
                sb.AppendLine($"The file does not contain a worksheet named neither {LOW_NET_TABLE_NAME_Opt1} nor {LOW_NET_TABLE_NAME_Opt2}.");
                throw new ArgumentException(sb.ToString());
            }

            DataTable? dtAllGolfersScores;
            if ((dtAllGolfersScores = result.Tables[ALL_GOLFERS_SCORES]) == null)
            {
                StringBuilder sb = new();
                sb.AppendLine($"This appears to be the wrong spreadsheet!:");
                sb.AppendLine($"The file named file exists but does not contain a worksheet named {ALL_GOLFERS_SCORES}.");
                throw new ArgumentException(sb.ToString());
            }

            DataTable? dtSkins;
            if ((dtSkins = result.Tables[SKINS_TABLE_NAME]) == null)
            {
                StringBuilder sb = new();
                sb.AppendLine($"This appears to be the wrong spreadsheet!:");
                sb.AppendLine($"The file named {stream} exists but does not contain a worksheet named {SKINS_TABLE_NAME}.");
                throw new ArgumentException(sb.ToString());
            }

            ExtractAllGolferScores(dtAllGolfersScores);
            ExtractFlights(dtLowNetTournament);
            ExtractSkins(dtSkins);

            //_leaderBoardData.SetSkinPurses(); // Loads Skin winnings into the Players' Purse column
            //_leaderBoardData.RecordCTPs();

            return _leaderBoardData;
        }
        private void ExtractSkins(DataTable Skins)
        {
            int currentRowIDX = 0;
            bool isFirstRow = true;
            foreach (DataRow row in Skins.Rows)
            {
                if (isFirstRow &&row.ItemArray.Count() < 4) return; // Not enough columns, so no skins
                currentRowIDX++;
                if (row.ItemArray[0] == System.DBNull.Value) continue;
                string rowCol0 = (row.ItemArray[0] as string ?? "").Trim();
                string rowCol1 = (row.ItemArray[1] as string ?? "").Trim();
                string rowCol2 = (row.ItemArray[2] as string ?? "").Trim();
                string rowCol3 = (row.ItemArray[3] as string ?? "").Trim();
                if (isFirstRow)
                {
                    for (int i = 0; i < Skins.Columns.Count; i++)
                    {
                        object? value = row.ItemArray[i];

                        // Cache the column numbers by name
                        switch (((string?)value))
                        {
                            case null:
                                continue;

                            case PLAYER:
                                _skinsColumnIndexes.Add(PLAYER, i);
                                break;

                            case SKINS:
                                _skinsColumnIndexes.Add(SKINS, i);
                                break;

                            case PURSE:
                                _skinsColumnIndexes.Add(PURSE, i);
                                break;

                            case DETAILS:
                                _skinsColumnIndexes.Add(DETAILS, i);
                                break;

                            default: break;
                        }
                    }
                    isFirstRow = false;
                    continue; // Done parsing column headings
                } // end first row

                if (rowCol0.StartsWith("Total Purse Allocated")) continue; // Skip

                string playerName = (string?)row.ItemArray[_skinsColumnIndexes.GetValueOrDefault(PLAYER)] ?? throw new Exception($"Error parsing player name on basic skins player tab. Row {currentRowIDX}.");
                PlayerResult currentPlayer = LeaderBoardData.GetPlayerByName(playerName);

                try
                {
                    currentPlayer.NumSkins = Convert.ToInt32(row.ItemArray[_skinsColumnIndexes.GetValueOrDefault(SKINS)] ?? throw GetFileLoadException(currentRowIDX, SKINS));
                    currentPlayer.SkinsPurse = 0;
                    currentPlayer.SkinsDetailsGG = (string?)row.ItemArray[_skinsColumnIndexes.GetValueOrDefault(DETAILS)] ?? throw GetFileLoadException(currentRowIDX, DETAILS);

                    //if (currentPlayer.SkinsPurse > 0)
                    //    currentPlayer.SkinsDetails = (string?)row.ItemArray[_skinsColumnIndexes.GetValueOrDefault(DETAILS)] ?? throw GetFileLoadException(currentRowIDX, DETAILS);
                    //else
                    //    currentPlayer.SkinsDetails = string.Empty;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error parsing Skins Row Number {currentRowIDX}!: {ex.Message}", ex);
                }
            }
            //RecordCTPs();

            return;
        }
        private void ExtractFlights(DataTable dtLowNet)
        {
            int currentSpreadsheetRow = 0;
            bool headingsProcessed = false;

            // Make Flight 1 current
            int currentFlight = 1;

            foreach (DataRow row in dtLowNet.Rows)
            {
                currentSpreadsheetRow++;
                if (row.ItemArray[0] == System.DBNull.Value) continue;
                string rowCol0 = (row.ItemArray[0] as string ?? "").Trim();

                if (rowCol0 == POSITION && headingsProcessed) continue; // No need to process headings again

                if (rowCol0 == POSITION && !headingsProcessed) // Process headings
                {
                    for (int i = 0; i < dtLowNet.Columns.Count; i++)
                    {
                        object? value = row.ItemArray[i];

                        // Cache the column numbers by name
                        switch (((string?)value))
                        {
                            case null:
                                continue;
                            case POSITION:
                                _playerColumnIndexes.Add(POSITION, i);
                                break;

                            case PLAYER:
                                _playerColumnIndexes.Add(PLAYER, i);
                                break;

                            case PLAYING_HANDICAP:
                                _playerColumnIndexes.Add(PLAYING_HANDICAP, i);
                                break;

                            case TOTAL_GROSS:
                                _playerColumnIndexes.Add(TOTAL_GROSS, i);
                                break;

                            case TO_BASELINE_PAR_NET:
                            case TO_PAR_NET:
                                _playerColumnIndexes.Add(TO_PAR_NET, i);
                                break;

                            case TOTAL_NET:
                                _playerColumnIndexes.Add(TOTAL_NET, i);
                                break;

                            case PURSE:
                                _playerColumnIndexes.Add(PURSE, i);
                                break;

                            default: break;
                        }
                    }
                    headingsProcessed = true;
                    continue; // Done parsing column headings
                }
                if (rowCol0.StartsWith("Total Purse Allocated")) continue; // Skip 

                if (rowCol0.StartsWith("Flight "))
                {
                    var words = rowCol0.Trim().Split(" ");
                    if (words.Length != 2) 
                    {
                        throw new Exception($"The \"Flight n\" header row is in the wrong format. The cell contains \"{rowCol0}\".");
                    }
                    try
                    {
                        currentFlight = int.Parse(words[1]);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Cannot parse a flight number from this Flight header row: \"{rowCol0}\": {ex.Message}, ex");
                    }
                    currentSpreadsheetRow = 0;
                    continue;
                }

                try
                {
                    // If the Player name can't be parsed we skip the row. It's superfluous
                    string playerName;
                    try
                    {
                        playerName = (string?)row.ItemArray[_playerColumnIndexes.GetValueOrDefault(PLAYER)] ?? throw GetFileLoadException(currentSpreadsheetRow, PLAYER);
                    }
                    catch (Exception) { continue; }

                    PlayerResult currentPlayer = LeaderBoardData.GetPlayerByName(playerName);
                    currentPlayer.FlightNumber = currentFlight;

                    currentPlayer.IsGuest = false;
                    currentPlayer.IsInLowNet = true;
                    currentPlayer.IsInSkins = true;

                    if (_playerColumnIndexes.ContainsKey(POSITION))
                    {
                        int iPosition;
                        var objPosition = (object) (row.ItemArray[_playerColumnIndexes.GetValueOrDefault(POSITION)] ?? throw GetFileLoadException(currentSpreadsheetRow, POSITION));
                        if (objPosition is Double || objPosition is Int32 || objPosition is Decimal)
                        {
                            iPosition = Int32.Parse(((double)objPosition).ToString());
                            currentPlayer.FlightPosition = iPosition;
                            if (iPosition <= _purseSettings.PointsPerPlace.Count)
                            {
                                currentPlayer.Points = _purseSettings.PointsPerPlace[iPosition - 1];
                            }
                        }
                        else if (objPosition is String strFlightPosition)
                        {
                            // If the position begins with a T followed by a number, it's a tie 
                            if (strFlightPosition.StartsWith("T"))
                            {
                                currentPlayer.FlightPosition = Convert.ToInt32(strFlightPosition.Substring(1));
                                currentPlayer.FlightTie = true;
                                if (currentPlayer.FlightPosition <= _purseSettings.PointsPerPlace.Count)
                                {
                                    currentPlayer.Points = _purseSettings.PointsPerPlace[currentPlayer.FlightPosition - 1];
                                }
                            }
                        }
                        else currentPlayer.FlightPosition = 999;
                    }

                    // // No pulling Purse from the spreadsheet because we are going to calculate it later
                    //if (_playerColumnIndexes.ContainsKey(PURSE))
                    //    currentPlayer.LowNetPurse = Convert.ToDecimal(((string?)row.ItemArray[_playerColumnIndexes.GetValueOrDefault(PURSE)] ?? throw GetFileLoadException(currentSpreadsheetRow, PURSE)).Replace("$", ""));

                    //currentFlight.AddPlayer(currentPlayer);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error parsing Flight {currentFlight} Position {currentSpreadsheetRow}!: {ex.Message}", ex);
                }

            }
            _leaderBoardData.SetLowNetPurses(); // Loads LowNet winnings into the Players' Purse column
            return;
        }
        private void ExtractAllGolferScores(DataTable dtAllGolfers)
        {
            int currentSpreadsheetRow = 0;
            
            foreach (DataRow row in dtAllGolfers.Rows)
            {
                currentSpreadsheetRow++;
                if (row.ItemArray[0] == System.DBNull.Value) continue;
                string rowCol0 = (row.ItemArray[0] as string ?? "").Trim();

                if (rowCol0 == POSITION) // Process headings
                {
                    for (int i = 0; i < dtAllGolfers.Columns.Count; i++)
                    {
                        object? value = row.ItemArray[i];

                        // Cache the column numbers by name
                        switch (((string?)value))
                        {
                            case null:
                                continue;
                            case POSITION:
                                _allPlayerColumnIndexes.Add(POSITION, i);
                                break;

                            case PLAYER:
                                _allPlayerColumnIndexes.Add(PLAYER, i);
                                break;

                            case PLAYING_HANDICAP:
                                _allPlayerColumnIndexes.Add(PLAYING_HANDICAP, i);
                                break;

                            case TOTAL_GROSS:
                                _allPlayerColumnIndexes.Add(TOTAL_GROSS, i);
                                break;

                            case TO_BASELINE_PAR_NET:
                            case TO_PAR_NET:
                                _allPlayerColumnIndexes.Add(TO_PAR_NET, i);
                                break;

                            case TOTAL_NET:
                                _allPlayerColumnIndexes.Add(TOTAL_NET, i);
                                break;

                            case POINTS:
                                _allPlayerColumnIndexes.Add(POINTS, i);
                                break;

                            default: break;
                        }
                    }
                    continue; // Done parsing column headings
                }
                try
                {
                    PlayerResult result = new();

                    if (_allPlayerColumnIndexes.ContainsKey(POSITION))
                    {
                        int i;
                        var objPosition = (object)(row.ItemArray[_allPlayerColumnIndexes.GetValueOrDefault(POSITION)] ?? throw GetFileLoadException(currentSpreadsheetRow, POSITION));
                        if (objPosition is System.DBNull) continue;
                        if (objPosition is Double || objPosition is Int32 || objPosition is Decimal)
                        {
                            i = Int32.Parse(((double)objPosition).ToString());
                            result.AllPlayersPosition = i;
                        }
                        else if (objPosition is String strPosition)
                        {
                            // If the position begins with a T followed by a number, it's a tie 
                            if (strPosition.StartsWith("T"))
                            {
                                result.AllPlayersPosition = Convert.ToInt32(strPosition.Substring(1));
                                result.AllPlayersTie = true;
                            }
                            else if (strPosition == "NS" || strPosition == "DNS")
                            {
                                result.IsNoShow = true;
                                result.IsGuest = false;
                            }
                            else if (strPosition == "DNF")
                            {
                                result.IsDNF = true;
                            }
                            else result.AllPlayersPosition = 999;
                        }
                    }

                    if (_allPlayerColumnIndexes.ContainsKey(PLAYER))
                        try
                        {
                            result.Player = (string?)row.ItemArray[_allPlayerColumnIndexes.GetValueOrDefault(PLAYER)] ?? throw GetFileLoadException(currentSpreadsheetRow, PLAYER);

                        }
                        catch (Exception)
                        {

                            continue;
                        }
                    if (_allPlayerColumnIndexes.ContainsKey(PLAYING_HANDICAP))
                        result.PlayingHandicap = Convert.ToInt32(row.ItemArray[_allPlayerColumnIndexes.GetValueOrDefault(PLAYING_HANDICAP)] ?? throw GetFileLoadException(currentSpreadsheetRow, PLAYING_HANDICAP));

                    if (_allPlayerColumnIndexes.ContainsKey(TOTAL_GROSS))
                    {
                        var objTotalGross = (object)(row.ItemArray[_allPlayerColumnIndexes.GetValueOrDefault(TOTAL_GROSS)] ?? throw GetFileLoadException(currentSpreadsheetRow, TOTAL_GROSS));
                        int iTotalGross = 0;

                        if (objTotalGross is System.DBNull) continue;
                        if (objTotalGross is Double || objTotalGross is Int32 || objTotalGross is Decimal)
                        {
                            iTotalGross = Int32.Parse(((double)objTotalGross).ToString());
                            result.TotalGross = iTotalGross;
                        }
                        else if (objTotalGross is String)
                        {
                            result.TotalGross = 999;
                        }
                    }

                    if (_allPlayerColumnIndexes.ContainsKey(TO_PAR_NET))
                    {
                        var objToBaselineParNet = (object)(row.ItemArray[_allPlayerColumnIndexes.GetValueOrDefault(TO_PAR_NET)] ?? throw GetFileLoadException(currentSpreadsheetRow, TO_PAR_NET));
                        int i = 0;

                        if (objToBaselineParNet is System.DBNull) continue;
                        if (objToBaselineParNet is Double || objToBaselineParNet is Int32 || objToBaselineParNet is Decimal)
                        {
                            i = Int32.Parse(((double)objToBaselineParNet).ToString());
                            result.ToBaselineParNet = i.ToString();
                        }
                        else if (objToBaselineParNet is String)
                        {
                            result.ToBaselineParNet = (string) objToBaselineParNet;
                        }
                    }

                    if (_allPlayerColumnIndexes.ContainsKey(POINTS))
                    {
                        var objPoints = (object)(row.ItemArray[_allPlayerColumnIndexes.GetValueOrDefault(POINTS)] ?? throw GetFileLoadException(currentSpreadsheetRow, POINTS));
                        int iPoints = 0;

                        if (objPoints is System.DBNull) continue;
                        if (objPoints is Double || objPoints is Int32 || objPoints is Decimal)
                        {
                            iPoints = Int32.Parse(((double)objPoints).ToString());
                            result.Points = iPoints;
                        }
                        else if (objPoints is String)
                        {
                            result.Points = int.Parse((string)objPoints);
                        }
                    }

                    if (_allPlayerColumnIndexes.ContainsKey(TOTAL_NET))
                    {
                        var objTotalNet = (object)(row.ItemArray[_allPlayerColumnIndexes.GetValueOrDefault(TOTAL_NET)] ?? throw GetFileLoadException(currentSpreadsheetRow, TOTAL_NET));
                        int iTotalNet = 0;

                        if (objTotalNet is System.DBNull) continue;
                        if (objTotalNet is Double || objTotalNet is Int32 || objTotalNet is Decimal)
                        {
                            iTotalNet = Int32.Parse(((double)objTotalNet).ToString());
                            result.TotalNet = iTotalNet;
                        }
                        else if (objTotalNet is String)
                        {
                            result.TotalNet = 999;
                            result.AllPlayersPosition = 999;
                        }

                    }

                    _leaderBoardData.StorePlayer(result);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error parsing all golfers scores tab row number {currentSpreadsheetRow}!: {ex.Message}", ex);
                }

            }
            return;
        }

        private static FileLoadException GetFileLoadException(int playerPosition, string columnName)
        {
            return new FileLoadException($"Row number {playerPosition}: Invalid null value n {columnName} Column!");
        }

        /// <summary>
        /// Loads a GG Roster Spreadsheet Currently Unused
        /// </summary>
        /// <param name="filePath"></param>
        /// <exception cref="FileLoadException"></exception>
        public void LoadRoster(string filePath)
        {
            int columnRowNumber;
            int currentRowNumber = 0;
            bool bColumnHeadersFound = false;
            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            // Auto-detect format, supports:
            //  - Binary Excel files (2.0-2003 format; *.xls)
            //  - OpenXml Excel files (2007 format; *.xlsx, *.xlsb)
            using var reader = ExcelReaderFactory.CreateReader(stream);

            do
            {
                while (reader.Read())
                {
                    currentRowNumber++;
                    object[] values = new object[reader.FieldCount];

                    if (bColumnHeadersFound)
                    {
                        MemberData player = new();
                        int idx;

                        idx = _playerColumnIndexes.GetValueOrDefault("ID");
                        player.ID = decimal.Parse((string)reader.GetValue(idx));

                        idx = _playerColumnIndexes.GetValueOrDefault("GHIN_Index");
                        string? strGHIN_Index = (string)reader.GetValue(idx);
                        if (strGHIN_Index is null)
                            player.HasGHIN_Index = false;
                        else
                        {
                            player.GHIN_Index = decimal.Parse(strGHIN_Index);
                            player.HasGHIN_Index = true;
                        }


                        idx = _playerColumnIndexes.GetValueOrDefault("GHIN_ID");
                        player.GHIN_ID = int.Parse((string)reader.GetValue(idx));

                        idx = _playerColumnIndexes.GetValueOrDefault("FirstName");
                        player.FirstName = (string)reader.GetValue(idx);

                        idx = _playerColumnIndexes.GetValueOrDefault("LastName");
                        player.LastName = (string)reader.GetValue(idx);

                        idx = _playerColumnIndexes.GetValueOrDefault("Email");
                        player.Email = (string)reader.GetValue(idx);

                        _members.Add(player);
                    }

                    else // Check to see if this is the column header row and save column numbers by name

                        for (int i = 0; i < values.Length; i++)
                        {
                            object value = reader.GetValue(i);

                            // Cache the column numbers by name
                            switch (((string)value))
                            {
                                case "Email":
                                    _playerColumnIndexes.Add("Email", i);
                                    bColumnHeadersFound = true;
                                    columnRowNumber = currentRowNumber;
                                    break;

                                case "First Name":
                                    _playerColumnIndexes.Add("FirstName", i);
                                    break;

                                case "Last Name":
                                    _playerColumnIndexes.Add("LastName", i);
                                    break;

                                case "GHIN Id":
                                    _playerColumnIndexes.Add("GHIN_ID", i);
                                    break;

                                case "Index":
                                    _playerColumnIndexes.Add("GHIN_Index", i);
                                    break;

                                default: break;
                            }
                        }
                    // All required colums should have been found
                    string msg = $"The spreadsheet must have Email, First Name, Last Name, GHIN Id and Index columns. Only {_playerColumnIndexes.Count} were found.";
                    if (bColumnHeadersFound && _playerColumnIndexes.Count != 5) throw new FileLoadException("");

                }

            } while (reader.NextResult());

            //// 2. Use the AsDataSet extension method
            //var result = reader.AsDataSet();

        }
    }
}