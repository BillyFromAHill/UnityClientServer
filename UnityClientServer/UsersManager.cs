using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UnityClientServer
{
    public class NetworkManager
    {
        private ConnectionManager _connectionManager;

        public NetworkManager(string address, Int16 port)
        {
            _connectionManager = new ConnectionManager(address, port);
        }

        public void Start()
        {
            _connectionManager.SocketConnected += ConnectionManager_SocketConnected;
        }

        private void ConnectionManager_SocketConnected(object sender, Socket e)
        {
            
        }
    }
}
