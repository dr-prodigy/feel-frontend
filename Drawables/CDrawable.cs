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
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace feel
{
    public abstract class CDrawable
    {
        protected static Feel _feel;
        protected int _x = 0;
        protected int _y = 0;
        protected int _width = 0;
        protected int _height = 0;

        protected Transition _currentTransition = Transition.None;
        protected Action _callbackAction = null;
        protected float _alpha = 1f;
        protected float _alphaBlink = 1f;
        protected double _delayMs = 0;

        protected static bool _drawRotated;
        protected static int _screenWidth;
        protected static int _screenHeight;
        protected static float _rotationAngle;

        protected Color _fgColor = Color.TransparentWhite;
        protected Color _bgColor = Color.TransparentWhite;
        protected Color _borderColor = Color.TransparentWhite;
        private Texture2D _backgroundPixel;
        private Texture2D _borderPixel;
        protected Rectangle _backgroundRect;
        protected Rectangle _rotatedBackgroundRect;

        public virtual float Alpha
        {
            get { return _alpha; }
        }

        public virtual int X { get { return _x; } set { _x = value; ResetBackgroundRect(); } }
        public virtual int Y { get { return _y; } set { _y = value; ResetBackgroundRect(); } }
        public virtual int Width { get { return _width; } set { _width = value; ResetBackgroundRect(); } }
        public virtual int Height { get { return _height; } set { _height = value; ResetBackgroundRect(); } }
        public bool Visible { get; set; }
        public bool FillParent { get; set; }
        public virtual Color ForeColor { get { return _fgColor; } set { _fgColor = value; } }
        public virtual Color BackColor { get { return _bgColor; } set { if (_bgColor != value) { _bgColor = value; CreateSurface(); } } }
        public virtual Color BorderColor { get { return _borderColor; } set { _borderColor = value; CreateSurface(); } }
        public string KeyName { get; set; }
        public virtual string Text { get; set; }


        public CDrawable Parent { get; set; }

        public enum Transition
        {
            None,
            FadeIn,
            FadeOut,
            FastFadeOut,
            Slide,
            Freeze
        }

        private static CDrawable focusedDrawable;

        public bool IsFocusable { get; set; }

        public static void Init(Feel feel, int screenWidth, int screenHeight, bool drawRotated)
        {
            _feel = feel;
            _drawRotated = drawRotated;
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            _rotationAngle = drawRotated ? MathHelper.PiOver2 : 0;
        }

        public void StartTransition(Transition pendingTransition)
        {
            StartTransition(pendingTransition, 0, null);
        }

        public void StartTransition(Transition pendingTransition, int delayMs)
        {
            StartTransition(pendingTransition, delayMs, null);
        }

        public virtual void StartTransition(Transition pendingTransition, int delayMs, Action callbackAction)
        {
            if (pendingTransition != Transition.None)
            {
                _delayMs = delayMs;
            }

            switch (pendingTransition)
            {
                case Transition.FadeIn:
                case Transition.Freeze:
                    _alpha = 0;
                    _currentTransition = pendingTransition;
                    _callbackAction = callbackAction;
                    break;
                case Transition.FadeOut:
                case Transition.FastFadeOut:
                    //_alpha = 1;
                    _currentTransition = pendingTransition;
                    _callbackAction = callbackAction;
                    break;
            }
        }

        public void ResetPendingTransitions()
        {
            _alpha = 1f;
            _delayMs = 0;
            _currentTransition = Transition.None;
            _callbackAction = null;
        }

        public void SetAlpha(float a)
        {
            _alpha = a;
        }

        public virtual void SetFocus()
        {
            focusedDrawable = this;
        }

        public bool Blinking { get; set; }

        public virtual bool HasFocus { get { return focusedDrawable == this | (Parent != null && Parent.HasFocus); } }

        public void ResetBackgroundRect()
        {
            _backgroundRect = new Rectangle(_x, _y, _width, _height);
            _rotatedBackgroundRect = _drawRotated ? new Rectangle(_screenHeight - _backgroundRect.Y, _backgroundRect.X, _backgroundRect.Width, _backgroundRect.Height) : _backgroundRect;
        }

        protected void CreateSurface()
        {
            _backgroundPixel = new Texture2D(_feel.GraphicsDevice, 1, 1, 1, TextureUsage.None, SurfaceFormat.Color);
            _backgroundPixel.SetData(new Color[] { _bgColor });
            _borderPixel = new Texture2D(_feel.GraphicsDevice, 1, 1, 1, TextureUsage.None, SurfaceFormat.Color);
            _borderPixel.SetData(new[] { _borderColor });
        }

        private float blinkChange = .025f;
        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch, Transition pendingTransition)
        {
            if (!Visible) return;
            if (_delayMs <= 0)
            {
                // process pending transitions
                switch (_currentTransition)
                {
                    case Transition.FadeIn:
                        if (_alpha < 1)
                            _alpha += 0.07f;
                        else
                        {
                            _alpha = 1;
                            _currentTransition = Transition.None;
                            if (_callbackAction != null)
                                _callbackAction.Invoke();
                        }
                        break;
                    case Transition.FadeOut:
                    case Transition.FastFadeOut:
                        if (_alpha > 0)
                        {
                            _alpha -= 0.02f;
                            if (_currentTransition == Transition.FastFadeOut)
                                _alpha -= 0.06f;
                        }
                        else
                        {
                            _alpha = 0;
                            _currentTransition = Transition.None;
                            if (_callbackAction != null)
                                _callbackAction.Invoke();
                        }
                        break;
                }

                // start pending transition
                //if (_currentTransition != Transition.Freeze)
                //    StartTransition(pendingTransition);
            }
            else
                _delayMs -= gameTime.ElapsedRealTime.TotalMilliseconds;

            // update blink
            if (_alphaBlink >= 1.0f || _alphaBlink < 0.3f)
                blinkChange = -blinkChange;
            _alphaBlink += blinkChange;

            // draw background
            if (_bgColor != Color.TransparentWhite)
                spriteBatch.Draw(_backgroundPixel, _rotatedBackgroundRect, null, new Color(_bgColor, _alpha), _rotationAngle, new Vector2(0, 0), SpriteEffects.None, 0f);

            // draw border
            if (_borderColor != Color.TransparentWhite)
                DrawBorder(spriteBatch, _rotatedBackgroundRect, 1, new Color(_borderColor, _alpha));
        }

        private void DrawBorder(SpriteBatch spriteBatch, Microsoft.Xna.Framework.Rectangle rectangleToDraw, int thicknessOfBorder, Color borderColor)
        {
            // draw top line
            spriteBatch.Draw(_borderPixel, new Microsoft.Xna.Framework.Rectangle(rectangleToDraw.X, rectangleToDraw.Y, rectangleToDraw.Width, thicknessOfBorder), null, borderColor, _rotationAngle, new Vector2(0, 0), SpriteEffects.None, 0f);

            // draw left line
            spriteBatch.Draw(_borderPixel, new Microsoft.Xna.Framework.Rectangle(rectangleToDraw.X, rectangleToDraw.Y, thicknessOfBorder, rectangleToDraw.Height), null, borderColor, _rotationAngle, new Vector2(0, 0), SpriteEffects.None, 0f);

            // draw right line
            spriteBatch.Draw(_borderPixel, new Microsoft.Xna.Framework.Rectangle((rectangleToDraw.X + (_drawRotated ? 0 : rectangleToDraw.Width - thicknessOfBorder)),
                                            rectangleToDraw.Y + (_drawRotated ? rectangleToDraw.Width - thicknessOfBorder : 0),
                                            thicknessOfBorder,
                                            rectangleToDraw.Height)
                                            , null, borderColor, _rotationAngle, new Vector2(0, 0), SpriteEffects.None, 0f);
            // draw bottom line
            spriteBatch.Draw(_borderPixel, new Microsoft.Xna.Framework.Rectangle(rectangleToDraw.X - (_drawRotated ? rectangleToDraw.Height - thicknessOfBorder : 0),
                                            rectangleToDraw.Y + (_drawRotated ? 0 : rectangleToDraw.Height - thicknessOfBorder),
                                            rectangleToDraw.Width,
                                            thicknessOfBorder)
                                            , null, borderColor, _rotationAngle, new Vector2(0, 0), SpriteEffects.None, 0f);
        }
    }
}
