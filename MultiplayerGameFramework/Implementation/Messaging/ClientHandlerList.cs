using MultiplayerGameFramework.Interfaces.Messaging;
using MultiplayerGameFramework.Interfaces.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerGameFramework.Implementation.Messaging
{
    public class ClientHandlerList : IHandlerList<IClientPeer>
    {
        private readonly List<IHandler<IClientPeer>> _requestSubCodeHandlerList;
        private readonly List<IHandler<IClientPeer>> _requestCodeHandlerList;

        public ClientHandlerList(IEnumerable<IHandler<IClientPeer>> handlers)
        {
            _requestSubCodeHandlerList = new List<IHandler<IClientPeer>>();
            _requestCodeHandlerList = new List<IHandler<IClientPeer>>();

            foreach (var handler in handlers)
            {
                RegisterHandler(handler);
            }
        }

        public bool RegisterHandler(IHandler<IClientPeer> handler)
        {
            var registered = false;
            if((handler.Type & MessageType.Request) == MessageType.Request) //Guarantees that we are having a request type hanler comming in
            {
                if(handler.SubCode.HasValue)
                {
                    _requestSubCodeHandlerList.Add(handler);
                    registered = true;
                }
                else
                {
                    _requestCodeHandlerList.Add(handler);
                    registered = true;
                }
            }
            return registered;
        }

        public bool HandleMessage(IMessage message, IClientPeer peer)
        {
            bool handled = false;
            switch (message.Type)
            {
                case MessageType.Request:
                    if(message.SubCode.HasValue && _requestSubCodeHandlerList.ContainsKey(message.SubCode.Value))
                    {
                        _requestSubCodeHandlerList[message.SubCode.Value].HandleMessage(message, peer);
                        handled = true;
                    }
                    else if (!message.SubCode.HasValue && _requestCodeHandlerList.ContainsKey(message.Code))
                    {
                        _requestCodeHandlerList[message.Code].HandleMessage(message, peer);
                        handled = true;
                    }
                    break;
            }
            return handled;
        }
    }
}