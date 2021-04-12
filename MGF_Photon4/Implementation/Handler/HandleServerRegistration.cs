﻿using ExitGames.Logging;
using MGF_Photon.Implementation.Codes;
using MGF_Photon.Implementation.Data;
using MGF_Photon.Implementation.Operation;
using MGF_Photon.Implementation.Operation.Data;
using MGF_Photon.Implementation.Server;
using MultiplayerGameFramework.Implementation.Messaging;
using MultiplayerGameFramework.Interfaces.Config;
using MultiplayerGameFramework.Interfaces.Messaging;
using MultiplayerGameFramework.Interfaces.Server;
using Photon.SocketServer;
using System.IO;
using System.Xml.Serialization;

namespace MGF_Photon.Implementation.Handler
{
    public class HandleServerRegistration : ServerHandler
    {
        private readonly IServerType _serverType;
        public ILogger Log { get; set; }

        public HandleServerRegistration(ILogger log, IServerType serverType)
        {
            Log = log;
            _serverType = serverType;
        }


        public override MessageType Type
        {
            get
            {
                return MessageType.Request;
            }
        }

        public override byte Code
        {
            get
            {
                return 0;
            }
        }

        public override int? SubCode
        {
            get
            {
                return null;
            }
        }

        public override bool OnHandleMessage(IMessage message, IServerPeer serverPeer)
        {
            var peer = serverPeer as PhotonServerPeer;
            if (peer != null)
            {
                return OnHandleMessage(message, peer);
            }
            return false;

        }

        protected bool OnHandleMessage(IMessage message, PhotonServerPeer serverPeer)
        {
            OperationResponse operationResponse;
            //We are already registered, tell the subserver it tried to register more than once.
            if (serverPeer.Registered)
            {
                operationResponse = new OperationResponse(message.Code) { ReturnCode = (short)ErrorCode.InternalServerError, DebugMessage = "Already Registered" };
            }
            else
            {
                var registerRequest = new RegisterSubServer(serverPeer.Protocol, message);

                //Register Sub ServerOperatoin is bad, something is missing, etc...
                if (!registerRequest.IsValid)
                {
                    string msg = registerRequest.GetErrorMessage();
                    if (Log.IsDebugEnabled)
                    {
                        Log.DebugFormat("Invalid Register Request {0}", msg);
                    }
                    operationResponse = new OperationResponse(message.Code) { DebugMessage = msg, ReturnCode = (short)ErrorCode.OperationInvalid };
                }
                else
                {
                    //Valid Mesage, not registered, process the registration.
                    XmlSerializer mySerializer = new XmlSerializer(typeof(RegisterSubServerData));
                    StringReader instream = new StringReader(registerRequest.RegisterSubServerOperation);
                    var registerData = (RegisterSubServerData)mySerializer.Deserialize(instream);

                    if (Log.IsDebugEnabled)
                    {
                        Log.DebugFormat("Received register request: Address={0}, UdpPort={1}, TcpPort={2}, Type={3}",
                            registerData.GameServerAddress, registerData.UdpPort, registerData.TcpPort, registerData.ServerType);

                    }

                    var serverData = serverPeer.ServerData<ServerData>();
                    if (serverData == null)
                    {
                        // Autofac doesn't have a reference to ServerData so it doesn't exist in the server's IServerData list
                        Log.DebugFormat("ServerData is null...");
                    }
                    if (registerData.UdpPort.HasValue)
                    {
                        serverData.UdpAddress = registerData.GameServerAddress + ":" + registerData.UdpPort;
                    }
                    if (registerData.TcpPort.HasValue)
                    {
                        serverData.UdpAddress = registerData.GameServerAddress + ":" + registerData.TcpPort;
                    }

                    // setting server id
                    serverData.ServerId = registerData.ServerId;
                    // setting server type
                    serverData.ServerType = registerData.ServerType;
                    // looking up the server type for the server peer!
                    serverPeer.ServerType = _serverType.GetServerType(registerData.ServerType);
                    // setting applicatoin name = to the server name
                    serverData.ApplicationName = registerData.ServerName;

                    operationResponse = new OperationResponse(message.Code);
                    serverPeer.Registered = true;
                }

            }

            serverPeer.SendOperationResponse(operationResponse, new SendParameters());
            return true;
        }

    }
}
