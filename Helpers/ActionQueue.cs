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

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace feel
{
    public class ActionQueue : IDisposable
    {
        EventWaitHandle _wh = new AutoResetEvent(false);
        Thread _worker;
        readonly object _locker = new object();
        Queue<Action> _actions = new Queue<Action>();

        public ActionQueue()
        {
            _worker = new Thread(Work);
            _worker.Start();
        }

        public void EnqueueAction(Action action)
        {
            lock (_locker) _actions.Enqueue(action);
            _wh.Set();
        }

        public void Dispose()
        {
            EnqueueAction(null);    // Signal the consumer to exit.
            _worker.Join();         // Wait for the consumer's thread to finish.
            _wh.Close();            // Release any OS resources.
        }

        void Work()
        {
            while (true)
            {
                Action action = null;
                lock (_locker)
                    if (_actions.Count > 0)
                    {
                        action = _actions.Dequeue();
                        if (action == null) return;
                    }
                if (action != null)
                {
                    action();
                }
                else
                {
                    _wh.WaitOne();         // No more tasks - wait for a signal
                }
            }
        }
    }
}
