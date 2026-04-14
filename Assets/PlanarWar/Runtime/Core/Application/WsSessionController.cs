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
        private void OnAnyMessage(JObject msg) => sessionState.SetLastInboundOp(msg?["op"]?.Value<string>() ?? "-");

        private void OnWelcome(JObject payload)
            => sessionState.ApplyWelcome(payload?["sessionId"]?.Value<string>(), payload?["displayName"]?.Value<string>(), payload?["shardId"]?.Value<string>());

        private void OnHelloAck(JObject payload) => sessionState.ApplyHelloAck(payload?["shardId"]?.Value<string>());

        private void OnWhereAmI(JObject payload) => sessionState.ApplyWhereAmI(payload?["shardId"]?.Value<string>(), payload?["roomId"]?.Value<string>());

        private void OnRoomJoined(JObject payload) => sessionState.ApplyRoomJoined(payload?["roomId"]?.Value<string>());

        private void OnError(JObject payload) => sessionState.ApplyWsError(payload?["code"]?.Value<string>(), payload?["detail"]?.Value<string>());

        private void OnChat(JObject payload)
            => sessionState.ApplyChat(payload?["channelId"]?.Value<string>(), payload?["channelLabel"]?.Value<string>(), payload?["from"]?.Value<string>(), payload?["text"]?.Value<string>());
    }
}
