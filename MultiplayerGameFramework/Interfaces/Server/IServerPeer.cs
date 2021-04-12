﻿using MultiplayerGameFramework.Interfaces.Config;
using MultiplayerGameFramework.Interfaces.Messaging;


namespace MultiplayerGameFramework.Interfaces.Server
{
    public interface IServerPeer
    {
        IServerType ServerType { get; set; }
        T ServerData<T>() where T : class, IServerData;
        bool Registered { get; set; }
        IServerApplication Server { get; set; }
        void Disconnect();
        void SendMessage(IMessage message);
        void HandleMessage(IMessage message);
    }
}
