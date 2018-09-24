using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnityClientServer
{
    public class ConnectionManager
    {
        private TcpListener _tcpListener;

        private CancellationTokenSource _tokenSource;

        public event EventHandler<Socket> SocketConnected;

        public ConnectionManager(string address, Int16 port)
        {
            var localAddr = IPAddress.Parse(address);
            _tcpListener = new TcpListener(localAddr, port);
            _tokenSource = new CancellationTokenSource();
        }

        public void StartListen()
        {
            Task.Factory.StartNew(() =>
            {
                ListenWorker(_tokenSource.Token);
            }, TaskCreationOptions.LongRunning);
        }

        private async void ListenWorker(CancellationToken cancellationToken)
        {
            try
            {
                _tcpListener.Start();
                while (!cancellationToken.IsCancellationRequested)
                {
                    Socket socket = await _tcpListener.AcceptSocketAsync();
                    OnSocketConnected(socket);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
        }

        public void StopListen()
        {
            _tokenSource.Cancel();
        }


        protected virtual void OnSocketConnected(Socket socket)
        {
            var handler = SocketConnected;
            if (handler != null)
            {
                handler(this, socket);
            }
        }
    }


}
