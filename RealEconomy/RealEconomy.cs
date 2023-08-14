using Rocket.Core.Plugins;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rocket.Unturned.Player;
using Rocket.Unturned;
using Rocket.Unturned.Enumerations;
using SDG.Unturned;
using Logger = Rocket.Core.Logging.Logger;
using Rocket.Unturned.Chat;
using Rocket.API.Collections;
using System.Security.Policy;
using System.Xml.Linq;
using Pathfinding;
using fr34kyn01535.Uconomy;
using HarmonyLib;
using Rocket.Core.Serialization;

namespace RealEconomy
{
    public class RealEconomy : RocketPlugin<RealEconomyConfiguration>
    {
        public static RealEconomy Instance;

        private Harmony _harmony;

        private DB _db;
        private Patches _patches;
        
        public int minValue = int.MaxValue;
        private Dictionary<int, int> _idAmount = new Dictionary<int, int>();

        protected override void Load()
        {
            if (_harmony == null)
            {
                _harmony = new Harmony("Uconomy.RealUconomy");
                _harmony.PatchAll();
            }

            Instance = this;

            _db = new DB();
            _patches = new Patches();

            foreach (var keyValue in Configuration.Instance.IdAmountList)
            {
                _idAmount.Add(keyValue.Key, keyValue.Value);
                if (minValue > keyValue.Value)
                {
                    minValue = keyValue.Value;
                }
            }      
            Logger.Log($"Plugin {Assembly.FullName} loaded succesfully!");
        }

        protected override void Unload()
        {
            Instance = null;
            _idAmount = null;            

            Logger.Log($"Plugin {Assembly.FullName} unloaded!");
        }
     

        public bool UserPlayedBefore(UnturnedPlayer unturnedPlayer)
        {
            return _db.db.Element("Users").Elements("User").Any(x => x.Value == unturnedPlayer.CSteamID.ToString());
        }
        internal void OnPlayerConnected(UnturnedPlayer player)
        {
            if (!UserPlayedBefore(player))
            {
                ConsoleSendMoney(player, (int)Uconomy.Instance.Configuration.Instance.InitialBalance); // read config by xdocument
                _db.db.Element("Users").Add(new XElement("User", player.CSteamID));
                _db.db.Save(System.IO.Directory.GetCurrentDirectory() + "\\Plugins\\RealEconomy\\db.xml");
                _db.db = XDocument.Load(System.IO.Directory.GetCurrentDirectory() + "\\Plugins\\RealEconomy\\db.xml");
            }            
        }       
        public void PlayerSendMoney(UnturnedPlayer sender, UnturnedPlayer receiver, int amount)
        {
            var senderInventory = sender.Inventory;
            int substractAmount = amount;
            var idAmountCopy = _idAmount.ToDictionary(entry => entry.Key, entry => entry.Value);


            List<InventorySearch> search = new List<InventorySearch>();
            while (substractAmount > 0) 
            {
                int id = 0;
                while (true)
                {
                    int minValue = idAmountCopy.Min(x => x.Value);
                    id = idAmountCopy.FirstOrDefault(x => x.Value == minValue).Key;
                    search = senderInventory.search((ushort)id, false, true);
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
            ConsoleSendMoney(sender, Math.Abs(substractAmount));          
            while (amount >= minValue) 
            {
                int maxValue = _idAmount.Where(x => x.Value <= amount).Max(x => x.Value);
                int id = _idAmount.FirstOrDefault(x => x.Value == maxValue).Key;               
                if(id == 0) { break; }
                amount -= _idAmount[id];
                receiver.GiveItem((ushort)id,1);
            }           
        }
        public void ConsoleSendMoney(UnturnedPlayer receiver, int amount) 
        {
            while (amount >= minValue) 
            {
                int maxValue = _idAmount.Where(x => x.Value <= amount).Max(x => x.Value);
                int id = _idAmount.FirstOrDefault(x => x.Value == maxValue).Key;
                if (id == 0) { break; }
                if (amount - _idAmount[id] < 0) { break; }
                amount -= _idAmount[id];
                receiver.GiveItem((ushort)id, 1);
            }
        }
        public void UpdateMoney(UnturnedPlayer player, int amount) //something wrong i can feel it
        {
            if (amount > 0)
            {
                ConsoleSendMoney(player, amount);
            }
            else if (amount < 0)
            {
                var senderInventory = player.Inventory;
                int substractAmount = Math.Abs(amount);
                var idAmountCopy = _idAmount.ToDictionary(entry => entry.Key, entry => entry.Value);
                List<InventorySearch> search = new List<InventorySearch>();
                while (substractAmount > 0)
                {
                    int id = 0;
                    while (true)
                    {

                        int minValue = idAmountCopy.Min(x => x.Value);
                        id = idAmountCopy.FirstOrDefault(x => x.Value == minValue).Key;
                        search = senderInventory.search((ushort)id, false, true);
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
        public int CheckBalance(UnturnedPlayer player)
        {
            int preSum = 0;
            int sum = 0;
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
    }
}
