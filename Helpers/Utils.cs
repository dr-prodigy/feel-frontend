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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Shell32;
using Color = Microsoft.Xna.Framework.Graphics.Color;

namespace feel
{
    class Utils
    {
        [DllImport("user32.dll")]
        private static extern int EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);
        [DllImport("user32.dll")]
        private static extern int ChangeDisplaySettings(ref DEVMODE devMode, int flags);
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern int ShowCursor(bool bShow);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern uint GetShortPathName([MarshalAs(UnmanagedType.LPTStr)]   string lpszLongPath, [MarshalAs(UnmanagedType.LPTStr)]   StringBuilder lpszShortPath, uint cchBuffer);

        private const int ENUM_CURRENT_SETTINGS = -1;

        private const int CDS_UPDATEREGISTRY = 0x00000001;
        private const int CDS_TEST = 0x00000002;
        private const int CDS_FULLSCREEN = 0x00000004;
        private const int CDS_SET_PRIMARY = 0x00000010;
        private const int CDS_NORESET = 0x10000000;
        private const int CDS_RESET = 0x40000000;

        private const int DISP_CHANGE_SUCCESSFUL = 0;
        private const int DISP_CHANGE_RESTART = 1;
        private const int DISP_CHANGE_FAILED = -1;

        [StructLayout(LayoutKind.Sequential)]
        private struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;

            public short dmOrientation;
            public short dmPaperSize;
            public short dmPaperLength;
            public short dmPaperWidth;

            public short dmScale;
            public short dmCopies;
            public short dmDefaultSource;
            public short dmPrintQuality;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;
            public short dmLogPixels;
            public short dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;

            public int dmDisplayFlags;
            public int dmDisplayFrequency;

            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;

            public int dmPanningWidth;
            public int dmPanningHeight;
        };

        // disable warning: initialized by Marshal.PtrToStructure
        #pragma warning disable 0649
        private struct BitmapInfoHeader
        {
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public uint biCompression;
            public uint biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;
        }
        #pragma warning restore 0649

        public static int ShowHideCursor(bool show)
        {
            return ShowCursor(show);
        }

        public static bool ShowHideWindow(string lpClassName, string lpWindowName, bool show)
        {
            int val = (show ? 1 : 0);
            IntPtr hWnd = FindWindow(lpClassName, lpWindowName);
            if (hWnd != IntPtr.Zero)
                return ShowWindow(hWnd, val);
            return false;
        }

        public static string ConvertToDosPath(string path)
        {
            uint bufferSize = 256;
            StringBuilder shortNameBuffer = new StringBuilder((int)bufferSize);
            uint result = GetShortPathName(path, shortNameBuffer, bufferSize);

            return shortNameBuffer.ToString();
        }

        public static Size ScaleDimension(int srcWidth, int srcHeight, int destWidth, int destHeight)
        {
            var factorW = (double)destWidth / (double)srcWidth;
            var factorH = (double)destHeight / (double)srcHeight;
            var factor = Math.Min(factorW, factorH);
            var newSize = new Size((int)((double)srcWidth * factor), (int)((double)srcHeight * factor));
            return newSize;
        }

        public static Vector2 VectScaleImage(int srcWidth, int srcHeight, int destWidth, int destHeight)
        {
            var factorW = (double)destWidth / (double)srcWidth;
            var factorH = (double)destHeight / (double)srcHeight;
            var factor = Math.Min(factorW, factorH);
            var newSize = new Vector2((int)((double)srcWidth * factor), (int)((double)srcHeight * factor));
            return newSize;
        }

        public static void CreateLog(CultureInfo cultureInfo)
        {
            var file = File.CreateText(Application.StartupPath  + Path.DirectorySeparatorChar + "feel.log");
            file.WriteLine("**** " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss", cultureInfo) + " - Start Front-End Emulator Launcher ****");
            file.Close();
        }

        public static void PrintLog(string message)
        {
            File.AppendAllText(Application.StartupPath  + Path.DirectorySeparatorChar + "feel.log", message + "\r\n", Encoding.UTF8);
        }

        public static string[] GetFiles(string romPath, string romExtension)
        {
            var extensions = romExtension.Split(',');

            var list = new List<string>();

            // recurse subdirectories
            foreach (var dir in Directory.GetDirectories(romPath, "*"))
                list.AddRange(GetFiles(dir, romExtension));

            // add roms
            foreach (string ext in extensions)
                list.AddRange(Directory.GetFiles(romPath, "*." + ext.Trim()));

            return (string[])list.ToArray();
        }

        public static string GetRomExtension(string romPath, string romName, string romExtension)
        {
            if (romExtension.IndexOf(',') > -1)
            {
                var extensions = romExtension.Split(',');

                foreach (string ext in extensions)
                {
                    if (File.Exists(romPath + Path.DirectorySeparatorChar + romName + "." + ext.Trim()))
                        return ext.Trim();
                }
                return "";
            }
            else
                return romExtension;
        }

        public static Bitmap ScaleBitmap(Bitmap srcBmp, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height);
            Graphics graphic = Graphics.FromImage((Image)bmp);
            graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphic.Clear(System.Drawing.Color.Black);
            graphic.DrawImage(srcBmp, 0, 0, width, height);
            graphic.Dispose();
            srcBmp.Dispose();
            srcBmp = null;
            return bmp;
        }

        public static Bitmap RemoveBlackZoneFromBitmap(Bitmap srcBmp)
        {
            var x = 0;
            var y = 0;
            var width = srcBmp.Width;
            var height = srcBmp.Height;
            // LEFT
            var found = false;
            for (var vx = 0; vx < srcBmp.Width; vx++)
            {
                for (var vy = 0; vy < srcBmp.Height; vy++)
                {
                    if (srcBmp.GetPixel(vx, vy).ToArgb() != System.Drawing.Color.Black.ToArgb())
                    {
                        x = vx;
                        found = true;
                        break;
                    }
                }
                if (found)
                    break;
            }
            // RIGHT
            found = false;
            for (var vx = srcBmp.Width - 1; vx > x; vx--)
            {
                for (var vy = 0; vy < srcBmp.Height; vy++)
                {
                    if (srcBmp.GetPixel(vx, vy).ToArgb() != System.Drawing.Color.Black.ToArgb())
                    {
                        width = vx - x + 1;
                        found = true;
                        break;
                    }
                }
                if (found)
                    break;
            }
            // TOP
            found = false;
            for (var vy = 0; vy < srcBmp.Height; vy++)
            {
                for (var vx = 0; vx < srcBmp.Width; vx++)
                {
                    if (srcBmp.GetPixel(vx, vy).ToArgb() != System.Drawing.Color.Black.ToArgb())
                    {
                        y = vy;
                        found = true;
                        break;
                    }
                }
                if (found)
                    break;
            }
            // BOTTOM
            found = false;
            for (var vy = srcBmp.Height - 1; vy > y; vy--)
            {
                for (var vx = 0; vx < srcBmp.Width; vx++)
                {
                    if (srcBmp.GetPixel(vx, vy).ToArgb() != System.Drawing.Color.Black.ToArgb())
                    {
                        height = vy - y + 1;
                        found = true;
                        break;
                    }
                }
                if (found)
                    break;
            }
            // Only pair values (for video compatibility)
            if ((double)width / 2 != width / 2) { width--; }
            if ((double)height / 2 != height / 2) { height--; }
            // Bitmap
            Bitmap bmp = new Bitmap(width, height);
            Graphics graphic = Graphics.FromImage((Image)bmp);
            graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphic.Clear(System.Drawing.Color.Black);
            graphic.DrawImage(srcBmp, 0, 0, new System.Drawing.Rectangle(x, y, width, height), GraphicsUnit.Pixel);
            graphic.Dispose();
            srcBmp.Dispose();
            srcBmp = null;
            return bmp;
        }

        public static void GetBitmapInfo(byte[] data, out int width, out int height, out int bpp)
        {
            var infoHeader = new BitmapInfoHeader();

            int pos = 0;
            int rawsize = Marshal.SizeOf(infoHeader.GetType());
            IntPtr buffer = Marshal.AllocHGlobal(rawsize);
            Marshal.Copy(data, pos, buffer, rawsize);
            infoHeader = (BitmapInfoHeader)Marshal.PtrToStructure(buffer, infoHeader.GetType());
            Marshal.FreeHGlobal(buffer);

            width = infoHeader.biWidth;
            height = infoHeader.biHeight;
            bpp = infoHeader.biBitCount;
        }

        public static FeelKey GetFeelKeyFromIniString(string key)
        {
            switch (key.ToUpper())
            {
                case "BACKSPACE":
                    return FeelKey.Back;
                case "TAB":
                    return FeelKey.Tab;
                case "ENTER":
                case "RETURN":
                    return FeelKey.Enter;
                case "PAUSE":
                    return FeelKey.Pause;
                case "ESC":
                case "ESCAPE":
                    return FeelKey.Esc;
                case "SPACE":
                    return FeelKey.Space;
                case "PAGEUP":
                    return FeelKey.PageUp;
                case "PAGEDOWN":
                    return FeelKey.PageDown;
                case "END":
                    return FeelKey.End;
                case "HOME":
                    return FeelKey.Home;
                case "LEFT":
                    return FeelKey.Left;
                case "UP":
                    return FeelKey.Up;
                case "RIGHT":
                    return FeelKey.Right;
                case "DOWN":
                    return FeelKey.Down;
                case "INS":
                case "INSERT":
                    return FeelKey.Insert;
                case "DEL":
                case "DELETE":
                    return FeelKey.Delete;
                case "0":
                    return FeelKey.D0;
                case "1":
                    return FeelKey.D1;
                case "2":
                    return FeelKey.D2;
                case "3":
                    return FeelKey.D3;
                case "4":
                    return FeelKey.D4;
                case "5":
                    return FeelKey.D5;
                case "6":
                    return FeelKey.D6;
                case "7":
                    return FeelKey.D7;
                case "8":
                    return FeelKey.D8;
                case "9":
                    return FeelKey.D9;
                case "A":
                    return FeelKey.A;
                case "B":
                    return FeelKey.B;
                case "C":
                    return FeelKey.C;
                case "D":
                    return FeelKey.D;
                case "E":
                    return FeelKey.E;
                case "F":
                    return FeelKey.F;
                case "G":
                    return FeelKey.G;
                case "H":
                    return FeelKey.H;
                case "I":
                    return FeelKey.I;
                case "J":
                    return FeelKey.J;
                case "K":
                    return FeelKey.K;
                case "L":
                    return FeelKey.L;
                case "M":
                    return FeelKey.M;
                case "N":
                    return FeelKey.N;
                case "O":
                    return FeelKey.O;
                case "P":
                    return FeelKey.P;
                case "Q":
                    return FeelKey.Q;
                case "R":
                    return FeelKey.R;
                case "S":
                    return FeelKey.S;
                case "T":
                    return FeelKey.T;
                case "U":
                    return FeelKey.U;
                case "V":
                    return FeelKey.V;
                case "W":
                    return FeelKey.W;
                case "X":
                    return FeelKey.X;
                case "Y":
                    return FeelKey.Y;
                case "Z":
                    return FeelKey.Z;
                case "LWIN":
                    return FeelKey.LWin;
                case "RWIN":
                    return FeelKey.RWin;
                case "NUMPAD0":
                    return FeelKey.NumPad0;
                case "NUMPAD1":
                    return FeelKey.NumPad1;
                case "NUMPAD2":
                    return FeelKey.NumPad2;
                case "NUMPAD3":
                    return FeelKey.NumPad3;
                case "NUMPAD4":
                    return FeelKey.NumPad4;
                case "NUMPAD5":
                    return FeelKey.NumPad5;
                case "NUMPAD6":
                    return FeelKey.NumPad6;
                case "NUMPAD7":
                    return FeelKey.NumPad7;
                case "NUMPAD8":
                    return FeelKey.NumPad8;
                case "NUMPAD9":
                    return FeelKey.NumPad9;
                case "MULTIPLY":
                    return FeelKey.Multiply;
                case "ADD":
                    return FeelKey.Add;
                case "SUBTRACT":
                    return FeelKey.Subtract;
                case "DIVIDE":
                    return FeelKey.Divide;
                case "F1":
                    return FeelKey.F1;
                case "F2":
                    return FeelKey.F2;
                case "F3":
                    return FeelKey.F3;
                case "F4":
                    return FeelKey.F4;
                case "F5":
                    return FeelKey.F5;
                case "F6":
                    return FeelKey.F6;
                case "F7":
                    return FeelKey.F7;
                case "F8":
                    return FeelKey.F8;
                case "F9":
                    return FeelKey.F9;
                case "F10":
                    return FeelKey.F10;
                case "F11":
                    return FeelKey.F11;
                case "F12":
                    return FeelKey.F12;
                case "LSHIFT":
                    return FeelKey.LShift;
                case "RSHIFT":
                    return FeelKey.RShift;
                case "LCTRL":
                case "LCONTROL":
                    return FeelKey.LCtrl;
                case "RCTRL":
                case "RCONTROL":
                    return FeelKey.RCtrl;
                case "LALT":
                    return FeelKey.LAlt;
                case "RALT":
                    return FeelKey.RAlt;
                case "JOYUP":
                    return FeelKey.JoyUp;
                case "JOYDOWN":
                    return FeelKey.JoyDown;
                case "JOYLEFT":
                    return FeelKey.JoyLeft;
                case "JOYRIGHT":
                    return FeelKey.JoyRight;
                case "JOYB1":
                    return FeelKey.JoyB1;
                case "JOYB2":
                    return FeelKey.JoyB2;
                case "JOYB3":
                    return FeelKey.JoyB3;
                case "JOYB4":
                    return FeelKey.JoyB4;
                case "JOYB5":
                    return FeelKey.JoyB5;
                case "JOYB6":
                    return FeelKey.JoyB6;
                case "JOYB7":
                    return FeelKey.JoyB7;
                case "JOYB8":
                    return FeelKey.JoyB8;
                case "JOYB9":
                    return FeelKey.JoyB9;
                case "JOYB10":
                    return FeelKey.JoyB10;
                case "JOYB11":
                    return FeelKey.JoyB11;
                case "JOYB12":
                    return FeelKey.JoyB12;
                case "JOYB13":
                    return FeelKey.JoyB13;
                case "JOYB14":
                    return FeelKey.JoyB14;
                case "JOYB15":
                    return FeelKey.JoyB15;
                case "NONE":
                default:
                    return FeelKey.None;
            }
        }

        private static FileInfo[] spriteFonts = null;
        public static SpriteFont LoadSpriteFont(Game xnaGame, string fontName, int fontSize, System.Drawing.FontStyle fontStyle)
        {
            // default font supported sizes
            int[] defaultSupportedSizes = { 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 16, 18, 20, 24, 26, 30, 32, 36, 40, 45 };

            // lazy content list initialization
            if (spriteFonts == null)
            {
                DirectoryInfo dir = new DirectoryInfo(xnaGame.Content.RootDirectory);
                if (!dir.Exists)
                    throw new DirectoryNotFoundException();
                spriteFonts = dir.GetFiles("*.xnb");
            }

            fontName = fontName.ToLowerInvariant();
            var fontStylePostfix = "";
            switch (fontStyle)
            {
                case FontStyle.Bold: fontStylePostfix = "b"; break;
                case FontStyle.Italic: fontStylePostfix = "i"; break;
            }

            List<int> availableSizes = new List<int>();
            foreach (var spriteFont in spriteFonts)
            {
                var curFontName = spriteFont.Name.Split('.')[spriteFont.Name.Split('.').Length - 2].ToLowerInvariant();
                // if family is right, and style too
                if (spriteFont.Name.ToLowerInvariant().StartsWith(fontName))
                    if ((fontStylePostfix != "" && curFontName.EndsWith(fontStylePostfix)) ||
                        (fontStylePostfix == "" && !curFontName.EndsWith("b") && !curFontName.EndsWith("b")))
                    {
                        // add size to available ones
                        var size = spriteFont.Name.Split('.')[spriteFont.Name.Split('.').Length - 2].Substring(fontName.Length);
                        if (size.EndsWith("b") || size.EndsWith("i"))
                        {
                            size = size.Remove(size.Length - 1);
                        }
                        availableSizes.Add(int.Parse(size));
                    }
            }
            availableSizes.Sort();

            // if no available size, default to arial
            if (availableSizes.Count == 0)
            {
                fontName = "arial";
                availableSizes.AddRange(defaultSupportedSizes);
            }

            if (fontSize <= 0)
            {
                // if font size is not specified, default to 6
                fontSize = 6;
            }
            else if (fontSize > availableSizes[availableSizes.Count - 1])
            {
                // if larger than max, use max
                fontSize = availableSizes[availableSizes.Count - 1];
            }

            // locate nearest size
            for (var iLoop = 0; iLoop < availableSizes.Count; iLoop++)
            {
                if (availableSizes[iLoop] == fontSize)
                {
                    // found matching size: exit
                    break;
                }
                else if (availableSizes[iLoop] > fontSize)
                {
                    // matching size not supported: check which size (current/last one, if present) is nearer
                    if (iLoop > 0 && Math.Abs(availableSizes[iLoop] - fontSize) > Math.Abs(availableSizes[iLoop - 1] - fontSize))
                    {
                        // last one
                        fontSize = availableSizes[iLoop - 1];
                        break;
                    }
                    // current
                    fontSize = availableSizes[iLoop];
                    break;
                }
            }

            // load SpriteFont
            return xnaGame.Content.Load<SpriteFont>(fontName + fontSize + fontStylePostfix);
        }

        public static string StringCleanup(string value)
        {
            var cleanText = "";
            for (var iLoop = 0; iLoop < value.Length; iLoop++)
            {
                if (value[iLoop] >= ' ' && value[iLoop] <= '»')
                    cleanText += value[iLoop];
            }
            return cleanText;
        }

        public static FontStyle GetFontStyleFromIniString(string str)
        {
            var value = int.Parse(str);

            if (value > 0 && value < 16)
                return (FontStyle)value;

            return FontStyle.Regular;
        }

        public static ScreenSaverType GetScreenSaverTypeFromIniString(string str)
        {
            var value = int.Parse(str);

            if (value > 0 && value < Enum.GetValues(typeof(ScreenSaverType)).Length)
                return (ScreenSaverType)value;

            return ScreenSaverType.None;
        }

        public static ListType GetListTypeFromIniString(string str)
        {
            var value = int.Parse(str);

            if (value > 0 && value < Enum.GetValues(typeof(ListType)).Length)
                return (ListType)value;

            return ListType.rom_list;
        }

        public static SortType GetSortTypeFromIniString(string str)
        {
            var value = int.Parse(str);

            if (value > 0 && value < 7)
                return (SortType)value;

            return SortType.AZ;
        }

        public static FnetSortType GetFnetSortTypeFromIniString(string str)
        {
            try
            {
                var value = int.Parse(str);

                if (value > 0 && value < 2)
                    return (FnetSortType)value;
            }
            catch { }

            return FnetSortType.LastPlayed;
        }


        public static UseMouse GetUseMouseFromIniString(string str)
        {
            var value = int.Parse(str);

            if (value > 0 && value < 3)
                return (UseMouse)value;

            return UseMouse.No;
        }

        public static SmartAsdMode GetSmartAsdModeFromIniString(string str)
        {
            var value = int.Parse(str);

            if (value > 0 && value < 4)
                return (SmartAsdMode)value;

            return SmartAsdMode.None;
        }

        public static TextAlign GetTextAlignFromIniString(string str)
        {
            var value = int.Parse(str);

            if (value > 0 && value < 3)
                return (TextAlign)value;

            return TextAlign.Left;
        }

        public static AutostartMode GetAutostartFromIniString(string str)
        {
            var value = int.Parse(str);

            if (value > 0 && value < 4)
                return (AutostartMode)value;

            return AutostartMode.Off;
        }

        public static Color GetColorFromIniString(string str)
        {
            var color = Color.TransparentWhite;

            var arg = str.Replace(" ", "").Split(',');
            if (arg.Length >= 3)
            {
                var red = int.Parse(arg[0]);
                if (red >= 0 && red <= 255)
                {
                    var green = int.Parse(arg[1]);
                    if (green >= 0 && green <= 255)
                    {
                        var blue = int.Parse(arg[2]);
                        if (blue >= 0 && blue <= 255)
                        {
                            if (arg.Length == 4)
                            {
                                var alpha = int.Parse(arg[3]);
                                if (alpha >= 0 && alpha <= 255)
                                    return new Color((byte)red, (byte)green, (byte)blue, (byte)alpha);
                            }
                            else return new Color((byte)red, (byte)green, (byte)blue);
                        }
                    }
                }
            }

            return color;
        }

        public static Color ChangeColorAlpha(Color color, byte alpha)
        {
            var newColor = new Color(color, alpha);
            return newColor;
        }

        public static void ChangeResolution(int width, int height, bool rotateScreen)
        {
            if (rotateScreen)
            {
                var tmp = width;
                width = height;
                height = tmp;
            }
            DEVMODE dm = new DEVMODE();
            dm.dmDeviceName = new String(new char[32]);
            dm.dmFormName = new String(new char[32]);
            dm.dmSize = (short)Marshal.SizeOf(dm);

            if (0 != EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref dm))
            {
                if (dm.dmPelsWidth == width && dm.dmPelsHeight == height)
                    return;

                dm.dmPelsWidth = width;
                dm.dmPelsHeight = height;

                int iRet = ChangeDisplaySettings(ref dm, CDS_TEST);

                if (iRet == DISP_CHANGE_FAILED)
                    throw new Exception("Change Resolution Failed: aborting");
                else
                {
                    iRet = ChangeDisplaySettings(ref dm, CDS_UPDATEREGISTRY);

                    switch (iRet)
                    {
                        case DISP_CHANGE_SUCCESSFUL:
                            //successful change
                            break;
                        case DISP_CHANGE_RESTART:
                            //on windows 9x series you have to restart
                            break;
                        default:
                            //failed to change
                            throw new Exception("Change Resolution Failed: aborting");
                    }
                }
            }
        }

        delegate void NetworkMethodDelegate(List<string> str);
        delegate void CallbackDelegate(string str);
        public static void RunAsynchronously(Action method, Action callback)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    method();
                }
                catch (ThreadAbortException) { /* dont report on this */ }
                {
                }
                // note: this will not be called if the thread is aborted
                if (callback != null) callback();
            });
        }

        public static string LocateLnkPath(string arguments, ref string workingDir)
        {
            if (Path.GetExtension(arguments).Equals(".lnk", StringComparison.InvariantCultureIgnoreCase))
            {
                string pathOnly = Path.GetDirectoryName(arguments);
                string filenameOnly = Path.GetFileName(arguments);
                workingDir = string.Empty;

                // fix for Shell32.IShellDispatch5 (Win7 version) not available on XP
                //Shell shell = new Shell();
                //Folder folder = shell.NameSpace(pathOnly);
                Folder folder = GetShell32NameSpace(pathOnly);

                FolderItem folderItem = folder.ParseName(filenameOnly);
                if (folderItem != null)
                {
                    ShellLinkObject link = (ShellLinkObject)folderItem.GetLink;
                    if (link.WorkingDirectory != "")
                        workingDir = link.WorkingDirectory;
                    return link.Path;
                }
            }
            return arguments;
        }

        private static Folder GetShell32NameSpace(Object folder)
        {
            Type shellAppType = Type.GetTypeFromProgID("Shell.Application");
            Object shell = Activator.CreateInstance(shellAppType);
            return (Folder)shellAppType.InvokeMember("NameSpace", System.Reflection.BindingFlags.InvokeMethod, null, shell, new object[] { folder });
        }

        public static void RestoreWindowsShell()
        {
            var startExplorer = true;
            foreach (var process in Process.GetProcesses())
            {
                if (process.ProcessName.Contains("explorer"))
                {
                    startExplorer = false;
                    break;
                }
            }
            if (startExplorer)
                Process.Start("explorer.exe");
        }

        public static bool SaveSnapShotFromClipboard(OBJConfig objConfig, RomDesc currentRom)
        {
            try
            {
                Bitmap screenShot = (Bitmap)Clipboard.GetImage();

                if (screenShot == null || objConfig.rotate_screen)
                {
                    var data = Clipboard.GetData(DataFormats.Dib) as MemoryStream;
                    var buffer = data.ToArray();
                    int width = 0;
                    int height = 0;
                    int bpp = 0;
                    Utils.GetBitmapInfo(buffer, out width, out height, out bpp);
                    PixelFormat pixF;
                    switch (bpp)
                    {
                        case 8:
                            pixF = PixelFormat.Format8bppIndexed;
                            break;
                        case 16:
                            pixF = PixelFormat.Format16bppRgb565;
                            break;
                        case 24:
                            pixF = PixelFormat.Format24bppRgb;
                            break;
                        case 32:
                            pixF = PixelFormat.Format32bppRgb;
                            break;
                        default:
                            pixF = PixelFormat.Undefined;
                            break;
                    }
                    if (pixF != PixelFormat.Undefined)
                    {
                        screenShot = new Bitmap(width, height);
                        BitmapData bData = screenShot.LockBits(new System.Drawing.Rectangle(new System.Drawing.Point(), screenShot.Size), ImageLockMode.WriteOnly, pixF);
                        Marshal.Copy(buffer, 52, bData.Scan0, buffer.Length - 52);
                        screenShot.UnlockBits(bData);
                        if (objConfig.rotate_screen)
                            screenShot.RotateFlip(RotateFlipType.Rotate90FlipY);
                        else
                            screenShot.RotateFlip(RotateFlipType.RotateNoneFlipY);
                    }
                }

                if (screenShot != null)
                {
                    if (!objConfig.screenshot_original_size)
                    {
                        Size size;
                        if (objConfig.screenshot_stretch_to_fixed_size)
                        {
                            size = objConfig.rotate_screen ? new Size(objConfig.screenshot_height, objConfig.screenshot_width) : new Size(objConfig.screenshot_width, objConfig.screenshot_height);
                        }
                        else
                        {
                            size = objConfig.rotate_screen ?
                                Utils.ScaleDimension(screenShot.Height, screenShot.Width, objConfig.screenshot_height, objConfig.screenshot_width) :
                                Utils.ScaleDimension(screenShot.Width, screenShot.Height, objConfig.screenshot_width, objConfig.screenshot_height);
                        }
                        if (screenShot.Width != objConfig.screenshot_width || screenShot.Height != objConfig.screenshot_height)
                            screenShot = Utils.ScaleBitmap(screenShot, size.Width, size.Height);
                    }
                    screenShot = Utils.RemoveBlackZoneFromBitmap(screenShot);
                    var fileName = objConfig.snapshot_path + Path.DirectorySeparatorChar + currentRom.Key + "." + objConfig.snapshot_extension;
                    screenShot.Save(fileName, objConfig.snapshot_extension.ToLower() == "jpg" ? ImageFormat.Jpeg : ImageFormat.Png);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Utils.PrintLog("Save Snapshot: " + ex.Message);
            }
            return false;
        }

        public static string ShortenString(string inputString, int length)
        {
            var shortenedString = inputString;
            if (inputString.Length > length)
            {
                shortenedString = inputString.Substring(0, length - 10) + "..." + inputString.Substring(inputString.Length - 7);
            }
            return shortenedString;
        }

        public static string GetFilenameBase(string inputPath)
        {
            return inputPath.Substring(0, inputPath.LastIndexOf("."));
        }

        public static string GetFilenameExt(string inputPath)
        {
            return inputPath.Substring(inputPath.LastIndexOf("."));
        }
    }
}
