﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfinityScript;

namespace LambAdmin
{
    public partial class DGAdmin
    {
        public static partial class ConfigValues
        {
            public static bool ISNIPE_MODE
            {
                get
                {
                    return bool.Parse(Sett_GetString("settings_isnipe"));
                }
            }

            public static class ISNIPE_SETTINGS
            {
                public static bool ANTIHARDSCOPE
                {
                    get
                    {
                        return bool.Parse(Sett_GetString("settings_isnipe_antihardscope"));
                    }
                }
                public static bool ANTIBOLTCANCEL
                {
                    get
                    {
                        return bool.Parse(Sett_GetString("settings_isnipe_antiboltcancel"));
                    }
                }
                public static bool ANTICRTK
                {
                    get
                    {
                        return bool.Parse(Sett_GetString("settings_isnipe_anticrtk"));
                    }
                }
                public static bool ANTIKNIFE
                {
                    get
                    {
                        return bool.Parse(Sett_GetString("settings_isnipe_antiknife"));
                    }
                }
                public static bool ANTIPLANT
                {
                    get
                    {
                        return bool.Parse(Sett_GetString("settings_isnipe_antiplant"));
                    }
                }
                public static bool ANTIFALLDAMAGE
                {
                    get
                    {
                        return bool.Parse(Sett_GetString("settings_isnipe_antifalldamage"));
                    }
                }
            }
        }

        public void SNIPE_OnServerStart()
        {
            WriteLog.Info("Initializing isnipe settings...");
            SNIPE_InitCommands();
            PlayerActuallySpawned += SNIPE_OnPlayerSpawn;
            OnPlayerDamageEvent += SNIPE_PeriodicChecks;
            PlayerConnected += SNIPE_OnPlayerConnect;

            if (ConfigValues.ISNIPE_SETTINGS.ANTIKNIFE)
            {
                DisableKnife();
                WriteLog.Info("Knife auto-disabled.");
            }

            AfterDelay(5000, () =>
            {
                if (UTILS_GetDvar("g_gametype") == "infect")
                    EnableKnife();
            });

            WriteLog.Info("Done initializing isnipe settings.");
        }

        public void SNIPE_OnPlayerSpawn(Entity player)
        {
            CMD_HideBombIcon(player);
            CMD_GiveMaxAmmo(player);

            if (ConfigValues.ISNIPE_SETTINGS.ANTIPLANT)
                OnInterval(1000, () =>
                {
                    if (!player.IsAlive)
                        return false;
                    if (player.CurrentWeapon.Equals("briefcase_bomb_mp"))
                    {
                        player.TakeWeapon("briefcase_bomb_mp");
                        player.IPrintLnBold(Lang_GetString("Message_PlantingNotAllowed"));
                        return true;
                    }
                    return true;
                });

            if (ConfigValues.ISNIPE_SETTINGS.ANTIHARDSCOPE)
            {
                player.SetField("adscycles", 0);
                player.SetField("letmehardscope", 0);
                OnInterval(50, () =>
                {
                    if (!player.IsAlive)
                        return false;
                    if (player.GetField<int>("letmehardscope") == 1)
                        return true;
                    if (UTILS_GetDvar("g_gametype") == "infect" && player.GetTeam() != "allies")
                        return true;
                    float ads = player.PlayerAds();
                    int adscycles = player.GetField<int>("adscycles");
                    if (ads == 1f)
                        adscycles++;
                    else
                        adscycles = 0;

                    if (adscycles > 5)
                    {
                        player.AllowAds(false);
                        player.IPrintLnBold(Lang_GetString("Message_HardscopingNotAllowed"));
                    }

                    if (!player.AdsButtonPressed() && ads == 0)
                    {
                        player.AllowAds(true);
                    }

                    player.SetField("adscycles", adscycles);
                    return true;
                });
            }
        }

        public void SNIPE_PeriodicChecks(Entity player, Entity inflictor, Entity attacker, int damage, int dFlags, string mod, string weapon, Vector3 point, Vector3 dir, string hitLoc)
        {
            if (ConfigValues.ISNIPE_SETTINGS.ANTIFALLDAMAGE && mod == "MOD_FALLING")
            {
                player.Health += damage;
                return;
            }
            if (!attacker.IsPlayer)
                return;

            if (attacker.HasField("CMD_FLY"))
                if(attacker.IsSpectating() || !attacker.IsAlive)
                    player.Health += damage;

            if (weapon == "iw5_usp45_mp_tactical" && UTILS_GetDvar("g_gametype") == "infect" && attacker.GetTeam() != "allies")
                return;
            //if (ConfigValues.ISNIPE_SETTINGS.ANTINOSCOPE && (UTILS_GetFieldSafe<int>(attacker, "weapon_fired_noscope") == 1))
            //    player.Health += damage;
            if (ConfigValues.ISNIPE_SETTINGS.ANTICRTK && (weapon == "throwingknife_mp") && (attacker.Origin.DistanceTo2D(player.Origin) < 200f))
            {
                player.Health += damage;
                attacker.IPrintLnBold(Lang_GetString("Message_CRTK_NotAllowed"));
            }
            if (ConfigValues.ISNIPE_SETTINGS.ANTIBOLTCANCEL && (UTILS_GetFieldSafe<int>(attacker, "weapon_fired_boltcancel") == 1))
                player.Health += damage;

        }

        public void SNIPE_OnPlayerConnect(Entity player)
        {
            if (ConfigValues.ISNIPE_SETTINGS.ANTIBOLTCANCEL)
                EventDispatcher_AntiBoltCancel(player);

            player.OnNotify("giveLoadout", (ent) =>
            {
                CMD_GiveMaxAmmo(ent);
            });
            if (UTILS_GetDvar("g_gametype") == "infect")
                player.OnNotify("giveLoadout", (ent) =>
                {
                    ent.TakeAllWeapons();
                    switch (ent.GetTeam())
                    {
                        case "allies":
                            ent.GiveWeapon("iw5_l96a1_mp_l96a1scope_xmags");
                            ent.SwitchToWeaponImmediate("iw5_l96a1_mp_l96a1scope_xmags");
                            break;
                        default:
                            try
                            {
                                ent.GiveWeapon("iw5_usp45_mp_tactical");
                                ent.ClearPerks();
                                AfterDelay(100, () =>
                                {
                                    ent.SwitchToWeaponImmediate("iw5_usp45_mp_tactical");
                                });
                                ent.SetWeaponAmmoClip("iw5_usp45_mp_tactical", 0);
                                ent.SetWeaponAmmoStock("iw5_usp45_mp_tactical", 0);
                                ent.SetPerk("specialty_tacticalinsertion", true, true);
                            }
                            catch (Exception)
                            {

                            }
                            break;
                    }
                });
        }

        #region COMMANDS

        public void SNIPE_InitCommands()
        {
            // GA
            CommandList.Add(new Command("ga", 0, Command.Behaviour.Normal,
                (sender, arguments, optarg) =>
                {
                    if (UTILS_GetDvar("g_gametype") == "infect" && sender.GetTeam() == "axis")
                    {
                        WriteChatToPlayer(sender, "I like the way you're thinking, but nope.");
                        return;
                    }
                    CMD_GiveMaxAmmo(sender);
                    WriteChatToPlayer(sender, Command.GetString("ga", "message"));
                }));

            // HIDEBOMBICON
            CommandList.Add(new Command("hidebombicon", 0, Command.Behaviour.Normal,
                (sender, arguments, optarg) =>
                {
                    CMD_HideBombIcon(sender);
                    WriteChatToPlayer(sender, Command.GetString("hidebombicon", "message"));
                }));

            // KNIFE
            CommandList.Add(new Command("knife", 1, Command.Behaviour.Normal,
                (sender, arguments, optarg) =>
                {
                    bool enabled = UTILS_ParseBool(arguments[0]);
                    CMD_knife(enabled);
                    if (enabled)
                        WriteChatToAll(Command.GetString("knife", "message_on"));
                    else
                        WriteChatToAll(Command.GetString("knife", "message_off"));
                }));

            // LETMEHARDSCOPE
            CommandList.Add(new Command("letmehardscope", 1, Command.Behaviour.Normal,
                (sender, arguments, optarg) =>
                {
                    bool state = UTILS_ParseBool(arguments[0]);
                    if (state)
                    {
                        sender.SetField("letmehardscope", 1);
                        WriteChatToPlayer(sender, Command.GetString("letmehardscope", "message_on"));
                    }
                    else
                    {
                        sender.SetField("letmehardscope", 0);
                        WriteChatToPlayer(sender, Command.GetString("letmehardscope", "message_off"));
                    }
                }));
        }

        #region CMDS

        public void CMD_GiveMaxAmmo(Entity player)
        {
            player.GiveMaxAmmo(player.CurrentWeapon);
        }

        public void CMD_HideBombIcon(Entity player)
        {
            player.SetClientDvar("waypointIconHeight", "1");
            player.SetClientDvar("waypointIconWidth", "1");
        }

        public void CMD_knife(bool state)
        {
            if (state)
                EnableKnife();
            else
                DisableKnife();
        }

        #endregion

        #endregion
        //private void EventDispatcher_AntiNoScope(Entity player)
        //{
        //    player.SetField("weapon_fired_noscope", new Parameter((int)0));

        //    player.OnNotify("weapon_fired", new Action<Entity, Parameter>((_player, args) =>
        //    {
        //        if (_player.Call<float>("playerads", new Parameter[0]) == 0f ||
        //            _player.Call<int>("adsbuttonpressed", new Parameter[0]) == 0)
        //        {
        //            string currentWeapon = _player.CurrentWeapon;
        //            if (currentWeapon.Contains("iw5_l96a1") || currentWeapon.Contains("iw5_msr"))
        //            {
        //                player.SetField("weapon_fired_noscope", (int)1);
        //                _player.Call("iprintlnbold", new Parameter[] { Lang_GetString("Message_CRNS_NotAllowed") });
        //                _player.Call("stunplayer", new Parameter[] { 1 });
        //                _player.AfterDelay(2000, new Action<Entity>((__player) =>
        //                {
        //                    __player.SetField("weapon_fired_noscope", (int)0);
        //                    __player.Call("stunplayer", new Parameter[] { 0 });
        //                }));
        //            }
        //        }
        //    }));
        //}

        private void EventDispatcher_AntiBoltCancel(Entity player)
        {
            //Asynchronous hell

            player.SetField("fired", 0);
            player.SetField("weapon_fired_boltcancel", 0);

            player.NotifyOnPlayerCommand("weapon_reloading", "+reload");
            player.OnNotify("weapon_fired", new Action<Entity, Parameter>((_player, args) =>
            {
                _player.SetField("fired", 1);
                AfterDelay(300, () =>
                {
                    _player.SetField("fired", 0);
                });
            }));
            player.OnNotify("weapon_reloading",
                new Action<Entity>((_player) =>
                {
                    if (UTILS_GetFieldSafe<int>(_player, "fired") == 1)
                    {
                        if (UTILS_GetFieldSafe<int>(_player, "weapon_fired_boltcancel") == 0)
                        {
                            _player.SetField("weapon_fired_boltcancel", 1);
                            AfterDelay(2000, () =>
                            {
                                _player.SetField("weapon_fired_boltcancel", 0);
                            });
                        }
                        else
                        {
                            _player.IPrintLnBold(Lang_GetString("Message_BoltCancel_NotAllowed"));
                            _player.AllowAds( false );
                            _player.StunPlayer(1);
                            AfterDelay(300, () => {
                                    _player.AllowAds(true);
                                    _player.SetField("fired", 0);
                                    _player.SetField("weapon_fired_boltcancel", 0);
                                    _player.StunPlayer(0);
                            });
                        }
                    }
                })
            );  
        }
    }
}
