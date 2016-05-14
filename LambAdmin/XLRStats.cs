using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfinityScript;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace LambAdmin
{
    public partial class DGAdmin
    {
        public class XLR_database
        {
            public struct XLREntry
            {
                [XmlAttribute]
                public long kills;
                [XmlAttribute]
                public long deaths;
                [XmlAttribute]
                public long headshots;
                [XmlAttribute]
                public long tk_kills;
                [XmlAttribute]
                public long shots_total;
                [XmlAttribute]
                public float score;
            }
            [Flags] public enum XLRUpdateFlags
            {
                kill = 1,
                death = 2,
                headshot = 4,
                tk_kill = 8,
                weapon_fired = 16
            }

            public string FilePath = @"Utils\internal\XLRStats.xml";
            // <GUID, XLREntry>
            public volatile SerializableDictionary<long, XLREntry> xlr_players = new SerializableDictionary<long, XLREntry>();
            
            public void Init()
            {
                if (!System.IO.File.Exists(ConfigValues.ConfigPath + FilePath))
                    System.IO.File.WriteAllLines(ConfigValues.ConfigPath + FilePath, new string[] { 
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?>",
                    "<dictionary />",
                });
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(SerializableDictionary<long, XLREntry>));
                using (FileStream fs = new FileStream(ConfigValues.ConfigPath + FilePath, FileMode.Open))
                {
                    xlr_players = (SerializableDictionary<long, XLREntry>)xmlSerializer.Deserialize(fs);
                }

            }

            public void Update(long GUID, XLRUpdateFlags flags)
            {
                if (!xlr_players.ContainsKey(GUID))
                    return;
                XLREntry entry = xlr_players[GUID];
                if (flags.HasFlag(XLRUpdateFlags.kill))
                    entry.kills += 1;
                if (flags.HasFlag(XLRUpdateFlags.death))
                    entry.deaths += 1;
                if (flags.HasFlag(XLRUpdateFlags.headshot))
                    entry.headshots += 1;
                if (flags.HasFlag(XLRUpdateFlags.tk_kill))
                    entry.tk_kills += 1;
                if (flags.HasFlag(XLRUpdateFlags.weapon_fired))
                    entry.shots_total += 1;

                entry.score = math_score(entry);

                xlr_players[GUID] = entry;
            }

            public void Save()
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(SerializableDictionary<long, XLREntry>));
                using (FileStream fs = new FileStream(ConfigValues.ConfigPath + FilePath, FileMode.Create))
                {
                    xmlSerializer.Serialize(fs, xlr_players);
                }
            }

            public bool CMD_Register(long GUID)
            {
                if (xlr_players.ContainsKey(GUID))
                    return false;
                xlr_players.Add(GUID, new XLREntry { deaths = 0, headshots = 0, shots_total = 0, tk_kills = 0, kills = 0, score = 0 });
                return true;
            }

            public List<KeyValuePair<long, XLREntry>> CMD_XLRTOP(int amount)
            {
                amount = Math.Min(amount, xlr_players.Count);
                List<KeyValuePair<long, XLREntry>> top = xlr_players.ToList();
                if (xlr_players.Count == 0)
                    return top;
                top.Sort((pair1, pair2) => pair1.Value.score.CompareTo(pair2.Value.score));
                return top.Take(amount).ToList();
            }

            public float math_kd(XLREntry entry)
            {
                return (entry.kills / (float)((entry.deaths == 0) ? 1 : entry.deaths));
            }
            public float math_precision(XLREntry entry)
            {
                return (entry.shots_total == 0) ?
                    0 :
                    (entry.kills - entry.tk_kills) / (float)entry.shots_total;
            }
            public float math_score(XLREntry entry)
            {
                return math_kd(entry) * math_precision(entry) * 100;
            }
        }

        public volatile XLR_database xlr_database;

        public void XLR_OnServerStart()
        {
            WriteLog.Info("Initializing XLRstats...");
            xlr_database = new XLR_database();
            xlr_database.Init();
            PlayerConnected += XLR_OnPlayerConnected;
            OnPlayerKilledEvent += XLR_OnPlayerKilled;
            WriteLog.Info("Done initializing XLRstats.");
        }

        public void XLR_OnPlayerKilled(Entity player, Entity inflictor, Entity attacker, int damage, string mod, string weapon, Vector3 dir, string hitLoc)
        {
            try
            {
                xlr_database.Update(player.GUID, XLR_database.XLRUpdateFlags.death);

                if (!attacker.IsPlayer || (attacker == player))
                    return;

                XLR_database.XLRUpdateFlags flags = new XLR_database.XLRUpdateFlags();
                flags = flags | XLR_database.XLRUpdateFlags.kill;
                if (mod == "MOD_HEAD_SHOT")
                    flags = flags | XLR_database.XLRUpdateFlags.headshot;
                if (weapon == "throwingknife_mp")
                    flags = flags | XLR_database.XLRUpdateFlags.tk_kill;

                xlr_database.Update(attacker.GUID, flags);
            }
            catch (Exception e)
            {
                DGAdmin.WriteLog.Error("Error at XLR::OnPlayerKilled");
                DGAdmin.WriteLog.Error(e.Message);
            }
        }

        public void XLR_OnPlayerConnected(Entity player)
        {
            player.OnNotify("weapon_fired", new Action<Entity, Parameter>((_player, args) =>
            {
                xlr_database.Update(player.GUID, XLR_database.XLRUpdateFlags.weapon_fired);
            }));
        }

        #region COMMANDS

        public void XLR_InitCommands()
        {
            // REGISTER
            CommandList.Add(new Command("register", 0, Command.Behaviour.Normal,
                (sender, arguments, optarg) =>
                {
                    if(xlr_database.CMD_Register(sender.GUID))
                        WriteChatToPlayer(sender, Command.GetString("register", "message"));
                    else
                        WriteChatToPlayer(sender, Command.GetString("register", "error"));
                }));

            // XLRSTATS
            CommandList.Add(new Command("xlrstats", 0, Command.Behaviour.HasOptionalArguments,
                (sender, arguments, optarg) =>
                {
                    Entity target;
                    if (!String.IsNullOrEmpty(optarg))
                    {
                        target = FindSinglePlayer(optarg);
                        if (target == null)
                        {
                            WriteChatToPlayer(sender, Command.GetMessage("NotOnePlayerFound"));
                            return;
                        }
                    }
                    else
                        target = sender;
                    if (xlr_database.xlr_players.ContainsKey(target.GUID)){
                        XLR_database.XLREntry xlr_entry = xlr_database.xlr_players[target.GUID];
                        WriteChatToPlayer(sender, Command.GetString("xlrstats", "message").Format(
                            new Dictionary<string, string>()
                                {
                                    {"<score>", xlr_entry.score.ToString()},
                                    {"<kills>", xlr_entry.kills.ToString()},
                                    {"<deaths>", xlr_entry.deaths.ToString()},
                                    {"<kd>", xlr_database.math_kd(xlr_entry).ToString() },
                                    {"<headshots>", xlr_entry.headshots.ToString()},
                                    {"<tk_kills>", xlr_entry.tk_kills.ToString()},
                                    {"<precision>", (xlr_database.math_precision(xlr_entry) * 100).ToString()},
                                }));
                    }
                    else
                        WriteChatToPlayer(sender, Command.GetString("xlrstats", "error"));
                }));

            // @XLRSTATS
            CommandList.Add(new Command("@xlrstats", 0, Command.Behaviour.HasOptionalArguments,
                (sender, arguments, optarg) =>
                {
                    Entity target;
                    if (!String.IsNullOrEmpty(optarg))
                    {
                        target = FindSinglePlayer(optarg);
                        if (target == null)
                        {
                            WriteChatToPlayer(sender, Command.GetMessage("NotOnePlayerFound"));
                            return;
                        }
                    }
                    else
                        target = sender;
                    if (xlr_database.xlr_players.ContainsKey(target.GUID))
                    {
                        XLR_database.XLREntry xlr_entry = xlr_database.xlr_players[target.GUID];
                        WriteChatToAll(Command.GetString("@xlrstats", "message").Format(
                            new Dictionary<string, string>()
                                {
                                    {"<player>", target.Name},
                                    {"<score>", xlr_entry.score.ToString()},
                                    {"<kills>", xlr_entry.kills.ToString()},
                                    {"<deaths>", xlr_entry.deaths.ToString()},
                                    {"<kd>", xlr_database.math_kd(xlr_entry).ToString() },
                                    {"<headshots>", xlr_entry.headshots.ToString()},
                                    {"<tk_kills>", xlr_entry.tk_kills.ToString()},
                                    {"<precision>", (xlr_database.math_precision(xlr_entry) * 100).ToString()},
                                }));
                    }
                    else
                        WriteChatToPlayer(sender, Command.GetString("xlrstats", "error"));
                }));

            // XLRTOP
            CommandList.Add(new Command("xlrtop", 0, Command.Behaviour.HasOptionalArguments,
                (sender, arguments, optarg) =>
                {
                    int amount = 4;
                    if(!String.IsNullOrEmpty(optarg))
                        if(!int.TryParse(optarg, out amount)){
                            WriteChatToPlayer(sender, Command.GetString("xlrtop", "usage"));
                            return;
                        }
                    List<KeyValuePair<long, XLR_database.XLREntry>> topscores = xlr_database.CMD_XLRTOP(amount);
                    if (topscores.Count == 0)
                    {
                        WriteChatToPlayer(sender, Command.GetString("xlrtop", "error"));
                        return;
                    }
                    List<string> output = new List<string>();
                    for (int i = 0; i < topscores.Count; i++)
                    {
                        XLR_database.XLREntry entry = topscores.ElementAt(i).Value;
                        output.Add(Command.GetString("xlrtop", "message").Format(
                            new Dictionary<string, string>()
                            {
                                {"<place>",(topscores.Count - i).ToString()},
                                {"<player>", UTILS_ResolveGUID(topscores.ElementAt(i).Key)},
                                {"<score>",entry.score.ToString()},
                                {"<kills>",entry.kills.ToString()},
                                {"<kd>", xlr_database.math_kd(entry).ToString()},
                                {"<precision>",(xlr_database.math_precision(entry)*100).ToString()}
                            }));
                    }
                    WriteChatToPlayerMultiline(sender, output.ToArray(), 1500);
                }));

            // @XLRTOP
            CommandList.Add(new Command("@xlrtop", 0, Command.Behaviour.HasOptionalArguments,
                (sender, arguments, optarg) =>
                {
                    int amount = 4;
                    if (!String.IsNullOrEmpty(optarg))
                        if (!int.TryParse(optarg, out amount))
                        {
                            WriteChatToPlayer(sender, Command.GetString("xlrtop", "usage"));
                            return;
                        }
                    List<KeyValuePair<long, XLR_database.XLREntry>> topscores = xlr_database.CMD_XLRTOP(amount);
                    if (topscores.Count == 0)
                    {
                        WriteChatToPlayer(sender, Command.GetString("xlrtop", "error"));
                        return;
                    }
                    List<string> output = new List<string>();
                    for (int i = 0; i < topscores.Count; i++)
                    {
                        XLR_database.XLREntry entry = topscores.ElementAt(i).Value;
                        output.Add(Command.GetString("xlrtop", "message").Format(
                            new Dictionary<string, string>()
                            {
                                {"<place>",(topscores.Count - i).ToString()},
                                {"<player>", UTILS_ResolveGUID(topscores.ElementAt(i).Key)},
                                {"<score>",entry.score.ToString()},
                                {"<kills>",entry.kills.ToString()},
                                {"<kd>", xlr_database.math_kd(entry).ToString()},
                                {"<precision>",(xlr_database.math_precision(entry)*100).ToString()}
                            }));
                    }
                    WriteChatToAllMultiline(output.ToArray(), 1500);
                }));
        }
        #endregion
    }
}
