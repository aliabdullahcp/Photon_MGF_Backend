using MultiplayerGameFramework.Interfaces.Messaging;
using MultiplayerGameFramework.Interfaces.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerGameFramework.Implementation.Messaging
{
    public class ServerHandlerList : IHandlerList<IServerPeer>
    {
        private readonly IDefaultRequestHandler<IServerPeer> _defaultRequestHandler;
        private readonly IDefaultResponseHandler<IServerPeer> _defaultResponseHandler;
        private readonly IDefaultEventHandler<IServerPeer> _defaultEventHandler;

        private readonly List<IHandler<IServerPeer>> _requestCodeHandlerList;
        private readonly List<IHandler<IServerPeer>> _requestSubCodeHandlerList;

        private readonly List<IHandler<IServerPeer>> _responseCodeHandlerList;
        private readonly List<IHandler<IServerPeer>> _responseSubCodeHandlerList;

        private readonly List<IHandler<IServerPeer>> _eventCodeHandlerList;
        private readonly List<IHandler<IServerPeer>> _eventSubCodeHandlerList;

        public ServerHandlerList(IEnumerable<IHandler<IServerPeer>> handlers,
            IDefaultRequestHandler<IServerPeer> defaultRequestHandler,
            IDefaultResponseHandler<IServerPeer> defaultResponseHandler,
            IDefaultEventHandler<IServerPeer> defaultEventHandler)
        {
            _defaultRequestHandler = defaultRequestHandler;
            _defaultResponseHandler = defaultResponseHandler;
            _defaultEventHandler = defaultEventHandler;

            _requestCodeHandlerList = new List<IHandler<IServerPeer>>();
            _requestSubCodeHandlerList = new List<IHandler<IServerPeer>>();

            _responseCodeHandlerList = new List<IHandler<IServerPeer>>();
            _responseSubCodeHandlerList = new List<IHandler<IServerPeer>>();

            _eventCodeHandlerList = new List<IHandler<IServerPeer>>();
            _eventSubCodeHandlerList = new List<IHandler<IServerPeer>>();

            foreach (var handler in handlers)
            {
                RegisterHandler(handler);
            }
        }

        public bool RegisterHandler(IHandler<IServerPeer> handler)
        {
            var registered = false;

            //Request
            if((handler.Type & MessageType.Request) == MessageType.Request)
            {
                if(handler.SubCode.HasValue)
                {
                    _requestSubCodeHandlerList.Add(handler);
                    registered = true;
                }else
                {
                    _requestCodeHandlerList.Add(handler);
                    registered = true;
                }
            }

            //Response
            if ((handler.Type & MessageType.Response) == MessageType.Response)
            {
                if (handler.SubCode.HasValue)
                {
                    _responseSubCodeHandlerList.Add(handler);
                    registered = true;
                }
                else
                {
                    _responseCodeHandlerList.Add(handler);
                    registered = true;
                }
            }

            //Event
            if ((handler.Type & MessageType.Async) == MessageType.Async)
            {
                if (handler.SubCode.HasValue)
                {
                    _eventSubCodeHandlerList.Add(handler);
                    registered = true;
                }
                else
                {
                    _eventCodeHandlerList.Add(handler);
                    registered = true;
                }
            }

            return registered;
        }

        public bool HandleMessage(IMessage message, IServerPeer serverPeer)
        {
            bool handled = false;
            IEnumerable<IHandler<IServerPeer>> handlers;
            switch (message.Type)
            {
                case MessageType.Request:
                    handlers = _requestCodeHandlerList.Where(h => h.Code == message.Code && h.SubCode == message.SubCode);
                    if(handlers == null || handlers.Count() == 0)
                    {
                        handlers = _requestCodeHandlerList.Where(h => h.Code == message.Code);
                        handled = true;
                    }
                    else if(!message.SubCode.HasValue && _requestCodeHandlerList.ContainsKey(message.Code))
                    {
                        _requestCodeHandlerList[message.Code].HandleMessage(message, serverPeer);
                        handled = true;
                    }
                    else
                    {
                        _defaultRequestHandler.HandleMessage(message, serverPeer);
                    }
                    break;

                case MessageType.Response:
                    if (message.SubCode.HasValue && _responseSubCodeHandlerList.ContainsKey(message.SubCode.Value))
                    {
                        _responseSubCodeHandlerList[message.SubCode.Value].HandleMessage(message, serverPeer);
                        handled = true;
                    }
                    else if (!message.SubCode.HasValue && _responseCodeHandlerList.ContainsKey(message.Code))
                    {
                        _responseCodeHandlerList[message.Code].HandleMessage(message, serverPeer);
                        handled = true;
                    }
                    else
                    {
                        _defaultResponseHandler.HandleMessage(message, serverPeer);
                    }
                    break;

                case MessageType.Async:
                    if (message.SubCode.HasValue && _eventSubCodeHandlerList.ContainsKey(message.SubCode.Value))
                    {
                        _eventSubCodeHandlerList[message.SubCode.Value].HandleMessage(message, serverPeer);
                        handled = true;
                    }
                    else if (!message.SubCode.HasValue && _eventCodeHandlerList.ContainsKey(message.Code))
                    {
                        _eventCodeHandlerList[message.Code].HandleMessage(message, serverPeer);
                        handled = true;
                    }
                    else
                    {
                        _defaultEventHandler.HandleMessage(message, serverPeer);
                    }
                    break;
            }
            return handled;
        }

    }
}
