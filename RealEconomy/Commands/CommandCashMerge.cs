using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System.Collections.Generic;
using fr34kyn01535.Uconomy;
using Rocket.API.Collections;
using UnityEngine;

namespace RealEconomy.Commands
{
    internal class CommandCashMerge : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "cashmerge";

        public string Help => "Merge your money into a bigger bill.";

        public string Syntax => "/merge [desire bill]";

        public List<string> Aliases => new List<string>() {"cmerge"};

        public List<string> Permissions => new List<string>() {"RealEconomy.cashmerge" };
        private RealEconomy pluginInstance => RealEconomy.Instance;
        private RealEconomyConfiguration pluginConfiguration => RealEconomy.Instance.Configuration.Instance;
        private TranslationList pluginTranslation => RealEconomy.Instance.Translations.Instance;
        private UconomyConfiguration uconomyConfiguration => Uconomy.Instance.Configuration.Instance;

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = caller as UnturnedPlayer;
            if (command.Length > 0)
            {
                long valueToMerge = 0;
                if (long.TryParse(command[0], out valueToMerge))
                {
                    if (valueToMerge < 0)
                    {
                        UnturnedChat.Say(caller, pluginInstance.Translate("command_cannot_use_negative_ammount"), Color.red); return;
                    }
                    if (pluginInstance.CashMerge(player, valueToMerge))
                    {
                        UnturnedChat.Say(player, pluginTranslation.Translate("command_cmerge_succes_value", valueToMerge, uconomyConfiguration.MoneyName), pluginInstance.messageColor);
                    }
                    else
                    {
                        UnturnedChat.Say(player, pluginTranslation.Translate("command_cmerge_fail_value", uconomyConfiguration.MoneyName, valueToMerge), Color.red);
                    }
                }
                else
                {
                    UnturnedChat.Say(player, Syntax, Color.red);
                }

            }
            else
            {
                if (pluginInstance.CashMerge(player, 0))
                {
                    UnturnedChat.Say(player, pluginTranslation.Translate("command_cmerge_succes_novalue", uconomyConfiguration.MoneyName), pluginInstance.messageColor);
                }
                else
                {
                    UnturnedChat.Say(player, pluginTranslation.Translate("command_cmerge_fail_novalue", uconomyConfiguration.MoneyName), Color.red);
                }
            }
        }
    }
}
