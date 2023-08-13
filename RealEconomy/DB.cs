using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace RealEconomy
{
    public class DB
    {
        internal XDocument db;
        public DB()
        {
            if (!File.Exists(Directory.GetCurrentDirectory() + "\\Plugins\\RealEconomy\\db.xml"))
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.OmitXmlDeclaration = true;
                XmlWriter xw = XmlWriter.Create(Directory.GetCurrentDirectory() + "\\Plugins\\RealEconomy\\db.xml",settings);
                xw.WriteStartElement("Users");
                xw.WriteStartElement("User");
                xw.WriteString("76561198125822773");
                xw.WriteEndElement();
                xw.WriteEndElement();
                xw.Close();
            }
            db = XDocument.Load(Directory.GetCurrentDirectory() + "\\Plugins\\RealEconomy\\db.xml");
        }
    }
}
