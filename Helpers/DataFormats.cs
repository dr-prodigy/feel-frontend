/* 
 * Copyright (c) 2011-2018 FEELTeam - Maurizio Montel.
 * 
 * This file is part of the FEEL (FrontEnd - Emulator Launcher) distribution.
 *   (https://github.com/dr-prodigy/feel-frontend)
 * 
 * FEEL is free software: you can redistribute it and/or modify  
 * it under the terms of the GNU Lesser General Public License as   
 * published by the Free Software Foundation, version 3.
 *
 * FEEL is distributed in the hope that it will be useful, but 
 * WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU 
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 *
 * doc/info/contacts: http://feelfrontend.altervista.org
 */

using System;
using System.Text;
using System.Collections.Generic;

namespace feel
{
    public enum SortType
    {
        AZ = 0,
        Ranking = 1,
        MostRecentlyPlayed = 2,
        Year = 3,
        Manufacturer = 4,
        Category = 5,
        InputControl = 6,
        ExternalKey = 7
    }

    public enum FnetSortType
    {
        LastPlayed = 0,
        MostPlayed = 1
    }

    public enum ScreenSaverType
    {
        None = 0,
        Slideshow = 1,
        StartRandomGame = 2
    }

    public enum Levels
    {
        FEEL_INI = 0,
        PLATFORM_INI = 1,
        EMULATOR_INI = 2,
        GAMELIST_INI = 3,
        LAYOUT_INI = 4,
        ROM_INI = 5
    }

    public enum ListType
    {
        rom_list = 0,
        mame_xml_list = 1,
        rom_settings_ini_list = 2,
        mame_listinfo = 3,
        mess_machine = 4
    }

    public enum TextAlign
    {
        Left = 0,
        Center = 1,
        Right = 2
    }

    public enum SmartAsdMode
    {
        None = 0,
        Hybrid = 1,
        Dedicated = 2,
        Dedicated6Buttons = 3
    }

    public enum UseMouse
    {
        No = 0,
        XAxis = 1,
        YAxis = 2
    }

    public enum InputState
    {
        None = 0,
        KeyDown = 1,
        KeyPress = 2,
        MouseClick = 2,
        MouseDoubleClick = 3,
        MouseDown = 1
    }

    public enum AutostartMode
    {
        Off = 0,
        LastSelected = 1,
        LastPlayed = 2,
        SingleGame = 3
    }

    public enum FeelKeyDX
    {
        None = 0,
        Back = 14,
        Tab = 15,
        Enter = 28,
        Pause = 197,
        Esc = 1,
        Space = 57,
        PageUp = 201,
        PageDown = 209,
        End = 207,
        Home = 199,
        Left = 203,
        Up = 200,
        Right = 205,
        Down = 208,
        Insert = 210,
        Delete = 211,
        D0 = 11,
        D1 = 2,
        D2 = 3,
        D3 = 4,
        D4 = 5,
        D5 = 6,
        D6 = 7,
        D7 = 8,
        D8 = 9,
        D9 = 10,
        A = 30,
        B = 48,
        C = 46,
        D = 32,
        E = 18,
        F = 33,
        G = 34,
        H = 35,
        I = 23,
        J = 36,
        K = 37,
        L = 38,
        M = 50,
        N = 49,
        O = 24,
        P = 25,
        Q = 16,
        R = 19,
        S = 31,
        T = 20,
        U = 22,
        V = 47,
        W = 17,
        X = 45,
        Y = 21,
        Z = 44,
        LWin = 219,
        RWin = 220,
        NumPad0 = 82,
        NumPad1 = 79,
        NumPad2 = 80,
        NumPad3 = 81,
        NumPad4 = 75,
        NumPad5 = 76,
        NumPad6 = 77,
        NumPad7 = 71,
        NumPad8 = 72,
        NumPad9 = 73,
        Multiply = 55,
        Add = 78,
        Subtract = 74,
        Divide = 181,
        F1 = 59,
        F2 = 60,
        F3 = 61,
        F4 = 62,
        F5 = 63,
        F6 = 64,
        F7 = 65,
        F8 = 66,
        F9 = 67,
        F10 = 68,
        F11 = 87,
        F12 = 88,
        LShift = 42,
        RShift = 54,
        LCtrl = 29,
        RCtrl = 157,
        LAlt = 56,
        RAlt = 184,
        JoyUp = 996,
        JoyDown = 997,
        JoyLeft = 998,
        JoyRight = 999,
        JoyB1 = 1000,
        JoyB2 = 1001,
        JoyB3 = 1002,
        JoyB4 = 1003,
        JoyB5 = 1004,
        JoyB6 = 1005,
        JoyB7 = 1006,
        JoyB8 = 1007,
        JoyB9 = 1008,
        JoyB10 = 1009,
        JoyB11 = 1010,
        JoyB12 = 1011,
        JoyB13 = 1012,
        JoyB14 = 1013,
        JoyB15 = 1014
    }

    public enum FeelKey
    {
		None = 0,
		Back = 8,
		Tab = 9,
		Enter = 13,
		CapsLock = 20,
		Esc = 27,
		Space = 32,
		PageUp = 33,
		PageDown = 34,
		End = 35,
		Home = 36,
		Left = 37,
		Up = 38,
		Right = 39,
		Down = 40,
		Select = 41,
		Print = 42,
		Execute = 43,
		PrintScreen = 44,
		Insert = 45,
		Delete = 46,
		Help = 47,
		D0 = 48,
		D1 = 49,
		D2 = 50,
		D3 = 51,
		D4 = 52,
		D5 = 53,
		D6 = 54,
		D7 = 55,
		D8 = 56,
		D9 = 57,
		A = 65,
		B = 66,
		C = 67,
		D = 68,
		E = 69,
		F = 70,
		G = 71,
		H = 72,
		I = 73,
		J = 74,
		K = 75,
		L = 76,
		M = 77,
		N = 78,
		O = 79,
		P = 80,
		Q = 81,
		R = 82,
		S = 83,
		T = 84,
		U = 85,
		V = 86,
		W = 87,
		X = 88,
		Y = 89,
		Z = 90,
        LWin = 91,
        RWin = 92,
		NumPad0 = 96,
		NumPad1 = 97,
		NumPad2 = 98,
		NumPad3 = 99,
		NumPad4 = 100,
		NumPad5 = 101,
		NumPad6 = 102,
		NumPad7 = 103,
		NumPad8 = 104,
		NumPad9 = 105,
		Multiply = 106,
		Add = 107,
		Separator = 108,
		Subtract = 109,
		Decimal = 110,
		Divide = 111,
		F1 = 112,
		F2 = 113,
		F3 = 114,
		F4 = 115,
		F5 = 116,
		F6 = 117,
		F7 = 118,
		F8 = 119,
		F9 = 120,
		F10 = 121,
		F11 = 122,
		F12 = 123,
        LShift = 160,
        RShift = 161,
        LCtrl = 162,
        RCtrl = 163,
        LAlt = 164,
        RAlt = 165,
		Pause = 19,
        JoyUp = 996,
        JoyDown = 997,
        JoyLeft = 998,
        JoyRight = 999,
        JoyB1 = 1000,
        JoyB2 = 1001,
        JoyB3 = 1002,
        JoyB4 = 1003,
        JoyB5 = 1004,
        JoyB6 = 1005,
        JoyB7 = 1006,
        JoyB8 = 1007,
        JoyB9 = 1008,
        JoyB10 = 1009,
        JoyB11 = 1010,
        JoyB12 = 1011,
        JoyB13 = 1012,
        JoyB14 = 1013,
        JoyB15 = 1014
    }

	public class RomDesc : IComparable<RomDesc>
    {
        // List File (.lst)
        private string _key;
        public string Key
        {
            get
            {
                if (FeelInfo.RunChain.IsSet)
                    return FeelInfo.RunChain.Key;
                return _key;
            }
            set
            {
                _key = value;
                FeelInfo.RomName = _key;
            }
        }
        private string _description;
        private string _cleanedupDescription;
        public string Description
        {
            get
            {
                
                return CleanupName ? _cleanedupDescription : _description;
            }
            set
            {
                _description = value;
                // name cleanup: remove everything after first bracket
                // find first bracket (avoid to trim from 0 position)
                var bracket = value.IndexOf('(') > 0 ? value.IndexOf('(') : value.Length;
                bracket = Math.Min(bracket, value.IndexOf('[') > 0 ? value.IndexOf('[') : value.Length);
                _cleanedupDescription = value.Substring(0, bracket).Trim();
            }
        }
        public string Year { get; set; }
        public string Manufacturer { get; set; }
        public string CloneOf { get; set; }
        public string Bios { get; set; }
        private List<string> _extraData = new List<string>();
        // §-separated data: VideoType, RomRelativePath
        public string ExtraData
        {
            get
            {
                return string.Join("§", _extraData.ToArray());
            }
            set
            {
                _extraData.Clear();
                _extraData.AddRange(value.Split('§'));
            }
        }
        public string VideoType
        {
            get
            {
                return _extraData[0];
            }
            set
            {
                _extraData[0] = value;
            }
        }

        public string RomRelativePath
        {
            get
            {
                if (_extraData.Count > 1)
                    return _extraData[1];
                return string.Empty;
            }
            set
            {
                if (_extraData.Count > 1)
                    _extraData[1] = value;
                else
                    _extraData.Add(value);
            }
        }
        public string ScreenOrientation { get; set; }
        public string InputControl { get; set; }
        public string Status { get; set; }
        public string Color { get; set; }
        public string Sound { get; set; }
        public string Category { get; set; }
        // Stats File (.sts)
        public int PlayedCount { get; set; }
        public TimeSpan PlayedTime { get; set; }
        public DateTime TimeStamp { get; set; }
        public TimeSpan PlayedTimeNow { get; set; }
        public int ExternalSortKey { get; set; }
        public static bool CleanupName { get; set; }

        RomFEELInfo _feelInfo;
        public RomFEELInfo FeelInfo
        {
            get
            {
                if (_feelInfo == null)
                    _feelInfo = new RomFEELInfo();
                return _feelInfo;
            }
            set
            {
                _feelInfo = value;
            }
        }

        public RomDesc()
        {
            Key = Description = Year = Manufacturer = CloneOf = Bios =
                ExtraData = ScreenOrientation = InputControl = Status =
                Color = Sound = Category = string.Empty;

            PlayedCount = 0;
            PlayedTime = TimeSpan.Zero;
            TimeStamp = DateTime.MinValue;
            PlayedTimeNow = TimeSpan.Zero;
            ExternalSortKey = int.MaxValue;
        }

        public RomDesc(string allListLine)
        {
            var lineValues = allListLine.Split('|');
            Key = lineValues[0];
            Description = lineValues[1];
            Year = lineValues[2];
            Manufacturer = lineValues[3];
            CloneOf = lineValues[4];
            Bios = lineValues[5];
            ExtraData = lineValues[6];
            ScreenOrientation = lineValues[7];
            InputControl = lineValues[8];
            Status = lineValues[9];
            Color = lineValues[10];
            Sound = lineValues[11];
            Category = lineValues[12];
            FeelInfo = new RomFEELInfo(lineValues[13], lineValues[14], lineValues[15], lineValues[16].Split('§')[0],
                lineValues[17], lineValues[18], lineValues[19], lineValues[20], lineValues[21], lineValues[22],
                lineValues[23], lineValues[24], lineValues[25], lineValues[26]);
        }

        public override string ToString()
        {
            return Description;
        }

        public string SortComparisonChars(SortType CurrentSort)
        {
            var value = string.Empty;
            switch (CurrentSort)
            {
                case SortType.Category: value = Category; break;
                case SortType.InputControl: value = InputControl; break;
                case SortType.Manufacturer: value = Manufacturer; break;
                case SortType.Year: value = Year; break;
                default: value = Description.Substring(0, 1).ToUpper(); break;
            }
            return value;
        }

        public int CompareTo(RomDesc rom2)
        {
            return Description.CompareTo(rom2.Description);
        }
    }

    public class RankingComparer : IComparer<RomDesc>
    {
        public int Compare(RomDesc first, RomDesc second)
        {
            if (first.PlayedTimeNow < second.PlayedTimeNow)
                return 1;
            if (first.PlayedTimeNow > second.PlayedTimeNow)
                return -1;
            if (first.PlayedTime < second.PlayedTime)
                return 1;
            if (first.PlayedTime > second.PlayedTime)
                return -1;
            if (first.PlayedCount < second.PlayedCount)
                return 1;
            if (first.PlayedCount > second.PlayedCount)
                return -1;
            return first.Description.CompareTo(second.Description);
        }
    }

    public class MostRecentlyPlayedComparer : IComparer<RomDesc>
    {
        public int Compare(RomDesc first, RomDesc second)
        {
            if (first == second)
                return 0;
            if (first.TimeStamp != null && second.TimeStamp != null && !first.TimeStamp.Equals(second.TimeStamp))
                return second.TimeStamp.CompareTo(first.TimeStamp);
            return first.Description.CompareTo(second.Description);
        }
    }


    public class YearComparer : IComparer<RomDesc>
    {
        public int Compare(RomDesc first, RomDesc second)
        {
            if (first == second)
                return 0;
            if (first.Year != null && second.Year != null && !first.Year.Equals(second.Year))
                return first.Year.CompareTo(second.Year);
            return first.Description.CompareTo(second.Description);
        }
    }

    public class ManufacturerComparer : IComparer<RomDesc>
    {
        public int Compare(RomDesc first, RomDesc second)
        {
            if (first == second)
                return 0;
            if (first.Manufacturer != null && second.Manufacturer != null && !first.Manufacturer.Equals(second.Manufacturer))
                return first.Manufacturer.CompareTo(second.Manufacturer);
            return first.Description.CompareTo(second.Description);
        }
    }

    public class CategoryComparer : IComparer<RomDesc>
    {
        public int Compare(RomDesc first, RomDesc second)
        {
            if (first == second)
                return 0;
            if (first.Category != null && second.Category != null && !first.Category.Equals(second.Category))
                return first.Category.CompareTo(second.Category);
            return first.Description.CompareTo(second.Description);
        }
    }

    public class InputControlComparer : IComparer<RomDesc>
    {
        public int Compare(RomDesc first, RomDesc second)
        {
            if (first == second)
                return 0;
            if (first.InputControl != null && second.InputControl != null && !first.InputControl.Equals(second.InputControl))
                return first.InputControl.CompareTo(second.InputControl);
            return first.Description.CompareTo(second.Description);
        }
    }

    public class ExternalKeyComparer : IComparer<RomDesc>
    {
        public int Compare(RomDesc first, RomDesc second)
        {
            if (first == second)
                return 0;
            if (first.ExternalSortKey == second.ExternalSortKey)
                return first.Description.CompareTo(second.Description);

            return first.ExternalSortKey.CompareTo(second.ExternalSortKey);
        }
    }

    public class GameRunChain
    {
        public string Platform { get; private set; }
        public string Emulator { get; private set; }
        public string Gamelist { get; private set; }
        public string Game { get; private set; }

        public GameRunChain()
        {
            Platform = Emulator = Gamelist = Game = string.Empty;
        }

        public GameRunChain(string gameRunChainString)
        {
            var chain = gameRunChainString.Split('|');
            if (chain.Length > 3)
            {
                Platform = chain[0];
                Emulator = chain[1];
                Gamelist = chain[2];
                Game = chain[3];
            }
        }

        public GameRunChain(string platform, string emulator, string gamelist, string game)
        {
            Platform = platform;
            Emulator = emulator;
            Gamelist = gamelist;
            Game = game;
        }

        public bool IsSet
        {
            get
            {
                return !(string.IsNullOrEmpty(Platform) | string.IsNullOrEmpty(Emulator) | string.IsNullOrEmpty(Gamelist) | string.IsNullOrEmpty(Game));
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder(Platform);
            sb.Append("|");
            sb.Append(Emulator);
            sb.Append("|");
            sb.Append(Gamelist);
            sb.Append("|");
            sb.Append(Game);

            return sb.ToString();
        }

        public string Key
        {
            get
            {
                var sb = new StringBuilder(Game);
                sb.Append("§");
                sb.Append(Platform);
                sb.Append("§");
                sb.Append(Emulator);
                //sb.Append("§");
                //sb.Append(Gamelist);

                return sb.ToString();
            }
        }

        public void Clear()
        {
            Platform = Emulator = Gamelist = string.Empty;
        }
    }

    public class RomFEELInfo
    {
        public GameRunChain RunChain { get; set; }
        public string RomName { get; set; }
        public string PlatformDesc { get; set; }
        public string EmulatorDesc { get; set; }
        public string GamelistDesc { get; set; }
        public string SnapshotPath { get; set; }
        public string VideoPath { get; set; }
        public string CabinetPath { get; set; }
        public string MarqueePath { get; set; }
        public string SnapshotExtension { get; set; }
        public string CabinetExtension { get; set; }
        public string MarqueeExtension { get; set; }

        public RomFEELInfo()
        {
            Init();
        }

        public RomFEELInfo(string platform, string emulator, string gameList, string romName, string platformDesc,
            string emulatorDesc, string gamelistDesc, string snapshotPath,
            string snapshotExtension, string videoPath, string cabinetPath, string cabinetExtension, string marqueePath,
            string marqueeExtension)
        {
            Init();
            RunChain = new GameRunChain(platform, emulator, gameList, romName);
            RomName = romName;
            PlatformDesc = platformDesc;
            EmulatorDesc = emulatorDesc;
            GamelistDesc = gamelistDesc;
            SnapshotPath = snapshotPath;
            SnapshotExtension = snapshotExtension;
            VideoPath = videoPath;
            CabinetPath = cabinetPath;
            CabinetExtension = cabinetExtension;
            MarqueePath = marqueePath;
            MarqueeExtension = marqueeExtension;
        }

        public RomFEELInfo(string gameInformationString)
        {
            Init();
            var infoString = gameInformationString.Split('|');
            if (infoString.Length > 10)
            {
                RunChain = new GameRunChain(gameInformationString);
                RomName = RunChain.Game;
                SnapshotPath = infoString[4];
                SnapshotExtension = infoString[5];
                VideoPath = infoString[6];
                CabinetPath = infoString[7];
                CabinetExtension = infoString[8];
                MarqueePath = infoString[9];
                MarqueeExtension = infoString[10];
            }
        }

        private void Init()
        {
            RunChain = new GameRunChain();
            RomName = SnapshotPath = VideoPath = CabinetPath = MarqueePath =
                SnapshotExtension = CabinetExtension = MarqueeExtension = string.Empty;
        }

        public override string ToString()
        {
            var sb = new StringBuilder(RunChain.ToString());
            sb.Append("|");
            sb.Append(PlatformDesc);
            sb.Append("|");
            sb.Append(EmulatorDesc);
            sb.Append("|");
            sb.Append(GamelistDesc);
            sb.Append("|");
            sb.Append(SnapshotPath);
            sb.Append("|");
            sb.Append(SnapshotExtension);
            sb.Append("|");
            sb.Append(VideoPath);
            sb.Append("|");
            sb.Append(CabinetPath);
            sb.Append("|");
            sb.Append(CabinetExtension);
            sb.Append("|");
            sb.Append(MarqueePath);
            sb.Append("|");
            sb.Append(MarqueeExtension);

            return sb.ToString();
        }
    }
}
