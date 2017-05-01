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
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace feel
{
    class CVideo
    {
        private GraphicsDeviceManager _graphicsDM;
        private Feel _feel;
        private int _xPos = 0;
        private int _yPos = 0;
        private int _width;
        private int _height;
        private string _fileName;
        private int _volume;
        private bool _isPlaying = false;
        private bool _isPaused = false;
        private XNAPlayer _dxPlay;

        public CVideo(ref GraphicsDeviceManager graphicsDM, ref Feel feel)
        {
            _graphicsDM = graphicsDM;
            _feel = feel;
        }

        ~CVideo()
        {
            Dispose();
        }

        public int X { get { return _xPos; } set { _xPos = value; } }

        public int Y { get { return _yPos; } set { _yPos = value; } }

        public int Width { get { return _width; } }

        public int Height { get { return _height; } }

        public bool PlayVideo(Feel feel, string fileNameWithoutExt, string parentFilenameWithoutExt, int width, int height, int xPos, int yPos, int volume, float speed, Action callback)
        {
            if (_fileName == fileNameWithoutExt || _fileName == parentFilenameWithoutExt)
            {
                return false;
            }

            _width = width;
            _height = height;
            _xPos = xPos;
            _yPos = yPos;

            _isPlaying = false;

            Dispose();
            var fileName = string.Empty;
            for (var iLoop = 0; iLoop < 2; iLoop++)
            {
                switch (iLoop)
                {
                    case 0:
                        _fileName = fileNameWithoutExt;
                        break;
                    case 1:
                        _fileName = parentFilenameWithoutExt;
                        break;
                }
                if (File.Exists(_fileName + ".avi"))
                {
                    fileName = _fileName + ".avi";
                    break;
                }
                else if (File.Exists(_fileName + ".mp4"))
                {
                    fileName = _fileName + ".mp4";
                    break;
                }
            }
            if (fileName != string.Empty)
            {
                _dxPlay = new XNAPlayer(feel, fileName, _graphicsDM.GraphicsDevice, () =>
                    {
                        if (_dxPlay != null)
                        {
                            _dxPlay.OnVideoComplete += new EventHandler(videoPlayer_OnVideoComplete);
                            if (speed != 1)
                            {
                                _dxPlay.SetSpeed(speed);
                            }
                            SetVolume(volume);
                            Play();
                            callback();
                        }
                    }
                );
                return true;
            }
            return false;
        }

        public int Volume { get { return _volume; } set { SetVolume(value); } }
        public bool IsPlaying { get { return _isPlaying; } }
        public bool PlayLooped { get; set; }

        public bool Update()
        {
            if (_isPlaying && _dxPlay != null)
                _dxPlay.Update();
            return !_isPlaying;
        }

        void videoPlayer_OnVideoComplete(object sender, EventArgs e)
        {
            if (PlayLooped && _dxPlay != null)
            {
                _dxPlay.Rewind();
                _dxPlay.Play();
            }
        }

        public Texture2D GetTexture()
        {
            if (_dxPlay == null)
                return null;
            return _dxPlay.OutputFrame;
        }

        private void SetVolume(int percentage)
        {
            if (percentage < 0)
                percentage = 0;
            if (percentage > 100)
                percentage = 100;
            _volume = percentage * 10;
            if (_dxPlay != null)
                _dxPlay.SetVolume(percentage);
        }

        public void Dispose()
        {
            if (_dxPlay != null)
                _dxPlay.Dispose();

            _isPlaying = false;
        }

        public void Play()
        {
            if (!_isPlaying && !_isPaused)
            {
                _dxPlay.Play();
                _isPlaying = true;
            }
        }

        public void Stop()
        {
            if (_isPlaying)
            {
                if (_dxPlay != null)
                    _dxPlay.Stop();
            }
            _fileName = string.Empty;
            _isPlaying = false;
            _isPaused = false;
        }

        public void Pause()
        {
            if (_isPlaying)
            {
                if (_dxPlay != null)
                    _dxPlay.Pause();
            }
            _isPaused = true;
        }

        public void Resume()
        {
            if (_isPlaying)
            {
                if (_dxPlay != null)
                    _dxPlay.Play();
            }
            _isPaused = false;
        }
    }
}
