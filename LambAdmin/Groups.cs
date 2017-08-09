using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using InfinityScript;

namespace LambAdmin
{
    public partial class DGAdmin
    {
        public static partial class ConfigValues
        {
            public static string formatting_onlineadmins = "^1Online Admins: ^7";
            public static string formatting_eachadmin = "{0} {1}";
        }

        private volatile GroupsDatabase database;

        public class GroupsDatabase
        {
            public volatile List<PlayerInfo> ImmunePlayers = new List<PlayerInfo>();
            public volatile List<Group> Groups = new List<Group>();
            public volatile Dictionary<PlayerInfo, string> Players = new Dictionary<PlayerInfo, string>();

            public class Group
            {
                public List<string> permissions = new List<string>();
                public string group_name;
                public string login_password;
                public string short_name;

                public Group(string name, string password, string short_name = "")
                {
                    group_name = name.ToLowerInvariant();
                    login_password = password;
                }

                public Group(string name, string password, List<string> perms, string sh_name = "")
                {
                    group_name = name.ToLowerInvariant();
                    login_password = password;
                    short_name = sh_name;
                    permissions = perms;
                }

                public bool CanDo(string permission)
                {
                    List<string> abusive = new List<string>() {
                        "freeze",       "unfreeze",   /*"mute",         "unmute",*/
                        "kill",         "sunlight",     "rotatescreen", "fakesay",
                        "rek",          "rektroll",     "nootnoot",     "daytime",
                        "kd",           "setfx",        "hell",         "fire",
                        "3rdperson",    "teleport",     "fly",          "jump",
                        "speed",        "gravity",      "ac130",        /*"setclantag",*/
                        "letmehardscope","moab"
                    };
                    List<string> _unsafe = new List<string>() {
                        "sdvar",        "server",       /*"setclantag",*/   "fc",
                        "foreach",      "svpassword"
                    };
                    if (permissions.Contains("-" + permission))
                        return false;
                    if (permissions.Contains("-*abusive*") && abusive.Contains(permission))
                        return false;
                    if (permissions.Contains("-*unsafe*")  && _unsafe.Contains(permission))
                        return false;
                    if (permissions.Contains(permission) || permissions.Contains("*all*"))
                        return true;
                    return false;
                }
            }

            public GroupsDatabase()
            {
                WriteLog.Info("Reading groups...");

                Directory.CreateDirectory(ConfigValues.ConfigPath + @"Groups");

                if (!File.Exists(ConfigValues.ConfigPath + @"Groups\groups.txt"))
                {
                    WriteLog.Warning("Groups file not found, creating new one...");
                    File.WriteAllLines(ConfigValues.ConfigPath + @"Groups\groups.txt", new string[]
                    {
                        "default::pm,admins,guid,version,rules,afk,credits,hidebombicon,help,rage,maps,dsrnames,time,amsg,ft,hwid,apply,night,ga,report,suicide,yes,no,register,xlrstats,xlrtop,votekick,drunk,fx",
                        "moderator::login,warn,unwarn,kick,mode,map,setafk,kick,tmpban,changeteam,lastreports,@admins,@rules,@apply,@time,@xlrstats,@xlrtop:^0[^5Moderator^0]^7",
                        "family::kickhacker,kill,mute,unmute,end,tmpbantime,cdvar,getplayerinfo,say,sayto,resetwarns,setgroup,scream,whois,changeteam,yell,gametype,mode,login,map,status,kick,tmpban,ban,warn,unwarn,getwarns,res,setafk,setteam,balance,clanvsall,clanvsallspectate,sunlight,alias,lastreports,fire,@admins,@rules,@apply,@time,@xlrstats,@xlrtop:^0[^3F^0]^7",
                        "elder:password:-*unsafe*,*all*:^0[^4Elder^0]^7",
                        "developer:password:*all*:^0[^;Dev^0]^1",
                        "owner:password:*all*:^0[^1O^2w^3n^4e^5r^0]^3",
                        "admin::scream,whois,changeteam,yell,gametype,mode,login,map,status,unban,unban-id,kick,tmpban,ban,warn,unwarn,getwarns,res,setafk,setteam,balance,clanvsall,clanvsallspectate,login,lastreports,@admins,@rules,@apply,@time,@xlrstats,@xlrtop:^0[^1Admin^0]^7",
                        "leader:password:*all*:^0[^1L^2e^3a^4d^5e^7r^0]^2",
                        "trial::login,warn,unwarn,kick:^0[^5Trial^0]^7",
                        "member::login,warn,unwarn,kick,mode,map,setafk,kick,tmpban,lastreports,@admins,@rules,@apply,@time,@xlrstats,@xlrtop:^0[^5Member^0]^7",
                        "friend::login,warn,unwarn,kick,mode,map,setafk,kick,tmpban,map,mode,tmpban,lastreports,@admins,@rules,@apply,@time,@xlrstats,@xlrtop:^0[^6Friend^0]^7",
                        "vip::ban,kick,tmpban,warn,unwarn,map,balance,mode,whois,status,login,setafk,changeteam,scream,fakesay,myalias,fire,@admins,@rules,@apply,@time,@xlrstats,@xlrtop:^0[^3V.I.P.^0]^7",
                        "founder:password:*all*:^0[^1F^2o^3u^4n^5d^6e^8r^0]^6",
                        "donator::kick,warn,tmpban,unwarn,mute,unmute,login,balance,setafk,changeteam,myalias,lastreports,fire,@admins,@rules,@apply,@time,@xlrstats,@xlrtop:^0[^2Donator^0]^7",
                        "coleader:password:-*abusive*,-*unsafe*,*all*:^0[^3CoLeader^0]^7"
                    });
                }

                if (!File.Exists(ConfigValues.ConfigPath + @"Groups\players.txt"))
                {
                    WriteLog.Warning("Players file not found, creating new one...");
                    File.WriteAllLines(ConfigValues.ConfigPath + @"Groups\players.txt", new string[0]);
                }

                if (!File.Exists(ConfigValues.ConfigPath + @"Groups\immuneplayers.txt"))
                {
                    WriteLog.Warning("Immuneplayers file not found, creating new one...");
                    File.WriteAllLines(ConfigValues.ConfigPath + @"Groups\immuneplayers.txt", new string[0]);
                }

                try
                {
                    foreach (string line in File.ReadAllLines(ConfigValues.ConfigPath + @"Groups\groups.txt"))
                    {
                        string[] parts = line.Split(':');
                        string[] permissions = parts[2].ToLowerInvariant().Split(',');
                        if (parts.Length == 3)
                            Groups.Add(new Group(parts[0].ToLowerInvariant(), parts[1], permissions.ToList()));
                        else if (parts.Length == 4)
                            Groups.Add(new Group(parts[0].ToLowerInvariant(), parts[1], permissions.ToList(), parts[3]));
                        else
                            Groups.Add(new Group(parts[0].ToLowerInvariant(), parts[1], permissions.ToList(), string.Join(":", parts.Skip(3).ToList())));
                    }
                }
                catch (Exception ex)
                {
                    WriteLog.Error("Could not set up groups.");
                    WriteLog.Error(ex.Message);
                }

                try
                {
                    foreach (string line in File.ReadAllLines(ConfigValues.ConfigPath + @"Groups\players.txt"))
                    {
                        string[] parts = line.ToLowerInvariant().Split(':');
                        Players.Add(PlayerInfo.Parse(parts[0]), parts[1].ToLowerInvariant());
                    }
                }
                catch (Exception ex)
                {
                    WriteLog.Error("Could not set up playergroups.");
                    WriteLog.Error(ex.Message);
                }

                try
                {
                    foreach (string line in File.ReadAllLines(ConfigValues.ConfigPath + @"Groups\immuneplayers.txt"))
                    {
                        ImmunePlayers.Add(PlayerInfo.Parse(line));
                    }
                }
                catch (Exception ex)
                {
                    WriteLog.Error("Could not set up immuneplayers");
                    WriteLog.Error(ex.Message);
                }

                if (ConfigValues.DEBUG)
                    foreach (string message in GetGroupScheme())
                        WriteLog.Debug(message);

                if (!Directory.Exists(ConfigValues.ConfigPath + @"Groups\internal"))
                    Directory.CreateDirectory(ConfigValues.ConfigPath + @"Groups\internal");

                if (!File.Exists(ConfigValues.ConfigPath + @"Groups\internal\loggedinplayers.txt"))
                    File.WriteAllLines(ConfigValues.ConfigPath + @"Groups\internal\loggedinplayers.txt", new string[0]);
                WriteLog.Info("Groups successfully set up.");
            }

            public void SaveGroups()
            {
                using (StreamWriter groupsfile = new StreamWriter(ConfigValues.ConfigPath + @"Groups\groups.txt"))
                {
                    foreach (Group group in Groups)
                    {
                        if (group.short_name == "")
                            groupsfile.WriteLine(string.Join(":", group.group_name, group.login_password, string.Join(",", group.permissions.ToArray())));
                        else
                            groupsfile.WriteLine(string.Join(":", new string[] { group.group_name, group.login_password, string.Join(",", group.permissions.ToArray()), group.short_name }));
                    }
                }
                using (StreamWriter playersfile = new StreamWriter(DGAdmin.ConfigValues.ConfigPath + "Groups\\players.txt"))
                {
                    foreach (KeyValuePair<DGAdmin.PlayerInfo, string> keyValuePair in Players)
                        playersfile.WriteLine(keyValuePair.Key.getIdentifiers() + ":" + keyValuePair.Value);
                }
                using (StreamWriter immuneplayersfile = new StreamWriter(DGAdmin.ConfigValues.ConfigPath + "Groups\\immuneplayers.txt"))
                {
                    foreach (DGAdmin.PlayerInfo playerInfo in ImmunePlayers)
                        immuneplayersfile.WriteLine(playerInfo.getIdentifiers());
                }
            }

            public string[] GetGroupScheme()
            {
                List<string> list = new List<string>() {
                    "GroupScheme:"
                };

                foreach (Group group in Groups)
                {
                    list.Add(string.Format("Name: {0}, password: {1}, displayname: {2}", group.group_name, group.login_password, group.short_name));
                    foreach (string str in group.permissions)
                        list.Add("    " + str);
                }
                return list.ToArray();
            }

            public bool TryGetGroup(string name, out Group gottengroup)
            {
                gottengroup = GetGroup(name);
                return gottengroup != null;
            }

            public Group GetGroup(string name)
            {
                foreach (Group group in Groups)
                {
                    if (group.group_name == name.ToLowerInvariant())
                        return group;
                }
                return null;
            }

            public KeyValuePair<PlayerInfo, string>? FindEntryFromPlayersAND(PlayerInfo playerinfo)
            {
                foreach (KeyValuePair<PlayerInfo, string> keyValuePair in Players)
                {
                    if (playerinfo.MatchesAND(keyValuePair.Key))
                        return keyValuePair;
                }
                return null;
            }

            //CHANGE
            public KeyValuePair<PlayerInfo, string>? FindEntryFromPlayersOR(PlayerInfo playerinfo)
            {
                foreach (KeyValuePair<PlayerInfo, string> keyValuePair in Players)
                {
                    if (playerinfo.MatchesOR(keyValuePair.Key))
                        return keyValuePair;
                }
                return null;
            }

            public PlayerInfo FindMatchingPlayerFromImmunes(PlayerInfo playerinfo)
            {
                WriteLog.Debug("Finding immune status for playerinfo " + playerinfo.getIdentifiers());
                foreach (PlayerInfo B in ImmunePlayers)
                {
                    WriteLog.Debug("    " + B.getIdentifiers());
                    if (playerinfo.MatchesAND(B))
                        return B;
                }
                WriteLog.Debug("Found none");
                return null;
            }

            public bool GetEntityPermission(Entity player, string permission_string)
            {
                WriteLog.Debug("Getting Entity permission for " + player.Name.ToString() + " permission string = " + permission_string);

                Group group = player.GetGroup(this);
                WriteLog.Debug("playergroup acquired");
                if (GetGroup("default").permissions.Contains(permission_string))
                {
                    WriteLog.Debug("Default contained...");
                    return true;
                }

                if (!player.isLogged() && !string.IsNullOrWhiteSpace(group.login_password))
                {
                    WriteLog.Debug("Player not logged");
                    return false;
                }

                if (group.CanDo(permission_string))
                {
                    WriteLog.Debug("Player's group permission authorized");
                    return true;
                }

                WriteLog.Debug("Not authorized");
                return false;
            }

            public string[] GetAdminsString(List<Entity> Players)
            {
                return (from player in Players
                        let grp = player.GetGroup(this)
                        where !string.IsNullOrWhiteSpace(grp.short_name)
                        select Command.GetString("admins", "formatting").Format(new Dictionary<string, string>()
                        {
                            {"<name>", player.Name },
                            {"<formattedname>", player.GetFormattedName(this) },
                            {"<rankname>", grp.group_name },
                            {"<shortrank>", grp.short_name },
                        })).ToArray();
            }
        }

        public void groups_OnDisconnect(Entity player)
        {
            player.setLogged(false);
        }

        public void groups_OnServerStart()
        {
            PlayerDisconnected += groups_OnDisconnect;

            database = new GroupsDatabase();
        }
    }

    public static partial class EntityExtensions
    {
        public static DGAdmin.PlayerInfo GetInfo(this Entity entity)
        {
            return new DGAdmin.PlayerInfo(entity);
        }

        public static DGAdmin.GroupsDatabase.Group GetGroup(this Entity entity, DGAdmin.GroupsDatabase database)
        {
            KeyValuePair<DGAdmin.PlayerInfo, string>? playerFromGroups = database.FindEntryFromPlayersAND(entity.GetInfo());
            if (playerFromGroups == null)
                return database.GetGroup("default");
            DGAdmin.GroupsDatabase.Group grp = database.GetGroup(playerFromGroups.Value.Value);
            if (grp != null)
                return grp;
            else
            {
                DGAdmin.WriteLog.Error("# Player " + entity.Name + ": GUID=" + entity.GUID + ", HWID = " + entity.GetHWID().ToString() + ", IP:" + entity.IP.ToString());
                DGAdmin.WriteLog.Error("# Is in nonexistent group: " + playerFromGroups);
                return database.GetGroup("default");
            }
        }

        public static bool isLogged(this Entity entity)
        {
            return File.ReadAllLines(DGAdmin.ConfigValues.ConfigPath + @"Groups\internal\loggedinplayers.txt").ToList().Contains(entity.GetInfo().getIdentifiers());
        }

        public static void setLogged(this Entity entity, bool state)
        {
            List<string> loggedinfile = File.ReadAllLines(DGAdmin.ConfigValues.ConfigPath + @"Groups\internal\loggedinplayers.txt").ToList();
            string identifiers = entity.GetInfo().getIdentifiers();
            bool isalreadylogged = loggedinfile.Contains(identifiers);

            if (isalreadylogged && !state)
            {
                loggedinfile.Remove(identifiers);
                File.WriteAllLines(DGAdmin.ConfigValues.ConfigPath + @"Groups\internal\loggedinplayers.txt", loggedinfile.ToArray());
                return;
            }
            if (!isalreadylogged && state)
            {
                loggedinfile.Add(identifiers);
                File.WriteAllLines(DGAdmin.ConfigValues.ConfigPath + @"Groups\internal\loggedinplayers.txt", loggedinfile.ToArray());
                return;
            }
        }

        public static bool isImmune(this Entity entity, DGAdmin.GroupsDatabase database)
        {
            return database.FindMatchingPlayerFromImmunes(entity.GetInfo()) != null;
        }

        public static void setImmune(this Entity entity, bool state, DGAdmin.GroupsDatabase database)
        {
            DGAdmin.PlayerInfo playerFromImmunes = database.FindMatchingPlayerFromImmunes(entity.GetInfo());
            if (playerFromImmunes == null && state)
                database.ImmunePlayers.Add(entity.GetInfo());
            if (playerFromImmunes != null && !state)
                database.ImmunePlayers.Remove(playerFromImmunes);
            return;
        }

        public static bool HasPermission(this Entity player, string permission_string, DGAdmin.GroupsDatabase database)
        {
            if (DGAdmin.ConfigValues.DEBUG && DGAdmin.ConfigValues.DEBUGOPT.PERMSFORALL)
                return true;
            return database.GetEntityPermission(player, permission_string);
        }

        public static bool SetGroup(this Entity player, string groupname, DGAdmin.GroupsDatabase database)
        {
            groupname = groupname.ToLowerInvariant();
            player.setLogged(false);
            if (database.GetGroup(groupname) == null)
                return false;
            var matchedplayerinfo = database.FindEntryFromPlayersAND(player.GetInfo());
            if (matchedplayerinfo != null)
            {
                if (groupname == "default")
                {
                    database.Players.Remove(matchedplayerinfo.Value.Key);
                }
                else
                    database.Players[matchedplayerinfo.Value.Key] = groupname;
            }
            else if (groupname != "default")
                database.Players[player.GetInfo()] = groupname;
            return true;
        }

        //CHANGE
        public static bool FixPlayerIdentifiers(this Entity player, DGAdmin.GroupsDatabase database)
        {
            player.setLogged(false);
            var matchedplayerinfo = database.FindEntryFromPlayersOR(player.GetInfo());
            if (matchedplayerinfo != null)
            {
                database.Players.Remove(matchedplayerinfo.Value.Key);
                database.Players[DGAdmin.PlayerInfo.CommonIdentifiers(player.GetInfo(), matchedplayerinfo.Value.Key)] = matchedplayerinfo.Value.Value;
                return true;
            }
            return false;
        }

        public static string GetFormattedName(this Entity player, DGAdmin.GroupsDatabase database)
        {
            DGAdmin.GroupsDatabase.Group grp = player.GetGroup(database);
            var alias = "";
            if (DGAdmin.ChatAlias.Keys.Contains(player.GUID))
                alias = DGAdmin.ChatAlias[player.GUID];
            if (!string.IsNullOrWhiteSpace(grp.short_name))
                return DGAdmin.Lang_GetString("FormattedNameRank").Format(new Dictionary<string, string>()
                {
                    { "<shortrank>", grp.short_name },
                    { "<rankname>",  grp.group_name},
                    { "<name>", (alias != "")?alias : player.Name },
                });
            return DGAdmin.Lang_GetString("FormattedNameRankless").Format(new Dictionary<string, string>()
                {
                    { "<name>", (alias != "")?alias : player.Name },
                });
        }
    }

}
