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
using System.Runtime.InteropServices;
using System.IO;
using System.Text;

namespace feel
{
    class CMciVideo
    {
        private int _xPos = 0;
        private int _yPos = 0;
        private int _width;
        private int _height;
        private string _fileName;
        private string _command = "";
        private int _volume;
        private IntPtr _parentWindowHandle;
        private bool _isPlaying = false;
        private bool _isPaused = false;
        private CSoundForm _controllerForm;

        //[DllImport("winmm.dll")]
        //private static extern long mciSendString(string strCommand, StringBuilder strReturn, int iReturnLength, IntPtr hwndCallback);

		private static long mciSendString(string strCommand, StringBuilder strReturn, int iReturnLength, IntPtr hwndCallback)
		{
			// foo
			return 0;
		}

        public CMciVideo(IntPtr parentWindowHandle)
        {
            _parentWindowHandle = parentWindowHandle;
            _controllerForm = new CSoundForm();
        }

        ~CMciVideo()
        {
            Dispose();
        }

        public int X { get { return _xPos; } set { _xPos = value; } }

        public int Y { get { return _yPos; } set { _yPos = value; } }

        public int Width { get { return _width; } }

        public int Height { get { return _height; } }

        public void PlayVideo(string fileName, int width, int height, int xPos, int yPos, int volume, float speed)
        {
                if (_fileName == fileName)
                {
                    if (_controllerForm.IsPlayEnded && PlayLooped)
                    {
                        _isPlaying = false;
                        _command = "seek VideoSnap to start";
                        mciSendString(_command, null, 0, IntPtr.Zero);
                        Play();
                        return;
                    }
                    else
                        return;
                }

            _width = width;
            _height = height;
            _xPos = xPos;
            _yPos = yPos;

            _isPlaying = false;

            _fileName = fileName;
            Dispose();
            if (File.Exists(_fileName))
            {
                _fileName = fileName;
                _command = "open \"" + fileName + "\" type mpegvideo alias VideoSnap style child parent " + _parentWindowHandle;
                mciSendString(_command, null, 0, IntPtr.Zero);
                _command = String.Format("put VideoSnap window at {0} {1} {2} {3}", _xPos, _yPos, _width, _height);
                mciSendString(_command, null, 0, IntPtr.Zero);
                if (speed != 1)
                {
                    _command = String.Format("set VideoSnap speed {0}", speed * 1000);
                    mciSendString(_command, null, 0, IntPtr.Zero);
                }
                SetVolume(volume);
                Play();
            }
        }

        public int Volume { get { return _volume; } set { SetVolume(value); } }
        public bool IsPlaying { get { return _isPlaying; } }
        public bool PlayLooped { get; set; }

        private void SetVolume(int percentage)
        {
            if (percentage < 0)
                percentage = 0;
            if (percentage > 100)
                percentage = 100;
            _volume = percentage * 10;
            _command = String.Format("setaudio VideoSnap volume to {0}", _volume);
            mciSendString(_command, null, 0, IntPtr.Zero);
        }

        private void Dispose()
        {
            _command = "close VideoSnap";
            mciSendString(_command, null, 0, IntPtr.Zero);
            _isPlaying = false;
        }

        private void ShowVideo()
        {
            _command = "window VideoSnap state show wait";
            mciSendString(_command, null, 0, IntPtr.Zero);
        }

        private void HideVideo()
        {
            _command = "window VideoSnap state hide wait";
            mciSendString(_command, null, 0, IntPtr.Zero);
        }

        public void Play()
        {
            if (!_isPlaying && !_isPaused)
            {
                _command = "play VideoSnap notify";
                mciSendString(_command, null, 0, _controllerForm.Handle);
                _isPlaying = true;
                _controllerForm.Reset();
            }
        }

        public void Stop()
        {
            if (_isPlaying)
            {
                HideVideo();
                _command = "stop VideoSnap wait";
                mciSendString(_command, null, 0, IntPtr.Zero);
            }
            _fileName = string.Empty;
            _isPlaying = false;
            _isPaused = false;
        }

        public void Pause()
        {
            if (_isPlaying)
            {
                HideVideo();
                _command = "pause VideoSnap wait";
                mciSendString(_command, null, 0, IntPtr.Zero);
            }
            _isPaused = true;
        }

        public void Resume()
        {
            if (_isPlaying)
            {
                ShowVideo();
                _command = "play VideoSnap";
                mciSendString(_command, null, 0, IntPtr.Zero);
            }
            _isPaused = false;
        }
    }
}
