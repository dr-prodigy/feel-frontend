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

using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Graphics.Color;
using System.Collections.Generic;
using System;
using System.IO;

namespace feel
{
    class CMenu : CLayout
    {
        private List<List<string>> _menuStrings;
        private List<CDrawable> _menuItems = new List<CDrawable>();
        private CLayout _additionalInfoLayout;
        private CLayout _menuSidebarLayout;
        private CLabel _titlebar;
        private CLabel _statusbar;
        private CImage _iconImage;
        private CImage _snapshotImage;
        private string _currentMenu;
        private string _additionalData;
        private string _additionalInfo;
        private int _currentMenuItem = 0;
        private bool _isMenuTextReader = false;
        private string _snapshotPath;
        private string _iconPath;
        private bool _showSideBar;
        private int menu_max_item_count;
        private int menu_first_item_shown = 0;

        private int menuRowsCount = 0;
        private int menuColumnCount = 0;
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

        public int screenResX;
        public int screenResY;
        private bool drawRotated;

        public CMenu() :
            base(0, 0, Orientation.Vertical, true)
        {
        }

        public void Init(int resX, int resY, bool rotateScreen, int width, int itemHeight, string fontName, int fontSize,
            FontStyle fontStyle, Color fontColor, Color backcolor, Color selectedFontColor, Color selectedBackcolor)
        {
            screenResX = resX;
            screenResY = resY;
            drawRotated = rotateScreen;
            Width = menu_min_width = width;
            menu_item_height = itemHeight;
            menu_font_name = fontName;
            menu_font_size = fontSize;
            menu_font_style = fontStyle;
            menu_font_color = fontColor;
            menu_backcolor = backcolor;
            menu_selected_font_color = selectedFontColor;
            menu_selected_backcolor = selectedBackcolor;
            menu_sprite_font = Utils.LoadSpriteFont(_feel, menu_font_name, menu_font_size, menu_font_style);
            BackColor = menu_backcolor;
        }

        public void ShowMenu(List<List<string>> menuCols, string titleText, string statusbarText, string currentMenu, 
            string additionalData, string additionalInfo, bool menuChanged, string preselectedMenuKey, bool isWindowTextReader,
            string snapshotPath, string iconPath, bool showSideBar)
        {
            _currentMenu = currentMenu;
            _additionalData = additionalData;
            _additionalInfo = additionalInfo;
            _menuStrings = menuCols;
            menuColumnCount = menuCols.Count;
            _isMenuTextReader = isWindowTextReader;
            _snapshotPath = snapshotPath != string.Empty ? snapshotPath + (snapshotPath[snapshotPath.Length - 1] != '\\' ? "\\" : string.Empty) : string.Empty;
            _iconPath = iconPath;
            _showSideBar = showSideBar;
            Width = menu_min_width;
            menu_first_item_shown = 0;

            var currentCol = 0;
            var indent = menuCols.Count == 1 ? "      " : string.Empty;
            _menuItems.Clear();
            _titlebar = _statusbar = null;

            // create titlebar
            if (titleText != string.Empty)
            {
                _titlebar = new CLabel(0, 0,
                                        Width, (int)(menu_item_height * 1.2),
                                        titleText,
                                        menu_font_name, menu_font_size + 1, menu_font_style,
                                        menu_selected_font_color, menu_selected_backcolor, TextAlign.Left,
                                        false);
                _titlebar.BorderColor = menu_font_color;
            }

            // additional info
            _additionalInfoLayout = BuildTable(additionalInfo, TextAlign.Left, 0);
            _additionalInfoLayout.PaddingLeft = 10;

            var itemWidth = Width;
            var yCounter = 0;
            foreach (var menuRows in menuCols)
            {
                // on first column, decide menu size
                if (currentCol == 0)
                {
                    var maxWidth = Width;
                    foreach (var menuRow in menuRows)
                    {
                        var text = menuRow.Split('|').Length < 3 ? indent + menuRow.Split('|')[1] + indent : menuRow.Split('|')[1];
                        maxWidth = Math.Max(maxWidth, (int)menu_sprite_font.MeasureString(Utils.StringCleanup(text)).X + 8);
                    }
                    itemWidth = (int)Math.Min(maxWidth,
                        _showSideBar && _currentMenu != "show_game_info" ? screenResX / 1.2 : screenResX - screenResX / 64) / menuColumnCount;
                }

                for (yCounter = 0; yCounter < menuRows.Count; yCounter++)
                {
                    var menuRow = menuRows[yCounter];
                    var arr = menuRow.Split('|');
                    if (arr[1].StartsWith(("*#*T")))
                    {
                        // process table
                        var alignChar = arr[1].Substring("*#*T".Length, 1);
                        var textAlign = alignChar == "L" ? TextAlign.Left : alignChar == "C" ? TextAlign.Center : TextAlign.Right;
                        var tableString = string.Empty;
                        for (; yCounter < menuRows.Count; yCounter++)
                        {
                            var tableRow = menuRows[yCounter].Split('|');
                            var rowString = menuRows[yCounter].Replace("*#*TL*#*", string.Empty).
                                Replace("*#*TC*#*", string.Empty).
                                Replace("*#*TR*#*", string.Empty).
                                Replace("*#*/T*#*", string.Empty);
                            tableString += rowString + "\n";
                            if (tableRow[1].Contains("*#*/T*#*"))
                            {
                                break;
                            }
                        }
                        // remove last \n
                        tableString = tableString.Substring(0, tableString.Length - 1);

                        // inject table in menu
                        _menuItems.AddRange(BuildTable(tableString, textAlign, itemWidth).Items);
                    }
                    else
                    {
                        // process item
                        var lbl = new CLabel(0, 0,
                                 itemWidth, menu_item_height,
                                 arr.Length < 3 ? indent + arr[1] + indent : arr[1],
                                 menu_font_name, menu_font_size, menu_font_style,
                                 menu_font_color, Color.TransparentWhite,
                                 TextAlign.Left,
                                 arr.Length < 3 ? true : false);
                        lbl.KeyName = arr[0];
                        lbl.Scrolling = false;
                        _menuItems.Add(lbl);
                    }
                }
                currentCol++;
            }
            menuRowsCount = yCounter;

            // create statusbar
            if (statusbarText != string.Empty)
            {
                _statusbar = new CLabel(0, 0,
                            Width, (int)(menu_item_height * 0.9),
                            statusbarText,
                            menu_font_name, menu_font_size - 1, menu_font_style,
                            menu_selected_font_color, menu_selected_backcolor, TextAlign.Right,
                            false);
                _statusbar.BorderColor = menu_font_color;
            }

            _snapshotImage = new CImage(string.Empty, screenResX / 4, screenResY / 4, null, false, true, true);
            _snapshotImage.BorderColor = menu_font_color;
            _previousSnapshotPath = string.Empty;

            _iconImage = new CImage(string.Empty, screenResX / 12, screenResX / 12, null, false, true, false);
            _previousIconPath = string.Empty;

            StartTransition(CDrawable.Transition.FadeIn, 0);
            SelectMenuItem(preselectedMenuKey, menuChanged);
        }

        public void Hide(Action callback)
        {
            StartTransition(Transition.FastFadeOut, 0, callback);
        }

        public KeyValuePair<string, string> SelectedItem { get { return new KeyValuePair<string, string>(_menuItems[_currentMenuItem].KeyName, _menuItems[_currentMenuItem].Text); } }

        #region Menu commands
        public bool SelectMenuItem(string KeyName, bool menuChanged)
        {
            var selectionFound = false;
            _currentMenuItem = 0;
            for (var i = 0; i < _menuItems.Count; i++)
            {
                if (_menuItems[i].KeyName == KeyName && _menuItems[i].IsFocusable)
                {
                    _currentMenuItem = i;
                    selectionFound = UpdateMenu(menuChanged);
                    break;
                }
            }
            if (!selectionFound)
            {
                // focus on first focusable item
                for (_currentMenuItem = 0; _currentMenuItem < _menuItems.Count && !_menuItems[_currentMenuItem].IsFocusable; _currentMenuItem++) ;
                selectionFound = UpdateMenu(true);
            }

            return selectionFound;
        }

        public bool SelectMenuDownItem()
        {
            var last = _currentMenuItem;
            if (_isMenuTextReader && _currentMenuItem < (menu_first_item_shown + menu_max_item_count) && _menuItems.Count > menu_max_item_count)
                _currentMenuItem = menu_first_item_shown + menu_max_item_count;
            else
                _currentMenuItem++;
            if (_currentMenuItem == _menuItems.Count)
                if (_isMenuTextReader)
                    _currentMenuItem = _menuItems.Count - 1;
                else
                    _currentMenuItem = 0;
            if (last != _currentMenuItem)
            {
                // search for first focusable item
                while (!UpdateMenu(false) && !_isMenuTextReader)
                {
                    _currentMenuItem++;
                    if (_currentMenuItem > _menuItems.Count - 1)
                    {
                        _currentMenuItem = 0;
                        // search again from start
                        while (!UpdateMenu(false) && !_isMenuTextReader)
                        {
                            _currentMenuItem++;
                        }
                    }
                }
                return true;
            }
            return false;
        }

        public bool SelectMenuUpItem()
        {
            var last = _currentMenuItem;
            if (_isMenuTextReader && _currentMenuItem > menu_first_item_shown)
                _currentMenuItem = menu_first_item_shown - 1;
            else
                _currentMenuItem--;
            if (_currentMenuItem < 0)
                if (_isMenuTextReader)
                    _currentMenuItem = 0;
                else
                    _currentMenuItem = _menuItems.Count - 1;
            if (last != _currentMenuItem)
            {
                // search for first focusable item
                while (!UpdateMenu(false) && !_isMenuTextReader)
                {
                    _currentMenuItem--;
                    if (_currentMenuItem < 0)
                    {
                        _currentMenuItem = _menuItems.Count - 1;
                        // search again from end
                        while (!UpdateMenu(false) && !_isMenuTextReader)
                        {
                            _currentMenuItem--;
                        }
                    }
                }
                return true;
            }
            return false;
        }

        public bool SelectMenuRightItem()
        {
            var last = _currentMenuItem;
            if (_isMenuTextReader)
                _currentMenuItem = Math.Min(_menuItems.Count - 1, _currentMenuItem + menu_max_item_count);
            else if (_currentMenuItem < menuRowsCount * (menuColumnCount - 1))
                _currentMenuItem += menuRowsCount;
            else
                _currentMenuItem -= menuRowsCount * (menuColumnCount - 1);

            if (_currentMenuItem == _menuItems.Count)
                _currentMenuItem = 0;
            if (last != _currentMenuItem)
            {
                UpdateMenu(false);
                return true;
            }
            return false;
        }

        public bool SelectMenuLeftItem()
        {
            var last = _currentMenuItem;
            if (_isMenuTextReader)
                _currentMenuItem = Math.Max(0, _currentMenuItem - menu_max_item_count);
            else if (_currentMenuItem >= menuRowsCount)
                _currentMenuItem -= menuRowsCount;
            else
                _currentMenuItem += menuRowsCount * (menuColumnCount - 1);
            if (_currentMenuItem < 0)
                _currentMenuItem = _menuItems.Count - 1;
            if (last != _currentMenuItem)
            {
                UpdateMenu(false);
                return true;
            }
            return false;
        }

        public bool SelectNextMenuPage()
        {
            var last = _currentMenuItem;
            _currentMenuItem = Math.Min(_menuItems.Count - 1, _currentMenuItem + menu_max_item_count);
            if (last != _currentMenuItem)
            {
                UpdateMenu(false);
                return true;
            }
            return false;
        }

        public bool SelectPreviousMenuPage()
        {
            var last = _currentMenuItem;
            _currentMenuItem = Math.Max(0, _currentMenuItem - menu_max_item_count);
            if (last != _currentMenuItem)
            {
                UpdateMenu(false);
                return true;
            }
            return false;
        }
        #endregion

        string _previousSnapshotPath = string.Empty;
        string _previousIconPath = string.Empty;
        private bool UpdateMenu(bool menuChanged)
        {
            var selectionMade = false;
            Clear();

            var windowLayout = new CLayout(0, 0, Orientation.Vertical, true)
            {
                Padding = screenResX / 128
            };
            var infoLayout = new CLayout(0, 0, Orientation.Horizontal, true)
            {
                PaddingLeft = screenResX / 25,
                PaddingRight = screenResX / 25,
                PaddingTop = screenResY / 48,
                PaddingBottom = screenResY / 48
            };
            var mainLayout = new CLayout(0, 0, Orientation.Horizontal, true);
            var menuLayout = new CLayout(0, 0, Orientation.Vertical, true)
            {
                FillParent = true
            };
            _menuSidebarLayout = new CLayout(0, 0, Orientation.Vertical, true)
            {
                Padding = screenResX / 64,
                PaddingRight = screenResX / 128 * 3
            };

            // titlebar
            if (_titlebar != null)
            {
                AddItem(_titlebar);
            }

            // menu scrolling
            if (menuColumnCount == 1)
                menu_max_item_count = (screenResY / menu_item_height) - (_currentMenu == "show_game_info" ? 10 : 3) + (_statusbar == null ? 1 : 0);
            else if (menuColumnCount > 0)
                menu_max_item_count = menuColumnCount * _menuItems.Count;
            else
                menu_max_item_count = 0;
            if (_currentMenuItem < menu_first_item_shown)
                menu_first_item_shown = _currentMenuItem;
            if (_currentMenuItem > menu_first_item_shown + menu_max_item_count - 1)
                menu_first_item_shown = _currentMenuItem - menu_max_item_count + 1;
            var menu_last_item_shown = Math.Min(menu_first_item_shown + menu_max_item_count, menu_first_item_shown + menuRowsCount);

            // menu items display
            for (var yCounter = menu_first_item_shown; yCounter < menu_last_item_shown; yCounter++)
            {
                var rowLayout = new CLayout(0, 0, Orientation.Horizontal, true);
                for (var xCounter = 0; xCounter < _menuStrings.Count; xCounter++)
                {
                    var pointer = yCounter + xCounter * _menuStrings[0].Count;
                    if (pointer == _currentMenuItem && _menuItems[pointer].IsFocusable)
                    {
                        selectionMade = true;
                        if (!_isMenuTextReader)
                        {
                            _menuItems[pointer].ForeColor = menu_selected_font_color;
                            _menuItems[pointer].BackColor = menu_selected_backcolor;
                            _menuItems[pointer].BorderColor = menu_font_color;
                            _menuItems[pointer].SetFocus();
                        }
                        else
                        {
                            _menuItems[pointer].ForeColor = _menuItems[pointer].IsFocusable ? menu_font_color : menu_selected_font_color;
                            _menuItems[pointer].BackColor = _menuItems[pointer].BorderColor = Color.TransparentWhite;
                        }
                    }
                    else
                    {
                        _menuItems[pointer].ForeColor = _menuItems[pointer].IsFocusable ? menu_font_color : menu_selected_font_color;
                        _menuItems[pointer].BackColor = _menuItems[pointer].BorderColor = Color.TransparentWhite;
                    }
                    rowLayout.AddItem(_menuItems[pointer]);
                }
                menuLayout.AddItem(rowLayout);
            }

            //if (_currentMenu == "show_game_info")
            if (_additionalInfo != string.Empty)
            {
                // game info (snapshot)
                if (_additionalData != string.Empty)
                {
                    var currentSnapshotPath = _snapshotPath + _additionalData + ".png";
                    if (_previousSnapshotPath != currentSnapshotPath)
                    {
                        _previousSnapshotPath = currentSnapshotPath;
                        _snapshotImage.LoadImage(_previousSnapshotPath);
                    }
                    infoLayout.AddItem(_snapshotImage);
                }
                infoLayout.AddItem(_additionalInfoLayout);
                windowLayout.AddItem(infoLayout);
            }
            else
            {
                if (_showSideBar)
                {
                    // sidebar
                    var currentIconPath = _iconPath + _menuItems[_currentMenuItem].KeyName + ".png";
                    if (_previousIconPath != currentIconPath)
                    {
                        _previousIconPath = currentIconPath;
                        if (File.Exists(_previousIconPath))
                            _iconImage.LoadImage(_previousIconPath);
                        else
                            _iconImage.LoadImage("*feel");
                    }
                    _menuSidebarLayout.AddItem(_iconImage);
                    mainLayout.AddItem(_menuSidebarLayout);
                }
            }
            BorderColor = menu_font_color;

            mainLayout.AddItem(menuLayout);
            windowLayout.AddItem(mainLayout);
            AddItem(windowLayout);

            // statusbar
            if (_statusbar != null) AddItem(_statusbar);

            if (menuChanged)
                StartTransition(CDrawable.Transition.FadeIn);
            else
                ResetPendingTransitions();

            // define max layout sizes
            Update();

            // center menu + rearrange
            X = (screenResX - Width) / 2;
            Y = ((screenResY - Height) / 2);
            Update();

            return selectionMade;
        }

        public CLayout BuildTable(string message, TextAlign textAlign, int tableWidth)
        {
            var tableLayout = new CLayout(0, 0, CLayout.Orientation.Vertical, false);

            // split lines
            var lines = message.Split('\n');
            var lineCount = lines.Length;

            // get column count
            var columnCount = 0;
            for (var lineNo = 0; lineNo < lineCount; lineNo++)
                columnCount = Math.Max(columnCount, lines[lineNo].Split('§').Length);

            // define column widths
            var columnWidth = new int[columnCount];
            var totalWidth = 0;
            for (var lineNo = 0; lineNo < lineCount; lineNo++)
            {
                var curLine = lines[lineNo];
                if (curLine.Contains("|"))
                    curLine = curLine.Split('|')[1];
                // skip empty lines
                if (string.IsNullOrEmpty(curLine))
                    continue;

                // split row in columns
                var line = curLine.Split('§');
                totalWidth = 0;
                for (var colNo = 0; colNo < line.Length; colNo++)
                {
                    // track column width if current row has no "colspan"
                    if (line.Length == columnCount)
                    {
                        columnWidth[colNo] = Math.Max(columnWidth[colNo],
                            (int)(menu_sprite_font.MeasureString(Utils.StringCleanup(line[colNo])).X + 8));
                        totalWidth += columnWidth[colNo];
                    }
                }
            }
            // if table width supplied, resize all columns
            if (tableWidth > 0 && totalWidth != tableWidth)
                for (var colNo = 0; colNo < columnWidth.Length; colNo++)
                    columnWidth[colNo] = (int)((float)columnWidth[colNo] / (float)totalWidth * (float)tableWidth);

            // find message width
            var messageWidth = 0;
            for (var i = 0; i < columnCount; i++)
                messageWidth += columnWidth[i];

            var messageHeight = lineCount * menu_item_height;

            for (var lineNo = 0; lineNo < lineCount; lineNo++)
            {
                var row = new CLayout(0, 0, CLayout.Orientation.Horizontal, true);
                var curLine = lines[lineNo];
                if (curLine.Contains("|"))
                {
                    row.KeyName = curLine.Split('|')[0];
                    row.IsFocusable = curLine.Split('|').Length < 3;
                    curLine = curLine.Split('|')[1];
                }
                var line = curLine.Split('§');
                row.Width = messageWidth;
                row.Gravity = LayoutGravity.Left;
                for (var colNo = 0; colNo < line.Length; colNo++)
                {
                    CLabel lbl;
                    lbl = new CLabel(0, 0,
                                        columnWidth[colNo], menu_item_height, line[colNo], menu_font_name,
                                        menu_font_size, menu_font_style, menu_font_color, Color.TransparentWhite, textAlign, false) { Scrolling = false };
                    // first row is table header
                    //if (lineNo == 0)
                    //{
                    //    lbl = new CLabel(0, 0,
                    //                        columnWidth[colNo], (int)(menu_item_height * 1), line[colNo], menu_font_name,
                    //                        menu_font_size + 1, menu_font_style, menu_selected_font_color, menu_selected_backcolor, textAlign, false);
                    //    lbl.BackColor = menu_selected_backcolor;
                    //    lbl.BorderColor = menu_font_color;
                    //}
                    //else
                    //    lbl = new CLabel(0, 0,
                    //                        columnWidth[colNo], menu_item_height, line[colNo], menu_font_name,
                    //                        menu_font_size, menu_font_style, menu_font_color, Color.TransparentWhite, textAlign, false);
                    row.AddItem(lbl);
                }
                tableLayout.AddItem(row);
            }
            tableLayout.Update();
            return tableLayout;
        }

        public override void SetFocus()
        {
            if (_menuItems.Count > _currentMenuItem)
                _menuItems[_currentMenuItem].SetFocus();
        }
    }
}