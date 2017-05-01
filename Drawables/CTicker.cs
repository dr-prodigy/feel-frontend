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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace feel
{
    class CTicker : CLabel
    {
        class TickerItem
        {
            public string Text;
            public DateTime ShownAt;
            public DateTime PublishDate;
            public TickerItem(string text, DateTime publishDate)
            {
                Text = text;
                ShownAt = DateTime.Now;
                PublishDate = publishDate;
            }
        }

        public DateTime LastUpdate = DateTime.MinValue;
        private List<TickerItem> _itemList = new List<TickerItem>();
        private int _pointer = 0;

        public CTicker(int xPos, int yPos, int width, int height, string fontName,
              int fontSize, System.Drawing.FontStyle fontStyle, Color fontColor, Color bgColor, bool isFocusable):
            base (xPos, yPos, width, height, string.Empty, fontName, fontSize, fontStyle, fontColor, bgColor, TextAlign.Left, isFocusable)
        {
            _isTicker = true;
            ScrollSpeed = 2;
        }

        public void AddMessage(string text, DateTime publishDate)
        {
            if (_itemList.Count == 0) StartTransition(Transition.FadeIn);
            _itemList.Add(new TickerItem(text, publishDate));
        }

        protected override void SetNewText()
        {
            if (_itemList.Count == 0) return;
            if (_pointer >= _itemList.Count)
            {
                _pointer = 0;
            }
            while (_itemList.Count > _pointer)
            {
                if (DateTime.Now.Subtract(_itemList[_pointer].ShownAt).TotalMinutes > 1)
                {
                    _itemList.RemoveAt(_pointer);
                    Text = string.Empty;
                    if (_itemList.Count == 0)
                        this.StartTransition(Transition.FadeOut);
                }
                else
                {
                    Text = _itemList[_pointer].Text;
                    LastUpdate = LastUpdate < _itemList[_pointer].PublishDate ? _itemList[_pointer].PublishDate : LastUpdate;
                    _pointer++;
                    break;
                }
            }
        }
    }
}