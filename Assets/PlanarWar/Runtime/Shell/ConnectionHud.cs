using Newtonsoft.Json.Linq;
using PlanarWar.Client.Network;
using UnityEngine;

namespace PlanarWar.Client.UI
{
    public sealed class ConnectionHud : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private PlanarWarWsClient networkClient;
        [SerializeField] private PlanarWarMessageRouter router;

        [Header("Toggle Key")]
        [SerializeField] private KeyCode toggleKey = KeyCode.F1;
        [SerializeField] private bool startVisible = false;

        private bool _visible;

        private string _wsStatus = "Disconnected";
        private string _lastOp = "-";
        private string _lastError = "-";
        private string _lastShard = "-";
        private string _lastRoom = "-";
        private string _lastChat = "-";

        private void Awake()
        {
            _visible = startVisible;
        }

        private void OnEnable()
        {
            if (networkClient != null)
            {
                networkClient.Connected += OnConnected;
                networkClient.Disconnected += OnDisconnected;
                networkClient.MessageReceived += OnAnyMessage;
            }

            if (router != null)
            {
                router.HelloAck += OnHelloAck;
                router.Welcome += OnWelcome;
                router.Chat += OnChat;
                router.WhereAmIResult += OnWhereAmIResult;
            }
        }

        private void OnDisable()
        {
            if (networkClient != null)
            {
                networkClient.Connected -= OnConnected;
                networkClient.Disconnected -= OnDisconnected;
                networkClient.MessageReceived -= OnAnyMessage;
            }

            if (router != null)
            {
                router.HelloAck -= OnHelloAck;
                router.Welcome -= OnWelcome;
                router.Chat -= OnChat;
                router.WhereAmIResult -= OnWhereAmIResult;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                _visible = !_visible;
            }

            if (networkClient != null)
            {
                _lastOp = networkClient.LastInboundOp;
                _lastError = networkClient.LastError;
            }
        }

        private void OnGUI()
        {
            if (!_visible)
            {
                return;
            }

            var style = new GUIStyle(GUI.skin.box)
            {
                fontSize = 12,
                alignment = TextAnchor.UpperLeft,
                padding = new RectOffset(10, 10, 10, 10)
            };
            style.normal.textColor = Color.white;

            GUILayout.BeginArea(new Rect(12, 12, 280, 170), style);
            GUILayout.Label("Planar War Debug HUD");
            GUILayout.Space(6);
            GUILayout.Label($"WS: {_wsStatus}");
            GUILayout.Label($"Op: {_lastOp}");
            GUILayout.Label($"Shard: {_lastShard}");
            GUILayout.Label($"Room: {_lastRoom}");
            GUILayout.Label($"Chat: {_lastChat}");
            GUILayout.Label($"Error: {_lastError}");
            GUILayout.Space(8);
            GUILayout.Label("F1 toggles this HUD.");
            GUILayout.EndArea();
        }

        private void OnConnected()
        {
            _wsStatus = "Connected";
            _lastError = "-";
        }

        private void OnDisconnected()
        {
            _wsStatus = "Disconnected";
        }

        private void OnAnyMessage(JObject msg)
        {
            _lastOp = msg?["op"]?.Read<string>() ?? "-";
        }

        private void OnHelloAck(JObject payload)
        {
            _lastShard = payload?["shardId"]?.Read<string>() ?? _lastShard;
        }

        private void OnWelcome(JObject payload)
        {
            _lastShard = payload?["shardId"]?.Read<string>() ?? _lastShard;
        }

        private void OnWhereAmIResult(JObject payload)
        {
            _lastRoom = payload?["roomId"]?.Read<string>() ?? _lastRoom;
            _lastShard = payload?["shardId"]?.Read<string>() ?? _lastShard;
        }

        private void OnChat(JObject payload)
        {
            var channel = payload?["channel"]?.Read<string>() ?? "chat";
            var text = payload?["text"]?.Read<string>() ?? "(empty)";
            _lastChat = $"{channel}: {text}";
        }
    }
}
