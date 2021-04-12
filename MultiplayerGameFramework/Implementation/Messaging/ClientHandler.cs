using MultiplayerGameFramework.Interfaces.Messaging;
using MultiplayerGameFramework.Interfaces.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerGameFramework.Implementation.Messaging
{
    public abstract class ClientHandler : IHandler<IClientPeer>
    {
        public abstract MessageType Type { get; }
        public abstract byte Code { get; }
        public abstract int? SubCode { get; }

        public bool HandleMessage(IMessage message, IClientPeer peer)
        {
            return OnHandleMessage(message, peer);
        }

        protected abstract bool OnHandleMessage(IMessage messsage, IClientPeer peer);

    }
}
