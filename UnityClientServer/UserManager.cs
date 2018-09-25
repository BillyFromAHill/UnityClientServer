using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Shared;

namespace UnityClientServer
{
    class UserManager
    {
        private World _userWorld;

        private Socket _socket;

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public UserManager(Socket socket)
        {
            _socket = socket;
            _userWorld = new World(_cancellationTokenSource.Token);
            Task.Factory.StartNew(() => UserInteractor(_cancellationTokenSource.Token), TaskCreationOptions.LongRunning);
        }

        private async void UserInteractor(CancellationToken token)
        {
            using (NetworkStream stream = new NetworkStream(_socket))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {

                    while (_socket.Connected)
                    {
                        try
                        {
                            byte[] sizeBytes = new byte[sizeof(long)];

                            // Не помешала бы проверка, что сокет еще живой из другого потока.
                            int readBytesCount = await stream.ReadAsync(sizeBytes, 0, sizeBytes.Length, token);
                            if (readBytesCount == 0)
                            {
                                break;
                            }

                            long size = BitConverter.ToInt64(sizeBytes, 0);

                            long readSize = 0;

                            memoryStream.Write(sizeBytes, 0, sizeBytes.Length);
                            while (readSize < size)
                            {
                                long readBufferSize = 1024;
                                if (size - readSize < readBufferSize)
                                {
                                    readBufferSize = size - readSize;
                                }

                                var readBuffer = new byte[readBufferSize];
                                int currentRead = await stream.ReadAsync(readBuffer, 0, readBuffer.Length, token);

                                memoryStream.Write(readBuffer, 0, currentRead);

                                readSize += currentRead;
                            }

                            memoryStream.Position = 0;
                            ProcessPacket(new Packet(memoryStream), stream);

                            memoryStream.Position = 0;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                }
            }

            _cancellationTokenSource.Cancel(false);
        }

        private void ProcessPacket(Packet packet, Stream socketStream)
        {
            switch (packet.Type)
            {
                case PacketTypes.WorldStatus :
                {
                    WorldDescription currentDescription = _userWorld.GetCurrentDescription();

                    var returnPacket = new Packet(PacketTypes.WorldStatus, currentDescription);

                    returnPacket.CopyTo(socketStream);
                    break;
                }

                case PacketTypes.ClientCommand:
                {
                    MoveArguments args = packet.Data as MoveArguments;

                    if (args == null)
                    {
                        return;
                    }

                    _userWorld.SendTo(args.Units, args.Position);
                    break;
                }
            }
        }
    }
}
