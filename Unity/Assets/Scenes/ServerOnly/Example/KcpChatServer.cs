using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using HotFix;
using ET;
using NetCoreServer;
using NetCoreServer.Utils;
using UnityEditor.Search;

namespace kcp2k.Examples
{
    public class KcpChatServer : MonoBehaviour
    {
        // configuration
        public ushort Port = 7777;
        public KcpConfig config = new KcpConfig();

        public static ServerRoomManager m_RoomManager;
        public static ServerPlayerManager m_PlayerManager;

        // server
        public KcpServer server;

        // MonoBehaviour ///////////////////////////////////////////////////////
        void Awake()
        {
            // logging
            Log.Info = Debug.Log;
            Log.Warning = Debug.LogWarning;
            Log.Error = Debug.LogError;

            server = new KcpServer(OnConnected, OnData, OnDisonnected, OnError, config);
        }

        public void LateUpdate() => server.Tick();

        void OnGUI()
        {
            int firstclient = server.connections.Count > 0 ? server.connections.First().Key : -1;

            GUILayout.BeginArea(new Rect(160, 5, 250, 400));
            GUILayout.Label("Server:");
            if (GUILayout.Button("Start"))
            {
                server.Start(Port);
            }
            if (GUILayout.Button("Send 0x01, 0x02 to " + firstclient))
            {
                server.Send(firstclient, new ArraySegment<byte>(new byte[] { 0x01, 0x02 }), KcpChannel.Reliable);
            }
            if (GUILayout.Button("Send 0x03, 0x04 to " + firstclient + " unreliable"))
            {
                server.Send(firstclient, new ArraySegment<byte>(new byte[] { 0x03, 0x04 }), KcpChannel.Unreliable);
            }
            if (GUILayout.Button("Disconnect connection " + firstclient))
            {
                server.Disconnect(firstclient);
            }
            if (GUILayout.Button("Stop"))
            {
                server.Stop();
            }
            GUILayout.EndArea();
        }

        void OnConnected(int connectionId)
        {
            Debug.Log($"KCP: OnConnected({connectionId}");

            EmptyPacket cmd = new EmptyPacket();
            SendAsync(connectionId, PacketType.Connected, cmd);
        }
        void OnData(int connectionId, ArraySegment<byte> message, KcpChannel channel)
        {
            Debug.Log($"KCP: OnServerDataReceived({connectionId}, {BitConverter.ToString(message.Array, message.Offset, message.Count)} @ {channel})");
            
            Handle(connectionId, message.ToArray());
        }
        void OnDisonnected(int connectionId)
        {
            Debug.Log($"KCP: OnDisonnected({connectionId}");
        }
        void OnError(int connectionId, ErrorCode error, string reason)
        {
            Debug.LogWarning($"KCP: OnServerError({connectionId}, {error}, {reason}");
        }

        private void SendAsync(int clientId, PacketType msgId, object cmd, KcpChannel channel = KcpChannel.Reliable)
        {
            byte[] header = new byte[1] { (byte)msgId };
            byte[] body = ProtobufHelper.ToBytes(cmd);
            byte[] buffer = new byte[header.Length + body.Length];
            Array.Copy(header, 0, buffer, 0, header.Length);
            Array.Copy(body, 0, buffer, header.Length, body.Length);
            //Debug.Log($"header:{header.Length},body:{body.Length},buffer:{buffer.Length},");
            server.Send(clientId, new ArraySegment<byte>(buffer), channel);
        }

        private void Handle(int connectionId, byte[] buffer)
        {
            // 解析msgId
            byte msgId = buffer[0];
            int size = buffer.Length - 1;
            byte[] body = new byte[size - 1];
            try
            {
                Array.Copy(buffer, 1, body, 0, size - 1);
            }
            catch (Exception e)
            {
                throw e;
            }
            MemoryStream ms = new MemoryStream(body, 0, body.Length);
            PacketType type = (PacketType)msgId;
            Debug.Log($"msgType={type}, from {connectionId}");

            switch (type)
            {
                case PacketType.Connected:
                    break;
                case PacketType.Disconnect:
                    break;
                case PacketType.C2S_LoginToken:
                    //OnLoginByToken(connectionId, ms);
                    break;
                case PacketType.C2S_LoginReq:
                    OnLoginReq(connectionId, ms);
                    break;
                case PacketType.C2S_RegisterReq:
                    //OnSignUpReq(connectionId, ms);
                    break;
                case PacketType.C2S_Chat:
                    //OnChat(connectionId, ms);
                    break;
                case PacketType.C2S_RoomList:
                    //OnGetRoomList(connectionId, ms);
                    break;
                case PacketType.C2S_CreateRoom:
                    //OnCreateRoom(connectionId, ms);
                    break;
                case PacketType.C2S_JoinRoom:
                    //OnJoinRoom(connectionId, ms);
                    break;
                case PacketType.C2S_LeaveRoom:
                    //OnLeaveRoom(connectionId);
                    break;
                case PacketType.C2S_OperateSeat:
                    //OnOperateSeat(connectionId, ms);
                    break;
                case PacketType.C2S_GameStart:
                    //OnGameStart(connectionId);
                    break;
                case PacketType.C2S_GamePlay:
                    //OnGamePlay(connectionId, ms);
                    break;
                default:
                    Debug.Log($"无法识别的消息: {type}");
                    break;
            }
        }

        protected async void OnLoginByToken(int Id, MemoryStream ms)
        {
            var request = ProtobufHelper.Deserialize<C2S_LoginTokenPacket>(ms); //解包
            if (request == null)
            {
                Debug.Log("Token.空数据");
                return;
            }
            Debug.Log($"[C2S] Token={request.Token} by {Id}");

            UserInfo result = await MySQLTool.GetUserInfo(request.Token);
            if (result == null)
            {
                Debug.Log($"用户名或密码错误");
                var tempData = new BasePlayerData { PeerId = Id, };
                var tempPlayer = new ServerPlayer(tempData);
                ErrorPacket err_packet = new ErrorPacket { Code = (int)HotFix.ErrorCode.LOGIN_FAILED, Message = "用户名或密码错误" };
                tempPlayer.SendAsync(PacketType.S2C_ErrorOperate, err_packet);
                return;
            }
            var playerData = new BasePlayerData
            {
                IsBot = false,
                PeerId = Id,
                UserName = result.username,
                NickName = result.nickname,
                RoomId = -1,
                SeatId = -1,
                Status = PlayerStatus.LOBBY,
            };
            var serverPlayer = new ServerPlayer(playerData);
            m_PlayerManager.AddPlayer(serverPlayer);

            var packet = new S2C_LoginResultPacket { Code = 0, Username = result.username, Nickname = result.nickname };
            serverPlayer.SendAsync(PacketType.S2C_LoginResult, packet);
        }
        protected async void OnLoginReq(int Id, MemoryStream ms)
        {
            var request = ProtobufHelper.Deserialize<C2S_LoginPacket>(ms); //解包
            if (request == null)
            {
                Debug.Log("OnLoginReq.空数据");
                return;
            }
            Debug.Log($"[C2S] Username={request.Username}, Password={request.Password} by {Id}");

            UserInfo result = await MySQLTool.GetUserInfo(request.Username, request.Password);
            if (result == null)
            {
                Debug.Log($"用户名或密码错误");
                var tempData = new BasePlayerData { PeerId = Id, };
                var tempPlayer = new ServerPlayer(tempData);
                ErrorPacket err_packet = new ErrorPacket { Code = (int)HotFix.ErrorCode.LOGIN_FAILED, Message = "用户名或密码错误" };
                tempPlayer.SendAsync(PacketType.S2C_ErrorOperate, err_packet);
                return;
            }
            var playerData = new BasePlayerData
            {
                IsBot = false,
                PeerId = Id,
                UserName = request.Username,
                NickName = result.nickname,
                RoomId = -1,
                SeatId = -1,
                Status = PlayerStatus.LOBBY,
            };
            var serverPlayer = new ServerPlayer(playerData);
            m_PlayerManager.AddPlayer(serverPlayer);

            var packet = new S2C_LoginResultPacket { Code = 0, Username = request.Username, Nickname = result.nickname };
            serverPlayer.SendAsync(PacketType.S2C_LoginResult, packet);
        }
        protected async void OnSignUpReq(int Id, MemoryStream ms)
        {
            var request = ProtobufHelper.Deserialize<C2S_LoginPacket>(ms); //解包
            if (request == null)
            {
                Debug.Log("OnSignUpReq.空数据");
                return;
            }

            Debug.Log($"[C2S] 请求注册:{request.Username}");

            bool result = await MySQLTool.SignUp(request.Username, request.Password);
            Debug.Log($"注册结果: {result}");

            var playerData = new BasePlayerData
            {
                IsBot = false,
                PeerId = Id,
                UserName = request.Username,
                NickName = request.Username,
                RoomId = -1,
                SeatId = -1,
                Status = PlayerStatus.OFFLINE,
            };
            var serverPlayer = new ServerPlayer(playerData);
            if (result)
            {
                var packet = new S2C_LoginResultPacket { Code = 0, Username = request.Username, Nickname = request.Username };
                serverPlayer.SendAsync(PacketType.S2C_LoginResult, packet);
            }
            else
            {
                var err_packet = new ErrorPacket { Code = (int)HotFix.ErrorCode.USERNAME_USED, Message = "用户名已被注册" };
                serverPlayer.SendAsync(PacketType.S2C_ErrorOperate, err_packet);
            }
        }
        protected void OnChat(int Id, MemoryStream ms)
        {
            var request = ProtobufHelper.Deserialize<TheMsg>(ms);
            if (request == null)
            {
                Debug.Log("OnChat.空数据");
                return;
            }
            Debug.Log($"{request.Name}说: {request.Content}");
        }
    }
}