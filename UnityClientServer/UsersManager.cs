using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UnityClientServer
{
    public class UsersManager
    {
        private ConnectionManager _connectionManager;

        private List<UserManager> _managers = new List<UserManager>();

        public UsersManager(string address, Int16 port)
        {
            _connectionManager = new ConnectionManager(address, port);
        }

        public void Start()
        {
            _connectionManager.SocketConnected += ConnectionManager_SocketConnected;
            _connectionManager.StartListen();
        }

        private void ConnectionManager_SocketConnected(object sender, Socket e)
        {
            _managers.Add(new UserManager(e));
        }
    }
}
