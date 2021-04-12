﻿using ExitGames.Logging;
using MultiplayerGameFramework;
using MultiplayerGameFramework.Implementation.Config;
using MultiplayerGameFramework.Interfaces.Config;
using MultiplayerGameFramework.Interfaces.Support;
using MultiplayerGameFramework.Interfaces.Client;
using MultiplayerGameFramework.Interfaces.Server;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.SocketServer;
using System.IO;
using System.Xml.Serialization;
using MGF_Photon.Implementation.Operation.Data;
using MGF_Photon.Implementation.Operation;
using MGF_Photon.Implementation.Server;
using MGF.Photon.Implementation.Server;

namespace MGF_Photon.Implementation
{
    public class ServerApplication : IServerApplication
    {
        public ServerConfiguration ServerConfiguration { get; set; }
        protected ILogger Log;
        protected PhotonApplication Server;

        private readonly IEnumerable<PeerInfo> _peerInfo;
        private readonly IEnumerable<IBackgroundThread> _backgroundThreads;
        private readonly IServerConnectionCollection<IServerType, IServerPeer> _serverCollection;
        private readonly IConnectionCollection<IClientPeer> _clientCollection;
        private readonly IEnumerable<IAfterServerRegistration> _afterServerRegistrationEvents;

        public byte SubCodeParameterCode { get { return ServerConfiguration.SubCodeParameterCode; } }
        public string BinaryPath { get { return Server.BinaryPath; } }
        public string ApplicationName { get { return Server.ApplicationName; } }

        public ServerApplication(ServerConfiguration serverConfiguration, ILogger log, PhotonApplication server,
            IEnumerable<PeerInfo> peerInfo,
            IEnumerable<IBackgroundThread> backgroundThreads,
            IServerConnectionCollection<IServerType, IServerPeer> serverCollection,
            IConnectionCollection<IClientPeer> clientCollection,
            IEnumerable<IAfterServerRegistration> afterServerRegistrationEvents)
        {
            ServerConfiguration = serverConfiguration;
            Log = log;
            Server = server;
            _peerInfo = peerInfo;
            _backgroundThreads = backgroundThreads;
            _serverCollection = serverCollection;
            _clientCollection = clientCollection;
            _afterServerRegistrationEvents = afterServerRegistrationEvents;

        }

        public void Setup()
        {
            //resolve all parameters
            //start background threads
            foreach (var backgroundThread in _backgroundThreads)
            {
                backgroundThread.Setup(this);
                ThreadPool.QueueUserWorkItem(backgroundThread.Run);
            }
            //connect all "peer" servers
            foreach(var peerInfo in _peerInfo)
            {
                ConnectToPeer(peerInfo);
            }
        }

        public void TearDown()
        {
            //disconnect all clients
            var clients = _clientCollection.GetPeers<IClientPeer>();
            Log.DebugFormat("Disconnection {0} peers", clients.Count);
            foreach(var client in clients)
            {
                client.Disconnect();
            }
            _clientCollection.Clear();
            //disconnect from all servers
            var servers = _serverCollection.GetPeers<IServerPeer>();
            Log.DebugFormat("Disconnecting {0} servers", servers.Count);
            foreach(var server in servers)
            {
                server.Disconnect();
            }
            _serverCollection.Clear();
            //stop all background threads
            foreach(var backgroundThread in _backgroundThreads)
            {
                backgroundThread.Stop();
            }
        }

        public void OnServerConnectionFailed(int errorCode,string errorMessage, object state)
        {
            //called when a connection to a parent fails
            //var peerInfo = state as PeerInfo;
            //if(peerInfo != null)
            //{
            //    ReconnectToPeer(peerInfo);
            //}
        }

        public void ReconnectToPeer(PeerInfo peerInfo)
        {
            peerInfo.NumTries++;
            if(peerInfo.NumTries < peerInfo.MaxTries)
            {
                var timer = new Timer(o => ConnectToPeer(peerInfo), null, peerInfo.ConnectRetryIntervalSeconds * 1000, 0);
            }
        }

        public void AfterServerRegistration(IServerPeer serverPeer)
        {
            foreach(var afterServerRegistration in _afterServerRegistrationEvents)
            {
                afterServerRegistration.AfterRegister(serverPeer);
            }
        }

        public void ConnectToPeer(PeerInfo peerInfo)
        {
            var outbound = new OutboundPhotonPeer(Server, peerInfo);
            //called by ReconnectToPeer and Setup in PhotonApplication
            // By Tutorial he used Server.ConnectToServerTcp
            if (outbound.ConnectTcp(peerInfo.MasterEndPoint, peerInfo.ApplicationName, TypeCache.SerializePeerInfo(peerInfo)) == false)
            {
                Log.Warn("Connection refused");
            }
        }

        //Will be called anytime server talk successfully to another server
        public void Register(PhotonServerPeer peer)
        {
            var registerSubServerOperation = new RegisterSubServerData()
            {
                GameServerAddress = ServerConfiguration.PublicIpAddress,
                TcpPort = ServerConfiguration.TcpPort,
                UdpPort = ServerConfiguration.UdpPort,
                ServerId = ServerConfiguration.ServerId,
                ServerType = ServerConfiguration.ServerType,
                ServerName = ServerConfiguration.ServerName
            };

            XmlSerializer mySerializer = new XmlSerializer(typeof(RegisterSubServerData));
            StringWriter outString = new StringWriter();
            mySerializer.Serialize(outString, registerSubServerOperation);

            peer.SendOperationRequest(new OperationRequest(0, new RegisterSubServer() { RegisterSubServerOperation = outString.ToString() }), new SendParameters());
        }

    }
}
