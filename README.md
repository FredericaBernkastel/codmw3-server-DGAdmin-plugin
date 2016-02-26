# codmw3-server-DGAdmin-plugin
DG Admin script for Call of Duty: MW3 dedicated server. Based on RGAdmin, modified by **F. Bernkastel**<br>
**New commands**
```Javascript
!apply - print apply message (apply.txt)
!night <on/off> - turn night mode for you
!sunlight <float RED> <float GREEN> <float BLUE> - set sunlight color multiplier
!cdvar <int/foat/string/direct> <key> <value> - set custom cdvar. In direct mode you can separate multiple values by comma.
!alias <player> [alias] - set chat alias *(leave [alias] field to reset it)*
!myalias [alias]
```
**New features**
 - Spree messages (option: settings_enable_spree_messages)
 - Chat aliases for player (option: settings_enable_chat_alias)
 - Advanced cdvar manager - you can set default client dvars in "Utils\cdvars.txt"
 Fixed issue with trophy-kill (when victim gets banned for it)
