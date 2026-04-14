// Assets/Runtime/PlanarWar/Network/Routing/PlanarWarMessageRouter.cs
using Newtonsoft.Json.Linq;
using System;
using System.Xml.Linq;
using UnityEngine;

namespace PlanarWar.Client.Network
{
    public sealed class PlanarWarMessageRouter : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private PlanarWarWsClient networkClient;

        public event Action<JObject> HelloAck;
        public event Action<JObject> Welcome;
        public event Action<JObject> Chat;
        public event Action<JObject> MudResult;
        public event Action<JObject> WhereAmIResult;
        public event Action<JObject> RoomJoined;
        public event Action<JObject> RoomLeft;
        public event Action<JObject> Error;
        public event Action<string, JObject> AnyOp;

        private void OnEnable()
        {
            if (networkClient == null)
            {
                Debug.LogError("[PlanarWarMessageRouter] PlanarWarWsClient ref is null.");
                return;
            }

            networkClient.MessageReceived += Route;
        }

        private void OnDisable()
        {
            if (networkClient != null)
            {
                networkClient.MessageReceived -= Route;
            }
        }

        public void Route(JObject msg)
        {
            if (msg == null)
            {
                Debug.LogWarning("[PlanarWarMessageRouter] Null message.");
                return;
            }

            var op = msg["op"]?.Read<string>();
            if (string.IsNullOrWhiteSpace(op))
            {
                Debug.LogWarning($"[PlanarWarMessageRouter] Invalid message, no op: {msg}");
                return;
            }

            var payload = msg["payload"] as JObject ?? new JObject();

            Debug.Log($"[PlanarWarMessageRouter] op={op}");
            AnyOp?.Invoke(op, payload);

            switch (op)
            {
                case "hello_ack":
                    HelloAck?.Invoke(payload);
                    break;

                case "welcome":
                    Welcome?.Invoke(payload);
                    break;

                case "chat":
                    Chat?.Invoke(payload);
                    break;

                case "mud_result":
                    MudResult?.Invoke(payload);
                    break;

                case "whereami_result":
                    WhereAmIResult?.Invoke(payload);
                    break;

                case "room_joined":
                    RoomJoined?.Invoke(payload);
                    break;

                case "room_left":
                    RoomLeft?.Invoke(payload);
                    break;

                case "error":
                    Error?.Invoke(payload);
                    break;

                default:
                    // Keep quiet for now; raw op still visible through AnyOp and the WS client.
                    break;
            }
        }
    }
}