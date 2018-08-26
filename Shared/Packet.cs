using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Shared
{
    public class Packet
    {
        private PacketTypes _type;
        private object _data;

        public Packet(PacketTypes type, object data)
        {
            _type = type;
            _data = data;
        }

        public Packet(Stream stream)
        {
            var formatter = new BinaryFormatter();

            // Не безопасно.
            _type = (PacketTypes)formatter.Deserialize(stream);
            _data = formatter.Deserialize(stream);
        }

        public PacketTypes Type
        {
            get
            {
                return _type;
            }
        }

        public object Data
        {
            get
            {
                return _data;
            }
        }

        public void CopyTo(Stream stream)
        {
            var formatter = new BinaryFormatter();

            formatter.Serialize(stream, _type);
            formatter.Serialize(stream, _data);
        }
    }
}
