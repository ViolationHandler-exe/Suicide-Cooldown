using System;
using Oxide.Core;
using Network;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Oxide.Plugins
{
    [Info("Suicide Cooldown", "ViolationHandler", "0.0.3")]
    [Description("Forces the suicide cooldown unless given bypass permissions.")]
    class SuicideCooldown : RustPlugin
    {
        private readonly object False = false;
        private Configuration config;
        
        private const string SuicideCooldownBypass = "suicidecooldown.bypass";

        private void Init()
        {
            permission.RegisterPermission(SuicideCooldownBypass, this);
            LoadConfig();
        }

        private void Loaded()
        {
            if (config.DefaultCooldown || (!config.DefaultCooldown && config.RespawnCooldown > 60)) return;
            foreach (BasePlayer player in BasePlayer.activePlayerList)
            {
                if (player.nextSuicideTime - UnityEngine.Time.realtimeSinceStartup > config.RespawnCooldown)
                {
                    player.nextSuicideTime = UnityEngine.Time.realtimeSinceStartup + config.RespawnCooldown;
                }
            }
        }

        private void Unload()
        {
            if (config.DefaultCooldown || (!config.DefaultCooldown && config.RespawnCooldown < 60)) return;
            foreach (BasePlayer player in BasePlayer.activePlayerList)
            {
                if (player.nextSuicideTime - UnityEngine.Time.realtimeSinceStartup > 60)
                {
                    player.nextSuicideTime = UnityEngine.Time.realtimeSinceStartup + 60;
                }
            }
        }

        private class Configuration
        {
            
            [JsonProperty("Default Respawn Cooldown")]
            public bool DefaultCooldown { get; set; } = true;
            
            [JsonProperty("Respawn Cooldown")]
            public int RespawnCooldown { get; set; } = 120;
            
            private string ToJson() => JsonConvert.SerializeObject(this);

            public Dictionary<string, object> ToDictionary() =>
                JsonConvert.DeserializeObject<Dictionary<string, object>>(ToJson());
        }

        protected override void LoadDefaultConfig() => config = new Configuration();

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                config = Config.ReadObject<Configuration>();
                if (config == null)
                {
                    throw new JsonException();
                }

                if (!config.ToDictionary().Keys.SequenceEqual(Config.ToDictionary(x => x.Key, x => x.Value).Keys))
                {
                    Puts("Configuration appears to be outdated; updating and saving.");
                    SaveConfig();
                }
            }
            catch
            {
                Puts($"Configuration file {Name}.json is invalid; using defaults");
                LoadDefaultConfig();
            }
        }
        
        protected override void SaveConfig()
        {
            Puts($"Configuration changes saved to {Name}.json");
            Config.WriteObject(config, true);
        }
        
        // Credit to WhiteThunder for original code snippet that this started as and then eventually modified and patched a little <3
        
        object OnClientCommand(Connection connection, string command)
        {
            // Fixes issues when users puts a space prior to sending kill.
            command = command.Trim();

            // Checks if first four letters are kill as long as the message itself is only 4 letters,
            // Otherwise it makes sure theres a space separating the kill from any other words, otherwise it will just return null.
            if ((command.Length == 4 && command.Substring(0, 4) != "kill") || command.Length < 4  || ((command.Length > 4 && command.Substring(0, 4) == "kill" && command[4] != ' ') ^ (command.Length > 4 && command.Substring(0, 4) != "kill")))
            {
                return null;
            }


            var player = connection.player as BasePlayer;
            if (player == null){
                return null;
            }

            // Can't kill yourself while already dead, so no point to return anything.
            if (player.IsDead())
            {
                player.ConsoleMessage("You can't suicide while dead.");
                return false;
            }

            if (player.IPlayer.HasPermission(SuicideCooldownBypass)) return null;
            
            
            if (UnityEngine.Time.realtimeSinceStartup > player.nextSuicideTime){
                NextTick(() =>
                {
                    if (!config.DefaultCooldown)
                    {
                        player.nextSuicideTime = UnityEngine.Time.realtimeSinceStartup + config.RespawnCooldown;
                    }
                });
                return null;
            }
            
            player.ConsoleMessage($"You can't suicide again so quickly, wait {player.nextSuicideTime-UnityEngine.Time.realtimeSinceStartup} seconds.");
            return False;
        }
    }
}
