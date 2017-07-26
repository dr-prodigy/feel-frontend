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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using feel.fnet;
using Microsoft.Xna.Framework;

namespace feel
{
    public sealed class Feel : Game, IDisposable, IFeel
    {

        #region MachineState
        public class MachineState
        {
            private string _toString = string.Empty;
            private StateEnum _state = StateEnum.Normal;
            private string _menu = string.Empty;
            private string _menuKey = string.Empty;

            public enum StateEnum
            {
                Normal,
                NewVersion,
                ScreenSaver,
                CustomImage,
                Restart,
                Shutdown,
                BuildGameList,
                StartEmulator
            }
            public bool testMode;
            public int fps;
            public bool isRunning;
            public StateEnum State { get { return _state; } set { _state = value; UpdateString(); } }
            public string Menu { get { return _menu; } set { _menu = value != null ? value : string.Empty; UpdateString(); } }
            public string MenuKey { get { return _menuKey; } set { _menuKey = value != null ? value : string.Empty; UpdateString(); } }

            private void UpdateString()
            {
                var sb = new StringBuilder("State: ").Append(_state.ToString());
                if (!string.IsNullOrEmpty(_menu))
                {
                    sb.Append("\nMenu: ").Append(_menu).Append("\nMenu key: ");
                    if (!string.IsNullOrEmpty(_menuKey) && _menuKey != "menu_close")
                        sb.Append(_menuKey);
                    else
                        sb.Append("-");
                }
                _toString = sb.ToString();
            }

            public override string ToString()
            {
                return _toString;
            }
        }
        #endregion

        #region Private members
        private OBJScene objScene;
        private OBJInput objInput;
        private CMenuManager objMenu;
        private CListBox romList;
        private RomDesc currentRom;
        private RomDesc previousRom;

        private List<string> platformList = new List<string>();
        private List<string> emulatorList = new List<string>();
        private List<string> gameListList = new List<string>();

        private const int FNET_TASKS_TIME_MS = 30000;

        private int TickCount = 0;
        private int originalScreenResX;
        private int originalScreenResY;
        private double _elapsedTime = 0.0f;
        private int _fpsCounter = 0;
        private int keyboardTickCount = 0;
        private int mouseTickCount = 0;
        private int screenSaverTickCount = 0;
        private int screenSaverResume = 0;
        private int videoTickCount = 0;
        private int idleUpdateTickCount = 0;

        // FNET
        private bool runFnet;
        private int fnetTasksTickCount = 0;
        private DateTime fnetLastUpdate = DateTime.MinValue;

        private bool keyboardScrolling = false;
        private bool mouseScrolling = false;
        private bool _asyncTaskWait = false;
        private bool isFocusGained;
        private bool isVersionCheckDone;
        private MachineState _machineState = new MachineState();

        private bool shortCut = false;
        private string currentMenu = null;
        private string preselectedMenuKey = string.Empty;
        private string searchString = string.Empty;
        private string newGamelistName = string.Empty;
        private AutostartMode autostartMode = AutostartMode.Off;

        private bool romChanged = false;

        private CLabel romCounter;
        private CLabel platformName;
        private CLabel emulatorName;
        private CLabel gameListName;
        private CLabel romName;
        private CLabel romDescription;
        private CLabel romManufacturer;
        private CLabel romDisplayType;
        private CLabel romInputControl;
        private CLabel romStatus;
        private CLabel romCategory;
        private CToast toastMessage;
        private CTicker fnetMessage;

        private CStatusAnimatedImage _backgroundImage;
        private CStatusAnimatedImage _bezelImage;
        private CStatusAnimatedImage _actorsImage;
        private CImage _snapshotImage;
        private CImage _cabinetImage;
        private CImage _marqueeImage;

        private RomDesc screenSaverRom;

        private CSound sfxList;
        private CSound sfxMenu;
        private CSound sfxCancel;
        private CSound sfxConfirm;
        private CSound sfxStartEmu;

        private int current_keyboard_scroll_rate;

        private string currentLayout;

        private OBJConfig objConfig;
        private NetworkHelper networkHelper;
        private ActionQueue actionQueue;

        private CultureInfo cultureInfo = new CultureInfo("en-US", true);

        public GraphicsDeviceManager GraphicsDM;

        private Form feelForm;
        #endregion

        #region Init, Start, Stop & Dispose
        public Feel()
        {
            objConfig = new OBJConfig(Application.StartupPath);
            GraphicsDM = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

		~Feel()
        {
            Dispose();
        }


		public new void Dispose ()
		{
			if (sfxList != null) sfxList.Dispose();
        	if (sfxMenu != null) sfxMenu.Dispose();
        	if (sfxCancel != null) sfxCancel.Dispose();
        	if (sfxConfirm != null) sfxConfirm.Dispose();
            if (sfxStartEmu != null) sfxStartEmu.Dispose();
            if (objScene != null) objScene.Dispose();
            if (objInput != null) objInput.Dispose();

            // reset default cursor
            CCursorManager.RestoreCursors();

            // stop LED Manager
            CLedManager.Shutdown();

            if (networkHelper != null) networkHelper.Dispose();
            if (actionQueue != null) actionQueue.Dispose();

            sfxList = sfxMenu = sfxCancel = sfxConfirm = sfxStartEmu = null;
            objScene = null;
            objInput = null;
            networkHelper = null;
            actionQueue = null;

            base.Dispose();
        }

        public void Main(bool newVersion)
        {
            var error = false;
            var friendlyMsg = "";
            var logMsg = "";

            try
            {
                Utils.CreateLog(cultureInfo);
                _machineState.State = newVersion ? MachineState.StateEnum.NewVersion : MachineState.StateEnum.Normal;
                originalScreenResX = Screen.PrimaryScreen.Bounds.Width;
                originalScreenResY = Screen.PrimaryScreen.Bounds.Height;

                // start FNET
                networkHelper = new NetworkHelper(this);
                actionQueue = new ActionQueue();

                if (objConfig.LoadConfig())
                {
                    // FNET login (async)
                    networkHelper.EnqueueTask(new NetworkTask(NetworkTask.TaskTypeEnum.Login, new string[] { objConfig.feel_uuid }, true));

                    if (objConfig.LoadConfig(Levels.LAYOUT_INI))
                    {
                        FixConfigParameters();
                        // find FEEL form
                        IntPtr hWnd = this.Window.Handle;
                        var control = System.Windows.Forms.Control.FromHandle(hWnd);
                        feelForm = control.FindForm();

                        if (!objConfig.test_mode)
                        {
                            // hide mouse cursors
                            if (objConfig.hide_mouse_pointer)
                                CCursorManager.HideCursors();

                            if (objConfig.disable_alt_f4)
                                feelForm.FormClosing += new FormClosingEventHandler(FormClosing);

                            if (objConfig.switch_res)
                            {
                                // change desktop resolution
                                Utils.ChangeResolution(objConfig.screen_res_x, objConfig.screen_res_y, objConfig.rotate_screen);
                            }
                            // remove window border
                            feelForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                            // maximize window
                            feelForm.WindowState = System.Windows.Forms.FormWindowState.Maximized;
                        }

                        current_keyboard_scroll_rate = objConfig.keyboard_scroll_rate;
                        currentLayout = objConfig.current_layout;

                        // set autostart mode
                        autostartMode = objConfig.autostart_mode;

                        this.Run();
                    }
                }
            }
            catch (Exception ex)
            {
                error = true;
                friendlyMsg = "EXECUTION ERROR\r\n" + ex.Message;
                logMsg = friendlyMsg + "\r\n" + ex.ToString();

                if (ex.InnerException != null)
                    logMsg += "\r\nINNER EXCEPTION:\r\n" + ex.InnerException.ToString();
            }
            finally
            {
                if (!objConfig.test_mode)
                {
                    Utils.ShowHideCursor(true);
                    Utils.ShowHideWindow("Shell_TrayWnd", null, true);
                    if (objConfig.switch_res)
                    {
                        Utils.ChangeResolution(originalScreenResX, originalScreenResY, false);
                    }
                }
                if (error)
                {
                    Utils.PrintLog(logMsg);
                    MessageBox.Show(friendlyMsg, "FEEL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    SaveConfig();
                    if (_machineState.State == MachineState.StateEnum.Shutdown)
                        Process.Start("shutdown.exe", "-s -f -t 00");
                    else if (_machineState.State == MachineState.StateEnum.Restart)
                        Process.Start("shutdown.exe", "-r -f -t 00");
                    else if (objConfig.restore_explorer_at_exit)
                        Utils.RestoreWindowsShell();
                }
                Utils.PrintLog("*******************************************************************");
            }
        }

        private void FormClosing(object sender, FormClosingEventArgs e)
        {
            // disable Alt+F4
            if (_machineState.isRunning)
                e.Cancel = true;
        }

        public void SetWindowPos(bool onTop)
        {
            objScene.SetWindowPos(this.Window.Handle, onTop);
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _machineState.isRunning = true;
            base.Initialize();
            objScene.ChangeRes(objConfig.screen_res_x, objConfig.screen_res_y, objConfig.rotate_screen, true);
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            CreateFrontend();
        }

        public void Stop()
        {
            objMenu.HideMessage(true);
            _machineState.isRunning = false;
        }

        #endregion

        #region Main Loop
        private bool runAutostart = true;
        private bool drawHasRun = false;
        protected override void Update(GameTime gameTime)
        {
            UpdateUI(gameTime);
            base.Update(gameTime);

            // wait first Draw before starting mono-game mode
            if (drawHasRun)
            {
                if (runAutostart)
                {
                    if ((autostartMode == AutostartMode.LastPlayed && !string.IsNullOrEmpty(objConfig.last_game_played))
                        || (autostartMode == AutostartMode.SingleGame && !string.IsNullOrEmpty(objConfig.autostart_single_game)))
                    {
                        GameRunChain gameRunChain;
                        // initialize FEEL on last game played
                        if (autostartMode == AutostartMode.LastPlayed)
                            gameRunChain = new GameRunChain(objConfig.last_game_played);
                        else
                            gameRunChain = new GameRunChain(objConfig.autostart_single_game);
                        SelectPlatform(gameRunChain.Platform);
                        SelectEmulator(gameRunChain.Emulator);
                        SelectGameList(gameRunChain.Gamelist);
                        currentRom = romList.SelectItem(romList.FindIndexByName(gameRunChain.Game, true));

                        // request game start
                        _machineState.State = MachineState.StateEnum.StartEmulator;
                    }
                    else if (autostartMode == AutostartMode.LastSelected && currentRom != null)
                    {
                        // request game start
                        _machineState.State = MachineState.StateEnum.StartEmulator;
                    }
                }
                runAutostart = false;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            _fpsCounter++;

			// render scene
            if (objScene != null)
                objScene.Render(gameTime, _machineState);
            base.Draw(gameTime);

            // first Draw executed: allow game autostart 
            drawHasRun = true;
        }
        #endregion

        #region Graphics
        private void CreateFrontend()
        {

            objScene = new OBJScene(this, objConfig.screen_res_x, objConfig.screen_res_y, objConfig.rotate_screen);
            objMenu = objScene.CreateMenu();
            objMenu.Init(objConfig.screen_res_x, objConfig.screen_res_y, objConfig.rotate_screen, objConfig.menu_width, objConfig.menu_item_height, objConfig.menu_font_name, objConfig.menu_font_size, objConfig.menu_font_style, objConfig.menu_font_color, objConfig.menu_backcolor, objConfig.menu_selected_font_color, objConfig.menu_selected_backcolor);
            objScene.CreateScreenSaver(objConfig.snapshot_width, objConfig.snapshot_height, objConfig.screen_saver_font_color, objConfig.screen_saver_backcolor, objConfig.menu_font_name, objConfig.menu_font_size, objConfig.menu_font_style);

            objInput = new OBJInput(objScene.Handle, objConfig.screen_res_x, objConfig.screen_res_y);

            //objScene.SetWindowPos(true);

            objInput.CreateKeyboard(objConfig.test_mode);
            if (objConfig.use_mouse != UseMouse.No)
                objInput.CreateMouse(objConfig.mouse_sensitivity);
            if (objConfig.use_joypad)
                objInput.CreateJoyPad(objConfig.joypad_dead_zone * 6); // convert 0..5000 to 0..32767 (roughly)

            // Reset Scene Images
            objScene.ResetImages();

            // Main Image
            var filename = Application.StartupPath + Path.DirectorySeparatorChar + "layouts" + Path.DirectorySeparatorChar  + objConfig.current_layout  + Path.DirectorySeparatorChar + "main.png";
            if (File.Exists(filename))
                _backgroundImage = objScene.CreateBackgroundImage(filename, objConfig.background_width, objConfig.background_height, objConfig.background_frame_duration_ms, objConfig.background_repeat_delay_ms);


            // Bezel Image
            filename = Application.StartupPath + Path.DirectorySeparatorChar + "layouts" + Path.DirectorySeparatorChar + objConfig.current_layout + Path.DirectorySeparatorChar + "bezel.png";
            if (File.Exists(filename))
                _bezelImage = objScene.CreateBezelImage(filename, objConfig.background_width, objConfig.background_height, objConfig.bezel_frame_duration_ms, objConfig.bezel_repeat_delay_ms);

            // Actors Image
            filename = Application.StartupPath + Path.DirectorySeparatorChar + "layouts" + Path.DirectorySeparatorChar + objConfig.current_layout + Path.DirectorySeparatorChar + "actors.png";
            if (File.Exists(filename))
                _actorsImage = objScene.CreateActorsImage(filename, objConfig.background_width, objConfig.background_height, objConfig.actors_frame_duration_ms, objConfig.actors_repeat_delay_ms);

            // Snapshot Image
            filename = Application.StartupPath  + Path.DirectorySeparatorChar + "layouts" + Path.DirectorySeparatorChar  + objConfig.current_layout  + Path.DirectorySeparatorChar + "snap.png";
            _snapshotImage = objScene.CreateImage(OBJScene.ImageType.Snapshot, filename, objConfig.snapshot_width, objConfig.snapshot_height, objConfig.snapshot_x_pos, objConfig.snapshot_y_pos, filename, objConfig.snapshot_stretch, true, objConfig.snapshot_blackbackground, false);

            // Cabinet Image
            filename = Application.StartupPath  + Path.DirectorySeparatorChar + "layouts" + Path.DirectorySeparatorChar  + objConfig.current_layout  + Path.DirectorySeparatorChar + "cabinet.png";
            _cabinetImage = objScene.CreateImage(OBJScene.ImageType.Cabinet, filename, objConfig.cabinet_width, objConfig.cabinet_height, objConfig.cabinet_x_pos, objConfig.cabinet_y_pos, filename, objConfig.cabinet_stretch, objConfig.cabinet_visible, objConfig.cabinet_blackbackground, false);

            // Marquee Image
            filename = Application.StartupPath  + Path.DirectorySeparatorChar + "layouts" + Path.DirectorySeparatorChar  + objConfig.current_layout  + Path.DirectorySeparatorChar + "marquee.png";
            _marqueeImage = objScene.CreateImage(OBJScene.ImageType.Marquee, filename, objConfig.marquee_width, objConfig.marquee_height, objConfig.marquee_x_pos, objConfig.marquee_y_pos, filename, objConfig.marquee_stretch, objConfig.marquee_visible, objConfig.marquee_blackbackground, false);

            // LEDs
            CLedManager.Initialize(objScene, objConfig.smartasd_id, objConfig.smartasd_debug_mode, objConfig.smartasd_mode);

            // Feel Labels
            romCounter = objScene.CreateLabel("RomCounter", objConfig.romcounter_x_pos, objConfig.romcounter_y_pos, objConfig.romcounter_width, objConfig.romcounter_height, "",
			                                  objConfig.romcounter_font_name,
                                              objConfig.romcounter_font_size,
                                              objConfig.romcounter_font_style,
			                                  objConfig.romcounter_font_color, objConfig.romcounter_backcolor, objConfig.romcounter_text_align, objConfig.romcounter_visible, true);
            platformName = objScene.CreateLabel("PlatformName", objConfig.platformname_x_pos, objConfig.platformname_y_pos, objConfig.platformname_width, objConfig.platformname_height, "",
			                                  objConfig.platformname_font_name,
                                              objConfig.platformname_font_size,
                                              objConfig.platformname_font_style,
                                              objConfig.platformname_font_color, objConfig.platformname_backcolor, objConfig.platformname_text_align, objConfig.platformname_visible, true);
            emulatorName = objScene.CreateLabel("EmulatorName", objConfig.emulatorname_x_pos, objConfig.emulatorname_y_pos, objConfig.emulatorname_width, objConfig.emulatorname_height, "",
			                                  objConfig.emulatorname_font_name,
                                              objConfig.emulatorname_font_size,
                                              objConfig.emulatorname_font_style,
                                              objConfig.emulatorname_font_color, objConfig.emulatorname_backcolor, objConfig.emulatorname_text_align, objConfig.emulatorname_visible, true);
            gameListName = objScene.CreateLabel("GameListName", objConfig.gamelistname_x_pos, objConfig.gamelistname_y_pos, objConfig.gamelistname_width, objConfig.gamelistname_height, "",
			                                  objConfig.gamelistname_font_name,
                                              objConfig.gamelistname_font_size,
                                              objConfig.gamelistname_font_style,
                                              objConfig.gamelistname_font_color, objConfig.gamelistname_backcolor, objConfig.gamelistname_text_align, objConfig.gamelistname_visible, true);
            romName = objScene.CreateLabel("RomName", objConfig.romname_x_pos, objConfig.romname_y_pos, objConfig.romname_width, objConfig.romname_height, "",
			                                  objConfig.romname_font_name,
                                              objConfig.romname_font_size,
                                              objConfig.romname_font_style,
                                              objConfig.romname_font_color, objConfig.romname_backcolor, objConfig.romname_text_align, objConfig.romname_visible, true);
            romDescription = objScene.CreateLabel("RomDescription", objConfig.romdescription_x_pos, objConfig.romdescription_y_pos, objConfig.romdescription_width, objConfig.romdescription_height, "",
			                                  objConfig.romdescription_font_name,
                                              objConfig.romdescription_font_size,
                                              objConfig.romdescription_font_style,
                                              objConfig.romdescription_font_color, objConfig.romdescription_backcolor, objConfig.romdescription_text_align, objConfig.romdescription_visible, true);
            romManufacturer = objScene.CreateLabel("RomManufacturer", objConfig.rommanufacturer_x_pos, objConfig.rommanufacturer_y_pos, objConfig.rommanufacturer_width, objConfig.rommanufacturer_height, "",
			                                  objConfig.rommanufacturer_font_name,
                                              objConfig.rommanufacturer_font_size,
                                              objConfig.rommanufacturer_font_style,
                                              objConfig.rommanufacturer_font_color, objConfig.rommanufacturer_backcolor, objConfig.rommanufacturer_text_align, objConfig.rommanufacturer_visible, true);
            romDisplayType = objScene.CreateLabel("RomDisplayType", objConfig.romdisplaytype_x_pos, objConfig.romdisplaytype_y_pos, objConfig.romdisplaytype_width, objConfig.romdisplaytype_height, "",
			                                  objConfig.romdisplaytype_font_name,
                                              objConfig.romdisplaytype_font_size,
                                              objConfig.romdisplaytype_font_style,
                                              objConfig.romdisplaytype_font_color, objConfig.romdisplaytype_backcolor, objConfig.romdisplaytype_text_align, objConfig.romdisplaytype_visible, true);
            romInputControl = objScene.CreateLabel("RomInputControl", objConfig.rominputcontrol_x_pos, objConfig.rominputcontrol_y_pos, objConfig.rominputcontrol_width, objConfig.rominputcontrol_height, "",
			                                  objConfig.rominputcontrol_font_name,
                                              objConfig.rominputcontrol_font_size,
                                              objConfig.rominputcontrol_font_style,
                                              objConfig.rominputcontrol_font_color, objConfig.rominputcontrol_backcolor, objConfig.rominputcontrol_text_align, objConfig.rominputcontrol_visible, true);
            romStatus = objScene.CreateLabel("RomStatus", objConfig.romstatus_x_pos, objConfig.romstatus_y_pos, objConfig.romstatus_width, objConfig.romstatus_height, "",
			                                  objConfig.romstatus_font_name,
                                              objConfig.romstatus_font_size,
                                              objConfig.romstatus_font_style,
                                              objConfig.romstatus_font_color, objConfig.romstatus_backcolor, objConfig.romstatus_text_align, objConfig.romstatus_visible, true);
            romCategory = objScene.CreateLabel("RomCategory", objConfig.romcategory_x_pos, objConfig.romcategory_y_pos, objConfig.romcategory_width, objConfig.romcategory_height, "",
			                                 objConfig.romcategory_font_name,
                                             objConfig.romcategory_font_size,
                                             objConfig.romcategory_font_style,
                                             objConfig.romcategory_font_color, objConfig.romcategory_backcolor, objConfig.romcategory_text_align, objConfig.romcategory_visible, true);

            // FNET
            fnetMessage = objScene.CreateTicker("FnetMessage", 0, (int)(objConfig.screen_res_y * .9f), objConfig.screen_res_x, objConfig.menu_font_size * 3,
                                  objConfig.menu_font_name,
                                  (int)(objConfig.menu_font_size * 1.5f),
                                  objConfig.menu_font_style,
                                  objConfig.menu_selected_font_color, objConfig.menu_backcolor, runFnet, true);

            // TOAST
            toastMessage = objScene.CreateToast("ToastMessage", 0, (int)(objConfig.screen_res_y * .8), objConfig.screen_res_x, 50, string.Empty,
                                 objConfig.menu_font_name,
                                 objConfig.menu_font_size,
                                 objConfig.menu_font_style,
                                 objConfig.menu_selected_font_color, objConfig.menu_backcolor, TextAlign.Center);

            // FX Sounds
            sfxList = objScene.CreateSound("sfxList", objConfig.sound_fx_list, objConfig.sound_fx_volume);
            sfxMenu = objScene.CreateSound("sfxMenu", objConfig.sound_fx_menu, objConfig.sound_fx_volume);
            sfxCancel = objScene.CreateSound("sfxCancel", objConfig.sound_fx_cancel, objConfig.sound_fx_volume);
            sfxConfirm = objScene.CreateSound("sfxConfirm", objConfig.sound_fx_confirm, objConfig.sound_fx_volume);
            sfxStartEmu = objScene.CreateSound("sfxStartEmu", objConfig.sound_fx_startemu, objConfig.sound_fx_volume);

            // Background Music
            objScene.SetBackgroundMusic(objConfig.music_path, objConfig.music_volume);

            currentRom = null;
            romList = new CListBox(objScene, this, objConfig.romlist_width, objConfig.romlist_height, objConfig.romlist_item_height, objConfig.romlist_x_pos, objConfig.romlist_y_pos,
			                       objConfig.romlist_font_name,
                                   objConfig.romlist_font_size,
                                   objConfig.romlist_font_style,
			                       objConfig.romlist_font_color, objConfig.romlist_backcolor, objConfig.romlist_selected_font_color, objConfig.romlist_selected_backcolor, objConfig.current_sort,
                                   objConfig.romlist_text_align, objConfig.romlist_disable_stars);

            SelectPlatform(objConfig.current_platform);

            if (currentRom != null)
            {
                var index = romList.FindIndexByName(objConfig.current_game, true);
                if (index == 0)
                {
                    currentRom = romList.SelectFirstItem();
                    objConfig.SetParameter("current_game", currentRom.Key);
                }
                else
                    currentRom = romList.SelectItem(index);
            }

            platformList = objConfig.GetPlatformList();
            if (objConfig.current_platform != "all_emu")
            {
                emulatorList = objConfig.GetEmulatorList();
                gameListList = objConfig.GetGameListList();
            }
            else
            {
                emulatorList = new List<string>();
                gameListList = new List<string>();
            }

            objScene.CreateVideo("", objConfig.snapshot_width, objConfig.snapshot_height, objConfig.snapshot_x_pos, objConfig.snapshot_y_pos, objConfig.video_volume, objConfig.video_speed);
        }

        private void RefreshMediaObjects()
        {
            // Mapping Image (use emulator's specific mapping file; if missing, default to general one)
            objScene.ResetCustomImage();
            var fileName = new StringBuilder(Application.StartupPath).Append(Path.DirectorySeparatorChar).Append("config")
                .Append(Path.DirectorySeparatorChar).Append(objConfig.current_platform).Append(Path.DirectorySeparatorChar)
                .Append(objConfig.current_emulator).Append(Path.DirectorySeparatorChar).Append("mapping.png").ToString();
            if (!File.Exists(fileName))
                fileName = new StringBuilder(Application.StartupPath).Append(Path.DirectorySeparatorChar).Append("mapping.png").ToString();
            objScene.SetCustomImage(fileName);

            // FX Sounds
            sfxList.SetSound(new StringBuilder(Application.StartupPath).Append(Path.DirectorySeparatorChar).Append("media").Append(Path.DirectorySeparatorChar).Append(objConfig.sound_fx_list).ToString(), objConfig.sound_fx_volume);
            sfxMenu.SetSound(new StringBuilder(Application.StartupPath).Append(Path.DirectorySeparatorChar).Append("media").Append(Path.DirectorySeparatorChar).Append(objConfig.sound_fx_menu).ToString(), objConfig.sound_fx_volume);
            sfxCancel.SetSound(new StringBuilder(Application.StartupPath).Append(Path.DirectorySeparatorChar).Append("media").Append(Path.DirectorySeparatorChar).Append(objConfig.sound_fx_cancel).ToString(), objConfig.sound_fx_volume);
            sfxConfirm.SetSound(new StringBuilder(Application.StartupPath).Append(Path.DirectorySeparatorChar).Append("media").Append(Path.DirectorySeparatorChar).Append(objConfig.sound_fx_confirm).ToString(), objConfig.sound_fx_volume);
            sfxStartEmu.SetSound(new StringBuilder(Application.StartupPath).Append(Path.DirectorySeparatorChar).Append("media").Append(Path.DirectorySeparatorChar).Append(objConfig.sound_fx_startemu).ToString(), objConfig.sound_fx_volume);

            // Background Music
            objScene.SetBackgroundMusic(objConfig.music_path, objConfig.music_volume);
            _marqueeImage.Visible = objConfig.marquee_visible;
            _cabinetImage.Visible = objConfig.cabinet_visible;
        }

        private void RefreshLayout()
        {
            Utils.PrintLog("RefreshLayout");
            objConfig.RestoreDefaultValuesFromLevel(Levels.LAYOUT_INI);
            objConfig.LoadConfig(Levels.LAYOUT_INI);

            if (!objConfig.test_mode && objConfig.switch_res)
            {
                Utils.ChangeResolution(objConfig.screen_res_x, objConfig.screen_res_y, objConfig.rotate_screen);
            }
            objScene.ChangeRes(objConfig.screen_res_x, objConfig.screen_res_y, objConfig.rotate_screen, true);

            // Reset Scene Images
            objScene.ResetImages();

            // Main Image
            var filename = Application.StartupPath + Path.DirectorySeparatorChar + "layouts" + Path.DirectorySeparatorChar + objConfig.current_layout + Path.DirectorySeparatorChar + "main.png";
            if (File.Exists(filename))
                _backgroundImage = objScene.CreateBackgroundImage(filename, objConfig.background_width, objConfig.background_height, objConfig.background_frame_duration_ms, objConfig.background_repeat_delay_ms);

            // Bezel Image
            filename = Application.StartupPath + Path.DirectorySeparatorChar + "layouts" + Path.DirectorySeparatorChar + objConfig.current_layout + Path.DirectorySeparatorChar + "bezel.png";
            if (File.Exists(filename))
                _bezelImage = objScene.CreateBezelImage(filename, objConfig.background_width, objConfig.background_height, objConfig.bezel_frame_duration_ms, objConfig.bezel_repeat_delay_ms);

            // Actors Image
            filename = Application.StartupPath + Path.DirectorySeparatorChar + "layouts" + Path.DirectorySeparatorChar + objConfig.current_layout + Path.DirectorySeparatorChar + "actors.png";
            if (File.Exists(filename))
                _actorsImage = objScene.CreateActorsImage(filename, objConfig.background_width, objConfig.background_height, objConfig.actors_frame_duration_ms, objConfig.actors_repeat_delay_ms);

            // Mapping Image (default location = startup path)
            var fileName = new StringBuilder(Application.StartupPath).Append(Path.DirectorySeparatorChar).Append("mapping.png").ToString();
            objScene.SetCustomImage(fileName);

            // SnapShot Image
            fileName = Application.StartupPath + Path.DirectorySeparatorChar + "layouts" + Path.DirectorySeparatorChar + objConfig.current_layout + Path.DirectorySeparatorChar + "snap.png";
            _snapshotImage = objScene.CreateImage(OBJScene.ImageType.Snapshot, fileName, objConfig.snapshot_width, objConfig.snapshot_height, objConfig.snapshot_x_pos, objConfig.snapshot_y_pos, fileName, objConfig.snapshot_stretch, true, objConfig.snapshot_blackbackground, false);

            // Cabinet Image
            fileName = Application.StartupPath + Path.DirectorySeparatorChar + "layouts" + Path.DirectorySeparatorChar + objConfig.current_layout + Path.DirectorySeparatorChar + "cabinet.png";
            _cabinetImage = objScene.CreateImage(OBJScene.ImageType.Cabinet, fileName, objConfig.cabinet_width, objConfig.cabinet_height, objConfig.cabinet_x_pos, objConfig.cabinet_y_pos, fileName, objConfig.cabinet_stretch, objConfig.cabinet_visible, objConfig.cabinet_blackbackground, false);

            // Marquee Image
            fileName = Application.StartupPath + Path.DirectorySeparatorChar + "layouts" + Path.DirectorySeparatorChar + objConfig.current_layout + Path.DirectorySeparatorChar + "marquee.png";
            _marqueeImage = objScene.CreateImage(OBJScene.ImageType.Marquee, fileName, objConfig.marquee_width, objConfig.marquee_height, objConfig.marquee_x_pos, objConfig.marquee_y_pos, fileName, objConfig.marquee_stretch, objConfig.marquee_visible, objConfig.marquee_blackbackground, false);

            // Video
            objScene.CreateVideo("", objConfig.snapshot_width, objConfig.snapshot_height, objConfig.snapshot_x_pos, objConfig.snapshot_y_pos, objConfig.video_volume, objConfig.video_speed);

            // Background image
            if (objScene.backgroundImage != null)
            {
                objScene.backgroundImage.LoadImage(Application.StartupPath + Path.DirectorySeparatorChar + "layouts" + Path.DirectorySeparatorChar + objConfig.current_layout + Path.DirectorySeparatorChar + "main.png");
            }

            // Actors image
            if (objScene.actorsImage != null)
            {
                objScene.actorsImage.LoadImage(Application.StartupPath + Path.DirectorySeparatorChar + "layouts" + Path.DirectorySeparatorChar + objConfig.current_layout + Path.DirectorySeparatorChar + "actors.png");
            }

            // Bezel image
            if (objScene.bezelImage != null)
            {
                objScene.bezelImage.LoadImage(Application.StartupPath + Path.DirectorySeparatorChar + "layouts" + Path.DirectorySeparatorChar + objConfig.current_layout + Path.DirectorySeparatorChar + "bezel.png");
            }

            // Scene Objects (screensaver, labels, etc.)
            objScene.RefreshObjects(_snapshotImage.FileNotFoundName, objConfig.romlist_font_color, objConfig.romlist_backcolor);
            objMenu.Init(objConfig.screen_res_x, objConfig.screen_res_y, objConfig.rotate_screen, objConfig.menu_width, objConfig.menu_item_height, objConfig.menu_font_name, objConfig.menu_font_size, objConfig.menu_font_style, objConfig.menu_font_color, objConfig.menu_backcolor, objConfig.menu_selected_font_color, objConfig.menu_selected_backcolor);
            objScene.CreateScreenSaver(objConfig.snapshot_width, objConfig.snapshot_height, objConfig.screen_saver_font_color, objConfig.screen_saver_backcolor, objConfig.menu_font_name, objConfig.menu_font_size, objConfig.menu_font_style);

            // LEDs
            CLedManager.Initialize(objScene, objConfig.smartasd_id, objConfig.smartasd_debug_mode, objConfig.smartasd_mode);

            // start slide
            objScene.StartScreenTransition(CDrawable.Transition.Slide);

            // Feel Labels
            //var bmp = objScene.BackBufferBPM;
            romCounter.RefreshObject(objConfig.romcounter_x_pos, objConfig.romcounter_y_pos, objConfig.romcounter_width, objConfig.romcounter_height, "",
                                     objConfig.romcounter_font_name,
                                     objConfig.romcounter_font_size,
                                     objConfig.romcounter_font_style,
                                     objConfig.romcounter_font_color, objConfig.romcounter_backcolor, objConfig.romcounter_text_align, objConfig.romcounter_visible, true);
            platformName.RefreshObject(objConfig.platformname_x_pos, objConfig.platformname_y_pos, objConfig.platformname_width, objConfig.platformname_height, "",
                                     objConfig.platformname_font_name,
                                     objConfig.platformname_font_size,
                                     objConfig.platformname_font_style,
                                     objConfig.platformname_font_color, objConfig.platformname_backcolor, objConfig.platformname_text_align, objConfig.platformname_visible, true);
            emulatorName.RefreshObject(objConfig.emulatorname_x_pos, objConfig.emulatorname_y_pos, objConfig.emulatorname_width, objConfig.emulatorname_height, "",
                                     objConfig.emulatorname_font_name,
                                     objConfig.emulatorname_font_size,
                                     objConfig.emulatorname_font_style,
                                     objConfig.emulatorname_font_color, objConfig.emulatorname_backcolor, objConfig.emulatorname_text_align, objConfig.emulatorname_visible, true);
            gameListName.RefreshObject(objConfig.gamelistname_x_pos, objConfig.gamelistname_y_pos, objConfig.gamelistname_width, objConfig.gamelistname_height, "",
                                     objConfig.gamelistname_font_name,
                                     objConfig.gamelistname_font_size,
                                     objConfig.gamelistname_font_style,
                                     objConfig.gamelistname_font_color, objConfig.gamelistname_backcolor, objConfig.gamelistname_text_align, objConfig.gamelistname_visible, true);
            romName.RefreshObject(objConfig.romname_x_pos, objConfig.romname_y_pos, objConfig.romname_width, objConfig.romname_height, "",
                                     objConfig.romname_font_name,
                                     objConfig.romname_font_size,
                                     objConfig.romname_font_style,
                                     objConfig.romname_font_color, objConfig.romname_backcolor, objConfig.romname_text_align, objConfig.romname_visible, true);
            romDescription.RefreshObject(objConfig.romdescription_x_pos, objConfig.romdescription_y_pos, objConfig.romdescription_width, objConfig.romdescription_height, "",
                                     objConfig.romdescription_font_name,
                                     objConfig.romdescription_font_size,
                                     objConfig.romdescription_font_style,
                                     objConfig.romdescription_font_color, objConfig.romdescription_backcolor, objConfig.romdescription_text_align, objConfig.romdescription_visible, true);
            romManufacturer.RefreshObject(objConfig.rommanufacturer_x_pos, objConfig.rommanufacturer_y_pos, objConfig.rommanufacturer_width, objConfig.rommanufacturer_height, "",
                                     objConfig.rommanufacturer_font_name,
                                     objConfig.rommanufacturer_font_size,
                                     objConfig.rommanufacturer_font_style,
                                     objConfig.rommanufacturer_font_color, objConfig.rommanufacturer_backcolor, objConfig.rommanufacturer_text_align, objConfig.rommanufacturer_visible, true);
            romDisplayType.RefreshObject(objConfig.romdisplaytype_x_pos, objConfig.romdisplaytype_y_pos, objConfig.romdisplaytype_width, objConfig.romdisplaytype_height, "",
                                       objConfig.romdisplaytype_font_name,
                                       objConfig.romdisplaytype_font_size,
                                       objConfig.romdisplaytype_font_style,
                                       objConfig.romdisplaytype_font_color, objConfig.romdisplaytype_backcolor, objConfig.romdisplaytype_text_align, objConfig.romdisplaytype_visible, true);
            romInputControl.RefreshObject(objConfig.rominputcontrol_x_pos, objConfig.rominputcontrol_y_pos, objConfig.rominputcontrol_width, objConfig.rominputcontrol_height, "",
                                       objConfig.rominputcontrol_font_name,
                                       objConfig.rominputcontrol_font_size,
                                       objConfig.rominputcontrol_font_style,
                                       objConfig.rominputcontrol_font_color, objConfig.rominputcontrol_backcolor, objConfig.rominputcontrol_text_align, objConfig.rominputcontrol_visible, true);
            romStatus.RefreshObject(objConfig.romstatus_x_pos, objConfig.romstatus_y_pos, objConfig.romstatus_width, objConfig.romstatus_height, "",
                                       objConfig.romstatus_font_name,
                                       objConfig.romstatus_font_size,
                                       objConfig.romstatus_font_style,
                                       objConfig.romstatus_font_color, objConfig.romstatus_backcolor, objConfig.romstatus_text_align, objConfig.romstatus_visible, true);
            romCategory.RefreshObject(objConfig.romcategory_x_pos, objConfig.romcategory_y_pos, objConfig.romcategory_width, objConfig.romcategory_height, "",
                                       objConfig.romcategory_font_name,
                                       objConfig.romcategory_font_size,
                                       objConfig.romcategory_font_style,
                                       objConfig.romcategory_font_color, objConfig.romcategory_backcolor, objConfig.romcategory_text_align, objConfig.romcategory_visible, true);
            // FNET
            fnetMessage.RefreshObject(0, (int)(objConfig.screen_res_y * .9f), objConfig.screen_res_x, objConfig.menu_font_size * 3, string.Empty,
                                  objConfig.menu_font_name,
                                  (int)(objConfig.menu_font_size * 1.5f),
                                  objConfig.menu_font_style,
                                  objConfig.menu_selected_font_color, objConfig.menu_backcolor, TextAlign.Left, runFnet, true);

            // TOAST
            toastMessage.RefreshObject(0, (int)(objConfig.screen_res_y * .8), objConfig.screen_res_x, 50, string.Empty,
                                 objConfig.menu_font_name,
                                 objConfig.menu_font_size,
                                 objConfig.menu_font_style,
                                 objConfig.menu_selected_font_color, objConfig.menu_selected_backcolor, TextAlign.Center, true, false);

            // Rom List
            var list = romList.List;
            currentRom = null;
            romList = new CListBox(objScene, this, objConfig.romlist_width, objConfig.romlist_height, objConfig.romlist_item_height, objConfig.romlist_x_pos, objConfig.romlist_y_pos,
                                   objConfig.romlist_font_name,
                                   objConfig.romlist_font_size,
                                   objConfig.romlist_font_style,
                                   objConfig.romlist_font_color, objConfig.romlist_backcolor, objConfig.romlist_selected_font_color, objConfig.romlist_selected_backcolor, objConfig.current_sort,
                                   objConfig.romlist_text_align, objConfig.romlist_disable_stars);
            romList.List = list;
            romList.FilterByDescription(searchString, objConfig.fulltext_search);
            romList.StartTransition(CDrawable.Transition.FadeIn);

            // update stats
            romList.UpdateStats();
            romList.UpdateSelection();

            currentRom = romList.SelectItem(romList.FindIndexByName(objConfig.current_game, true));
            videoTickCount = TickCount + (objConfig.video_delay * 1000);
            screenSaverTickCount = TickCount + (objConfig.screen_saver_slide_time * 1000);

            RefreshMediaObjects();
        }

        private void ClearRomObjects()
        {
            if (romList != null) { romList.Clear(); }
            currentRom = null;
            if (romCounter != null) { romCounter.Text = ""; }
            if (romName != null) { romName.Text = ""; }
            if (romDescription != null) { romDescription.Text = ""; }
            if (romManufacturer != null) { romManufacturer.Text = ""; }
            if (romDisplayType != null) { romDisplayType.Text = ""; }
            if (romInputControl != null) { romInputControl.Text = ""; }
            if (romStatus != null) { romStatus.Text = ""; }
            if (romCategory != null) { romCategory.Text = ""; }
            if (objScene.snapshotImage != null && objScene.snapshotImage.Visible) { objScene.snapshotImage.LoadImage(objScene.snapshotImage.FileNotFoundName); }

            var cabinet = _cabinetImage;
            if (cabinet != null && cabinet.Visible) { cabinet.LoadImage(cabinet.FileNotFoundName); }

            var marquee = _marqueeImage;
            if (marquee != null && marquee.Visible) { marquee.LoadImage(marquee.FileNotFoundName); }
            if (objScene.videosnapVideo != null)
            {
                objScene.videosnapVideo.PlayVideo(this, "", "", objConfig.snapshot_width, objConfig.snapshot_height, 0, 0, 0, 1, () => { });
            }
        }

        private void StartRomlistTransition(CDrawable.Transition pendingTransition)
        {
            romList.StartTransition(pendingTransition);
            romCategory.StartTransition(pendingTransition);
            romCounter.StartTransition(pendingTransition);
            romDescription.StartTransition(pendingTransition);
            romDisplayType.StartTransition(pendingTransition);
            romInputControl.StartTransition(pendingTransition);
            romManufacturer.StartTransition(pendingTransition);
            romName.StartTransition(pendingTransition);
            romStatus.StartTransition(pendingTransition);
            objScene.snapshotImage.StartTransition(pendingTransition);
        }

        #endregion

        #region UI Management
        private void UpdateUI(GameTime gameTime)
        {
            if (!_machineState.isRunning)
                Exit();

            TickCount = Environment.TickCount & Int32.MaxValue;

            // Update
            _elapsedTime += gameTime.ElapsedGameTime.TotalMilliseconds;

            // 1 Second has passed
            if (_elapsedTime >= 1000.0d)
            {
                _machineState.fps = _fpsCounter;
                _fpsCounter = 0;
                _elapsedTime = 0;
            }

            objInput.UpdateKeyboard();

            if (objConfig.use_mouse != UseMouse.No)
                objInput.UpdateMouse();

            if (objConfig.use_joypad)
                objInput.UpdateJoyPad();

            // update pending messages
            if (_isMessageWaiting)
            {
                objMenu.ShowMessage(_pendingMessage, TextAlign.Center);
                _isMessageWaiting = false;
            }

            // delay screensaver during async tasks
            if (_asyncTaskWait)
            {
                screenSaverTickCount = TickCount + (objConfig.screen_saver_delay * 1000);
            }

            if (fnetTasksTickCount <= TickCount)
            {
                // do scheduled tasks
                if (runFnet)
                {
                    // update list order (don't move selection)
                    networkHelper.EnqueueTask(
                        new NetworkTask(
                            objConfig.fnet_sort == FnetSortType.LastPlayed ? NetworkTask.TaskTypeEnum.LastPlayedList : NetworkTask.TaskTypeEnum.MostPlayedList,
                            new string[] { false.ToString() },
                            true));
                    networkHelper.EnqueueTask(
                        new NetworkTask(NetworkTask.TaskTypeEnum.UserNotifications,
                            new string[] { fnetLastUpdate.ToString("yyyy-MM-dd H:mm:ss") },
                            true));

                }
                fnetTasksTickCount = TickCount + FNET_TASKS_TIME_MS;
            }

            var workingDir_foo = string.Empty;
            if (objConfig.screen_saver_enabled != ScreenSaverType.None && !string.IsNullOrEmpty(objConfig.snapshot_path) && romList.ItemsCount > 0)
            {
                if (_machineState.State == MachineState.StateEnum.ScreenSaver && objInput.KeyState(objConfig.action_key) == InputState.KeyDown)
                {
                    // find current screensaver game
                    currentRom = romList.SelectItem(romList.FindIndexByName(screenSaverRom.Key, true));
                    ShowToast("Selected: " + screenSaverRom.Description);
                    videoTickCount = TickCount + (objConfig.video_delay * 1000);
                }

                if (objInput.NoEvent && screenSaverTickCount != 0)
                {
                    // start screensaver
                    if (screenSaverTickCount <= TickCount && _machineState.State != MachineState.StateEnum.ScreenSaver)
                    {
                        _machineState.State = MachineState.StateEnum.ScreenSaver;

                        // start scene fade in
                        objScene.StartScreenTransition(CDrawable.Transition.FadeIn);
                        // CLedManager.StartAboutBoxProgram();

                        // pause video during screen saver
                        objScene.videosnapVideo.Pause();
                        objScene.PauseBackgroundMusic();
                    }
                }
                else
                {
                    if (_machineState.State == MachineState.StateEnum.ScreenSaver)
                    {
                        if (objInput.KeyState(objConfig.screensaver_previous_game) == InputState.KeyDown && !objScene.IsChangingScreenSaver)
                        {

                            screenSaverTickCount = TickCount + (objConfig.screen_saver_slide_time * 1000);
                            screenSaverRom = romList.FilteredList[romList.PreviousShuffleIndex() - 1];

                            if (screenSaverRom.FeelInfo.RunChain.IsSet)
                            {
                                objScene.SetScreenSaverImage(new StringBuilder(ParseArguments(objConfig, screenSaverRom.FeelInfo.SnapshotPath, false, ref workingDir_foo))
                                    .Append(Path.DirectorySeparatorChar)
                                    .Append(screenSaverRom.FeelInfo.RomName).Append(".").Append(screenSaverRom.FeelInfo.SnapshotExtension).ToString(),
                                    screenSaverRom.Description, objConfig.snapshot_stretch, false);
                            }
                            else
                            {
                                objScene.SetScreenSaverImage(new StringBuilder(ParseArguments(objConfig, objConfig.snapshot_path, false, ref workingDir_foo))
                                    .Append(Path.DirectorySeparatorChar)
                                    .Append(screenSaverRom.Key).Append(".").Append(objConfig.snapshot_extension).ToString(),
                                    screenSaverRom.Description, objConfig.snapshot_stretch, false);
                            }
                            CLedManager.StartRomChangeProgram();
                            sfxList.Play();
                        }

                        if (objInput.KeyState(objConfig.screensaver_next_game) == InputState.KeyDown && !objScene.IsChangingScreenSaver)
                        {

                            screenSaverTickCount = TickCount + (objConfig.screen_saver_slide_time * 1000);
                            screenSaverRom = romList.FilteredList[romList.NextShuffleIndex() - 1];
                            if (screenSaverRom.FeelInfo.RunChain.IsSet)
                            {
                                objScene.SetScreenSaverImage(new StringBuilder(ParseArguments(objConfig, screenSaverRom.FeelInfo.SnapshotPath, false, ref workingDir_foo))
                                    .Append(Path.DirectorySeparatorChar)
                                    .Append(screenSaverRom.FeelInfo.RomName).Append(".").Append(screenSaverRom.FeelInfo.SnapshotExtension).ToString(),
                                    screenSaverRom.Description, objConfig.snapshot_stretch, true);
                            }
                            else
                            {
                                objScene.SetScreenSaverImage(new StringBuilder(ParseArguments(objConfig, objConfig.snapshot_path, false, ref workingDir_foo))
                                    .Append(Path.DirectorySeparatorChar)
                                    .Append(screenSaverRom.Key).Append(".").Append(objConfig.snapshot_extension).ToString(),
                                    screenSaverRom.Description, objConfig.snapshot_stretch, true);
                            }
                            CLedManager.StartRomChangeProgram();
                            sfxList.Play();
                        }

                        if (objInput.KeyState(objConfig.screensaver_previous_game) == InputState.None && objInput.KeyState(objConfig.screensaver_next_game) == InputState.None)
                        {
                            CLedManager.SetFECommandLeds(objConfig.frontend_list_controls, CLedManager.FETransition.ListCommands);

                            // wait 1sec to let user release button..
                            screenSaverResume = TickCount + 1000;
                            _machineState.State = MachineState.StateEnum.Normal;

                            // start fade in
                            objScene.StartScreenTransition(CDrawable.Transition.FadeIn);

                            objScene.videosnapVideo.Resume();
                            objScene.ResumeBackgroundMusic();
                        }
                    }
                    else
                        screenSaverTickCount = TickCount + (objConfig.screen_saver_delay * 1000);
                }
            }

            if (_machineState.State == MachineState.StateEnum.ScreenSaver && screenSaverTickCount <= TickCount)
            {
                screenSaverTickCount = TickCount + (objConfig.screen_saver_slide_time * 1000);
                screenSaverRom = romList.FilteredList[romList.NextShuffleIndex() - 1];
                if (objConfig.screen_saver_enabled == ScreenSaverType.StartRandomGame)
                {
                    // random game screensaver
                    currentRom = screenSaverRom;
                    _machineState.State = MachineState.StateEnum.StartEmulator;
                }
                else
                {
                    // slideshow screensaver
                    if (screenSaverRom.FeelInfo.RunChain.IsSet)
                    {
                        objScene.SetScreenSaverImage(new StringBuilder(ParseArguments(objConfig, screenSaverRom.FeelInfo.SnapshotPath, false, ref workingDir_foo))
                            .Append(Path.DirectorySeparatorChar)
                            .Append(screenSaverRom.FeelInfo.RomName).Append(".").Append(screenSaverRom.FeelInfo.SnapshotExtension).ToString(),
                            screenSaverRom.Description, objConfig.snapshot_stretch, true);
                    }
                    else
                    {
                        objScene.SetScreenSaverImage(new StringBuilder(ParseArguments(objConfig, objConfig.snapshot_path, false, ref workingDir_foo))
                            .Append(Path.DirectorySeparatorChar)
                            .Append(screenSaverRom.Key).Append(".").Append(objConfig.snapshot_extension).ToString(),
                            screenSaverRom.Description, objConfig.snapshot_stretch, true);
                    }
                    CLedManager.StartRomChangeProgram();
                }
            }

            if (currentRom != null)
            {
                // rom change
                if (previousRom == null || previousRom != currentRom)
                {
                    romChanged = true;
                    idleUpdateTickCount = TickCount + 300;

                    if (_machineState.State != Feel.MachineState.StateEnum.ScreenSaver)
                    {
                        previousRom = currentRom;

                        romCounter.Text = "Game " + romList.SelectedIndex + " of " + romList.ItemsCount;
                        if (objConfig.romname_visible)
                        {
                            var name = new StringBuilder(currentRom.FeelInfo.RomName);
                            if (!string.IsNullOrEmpty(currentRom.CloneOf))
                                name.Append(" (" + currentRom.CloneOf + ")");
                            romName.Text = CRomManager.LabelCleanup(name.ToString());
                        }
                        romDescription.Text = currentRom.Description;
                        if (objConfig.rommanufacturer_visible)
                        {
                            var manufacturer = new StringBuilder(currentRom.Year);
                            if (!string.IsNullOrEmpty(currentRom.Manufacturer))
                            {
                                if (manufacturer.Length > 0)
                                    manufacturer.Append(" - ");
                                manufacturer.Append(currentRom.Manufacturer);
                            }
                            romManufacturer.Text = manufacturer.ToString();
                        }
                        romDisplayType.Text = currentRom.VideoType;
                        romInputControl.Text = currentRom.InputControl;
                        romStatus.Text = currentRom.Status;
                        romCategory.Text = currentRom.Category;
                    }

                    // video management
                    // reset videosnap timer
                    videoTickCount = TickCount + (objConfig.video_delay * 1000);
                    // stop any running video
                    if (objScene.videosnapVideo != null)
                        objScene.videosnapVideo.Stop();

                    platformName.Text = objConfig.platform_title;
                    var workingDir = string.Empty;
                    if (currentRom.FeelInfo.RunChain.IsSet)
                    {
                        var path = new StringBuilder(ParseArguments(objConfig, currentRom.FeelInfo.SnapshotPath, false, ref workingDir)).Append(Path.DirectorySeparatorChar).ToString();
                        var snapshotImage = path + new StringBuilder(currentRom.FeelInfo.RomName).Append(".").Append(currentRom.FeelInfo.SnapshotExtension).ToString();
                        var snapshotAltImage = !string.IsNullOrEmpty(currentRom.CloneOf) ? path + new StringBuilder(currentRom.CloneOf).Append(".").Append(currentRom.FeelInfo.SnapshotExtension).ToString() : string.Empty;
                        _snapshotImage.LoadImage(snapshotImage, snapshotAltImage, false, false);

                        // platformName.Text = currentRom.FeelInfo.PlatformDesc;
                        emulatorName.Text = currentRom.FeelInfo.PlatformDesc;
                        gameListName.Text = currentRom.FeelInfo.EmulatorDesc;
                    }
                    else
                    {
                        var path = new StringBuilder(ParseArguments(objConfig, objConfig.snapshot_path, false, ref workingDir)).Append(Path.DirectorySeparatorChar).ToString();
                        var snapshotImage = path + new StringBuilder(currentRom.Key).Append(".").Append(objConfig.snapshot_extension).ToString();
                        var snapshotAltImage = !string.IsNullOrEmpty(currentRom.CloneOf) ? path + new StringBuilder(currentRom.CloneOf).Append(".").Append(objConfig.snapshot_extension).ToString() : string.Empty;
                        _snapshotImage.LoadImage(snapshotImage, snapshotAltImage, false, false);

                        // platformName.Text = objConfig.platform_title;
                        emulatorName.Text = objConfig.emulator_title;
                        gameListName.Text = objConfig.gamelist_title;
                    }

                    
                    // switch cabinet and marquee to not-found images (high speed scroll)
                    _cabinetImage.LoadImage("");
                    _marqueeImage.LoadImage("");

                    CLedManager.SetFECommandLeds(objConfig.frontend_list_controls, CLedManager.FETransition.ListCommands);
                }

                // commands are idle: do some other things..
                if (objInput.NoEvent && romChanged && idleUpdateTickCount < TickCount)
                {
                    romChanged = false;
                    // set background to snapshot image: DISABLED
                    //_backgroundImage.LoadImage(objConfig.snapshot_path + Path.DirectorySeparatorChar + currentRom.Name + ".png");

                    // refresh cabinet and marquee
                    if (currentRom.FeelInfo.RunChain.IsSet)
                    {

                        var path = new StringBuilder(ParseArguments(objConfig, currentRom.FeelInfo.CabinetPath, false, ref workingDir_foo)).Append(Path.DirectorySeparatorChar).ToString();
                        var cabinetImage = path + new StringBuilder(currentRom.FeelInfo.RomName).Append(".").Append(currentRom.FeelInfo.CabinetExtension).ToString();
                        var cabinetAltImage = !string.IsNullOrEmpty(currentRom.CloneOf) ? path + new StringBuilder(currentRom.CloneOf).Append(".").Append(currentRom.FeelInfo.CabinetExtension).ToString() : string.Empty;
                        _cabinetImage.LoadImage(cabinetImage, cabinetAltImage, true, false);

                        path = new StringBuilder(ParseArguments(objConfig, currentRom.FeelInfo.MarqueePath, false, ref workingDir_foo)).Append(Path.DirectorySeparatorChar).ToString();
                        var marqueeImage = path + new StringBuilder(currentRom.FeelInfo.RomName).Append(".").Append(currentRom.FeelInfo.MarqueeExtension).ToString();
                        var marqueeAltImage = !string.IsNullOrEmpty(currentRom.CloneOf) ? path + new StringBuilder(currentRom.CloneOf).Append(".").Append(currentRom.FeelInfo.MarqueeExtension).ToString() : string.Empty;
                        _marqueeImage.LoadImage(marqueeImage, marqueeAltImage, true, false);
                    }
                    else
                    {
                        var path = new StringBuilder(ParseArguments(objConfig, objConfig.cabinet_path, false, ref workingDir_foo)).Append(Path.DirectorySeparatorChar).ToString();
                        var cabinetImage = path + new StringBuilder(currentRom.Key).Append(".").Append(objConfig.cabinet_extension).ToString();
                        var altCabinetImage = !string.IsNullOrEmpty(currentRom.CloneOf) ? path + new StringBuilder(currentRom.CloneOf).Append(".").Append(objConfig.cabinet_extension).ToString() : string.Empty;
                        _cabinetImage.LoadImage(cabinetImage, altCabinetImage, true, false);

                        path = new StringBuilder(ParseArguments(objConfig, objConfig.marquee_path, false, ref workingDir_foo)).Append(Path.DirectorySeparatorChar).ToString();
                        var marqueeImage = path + new StringBuilder(currentRom.Key).Append(".").Append(objConfig.marquee_extension).ToString();
                        var marqueeAltImage = !string.IsNullOrEmpty(currentRom.CloneOf) ? path + new StringBuilder(currentRom.CloneOf).Append(".").Append(objConfig.marquee_extension).ToString() : string.Empty;
                        _marqueeImage.LoadImage(marqueeImage, marqueeAltImage, true, false);
                    }
                }
            }

            // new version: show changelog
            if (_machineState.State == MachineState.StateEnum.NewVersion)
            {
                _machineState.State = MachineState.StateEnum.Normal;
                shortCut = true;
                SwitchMenu("changelog");
            }

            // manage videosnap
            if (videoTickCount <= TickCount && (screenSaverResume <= TickCount || objConfig.screen_saver_enabled == ScreenSaverType.None))
            {
                // gain focus on first videosnap timeout
                if (!isFocusGained)
                {
                    feelForm.Activate();
                    isFocusGained = true;

                    if (!isVersionCheckDone)
                    {
                        // run FEEL update
                        networkHelper.EnqueueTask(
                            new NetworkTask(
                                NetworkTask.TaskTypeEnum.FeelUpdate,
                                new string[] { objConfig.feel_uuid, Application.ProductVersion, objConfig.update_beta ? "1" : "0" },
                                true));
                        isVersionCheckDone = true;
                    }
                }

                if (currentRom != null)
                {
                    // play videosnap
                    if (objConfig.current_platform != "all_emu")
                    {
                        var videoPath = new StringBuilder(objConfig.video_path).Append(Path.DirectorySeparatorChar)
                            .Append(currentRom.Key).ToString();
                        var parentVideoPath = videoPath;
                        if (!string.IsNullOrEmpty(currentRom.CloneOf))
                            parentVideoPath = new StringBuilder(objConfig.video_path).Append(Path.DirectorySeparatorChar)
                            .Append(currentRom.CloneOf).ToString();
                        if (objScene.videosnapVideo.PlayVideo(this, videoPath, parentVideoPath, objConfig.snapshot_width, objConfig.snapshot_height,
                            objConfig.snapshot_x_pos, objConfig.snapshot_y_pos, objConfig.video_volume, objConfig.video_speed, () =>
                        {
                            // gain focus
                            try
                            {
                                feelForm.Activate();
                            }
                            catch { }

                            // a new video started: fade in
                            objScene.snapshotImage.StartTransition(CDrawable.Transition.FadeIn);
                            objScene.snapshotImage.VideoSizeIsSet = false;
                        } ))
                        { }
                    }
                    else
                    {
                        var videoPath = new StringBuilder(currentRom.FeelInfo.VideoPath).Append(Path.DirectorySeparatorChar)
                            .Append(currentRom.FeelInfo.RomName).ToString();
                        // no parent search on all_emu list ;)
                        if (objScene.videosnapVideo.PlayVideo(this, videoPath, videoPath, objConfig.snapshot_width, objConfig.snapshot_height,
                            objConfig.snapshot_x_pos, objConfig.snapshot_y_pos, objConfig.video_volume, objConfig.video_speed, () => 
                        {
                            // a new video started: fade in
                            objScene.snapshotImage.StartTransition(CDrawable.Transition.FadeIn);
                            objScene.snapshotImage.VideoSizeIsSet = false;
                        } ))
                        { }
                    }
                }
            }
            // update current video frame
            objScene.videosnapVideo.Update();

            StartEmulator();

            RunPendingActions();

            if (screenSaverResume <= TickCount && _machineState.State != Feel.MachineState.StateEnum.ScreenSaver && !_asyncTaskWait)
            {
                if (string.IsNullOrEmpty(currentMenu))
                    CheckInputNormal();
                else
                    CheckInputMenu();
            }
            if (objConfig.smartasd_mode != SmartAsdMode.None)
                CLedManager.Update(TickCount);

            // update layout
            if (currentLayout != objConfig.current_layout)
            {
                //SaveConfig();
                RefreshLayout();
                currentLayout = objConfig.current_layout;
            }
        }

        private void UpdateCommandStatus(CStatusAnimatedImage.enStatus status)
        {
            if (_backgroundImage != null) _backgroundImage.Status = status;
            if (_actorsImage != null) _actorsImage.Status = status;
            if (_bezelImage != null) _bezelImage.Status = status;
        }

        private bool RunAfterAnimation(CStatusAnimatedImage.enStatus status, Action action)
        {
            var longestAnimation = _backgroundImage;
            if (longestAnimation == null ||(_actorsImage != null && _actorsImage.GetTotalFrameDuration(status) > longestAnimation.GetTotalFrameDuration(status)))
                longestAnimation = _actorsImage;
            if (longestAnimation == null || (_bezelImage != null && _bezelImage.GetTotalFrameDuration(status) > longestAnimation.GetTotalFrameDuration(status)))
                longestAnimation = _bezelImage;

            if (longestAnimation != null && longestAnimation.GetTotalFrameDuration(status) > 0)
            {
                if (longestAnimation.RunAfterAnimation(status, action))
                {
                    UpdateCommandStatus(status);
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region UI Messages
        private string _pendingMessage = string.Empty;
        private bool _isMessageWaiting = false;
        public void ShowMessage(string message, bool modal)
        {
            ShowMessage(message, modal, false);
        }

        private void ShowMessage(string message, bool modal, bool immediate)
        {
            Action action = () =>
            {
                _asyncTaskWait = modal;
                _pendingMessage = string.Empty;
                foreach (var row in message.Split('\n'))
                {
                    _pendingMessage += "        " + row + "        \n";
                }
                _pendingMessage = " \n" + _pendingMessage + " ";
                _isMessageWaiting = true;
            };

            if (immediate)
                action();
            else
                RunOnUIThread(action);
        }

        public void ShowToast(string message)
        {
            ShowToast(message, false);
        }

        private void ShowToast(string message, bool immediate)
        {
            Action action = () =>
            {
                _asyncTaskWait = false;
                objMenu.HideMessage(true);
                objMenu.HideMenu(true);
                currentMenu = null;
                UpdateCommandStatus(CStatusAnimatedImage.enStatus.Idle);

                toastMessage.ShowMessage(message);
            };
            if (immediate)
                action();
            else
                RunOnUIThread(action);
        }
        #endregion

        #region Emulation Management
        private void StartEmulator()
        {
            if (_machineState.State == MachineState.StateEnum.StartEmulator && currentRom != null)
            {
                if (objConfig.smartasd_mode != SmartAsdMode.None)
                {
                    // set control rom LEDs and close channel
                    CLedManager.SetInputControlLeds(currentRom.InputControl != string.Empty ? currentRom.InputControl : objConfig.input_controls, false, true);
                    CLedManager.Update(TickCount + 1000);
                    CLedManager.Close();
                }

                objScene.videosnapVideo.Stop();
                objScene.PauseBackgroundMusic();
                objInput.Release();

                // re-enable keyboard
                _asyncTaskWait = false;

                Utils.PrintLog("---- Start Process ----");
                var command = "";
                var arguments = "";
                var workingDir = "";

                var _currentRomName = currentRom.FeelInfo.RomName;
                OBJConfig _runConfig;

                _runConfig = objConfig;
                // update current game
                objConfig.SetParameter("current_game", _currentRomName);
                // save config immediately (preserve data in case of crash)
                SaveConfig();
                // backup config
                objConfig.BackUpState();

                if (_runConfig.list_type == ListType.rom_settings_ini_list)
                {
                    if (_runConfig.rom_extension.IndexOf(',') > -1)
                    {
                        ShowMessage("Multiple rom extensions are not allowed with list_type = 2 (rom_settings_ini_list).\r\nPlease check " + objConfig.emulator_title + " configuration", false);
                        _machineState.State = MachineState.StateEnum.Normal;
                        return;
                    }
                    else
                        _runConfig.LoadConfig(Levels.EMULATOR_INI, _runConfig.rom_path + Path.DirectorySeparatorChar + currentRom.Key + "." + _runConfig.rom_extension);
                }
                else
                {
                    // multi-emu start: load temp config
                    if (currentRom.FeelInfo.RunChain.IsSet)
                    {
                        _runConfig = LoadTempConfig(currentRom.FeelInfo.RunChain);
                        _runConfig.SetParameter("current_game", _currentRomName);
                    }
                    // rom custom settings
                    _runConfig.LoadConfig(Levels.ROM_INI, string.Empty);
                }

                // set hidden cursor
                //CCursorManager.SetFeelSystemCursor();

                if (_runConfig.list_type == ListType.mame_listinfo || _runConfig.list_type == ListType.mame_xml_list)
                    networkHelper.EnqueueTask(new NetworkTask(
                        NetworkTask.TaskTypeEnum.GameStart,
                        new string[] { currentRom.Key, currentRom.Description }, true));

                // PRE EMULATOR APP
                if (!string.IsNullOrEmpty(_runConfig.pre_emulator_app_commandline))
                {
                    command = ParseArguments(_runConfig, _runConfig.pre_emulator_app_commandline, true, ref workingDir);
                    arguments = ParseArguments(_runConfig, _runConfig.pre_emulator_app_arguments, false, ref workingDir);
                    StartProcess(command, arguments, workingDir, true, false, objConfig.pre_emulator_wait_for_exit, "pre_emulator_app");
                }
                command = ParseArguments(_runConfig, _runConfig.emulator_commandline, true, ref workingDir);
                arguments = ParseArguments(_runConfig, _runConfig.emulator_arguments, false, ref workingDir);

                var startTime = DateTime.Now;
                Clipboard.Clear();
                if (!string.IsNullOrEmpty(_runConfig.input_mapping))
                {
                    // REMAPPING
                    var arr = _runConfig.input_mapping.Split(',');
                    var str = "-run " + "\"" + command + " " + arguments.Replace("\"", "'") + "\"";
                    //var str = "-run " + "\"" + command + " " + arguments.Replace("\"", "\"\"") + "\"";
                    foreach (var map in arr)
                        str += " -map " + map.Trim();
                    StartProcess(Application.StartupPath + Path.DirectorySeparatorChar + "hook.exe", str, workingDir, true, false, true, "hook.exe");
                }
                else
                {
                    // EMULATOR
                    StartProcess(command, arguments, workingDir, _runConfig.emulator_nodosbox, _runConfig.emulator_useshell, true, "emulator");
                }

                objConfig.RestoreBackUpState();

                var playTime = DateTime.Now.Subtract(startTime).TotalSeconds;
                if (playTime > 3)
                {
                    currentRom.PlayedCount++;
                    currentRom.PlayedTime = currentRom.PlayedTime.Add(TimeSpan.FromSeconds(playTime));
                    currentRom.TimeStamp = DateTime.Now;
                    CRomManager.UpdateRomStats(objConfig, currentRom);
                    romList.UpdateStats();
                    romList.Sort();
                    romList.FindRom(currentRom);
                    CRomManager.SetLastPlayedGame(objConfig, currentRom);
                }
                // POST EMULATOR APP
                if (!string.IsNullOrEmpty(objConfig.post_emulator_app_commandline))
                {
                    command = ParseArguments(_runConfig, _runConfig.post_emulator_app_commandline, true, ref workingDir);
                    arguments = ParseArguments(_runConfig, _runConfig.post_emulator_app_arguments, false, ref workingDir);
                    StartProcess(command, arguments, workingDir, true, false, objConfig.post_emulator_wait_for_exit, "post_emulator_app");
                }

                //if (objConfig.list_type == ListType.rom_settings_ini_list ||
                //    objConfig.list_type == ListType.rom_custom_settings)

                if (_runConfig.list_type == ListType.mame_listinfo || _runConfig.list_type == ListType.mame_xml_list)
                    networkHelper.EnqueueTask(new NetworkTask(
                        NetworkTask.TaskTypeEnum.GameStop,
                        new string[] { currentRom.Key }, true));

                Utils.PrintLog("-----------------------");

                CLedManager.SetFECommandLeds(objConfig.frontend_list_controls, CLedManager.FETransition.ListCommands);

                // reset default cursor
                //CCursorManager.ResetSystemCursor();

                // refresh TickCount
                TickCount = Environment.TickCount & Int32.MaxValue;
                screenSaverResume = TickCount + 1000;
                screenSaverTickCount = TickCount + (objConfig.screen_saver_delay * 1000);
                
                // start fade in
                objScene.StartScreenTransition(CDrawable.Transition.FadeIn);
                UpdateCommandStatus(currentMenu == null ? CStatusAnimatedImage.enStatus.Idle : CStatusAnimatedImage.enStatus.CommandMenu);

                // restore default behaviour
                objScene.ResumeBackgroundMusic();

                // save printscreen, if available
                if (Clipboard.ContainsData(DataFormats.Dib))
                {
                    if (Utils.SaveSnapShotFromClipboard(objConfig, currentRom))
                    {
                        objScene.snapshotImage.Restore();
                        sfxConfirm.Play();
                    }
                    else
                        sfxCancel.Play();
                }
                
                // regain focus
                isFocusGained = false;
            }

            if (_machineState.State == MachineState.StateEnum.BuildGameList)
            {
                BuildGameList();
            }
        }

        private void StartProcess(string command, string arguments, string workingDir, bool noDosBox, bool useShellExecute, bool waitForExit, string currentPhase)
        {
            if (!File.Exists(command))
            {
                if (currentPhase == "hook.exe")
                    ShowMessage("Error executing " + currentPhase + " - file not found:\r\n" + Utils.ShortenString(command, 40) + "\r\nPlease check hook.exe is available.", false);
                else
                    ShowMessage("Error executing " + currentPhase + " - file not found:\r\n" + Utils.ShortenString(command, 40) + "\r\nPlease check commandline parameter in " +
                        Utils.ShortenString(objConfig.GetFileNameFromParam(currentPhase + "_commandline"), 20), false);

                _machineState.State = MachineState.StateEnum.Normal;
                return;
            }
            var folder = Path.GetDirectoryName(command);
            var startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = noDosBox;
            startInfo.UseShellExecute = useShellExecute;
            if (workingDir != string.Empty)
                startInfo.WorkingDirectory = workingDir;
            else
                startInfo.WorkingDirectory = folder;
            startInfo.FileName = command;
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            startInfo.Arguments = arguments;
            Utils.PrintLog("Current phase: " + currentPhase);
            Utils.PrintLog("Command run: " + command + " " + arguments);
            Utils.PrintLog("Working dir: " + startInfo.WorkingDirectory);

            if (waitForExit)
            {
                using (var exeProcess = Process.Start(startInfo))
                {
                    SetWindowPos(false);
                    try
                    {
                        exeProcess.WaitForInputIdle();
                    }
                    catch
                    {
                        // no problem here ;-)
                        //Utils.PrintLog("NoWaitForInputIdle: " + e);
                    }
                    if (exeProcess != null)
                        exeProcess.WaitForExit();
                    _machineState.State = MachineState.StateEnum.Normal;
                    objMenu.HideMessage(true);
                    SetWindowPos(true);
                }
                if (!objConfig.test_mode && objConfig.switch_res)
                {
                    Utils.ChangeResolution(objConfig.screen_res_x, objConfig.screen_res_y, objConfig.rotate_screen);
                }
            }
            else
            {
                using (var exeProcess = Process.Start(startInfo))
                    exeProcess.WaitForExit(3000);
            }
        }

        private string ParseArguments(OBJConfig config, string arguments, bool checkForLnk, ref string workingDir)
        {
            while (true)
            {
                if (string.IsNullOrEmpty(arguments))
                    return string.Empty;
                var found = false;
                var romPath = config.rom_path;
                if (currentRom != null)
                {
                    romPath += currentRom.RomRelativePath != string.Empty ? Path.DirectorySeparatorChar + currentRom.RomRelativePath : string.Empty;
                }

                var index = arguments.IndexOf("[rom_path]", 0, StringComparison.CurrentCultureIgnoreCase);
                if (index >= 0) { arguments = arguments.Substring(0, index) + romPath + arguments.Substring(index + 10); found = true; }

                if (currentRom != null)
                {
                    index = romPath.IndexOf("[emulator_path]", 0, StringComparison.CurrentCultureIgnoreCase);
                    if (index >= 0) { romPath = romPath.Substring(0, index) + config.emulator_path + romPath.Substring(index + 15); }

                    var romName = currentRom.FeelInfo.RomName;
                    var romExtension = Utils.GetRomExtension(romPath, romName, config.rom_extension);

                    index = arguments.IndexOf("[rom_name]", 0, StringComparison.CurrentCultureIgnoreCase);
                    if (index >= 0) { arguments = arguments.Substring(0, index) + romName + arguments.Substring(index + 10); found = true; }

                    index = arguments.IndexOf("[full_path]", 0, StringComparison.CurrentCultureIgnoreCase);
                    if (index >= 0) { arguments = arguments.Substring(0, index) + romPath + Path.DirectorySeparatorChar + romName + "." + romExtension + arguments.Substring(index + 11); found = true; }

                    index = arguments.IndexOf("[rom_extension]", 0, StringComparison.CurrentCultureIgnoreCase);
                    if (index >= 0) { arguments = arguments.Substring(0, index) + romExtension + arguments.Substring(index + 15); found = true; }

                    index = arguments.IndexOf("[full_dos_path]", 0, StringComparison.CurrentCultureIgnoreCase);
                    if (index >= 0)
                    {
                        var full_path = romPath + Path.DirectorySeparatorChar + romName + "." + romExtension;
                        arguments = arguments.Substring(0, index) + Utils.ConvertToDosPath(full_path) + arguments.Substring(index + 15);
                        found = true;
                    }
                }

                index = arguments.IndexOf("[emulator_path]", 0, StringComparison.CurrentCultureIgnoreCase);
                if (index >= 0) { arguments = arguments.Substring(0, index) + config.emulator_path + arguments.Substring(index + 15); found = true; }

                index = arguments.IndexOf("[video_path]", 0, StringComparison.CurrentCultureIgnoreCase);
                if (index >= 0) { arguments = arguments.Substring(0, index) + config.video_path + arguments.Substring(index + 12); found = true; }

                if (!found)
                    break;
            }

            if (checkForLnk)
            {
                arguments = Utils.LocateLnkPath(arguments, ref workingDir);
            }
            return arguments;
        }
        #endregion

        #region Input Check
        private void CheckInputNormal()
        {
            // keyboard repeat unramping
            if (objInput.NoEvent)
            {
                keyboardScrolling = false;
                if (current_keyboard_scroll_rate < objConfig.keyboard_scroll_rate - 5)
                    current_keyboard_scroll_rate += 5;
                else
                    current_keyboard_scroll_rate = objConfig.keyboard_scroll_rate;
            }

            if (
                    objInput.KeyState(objConfig.next_game) == InputState.KeyPress
                    || objInput.KeyState(objConfig.previous_game) == InputState.KeyPress
                    || objInput.KeyState(objConfig.next_letter_game) == InputState.KeyPress
                    || objInput.KeyState(objConfig.previous_letter_game) == InputState.KeyPress
                    || objInput.KeyState(objConfig.next_page) == InputState.KeyPress
                    || objInput.KeyState(objConfig.previous_page) == InputState.KeyPress
                )
            {
                // keyboard repeat auto-reset: DISABLED
                //current_keyboard_scroll_rate = objConfig.keyboard_scroll_rate;
                keyboardScrolling = false;
                sfxList.StopLooping();
                UpdateCommandStatus (CStatusAnimatedImage.enStatus.Idle);
            }

            if (objInput.KeyState(objConfig.show_exit_menu) == InputState.KeyPress && !objMenu.MessageShown && _machineState.State != Feel.MachineState.StateEnum.CustomImage)
            {
                SwitchMenu("exit");
                shortCut = true;
            }

            if (!objMenu.MessageShown)
            {
                if (objInput.KeyState(objConfig.show_custom_image) == InputState.KeyPress)
                {
                    if (_machineState.State == Feel.MachineState.StateEnum.CustomImage)
                    {
                        if (!objScene.SetNextCustomImage())
                        {
                            _machineState.State = Feel.MachineState.StateEnum.Normal;
                            // resume videosnap
                            objScene.videosnapVideo.Resume();
                        }
                    }
                    else
                    {
                        objScene.SetLastCustomImage();
                        _machineState.State = Feel.MachineState.StateEnum.CustomImage;
                        // pause videosnap
                        objScene.videosnapVideo.Pause();
                    }
                }
                else
                {
                    if (_machineState.State == Feel.MachineState.StateEnum.CustomImage && (!objInput.KeyboardNoEvent || !objInput.JoypadNoEvent) && objInput.KeyState(objConfig.show_custom_image) != InputState.KeyDown)
                    {
                        _machineState.State = Feel.MachineState.StateEnum.Normal;
                        keyboardScrolling = true;
                        keyboardTickCount = TickCount + objConfig.keyboard_scroll_rate;
                    }
                }
            }

            if (_machineState.State != Feel.MachineState.StateEnum.CustomImage && !keyboardScrolling)
            {
                if (currentRom != null && objInput.KeyState(objConfig.show_game_info) == InputState.KeyPress)
                {
                    SwitchMenu("show_game_info_dl");
                    shortCut = true;
                }
                else
                {
                    // cancel on game info shown
                    if (objMenu.MessageShown && (!objInput.KeyboardNoEvent || !objInput.JoypadNoEvent)
                        && objInput.KeyState(objConfig.show_game_info) != InputState.KeyDown)
                    {
                        objMenu.HideMessage(true);
                        // resume videosnap
                        objScene.videosnapVideo.Resume();

                        // update LEDs for controls
                        if (currentRom != null)
                            CLedManager.SetFECommandLeds(objConfig.frontend_list_controls, CLedManager.FETransition.ListCommands);

                        // play close sound
                        if (objInput.KeyState(objConfig.show_game_info) == InputState.KeyPress)
                            sfxMenu.Play();
                    }
                }
            }

            if (!keyboardScrolling && _machineState.State != Feel.MachineState.StateEnum.CustomImage && !objMenu.MessageShown)
            {
                if (objInput.KeyState(objConfig.action_key) == InputState.KeyPress && currentRom != null)
                {
                    if (objConfig.show_extended_messages) ShowMessage("Running " + currentRom.Description + "...", false, true);

                    if (!RunAfterAnimation(CStatusAnimatedImage.enStatus.CommandStartEmu, () => { _machineState.State = MachineState.StateEnum.StartEmulator; }))
                        _machineState.State = MachineState.StateEnum.StartEmulator;

                    if (!sfxStartEmu.Play())
                        sfxConfirm.Play();
                }

                if (objInput.KeyState(objConfig.menu_key) == InputState.KeyPress && !keyboardScrolling && !mouseScrolling)
                {
                    SwitchMenu("menu");
                }

                if (platformList.Count > 1)
                {
                    if (objInput.KeyState(objConfig.select_platform) == InputState.KeyPress) { SwitchMenu("select_platform"); shortCut = true; }
                    if (objInput.KeyState(objConfig.previous_platform) == InputState.KeyPress ||
                        objInput.KeyState(objConfig.next_platform) == InputState.KeyPress)
                    {
                        var isPrevious = objInput.KeyState(objConfig.previous_platform) == InputState.KeyPress;
                        for (var i = 0; i < platformList.Count; i++)
                        {
                            var arr = platformList[i].Split('|');
                            if (objConfig.current_platform.Equals(arr[0], StringComparison.CurrentCultureIgnoreCase))
                            {
                                var newId = i + (isPrevious ? -1 : 1);
                                if (!isPrevious && newId == platformList.Count)
                                    newId = 0;
                                if (isPrevious && newId == -1)
                                    newId = platformList.Count - 1;
                                SaveConfig();
                                arr = platformList[newId].Split('|');
                                SelectPlatform(arr[0]);
                                if (arr[0] != "all_emu" && objConfig.show_extended_messages)
                                    ShowToast("Platform: " + arr[1]);
                                else if (objConfig.show_extended_messages)
                                    ShowToast("\"TOP\" gamelist");
                                sfxConfirm.Play();
                                break;
                            }
                        }
                    }
                }

                if (emulatorList.Count > 1)
                {
                    if (objInput.KeyState(objConfig.select_emulator) == InputState.KeyPress) { SwitchMenu("select_emulator"); shortCut = true; }
                    if (objInput.KeyState(objConfig.previous_emulator) == InputState.KeyPress ||
                        objInput.KeyState(objConfig.next_emulator) == InputState.KeyPress)
                    {
                        var isPrevious = objInput.KeyState(objConfig.previous_emulator) == InputState.KeyPress;
                        for (var i = 0; i < emulatorList.Count; i++)
                        {
                            var arr = emulatorList[i].Split('|');
                            if (objConfig.current_emulator.Equals(arr[0], StringComparison.CurrentCultureIgnoreCase))
                            {
                                var newId = i + (isPrevious ? -1 : 1);
                                if (!isPrevious && newId == emulatorList.Count)
                                    newId = 0;
                                if (isPrevious && newId == -1)
                                    newId = emulatorList.Count - 1;
                                SaveConfig();
                                arr = emulatorList[newId].Split('|');
                                SelectEmulator(arr[0]);
                                if (objConfig.show_extended_messages) ShowToast("Emulator: " + arr[1]);
                                sfxConfirm.Play();
                                break;
                            }
                        }
                    }
                }

                if (gameListList.Count > 1)
                {
                    if (objInput.KeyState(objConfig.select_gamelist) == InputState.KeyPress) { SwitchMenu("select_gamelist"); shortCut = true; }
                    if (objInput.KeyState(objConfig.previous_gamelist) == InputState.KeyPress ||
                        objInput.KeyState(objConfig.next_gamelist) == InputState.KeyPress)
                    {
                        var isPrevious = objInput.KeyState(objConfig.previous_gamelist) == InputState.KeyPress;
                        for (var i = 0; i < gameListList.Count; i++)
                        {
                            var arr = gameListList[i].Split('|');
                            if (objConfig.current_gamelist.Equals(arr[0], StringComparison.CurrentCultureIgnoreCase))
                            {
                                var newId = i + (isPrevious ? -1 : 1);
                                if (!isPrevious && newId == gameListList.Count)
                                    newId = 0;
                                if (isPrevious && newId == -1)
                                    newId = gameListList.Count - 1;
                                SaveConfig();
                                arr = gameListList[newId].Split('|');
                                SelectGameList(arr[0]);
                                if (objConfig.show_extended_messages) ShowToast("Gamelist: " + arr[1]);
                                sfxConfirm.Play();
                                break;
                            }
                        }
                    }
                }

                if (gameListList.Count > 0)
                {
                    if (objInput.KeyState(objConfig.add_game_to_list) == InputState.KeyPress) { SwitchMenu("add_game_to_list"); shortCut = true; }
                    if (objConfig.current_gamelist != objConfig.current_emulator + "-0")
                    {
                        if (objInput.KeyState(objConfig.remove_game_from_list) == InputState.KeyPress)
                        {
                            if (currentRom != null)
                            {
                                ShowToast(currentRom.Description + ": removed");
                                RemoveGameFromList(currentRom);
                                sfxConfirm.Play();
                            }
                            else
                                sfxCancel.Play();
                        }
                    }
                    if (objInput.KeyState(objConfig.next_sort) == InputState.KeyPress)
                    {
                        currentRom = romList.SetNextSort();
                        romList.UpdateSelection();
                        romCounter.Text = "Game " + romList.SelectedIndex + " of " + romList.ItemsCount;
                        romList.UpdateStats();
                        objConfig.SetParameter("current_sort", ((int)romList.CurrentSort).ToString());
                        if (currentRom != null)
                        {
                            ShowToast("Current order: " + romList.CurrentSortDesc);
                            sfxConfirm.Play();
                        }
                        else
                            sfxCancel.Play();
                    }
                }

                if (currentRom != null)
                {
                    var index = romList.SelectedIndex;
                    if (objInput.KeyState(objConfig.next_game) == InputState.KeyDown)
                    {
                        UpdateCommandStatus(CStatusAnimatedImage.enStatus.CommandDown);
                        currentRom = romList.SelectNextItem(); keyboardScrolling = true;
                    }
                    else if (objInput.KeyState(objConfig.previous_game) == InputState.KeyDown)
                    {
                        UpdateCommandStatus(CStatusAnimatedImage.enStatus.CommandUp);
                        currentRom = romList.SelectPreviousItem(); keyboardScrolling = true; 
                    }
                    else if (objInput.KeyState(objConfig.next_letter_game) == InputState.KeyDown)
                    {
                        UpdateCommandStatus(CStatusAnimatedImage.enStatus.CommandDown);
                        currentRom = romList.SelectNextCharItem(); keyboardScrolling = true;
                    }
                    else if (objInput.KeyState(objConfig.previous_letter_game) == InputState.KeyDown)
                    {
                        UpdateCommandStatus(CStatusAnimatedImage.enStatus.CommandUp);
                        currentRom = romList.SelectPreviousCharItem(); keyboardScrolling = true;
                    }
                    else if (objInput.KeyState(objConfig.next_page) == InputState.KeyDown)
                    {
                        UpdateCommandStatus(CStatusAnimatedImage.enStatus.CommandDown);
                        currentRom = romList.SelectNextPageItem(); keyboardScrolling = true;
                    }
                    else if (objInput.KeyState(objConfig.previous_page) == InputState.KeyDown)
                    {
                        UpdateCommandStatus(CStatusAnimatedImage.enStatus.CommandUp);
                        currentRom = romList.SelectPreviousPageItem(); keyboardScrolling = true;
                    }

                    if (objConfig.show_search_key && keyboardScrolling && romList.CurrentSort != SortType.Ranking && romList.CurrentSort != SortType.MostRecentlyPlayed)
                        ShowToast(currentRom.SortComparisonChars(romList.CurrentSort), true);
                    toastMessage.Frozen = keyboardScrolling;

                    if (objInput.KeyState(objConfig.find_game) == InputState.KeyPress) { SwitchMenu("find_game"); shortCut = true; }

                    if (objInput.KeyState(objConfig.reload_config) == InputState.KeyPress)
                    {
                        RefreshConfiguration();
                        sfxConfirm.Play();
                    }

                    if (keyboardScrolling)
                    {
                        keyboardTickCount = TickCount + current_keyboard_scroll_rate;
                        if (current_keyboard_scroll_rate >= objConfig.keyboard_min_scroll_rate + 10)
                            current_keyboard_scroll_rate -= 10;
                        else
                            current_keyboard_scroll_rate = objConfig.keyboard_min_scroll_rate;  
                    }
                    else
                    {
                        if (objConfig.use_mouse != UseMouse.No)
                        {
                            if (mouseTickCount <= TickCount && mouseScrolling)
                            {
                                mouseScrolling = false;
                                mouseTickCount = TickCount + objConfig.mouse_scroll_rate;
                            }
                            if (!mouseScrolling)
                            {
                                if (objConfig.use_mouse == UseMouse.XAxis)
                                {
                                    if (objInput.MouseMoveLeft) { currentRom = romList.SelectPreviousItem(); mouseScrolling = true; }
                                    if (objInput.MouseMoveRight) { currentRom = romList.SelectNextItem(); mouseScrolling = true; }
                                }
                                else
                                {
                                    if (objInput.MouseMoveUp) { currentRom = romList.SelectPreviousItem(); mouseScrolling = true; }
                                    if (objInput.MouseMoveDown) { currentRom = romList.SelectNextItem(); mouseScrolling = true; }
                                }

                                if (mouseScrolling && romList.SelectedIndex != index)
                                    mouseTickCount = TickCount + objConfig.mouse_scroll_rate;
                            }
                        }
                    }
                    if (romList.SelectedIndex != index)
                    {
                        sfxList.Play();
                        videoTickCount = TickCount + (objConfig.video_delay * 1000);
                    }
                    else
                    {
                        if (mouseTickCount <= TickCount - objConfig.mouse_scroll_rate)
                            sfxList.StopLooping();
                    }
                }
            }

            // reset scrolling status
            if (keyboardScrolling)
            {
                if (keyboardTickCount <= TickCount && keyboardScrolling)
                    keyboardScrolling = false;
            }
        }

        private void CheckInputMenu()
        {
            sfxList.StopLooping();

            if (objInput.KeyState(objConfig.menu_down) == InputState.KeyPress) { keyboardScrolling = false; }
            if (objInput.KeyState(objConfig.menu_up) == InputState.KeyPress) { keyboardScrolling = false; }
            if (objInput.KeyState(objConfig.menu_left) == InputState.KeyPress) { keyboardScrolling = false; }
            if (objInput.KeyState(objConfig.menu_right) == InputState.KeyPress) { keyboardScrolling = false; }

            if (objInput.KeyState(objConfig.action_key) == InputState.KeyPress || objInput.KeyState(objConfig.menu_ok) == InputState.KeyPress)
            {
                var menuKey = objMenu.MenuItemSelected.Key;
                var menuValue = objMenu.MenuItemSelected.Value;

                while (true)
                {
                    // find game
                    if (menuKey.StartsWith("find_game_") && menuKey.Length == "find_game_".Length + 1)
                    {
                        if (objInput.KeyState(objConfig.action_key) == InputState.KeyPress)
                        {
                            var index = romList.FindIndexByDescription(searchString, !objConfig.fulltext_search);
                            if (index > 0)
                            {
                                objConfig.SetParameter("fulltext_search", !objConfig.fulltext_search ? "1" : "0");
                                romList.FilterByDescription(searchString, objConfig.fulltext_search);
                                currentRom = romList.SelectItem(1);
                                romList.StartTransition(CDrawable.Transition.FadeIn);
                                SwitchMenu(null, "find_game", menuKey);
                                videoTickCount = TickCount + (objConfig.video_delay * 1000);
                                sfxMenu.Play();
                            }
                            else
                                sfxCancel.Play();
                        }
                        else
                        {
                            var index = romList.FindIndexByDescription(searchString + menuKey.Substring(10).Replace("_", " "), objConfig.fulltext_search);
                            if (index > 0)
                            {
                                searchString += menuKey.Substring(10).Replace("_", " ");
                                romList.FilterByDescription(searchString, objConfig.fulltext_search);
                                currentRom = romList.SelectItem(1);
                                romList.StartTransition(CDrawable.Transition.FadeIn);
                                SwitchMenu(null, "find_game", menuKey);
                                videoTickCount = TickCount + (objConfig.video_delay * 1000);
                                sfxMenu.Play();
                            }
                            else
                                sfxCancel.Play();
                        }
                        break;
                    }
                    // new gamelist
                    if (menuKey.StartsWith("new_gamelist_") && menuKey.Length == "new_gamelist_".Length + 1)
                    {
                        if (objInput.KeyState(objConfig.action_key) == InputState.KeyPress)
                        {
                            var arr = gameListList[gameListList.Count - 1].Split('|');
                            var root = arr[0].Substring(0, arr[0].LastIndexOf('-') + 1);
                            var index = 0;
                            foreach (var gamelist in gameListList)
                            {
                                try
                                {
                                    var key = gamelist.Split('|')[0];
                                    index = Math.Max(index, int.Parse(key.Substring(key.LastIndexOf('-') + 1)));
                                }
                                catch { }
                            }
                            index++;
                            // update gamelist list
                            gameListList.Add(root + index + "|" + newGamelistName);
                            // select new gamelist
                            SelectGameList(root + index);
                            // set title
                            objConfig.SetParameter("gamelist_title", newGamelistName);
                            // save config
                            SaveConfig();
                            ShowToast("Gamelist: " + newGamelistName + " created");
                            SwitchMenu(null);
                            sfxConfirm.Play();
                        }
                        else
                        {
                            newGamelistName += menuKey.Substring("new_gamelist_".Length).Replace("_", " ").ToLower();
                            newGamelistName = char.ToUpper(newGamelistName[0]) + newGamelistName.Substring(1);
                            SwitchMenu(null, "new_gamelist", menuKey);
                            sfxMenu.Play();
                        }
                        break;
                    }
                    if (menuKey.StartsWith("fnet_select_nickname_") && menuKey.Length == "fnet_select_nickname_".Length + 1)
                    {
                        if (objInput.KeyState(objConfig.action_key) == InputState.KeyPress)
                        {
                            // save nickname
                            networkHelper.EnqueueTask(new NetworkTask(NetworkTask.TaskTypeEnum.UpdateNickname, null, true));
                            ShowToast("Nickname: " + networkHelper.CurrentSession.nickname);
                            SwitchMenu(null);
                            sfxConfirm.Play();
                        }
                        else
                        {
                            if (networkHelper.CurrentSession.nickname.Length < 8)
                            {
                                networkHelper.CurrentSession.nickname += menuKey.Substring("fnet_select_nickname_".Length).Replace("_", " ").ToLower();
                                sfxMenu.Play();
                            }
                            else
                                sfxCancel.Play();
                            SwitchMenu(null, "fnet_select_nickname", menuKey);
                        }
                        break;
                    }
                    if (menuKey.StartsWith("select_platform_"))
                    {
                        var platform = menuKey.Substring(16);
                        var arr = platform.Split('|');
                        if (objConfig.current_platform != arr[0])
                        {
                            SaveConfig();
                            if (SelectPlatform(arr[0]))
                            {
                                if (arr[0] != "all_emu" && objConfig.show_extended_messages)
                                    ShowToast("Platform: " + menuValue);
                                else if (objConfig.show_extended_messages)
                                    ShowToast("\"TOP\" gamelist");
                                SwitchMenu(null);
                                sfxConfirm.Play();
                            }
                            else
                            {
                                if (emulatorList.Count > 0)
                                    SwitchMenu("select_emulator");
                                else
                                {
                                    SwitchMenu(null);
                                    sfxConfirm.Play();
                                }
                            }
                        }
                        else
                            sfxCancel.Play();
                        break;
                    }
                    if (menuKey.StartsWith("select_emulator_"))
                    {
                        var emulator = menuKey.Substring(16);
                        var arr = emulator.Split('|');
                        if (objConfig.current_emulator != arr[0])
                        {
                            SaveConfig();
                            SelectEmulator(arr[0]);
                            ShowToast("Emulator: " + menuValue);
                            SwitchMenu(null);
                            sfxConfirm.Play();
                            break;
                        }
                        else
                            sfxCancel.Play();
                        break;
                    }
                    if (menuKey.StartsWith("select_gamelist_"))
                    {
                        var gameList = menuKey.Substring(16);
                        var arr = gameList.Split('|');
                        if (objConfig.current_gamelist != arr[0])
                        {
                            SaveConfig();
                            SelectGameList(arr[0]);
                            ShowToast("Gamelist: " + menuValue);
                            SwitchMenu(null);
                            sfxConfirm.Play();
                            break;
                        }
                        else
                            sfxCancel.Play();
                        break;
                    }
                    if (menuKey.StartsWith("change_sort_"))
                    {
                        var list = menuKey.Substring(12);
                        var arr = list.Split('|');
                        currentRom = romList.ChangeSort((SortType)int.Parse(arr[0]));
                        romList.UpdateSelection();
                        romCounter.Text = "Game " + romList.SelectedIndex + " of " + romList.ItemsCount;
                        romList.UpdateStats();
                        objConfig.SetParameter("current_sort", ((int)romList.CurrentSort).ToString());
                        SaveConfig();
                        ShowToast("Current order: " + menuValue);
                        SwitchMenu(null);
                        sfxConfirm.Play();
                        break;
                    }
                    if (menuKey.StartsWith("add_game_to_list_"))
                    {
                        var gameList = menuKey.Substring(17);
                        var arr = gameList.Split('|');
                        var rom = currentRom;
                        if (arr[0] == "all_emu")
                        {
                            rom.FeelInfo = new RomFEELInfo(objConfig.current_platform, objConfig.current_emulator, objConfig.current_gamelist, rom.Key,
                                objConfig.platform_title, objConfig.emulator_title, objConfig.gamelist_title,
                                objConfig.snapshot_path, objConfig.snapshot_extension, objConfig.video_path,
                                objConfig.cabinet_path, objConfig.cabinet_extension, objConfig.marquee_path, objConfig.marquee_extension);
                        }
                        if (objConfig.current_gamelist != arr[0])
                        {
                            var oldList = objConfig.current_gamelist;
                            var oldPlatform = objConfig.current_platform;
                            SaveConfig();
                            if (arr[0] == "all_emu")
                                SelectPlatform(arr[0]);
                            else
                                SelectGameList(arr[0]);

                            if (AddGameToList(rom))
                            {
                                ShowToast(rom.Description + ": added");
                                SwitchMenu(null);
                                sfxConfirm.Play();
                            }
                            else
                                sfxCancel.Play();

                            // restore simple name (run chain is not suitable on normal lists)
                            rom.FeelInfo.RunChain.Clear();
                            
                            if (arr[0] == "all_emu")
                                SelectPlatform(oldPlatform);
                            else
                                SelectGameList(oldList);

                            currentRom = romList.FindRom(rom);

                            // don't run fade-in
                            //romList.ResetPendingTransitions();
                            //_snapshotImage.ResetPendingTransitions();
                            //_marqueeImage.ResetPendingTransitions();
                            //_cabinetImage.ResetPendingTransitions();

                            romList.FindRom(rom);

                            break;
                        }
                        break;
                    }
                    if (menuKey == "start_game")
                    {
                        SwitchMenu(null);
                        if (objConfig.show_extended_messages) ShowMessage("Running " + currentRom.Description + "...", false, true);
                        if (!RunAfterAnimation(CStatusAnimatedImage.enStatus.CommandStartEmu, () => { _machineState.State = MachineState.StateEnum.StartEmulator; }))
                            _machineState.State = MachineState.StateEnum.StartEmulator;
                        if (!sfxStartEmu.Play())
                            sfxConfirm.Play();
                    }
                    if (menuKey == "service_mode")
                    {
                        objConfig.SetParameter("service_mode", !objConfig.service_mode ? "1" : "0");
                        SwitchMenu(null, "options", menuKey);
                        sfxConfirm.Play();
                        break;
                    }
                    if (menuKey == "show_search_key")
                    {
                        objConfig.SetParameter("show_search_key", !objConfig.show_search_key ? "1" : "0");
                        SwitchMenu(null, "options", menuKey);
                        sfxConfirm.Play();
                        break;
                    }
                    if (menuKey == "restore_explorer_on_exit")
                    {
                        objConfig.SetParameter("restore_explorer_at_exit", !objConfig.restore_explorer_at_exit ? "1" : "0");
                        SwitchMenu(null, "options", menuKey);
                        sfxConfirm.Play();
                        break;
                    }
                    if (menuKey == "show_extended_messages")
                    {
                        objConfig.SetParameter("show_extended_messages", !objConfig.show_extended_messages ? "1" : "0");
                        SwitchMenu(null, "options", menuKey);
                        sfxConfirm.Play();
                        break;
                    }
                    if (menuKey == "show_clock")
                    {
                        objConfig.SetParameter("show_clock", !objConfig.show_clock ? "1" : "0");
                        SwitchMenu(null, "options", menuKey);
                        sfxConfirm.Play();
                        break;
                    }
                    if (menuKey == "menu_show_sidebar")
                    {
                        objConfig.SetParameter("menu_show_sidebar", !objConfig.menu_show_sidebar ? "1" : "0");
                        SwitchMenu(null, "options", menuKey);
                        sfxConfirm.Play();
                        break;
                    }
                    if (menuKey == "service_mode_available")
                    {
                        objConfig.SetParameter("service_mode_available", !objConfig.service_mode_available ? "1" : "0");
                        // when service mode is disabled, force service menu hiding
                        if (!objConfig.service_mode_available)
                            objConfig.SetParameter("service_mode", "0");
                        SwitchMenu(null, "options", menuKey);
                        sfxConfirm.Play();
                        break;
                    }
                    if (menuKey == "update_beta")
                    {
                        objConfig.SetParameter("update_beta", !objConfig.update_beta ? "1" : "0");
                        SwitchMenu(null, "options", menuKey);
                        sfxConfirm.Play();
                        break;
                    }
                    if (menuKey == "fulltext_search")
                    {
                        if (searchString == string.Empty)
                        {
                            objConfig.SetParameter("fulltext_search", !objConfig.fulltext_search ? "1" : "0");
                            SwitchMenu(null, "options", menuKey);
                            sfxConfirm.Play();
                        }
                        else if (romList.FindIndexByDescription(searchString, !objConfig.fulltext_search) > 0)
                        {
                            objConfig.SetParameter("fulltext_search", !objConfig.fulltext_search ? "1" : "0");
                            currentRom = romList.Sort();
                            romList.FilterByDescription(searchString, objConfig.fulltext_search);
                            currentRom = romList.SelectItem(1);
                            romList.StartTransition(CDrawable.Transition.FadeIn);
                            SwitchMenu(null, "options", menuKey);
                            sfxConfirm.Play();
                        }
                        else
                        {
                            sfxCancel.Play();
                        }
                        break;
                    }
                    if (menuKey == "show_stars")
                    {
                        objConfig.SetParameter("romlist_disable_stars", !objConfig.romlist_disable_stars ? "1" : "0");
                        romList.DisableStars = objConfig.romlist_disable_stars;
                        romList.StartTransition(CDrawable.Transition.FadeIn);
                        SwitchMenu(null, "options", menuKey);
                        sfxConfirm.Play();
                        break;
                    }
                    if (menuKey == "show_clones")
                    {
                        objConfig.SetParameter("show_clones", !objConfig.show_clones ? "1" : "0");
                        SelectGameList(objConfig.current_gamelist);
                        SwitchMenu(null, "options", menuKey);
                        sfxConfirm.Play();
                        break;
                    }
                    if (menuKey == "cleanup_names")
                    {
                        objConfig.SetParameter("cleanup_names", !objConfig.cleanup_names ? "1" : "0");
                        RomDesc.CleanupName = objConfig.cleanup_names;
                        romList.StartTransition(CDrawable.Transition.FadeIn);
                        romList.UpdateSelection();
                        SwitchMenu(null, "options", menuKey);
                        sfxConfirm.Play();
                        break;
                    }
                    if (menuKey == "change_autostart")
                    {
                        if (objConfig.autostart_mode == AutostartMode.SingleGame)
                            autostartMode = AutostartMode.Off;
                        else
                            autostartMode = objConfig.autostart_mode + 1;
                        objConfig.SetParameter("autostart_mode", ((int)autostartMode).ToString());
                        SwitchMenu(null, "options", menuKey);
                        sfxConfirm.Play();
                        break;
                    }
                    if (menuKey == "change_screensaver")
                    {
                        if (objConfig.screen_saver_enabled == ScreenSaverType.StartRandomGame)
                            objConfig.SetParameter("screen_saver_enabled", ((int)ScreenSaverType.None).ToString());
                        else
                            objConfig.SetParameter("screen_saver_enabled", ((int)objConfig.screen_saver_enabled + 1).ToString());
                        screenSaverTickCount = TickCount + (objConfig.screen_saver_slide_time * 1000);
                        SwitchMenu(null, "options", menuKey);
                        sfxConfirm.Play();
                        break;
                    }
                    if (menuKey == "fnet_change_sort")
                    {
                        if (objConfig.fnet_sort == FnetSortType.LastPlayed)
                        {
                            objConfig.SetParameter("fnet_sort", ((int)FnetSortType.MostPlayed).ToString());
                            networkHelper.EnqueueTask(
                                new NetworkTask(
                                    NetworkTask.TaskTypeEnum.MostPlayedList,
                                    new string[] { false.ToString() },
                                    true));
                        }
                        else
                        {
                            objConfig.SetParameter("fnet_sort", ((int)FnetSortType.LastPlayed).ToString());
                            networkHelper.EnqueueTask(
                                new NetworkTask(
                                    NetworkTask.TaskTypeEnum.LastPlayedList,
                                    new string[] { false.ToString() },
                                    true));
                        }
                        SwitchMenu(null, "menu", menuKey);
                        sfxConfirm.Play();
                        break;
                    }

                    var currentRomGC = "";
                    if (currentRom != null)
                        currentRomGC = new GameRunChain(objConfig.current_platform, objConfig.current_emulator, objConfig.current_gamelist, currentRom.Key).ToString();
                    if (menuKey == "autostart_choose_game")
                    {
                        // current game is chosen: switch to saved config value
                        if (objConfig.autostart_single_game == currentRomGC)
                            objConfig.SetParameter("autostart_single_game", objConfig.autostart_single_game);
                        else if (currentRomGC != null)
                        {
                            // switch to current game
                            objConfig.SetParameter("autostart_single_game", currentRomGC.ToString());
                        }
                        SwitchMenu(null, "options", menuKey);
                        sfxConfirm.Play();
                        break;
                    }

                    if (menuKey == "select_random_game")
                    {
                        if (currentRom != null)
                        {
                            currentRom = romList.SelectItem(romList.NextShuffleIndex());
                            sfxConfirm.Play();
                            // reset focus on menu item
                            objMenu.MenuSetFocus();
                            videoTickCount = TickCount + (objConfig.video_delay * 1000);
                        }
                        break;
                    }
                    if (menuKey == "next_track")
                    {
                        sfxConfirm.Play();
                        objScene.NextTrackBackgroundMusic();
                        break;
                    }
                    if (menuKey == "select_platform"
                        || menuKey == "select_emulator"
                        || menuKey == "select_gamelist"
                        || menuKey == "find_game"
                        || menuKey == "options"
                        || menuKey == "new_gamelist"
                        || menuKey == "exit"
                        || menuKey == "add_game_to_list"
                        || menuKey == "select_layout"
                        || menuKey == "about"
                        || menuKey == "changelog"
                        || menuKey == "change_sort"
                        || menuKey == "change_autostart"
                        || menuKey == "change_screensaver"
                        || menuKey == "autostart_choose_game"
                        || menuKey == "reset_game_stats"
                        || menuKey == "build_list"
                        || menuKey == "show_feel_settings"
                        || menuKey == "show_game_info") { SwitchMenu(menuKey); break; };

                    // delete character
                    if (menuKey == "find_game_delete")
                    {
                        if (searchString != string.Empty)
                        {
                            searchString = searchString.Substring(0, searchString.Length - 1);
                            romList.FilterByDescription(searchString, objConfig.fulltext_search);
                            currentRom = romList.SelectItem(romList.FindIndexByName(currentRom.Key, true));
                            romList.StartTransition(CDrawable.Transition.FadeIn);
                            videoTickCount = TickCount + (objConfig.video_delay * 1000);
                            SwitchMenu(null, currentMenu, menuKey);
                            sfxMenu.Play();
                        }
                        else 
                        {
                            sfxCancel.Play();
                        }
                        break;
                    };
                    if (menuKey == "new_gamelist_delete")
                    {
                        if (newGamelistName != string.Empty)
                        {
                            newGamelistName = newGamelistName.Substring(0, newGamelistName.Length - 1);
                            SwitchMenu(null, currentMenu, menuKey);
                            sfxMenu.Play();
                        }
                        else 
                        {
                            sfxCancel.Play();
                        }
                        break;
                    };
                    if (menuKey == "fnet_select_nickname_delete")
                    {
                        if (networkHelper.CurrentSession.nickname != string.Empty)
                        {
                            networkHelper.CurrentSession.nickname = networkHelper.CurrentSession.nickname.
                                Substring(0, networkHelper.CurrentSession.nickname.Length - 1);
                            SwitchMenu(null, currentMenu, menuKey);
                            sfxMenu.Play();
                        }
                        else
                        {
                            sfxCancel.Play();
                        }
                        break;
                    };
                    if (menuKey == "remove_game_from_list")
                    {
                        var romDesc = currentRom.Description;
                        RemoveGameFromList(currentRom);
                        ShowToast(romDesc + ": removed");
                        SwitchMenu(null);
                        sfxConfirm.Play();
                        break;
                    }

                    if (menuKey == "reset_game_stats_ok")
                    {
                        currentRom.PlayedCount = 0;
                        currentRom.PlayedTime = currentRom.PlayedTimeNow = TimeSpan.Zero;
                        currentRom.TimeStamp = DateTime.MinValue;
                        romList.UpdateStats();
                        CRomManager.UpdateRomStats(objConfig, currentRom);
                        romList.Sort();
                        romList.UpdateSelection();
                        ShowToast(currentRom.Description + ": game stats reset");
                        SwitchMenu(null);
                        sfxConfirm.Play();
                        break;
                    }

                    if (menuKey == "reset_game_stats_cancel" || menuKey == "build_list_cancel")
                    {
                        SwitchMenu("options");
                        sfxCancel.Play();
                        break;
                    }

                    if (menuKey == "build_list_ok")
                    {
                        _machineState.State = MachineState.StateEnum.BuildGameList;
                        SwitchMenu(null);
                        break;
                    }
                    if (menuKey == "exit_to_windows")
                    {
                        _machineState.isRunning = false;
                        break;
                    }
                    if (menuKey == "restart")
                    {
                        _machineState.State = MachineState.StateEnum.Restart;
                        _machineState.isRunning = false;
                        break;
                    }
                    if (menuKey == "shut_down")
                    {
                        _machineState.State = MachineState.StateEnum.Shutdown;
                        _machineState.isRunning = false;
                        break;
                    }
                    if (menuKey == "standby")
                    {
                        Application.SetSuspendState(PowerState.Suspend, false, false);
                        SwitchMenu(null);
                        break;
                    }
                    if (menuKey == "hibernate")
                    {
                        Application.SetSuspendState(PowerState.Hibernate, false, false);
                        SwitchMenu(null);
                        break;
                    }
                    if (menuKey == "reload_configuration")
                    {
                        RefreshConfiguration();
                        SwitchMenu(null);
                        sfxConfirm.Play();
                        break;
                    }

                    if (menuKey.StartsWith("select_layout_"))
                    {
                        var layout = menuKey.Substring(14);
                        if (objConfig.current_layout != layout)
                        {
                            objConfig.SetParameter("current_layout", layout);
                            RefreshLayout();
                            // update configuration
                            SaveConfig();
                            currentLayout = objConfig.current_layout;
                            romList.UpdateStats();
                            romList.FindRom(currentRom);
                            SwitchMenu("select_layout");
                            //objScene.ShowToast("Current layout: " + currentLayout);
                            sfxConfirm.Play();
                            break;
                        }
                        break;
                    }
                    if (menuKey == "menu_close")
                    {
                        SwitchMenu(null);
                        sfxMenu.Play();
                        break;
                    }
                    if (menuKey == "feel_update")
                    {
                        if (networkHelper.EnqueueTask(
                            new NetworkTask(
                                NetworkTask.TaskTypeEnum.FeelUpdate,
                                new string[] { objConfig.feel_uuid, Application.ProductVersion, objConfig.update_beta ? "1" : "0" },
                                false)))
                        {
                            ShowMessage("Checking for updates: please wait...", true);
                        }
                        else
                            ShowToast("No updates on Community Edition, sorry :)", true);
                        SwitchMenu(null);
                        break;
                    }
                    if (menuKey == "show_game_info_dl")
                    {
                        if (networkHelper.EnqueueTask(new NetworkTask(
                            NetworkTask.TaskTypeEnum.ADBGetRomInfo,
                            new string[] { currentRom.Key }, false)))
                        {
                            ShowMessage("Contacting ArcadeDB: please wait...", true);
                            SwitchMenu(null);
                        }
                        else
                            SwitchMenu("Community Edition: no ADB history, sorry :)", "show_game_info", string.Empty);
                        break;
                    }
                    if (menuKey.StartsWith("update_feel_ok_"))
                    {
                        ShowMessage("Downloading FEEL...\n0%", true);
                        var download_url = menuKey.Substring("update_feel_ok_".Length);
                        networkHelper.EnqueueTask(new NetworkTask(
                            NetworkTask.TaskTypeEnum.FeelUpdateDownload,
                            new string[] { objConfig.feel_uuid, download_url }, false));
                        SwitchMenu(null);
                        sfxConfirm.Play();
                        break;
                    }
                    if (menuKey == "enter_fnet")
                    {
                        // update list order (move selection to home)
                        if (
                            networkHelper.EnqueueTask(
                            new NetworkTask(
                                objConfig.fnet_sort == FnetSortType.LastPlayed ? NetworkTask.TaskTypeEnum.LastPlayedList : NetworkTask.TaskTypeEnum.MostPlayedList,
                                new string[] { true.ToString() },
                                false)) &&
                            networkHelper.EnqueueTask(
                            new NetworkTask(NetworkTask.TaskTypeEnum.UserNotifications,
                                new string[] { fnetLastUpdate.ToString("yyyy-MM-dd H:mm:ss") },
                                true))
                            )
                        {
                            runFnet = fnetMessage.Visible = true;
                            ShowMessage("Initializing FNET: please wait...", true);
                            SwitchMenu(null);
                            sfxConfirm.Play();
                        }
                        else
                        {
                            ShowToast("No FNET on Community Edition: sorry :)", true);
                        }
                        break;
                    }
                    if (menuKey == "exit_fnet")
                    {
                        runFnet = fnetMessage.Visible = false;
                        romList.IsFnetRunning = false;
                        SelectGameList(objConfig.current_gamelist);
                        currentRom = romList.ChangeSort(objConfig.current_sort);
                        romList.UpdateSelection();
                        romCounter.Text = "Game " + romList.SelectedIndex + " of " + romList.ItemsCount;
                        romList.UpdateStats();
                        SwitchMenu(null);
                        sfxConfirm.Play();
                        break;
                    }
                    if (menuKey.StartsWith("hint_empty_list_ok"))
                    {
                        _machineState.State = MachineState.StateEnum.BuildGameList;
                        SwitchMenu(null);
                        break;
                    }
                    break;
                }
            }

            if (objInput.KeyState(objConfig.menu_key) == InputState.KeyPress || objInput.KeyState(objConfig.menu_cancel) == InputState.KeyPress)
            {
                while (true)
                {
                    if (currentMenu == "select_platform") { SwitchMenu(null, shortCut ? null : "menu", currentMenu); shortCut = false; break; }
                    if (currentMenu == "select_emulator") { SwitchMenu(null, shortCut ? null : "menu", currentMenu); shortCut = false; break; }
                    if (currentMenu == "select_gamelist") { SwitchMenu(null, shortCut ? null : "menu", currentMenu); shortCut = false; break; }
                    if (currentMenu == "select_gamelist") { SwitchMenu(null, shortCut ? null : "menu", currentMenu); shortCut = false; break; }
                    if (currentMenu.StartsWith("update_feel_")) { SwitchMenu(null, "menu", currentMenu); shortCut = false; break; }
                    if (currentMenu == "find_game") { SwitchMenu(null, shortCut ? null : "menu", currentMenu); break; }
                    if (currentMenu == "options") { SwitchMenu(null, "menu", currentMenu); break; }
                    if (currentMenu == "show_feel_settings") { SwitchMenu(null, "menu", currentMenu); break; }
                    if (currentMenu == "about") { SwitchMenu(null, "menu", currentMenu); break; }
                    if (currentMenu == "exit") { SwitchMenu(null, shortCut ? null : "menu", currentMenu); shortCut = false; break; }
                    if (currentMenu == "show_game_info") { SwitchMenu(null, shortCut ? null : "menu", "show_game_info_dl"); shortCut = false; break; }
                    if (currentMenu == "select_layout") { SwitchMenu(null, shortCut ? null : "options", currentMenu); shortCut = false; break; }
                    if (currentMenu == "change_sort") { SwitchMenu(null, "options", currentMenu); break; }
                    if (currentMenu == "reset_game_stats") { SwitchMenu(null, "options", currentMenu); break; }
                    if (currentMenu == "build_list") { SwitchMenu(null, "options", currentMenu); break; }
                    if (currentMenu == "add_game_to_list") { SwitchMenu(null, shortCut ? null : "options", currentMenu); shortCut = false; break; }
                    if (currentMenu == "new_gamelist") { SwitchMenu(null, shortCut ? null : "options", currentMenu); shortCut = false; break; }
                    if (currentMenu == "changelog") { SwitchMenu(null, shortCut ? null : "options", currentMenu); shortCut = false; break; }
                    if (currentMenu == "fnet_select_nickname") { SwitchMenu(null, "exit_fnet"); shortCut = false; break; }

                    SwitchMenu(null);
                    break;
                }
                preselectedMenuKey = string.Empty;
            }

            if (objInput.KeyState(objConfig.show_exit_menu) == InputState.KeyPress)
            {
                sfxMenu.Play();
                SwitchMenu(null);
            }

            if (keyboardScrolling)
            {
                // anti-bounce
                if (keyboardTickCount <= TickCount)
                    keyboardScrolling = false;
            }
            else
            {
                // keyboard repeat unramping
                if (objInput.NoEvent)
                {
                    if (current_keyboard_scroll_rate < objConfig.keyboard_scroll_rate)
                        current_keyboard_scroll_rate += 10;
                    else
                        current_keyboard_scroll_rate = objConfig.keyboard_scroll_rate;
                }

                if (objInput.KeyState(objConfig.menu_up) == InputState.KeyDown) { if (objMenu.SelectMenuUpItem()) { sfxMenu.Play(); } keyboardScrolling = true; }
                if (objInput.KeyState(objConfig.menu_down) == InputState.KeyDown) { if (objMenu.SelectMenuDownItem()) { sfxMenu.Play(); } keyboardScrolling = true; }
                if (objInput.KeyState(objConfig.menu_left) == InputState.KeyDown) { if (objMenu.SelectMenuLeftItem()) { sfxMenu.Play(); } keyboardScrolling = true; }
                if (objInput.KeyState(objConfig.menu_right) == InputState.KeyDown) { if (objMenu.SelectMenuRightItem()) { sfxMenu.Play(); } keyboardScrolling = true; }
                
                // in menu repeat time is not ramping
                //if (keyboardScrolling)
                //    keyboardTickCount = TickCount + objConfig.keyboard_scroll_rate;
                if (keyboardScrolling)
                {
                    _machineState.MenuKey = objMenu.MenuItemSelected.Key;
                    keyboardTickCount = TickCount + current_keyboard_scroll_rate;
                    if (current_keyboard_scroll_rate > objConfig.keyboard_min_scroll_rate)
                        current_keyboard_scroll_rate -= 10;
                    else
                        current_keyboard_scroll_rate = objConfig.keyboard_min_scroll_rate;
                }

                if (objConfig.use_mouse != UseMouse.No)
                {
                    if (!mouseScrolling)
                    {
                        if (objConfig.use_mouse == UseMouse.XAxis)
                        {
                            if (objInput.MouseMoveLeft) { objMenu.SelectMenuUpItem(); mouseScrolling = true; }
                            if (objInput.MouseMoveRight) { objMenu.SelectMenuDownItem(); mouseScrolling = true; }
                        }
                        else
                        {
                            if (objInput.MouseMoveUp) { objMenu.SelectMenuUpItem(); mouseScrolling = true; }
                            if (objInput.MouseMoveDown) { objMenu.SelectMenuDownItem(); mouseScrolling = true; }
                        }

                        if (mouseScrolling)
                            mouseTickCount = TickCount + objConfig.mouse_scroll_rate;
                    }
                    else
                    {
                        if (mouseTickCount <= TickCount && mouseScrolling)
                            mouseScrolling = false;
                    }
                }
            }
        }
        #endregion

        #region Menu Management
        private void SwitchMenu(string newMenu)
        {
            SwitchMenu(null, newMenu, string.Empty);
        }

        public void SwitchMenu(string result, string newMenu)
        {
            SwitchMenu(result, newMenu, string.Empty);
        }

        public void SwitchMenu(string result, string newMenu, string preselectedKey)
        {
            var additionalData = string.Empty;
            var additionalInfo = string.Empty;
            preselectedMenuKey = preselectedKey;
            current_keyboard_scroll_rate = objConfig.keyboard_scroll_rate;

            var isTextReader = false;
            CLedManager.SetFECommandLeds(objConfig.frontend_menu_controls, CLedManager.FETransition.MenuCommands);

            // update Command image
            UpdateCommandStatus(CStatusAnimatedImage.enStatus.CommandMenu);

            if (string.IsNullOrEmpty(newMenu))
            {
                objMenu.HideMenu(false);
                _machineState.Menu = _machineState.MenuKey = currentMenu = null;
                
                // reset focus on list
                romList.SetFocus();

                if (!objMenu.MessageShown && objScene.videosnapVideo != null)
                    objScene.videosnapVideo.Resume();

                // update LEDs for controls
                if (currentRom != null)
                    CLedManager.SetFECommandLeds(objConfig.frontend_list_controls, CLedManager.FETransition.ListCommands);

                // update Command image
                UpdateCommandStatus(CStatusAnimatedImage.enStatus.Idle);
            }
            else
            {
                _asyncTaskWait = false;
                objMenu.HideMessage(true);

                var windowTitle = string.Empty;
                var windowStatusbarText = string.Empty;
                if (objConfig.show_clock)
                    windowStatusbarText = "h. " + DateTime.Now.Hour.ToString("00") + "." + DateTime.Now.Minute.ToString("00");

                List<List<String>> menuCols = SwitchMenuFeel(ref windowTitle, ref windowStatusbarText, ref isTextReader,
                    ref additionalData, ref additionalInfo,
                    result, newMenu, preselectedKey);

                if (menuCols.Count > 0 && menuCols[0].Count > 0)
                {
                    var snapshotPath =
                        objConfig.current_platform == "all_emu" ?
                            (currentRom != null ? currentRom.FeelInfo.SnapshotPath : string.Empty) :
                            objConfig.snapshot_path;
                    var workingDir = string.Empty;
                    snapshotPath = ParseArguments(objConfig, snapshotPath, false, ref workingDir);
                    objMenu.ShowMenu(menuCols, windowTitle, windowStatusbarText, newMenu, additionalData, additionalInfo, currentMenu != newMenu,
                        preselectedMenuKey, isTextReader, snapshotPath,
                        Application.StartupPath + Path.DirectorySeparatorChar + "layouts" + Path.DirectorySeparatorChar +
                        objConfig.current_layout + Path.DirectorySeparatorChar + "icons" + Path.DirectorySeparatorChar, objConfig.menu_show_sidebar);
                    
                    // play menu sound if menu changed
                    if (currentMenu != newMenu)
                        sfxMenu.Play();

                    _machineState.Menu = currentMenu = newMenu;
                    _machineState.MenuKey = objMenu.MenuItemSelected.Key;
                }
            }
        }

        public void AddNotification(string text, DateTime publishDate)
        {
            fnetMessage.AddMessage(text, publishDate);
            fnetLastUpdate = publishDate;
        }

        public void UpdateExternalRomListData(bool selectFirstRom)
        {
            romList.ResetStats();
            var iLoop = 0;
            foreach (var romStat in networkHelper.RomStatList)
            {
                var rom = romList.List.Find(c => c.Key == romStat.game_id);
                if (rom != null)
                {
                    rom.ExternalSortKey = iLoop;
                    rom.PlayedCount = romStat.total_playing_counter;
                    rom.TimeStamp = romStat.last_played_on;
                    rom.PlayedTimeNow = rom.PlayedTime = new TimeSpan((int)Math.Floor(romStat.total_mins / 60), (int)Math.Floor(romStat.total_mins % 60.0f), (int)((romStat.total_mins - Math.Truncate(romStat.total_mins)) * 60.0f));
                }
                iLoop++;
            }

            romList.IsFnetRunning = true;
            romList.ChangeSort(SortType.ExternalKey);
            if (selectFirstRom)
                currentRom = romList.SelectFirstItem();
            else
                romList.UpdateSelection();
            romList.UpdateStats();
            romCounter.Text = "Game " + romList.SelectedIndex + " of " + romList.ItemsCount;
        }

        private List<List<String>> SwitchMenuFeel(ref string windowTitle, ref string windowStatusbarText, ref bool isTextReader,
            ref string additionalData, ref string additionalInfo,
            string result, string newMenu, string preselectedKey)
        {
            List<List<string>> menuCols = new List<List<string>>(); ;
            while (true)
            {
                List<string> menuRows = new List<string>(); ;
                menuCols.Add(menuRows);

                if (!runFnet && newMenu == "menu")
                {
                    windowTitle = "FEEL - Menu";
                    //if (objConfig.list_type == ListType.mame_listinfo || objConfig.list_type == ListType.mame_xml_list)
                    //    menuRows.Add("enter_fnet|Enter F.NET");
                    if (currentRom != null)
                    {
                        menuRows.Add("start_game|Start game");
                    }
                    else
                    {
//                        if (objConfig.current_gamelist.Equals(objConfig.current_emulator + "-0", StringComparison.CurrentCultureIgnoreCase))
                            menuRows.Add("build_list|Build List");
                    }
                    if (platformList.Count > 1)
                        menuRows.Add("select_platform|" + (objConfig.current_platform != "all_emu" ? "Select platform" : "[BACK TO NORMAL LIST]"));
                    if (platformList.Count == 1 && objConfig.current_platform == "all_emu")
                        menuRows.Add("select_platform_" + platformList[0].Split('|')[0] + "|[BACK TO NORMAL LIST]");
                    if (emulatorList.Count > 1 && objConfig.current_platform != "all_emu")
                        menuRows.Add("select_emulator|Select emulator");
                    if (gameListList.Count > 1 && objConfig.current_platform != "all_emu")
                        menuRows.Add("select_gamelist|Select gamelist");
                    if (objConfig.current_platform != "all_emu")
                        menuRows.Add("select_platform_all_emu|Select * TOP GAMES *");
                    if (currentRom != null)
                    {
                        menuRows.Add("find_game|Find game" + (searchString != string.Empty ? " [ " + (objConfig.fulltext_search ? "*" : "") + searchString + "* ]" : string.Empty));
                        menuRows.Add("show_game_info_dl|Show game info");
                    }
                    if (objScene.MultipleFilesMusic)
                    {
                        menuRows.Add("next_track|Audio track >>");
                    }
                    if ((!string.IsNullOrEmpty(objConfig.current_emulator) || objConfig.current_platform == "all_emu")
                        && !string.IsNullOrEmpty(objConfig.rom_path) && !string.IsNullOrEmpty(objConfig.rom_extension))
                    {
                        menuRows.Add("|Tools|");
                        menuRows.Add("options|Options");
                    }
                    if (objConfig.test_mode || objConfig.service_mode)
                    {
                        menuRows.Add("|Service menu|");
                        menuRows.Add("reload_configuration|Reload configuration");
                        menuRows.Add("show_feel_settings|Show FEEL parameters");
                        menuRows.Add("feel_update|Check for updates");
                    }
                    menuRows.Add("|Others|");
                    menuRows.Add("about|About");
                    if (objConfig.exit_from_menu_enabled)
                        menuRows.Add("exit|Exit");
                    break;
                }
                else if (runFnet && newMenu == "menu")
                {
                    windowTitle = "F.NET - Menu";
                    menuRows.Add("exit_fnet|Exit F.NET");
                    if (currentRom != null)
                    {
                        menuRows.Add("find_game|Find game" + (searchString != string.Empty ? " [ " + (objConfig.fulltext_search ? "*" : "") + searchString + "* ]" : string.Empty));
                        menuRows.Add("show_game_info_dl|Show game info");
                    }
                    menuRows.Add("|F.Net|");
                    menuRows.Add("fnet_change_sort|Order by " + (objConfig.fnet_sort == FnetSortType.LastPlayed ? "[last played]" : "[most played]"));
                    menuRows.Add("show_chat|Show chat");
                    menuRows.Add("|Others|");
                    menuRows.Add("about|About");
                    menuRows.Add("exit|Exit");
                    break;
                }
                if (newMenu == "fnet_select_nickname")
                {
                    windowTitle = "Select FNET nickname";
                    additionalInfo = "Please choose your FNET nickname.";//\nBe careful: it can't be changed after!";
                    AddQwertyMenu("fnet_select_nickname_", menuCols);
                    windowStatusbarText = "[ " + networkHelper.CurrentSession.nickname + " ]   Action = set";
                    break;
                }
                if (newMenu == "select_platform")
                {
                    windowTitle = "Select platform";
                    foreach (var platform in platformList)
                    {
                        //if (item != current_platform)
                        menuRows.Add("select_platform_" + platform);
                    }
                    preselectedMenuKey = "select_platform_" + objConfig.current_platform;
                    break;
                }
                if (newMenu == "select_emulator")
                {
                    windowTitle = "Select emulator";
                    foreach (var emulator in emulatorList)
                    {
                        //if (item != current_emulator + "|" + emulator_title)
                        menuRows.Add("select_emulator_" + emulator);
                    }
                    preselectedMenuKey = "select_emulator_" + objConfig.current_emulator;
                    break;
                }
                if (newMenu == "select_gamelist")
                {
                    windowTitle = "Select gamelist";
                    foreach (var gameList in gameListList)
                    {
                        //if (item != current_list + "|" + list_title)
                        menuRows.Add("select_gamelist_" + gameList);
                    }
                    preselectedMenuKey = "select_gamelist_" + objConfig.current_gamelist;
                    break;
                }
                if (newMenu == "options")
                {
                    windowTitle = "Options";
                    newGamelistName = string.Empty;
                    if (currentRom != null)
                    {
                        menuRows.Add("select_random_game|Select random game");
                        var list = gameListList.FindAll(c => !c.StartsWith(objConfig.current_gamelist) && !c.StartsWith(objConfig.current_emulator + "-0"));
                        if (list.Count > 0 || objConfig.current_platform != "all_emu")
                            menuRows.Add("add_game_to_list|Add game to list");
                        if (objConfig.current_gamelist != objConfig.current_emulator + "-0" || objConfig.current_platform == "all_emu")
                            menuRows.Add("remove_game_from_list|Remove game from list");
                        if (currentRom.PlayedTime > TimeSpan.Zero)
                            menuRows.Add("reset_game_stats|Reset game stats");
                    }
                    if (romList.ItemsCount > 0)
                    {
                        menuRows.Add("change_sort|Set order");
                        menuRows.Add("fulltext_search|Full-text search [" + (objConfig.fulltext_search ? "X" : "-") + "]");

                        var autoStartModeItem = "";
                        switch (objConfig.autostart_mode)
                        {
                            case AutostartMode.LastPlayed: autoStartModeItem = "last played"; break;
                            case AutostartMode.LastSelected: autoStartModeItem = "last selected"; break;
                            case AutostartMode.SingleGame:
                                if (currentRom != null)
                                    autoStartModeItem = "mono-game";
                                else
                                {
                                    objConfig.SetParameter("autostart_mode", "0");
                                    autoStartModeItem = "-";
                                }
                                break;
                            default: autoStartModeItem = "-"; break;
                        }
                        menuRows.Add("change_autostart|Auto-start [" + autoStartModeItem + "]");
                        if (objConfig.autostart_mode == AutostartMode.SingleGame)
                        {
                            var currentGC = "";
                            if (currentRom != null)
                                currentGC = new GameRunChain(objConfig.current_platform, objConfig.current_emulator, objConfig.current_gamelist, currentRom.Key).ToString();
                            if (objConfig.autostart_single_game == "")
                            {
                                // new parameter: default to current game
                                objConfig.SetParameter("autostart_single_game", currentGC);
                            }
                            if (!string.IsNullOrEmpty(objConfig.autostart_single_game))
                            {
                                menuRows.Add("autostart_choose_game|    >> " + new GameRunChain(objConfig.autostart_single_game).Game + "");
                            }
                        }
                    }
                    if (!objConfig.test_mode && objConfig.service_mode_available)
                    {
                        menuRows.Add("service_mode|Service menu [" + (objConfig.service_mode ? "X" : "-") + "]");
                    }
                    if (objConfig.test_mode)
                        menuRows.Add("service_mode_available|Service menu (fullscreen) [" + (objConfig.service_mode_available ? "X" : "-") + "]");
                    if (objConfig.test_mode || objConfig.service_mode)
                    {
                        var screenSaverTypeItem = "";
                        switch (objConfig.screen_saver_enabled)
                        {
                            case ScreenSaverType.Slideshow: screenSaverTypeItem = "slideshow"; break;
                            case ScreenSaverType.StartRandomGame: screenSaverTypeItem = "start random game"; break;
                            default: screenSaverTypeItem = "-"; break;
                        }

                        menuRows.AddRange(new string[] {
                            "|Service menu|",
                            "build_list|" + (currentRom != null ? "Reb" : "B") + "uild List",
                            "select_layout|Select layout",
                            "new_gamelist|New gamelist",
                            "show_clones|Show clones [" + (objConfig.show_clones ? "X" : "-") + "]",
                            "cleanup_names|Cleanup names [" + (objConfig.cleanup_names ? "X" : "-") + "]",
                            "show_stars|Show ranking stars [" + (objConfig.romlist_disable_stars ? "-" : "X") + "]",
                            //"menu_show_sidebar|Show menu sidebar [" + (objConfig.menu_show_sidebar ? "X" : "-") + "]",
                            "show_clock|Show clock [" + (objConfig.show_clock ? "X" : "-") + "]",
                            "show_search_key|Show search key hint [" + (objConfig.show_search_key ? "X" : "-") + "]",
                            //"show_extended_messages|Show extended messages [" + (objConfig.show_extended_messages ? "X" : "-") + "]",
                            "change_screensaver|Screensaver [" + screenSaverTypeItem + "]",
                            "restore_explorer_on_exit|Restore Explorer on exit [" + (objConfig.restore_explorer_at_exit ? "X" : "-") + "]",
                            "update_beta|Beta version upgrades [" + (objConfig.update_beta ? "X" : "-") + "]",
                            "changelog|View changelog"
                        });
                    }
                    break;
                }
                if (newMenu == "reset_game_stats")
                {
                    windowTitle = "Reset game stats?";
                    menuRows.Add("reset_game_stats_ok|PROCEED");
                    menuRows.Add("reset_game_stats_cancel|Cancel");
                    break;
                }
                if (newMenu == "build_list")
                {
                    if (objConfig.current_gamelist.Equals(objConfig.current_emulator + "-0", StringComparison.CurrentCultureIgnoreCase))
                    {
                        windowTitle = (currentRom != null ? "Rebuild" : "Build") + " games list?";
                        menuRows.Add("build_list_ok|PROCEED");
                        menuRows.Add("build_list_cancel|Cancel");
                    }
                    else
                    {
                        ShowMessage("List build is available only on \"All games\" lists.\n\nTo add games here, please select another list and use \n\"Add game to list\"\n option from there.", false);
                        SwitchMenu(null);
                    }
                    break;
                }
                if (newMenu == "change_sort")
                {
                    windowTitle = "Order by";
                    var iLoop = 0;
                    foreach (SortType sortType in Enum.GetValues(typeof(SortType)))
                    {
                        if (sortType != SortType.ExternalKey)
                        {
                            menuRows.Add("change_sort_" + iLoop + "|" + CListBox.SortTypeToDesc(sortType));
                            iLoop++;
                        }
                    }
                    preselectedMenuKey = "change_sort_" + (int)objConfig.current_sort;
                    break;
                }
                if (newMenu == "add_game_to_list")
                {
                    windowTitle = "Add game to";
                    var list = gameListList.FindAll(c => c.Split('|')[0] != objConfig.current_gamelist && !c.StartsWith(objConfig.current_emulator + "-0"));
                    foreach (var item in list)
                        menuRows.Add("add_game_to_list_" + item);
                    menuRows.Add("add_game_to_list_all_emu|\"TOP GAMES\"");
                    break;
                }
                if (newMenu == "about")
                {
                    windowTitle = "F.E.(E.L.) - FrontEnd (Emulator Launcher)";
                    menuRows.Add("|CONCEPT & DEVELOPMENT|");
                    menuRows.Add("menu_close|FEELTeam / dr.prodigy");
                    menuRows.Add("|WEBSITE                            |");
                    menuRows.Add("menu_close|http://feelfrontend.altervista.net");
                    menuRows.Add("menu_close|https://github.com/dr-prodigy/feel-frontend-ce");
                    menuRows.Add("|special thanks to|");
                    menuRows.Add("menu_close|antogeno24 (initial concept)");
                    menuRows.Add("menu_close|adolfo69 (themes design)");
                    menuRows.Add("menu_close|ArcadeItalia community ( http://www.arcadeitalia.net )");
                    menuRows.Add("menu_close|picerno/SmartASD ( http://adb.arcadeitalia.net )");
                    menuRows.Add("menu_close|motoschifo/ArcadeDB ( http://adb.arcadeitalia.net )");
                    menuRows.Add("|");
                    menuRows.Add("|INFO|");
                    menuRows.Add("menu_close|v. " + Application.ProductVersion + "   (c) 2011-2017");
                    menuRows.Add("menu_close|FEEL is free software: refer to doc for info.");
                    isTextReader = true;
                    CLedManager.StartAboutBoxProgram();
                    break;
                }
                if (newMenu == "exit")
                {
                    windowTitle = "Exit";
                    if (!objConfig.exit_to_windows_menu_enabled && !objConfig.restart_windows_menu_enabled && !objConfig.shut_down_menu_enabled && !objConfig.standby_menu_enabled && !objConfig.hibernate_menu_enabled)
                        objConfig.SetParameter("exit_to_windows_menu_enabled", "1");
                    if (objConfig.exit_to_windows_menu_enabled)
                        menuRows.Add("exit_to_windows|Exit to windows");
                    if (objConfig.restart_windows_menu_enabled)
                        menuRows.Add("restart|Restart");
                    if (objConfig.shut_down_menu_enabled)
                        menuRows.Add("shut_down|Shutdown");
                    if (objConfig.standby_menu_enabled)
                        menuRows.Add("standby|Standby");
                    if (objConfig.hibernate_menu_enabled)
                        menuRows.Add("hibernate|Hibernate");
                    break;
                }
                if (newMenu == "find_game")
                {
                    windowTitle = "Find game";
                    AddQwertyMenu("find_game_", menuCols);
                    windowStatusbarText = "[ " + (objConfig.fulltext_search ? "*" : "") + searchString + "* ]   Action = toggle fulltext";
                    break;
                }

                if (newMenu == "show_game_info_dl")
                {
                    if (networkHelper.EnqueueTask(new NetworkTask(
                        NetworkTask.TaskTypeEnum.ADBGetRomInfo,
                        new string[] { currentRom.Key }, false)))
                        {
                            ShowMessage("Contacting ArcadeDB: please wait...", true);
                            SwitchMenu(null);
                        }
                        else
                            SwitchMenu("Community Edition: no ADB history, sorry :)", "show_game_info", string.Empty);
                    break;
                }
                
                // options
                if (newMenu == "new_gamelist")
                {
                    windowTitle = "New gamelist";
                    AddQwertyMenu("new_gamelist_", menuCols);
                    windowStatusbarText = "[ " + newGamelistName + " ]   Action = create";
                    break;
                }
                if (newMenu == "select_layout")
                {
                    windowTitle = "Select layout";
                    var lays = Directory.GetDirectories(Application.StartupPath + Path.DirectorySeparatorChar + "layouts");
                    foreach (var lay in lays)
                    {
                        var index = lay.LastIndexOf("" + Path.DirectorySeparatorChar );
                        if (index > 0)
                        {
                            var name = lay.Substring(index + 1);
                            menuRows.Add("select_layout_" + name + "|" + name);
                        }
                    }
                    preselectedMenuKey = "select_layout_" + objConfig.current_layout;
                    break;
                }
                if (newMenu == "show_feel_settings")
                {
                    windowTitle = "FEEL parameters";
                    menuRows.AddRange(new string[] {
                        "|General settings|",
                        "menu_close|*#*TL*#*Screen res: §" + objConfig.screen_res_x + "x" + objConfig.screen_res_y,
                        "|General commands|",
                        "menu_close|Action: §" + objConfig.action_key,
                        "menu_close|Menu: §" + objConfig.menu_key,
                        "menu_close|OK: §" + objConfig.menu_ok,
                        "|Browse commands|",
                        "menu_close|Game (prev/next): §" + objConfig.previous_game + "/" + objConfig.next_game,
                        "menu_close|Letter (prev/next): §" + objConfig.previous_letter_game + "/" + objConfig.next_letter_game,
                        "menu_close|Page (prev/next): §" + objConfig.previous_page + "/" + objConfig.next_page,
                        "menu_close|Menu (l/r/u/d): §" + objConfig.menu_left + "/" + objConfig.menu_right + "/" + objConfig.menu_up + "/" + objConfig.menu_down,
                        "menu_close|Platform (prev/next/sel): §" + objConfig.previous_platform + "/" + objConfig.next_platform + "/" + objConfig.select_platform,
                        "menu_close|Emulator (prev/next/sel): §" + objConfig.previous_emulator + "/" + objConfig.next_emulator + "/" + objConfig.select_emulator,
                        "menu_close|Gamelist (prev/next/sel): §" + objConfig.previous_gamelist + "/" + objConfig.next_gamelist + "/" + objConfig.select_gamelist,
                        "menu_close|Find game: §" + objConfig.find_game,
                        "menu_close|Add game to list: §" + objConfig.add_game_to_list,
                        "menu_close|Remove game from list: §" + objConfig.remove_game_from_list,
                        "|Other commands|",
                        "menu_close|Show custom image: §" + objConfig.show_custom_image,
                        "menu_close|Change order: §" + objConfig.next_sort,
                        "|Screensaver|",
                        "menu_close|Screensaver choose game: §" + objConfig.screensaver_previous_game + "/" + objConfig.screensaver_next_game,
                        "|Exit commands|",
                        "menu_close|Exit menu: §" + objConfig.show_exit_menu,
                        "|Emulator settings|",
                        "menu_close|Emulator path: §" + Utils.ShortenString(objConfig.emulator_path, 35),
                        "menu_close|Emulator command: §" + Utils.ShortenString(objConfig.emulator_commandline + " " + objConfig.emulator_arguments, 35),
                        "menu_close|Rom path / ext: §" + Utils.ShortenString(objConfig.rom_path, 25) + " / " + objConfig.rom_extension,
                        "menu_close|Snapshot path / ext: §" + Utils.ShortenString(objConfig.snapshot_path, 25) + " / " + objConfig.snapshot_extension,
                        "menu_close|Video path: §" + Utils.ShortenString(objConfig.video_path, 35),
                        "menu_close|Marquee path / ext: §" + Utils.ShortenString(objConfig.marquee_path, 25) + " / " + objConfig.marquee_extension,
                        "menu_close|Cabinet path / ext: §" + Utils.ShortenString(objConfig.cabinet_path, 25) + " / " + objConfig.cabinet_extension + "*#*/T*#*"
                    });
                    isTextReader = true;
                    CLedManager.StartAboutBoxProgram();

                    // Parameter dump
                    //foreach (var message in objConfig.ParameterDump())
                    //    MessageBox.Show(message);
                    
                    break;
                }
                if (newMenu == "changelog")
                {
                    windowTitle = "FEEL v. " + Application.ProductVersion + " - changelog";
                    isTextReader = true;
                    foreach (var line in CFileManager.GetChangelog(
                        (int)(objConfig.screen_res_x * .8),
                        Utils.LoadSpriteFont(this, objConfig.menu_font_name, objConfig.menu_font_size, objConfig.menu_font_style)
                        ).Split('\n'))
                    {
                        menuRows.Add("menu_close|" + line);
                    }
                    break;
                }
                if (newMenu.StartsWith("update_feel_"))
                {
                    var url = newMenu.Substring("update_feel_".Length);
                    windowTitle = "FEEL v." + result + " available";
                    menuRows.Add("|Would you like to update?|");
                    menuRows.Add("update_feel_ok_" + url + "|Yes");
                    menuRows.Add("menu_close|Not now");
                    break;
                }
                if (newMenu == "show_game_info")
                {
                    CLedManager.SetInputControlLeds(currentRom.InputControl);

                    var history = result;
                    var romName = objConfig.current_platform == "all_emu" ? currentRom.FeelInfo.RomName : currentRom.Key;
                    additionalData = romName;
                    windowTitle = (runFnet ? "F.NET stats - " : "Stats - ") + (currentRom.Description != string.Empty ? currentRom.Description + " - " : "") + "[" + Utils.ShortenString(romName, 20) + "]";
                    isTextReader = true;

                    var hiscore = string.Empty;
                    if (!runFnet) hiscore = CRomManager.GetGameHiscore(objConfig, currentRom);
                    if (!string.IsNullOrEmpty(hiscore))
                    {
                        menuRows.Add("menu_close|");
                        foreach (var line in hiscore.Split('\n'))
                        {
                            menuRows.Add("menu_close|" + line);
                        }
                    }

                    var sf = Utils.LoadSpriteFont(this, objConfig.menu_font_name, objConfig.menu_font_size, objConfig.menu_font_style);
                    if (history != null)
                        menuRows.Add("menu_close|");
                    foreach (var line in CFileManager.Justify(history,
                        (int)(objConfig.screen_res_x * .8),
                        sf).Split('\n'))
                    {
                        menuRows.Add("menu_close|" + line);
                    }

                    additionalInfo = CRomManager.GetGameInfo(objConfig, currentRom, romList);
                    
                    break;
                }

                if (newMenu == "hint_empty_list")
                {
                    windowTitle = "Empty gamelist";
                    menuRows.Add("|Would you like to run build?|");
                    menuRows.Add("hint_empty_list_ok|Yes");
                    menuRows.Add("menu_close|Not now");
                }

                break;
            }
            return menuCols;
        }

        private List<List<String>> SwitchMenuFnet(ref string windowTitle, ref string windowStatusbarText, ref bool isTextReader,
            ref string additionalData, ref string additionalInfo,
            string result, string newMenu, string preselectedKey)
        {
            List<List<string>> menuCols = new List<List<string>>(); ;
            while (true)
            {
                List<string> menuRows = new List<string>(); ;
                menuCols.Add(menuRows);

                break;
            }
            return menuCols;
        }

        private void AddQwertyMenu (string rootCommand, List<List<string>> menuCols)
        {
                menuCols[0].Add(rootCommand + "1|1");
                menuCols[0].Add(rootCommand + "Q|Q");
                menuCols[0].Add(rootCommand + "A|A");
                menuCols[0].Add(rootCommand + "Z|Z");
                menuCols[0].Add(rootCommand + "!|!");
                menuCols.Add(new List<string>());
                menuCols[1].Add(rootCommand + "2|2");
                menuCols[1].Add(rootCommand + "W|W");
                menuCols[1].Add(rootCommand + "S|S");
                menuCols[1].Add(rootCommand + "X|X");
                menuCols[1].Add(rootCommand + "&|&");
                menuCols.Add(new List<string>());
                menuCols[2].Add(rootCommand + "3|3");
                menuCols[2].Add(rootCommand + "E|E");
                menuCols[2].Add(rootCommand + "D|D");
                menuCols[2].Add(rootCommand + "C|C");
                menuCols[2].Add(rootCommand + "/|/");
                menuCols.Add(new List<string>());
                menuCols[3].Add(rootCommand + "4|4");
                menuCols[3].Add(rootCommand + "R|R");
                menuCols[3].Add(rootCommand + "F|F");
                menuCols[3].Add(rootCommand + "V|V");
                menuCols[3].Add(rootCommand + "(|(");
                menuCols.Add(new List<string>());
                menuCols[4].Add(rootCommand + "5|5");
                menuCols[4].Add(rootCommand + "T|T");
                menuCols[4].Add(rootCommand + "G|G");
                menuCols[4].Add(rootCommand + "B|B");
                menuCols[4].Add(rootCommand + "_|_");
                menuCols.Add(new List<string>());
                menuCols[5].Add(rootCommand + "6|6");
                menuCols[5].Add(rootCommand + "Y|Y");
                menuCols[5].Add(rootCommand + "H|H");
                menuCols[5].Add(rootCommand + "N|N");
                menuCols[5].Add(rootCommand + "_|_");
                menuCols.Add(new List<string>());
                menuCols[6].Add(rootCommand + "7|7");
                menuCols[6].Add(rootCommand + "U|U");
                menuCols[6].Add(rootCommand + "J|J");
                menuCols[6].Add(rootCommand + "M|M");
                menuCols[6].Add(rootCommand + ")|)");
                menuCols.Add(new List<string>());
                menuCols[7].Add(rootCommand + "8|8");
                menuCols[7].Add(rootCommand + "I|I");
                menuCols[7].Add(rootCommand + "K|K");
                menuCols[7].Add(rootCommand + ",|,");
                menuCols[7].Add(rootCommand + "?|?");
                menuCols.Add(new List<string>());
                menuCols[8].Add(rootCommand + "9|9");
                menuCols[8].Add(rootCommand + "O|O");
                menuCols[8].Add(rootCommand + "L|L");
                menuCols[8].Add(rootCommand + ".|.");
                menuCols[8].Add(rootCommand + "'|'");
                menuCols.Add(new List<string>());
                menuCols[9].Add(rootCommand + "0|0");
                menuCols[9].Add(rootCommand + "P|P");
                menuCols[9].Add(rootCommand + "+|+");
                menuCols[9].Add(rootCommand + "-|-");
                menuCols[9].Add(rootCommand + "delete|«");
        }

        private void ShowUserHints()
        {
            if (objConfig.current_gamelist == objConfig.current_emulator + "-0" && objConfig.current_platform != "all_emu" && currentRom == null)
            {
                SwitchMenu("hint_empty_list");
            }
        }
        #endregion 

        #region Rom Management
        private bool _isBuildTaskRunning = false;
        private void BuildGameList()
        {
            try
            {
                if (!_isBuildTaskRunning)
                {
                    ShowMessage("Please wait...", true, true);
                    _isBuildTaskRunning = true;
                    Utils.RunAsynchronously(() =>
                    {
                        var dummy = "";
                        var command = ParseArguments(objConfig, objConfig.emulator_commandline, true, ref dummy);
                        var romPath = ParseArguments(objConfig, objConfig.rom_path, true, ref dummy);

                        if (objConfig.list_type == ListType.mame_xml_list ||
                            objConfig.list_type == ListType.mame_listinfo ||
                            objConfig.list_type == ListType.mess_machine)
                            romList.List = CRomManager.BuildListFromMameXML(this, objConfig, command, romPath);
                        else
                            romList.List = CRomManager.BuildListFromPath(this, objConfig, romPath);

                        currentRom = null;
                        CRomManager.SaveRomList(objConfig, romList);
                        if (!objConfig.show_clones && objConfig.current_gamelist == objConfig.current_emulator + "-0")
                        {
                            romList.List = CRomManager.LoadRomList(objConfig);
                            romList.UpdateStats();
                        }
                        if (romList.ItemsCount > 0)
                        {
                            currentRom = romList.SelectItem(1);
                            videoTickCount = TickCount + (objConfig.video_delay * 1000);
                        }
                        _isBuildTaskRunning = false;

                        // restore UI from loading
                        _machineState.State = MachineState.StateEnum.Normal;
                        screenSaverResume = TickCount + 1000;
                        screenSaverTickCount = TickCount + (objConfig.screen_saver_delay * 1000);
                    },
                    () => { /* done */ });
                }
            }
            catch (Exception e)
            {
                throw (e);
            }
        }

        private bool AddGameToList(RomDesc rom)
        {
            return CRomManager.AddGameToList(objConfig, romList, rom);
        }

        private void RemoveGameFromList(RomDesc rom)
        {
            videoTickCount = TickCount + (objConfig.video_delay * 1000);
            currentRom = CRomManager.RemoveGameFromList(objConfig, romList, currentRom);
            SaveConfig();
        }

        private bool SelectPlatform(string platform)
        {
            ClearRomObjects();

            objConfig.RestoreDefaultValuesFromLevel(Levels.PLATFORM_INI);
            objConfig.SetParameter("current_platform", platform);
            var retVal = objConfig.LoadConfig(Levels.PLATFORM_INI);

            if (objConfig.current_platform != "all_emu")
            {
                emulatorList = objConfig.GetEmulatorList();
                gameListList = new List<string>();

                if (emulatorList.Count > 0)
                {
                    if (string.IsNullOrEmpty(objConfig.current_emulator))
                    {
                        var arr = emulatorList[0].Split('|');
                        objConfig.SetParameter("current_emulator", arr[0]);
                    }
                    return retVal & SelectEmulator(objConfig.current_emulator);
                }
                else
                    RefreshMediaObjects();
            }
            else
            {
                return retVal & SelectEmulator(string.Empty);
            }

            return false;
        }

        private bool SelectEmulator(string emulator)
        {
            var retVal = true;
            ClearRomObjects();

            objConfig.RestoreDefaultValuesFromLevel(Levels.EMULATOR_INI);
            objConfig.SetParameter("current_emulator", emulator);
            // for all-emu list, skip emulator config loading
            if (objConfig.current_platform != "all_emu")
            {
                retVal = objConfig.LoadConfig(Levels.EMULATOR_INI);

                if (string.IsNullOrEmpty(objConfig.emulator_list))
                    objConfig.SetParameter("emulator_list", emulator + "-0");

                if (!objConfig.gamelist_list.Contains(emulator))
                    objConfig.SetParameter("gamelist_list", emulator + "-0");

                if (!objConfig.current_gamelist.StartsWith(emulator, StringComparison.CurrentCultureIgnoreCase))
                {
                    var list = objConfig.current_emulator + "-0";
                    objConfig.SetParameter("current_gamelist", list);
                    objConfig.SetParameter("gamelist_title", "All Games");
                    objConfig.SaveConfig();
                }
                gameListList = objConfig.GetGameListList();
            }
            return retVal & SelectGameList(objConfig.current_gamelist);
        }

        private bool SelectGameList(string gameList)
        {
            var retVal = false;
            ClearRomObjects();
            searchString = string.Empty;

            objConfig.RestoreDefaultValuesFromLevel(Levels.GAMELIST_INI);
            objConfig.SetParameter("current_gamelist", gameList);

            // for all-emu list, skip gamelist config loading, but loads list
            if (objConfig.current_platform == "all_emu"
                || objConfig.LoadConfig(Levels.GAMELIST_INI))
            {
                if (romList != null)
                {
                    romList.List = CRomManager.LoadRomList(objConfig);
                    romList.FilterByDescription(searchString, objConfig.fulltext_search);
                    if (romList.ItemsCount > 0)
                    {
                        var index = romList.FindIndexByName(objConfig.current_game, true);
                        if (index == 0)
                        {
                            currentRom = romList.SelectFirstItem();
                            objConfig.SetParameter("current_game", currentRom.Key);
                        }
                        else
                            currentRom = romList.SelectItem(index);
                        romList.StartTransition(CDrawable.Transition.FadeIn);
                        videoTickCount = TickCount + (objConfig.video_delay * 1000);
                        romList.UpdateStats();
                    }
                }
                retVal = true;
            }

            RefreshMediaObjects();

            // start fade in
            StartRomlistTransition(CDrawable.Transition.FadeIn);

            ShowUserHints();

            return retVal;
        }
        #endregion

        #region Config Management
        private void RefreshConfiguration()
        {
            objConfig.LoadConfig();
            objConfig.LoadConfig(Levels.LAYOUT_INI);
            RefreshLayout();
            FixConfigParameters();

            if (!string.IsNullOrEmpty(objConfig.current_platform))
                SelectPlatform(objConfig.current_platform);

            var index = romList.FindIndexByName(objConfig.current_game, true);
            if (index == 0 && currentRom != null)
            {
                currentRom = romList.SelectFirstItem();
                objConfig.SetParameter("current_game", currentRom.Key);
            }
            else
                currentRom = romList.SelectItem(index);

            videoTickCount = TickCount + (objConfig.video_delay * 1000);

            platformList = objConfig.GetPlatformList();
            if (objConfig.current_platform != "all_emu")
            {
                emulatorList = objConfig.GetEmulatorList();
                gameListList = objConfig.GetGameListList();
            }
            else
            {
                emulatorList = new List<string>();
                gameListList = new List<string>();
            }

            RefreshMediaObjects();
        }

        private OBJConfig LoadTempConfig(GameRunChain runChain)
        {
            var retConfig = new OBJConfig(Application.StartupPath);
            retConfig.RestoreDefaultValuesFromLevel(Levels.PLATFORM_INI);
            retConfig.SetParameter("current_platform", runChain.Platform);
            retConfig.LoadConfig(Levels.PLATFORM_INI);
            retConfig.RestoreDefaultValuesFromLevel(Levels.EMULATOR_INI);
            retConfig.SetParameter("current_emulator", runChain.Emulator);
            retConfig.LoadConfig(Levels.EMULATOR_INI);
            retConfig.RestoreDefaultValuesFromLevel(Levels.GAMELIST_INI);
            retConfig.SetParameter("current_gamelist", runChain.Gamelist);
            retConfig.LoadConfig(Levels.GAMELIST_INI);
            return retConfig;
        }

        private void FixConfigParameters()
        {
            _machineState.testMode = objConfig.test_mode;

            // fix missing emulator path
            if (!Directory.Exists(objConfig.emulator_path) &&
                objConfig.emulator_commandline.Contains(Path.DirectorySeparatorChar.ToString()))
            {
                var path = Path.GetDirectoryName(objConfig.emulator_commandline);
                if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                {
                    var emulator_commandline = "[emulator_path]" + Path.DirectorySeparatorChar +
                        objConfig.emulator_commandline.Substring(
                        objConfig.emulator_commandline.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                    objConfig.SetParameter("emulator_commandline", emulator_commandline);
                    objConfig.SetParameter("emulator_path", path);
                }
            }
            // cleanup recursive paths
            if (objConfig.emulator_path.Contains("[emulator_path]"))
                objConfig.SetParameter("emulator_path", objConfig.emulator_path.Replace("[emulator_path]", "."));

            if (objConfig.rom_path.Contains("[rom_path]") ||
                objConfig.rom_path.Contains("[full_path]") ||
                objConfig.rom_path.Contains("[full_dos_path]"))
                objConfig.SetParameter("rom_path",
                    objConfig.rom_path.Replace("[rom_path]", ".").
                    Replace("[full_path]", ".").
                    Replace("[full_dos_path]", "."));

            objConfig.SaveConfig();
        }

        private void SaveConfig()
        {
            if (currentRom != null)
            {
                objConfig.SetParameter("current_game", currentRom.Key);
            }
            // FNET
            if (!runFnet) objConfig.SaveConfig();
        }

        public void SetParameter(string name, string value)
        {
            objConfig.SetParameter(name, value);
        }
        #endregion

        #region Action Queue Management
        private Queue<Action> _pendingActions = new Queue<Action>();
        private object _lock = new object();

        public void RunAsyncAction(Action action)
        {
            actionQueue.EnqueueAction(action);
        }

        public void RunOnUIThread(Action action)
        {
            lock (_lock)
            {
                _pendingActions.Enqueue(action);
            }
        }

        private void RunPendingActions()
        {
            // consume one action per UpdateUI loop
            //while (true)
            {
                Action action = null;
                lock (_lock)
                {
                    if (_pendingActions.Count != 0)
                        action = _pendingActions.Dequeue();
                    //else
                    //    break;
                }
                if (action != null) action();
            }
        }
        #endregion
    }
}
