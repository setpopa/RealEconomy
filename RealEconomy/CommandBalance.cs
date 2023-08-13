using fr34kyn01535.Uconomy;
using HarmonyLib;
using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RealEconomy
{
    internal class CommandBalance : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "balance";

        public string Help => "Show your current balance.";

        public string Syntax => "/balance";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "RealEconomy.balance" };

        public void Execute(IRocketPlayer caller, params string[] command)
        {
            int balance = RealEconomy.Instance.CheckBalance((UnturnedPlayer)caller);
            UnturnedChat.Say(caller, Uconomy.Instance.Translations.Instance
                .Translate("command_balance_show", balance, Uconomy.Instance.Configuration.Instance.MoneyName));
            return;
        }
    }
}
