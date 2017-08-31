using System;
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
            public static string format_message = "{0}{1}^7: {2}";
            public static string format_prefix_spectator = "(Spectator)";
            public static string format_prefix_dead = "^7(Dead)^7";
            public static string format_prefix_team = "^5[TEAM]^7";
        }

        public void CHAT_WriteChat(Entity sender, ChatType type, string message)
        {
            if (sender.isMuted())
                return;
            /*ChatLog(sender, message, type);
            if (sender.HasField("nootnoot") && sender.GetField<int>("nootnoot") == 1)
                message = "noot noot";
            if(type == ChatType.All)
            {
                if (sender.IsAlive)
                {
                    Utilities.RawSayAll(string.Format(ConfigValues.format_message, "", sender.GetFormattedName(database), message));
                    return;
                }
                if (sender.IsSpectating())
                {
                    Utilities.RawSayAll(string.Format(ConfigValues.format_message, ConfigValues.format_prefix_spectator, sender.GetFormattedName(database), message));
                    return;
                }
                if(!sender.IsAlive)
                {
                    Utilities.RawSayAll(string.Format(ConfigValues.format_message, ConfigValues.format_prefix_dead, sender.GetFormattedName(database), message));
                    return;
                }
            }
            else
            {
                string team = sender.GetTeam();
                if (sender.IsAlive)
                {
                    CHAT_WriteToAllFromTeam(team, string.Format(ConfigValues.format_message, ConfigValues.format_prefix_team, sender.GetFormattedName(database), message));
                    return;
                }
                if (sender.IsSpectating())
                {
                    CHAT_WriteToAllFromTeam(team, string.Format(ConfigValues.format_message, ConfigValues.format_prefix_team + ConfigValues.format_prefix_spectator, sender.GetFormattedName(database), message));
                    return;
                }
                if (!sender.IsAlive)
                {
                    CHAT_WriteToAllFromTeam(team, string.Format(ConfigValues.format_message, ConfigValues.format_prefix_team + ConfigValues.format_prefix_dead, sender.GetFormattedName(database), message));
                    return;
                }
            }*/
        }

        public void ChatLog(Entity sender, string message, ChatType chattype)
        {
            Log.Write(LogLevel.Info, "[" + chattype + "] " + sender.Name + ": " + message);
        }

        public void CHAT_WriteToAllFromTeam(string team, string message)
        {
            foreach(Entity player in Players)
                if(player.GetTeam() == team)
                    Utilities.RawSayTo(player, message);
        }
    }
}
