# Suicide-Cooldown
A plugin that allows server owners to prevent their staff from bypassing suicide cooldown, along with adding configurable suicide cooldowns. Working on a better unload and load process.

# Documentation
Just use the config to enable/disable the default respawn cooldown of 60 seconds, if you put ``false`` for the ``Default Suicide Cooldown``, then it will follow whatever cooldown value you put into ``Suicide Cooldown``. This can be however long you want, from 0-9999999. 

# Initalization
Upon loading the plugin, it will update the users cooldown if the default cooldown is disabled and the cooldown is less than 60 seconds (the default). This means if you have it set to 40 seconds, and the users cooldown is at 50, it will be set to 40 instead. However, if the cooldown happens to be 120 seconds, the plugin during loading will do nothing because I have no fair way of making a user who suicided prior to loading the plugin wait longer after already waiting a while.

# Unloading
Upon unloading this plugin, if the cooldown for the user is longer than the default cooldown, it will update their cooldown to the default cooldown. However, if your suicide cooldown is less than typical respawn times, it does nothing since I couldn't find a fair way to set it to 60 without knowing that they did or didn't just wait 50 seconds to try to suicide (ie, if the suicide cooldown happened to be 55), and then the plugin got unloaded and the user now has to wait another 60 seconds. 

# Config
(Default Config
```
{
  "Default Suicide Cooldown": true,
  "Suicide Cooldown": 60
}
```
