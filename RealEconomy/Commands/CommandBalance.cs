using fr34kyn01535.Uconomy;
using Rocket.API;
using Rocket.API.Collections;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System.Collections.Generic;


namespace RealEconomy.Commands
{
    internal class CommandBalance : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "balance";

        public string Help => "Show your current balance.";

        public string Syntax => "/balance";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "RealEconomy.balance" };
        private RealEconomy pluginInstance => RealEconomy.Instance;
        private RealEconomy pluginConfiguration => RealEconomy.Instance;
        private TranslationList pluginTranslation => RealEconomy.Instance.Translations.Instance;
        private UconomyConfiguration uconomyConfiguration => Uconomy.Instance.Configuration.Instance;

        public void Execute(IRocketPlayer caller, params string[] command)
        {
            long balance = pluginConfiguration.CheckBalance((UnturnedPlayer)caller);
            UnturnedChat.Say(caller, pluginTranslation.Translate("command_balance_show", balance, uconomyConfiguration.MoneyName), pluginInstance.messageColor);
            return;
        }
    }
}
