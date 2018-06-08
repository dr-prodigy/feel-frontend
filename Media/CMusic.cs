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

using System.IO;
using System.Runtime.InteropServices;
using System;
using System.Text;

namespace feel
{
    class CMusic
    {
        private string _command = "";
        private int _volume;
        private bool isOpen;

        [DllImport("winmm.dll")]
        private static extern long mciSendString(string strCommand, StringBuilder strReturn, int iReturnLength, IntPtr hwndCallback);

        ~CMusic()
        {
            Dispose();
        }

        public string KeyName { get; set; }

        public string FileName { get; set; }

        public void SetMusic(string fileName, int volume)
        {
            if (FileName == fileName)
            {
                if (isOpen)
                {
                    _command = "seek MediaFile to start";
                    mciSendString(_command, null, 0, IntPtr.Zero);
                }
                return;
            }

            FileName = fileName;
            if (File.Exists(FileName))
            {
                if (isOpen)
                    Dispose();
                FileName = fileName;
                _command = "open \"" + fileName + "\" type mpegvideo alias MediaFile";
                mciSendString(_command, null, 0, IntPtr.Zero);
                _command = "set MediaFile time format milliseconds";
                mciSendString(_command, null, 0, IntPtr.Zero);
                _command = "set MediaFile seek exactly on";
                mciSendString(_command, null, 0, IntPtr.Zero);
                isOpen = true;
                SetVolume(volume);
            }
            else
                isOpen = false;
        }

        public int Volume { get { return _volume; } set { SetVolume(value); } }

        private void SetVolume(int percentage)
        {
            if (isOpen)
            {
                if (percentage < 0)
                    percentage = 0;
                if (percentage > 100)
                    percentage = 100;
                _volume = percentage * 10;
                _command = String.Format("setaudio MediaFile volume to {0}", _volume);
                mciSendString(_command, null, 0, IntPtr.Zero);
            }
        }

        public void Dispose()
        {
            if (isOpen)
            {
                _command = "close MediaFile";
                mciSendString(_command, null, 0, IntPtr.Zero);
                isOpen = false;
            }
        }

        public void Play()
        {
            Play(IntPtr.Zero);
        }

        public void Play(IntPtr hWnd)
        {
            if (isOpen)
            {
                _command = "play MediaFile notify";
                mciSendString(_command, null, 0, hWnd);
            }
        }

        public void Stop()
        {
            _command = "stop MediaFile";
            mciSendString(_command, null, 0, IntPtr.Zero);
        }

        public void Pause()
        {
            _command = "pause MediaFile";
            mciSendString(_command, null, 0, IntPtr.Zero);
        }

        public void Resume()
        {
            _command = "play MediaFile";
            mciSendString(_command, null, 0, IntPtr.Zero);
        }
    }
}
