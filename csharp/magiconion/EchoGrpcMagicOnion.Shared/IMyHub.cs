﻿using MagicOnion;
using MessagePack;
using System.Threading.Tasks;

namespace EchoGrpcMagicOnion.Shared
{
    public interface IMyHubReceiver
    {
        void OnJoin(Player player);
        void OnSend(string message);
        void OnLeave(Player player);
    }

    public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
    {
        Task<Player[]> JoinAsync(string roomName, string username);
        Task SendAsync(string message);
        Task LeaveAsync();
    }

    [MessagePackObject]
    public class Player
    {
        [Key(0)]
        public string Name { get; set; }
    }
}
