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
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace feel
{
    class CImage: CDrawable
    {
        public bool VideoSizeIsSet = false;
        private int _deltaX = 0;
        private int _deltaY = 0;

        protected Texture2D _texture;
        private Texture2D _notFoundTexture;
        private Rectangle _imageDef;
        private Rectangle _backgroundImageDef;
        private Rectangle _rotatedImageDef;
        private Rectangle _rotatedBackgroundImageDef;
        private static Vector2 _origin = new Vector2(0, 0);
        private Vector2 _position;
        private Vector2 _rotatedPosition;
        private string _altFileName = "";
        private string _fileNotFoundName = "";
        private bool _stretch;

        private Texture2D _blackTexture;
        public bool BlackBackground { get; set; }
        public override int X { get { return _x; } set { base.X = value; Refresh(true); } }
        public override int Y { get { return _y; } set { base.Y = value; Refresh(true); } }
        public override int Height { get { return _height; } set { base.Height = _imageDef.Height = value; Refresh(false); } }
        public override int Width { get { return _width; } set { base.Width = _imageDef.Width = value; Refresh(false); } }
        public string FileNotFoundName { get { return _fileNotFoundName; } set { _fileNotFoundName = value; } }
        public string FileName { get; set; }

        public CImage(string fileName, int width, int height, string fileNotFoundName, bool stretch, bool isVisible)
            : this(fileName, width, height, fileNotFoundName, stretch, isVisible, false)
        {
        }

        public CImage(string fileName, int width, int height, string fileNotFoundName, bool stretch, bool isVisible, bool blackBackground)
        {
            _fileNotFoundName = fileNotFoundName;
            Visible = isVisible;
            _width = width;
            _height = height;
            _stretch = stretch;
            _notFoundTexture = CreateNotFoundTexture();
            _imageDef = new Rectangle(0, 0, _width, _height);

            _blackTexture = new Texture2D(_feel.GraphicsDevice, 1, 1);
            _blackTexture.SetData(new Color[] { Color.Black });
            BlackBackground = blackBackground;

            LoadImage(fileName);
        }

        public virtual void LoadImage(string fileName)
        {
            LoadImage(fileName, string.Empty, false, true);
        }

        public void LoadImage(string fileName, bool withTransition)
        {
            LoadImage(fileName, string.Empty, withTransition, true);
        }

        public void LoadImage(string fileName, string altFileName, bool withTransition, bool immediate)
        {
            if (!Visible || fileName == FileName) return;

            Action action = () =>
            {
                do
                {
                    FileName = fileName;
                    if (fileName == null)
                        _texture = null;
                    else if (fileName.StartsWith("*"))
                    {
                        _texture = _feel.Content.Load<Texture2D>(fileName.Substring(1));
                        if (withTransition)
                            StartTransition(Transition.FadeIn);
                    }
                    else if (File.Exists(fileName))
                    {
                        _texture = CreateTexture(fileName);
                        if (withTransition)
                            StartTransition(Transition.FadeIn);
                    }
                    else if (fileName != string.Empty && altFileName == FileName && _altFileName == altFileName)
                    {
                        break;
                    }
                    else if (altFileName != string.Empty && File.Exists(altFileName))
                    {
                        FileName = _altFileName = altFileName;
                        _texture = CreateTexture(altFileName);
                        if (withTransition)
                            StartTransition(Transition.FadeIn);
                    }
                    else
                    {
                        if (_texture != _notFoundTexture)
                        {
                            _texture = _notFoundTexture;
                            if (withTransition)
                                StartTransition(Transition.FadeIn);
                        }
                    }
                    Refresh(false);
                } while (false);
            };
            if (immediate)
                action();
            else
                _feel.RunAsyncAction(action);
        }

        private Texture2D CreateNotFoundTexture()
        {
            if (_notFoundTexture == null && !string.IsNullOrEmpty(_fileNotFoundName) && File.Exists(_fileNotFoundName))
                _notFoundTexture = CreateTexture(_fileNotFoundName);
            return _notFoundTexture;
        }

        protected Texture2D CreateTexture(string filename)
        {
            return Texture2D.FromFile(_feel.GraphicsDevice, filename);
        }

        public void SetTexture(Texture2D texture)
        {
            _texture = texture;
            if (!VideoSizeIsSet)
            {
                VideoSizeIsSet = true;
                Refresh(false);
            }
        }

        public void Refresh(bool moved)
        {
            if (_texture != null)
            {
                if (moved | _stretch)
                {
                    _imageDef.X = _x + _deltaX;
                    _imageDef.Y = _y + _deltaY;
                }
                else
                {
                    // stretch management
                    if ((float)_texture.Width / (float)_texture.Height < (float)_width / (float)_height)
                    {
                        _imageDef.Width = (int)((float)_height * (float)_texture.Width / (float)_texture.Height);
                        _imageDef.Height = _height;
                        _imageDef.Y = _y;
                        _deltaX = (_width - _imageDef.Width) / 2;
                        _imageDef.X = _x + _deltaX;
                    }
                    else
                    {
                        _imageDef.Width = _width;
                        _imageDef.Height = (int)((float)_width * (float)_texture.Height / (float)_texture.Width);
                        _imageDef.X = _x;
                        _deltaY = (_height - _imageDef.Height) / 2;
                        _imageDef.Y = _y + _deltaY;
                    }
                }
                _position = new Vector2(_x + (_imageDef.Width - _texture.Width) / 2, _y + (_imageDef.Height - _texture.Height) / 2);
                _rotatedPosition = _drawRotated ? new Vector2(_screenHeight - (_y + (_imageDef.Height - _texture.Height) / 2), _x + (_imageDef.Width - _texture.Width) / 2) : _position;
                _rotatedImageDef = _drawRotated ? new Rectangle(_screenHeight - _imageDef.Y, _imageDef.X, _imageDef.Width, _imageDef.Height) : _imageDef;
            }
            _backgroundImageDef = new Rectangle(_x, _y, _width, _height);
            _rotatedBackgroundImageDef = _drawRotated ? new Rectangle(_screenHeight - _y, _x, _width, _height) : _backgroundImageDef;
        }

        public virtual Rectangle? GetRect(int vpX, int vpY)
        {
            Rectangle destRect = _imageDef;
            int posX = _x;
            int posY = _y;

            if (posX + destRect.Width <= 0 || posX >= vpX || posY + destRect.Height <= 0 || posY >= vpY)
                return null;

            if (posX < 0)
            {
                destRect.X = destRect.X - posX;
                destRect.Width = destRect.Width + posX;
                posX = 0;
            }
            if (posY < 0)
            {
                destRect.Y = destRect.Y - posY;
                destRect.Height = destRect.Height + posY;
                posY = 0;
            }
            if (posX + destRect.Width > vpX)
                destRect.Width = destRect.Width - ((posX + destRect.Width) - vpX);
            if (posY + destRect.Height > vpY)
                destRect.Height = destRect.Height - ((posY + destRect.Height) - vpY);

            return destRect;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, Transition pendingTransition)
        {
            if (!Visible) return;

            var alpha = new Color(Color.White, _alpha * (HasFocus | Blinking ? _alphaBlink : 1f));
            if (BlackBackground)
            {
                //var backgroundAlpha = _currentTransition == Transition.Freeze ? alpha : Color.White;
                var backgroundAlpha = alpha;
                // always fill with solid black
                spriteBatch.Draw(_blackTexture, _rotatedBackgroundImageDef, null, backgroundAlpha, _rotationAngle, _origin, SpriteEffects.None, 0f);
            }
            if (_texture != null)
            {
                spriteBatch.Draw(_texture, _rotatedImageDef, null, alpha, _rotationAngle, _origin, SpriteEffects.None, 0f);
            }
            base.Draw(gameTime, spriteBatch, pendingTransition);
        }

        public void Restore()
        {
            var tmp = FileName;
            FileName = "";
            LoadImage(tmp);
        }
    }
}