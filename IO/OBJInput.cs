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
using Microsoft.Xna.Framework.Input;
using SharpDX.DirectInput;
using System.Runtime.InteropServices;

namespace feel
{
    class OBJInput
    {
        private IntPtr handle;
        private int screenResX;
        private int screenResY;
        private Joystick joypad = null;
        private DirectInputDPad dPad;
        private InputState[] keyState;
        private int mouseXCenter = 0;
        private int mouseYCenter = 0;
        private bool mouseMoveUp = false;
        private bool mouseMoveDown = false;
        private bool mouseMoveLeft = false;
        private bool mouseMoveRight = false;
        private bool kNoEvent = true;
        private bool mNoEvent = true;
        private bool mouseMoving = false;
        private bool jNoEvent = true;
        int mouse_sensitivity;
        private bool joypadPresent = false;
        private bool joypadLost = false;
        private InputState[] joyState;
        private int joyStartCount;
        private int joyDeadZone;

        // DirectX version
        //private float mouseSpeed = 1.3f;

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        public class DirectInputDPad
        {
            public InputState Up = InputState.None;
            public InputState Right = InputState.None;
            public InputState Down = InputState.None;
            public InputState Left = InputState.None;

            public void Update(int direction)
            {
                Up = Right = Down = Left = InputState.None;

                if (direction == -1)
                    return;

                if (direction > 27000 || direction < 9000)
                {
                    Up = InputState.KeyDown;
                }

                if (0 < direction && direction < 18000)
                {
                    Right = InputState.KeyDown;
                }

                if (9000 < direction && direction < 27000)
                {
                    Down = InputState.KeyDown;
                }

                if (18000 < direction)
                {
                    Left = InputState.KeyDown;
                }
            }
        }

        public OBJInput(IntPtr hwnd, int resX, int resY)
        {
            handle = hwnd;
            screenResX = resX;
            screenResY = resY;
        }

        public bool NoEvent { get { return (kNoEvent && mNoEvent && jNoEvent); } }

        public bool KeyboardNoEvent { get { return (kNoEvent); } }

        public bool MouseNoEvent { get { return (mNoEvent); } }

        public bool JoypadNoEvent { get { return (jNoEvent); } }

        // DirectX version
        //public void CreateKeyboard(bool testMode)
        //{
        //    keyState = new InputState[240];
        //    keyboard = new Device(SystemGuid.Keyboard);
        //    keyboard.SetCooperativeLevel(handle, CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
        //    keyboard.SetDataFormat(DeviceDataFormat.Keyboard);
        //    keyboard.Acquire();
        //    joyState = new InputState[19];
        //    for (var i = 0; i < 19; i++)
        //        joyState[i] = InputState.None;
        //    joyStartCount = (int)FeelKey.JoyUp;
        //}

        //public void UpdateKeyboard()
        //{
        //    keyboard.Poll();
        //    kNoEvent = true;
        //    var keyboardState = keyboard.GetCurrentKeyboardState();
        //    for (var i = 0; i < 238; i++)
        //    {
        //        if (keyboardState[(Key)i])
        //        {
        //            keyState[i] = InputState.KeyDown;
        //            kNoEvent = false;
        //        }
        //        else
        //        {
        //            if (keyState[i] == InputState.KeyDown)
        //            {
        //                keyState[i] = InputState.KeyPress;
        //                kNoEvent = false;
        //            }
        //            else
        //                keyState[i] = InputState.None;
        //        }
        //    }
        //}

        //public InputState KeyState(FeelKey key)
        //{
        //    var k = (int)key;
        //    if (k >= 240)
        //        return JoyState(key);
        //    return keyState[k];
        //}

        public void CreateKeyboard(bool testMode)
        {
            keyState = new InputState[255];
            joyState = new InputState[19];
            for (var i = 0; i < 19; i++)
                joyState[i] = InputState.None;
            joyStartCount = (int)FeelKey.JoyUp;
        }

        public void UpdateKeyboard()
        {
            kNoEvent = true;
            Microsoft.Xna.Framework.Input.KeyboardState keyboardState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            for (var i = 1; i <= (int)Microsoft.Xna.Framework.Input.Keys.VolumeUp; i++)
            {
                if (keyboardState.IsKeyDown((Keys)i))
                {
                    keyState[i] = InputState.KeyDown;
                    kNoEvent = false;
                }
                else
                {
                    if (keyState[i] == InputState.KeyDown)
                    {
                        keyState[i] = InputState.KeyPress;
                        kNoEvent = false;
                    }
                    else
                        keyState[i] = InputState.None;
                }
            }
        }

        public InputState KeyState(FeelKey key)
        {
            var k = (int)key;
            if (k >= 240)
                return JoyState(key);
            return keyState[k];
        }

        // DirectX version
        //public void CreateMouse(int sensitivity)
        //{
        //    mouse_sensitivity = sensitivity;
        //    SetCursorPos(screenResX / 2, screenResY / 2);
        //    mouse = new Device(SystemGuid.Mouse);
        //    mouse.SetCooperativeLevel(handle, CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
        //    mouse.SetDataFormat(DeviceDataFormat.Mouse);
        //    mouse.Acquire();
        //}

        //public void UpdateMouse()
        //{
        //    mouse.Poll();
        //    mNoEvent = true;
        //    var mouseState = mouse.CurrentMouseState;
        //    mouseMoveDown = false;
        //    mouseMoveUp = false;
        //    mouseMoveLeft = false;
        //    mouseMoveRight = false;
        //    if (mouseState.X > mouse_sensitivity) { mouseMoveRight = true; mNoEvent = false; }
        //    if (mouseState.X < -mouse_sensitivity) { mouseMoveLeft = true; mNoEvent = false; }
        //    if (mouseState.Y > mouse_sensitivity) { mouseMoveDown = true; mNoEvent = false; }
        //    if (mouseState.Y < -mouse_sensitivity) { mouseMoveUp = true; mNoEvent = false; }
        //    mouseMoving = (mouseState.X != 0 || mouseState.Y != 0);
        //    var mouseVX = mouseState.X * mouseSpeed;
        //    var mouseVY = mouseState.Y * mouseSpeed;
        //    mouseX = mouseX + (int)mouseVX;
        //    mouseY = mouseY + (int)mouseVY;
        //    if (mouseX < 0)
        //        mouseX = 0;
        //    if (mouseY < 0)
        //        mouseY = 0;
        //    if (mouseX >= screenResX)
        //        mouseX = screenResX - 1;
        //    if (mouseY >= screenResY)
        //        mouseY = screenResY - 1;
        //}

        //public void SetMouseSpeed(float speed)
        //{
        //    if (speed > 0)
        //        mouseSpeed = speed;
        //    else
        //        speed = 0.0f;
        //}

        public void CreateMouse(int sensitivity)
        {
            mouseXCenter = screenResX / 2;
            mouseYCenter = screenResY / 2;
            mouse_sensitivity = sensitivity;
            Microsoft.Xna.Framework.Input.Mouse.SetPosition(mouseXCenter, mouseYCenter);
        }

        public void UpdateMouse()
        {
            mNoEvent = true;
            mouseMoveDown = mouseMoveUp = mouseMoveLeft = mouseMoveRight = false;
            var mouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();
            if (mouseState.X > mouseXCenter + mouse_sensitivity)
            {
                Microsoft.Xna.Framework.Input.Mouse.SetPosition(mouseXCenter, mouseYCenter);
                mouseMoveRight = mouseMoving = true;
                mNoEvent = false;
            }
            else if (mouseState.X < mouseXCenter - mouse_sensitivity)
            {
                Microsoft.Xna.Framework.Input.Mouse.SetPosition(mouseXCenter, mouseYCenter);
                mouseMoveLeft = mouseMoving = true;
                mNoEvent = false;
            }
            if (mouseState.Y > mouseYCenter + mouse_sensitivity)
            {
                Microsoft.Xna.Framework.Input.Mouse.SetPosition(mouseXCenter, mouseYCenter);
                mouseMoveDown = mouseMoving = true;
                mNoEvent = false;
            }
            else if (mouseState.Y < mouseYCenter - mouse_sensitivity)
            {
                Microsoft.Xna.Framework.Input.Mouse.SetPosition(mouseXCenter, mouseYCenter);
                mouseMoveUp = mouseMoving = true;
                mNoEvent = false;
            }
        }

        public bool MouseMoveUp { get { return mouseMoveUp; } }
        public bool MouseMoveDown { get { return mouseMoveDown; } }
        public bool MouseMoveLeft { get { return mouseMoveLeft; } }
        public bool MouseMoveRight { get { return mouseMoveRight; } }
        public bool MouseMoving { get { return mouseMoving; } }

        public void CreateJoyPad(int deadZone)
        {
            joypadPresent = false;
            joyDeadZone = deadZone;

            // Initialize DirectInput
            var directInput = new DirectInput();
            dPad = new DirectInputDPad();

            // if refreshing an old reference, first unacquire joypad
            if (joypad != null)
                joypad.Unacquire();

            // Find a Joystick Guid
            var joystickGuid = Guid.Empty;

            foreach (var deviceInstance in directInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly))
            {
                joystickGuid = deviceInstance.InstanceGuid;
                joypadPresent = true;
                // stop at first
                break;
            }

            // If Joystick not found, throws an error
            if (joystickGuid == Guid.Empty)
            {
                throw new Exception("No joystick found: aborting");
            }

            // Instantiate the joystick
            joypad = new Joystick(directInput, joystickGuid);

            // Acquire the joystick
            joypad.Acquire();
        }

        public void UpdateJoyPad()
        {
            jNoEvent = true;
            if (joypadPresent)
            {
                JoystickState joypadState;

                // joy is not there anymore: try to reacquire it
                if (joypadLost)
                {
                    CreateJoyPad(joyDeadZone);
                }

                try
                {
                    joypad.Poll();
                    joypadState = joypad.GetCurrentState();
                    joypadLost = false;
                }
                catch
                {
                    // joypad lost: skip and wait next time to reacquire it
                    joypadLost = true;
                    return;
                }

                // get Joy POV state
                dPad.Update(joypadState.PointOfViewControllers[0]);

                // JoyUp
                CheckJoyPadState(0, (joypadState.Y - 32767 < -joyDeadZone) || dPad.Up == InputState.KeyDown);
                // JoyDown
                CheckJoyPadState(1, (joypadState.Y - 32767 > joyDeadZone) || dPad.Down == InputState.KeyDown);
                // JoyLeft
                CheckJoyPadState(2, (joypadState.X - 32767 < -joyDeadZone) || dPad.Left == InputState.KeyDown);
                // JoyRight
                CheckJoyPadState(3, (joypadState.X - 32767 > joyDeadZone) || dPad.Right == InputState.KeyDown);

                // JoyButtons
                var b = joypadState.Buttons;
                for (var i = 0; i < 15; i++)
                {
                    if (i < b.Length)
                        CheckJoyPadState(i + 4, (b[i]));
                    else
                        CheckJoyPadState(i + 4, false);
                }
            }
        }

        public InputState JoyState(FeelKey key)
        {
            if (!joypadPresent)
                return InputState.None;
            switch (key)
            {
                case FeelKey.JoyUp:
                case FeelKey.JoyDown:
                case FeelKey.JoyLeft:
                case FeelKey.JoyRight:
                case FeelKey.JoyB1:
                case FeelKey.JoyB2:
                case FeelKey.JoyB3:
                case FeelKey.JoyB4:
                case FeelKey.JoyB5:
                case FeelKey.JoyB6:
                case FeelKey.JoyB7:
                case FeelKey.JoyB8:
                case FeelKey.JoyB9:
                case FeelKey.JoyB10:
                case FeelKey.JoyB11:
                case FeelKey.JoyB12:
                case FeelKey.JoyB13:
                case FeelKey.JoyB14:
                case FeelKey.JoyB15:
                    var i = (int)key - joyStartCount;
                    return joyState[i];
                default:
                    return InputState.None;
            }
        }

        private void CheckJoyPadState(int index, bool expression)
        {
            if (expression)
            {
                joyState[index] = InputState.KeyDown;
                jNoEvent = false;
            }
            else
            {
                if (joyState[index] == InputState.KeyDown)
                {
                    joyState[index] = InputState.KeyPress;
                    jNoEvent = false;
                }
                else
                    joyState[index] = InputState.None;
            }
        }

        public void Release()
        {
            if (joypad != null)
                joypad.Unacquire();
            joypadLost = true;
        }

        public void Dispose()
        {
            //if (joypad != null)
            //    joypad.Dispose();
        }
    }
}
