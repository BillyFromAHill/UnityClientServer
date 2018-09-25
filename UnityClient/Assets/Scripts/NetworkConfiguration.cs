using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Assets.Scripts
{
    [Serializable]
    public class NetworkConfiguration
    {
        private NetworkConfiguration()
        {
        }

        public NetworkConfiguration(string fileName)
        {

            if (!File.Exists(fileName))
            {
                XmlSerializer xsSubmit = new XmlSerializer(typeof(NetworkConfiguration));

                using (var fileStream = File.Create(fileName))
                {
                    using (XmlWriter writer = XmlWriter.Create(fileStream, new XmlWriterSettings(){Indent = true}))
                    {
                        xsSubmit.Serialize(writer, Default);

                        this.Address = Default.Address;
                        this.Port = Default.Port;
                        this.ReconnectionTime = Default.ReconnectionTime;
                    }
                }
            }
            else
            {
                try
                {
                    XmlSerializer xsSubmit = new XmlSerializer(typeof(NetworkConfiguration));

                    using (var fileStream = File.Open(fileName, FileMode.Open))
                    {
                        using (XmlReader reader = XmlReader.Create(fileStream))
                        {
                            NetworkConfiguration configuration = (NetworkConfiguration)xsSubmit.Deserialize(reader);

                            this.Address = configuration.Address;
                            this.Port = configuration.Port;
                            this.ReconnectionTime = configuration.ReconnectionTime;
                        }
                    }
                }
                catch (XmlException e)
                {

                    Console.WriteLine(e);
                }
            }
        }

        public static NetworkConfiguration Default
        {
            get
            {
                return new NetworkConfiguration()
                {
                    Address = "127.0.0.1",
                    Port = 11500,
                    ReconnectionTime = 3000,
                };
            }
        }

        public string Address { get; private set; }

        public int Port { get; private set; }

        public int ReconnectionTime { get; private set; }
    }
}
