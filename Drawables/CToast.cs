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
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace feel
{
    class CToast: CLabel
    {
        private int _currentFrame = 0;
        private enum TransitionChangeFrames
        {
            StartFadeIn = 0,
            StartFadeOut = 50,
            Reset = 100 // warn: tuned on fade-out duration!
        }

        public CToast(ref GraphicsDeviceManager graphicsDM, ref Feel feel, int xPos, int yPos, int width, int height, string text,
                      string fontName, int fontSize, System.Drawing.FontStyle fontStyle, Color fontColor, Color backColor, TextAlign textAlign, bool isFocusable):
            base(xPos, yPos, width, height, text, fontName, fontSize, fontStyle, fontColor, backColor, textAlign, isFocusable)
        {
            Trimmed = true;
            ShowMessage(text);
        }

        public override void RefreshObject(int xPos, int yPos, int width, int height, string text,
                                  string fontName,
                                  int fontSize,
                                  System.Drawing.FontStyle fontStyle,
                                  Color fontColor, Color backColor, TextAlign textAlign, bool visible, bool isFocusable)
        {
            // don't overwrite text (toast message survives to layout change!)
            base.RefreshObject(xPos, yPos, width, height, Text,
                               fontName, fontSize, fontStyle, fontColor, backColor, textAlign, visible, isFocusable);
            ShowMessage(Text);
        }

        public void ShowMessage(string message)
        {
            if (!Frozen)
                _currentFrame = 0;
            Text = message;
            Visible = (message != string.Empty);
        }

        public bool Frozen { get; set; }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, Transition pendingTransition)
        {
            if (!Visible) return;
            // process transition changes
            switch (_currentFrame)
            {
                case (int)TransitionChangeFrames.StartFadeIn:
                    pendingTransition = Transition.FadeIn;
                    break;
                case (int)TransitionChangeFrames.StartFadeOut:
                    if (!Frozen)
                        pendingTransition = Transition.FadeOut;
                    else
                        _currentFrame--;
                    break;
            }
            if (_currentFrame < (int)TransitionChangeFrames.Reset)
            {
                _currentFrame++;
                StartTransition(pendingTransition);
                base.Draw(gameTime, spriteBatch, pendingTransition);
            }
            else
            {
                ShowMessage(string.Empty);
            }
        }
    }
}
