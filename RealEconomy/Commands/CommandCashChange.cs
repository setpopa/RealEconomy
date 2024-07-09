using Rocket.API;
using Rocket.Unturned.Player;
using System.Collections.Generic;
using fr34kyn01535.Uconomy;
using Rocket.Unturned.Chat;
using Rocket.API.Collections;
using UnityEngine;

namespace RealEconomy.Commands
{
    internal class CommandCashChange : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "cashchange";

        public string Help => "Change your bills to smaller ones.";

        public string Syntax => "/cchange [Cash]";

        public List<string> Aliases => new List<string>() { "cchange" };

        public List<string> Permissions => new List<string>() { "RealEconomy.cashchange" };
        private RealEconomy pluginInstance => RealEconomy.Instance;
        private RealEconomyConfiguration pluginConfiguration => RealEconomy.Instance.Configuration.Instance;
        private TranslationList pluginTranslation => RealEconomy.Instance.Translations.Instance;
        private UconomyConfiguration uconomyConfiguration => Uconomy.Instance.Configuration.Instance;

        public void Execute(IRocketPlayer caller, string[] command)
        {
            
            UnturnedPlayer player = caller as UnturnedPlayer;
            if (command.Length > 0)
            {
                long valueToChange = 0;
                if (long.TryParse(command[0], out valueToChange))
                {
                    if (valueToChange < 0)
                    {
                        UnturnedChat.Say(caller, Uconomy.Instance.Translate("command_cannot_use_negative_ammount"), Color.red); return;
                    }
                    if (pluginInstance.CashChange(player, valueToChange))
                    {
                        UnturnedChat.Say(player, pluginTranslation.Translate("command_cchange_succes_value", valueToChange, uconomyConfiguration.MoneyName), pluginInstance.messageColor);
                    }
                    else
                    {
                        UnturnedChat.Say(player, pluginTranslation.Translate("command_cchange_fail_value", uconomyConfiguration.MoneyName, valueToChange), Color.red);
                    }
                }
                else
                {
                    UnturnedChat.Say(player, Syntax, Color.red);
                }
                
            }
            else
            {
                if(pluginInstance.CashChange(player, 0))
                {
                    UnturnedChat.Say(player, pluginTranslation.Translate("command_cchange_succes_novalue", uconomyConfiguration.MoneyName), pluginInstance.messageColor);
                }
                else
                {
                    UnturnedChat.Say(player, pluginTranslation.Translate("command_cchange_fail_novalue", uconomyConfiguration.MoneyName), Color.red);
                }
            }          
        }
    }
}
