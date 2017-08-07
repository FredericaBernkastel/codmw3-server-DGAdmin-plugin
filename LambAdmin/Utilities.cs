using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfinityScript;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace LambAdmin
{
    public partial class DGAdmin
    {
        System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;

        SLOG MainLog = new SLOG("main");
        SLOG PlayersLog = new SLOG("players");
        SLOG CommandsLog = new SLOG("commands");
        SLOG HaxLog = new SLOG("haxor");

        HudElem RGAdminMessage;
        HudElem OnlineAdmins;
        HudElem VoteStatsHUD = null;

        //typedef
        public struct Dvar
        {
            [XmlAttribute]
            public string key;
            [XmlAttribute]
            public string value;
        }
        public class Dvars : List<Dvar>{};
        //-------
        public static partial class ConfigValues
        {
#if DEBUG
            public static string Version = "v3.4n19d";
#else
            public static string Version = "v3.4n19r";
#endif
            public static string ConfigPath = @"scripts\DGAdmin\";
            public static string ChatPrefix
            {
                get
                {
                    return Lang_GetString("ChatPrefix");
                }
            }
            public static string ChatPrefixPM
            {
                get
                {
                    return Lang_GetString("ChatPrefixPM");
                }
            }
            public static string ChatPrefixSPY
            {
                get
                {
                    return Lang_GetString("ChatPrefixSPY");
                }
            }
            public static string ChatPrefixAdminMSG
            {
                get
                {
                    return Lang_GetString("ChatPrefixAdminMSG");
                }
            }
            public static string settings_teamnames_allies
            {
                get
                {
                    return Sett_GetString("settings_teamnames_allies");
                }
            }
            public static string settings_teamnames_axis
            {
                get
                {
                    return Sett_GetString("settings_teamnames_axis");
                }
            }
            public static string settings_teamicons_allies
            {
                get
                {
                    return Sett_GetString("settings_teamicons_allies");
                }
            }
            public static string settings_teamicons_axis
            {
                get
                {
                    return Sett_GetString("settings_teamicons_axis");
                }
            }
#if DEBUG
            public static bool DEBUG = true;
#else
            public static bool DEBUG = false;
#endif
            public static Dictionary<string, string> AvailableMaps = Data.StandardMapNames;
            public static class DEBUGOPT
            {
                public static bool PERMSFORALL = false;
            }
        }

        public static class Data
        {
            public static int HWIDOffset = 0x4A30335;
            public static int HWIDDataSize = 0x78688;

            public static int XNADDROffset = 0x049EBD00;
            public static int XNADDRDataSize = 0x78688;

            public static int ClantagOffset = 0x01AC5564;
            public static int ClantagPlayerDataSize = 0x38A4;

            public static List<char> HexChars = new List<char>()
            {
                'a', 'b', 'c', 'd', 'e', 'f', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            };

            public static Dictionary<string, string> Colors = new Dictionary<string, string>()
            {
                {"^1", "red"},
                {"^2", "green"},
                {"^3", "yellow"},
                {"^4", "blue"},
                {"^5", "lightblue"},
                {"^6", "purple"},
                {"^7", "white"},
                {"^8", "defmapcolor"},
                {"^9", "grey"},
                {"^0", "black"},
                {"^;", "yaleblue"},
                {"^:", "orange"}
            };
            public static Dictionary<string, string> StandardMapNames = new Dictionary<string, string>()
            {
                {"dome", "mp_dome"},
                {"mission", "mp_bravo"},
                {"lockdown", "mp_alpha"},
                {"bootleg", "mp_bootleg"},
                {"hardhat", "mp_hardhat"},
                {"bakaara", "mp_mogadishu"},
                {"arkaden", "mp_plaza2"},
                {"carbon", "mp_carbon"},
                {"fallen", "mp_lambeth"},
                {"outpost", "mp_radar"},
                {"downturn", "mp_exchange"},
                {"interchange", "mp_interchange"},
                {"resistance", "mp_paris"},
                {"seatown", "mp_seatown"},
                {"village", "mp_village"},
                {"underground", "mp_underground"}
            };

            public static Dictionary<string, string> DLCMapNames = new Dictionary<string, string>()
            {
                {"piazza", "mp_piazza"},
                {"liberation", "mp_italy"},
                {"blackbox", "mp_plane"},
                {"overwatch", "mp_overwatch"},
                {"aground", "mp_aground_ss"},
                {"erosion", "mp_courtyard_ss"},
                {"foundation", "mp_cement"},
                {"getaway", "mp_hillside_ss"},
                {"sanctuary", "mp_museum"},
                {"oasis", "mp_qadeem"},
                {"lookout", "mp_restrepo_ss"},
                {"terminal", "mp_terminal_cls"},
                {"intersection", "mp_crosswalk_ss"},
                {"u-turn", "mp_burn_ss"},
                {"vortex", "mp_six_ss"},
                {"gulch", "mp_moab"},
                {"boardwalk", "mp_boardwalk"},
                {"parish", "mp_parish"},
                {"offshore", "mp_roughneck"},
                {"decommision", "mp_shipbreaker"}   
            };
            public static Dictionary<string, string> AllMapNames = StandardMapNames.Concat(DLCMapNames).GroupBy(d => d.Key).ToDictionary(d => d.Key, d => d.First().Value);
            public static List<string> TeamNames = new List<string>()
            {
                "axis", "allies", "spectator"
            };
        }

        public static class WriteLog
        {
            public static void Info(string message)
            {
                Log.Write(LogLevel.Info, message);
            }

            public static void Error(string message)
            {
                Log.Write(LogLevel.Error, message);
            }

            public static void Warning(string message)
            {
                Log.Write(LogLevel.Warning, message);
            }

            public static void Debug(string message)
            {
                if (ConfigValues.DEBUG)
                    Log.Write(LogLevel.Debug, message);
            }
        }

        public static class Mem
        {
            public static unsafe string ReadString(int address, int maxlen = 0)
            {
                string ret = "";
                maxlen = (maxlen == 0) ? int.MaxValue : maxlen;
                for (; address < address + maxlen && *(byte*)address != 0; address++)
                {
                    ret += Encoding.ASCII.GetString(new byte[] { *(byte*)address });
                }
                return ret;
            }

            public static unsafe void WriteString(int address, string str)
            {
                byte[] strarr = Encoding.ASCII.GetBytes(str);
                foreach (byte ch in strarr)
                {
                    *(byte*)address = ch;
                    address++;
                }
                *(byte*)address = 0;
            }
        }

        public class SLOG
        {
            string path = ConfigValues.ConfigPath + @"Logs\";
            string filepath;
            bool notify;

            public SLOG(string filename, bool NotifyIfFileExists = false)
            {
                if (!System.IO.Directory.Exists(ConfigValues.ConfigPath + @"Logs"))
                    System.IO.Directory.CreateDirectory(ConfigValues.ConfigPath + @"Logs");
                path += filename;
                notify = NotifyIfFileExists;
            }

            private void CheckFile()
            {
                filepath = path + " " + DateTime.Now.ToString("yyyy MM dd") + ".log";
                if (!System.IO.File.Exists(filepath))
                    System.IO.File.WriteAllLines(filepath, new string[]
                    {
                        "---- LOG FILE CREATED ----",
                    });
                if (notify)
                    System.IO.File.AppendAllLines(filepath, new string[]
                    {
                        "---- INSTANCE CREATED ----",
                    });
            }

            public void WriteInfo(string message)
            {
                WriteMsg("INFO", message);
            }

            public void WriteError(string message)
            {
                WriteMsg("ERROR", message);
            }

            public void WriteWarning(string message)
            {
                WriteMsg("WARNING", message);
            }
            public void WriteMsg(string prefix, string message)
            {
                CheckFile();
                using (System.IO.StreamWriter file = System.IO.File.AppendText(filepath))
                {
                    file.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [" + prefix + "] " + message);
                }
            }
        }

        public class Announcer
        {
            List<string> message_list;
            public int message_interval;
            string name;

            public Announcer(string announcername, List<string> messages, int interval = 40000)
            {
                message_interval = interval;
                message_list = messages;
                name = announcername;
            }

            public string SpitMessage()
            {
                int currentmsg = GetStep();
                string messagetobespit = message_list[currentmsg];
                if (++currentmsg >= message_list.Count)
                    currentmsg = 0;
                SetStep(currentmsg);
                return messagetobespit;
            }

            public int GetStep()
            {
                string path = ConfigValues.ConfigPath + @"Utils\internal\announcers\" + name + ".txt";
                if (System.IO.File.Exists(path))
                    try
                    {
                        return int.Parse(System.IO.File.ReadAllText(path));
                    }
                    catch
                    {
                        System.IO.File.Delete(path);
                    }
                return 0;
            }

            public void SetStep(int step)
            {
                System.IO.File.WriteAllLines(ConfigValues.ConfigPath + @"Utils\internal\announcers\" + name + ".txt", new string[] { step.ToString() });
            }
        }

        public class HWID
        {
            public string Value
            {
                get; private set;
            }

            public HWID(Entity player)
            {
                if (player == null || !player.IsPlayer)
                {
                    Value = null;
                    return;
                }
                int address = Data.HWIDDataSize * player.GetEntityNumber() + Data.HWIDOffset;
                string formattedhwid = "";
                unsafe
                {
                    for (int i = 0; i < 12; i++)
                    {
                        if (i % 4 == 0 && i != 0)
                            formattedhwid += "-";
                        formattedhwid += (*(byte*)(address + i)).ToString("x2");
                    }
                }
                Value = formattedhwid;
            }

            private HWID(string value)
            {
                Value = value;
            }

            public bool IsBadHWID()
            {
                return string.IsNullOrWhiteSpace(Value) || Value == "00000000-00000000-00000000";
            }

            public override string ToString()
            {
                return Value;
            }

            public static bool TryParse(string str, out HWID parsedhwid)
            {
                str = str.ToLowerInvariant();
                if (str.Length != 26)
                {
                    parsedhwid = new HWID((string)null);
                    return false;
                }
                for (int i = 0; i < 26; i++)
                {
                    if (i == 8 || i == 17)
                    {
                        if (str[i] != '-')
                        {
                            parsedhwid = new HWID((string)null);
                            return false;
                        }
                        continue;
                    }
                    if (!str[i].IsHex())
                    {
                        parsedhwid = new HWID((string)null);
                        return false;
                    }
                }
                parsedhwid = new HWID(str);
                return true;
            }
        }

        public class XNADDR
        {
            public string Value
            {
                get; private set;
            }

            public XNADDR(Entity player)
            {
                if (player == null || !player.IsPlayer)
                {
                    Value = null;
                    return;
                }
                string connectionstring = Mem.ReadString(Data.XNADDRDataSize * player.GetEntityNumber() + Data.XNADDROffset, Data.XNADDRDataSize);
                string[] parts = connectionstring.Split('\\');
                for (int i = 1; i < parts.Length; i++)
                {
                    if (parts[i - 1] == "xnaddr")
                    {
                        Value = parts[i].Substring(0, 12);
                        return;
                    }
                }
                Value = null;
            }

            public override string ToString()
            {
                return Value;
            }

            public static bool TryParse(string str, out XNADDR parsedxnaddr)
            {
                str = str.ToLowerInvariant();
                if (str.Length != 12)
                {
                    parsedxnaddr = null;
                    return false;
                }
                for (int i = 0; i < 12; i++)
                {
                    if (!str[i].IsHex())
                    {
                        parsedxnaddr = null;
                        return false;
                    }
                }
                parsedxnaddr = null;
                return true;
            }
        }

        public class PlayerInfo
        {
            private string player_ip = null;
            private long? player_guid = null;
            private HWID player_hwid = null;

            public PlayerInfo(Entity player)
            {
                player_ip = player.IP.Address.ToString();
                player_guid = player.GUID;
                player_hwid = player.GetHWID();
            }

            private PlayerInfo()
            {

            }

            public bool MatchesAND(PlayerInfo B)
            {
                if (B.isNull() || isNull())
                    return false;
                return
                    (B.player_ip == null || player_ip == B.player_ip) &&
                    (B.player_guid == null || player_guid.Value == B.player_guid.Value) &&
                    (B.player_hwid == null || player_hwid.Value == B.player_hwid.Value);
            }

            public bool MatchesOR(PlayerInfo B)
            {
                if (B.isNull() || isNull())
                    return false;
                if ((player_ip != null && B.player_ip != null) && player_ip == B.player_ip)
                    return true;
                if ((player_guid != null && B.player_guid != null) && player_guid.Value == B.player_guid.Value)
                    return true;
                if ((player_hwid != null && B.player_hwid != null) && player_hwid.Value == B.player_hwid.Value)
                    return true;
                return false;
            }

            public void addIdentifier(string identifier)
            {
                long result;
                if (long.TryParse(identifier, out result))
                {
                    player_guid = result;
                    return;
                }
                IPAddress address;
                if (IPAddress.TryParse(identifier, out address))
                {
                    player_ip = address.ToString();
                    return;
                }
                HWID possibleHWID;
                if (HWID.TryParse(identifier, out possibleHWID))
                {
                    player_hwid = possibleHWID;
                    return;
                }
            }

            public static PlayerInfo Parse(string str)
            {
                PlayerInfo pi = new PlayerInfo();
                string[] parts = str.Split(',');
                foreach (string part in parts)
                    pi.addIdentifier(part);
                return pi;
            }

            public string getIdentifiers()
            {
                List<string> identifiers = new List<string>();
                if (player_guid != null)
                    identifiers.Add(player_guid.ToString());
                //if (player_ip != null)
                //    identifiers.Add(player_ip.ToString());
                if (player_hwid != null)
                    identifiers.Add(player_hwid.ToString());
                return string.Join(",", identifiers);
            }

            public bool isNull()
            {
                return player_ip == null && !player_guid.HasValue && player_hwid == null;
            }

            public override string ToString()
            {
                return getIdentifiers();
            }

            public string GetGUIDString()
            {
                if (player_guid.HasValue)
                    return player_guid.Value.ToString();
                return null;
            }

            public string GetIPString()
            {
                return player_ip;
            }

            //CHANGE
            public string GetHWIDString()
            {
                return player_hwid != null ? player_hwid.Value : null;
            }

            //CHANGE
            public static PlayerInfo CommonIdentifiers(PlayerInfo A, PlayerInfo B)
            {
                PlayerInfo commoninfo = new PlayerInfo();
                if (B.isNull() || A.isNull())
                    return null;
                if (!string.IsNullOrWhiteSpace(A.GetIPString()))
                {
                    if (!string.IsNullOrWhiteSpace(B.GetIPString()) && A.GetIPString() == B.GetIPString())
                        commoninfo.player_ip = A.player_ip;
                }
                if (!string.IsNullOrWhiteSpace(A.GetGUIDString()))
                {
                    if (!string.IsNullOrWhiteSpace(B.GetGUIDString()) && A.GetGUIDString() == B.GetGUIDString())
                        commoninfo.player_guid = A.player_guid;
                }
                if (!string.IsNullOrWhiteSpace(A.GetHWIDString()))
                {
                    if (!string.IsNullOrWhiteSpace(B.GetHWIDString()) && A.GetHWIDString() == B.GetHWIDString())
                        commoninfo.player_hwid = A.player_hwid;
                }
                return commoninfo;
            }
        }

        public static void WriteChatToAll(string message)
        {
            Utilities.RawSayAll(ConfigValues.ChatPrefix + " " + message);
        }

        public void WriteChatToPlayer(Entity player, string message)
        {
            Utilities.RawSayTo(player, ConfigValues.ChatPrefixPM + " " + message);
        }

        public void WriteChatToAllMultiline(string[] messages, int delay = 500)
        {
            int num = 0;
            foreach (string str in messages)
            {
                string message = str;
                AfterDelay(num * delay, (() => WriteChatToAll(message)));
                ++num;
            }
        }

        public void WriteChatToAllCondensed(string[] messages, int delay = 1000, int condenselevel = 40, string separator = ", ")
        {
            WriteChatToAllMultiline(messages.Condense(condenselevel, separator), delay);
        }

        public void WriteChatToPlayerMultiline(Entity player, string[] messages, int delay = 500)
        {
            int num = 0;
            foreach (string str in messages)
            {
                string message = str;
                AfterDelay(num * delay, () => WriteChatToPlayer(player, message));
                ++num;
            }
        }

        public void WriteChatToPlayerCondensed(Entity player, string[] messages, int delay = 1000, int condenselevel = 40, string separator = ", ")
        {
            WriteChatToPlayerMultiline(player, messages.Condense(condenselevel, separator), delay);
        }

        public void WriteChatSpyToPlayer(Entity player, string message)
        {
            Utilities.RawSayTo(player, ConfigValues.ChatPrefixSPY + " " + message);
        }

        public void WriteChatAdmToPlayer(Entity player, string message)
        {
            Utilities.RawSayTo(player, ConfigValues.ChatPrefixAdminMSG + message);
        }

        public void ChangeMap(string devmapname)
        {
            ExecuteCommand("map " + devmapname);
        }

        public List<Entity> FindPlayers(string identifier)
        {
            if (identifier.StartsWith("#"))
            {
                try
                {
                    int number = int.Parse(identifier.Substring(1));
                    Entity ent = Entity.GetEntity(number);
                    if (number >= 0 && number < 18)
                    {
                        foreach (Entity player in Players)
                        {
                            if (player.GetEntityNumber() == number)
                                return new List<Entity>() { ent };
                        }
                    }
                    return new List<Entity>();
                }
                catch (Exception)
                {
                }
            }
            identifier = identifier.ToLowerInvariant();
            return (from player in Players
                    where player.Name.ToLowerInvariant().Contains(identifier)
                    select player).ToList();
        }

        public Entity FindSinglePlayer(string identifier)
        {
            List<Entity> players = FindPlayers(identifier);
            if (players.Count != 1)
                return null;
            return players[0];
        }

        public List<string> FindMaps(string identifier)
        {
            return (from map in ConfigValues.AvailableMaps
                    where map.Key.Contains(identifier) || map.Value.Contains(identifier)
                    select map.Value).ToList();
        }

        public string FindSingleMap(string identifier)
        {
            List<string> maps = FindMaps(identifier);
            if (maps.Count != 1)
                return null;
            return maps[0];
        }

        public string DevMapName2Mapname(string devMapname)
        {
            List<string> maps =
                (from map in ConfigValues.AvailableMaps
                    where map.Value.Contains(devMapname)
                    select map.Key).ToList();
            if (maps.Count != 1)
                return null;
            return maps[0];
        }

        public static bool ParseCommand(string CommandToBeParsed, int ArgumentAmount, out string[] arguments, out string optionalarguments)
        {
            CommandToBeParsed = CommandToBeParsed.TrimEnd(' ');
            List<string> list = new List<string>();
            if (CommandToBeParsed.IndexOf(' ') == -1)
            {
                arguments = new string[0];
                optionalarguments = null;
                if (ArgumentAmount == 0)
                    return true;
                else
                    return false;
            }
            CommandToBeParsed = CommandToBeParsed.Substring(CommandToBeParsed.IndexOf(' ') + 1);
            while (list.Count < ArgumentAmount)
            {
                int length = CommandToBeParsed.IndexOf(' ');
                if (length == -1)
                {
                    list.Add(CommandToBeParsed);
                    CommandToBeParsed = null;
                }
                else
                {
                    if (CommandToBeParsed == null)
                    {
                        arguments = new string[0];
                        optionalarguments = null;
                        return false;
                    }
                    list.Add(CommandToBeParsed.Substring(0, length));
                    CommandToBeParsed = CommandToBeParsed.Substring(CommandToBeParsed.IndexOf(' ') + 1);
                }
            }
            arguments = list.ToArray();
            optionalarguments = CommandToBeParsed;
            return true;
        }

        public IEnumerable<Entity> GetEntities()
        {
            for (int i = 0; i < 2048; i++)
                yield return Entity.GetEntity(i);
        }

        public void UTILS_OnPlayerConnect(Entity player)
        {
            //check if bad name
            foreach (string identifier in System.IO.File.ReadAllLines(ConfigValues.ConfigPath + @"Utils\badnames.txt"))
                if (player.Name == identifier)
                {
                    CMD_tmpban(player, "^1Piss off, hacker.");
                    WriteChatToAll(Command.GetString("tmpban", "message").Format(new Dictionary<string, string>()
                    {
                        {"<target>", "^:" + player.Name },
                        {"<targetf>", "^:" + player.GetFormattedName(database) },
                        {"<issuer>", ConfigValues.ChatPrefix },
                        {"<issuerf>", ConfigValues.ChatPrefix },
                        {"<reason>", "Piss off, hacker." },
                    }));
                    return;
                }
            if (!forced_clantags.Keys.Contains(player.GUID))
            {
                //check if bad clantag
                foreach (string identifier in System.IO.File.ReadAllLines(ConfigValues.ConfigPath + @"Utils\badclantags.txt"))
                if (player.GetClantag() == identifier)
                {
                        CMD_tmpban(player, "^1Piss off, hacker.");
                        WriteChatToAll(Command.GetString("tmpban", "message").Format(new Dictionary<string, string>()
                        {
                            {"<target>", "^:" + player.Name },
                            {"<targetf>", "^:" + player.GetFormattedName(database) },
                            {"<issuer>", ConfigValues.ChatPrefix },
                            {"<issuerf>", ConfigValues.ChatPrefix },
                            {"<reason>", "Piss off, hacker." },
                        }));
                        return;
                    }
            }

            //check if bad xnaddr
            if (player.GetXNADDR() == null || string.IsNullOrEmpty(player.GetXNADDR().ToString()))
            {
                ExecuteCommand("dropclient " + player.EntRef + " \"Cool story bro.\"");
                return;
            }

            //check if EO Unbanner
            string rawplayerhwid = player.GetHWIDRaw();
            if (System.Text.RegularExpressions.Regex.Matches(rawplayerhwid, "adde").Count >= 3)
            {
                WriteChatToAll(Command.GetString("ban", "message").Format(new Dictionary<string, string>()
                    {
                        {"<target>", "^:" + player.Name },
                        {"<targetf>", "^:" + player.GetFormattedName(database) },
                        {"<issuer>", ConfigValues.ChatPrefix },
                        {"<issuerf>", ConfigValues.ChatPrefix },
                        {"<reason>", "Piss off, hacker." },
                    }));
                ExecuteCommand("dropclient " + player.EntRef + " \"^3BAI SCRUB :D\"");
                return;
            }

            //log player name
            if (System.IO.File.Exists(string.Format(ConfigValues.ConfigPath + @"Utils\playerlogs\{0}.txt", player.GUID)))
            {
                List<string> lines = System.IO.File.ReadAllLines(string.Format(ConfigValues.ConfigPath + @"Utils\playerlogs\{0}.txt", player.GUID)).ToList();
                if (!lines.Contains(player.Name))
                    lines.Add(player.Name);
                System.IO.File.WriteAllLines(string.Format(ConfigValues.ConfigPath + @"Utils\playerlogs\{0}.txt", player.GUID), lines);
            }
            else
                System.IO.File.WriteAllLines(string.Format(ConfigValues.ConfigPath + @"Utils\playerlogs\{0}.txt", player.GUID), new string[] { player.Name });

            //check for names ingame
            if (player.Name.Length < 3)
            {
                CMD_kick(player, "Name must be at least 3 characters long.");
                return;
            }
            //string invariantname = player.Name.ToLowerInvariant();
            //foreach (Entity scrub in Players)
            //{
            //    string invariantscrub = scrub.Name.ToLowerInvariant();
            //    if (player.EntRef != scrub.EntRef && (invariantscrub.Contains(invariantname) || invariantname.Contains(invariantscrub)))
            //    {
            //        CMD_kick(player, "Your name is containing another user's/contained by another user");
            //        return;
            //    }
            //}

            //check issue #11

            UTILS_SetTeamNames(player);
            if (!player.HasField("killstreak"))
            {
                int v = 0;
                player.SetField("killstreak", new Parameter(v));
            }

        }

        public void UTILS_OnServerStart()
        {
            PlayerConnected += UTILS_OnPlayerConnect;
            PlayerConnecting += UTILS_OnPlayerConnecting;
            OnPlayerKilledEvent += UTILS_BetterBalance;

            if (!System.IO.Directory.Exists(ConfigValues.ConfigPath + @"Utils"))
                System.IO.Directory.CreateDirectory(ConfigValues.ConfigPath + @"Utils");

            if (!System.IO.File.Exists(ConfigValues.ConfigPath + @"Utils\badnames.txt"))
                System.IO.File.WriteAllLines(ConfigValues.ConfigPath + @"Utils\badnames.txt", new string[]
                    {
                        "thisguyhax.",
                        "MW2Player",
                    });

            if (!System.IO.File.Exists(ConfigValues.ConfigPath + @"Utils\badclantags.txt"))
                System.IO.File.WriteAllLines(ConfigValues.ConfigPath + @"Utils\badclantags.txt", new string[]
                    {
                        "hkClan",
                    });

            if (System.IO.File.Exists(ConfigValues.ConfigPath + @"Utils\announcer.txt"))
            {
                Announcer announcer = new Announcer("default", System.IO.File.ReadAllLines(ConfigValues.ConfigPath + @"Utils\announcer.txt").ToList());
                OnInterval(announcer.message_interval, () =>
                {
                    WriteChatToAll(announcer.SpitMessage());
                    return true;
                });
            }

            if (!System.IO.Directory.Exists(ConfigValues.ConfigPath + @"Utils\playerlogs"))
                System.IO.Directory.CreateDirectory(ConfigValues.ConfigPath + @"Utils\playerlogs");

            if (!System.IO.Directory.Exists(ConfigValues.ConfigPath + @"Utils\internal\announcers"))
                System.IO.Directory.CreateDirectory(ConfigValues.ConfigPath + @"Utils\internal\announcers");

            // TEAM NAMES
            foreach (Entity player in Players)
            {
                UTILS_SetTeamNames(player);
            }

            // RGADMIN HUDELEM
            if (bool.Parse(Sett_GetString("settings_showversion")))
            {
                RGAdminMessage = HudElem.CreateServerFontString("bigfixed", 0.6f);
                RGAdminMessage.SetPoint("BOTTOMRIGHT", "BOTTOMRIGHT",0,-30);
                RGAdminMessage.SetText(" ^0[^1DG^0]\n^:Admin\n^0" + ConfigValues.Version);
                RGAdminMessage.Color = new Vector3(1f, 0.75f, 0f);
                RGAdminMessage.GlowAlpha = 1f;
                RGAdminMessage.GlowColor = new Vector3(0.349f, 0f, 0f);
                RGAdminMessage.Foreground = true;
                RGAdminMessage.HideWhenInMenu = true;
            }

            // ADMINS HUDELEM
            if (bool.Parse(Sett_GetString("settings_adminshudelem")))
            {
                OnlineAdmins = HudElem.CreateServerFontString("hudsmall", 0.5f);
                OnlineAdmins.SetPoint("top", "top", 0, 5);
                OnlineAdmins.Foreground = true;
                OnlineAdmins.Archived = false;
                OnlineAdmins.HideWhenInMenu = true;
                OnInterval(5000, () =>
                {
                    OnlineAdmins.SetText("^1Online Admins:\n" + string.Join("\n", database.GetAdminsString(Players).Condense(100, "^7, ")));
                    return true;
                });
            }

            OnGameEnded += UTILS_OnGameEnded;

            // BETTER BALANCE
            Call("setdvarifuninitialized", "betterbalance", bool.Parse(Sett_GetString("settings_betterbalance_enable")) ? "1" : "0");

            // AUTOFPSUNLOCK
            if (bool.Parse(Sett_GetString("settings_enable_autofpsunlock")))
                PlayerConnected += (player) =>
                {
                    player.SetClientDvar("com_maxfps", "0");
                    player.SetClientDvar("con_maxfps", "0");
                };

            //DLCMAPS
            if (bool.Parse(Sett_GetString("settings_enable_dlcmaps")))
                ConfigValues.AvailableMaps = Data.AllMapNames;

            ConfigValues.mapname = UTILS_GetDvar("mapname");
            ConfigValues.g_gametype = UTILS_GetDvar("g_gametype");
        }

        public void ANTIWEAPONHACK (Entity player, Entity inflictor, Entity attacker, int damage, int dFlags, string mod, string weapon, Vector3 point, Vector3 dir, string hitLoc)
        {
            // antiweaponhack
            if (ConfigValues.settings_dynamic_properties &&
                ConfigValues.ANTIWEAPONHACK &&
                !UTILS_WeaponAllowed(weapon) &&
                !CMDS_IsRekt(attacker))
            {
                try
                {
                    WriteLog.Info("----STARTREPORT----");
                    WriteLog.Info("Bad weapon detected: " + weapon + " at player " + attacker.Name);
                    HaxLog.WriteInfo("----STARTREPORT----");
                    HaxLog.WriteInfo("BAD WEAPON: " + weapon);
                    HaxLog.WriteInfo("Player Info:");
                    HaxLog.WriteInfo(attacker.Name);
                    HaxLog.WriteInfo(attacker.GUID.ToString());
                    HaxLog.WriteInfo(attacker.IP.ToString());
                    HaxLog.WriteInfo(attacker.GetEntityNumber().ToString());
                }
                finally
                {
                    WriteLog.Info("----ENDREPORT----");
                    HaxLog.WriteInfo("----ENDREPORT----");
                    player.Health += damage;
                    CMDS_Rek(attacker);
                    WriteChatToAll(Command.GetString("rek", "message").Format(new Dictionary<string, string>()
                    {
                        {"<target>", attacker.Name},
                        {"<targetf>", attacker.GetFormattedName(database)},
                        {"<issuer>", ConfigValues.ChatPrefix},
                        {"<issuerf>", ConfigValues.ChatPrefix},
                    }));
                }
            }
        }

        public void CMD_Votekick_CreateHUD()
        {
            if (VoteStatsHUD == null)
            {
                VoteStatsHUD = HudElem.CreateServerFontString("hudsmall", 0.7f);
                VoteStatsHUD.SetPoint("TOPLEFT", "TOPLEFT", 10, 290);
                VoteStatsHUD.Foreground = true;
                VoteStatsHUD.HideWhenInMenu = true;
                VoteStatsHUD.Archived = false;
            }
            OnInterval(1000, () =>
            {
                if (voting.isActive())
                {
                    VoteStatsHUD.SetText(voting.hudText);
                    return true;
                }
                else
                {
                    VoteStatsHUD.SetText("");
                    return false;
                }
            });
        }

        public void UTILS_BetterBalance(Entity player, Entity inflictor, Entity attacker, int damage, string mod, string weapon, Vector3 dir, string hitLoc)
        {
            if (Call<string>("getdvar", "betterbalance") == "0" || Call<string>("getdvar", "g_gametype") == "infect")
                return;
            int axis = 0, allies = 0;
            UTILS_GetTeamPlayers(out axis, out allies);
            switch (player.GetTeam())
            {
                case "axis":
                    if (axis - allies > 1)
                    {
                        player.SetField("sessionteam", "allies");
                        player.Notify("menuresponse", "team_marinesopfor", "allies");
                        WriteChatToAll(Sett_GetString("settings_betterbalance_message").Format(new Dictionary<string, string>()
                        {
                            {"<player>", player.Name},
                            {"<playerf>", player.GetFormattedName(database)},
                        }));
                    }
                    return;
                case "allies":
                    if (allies - axis > 1)
                    {
                        player.SetField("sessionteam", "axis");
                        player.Notify("menuresponse", "team_marinesopfor", "axis");
                        WriteChatToAll(Sett_GetString("settings_betterbalance_message").Format(new Dictionary<string, string>()
                        {
                            {"<player>", player.Name},
                            {"<playerf>", player.GetFormattedName(database)},
                        }));
                    }
                    return;
            }
        }

        public void UTILS_OnGameEnded()
        {
            AfterDelay(1100, () =>
            {            
                // UNFREEZE PLAYERS ON GAME END
                if (bool.Parse(Sett_GetString("settings_unfreezeongameend")))
                    foreach (Entity player in Players)
                        if (!CMDS_IsRekt(player))
                            player.Call("freezecontrols", false);

                // Save xlr stats
                if (ConfigValues.settings_enable_xlrstats)
                {
                    WriteLog.Info("Saving xlrstats...");
                    xlr_database.Save(this);
                }

                WriteLog.Info("Saving PersonalPlayerDvars...");
                // Save FilmTweak settings
                UTILS_PersonalPlayerDvars_save(PersonalPlayerDvars);

                ConfigValues.SettingsMutex = true;
            });

        }

        public void UTILS_OnPlayerConnecting(Entity player)
        {
            if (player.GetClantag().Contains(Encoding.ASCII.GetString(new byte[] { 0x5E, 0x02 })))
                ExecuteCommand("dropclient " + player.GetEntityNumber() + " \"Get out.\"");
        }

        public bool UTILS_ParseBool(string message)
        {
            message = message.ToLowerInvariant().Trim();
            if (message == "y" || message == "ye" || message == "yes" || message == "on" || message == "true" || message == "1")
                return true;
            return false;
        }

        public bool UTILS_ParseTimeSpan(string message, out TimeSpan timespan)
        {
            timespan = TimeSpan.Zero;
            try
            {
                string[] parts = message.Split(',');
                foreach (string part in parts)
                {
                    int time = int.Parse(part.Substring(0, part.Length - 1));
                    if (time == 0)
                        return false;
                    if (part.EndsWith("h"))
                        timespan += new TimeSpan(time, 0, 0);
                    if (part.EndsWith("m"))
                        timespan += new TimeSpan(0, time, 0);
                    if (part.EndsWith("s"))
                        timespan += new TimeSpan(0, 0, time);
                    return false;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string UTILS_GetDvar(string dvar)
        {
            return Call<string>("getdvar", dvar);
        }

        public void UTILS_SetDvar(string dvar, string value)
        {
            Call<string>("setdvar", dvar, value);
        }

        public void UTILS_SetCliDefDvars(Entity player)
        {
            foreach (KeyValuePair<string, string> dvar in DefaultCDvars)
            {
                player.SetClientDvar(dvar.Key, dvar.Value);
            }
            player.SetClientDvar("fx_draw", "1");
            if(ConfigValues.settings_daytime == "night")
                UTILS_SetClientNightVision(player);
            else
            if (PersonalPlayerDvars.ContainsKey(player.GUID))
                foreach (Dvar dvar in PersonalPlayerDvars[player.GUID])
                    player.SetClientDvar(dvar.key, dvar.value);
        }

        public void UTILS_SetClientDvars(Entity player, List<Dvar> dvars){
            foreach (Dvar dvar in dvars)
                player.SetClientDvar(dvar.key, dvar.value);
        }

        public static void ExecuteCommand(string command)
        {
            Utilities.ExecuteCommand(command);
        }

        public void UTILS_SetTeamNames(Entity player)
        {
            player.SetClientDvar("g_TeamName_Allies", ConfigValues.settings_teamnames_allies);
            player.SetClientDvar("g_TeamName_Axis", ConfigValues.settings_teamnames_axis);
            player.SetClientDvar("g_TeamIcon_Allies", ConfigValues.settings_teamicons_allies);
            player.SetClientDvar("g_TeamIcon_Axis", ConfigValues.settings_teamicons_axis);
        }

        public void UTILS_SetClientNightVision(Entity player)
        {
            string[,] dvars = new string[10,2] { 
                { "r_filmUseTweaks", "1" }, 
                { "r_filmTweakEnable", "1" }, 
                { "r_filmTweakLightTint", "0 0.2 1" }, 
                { "r_filmTweakDarkTint", "0 0.125 1" },
                { "r_filmtweakbrightness","0" },
                {"r_glowTweakEnable", "1"},
                {"r_glowUseTweaks","1"},
                {"r_glowTweakRadius0","5"},
                {"r_glowTweakBloomIntensity0","0.5"},
                {"r_fog", "0"}
            };
            for (int i = 0; i < 10; i++ )
                player.SetClientDvar(dvars[i,0], dvars[i,1]);
        }

        public void UTILS_SetClientInShadowFX(Entity player)
        {
            player.SetClientDvar("r_filmUseTweaks", "1");
            player.SetClientDvar("r_filmTweakEnable", "1");
            player.SetClientDvar("r_filmTweakDesaturation", "1");
            player.SetClientDvar("r_filmTweakDesaturationDark", "1");
            player.SetClientDvar("r_filmTweakInvert", "1");
            player.SetClientDvar("r_glowTweakEnable", "1");
            player.SetClientDvar("r_glowUseTweaks", "1");
            player.SetClientDvar("r_glowTweakRadius0", "10");
            player.SetClientDvar("r_filmTweakContrast", "3");
            player.SetClientDvar("r_filmTweakBrightness", "1");
            player.SetClientDvar("r_filmTweakLightTint", "1 0.125 0");
            player.SetClientDvar("r_filmTweakDarkTint", "0 0 0");
            player.Call("ThermalVisionOn");
        }

        public void UTILS_SetHellMod() {
            string[] list = new string[] { 
                "fire/car_fire_mp -741.726 127.9078 186.8259",
                "fire/car_fire_mp -746.7289 -53.34315 192.0534",
                "fire/car_fire_mp -766.3276 -103.3418 213.1675",
                "fire/car_fire_mp -561.9498 -132.4966 176.1293",
                "fire/car_fire_mp -549.1067 -94.50184 199.4765",
                "fire/car_fire_mp -572.1155 29.96895 201.9723",
                "fire/car_fire_mp -589.7307 173.5736 246.368",
                "fire/car_fire_mp -544.9576 67.94726 284.9695",
                "fire/car_fire_mp -558.2468 86.67757 235.6196",
                "fire/car_fire_mp -527.1373 94.35648 228.0653",
                "fire/car_fire_mp -511.9091 99.72758 201.7868",
                "fire/car_fire_mp -502.6147 153.6443 212.324",
                "fire/car_fire_mp -499.7265 -52.09038 196.5952",
                "fire/car_fire_mp -500.0943 -82.36002 189.4241",
                "fire/car_fire_mp -750.3026 -286.7527 197.5594",
                "fire/car_fire_mp -820.0682 -299.949 199.7754",
                "fire/car_fire_mp -833.5555 -382.6624 199.3139",
                "fire/car_fire_mp -763.6664 -447.7071 209.4525",
                "fire/car_fire_mp -540.0521 -112.4356 185.476",
                "fire/car_fire_mp -770.0934 -1358.123 191.3906",
                "fire/car_fire_mp -800.5062 -1370.291 191.3906",
                "fire/car_fire_mp -697.7897 -1502.868 237.3132",
                "fire/car_fire_mp -657.2511 -1529.918 236.0008",
                "fire/car_fire_mp -739.0823 -1513.038 200.4902",
                "fire/car_fire_mp -685.9439 -1525.523 216.0745",
                "fire/car_fire_mp -656.4402 -1557.843 201.057",
                "fire/car_fire_mp -706.9456 -1566.001 212.2845",
                "fire/car_fire_mp -735.1216 -1525.2 235.0302",
                "fire/car_fire_mp -785.8285 -1430.447 159.8727",
                "fire/car_fire_mp -781.6548 -1393.441 160.6226",
                "fire/car_fire_mp -721.3155 -1404.557 169.403",
                "fire/car_fire_mp -1023.336 -1084.734 181.9677",
                "fire/car_fire_mp -1076.79 -1100.496 177.4189",
                "fire/car_fire_mp -1092.525 -1081.302 185.7586",
                "fire/car_fire_mp -1092.419 -1056.805 206.7297",
                "fire/car_fire_mp -1111.036 -1055.503 171.9781",
                "fire/car_fire_mp -368.555 -1353.024 216.1874",
                "fire/car_fire_mp -311.8722 -1348.547 215.7776",
                "fire/car_fire_mp -263.6784 -1341.829 218.2392",
                "fire/car_fire_mp -197.5623 -1374.538 223.8046",
                "fire/car_fire_mp -220.5542 -1420.733 219.851",
                "fire/car_fire_mp -260.6386 -1432.63 218.2054",
                "fire/car_fire_mp -282.946 -1395.434 214.258",
                "fire/car_fire_mp -295.6677 -1402.59 226.7604",
                "fire/car_fire_mp -312.2665 -1392.842 236.1707",
                "fire/car_fire_mp -313.3947 -1431.607 240.9264",
                "fire/car_fire_mp -246.2982 -1419.802 268.4792",
                "fire/car_fire_mp -203.2566 -1414.628 265.036",
                "fire/car_fire_mp -230.7615 -1388.27 263.0937",
                "fire/car_fire_mp -174.7606 -1069.309 481.0146",
                "fire/car_fire_mp -1373.858 -1223.6 175.7884",
                "fire/car_fire_mp -1398.202 -1183.905 174.6312",
                "fire/car_fire_mp -1417.298 -1215.233 185.0096",
                "fire/car_fire_mp -1144.135 -1453.069 152.1256",
                "fire/car_fire_mp -586.965 -97.91954 175.2606",
                "fire/car_fire_mp -592.9798 -46.85965 176.1271",
                "fire/car_fire_mp -582.035 14.52852 191.125",
                "fire/car_fire_mp -583.3083 -17.12712 181.7186",
                "fire/car_fire_mp -554.8394 173.5616 211.0177",
                "fire/car_fire_mp -554.5637 170.8945 230.507",
                "fire/car_fire_mp -547.4115 233.4936 215.5837",
                "fire/car_fire_mp -528.2179 212.7749 213.8737",
                "fire/car_fire_mp -993.1925 21.8281 348.125",
                "fire/car_fire_mp -984.5154 -12.24416 373.335",
                "fire/car_fire_mp -1019.694 3.782988 367.581",
                "smoke/bg_smoke_plume -313.9216 -1405.47 266.6604",
                "smoke/bg_smoke_plume -701.9982 -1517.155 225.1734",
                "smoke/bg_smoke_plume -769.1347 -1376.464 224.798",
                "smoke/bg_smoke_plume -1281.853 -1456.973 152.1338",
                "smoke/bg_smoke_plume -1264.148 -1428.778 152.1347",
                "smoke/bg_smoke_plume -1368.727 -1313.534 154.4869",
                "smoke/bg_smoke_plume -1415.86 -1238.368 173.8684",
                "smoke/bg_smoke_plume -1097.656 -1114.722 181.1771",
                "smoke/bg_smoke_plume -1147.323 -1101.099 174.4035",
                "smoke/bg_smoke_plume -1251.073 -1148.898 168.4118",
                "smoke/bg_smoke_plume -1345.156 -1193.335 162.5927",
                "smoke/bg_smoke_plume -764.8975 -452.6243 227.4975",
                "smoke/bg_smoke_plume -770.9753 -375.5204 225.8042",
                "smoke/bg_smoke_plume -774.4709 -91.01746 219.3868",
                "smoke/bg_smoke_plume -776.0869 58.22653 215.8386",
                "smoke/bg_smoke_plume -777.2213 151.9467 213.6097",
                "smoke/bg_smoke_plume -548.829 209.9325 238.3743",
                "smoke/bg_smoke_plume -536.4377 86.45039 239.1204",
                "smoke/bg_smoke_plume -528.6058 -41.20591 235.5137",
                "smoke/bg_smoke_plume -804.9241 -222.3701 174.1253",
                "smoke/bg_smoke_plume -541.8279 291.3926 176.2325",
                "smoke/bg_smoke_plume -705.7064 281.2861 174.6353",
                "smoke/bg_smoke_plume -907.6201 258.1046 175.9906",
                "smoke/bg_smoke_plume -1066.609 -1460.65 202.0231"
            };
            Array.ForEach(list, (row) => {
                ((Func<string[], bool>)((fx) => {
                    try
                    {
                        Call("triggerfx", 
                            Call<Entity>("spawnFx", 
                                Call<int>("loadfx", fx[0]), 
                                new Vector3(
                                    Single.Parse(fx[1]), 
                                    Single.Parse(fx[2]), 
                                    Single.Parse(fx[3])), 
                                new Vector3(0, 0, 1), 
                                new Vector3(0, 0, 0)));
                        return true;
                    }
                    catch{return false;}
                }))(row.Split(' '));
            });
        }

        public bool UTILS_ValidateFX(string fx)
        {
            string[] precached_fx = new string[] { };
            switch (UTILS_GetDvar("mapname"))
            {
                case "mp_alpha": 
                    precached_fx = new string[]{
                        "dust/falling_dirt_frequent_runner",
                        "dust/dust_wind_fast_paper",
                        "dust/dust_wind_slow_paper",
                        "misc/trash_spiral_runner",
                        "dust/room_dust_200_blend_mp_vacant",
                        "misc/insects_carcass_flies",
                        "explosions/spark_fall_runner_mp",
                        "weather/ash_prague",
                        "weather/embers_prague_light",
                        "fire/firelp_med_pm",
                        "fire/firelp_small",
                        "smoke/thin_black_smoke_s_fast",
                        "dust/falling_dirt_frequent_runner",
                        "smoke/steam_manhole",
                        "smoke/battlefield_smokebank_S_warm_dense",
                        "smoke/bg_smoke_plume",
                        "fire/firelp_med_pm_cheap",
                        "fire/banner_fire",
                        "misc/light_glow_white_lamp",
                        "fire/fire_wall_50",
                        "smoke/white_battle_smoke",
                        "fire/after_math_embers",
                        "fire/wall_fire_mp",
                        "fire/car_fire_mp",
                        "fire/firelp_small_cheap_mp",
                        "misc/falling_ash_mp"
                    };
                    break;
                case "mp_bootleg":
                    precached_fx = new string[]{
                        "dust/falling_dirt_frequent_runner",
                        "dust/dust_wind_fast_paper",
                        "dust/dust_wind_slow_paper",
                        "misc/trash_spiral_runner",
                        "dust/room_dust_200_blend_mp_vacant_light",
                        "misc/insects_carcass_flies",
                        "explosions/spark_fall_runner_mp",
                        "weather/embers_prague_light",
                        "fire/firelp_med_pm",
                        "fire/firelp_small",
                        "smoke/thin_black_smoke_s_fast",
                        "dust/falling_dirt_frequent_runner",
                        "smoke/steam_manhole",
                        "smoke/battlefield_smokebank_S_warm_dense",
                        "smoke/bg_smoke_plume",
                        "fire/firelp_med_pm_cheap",
                        "fire/car_fire_mp",
                        "fire/firelp_small_cheap_mp",
                        "misc/falling_ash_mp",   
                        "misc/flocking_birds_mp",
                        "misc/insects_light_hunted_a_mp",
                        "weather/rain_mp_bootleg",
                        "weather/rain_noise_splashes",
                        "weather/rain_splash_lite_64x64",
                        "water/water_drips_fat_fast_speed",
                        "smoke/bg_smoke_plume",
                        "smoke/bootleg_alley_steam",
                        "misc/palm_leaves",
                        "misc/light_glow_white_lamp",
                        "water/waterfall_drainage_short_london",
                        "water/waterfall_splash_medium_london",
                        "water/drainpipe_mp_bootleg"
                    };
                    break;
                case "mp_bravo":
                    precached_fx = new string[]{
                        "dust/dust_wind_fast_paper",
                        "dust/dust_wind_fast_no_paper_bravo",
                        "misc/moth_runner",
                        "misc/trash_spiral_runner_bravo",
                        "smoke/battlefield_smokebank_s_cheap",
                        "smoke/hallway_smoke_light",
                        "misc/falling_brick_runner_line_100",
                        "misc/falling_brick_runner_line_200",
                        "misc/falling_brick_runner_line_300",
                        "misc/falling_brick_runner_line_400",
                        "misc/falling_brick_runner_line_600",
                        "misc/falling_brick_runner_line_75",
                        "smoke/room_smoke_200",
                        "misc/insects_carcass_runner",
                        "smoke/smoke_plume_grey_01",
                        "smoke/smoke_plume_grey_02",
                        "smoke/thick_black_smoke_mp",
                        "dust/falling_dirt_light_1_runner_bravo",
                        "misc/flocking_birds_mp",
                        "fire/car_fire_mp",
                        "fire/firelp_small_cheap_mp",
                        "misc/insects_light_invasion",
                        "misc/insect_trail_runner_icbm"
                    };
                    break;
                case "mp_carbon":
                    precached_fx = new string[]{
                        "dust/dust_wind_fast_paper",
                        "dust/dust_wind_fast_no_paper",
                        "dust/dust_wind_slow_paper",
                        "misc/trash_spiral_runner",
                        "smoke/battlefield_smokebank_s_cheap_mp_carbon",
                        "smoke/hallway_smoke_light",
                        "smoke/room_smoke_200",
                        "smoke/room_smoke_400",
                        "explosions/electrical_transformer_falling_fire_runner",
                        "fire/car_fire_mp",
                        "fire/firelp_small_cheap_mp",
                        "smoke/smoke_plume_grey_01",
                        "smoke/smoke_plume_grey_02",
                        "smoke/smoke_plume_white_01",
                        "smoke/smoke_plume_white_02",
                        "smoke/steam_large_vent_rooftop",
                        "smoke/steam_manhole",
                        "smoke/steam_roof_ac",
                        "fire/flame_refinery_far",
                        "fire/flame_refinery_small_far",
                        "fire/flame_refinery_small_far_2",
                        "fire/flame_refinery_small_far_3",
                        "smoke/steam_cs_mp_carbon",
                        "smoke/steam_jet_loop_cheap_mp_carbon",
                        "dust/dust_wind_fast_no_paper_airiel",
                        "water/water_drips_fat_fast_singlestream_mp_carbon",
                        "smoke/bootleg_alley_steam",
                        "misc/moth_runner"
                    };
                    break;
                case "mp_dome":
                    precached_fx = new string[]{
                        "weather/sand_storm_mp_dome_exterior",
                        "weather/sand_storm_mp_dome_interior",
                        "weather/sand_storm_mp_dome_interior_outdoor_only",
                        "dust/sand_spray_detail_runner_0x400",
                        "dust/sand_spray_detail_runner_400x400",
                        "dust/sand_spray_detail_oriented_runner_mp_dome",
                        "dust/sand_spray_cliff_oriented_runner",
                        "smoke/hallway_smoke_light",
                        "dust/light_shaft_dust_large",
                        "dust/room_dust_200_blend",
                        "dust/room_dust_100_blend",
                        "smoke/battlefield_smokebank_S",
                        "dust/dust_ceiling_ash_large",
                        "dust/ash_spiral_runner",
                        "dust/dust_wind_fast_paper",
                        "dust/dust_wind_slow_paper",
                        "misc/trash_spiral_runner",
                        "misc/leaves_spiral_runner",
                        "dust/dust_ceiling_ash_large_mp_vacant",
                        "dust/room_dust_200_blend_mp_vacant",
                        "dust/light_shaft_dust_large_mp_vacant", 
                        "dust/light_shaft_dust_large_mp_vacant_sidewall",  
                        "misc/falling_brick_runner",
                        "misc/falling_brick_runner_line_400",
                        "fire/firelp_med_pm_nodistort",
                        "fire/firelp_small_pm"
                    };
                    break;
                case "mp_exchange":
                    precached_fx = new string[]{
                        "misc/floating_room_dust",
                        "dust/falling_dirt_frequent_runner",
                        "dust/dust_wind_fast_paper",
                        "dust/dust_wind_slow_paper",
                        "misc/trash_spiral_runner",
                        "dust/room_dust_200_blend_mp_vacant",
                        "misc/insects_carcass_flies",
                        "explosions/spark_fall_runner_mp",
                        "weather/embers_prague_light",
                        "smoke/thin_black_smoke_s_fast",
                        "dust/falling_dirt_frequent_runner_exchange",
                        "smoke/building_hole_smoke_mp",
                        "fire/car_fire_mp",
                        "fire/firelp_small_cheap_mp",
                        "misc/falling_ash_mp",
                        "misc/insects_light_hunted_a_mp",
                        "misc/building_hole_paper_fall_mp",
                        "weather/ground_fog_mp",
                        "misc/oil_drip_puddle",
                        "misc/antiair_runner_cloudy",
                        "fire/fire_falling_runner_point",
                        "weather/ceiling_smoke_exchange",
                        "smoke/large_battle_smoke_mp",
                        "misc/light_glow_white",
                        "fire/building_hole_embers_mp",
                        "dust/room_dust_200_z150_mp",
                        "explosions/building_hole_elec_short_runner",
                        "smoke/thick_black_smoke_mp",
                        "fire/burned_vehicle_sparks",
                        "water/water_drips_fat_fast_singlestream"
                    };
                    break;
                case "mp_hardhat":
                    precached_fx = new string[]{
                        "explosions/electrical_transformer_spark_runner_loop",
                        "explosions/large_vehicle_explosion_ir",
                        "explosions/vehicle_explosion_btr80",
                        "explosions/generator_spark_runner_loop_interchange",
                        "explosions/spark_fall_runner_mp",
                        "dust/falling_dirt_light_1_runner_bravo",
                        "weather/ash_prague",
                        "weather/embers_prague_light",
                        "smoke/bg_smoke_plume_mp",
                        "smoke/white_battle_smoke",
                        "smoke/hallway_smoke_light",
                        "smoke/room_smoke_400",
                        "smoke/smoke_plume_grey_01",
                        "smoke/smoke_plume_grey_02",
                        "misc/light_glow_white_lamp",
                        "misc/falling_ash_mp",
                        "misc/trash_spiral_runner",
                        "misc/antiair_runner_cloudy",
                        "misc/insects_carcass_runner",
                        "misc/moth_runner",
                        "fire/firelp_small_cheap_mp",
                        "fire/car_fire_mp_far",
                        "dust/dust_cloud_mp_hardhat",
                        "dust/sand_spray_detail_oriented_runner_hardhat",
                        "dust/dust_spiral_runner_small",
                        "misc/jet_flyby_runner",
                        "explosions/building_missilehit_runner"
                    };
                    break;
                case "mp_interchange":
                    precached_fx = new string[]{
                        "dust/falling_dirt_frequent_runner",
                        "dust/dust_wind_fast_paper",
                        "dust/dust_wind_slow_paper",
                        "misc/trash_spiral_runner",
                        "dust/room_dust_200_blend_mp_vacant_light",
                        "misc/insects_carcass_flies",
                        "explosions/spark_fall_runner_mp",
                        "weather/embers_prague_light",
                        "fire/firelp_med_pm",
                        "fire/firelp_small",
                        "smoke/thin_black_smoke_s_fast",
                        "dust/falling_dirt_frequent_runner",
                        "smoke/steam_manhole",
                        "smoke/battlefield_smokebank_S_warm_dense",
                        "smoke/bg_smoke_plume",
                        "fire/firelp_med_pm_cheap",
                        "fire/car_fire_mp",
                        "fire/firelp_small_cheap_mp",
                        "misc/falling_ash_mp",
                        "misc/flocking_birds_mp",
                        "misc/insects_light_hunted_a_mp"
                    };
                    break;
                case "mp_lambeth":
                    precached_fx = new string[]{
                        "dust/falling_dirt_infrequent_runner",
                        "fire/car_fire_mp",
                        "fire/firelp_small_cheap_mp",
                        "fire/firelp_small_cheap_dist_mp",
                        "fire/firelp_med_cheap_dist_mp",
                        "fire/fire_falling_runner_point_infrequent_mp",     
                        "misc/trash_spiral_runner",
                        "misc/falling_brick_runner_line_100",
                        "misc/falling_brick_runner_line_200",
                        "misc/falling_brick_runner_line_300",
                        "misc/falling_brick_runner_line_400",
                        "misc/falling_brick_runner_line_600",
                        "misc/insects_carcass_runner",
                        "misc/insect_trail_runner_icbm",
                        "misc/insects_dragonfly_runner_a",
                        "misc/insects_light_complex",
                        "misc/leaves_fall_gentlewind_lambeth",
                        "misc/falling_ash_mp",
                        "misc/leaves_spiral_runner",
                        "smoke/battlefield_smokebank_s_cheap_mp_carbon",
                        "smoke/battlefield_smokebank_s_cheap_heavy_mp",
                        "smoke/hallway_smoke_light",
                        "smoke/room_smoke_200",
                        "smoke/room_smoke_400",
                        "smoke/steam_roof_ac",
                        "smoke/steam_jet_loop_cheap_mp_carbon",
                        "smoke/thin_black_smoke_m_mp",
                        "smoke/mist_drifting_groundfog_lambeth",
                        "smoke/mist_drifting_lambeth",
                        "water/water_drips_fat_fast_singlestream_mp_carbon",
                        "weather/ceiling_smoke"
                    };
                    break;
                case "mp_mogadishu":
                    precached_fx = new string[]{
                        "fire/firelp_med_pm_cheap",
                        "fire/firelp_small_pm_a_cheap",
                        "dust/dust_wind_fast_paper",
                        "dust/dust_wind_fast_no_paper",
                        "dust/dust_wind_slow_paper",
                        "misc/trash_spiral_runner",
                        "smoke/battlefield_smokebank_s_cheap",
                        "smoke/hallway_smoke_light",
                        "misc/falling_brick_runner_line_400",
                        "smoke/room_smoke_200",
                        "smoke/room_smoke_400",
                        "misc/insects_carcass_runner",
                        "explosions/electrical_transformer_spark_runner_loop",
                        "smoke/smoke_plume_grey_01",
                        "smoke/smoke_plume_grey_02",
                        "smoke/thick_black_smoke_mp",
                        "dust/falling_dirt_light_1_runner",
                        "dust/falling_dirt_light_2_runner"
                    };
                    break;
                case "mp_paris":
                    precached_fx = new string[]{
                        "dust/falling_dirt_light_1_runner_bravo",
                        "dust/falling_dirt_frequent_runner",
                        "dust/room_dust_200_z150_mp",
                        "weather/ash_prague",
                        "weather/embers_prague_light",
                        "weather/ceiling_smoke_seatown",
                        "smoke/chimney_smoke_mp",
                        "smoke/chimney_smoke_large_mp",
                        "misc/falling_ash_mp",
                        "misc/trash_spiral_runner",
                        "misc/leaves_fall_gentlewind_lambeth",
                        "misc/leaves_spiral_runner",
                        "misc/antiair_runner_cloudy",
                        "misc/insects_carcass_runner",
                        "misc/insects_light_hunted_a_mp",
                        "misc/insect_trail_runner_icbm",
                        "fire/firelp_med_pm",
                        "fire/firelp_small",
                        "fire/firelp_small_cheap_mp",
                        "fire/firelp_med_pm_cheap",
                        "fire/building_hole_embers_mp"
                    };
                    break;
                case "mp_plaza2":
                    precached_fx = new string[]{
                        "misc/floating_room_dust",
                        "dust/falling_dirt_frequent_runner",
                        "dust/dust_wind_slow_paper",
                        "misc/trash_spiral_runner",
                        "dust/room_dust_200_blend_mp_vacant",
                        "misc/insects_carcass_flies",
                        "explosions/spark_fall_runner_mp",
                        "weather/embers_prague_light",
                        "smoke/thin_black_smoke_s_fast",
                        "dust/falling_dirt_frequent_runner",
                        "smoke/building_hole_smoke_mp",
                        "fire/car_fire_mp",
                        "fire/firelp_small_cheap_mp",
                        "misc/falling_ash_mp",
                        "misc/insects_light_hunted_a_mp",
                        "smoke/bootleg_alley_steam",
                        "misc/building_hole_paper_fall_mp",
                        "weather/ground_fog_mp",
                        "misc/oil_drip_puddle",
                        "misc/antiair_runner_cloudy",
                        "misc/leaves_spiral_runner",
                        "fire/fire_falling_runner_point",
                        "weather/ceiling_smoke_seatown",
                        "smoke/large_battle_smoke_mp",
                        "misc/light_glow_white",
                        "fire/building_hole_embers_mp",
                        "dust/room_dust_200_z150_mp",
                        "explosions/building_hole_elec_short_runner",
                        "smoke/thick_black_smoke_mp",
                        "misc/leaves_fall_gentlewind_green",
                        "smoke/smoke_plume_grey_02",
                        "fire/burned_vehicle_sparks"
                    };
                    break;
                case "mp_radar":
                    precached_fx = new string[]{
                        "snow/tree_snow_dump_fast",
                        "snow/tree_snow_dump_fast_small",
                        "snow/tree_snow_fallen_heavy",
                        "snow/tree_snow_fallen",
                        "snow/tree_snow_fallen_small",
                        "snow/tree_snow_dump_runner",
                        "snow/snow_spray_detail_contingency_runner_0x400",
                        "snow/snow_spray_detail_oriented_runner_0x400",
                        "snow/snow_spray_detail_oriented_runner_400x400",
                        "snow/snow_spray_detail_oriented_runner",
                        "snow/snow_spray_detail_oriented_large_runner",
                        "snow/snow_spray_large_oriented_radar_od_runner",
                        "snow/snow_spray_large_oriented_runner",
                        "snow/snow_vortex_runner_cheap",
                        "smoke/room_smoke_200",
                        "snow/snow_spiral_runner",
                        "snow/snow_blowoff_ledge_runner",
                        "snow/snow_clifftop_runner",
                        "snow/radar_windy_snow",
                        "snow/blowing_ground_snow",
                        "snow/tree_snow_dump_radar_runner",
                        "water/water_drips_fat_slow_speed",
                        "snow/snow_blizzard_radar",
                        "misc/light_glow_white_lamp",
                        "snow/snow_gust_runner_radar",
                        "fire/heat_lamp_distortion",
                        "fire/car_fire_mp",
                        "fire/firelp_cheap_mp",
                        "snow/radar_windy_snow_no_mist",
                        "snow/radar_windy_snow_small_area",
                        "misc/moth_runner"
                    };
                    break;
                case "mp_seatown":
                    precached_fx = new string[]{
                        "dust/sand_spray_detail_oriented_runner_mp_dome",
                        "dust/room_dust_100_blend",
                        "smoke/battlefield_smokebank_S_warm",
                        "dust/dust_wind_fast_paper",
                        "misc/trash_spiral_runner",
                        "dust/room_dust_200_blend_mp_seatown",
                        "weather/ceiling_smoke_seatown",
                        "misc/insects_carcass_flies",
                        "explosions/spark_fall_runner_mp",
                        "water/seatown_lookout_splash_runner",
                        "water/seatown_pillar_mist",
                        "misc/palm_leaves",
                        "dust/falling_dirt_frequent_runner",
                        "misc/flocking_birds_mp",
                        "misc/insects_light_hunted_a_mp",
                        "fire/firelp_small_cheap_mp",
                        "fire/car_fire_mp",
                        "smoke/bg_smoke_plume",
                        "dust/room_dust_200_blend_seatown_wind_fast" 
                    };
                    break;
                case "mp_underground":
                    precached_fx = new string[]{
                        "dust/falling_dirt_frequent_runner",
                        "dust/dust_wind_fast_paper",
                        "dust/dust_wind_slow_paper",
                        "misc/trash_spiral_runner",
                        "dust/room_dust_200_blend_mp_vacant",
                        "misc/insects_carcass_flies",
                        "explosions/spark_fall_runner_mp",
                        "weather/embers_prague_light",
                        "smoke/thin_black_smoke_s_fast",
                        "dust/falling_dirt_frequent_runner",
                        "smoke/steam_manhole",
                        "smoke/battlefield_smokebank_S_warm_dense",
                        "smoke/building_hole_smoke_mp",
                        "fire/car_fire_mp",
                        "fire/firelp_small_cheap_mp",
                        "misc/falling_ash_mp",
                        "misc/insects_light_hunted_a_mp",
                        "smoke/bootleg_alley_steam",
                        "misc/building_hole_paper_fall_mp",
                        "weather/ground_fog_mp",
                        "misc/leaves_fall_gentlewind_green",
                        "smoke/chimney_smoke_mp",
                        "misc/oil_drip_puddle",
                        "misc/antiair_runner_cloudy",
                        "misc/leaves_spiral_runner",
                        "fire/fire_falling_runner_point",
                        "weather/ceiling_smoke_seatown",
                        "smoke/large_battle_smoke_mp",
                        "misc/light_glow_white",
                        "dust/room_dust_200_z150_mp",
                        "smoke/chimney_smoke_large_mp",
                        "fire/building_hole_embers_mp"
                    };
                    break;
                case "mp_village":
                    precached_fx = new string[]{
                        "distortion/heat_haze_mirage",
                        "dust/falling_dirt_area_runner",
                        "dust/sand_spray_detail_oriented_runner_hardhat",
                        "dust/sand_spray_cliff_oriented_runner_hardhat",
                        "dust/dust_spiral_runner_small",
                        "fire/car_fire_mp",
                        "fire/car_fire_mp_far",
                        "misc/trash_spiral_runner",
                        "misc/birds_takeoff_infrequent_runner",
                        "misc/leaves_fall_gentlewind_mp_village",
                        "misc/leaves_fall_gentlewind_mp_village_far",
                        "misc/insects_carcass_runner",
                        "misc/insects_light_hunted_a_mp",
                        "misc/insect_trail_runner_icbm",
                        "misc/insects_dragonfly_runner_a",
                        "smoke/hallway_smoke_light",
                        "smoke/room_smoke_400",
                        "water/waterfall_mist_mp_village",
                        "water/waterfall_mist_ground",
                        "water/waterfall_village_1",
                        "water/waterfall_village_2",
                        "water/waterfall_drainage_splash",
                        "water/waterfall_drainage_splash_mp",
                        "water/waterfall_drainage_splash_large"
                    };
                    break;
                case "mp_terminal_cls":
                    precached_fx = new string[]{
                        "smoke/ground_smoke1200x1200",
                        "smoke/hallway_smoke_light",
                        "smoke/room_smoke_200",
                        "smoke/room_smoke_400",
                        "dust/light_shaft_motes_airport",
                        "misc/moth_runner"
                    };
                    break;
                case "mp_aground_ss":
                    precached_fx = new string[]{
                        "misc/birds_circle_main",
                        "water/water_wave_splash2_runner",
                        "water/water_shore_splash_xlg_r",
                        "water/water_wave_splash_xsm_runner",
                        "water/water_shore_splash_r",
                        "water/mist_light",
                        "weather/fog_aground",
                        "misc/drips_slow",
                        "maps/mp_crosswalk_ss/mp_cw_insects",
                        "weather/fog_bog_c",
                        "dust/falling_dirt_infrequent_runner_mp",
                        "lights/godrays_aground",
                        "lights/godrays_aground_b",
                        "lights/tinhat_beam",
                        "lights/bulb_single_orange",
                        "maps/mp_overwatch/light_dust_motes_fog",
                        "misc/paper_blowing_Trash_r",
                        "animals/penguin"
                    };
                    break;
                case "mp_courtyard_ss":
                    precached_fx = new string[]{
                        "maps/mp_courtyard_ss/mp_ct_volcano",
                        "maps/mp_courtyard_ss/mp_ct_godrays_a",
                        "maps/mp_courtyard_ss/mp_ct_godrays_b",
                        "maps/mp_courtyard_ss/mp_ct_godrays_c",
                        "maps/mp_courtyard_ss/mp_ct_ash",
                        "maps/mp_courtyard_ss/mp_ct_ambdust",
                        "maps/mp_courtyard_ss/mp_ct_insects",
                        "misc/insects_dragonfly_runner_a"
                    };
                    break;
            }
            return precached_fx.Contains(fx);
        }

        public string UTILS_GetDefCDvar(string key)
        {
            return Call<string>("getdvar", key);
        }

        public void UTILS_SetChatAlias(Entity sender, string player, string alias)
        {
            Entity target = FindSinglePlayer(player);
            if (target == null)
            {
                WriteChatToPlayer(sender, Command.GetMessage("NotOnePlayerFound"));
                return;
            }

            bool hasAlias = ChatAlias.Keys.Contains(target.GUID);

            if (string.IsNullOrEmpty(alias))
            {
                if (hasAlias)
                    ChatAlias.Remove(target.GUID);
                WriteChatToAll(Command.GetString("alias", "reset").Format(new Dictionary<string, string>()
                {
                    {"<player>", target.Name }
                }));
            }
            else
            {
                if (hasAlias)
                    ChatAlias[target.GUID] = alias;
                else
                    ChatAlias.Add(target.GUID, alias);
                WriteChatToAll(Command.GetString("alias", "message").Format(new Dictionary<string, string>()
                {
                    {"<player>", target.Name },
                    {"<alias>", alias}
                }));
            }
            //save settings
            List<string> aliases = new List<string>();
            foreach (KeyValuePair<long, string> entry in ChatAlias)
            {
                aliases.Add(entry.Key.ToString() + "=" + entry.Value);
            }
            System.IO.File.WriteAllLines(ConfigValues.ConfigPath + @"Utils\chatalias.txt", aliases.ToArray());
        }

        public void UTILS_SetForcedClantag(Entity sender, string player, string tag)
        {
            Entity target = FindSinglePlayer(player);
            if (target == null)
            {
                WriteChatToPlayer(sender, Command.GetMessage("NotOnePlayerFound"));
                return;
            }
            bool hasTag = forced_clantags.Keys.Contains(target.GUID);
            if (string.IsNullOrEmpty(tag))
            {
                if (hasTag)
                    forced_clantags.Remove(target.GUID);
                target.SetClantag("");
                WriteChatToAll(Command.GetString("clantag", "reset").Format(new Dictionary<string, string>()
                {
                    {"<player>", target.Name }
                }));
            }
            else
            {
                if (tag.Length > 7)
                {
                    WriteChatToPlayer(sender, Command.GetString("clantag", "error"));
                    return;
                }
                if (hasTag)
                    forced_clantags[target.GUID] = tag;
                else
                    forced_clantags.Add(target.GUID, tag);
                WriteChatToAll(Command.GetString("clantag", "message").Format(new Dictionary<string, string>()
                {
                    {"<player>", target.Name },
                    {"<tag>", tag}
                }));
            }
            //save settings
            List<string> tags = new List<string>();
            foreach (KeyValuePair<long, string> entry in forced_clantags)
            {
                tags.Add(entry.Key.ToString() + "=" + entry.Value);
            }
            System.IO.File.WriteAllLines(ConfigValues.ConfigPath + @"Utils\forced_clantags.txt", tags.ToArray());
        }

        public void UTILS_GetTeamPlayers(out int axis, out int allies)
        {
            axis = 0;
            allies = 0;
            foreach (Entity player in Players)
            {
                string team = player.GetTeam();
                switch (team)
                {
                    case "axis":
                        axis++;
                        break;
                    case "allies":
                        allies++;
                        break;
                }
            }
        }

        public T UTILS_GetFieldSafe<T>(Entity player, string field)
        {
            if (player.HasField(field))
                return player.GetField<T>(field);
            return default(T);
        }

        public string UTILS_ResolveGUID(long GUID)
        {
            if (System.IO.File.Exists(string.Format(ConfigValues.ConfigPath + @"Utils\playerlogs\{0}.txt", GUID)))
                return System.IO.File.ReadAllLines(string.Format(ConfigValues.ConfigPath + @"Utils\playerlogs\{0}.txt", GUID)).Last();
            return "unknown";
        }

        public SerializableDictionary<long, List<Dvar>> UTILS_PersonalPlayerDvars_load()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(SerializableDictionary<long, List<Dvar>>));
            using (FileStream fs = new FileStream(ConfigValues.ConfigPath + @"Utils\internal\PersonalPlayerDvars.xml", FileMode.Open))
            {
                return (SerializableDictionary<long, List<Dvar>>)xmlSerializer.Deserialize(fs);
            }
        }
        public void UTILS_PersonalPlayerDvars_save(SerializableDictionary<long, List<Dvar>> db)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(SerializableDictionary<long, List<Dvar>>));
            using (FileStream fs = new FileStream(ConfigValues.ConfigPath + @"Utils\internal\PersonalPlayerDvars.xml", FileMode.Create))
            {
                xmlSerializer.Serialize(fs, db);
            }
        }

        public static void Delay(int ms, System.Timers.ElapsedEventHandler action)
        {
            System.Timers.Timer _timer = new System.Timers.Timer(ms);
            _timer.Elapsed += (o, e) => { _timer.Enabled = false; };
            _timer.Elapsed += action;
            _timer.Enabled = true;
        }
        public static string UTILS_GetDSRName()
        {
            return DGAdmin.Mem.ReadString(0x6480E70, 32);
        }

        public bool UTILS_WeaponAllowed(string s)
        {
            if (s == "none")
                return true;

            foreach (string weapon in RestrictedWeapons)
                if (s.StartsWith(weapon))
                    return false;
            return true;
        }

        public bool UTILS_Antiweaponhack_allowweapon(string weapon)
        {
            if (UTILS_WeaponAllowed(weapon))
                return true;
            else
            {
                foreach (string _weapon in RestrictedWeapons)
                    if (weapon.StartsWith(_weapon))
                    {
                        RestrictedWeapons.RemoveAt(RestrictedWeapons.IndexOf(_weapon));
                        return true;
                    }
                return false;
            }
        }

        public void UTILS_ServerTitle_MapFormat()
        {
            string mapname = DevMapName2Mapname(UTILS_GetDvar("mapname"));

            // ToTitleCase
            if (!String.IsNullOrEmpty(mapname))
            {
                Char[] ca = mapname.ToCharArray();
                ca[0] = Char.ToUpperInvariant(ca[0]);
                mapname = new string(ca);
            }

            UTILS_ServerTitle(ConfigValues.servertitle_map.Format(new Dictionary<string, string>()
                {
                    {"<map>", mapname }
                })
                , ConfigValues.servertitle_mode);
        }

        public List<Dvar> UTILS_DvarListUnion(List<Dvar> set1, List<Dvar> set2)
        {
            Dictionary<string, string> _dvars = set1.ToDictionary(x => x.key, x => x.value);
            foreach (Dvar dvar in set2)
                if (_dvars.ContainsKey(dvar.key))
                    _dvars[dvar.key] = dvar.value;
                else
                    _dvars.Add(dvar.key, dvar.value);
            set1.Clear();
            foreach (KeyValuePair<string, string> dvar in _dvars)
                set1.Add(new Dvar { key = dvar.Key, value = dvar.Value });
            return set1;
        }

        //this is A wrong way to convert encodings :)
        public static string Win1251xUTF8(string s)
        {
            string utf8_String = s;
            byte[] bytes = Encoding.Default.GetBytes(utf8_String);
            for (int i = 0; i < bytes.Length - 1; i++)
            {
                if ((bytes[i] == 0xC3) && (bytes[i + 1] >= 0x80) && (bytes[i + 1] <= 0xAF)) //А-Яа-п
                { bytes[i] = 0xD0; bytes[i + 1] += 0x10; } else
                if ((bytes[i] == 0xC3) && (bytes[i + 1] >= 0xB0) && (bytes[i + 1] <= 0xBF)) //р-я
                { bytes[i] = 0xD1; bytes[i + 1] -= 0x30; } else
                if ((bytes[i] == 0xC2) && (bytes[i + 1] == 0xA8)) //Ё
                { bytes[i] = 0xD0; bytes[i + 1] = 0x81; }  else
                if ((bytes[i] == 0xC2) && (bytes[i + 1] == 0xB8)) //ё
                { bytes[i] = 0xD1; bytes[i + 1] = 0x91; }
            }
                utf8_String = Encoding.UTF8.GetString(bytes);
            return utf8_String;
        }

    }

    public static partial class Extensions
    {
        public static string RemoveColors(this string message)
        {
            foreach (string color in DGAdmin.Data.Colors.Keys)
                message = message.Replace(color, "");
            return message;
        }

        public static void LogTo(this string message, params DGAdmin.SLOG[] logs)
        {
            foreach (DGAdmin.SLOG log in logs)
            {
                log.WriteInfo(message);
            }
        }

        public static void IPrintLnBold(this Entity player, string message)
        {
            player.Call("iprintlnbold", message);
        }

        public static void IPrintLn(this Entity player, string message)
        {
            player.Call("iprintln", message);
        }

        public static int GetEntityNumber(this Entity player)
        {
            return player.Call<int>("getentitynumber");
        }

        public static void Suicide(this Entity player)
        {
            player.Call("suicide");
        }

        public static string GetTeam(this Entity player)
        {
            return player.GetField<string>("sessionteam");
        }

        public static bool IsSpectating(this Entity player)
        {
            return player.GetTeam() == "spectator";
        }

        public static string Format(this string str, Dictionary<string, string> format)
        {
            foreach (KeyValuePair<string, string> pair in format)
                str = str.Replace(pair.Key, pair.Value);
            return str;
        }

        public static TValue GetValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
        {
            TValue value;
            if (dict.TryGetValue(key, out value))
                return value;
            throw new ArgumentOutOfRangeException();
        }

        public static string[] Condense(this string[] arr, int condenselevel = 40, string separator = ", ")
        {
            if (arr.Length < 1)
                return arr;
            List<string> lines = new List<string>();
            int index = 0;
            string line = arr[index++];
            while (index < arr.Length)
            {
                if ((line + separator + arr[index]).RemoveColors().Length > condenselevel)
                {
                    lines.Add(line);
                    line = arr[index];
                    index++;
                    continue;
                }
                line += separator + arr[index];
                index++;
            }
            lines.Add(line);
            return lines.ToArray();
        }

        public static DGAdmin.HWID GetHWID(this Entity player)
        {
            return new DGAdmin.HWID(player);
        }

        public static string GetHWIDRaw(this Entity player)
        {
            int address = DGAdmin.Data.HWIDDataSize * player.GetEntityNumber() + DGAdmin.Data.HWIDOffset;
            string formattedhwid = "";
            unsafe
            {
                for (int i = 0; i < 12; i++)
                {
                    formattedhwid += (*(byte*)(address + i)).ToString("x2");
                }
            }
            return formattedhwid;
        }

        public static string GetClantag(this Entity player)
        {
            if (player == null || !player.IsPlayer)
                return null;
            int address = DGAdmin.Data.ClantagPlayerDataSize * player.GetEntityNumber() + DGAdmin.Data.ClantagOffset;
            return DGAdmin.Mem.ReadString(address, 8);
        }

        public static void SetClantag(this Entity player, string clantag)
        {
            if (player == null || !player.IsPlayer || clantag.Length > 7)
                return;
            int address = DGAdmin.Data.ClantagPlayerDataSize * player.GetEntityNumber() + DGAdmin.Data.ClantagOffset;
            unsafe
            {
                for (int i = 0; i < clantag.Length; i++)
                {
                    *(byte*)(address + i) = (byte)clantag[i];
                }
                *(byte*)(address + clantag.Length) = 0;
            }
        }

        public static bool IsHex(this char ch)
        {
            return DGAdmin.Data.HexChars.Contains(ch);
        }

        public static DGAdmin.XNADDR GetXNADDR(this Entity player)
        {
            return new DGAdmin.XNADDR(player);
        }
    }
}
