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
using System.Windows.Forms;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

namespace feel
{
    static class CLedManager
    {
        #region SmartAPI
        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct smart_device
        {
        }
        
        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct smart_info
        {

            /// smart_dev->smart_device*
            public IntPtr device;

            /// smart_info*
            public IntPtr next;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct smart_input
        {

            /// e1a : 1
            ///e1b : 1
            ///e2a : 1
            ///e2b : 1
            ///d1 : 1
            ///d2 : 1
            ///d3 : 1
            ///d4 : 1
            ///d5 : 1
            ///d6 : 1
            ///d7 : 1
            ///d8 : 1
            ///d9 : 1
            ///d10 : 1
            ///d11 : 1
            ///d12 : 1
            ///e3a : 1
            ///e3b : 1
            ///e4a : 1
            ///e4b : 1
            ///d13 : 1
            ///d14 : 1
            ///d15 : 1
            ///d16 : 1
            ///d17 : 1
            ///d18 : 1
            ///d19 : 1
            ///d20 : 1
            ///d21 : 1
            ///d22 : 1
            ///d23 : 1
            ///d24 : 1
            public uint bitvector1;

            /// short
            public short enc1;

            /// short
            public short enc2;

            /// short
            public short enc3;

            /// short
            public short enc4;

            /// short
            public short a1;

            /// short
            public short a2;

            /// short
            public short a3;

            /// short
            public short a4;

            /// short
            public short a5;

            /// short
            public short a6;

            /// short
            public short a7;

            /// short
            public short a8;

            public uint e1a
            {
                get
                {
                    return ((uint)((this.bitvector1 & 1u)));
                }
                set
                {
                    this.bitvector1 = ((uint)((value | this.bitvector1)));
                }
            }

            public uint e1b
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 2u)
                                / 2)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 2)
                                | this.bitvector1)));
                }
            }

            public uint e2a
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 4u)
                                / 4)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 4)
                                | this.bitvector1)));
                }
            }

            public uint e2b
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 8u)
                                / 8)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 8)
                                | this.bitvector1)));
                }
            }

            public uint d1
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 16u)
                                / 16)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 16)
                                | this.bitvector1)));
                }
            }

            public uint d2
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 32u)
                                / 32)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 32)
                                | this.bitvector1)));
                }
            }

            public uint d3
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 64u)
                                / 64)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 64)
                                | this.bitvector1)));
                }
            }

            public uint d4
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 128u)
                                / 128)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 128)
                                | this.bitvector1)));
                }
            }

            public uint d5
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 256u)
                                / 256)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 256)
                                | this.bitvector1)));
                }
            }

            public uint d6
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 512u)
                                / 512)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 512)
                                | this.bitvector1)));
                }
            }

            public uint d7
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 1024u)
                                / 1024)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 1024)
                                | this.bitvector1)));
                }
            }

            public uint d8
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 2048u)
                                / 2048)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 2048)
                                | this.bitvector1)));
                }
            }

            public uint d9
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 4096u)
                                / 4096)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 4096)
                                | this.bitvector1)));
                }
            }

            public uint d10
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 8192u)
                                / 8192)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 8192)
                                | this.bitvector1)));
                }
            }

            public uint d11
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 16384u)
                                / 16384)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 16384)
                                | this.bitvector1)));
                }
            }

            public uint d12
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 32768u)
                                / 32768)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 32768)
                                | this.bitvector1)));
                }
            }

            public uint e3a
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 65536u)
                                / 65536)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 65536)
                                | this.bitvector1)));
                }
            }

            public uint e3b
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 131072u)
                                / 131072)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 131072)
                                | this.bitvector1)));
                }
            }

            public uint e4a
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 262144u)
                                / 262144)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 262144)
                                | this.bitvector1)));
                }
            }

            public uint e4b
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 524288u)
                                / 524288)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 524288)
                                | this.bitvector1)));
                }
            }

            public uint d13
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 1048576u)
                                / 1048576)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 1048576)
                                | this.bitvector1)));
                }
            }

            public uint d14
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 2097152u)
                                / 2097152)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 2097152)
                                | this.bitvector1)));
                }
            }

            public uint d15
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 4194304u)
                                / 4194304)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 4194304)
                                | this.bitvector1)));
                }
            }

            public uint d16
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 8388608u)
                                / 8388608)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 8388608)
                                | this.bitvector1)));
                }
            }

            public uint d17
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 16777216u)
                                / 16777216)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 16777216)
                                | this.bitvector1)));
                }
            }

            public uint d18
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 33554432u)
                                / 33554432)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 33554432)
                                | this.bitvector1)));
                }
            }

            public uint d19
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 67108864u)
                                / 67108864)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 67108864)
                                | this.bitvector1)));
                }
            }

            public uint d20
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 134217728u)
                                / 134217728)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 134217728)
                                | this.bitvector1)));
                }
            }

            public uint d21
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 268435456u)
                                / 268435456)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 268435456)
                                | this.bitvector1)));
                }
            }

            public uint d22
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 536870912u)
                                / 536870912)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 536870912)
                                | this.bitvector1)));
                }
            }

            public uint d23
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 1073741824u)
                                / 1073741824)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 1073741824)
                                | this.bitvector1)));
                }
            }

            public uint d24
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 2147483648u)
                                / 2147483648)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 2147483648)
                                | this.bitvector1)));
                }
            }
        }

        /// Return Type: smart_info*
        [DllImportAttribute("smartapi.dll", EntryPoint = "SmartScan")]
        public static extern IntPtr SmartScan();


        /// Return Type: wchar_t*
        ///device: smart_dev->smart_device*
        [DllImportAttribute("smartapi.dll", EntryPoint = "SmartName")]
        public static extern IntPtr SmartName(System.IntPtr device);


        /// Return Type: int
        ///device: smart_dev->smart_device*
        [DllImportAttribute("smartapi.dll", EntryPoint = "SmartOpen")]
        public static extern int SmartOpen(System.IntPtr device);


        /// Return Type: void
        ///device: smart_dev->smart_device*
        [DllImportAttribute("smartapi.dll", EntryPoint = "SmartClose")]
        public static extern void SmartClose(System.IntPtr device);


        /// Return Type: int
        ///device: smart_dev->smart_device*
        ///channel: int
        ///state: int
        [DllImportAttribute("smartapi.dll", EntryPoint = "SmartSetAll")]
        public static extern int SmartSetAll(System.IntPtr device, uint state, uint mask);


        /// Return Type: int
        ///device: smart_dev->smart_device*
        ///channel: int
        ///state: int
        [DllImportAttribute("smartapi.dll", EntryPoint = "SmartSetSingle")]
        public static extern int SmartSetSingle(System.IntPtr device, int channel, int state);


        /// Return Type: int
        ///device: smart_dev->smart_device*
        ///state: smart_input*
        ///timeout: int
        [DllImportAttribute("smartapi.dll", EntryPoint = "SmartGetAll")]
        public static extern int SmartGetAll(System.IntPtr device, ref smart_input state, int timeout);
        #endregion

        private static CImage[] _ledImages = new CImage[32];
        private static CImage _background;
        private static BlinkStyle[] _ledBlinkStyle = new BlinkStyle[32];
        private static uint _ledBlinkStatus = 0u;
        private static uint _ledStatus = 0u;
        private static uint _ledOldStatus = 0u;
        private static smart_input _smartInput = new smart_input();
        private static OBJScene _scene;

        private enum BlinkStyle
        {
            None,
            Slow,
            Fast,
            VeryFast
        }

        private enum TransitionProgram
        {
            None,
            WakeUp,
            ReverseWakeUp,
            Chase,
            Curtain
        }

        public enum FETransition
        {
            None,
            ListCommands,
            MenuCommands,
            RomCommands,
            Screensaver,
            AboutBox,
            RomChange
        }

        private static int ATTACH_TICK_RETRY = 5000;
        //private static int SLOW_TICK_DURATION = 300;
        //private static int FAST_TICK_DURATION = 100;
        //private static int VERYFAST_TICK_DURATION = 50;
        //private static int SLOW_TRANSITION_TICK_DURATION = 30;
        //private static int FAST_TRANSITION_TICK_DURATION = 10;

        private static int SLOW_TICK_DURATION = 800;
        private static int FAST_TICK_DURATION = 300;
        private static int VERYFAST_TICK_DURATION = 150;
        private static int SLOW_TRANSITION_TICK_DURATION = 400;
        private static int FAST_TRANSITION_TICK_DURATION = 50;
        private static int VERYFAST_TRANSITION_TICK_DURATION = 20;

        private static uint[] wakeUpFrames = new uint[17];
        private static uint[] reverseWakeUpFrames = new uint[17];
        private static uint[] noneFrames = new uint[1];
        private static uint[] chaseFrames = new uint[26];
        private static uint[] curtainFrames = new uint[7];
        private static uint[] _curTransitionFrames;
        private static TransitionProgram _curTransition;
        private static bool _repeatTransition = false;
        private static int _transitionFrameDuration = FAST_TRANSITION_TICK_DURATION;

        private static IntPtr _smartAsd;
        private static string _smartAsdName;
        private static bool _debugMode;
        private static SmartAsdMode _wiringMode;

        private static FETransition _curFETransition;

        /* LED LAYOUT
        //                       28  29  30  31
        //
        //          03 04 05 06                  17 18 19 20
        //                        11                           25
        // 13  02   07 08 09 10             16   21 22 23 24       27
        //                        12                           26
        //          00  01                       14  15
        */

        /*
        HYBRID MODE
        ===========
        PLAYER 1
        d6 = start
        d7 = joy
        d8 = button 1
        d9 = button 2
        d10 = button 3
        d11 = button 4

        PLAYER 2
        d18 = start
        d19 = joy
        d20 = button 1
        d21 = button 2
        d22 = button 3
        d23 = button 4

        DEDICATED MODE
        ==============
        PLAYER 1
        e1a = start
        e1b = coindoor
        e2a = joy
        e2b..d7 = button 1..8
        d8 = spinner
        d9 = trackball

        PLAYER 2
        e3a = start
        e3b = coindoor
        e4a = joy
        e4b..d19 = button 1..8
        d20 = spinner
        d21 = trackball
        */

        public static void Initialize(OBJScene scene, string name, bool debugMode, SmartAsdMode wiringMode)
        {
            _smartAsd = IntPtr.Zero;
            _scene = scene;
            _smartAsdName = name;
            _wiringMode = wiringMode;
            _debugMode = debugMode;

            var xOffset = scene.screenResX - 380;
            var yOffset = scene.screenResY - 90;

            var dedicatedMode = _wiringMode == SmartAsdMode.Dedicated || _wiringMode == SmartAsdMode.Dedicated6Buttons ? 1 : 0;

            _background = _scene.CreateImage(OBJScene.ImageType.Led, null, 400, 130, xOffset - 20, yOffset - 40, string.Empty, false, true, true, true);
            _background.StartTransition(CDrawable.Transition.Freeze);

            var ledIndex = 0;
            for (var iPlayers = 0; iPlayers < 2; iPlayers++)
            {
                // start [0]
                _ledImages[ledIndex] = _scene.CreateImage(OBJScene.ImageType.Led, "*start", 20, 20, xOffset + 50 + iPlayers * 180, yOffset + 60, string.Empty, false, true, false, true);
                _ledImages[ledIndex].StartTransition(CDrawable.Transition.Freeze);
                ledIndex++;
                // coindoor [1]
                _ledImages[ledIndex] = _scene.CreateImage(OBJScene.ImageType.Led, "*coindoor", 30 * dedicatedMode, 30 * dedicatedMode, xOffset + 90 + iPlayers * 180, yOffset + 55, string.Empty, false, true, false, true);
                _ledImages[ledIndex].StartTransition(CDrawable.Transition.Freeze);
                ledIndex++;
                // joy [2]
                _ledImages[ledIndex] = _scene.CreateImage(OBJScene.ImageType.Led, "*joy", 30, 30, xOffset + 15 + iPlayers * 180, yOffset + 15, string.Empty, false, true, false, true);
                _ledImages[ledIndex].StartTransition(CDrawable.Transition.Freeze);
                ledIndex++;
                // buttons 1..4 [3-6]
                for (var iLoop = 0; iLoop < 4; iLoop++)
                {
                    var size = 20;
                    if (iLoop == 3 && _wiringMode == SmartAsdMode.Dedicated6Buttons)
                        size = 0;
                    _ledImages[ledIndex] = _scene.CreateImage(OBJScene.ImageType.Led, "*button", size, size, xOffset + 50 + iLoop * 20 + iPlayers * 180, yOffset + 10, string.Empty, false, true, false, true);
                    _ledImages[ledIndex].StartTransition(CDrawable.Transition.Freeze);
                    ledIndex++;
                }
                // buttons 5..8 [7-10]
                for (var iLoop = 0; iLoop < 4; iLoop++)
                {
                    var size = 20;
                    if (iLoop == 3 && _wiringMode == SmartAsdMode.Dedicated6Buttons)
                        size = 0;
                    _ledImages[ledIndex] = _scene.CreateImage(OBJScene.ImageType.Led, "*button", size * dedicatedMode, size * dedicatedMode, xOffset + 50 + iLoop * 20 + iPlayers * 180, yOffset + 30, string.Empty, false, true, false, true);
                    _ledImages[ledIndex].StartTransition(CDrawable.Transition.Freeze);
                    ledIndex++;
                }
                // spinner [11]
                _ledImages[ledIndex] = _scene.CreateImage(OBJScene.ImageType.Led, "*spinner", 30 * dedicatedMode, 30 * dedicatedMode, xOffset + 135 + iPlayers * 180, yOffset + 0, string.Empty, false, true, false, true);
                _ledImages[ledIndex].StartTransition(CDrawable.Transition.Freeze);
                ledIndex++;
                // trackball [12]
                _ledImages[ledIndex] = _scene.CreateImage(OBJScene.ImageType.Led, "*trackball", 40 * dedicatedMode, 40 * dedicatedMode, xOffset + 130 + iPlayers * 180, yOffset + 30, string.Empty, false, true, false, true);
                _ledImages[ledIndex].StartTransition(CDrawable.Transition.Freeze);
                ledIndex++;
                // side button [13]
                _ledImages[ledIndex] = _scene.CreateImage(OBJScene.ImageType.Led, "*button", 20 * dedicatedMode, 20 * dedicatedMode, xOffset - 10 + iPlayers * 360, yOffset + 20, string.Empty, false, true, false, true);
                _ledImages[ledIndex].StartTransition(CDrawable.Transition.Freeze);
                ledIndex++;
            }
            // extra buttons 1..4 [28-31]
            for (var iLoop = 0; iLoop < 4; iLoop++)
            {
                _ledImages[ledIndex] = _scene.CreateImage(OBJScene.ImageType.Led, "*button", 20 * dedicatedMode, 20 * dedicatedMode, xOffset + 120 + iLoop * 30, yOffset - 30, string.Empty, false, true, false, true);
                _ledImages[ledIndex].StartTransition(CDrawable.Transition.Freeze);
                ledIndex++;
            }

            // initialize "none" transition frames
            noneFrames[0] = 0xffffffff;

            // initialize "wake-up" and "reverse-wake-up" transition frames
            for (var frame = 0; frame < 14; frame++)
            {
                for (var led = 0; led < 14; led++)
                {
                    wakeUpFrames[frame] |= led <= frame ? 1u << led : 0u;
                    wakeUpFrames[frame] |= led <= frame ? 1u << led + 14 : 0u;
                }
                reverseWakeUpFrames[frame] = ReverseWord(wakeUpFrames[frame] & 0xFFFF);
                reverseWakeUpFrames[frame] |= ReverseWord(wakeUpFrames[frame] >> 16) << 16;
            }
            for (var frame = 14; frame < 17; frame++)
            {
                for (var led = 0; led < 32; led++)
                {
                    wakeUpFrames[frame] |= led <= frame + 14 ? 1u << led : 0u;
                }
                reverseWakeUpFrames[frame] = ReverseWord(wakeUpFrames[frame] & 0xFFFF);
                reverseWakeUpFrames[frame] |= ReverseWord(wakeUpFrames[frame] >> 16) << 16;
            }

            // initialize "chase" transition frames
            for (var frame = 0; frame < 14; frame++)
            {
                for (var led = 0; led < 14; led++)
                {
                    chaseFrames[frame] |= led == frame ? 1u << led - 1: 0u;
                    //chaseFrames[frame] |= led == frame ? 1u << led + 14 : 0u;
                    chaseFrames[frame] |= led == 14 - frame ? 1u << led + 13 : 0u;
                }
            }
            for (var frame = 14; frame < 26; frame++)
            {
                for (var led = 0; led < 14; led++)
                {
                    chaseFrames[frame] |= led == 14 - (frame - 14) ? 1u << led - 1 : 0u;
                    //chaseFrames[frame] |= led == 14 - (frame - 14) ? 1u << led + 14: 0u;
                    chaseFrames[frame] |= led == frame - 14 ? 1u << led + 13 : 0u;
                }
            }

            // initialize "curtain" transition frames
            if (_wiringMode == SmartAsdMode.Dedicated6Buttons)
                curtainFrames = new uint[] {
                    0xFEEFFBBF, 0xFEEFFBBF,
                    0xFEEFFBBF, 0xFEEFFBBF,
                    0x9EEEE3BF, 0x9EEEE3BF,
                    0x0ECCA19D, 0x0ECCA19D,
                    0x0E88208C, 0x0E88208C,
                    0x0E002004, 0x0E002004,
                    0x08002000, 0x08002000,
                    0x00000000, 0x00000000,
                    0x00000000, 0x00000000,
                    0x08002000, 0x08002000,
                    0x0E002004, 0x0E002004,
                    0x0E88208C, 0x0E88208C,
                    0x0ECCA19D, 0x0ECCA19D,
                    0x9EEEE3BF, 0x9EEEE3BF,
                    0xFEEFFBBF, 0xFEEFFBBF,
                    0xFEEFFBBF, 0xFEEFFBBF,
                };
            // dungeonmaster's version
            //curtainFrames = new uint[] {
            //    0x00000000, 0x00000000,
            //    0x00000000, 0x00000000,
            //    0x00884089, 0x00884089,
            //    0x90440110, 0x90440110,
            //    0x60220220, 0x60220220, 
            //    0x08012004, 0x08012004,
            //    0x60220220, 0x60220220,
            //    0x90440110, 0x90440110,
            //    0x00884089, 0x00884089
            //};
            else
            {
                curtainFrames = new uint[] {
                    0x00000000, 0x00000000,
                    0x00000000, 0x00000000,
                    0x08002000, 0x08002000,
                    0x0E002004, 0x0E002004,
                    0x0F10208C, 0x0F10208C,
                    0x0F98A19D, 0x0F98A19D,
                    0x0FDCE3BF, 0x0FDCE3BF,
                    0x9FFEE7FF, 0x9FFEE7FF,
                    0xFFFFFFFF, 0xFFFFFFFF,
                    0xFFFFFFFF, 0xFFFFFFFF
                };

                curtainFrames = new uint[] {
                    0xFFFFFFFF, 0xFFFFFFFF,
                    0xFFFFFFFF, 0xFFFFFFFF,
                    0x9FFFFFFF, 0x9FFFFFFF,
                    0x0FFFFFFF, 0x0FFFFFFF,
                    0x0FFEE7FF, 0x0FFEE7FF,
                    0x0FDCE3BF, 0x0FDCE3BF,
                    0x0F98A19D, 0x0F98A19D,
                    0x0F10208C, 0x0F10208C,
                    0x0E002004, 0x0E002004,
                    0x08002000, 0x08002000,
                    0x00000000, 0x00000000,
                    0x00000000, 0x00000000,
                    0x08002000, 0x08002000,
                    0x0E002004, 0x0E002004,
                    0x0F10208C, 0x0F10208C,
                    0x0F98A19D, 0x0F98A19D,
                    0x0FDCE3BF, 0x0FDCE3BF,
                    0x9FFEE7FF, 0x9FFEE7FF,
                    0xFFFFFFFF, 0xFFFFFFFF
                };
            }

            _curFETransition = FETransition.None;
            _curTransitionFrames = noneFrames;
            ResetLed(false);
        }

        private static uint ReverseWord(uint word)
        {
            uint result = 0;

            for (var i = 0; i < 16; i++)
            {
                result |= (word & 1u << (15 - i)) > 0 ? 1u << i: 0;
            }
            return result;
        }

        private static IntPtr AttachSmartASD()
        {
            try
            {
                var smartInfo = new smart_info();
                var smartScanResult = SmartScan();
                while (smartScanResult != IntPtr.Zero)
                {
                    smartInfo = (smart_info)Marshal.PtrToStructure(smartScanResult, smartInfo.GetType());
                    var smartAsdName = Marshal.PtrToStringAuto(SmartName(smartInfo.device));
                    if (smartAsdName == _smartAsdName || _smartAsdName == "*")
                    {
                        _smartAsd = smartInfo.device;
                        SmartOpen(_smartAsd);
                        break;
                    }
                    smartScanResult = smartInfo.next;
                }
            }
            catch
            {
            }
            return _smartAsd;
        }

        public static void Shutdown()
        {
            if (_smartAsd != IntPtr.Zero)
            {
                // shutdown LEDs
                SmartSetAll(_smartAsd, 0x0, 0xffffffff);
                Close();
            }
        }

        public static void Close()
        {
            if (_smartAsd != IntPtr.Zero)
            {
                SmartClose(_smartAsd);
                _smartAsd = IntPtr.Zero;
            }
        }

        private static void SetLed(int ledNo, bool active)
        {
            SetLed(ledNo, active, BlinkStyle.None);
        }

        private static void SetLed(int ledNo, bool active, BlinkStyle blink)
        {
            var mask = 1u << ledNo;
            if (active)
                _ledStatus |= mask;
            else
                _ledStatus &= ~mask;
            _ledBlinkStatus |= mask;
            _ledBlinkStyle[ledNo] = blink;
        }

        private static string curRomCommands = string.Empty;
        public static void SetFECommandLeds(string controls, FETransition newTransition)
        {
            if (_curFETransition != newTransition)
            {
                SetLed(0, true, controls.Contains("start1") ? BlinkStyle.Fast : BlinkStyle.None);  // Start 1
                SetLed(1, true, controls.Contains("coin1") ? BlinkStyle.Fast : BlinkStyle.None);   // Coindoor
                SetLed(2, true, controls.Contains("joy1") ? BlinkStyle.Fast : BlinkStyle.None);    // Joy 1
                SetLed(3, true, controls.Contains("bt1-1") ? BlinkStyle.Fast : BlinkStyle.None);   // Bt1
                SetLed(4, true, controls.Contains("bt1-2") ? BlinkStyle.Fast : BlinkStyle.None);   // Bt2
                SetLed(5, true, controls.Contains("bt1-3") ? BlinkStyle.Fast : BlinkStyle.None);   // Bt3
                SetLed(6, true, controls.Contains("bt1-4") ? BlinkStyle.Fast : BlinkStyle.None);   // Bt4
                SetLed(7, true, controls.Contains("bt1-5") ? BlinkStyle.Fast : BlinkStyle.None);   // Bt5
                SetLed(8, true, controls.Contains("bt1-6") ? BlinkStyle.Fast : BlinkStyle.None);   // Bt6
                SetLed(9, true, controls.Contains("bt1-7") ? BlinkStyle.Fast : BlinkStyle.None);   // Bt7
                SetLed(10, true, controls.Contains("bt1-8") ? BlinkStyle.Fast : BlinkStyle.None);  // Bt8
                SetLed(11, true, controls.Contains("dial1") ? BlinkStyle.Fast : BlinkStyle.None);  // Spinner
                SetLed(12, true, controls.Contains("track1") ? BlinkStyle.Fast : BlinkStyle.None); // Trackball
                SetLed(13, true, controls.Contains("side1") ? BlinkStyle.Fast : BlinkStyle.None);  // Side button 1
                SetLed(14, true, controls.Contains("start2") ? BlinkStyle.Fast : BlinkStyle.None); // Start 2
                SetLed(15, true, controls.Contains("coin2") ? BlinkStyle.Fast : BlinkStyle.None);  // Coindoor
                SetLed(16, true, controls.Contains("joy2") ? BlinkStyle.Fast : BlinkStyle.None);   // Joy 2
                SetLed(17, true, controls.Contains("bt2-1") ? BlinkStyle.Fast : BlinkStyle.None);  // Bt1
                SetLed(18, true, controls.Contains("bt2-2") ? BlinkStyle.Fast : BlinkStyle.None);  // Bt2
                SetLed(19, true, controls.Contains("bt2-3") ? BlinkStyle.Fast : BlinkStyle.None);  // Bt3
                SetLed(20, true, controls.Contains("bt2-4") ? BlinkStyle.Fast : BlinkStyle.None);  // Bt4
                SetLed(21, true, controls.Contains("bt2-5") ? BlinkStyle.Fast : BlinkStyle.None);  // Bt5
                SetLed(22, true, controls.Contains("bt2-6") ? BlinkStyle.Fast : BlinkStyle.None);  // Bt6
                SetLed(23, true, controls.Contains("bt2-7") ? BlinkStyle.Fast : BlinkStyle.None);  // Bt7
                SetLed(24, true, controls.Contains("bt2-8") ? BlinkStyle.Fast : BlinkStyle.None);  // Bt8
                SetLed(25, true, controls.Contains("dial2") ? BlinkStyle.Fast : BlinkStyle.None);  // Spinner
                SetLed(26, true, controls.Contains("track2") ? BlinkStyle.Fast : BlinkStyle.None); // Trackball
                SetLed(27, true, controls.Contains("side2") ? BlinkStyle.Fast : BlinkStyle.None);  // Side button 2
                SetLed(28, true, controls.Contains("extra1") ? BlinkStyle.Fast : BlinkStyle.None); // Extra button 1
                SetLed(29, true, controls.Contains("extra2") ? BlinkStyle.Fast : BlinkStyle.None); // Extra button 2
                SetLed(30, true, controls.Contains("extra3") ? BlinkStyle.Fast : BlinkStyle.None); // Extra button 3
                SetLed(31, true, controls.Contains("extra4") ? BlinkStyle.Fast : BlinkStyle.None); // Extra button 4

                curRomCommands = string.Empty;
                StartTransition(newTransition == FETransition.ListCommands ? TransitionProgram.WakeUp : TransitionProgram.ReverseWakeUp, 
                    false, VERYFAST_TRANSITION_TICK_DURATION);
                _curFETransition = newTransition;
            }
        }

        public static void SetInputControlLeds(string inputControl)
        {
            SetInputControlLeds(inputControl, false, false);
        }

        public static void SetInputControlLeds(string inputControl, bool reset, bool immediate)
        {
            curRomCommands = reset ? string.Empty : curRomCommands;
            if (_curFETransition != FETransition.RomCommands || curRomCommands != inputControl)
            {
                _curFETransition = FETransition.RomCommands;
                curRomCommands = inputControl;

                var players = 0;
                var buttons = 0;
                var usesJoy = false;
                var usesDoubleJoy = false;
                var usesSpinner = false;
                var usesTrackball = false;

                foreach (var element in inputControl.Split('-'))
                {
                    var value = element.Trim();

                    var txtPos = value.IndexOf('P');
                    if (txtPos > -1)
                        players = int.Parse(value.Substring(0, txtPos));

                    usesJoy |= value.IndexOf("joy") > -1 || value.IndexOf("stick") > -1;
                    usesDoubleJoy |= value.IndexOf("doublejoy") > -1;
                    usesSpinner |= value.IndexOf("dial") > -1 || value.IndexOf("paddle") > -1;
                    usesTrackball |= value.IndexOf("trackball") > -1;

                    txtPos = value.IndexOf("Bt");
                    if (txtPos > -1)
                        buttons = int.Parse(value.Substring(0, txtPos));
                }

                ResetLed(false);
                if (players > 2) players = 2;
                var blink = immediate ? BlinkStyle.None : BlinkStyle.Fast;
                for (var player = 0; player < players; player++)
                {
                    var offset = 14 * player;
                    if (usesJoy) SetLed(offset + 2, usesJoy, blink);
                    if (usesSpinner) SetLed(offset + 11, usesSpinner, blink);
                    if (usesTrackball) SetLed(offset + 12, usesTrackball, blink);
                    SetLed(offset, true, blink); // Start
                    SetLed(offset + 1, true, blink); // Coindoor
                    // buttons
                    for (var iLed = 3; iLed < 11; iLed++)
                    {
                        if (buttons > iLed - 3) SetLed(iLed + offset, buttons > iLed - 3, blink);
                    }
                }
                // doublejoy
                if (usesDoubleJoy) SetLed(16, true, blink);

                if(!immediate)
                    StartTransition(TransitionProgram.ReverseWakeUp, false, FAST_TRANSITION_TICK_DURATION);
            }
        }

        public static void StartAboutBoxProgram()
        {
            if (_curFETransition != FETransition.AboutBox)
            {
                _curFETransition = FETransition.AboutBox;
                ResetLed(true);
                StartTransition(TransitionProgram.Chase, true, SLOW_TRANSITION_TICK_DURATION);
            }
        }

        public static void StartRomChangeProgram()
        {
            if (_curFETransition != FETransition.RomChange)
            {
                _curFETransition = FETransition.RomChange;
                ResetLed(true);
                StartTransition(TransitionProgram.Curtain, false, VERYFAST_TRANSITION_TICK_DURATION);
            }
        }

        private static int attachTick = 0;
        private static int slowTick = 0;
        private static int fastTick = 0;
        private static int veryFastTick = 0;
        private static int transitionTick = 0;
        private static int transitionCurrentFrame = 0;

        private static void StartTransition(TransitionProgram transition, bool repeat, int frameDuration)
        {
            if (transition != _curTransition || !repeat)
            {
                _curTransition = transition;
                if (transition == TransitionProgram.WakeUp)
                    _curTransitionFrames = wakeUpFrames;
                else if (transition == TransitionProgram.ReverseWakeUp)
                    _curTransitionFrames = reverseWakeUpFrames;
                else if (transition == TransitionProgram.Chase)
                    _curTransitionFrames = chaseFrames;
                else if (transition == TransitionProgram.Curtain)
                    _curTransitionFrames = curtainFrames;
                else if (transition == TransitionProgram.None)
                    _curTransitionFrames = noneFrames;

                transitionCurrentFrame = 0;
                _repeatTransition = repeat;
            }
            _transitionFrameDuration = frameDuration;
        }

        public static void Update(int tickCount)
        {
            // update timers
            var slowChangeTick = false;
            var fastChangeTick = false;
            var veryFastChangeTick = false;

            if (_curTransition != TransitionProgram.None)
            {
                if (tickCount > transitionTick)
                {
                    if (transitionCurrentFrame < _curTransitionFrames.Length - 1)
                        transitionCurrentFrame++;
                    else if (_repeatTransition)
                        transitionCurrentFrame = 0;
                    else
                    {
                        _curTransition = TransitionProgram.None;
                        if (_curFETransition == FETransition.RomChange)
                            _curFETransition = FETransition.None;
                        _curTransitionFrames = noneFrames;
                        transitionCurrentFrame = 0;
                    }
                    transitionTick = tickCount + _transitionFrameDuration;

                    // reset blink
                    slowTick = tickCount + SLOW_TICK_DURATION;
                    fastTick = tickCount + FAST_TICK_DURATION;
                    veryFastTick = tickCount + VERYFAST_TICK_DURATION;
                }
            }
            else
            {
                if (tickCount > slowTick)
                {
                    slowTick = tickCount + SLOW_TICK_DURATION;
                    slowChangeTick = true;
                }
                if (tickCount > fastTick)
                {
                    fastTick = tickCount + FAST_TICK_DURATION;
                    fastChangeTick = true;
                }
                if (tickCount > veryFastTick)
                {
                    veryFastTick = tickCount + VERYFAST_TICK_DURATION;
                    veryFastChangeTick = true;
                }
            }

            _smartInput.bitvector1 = 0;

            if (_debugMode)
                _background.SetAlpha(.6f);

            // loop LEDs
            for (var ledNo = 0; ledNo < _ledImages.Length; ledNo++)
            {
                if ((_ledBlinkStyle[ledNo] == BlinkStyle.Slow && slowChangeTick) ||
                    (_ledBlinkStyle[ledNo] == BlinkStyle.Fast && fastChangeTick) ||
                    (_ledBlinkStyle[ledNo] == BlinkStyle.VeryFast && veryFastChangeTick))
                {
                    _ledBlinkStatus = _ledBlinkStatus ^ 1u << ledNo;
                }

                // set LED status
                var ledStatus = false;
                // apply blink when no transition running
                if (_curTransition == TransitionProgram.None)
                    ledStatus = (_ledStatus & _ledBlinkStatus & 1u << ledNo) > 0;
                else
                    ledStatus = (_ledStatus & _curTransitionFrames[transitionCurrentFrame] & 1u << ledNo) > 0;

                // update debug dashboard
                if (_debugMode)
                    _ledImages[ledNo].SetAlpha(ledStatus ? .6f : .2f);

                // update LED status
                if (_wiringMode == SmartAsdMode.Hybrid)
                {
                    // smartASD wiring type A (hybrid)
                    switch (ledNo)
                    {
                        // PLAYER 1
                        case 0: _smartInput.d6 = ledStatus ? 1u : 0u; break; // start
                        case 2: _smartInput.d7 = ledStatus ? 1u : 0u; break; // joy
                        case 3: _smartInput.d8 = ledStatus ? 1u : 0u; break; // button 1
                        case 4: _smartInput.d9 = ledStatus ? 1u : 0u; break; // button 2
                        case 5: _smartInput.d10 = ledStatus ? 1u : 0u; break;// button 3
                        case 6: _smartInput.d11 = ledStatus ? 1u : 0u; break;// button 4
                        // PLAYER 2
                        case 14: _smartInput.d18 = ledStatus ? 1u : 0u; break; // start
                        case 16: _smartInput.d19 = ledStatus ? 1u : 0u; break; // joy
                        case 17: _smartInput.d20 = ledStatus ? 1u : 0u; break; // button 1
                        case 18: _smartInput.d21 = ledStatus ? 1u : 0u; break; // button 2
                        case 19: _smartInput.d22 = ledStatus ? 1u : 0u; break; // button 3
                        case 20: _smartInput.d23 = ledStatus ? 1u : 0u; break; // button 4
                    }
                }
                else if (_wiringMode == SmartAsdMode.Dedicated)
                {
                    // smartASD wiring type B (dedicated)
                    if (ledNo < 14)
                        _smartInput.bitvector1 |= ledStatus ? 1u << ledNo : 0u; // PLAYER 1
                    else if (ledNo < 28)
                        _smartInput.bitvector1 |= ledStatus ? 1u << ledNo + 2 : 0u; // PLAYER 2
                    else if (ledNo < 30)
                        _smartInput.bitvector1 |= ledStatus ? 1u << ledNo - 14 : 0u; // EXTRA 1-2
                    else
                        _smartInput.bitvector1 |= ledStatus ? 1u << ledNo : 0u; // EXTRA 3-4
                }
            }
            
            // attach SmartASD
            if (_smartAsd == IntPtr.Zero && tickCount > attachTick)
            {
                attachTick = tickCount + ATTACH_TICK_RETRY;
                AttachSmartASD();
            }

            // send LED changes
            if (_smartInput.bitvector1 != _ledOldStatus && _smartAsd != IntPtr.Zero)
            {
                _ledOldStatus = _smartInput.bitvector1;
                if (SmartSetAll(_smartAsd, _smartInput.bitvector1, 0xffffffff) != 0)
                {
                    _smartAsd = IntPtr.Zero;
                }
            }
        }

        #region Private Members
        private static void ResetLed(bool active)
        {
            ResetLed(active, 0, _ledImages.Length - 1);
        }

        private static void ResetLed(bool active, int startLed, int endLed)
        {
            for (var ledNo = startLed; ledNo <= endLed; ledNo++)
            {
                var mask = 1u << ledNo;
                if (active)
                    _ledStatus |= mask;
                else
                    _ledStatus &= ~mask;

                _ledBlinkStatus |= 1u << ledNo;
                _ledBlinkStyle[ledNo] = BlinkStyle.None;
            }
        }

        private static void SetBlink(int ledNo, BlinkStyle blink)
        {
            _ledBlinkStyle[ledNo] = blink;
        }
        #endregion
    }
}
