using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;

namespace LambAdmin
{
    public partial class DGAdmin
    {
        public static Dictionary<string, string> Lang = new Dictionary<string, string>();
        public static Dictionary<string, string> Settings = new Dictionary<string, string>();
        public static Dictionary<string, string> CmdLang = new Dictionary<string, string>();

        public static Dictionary<string, string> DefaultLang = new Dictionary<string, string>()
        {
            { "ChatPrefix", "^0[^1DG^0]^7" },
            { "ChatPrefixPM", "^0[^5PM^0]^7" },
            { "ChatPrefixSPY", "^0[^6SPY^0]^7" },
            { "ChatPrefixAdminMSG", "^0[^3ADM^0]^3" },
            { "FormattedNameRank", "<shortrank> <name>" },
            { "FormattedNameRankless", "<name>" },
            { "Message_HardscopingNotAllowed", "^1Hardscoping is not allowed!" },
            { "Message_PlantingNotAllowed", "^1Planting not allowed!" },
            { "Spree_Headshot", "^3<attacker> ^7killed ^3<victim> ^7by ^2 Headshot"},
            { "Spree_Kills_5", "Nice spree, ^2<attacker>! ^7got ^35 ^7kills in a row."},
            { "Spree_Kills_10", "Nice spree, ^2<attacker>! ^7got ^310 ^7kills in a row!"},
            { "Spree_Ended", "^2<victim>'s ^7killing spree ended (^3<killstreak> ^7kills). He was killed by ^3<attacker>!"},
            { "Spree_Explosivekill", "^3<victim> ^7has exploded!"},
            { "Spree_Trophykill", "^1L^2O^1L^9Z!!!! ^3<attacker> ^7killed ^3<victim> ^7by ^2Trophy^1!!!"},
            { "Spree_KnifeKill", "^2<attacker> ^3humiliated ^5<victim>"}
        };

        public static Dictionary<string, string> DefaultCDvars = new Dictionary<string, string>();

        public static Dictionary<long,string> ChatAlias = new Dictionary<long,string>();

        public static Dictionary<string, string> DefaultSettings = new Dictionary<string, string>()
        {
            { "settings_isnipe", "true" },
            { "settings_isnipe_antiplant", "true" },
            { "settings_isnipe_antihardscope", "true" },
            { "settings_isnipe_antiknife", "true" },
            { "settings_isnipe_antifalldamage", "true" },
            { "settings_isnipe_antiweaponhack", "true" },
            { "settings_teamnames_allies", "^0[^1DG^0] ^7Clan" },
            { "settings_teamnames_axis", "^7Noobs" },
            { "settings_teamicons_allies", "cardicon_weed" },
            { "settings_teamicons_axis", "cardicon_thebomb" },
            { "settings_enable_connectmessage", "false" },
            { "format_connectmessage", "^3<player> ^7connected." },
            { "settings_enable_misccommands", "true" },
            { "settings_maxwarns", "3" },
            { "settings_groups_autosave", "true" },
            { "settings_enable_spy_onlogin", "false" },
            { "settings_showversion", "true" },
            { "settings_adminshudelem", "true"},
            { "settings_unfreezeongameend", "true" },
            { "settings_betterbalance_enable", "true" },
            { "settings_betterbalance_message", "^3<player> ^2got teamchanged for balance." },
            { "settings_enable_autofpsunlock", "true" },
            { "settings_enable_dlcmaps", "true" },
            { "settings_enable_chat_alias", "true" },
            { "settings_enable_spree_messages", "true"}
        };

        public static Dictionary<string, string> DefaultCmdLang = new Dictionary<string, string>()
        {
            #region MESSAGES

            {"Message_NotOnePlayerFound", "^1No or more players found under that criteria." },
            {"Message_TargetIsImmune", "^1Target is immune." },
            {"Message_NotOneMapFound", "^1No or more maps found under that criteria." },
            {"Message_GroupNotFound", "^1No group was found under that name." },
            {"Message_GroupsSaved", "^2Groups configuration saved." },
            {"Message_PlayerIsSpectating", "^1Player is spectating." },
            {"Message_InvalidTeamName", "^1Invalid team name." },
            {"Message_DSRNotFound", "^1DSR file not found." },
            {"Message_InvalidTimeSpan", "^1Invalid time span." },
            {"Message_InvalidSearch", "^1Invalid search term(s)" },
            {"Message_NoPermission", "^1You do not have permission to do that." },
            {"Message_CommandNotFound", "^1Command not found." },
            {"Message_YouHaveBeenWarned", "^1You have been warned!" },
            {"Message_YouHaveBeenUnwarned", "^2You have been unwarned!" },
            {"Message_NotLoggedIn", "^1You need to log in first." },
            {"Message_InvalidNumber", "^1Invalid number." },
            {"Message_DefaultError", "^1Something went wrong. Check console for more details." },
            {"Message_NoEntriesFound", "^1No entries found." },
            {"Message_blockedByNightMode", "^1You cant use this command when night mode is active" },

            #endregion

            {"command_version_usage", "^1Usage: !version" },
            {"command_credits_usage", "^1Usage: !credits" },

            {"command_pm_usage", "^1Usage: !pm <player> <message>" },
            {"command_pm_message", "^1<sender>^0: ^2<message>" },
            {"command_pm_confirmation", "^2PM SENT." },

            {"command_admins_usage", "^1Usage: !admins" },
            {"command_admins_firstline", "^1Online Admins: ^7" },
            {"command_admins_formatting", "<formattedname>" },
            {"command_admins_separator", "^7, " },

            {"command_status_usage", "^1Usage: !status [filter]" },
            {"command_status_firstline", "^3Online players:" },
            {"command_status_formatting", "^1<id>^0 : ^7<namef>" },
            {"command_status_separator", "^7, " },

            {"command_login_usage", "^1Usage: !login <password>" },
            {"command_login_alreadylogged", "^1You are already logged in." },
            {"command_login_successful", "^2You have successfully logged in." },
            {"command_login_wrongpassword", "^1Wrong password." },
            {"command_login_notrequired", "^2Login is not required." },

            {"command_kick_usage", "^1Usage: !kick <player> [reason]" },
            {"command_kick_message", "^3<target>^7 was ^5kicked^7 by ^1<issuer>^7. Reason: ^6<reason>" },

            {"command_tmpban_usage", "^1Usage: !tmpban <player> [reason]" },
            {"command_tmpban_message", "^3<target>^7 was ^4tmpbanned^7 by ^1<issuer>^7. Reason: ^6<reason>" },

            {"command_ban_usage", "^1Usage: !ban <player> [reason]" },
            {"command_ban_message", "^3<target>^7 was ^1banned^7 by ^1<issuer>^7. Reason: ^6<reason>" },

            {"command_say_usage", "^1Usage: !say <message>" },

            {"command_sayto_usage", "^1Usage: !sayto <player>, <message>" },

            {"command_map_usage", "^1Usage: !map <mapname>" },
            {"command_map_message", "^5Map was changed by ^1<player>^5 to ^2<mapname>^5." },

            {"command_guid_usage", "^1Usage: !guid" },
            {"command_guid_message", "^1Your GUID: ^5<guid>" },

            {"command_warn_usage", "^1Usage: !warn <player> [reason]" },
            {"command_warn_message", "^3<target>^7 was ^3warned (<warncount>/<maxwarns>)^7 by ^1<issuer>^7. Reason: ^6<reason>"},

            {"command_unwarn_usage", "^1Usage: !unwarn <player> [reason]" },
            {"command_unwarn_message", "^3<target>^7 was ^2unwarned (<warncount>/<maxwarns>)^7 by ^1<issuer>^7. Reason: ^6<reason>" },

            {"command_resetwarns_usage", "^1Usage: !resetwarns <player> [reason]" },
            {"command_resetwarns_message","^3<target>^7 had his warnings ^2reset ^7by ^1<issuer>^7. Reason: ^6<reason>" },

            {"command_getwarns_usage", "^1Usage: !getwarns <player>" },
            {"command_getwarns_message", "^1<target>^7 has ^3(<warncount>/<maxwarns>) ^7warnings."},

            {"command_addimmune_usage", "^1Usage: !addimmune <player>" },
            {"command_addimmune_message", "^2<target>^5 has been added to the immune group by ^2<issuer>" },

            {"command_unimmune_usage", "^1Usage: !unimmune <player>" },
            {"command_unimmune_message", "^2<target>^5 has been ^1removed^5 from the immune group by ^2<issuer>" },

            {"command_setgroup_usage", "^1Usage: !setgroup <player> <groupname/default>" },
            {"command_setgroup_message", "^2<target> ^5has been added to group ^1<rankname> ^5by ^2<issuer>" },

            {"command_savegroups_usage", "^1Usage: !savegroups" },
            {"command_savegroups_message", "^2Groups have been saved." },

            {"command_res_usage", "^1Usage: !res" },

            {"command_getplayerinfo_usage", "^1Usage: !getplayerinfo <player>" },
            {"command_getplayerinfo_message", "^1<target>^7:^3<id>^7, ^5<guid>^7, ^2<ip>, ^5<hwid>" },

            {"command_balance_usage", "^1Usage: !balance" },
            {"command_balance_message", "^2Teams have been balanced." },
            {"command_balance_teamsalreadybalanced", "^1Teams are already balanced." },

            {"command_afk_usage", "^1Usage: !afk" },

            {"command_setafk_usage", "^1Usage: !setafk <player>" },

            {"command_setteam_usage", "^1Usage: !setteam <player> <axis/allies/spectator>" },
            {"command_setteam_message", "^2<target>^5's team has been changed by ^1<issuer>^5." },

            {"command_clanvsall_usage", "^1Usage: !clanvsall <matches...>" },
            {"command_clanvsallspectate_usage" , "^1Usage: !clanvsallspectate <matches...>" },
            {"command_clanvsall_message", "^1<issuer>^5 used ^2clanvsall ^5with terms ^3<identifiers>" },

            {"command_cdvar_usage", "^1Usage: !cdvar <type> <key> <value>" },
            {"command_cdvar_message", "^5Dvar (^1<type>^5)^3<key> ^5= ^3<value>" },

            {"command_mode_usage", "^1Usage: !mode <DSR>" },
            {"command_mode_message", "^5DSR was changed by ^1<issuer>^5 to ^2<dsr>^5." },

            {"command_gametype_usage", "^1Usage: !gametype <DSR> <mapname>" },
            {"command_gametype_message", "^5Game changed to map ^3<mapname>^5, DSR ^3<dsr>" },

            {"command_server_usage", "^1Usage: !server <cmd>" },
            {"command_server_message", "^5Command executed: ^3<command>" },

            {"command_tmpbantime_usage", "^1Usage: !tmpbantime <minutes> <player> [reason]" },
            {"command_tmpbantime_message", "^3<target> ^7was tmpbanned by ^1<issuer> ^7for ^5<timespan>^7. Reason: ^6<reason>" },

            {"command_pban_usage", "^1Usage: !pban <player>" },
            {"command_pban_message", "^3<target> ^7 was ^1permanently banned ^7by ^1<issuer>" },

            {"command_unban_usage", "^1Usage: !unban <playerinfo>" },
            {"command_unban_message", "^3Ban entry removed." },

            {"command_lastbans_usage", "^1Usage: !lastbans [amount]" },
            {"command_lastbans_firstline", "^2Last <nr> bans:" },
            {"command_lastbans_message", "^1<banid>: <name>, <guid>, <ip>, <hwid>, <time>" },

            {"command_searchbans_usage", "^1Usage: !searchbans <name/playerinfo>" },
            {"command_searchbans_firstline", "^2Search results:" },
            {"command_searchbans_message", "^1<banid>: <name>, <guid>, <ip>, <hwid>, <time>" },

            {"command_help_usage", "^1Usage: !help [command]" },
            {"command_help_firstline", "^5Available commands:" },

            {"command_cleartmpbanlist_usage", "^1Usage: !cleartmpbanlist" },

            {"command_rage_usage", "^1Usage: !rage" },
            {"command_rage_message", "^3<issuer> ^5ragequit." },
            {"command_rage_kickmessage", "RAGEEEEEEEEE" },
            {"command_rage_custommessagenames", "lambder,juice,future,hotshot,destiny,peppah,moskvish,myst" },
            {"command_rage_message_lambder", "^3<issuerf> ^5went back to fucking coding." },
            {"command_rage_message_juice", "^3<issuer> ^5squeezed outta here." },
            {"command_rage_message_future", "^3<issuer> ^5ragequit again." },
            {"command_rage_message_hotshot", "^3<issuer> ^5hit the shitty road." },
            {"command_rage_message_destiny", "^3<issuer> ^5resterino in pepperonis." },
            {"command_rage_message_peppah", "^3<issuer> ^5went back to the fap cave." },
            {"command_rage_message_moskvish", "^3<issuer> ^5is done with this fucking lag." },
            {"command_rage_message_myst", "^3<issuer> ^5will rek you scrubs later." },

            {"command_loadgroups_usage", "^1Usage: !loadgroups" },
            {"command_loadgroups_message", "^2Groups configuration loaded." },

            {"command_maps_usage", "^1Usage: !maps" },
            {"command_maps_firstline", "^2Available maps:" },

            {"command_time_usage", "^1Usage: !time" },
            {"command_time_message", "^2Time: {0:HH:mm:ss}" },

            {"command_yell_usage", "^1Usage: !yell <player|all> <message>" },

            {"command_changeteam_usage", "^1Usage: !changeteam <player>" },

            {"command_whois_usage", "^1Usage: !whois <player>" },
            {"command_whois_firstline", "^3All known names for player ^4<target>^3:" },
            {"command_whois_separator", "^1, ^7" },

            {"command_end_usage", "^1Usage: !end" },
            {"command_end_message", "^2Game ended by ^3<issuer>" },

            {"command_foreach_usage", "^1Usage: !foreach <include self> <command>" },

            {"command_spy_usage", "^1Usage: !spy <on|off>" },
            {"command_spy_message_on", "^0Spy mode ^2enabled"},
            {"command_spy_message_off", "^0Spy mode ^1disabled" },

            {"command_amsg_usage", "^1Usage: !amsg <message>" },
            {"command_amsg_message", "^7<senderf>^7: ^3<message>" },
            {"command_amsg_confirmation", "^3Your message will be read by all online admins." },

            {"command_ga_usage", "^1Usage: !ga"},
            {"command_ga_message", "^5Ammo given." },

            {"command_hidebombicon_usage", "^1Usage: !hidebombicon" },
            {"command_hidebombicon_message", "^5Bomb icons hidden." },

            {"command_knife_usage", "^1Usage: !knife <on|off>" },
            {"command_knife_message_on", "^2Knife enabled." },
            {"command_knife_message_off", "^1Knife disabled." },

            {"command_letmehardscope_usage", "^1Usage: !letmehardscope <on/off>" },
            {"command_letmehardscope_message_on", "^5Hardscoping enabled for you. NEWB." },
            {"command_letmehardscope_message_off", "^5Hardscoping disabled for you." },

            {"command_freeze_usage", "^1Usage: !freeze <player>" },
            {"command_freeze_message", "^3<target> ^7was frozen by ^1<issuer>" },

            {"command_unfreeze_usage", "^1Usage: !unfreeze <player>" },
            {"command_unfreeze_message", "^3<target> ^7was ^0unfrozen ^7by ^1<issuer>" },

            {"command_mute_usage", "^1Usage: !mute <player>" },
            {"command_mute_message", "^3<target>^7 was ^:muted^7 by ^1<issuer>^7." },

            {"command_unmute_usage", "^1Usage: !unmute <player>" },
            {"command_unmute_message", "^3<target>^7 was ^;unmuted^7 by ^1<issuer>^7." },

            {"command_kill_usage", "^1Usage: !kill <player>" },

            {"command_ft_usage", "^1Usage: !ft <0-10>" },
            {"command_ft_message", "^3FilmTweak ^2<ft> ^3applied." },

            {"command_scream_usage", "^1Usage: !scream <message>" },

            {"command_kickhacker_usage", "^1Usage: !kickhacker <full name>" },

            {"command_fakesay_usage", "^1Usage: !fakesay <player> <message>" },

            {"command_silentban_usage", "^1Usage: !silentban <player>" },
            {"command_silentban_message", "^3Player added to banlist. Will be kicked next game." },

            {"command_hwid_usage", "^1Usage: !hwid" },
            {"command_hwid_message", "^1Your HWID: ^5<hwid>" },

            {"command_rek_usage", "^1Usage: !rek <player>" },
            {"command_rek_message", "^3<target>^7 was ^:REKT^7 by ^1<issuer>^7." },

            {"command_rektroll_usage", "^1Usage: !rektroll <player>" },

            {"command_clankick_usage", "^1Usage: !clankick <player>" },
            {"command_clankick_kickmessage", "You have been kicked from the clan. You can remove clantag and reconnect." },
            {"command_clankick_message", "^2<target> ^7was ^5CLANKICKED ^7by ^3<sender>^7." },

            {"command_nootnoot_usage", "^1Usage: !nootnoot <player>" },
            {"command_nootnoot_message_on", "^3<target> ^5was nootnooted." },
            {"command_nootnoot_message_off", "^3<target> ^5was ^1unnootnooted." },

            {"command_betterbalance_usage", "^1Usage: !autobalance <off/on>" },
            {"command_betterbalance_message_on", "^3BetterBalance is now ^2enabled^3." },
            {"command_betterbalance_message_off", "^3BetterBalance is now ^1disabled^3." },

            {"command_xban_usage", "^1Usage: !xban <player> [reason]" },
            {"command_xban_message", "^5<target> ^7has been ^0xbanned ^7by ^2<issuer>^7. Reason: ^6" },

            {"command_dbsearch_usage", "^1Usage: !dbsearch <player>" },
            {"command_dbsearch_message_firstline", "^3<nr> entries found!" },
            {"command_dbsearch_message_found", "^3<playerinfo>" },
            {"command_dbsearch_message_notfound", "^1Player info not found in the database." },

            {"command_ac130_usage", "^1Usage: !ac130 <player>" },
            {"command_ac130_message", "^7AC130 Given to ^1<target>^7." },

            {"command_fixplayergroup_usage", "^1Usage: !fixplayergroup <player>" },
            {"command_fixplayergroup_message", "^2User group fixed." },
            {"command_fixplayergroup_notfound", "^1User IDs not found in the database." },

            {"command_sunlight_usage", "^1Usage: !sunlight <float R> <float G> <float B>"},

            {"command_night_usage", "^1Usage: !night <on|off>"},

            {"command_alias_usage", "^1Usage: !alias <player> [alias]"},
            {"command_alias_reset", "^2<player> ^7alias has been ^2reset"},
            {"command_alias_message", "^2<player> ^7alias has been set to «<alias>^7»"},
            {"command_alias_disabled", "^1Chat alias feature disabled in settings"},

            {"command_myalias_usage", "^1Usage: !myalias [alias]"},

            {"command_daytime_usage", "^1Usage: !daytime <day|night|morning|cloudy>"},

            {"command_kd_usage", "^Usage: !kd <player> <kills> <deaths>"},
            {"command_kd_message","^2<player>'s ^7KD has been set to ^1<kills>^7/^1<deaths>"} 
        };

        public static void CFG_ReadConfig()
        {
            WriteLog.Info("Reading config...");
            if (!File.Exists(ConfigValues.ConfigPath + @"settings.txt") || !File.Exists(ConfigValues.ConfigPath + @"lang.txt") || !File.Exists(ConfigValues.ConfigPath + @"cmdlang.txt"))
                CFG_CreateConfig();
            CFG_ReadDictionary(ConfigValues.ConfigPath + @"settings.txt", ref Settings);
            CFG_ReadDictionary(ConfigValues.ConfigPath + @"lang.txt", ref Lang);
            CFG_ReadDictionary(ConfigValues.ConfigPath + @"cmdlang.txt", ref CmdLang);

            WriteLog.Info("Done reading config...");
        }

        public static void CFG_CreateConfig()
        {
            WriteLog.Warning("Config files not found. Creating new ones...");

            CFG_WriteDictionary(DefaultSettings, ConfigValues.ConfigPath + @"settings.txt");

            CFG_WriteDictionary(DefaultLang, ConfigValues.ConfigPath + @"lang.txt");

            CFG_WriteDictionary(DefaultCmdLang, ConfigValues.ConfigPath + @"cmdlang.txt");
        }

        public static string Lang_GetString(string key)
        {
            string value;
            if (!Lang.TryGetValue(key, out value))
            {
                string defval;
                if (DefaultLang.TryGetValue(key, out defval))
                    return defval;
                else
                    throw new Exception("Settings string not found");
            }
            return value;
        }

        public static string Sett_GetString(string key)
        {
            string value;
            if (!Settings.TryGetValue(key, out value))
            {
                string defval;
                if (DefaultSettings.TryGetValue(key, out defval))
                    return defval;
                else
                    throw new Exception("Setting string not found");
            }
            return value;
        }

        public static string CmdLang_GetString(string key)
        {
            string value;
            if (!CmdLang.TryGetValue(key, out value))
            {
                string defval;
                if (DefaultCmdLang.TryGetValue(key, out defval))
                    return defval;
                else
                    throw new Exception("Language string not found");
            }
            return value;
        }

        public static bool CmdLang_HasString(string key)
        {
            if (CmdLang.Keys.Contains(key) || DefaultCmdLang.Keys.Contains(key))
            {
                return true;
            }
            return false;
        }

        public static void CFG_OnServerStart()
        {
            CFG_ReadConfig();
        }

        public static void CFG_WriteDictionary(Dictionary<string, string> dict, string path)
        {
            List<string> lines = new List<string>();
            foreach (KeyValuePair<string, string> pair in dict)
            {
                lines.Add(string.Join("=", pair.Key, pair.Value));
            }
            File.WriteAllLines(path, lines.ToArray());
        }

        public static void CFG_ReadDictionary(string path, ref Dictionary<string, string> dict)
        {
            foreach (string line in File.ReadAllLines(path))
            {
                int index = line.IndexOf('=');
                if (index == line.Length || index == 0)
                    continue;
                string key = line.Substring(0, index);
                string value = line.Substring(index + 1);
                dict[key] = value;
            }
        }
    }
}
