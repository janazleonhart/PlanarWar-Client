using Newtonsoft.Json.Linq;
using PlanarWar.Client.Network;
using System;

namespace PlanarWar.Client.Core.Application
{
    public sealed class WsSessionController : IDisposable
    {
        private readonly PlanarWarWsClient wsClient;
        private readonly PlanarWarMessageRouter router;
        private readonly SessionState sessionState;
        private readonly bool autoRequestWhereAmIOnConnect;
        private readonly bool autoJoinLobbyOnConnect;

        public WsSessionController(PlanarWarWsClient wsClient, PlanarWarMessageRouter router, SessionState sessionState, bool autoRequestWhereAmIOnConnect, bool autoJoinLobbyOnConnect)
        {
            this.wsClient = wsClient;
            this.router = router;
            this.sessionState = sessionState;
            this.autoRequestWhereAmIOnConnect = autoRequestWhereAmIOnConnect;
            this.autoJoinLobbyOnConnect = autoJoinLobbyOnConnect;

            if (wsClient != null)
            {
                wsClient.Connected += OnConnected;
                wsClient.Disconnected += OnDisconnected;
                wsClient.MessageReceived += OnAnyMessage;
            }

            if (router != null)
            {
                router.Welcome += OnWelcome;
                router.HelloAck += OnHelloAck;
                router.WhereAmIResult += OnWhereAmI;
                router.RoomJoined += OnRoomJoined;
                router.RoomLeft += _ => sessionState.ApplyRoomLeft();
                router.Error += OnError;
                router.Chat += OnChat;
            }
        }

        public void Dispose()
        {
            if (wsClient != null)
            {
                wsClient.Connected -= OnConnected;
                wsClient.Disconnected -= OnDisconnected;
                wsClient.MessageReceived -= OnAnyMessage;
            }

            if (router != null)
            {
                router.Welcome -= OnWelcome;
                router.HelloAck -= OnHelloAck;
                router.WhereAmIResult -= OnWhereAmI;
                router.RoomJoined -= OnRoomJoined;
                router.Error -= OnError;
                router.Chat -= OnChat;
            }
        }

        public void RequestWhereAmI() => wsClient?.SendOp("whereami");
        public void SendPing() => wsClient?.SendOp("ping");

        private void OnConnected()
        {
            sessionState.SetConnectionState(true);
            wsClient?.SendOp("hello", new JObject { ["client"] = "unity-shell" });
            if (autoRequestWhereAmIOnConnect) RequestWhereAmI();
            if (autoJoinLobbyOnConnect) wsClient?.SendOp("room_join", new JObject { ["roomId"] = "lobby" });
        }

        private void OnDisconnected() => sessionState.SetConnectionState(false);
        private void OnAnyMessage(JObject msg) => sessionState.SetLastInboundOp(msg?["op"]?.Read<string>() ?? "-");

        private void OnWelcome(JObject payload)
            => sessionState.ApplyWelcome(payload?["sessionId"]?.Read<string>(), payload?["displayName"]?.Read<string>(), payload?["shardId"]?.Read<string>());

        private void OnHelloAck(JObject payload) => sessionState.ApplyHelloAck(payload?["shardId"]?.Read<string>());

        private void OnWhereAmI(JObject payload) => sessionState.ApplyWhereAmI(payload?["shardId"]?.Read<string>(), payload?["roomId"]?.Read<string>());

        private void OnRoomJoined(JObject payload) => sessionState.ApplyRoomJoined(payload?["roomId"]?.Read<string>());

        private void OnError(JObject payload) => sessionState.ApplyWsError(payload?["code"]?.Read<string>(), payload?["detail"]?.Read<string>());

        private void OnChat(JObject payload)
            => sessionState.ApplyChat(payload?["channelId"]?.Read<string>(), payload?["channelLabel"]?.Read<string>(), payload?["from"]?.Read<string>(), payload?["text"]?.Read<string>());
    }
}
