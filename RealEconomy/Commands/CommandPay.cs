using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System.Collections.Generic;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;
using fr34kyn01535.Uconomy;
using Rocket.API.Collections;

namespace RealEconomy.Commands
{
    public class CommandPay : IRocketCommand 
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "pay";

        public string Help => "Send players money.";

        public string Syntax => "/pay <player> <amount>";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "RealEconomy.pay"};
        private RealEconomy pluginInstance => RealEconomy.Instance;
        private RealEconomyConfiguration pluginConfiguration => RealEconomy.Instance.Configuration.Instance;
        private TranslationList uconomyTranslation => Uconomy.Instance.Translations.Instance;
        private UconomyConfiguration uconomyConfiguration => Uconomy.Instance.Configuration.Instance;

        public void Execute(IRocketPlayer caller, params string[] command) 
        {
            if (command.Length != 2)
            { UnturnedChat.Say(caller, Syntax, Color.red); return; } 
            UnturnedPlayer otherPlayer = UnturnedPlayer.FromName(command[0]);
            if (otherPlayer != null) 
            {               
                long amount = 0;
                
                if (!long.TryParse(command[1], out amount) || amount < pluginInstance.minValue)
                {
                    UnturnedChat.Say(caller, uconomyTranslation.Translate("command_pay_error_invalid_amount"), Color.red);
                    return;
                }
                var player = caller as UnturnedPlayer;
                if (caller is ConsolePlayer)
                {
                    pluginInstance.ConsoleSendMoney(otherPlayer, amount);
                    UnturnedChat.Say(otherPlayer.CSteamID, uconomyTranslation.Translate("command_pay_console", amount, uconomyConfiguration.MoneyName), pluginInstance.messageColor);
                    Logger.Log(string.Format("Succesfully payed {0} {1} to {2}!", command[1], uconomyConfiguration.MoneyName, command[0]));
                    return;
                }
                
     
                if (player.CSteamID.m_SteamID == otherPlayer.CSteamID.m_SteamID) { UnturnedChat.Say(caller, uconomyTranslation.Translate("command_pay_error_pay_self"), Color.red);
                    return;
                }            
                else
                {                   
                    if (pluginConfiguration.AllowPayCommand || caller.IsAdmin || caller.HasPermission("RealEconomy.adminpay")) 
                    {                      
                            if (amount < 0)
                            {
                                UnturnedChat.Say(caller, uconomyTranslation.Translate("command_cannot_use_negative_ammount"),Color.red); return;
                            }
                            if (pluginInstance.CheckBalance(player) - amount < 0)
                            {
                                UnturnedChat.Say(caller, uconomyTranslation.Translate("command_pay_error_cant_afford"), Color.red); return;
                            }
                            pluginInstance.PlayerSendMoney(player, otherPlayer, amount);
                            UnturnedChat.Say(caller, uconomyTranslation.Translate("command_pay_private", otherPlayer.CharacterName, amount, pluginInstance.messageColor));
                            UnturnedChat.Say(otherPlayer.CSteamID, uconomyTranslation.Translate("command_pay_other_private", amount, pluginInstance.messageColor));
                            return;   
                    }                   
                }
            } 
            else 
            { 
                UnturnedChat.Say(caller, uconomyTranslation.Translate("command_pay_error_player_not_found"), Color.red); return; 
            }
            
        }
    }
}
