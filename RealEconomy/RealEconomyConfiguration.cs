using Rocket.API;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace RealEconomy
{
    
    public class RealEconomyConfiguration : IRocketPluginConfiguration
    {        
        [XmlArrayItem(ElementName = "MoneyType")]
        public List<KeyValuePair> IdAmountList {  get; set; }

        public bool AllowPayCommand {  get; set; }
        public bool AutoMerge { get; set; }

        public string MessageColor { get; set; }

        public void LoadDefaults()
        {
            AllowPayCommand = true;
            AutoMerge = true;
            MessageColor = "green";

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
        public ushort Key;
        [XmlAttribute("Value")]
        public long Value;
    }
}
