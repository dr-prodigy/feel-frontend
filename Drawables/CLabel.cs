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

namespace feel
{
    class CLabel: CDrawable
    {
        private Vector2 _position;
		private Rectangle _rect;
        private Rectangle _scissorRect;
        private Rectangle _rotatedScissorRect;
        private Vector2 _rotatedPosition;
        private Rectangle _srcRect;
        private Rectangle _dstRect;
        protected string _text = "";
        private bool _scrolling = true;
        protected bool _isTicker = false;
        private string _fontName;
        private Color _fontColor;
        private float _fontColorAlpha;
        private int _fontSize;
        private System.Drawing.FontStyle _fontStyle;
        private SpriteFont _spriteFont;
        private TextAlign _textAlign;

        public CLabel(int xPos, int yPos, int width, int height, string text,
		              string fontName,
                      int fontSize,
                      System.Drawing.FontStyle fontStyle,
		              Color fontColor, Color bgColor, TextAlign textAlign, bool isFocusable)
        {
            RefreshObject(xPos, yPos, width, height, text,
			              fontName,
                          fontSize,
                          fontStyle,
			              fontColor, bgColor, textAlign, true, isFocusable);
        }

        public virtual void RefreshObject(int xPos, int yPos, int width, int height, string text,
		                          string fontName,
                                  int fontSize,
                                  System.Drawing.FontStyle fontStyle,
		                          Color fontColor, Color bgColor, TextAlign textAlign, bool visible, bool isFocusable)
        {
            _x = xPos;
            _y = yPos;
            _rect = new Rectangle(xPos, yPos, width, height);
            _width = width;
            _height = height;
            // apply scissor only on large labels
            if (width > 30)
                _scissorRect = new Rectangle(xPos + 4, yPos, width - 8, height);
            else
                _scissorRect = new Rectangle(xPos, yPos, width, height);

            _fontName = fontName;
            _fontColor = fontColor;
            _fontSize = fontSize;
            _fontStyle = fontStyle;
            _fontColorAlpha = _fontColor.A / 255.0f;
            _spriteFont = Utils.LoadSpriteFont(_feel, _fontName, _fontSize, _fontStyle);
            _textAlign = textAlign;
            _bgColor = bgColor;
            Visible = visible;
            if (text != string.Empty)
                Text = text;
            IsFocusable = isFocusable;
            CreateSurface();
            ResetTextPosition();
        }

        public override int X { get { return _x; } set { base.X = value; _rect.X = _x; _scissorRect.X = _x + (_rect.Width > 30 ? 4 : 0); ResetTextPosition(); } }
        public override int Y { get { return _y; } set { base.Y = value; _rect.Y = _scissorRect.Y = _y; ResetTextPosition(); } }
        public override int Width { get { return _rect.Width; } set { base.Width = _rect.Width = value; _scissorRect.Width = value > 30 ? value - 8 : value; ResetTextPosition(); } }
        public override int Height { get { return _rect.Height; } set { base.Height = _rect.Height = value; _scissorRect.Height = value; ResetTextPosition(); } }
        public bool Scrolling { get { return _scrolling; } set { _scrolling = value; /* ResetTextPosition(); */ } }
        public int ScrollSpeed = 1;
        public bool Trimmed { get; set; }
        public override Color ForeColor { get { return _fontColor; } set { if (_fontColor != value) { _fontColor = value; _fontColorAlpha = _fontColor.A / 255.0f; } } }
        public TextAlign TextAlign { get { return _textAlign; } set { _textAlign = value; } }
        public override string Text
        {
            get { return _text; }
            set
            {
                if (_text != value)
                {
                    _text = Utils.StringCleanup(value);
                    if (_fontSize > 0)
                    {
                        var sSize = _spriteFont.MeasureString(_text);
                        _srcRect = new Rectangle(0, 0, (int)sSize.X, (int)sSize.Y);
                    }
                    ResetTextPosition();
                }
            }
        }

        public void ResetTextPosition()
        {
            _rotatedScissorRect = _drawRotated ? new Rectangle(_screenHeight - _scissorRect.Y - _scissorRect.Height, _scissorRect.X, _scissorRect.Height, _scissorRect.Width) : _scissorRect;
            _dstRect = new Rectangle(_srcRect.X, _srcRect.Y, _srcRect.Width, _srcRect.Height);
            if (_srcRect.Width > _rect.Width - 4)
                _dstRect.Width = _rect.Width - 4;
            if (_srcRect.Height > _rect.Height - 4)
                _dstRect.Height = _rect.Height - 4;
            ResetBackgroundRect();
            UpdateTextPosition();
        }

        private void UpdateTextPosition()
        {
            _position.Y = _y + (_rect.Height - _dstRect.Height) / 2;
            switch (_textAlign)
            {
                case TextAlign.Center:
                    _position.X = _x + (_rect.Width - _srcRect.Width) / 2;
                    break;
                case TextAlign.Right:
                    _position.X = _x + (_rect.Width - _srcRect.Width) - 4;
                    break;
                default:
                    _position.X = _x + 4;
                    break;
            }
            _rotatedPosition = _drawRotated ? new Vector2(_screenHeight - _position.Y, _position.X) : _position;
            if (Trimmed)
            {
                // in case of trimmed label (message / toast), redefine boundaries according to text
                var width = _srcRect.Width + 80 < _rect.Width ? _srcRect.Width + 80 : _rect.Width;
                _backgroundRect = new Rectangle((int)_position.X - 40, (int)_position.Y, width, _srcRect.Height);
                _rotatedBackgroundRect = _drawRotated ? new Rectangle(_screenHeight - _backgroundRect.Y, _backgroundRect.X, _backgroundRect.Width, _backgroundRect.Height) : _backgroundRect;
            }
        }

        protected virtual void SetNewText() { }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, Transition pendingTransition)
        {
            if (!Visible) return;
            base.Draw(gameTime, spriteBatch, pendingTransition);

            // end sprite batch
            spriteBatch.End();

            // apply text scrolling
            if ((_scrolling && _srcRect.Width > _rect.Width - 4) || _isTicker)
            {
                if (_dstRect.X < _srcRect.Width)
                {
                    if (_position.X > _x)
                        // text appearing from right
                        _position.X -= ScrollSpeed;
                    else
                        // text disappearing to left
                        _dstRect.X += ScrollSpeed;
                }
                else
                {
                    // text disappeared: reset position at right
                    SetNewText();
                    _dstRect.X = 0;
                    _position.X = _x + _rect.Width - 4;
                }
                _rotatedPosition = _drawRotated ? new Vector2(_screenHeight - _position.Y, _position.X) : _position;
            }

            // enable scissor
            spriteBatch.GraphicsDevice.RenderState.ScissorTestEnable = true;
            
            // copy the current scissor rect so we can restore it after
            Rectangle currentRect = spriteBatch.GraphicsDevice.ScissorRectangle;

            // set the current scissor rectangle
            spriteBatch.GraphicsDevice.ScissorRectangle = _rotatedScissorRect;

            // begin new sprite batch
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);

            // draw the text at the top left of the scissor rectangle
            // blink thru luminance version
            // var blink = (HasFocus ? _alphaBlink : 1f);
            // spriteBatch.DrawString(_spriteFont, _text, _rotatedPosition, new Color(((float)_fontColor.R / 256f) * blink, ((float)_fontColor.G / 256f) * blink, ((float)_fontColor.B / 256f) * blink, _fontColorAlpha * _alpha), _rotationAngle, new Vector2(_dstRect.X, _dstRect.Y), 1f, SpriteEffects.None, 0);
            spriteBatch.DrawString(_spriteFont, _text, _rotatedPosition, new Color(_fontColor, _fontColorAlpha * _alpha * (HasFocus | Blinking ? _alphaBlink : 1f)), _rotationAngle, new Vector2(_dstRect.X, _dstRect.Y), 1f, SpriteEffects.None, 0);
            spriteBatch.End();

            // restore spritebatch and scissor rectangle to the default state
            spriteBatch.GraphicsDevice.ScissorRectangle = currentRect;
            spriteBatch.GraphicsDevice.RenderState.ScissorTestEnable = false;
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
        }

        public void Restore()
        {
            CreateSurface();
            ResetTextPosition();
        }
    }
}
