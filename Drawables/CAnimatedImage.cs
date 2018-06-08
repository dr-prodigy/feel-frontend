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

using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Threading;
using System;

namespace feel
{
    class CAnimatedImage: CImage
    {
        private Texture2D[] frames = new Texture2D[50];
        private int _currentFrame;
        private int _frameCount;
        private double _frameDuration;
        private double _repeatDelay;
        private double _frameChange;

        public CAnimatedImage(string fileName, int width, int height, string fileNotFoundName, bool stretch, bool isVisible, int frame_duration_ms, int repeat_delay_ms)
            : base(fileName, width, height, fileNotFoundName, stretch, isVisible, false)
        {
            _frameChange = 0;
            _frameDuration = frame_duration_ms;
            _repeatDelay = repeat_delay_ms;
        }

        public double TotalFrameDuration { get { return _frameDuration * _frameCount; } }

        public override void LoadImage(string fileName)
        {
            _currentFrame = -1;
            _frameCount = 0;

            // load main image
            base.LoadImage(fileName);

            // load additional frames
            var fileBasename = Utils.GetFilenameBase(fileName);
            var fileExt = Utils.GetFilenameExt(fileName);

            // loop files
            for (var iLoop = 0; iLoop < 50; iLoop++)
            {
                if (iLoop == 0)
                    frames[iLoop] = _texture;
                else
                {
                    var tmpFilename = iLoop == 0 ? fileName : (fileBasename + iLoop.ToString("0#") + fileExt); // filename01.png, filename02.png, filename03.png...
                    var tmpFilename2 = iLoop == 0 ? fileName : (fileBasename + iLoop + fileExt); // filename1.png, filename2.png, filename3.png...
                    if (File.Exists(tmpFilename))
                    {
                        _frameCount = iLoop + 1;
                        frames[iLoop] = CreateTexture(tmpFilename);
                    }
                    else if (File.Exists(tmpFilename2))
                    {
                        _frameCount = iLoop + 1;
                        frames[iLoop] = CreateTexture(tmpFilename2);
                    }
                    else
                    {
                        frames[iLoop] = null;
                        break;
                    }
                }
            }
        }

        private Action _callback = null;
        public virtual void SetCallbackAction(Action callback)
        {
            _callback = callback;
        }

        public void Reset()
        {
            _currentFrame = -1;
            _frameChange = 0;
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime, SpriteBatch spriteBatch, Transition pendingTransition)
        {
            _frameChange -= gameTime.ElapsedRealTime.TotalMilliseconds;
            if (_frameChange <= 0)
            {
                _frameChange = _frameDuration;
                _currentFrame++;
                if (_currentFrame == _frameCount - 1)
                {
                    if (_callback != null)
                    {
                        _callback.Invoke();
                        _callback = null;
                    }
                    _frameChange += _repeatDelay;
                }
                //else if (_currentFrame < 9 && frames[_currentFrame + 1] == null)
                //{
                //    if (_callback != null)
                //    {
                //        _callback.Invoke();
                //        _callback = null;
                //    }
                //    _frameChange += _repeatDelay;
                //}
                //if (_currentFrame > 9 || frames[_currentFrame] == null)
                if (_currentFrame >= _frameCount)
                {
                    _currentFrame = 0;
                }
                _texture = frames[_currentFrame];
            }
            base.Draw(gameTime, spriteBatch, pendingTransition);
        }
    }
}
