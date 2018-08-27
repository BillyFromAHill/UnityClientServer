using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnityClientServer
{
    class Application
    {
        private UsersManager _usersManager;

        private int _mainThreadLoopInterval = 500;

        public Application()
        {
            _usersManager = new UsersManager(
                UnityClientServer.Server.Default.Address,
                UnityClientServer.Server.Default.Port);
        }

        public void Start()
        {
            _usersManager.Start();

            while (true)
            {
                Thread.Sleep(_mainThreadLoopInterval);
            }
        }
    }
}
