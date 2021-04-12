﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerGameFramework.Interfaces.Messaging
{
    //T is gonna matchup rather a server or client handler
    public interface IHandlerList<T>
    {
        bool RegisterHandler(IHandler<T> handler);
        bool HandleMessage(IMessage message, T peer);
    }
}
