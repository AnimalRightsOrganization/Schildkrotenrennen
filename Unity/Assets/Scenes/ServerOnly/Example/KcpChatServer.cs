﻿using System;
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
                    OnLoginByToken(connectionId, ms);
                    break;
                case PacketType.C2S_LoginReq:
                    OnLoginReq(connectionId, ms);
                    break;
                case PacketType.C2S_RegisterReq:
                    OnSignUpReq(connectionId, ms);
                    break;
                case PacketType.C2S_Chat:
                    OnChat(connectionId, ms);
                    break;
                case PacketType.C2S_RoomList:
                    OnGetRoomList(connectionId, ms);
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
        protected void OnGetRoomList(int Id, MemoryStream ms)
        {
            var request = ProtobufHelper.Deserialize<C2S_RoomListPacket>(ms);
            if (request == null)
            {
                Debug.Log("OnGetRoomList.空数据");
                return;
            }
            Debug.Log($"[C2S_GetRoomList] Page={request.Page} by {Id}");

            // 空消息，不用解析
            S2C_GetRoomList packet = new S2C_GetRoomList();
            var listAll = m_RoomManager.Sort();
            if (listAll.Count > 0)
            {
                // 获取指定区间
                var listRange = GetPage(listAll, request.Page);
                for (int i = 0; i < listRange.Count; i++)
                {
                    var serverRoom = listRange[i];
                    var roomInfo = serverRoom.GetRoomInfo(); //构建房间列表
                    roomInfo.Pwd = string.Empty;
                    packet.Rooms.Add(roomInfo);
                }
                packet.Page = listRange.Count == 0 ? 0 : request.Page; //0代表不存在的页码
            }

            ServerPlayer p = m_PlayerManager.GetPlayerByPeerId(Id);
            p.SendAsync(PacketType.S2C_RoomList, packet);
        }
        private const int EachCount = 10;
        private List<ServerRoom> GetPage(List<ServerRoom> DataSource, int PageIndex)
        {
            //Skip()跳过多少条
            //Take()查询多少条
            return DataSource.Skip((PageIndex - 1) * EachCount).Take(EachCount).ToList();
        }
        protected void OnCreateRoom(int Id, MemoryStream ms)
        {
            var request = ProtobufHelper.Deserialize<C2S_CreateRoomPacket>(ms);
            if (request == null)
            {
                Debug.Log("OnCreateRoom.空数据");
                return;
            }
            Debug.Log($"[C2S] Name={request.RoomName}, Pwd={request.RoomPwd}, playerNum={request.LimitNum} by {Id}");
            ServerPlayer p = m_PlayerManager.GetPlayerByPeerId(Id);

            // 验证合法性（总数是否超过等），在服务器创建房间
            var roomData = new BaseRoomData
            {
                RoomID = -1, //通过Manager创建时会生成。
                RoomName = request.RoomName,
                RoomPwd = request.RoomPwd,
                RoomLimit = request.LimitNum,
                Players = new List<BasePlayerData>()
                {
                    new BasePlayerData { PeerId = p.PeerId, UserName = p.UserName, NickName = p.NickName, SeatId = p.SeatId },
                },
            };
            var serverRoom = m_RoomManager.CreateServerRoom(p, roomData);
            if (serverRoom == null)
            {
                Debug.Log("大厅爆满，无法创建");
                ErrorPacket err_packet = new ErrorPacket { Code = (int)HotFix.ErrorCode.LOBBY_IS_FULL, Message = "大厅爆满" };
                p.SendAsync(PacketType.S2C_ErrorOperate, err_packet);
                return;
            }

            var roomInfo = serverRoom.GetRoomInfo(); //创建房间
            S2C_RoomInfo packet = new S2C_RoomInfo { Room = roomInfo };
            Debug.Log($"[S2C_RoomInfo] {roomInfo.ToString()}");
            p.SendAsync(PacketType.S2C_RoomInfo, packet);

            //WinFormsApp1.MainForm.Instance.RefreshRoomNum();
        }
        protected void OnJoinRoom(int Id, MemoryStream ms)
        {
            var request = ProtobufHelper.Deserialize<C2S_JoinRoomPacket>(ms);
            if (request == null)
            {
                Debug.Log("OnJoinRoom.空数据");
                return;
            }
            ServerPlayer p = m_PlayerManager.GetPlayerByPeerId(Id);
            Debug.Log($"{p.UserName}请求加入房间#{request.RoomID}");

            // 验证合法性（座位是否够，房间状态是否在等待，密码，等）
            ServerRoom serverRoom = m_RoomManager.GetServerRoom(request.RoomID);
            if (serverRoom.RoomPwd != request.RoomPwd)
            {
                ErrorPacket err_packet = new ErrorPacket { Code = (int)HotFix.ErrorCode.ROOM_PWD_ERR, Message = "房间密码错误" };
                p.SendAsync(PacketType.S2C_ErrorOperate, err_packet);
                return;
            }
            if (serverRoom.m_PlayerDic.Count >= serverRoom.RoomLimit)
            {
                ErrorPacket err_packet = new ErrorPacket { Code = (int)HotFix.ErrorCode.ROOM_IS_FULL, Message = "房间爆满" };
                p.SendAsync(PacketType.S2C_ErrorOperate, err_packet);
                return;
            }
            serverRoom.AddPlayer(p);
            Debug.Log($"允许加入，服务器修改{p.UserName}状态，#{p.RoomId},#{p.SeatId},状态:{p.Status}");

            Debug.Log(serverRoom.ToString());
            var roomInfo = serverRoom.GetRoomInfo(); //加入房间
            S2C_RoomInfo packet = new S2C_RoomInfo { Room = roomInfo };
            serverRoom.SendAsync(PacketType.S2C_RoomInfo, packet);
        }
        protected void OnLeaveRoom(int Id)
        {
            ServerPlayer p = m_PlayerManager.GetPlayerByPeerId(Id);
            Debug.Log($"{p.UserName}当前在房间#{p.RoomId}，座位#{p.SeatId}，请求离开");

            // 验证合法性
            var serverRoom = m_RoomManager.GetServerRoom(p.RoomId);
            var roomData = serverRoom.m_Data;
            bool verify = serverRoom.ContainsPlayer(p);
            if (verify == false)
            {
                Debug.Log($"错误，用户不在该房间#{p.RoomId}");
                return;
            }
            if (serverRoom.hostPlayer.Status == PlayerStatus.GAME)
            {
                Debug.Log("比赛中投降，结算时判负");
                //接下来比赛会跳过该用户回合
                //判断剩下几人，如果只剩一个真实玩家，直接弹出结算。
                return;
            }
            Debug.Log($"验证成功，可以离开房间#{p.RoomId}，座位#{p.SeatId}"); //-1, -1
            if (p.SeatId == 0)
            {
                // 如果是房主，解散房间
                m_RoomManager.RemoveServerRoom(roomData.RoomID);
                Debug.Log("是房主，解散房间");
            }
            else
            {
                serverRoom.RemovePlayer(p);

                var packet1 = new S2C_LeaveRoomPacket
                {
                    RoomID = roomData.RoomID,
                    RoomName = roomData.RoomName,
                    LeaveBy = (int)LeaveRoomType.SELF,
                };
                p.SendAsync(PacketType.S2C_LeaveRoom, packet1); //主动离开
                Debug.Log($"[S2C] 单发给{p.UserName}，离开房间");

                // 房间内其他人员广播，更新房间信息
                var roomInfo = serverRoom.GetRoomInfo(); //离开房间
                var packet2 = new S2C_RoomInfo { Room = roomInfo };
                serverRoom.SendAsync(PacketType.S2C_RoomInfo, packet2);
                Debug.Log("[S2C] 广播给房间内剩余人员，更新房间信息");
            }

            //WinFormsApp1.MainForm.Instance.RefreshRoomNum();
        }
        protected void OnOperateSeat(int Id, MemoryStream ms)
        {
            var request = ProtobufHelper.Deserialize<C2S_OperateSeatPacket>(ms);
            if (request == null)
            {
                Debug.Log("OnOperateSeat.空数据");
                return;
            }
            ServerPlayer p = m_PlayerManager.GetPlayerByPeerId(Id);
            ServerRoom serverRoom = m_RoomManager.GetServerRoom(p.RoomId);
            BaseRoomData roomData = serverRoom.m_Data;
            Debug.Log($"[C2S] {p.UserName}在房间#{p.RoomId}对座位#{request.SeatID}添加操作：{(SeatOperate)request.Operate}");

            // 都要房主权限
            if (p.SeatId != 0)
            {
                Debug.Log($"只有房主可以操作");
                return;
            }
            // 校验操作合法性，操作修改房间信息
            switch ((SeatOperate)request.Operate)
            {
                case SeatOperate.ADD_BOT:
                    {
                        bool available = serverRoom.IsAvailableSeat(request.SeatID);
                        if (available == false)
                        {
                            Debug.Log($"无法对座位#{request.SeatID}，操作：{(SeatOperate)request.Operate}");
                            return;
                        }
                        // 创建机器人
                        BasePlayerData playerData = new BasePlayerData
                        {
                            IsBot = true,
                            UserName = "_BOT_",
                            NickName = "_BOT_",
                            RoomId = roomData.RoomID,
                            SeatId = request.SeatID,
                            Status = PlayerStatus.ROOM,
                        };
                        ServerPlayer robot = new ServerPlayer(playerData);
                        m_PlayerManager.AddPlayer(robot);
                        serverRoom.AddPlayer(robot, request.SeatID);
                        break;
                    }
                case SeatOperate.KICK_PLAYER:
                    {
                        var targetPlayer = serverRoom.GetPlayer(request.SeatID);
                        if (targetPlayer == null)
                        {
                            Debug.Log($"座位上没人");
                            return;
                        }
                        serverRoom.RemovePlayer(targetPlayer);
                        var packet = new S2C_LeaveRoomPacket { RoomID = roomData.RoomID, RoomName = roomData.RoomName, LeaveBy = (int)LeaveRoomType.KICK };
                        targetPlayer.SendAsync(PacketType.S2C_LeaveRoom, packet); //被房主移除
                        break;
                    }
            }

            var roomInfo = serverRoom.GetRoomInfo(); //房主操作客位
            var packet1 = new S2C_RoomInfo { Room = roomInfo };
            Debug.Log($"[S2C] {roomInfo.ToString()}");
            // 房间内广播，更新房间信息
            serverRoom.SendAsync(PacketType.S2C_RoomInfo, packet1);
        }
        protected void OnGameStart(int Id)
        {
            ServerPlayer p = m_PlayerManager.GetPlayerByPeerId(Id);
            ServerRoom serverRoom = m_RoomManager.GetServerRoom(p.RoomId);
            Debug.Log($"{p.UserName}请求开始比赛，房间#{p.RoomId}({serverRoom.m_PlayerDic.Count}/{serverRoom.RoomLimit})");

            // 校验房间人数。
            if (serverRoom.m_PlayerDic.Count < serverRoom.RoomLimit)
            {
                Debug.Log("ERROR: 房间未满员");
                //ErrorPacket response = new ErrorPacket { Code = 0, Message = "ERROR: 房间未满员" };
                //p.SendAsync(PacketType.S2C_ErrorOperate, response);
                return;
            }

            serverRoom.OnGameStart_Server();
        }
    }
}