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
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Color = Microsoft.Xna.Framework.Graphics.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace feel
{
    class OBJScene
    {
        private Feel feel;
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private RenderTarget2D renderTarget;
        private Texture2D _lastFrame;
        private float _transitionStep = 0f;
        private Rectangle _screenRectangle;
        private SpriteFont testTextFont;
        private Vector2 testTextPosition;

        public int screenResX;
        public int screenResY;
        private CSoundForm topmostForm;

        private List<CLabel> labels = new List<CLabel>();
        private List<CSound> sounds = new List<CSound>();
        private CMenuManager menu;
        private CToast messageToast;
        private CLabel fnetMessage;
        private List<KeyValuePair<ImageType, CImage>> images = new List<KeyValuePair<ImageType, CImage>>();
        private List<KeyValuePair<ImageType, CImage>> onTopImages = new List<KeyValuePair<ImageType, CImage>>();
        public CStatusAnimatedImage backgroundImage;
        public CStatusAnimatedImage bezelImage;
        public CStatusAnimatedImage actorsImage;
        public CImage snapshotImage;
        public List<CImage> ledPanelImage = new List<CImage>();
        public CImage[] customImages = new CImage[2];
        public CVideo videosnapVideo;

        private int currentCustomImage = -1;

        private CImage screenSaverImage;
        private int screenSaverX;
        private int screenSaverY;
        private Size screenSaverSize;
        private bool screenSaverChange = false;
        private string nextScreenSaverFileName;
        private string nextScreenSaverTitle;
        private bool nextScreenSaverSnapshotStretch;
        private bool nextScreenSaverLeftScrolling;
        private Texture2D screenSaverLeftArrow;
        private CLabel screenSaverTitle;
        private Color screenSaverFontColor;
        private Color screenSaverBackcolor;

        private List<string> backgroundMusicPlayList = new List<string>();
        private CSound backgroundSound;
        public CMusic backgroundMusic;
        private string backgroundMusicPath;
        private int backgroundMusicVolume;
        private int currentBackgroundMusic;

        private float ratioX;
        private float ratioY;

        private bool drawRotated;

        private Effect effectPost;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int W, int H, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetActiveWindow(IntPtr hWnd);

        public IntPtr Handle { get { return topmostForm.Handle; } }

        public enum ImageType { Snapshot, Cabinet, Marquee, Background, Led }

        public OBJScene(Feel game, int resX, int resY, bool rotateScreen)
        {
            // Create Sound Form
            topmostForm = new CSoundForm();
            topmostForm.IsPlayEnded = false;

            // Create Devices
            feel = game;
            graphics = ((Feel)feel).GraphicsDM;
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            effectPost = feel.Content.Load<Effect>("Shader");
            testTextFont = feel.Content.Load<SpriteFont>("arial10");
            testTextPosition = new Vector2(5, 1);

            // set resolution ratio
            ChangeRes(resX, resY, rotateScreen, true);
		}

        ~OBJScene()
        {
            Dispose();
        }

        public void ResetImages()
        {
            images.Clear();
            onTopImages.Clear();
            CImage[] customImages = new CImage[2];
            backgroundImage = bezelImage = actorsImage = null;
        }

        bool _resolutionChanged = false;
        public void ChangeRes(int resX, int resY, bool rotateScreen, bool applyChanges)
        {
            // initialize rotation
            CDrawable.Init(feel, resX, resY, rotateScreen);

            if ((screenResX == resX && screenResY == resY && drawRotated == rotateScreen) ||
                (screenResY == resX && screenResX == resY && drawRotated != rotateScreen))
            {
            }
            else
            {
                _resolutionChanged = true;
                if (applyChanges)
                {
                    if (rotateScreen)
                    {
                        graphics.PreferredBackBufferHeight = resX;
                        graphics.PreferredBackBufferWidth = resY;
                    }
                    else
                    {
                        graphics.PreferredBackBufferWidth = resX;
                        graphics.PreferredBackBufferHeight = resY;
                    }
                    graphics.ApplyChanges();
                }
            }

            drawRotated = rotateScreen;
            screenResX = resX;
            screenResY = resY;
            ratioX = (float)screenResX / 640;
            ratioY = (float)screenResY / 480;
            _screenRectangle = new Rectangle(0, 0, drawRotated ? screenResY : screenResX, drawRotated ? screenResX : screenResY);
        }

        public void SetWindowPos(IntPtr handle, bool topMost)
        {
            if (topMost)
                SetActiveWindow(handle);
        }
        //public void SetWindowPos(IntPtr handle, bool topMost)
        //{
        //    const int HWND_TOPMOST = -1;
        //    const int HWND_NOTOPMOST = -2;
        //    const int SWP_SHOWWINDOW = 0x0040;
        //    const int SWP_HIDEWINDOW = 0x0080;
        //    const int SWP_NOREDRAW = 0x0008;

        //    if (topMost)
        //        SetWindowPos(handle, new IntPtr(HWND_TOPMOST), 0, 0, screenResX, screenResY, SWP_SHOWWINDOW);
        //    else
        //        SetWindowPos(handle, new IntPtr(HWND_NOTOPMOST), 0, 0, screenResX, screenResY, SWP_NOREDRAW);
        //}

        public void  CreateScreenSaver(int width, int height, Color titleFontColor, Color backColor, string fontName, int fontSize, FontStyle fontStyle)
        {
            screenSaverSize = Utils.ScaleDimension(width, height, screenResX - (int)(100 * ratioX), screenResY - (int)(150 * ratioY));
            screenSaverX = (screenResX - screenSaverSize.Width) / 2 - 1;
            screenSaverY = (((screenResY - (int)(150 * ratioY)) - screenSaverSize.Height) / 2) + (int)(50 * ratioY) - 1;
            screenSaverFontColor = titleFontColor;
            screenSaverBackcolor = backColor;
            screenSaverTitle = new CLabel(0, screenResY - (int)(100 * ratioY), screenResX, (int)(50 * ratioY), "",
                                          fontName, fontSize, fontStyle, screenSaverFontColor, screenSaverBackcolor,
                                          TextAlign.Center, false);
            CreateArrow();
        }

        private void CreateArrow()
        {
            screenSaverLeftArrow = feel.Content.Load<Texture2D>("leftarrow");
        }

        public CStatusAnimatedImage CreateBackgroundImage(string filename, int width, int height, int frame_duration_ms, int repeat_delay_ms)
        {
            backgroundImage = new CStatusAnimatedImage(filename, width, height, "", false, true, frame_duration_ms, repeat_delay_ms);
            return backgroundImage;
        }

        public CStatusAnimatedImage CreateActorsImage(string filename, int width, int height, int frame_duration_ms, int repeat_delay_ms)
        {
            actorsImage = new CStatusAnimatedImage(filename, width, height, "", false, true, frame_duration_ms, repeat_delay_ms);
            return actorsImage;
        }

        public CStatusAnimatedImage CreateBezelImage(string filename, int width, int height, int frame_duration_ms, int repeat_delay_ms)
        {
            bezelImage = new CStatusAnimatedImage(filename, width, height, "", false, true, frame_duration_ms, repeat_delay_ms);
            return bezelImage;
        }

        public CImage CreateImage(ImageType type, string filename, int width, int height, int posX, int posY, string fileNotFoundImage, bool stretch, bool isVisible)
        {
            return CreateImage(type, filename, width, height, posX, posY, fileNotFoundImage, stretch, isVisible, false, false);
        }

        public CImage CreateImage(ImageType type, string filename, int width, int height, int posX, int posY, string fileNotFoundImage, bool stretch, bool isVisible, bool blackBackground, bool onTop)
        {
            var image = new CImage(filename, width, height, fileNotFoundImage, stretch, isVisible, blackBackground);
            image.X = posX;
            image.Y = posY;

            if (!onTop)
                images.Add(new KeyValuePair<ImageType, CImage>(type, image));
            else
                onTopImages.Add(new KeyValuePair<ImageType, CImage>(type, image));

            if (type == ImageType.Snapshot)
                snapshotImage = image;
            return image;
        }

        public CImage GetImage(ImageType type)
        {
            return images.Find(c => c.Key == type).Value;
        }

        public CLabel CreateStarLabel(string keyname, int posX, int posY, int width, int height, string text,
                          int fontSize, FontStyle fontStyle,
                          Color fontColor, Color backColor, bool isVisible)
        {
            var newItem = new CLabel(posX, posY, width, height, text,
                                     "Wingdings", 9, FontStyle.Regular, fontColor, backColor, TextAlign.Center, true);
            newItem.KeyName = keyname;
            newItem.Visible = isVisible;
            labels.Add(newItem);
            return newItem;
        }

        public CTicker CreateTicker(string keyname, int posX, int posY, int width, int height, string fontName, int fontSize,
            FontStyle fontStyle, Color fontColor, Color backColor, bool isVisible, bool isFocusable)
        {
            var newItem = new CTicker(posX, posY, width, height, fontName, fontSize, fontStyle, fontColor, backColor, isFocusable);
            newItem.KeyName = keyname;
            newItem.Visible = isVisible;
            if (keyname != "FnetMessage")
                labels.Add(newItem);
            else
                fnetMessage = newItem;

            return newItem;
        }

        public CLabel CreateLabel(string keyname, int posX, int posY, int width, int height, string text,
		                          string fontName, int fontSize, FontStyle fontStyle, Color fontColor, Color backColor,
                                  TextAlign textAlign, bool isVisible, bool isFocusable)
        {
            var newItem = new CLabel(posX, posY, width, height, text,
                                     fontName, fontSize, fontStyle, fontColor, backColor, textAlign, isFocusable);
            newItem.KeyName = keyname;
            newItem.Visible = isVisible;
            if (keyname != "FnetMessage")
                labels.Add(newItem);
            else
                fnetMessage = newItem;

            return newItem;
        }

        public CToast CreateToast(string keyname, int posX, int posY, int width, int height, string text,
                                  string fontName, int fontSize, FontStyle fontStyle, Color fontColor, Color backColor,
                                  TextAlign textAlign)
        {
            messageToast = new CToast(ref graphics, ref feel, posX, posY, width, height, text,
                                     fontName, fontSize, fontStyle, fontColor, backColor, textAlign, false);
            messageToast.KeyName = keyname;
            return messageToast;
        }

        public CMenuManager CreateMenu()
        {
            menu = new CMenuManager(ref graphics, ref feel);
            return menu;
        }

        public CLabel GetLabel(string keyName)
        {
            return labels.Find(c => c.KeyName == keyName);
        }

        public CSound CreateSound(string keyname, string filename, int volume)
        {
            var newItem = new CSound(
                Application.StartupPath + Path.DirectorySeparatorChar + "media" + Path.DirectorySeparatorChar + filename, volume, topmostForm);
            newItem.KeyName = keyname;
            sounds.Add(newItem);
            return newItem;
        }

        public CSound GetSound(string keyName)
        {
            return sounds.Find(c => c.KeyName == keyName);
        }

        public CVideo CreateVideo(string fileName, int width, int height, int posX, int posY, int videoVolume, float speed)
        {
            if (videosnapVideo != null)
                videosnapVideo.Dispose();

            videosnapVideo = new CVideo(ref graphics, ref feel);
            videosnapVideo.PlayLooped = true;
            videosnapVideo.PlayVideo(feel, fileName, fileName, width, height, posX, posY, videoVolume, speed, () => { });
            return videosnapVideo;
        }

        public bool IsVideoPlaying { get { return !videosnapVideo.IsPlaying; } }

        private bool _multipleFilesMusic = false;
        public bool MultipleFilesMusic { get { return _multipleFilesMusic; } }
        private double _changeTrackDelay = 0d;
        public void SetBackgroundMusic(string musicPath, int volume, double changeTrackDelay)
        {
            if (backgroundMusicPath == musicPath)
            {
                if (backgroundMusicVolume != volume)
                {
                    backgroundMusicVolume = volume;
                    if (backgroundMusic != null)
                        backgroundMusic.Volume = backgroundMusicVolume;
                    else if (backgroundSound != null)
                        backgroundSound.Volume = backgroundMusicVolume;
                }
                return;
            }

            _multipleFilesMusic = false;
            backgroundMusicPlayList.Clear();
            if (backgroundSound != null)
            {
                backgroundSound.Dispose();
                backgroundSound = null;
            }
            if (backgroundMusic != null)
            {
                backgroundMusic.Dispose();
                backgroundMusic = null;
            }
            topmostForm.IsPlayEnded = true;
            currentBackgroundMusic = -1;
            backgroundMusicVolume = volume;
            backgroundMusicPath = musicPath;
            _changeTrackDelay = changeTrackDelay;

            // it's a directory..
            if (Directory.Exists(backgroundMusicPath))
            {
                var sourceList = new List<string>();
                sourceList.AddRange(Directory.GetFiles(backgroundMusicPath, "*.wav"));
                sourceList.AddRange(Directory.GetFiles(backgroundMusicPath, "*.mp3"));

                // shuffle list
                var rnd = new Random();
                var itemsCount = sourceList.Count;

                _multipleFilesMusic = itemsCount > 1;
                while (itemsCount > 0)
                {
                    var index = rnd.Next(0, itemsCount);
                    backgroundMusicPlayList.Add(sourceList[index]);
                    sourceList.RemoveAt(index);
                    itemsCount--;
                }

                backgroundMusic = new CMusic();
                return;
            }

            var fileName = backgroundMusicPath;
            if (!File.Exists(fileName))
                fileName = Application.StartupPath  + Path.DirectorySeparatorChar + "media" + Path.DirectorySeparatorChar  + backgroundMusicPath;

            // it's a file
            if (File.Exists(fileName))
            {
                var ext = Path.GetExtension(fileName).Trim().ToLower();
                if (ext == ".wav")
                {
                    backgroundSound = new CSound(fileName, volume, topmostForm);
                }
                else
                {
                    backgroundMusicPlayList.Add(fileName);
                    backgroundMusic = new CMusic();
                }
            }
        }

        private double _changeTrackDelayCounter = 0;
        public void PlayBackgroundMusic(GameTime gameTime)
        {
            if (topmostForm.IsPlayEnded)
            {
                _changeTrackDelayCounter -= gameTime.ElapsedGameTime.TotalMilliseconds;
                if (_changeTrackDelayCounter <= 0)
                {
                    // PlayList
                    if (backgroundMusic != null && backgroundMusicPlayList.Count > 0)
                    {
                        currentBackgroundMusic++;
                        if (currentBackgroundMusic >= backgroundMusicPlayList.Count)
                            currentBackgroundMusic = 0;
                        backgroundMusic.SetMusic(backgroundMusicPlayList[currentBackgroundMusic], backgroundMusicVolume);
                        backgroundMusic.Play(topmostForm.Handle);
                    }
                    else if (backgroundSound != null)
                    {
                        // Single File
                        backgroundSound.PlayLooping();
                    }
                    topmostForm.IsPlayEnded = false;
                }
            }
            else
                _changeTrackDelayCounter = _changeTrackDelay * 1000d;
        }

        public void NextTrackBackgroundMusic()
        {
            topmostForm.IsPlayEnded = true;
        }

        public void PauseBackgroundMusic()
        {
            // PlayList
            if (backgroundMusic != null)
                backgroundMusic.Pause();
            // Single File
            if (backgroundSound != null)
                backgroundSound.Pause();
        }

        public void ResumeBackgroundMusic()
        {
            // PlayList
            if (backgroundMusic != null)
                backgroundMusic.Resume();
            // Single File
            if (backgroundSound != null)
                backgroundSound.Resume();
        }

        public bool IsChangingScreenSaver { get { return screenSaverChange; } }

        private CDrawable.Transition _pendingTransition = CDrawable.Transition.None;

        public void StartScreenTransition(CDrawable.Transition pendingTransition)
        {
            _pendingTransition = pendingTransition;
        }

        public void Render(GameTime gameTime, Feel.MachineState machineState)
        {
            if (_pendingTransition != CDrawable.Transition.None || _resolutionChanged)
            {
                if (_pendingTransition != CDrawable.Transition.None && renderTarget != null)
                {
                    // save last frame
                    _lastFrame = renderTarget.GetTexture();
                    _transitionStep = 0f;

                    effectPost.CurrentTechnique = effectPost.Techniques[
                        _pendingTransition == CDrawable.Transition.Slide ? "PostProcessSlide" : "PostProcessFade"];
                }

                // refresh render target
                PresentationParameters pp = graphics.GraphicsDevice.PresentationParameters;
                renderTarget = new RenderTarget2D(graphics.GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, 1, graphics.GraphicsDevice.DisplayMode.Format);
                _resolutionChanged = false;
            }

            graphics.GraphicsDevice.SetRenderTarget(0, renderTarget);
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
            if (machineState.State == Feel.MachineState.StateEnum.ScreenSaver)
            {
                graphics.GraphicsDevice.Clear(screenSaverBackcolor);

                if (screenSaverChange)
                {
                    if (screenSaverImage != null)
                    {
                        if (nextScreenSaverLeftScrolling)
                        {
                            if (screenSaverImage.X > screenSaverX)
                            {
                                screenSaverImage.X = screenSaverImage.X - (screenResX / 10);
                                if (screenSaverImage.X <= screenSaverX)
                                {
                                    screenSaverImage.X = screenSaverX;
                                    screenSaverChange = false;
                                }
                            }
                            else
                            {
                                screenSaverImage.X = screenSaverImage.X - (screenResX / 10);
                                if (screenSaverImage.X < -screenSaverSize.Width)
                                    screenSaverImage = null;
                            }
                        }
                        else
                        {
                            if (screenSaverImage.X < screenSaverX)
                            {
                                screenSaverImage.X = screenSaverImage.X + (screenResX / 10);
                                if (screenSaverImage.X >= screenSaverX)
                                {
                                    screenSaverImage.X = screenSaverX;
                                    screenSaverChange = false;
                                }
                            }
                            else
                            {
                                screenSaverImage.X = screenSaverImage.X + (screenResX / 10);
                                if (screenSaverImage.X > screenResX)
                                    screenSaverImage = null;
                            }
                        }
                    }
                    else
                    {
                        screenSaverImage = new CImage(nextScreenSaverFileName, screenSaverSize.Width, screenSaverSize.Height, snapshotImage.FileNotFoundName, nextScreenSaverSnapshotStretch, true);

                        if (nextScreenSaverLeftScrolling)
                            screenSaverImage.X = screenResX;
                        else
                            screenSaverImage.X = -screenSaverImage.Width;
                        screenSaverImage.Y = screenSaverY;
                        screenSaverTitle.Text = nextScreenSaverTitle;
                    }
                }

                if (screenSaverImage != null)
                {
                    var posX = screenSaverImage.X < 0 ? 0 : screenSaverImage.X;
                    var posY = screenSaverImage.Y < 0 ? 0 : screenSaverImage.Y;
                    var srcRect = screenSaverImage.GetRect(screenResX, screenResY);
                    if (srcRect != null)
                        screenSaverImage.Draw(gameTime, spriteBatch, _pendingTransition);
                    screenSaverTitle.Draw(gameTime, spriteBatch, _pendingTransition);
                }

                // screensaver written directly on SB!
                var position = new Vector2(screenResX - (int)(50 * ratioX), (screenResY - (int)(60 * ratioY)) / 2);
                var rotatedPosition = drawRotated ? new Vector2(screenResY - position.Y, position.X) : position;
                spriteBatch.Draw(screenSaverLeftArrow, rotatedPosition, null, new Color(Color.White, screenSaverChange && nextScreenSaverLeftScrolling ? .6f : .2f), drawRotated ? MathHelper.PiOver2 : 0f, Vector2.Zero, ratioX, SpriteEffects.FlipHorizontally, 0f);

                position = new Vector2((int)(20 * ratioX), (screenResY - (int)(60 * ratioY)) / 2);
                rotatedPosition = drawRotated ? new Vector2(screenResY - position.Y, position.X) : position;
                spriteBatch.Draw(screenSaverLeftArrow, rotatedPosition, null, new Color(Color.White, screenSaverChange && !nextScreenSaverLeftScrolling ? .6f : .2f), drawRotated ? MathHelper.PiOver2 : 0f, Vector2.Zero, ratioX, SpriteEffects.None, 0f);

                // OnTop Images
                foreach (var image in onTopImages)
                {
                    image.Value.Draw(gameTime, spriteBatch, _pendingTransition);
                }
            }
            else
            {
                graphics.GraphicsDevice.Clear(Color.Black);

                // Background Image
                if (backgroundImage != null)
                    backgroundImage.Draw(gameTime, spriteBatch, _pendingTransition);

                // Video Image
                Texture2D texture = videosnapVideo.GetTexture();
                if (videosnapVideo.IsPlaying && texture != null)
                {
                    snapshotImage.SetTexture(texture);
                }

                // Images
                foreach (var image in images)
                {
                    image.Value.Draw(gameTime, spriteBatch, _pendingTransition);
                }

                // Labels
                foreach (var label in labels)
                {
                    label.Draw(gameTime, spriteBatch, _pendingTransition);
                }

                // Actors Image
                if (actorsImage != null)
                    actorsImage.Draw(gameTime, spriteBatch, _pendingTransition);

                // Bezel Image
                if (bezelImage != null)
                    bezelImage.Draw(gameTime, spriteBatch, _pendingTransition);

                // OnTop Images
                foreach (var image in onTopImages)
                {
                    image.Value.Draw(gameTime, spriteBatch, _pendingTransition);
                }

                // FNET
                fnetMessage.Draw(gameTime, spriteBatch, _pendingTransition);

                // Menu
                menu.Draw(gameTime, spriteBatch, _pendingTransition);

                // Mapping Image
                if (machineState.State == Feel.MachineState.StateEnum.CustomImage && currentCustomImage >= 0)
                {
                    customImages[currentCustomImage].Draw(gameTime, spriteBatch, _pendingTransition);
                }

                // Message Labels
                menu.DrawMessage(gameTime, spriteBatch, _pendingTransition);

                // Toast
                messageToast.Draw(gameTime, spriteBatch, _pendingTransition);

				PlayBackgroundMusic(gameTime);
            }

            if (machineState.testMode)
            {
                // test mode written directly on SB!
                spriteBatch.DrawString(
                    testTextFont, "F.E.E.L. " + Application.ProductVersion + " (Test Mode) - FPS: " + machineState.fps + "\n" + machineState.ToString(),
                                       new Vector2(testTextPosition.X, testTextPosition.Y), Color.GhostWhite);
            }
            spriteBatch.End();

            // Switch back to drawing onto the back buffer
            graphics.GraphicsDevice.SetRenderTarget(0, null);

            // Draw to backbuffer
            graphics.GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            // Apply the post process shader
            effectPost.Parameters["fFadeAmount"].SetValue(_transitionStep);
            effectPost.Parameters["ColorMap2"].SetValue(_lastFrame);
            effectPost.Parameters["fSmoothSize"].SetValue(.3f);
            effectPost.CommitChanges();
            effectPost.Begin();
            {
                effectPost.CurrentTechnique.Passes[0].Begin();
                {
                    spriteBatch.Draw(renderTarget.GetTexture(), _screenRectangle, Color.White);
                    effectPost.CurrentTechnique.Passes[0].End();
                }
            }
            effectPost.End();
            spriteBatch.End();

            // Update transition step
            if (_transitionStep < 1f)
                _transitionStep += .1f;
            else
            {
                _lastFrame = null;
                _transitionStep = 1f;
            }

            _pendingTransition = CDrawable.Transition.None;
        }

        public void Dispose()
        {
            labels.Clear();
            sounds.Clear();
            for (var i = 0; i < 2; i++)
                customImages[i] = null;
            if (backgroundSound != null)
            {
                backgroundSound.Dispose();
                backgroundSound = null;
            }
            if (backgroundMusic != null)
            {
                backgroundMusic.Dispose();
                backgroundMusic = null;
            }
            if (videosnapVideo != null)
            {
                videosnapVideo.Dispose();
                videosnapVideo = null;
            }
            backgroundImage = null;
            screenSaverImage = null;
            images.Clear();
        }

        public void SetScreenSaverImage(string fileName, string title, bool snapshot_stretch, bool leftScrolling)
        {
            nextScreenSaverFileName = fileName;
            nextScreenSaverTitle = title;

            nextScreenSaverSnapshotStretch = snapshot_stretch;
            nextScreenSaverLeftScrolling = leftScrolling;
            screenSaverChange = true;
        }

        public void SetCustomImage(string fileName)
        {
            if (customImages[0] == null)
                customImages[0] = new CImage(fileName, screenResX, screenResY, "", false, true);
            else
                if (File.Exists(fileName))
                {
                    customImages[0].LoadImage(fileName);
                }
        }

        public void ResetCustomImage()
        {
            customImages[0] = null;
        }

        public bool SetNextCustomImage()
        {
            customImages[0].StartTransition(CDrawable.Transition.FadeIn);
            currentCustomImage--;
            if (currentCustomImage < 0)
                return false;
            return true;
        }

        public void SetLastCustomImage()
        {
            customImages[0].StartTransition(CDrawable.Transition.FadeIn);
            currentCustomImage = 0;
            if (customImages[1] == null)
                currentCustomImage = 0;
        }

        public void RefreshObjects(string defaultImageFile, Color saverFontColor, Color saverBackColor)
        {
            if (screenSaverImage != null)
                screenSaverImage.FileNotFoundName = defaultImageFile;
            labels.RemoveAll(c => c.KeyName.StartsWith("RomListLabel_"));
            labels.RemoveAll(c => c.KeyName.StartsWith("RomListStarLabel_"));
            screenSaverFontColor = saverFontColor;
            screenSaverBackcolor = saverBackColor;
            screenSaverTitle.ForeColor = screenSaverFontColor;
            screenSaverTitle.BackColor = screenSaverBackcolor;
        }
    }
}
