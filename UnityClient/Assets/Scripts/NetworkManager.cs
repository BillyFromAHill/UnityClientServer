using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Assets.Scripts;
using Shared;
using UnityEngine;

public class NetworkManager
{
    private bool _working;

    private Thread _networkThread;

    private TcpClient _tcpClient;

    private string _networkConfigFilename = "network.cfg";

    private NetworkConfiguration _networkConfig;

    private Queue<Packet> _packets = new Queue<Packet>();

    private int _sleepNetworkMs = 10;

	// Use this for initialization
	public void Start ()
	{
        _networkConfig = new NetworkConfiguration(_networkConfigFilename);

	    _working = true;
        _networkThread = new Thread(NetworkWorker);
        _networkThread.Start();
	}

    public WorldDescription LastDescription { get; private set; }

    public void RequestUpdateWorldState()
    {
        var updateWorldPacket = new Packet(PacketTypes.WorldStatus, new object());

        lock (_packets)
        {
            _packets.Enqueue(updateWorldPacket);
        }
    }

    public void SendCommand(MoveArguments args)
    {
        var commandPacket = new Packet(PacketTypes.ClientCommand, args);

        lock (_packets)
        {
            _packets.Enqueue(commandPacket);
        }
    }

    private void NetworkWorker()
    {
        while (_working)
        {
            try
            {
                _tcpClient = new TcpClient(_networkConfig.Address, _networkConfig.Port);

                if (!_tcpClient.Connected)
                {
                    _tcpClient.Connect(_networkConfig.Address, _networkConfig.Port);
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
                Debug.logger.LogException(e);
                Thread.Sleep(_networkConfig.ReconnectionTime);
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
