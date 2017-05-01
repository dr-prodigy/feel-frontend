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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace feel
{
    class CLayout : CDrawable
    {
        private List<CDrawable> _items = new List<CDrawable>();

        public enum Orientation { Horizontal, Vertical };
        public Orientation LayoutOrientation { get; set; }
        public enum LayoutGravity { Left, Center, Right, Top, Bottom };
        public LayoutGravity Gravity { get; set; }
        public int PaddingLeft { get; set; }
        public int PaddingRight { get; set; }
        public int PaddingTop { get; set; }
        public int PaddingBottom { get; set; }
        public int Padding { set { PaddingLeft = PaddingRight = PaddingTop = PaddingBottom = value; } }
        public override Color ForeColor { get { return _fgColor; }
            set
            {
                foreach (var item in _items)
                {
                    item.ForeColor = value;
                }
                _fgColor = value;
            }
        }
        public override Color BackColor { get { return _bgColor; }
            set
            {
                if (_bgColor != value)
                {
                    foreach (var item in _items)
                    {
                        item.BackColor = value;
                    }
                    _bgColor = value; CreateSurface();
                }
            }
        }
        public List<CDrawable> Items { get { return _items; } } 

        public CLayout()
        {
            Visible = true;
        }

        public CLayout(int xPos, int yPos, Orientation orientation, bool isFocusable)
        {
            _x = xPos;
            _y = yPos;
            IsFocusable = isFocusable;
            Visible = true;
            LayoutOrientation = orientation;
        }

        public void Clear()
        {
            _items.Clear();
            _height = _width = 0;
        }

        public void AddItem(CDrawable item)
        {
            if (item == null) return;
            item.Parent = (CDrawable)this;
            _items.Add(item);
        }

        public void Update()
        {
            if (_items.Count > 0)
            {
                if (LayoutOrientation == Orientation.Horizontal)
                {
                    var totalWidth = PaddingLeft;
                    // loop items
                    foreach (var item in _items)
                    {
                        // move after last item
                        item.X = _x + totalWidth;
                        item.Y = _y + PaddingTop;

                        // update heights (parent, item)
                        _height = Math.Max(_height, PaddingTop + item.Height + PaddingBottom);
                        item.Height = _height - PaddingTop - PaddingBottom;
                        if (item.FillParent && _width > 0)
                            item.Width = _width - totalWidth - PaddingLeft - PaddingRight;

                        // if it's a layout, update it
                        if (item.GetType() == typeof(CLayout))
                            ((CLayout)item).Update();
                        
                        // update parent height
                        _height = Math.Max(_height, PaddingTop + item.Height + PaddingBottom);
                        totalWidth += item.Width;
                    }
                    totalWidth += PaddingRight;
                    // apply gravity
                    switch (Gravity)
                    {
                        case LayoutGravity.Center:
                            if (_width != totalWidth) foreach (var item in _items) item.X += (_width - totalWidth) / 2;
                            break;
                        case LayoutGravity.Right:
                            if (_width != totalWidth) foreach (var item in _items) item.X += _width - totalWidth;
                            break;
                    }
                    // update parent width
                    _width = Math.Max(_width, totalWidth);
                }
                else
                {
                    var totalHeight = PaddingTop;
                    // loop items
                    foreach (var item in _items)
                    {
                        // move after last item
                        item.Y = _y + totalHeight;
                        item.X = _x + PaddingLeft;

                        // update widths (parent, item)
                        _width = Math.Max(_width, PaddingLeft + item.Width + PaddingRight);
                        item.Width = _width - PaddingLeft - PaddingRight;
                        if (item.FillParent && _height > 0)
                            item.Height = _height - totalHeight - PaddingTop - PaddingBottom;

                        // if it's a layout, update it
                        if (item.GetType() == typeof(CLayout))
                            ((CLayout)item).Update();

                        // update parent width
                        _width = Math.Max(_width, PaddingLeft + item.Width + PaddingRight);
                        totalHeight += item.Height;
                    }
                    totalHeight += PaddingBottom;
                    // apply gravity
                    switch (Gravity)
                    {
                        case LayoutGravity.Center:
                            if (_height != totalHeight) foreach (var item in _items) item.Y += (_height - totalHeight) / 2;
                            break;
                        case LayoutGravity.Bottom:
                            if (_height != totalHeight) foreach (var item in _items) item.Y += _height - totalHeight;
                            break;
                    }
                    // update parent height
                    _height = Math.Max(_height, totalHeight);
                }
            }
        }

        public void CopyLayout(CLayout masterLayout)
        {
            var counter = 0;
            foreach (var item in masterLayout._items)
            {
                if (_items.Count > counter)
                {
                    _items[counter].X = item.X;
                    _items[counter].Y = item.Y;
                    _items[counter].Width = item.Width;
                    _items[counter].Height = item.Height;
                }
                counter++;
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, Transition pendingTransition)
        {
            if (!Visible) return;
            base.Draw(gameTime, spriteBatch, pendingTransition);
            foreach (var item in _items)
            {
                item.Draw(gameTime, spriteBatch, pendingTransition);
            }
        }

        public override void StartTransition(Transition pendingTransition, int delayMs, Action callbackAction)
        {
            // add callback
            base.StartTransition(pendingTransition, delayMs, callbackAction);
            // dispatch transition to all items
            foreach (var item in _items)
            {
                item.StartTransition(pendingTransition, delayMs, null);
            }
        }
    }
}