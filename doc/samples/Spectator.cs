using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfinityScript;
using System.IO;

namespace fly
{
    public class fly : BaseScript
    {
        public fly()
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
             
    }
}