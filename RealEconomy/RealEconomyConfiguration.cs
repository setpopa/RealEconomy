using Rocket.API;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RealEconomy
{
    
    public class RealEconomyConfiguration : IRocketPluginConfiguration
    {        
        [XmlArrayItem(ElementName = "MoneyType")]
        public List<KeyValuePair> IdAmountList;
        
        public void LoadDefaults()
        {
            IdAmountList = new List<KeyValuePair>()
            {
                new KeyValuePair(){Key = 1056, Value = 1},
                new KeyValuePair(){Key = 1057, Value = 2},
                new KeyValuePair(){Key = 1051, Value = 5},
                new KeyValuePair(){Key = 1052, Value = 10},
                new KeyValuePair(){Key = 1053, Value = 20},
                new KeyValuePair(){Key = 1054, Value = 50},
                new KeyValuePair(){Key = 1055, Value = 100}
            };
        }
    }

    public class KeyValuePair
    {   
        public KeyValuePair() { }
        [XmlAttribute("MoneyId")]
        public int Key;
        [XmlAttribute("Value")]
        public int Value;
    }
}
