using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using HarmonyLib;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using fr34kyn01535.Uconomy;

namespace RealEconomy
{
    public class CommandPay : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "pay";

        public string Help => "Send players money.";

        public string Syntax => "/pay <player> <amount>";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "RealEconomy.pay"};

        public void Execute(IRocketPlayer caller, params string[] command) // harmony patch
        {
            if (command.Length != 2) { UnturnedChat.Say(caller, 
                Uconomy.Instance.Translations.Instance.Translate("command_pay_invalid")); return; } // custom translations
            UnturnedPlayer otherPlayer = UnturnedPlayer.FromName(command[0]);
            if (otherPlayer != null) 
            {
                if (caller == otherPlayer) { UnturnedChat.Say(caller, 
                    Uconomy.Instance.Translations.Instance.Translate("command_pay_error_pay_self")); return; 
                }
                int amount = 0;
                if (!int.TryParse(command[1], out amount) || amount <= RealEconomy.Instance.minValue) { Uconomy.Instance.Translations // custom translations
                        .Instance.Translate("command_pay_error_invalid_amount"); return; 
                }
                
                if (caller is ConsolePlayer) {
                    RealEconomy.Instance.ConsoleSendMoney(otherPlayer, amount);
                    UnturnedChat.Say(otherPlayer.CSteamID, Uconomy.Instance.Translations.Instance
                        .Translate("command_pay_console", amount, Uconomy.Instance.Configuration.Instance.MoneyName)); // custom translations and config reader by xdocument
                    return;
                } 
                else 
                {
                    if (RealEconomy.Instance.CheckBalance((UnturnedPlayer)caller) - amount < 0)
                    {
                        UnturnedChat.Say(caller, Uconomy.Instance.Translate("command_pay_error_cant_afford")); return;
                    }
                    RealEconomy.Instance.PlayerSendMoney((UnturnedPlayer)caller,otherPlayer,amount);
                    UnturnedChat.Say(caller, Uconomy.Instance.Translations.Instance
                        .Translate("command_pay_private", otherPlayer.CharacterName, amount, Uconomy.Instance.Configuration.Instance.MoneyName)); // custom translations and config reader by xdocument
                    UnturnedChat.Say(otherPlayer.CSteamID, Uconomy.Instance.Translations.Instance
                        .Translate("command_pay_other_private", amount, Uconomy.Instance.Configuration.Instance.MoneyName, caller.DisplayName));
                    return;
                }
            } 
            else 
            { UnturnedChat.Say(caller, Uconomy.Instance.Translations.Instance  // custom translations
                .Translate("command_pay_error_player_not_found")); return; 
            }
            
        }
    }
}
