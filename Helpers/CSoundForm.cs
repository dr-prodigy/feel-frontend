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

using System.Windows.Forms;

namespace feel
{
    class CSoundForm : Form
    {
        private bool _isPlayEnded = false;

        private const int MM_MCINOTIFY = 0x03b9;
        private const int MCI_NOTIFY_SUCCESS = 0x01;
        private const int MCI_NOTIFY_SUPERSEDED = 0x02;
        private const int MCI_NOTIFY_ABORTED = 0x04;
        private const int MCI_NOTIFY_FAILURE = 0x08;

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case MM_MCINOTIFY:
                    switch (m.WParam.ToInt32())
                    {
                        case MCI_NOTIFY_SUCCESS:
                            // success handling
                            _isPlayEnded = true;
                            break;
                        case MCI_NOTIFY_SUPERSEDED:
                            // superseded handling
                            _isPlayEnded = true;
                            break;
                        case MCI_NOTIFY_ABORTED:
                            // abort handling
                            break;
                        case MCI_NOTIFY_FAILURE:
                            // failure! handling
                            _isPlayEnded = true;
                            break;
                        default:
                            // haha
                            break;
                    }
                    break;
            }
            base.WndProc(ref m);
        }

        public void Reset()
        {
            _isPlayEnded = false;
        }

        public bool IsPlayEnded { get { return _isPlayEnded; } set { _isPlayEnded = value; } }
    }
}
