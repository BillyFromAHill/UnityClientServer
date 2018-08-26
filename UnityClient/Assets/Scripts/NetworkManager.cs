using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class NetworkManager
{
    private object syncLock = new object();

    private bool _working;

    private Thread _networkThread;

    private TcpClient _tcpClient;

    private string _address = "127.0.0.1";

    private int _port = 25143;

    private int _reconnectionTime = 3000;

	// Use this for initialization
	public void Start ()
	{
	    _working = true;
        _networkThread = new Thread(NetworkWorker);
        _networkThread.Start();
	}
	

    private void NetworkWorker()
    {
        while (_working)
        {
            try
            {
                _tcpClient = new TcpClient(_address, _port);

                if (!_tcpClient.Connected)
                {
                    _tcpClient.Connect(_address, _port);
                }

                using (Stream networkStream = _tcpClient.GetStream())
                {
                    byte[] buffer = new byte[1024];

                    networkStream.Read(buffer, 0, buffer.Length);
                }
            }

            catch (SocketException e)
            {
                Console.WriteLine(e);
                Thread.Sleep(_reconnectionTime);
            }

        }
    }
}
