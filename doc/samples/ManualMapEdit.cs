using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfinityScript;
using System.IO;

namespace ManualMapEdit
{
    public class ManualMapEdit : BaseScript
    {
        bool creating = false;
        float startx;
        float starty;
        float startz;
        float endx;
        float endy;
        float endz;

        public ManualMapEdit()
            : base()
        {
            PlayerConnected += new Action<Entity>(player =>
            {
                player.Call("notifyonplayercommand", "fly", "+frag");
                player.OnNotify("fly", (ent) =>
                {
                    if (player.GetField<string>("sessionstate") != "spectator")
                    {
                        player.Call("allowspectateteam", "freelook", true);
                        player.SetField("sessionstate", "spectator");
                        player.Call("setcontents", 0);
                    }
                    else
                    {
                        player.Call("allowspectateteam", "freelook", false);
                        player.SetField("sessionstate", "playing");
                        player.Call("setcontents", 100);
                    }
                });
            });
        }
        public override void OnSay(Entity player, string playerName, string text)
        {
            var mapname = Call<string>("getdvar", "mapname");

            string[] msg = text.Split(' ');


            if (msg[0] == "!restart")
            {
                Call("setdvar", "map_restart", "1");
            }

            if (msg[0] == "!wall" && !creating)
            {
                player.Call("iprintlnbold", "^2START SET: " + player.Origin);
                startx = player.Origin.X;
                starty = player.Origin.Y;
                startz = player.Origin.Z;
                creating = true;
            }
            else if (msg[0] == "!wall" && creating)
            {
                player.Call("iprintlnbold", "^2END SET: "+player.Origin);
                endx = player.Origin.X;
                endy = player.Origin.Y;
                endz = player.Origin.Z;
                creating = false;
                File.AppendAllText("scripts\\maps\\" + mapname + ".txt", Environment.NewLine + "wall: (" + startx + "," + starty + "," + startz + ") ; (" + endx + "," + endy + "," + endz + ")");
            }

            if (msg[0] == "!ramp" && !creating)
            {
                player.Call("iprintlnbold", "^2START SET: " + player.Origin);
                startx = player.Origin.X;
                starty = player.Origin.Y;
                startz = player.Origin.Z;
                creating = true;
            }
            else if (msg[0] == "!ramp" && creating)
            {
                player.Call("iprintlnbold", "^2END SET: " + player.Origin);
                endx = player.Origin.X;
                endy = player.Origin.Y;
                endz = player.Origin.Z;
                creating = false;
                File.AppendAllText("scripts\\maps\\" + mapname + ".txt", Environment.NewLine + "ramp: (" + startx + "," + starty + "," + startz + ") ; (" + endx + "," + endy + "," + endz + ")");
            }

            if (msg[0] == "!tp" && !creating)
            {
                player.Call("iprintlnbold", "^2START SET: " + player.Origin);
                startx = player.Origin.X;
                starty = player.Origin.Y;
                startz = player.Origin.Z;
                creating = true;
            }
            else if (msg[0] == "!tp" && creating)
            {
                player.Call("iprintlnbold", "^2END SET: " + player.Origin);
                endx = player.Origin.X;
                endy = player.Origin.Y;
                endz = player.Origin.Z;
                creating = false;
                File.AppendAllText("scripts\\maps\\" + mapname + ".txt", Environment.NewLine + "elevator: (" + startx + "," + starty + "," + startz + ") ; (" + endx + "," + endy + "," + endz + ")");
            }

            if (msg[0] == "!htp" && !creating)
            {
                player.Call("iprintlnbold", "^2START SET: " + player.Origin);
                startx = player.Origin.X;
                starty = player.Origin.Y;
                startz = player.Origin.Z;
                creating = true;
            }
            else if (msg[0] == "!htp" && creating)
            {
                player.Call("iprintlnbold", "^2END SET: " + player.Origin);
                endx = player.Origin.X;
                endy = player.Origin.Y;
                endz = player.Origin.Z;
                creating = false;
                File.AppendAllText("scripts\\maps\\" + mapname + ".txt", Environment.NewLine + "HiddenTP: (" + startx + "," + starty + "," + startz + ") ; (" + endx + "," + endy + "," + endz + ")");
            }

            if (msg[0] == "!floor" && !creating)
            {
                player.Call("iprintlnbold", "^2START SET: " + player.Origin);
                startx = player.Origin.X;
                starty = player.Origin.Y;
                startz = player.Origin.Z;
                creating = true;
            }
            else if (msg[0] == "!floor" && creating)
            {
                player.Call("iprintlnbold", "^2END SET: " + player.Origin);
                endx = player.Origin.X;
                endy = player.Origin.Y;
                endz = player.Origin.Z;
                creating = false;
                File.AppendAllText("scripts\\maps\\" + mapname + ".txt", Environment.NewLine + "floor: (" + startx + "," + starty + "," + startz + ") ; (" + endx + "," + endy + "," + endz + ")");
            }

        }
    }
}