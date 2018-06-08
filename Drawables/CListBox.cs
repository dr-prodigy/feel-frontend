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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace feel
{
    class CListBox: CDrawable
    {
        private Rectangle _rect;
        private List<RomDesc> _listBox;
        private List<RomDesc> _filteredListBox;
        private List<int> _shuffleList;
        private int _fontSize;

        private Color _fontColor;
        private Color _backColor;
        private Color _selFontBackColor;
        private Color _selFontColor;
        private Color _backColorL;
        private Color _selFontBackColorL;
        private Color _starsForeColor;
        private Color _starsBackColor;
        private Color _selStarsForeColor;
        private Color _selStarsBackColor;

        private int _rowSize = 0;
        private int _rowCount = 0;
        private List<CLabel> Labels;
        private List<CLabel> StarLabels;
        private int actIndex = -1;
        private int shuffleIndex;
        private SortType _sortType = SortType.AZ;
        private bool _isFullTextSearch = false;
        private TimeSpan _maxPlayedTime;
        private TimeSpan _maxPlayedTimeNow;
        private TimeSpan _totalPlayedTime;
        private TimeSpan _totalPlayedTimeNow;
        private int _starLabelWidth;
        private bool _disableStars;

        private string _searchFilter = "";

        public CListBox(OBJScene _objScene, Game feel, int width, int height, int itemHeight, int xPos, int yPos,
		                string fontName, int fontSize, System.Drawing.FontStyle fontStyle, Color fontColor, Color backColor, Color selFontColor,
                        Color selFontBackColor, SortType sortType, TextAlign textAlign, bool disableStars)
        {
            _listBox = new List<RomDesc>();
            _x = xPos;
            _y = yPos;
            _starLabelWidth = (int)Utils.LoadSpriteFont(feel, "wingdings", fontSize, System.Drawing.FontStyle.Regular).MeasureString("«««««").X + 8;
            _rect = new Rectangle(0, 0, width, height);
            _fontColor = fontColor;
            _backColor = backColor;
            _fontSize = fontSize;
            _selFontColor = selFontColor;
            _selFontBackColor = selFontBackColor;
            _selFontBackColorL = _selFontBackColor;
            _backColorL = _backColor;
            _disableStars = disableStars;

            _starsForeColor = _disableStars ? Color.TransparentBlack : new Color(255, 220, 66, 80);
            _starsBackColor = Color.TransparentBlack;
            _selStarsForeColor = _disableStars ? Color.TransparentBlack : new Color(255, 220, 66, _selFontColor.A);
            _selStarsBackColor = _disableStars ? Color.TransparentBlack : new Color(_selFontBackColorL, (byte)(_selFontBackColorL.A * .9));

            _rowSize = itemHeight;
            _rowCount = height / _rowSize;
            _rect.Height = _rowCount * _rowSize;
            Labels = new List<CLabel>();
            StarLabels = new List<CLabel>();
            for (var i = 0; i < _rowCount; i++)
            {
                Labels.Add(_objScene.CreateLabel("RomListLabel_" + i, 0, 0, _rect.Width, _rowSize, "",
				                                 fontName, fontSize, fontStyle, _fontColor, _backColor, textAlign, true, true));
                StarLabels.Add(_objScene.CreateStarLabel("RomListStarLabel_" + i, 0, 0, _starLabelWidth, _rowSize, "",
                                                 fontSize, fontStyle, _starsForeColor, _starsBackColor, true));
            }
            _sortType = sortType;
            Sort();
            UpdatePosition();
        }

        public bool DisableStars { get { return _disableStars; }
            set
            {
                _disableStars = value;
                _starsForeColor = _disableStars ? Color.TransparentBlack : new Color(255, 220, 66, 80);
                _selStarsForeColor = _disableStars ? Color.TransparentBlack : new Color(255, 220, 66, _selFontColor.A);
                _selStarsBackColor = _disableStars ? Color.TransparentBlack : new Color(_selFontBackColorL, (byte)(_selFontBackColorL.A * .9));
                UpdateSelection();
            }
        }

        public bool IsFnetRunning = false;

        public new void StartTransition(Transition pendingTransition)
        {
            var counter = 0;
            foreach (var label in Labels)
            {
                counter++;
                label.StartTransition(pendingTransition, 7 * counter);
            }
            counter = 0;
            foreach (var label in StarLabels)
            {
                counter++;
                label.StartTransition(pendingTransition, 7 * counter);
            }
        }

        public new void ResetPendingTransitions()
        {
            foreach (var label in Labels)
            {
                label.ResetPendingTransitions();
            }
            foreach (var label in StarLabels)
            {
                label.ResetPendingTransitions();
            }
        }

        public void UpdateSelection()
        {
            var startRow = actIndex - (_rowCount / 2);
            if (startRow < 0)
                startRow = 0;
            var endRow = startRow + _rowCount;
            if (endRow > FilteredList.Count)
            {
                var diff = endRow - FilteredList.Count;
                startRow -= diff;
                if (startRow < 0)
                    startRow = 0;
                endRow = FilteredList.Count;
            }

            if (endRow > 0)
                for (var r = 0; r < _rowCount; r++)
                {
                    var i = startRow + r;
                    var text = "";
                    var starText = "";
                    Labels[r].ForeColor = _fontColor;
                    Labels[r].BackColor = _backColor;
                    Labels[r].Scrolling = false;
                    StarLabels[r].ForeColor = _starsForeColor;
                    StarLabels[r].BackColor = _starsBackColor;
                    StarLabels[r].Scrolling = false;
                    if (i < endRow)
                    {
                        //var ranking = _maxPlayedTime.TotalSeconds > 0 ? Math.Round((_listBox[i].PlayedTime.TotalSeconds * 100) / _maxPlayedTime.TotalSeconds, 0) : 0.0;
                        //var stars = (int)Math.Round(ranking / 10, 0);
                        var ranking = FilteredList[i].PlayedTimeNow != TimeSpan.Zero ? FilteredList[i].PlayedTimeNow.TotalSeconds / _maxPlayedTimeNow.TotalSeconds : 0.0;
                        var stars = 0;
                        if (ranking > 0.0)
                            stars = (int)Math.Round(ranking * 5, 0);
                        text = FilteredList[i].Description;

                        // FNET
                        if (IsFnetRunning) Labels[r].SetAlpha(FilteredList[i].ExternalSortKey != int.MaxValue ? 1 : .7f);

                        if (stars > 0 && StarLabels[r].Visible)
                        {
                            starText = "".PadRight(stars, '«');
                        }

                        if (i == actIndex)
                        {
                            _currentlyFocused = Labels[r];
                            Labels[r].ForeColor = _selFontColor;
                            Labels[r].BackColor = _selFontBackColor;
                            Labels[r].Scrolling = true;
                            StarLabels[r].ForeColor = _selStarsForeColor;
                            StarLabels[r].BackColor = _selStarsBackColor;
                        }
                    }
                    Labels[r].Text = text;
                    StarLabels[r].Text = starText;
                    Labels[r].ResetTextPosition();
                }
            else
            {
                Labels[0].Text = "< empty list >";
                Labels[0].ForeColor = _fontColor;
                Labels[0].BackColor = _backColor;
                Labels[0].Scrolling = false;
            }
            SetFocus();
        }

        private CLabel _currentlyFocused = null;
        public override void SetFocus()
        {
            if (_currentlyFocused != null)
                _currentlyFocused.SetFocus();
        }
 
        public SortType CurrentSort { get { return _sortType; } }

        public int SelectedIndex { get { return actIndex + 1; } }

        public int SelectedShuffleIndex { get { return _shuffleList[shuffleIndex]; } }

        public RomDesc SelectedItem { get { return FilteredList[actIndex]; } }

        public int ItemsCount { get { return FilteredList.Count; } }

        public override int X { get { return _x; } set { _x = value; UpdatePosition(); } }

        public override int Y { get { return _y; } set { _y = value; UpdatePosition(); } }

        public TimeSpan MaxPlayedTime { get { return _maxPlayedTime; } }

        public TimeSpan MaxPlayedTimeNow { get { return _maxPlayedTimeNow; } }

        public TimeSpan TotalPlayedTime { get { return _totalPlayedTime; } }

        public TimeSpan TotalPlayedTimeNow { get { return _totalPlayedTimeNow; } }

        public Rectangle Rect { get { return _rect; } }

        public List<RomDesc> List {
            get { return _listBox; }
            set
            {
                _listBox = value;
                CleanupSecondaryLists();
                UpdateSelection();
            }
        }

        public List<RomDesc> FilteredList
        {
            get
            {
                if (_filteredListBox == null)
                    CreateFilteredList();
                return _filteredListBox;
            }
        }

        public List<int> ShuffleList
        {
            get
            {
                if (_shuffleList == null)
                    CreateShuffleList();
                return _shuffleList;
            }
        }

        private void CleanupSecondaryLists()
        {
            _filteredListBox = null;
            _shuffleList = null;
        }

        private void CreateShuffleList()
        {
            var rnd = new Random();
            shuffleIndex = 0;
            var itemsCount = _filteredListBox.Count;
            var sourceList = new List<int>();
            _shuffleList = new List<int>();

            for (int i = 1; i <= itemsCount; i++)
                sourceList.Add(i);

            while (itemsCount > 0)
            {
                var index = rnd.Next(0, itemsCount);
                _shuffleList.Add(sourceList[index]);
                sourceList.RemoveAt(index);
                itemsCount--;
            }
        }

        private void CreateFilteredList()
        {
            if (!_isFullTextSearch)
                _filteredListBox = List.FindAll(c => c.Description.StartsWith(_searchFilter, StringComparison.CurrentCultureIgnoreCase));
            else
                _filteredListBox = List.FindAll(c => c.Description.ToLower().Contains(_searchFilter.ToLower()));
            _shuffleList = null;
            Sort();
        }

        private void UpdatePosition()
        {
            _rect.X = _x;
            _rect.Y = _y;

            var y = _y;
            for (var i = 0; i < _rowCount; i++)
            {
                // dx
                Labels[i].X = _x;
                // TEST StarLabels[i].X = _xPos + _rect.Width;
                StarLabels[i].X = _x + _rect.Width - _starLabelWidth;
                // sx
                //Labels[i].X = _xPos + _starLabelWidth;
                //StarLabels[i].X = _xPos;

                StarLabels[i].Scrolling = false;
                Labels[i].Y = StarLabels[i].Y = y;
                y += _rowSize;
            }
        }

        public RomDesc SelectNextItem()
        {
            if (actIndex < FilteredList.Count - 1)
                actIndex++;
            UpdateSelection();
            if (actIndex == -1)
                return null;
            return FilteredList[actIndex];
        }

        public RomDesc SelectPreviousItem()
        {
            if (actIndex == -1)
                return null;
            if (actIndex > 0)
                actIndex--;
            UpdateSelection();
            return FilteredList[actIndex];
        }

        public RomDesc SelectNextCharItem()
        {
            if (actIndex == -1)
                return SelectNextItem();
            var index = FilteredList.Count - 1;
            var c = FilteredList[actIndex].SortComparisonChars(CurrentSort);
            for (var i = actIndex; i < FilteredList.Count; i++)
            {
                if (FilteredList[i].SortComparisonChars(CurrentSort) != c)
                {
                    index = i;
                    break;
                }
            }
            actIndex = index;
            UpdateSelection();
            return FilteredList[actIndex];
        }

        public RomDesc SelectPreviousCharItem()
        {
            if (actIndex == -1)
                return SelectNextItem();
            var index = 0;
            var c = FilteredList[actIndex].SortComparisonChars(CurrentSort);
            for (var i = actIndex; i > 0; i--)
            {
                if (FilteredList[i].SortComparisonChars(CurrentSort) != c)
                {
                    index = i;
                    break;
                }
            }
            actIndex = index;
            UpdateSelection();
            return FilteredList[actIndex];
        }

        public RomDesc SelectNextPageItem()
        {
            if (actIndex == -1)
                return SelectNextItem();
            actIndex += _rowCount;
            if (actIndex >= FilteredList.Count)
                actIndex = FilteredList.Count - 1;
            UpdateSelection();
            return FilteredList[actIndex];
        }

        public RomDesc SelectPreviousPageItem()
        {
            if (actIndex == -1)
                return SelectNextItem();
            actIndex -= _rowCount;
            if (actIndex < 0)
                actIndex = 0;
            UpdateSelection();
            return FilteredList[actIndex];
        }

        public RomDesc SelectItem(int index)
        {
            index--;
            if (FilteredList == null || index < 0)
                return null;
            if (FilteredList.Count == 0)
                return null;
            if (FilteredList.Count <= index)
                actIndex = 0;
            else
                actIndex = index;
            UpdateSelection();
            return FilteredList[actIndex];
        }

        public RomDesc SelectFirstItem()
        {
            if (FilteredList == null)
                return null;
            if (FilteredList.Count == 0)
                return null;
            actIndex = 0;
            UpdateSelection();
            return FilteredList[actIndex];
        }

        public RomDesc RemoveGame(RomDesc rom)
        {
            _listBox.RemoveAll(c => c.Key == rom.Key);
            CleanupSecondaryLists();

            if (actIndex > 0)
                actIndex--;

            UpdateSelection();
            if (actIndex >= 0 && FilteredList.Count > 0)
                return FilteredList[actIndex];
            return null;
        }

        public bool AddGame(RomDesc rom)
        {
            var found = _listBox.Find(c => c.Key == rom.Key);
            if (found != null)
                return false;
            _listBox.Add(rom);
            CleanupSecondaryLists();

            return true;
        }

        public int FilterByDescription(string searchString, bool isFullTextSearch)
        {
            if (_listBox == null)
                return 0;
            _searchFilter = searchString;
            _isFullTextSearch = isFullTextSearch;
            CreateFilteredList();
            return _filteredListBox.Count > 0 ? 1 : 0;
        }

        public int FindIndexByDescription(string searchString, bool isFullTextSearch)
        {
            if (_listBox == null)
                return 0;
            if (string.IsNullOrEmpty(searchString))
                return 1;
            var index = -1;
            if (!isFullTextSearch)
                index = _listBox.FindIndex(c => c.Description.StartsWith(searchString, StringComparison.CurrentCultureIgnoreCase));
            else
                index = _listBox.FindIndex(c => c.Description.ToLower().Contains(searchString.ToLower()));
            if (index < 0)
                return 0;
            return index + 1;
        }

        public int FindIndexByName(string searchString, bool findOnFilteredList)
        {
            var currentList = findOnFilteredList ? FilteredList : _listBox;
            if (currentList == null)
                return 0;
            if (string.IsNullOrEmpty(searchString))
                return 1;
            var index = 0;
            index = currentList.FindIndex(c => c.Key == searchString);
            if (index < 0)
                return 0;
            return index + 1;
        }

        public void Clear()
        {
            List = new List<RomDesc>();
            foreach (var label in Labels)
            {
                label.BackColor = _backColor;
                label.ForeColor = _fontColor;
                label.Text = "";
            }
            foreach (var label in StarLabels)
            {
                label.BackColor = _backColor;
                label.ForeColor = _fontColor;
                label.Text = "";
            }
            actIndex = -1;
        }

        public int NextShuffleIndex()
        {
            if (shuffleIndex < ShuffleList.Count - 1)
                shuffleIndex++;
            else
                shuffleIndex = 0;
            return ShuffleList[shuffleIndex];
        }

        public int PreviousShuffleIndex()
        {
            if (shuffleIndex > 0)
                shuffleIndex--;
            else
                shuffleIndex = ShuffleList.Count - 1;
            return ShuffleList[shuffleIndex];
        }

        public RomDesc SetNextSort()
        {
            _sortType++;
            if ((int)_sortType == Enum.GetNames(typeof(SortType)).Length)
                _sortType = SortType.AZ;
            return ChangeSort(_sortType);
        }

        public RomDesc Sort()
        {
            return Sort(_sortType);
        }

        public RomDesc ChangeSort(SortType sortType)
        {
            if (FilteredList.Count < 1)
                return null;
            var tmpName = FilteredList[actIndex].Key;
            var filteredList = Sort(sortType);
            actIndex = FilteredList.FindIndex(c => c.Key == tmpName);
            return filteredList;
        }

        public RomDesc Sort(SortType sortType)
        {
            if (FilteredList.Count < 1)
                return null;
            if (actIndex < 0)
                actIndex = 0;
            if (actIndex >= FilteredList.Count)
                actIndex = FilteredList.Count - 1;
            var name = FilteredList[actIndex].Key;
            //UpdateStats();
            _sortType = sortType;
            switch (sortType)
            {
                case SortType.AZ:
                    FilteredList.Sort();
                    break;
                case SortType.Ranking:
                    FilteredList.Sort(new RankingComparer());
                    break;
                case SortType.MostRecentlyPlayed:
                    FilteredList.Sort(new MostRecentlyPlayedComparer());
                    break;
                case SortType.Year:
                    FilteredList.Sort(new YearComparer());
                    break;
                case SortType.Manufacturer:
                    FilteredList.Sort(new ManufacturerComparer());
                    break;
                case SortType.Category:
                    FilteredList.Sort(new CategoryComparer());
                    break;
                case SortType.InputControl:
                    FilteredList.Sort(new InputControlComparer());
                    break;
                case SortType.ExternalKey:
                    FilteredList.Sort(new ExternalKeyComparer());
                    break;
            }
            UpdateSelection();
            return FilteredList.Find(c => c.Key == name);
            //return FilteredList[actIndex];
        }

        public void ResetStats()
        {
            _maxPlayedTime = TimeSpan.Zero;         // il tempo della rom più giocata
            _totalPlayedTime = TimeSpan.Zero;       // il totale del tempo giocato

            foreach (var item in _listBox)
            {
                item.PlayedCount = 0;
                item.PlayedTimeNow = item.PlayedTime = TimeSpan.Zero;
                item.TimeStamp = DateTime.MinValue;
            }
        }

        public void UpdateStats()
        {
            var now = DateTime.MinValue;
            _maxPlayedTime = TimeSpan.Zero;         // il tempo della rom più giocata
            _totalPlayedTime = TimeSpan.Zero;       // il totale del tempo giocato
            foreach (var item in _listBox)
            {
                // exact values
                now = DateTime.Compare(item.TimeStamp, now) == 1 ? item.TimeStamp : now;
                _maxPlayedTime = TimeSpan.Compare(item.PlayedTime, _maxPlayedTime) == 1 ? item.PlayedTime : _maxPlayedTime;
                _totalPlayedTime = _totalPlayedTime.Add(item.PlayedTime);
            }

            _maxPlayedTimeNow = TimeSpan.Zero;      // 
            _totalPlayedTimeNow = TimeSpan.Zero;    // 
            foreach (var item in _listBox)
            {
                if (item.TimeStamp == DateTime.MinValue || item.PlayedTime == TimeSpan.Zero)
                    continue;
                var elapsedHours = Math.Min(now.Subtract(item.TimeStamp).TotalDays, 500);
                var minutes = Math.Max(item.PlayedTime.TotalMinutes - (elapsedHours / Math.Max(item.PlayedTime.TotalMinutes, 1)), 0.0);
                
                // NO REDUCED STATS ON OLDER PLAYED
                //item.PlayedTimeNow = TimeSpan.FromMinutes(minutes);
                item.PlayedTimeNow = item.PlayedTime;

                // time-rate values
                _maxPlayedTimeNow = TimeSpan.Compare(item.PlayedTimeNow, _maxPlayedTimeNow) == 1 ? item.PlayedTimeNow : _maxPlayedTimeNow;
                _totalPlayedTimeNow = _totalPlayedTimeNow.Add(item.PlayedTimeNow);
            }
        }

        public RomDesc FindRom(RomDesc rom)
        {
            if (rom != null)
                actIndex = FilteredList.FindIndex(c => c.Key == rom.Key);
            UpdateSelection();
            if (actIndex != -1)
                return FilteredList[actIndex];
            else
                return rom;
        }

        private static string[] sortDescList =
        {
            "A...Z",
            "Ranking",
            "Most Recently Played",
            "Year",
            "Manufacturer",
            "Category",
            "Input Control",
            "F.NET"
        };

        public static string SortTypeToDesc(SortType sortType)
        {
            return sortDescList[(int)sortType];
        }

        public string CurrentSortDesc
        {
            get
            {
                return SortTypeToDesc(_sortType);
            }
        }
    }
}
