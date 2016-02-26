using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfinityScript;
using System.Net;

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

        public static partial class ConfigValues
        {
            public static string Version = "v3.1n3";
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
            public static bool DEBUG = false;
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
                {"foundation", "mp_cement"},
                {"sanctuary", "mp_museum"},
                {"lookout", "mp_restrepo_ss"},
                {"vortex", "mp_six_ss"},
                {"getaway", "mp_hillside_ss"},
                {"parish", "mp_parish"},
                {"terminal", "mp_terminal_cls"}
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
                if (System.IO.File.Exists(ConfigValues.ConfigPath + @"Utils\internal\announcers\" + name + ".txt"))
                    return int.Parse(System.IO.File.ReadAllText(ConfigValues.ConfigPath + @"Utils\internal\announcers\" + name + ".txt"));
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

        public void WriteChatToAll(string message)
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
            string invariantname = player.Name.ToLowerInvariant();
            foreach (Entity scrub in Players)
            {
                string invariantscrub = scrub.Name.ToLowerInvariant();
                if (player.EntRef != scrub.EntRef && (invariantscrub.Contains(invariantname) || invariantname.Contains(invariantscrub)))
                {
                    CMD_kick(player, "Your name is containing another user's/contained by another user");
                    return;
                }
            }

            UTILS_SetClientDvars(player);
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
                UTILS_SetClientDvars(player);
            }

            // RGADMIN HUDELEM
            if (bool.Parse(Sett_GetString("settings_showversion")))
            {
                RGAdminMessage = HudElem.CreateServerFontString("hudsmall", 0.5f);
                RGAdminMessage.SetPoint("BOTTOMRIGHT", "BOTTOMRIGHT");
                RGAdminMessage.SetText("DG Admin " + ConfigValues.Version);
                RGAdminMessage.Color = new Vector3(1f, 0.75f, 0f);
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

            // UNFREEZE PLAYERS ON GAME END
            if (bool.Parse(Sett_GetString("settings_unfreezeongameend")))
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
                foreach (Entity player in Players)
                    if (!CMDS_IsRekt(player))
                        player.Call("freezecontrols", false);
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
            switch (ConfigValues.settings_daytime)
            {
                case "day": Call("setsunlight", 1f, 1f, 1f); break;
                case "night": UTILS_SetClientNightVision(player); Call("setsunlight", 0f, 0.7f, 1f); break;
                case "morning": Call("setsunlight", 1.5f, 0.65f, 0f); break;
                case "cloudy": Call("setsunlight", 0f, 0f, 0f); break;
            }
        }

        public void ExecuteCommand(string command)
        {
            Utilities.ExecuteCommand(command);
        }

        public void UTILS_SetClientDvars(Entity player)
        {
            player.SetClientDvar("g_TeamName_Allies", ConfigValues.settings_teamnames_allies);
            player.SetClientDvar("g_TeamName_Axis", ConfigValues.settings_teamnames_axis);
            player.SetClientDvar("g_TeamIcon_Allies", ConfigValues.settings_teamicons_allies);
            player.SetClientDvar("g_TeamIcon_Axis", ConfigValues.settings_teamicons_axis);
        }

        public void UTILS_SetClientNightVision(Entity player)
        {
            player.SetClientDvar("r_filmUseTweaks", "1");
            player.SetClientDvar("r_filmTweakEnable", "1");
            player.SetClientDvar("r_filmTweakLightTint", "0 0.2 1");
            player.SetClientDvar("r_filmTweakDarkTint", "0 0.125 1");
        }

        public void UTILS_GetDefCDvar(string key)
        {
            string val = Call<string>("getdvar", key);
            WriteLog.Info(key + "=" + val);
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
