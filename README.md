## "DGAdmin" - Call of Duty: MW3 dedicated server plugin
DG Admin script for Call of Duty: MW3 dedicated server. Based on RGAdmin, modified by **F. Bernkastel**<br>
**New commands**
```Javascript
!apply
    print apply message (commands\apply.txt)
    
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
    
!report <message>
    Player can report hackers. Message will be sent to all online admins, and saved to history.
    
!lastreports [amount]
    Admins can access to !report history. Amount may be 1 – 8, default is 4.
    
!unban <player>
    Unban single player by name.
    
!unban-id <ban id>
    Deletes given banentry from banlist.
    You can get banentries by doing !lastbans or !searchbans.

```
**New features**
 - Spree messages (option: settings_enable_spree_messages)
 - Chat aliases (option: settings_enable_chat_alias)
 - Advanced cdvar manager - you can set default client dvars in "Utils\cdvars.txt"

 Fixed issue with trophy-kill (when victim gets banned for it)
