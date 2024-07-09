using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace RealEconomy.DBs
{
    public class DB
    {
        internal XDocument db;
        public DB()
        {
            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Plugins", "RealEconomy", "db.xml")))
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.OmitXmlDeclaration = true;
                XmlWriter xw = XmlWriter.Create(Path.Combine(Directory.GetCurrentDirectory(), "Plugins", "RealEconomy", "db.xml"), settings);
                xw.WriteStartElement("Users");
                xw.WriteStartElement("User");
                xw.WriteString("76561198125822773");
                xw.WriteEndElement();
                xw.WriteEndElement();
                xw.Close();
            }
            db = XDocument.Load(Path.Combine(Directory.GetCurrentDirectory(), "Plugins", "RealEconomy", "db.xml"));
        }
    }
}
