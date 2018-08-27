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
            _userWorld = new World();

            Task.Factory.StartNew(() => UserInteractor(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
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
                            while(stream.DataAvailable)
                            {
                                byte[] buffer = new byte[1024];
                                int read = await stream.ReadAsync(buffer, 0, buffer.Length);

                                memoryStream.Write(buffer, 0, read);
                            }

                            while(memoryStream.Position < memoryStream.Length - 1)
                            {
                                var packet = new Packet(memoryStream);

                                ProcessPacket(packet, stream);
                            }

                            memoryStream.Position = 0;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }

                }
            }
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
            }
        }
    }
}
