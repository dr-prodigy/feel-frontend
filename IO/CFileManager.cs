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

using System.IO;
using System;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace feel
{
    class CFileManager
    {
        public static string GetBio(string romName)
        {
            var line = "";
            var cleanLine = "";
            var retVal = "";

            System.IO.StreamReader file =
                new System.IO.StreamReader("C:\\Users\\mauri\\Desktop\\History.dat");

            while ((line = file.ReadLine()) != null)
            {
                if (line.StartsWith("$info") && line.Contains(romName))
                {
                    while ((line = file.ReadLine()) != null)
                    {
                        if (line.StartsWith("$end"))
                            break;

                        cleanLine = "";
                        for (var iLoop = 0; iLoop < line.Length; iLoop++)
                        {
                            if (line[iLoop] >= ' ' && line[iLoop] <= 'z')
                                cleanLine += line[iLoop];
                        }
                        retVal += cleanLine + "\n";
                    }
                    break;
                }
            }

            return retVal;
        }

        public static string GetChangelog(int textMaxWidth, SpriteFont spriteFont)
        {
            var line = "";
            var cleanLine = "";
            var retVal = "";

            System.IO.StreamReader file =
                new System.IO.StreamReader(new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).Directory.ToString() + "\\changelog.txt");

            while ((line = file.ReadLine()) != null)
            {
                cleanLine = "";
                for (var iLoop = 0; iLoop < line.Length; iLoop++)
                {
                    if (line[iLoop] >= ' ' && line[iLoop] <= 'z')
                        cleanLine += line[iLoop];
                }
                retVal += WordWrap(cleanLine, textMaxWidth, spriteFont);
            }

            return retVal;
        }

        public static string Justify(string text, int textMaxWidth, SpriteFont spriteFont)
        {
            var cleanLine = "";
            var retVal = "";

            if (!string.IsNullOrEmpty(text))
                foreach (var line in text.Split('\n'))
                {
                    cleanLine = "";
                    for (var iLoop = 0; iLoop < line.Length; iLoop++)
                    {
                        if (line[iLoop] >= ' ' && line[iLoop] <= 'z')
                            cleanLine += line[iLoop];
                    }
                    retVal += WordWrap(cleanLine, textMaxWidth, spriteFont);
                }

            return retVal;
        }

        protected const string _newline = "\n";
        private static string WordWrap(string the_string, int textMaxWidth, SpriteFont spriteFont)
        {
            int pos, next;
            StringBuilder sb = new StringBuilder();

            // Lucidity check
            if (textMaxWidth < 1)
                return the_string;

            // Parse each line of text
            for (pos = 0; pos < the_string.Length; pos = next)
            {
                // Find end of line
                int eol = the_string.IndexOf(_newline, pos);

                if (eol == -1)
                    next = eol = the_string.Length;
                else
                    next = eol + _newline.Length;

                // Copy this line of text, breaking into smaller lines as needed
                if (eol > pos)
                {
                    do
                    {
                        int len = eol - pos;

                        var text = the_string.Substring(pos);
                        if (spriteFont.MeasureString(text).X > textMaxWidth)
                            len = BreakLine(text, spriteFont, textMaxWidth);

                        sb.Append(the_string, pos, len);
                        sb.Append(_newline);

                        // Trim whitespace following break
                        pos += len;

                        while (pos < eol && Char.IsWhiteSpace(the_string[pos]))
                            pos++;

                    } while (eol > pos);
                }
                else sb.Append(_newline); // Empty line
            }
            if (string.IsNullOrEmpty(sb.ToString()))
                sb.Append(_newline);

            return sb.ToString();
        }

        /// <summary>
        /// Locates position to break the given line so as to avoid
        /// breaking words.
        /// </summary>
        /// <param name="text">String that contains line of text</param>
        /// <param name="pos">Index where line of text starts</param>
        /// <param name="max">Maximum line length</param>
        /// <returns>The modified line length</returns>
        private static int BreakLine(string text, SpriteFont spriteFont, int textMaxWidth)
        {
            // Find last whitespace in line
            var lastChar = Math.Min((int)(textMaxWidth / spriteFont.MeasureString("f").X), text.Length - 1);
            while (spriteFont.MeasureString(text.Substring(0, lastChar)).X > textMaxWidth)
                lastChar--;
            int i = lastChar;
            while (i >= 0 && !Char.IsWhiteSpace(text[i]))
                i--;
            if (i < 0)
                return lastChar; // No whitespace found; break at maximum length
            // Find start of whitespace
            while (i >= 0 && Char.IsWhiteSpace(text[i]))
                i--;
            // Return length of text before whitespace
            return i + 1;
        }
    }
}