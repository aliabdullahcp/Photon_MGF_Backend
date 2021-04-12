﻿using MultiplayerGameFramework.Interfaces.Client;
using MultiplayerGameFramework.Interfaces.Support;
using System.Collections.Generic;
using System.Linq;

namespace MultiplayerGameFramework.Implementation.Client
{
    public class ClientConnectionCollection : IConnectionCollection<IClientPeer>
    {
        private readonly IEnumerable<IClientConnectionHandler> _clientConnectionHandlers;
        private List<IClientPeer> _clients;

        //What we should do when client connects
        public ClientConnectionCollection(IEnumerable<IClientConnectionHandler> clientConnectionHandlers)
        {
            _clientConnectionHandlers = clientConnectionHandlers;
            _clients = new List<IClientPeer>();
        }

        public void Connect(IClientPeer peer)
        {
            _clients.Add(peer);
            foreach (var clientConnectionHandler in _clientConnectionHandlers)
            {
                clientConnectionHandler.ClientConnect(peer);
            }
        }

        public void Clear()
        {
            _clients.Clear();
        }

        public void Disconnect(IClientPeer peer)
        {
            _clients.Remove(peer);
            foreach (var clientConnectionHandler in _clientConnectionHandlers)
            {
                clientConnectionHandler.ClientDisconnect(peer);

            }
            peer.Disconnect();
        }

        public List<T> GetPeers<T>() where T : class, IClientPeer
        {
            return new List<T>(_clients.Cast<T>());
        }
    }
}
