using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace PlanarWar.Client.Network
{
    public sealed class PlanarWarWsClient : MonoBehaviour
    {
        [Header("Connection")]
        [SerializeField] private string serverUrl = "ws://127.0.0.1:7777/ws";
        [SerializeField] private bool autoConnectOnStart = false;

        [Header("Heartbeat")]
        [SerializeField] private float heartbeatIntervalSeconds = 5f;

        public event Action Connected;
        public event Action Disconnected;
        public event Action<JObject> MessageReceived;

        public string ServerUrl => serverUrl;
        public bool IsConnected => _connected && _ws != null && _ws.State == WebSocketState.Open;
        public bool IsConnecting => _connecting;
        public string LastInboundOp { get; private set; } = "-";
        public string LastError { get; private set; } = "-";
        public int ReconnectCount { get; private set; }
        public string AuthToken => _authToken;

        private ClientWebSocket _ws;
        private CancellationTokenSource _cts;
        private readonly ConcurrentQueue<JObject> _messageQueue = new();
        private readonly ConcurrentQueue<Action> _mainThreadActions = new();

        private bool _connecting;
        private bool _connected;
        private float _heartbeatTimer;
        private string _authToken = string.Empty;

        private void Start()
        {
            if (autoConnectOnStart)
            {
                Connect();
            }
        }

        private void Update()
        {
            while (_messageQueue.TryDequeue(out var msg))
            {
                try
                {
                    LastInboundOp = msg["op"]?.Read<string>() ?? "-";
                    MessageReceived?.Invoke(msg);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[PlanarWarWsClient] Message dispatch failed: {ex}");
                }
            }

            while (_mainThreadActions.TryDequeue(out var action))
            {
                try
                {
                    action?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[PlanarWarWsClient] Main-thread action failed: {ex}");
                }
            }

            if (!IsConnected)
            {
                return;
            }

            _heartbeatTimer += Time.deltaTime;
            if (_heartbeatTimer >= heartbeatIntervalSeconds)
            {
                _heartbeatTimer = 0f;
                SendOp("heartbeat");
            }
        }

        private void OnDestroy()
        {
            Disconnect();
        }

        public void SetServerUrl(string url)
        {
            serverUrl = url?.Trim() ?? serverUrl;
        }

        public void SetAuthToken(string token)
        {
            _authToken = token?.Trim() ?? string.Empty;
        }

        public void ClearAuthToken()
        {
            _authToken = string.Empty;
        }

        public void Connect()
        {
            if (_connecting || IsConnected)
            {
                return;
            }

            _connecting = true;
            _cts = new CancellationTokenSource();
            _ws = new ClientWebSocket();
            _ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(15);

            Task.Run(ConnectAsync);
        }

        public void Reconnect()
        {
            Disconnect();
            Connect();
        }

        public void Disconnect()
        {
            try
            {
                _cts?.Cancel();
            }
            catch
            {
            }

            try
            {
                _ws?.Dispose();
            }
            catch
            {
            }

            _ws = null;

            if (_connected)
            {
                _connected = false;
                _mainThreadActions.Enqueue(() => Disconnected?.Invoke());
            }

            _connecting = false;
        }

        public void Send(JObject msg)
        {
            if (!IsConnected || msg == null)
            {
                return;
            }

            var json = msg.ToString();
            var bytes = Encoding.UTF8.GetBytes(json);
            var segment = new ArraySegment<byte>(bytes);

            Task.Run(async () =>
            {
                try
                {
                    await _ws.SendAsync(segment, WebSocketMessageType.Text, true, _cts.Token);
                }
                catch (Exception ex)
                {
                    LastError = ex.Message;
                    Debug.LogError($"[PlanarWarWsClient] Send failed: {ex}");
                }
            });
        }

        public void SendOp(string op, JObject payload = null)
        {
            if (string.IsNullOrWhiteSpace(op))
            {
                return;
            }

            var msg = new JObject
            {
                ["op"] = op,
                ["payload"] = payload ?? new JObject()
            };

            Send(msg);
        }

        private async Task ConnectAsync()
        {
            try
            {
                var resolvedUrl = BuildConnectUrl();
                Debug.Log($"[PlanarWarWsClient] Connecting to {resolvedUrl}");
                await _ws.ConnectAsync(new Uri(resolvedUrl), _cts.Token);

                if (_ws.State != WebSocketState.Open)
                {
                    _connecting = false;
                    LastError = $"Socket state after connect: {_ws.State}";
                    Debug.LogWarning($"[PlanarWarWsClient] {LastError}");
                    return;
                }

                _connected = true;
                _connecting = false;
                _heartbeatTimer = 0f;
                LastError = "-";

                _mainThreadActions.Enqueue(() =>
                {
                    Debug.Log("[PlanarWarWsClient] Connected.");
                    Connected?.Invoke();
                });

                await ReceiveLoop(_cts.Token);
            }
            catch (Exception ex)
            {
                _connecting = false;
                _connected = false;
                LastError = ex.Message;
                ReconnectCount++;
                Debug.LogError($"[PlanarWarWsClient] Connect failed: {ex}");
                _mainThreadActions.Enqueue(() => Disconnected?.Invoke());
            }
        }

        private string BuildConnectUrl()
        {
            if (string.IsNullOrWhiteSpace(_authToken))
            {
                return serverUrl;
            }

            if (!Uri.TryCreate(serverUrl, UriKind.Absolute, out var uri))
            {
                return serverUrl;
            }

            var builder = new UriBuilder(uri);
            var existingQuery = builder.Query?.TrimStart('?') ?? string.Empty;
            var tokenParam = $"token={Uri.EscapeDataString(_authToken)}";
            builder.Query = string.IsNullOrWhiteSpace(existingQuery)
                ? tokenParam
                : (existingQuery.Contains("token=") ? existingQuery : $"{existingQuery}&{tokenParam}");
            return builder.Uri.ToString();
        }

        private async Task ReceiveLoop(CancellationToken token)
        {
            var buffer = new byte[64 * 1024];

            try
            {
                while (!token.IsCancellationRequested && _ws != null && _ws.State == WebSocketState.Open)
                {
                    var segment = new ArraySegment<byte>(buffer);
                    WebSocketReceiveResult result;

                    using var ms = new MemoryStream();

                    do
                    {
                        result = await _ws.ReceiveAsync(segment, token);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "client closing", token);
                            _connected = false;
                            _mainThreadActions.Enqueue(() => Disconnected?.Invoke());
                            return;
                        }

                        ms.Write(segment.Array!, segment.Offset, result.Count);
                    }
                    while (!result.EndOfMessage);

                    var raw = Encoding.UTF8.GetString(ms.ToArray());
                    Debug.Log($"[PlanarWarWsClient] RAW <- {raw}");

                    try
                    {
                        var parsed = JObject.Parse(raw);
                        _messageQueue.Enqueue(parsed);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[PlanarWarWsClient] JSON parse failed: {ex}\nPayload: {raw}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                Debug.LogError($"[PlanarWarWsClient] Receive loop failed: {ex}");
            }
            finally
            {
                _connected = false;
                _connecting = false;
                _mainThreadActions.Enqueue(() => Disconnected?.Invoke());
            }
        }
    }
}
