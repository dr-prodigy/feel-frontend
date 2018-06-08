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
using System.Runtime.InteropServices;

namespace feel
{
    public static class CCursorManager
    {
        [DllImport("user32.dll")]
        private static extern IntPtr LoadCursorFromFile(string lpFileName);

        [DllImport("user32.dll")]
        private static extern IntPtr SetCursor(IntPtr hCursor);

        [DllImport("user32.dll")]
        private static extern IntPtr GetCursor();

        [DllImport("user32.dll")]
        private static extern bool SetSystemCursor(IntPtr hcur, uint id);

        private const uint OCR_NORMAL = 32512;
        private const uint OCR_WAIT = 32514;
        private const uint OCR_HAND = 32649;
        private const uint OCR_APPSTARTING = 32650;

        [DllImport("user32", CharSet = CharSet.Auto)]
        internal static extern long SystemParametersInfo(long uAction, int lpvParam, ref bool uParam, int fuWinIni);

        public struct CursorInfo
        {
            public int size;  // Or uint, but probably not necessary and
            public int flags; // isn't CLS compliant.
            public IntPtr cursor;
            public System.Drawing.Point point; // Should already marshal correctly.
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetCursorInfo(ref CursorInfo info);

        public static void HideCursors()
        {
            IntPtr cursor = LoadCursorFromFile("feel.cur");
            SetSystemCursor(cursor, OCR_NORMAL);
            cursor = LoadCursorFromFile("feel.cur");
            SetSystemCursor(cursor, OCR_HAND);
            cursor = LoadCursorFromFile("feel.cur");
            SetSystemCursor(cursor, OCR_WAIT);
            cursor = LoadCursorFromFile("feel.cur");
            SetSystemCursor(cursor, OCR_APPSTARTING);
        }

        public static void RestoreCursors()
        {
            var SPI_SETCURSORS = 0x0057;
            var uParam = false;
            SystemParametersInfo(SPI_SETCURSORS, 0, ref uParam, 0);
        }

    }
}