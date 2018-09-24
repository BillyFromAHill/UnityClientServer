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

            if (state.Data == null)
            {
                state.Data = new object();
            }
        }

        public Packet(Stream stream)
        {
            // Это бесполезно конвертировать в фабрику,
            // в которой можно использовать асинхронщину для
            // чтения из стрима, потому что в общей библиотеке имеем 3.5
            // фреймворк, поэтому будет некоторая копипаста на сервере.
            byte[] sizeBytes = new byte[sizeof(long)];

            stream.Read(sizeBytes, 0, sizeBytes.Length);
            long size = BitConverter.ToInt64(sizeBytes, 0);

            var formatter = new BinaryFormatter();

            if (size == 0)
            {
                return;
            }

            long readSize = 0;
            using (var memStream = new MemoryStream())
            {
                while (readSize < size)
                {
                    long readBufferSize = 1024;
                    if (size - readSize < readBufferSize)
                    {
                        readBufferSize = size - readSize;
                    }

                    var readBuffer = new byte[readBufferSize];
                    int currentRead = stream.Read(readBuffer, 0, readBuffer.Length);

                    memStream.Write(readBuffer, 0, currentRead);

                    readSize += currentRead;
                }

                memStream.Position = 0;
                state = (PacketState)formatter.Deserialize(memStream);
            }
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
            using (var memStream = new MemoryStream())
            {
                formatter.Serialize(memStream, state);
                stream.Write(BitConverter.GetBytes(memStream.Length), 0, sizeof(long));
                stream.Write(memStream.ToArray(), 0, (int)memStream.Length);

            }
        }
    }
}
