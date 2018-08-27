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
        [Serializable]
        private class PacketState
        {
            public PacketTypes Type { get; set; }

            public object Data { get; set; }
        }

        private PacketState state = new PacketState();

        public Packet(PacketTypes type, object data)
        {
            state.Type = type;
            state.Data = data;
        }

        public Packet(Stream stream)
        {
            var formatter = new BinaryFormatter();

            // Не безопасно.
            state = (PacketState)formatter.Deserialize(stream);
        }

        public PacketTypes Type
        {
            get
            {
                return state.Type;
            }
        }

        public object Data
        {
            get
            {
                return state.Data;
            }
        }

        public void CopyTo(Stream stream)
        {
            var formatter = new BinaryFormatter();

            formatter.Serialize(stream, state);
        }
    }
}
