using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Shared;
using UnityEngine;

public class NetworkManager
{
    private object syncLock = new object();

    private bool _working;

    private Thread _networkThread;

    private TcpClient _tcpClient;

    private string _address = "127.0.0.1";

    private int _port = 25148;

    private int _reconnectionTime = 3000;

    private Queue<Packet> _packets = new Queue<Packet>();

    private int _sleepNetworkMs = 10;

	// Use this for initialization
	public void Start ()
	{
	    _working = true;
        _networkThread = new Thread(NetworkWorker);
        _networkThread.Start();
	}

    public WorldDescription LastDescription { get; private set; }

    public void RequestUpdateWorldState()
    {
        var updateWorldPacket = new Packet(PacketTypes.WorldStatus, null);

        lock (_packets)
        {
            _packets.Enqueue(updateWorldPacket);
        }
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

                using (NetworkStream networkStream = _tcpClient.GetStream())
                {

                    while (_tcpClient.Connected)
                    {
                        lock (_packets)
                        {
                            while (_packets.Count > 0)
                            {
                                var packet = _packets.Dequeue();

                                packet.CopyTo(networkStream);
                            }
                        }

                        while (networkStream.DataAvailable)
                        {
                            var packet = new Packet(networkStream);

                            ProcessPacket(packet);
                        }

                        Thread.Sleep(_sleepNetworkMs);
                    }
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e);
                Thread.Sleep(_reconnectionTime);
            }

        }
    }

    private void ProcessPacket(Packet packet)
    {
        switch (packet.Type)
        {
            case PacketTypes.WorldStatus:
            {
                LastDescription = (WorldDescription)packet.Data;
                break;
            }
        }
    }
}
