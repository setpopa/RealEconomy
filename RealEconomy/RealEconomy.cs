using Rocket.Core.Plugins;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Logger = Rocket.Core.Logging.Logger;
using Rocket.API.Collections;
using System.Xml.Linq;
using fr34kyn01535.Uconomy;
using HarmonyLib;
using RealEconomy.DBs;
using RealEconomy.HelpMethods;
using RealEconomy.Pathes;
using Rocket.Unturned.Chat;

namespace RealEconomy
{
    public class RealEconomy : RocketPlugin<RealEconomyConfiguration>
    {
        
        public static RealEconomy Instance;

        private Harmony _harmony;

        private DB _db;
        private UconomyPatches _patches;

        private delegate bool ConsoleSendedMoneyHandler(UnturnedPlayer player, long amount);
        private ConsoleSendedMoneyHandler OnMoneySend;
        internal Dictionary<ushort, long> _idAmount = new Dictionary<ushort, long>();
        internal long minValue = 0;
        private ushort lowestIdMoney = 0;
        private ushort highestIdMoney = 0;
        internal Color messageColor;
        protected override void Load()
        {
            
            if (_harmony == null)
            {
                _harmony = new Harmony("Uconomy.RealUconomy");
                _harmony.PatchAll();
            }
            Instance = this;
            _db = new DB();
            _patches = new UconomyPatches();
            if(Configuration.Instance.IdAmountList != null)
            {
                foreach (var keyValue in Configuration.Instance.IdAmountList)
                {
                    _idAmount.Add(keyValue.Key, keyValue.Value);
                }
                minValue = _idAmount.Min(x => x.Value);
                _idAmount = _idAmount.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, y => y.Value);
                lowestIdMoney = _idAmount.OrderBy(x => x.Value).FirstOrDefault().Key;
                highestIdMoney = _idAmount.OrderByDescending(x => x.Value).FirstOrDefault().Key;
            }            
            else
            {
                Logger.LogError("ERROR! Config has no values!!!");
                Instance.UnloadPlugin();
                return; //just in case
            }
            if (Configuration.Instance.AutoMerge)
            {
                OnMoneySend += CashMerge;
            }
            messageColor = UnturnedChat.GetColorFromName(Configuration.Instance.MessageColor, Color.green);
            Logger.Log($"Plugin {Assembly.FullName} loaded succesfully!");
            
        }

        protected override void Unload()
        {
            Instance = null;
            _idAmount = null;

            Logger.Log($"Plugin {Assembly.FullName} unloaded!");
        }

        public override TranslationList DefaultTranslations => new TranslationList()
        {
            {"command_balance_show","Your current balance is {0} {1} in your pockets." },
            {"command_cchange_succes_value", "You changed your {0} {1} for change." },
            {"command_cchange_succes_novalue", "You changed your {0} for change." },
            {"command_cchange_fail_novalue", "You dont have any {0}." },
            {"command_cchange_fail_value", "You dont have any {0} in value {1}." },
            {"command_cmerge_succes_value", "You merged your {0} {1}." },
            {"command_cmerge_succes_novalue", "You merger your {0}." },
            {"command_cmerge_fail_novalue", "You dont have any {0} to merge." },
            {"command_cmerge_fail_value", "You cant get {0} your {1} dont add up." },
            {"command_cannot_use_negative_ammount", "You cannot use negative ammount!" },
            {"first_connect_bonus", "You recieved {0} {1} for first connection! Check your inventory becouse your currancy is real!" },
        };
        
       
        private bool UserPlayedBefore(UnturnedPlayer unturnedPlayer)
        {
            return _db.db.Element("Users").Elements("User").Any(x => x.Value == unturnedPlayer.CSteamID.ToString());
        }
        internal void OnPlayerConnected(UnturnedPlayer player) 
        {
            if (!UserPlayedBefore(player))
            {
                ConsoleSendMoney(player, (long)Uconomy.Instance.Configuration.Instance.InitialBalance); // read config by xdocument
                UnturnedChat.Say(player, Translations.Instance.Translate("first_connect_bonus", Uconomy.Instance.Configuration.Instance.InitialBalance, Uconomy.Instance.Configuration.Instance.MoneyName));
                _db.db.Element("Users").Add(new XElement("User", player.CSteamID));
                _db.db.Save(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Plugins", "RealEconomy", "db.xml"));
                _db.db = XDocument.Load(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Plugins", "RealEconomy", "db.xml"));
            }           
        }
        internal void PlayerSendMoney(UnturnedPlayer sender, UnturnedPlayer receiver, long amount)
        {
            var senderInventory = sender.Inventory;
            long substractAmount = amount;
            var idAmountCopy = _idAmount.ToDictionary(entry => entry.Key, entry => entry.Value);


            List<InventorySearch> search = new List<InventorySearch>();
            while (substractAmount > 0) //remove the items
            {
                ushort id = 0;
                while (true)
                {
                    long currMinValue = idAmountCopy.Min(x => x.Value);
                    id = idAmountCopy.FirstOrDefault(x => x.Value == currMinValue).Key;
                    search = senderInventory.search(id, false, true);
                    if (search.Count == 0)
                    {
                        idAmountCopy.Remove(id);
                    }
                    else
                    {
                        break;
                    }
                }
                substractAmount -= idAmountCopy[id];
                senderInventory.removeItem(search[0].page, senderInventory.getIndex(search[0].page, search[0].jar.x, search[0].jar.y));
                search.Clear();
            }
            ConsoleSendMoney(sender, Math.Abs(substractAmount)); //give the sender the change
            ConsoleSendMoney(receiver, amount); // give the reciever the money
        }
        internal void ConsoleSendMoney(UnturnedPlayer receiver, long amount)
        {
            while (amount >= minValue)
            {
                long maxValue = _idAmount.Where(x => x.Value <= amount).Max(x => x.Value);
                ushort id = _idAmount.FirstOrDefault(x => x.Value == maxValue).Key;
                if (id == 0) { break; }
                if (amount - _idAmount[id] < 0) { break; }
                amount -= (int)_idAmount[id];
                receiver.GiveItem(id, 1);
            }
            try { OnMoneySend(receiver, 0); } catch { }
            
        }
        internal void SendMoney(UnturnedPlayer receiver, long amount) //TODO find better solution to prevent loop
        {
            while (amount >= minValue)
            {
                long maxValue = _idAmount.Where(x => x.Value <= amount).Max(x => x.Value);
                ushort id = _idAmount.FirstOrDefault(x => x.Value == maxValue).Key;
                if (id == 0) { break; }
                if (amount - _idAmount[id] < 0) { break; }
                amount -= (int)_idAmount[id];
                receiver.GiveItem(id, 1);
            }
        }
        internal void UpdateMoney(UnturnedPlayer player, long amount) //to change player cash in inventory
        {
            if (amount > 0)
            {
                ConsoleSendMoney(player, amount);
            }
            else if (amount < 0)
            {
                var senderInventory = player.Inventory;
                long substractAmount = Math.Abs(amount);
                var idAmountCopy = _idAmount.ToDictionary(entry => entry.Key, entry => entry.Value);
                List<InventorySearch> search = new List<InventorySearch>();
                while (substractAmount > 0) 
                {
                    ushort id = 0;
                    while (true)
                    {

                        long minValue = idAmountCopy.Min(x => x.Value);
                        id = idAmountCopy.FirstOrDefault(x => x.Value == minValue).Key;
                        search = senderInventory.search(id, false, true);
                        if (search.Count == 0)
                        {
                            idAmountCopy.Remove(id);
                        }
                        else
                        {
                            break;
                        }
                    }
                    substractAmount -= idAmountCopy[id];
                    senderInventory.removeItem(search[0].page, senderInventory.getIndex(search[0].page, search[0].jar.x, search[0].jar.y));
                    search.Clear();
                }
                ConsoleSendMoney(player, Math.Abs(substractAmount));
            }
        }
        internal long CheckBalance(UnturnedPlayer player)
        {
            long preSum = 0;
            long sum = 0;
            for (byte num = 0; num < PlayerInventory.PAGES - 2; num++)
            {
                for (byte num2 = 0; num2 < player.Inventory.getItemCount(num); num2++)
                {
                    var item = player.Inventory.getItem(num, num2);
                    if (item == null)
                        continue;

                    _idAmount.TryGetValue(item.item.id, out preSum);
                    sum += preSum;
                }
            }
            return sum;
        }

        internal bool CashChange(UnturnedPlayer player, long valueToChange) //change the lowest cash or change the given value                               
        {
            var idAmountCopy = _idAmount.ToDictionary(entry => entry.Key, entry => entry.Value);

            idAmountCopy.Remove(lowestIdMoney);

            InventorySearch inventorySearch = null;
            var playerInventory = player.Inventory; //gets players inventory

            List<InventorySearch> inventorySearches = new List<InventorySearch>();
            List<long> valuesInInventory = new List<long>();
            foreach (var itemId in idAmountCopy.Keys) //add all possible cash items in the serches list
            {
                try
                {
                    inventorySearch = playerInventory.search(itemId, false, true)[0];
                }
                catch { continue; }
                inventorySearches.Add(inventorySearch);
            }
            if (inventorySearches != null)
            {
                foreach (var item in inventorySearches) //aquire cash value
                {
                    valuesInInventory.Add(idAmountCopy[item.jar.item.id]);
                }
                if (valuesInInventory.Any()) //determin if he doesnt have alredy the lowest cash // TODO OPTIMAZE SIMILARY TO MERGE!!! //update looks good :D //future plan give the item to the slot where it was taken
                {                   
                    if (valueToChange == 0) //check if there is specific value to change                       
                    {
                        var item = inventorySearches.FirstOrDefault(x => x.jar.item.id == _idAmount.FirstOrDefault(y => y.Value == valuesInInventory.Min()).Key); //get the chash item to change based on lowest changeble item
                        playerInventory.removeItem(item.page, playerInventory.getIndex(item.page, item.jar.x, item.jar.y)); //removes the cash item
                        valueToChange = valuesInInventory.Min(); //aquire the amount to change                      
                        var idAmountLessThenValueToChange = _idAmount.Where(x => x.Value < valueToChange).OrderByDescending(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value); //copy the dictionary to get all ids lower than value to change
                        long amountGiven = 0; //optimalized?
                        do
                        {
                            var givenIdAmount = idAmountLessThenValueToChange.FirstOrDefault(pair => pair.Value + amountGiven <= valueToChange);
                            if (givenIdAmount.Value == 0) { break; }
                            player.GiveItem(givenIdAmount.Key, 1);
                            amountGiven += givenIdAmount.Value;
                        } while (amountGiven < valueToChange);

                    }
                    else //specific value to change
                    {
                        var item = inventorySearches.FirstOrDefault(x => x.jar.item.id == _idAmount.FirstOrDefault(y => y.Value == valueToChange).Key); //get item to change based on imput value
                        if (item == null) { return false; }
                        playerInventory.removeItem(item.page, playerInventory.getIndex(item.page, item.jar.x, item.jar.y));
                        var idAmountLessThenValueToChange = idAmountCopy.Where(x => x.Value < valueToChange).OrderByDescending(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value); //copy the dictionary to get all ids lower than value to change
                        long amountGiven = 0; //optimalized?
                        do
                        {
                            var givenIdAmount = idAmountLessThenValueToChange.FirstOrDefault(pair => pair.Value + amountGiven <= valueToChange);
                            if (givenIdAmount.Value == 0) { break; }
                            player.GiveItem(givenIdAmount.Key, 1);
                            amountGiven += givenIdAmount.Value;
                        } while (amountGiven < valueToChange);

                    }
                    return true; //the change was succesfull
                }
            }           
            return false; //the change failed alredy lowest            
        }      
        internal bool CashMerge(UnturnedPlayer player, long valueToMerge)
        {
            var idAmountCopy = _idAmount.ToDictionary(entry => entry.Key, entry => entry.Value);
            idAmountCopy.Remove(highestIdMoney);

            List<InventorySearch> inventorySearch = new List<InventorySearch>();
            var playerInventory = player.Inventory; //gets players inventory

            List<List<InventorySearch>> inventorySearches = new List<List<InventorySearch>>();
            List<long> valuesInInventory = new List<long>();
            foreach (var itemId in idAmountCopy.Keys) //add all possible cash items in the serches list
            {
                try
                {
                    inventorySearch = playerInventory.search(itemId, false, true);
                }
                catch { continue; }
                inventorySearches.Add(inventorySearch);
            }
            if (inventorySearches.Any()) //future plan give the item to the slot where it was taken
            {
                if (valueToMerge == 0) // all cash is merged //TODO check if he already dont have highest values
                {
                    foreach (var moneyItemList in inventorySearches)
                    { 
                        foreach(var moneyItem in moneyItemList)
                        {
                            playerInventory.removeItem(moneyItem.page, playerInventory.getIndex(moneyItem.page, moneyItem.jar.x, moneyItem.jar.y));
                            valueToMerge += _idAmount[moneyItem.jar.item.id];
                        }                       
                    }
                    SendMoney(player, valueToMerge); 
                }
                else  //specific amount
                {
                    
                    List<InventorySearch> moneyItemsUnderValueToMerge = new List<InventorySearch>();
                    foreach( var moneyItemList in inventorySearches) //aquire all items in inventory under certen value from the lists takened before
                    {
                        List<InventorySearch> temporaryListOfItemsUnderValueToMerge = moneyItemList.Where(x => _idAmount[x.jar.item.id] < valueToMerge).ToList(); //filter items base on their value
                        
                        if (temporaryListOfItemsUnderValueToMerge.Any()) //check if any item fitting the filter was found
                        {
                            foreach (var cashItem in temporaryListOfItemsUnderValueToMerge) //adds all items in
                            {
                                moneyItemsUnderValueToMerge.Add(cashItem);
                            }                           
                        }                       
                    }
                    List<InventorySearch> cashItemsToBeRemoved = new List<InventorySearch>();
                    cashItemsToBeRemoved = HelpMethod.FindSubsetWithTargetSum(moneyItemsUnderValueToMerge, valueToMerge); //find the items to remove
                    if (cashItemsToBeRemoved == null) // check if mergeable
                    {
                        return false;
                    }
                    else
                    {
                        foreach(var cashItemToRemove in cashItemsToBeRemoved)
                        {
                            playerInventory.removeItem(cashItemToRemove.page, playerInventory.getIndex(cashItemToRemove.page, cashItemToRemove.jar.x, cashItemToRemove.jar.y));
                        }
                        SendMoney(player, valueToMerge);
                    }                  
                }
                return true;
            }
            else 
            {
                return false;
            }
        }

        
    }   
}
