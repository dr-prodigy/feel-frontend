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

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System;
using System.Drawing;
using System.Globalization;
using Microsoft.Xna.Framework;

using Color = Microsoft.Xna.Framework.Graphics.Color;

namespace feel
{
    public class OBJConfig
    {
        private Dictionary<string, Parameter> paramList;
        private Dictionary<string, Parameter> paramListBackUp;
        private const Levels MAX_LEVEL = Levels.GAMELIST_INI;

        private List<string>[] iniFile;
        private List<string>[] iniFileBackUp;
        private string startupPath = "";

        #region Properties

        public bool emulator_nodosbox { get; private set; }
        public bool emulator_useshell { get; private set; }
        public bool pre_emulator_wait_for_exit { get; private set; }
        public bool post_emulator_wait_for_exit { get; private set; }
        public bool emulatorname_visible { get; private set; }
        public bool exit_from_menu_enabled { get; private set; }
        public bool exit_to_windows_menu_enabled { get; private set; }
        public bool gamelistname_visible { get; private set; }
        public bool hibernate_menu_enabled { get; private set; }
        public bool platformname_visible { get; private set; }
        public bool restart_windows_menu_enabled { get; private set; }
        public bool romcategory_visible { get; private set; }
        public bool romcounter_visible { get; private set; }
        public bool romdescription_visible { get; private set; }
        public bool romdisplaytype_visible { get; private set; }
        public bool rominputcontrol_visible { get; private set; }
        public bool rommanufacturer_visible { get; private set; }
        public bool romname_visible { get; private set; }
        public bool romstatus_visible { get; private set; }
        public bool cabinet_visible { get; private set; }
        public bool marquee_visible { get; private set; }
        public bool screenshot_original_size { get; private set; }
        public bool screenshot_stretch_to_fixed_size { get; private set; }
        public bool shut_down_menu_enabled { get; private set; }
        public bool snapshot_stretch { get; private set; }
        public bool snapshot_blackbackground { get; private set; }
        public bool cabinet_stretch { get; private set; }
        public bool cabinet_blackbackground { get; private set; }
        public bool marquee_stretch { get; private set; }
        public bool marquee_blackbackground { get; private set; }
        public bool standby_menu_enabled { get; private set; }
        public bool test_mode { get; private set; }
        public bool disable_alt_f4 { get; private set; }
        public bool restore_explorer_at_exit { get; private set; }
        public bool switch_res { get; private set; }
        public bool hide_mouse_pointer { get; private set; }
        public bool show_extended_messages { get; private set; }
        public bool rotate_screen { get; private set; }
        public bool use_joypad { get; private set; }
        public bool show_clones { get; private set; }
        public bool show_clock { get; private set; }
        public bool show_search_key { get; private set; }
        public bool cleanup_names { get; private set; }
        public bool menu_show_sidebar { get; private set; }
        public bool fulltext_search { get; private set; }
        public bool romlist_disable_stars { get; private set; }
        public bool smartasd_debug_mode { get; private set; }
        public bool service_mode { get; private set; }
        public bool service_mode_available { get; private set; }
        public bool update_beta { get; private set; }

        public bool layout_test_mode { get; private set; }
        public Color emulatorname_backcolor { get; private set; }
        public Color emulatorname_font_color { get; private set; }
        public Color gamelistname_backcolor { get; private set; }
        public Color gamelistname_font_color { get; private set; }
        public Color menu_backcolor { get; private set; }
        public Color menu_font_color { get; private set; }
        public Color menu_selected_backcolor { get; private set; }
        public Color menu_selected_font_color { get; private set; }
        public Color platformname_backcolor { get; private set; }
        public Color platformname_font_color { get; private set; }
        public Color romcategory_backcolor { get; private set; }
        public Color romcategory_font_color { get; private set; }
        public Color romcounter_backcolor { get; private set; }
        public Color romcounter_font_color { get; private set; }
        public Color romdescription_backcolor { get; private set; }
        public Color romdescription_font_color { get; private set; }
        public Color romdisplaytype_backcolor { get; private set; }
        public Color romdisplaytype_font_color { get; private set; }
        public Color rominputcontrol_backcolor { get; private set; }
        public Color rominputcontrol_font_color { get; private set; }
        public Color romlist_backcolor { get; private set; }
        public Color romlist_font_color { get; private set; }
        public Color romlist_selected_backcolor { get; private set; }
        public Color romlist_selected_font_color { get; private set; }
        public Color rommanufacturer_backcolor { get; private set; }
        public Color rommanufacturer_font_color { get; private set; }
        public Color romname_backcolor { get; private set; }
        public Color romname_font_color { get; private set; }
        public Color romstatus_backcolor { get; private set; }
        public Color romstatus_font_color { get; private set; }
        public Color screen_saver_backcolor { get; private set; }
        public Color screen_saver_font_color { get; private set; }

        public FeelKey action_key { get; private set; }
        public FeelKey add_game_to_list { get; private set; }
        public FeelKey find_game { get; private set; }
        public FeelKey menu_cancel { get; private set; }
        public FeelKey menu_key { get; private set; }
        public FeelKey menu_ok { get; private set; }
        public FeelKey menu_up { get; private set; }
        public FeelKey menu_down { get; private set; }
        public FeelKey menu_left { get; private set; }
        public FeelKey menu_right { get; private set; }
        public FeelKey previous_platform { get; private set; }
        public FeelKey previous_emulator { get; private set; }
        public FeelKey previous_gamelist { get; private set; }
        public FeelKey next_emulator { get; private set; }
        public FeelKey next_game { get; private set; }
        public FeelKey next_gamelist { get; private set; }
        public FeelKey next_letter_game { get; private set; }
        public FeelKey next_page { get; private set; }
        public FeelKey next_platform { get; private set; }
        public FeelKey previous_game { get; private set; }
        public FeelKey previous_letter_game { get; private set; }
        public FeelKey previous_page { get; private set; }
        public FeelKey remove_game_from_list { get; private set; }
        public FeelKey select_emulator { get; private set; }
        public FeelKey select_gamelist { get; private set; }
        public FeelKey select_platform { get; private set; }
        public FeelKey show_custom_image { get; private set; }
        public FeelKey show_exit_menu { get; private set; }
        public FeelKey show_game_info { get; private set; }
        public FeelKey screensaver_previous_game { get; private set; }
        public FeelKey screensaver_next_game { get; private set; }
        public FeelKey next_sort { get; private set; }
        public FeelKey reload_config { get; private set; }

        public int emulatorname_font_size { get; private set; }
        public int gamelistname_font_size { get; private set; }
        public int menu_font_size { get; private set; }
        public int platformname_font_size { get; private set; }
        public int romcategory_font_size { get; private set; }
        public int romcounter_font_size { get; private set; }
        public int romdescription_font_size { get; private set; }
        public int romdisplaytype_font_size { get; private set; }
        public int rominputcontrol_font_size { get; private set; }
        public int romlist_font_size { get; private set; }
        public int rommanufacturer_font_size { get; private set; }
        public int romname_font_size { get; private set; }
        public int romstatus_font_size { get; private set; }

        public FontStyle emulatorname_font_style { get; private set; }
        public FontStyle gamelistname_font_style { get; private set; }
        public FontStyle menu_font_style { get; private set; }
        public FontStyle platformname_font_style { get; private set; }
        public FontStyle romcategory_font_style { get; private set; }
        public FontStyle romcounter_font_style { get; private set; }
        public FontStyle romdescription_font_style { get; private set; }
        public FontStyle romdisplaytype_font_style { get; private set; }
        public FontStyle rominputcontrol_font_style { get; private set; }
        public FontStyle romlist_font_style { get; private set; }
        public FontStyle rommanufacturer_font_style { get; private set; }
        public FontStyle romname_font_style { get; private set; }
        public FontStyle romstatus_font_style { get; private set; }

        public int background_height { get; private set; }
        public int background_width { get; private set; }
        public int background_frame_duration_ms { get; private set; }
        public int background_repeat_delay_ms { get; private set; }
        public int actors_frame_duration_ms { get; private set; }
        public int actors_repeat_delay_ms { get; private set; }
        public int bezel_frame_duration_ms { get; private set; }
        public int bezel_repeat_delay_ms { get; private set; }
        public int emulatorname_height { get; private set; }
        public int emulatorname_width { get; private set; }
        public int emulatorname_x_pos { get; private set; }
        public int emulatorname_y_pos { get; private set; }
        public int gamelistname_height { get; private set; }
        public int gamelistname_width { get; private set; }
        public int gamelistname_x_pos { get; private set; }
        public int gamelistname_y_pos { get; private set; }
        public int keyboard_scroll_rate { get; private set; }
        public int keyboard_min_scroll_rate { get; private set; }
        public int menu_item_height { get; private set; }
        public int menu_width { get; private set; }
        public int mouse_scroll_rate { get; private set; }
        public int mouse_sensitivity { get; private set; }
        public int platformname_height { get; private set; }
        public int platformname_width { get; private set; }
        public int platformname_x_pos { get; private set; }
        public int platformname_y_pos { get; private set; }
        public int romcategory_height { get; private set; }
        public int romcategory_width { get; private set; }
        public int romcategory_x_pos { get; private set; }
        public int romcategory_y_pos { get; private set; }
        public int romcounter_height { get; private set; }
        public int romcounter_width { get; private set; }
        public int romcounter_x_pos { get; private set; }
        public int romcounter_y_pos { get; private set; }
        public int romdescription_height { get; private set; }
        public int romdescription_width { get; private set; }
        public int romdescription_x_pos { get; private set; }
        public int romdescription_y_pos { get; private set; }
        public int romdisplaytype_height { get; private set; }
        public int romdisplaytype_width { get; private set; }
        public int romdisplaytype_x_pos { get; private set; }
        public int romdisplaytype_y_pos { get; private set; }
        public int rominputcontrol_height { get; private set; }
        public int rominputcontrol_width { get; private set; }
        public int rominputcontrol_x_pos { get; private set; }
        public int rominputcontrol_y_pos { get; private set; }
        public int romlist_height { get; private set; }
        public int romlist_item_height { get; private set; }
        public int romlist_width { get; private set; }
        public int romlist_x_pos { get; private set; }
        public int romlist_y_pos { get; private set; }
        public int rommanufacturer_height { get; private set; }
        public int rommanufacturer_width { get; private set; }
        public int rommanufacturer_x_pos { get; private set; }
        public int rommanufacturer_y_pos { get; private set; }
        public int romname_height { get; private set; }
        public int romname_width { get; private set; }
        public int romname_x_pos { get; private set; }
        public int romname_y_pos { get; private set; }
        public int romstatus_height { get; private set; }
        public int romstatus_width { get; private set; }
        public int romstatus_x_pos { get; private set; }
        public int romstatus_y_pos { get; private set; }
        public int screen_res_x { get; private set; }
        public int screen_res_y { get; private set; }
        public int screen_saver_delay { get; private set; }
        public int screen_saver_slide_time { get; private set; }
        public int screenshot_height { get; private set; }
        public int screenshot_width { get; private set; }
        public int snapshot_height { get; private set; }
        public int snapshot_width { get; private set; }
        public int snapshot_x_pos { get; private set; }
        public int snapshot_y_pos { get; private set; }
        public int cabinet_height { get; private set; }
        public int cabinet_width { get; private set; }
        public int cabinet_x_pos { get; private set; }
        public int cabinet_y_pos { get; private set; }
        public int marquee_height { get; private set; }
        public int marquee_width { get; private set; }
        public int marquee_x_pos { get; private set; }
        public int marquee_y_pos { get; private set; }
        public int sound_fx_volume { get; private set; }
        public int music_volume { get; private set; }
        public int video_volume { get; private set; }
        public int music_change_delay { get; private set; }
        public float video_speed { get; private set; }
        public int joypad_dead_zone { get; private set; }
        public int video_delay { get; private set; }

        public ListType list_type { get; private set; }

        public SortType current_sort { get; private set; }
        public FnetSortType fnet_sort { get; private set; }

        public ScreenSaverType screen_saver_enabled { get; private set; }

        public string current_layout { get; private set; }
        public string current_platform { get; private set; }
        public string current_emulator { get; private set; }
        public string current_gamelist { get; private set; }
        public string current_game { get; private set; }
        public string last_game_played { get; private set; }
        public string autostart_single_game { get; private set; }
        public string emulator_arguments { get; private set; }
        public string emulator_commandline { get; private set; }
        public string emulator_list { get; private set; }
        public string emulator_path { get; private set; }
        public string emulator_title { get; private set; }
        public string emulatorname_font_name { get; private set; }
        public string gamelist_list { get; private set; }
        public string gamelist_title { get; private set; }
        public string gamelistname_font_name { get; private set; }
        public string hiscore_path { get; private set; }
        public string hitotext_exe_path { get; private set; }
        public string input_mapping { get; private set; }
        public string menu_font_name { get; private set; }
        public string nms_file { get; private set; }
        public string nvram_path { get; private set; }
        public string mess_machine { get; private set; }
        public string platform_list { get; private set; }
        public string platform_title { get; private set; }
        public string platformname_font_name { get; private set; }
        public string post_emulator_app_arguments { get; private set; }
        public string post_emulator_app_commandline { get; private set; }
        public string pre_emulator_app_arguments { get; private set; }
        public string pre_emulator_app_commandline { get; private set; }
        public string rom_extension { get; private set; }
        public string rom_path { get; private set; }
        public string romcategory_font_name { get; private set; }
        public string romcounter_font_name { get; private set; }
        public string romdescription_font_name { get; private set; }
        public string romdisplaytype_font_name { get; private set; }
        public string rominputcontrol_font_name { get; private set; }
        public string romlist_font_name { get; private set; }
        public string rommanufacturer_font_name { get; private set; }
        public string romname_font_name { get; private set; }
        public string romstatus_font_name { get; private set; }
        public string snapshot_path { get; private set; }
        public string snapshot_extension { get; private set; }
        public string cabinet_path { get; private set; }
        public string cabinet_extension { get; private set; }
        public string marquee_path { get; private set; }
        public string marquee_extension { get; private set; }
        public string video_path { get; private set; }
        public string sound_fx_list { get; private set; }
        public string sound_fx_menu { get; private set; }
        public string sound_fx_cancel { get; private set; }
        public string sound_fx_confirm { get; private set; }
        public string sound_fx_startemu { get; private set; }
        public string music_path { get; private set; }
        public string input_controls { get; private set; }
        public string frontend_list_controls { get; private set; }
        public string frontend_menu_controls { get; private set; }
        public string smartasd_id { get; private set; }
        public string feel_uuid { get; private set; }

        public SmartAsdMode smartasd_mode { get; private set; }

        public TextAlign emulatorname_text_align { get; private set; }
        public TextAlign gamelistname_text_align { get; private set; }
        public TextAlign platformname_text_align { get; private set; }
        public TextAlign romcategory_text_align { get; private set; }
        public TextAlign romcounter_text_align { get; private set; }
        public TextAlign romdescription_text_align { get; private set; }
        public TextAlign romdisplaytype_text_align { get; private set; }
        public TextAlign rominputcontrol_text_align { get; private set; }
        public TextAlign rommanufacturer_text_align { get; private set; }
        public TextAlign romname_text_align { get; private set; }
        public TextAlign romstatus_text_align { get; private set; }
        public TextAlign romlist_text_align { get; private set; }

        public UseMouse use_mouse { get; private set; }

        public AutostartMode autostart_mode { get; private set; }

        #endregion

        public OBJConfig(string StartUpPath)
        {
            paramList = new Dictionary<string, Parameter>
            {
                // FEEL_INI
                { "layout_test_mode", new Parameter("layout_test_mode", Levels.FEEL_INI, "0", false) },
                { "test_mode", new Parameter("test_mode", Levels.FEEL_INI, "1", false) },
                { "disable_alt_f4", new Parameter("disable_alt_f4", Levels.FEEL_INI, "1", false) },
                { "restore_explorer_at_exit", new Parameter("restore_explorer_at_exit", Levels.FEEL_INI, "1", false) },
                { "switch_res", new Parameter("switch_res", Levels.FEEL_INI, "1", false) },
                { "hide_mouse_pointer", new Parameter("hide_mouse_pointer", Levels.FEEL_INI, "1", false) },
                { "rotate_screen", new Parameter("rotate_screen", Levels.FEEL_INI, "0", false) },
                { "platform_list", new Parameter("platform_list", Levels.FEEL_INI, "arcade, console", false) },
                { "current_platform", new Parameter("current_platform", Levels.FEEL_INI, "arcade", false) },
                { "last_game_played", new Parameter("last_game_played", Levels.FEEL_INI, "", false) },
                { "autostart_single_game", new Parameter("autostart_single_game", Levels.FEEL_INI, "", false) },
                { "screenshot_original_size", new Parameter("screenshot_original_size", Levels.FEEL_INI, "0", true) },
                { "screenshot_width", new Parameter("screenshot_width", Levels.FEEL_INI, "320", true) },
                { "screenshot_height", new Parameter("screenshot_height", Levels.FEEL_INI, "240", true) },
                { "screenshot_stretch_to_fixed_size", new Parameter("screenshot_stretch_to_fixed_size", Levels.FEEL_INI, "1", true) },
                { "use_mouse", new Parameter("use_mouse", Levels.FEEL_INI, "0", true) },
                { "keyboard_scroll_rate", new Parameter("keyboard_scroll_rate", Levels.FEEL_INI, "225", true) },
                { "keyboard_min_scroll_rate", new Parameter("keyboard_min_scroll_rate", Levels.FEEL_INI, "20", true) },
                { "mouse_scroll_rate", new Parameter("mouse_scroll_rate", Levels.FEEL_INI, "50", true) },
                { "mouse_sensitivity", new Parameter("mouse_sensitivity", Levels.FEEL_INI, "5", true) },
                { "screen_saver_enabled", new Parameter("screen_saver_enabled", Levels.FEEL_INI, "0", true) },
                { "screen_saver_delay", new Parameter("screen_saver_delay", Levels.FEEL_INI, "30", true) },
                { "screen_saver_slide_time", new Parameter("screen_saver_slide_time", Levels.FEEL_INI, "5", true) },
                { "smartasd_mode", new Parameter("smartasd_mode", Levels.FEEL_INI, "0", true) },
                { "smartasd_debug_mode", new Parameter("smartasd_debug_mode", Levels.FEEL_INI, "1", true) },
                { "smartasd_id", new Parameter("smartasd_id", Levels.FEEL_INI, "*", true) },
                { "service_mode", new Parameter("service_mode", Levels.FEEL_INI, "0", false) },
                { "service_mode_available", new Parameter("service_mode_available", Levels.FEEL_INI, "1", false) },
                { "update_beta", new Parameter("update_beta", Levels.FEEL_INI, "1", false) },
                { "feel_uuid", new Parameter("feel_uuid", Levels.FEEL_INI, "", false) },
                { "fnet_sort", new Parameter("fnet_sort", Levels.FEEL_INI, "0", true) },

                // moved to EMULATOR_INI!
                //{ "current_layout", new Parameter("current_layout", Levels.FEEL_INI, "sheet", false) },
                { "exit_to_windows_menu_enabled", new Parameter("exit_to_windows_menu_enabled", Levels.FEEL_INI, "1", false) },
                { "restart_windows_menu_enabled", new Parameter("restart_windows_menu_enabled", Levels.FEEL_INI, "0", true) },
                { "shut_down_menu_enabled", new Parameter("shut_down_menu_enabled", Levels.FEEL_INI, "0", true) },
                { "standby_menu_enabled", new Parameter("standby_menu_enabled", Levels.FEEL_INI, "0", true) },
                { "hibernate_menu_enabled", new Parameter("hibernate_menu_enabled", Levels.FEEL_INI, "0", true) },
                { "exit_from_menu_enabled", new Parameter("exit_from_menu_enabled", Levels.FEEL_INI, "1", false) },
                { "sound_fx_list", new Parameter("sound_fx_list", Levels.FEEL_INI, "", true) },
                { "sound_fx_menu", new Parameter("sound_fx_menu", Levels.FEEL_INI, "", true) },
                { "sound_fx_cancel", new Parameter("sound_fx_cancel", Levels.FEEL_INI, "", true) },
                { "sound_fx_confirm", new Parameter("sound_fx_confirm", Levels.FEEL_INI, "", true) },
                { "sound_fx_startemu", new Parameter("sound_fx_startemu", Levels.FEEL_INI, "", true) },
                { "sound_fx_volume", new Parameter("sound_fx_volume", Levels.FEEL_INI, "80", true) },
                { "music_path", new Parameter("music_path", Levels.FEEL_INI, "", true) },
                { "music_volume", new Parameter("music_volume", Levels.FEEL_INI, "60", true) },
                { "music_change_delay", new Parameter("music_change_delay", Levels.FEEL_INI, "0", true) },
                { "video_delay", new Parameter("video_delay", Levels.FEEL_INI, "2", true) },
                { "video_volume", new Parameter("video_volume", Levels.FEEL_INI, "30", true) },
                { "video_speed", new Parameter("video_speed", Levels.FEEL_INI, "1.0", true) },
                { "use_joypad", new Parameter("use_joypad", Levels.FEEL_INI, "0", true) },
                { "joypad_dead_zone", new Parameter("joypad_dead_zone", Levels.FEEL_INI, "2500", true) },
                { "action_key", new Parameter("action_key", Levels.FEEL_INI, "1", false) },
                { "menu_key", new Parameter("menu_key", Levels.FEEL_INI, "LAlt", false) },
                { "menu_ok", new Parameter("menu_ok", Levels.FEEL_INI, "LCtrl", true) },
                { "menu_cancel", new Parameter("menu_cancel", Levels.FEEL_INI, "2", true) },
                { "menu_up", new Parameter("menu_up", Levels.FEEL_INI, "Up", true) },
                { "menu_down", new Parameter("menu_down", Levels.FEEL_INI, "Down", true) },
                { "menu_left", new Parameter("menu_left", Levels.FEEL_INI, "Left", true) },
                { "menu_right", new Parameter("menu_right", Levels.FEEL_INI, "Right", true) },
                { "previous_game", new Parameter("previous_game", Levels.FEEL_INI, "Up", true) },
                { "next_game", new Parameter("next_game", Levels.FEEL_INI, "Down", true) },
                { "previous_letter_game", new Parameter("previous_letter_game", Levels.FEEL_INI, "R", true) },
                { "next_letter_game", new Parameter("next_letter_game", Levels.FEEL_INI, "F", true) },
                { "previous_page", new Parameter("previous_page", Levels.FEEL_INI, "Left", true) },
                { "next_page", new Parameter("next_page", Levels.FEEL_INI, "Right", true) },
                { "find_game", new Parameter("find_game", Levels.FEEL_INI, "A", true) },
                { "select_platform", new Parameter("select_platform", Levels.FEEL_INI, "2", true) },
                { "select_emulator", new Parameter("select_emulator", Levels.FEEL_INI, "", true) },
                { "select_gamelist", new Parameter("select_gamelist", Levels.FEEL_INI, "", true) },
                { "previous_platform", new Parameter("previous_platform", Levels.FEEL_INI, "", true) },
                { "previous_emulator", new Parameter("previous_emulator", Levels.FEEL_INI, "", true) },
                { "previous_gamelist", new Parameter("previous_gamelist", Levels.FEEL_INI, "", true) },
                { "next_platform", new Parameter("next_platform", Levels.FEEL_INI, "", true) },
                { "next_emulator", new Parameter("next_emulator", Levels.FEEL_INI, "LShift", true) },
                { "next_gamelist", new Parameter("next_gamelist", Levels.FEEL_INI, "Space", true) },
                { "add_game_to_list", new Parameter("add_game_to_list", Levels.FEEL_INI, "", true) },
                { "remove_game_from_list", new Parameter("remove_game_from_list", Levels.FEEL_INI, "", true) },
                { "show_custom_image", new Parameter("show_custom_image", Levels.FEEL_INI, "W", true) },
                { "show_game_info", new Parameter("show_game_info", Levels.FEEL_INI, "LCtrl", true) },
                { "show_exit_menu", new Parameter("show_exit_menu", Levels.FEEL_INI, "ESC", true) },
                { "screensaver_previous_game", new Parameter("screensaver_previous_game", Levels.FEEL_INI, "Left", true) },
                { "screensaver_next_game", new Parameter("screensaver_next_game", Levels.FEEL_INI, "Right", true) },
                { "next_sort", new Parameter("next_sort", Levels.FEEL_INI, "S", true) },
                { "reload_config", new Parameter("reload_config", Levels.FEEL_INI, "F2", true) },
                { "fulltext_search", new Parameter("fulltext_search", Levels.FEEL_INI, "0", false) },
                { "autostart_mode", new Parameter("autostart_mode", Levels.FEEL_INI, "0", true) },
                { "frontend_list_controls", new Parameter("frontend_list_controls", Levels.FEEL_INI, "joy1|start1|bt1-1|bt1-2|dial1|joy2|start2|extra1|side1|side2", true) },
                { "frontend_menu_controls", new Parameter("frontend_menu_controls", Levels.FEEL_INI, "joy1|start1|bt1-1|bt1-2|dial1|start2|extra1|side1|side2", true) },
                { "show_clock", new Parameter("show_clock", Levels.FEEL_INI, "1", true) },
                { "show_search_key", new Parameter("show_search_key", Levels.FEEL_INI, "1", true) },
                // PLATFORM_INI
                { "platform_title", new Parameter("platform_title", Levels.PLATFORM_INI, "", false) },
                { "emulator_list", new Parameter("emulator_list", Levels.PLATFORM_INI, "", false) },
                { "current_emulator", new Parameter("current_emulator", Levels.PLATFORM_INI, "", false) },
                // EMULATOR_INI
                { "emulator_title", new Parameter("emulator_title", Levels.EMULATOR_INI, "", false) },
                { "gamelist_list", new Parameter("gamelist_list", Levels.EMULATOR_INI, "", false) },
                { "current_gamelist", new Parameter("current_gamelist", Levels.EMULATOR_INI, "", false) },
                { "current_layout", new Parameter("current_layout", Levels.EMULATOR_INI, "sheet", false) },
                { "rom_path", new Parameter("rom_path", Levels.EMULATOR_INI, "." + Path.DirectorySeparatorChar + "mame" + Path.DirectorySeparatorChar + "roms", false) },
                { "rom_extension", new Parameter("rom_extension", Levels.EMULATOR_INI, "zip, rar", false) },
                { "nms_file", new Parameter("nms_file", Levels.EMULATOR_INI, "", true) },
                { "emulator_path", new Parameter("emulator_path", Levels.EMULATOR_INI, "." + Path.DirectorySeparatorChar + "mame", true) },
                { "hitotext_exe_path", new Parameter("hitotext_exe_path", Levels.EMULATOR_INI, "." + Path.DirectorySeparatorChar + "hitotext.exe", true) },
                { "hiscore_path", new Parameter("hiscore_path", Levels.EMULATOR_INI, "." + Path.DirectorySeparatorChar + "mame" + Path.DirectorySeparatorChar + "hi", true) },
                { "nvram_path", new Parameter("nvram_path", Levels.EMULATOR_INI, "." + Path.DirectorySeparatorChar + "mame" + Path.DirectorySeparatorChar + "nvram", true) },
                { "mess_machine", new Parameter("mess_machine", Levels.EMULATOR_INI, "", true) },
                { "list_type", new Parameter("list_type", Levels.EMULATOR_INI, "0", false) },
                { "pre_emulator_app_commandline", new Parameter("pre_emulator_app_commandline", Levels.EMULATOR_INI, "", true) },
                { "pre_emulator_app_arguments", new Parameter("pre_emulator_app_arguments", Levels.EMULATOR_INI, "", true) },
                { "pre_emulator_wait_for_exit", new Parameter("pre_emulator_wait_for_exit", Levels.EMULATOR_INI, "0", true) },
                { "post_emulator_wait_for_exit", new Parameter("post_emulator_wait_for_exit", Levels.EMULATOR_INI, "0", true) },
                { "emulator_commandline", new Parameter("emulator_commandline", Levels.EMULATOR_INI, "[emulator_path]" + Path.DirectorySeparatorChar + "mame.exe", true) },
                { "emulator_arguments", new Parameter("emulator_arguments", Levels.EMULATOR_INI, "[rom_name]", true) },
                { "emulator_nodosbox", new Parameter("emulator_nodosbox", Levels.EMULATOR_INI, "1", true) },
                { "emulator_useshell", new Parameter("emulator_useshell", Levels.EMULATOR_INI, "0", true) },
                { "post_emulator_app_commandline", new Parameter("post_emulator_app_commandline", Levels.EMULATOR_INI, "", true) },
                { "post_emulator_app_arguments", new Parameter("post_emulator_app_arguments", Levels.EMULATOR_INI, "", true) },
                { "input_mapping", new Parameter("input_mapping", Levels.EMULATOR_INI, "", true) },
                { "snapshot_path", new Parameter("snapshot_path", Levels.EMULATOR_INI, "." + Path.DirectorySeparatorChar + "mame" + Path.DirectorySeparatorChar + "snap", true) },
                { "snapshot_extension", new Parameter("snapshot_extension", Levels.EMULATOR_INI, "png", false) },
                { "cabinet_path", new Parameter("cabinet_path", Levels.EMULATOR_INI, "." + Path.DirectorySeparatorChar + "mame" + Path.DirectorySeparatorChar + "cabinets", true) },
                { "cabinet_extension", new Parameter("cabinet_extension", Levels.EMULATOR_INI, "png", false) },
                { "marquee_path", new Parameter("marquee_path", Levels.EMULATOR_INI, "." + Path.DirectorySeparatorChar + "mame" + Path.DirectorySeparatorChar + "marquees", true) },
                { "marquee_extension", new Parameter("marquee_extension", Levels.EMULATOR_INI, "png", false) },
                { "video_path", new Parameter("video_path", Levels.EMULATOR_INI, "." + Path.DirectorySeparatorChar + "mame" + Path.DirectorySeparatorChar + "video", true) },
                { "show_clones", new Parameter("show_clones", Levels.EMULATOR_INI, "1", true) },
                { "cleanup_names", new Parameter("cleanup_names", Levels.EMULATOR_INI, "0", true) },
                { "current_sort", new Parameter("current_sort", Levels.GAMELIST_INI, "0", true) },
                { "input_controls", new Parameter("input_controls", Levels.EMULATOR_INI, "joy4way - 1P - 4Bt", true) },
                // GAMELIST_INI
                { "gamelist_title", new Parameter("gamelist_title", Levels.GAMELIST_INI, "", false) },
                { "current_game", new Parameter("current_game", Levels.GAMELIST_INI, "", false) },
                // LAYOUT_INI
                { "screen_res_x", new Parameter("screen_res_x", Levels.LAYOUT_INI, "640", false) },
                { "screen_res_y", new Parameter("screen_res_y", Levels.LAYOUT_INI, "480", false) },
                { "menu_show_sidebar", new Parameter("menu_show_sidebar", Levels.LAYOUT_INI, "1", false) },
                { "show_extended_messages", new Parameter("show_extended_messages", Levels.LAYOUT_INI, "1", false) },
                { "romlist_x_pos", new Parameter("romlist_x_pos", Levels.LAYOUT_INI, "27", true) },
                { "romlist_y_pos", new Parameter("romlist_y_pos", Levels.LAYOUT_INI, "142", true) },
                { "romlist_width", new Parameter("romlist_width", Levels.LAYOUT_INI, "274", true) },
                { "romlist_height", new Parameter("romlist_height", Levels.LAYOUT_INI, "275", true) },
                { "romlist_item_height", new Parameter("romlist_item_height", Levels.LAYOUT_INI, "25", true) },
                { "romlist_font_name", new Parameter("romlist_font_name", Levels.LAYOUT_INI, "arial", true) },
                { "romlist_font_size", new Parameter("romlist_font_size", Levels.LAYOUT_INI, "11", true) },
                { "romlist_font_style", new Parameter("romlist_font_style", Levels.LAYOUT_INI, "1", true) },
                { "romlist_font_color", new Parameter("romlist_font_color", Levels.LAYOUT_INI, "150, 200, 50", true) },
                { "romlist_backcolor", new Parameter("romlist_backcolor", Levels.LAYOUT_INI, "22, 7, 27", true) },
                { "romlist_selected_font_color", new Parameter("romlist_selected_font_color", Levels.LAYOUT_INI, "255, 255, 196", true) },
                { "romlist_selected_backcolor", new Parameter("romlist_selected_backcolor", Levels.LAYOUT_INI, "196, 128, 196", true) },
                { "romlist_text_align", new Parameter("romlist_text_align", Levels.LAYOUT_INI, "0", true) },
                { "romlist_disable_stars", new Parameter("romlist_disable_stars", Levels.LAYOUT_INI, "0", true) },
                { "background_width", new Parameter("background_width", Levels.LAYOUT_INI, "640", true) },
                { "background_height", new Parameter("background_height", Levels.LAYOUT_INI, "480", true) },
                { "background_frame_duration_ms", new Parameter("background_frame_duration_ms", Levels.LAYOUT_INI, "300", false) },
                { "background_repeat_delay_ms", new Parameter("background_repeat_delay_ms", Levels.LAYOUT_INI, "5000", false) },
                { "actors_frame_duration_ms", new Parameter("actors_frame_duration_ms", Levels.LAYOUT_INI, "300", false) },
                { "actors_repeat_delay_ms", new Parameter("actors_repeat_delay_ms", Levels.LAYOUT_INI, "0", false) },
                { "bezel_frame_duration_ms", new Parameter("bezel_frame_duration_ms", Levels.LAYOUT_INI, "300", false) },
                { "bezel_repeat_delay_ms", new Parameter("bezel_repeat_delay_ms", Levels.LAYOUT_INI, "5000", false) },
                { "snapshot_x_pos", new Parameter("snapshot_x_pos", Levels.LAYOUT_INI, "361", true) },
                { "snapshot_y_pos", new Parameter("snapshot_y_pos", Levels.LAYOUT_INI, "196", true) },
                { "snapshot_width", new Parameter("snapshot_width", Levels.LAYOUT_INI, "228", true) },
                { "snapshot_height", new Parameter("snapshot_height", Levels.LAYOUT_INI, "171", true) },
                { "snapshot_stretch", new Parameter("snapshot_stretch", Levels.LAYOUT_INI, "0", true) },
                { "snapshot_blackbackground", new Parameter("snapshot_blackbackground", Levels.LAYOUT_INI, "1", true) },
                { "cabinet_visible", new Parameter("cabinet_visible", Levels.LAYOUT_INI, "0", true) },
                { "cabinet_x_pos", new Parameter("cabinet_x_pos", Levels.LAYOUT_INI, "0", true) },
                { "cabinet_y_pos", new Parameter("cabinet_y_pos", Levels.LAYOUT_INI, "0", true) },
                { "cabinet_width", new Parameter("cabinet_width", Levels.LAYOUT_INI, "0", true) },
                { "cabinet_height", new Parameter("cabinet_height", Levels.LAYOUT_INI, "0", true) },
                { "cabinet_stretch", new Parameter("cabinet_stretch", Levels.LAYOUT_INI, "0", true) },
                { "cabinet_blackbackground", new Parameter("cabinet_blackbackground", Levels.LAYOUT_INI, "1", true) },
                { "marquee_visible", new Parameter("marquee_visible", Levels.LAYOUT_INI, "0", true) },
                { "marquee_x_pos", new Parameter("marquee_x_pos", Levels.LAYOUT_INI, "0", true) },
                { "marquee_y_pos", new Parameter("marquee_y_pos", Levels.LAYOUT_INI, "0", true) },
                { "marquee_width", new Parameter("marquee_width", Levels.LAYOUT_INI, "0", true) },
                { "marquee_height", new Parameter("marquee_height", Levels.LAYOUT_INI, "0", true) },
                { "marquee_stretch", new Parameter("marquee_stretch", Levels.LAYOUT_INI, "0", true) },
                { "marquee_blackbackground", new Parameter("marquee_blackbackground", Levels.LAYOUT_INI, "1", true) },
                { "romcounter_visible", new Parameter("romcounter_visible", Levels.LAYOUT_INI, "1", true) },
                { "romcounter_x_pos", new Parameter("romcounter_x_pos", Levels.LAYOUT_INI, "196", true) },
                { "romcounter_y_pos", new Parameter("romcounter_y_pos", Levels.LAYOUT_INI, "423", true) },
                { "romcounter_width", new Parameter("romcounter_width", Levels.LAYOUT_INI, "102", true) },
                { "romcounter_height", new Parameter("romcounter_height", Levels.LAYOUT_INI, "18", true) },
                { "romcounter_font_name", new Parameter("romcounter_font_name", Levels.LAYOUT_INI, "arial", true) },
                { "romcounter_font_size", new Parameter("romcounter_font_size", Levels.LAYOUT_INI, "8", true) },
                { "romcounter_font_style", new Parameter("romcounter_font_style", Levels.LAYOUT_INI, "0", true) },
                { "romcounter_font_color", new Parameter("romcounter_font_color", Levels.LAYOUT_INI, "255, 128, 0", true) },
                { "romcounter_backcolor", new Parameter("romcounter_backcolor", Levels.LAYOUT_INI, "22, 7, 27", true) },
                { "romcounter_text_align", new Parameter("romcounter_text_align", Levels.LAYOUT_INI, "2", true) },
                { "platformname_visible", new Parameter("platformname_visible", Levels.LAYOUT_INI, "1", true) },
                { "platformname_x_pos", new Parameter("platformname_x_pos", Levels.LAYOUT_INI, "360", true) },
                { "platformname_y_pos", new Parameter("platformname_y_pos", Levels.LAYOUT_INI, "92", true) },
                { "platformname_width", new Parameter("platformname_width", Levels.LAYOUT_INI, "230", true) },
                { "platformname_height", new Parameter("platformname_height", Levels.LAYOUT_INI, "35", true) },
                { "platformname_font_name", new Parameter("platformname_font_name", Levels.LAYOUT_INI, "arial", true) },
                { "platformname_font_size", new Parameter("platformname_font_size", Levels.LAYOUT_INI, "14", true) },
                { "platformname_font_style", new Parameter("platformname_font_style", Levels.LAYOUT_INI, "0", true) },
                { "platformname_font_color", new Parameter("platformname_font_color", Levels.LAYOUT_INI, "128, 192, 192", true) },
                { "platformname_backcolor", new Parameter("platformname_backcolor", Levels.LAYOUT_INI, "22, 7, 27", true) },
                { "platformname_text_align", new Parameter("platformname_text_align", Levels.LAYOUT_INI, "1", true) },
                { "emulatorname_visible", new Parameter("emulatorname_visible", Levels.LAYOUT_INI, "1", true) },
                { "emulatorname_x_pos", new Parameter("emulatorname_x_pos", Levels.LAYOUT_INI, "67", true) },
                { "emulatorname_y_pos", new Parameter("emulatorname_y_pos", Levels.LAYOUT_INI, "77", true) },
                { "emulatorname_width", new Parameter("emulatorname_width", Levels.LAYOUT_INI, "196", true) },
                { "emulatorname_height", new Parameter("emulatorname_height", Levels.LAYOUT_INI, "25", true) },
                { "emulatorname_font_name", new Parameter("emulatorname_font_name", Levels.LAYOUT_INI, "arial", true) },
                { "emulatorname_font_size", new Parameter("emulatorname_font_size", Levels.LAYOUT_INI, "14", true) },
                { "emulatorname_font_style", new Parameter("emulatorname_font_style", Levels.LAYOUT_INI, "3", true) },
                { "emulatorname_font_color", new Parameter("emulatorname_font_color", Levels.LAYOUT_INI, "255, 128, 255", true) },
                { "emulatorname_backcolor", new Parameter("emulatorname_backcolor", Levels.LAYOUT_INI, "22, 7, 27", true) },
                { "emulatorname_text_align", new Parameter("emulatorname_text_align", Levels.LAYOUT_INI, "1", true) },
                { "gamelistname_visible", new Parameter("gamelistname_visible", Levels.LAYOUT_INI, "1", true) },
                { "gamelistname_x_pos", new Parameter("gamelistname_x_pos", Levels.LAYOUT_INI, "67", true) },
                { "gamelistname_y_pos", new Parameter("gamelistname_y_pos", Levels.LAYOUT_INI, "100", true) },
                { "gamelistname_width", new Parameter("gamelistname_width", Levels.LAYOUT_INI, "196", true) },
                { "gamelistname_height", new Parameter("gamelistname_height", Levels.LAYOUT_INI, "18", true) },
                { "gamelistname_font_name", new Parameter("gamelistname_font_name", Levels.LAYOUT_INI, "arial", true) },
                { "gamelistname_font_size", new Parameter("gamelistname_font_size", Levels.LAYOUT_INI, "10", true) },
                { "gamelistname_font_style", new Parameter("gamelistname_font_style", Levels.LAYOUT_INI, "0", true) },
                { "gamelistname_font_color", new Parameter("gamelistname_font_color", Levels.LAYOUT_INI, "128, 128, 192", true) },
                { "gamelistname_backcolor", new Parameter("gamelistname_backcolor", Levels.LAYOUT_INI, "22, 7, 27", true) },
                { "gamelistname_text_align", new Parameter("gamelistname_text_align", Levels.LAYOUT_INI, "1", true) },
                { "romname_visible", new Parameter("romname_visible", Levels.LAYOUT_INI, "1", true) },
                { "romname_x_pos", new Parameter("romname_x_pos", Levels.LAYOUT_INI, "30", true) },
                { "romname_y_pos", new Parameter("romname_y_pos", Levels.LAYOUT_INI, "423", true) },
                { "romname_width", new Parameter("romname_width", Levels.LAYOUT_INI, "154", true) },
                { "romname_height", new Parameter("romname_height", Levels.LAYOUT_INI, "18", true) },
                { "romname_font_name", new Parameter("romname_font_name", Levels.LAYOUT_INI, "arial", true) },
                { "romname_font_size", new Parameter("romname_font_size", Levels.LAYOUT_INI, "8", true) },
                { "romname_font_style", new Parameter("romname_font_style", Levels.LAYOUT_INI, "0", true) },
                { "romname_font_color", new Parameter("romname_font_color", Levels.LAYOUT_INI, "128, 0, 64", true) },
                { "romname_backcolor", new Parameter("romname_backcolor", Levels.LAYOUT_INI, "22, 7, 27", true) },
                { "romname_text_align", new Parameter("romname_text_align", Levels.LAYOUT_INI, "0", true) },
                { "romdescription_visible", new Parameter("romdescription_visible", Levels.LAYOUT_INI, "1", true) },
                { "romdescription_x_pos", new Parameter("romdescription_x_pos", Levels.LAYOUT_INI, "360", true) },
                { "romdescription_y_pos", new Parameter("romdescription_y_pos", Levels.LAYOUT_INI, "143", true) },
                { "romdescription_width", new Parameter("romdescription_width", Levels.LAYOUT_INI, "230", true) },
                { "romdescription_height", new Parameter("romdescription_height", Levels.LAYOUT_INI, "17", true) },
                { "romdescription_font_name", new Parameter("romdescription_font_name", Levels.LAYOUT_INI, "arial", true) },
                { "romdescription_font_size", new Parameter("romdescription_font_size", Levels.LAYOUT_INI, "8", true) },
                { "romdescription_font_style", new Parameter("romdescription_font_style", Levels.LAYOUT_INI, "0", true) },
                { "romdescription_font_color", new Parameter("romdescription_font_color", Levels.LAYOUT_INI, "255, 128, 255", true) },
                { "romdescription_backcolor", new Parameter("romdescription_backcolor", Levels.LAYOUT_INI, "22, 7, 27", true) },
                { "romdescription_text_align", new Parameter("romdescription_text_align", Levels.LAYOUT_INI, "1", true) },
                { "rommanufacturer_visible", new Parameter("rommanufacturer_visible", Levels.LAYOUT_INI, "1", true) },
                { "rommanufacturer_x_pos", new Parameter("rommanufacturer_x_pos", Levels.LAYOUT_INI, "360", true) },
                { "rommanufacturer_y_pos", new Parameter("rommanufacturer_y_pos", Levels.LAYOUT_INI, "404", true) },
                { "rommanufacturer_width", new Parameter("rommanufacturer_width", Levels.LAYOUT_INI, "230", true) },
                { "rommanufacturer_height", new Parameter("rommanufacturer_height", Levels.LAYOUT_INI, "17", true) },
                { "rommanufacturer_font_name", new Parameter("rommanufacturer_font_name", Levels.LAYOUT_INI, "arial", true) },
                { "rommanufacturer_font_size", new Parameter("rommanufacturer_font_size", Levels.LAYOUT_INI, "8", true) },
                { "rommanufacturer_font_style", new Parameter("rommanufacturer_font_style", Levels.LAYOUT_INI, "0", true) },
                { "rommanufacturer_font_color", new Parameter("rommanufacturer_font_color", Levels.LAYOUT_INI, "64, 128, 128", true) },
                { "rommanufacturer_backcolor", new Parameter("rommanufacturer_backcolor", Levels.LAYOUT_INI, "22, 7, 27", true) },
                { "rommanufacturer_text_align", new Parameter("rommanufacturer_text_align", Levels.LAYOUT_INI, "0", true) },
                { "romdisplaytype_visible", new Parameter("romdisplaytype_visible", Levels.LAYOUT_INI, "0", true) },
                { "romdisplaytype_x_pos", new Parameter("romdisplaytype_x_pos", Levels.LAYOUT_INI, "0", true) },
                { "romdisplaytype_y_pos", new Parameter("romdisplaytype_y_pos", Levels.LAYOUT_INI, "0", true) },
                { "romdisplaytype_width", new Parameter("romdisplaytype_width", Levels.LAYOUT_INI, "0", true) },
                { "romdisplaytype_height", new Parameter("romdisplaytype_height", Levels.LAYOUT_INI, "0", true) },
                { "romdisplaytype_font_name", new Parameter("romdisplaytype_font_name", Levels.LAYOUT_INI, "arial", true) },
                { "romdisplaytype_font_size", new Parameter("romdisplaytype_font_size", Levels.LAYOUT_INI, "0", true) },
                { "romdisplaytype_font_style", new Parameter("romdisplaytype_font_style", Levels.LAYOUT_INI, "0", true) },
                { "romdisplaytype_font_color", new Parameter("romdisplaytype_font_color", Levels.LAYOUT_INI, "0", true) },
                { "romdisplaytype_backcolor", new Parameter("romdisplaytype_backcolor", Levels.LAYOUT_INI, "0", true) },
                { "romdisplaytype_text_align", new Parameter("romdisplaytype_text_align", Levels.LAYOUT_INI, "0", true) },
                { "rominputcontrol_visible", new Parameter("rominputcontrol_visible", Levels.LAYOUT_INI, "1", true) },
                { "rominputcontrol_x_pos", new Parameter("rominputcontrol_x_pos", Levels.LAYOUT_INI, "518", true) },
                { "rominputcontrol_y_pos", new Parameter("rominputcontrol_y_pos", Levels.LAYOUT_INI, "423", true) },
                { "rominputcontrol_width", new Parameter("rominputcontrol_width", Levels.LAYOUT_INI, "72", true) },
                { "rominputcontrol_height", new Parameter("rominputcontrol_height", Levels.LAYOUT_INI, "17", true) },
                { "rominputcontrol_font_name", new Parameter("rominputcontrol_font_name", Levels.LAYOUT_INI, "arial", true) },
                { "rominputcontrol_font_size", new Parameter("rominputcontrol_font_size", Levels.LAYOUT_INI, "8", true) },
                { "rominputcontrol_font_style", new Parameter("rominputcontrol_font_style", Levels.LAYOUT_INI, "0", true) },
                { "rominputcontrol_font_color", new Parameter("rominputcontrol_font_color", Levels.LAYOUT_INI, "128, 128, 0", true) },
                { "rominputcontrol_backcolor", new Parameter("rominputcontrol_backcolor", Levels.LAYOUT_INI, "22, 7, 27", true) },
                { "rominputcontrol_text_align", new Parameter("rominputcontrol_text_align", Levels.LAYOUT_INI, "2", true) },
                { "romstatus_visible", new Parameter("romstatus_visible", Levels.LAYOUT_INI, "0", true) },
                { "romstatus_x_pos", new Parameter("romstatus_x_pos", Levels.LAYOUT_INI, "0", true) },
                { "romstatus_y_pos", new Parameter("romstatus_y_pos", Levels.LAYOUT_INI, "0", true) },
                { "romstatus_width", new Parameter("romstatus_width", Levels.LAYOUT_INI, "0", true) },
                { "romstatus_height", new Parameter("romstatus_height", Levels.LAYOUT_INI, "0", true) },
                { "romstatus_font_name", new Parameter("romstatus_font_name", Levels.LAYOUT_INI, "arial", true) },
                { "romstatus_font_size", new Parameter("romstatus_font_size", Levels.LAYOUT_INI, "0", true) },
                { "romstatus_font_style", new Parameter("romstatus_font_style", Levels.LAYOUT_INI, "0", true) },
                { "romstatus_font_color", new Parameter("romstatus_font_color", Levels.LAYOUT_INI, "0", true) },
                { "romstatus_backcolor", new Parameter("romstatus_backcolor", Levels.LAYOUT_INI, "0", true) },
                { "romstatus_text_align", new Parameter("romstatus_text_align", Levels.LAYOUT_INI, "0", true) },
                { "romcategory_visible", new Parameter("romcategory_visible", Levels.LAYOUT_INI, "1", true) },
                { "romcategory_x_pos", new Parameter("romcategory_x_pos", Levels.LAYOUT_INI, "360", true) },
                { "romcategory_y_pos", new Parameter("romcategory_y_pos", Levels.LAYOUT_INI, "423", true) },
                { "romcategory_width", new Parameter("romcategory_width", Levels.LAYOUT_INI, "145", true) },
                { "romcategory_height", new Parameter("romcategory_height", Levels.LAYOUT_INI, "17", true) },
                { "romcategory_font_name", new Parameter("romcategory_font_name", Levels.LAYOUT_INI, "arial", true) },
                { "romcategory_font_size", new Parameter("romcategory_font_size", Levels.LAYOUT_INI, "8", true) },
                { "romcategory_font_style", new Parameter("romcategory_font_style", Levels.LAYOUT_INI, "0", true) },
                { "romcategory_font_color", new Parameter("romcategory_font_color", Levels.LAYOUT_INI, "0, 96, 96", true) },
                { "romcategory_backcolor", new Parameter("romcategory_backcolor", Levels.LAYOUT_INI, "22, 7, 27", true) },
                { "romcategory_text_align", new Parameter("romcategory_text_align", Levels.LAYOUT_INI, "0", true) },
                { "menu_width", new Parameter("menu_width", Levels.LAYOUT_INI, "200", true) },
                { "menu_item_height", new Parameter("menu_item_height", Levels.LAYOUT_INI, "24", true) },
                { "menu_font_name", new Parameter("menu_font_name", Levels.LAYOUT_INI, "arial", true) },
                { "menu_font_size", new Parameter("menu_font_size", Levels.LAYOUT_INI, "11", true) },
                { "menu_font_style", new Parameter("menu_font_style", Levels.LAYOUT_INI, "1", true) },
                { "menu_font_color", new Parameter("menu_font_color", Levels.LAYOUT_INI, "150, 200, 50", true) },
                { "menu_backcolor", new Parameter("menu_backcolor", Levels.LAYOUT_INI, "96, 64, 96", true) },
                { "menu_selected_font_color", new Parameter("menu_selected_font_color", Levels.LAYOUT_INI, "255, 255, 196", true) },
                { "menu_selected_backcolor", new Parameter("menu_selected_backcolor", Levels.LAYOUT_INI, "196, 128, 196", true) },
                { "screen_saver_backcolor", new Parameter("screen_saver_backcolor", Levels.LAYOUT_INI, "0, 0, 0", true) },
                { "screen_saver_font_color", new Parameter("screen_saver_font_color", Levels.LAYOUT_INI, "0, 96, 96", true) }
            };

            InitializeDefaultValues();

            iniFile = new List<string>[(int)MAX_LEVEL + 3];
            for (var i = 0; i <= (int)MAX_LEVEL + 2; i++)
                iniFile[i] = new List<string>();

            startupPath = StartUpPath;
        }

        public void RestoreDefaultValuesFromLevel(Levels level)
        {
            if (level == Levels.LAYOUT_INI)
            {
                RestoreLevelValues(Levels.LAYOUT_INI);
            }
            else
            {
                // MAURI
                //while ((int)level <= (int)MAX_LEVEL)
                //{
                    RestoreLevelValues(level);
                //    level++;
                //}
            }
        }

        private void FixMissingParams()
        {
            if (string.IsNullOrEmpty(emulator_path))
            {
                var index = emulator_commandline.LastIndexOf("" + Path.DirectorySeparatorChar );
                if (index > 0)
                    SetParameter("emulator_path", emulator_commandline.Substring(0, index));
            }
            if (current_platform == "all_emu")
            {
                if (string.IsNullOrEmpty(platform_title))
                    SetParameter("platform_title", "* TOP GAMES *");
                var curLayout = GetParameter("current_layout");
                if (curLayout != null)
                    curLayout.CurrentLevel = Levels.PLATFORM_INI;
            }

            // set name cleanup
            RomDesc.CleanupName = this.cleanup_names;
        }

        private void InitializeDefaultValues()
        {
            RestoreLevelValues(Levels.FEEL_INI);
            RestoreLevelValues(Levels.PLATFORM_INI);
            RestoreLevelValues(Levels.EMULATOR_INI);
            RestoreLevelValues(Levels.GAMELIST_INI);
            RestoreLevelValues(Levels.LAYOUT_INI);
            FixMissingParams();
        }

        private void RestoreLevelValues(Levels level)
        {
            foreach (var param in paramList.Values)
            {
                if (param.CurrentLevel == level)
                {
                    // if parameter doesn't belong to level, look for its value in upper levels
                    var found = false;
                    if (param.DefaultLevel < level)
                    {
                        var previousLevel = level - 1;
                        while (previousLevel >= Levels.FEEL_INI && !found)
                        {
                            var line = iniFile[(int)previousLevel].FindLast(c => c.StartsWith(param.Name + " "));
                            if (!string.IsNullOrEmpty(line))
                            {
                                var value = line.Substring(param.Name.Length + 1).Trim();
                                param.CurrentLevel = previousLevel;
                                param.Value = value;
                                param.IsChanged = false;
                                found = true;
                            }
                            previousLevel--;
                        }
                    }
                    // if not found, assign default
                    if (!found)
                        param.RestoreDefaults();

                    var property = GetType().GetProperty(param.Name);
                    SetPropertyByReflection(param.Name, param.Value, layout_test_mode);
                }
            }
        }

        public bool LoadConfig()
        {
            return LoadConfig(Levels.FEEL_INI, null);
        }

        public bool LoadConfig(Levels startLevel)
        {
            return LoadConfig(startLevel, null);
        }

        public bool LoadConfig(Levels level, string fileName)
        {
            bool ret = true;
            if (string.IsNullOrEmpty(fileName))
                fileName = GetFileNameFromLevel(level);
            if (string.IsNullOrEmpty(fileName))
                return false;
            if (!File.Exists(fileName))
                CreateFileNameFromLevel(fileName, level);

            iniFile[(int)level].Clear();
            iniFile[(int)level] = ReadFileToList(fileName);

            // optimized parameter loading
            foreach (var line in iniFile[(int)level])
            {
                var cleanLine = line.Trim();
                if (cleanLine != string.Empty && !cleanLine.StartsWith("#"))
                {
                    var paramName = cleanLine.Split(' ')[0];
                    if (cleanLine.Length > paramName.Length)
                    {
                        var value = cleanLine.Substring(paramName.Length + 1).Trim();
                        if (value == "#") value = string.Empty;
                        if (paramList.ContainsKey(paramName))
                        {
                            var param = paramList[paramName];
                            param.CurrentLevel = level;
                            param.Value = value;
                            param.IsChanged = false;
                            SetPropertyByReflection(param.Name, value, layout_test_mode);
                        }
                    }
                }
            }
            //foreach (var param in paramList.Values)
            //{
            //    var line = iniFile[(int)level].FindLast(c => c.StartsWith(param.Name + " "));
            //    if (!string.IsNullOrEmpty(line))
            //    {
            //        var value = line.Substring(param.Name.Length + 1).Trim();
            //        param.CurrentLevel = level;
            //        param.Value = value;
            //        param.IsChanged = false;
            //        SetPropertyByReflection(param.Name, value, layout_test_mode);
            //    }
            //}
            if (level < MAX_LEVEL && current_platform != "all_emu")
                ret &= LoadConfig(level + 1, null);
            else
                FixMissingParams();

            return ret;
        }

        private void CreateFileNameFromLevel(string fileName, Levels level)
        {
            var path = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            var file = File.CreateText(fileName);

            if (level != Levels.ROM_INI)
            {
                file.WriteLine("####################################################################################################");
                file.WriteLine("#                                                                                                  #");
                file.WriteLine("# F.E.E.L. - FrontEnd (Emulator Launcher)                                                          #");
                file.WriteLine("#                                                                                                  #");
                file.WriteLine("# Copyright © FEELTeam 2011-" + DateTime.Now.Year + "                                                                   #");
                file.WriteLine("#                                                                                                  #");
                file.WriteLine("####################################################################################################");
                file.WriteLine("");
            }
            else
            {
                file.WriteLine("# Auto-generated file: add custom configuration params here");
            }

            foreach (var param in paramList.Values)
            {
                if (param.CurrentLevel == level)
                    file.WriteLine(param.Name.PadRight(40) + param.Value);
            }
            file.Close();
        }

        private void SetPropertyByReflection(string paramName, string value, bool layoutTestMode)
        {
            var property = GetType().GetProperty(paramName);
            if (property != null)
            {
                // layout test mode
                if (layoutTestMode && paramName.EndsWith("_backcolor"))
                    value = "200, 64, 200, 150";
                    //value = "150, 150, 150, 100";

                switch (property.PropertyType.Name)
                {
                    case "String":
                        property.SetValue(this, value, null);
                        break;
                    case "Boolean":
                        property.SetValue(this, value == "1" ? true : false, null);
                        break;
                    case "UseMouse":
                        property.SetValue(this, Utils.GetUseMouseFromIniString(value), null);
                        break;
                    case "SmartAsdMode":
                        property.SetValue(this, Utils.GetSmartAsdModeFromIniString(value), null);
                        break;
                    case "TextAlign":
                        property.SetValue(this, Utils.GetTextAlignFromIniString(value), null);
                        break;
                    case "AutostartMode":
                        property.SetValue(this, Utils.GetAutostartFromIniString(value), null);
                        break;
                    case "Color":
                        property.SetValue(this, Utils.GetColorFromIniString(value), null);
                        break;
                    case "ListType":
                        property.SetValue(this, Utils.GetListTypeFromIniString(value), null);
                        break;
                    case "SortType":
                        property.SetValue(this, Utils.GetSortTypeFromIniString(value), null);
                        break;
                    case "FnetSortType":
                        property.SetValue(this, Utils.GetFnetSortTypeFromIniString(value), null);
                        break;
                    case "ScreenSaverType":
                        property.SetValue(this, Utils.GetScreenSaverTypeFromIniString(value), null);
                        break;
                    case "FontStyle":
                        property.SetValue(this, Utils.GetFontStyleFromIniString(value), null);
                        break;
                    case "FeelKey":
                        property.SetValue(this, Utils.GetFeelKeyFromIniString(value), null);
                        break;
                    case "Single":
                        property.SetValue(this, float.Parse(value.Replace(",", "."), CultureInfo.InvariantCulture), null);
                        break;
                    case "Int32":
                        property.SetValue(this, int.Parse(value), null);
                        break;
                    default:
                        property.SetValue(this, Convert.ChangeType(value, property.PropertyType), null);
                        break;
                }
            }
        }

        private List<string> ReadFileToList(string fileName)
        {
            var list = new List<string>();
            if (File.Exists(fileName))
            {
                var file = File.OpenText(fileName);
                while (!file.EndOfStream)
                {
                    var original = file.ReadLine();
                    var line = original.Replace("\t", " ").Trim();
                    if (line.Length == 0 || line.StartsWith("#"))
                    {
                        list.Add(original);
                        continue;
                    }
                    line = Regex.Replace(line, " {2,}", " ");
                    var name = line;
                    var value = "";
                    var index = line.IndexOf(" ");
                    if (index > 0)
                    {
                        name = line.Substring(0, index).ToLower();
                        value = line.Substring(index + 1);
                    }
                    var param = GetParameter(name);
                    if (param != null)
                        list.Add(name.PadRight(40) + value);
                    else
                        list.Add(original);
                }
                file.Close();
            }
            return list;
        }

        public void SaveConfig()
        {
            for (var i = 0; i <= (int)MAX_LEVEL + 1; i++)
            {
                if (current_platform == "all_emu"
                    && (Levels)i > Levels.PLATFORM_INI)
                    break;
                if (IsLevelChanged((Levels)i))
                {
                    var fileName = GetFileNameFromLevel((Levels)i);
                    var file = File.CreateText(fileName);
                    // update existing params
                    foreach (var line in iniFile[i])
                    {
                        if (line.Length == 0 || line.StartsWith("#"))
                        {
                            file.WriteLine(line);
                            continue;
                        }
                        var index = line.IndexOf(" ");
                        if (index > 0)
                        {
                            var name = line.Substring(0, index).ToLower();
                            var value = line.Substring(index + 1);

                            var param = GetParameter(name);
                            if (param != null)
                            {
                                if (param.CurrentLevel == (Levels)i)
                                {
                                    if (param.IsChanged)
                                    {
                                        file.WriteLine(name.PadRight(40) + param.Value);
                                        continue;
                                    }
                                }
                            }
                        }
                        file.WriteLine(line);
                    }
                    // add missing params
                    var firstLine = true;
                    foreach (var param in paramList.Values)
                    {
                        if (param.CurrentLevel == (Levels)i)
                        {
                            var line = iniFile[i].Find(c => c.StartsWith(param.Name + " "));
                            if (!string.IsNullOrEmpty(line))
                                continue;
                            line = iniFile[i].Find(c => c.StartsWith("#" + param.Name + " "));
                            if (!string.IsNullOrEmpty(line) && !param.IsChanged && param.IsOptional)
                                continue;
                            if (firstLine) { file.WriteLine(""); firstLine = false; }
                            if (param.IsChanged || !param.IsOptional)
                                file.WriteLine(param.Name.PadRight(40) + param.Value);
                            else
                                file.WriteLine("#" + param.Name.PadRight(40) + param.Value);
                        }
                    }
                    file.Close();
                }
            }
        }

        public void BackUpState()
        {
            paramListBackUp = new Dictionary<string, Parameter>(paramList.Count);
            foreach (var param in paramList)
                paramListBackUp.Add(param.Key, new Parameter(param.Value));
            iniFileBackUp = new List<string>[iniFile.Length];
            for (var i = 0; i < iniFile.Length; i++)
            {
                iniFileBackUp[i] = new List<string>();
                iniFileBackUp[i].AddRange(iniFile[i].ToArray());
            }
        }

        public void RestoreBackUpState()
        {
            //paramList = new Dictionary<string, Parameter>(paramListBackUp.Count);
            //foreach (var param in paramListBackUp)
            //    paramList.Add(param.Key, param.Value);
            paramList = new Dictionary<string, Parameter>(paramListBackUp);
            iniFile = new List<string>[iniFileBackUp.Length];
            for (var i = 0; i < iniFileBackUp.Length; i++)
            {
                iniFile[i] = new List<string>();
                iniFile[i].AddRange(iniFileBackUp[i].ToArray());
            }
            foreach (var param in paramList.Values)
            {
                // param.IsChanged = false;
                SetPropertyByReflection(param.Name, param.Value, layout_test_mode);
            }
            FixMissingParams();
        }

        private bool IsLevelChanged(Levels level)
        {
            var hasChanged = false;
            foreach (var param in paramList.Values)
            {
                if (param.CurrentLevel == level)
                {
                    // check if a param has changed
                    if (param.IsChanged)
                    {
                        hasChanged = true;
                        break;
                    }
                    // check if a param is missing in its default level file
                    var line = iniFile[(int)level].Find(c => c.StartsWith(param.Name + " "));
                    if (string.IsNullOrEmpty(line))
                    {
                        hasChanged = true;
                        break;
                    }
                }
            }
            return hasChanged;
        }

        public Parameter GetParameter(string name)
        {
            Parameter ret;
            if (paramList.TryGetValue(name.Trim().ToLower(), out ret))
                return ret;
            return null;
        }

        public void SetParameter(string name, string value)
        {
            Parameter ret;
            if (paramList.TryGetValue(name.Trim().ToLower(), out ret))
            {
                ret.Value = value.Trim();
                SetPropertyByReflection(ret.Name, ret.Value, layout_test_mode);
            }
        }

        public string GetFileNameFromParam(string paramName)
        {
            var parameter = GetParameter(paramName);
            if (parameter != null)
                return GetFileNameFromLevel(parameter.CurrentLevel);
            else
                return string.Empty;
        }

        private string GetFileNameFromLevel(Levels configLevel)
        {
            var current_platform = "";
            var current_emulator = "";
            var current_gamelist = "";
            var current_layout = "";
            var current_rom = "";

            if (this.current_platform == "all_emu"
                && configLevel >= Levels.PLATFORM_INI && configLevel <= Levels.GAMELIST_INI)
            {
                return startupPath + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar + "all_emu.ini";
            }

            switch (configLevel)
            {
                case Levels.FEEL_INI:
                    return startupPath  + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar + "feel.ini";
                case Levels.PLATFORM_INI:
                    current_platform = GetParameter("current_platform").Value;
                    if (string.IsNullOrEmpty(current_platform))
                    {
                        // try to set to first platform
                        GetPlatformList();
                        current_platform = GetParameter("current_platform").Value;
                        if (string.IsNullOrEmpty(current_platform))
                        {
                            // no platforms defined => default
                            SetParameter("platform_list", "arcade");
                            SetParameter("platform_title", "Arcade");
                            current_platform = "arcade";
                        }
                    }
                    return startupPath  + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar  + current_platform  + Path.DirectorySeparatorChar  + current_platform + ".ini";
                case Levels.EMULATOR_INI:
                    current_platform = GetParameter("current_platform").Value;
                    current_emulator = GetParameter("current_emulator").Value;
                    if (string.IsNullOrEmpty(current_platform) ||
                        string.IsNullOrEmpty(current_emulator))
                    {
                        // try to set to first platform
                        GetPlatformList();
                        current_platform = GetParameter("current_platform").Value;
                        if (string.IsNullOrEmpty(current_platform))
                        {
                            // no platforms defined => default
                            SetParameter("platform_list", "arcade");
                            SetParameter("platform_title", "Arcade");
                            current_platform = "arcade";
                        }
                        // try to set to first emulator
                        GetEmulatorList();
                        current_emulator = GetParameter("current_emulator").Value;
                        if (string.IsNullOrEmpty(current_emulator))
                        {
                            // no emu defined => default
                            SetParameter("emulator_list", "mame");
                            SetParameter("emulator_title", "M.A.M.E.");
                            current_emulator = "mame";
                        }
                    }
                    return startupPath  + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar  + current_platform  + Path.DirectorySeparatorChar  + current_emulator  + Path.DirectorySeparatorChar  + current_emulator + ".ini";
                case Levels.GAMELIST_INI:
                    current_platform = GetParameter("current_platform").Value;
                    current_emulator = GetParameter("current_emulator").Value;
                    current_gamelist = GetParameter("current_gamelist").Value;
                    if (string.IsNullOrEmpty(current_platform) ||
                        string.IsNullOrEmpty(current_emulator) ||
                        string.IsNullOrEmpty(current_gamelist))
                    {
                        // try to set to first platform
                        GetPlatformList();
                        current_platform = GetParameter("current_platform").Value;
                        if (string.IsNullOrEmpty(current_platform))
                        {
                            // no platforms defined => default
                            SetParameter("platform_list", "arcade");
                            SetParameter("platform_title", "Arcade");
                            current_platform = "arcade";
                        }
                        // try to set to first emulator
                        GetEmulatorList();
                        current_emulator = GetParameter("current_emulator").Value;
                        if (string.IsNullOrEmpty(current_emulator))
                        {
                            // no emu defined => default
                            SetParameter("emulator_list", "mame");
                            SetParameter("emulator_title", "M.A.M.E.");
                            current_emulator = "mame";
                        }
                        // try to set to first gamelist
                        GetGameListList();
                        current_gamelist = GetParameter("current_gamelist").Value;
                        if (string.IsNullOrEmpty(current_gamelist))
                        {
                            // no gamelist defined => default
                            SetParameter("gamelist_list", current_emulator + "-0");
                            SetParameter("gamelist_title", "All Games");
                            current_gamelist = current_emulator + "-0";
                        }
                    }
                    return startupPath  + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar  + current_platform  + Path.DirectorySeparatorChar  + current_emulator  + Path.DirectorySeparatorChar  + current_gamelist + ".ini";
                case Levels.LAYOUT_INI:
                    current_layout = GetParameter("current_layout").Value;
                    if (string.IsNullOrEmpty(current_layout))
                        return "";
                    return startupPath  + Path.DirectorySeparatorChar + "layouts" + Path.DirectorySeparatorChar  + current_layout  + Path.DirectorySeparatorChar + "layout.ini";
                case Levels.ROM_INI:
                    current_platform = GetParameter("current_platform").Value;
                    current_emulator = GetParameter("current_emulator").Value;
                    current_rom = GetParameter("current_game").Value;
                    if (string.IsNullOrEmpty(current_platform) ||
                        string.IsNullOrEmpty(current_emulator) ||
                        string.IsNullOrEmpty(current_rom))
                    {
                        // try to set to first platform
                        GetPlatformList();
                        current_platform = GetParameter("current_platform").Value;
                        if (string.IsNullOrEmpty(current_platform))
                        {
                            // no platforms defined => default
                            SetParameter("platform_list", "arcade");
                            SetParameter("platform_title", "Arcade");
                            current_platform = "arcade";
                        }
                        // try to set to first emulator
                        GetEmulatorList();
                        current_emulator = GetParameter("current_emulator").Value;
                        if (string.IsNullOrEmpty(current_emulator))
                        {
                            // no emu defined => default
                            SetParameter("emulator_list", "mame");
                            SetParameter("emulator_title", "M.A.M.E.");
                            current_emulator = "mame";
                        }
                    }
                    return startupPath + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar + current_platform + Path.DirectorySeparatorChar + current_emulator + Path.DirectorySeparatorChar + "cfg" + Path.DirectorySeparatorChar + current_game + ".ini";
                default:
                    return "";
            }
        }

        public static string GetValueFromIniFile(string fileName, string paramName, string defaultValue)
        {
            var retValue = defaultValue;
            if (File.Exists(fileName))
            {
                var file = File.OpenText(fileName);
                while (!file.EndOfStream)
                {
                    var line = file.ReadLine().Replace("\t", " ").Trim();
                    if (line.Length == 0 || line.StartsWith("#"))
                        continue;
                    line = Regex.Replace(line, " {2,}", " ");
                    var name = line;
                    var value = "";
                    var index = line.IndexOf(" ");
                    if (index > 0)
                    {
                        name = line.Substring(0, index).ToLower();
                        value = line.Substring(index + 1);
                        if (name == paramName.ToLower())
                            retValue = value;
                    }
                }
                file.Close();
            }
            return retValue;
        }

        public List<string> GetPlatformList()
        {
            var fileSystemList = new List<string>(Directory.GetDirectories(startupPath  + Path.DirectorySeparatorChar + "config"));
            for (var i = 0; i < fileSystemList.Count; i++)
                fileSystemList[i] = fileSystemList[i].Substring(fileSystemList[i].LastIndexOf("" + Path.DirectorySeparatorChar ) + 1);

            var list = new List<string>(platform_list.Split(','));
            var newList = new List<string>();

            foreach (var item in list)
            {
                if (fileSystemList.Contains(item.Trim()))
                    newList.Add(item.Trim());
            }

            foreach (var item in fileSystemList)
            {
                if (!newList.Contains(item))
                    newList.Add(item);
            }

            var strList = "";
            foreach (var item in newList)
            {
                if (string.IsNullOrEmpty(strList))
                    strList = item;
                else
                    strList += ", " + item;
            }

            SetParameter("platform_list", strList);
            if (string.IsNullOrEmpty(current_platform) && newList.Count > 0)
                SetParameter("current_platform", newList[0]);

            for (var i = 0; i < newList.Count; i++)
            {
                var platform_name = newList[i].Trim();
                var fileName = startupPath  + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar  + platform_name  + Path.DirectorySeparatorChar  + platform_name + ".ini";
                var platform_title = GetValueFromIniFile(fileName, "platform_title", platform_name);
                newList[i] = platform_name + "|" + platform_title;
            }

            return newList;
        }

        public List<string> GetEmulatorList()
        {
            var fileSystemList = new List<string>(Directory.GetDirectories(startupPath  + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar  + current_platform));
            for (var i = 0; i < fileSystemList.Count; i++)
                fileSystemList[i] = fileSystemList[i].Substring(fileSystemList[i].LastIndexOf("" + Path.DirectorySeparatorChar ) + 1);

            var list = new List<string>(emulator_list.Split(','));
            var newList = new List<string>();

            foreach (var item in list)
            {
                if (fileSystemList.Contains(item.Trim()))
                    newList.Add(item.Trim());
            }

            foreach (var item in fileSystemList)
            {
                if (!newList.Contains(item))
                    newList.Add(item);
            }

            var strList = "";
            foreach (var item in newList)
            {
                if (string.IsNullOrEmpty(strList))
                    strList = item;
                else
                    strList += ", " + item;
            }

            SetParameter("emulator_list", strList);
            if (string.IsNullOrEmpty(current_emulator) && newList.Count > 0)
            {
                // if mame is found in list, default to it, otherwise take first
                var elementNo = newList.Contains("mame") ? newList.IndexOf("mame") : 0;
                SetParameter("current_emulator", newList[elementNo]);
            }

            for (var i = 0; i < newList.Count; i++)
            {
                var emulator_name = newList[i].Trim();
                var fileName = startupPath  + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar  + current_platform  + Path.DirectorySeparatorChar  + emulator_name  + Path.DirectorySeparatorChar  + emulator_name + ".ini";
                var emulator_title = GetValueFromIniFile(fileName, "emulator_title", emulator_name);
                newList[i] = emulator_name + "|" + emulator_title;
            }
            return newList;
        }

        public List<string> GetGameListList()
        {
            var fileSystemList = new List<string>(Directory.GetFiles(startupPath  + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar  + current_platform  + Path.DirectorySeparatorChar  + current_emulator, "*.ini"));
            for (var i = 0; i < fileSystemList.Count; i++)
                fileSystemList[i] = Path.GetFileNameWithoutExtension(fileSystemList[i]);

            var list = new List<string>(gamelist_list.Split(','));
            var newList = new List<string>();

            foreach (var item in list)
            {
                if (fileSystemList.Contains(item.Trim()) && !newList.Contains(item.Trim())) // rimuove i duplicati
                    newList.Add(item.Trim());
            }

            foreach (var item in fileSystemList)
            {
                if (!newList.Contains(item) && item != current_emulator)
                    newList.Add(item);
            }

            var strList = "";
            foreach (var item in newList)
            {
                if (string.IsNullOrEmpty(strList))
                    strList = item;
                else
                    strList += ", " + item;
            }

            SetParameter("gamelist_list", strList);
            if (string.IsNullOrEmpty(current_gamelist) && newList.Count > 0)
                SetParameter("current_gamelist", newList[0]);

            for (var i = 0; i < newList.Count; i++)
            {
                var gamelist_name = newList[i].Trim();
                var fileName = startupPath  + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar  + current_platform  + Path.DirectorySeparatorChar  + current_emulator  + Path.DirectorySeparatorChar  + gamelist_name + ".ini";
                var gamelist_title = GetValueFromIniFile(fileName, "gamelist_title", gamelist_name);
                newList[i] = gamelist_name + "|" + gamelist_title;
            }
            return newList;
        }

        public List<string> ParameterDump()
        {
            var output = new List<string>();
            var counter = 0;

            for (var level = Levels.FEEL_INI; level <= Levels.ROM_INI; level++)
            {
                var singleOut = level.ToString() + "\n";
                foreach (var param in paramList)
                {
                    if (param.Value.CurrentLevel == level)
                    {
                        counter++;
                        singleOut += param.Value.Name + " (" + param.Value.CurrentLevel + ") = " + param.Value.Value + "\n";
                        if (counter == 50)
                        {
                            output.Add(singleOut);
                            counter = 0;
                            singleOut = string.Empty;
                        }
                    }
                }
                if (counter > 0)
                    output.Add(singleOut);
                counter = 0;
            }

            return output;
        }

        public class Parameter
        {
            private string _name;
            private Levels _currentLevel;
            private Levels _defaultLevel;
            private string _defaultValue;
            private string _value;
            private bool _imChanged;
            private bool _imOptional;

            public Parameter(string name, Levels defaultLevel, string defaultValue, bool isOptional)
            {
                _name = name.Trim().ToLower();
                _defaultLevel = defaultLevel;
                _defaultValue = defaultValue;
                _imOptional = isOptional;
                RestoreDefaults();
            }

            public Parameter(Parameter parameter)
            {
                _name = parameter.Name;
                _currentLevel = parameter.CurrentLevel;
                _defaultLevel = parameter.DefaultLevel;
                _defaultValue = parameter.DefaultValue;
                _value = parameter.Value;
                _imChanged = parameter.IsChanged;
                _imOptional = parameter.IsOptional;
            }

            public string Name { get { return _name; } }

            public Levels CurrentLevel { get { return _currentLevel; } set { _currentLevel = value; } }

            public Levels DefaultLevel { get { return _defaultLevel; } }

            public string DefaultValue { get { return _defaultValue; } }

            public string Value { get { return _value; } set { if (_value != value) { _value = value; _imChanged = true; } } }

            public bool IsDefault { get { return (_value == _defaultValue && _currentLevel == _defaultLevel); } }

            public bool IsChanged { get { return _imChanged; } set { _imChanged = value; } }

            public bool IsOptional { get { return _imOptional; } }

            public void RestoreDefaults()
            {
                _currentLevel = _defaultLevel;
                _value = _defaultValue;
                _imChanged = false;
            }
        }
    }
}
