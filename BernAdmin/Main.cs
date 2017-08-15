using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfinityScript;
using System.IO;
using System.Net;
using System.Threading;

namespace LambAdmin
{
    public partial class DGAdmin : BaseScript
    {

        public static partial class ConfigValues
        {
            public static string sv_current_dsr = "";
        }

        event Action<Entity> PlayerActuallySpawned = ent => { };
        event Action<Entity, Entity, Entity, int, int, string, string, Vector3, Vector3, string> OnPlayerDamageEvent = (t1, t2, t3, t4, t5, t6, t7, t8, t9, t10) => { };
        event Action<Entity, Entity, Entity, int, string, string, Vector3, string> OnPlayerKilledEvent = (t1, t2, t3, t4, t5, t6, t7, t8) => { };
        event Action OnGameEnded = () => { };

        public DGAdmin()
            : base()
        {
            WriteLog.Info("DGAdmin is starting...");
            MainLog.WriteInfo("DGAdmin starting...");

            if (!Directory.Exists(ConfigValues.ConfigPath))
            {
                WriteLog.Info("Creating directory...");
                Directory.CreateDirectory(ConfigValues.ConfigPath);
            }

            #region MODULE LOADING
            MAIN_OnServerStart();
            CFG_OnServerStart();

            Action init_settings = () =>
            {
                UTILS_OnServerStart();
                CMDS_OnServerStart();
                groups_OnServerStart();
                if (ConfigValues.ISNIPE_MODE)
                {
                    WriteLog.Debug("Initializing iSnipe mode...");
                    SNIPE_OnServerStart();
                }
                WriteLog.Debug("Initializing PersonalPlayerDvars...");
                PersonalPlayerDvars = UTILS_PersonalPlayerDvars_load();
                if (ConfigValues.settings_enable_chat_alias)
                {
                    WriteLog.Debug("Initializing Chat aliases...");
                    InitChatAlias();
                }
                if (ConfigValues.settings_enable_alive_counter)
                    PlayerConnected += hud_alive_players;
                if (ConfigValues.settings_enable_xlrstats)
                {
                    WriteLog.Debug("Initializing XLRStats...");
                    XLR_OnServerStart();
                    XLR_InitCommands();
                }

                if (ConfigValues.settings_servertitle)
                    if (ConfigValues.LockServer)
                        UTILS_ServerTitle("^1::LOCKED", "^1" + File.ReadAllText(ConfigValues.ConfigPath + @"Utils\internal\LOCKSERVER"));
                    else
                        UTILS_ServerTitle_MapFormat();
            };


            if (ConfigValues.settings_dynamic_properties)
                Delay(50, () =>
                {
                    WriteLog.Debug("Sleep(50)");
                    ConfigValues.sv_current_dsr = UTILS_GetDSRName();
                    WriteLog.Debug("dsr: " + ConfigValues.sv_current_dsr);
                    CFG_Dynprop_Init();
                    init_settings();
                });
            else
            {
                init_settings();

                if (ConfigValues.ANTIWEAPONHACK)
                    WriteLog.Info("You have to enable \"settings_dynamic_properties\" if you wish to use antiweaponhack");
            }
            #endregion

        }

        public override EventEat OnSay3(Entity player, ChatType type, string name, ref string message)
        {
            if (!message.StartsWith("!") || type != ChatType.All)
            {
                MainLog.WriteInfo("[CHAT:" + type + "] " + player.Name + ": " + message);
                
                CHAT_WriteChat(player, type, message);
                return EventEat.EatGame;
            }

            if (message.ToLowerInvariant().StartsWith("!login"))
            {
                string line = "[SPY] " + player.Name + " : !login ****";
                WriteLog.Info(line);
                MainLog.WriteInfo(line);
                CommandsLog.WriteInfo(line);
            }
            else
            {
                string line = "[SPY] " + player.Name + " : " + message;
                WriteLog.Info(line);
                MainLog.WriteInfo(line);
                CommandsLog.WriteInfo(line);
            }
            ProcessCommand(player, name, message);
            return EventEat.EatGame;
        }

        public override void OnStartGameType()
        {
            MAIN_ResetSpawnAction();
            base.OnStartGameType();
        }

        public override void OnExitLevel()
        {
            WriteLog.Info("Saving groups...");
            database.SaveGroups();

            if (!ConfigValues.SettingsMutex)
            {
                // Save xlr stats
                if (ConfigValues.settings_enable_xlrstats)
                {
                    WriteLog.Info("Saving xlrstats...");
                    xlr_database.Save(this);
                }

                WriteLog.Info("Saving PersonalPlayerDvars...");
                // Save FilmTweak settings
                UTILS_PersonalPlayerDvars_save(PersonalPlayerDvars);

                ConfigValues.SettingsMutex = false;
            }

            MAIN_ResetSpawnAction();
            base.OnExitLevel();
        }

        public override void OnPlayerDamage(Entity player, Entity inflictor, Entity attacker, int damage, int dFlags, string mod, string weapon, Vector3 point, Vector3 dir, string hitLoc)
        {
            OnPlayerDamageEvent(player, inflictor, attacker, damage, dFlags, mod, weapon, point, dir, hitLoc);
            base.OnPlayerDamage(player, inflictor, attacker, damage, dFlags, mod, weapon, point, dir, hitLoc);
        }

        public override void OnPlayerKilled(Entity player, Entity inflictor, Entity attacker, int damage, string mod, string weapon, Vector3 dir, string hitLoc)
        {
            OnPlayerKilledEvent(player, inflictor, attacker, damage, mod, weapon, dir, hitLoc);
            base.OnPlayerKilled(player, inflictor, attacker, damage, mod, weapon, dir, hitLoc);
        }

        public void MAIN_OnServerStart()
        {
            WriteLog.Info("Setting up internal stuff...");

            PlayerConnected += e =>
            {
                e.SetField("spawnevent", 0);
                e.OnInterval(100, (ent) =>
                {
                    if (ent.IsAlive)
                    {
                        if (!ent.HasField("spawnevent") || ent.GetField<int>("spawnevent") == 0)
                        {
                            PlayerActuallySpawned(ent);
                            ent.SetField("spawnevent", 1);
                        }
                    }
                    else
                        ent.SetField("spawnevent", 0);
                    return true;
                });
            };

            OnNotify("game_ended", level =>
            {
                OnGameEnded();
            });

            PlayerConnected += MAIN_OnPlayerConnect;
            PlayerDisconnected += MAIN_OnPlayerDisconnect;
            PlayerConnecting += MAIN_OnPlayerConnecting;
            OnPlayerDamageEvent += ANTIWEAPONHACK;

            // CUSTOM EVENTS

            MAIN_ResetSpawnAction();

            WriteLog.Info("Done doing internal stuff.");
        }

        public void MAIN_OnPlayerConnecting(Entity player)
        {
            player.SetField("isConnecting", 1);
            player.SetClientDvar("didyouknow", "get REKT naba");
        }

        public void MAIN_OnPlayerConnect(Entity player)
        {
            try
            {
                player.SetField("spawnevent", 0);
                player.SetField("isConnecting", 0);
                GroupsDatabase.Group playergroup = player.GetGroup(database);
                WriteLog.Info("# Player " + player.Name + " from group \"" + playergroup.group_name + "\" connected.");
                WriteLog.Info("# GUID: " + player.GUID.ToString() + " IP: " + player.IP.ToString());
                WriteLog.Info("# HWID: " + player.GetHWID() + " ENTREF: " + player.GetEntityNumber());
                if (string.IsNullOrEmpty(player.GetXNADDR().Value))
                    throw new Exception("Bad xnaddr");
                WriteLog.Info("# XNADDR(12): " + player.GetXNADDR().ToString());
                if (!player.IsPlayer || player.GetHWID().IsBadHWID())
                    throw new Exception("Invalid entref/hwid");
            }
            catch (Exception)
            {
                WriteLog.Info("# Haxor connected. Could not retrieve/set player info. Kicking...");
                try
                {
                    HaxLog.WriteInfo("----STARTREPORT----");
                    HaxLog.WriteInfo("BAD PLAYER");
                    HaxLog.WriteInfo(player.ToString());
                }
                catch (Exception ex)
                {
                    HaxLog.WriteInfo("ERROR ON TOSTRING");
                    HaxLog.WriteInfo(ex.ToString());
                }
                finally
                {
                    HaxLog.WriteInfo("----ENDREPORT----");
                }
                AfterDelay(100, () =>
                {
                    ExecuteCommand("dropclient " + player.GetEntityNumber() + " \"Something went wrong. Please restart TeknoMW3 and try again.\"");
                });
            }

            UTILS_SetCliDefDvars(player);

            if (bool.Parse(Sett_GetString("settings_enable_connectmessage")) == true)
            {
                WriteChatToAll(Sett_GetString("format_connectmessage").Format(new Dictionary<string, string>()
                {
                    { "<player>", player.Name },
                    { "<playerf>", player.GetFormattedName(database) },
                    { "<clientnumber>", player.GetEntityNumber().ToString() },
                    { "<hour>", DateTime.Now.Hour.ToString() },
                    { "<min>", DateTime.Now.Minute.ToString() },
                    { "<rank>",  player.GetGroup(database).group_name.ToString() }
                }));
            }

            string line = "[CONNECT] " + string.Format("{0} : {1}, {2}, {3}, {4}, {5}", player.Name.ToString(), player.GetEntityNumber().ToString(), player.GUID, player.IP.Address.ToString(), player.GetHWID().Value, player.GetXNADDR().ToString());
            line.LogTo(PlayersLog, MainLog);
        }

        public void MAIN_OnPlayerDisconnect(Entity player)
        {
            player.SetField("spawnevent", 0);

            string line = "[DISCONNECT] " + string.Format("{0} : {1}, {2}, {3}, {4}, {5}", player.Name.ToString(), player.GetEntityNumber().ToString(), player.GUID, player.IP.Address.ToString(), player.GetHWID().Value, player.GetXNADDR().ToString());
            line.LogTo(PlayersLog, MainLog);
        }

        public void MAIN_ResetSpawnAction()
        {
            foreach (Entity player in Players)
                player.SetField("spawnevent", 0);
        }

        private void hud_alive_players(Entity player)
        {
            HudElem fontString1 = HudElem.CreateFontString(player, "hudbig", 0.6f);
            fontString1.SetPoint("DOWNRIGHT", "DOWNRIGHT", -19, 60);
            fontString1.SetText("^3Allies^7:");
            fontString1.HideWhenInMenu = true;
            HudElem fontString2 = HudElem.CreateFontString(player, "hudbig", 0.6f);
            fontString2.SetPoint("DOWNRIGHT", "DOWNRIGHT", -19, 80);
            fontString2.SetText("^1Enemy^7:");
            fontString2.HideWhenInMenu = true;
            HudElem hudElem2 = HudElem.CreateFontString(player, "hudbig", 0.6f);
            hudElem2.SetPoint("DOWNRIGHT", "DOWNRIGHT", -8, 60);
            hudElem2.HideWhenInMenu = true;
            HudElem hudElem3 = HudElem.CreateFontString(player, "hudbig", 0.6f);
            hudElem3.SetPoint("DOWNRIGHT", "DOWNRIGHT", -8, 80);
            hudElem3.HideWhenInMenu = true;
            this.OnInterval(50, (Func<bool>)(() =>
            {
                string str1 = (string)player.GetField<string>("sessionteam");
                string str2 = ((int)this.Call<int>("getteamplayersalive", new Parameter[1]
                    {
            "axis"
                    })).ToString();
                string str3 = ((int)this.Call<int>("getteamplayersalive", new Parameter[1]
                    {
             "allies"
                    })).ToString();
                hudElem2.SetText(str1.Equals("allies") ? str3 : str2);
                hudElem3.SetText(str1.Equals("allies") ? str2 : str3);
                return true;
            }));
        }
    }

    public static partial class Extensions
    {
        public static bool isConnecting(this Entity player)
        {
            return player.HasField("isConnecting") && player.GetField<int>("isConnecting") == 1;
        }
    }
}