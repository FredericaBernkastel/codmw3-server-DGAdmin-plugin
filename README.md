## "DGAdmin" - Call of Duty: MW3 dedicated server plugin
DG Admin script for Call of Duty: MW3 dedicated server. Based on RGAdmin v1.05, modified by **F. Bernkastel**<br>
Complete admin guide can be found [here](https://drive.google.com/file/d/0B4OfimTH0gRhdGxoSHBJY194UWs/view?usp=sharing).<br><br>
#### **Project officially [closed!](https://github.com/FredericaBernkastel/codmw3-server-DGAdmin-plugin/issues/8#issuecomment-225008784)** ####
<br><br>
**New commands** (not included in RG Admin)

**`!apply`**<br>
`　　　print apply message (commands\apply.txt)`
        
**`!night <on/off>`**<br>
`　　　turn night mode for you`

**`!sunlight <float RED> <float GREEN> <float BLUE>`**<br>
`　　　set sunlight color multiplier`

**`!cdvar <int/foat/string/direct> <key> <value>`**<br>
`　　　set custom cdvar. In direct mode, `<br>
`　　　you can separate multiple values by comma.`

**`!alias <player> [alias]`**<br>
`　　　set chat alias *(leave [alias] field to reset it)*`

**`!myalias [alias]`**<br>

**`!daytime <day|night|morning|cloudy>`**<br>
`　　　Force graphics mode for all players. `<br>
`　　　If “night”, commands “!fx” and “!night” are blocked.`

**`!kd <player> <kills> <deaths>`**<br>
`　　　Set custom kills/deaths score for player. `<br>
`　　　(Affects only scoreboard, but doesn’t actually changes it).`

**`!unban <player>`**<br>
`　　　Unban single player by name.`

**`!unban-id <ban id>`**<br>
`　　　Deletes given banentry from banlist.`<br>
`　　　You can get banentries by doing !lastbans or !searchbans.`

**`!report <message>`**<br>
`　　　Player can report hackers. Message will be sent to all online admins, and saved to history.`

**`!lastreports [amount]`**<br>
`　　　Admins can access to !report history. Amount may be 1 – 8, default is 4.`

**`!setfx <fx> [spawn key]`**<br>
`　　　Spawn custom FX at player origin. Triggered by key. If key not specified, default is “activate”.`

**`!fire`**<br>
`　　　Player emit beatiful fire sparks. `

**`!suicide`**<br>
`　　　Suicide.`

**`!yes`**<br>
`　　　Confirm command execution.`

**`!no`**<br>
`　　　Abort command execution.`

**`!sdvar <key> [value]`**<br>
`　　　Set server dvar. If value not specified, NULL value will be set. `
`　　　Multiple values are separated by space.`

**`!3rdperson`**<br>
`　　　Forced 3rd person view.`

**`!teleport <player1> <player2>`**<br>
`　　　Teleport player1 to player2.`

**`!fly <on|off> [bound key]`**<br>
`　　　Invisible flying god mode. If key not specified, default is “F” (activate).`

**`!jump <<height> | default>`**<br>
`　　　Set jump height.`

**`!speed <<speed> | default>`**<br>
`　　　Set speed multiplier.`

**`!gravity <<g> | default>`**<br>
`　　　Set gravity force.`

**`!ac130 <all | <player>> [-p]`**<br>
`　　　Hand-held AC130 gun. `<br>
`　　　–p flag makes it permanent, until round end.`

**`!register`**<br>
`　　　Register to XLR Stats.`

**`!xlrstats [player]`**<br>
`　　　Your statistics. `<br>
`　　　Specify player, if you wanna know how good other players.`

**`!xlrtop [amount]`**<br>
`　　　XLR Top scores. `<br>
`　　　Amount should’nt be greather than 8, and smaller than 1.`

**`!playfxontag <fx> [tag = j_head]`**<br>
`　　　Like !setfx, but effect is linked to the player's origin.`

**`!setclantag <player> [tag]`**<br>
`　　　Set clan tag of player. Up to 7 characters.`

**`!rotatescreen <player> <degree>`**<br>
`　　　Set camera roll of player. Very abusive.`

**`!votekick <player> [reason]`**<br>
`　　　Start a vote to kick player.`

**Misc comands** `(not enabled by default)`

**`!svpassword [password]`**<br>
`　　　Will set server password in “players2\server.cfg”, and kill server.`<br>
`　　　*Server should be run under daemon*, i.e. “Alani’s server manager” to be auto-restarted.`

**New set of broadcast commands** `(print to the public chat):`
  - **`!@admins`**
  - **`!@rules`**
  - **`!@apply`**
  - **`!@time`**
  - **`!@xlrstats`**
  - **`!@xlrtop`**


**New features**
 - **`Spree messages`**` (option: settings_enable_spree_messages)`
 - **`Chat aliases`**` (option: settings_enable_chat_alias)`
 - **`AntiNoScope`**` (option: settings_isnipe_antinoscope)`
 - **`AntiCRTK`**` (option: settings_isnipe_anticrtk)`
 - **`AntiBoltCancel`**`  (option: settings_isnipe_antiboltcancel)`
 - **`XLR Stats`**` (option: settings_enable_xlrstats)`
 - **`Advanced cdvar manager`**` - you can set default client dvars in "Utils\cdvars.txt"`
 - **`Voting`**

` Fixed issue with trophy-kill (when victim gets banned for it);`

 `Fixed issue with !ac130 in isnipe mode (when issuer gets banned for it)`

![nipaa~ =^_^=](http://anime.net.kg/uploads/pictures/Furude.Rika.low.1153817.png)
