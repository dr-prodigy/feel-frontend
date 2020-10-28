/* 
 * Copyright (c) 2011-2020 FEELTeam - Maurizio Montel.
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
using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Graphics.Color;

namespace feel
{
    class CMenuManager : CDrawable
    {
        private GraphicsDeviceManager graphics;
        private Feel feel;
        private bool showMenu = false;
        private CMenu menu;

        private static bool _showMessage;
        private List<CDrawable> messageItems = new List<CDrawable>();

        public int screenResX;
        public int screenResY;
        private bool drawRotated;

        private int menu_width;
        private int menu_min_width;
        private int menu_item_height;
        private string menu_font_name;
        private int menu_font_size;
        private FontStyle menu_font_style;
        private Color menu_font_color;
        private Color menu_backcolor;
        private Color menu_selected_font_color;
        private Color menu_selected_backcolor;
        private SpriteFont menu_sprite_font;

        public CMenuManager(ref GraphicsDeviceManager graphicsDM, ref Feel game)
        {
            graphics = graphicsDM;
            feel = game;
            menu = new CMenu();
        }

        public bool MessageShown
        {
            get
            {
                return _showMessage;
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, Transition pendingTransition)
        {
            if (!showMenu) return;
            base.Draw(gameTime, spriteBatch, pendingTransition);

            menu.Draw(gameTime, spriteBatch, pendingTransition);
        }

        public void DrawMessage(GameTime gameTime, SpriteBatch spriteBatch, Transition pendingTransition)
        {
            if (_showMessage)
                foreach (var messageItem in messageItems)
                {
                    messageItem.Draw(gameTime, spriteBatch, pendingTransition);
                }
        }

        public void Init(int resX, int resY, bool rotateScreen, int width, int itemHeight, string fontName, int fontSize, FontStyle fontStyle, Color fontColor, Color backcolor, Color selectedFontColor, Color selectedBackcolor)
        {
            screenResX = resX;
            screenResY = resY;
            drawRotated = rotateScreen;
            menu_width = menu_min_width = width;
            menu_item_height = itemHeight;
            menu_font_name = fontName;
            menu_font_size = fontSize;
            menu_font_style = fontStyle;
            menu_font_color = fontColor;
            menu_backcolor = backcolor;
            menu_selected_font_color = selectedFontColor;
            menu_selected_backcolor = selectedBackcolor;
            menu_sprite_font = Utils.LoadSpriteFont(feel, menu_font_name, menu_font_size, menu_font_style);
            menu.Init(resX, resY, rotateScreen, width, itemHeight, fontName, fontSize, fontStyle, fontColor, backcolor, selectedFontColor, selectedBackcolor);
        }

        public void ShowMenu(List<List<string>> menuCols, string titleText, string statusbarText, string currentMenu, string additionalData,
            string additionalInfo, bool menuChanged, string preselectedMenuKey, bool isWindowTextReader, string snapshotPath, string iconPath, bool showSidebar)
        {
            menu.ShowMenu(menuCols, titleText, statusbarText, currentMenu, additionalData, additionalInfo, menuChanged, preselectedMenuKey,
                isWindowTextReader, snapshotPath, iconPath, showSidebar);
            showMenu = true;
        }

        public void SelectMenuItem(string KeyName, bool menuChanged)
        {
            menu.SelectMenuItem(KeyName, menuChanged);
        }

        public bool SelectMenuDownItem()
        {
            return menu.SelectMenuDownItem();
        }

        public bool SelectMenuUpItem()
        {
            return menu.SelectMenuUpItem();
        }

        public bool SelectMenuRightItem()
        {
            return menu.SelectMenuRightItem();
        }

        public bool SelectMenuLeftItem()
        {
            return menu.SelectMenuLeftItem();
        }

        public bool SelectNextMenuPage()
        {
            return menu.SelectNextMenuPage();
        }

        public bool SelectPreviousMenuPage()
        {
            return menu.SelectPreviousMenuPage();
        }

        public KeyValuePair<string, string> MenuItemSelected { get { return menu.SelectedItem; } }

        public void HideMenu(bool immediate)
        {
            if (showMenu)
            {
                if (immediate)
                {
                    showMenu = false;
                }
                else
                {
                    menu.Hide(() => { showMenu = false; });
                }
            }
        }

        public void MenuSetFocus()
        {
            menu.SetFocus();
        }

        public int ShowMessage(string message, TextAlign textAlign)
        {
            return ShowMessage(message, textAlign, 0, 14);
        }
        public int ShowMessage(string message, TextAlign textAlign, int previousMessageHeight, int maxRows)
        {
            // split lines
            var lines = message.Split('\n');
            var lineCount = Math.Min(lines.Length, maxRows);

            // get column count
            var columnCount = 0;
            for (var lineNo = 0; lineNo < lineCount; lineNo++)
                columnCount = Math.Max(columnCount, lines[lineNo].Split('§').Length);

            // define column widths
            var columnWidth = new int[columnCount];
            for (var lineNo = 0; lineNo < lineCount; lineNo++)
            {
                // skip empty lines
                if (string.IsNullOrEmpty(lines[lineNo]))
                    continue;

                // split row in columns
                var line = lines[lineNo].Split('§');
                for (var colNo = 0; colNo < line.Length; colNo++)
                {
                    // track column width if current row has no "colspan"
                    if (line.Length == columnCount)
                        columnWidth[colNo] = Math.Max(columnWidth[colNo],
                            (int)(menu_sprite_font.MeasureString(Utils.StringCleanup(line[colNo])).X * (lineNo == 0 ? 1.2 : 1) + 8));
                }
            }

            // find message width
            var messageWidth = 0;
            for (var i = 0; i < columnCount; i++)
                messageWidth += columnWidth[i];

            var messageHeight = lineCount * menu_item_height;

            if (previousMessageHeight == 0)
                // first message: clear
                messageItems.Clear();
            else
                // appended message: re-position previous message
                foreach (var item in messageItems)
                    item.Y -= ((messageHeight + (menu_item_height / 2)) / 2);

            var lblX = (screenResX - messageWidth) / 2;
            var lblY = (previousMessageHeight / 2) + (screenResY - messageHeight + (menu_item_height / 2)) / 2;

            var column = new CLayout(lblX, lblY, CLayout.Orientation.Vertical, true);
            for (var lineNo = 0; lineNo < lineCount; lineNo++)
            {
                if (!string.IsNullOrEmpty(lines[lineNo]))
                {
                    var line = lines[lineNo].Split('§');
                    var row = new CLayout(lblX, lblY, CLayout.Orientation.Horizontal, true);
                    row.Width = messageWidth;
                    for (var colNo = 0; colNo < line.Length; colNo++)
                    {
                        CLabel lbl;
                        // first row is table header
                        if (lineNo == 0)
                        {
                            lbl = new CLabel(0, 0,
                                                columnWidth[colNo], (int)(menu_item_height * 1.2), line[colNo], menu_font_name,
                                                menu_font_size + 1, menu_font_style, menu_selected_font_color, menu_selected_backcolor, textAlign, false);
                            lbl.BorderColor = menu_font_color;
                        }
                        else
                            lbl = new CLabel(0, 0,
                                                columnWidth[colNo], menu_item_height, line[colNo], menu_font_name,
                                                menu_font_size, menu_font_style, menu_font_color, menu_backcolor, textAlign, false);
                        lbl.FillParent = line.Length != columnCount;
                        row.AddItem(lbl);
                    }
                    lblY += row.Height;
                    column.AddItem(row);
                }
            }
            messageItems.Add(column);
            column.Update();

            _showMessage = true;
            return messageHeight;
        }

        public void HideMessage(bool immediate)
        {
            if (immediate)
                _showMessage = false;
            else
            {
                foreach (var messageItem in messageItems)
                {
                    if (messageItem == messageItems[0])
                        messageItem.StartTransition(Transition.FastFadeOut, 0, () =>
                            {
                                _showMessage = false;
                            }
                            );
                    else
                        messageItem.StartTransition(Transition.FastFadeOut);
                }
            }
        }
    }
}