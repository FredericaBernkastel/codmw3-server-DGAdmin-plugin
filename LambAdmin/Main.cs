using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfinityScript;
using System.IO;
using System.Net;

namespace LambAdmin
{
    public partial class DGAdmin : BaseScript
    {
        public static partial class ConfigValues
        {

        }

        event Action<Entity> PlayerActuallySpawned = ent => { };
        event Action<Entity, Entity, Entity, int, int, string, string, Vector3, Vector3, string> OnPlayerDamageEvent = (t1, t2, t3, t4, t5, t6, t7, t8, t9, t10) => { };
        event Action<Entity, Entity, Entity, int, string, string, Vector3, string> OnPlayerKilledEvent = (t1, t2, t3, t4, t5, t6, t7, t8) => { };
        event Action OnGameEnded = () => { };

        public DGAdmin()
            : base()
        {
            WriteLog.Info("RGAdmin is starting...");
            MainLog.WriteInfo("RGAdmin starting...");

            if (!Directory.Exists(ConfigValues.ConfigPath))
            {
                WriteLog.Info("Creating directory...");
                Directory.CreateDirectory(ConfigValues.ConfigPath);
            }

            #region MODULE LOADING
            MAIN_OnServerStart();
            CFG_OnServerStart();
            UTILS_OnServerStart();
            CMDS_OnServerStart();
            groups_OnServerStart();
            if (ConfigValues.ISNIPE_MODE)
            {
                SNIPE_OnServerStart();
            }
            PersonalPlayerDvars = UTILS_PersonalPlayerDvars_load();
            if (ConfigValues.settings_enable_chat_alias)
                InitChatAlias();
            if (ConfigValues.settings_enable_xlrstats)
            {
                XLR_OnServerStart();
                XLR_InitCommands();
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
            if (ConfigValues.settings_enable_xlrstats)
                xlr_database.Save();
            UTILS_PersonalPlayerDvars_save(PersonalPlayerDvars);
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

            //Array.ForEach(new string[] { }, UTILS_GetDefCDvar);
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
    }

    public static partial class Extensions
    {
        public static bool isConnecting(this Entity player)
        {
            return player.HasField("isConnecting") && player.GetField<int>("isConnecting") == 1;
        }
    }
}