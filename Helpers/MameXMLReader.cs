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
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Windows.Forms;

namespace feel
{
    public class MameXMLReader
    {
        private struct Catver
        {
            public string Name { get; set; }
            public string Category { get; set; }
        }

        public static List<RomDesc> GetRomList(Feel feel, string emuPath, string feelPath, feel.ListType listType, string emuCommandLine, string machine)
        {
            var xmlFile = emuPath  + Path.DirectorySeparatorChar + "mame.xml";
            var infoFile = emuPath  + Path.DirectorySeparatorChar + "mame.info";
            var catverFile = emuPath  + Path.DirectorySeparatorChar + "catver.ini";

            var romList = new List<RomDesc>();
            var reader = new XmlTextReader(xmlFile);
            var exitMessage = string.Empty;

            if ((listType == ListType.mame_xml_list && !File.Exists(xmlFile)) ||
                (listType == ListType.mame_listinfo && !File.Exists(infoFile)) ||
                (listType == ListType.mess_machine && !File.Exists(xmlFile)))
            {
                feel.ShowMessage("Creating " + xmlFile + "...", true);
                // in mess-mode, process as mame_xml_list
                exitMessage = MameXMLReader.CreateMameXML(emuPath, feelPath,
                    listType == ListType.mess_machine ? ListType.mame_xml_list : listType,
                    emuCommandLine);
            }
            if (exitMessage == string.Empty)
            {
                if (!File.Exists(xmlFile))
                    exitMessage = xmlFile + " not found.\n\nPlease check mame.xml / mame.info generation.";
                else
                {
                    try
                    {
                        var mameBuild = "";
                        var counter = 0;
                        while (reader.Read())
                        {
                            switch (reader.NodeType)
                            {
                                case XmlNodeType.Element:
                                    switch (reader.Name)
                                    {
                                        case "mame":
                                            while (reader.MoveToNextAttribute())
                                            {
                                                switch (reader.Name)
                                                {
                                                    case "build":
                                                        mameBuild = reader.Value.Split(' ')[0];
                                                        // normalize version names like 0.106u1 .. 9 to 0.106u01
                                                        // to grant proper alpha comparison
                                                        if (mameBuild.LastIndexOf("u") == mameBuild.Length - 2)
                                                            mameBuild =
                                                                mameBuild.Substring(0, mameBuild.LastIndexOf("u") + 1) +
                                                                "0" +
                                                                mameBuild.Substring(mameBuild.LastIndexOf("u") + 1);
                                                        break;
                                                }
                                            }
                                            break;
                                        case "game":
                                        case "machine":
                                            var rom = ReadRom(reader, mameBuild);
                                            if (rom != null)
                                            {
                                                if (listType == ListType.mess_machine && rom.Key == machine)
                                                {
                                                    ProcessMessRomList(feel, romList, emuPath, feelPath, listType, emuCommandLine, machine);
                                                }
                                                else if (listType != ListType.mess_machine)
                                                {
                                                    if (++counter % 100 == 0)
                                                        feel.ShowMessage("Parsing available roms: " + counter + " roms...", true);
                                                    romList.Add(rom);
                                                }
                                            }
                                            break;
                                    }
                                    break;
                            }
                        }
                    }
                    catch
                    {
                        exitMessage = "Error parsing mame.xml file";
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }

            if (exitMessage == string.Empty && File.Exists(catverFile) && listType != ListType.mess_machine)
            {
                var catverList = new List<Catver>();
                var file = File.OpenText(catverFile);
                var listCounter = 0f;
                while (!file.EndOfStream)
                {
                    var line = file.ReadLine().Trim().ToString();
                    if (line.ToLower() == "[veradded]")
                        break;
                    if (line != "" && !line.StartsWith("[") && line.Contains("="))
                    {
                        if (++listCounter % 100 == 0)
                            feel.ShowMessage("Parsing catver.ini entries: " + listCounter + " roms...", true);
                        var split = line.IndexOf('=');
                        if (split > 0)
                        {
                            var catver = new Catver();
                            catver.Name = line.Substring(0, split);
                            catver.Category = line.Substring(split + 1);
                            catverList.Add(catver);
                        }
                    }
                }
                file.Close();

                var updateCounter = 0f;
                foreach (var cat in catverList)
                {
                    if (++updateCounter % 100 == 0)
                        feel.ShowMessage("Updating roms category: " + Math.Floor(updateCounter / listCounter * 100.0f) + "%", true);
                    var items = romList.FindAll(c => (c.Key == cat.Name || c.CloneOf == cat.Name));
                    foreach (var item in items)
                    {
                        if (item.Category == "")
                            item.Category = cat.Category;
                    }
                }
            }

            if (exitMessage != string.Empty)
            {
                if ((listType == ListType.mame_xml_list || listType == ListType.mess_machine)
                    && File.Exists(xmlFile))
                {
                    exitMessage += "\nCorrupted mame.xml file: please re-run build.";
                    File.Delete(xmlFile);
                }
                if (listType == ListType.mame_listinfo && File.Exists(infoFile))
                {
                    exitMessage += "\nCorrupted mame.info file: please re-run build.";
                    File.Delete(infoFile);
                }
                feel.ShowMessage(exitMessage, false);
                return null;
            }
            return romList;
        }

        public static List<RomDesc> ProcessMessRomList(Feel feel, List<RomDesc> romList, string emuPath, string feelPath, feel.ListType listType, string emuCommandLine, string machine)
        {
            var xmlFile = emuPath + Path.DirectorySeparatorChar + "mess_sw.xml";

            var reader = new XmlTextReader(xmlFile);
            var exitMessage = string.Empty;

            if (!File.Exists(xmlFile))
                exitMessage = MameXMLReader.CreateMameXML(emuPath, feelPath, listType, emuCommandLine);
            if (exitMessage == string.Empty)
            {
                if (!File.Exists(xmlFile))
                    exitMessage = "mess_sw.xml not found.\n\nPlease check mess_sw.xml generation.";
                else
                {
                    var mameBuild = "";
                    var counter = 0;
                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                var isRightMachine = false;
                                switch (reader.Name)
                                {
                                    case "softwarelist":
                                        while (reader.MoveToNextAttribute())
                                        {
                                            switch (reader.Name)
                                            {
                                                case "name":
                                                    if (reader.Value == machine)
                                                    {
                                                        isRightMachine = true;
                                                    }
                                                    break;
                                            }
                                        }
                                        break;
                                    case "software":
                                        var rom = ReadRom(reader, mameBuild);
                                        if (rom != null)
                                        {
                                            if (++counter % 100 == 0)
                                                feel.ShowMessage("Parsing available roms: " + counter + " roms...", true);
                                            romList.Add(rom);
                                        }
                                        break;
                                }
                                if (!isRightMachine)
                                    continue;
                                break;
                        }
                    }
                }
            }

            if (exitMessage != string.Empty)
            {
                feel.ShowMessage(exitMessage, false);
                return null;
            }
            return romList;
        }

        private static RomDesc ReadRom(XmlReader reader, string mameBuild)
        {
            var xmlRom = reader.ReadSubtree();
            var rom = new RomDesc();
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "name":
                        rom.Key = reader.Value;
                        break;
                    case "cloneof":
                        rom.CloneOf = reader.Value;
                        break;
                    case "romof":
                        rom.Bios = reader.Value;
                        break;
                    case "isbios":
                        return null;
                }
            }
            while (xmlRom.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.Name)
                        {
                            case "description":
                                try
                                {
                                    reader.Read();
                                    rom.Description = reader.Value;
                                }
                                catch
                                {
                                    rom.Description = rom.Key;
                                }
                                break;
                            case "year":
                                reader.Read();
                                rom.Year = reader.Value;
                                break;
                            case "manufacturer":
                            case "publisher":
                                reader.Read();
                                rom.Manufacturer = reader.Value;
                                break;
                            case "rom":
                                break;
                            case "disk":
                                //rom.HaveCHDFile = true;
                                break;
                            case "chip":
                                break;
                            case "display":
                                while (reader.MoveToNextAttribute())
                                {
                                    switch (reader.Name)
                                    {
                                        case "type":
                                            rom.VideoType = reader.Value;
                                            break;
                                        case "rotate":
                                            rom.ScreenOrientation = (reader.Value == "90" || reader.Value == "270") ? "Vertical" : "Horizontal";
                                            break;
                                        case "width":
                                            //rom.Display.Width = Convert.ToInt32(reader.Value);
                                            break;
                                        case "height":
                                            //rom.Display.Height = Convert.ToInt32(reader.Value);
                                            break;
                                    }
                                }
                                break;
                            case "sound":
                                break;
                            case "input":
                                var players = 0;
                                var control = string.Empty;
                                var buttons = 0;
                                // older listxml format (ex.: <input control="joy4way"...> )
                                if (mameBuild.CompareTo("0.106u12") <= 0)
                                {
                                    while (reader.MoveToNextAttribute())
                                    {
                                        if (reader.Name == "players")
                                        {
                                            players = int.Parse(reader.Value);
                                            continue;
                                        }
                                        if (reader.Name == "control")
                                        {
                                            if (!String.IsNullOrEmpty(control)) control += ",";
                                            control += reader.Value;
                                            continue;
                                        }
                                        if (reader.Name == "buttons")
                                        {
                                            buttons = int.Parse(reader.Value);
                                            continue;
                                        }
                                    }
                                }
                                else if (mameBuild.CompareTo("0.162") < 0)
                                {
                                    while (reader.MoveToNextAttribute())
                                    {
                                        if (reader.Name == "buttons")
                                        {
                                            buttons = int.Parse(reader.Value);
                                            continue;
                                        }
                                        if (reader.Name == "players")
                                        {
                                            players = int.Parse(reader.Value);
                                        }
                                    }

                                    while (reader.Read())
                                    {
                                        if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "input")
                                            break;
                                        if (reader.Name == "control")
                                        {
                                            while (reader.MoveToNextAttribute())
                                            {
                                                if (reader.Name == "type")
                                                {
                                                    if (!String.IsNullOrEmpty(control)) control += ",";
                                                    control += reader.Value;
                                                    continue;
                                                }
                                            }
                                            continue;
                                        }
                                    }
                                }
                                else
                                {
                                    while (reader.MoveToNextAttribute())
                                    {
                                        if (reader.Name == "buttons")
                                        {
                                            buttons = int.Parse(reader.Value);
                                            continue;
                                        }
                                        if (reader.Name == "players")
                                        {
                                            players = int.Parse(reader.Value);
                                        }
                                    }

                                    while (reader.Read())
                                    {
                                        if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "input")
                                            break;
                                        if (reader.Name == "control")
                                        {
                                            while (reader.MoveToNextAttribute())
                                            {
                                                if (reader.Name == "type")
                                                {
                                                    if (!String.IsNullOrEmpty(control)) control += ",";
                                                    control += reader.Value;
                                                }
                                                if (reader.Name == "ways")
                                                {
                                                    control += reader.Value + "way";
                                                    continue;
                                                }
                                            }
                                            continue;
                                        }
                                    }
                                }
                                rom.InputControl = control + (players > 0 ? (control != string.Empty ? " - " : string.Empty) + players + "P" : string.Empty) + (buttons > 0 ? " - " + buttons + "Bt" : string.Empty);
                                break;
                            case "driver":
                                while (reader.MoveToNextAttribute())
                                {
                                    switch (reader.Name)
                                    {
                                        case "status":
                                            rom.Status = reader.Value;
                                            break;
                                        case "emulation":
                                            //rom.Driver.Emulation = reader.Value;
                                            break;
                                        case "color":
                                            rom.Color = reader.Value;
                                            break;
                                        case "sound":
                                            rom.Sound = reader.Value;
                                            break;
                                        case "graphic":
                                            //rom.Driver.Graphic = reader.Value;
                                            break;
                                        case "protection":
                                            //rom.Driver.Protection = reader.Value;
                                            break;
                                        case "savestate":
                                            //rom.Driver.SaveState = reader.Value;
                                            break;
                                    }
                                }
                                break;
                        }
                        break;
                }
            }
            return rom;
        }

        private static string CreateMameXML(string emuPath, string feelPath, feel.ListType listType, string emuCommandLine)
        {
            var exitMessage = string.Empty;
            var xmlFile = "mame.xml";
            var infoFile = "mame.info";
            var messXmlFile = "mess_sw.xml";

            if (listType == ListType.mame_xml_list && File.Exists(emuPath + Path.DirectorySeparatorChar + xmlFile))
                File.Delete(emuPath  + Path.DirectorySeparatorChar  + xmlFile);
            if (listType == ListType.mame_listinfo && File.Exists(emuPath + Path.DirectorySeparatorChar + infoFile))
                File.Delete(emuPath  + Path.DirectorySeparatorChar  + infoFile);
            if (listType == ListType.mess_machine && File.Exists(emuPath + Path.DirectorySeparatorChar + messXmlFile))
                File.Delete(emuPath + Path.DirectorySeparatorChar + messXmlFile);

            var cmdParams = string.Empty;
            switch (listType)
            {
                case ListType.mame_xml_list:
                    cmdParams = " -listxml > " + xmlFile;
                    break;
                case ListType.mame_listinfo:
                    cmdParams = " -listinfo > " + infoFile;
                    break;
                case ListType.mess_machine:
                    cmdParams = " -listsoftware > " + messXmlFile;
                    break;
            }
            var listPlainCommandLine = "\"" + emuCommandLine + "\"" + cmdParams;
            var listCommandLine = "\"" + emuCommandLine + "\"" + cmdParams;

            var startInfo = new ProcessStartInfo("cmd.exe");
            startInfo.Arguments = "/C " + listCommandLine;
            startInfo.WorkingDirectory = emuPath;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            var exitCode = 0;
            using (var exeProcess = Process.Start(startInfo))
            {
                exeProcess.WaitForExit();
                exitCode = exeProcess.ExitCode;
            }

            if (exitCode != 0)
            {
                // wrong files cleanup
                if (listType == ListType.mame_xml_list && File.Exists(emuPath + Path.DirectorySeparatorChar + xmlFile))
                    File.Delete(emuPath + Path.DirectorySeparatorChar + xmlFile);
                if (listType == ListType.mame_listinfo && File.Exists(emuPath + Path.DirectorySeparatorChar + infoFile))
                    File.Delete(emuPath + Path.DirectorySeparatorChar + infoFile);
                if (listType == ListType.mess_machine && File.Exists(emuPath + Path.DirectorySeparatorChar + messXmlFile))
                    File.Delete(emuPath + Path.DirectorySeparatorChar + messXmlFile);
                exitMessage = "Error executing command\n" + Utils.ShortenString(listPlainCommandLine, 30) + "\nto create game info xml file.\n\nPlease check MAME emulator_commandline parameter.";
            }

            // convert listinfo thru datutil
            if (exitMessage == string.Empty && listType == ListType.mame_listinfo)
            {
                var datUtilCommandLine = "datutil -k -f listxml -o " + Utils.ConvertToDosPath(emuPath)  + Path.DirectorySeparatorChar + "mame.xml " +
                        Utils.ConvertToDosPath(emuPath)  + Path.DirectorySeparatorChar + "mame.info ";
                startInfo.Arguments = "/C " + datUtilCommandLine;
                startInfo.WorkingDirectory = Utils.ConvertToDosPath(feelPath);
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = false;
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                using (var exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                    exitCode = exeProcess.ExitCode;
                }
                if (exitCode != 0)
                    exitMessage = "Error executing command\n" + datUtilCommandLine.Substring(0, 20) + "...\nto create game info xml file.\nHave you installed datutil.exe?";
            }

            return exitMessage;
        }
    }
}
