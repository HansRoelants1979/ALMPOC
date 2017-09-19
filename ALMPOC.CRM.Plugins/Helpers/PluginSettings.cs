using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml;

namespace ALMPOC.CRM.Plugins.Helpers
{
    public class PluginSettings
    {
        Hashtable _secureSettings = null;
        Hashtable _unsecureSettings = null;

        public PluginSettings(string config, string secureConfig)
        {
            this.UnSecureConfig = (!string.IsNullOrEmpty(config) ? ParsePluginSettings(config) : null);
            this.SecureConfig = (!string.IsNullOrEmpty(secureConfig) ? ParsePluginSettings(secureConfig) : null);
        }

        public Hashtable SecureConfig
        {
            get { return _secureSettings; }
            set { _secureSettings = value; }
        }
        public Hashtable UnSecureConfig
        {
            get { return _unsecureSettings; }
            set { _unsecureSettings = value; }
        }

        private Hashtable ParsePluginSettings(string xml)
        {
            XmlDocument xmldoc = new XmlDocument();
            XmlElement root = null;
            XmlNodeList nodeList = null;
            Hashtable hash = new Hashtable();
            xmldoc.LoadXml(xml);
            root = xmldoc.DocumentElement;
            nodeList = root.SelectNodes("//setting");
            foreach (XmlNode node in nodeList)
                hash.Add(node.Attributes["name"].Value, node.Attributes["value"].Value);
            return hash;
        }
    }
}
