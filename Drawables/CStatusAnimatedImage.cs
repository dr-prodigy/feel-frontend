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

using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace feel
{
    class CStatusAnimatedImage : CAnimatedImage
    {
        private CAnimatedImage[] _images = new CAnimatedImage[6];

        public enum enStatus
        {
            Idle = 0,
            CommandUp = 1,
            CommandDown = 2,
            CommandLeft = 3,
            CommandRight = 4,
            CommandMenu = 5,
            CommandStartEmu = 6
        }

        private enStatus _status;
        private CAnimatedImage _currentImage;
        public enStatus Status
        {
            get
            {
                return _status;
            }
            set
            {
                if (_status != value)
                {
                    // status changed..
                    _status = value;

                    // save current animation
                    var previousImage = _currentImage;

                    // set new animation
                    if (_status == enStatus.Idle)
                        _currentImage = this;
                    else if (_images[(int)_status - 1] != null)
                        _currentImage = _images[(int)_status - 1];
                    else
                        // in case relevant animation missing, default to main
                        _currentImage = this;

                    // if animation changed, run it from first frame
                    if (previousImage != _currentImage)
                        _currentImage.Reset();
                }
            }
        }

        public CStatusAnimatedImage(string fileName, int width, int height, string fileNotFoundName, bool stretch, bool isVisible, int frame_duration_ms, int repeat_delay_ms)
            : base(fileName, width, height, fileNotFoundName, stretch, isVisible, frame_duration_ms, repeat_delay_ms)
        {
            _currentImage = this;
            var fileNameExt = Utils.GetFilenameExt(fileName);
            for (var iLoop = 0; iLoop < 7; iLoop++)
            {
                var completeFileName = Utils.GetFilenameBase(fileName);
                switch ((enStatus)iLoop)
                {
                    case enStatus.Idle:
                        // skip first: it's me
                        continue;
                    case enStatus.CommandUp:
                        completeFileName += "_up";
                        break;
                    case enStatus.CommandDown:
                        completeFileName += "_down";
                        break;
                    case enStatus.CommandLeft:
                        completeFileName += "_left";
                        break;
                    case enStatus.CommandRight:
                        completeFileName += "_right";
                        break;
                    case enStatus.CommandMenu:
                        completeFileName += "_menu";
                        break;
                    case enStatus.CommandStartEmu:
                        completeFileName += "_start";
                        break;
                }
                completeFileName += fileNameExt;
                if (File.Exists(completeFileName))
                    _images[iLoop - 1] = new CAnimatedImage(completeFileName, width, height, fileNotFoundName, stretch, isVisible, frame_duration_ms, repeat_delay_ms);
                else
                    _images[iLoop - 1] = null;
            }
        }

        public bool RunAfterAnimation(CStatusAnimatedImage.enStatus status, System.Action callback)
        {
            if (status == enStatus.Idle)
            {
                SetCallbackAction(callback);
                return true;
            }
            else if (_images[(int)status - 1] != null)
            {
                _images[(int)status - 1].SetCallbackAction(callback);
                return true;
            }
            // in case relevant animation missing, don't set it
            return false;
        }

        public double GetTotalFrameDuration(CStatusAnimatedImage.enStatus status)
        {
            if (status == enStatus.Idle)
                return this.TotalFrameDuration;
            else if (_images[(int)status - 1] != null)
                return _images[(int)status - 1].TotalFrameDuration;
            else
                // in case relevant animation missing, return 0
                return 0;
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime, SpriteBatch spriteBatch, Transition pendingTransition)
        {
            if (_currentImage == this)
                base.Draw(gameTime, spriteBatch, pendingTransition);
            else
                _currentImage.Draw(gameTime, spriteBatch, pendingTransition);
        }
    }
}