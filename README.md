## "DGAdmin" - Call of Duty: MW3 dedicated server plugin
DG Admin script for Call of Duty: MW3 dedicated server. Based on RGAdmin v1.05, modified by **F. Bernkastel**<br>
Complete admin guide can be found [here](https://docs.google.com/document/d/1SFeFLtie7718nz9ctME5oN99iv3-p2XiIQqsHDqasAo/edit?usp=sharing).<br><br>
**New commands** (not included in RG Admin)
```Javascript
!apply
    print apply message (commands\apply.txt)
 
!app
    print server wide apply message (commands\apply.txt)
    
!night <on/off>
    turn night mode for you
    
!sunlight <float RED> <float GREEN> <float BLUE>
    set sunlight color multiplier
    
!cdvar <int/foat/string/direct> <key> <value>
    set custom cdvar. In direct mode, 
    you can separate multiple values by comma.
    
!alias <player> [alias]
    set chat alias *(leave [alias] field to reset it)*
    
!myalias [alias]

!daytime <day|night|morning|cloudy>
    Force graphics mode for all players. 
    If “night”, commands “!fx” and “!night” are blocked.
    
!kd <player> <kills> <deaths>
    Set custom kills/deaths score for player. 
    (Affects only scoreboard, but doesn’t actually changes it).
    
!unban <player>
    Unban single player by name.
    
!unban-id <ban id>
    Deletes given banentry from banlist.
    You can get banentries by doing !lastbans or !searchbans.
    
!report <message>
    Player can report hackers. Message will be sent to all online admins, and saved to history.
    
!lastreports [amount]
    Admins can access to !report history. Amount may be 1 – 8, default is 4.
    
!setfx <fx> [spawn key]
    Spawn custom FX at player origin. Triggered by key. If key not specified, default is “activate”.
    
!fire
    Player emit beatiful fire sparks. 
    
!suicide
    Suicide.
  
!yes
    Confirm command execution.
    
!no
    Abort command execution.

!sdvar <key> [value]
    Set server dvar. If value not specified, NULL value will be set. 
    Multiple values are separated by space.

!3rdperson
    Forced 3rd person view.

!teleport <player1> <player2>
    Teleport player1 to player2.

!fly <on|off> [bound key]
    Invisible flying god mode. If key not specified, default is “F” (activate).

!jump <<height> | default>
    Set jump height.

!speed <<speed> | default>
    Set speed multiplier.

!gravity <<g> | default>
    Set gravity force.

!ac130 <all | <player>> [-p]
    Hand-held AC130 gun. 
    –p flag makes it permanent, until round end.

!register
    Register to XLR Stats.

!xlrstats [player]
    Your statistics. 
    Specify player, if you wanna know how good other players.

!xlrtop [amount]
    XLR Top scores. 
    Amount should’nt be greather than 8, and smaller than 1.

!playfxontag <fx> [tag = j_head]
    Like !setfx, but effect is linked to the player's origin.

!setclantag <player> [tag]
    Set clan tag of player. Up to 7 characters.

!rotatescreen <player> <degree>
    Set camera roll of player. Very abusive.

*Misc comands (not enabled by default)*

!svpassword [password]
    Will set server password in “players2\server.cfg”, and kill server.
    *Server should be run under daemon*, i.e. “Alani’s server manager” to be auto-restarted.

```
**New features**
 - Spree messages (option: settings_enable_spree_messages)
 - Chat aliases (option: settings_enable_chat_alias)
 - AntiNoScope (option: settings_isnipe_antinoscope)
 - AntiCRTK (option: settings_isnipe_anticrtk)
 - AntiBoltCancel  (option: settings_isnipe_antiboltcancel)
 - XLR Stats (option: settings_enable_xlrstats)
 - Advanced cdvar manager - you can set default client dvars in "Utils\cdvars.txt"

 Fixed issue with trophy-kill (when victim gets banned for it);

 Fixed issue with !ac130 in isnipe mode (when issuer gets banned for it)
