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
using System.Threading;
using feel.fnet;
using System.Windows.Forms;

namespace feel
{
    class Program
    {
        private static Mutex mutex;

        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 1 && args[0] == "--update")
            {
                CUpdateManager.Update(args[1]);
            }
            else
            {
                bool imFirstInstance;
                mutex = new Mutex(true, "F.E.E.L.", out imFirstInstance);
                if (!imFirstInstance)
                    return;
                GC.KeepAlive(mutex); 
            
                using (var feel = new Feel())
                {
                    var newVersion = (args.Length == 1 && args[0].Equals("--newversion")) ||
                        (args.Length == 2 && args[1].Equals("--newversion")) ||
                        (args.Length == 3 && args[2].Equals("--newversion"));
                    feel.Main(newVersion);
                }
            }
        }
    }
}
