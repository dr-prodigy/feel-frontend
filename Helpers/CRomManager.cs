/* 
 * Copyright (c) 2011-2017 FEELTeam - Maurizio Montel.
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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using feel.fnet;

namespace feel
{
    public struct NamingFile
    {
        public string Description { get; set; }
        public string Name { get; set; }
    }

    static class CRomManager
    {
        private static CultureInfo cultureInfo = new CultureInfo("en-US", true);

        public static List<RomDesc> BuildListFromPath(IFeel feel, OBJConfig objConfig, string romPath)
        {
            var dstList = new List<RomDesc>();
            if (Directory.Exists(romPath))
            {
                var fileList = Utils.GetFiles(romPath, objConfig.rom_extension);
                for (var iLoop = 0; iLoop < fileList.Length; iLoop++)
                {
                    var file = fileList[iLoop];
                    var romName = Path.GetFileNameWithoutExtension(file);
                    var item = new RomDesc();
                    item.Key = romName;
                    item.Description = LabelCleanup(romName);

                    var romRelativePath = fileList[iLoop].Substring(romPath.Length + 1); // skip initial backslash
                    romRelativePath = romRelativePath.Substring(0,
                        romRelativePath.Length - romName.Length - objConfig.rom_extension.Split(',')[0].Trim().Length - 1); // skip "."

                    if (romRelativePath != string.Empty)
                    {
                        // remove trailing backslash
                        if (romRelativePath.Substring(romRelativePath.Length - 1) == Path.DirectorySeparatorChar.ToString())
                            romRelativePath = romRelativePath.Substring(0, romRelativePath.Length - 1);
                        item.RomRelativePath = romRelativePath;
                    }

                    dstList.Add(item);
                    if (iLoop % 100 == 0)
                        feel.ShowMessage("Adding available roms: " + iLoop + " of " + fileList.Length + "...", true);
                }

                var nmsFile = romPath + Path.DirectorySeparatorChar + objConfig.nms_file;
                if (File.Exists(nmsFile))
                {
                    var namingList = new List<NamingFile>();
                    var file = File.OpenText(nmsFile);
                    while (!file.EndOfStream)
                    {
                        var line = file.ReadLine().Trim().ToString();
                        if (line.Contains("|"))
                        {
                            var split = line.Split('|');
                            var named = new NamingFile();
                            named.Description = split[0];
                            named.Name = split[1];
                            namingList.Add(named);
                        }
                    }
                    file.Close();

                    foreach (var name in namingList)
                    {
                        var item = dstList.Find(c => c.Key.Equals(name.Name, StringComparison.CurrentCultureIgnoreCase));
                        if (item != null)
                            item.Description = name.Description;
                    }
                }
                feel.ShowToast("Done.");
            }
            else
            {
                feel.ShowMessage("\"" + romPath + "\" directory not found.\n\nPlease check " + objConfig.emulator_title + " \"rom_path\" parameter.", false);
            }

            return dstList;
        }

        public static List<RomDesc> BuildListFromMameXML(Feel feel, OBJConfig objConfig, string emuCommandLine, string romPath)
        {
            var srcList = MameXMLReader.GetRomList(feel, objConfig.emulator_path, Application.StartupPath, objConfig.list_type, emuCommandLine, objConfig.mess_machine);
            var dstList = new List<RomDesc>();
            if (srcList != null)
            {
                if (Directory.Exists(romPath))
                {
                    var fileList = Utils.GetFiles(romPath, objConfig.rom_extension);
                    for (var iLoop = 0; iLoop < fileList.Length; iLoop++)
                    {
                        var romName = Path.GetFileNameWithoutExtension(fileList[iLoop]);
                        var romRelativePath = fileList[iLoop].Substring(romPath.Length + 1); // skip initial backslash
                        romRelativePath = romRelativePath.Substring(0,
                            romRelativePath.Length - romName.Length - objConfig.rom_extension.Split(',')[0].Trim().Length - 1); // skip "."
                        var item = srcList.Find(c => c.Key.ToLower() == romName);
                        if (item != null)
                        {
                            if (romRelativePath != string.Empty)
                            {
                                // remove trailing backslash
                                if (romRelativePath.Substring(romRelativePath.Length - 1) == Path.DirectorySeparatorChar.ToString())
                                    romRelativePath = romRelativePath.Substring(0, romRelativePath.Length - 1);
                                item.RomRelativePath = romRelativePath;
                            }
                            dstList.Add(item);
                        }
                        if (iLoop % 100 == 0)
                            feel.ShowMessage("Adding available roms: " + Math.Floor((float)iLoop / (float)fileList.Length * 100.0f) + "%", true);
                    }
                    feel.ShowToast("Done.");
                }
                else
                {
                    feel.ShowMessage("\"" + romPath + "\" directory not found.\n\nPlease check " + objConfig.emulator_title + " \"rom_path\" parameter.", false);
                }
            }
            return dstList;
        }

        public static List<RomDesc> LoadRomList(OBJConfig objConfig)
        {
            var list = new List<RomDesc>();
            var fileName = new StringBuilder(Application.StartupPath).Append(Path.DirectorySeparatorChar).Append("data")
                .Append(Path.DirectorySeparatorChar).Append(objConfig.current_platform != "all_emu" ? objConfig.current_gamelist : "all_emu")
                .Append(".lst").ToString();
            RomDesc rom;
            if (File.Exists(fileName))
            {
                var file = File.OpenText(fileName);
                while (!file.EndOfStream)
                {
                    if (objConfig.current_platform != "all_emu")
                    {
                        rom = new RomDesc();
                        rom.Key = file.ReadLine();
                        rom.FeelInfo.RomName = rom.Key;
                        rom.Description = file.ReadLine();
                        rom.Year = file.ReadLine();
                        rom.Manufacturer = file.ReadLine();
                        rom.CloneOf = file.ReadLine();
                        rom.Bios = file.ReadLine();
                        rom.ExtraData = file.ReadLine();
                        rom.ScreenOrientation = file.ReadLine();
                        rom.InputControl = file.ReadLine();
                        rom.Status = file.ReadLine();
                        rom.Color = file.ReadLine();
                        rom.Sound = file.ReadLine();
                        rom.Category = file.ReadLine();
                    }
                    else
                    {
                        rom = new RomDesc(file.ReadLine());
                    }
                    list.Add(rom);
                }
                file.Close();
            }
            var statFileName = objConfig.current_platform == "all_emu" ? "all_emu" : objConfig.current_emulator;
            fileName = new StringBuilder(Application.StartupPath).Append(Path.DirectorySeparatorChar).Append("data")
                .Append(Path.DirectorySeparatorChar).Append(statFileName).Append(".sts").ToString();
            if (File.Exists(fileName))
            {
                var file = File.OpenText(fileName);
                while (!file.EndOfStream)
                {
                    var arr = file.ReadLine().Split('|');
                    if (arr.Length > 3)
                    {
                        var romName = arr[0];
                        var playedCount = int.Parse(arr[1]);
                        var playedTime = TimeSpan.Parse(arr[2]);
                        var timeStamp = DateTime.ParseExact(arr[3], "dd/MM/yyyy HH:mm:ss", cultureInfo);
                        rom = list.Find(c => c.Key == romName);
                        if (rom != null)
                        {
                            rom.PlayedCount = playedCount;
                            rom.PlayedTime = playedTime;
                            rom.TimeStamp = timeStamp;
                        }
                    }
                }
                file.Close();
            }
            if (!objConfig.show_clones && objConfig.current_gamelist == objConfig.current_emulator + "-0")
                list.RemoveAll(c => c.CloneOf != "");
            return list;
        }

        public static void SaveRomList(OBJConfig objConfig, CListBox romList)
        {
            var fileName = new StringBuilder(Application.StartupPath)
                .Append(Path.DirectorySeparatorChar).Append("data").Append(Path.DirectorySeparatorChar);
            if (objConfig.current_platform != "all_emu")
                fileName.Append(objConfig.current_gamelist);
            else
                fileName.Append(objConfig.current_platform);
            fileName.Append(".lst");
            var file = File.CreateText(fileName.ToString());
            foreach (var rom in romList.List)
            {
                if (objConfig.current_platform != "all_emu")
                {
                    file.WriteLine(rom.Key);
                    file.WriteLine(rom.Description);
                    file.WriteLine(rom.Year);
                    file.WriteLine(rom.Manufacturer);
                    file.WriteLine(rom.CloneOf);
                    file.WriteLine(rom.Bios);
                    file.WriteLine(rom.ExtraData);
                    file.WriteLine(rom.ScreenOrientation);
                    file.WriteLine(rom.InputControl);
                    file.WriteLine(rom.Status);
                    file.WriteLine(rom.Color);
                    file.WriteLine(rom.Sound);
                    file.WriteLine(rom.Category);
                }
                else
                {
                    var outputLineSB = new StringBuilder(rom.Key).Append("|");
                    outputLineSB.Append(rom.Description).Append("|");
                    outputLineSB.Append(rom.Year).Append("|");
                    outputLineSB.Append(rom.Manufacturer).Append("|");
                    outputLineSB.Append(rom.CloneOf).Append("|");
                    outputLineSB.Append(rom.Bios).Append("|");
                    outputLineSB.Append(rom.ExtraData).Append("|");
                    outputLineSB.Append(rom.ScreenOrientation).Append("|");
                    outputLineSB.Append(rom.InputControl).Append("|");
                    outputLineSB.Append(rom.Status).Append("|");
                    outputLineSB.Append(rom.Color).Append("|");
                    outputLineSB.Append(rom.Sound).Append("|");
                    outputLineSB.Append(rom.Category).Append("|");
                    outputLineSB.Append(rom.FeelInfo.ToString());
                    file.WriteLine(outputLineSB.ToString());
                }
            }
            file.Close();
        }

        public static bool AddGameToList(OBJConfig objConfig, CListBox romList, RomDesc rom)
        {
            if (romList.AddGame(rom))
            {
                SaveRomList(objConfig, romList);
                return true;
            }
            return false;
        }

        public static RomDesc RemoveGameFromList(OBJConfig objConfig, CListBox romList, RomDesc rom)
        {
            var currentRom = romList.RemoveGame(rom);
            SaveRomList(objConfig, romList);
            return currentRom;
        }

        public static void SetLastPlayedGame(OBJConfig objConfig, RomDesc currentRom)
        {
            var lastPlayed = new GameRunChain(objConfig.current_platform, objConfig.current_emulator, objConfig.current_gamelist, currentRom.Key);
            objConfig.SetParameter("last_game_played", lastPlayed.ToString());
        }

        public static void UpdateRomStats(OBJConfig objConfig, RomDesc currentRom)
        {
            var statFileName = objConfig.current_platform == "all_emu" ? "all_emu" : objConfig.current_emulator;
            var filePath = new StringBuilder(Application.StartupPath).Append(Path.DirectorySeparatorChar).Append("data")
                .Append(Path.DirectorySeparatorChar).Append(statFileName).Append(".sts").ToString();
            if (!File.Exists(filePath))
            {
                var file = File.CreateText(filePath);
                file.Close();
            }
            var lines = new List<string>(File.ReadAllLines(filePath));
            var lineValue = new StringBuilder(currentRom.Key).Append("|").Append(currentRom.PlayedCount).Append("|").Append(currentRom.PlayedTime).Append("|").Append(currentRom.TimeStamp.ToString("dd/MM/yyyy HH:mm:ss", cultureInfo)).ToString();
            var item = lines.FindIndex(0, c => c.StartsWith(currentRom.Key + "|"));
            if (item >= 0)
                lines[item] = lineValue;
            else
                lines.Add(lineValue);
            File.WriteAllLines(filePath, lines.ToArray());
        }

        public static string LabelCleanup(string inputString)
        {
            // cleanup not printable characters
            var outputString = string.Empty;
            for (var iLoop2 = 0; iLoop2 < inputString.Length; iLoop2++)
            {
                if (inputString[iLoop2] >= 32 && inputString[iLoop2] <= 187)
                    outputString += inputString[iLoop2];
                else
                    outputString += "_";
            }
            return outputString;
        }

        #region Game Info
        public static string GetGameInfo(OBJConfig objConfig, RomDesc currentRom, CListBox romList)
        {
            // Rom Info
            var ranking = currentRom.PlayedTimeNow != TimeSpan.Zero ? currentRom.PlayedTimeNow.TotalSeconds / romList.MaxPlayedTimeNow.TotalSeconds : 0.0;
            var stars = 0;
            if (ranking > 0.0)
                stars = (int)Math.Round(ranking * 5, 0);
            var perc = ranking > 0 ? Math.Round(ranking * 100, 0) : 0.0;
            var str = "Ranking§" + perc + "%";
            if (stars > 0)
            {
                str += " (" + "".PadRight(stars, '*') + ")";
            }
            str += "\nPlayed§" + currentRom.PlayedCount + (currentRom.PlayedCount == 1 ? " time" : " times");
            str += "\nLast played§" + TimePassedBy(currentRom.TimeStamp);
            str += "\nTotal time§" + TimeSpent(currentRom.PlayedTime);
            var usage = romList.TotalPlayedTime.TotalSeconds > 0 ? Math.Round((currentRom.PlayedTime.TotalSeconds * 100) / romList.TotalPlayedTime.TotalSeconds, 0) : 0.0;
            str += "\nOn overall time§" + usage + "%";

            if (objConfig.current_platform == "all_emu")
            {
                str += "\n\nPlatform§" + currentRom.FeelInfo.PlatformDesc;
                str += "\nEmulator§" + currentRom.FeelInfo.EmulatorDesc;
            }

            return str;
        }

        private static string TimeSpent(TimeSpan time)
        {
            var ret = "";
            if ((int)time.TotalDays > 0)
            {
                ret += (int)time.TotalDays + ((int)time.TotalDays == 1 ? " day" : " days");
            }
            else if ((int)time.TotalHours > 0)
            {
                ret += (int)time.TotalHours + ((int)time.TotalHours == 1 ? " hour" : " hrs");
            }
            else if ((int)time.TotalMinutes > 0)
            {
                ret += (int)time.TotalMinutes + ((int)time.TotalMinutes == 1 ? " min" : " mins");
            }
            else if ((int)time.TotalSeconds > 0)
            {
                ret += (int)time.TotalSeconds + ((int)time.TotalSeconds == 1 ? " sec" : " secs");
            }
            if (ret.Length == 0)
                ret = "-";
            return ret;
        }

        private static string TimePassedBy(DateTime dateFrom)
        {
            var ret = string.Empty;
            if (dateFrom == DateTime.MinValue)
                ret = "-";
            else
            {
                var elapsed = DateTime.Now - dateFrom;
                if (elapsed.TotalDays > 365)
                    ret = "> 1 year ago";
                else if (elapsed.TotalDays > 30)
                    ret = (int)(elapsed.Days / 30) + " months ago";
                else if (elapsed.TotalDays > 1)
                    ret = (int)elapsed.TotalDays + " day" + ((int)elapsed.TotalDays != 1 ? "s" : string.Empty) + " ago";
                else if (elapsed.TotalHours > 1)
                    ret = (int)elapsed.TotalHours + " hr" + ((int)elapsed.TotalHours != 1 ? "s" : string.Empty) + " ago";
                else if (elapsed.TotalMinutes > 1)
                    ret = (int)elapsed.TotalMinutes + " min" + ((int)elapsed.TotalMinutes != 1 ? "s" : string.Empty) + " ago";
                else
                    ret = "now";
            }
            return ret;
        }

        public static string GetGameHiscore(OBJConfig objConfig, RomDesc currentRom)
        {
            var str = string.Empty;
            if (objConfig.list_type == ListType.mame_xml_list)
            {
                // Hi-Score
                var hiscoreString = ReadHiscore(objConfig, objConfig.hiscore_path + Path.DirectorySeparatorChar + currentRom.Key + ".usr");
                if (hiscoreString.StartsWith("error", StringComparison.CurrentCultureIgnoreCase))
                {
                    hiscoreString = ReadHiscore(objConfig, objConfig.hiscore_path + Path.DirectorySeparatorChar + currentRom.Key + ".hi");
                    if (hiscoreString.StartsWith("error", StringComparison.CurrentCultureIgnoreCase))
                    {
                        hiscoreString = ReadHiscore(objConfig, objConfig.nvram_path + Path.DirectorySeparatorChar + currentRom.Key + ".nv");
                        if (hiscoreString.StartsWith("error", StringComparison.CurrentCultureIgnoreCase))
                            hiscoreString = string.Empty;
                    }
                }
                if (!string.IsNullOrEmpty(hiscoreString))
                {
                    var firstNRecords = 0;
                    var hiscoreShort = string.Empty;
                    foreach (var line in hiscoreString.Split('\n'))
                    {
                        var cleanedline = line.Replace("\r", "");
                        if (cleanedline != string.Empty)
                        {
                            if (firstNRecords++ > 6) break;
                            hiscoreShort += cleanedline + "\n";
                        }
                    }
                    hiscoreShort = hiscoreShort.Substring(0, hiscoreShort.Length - 1);
                    str += "*#*TC*#*" + hiscoreShort.Replace('|', '§') + "*#*/T*#*";
                }
            }
            return str;
        }

        private static string ReadHiscore(OBJConfig objConfig, string fileName)
        {
            var str = "error";
            try
            {
                if (!File.Exists(fileName))
                    return str;
                if (Path.GetExtension(fileName) == ".usr")
                {
                    var file = File.OpenText(fileName);
                    str = file.ReadToEnd();
                    file.Close();
                }
                else
                {
                    var folder = Path.GetDirectoryName(objConfig.hitotext_exe_path);
                    if (Directory.Exists(folder))
                    {
                        var startInfo = new ProcessStartInfo();
                        startInfo.CreateNoWindow = true;
                        startInfo.UseShellExecute = false;
                        startInfo.FileName = objConfig.hitotext_exe_path;
                        startInfo.WorkingDirectory = folder;
                        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        startInfo.Arguments = "-r " + fileName;
                        startInfo.RedirectStandardOutput = true;
                        startInfo.RedirectStandardError = true;
                        using (var exeProcess = Process.Start(startInfo))
                        {
                            using (var reader = exeProcess.StandardOutput)
                                str = reader.ReadToEnd();
                        }
                    }
                }
            }
            catch
            {
                str = "error";
            }
            return str;
        }
        #endregion

    }
}