using System;
using System.Collections.Generic;
using System.Linq;

namespace PlanarWar.Client.Core
{
    [Serializable]
    public sealed class SessionState
    {
        public sealed class ChatLine
        {
            public string ChannelId { get; set; } = "room";
            public string ChannelLabel { get; set; } = "Room";
            public string From { get; set; } = string.Empty;
            public string Text { get; set; } = string.Empty;
            public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

            public string ToDisplayText()
            {
                var time = TimestampUtc.ToLocalTime().ToString("HH:mm");
                var speaker = string.IsNullOrWhiteSpace(From) ? string.Empty : $"{From}: ";
                return $"[{time}] {speaker}{Text}";
            }
        }

        public event Action Changed;

        public string WsUrl { get; private set; } = "-";
        public string HttpBaseUrl { get; private set; } = "-";
        public string SessionId { get; private set; } = "-";
        public string DisplayName { get; private set; } = "Anon";
        public string ShardId { get; private set; } = "-";
        public string RoomId { get; private set; } = "-";
        public bool HasJoinedChatRoom => !string.IsNullOrWhiteSpace(RoomId) && RoomId != "-" && RoomId != "(unattached)";
        public bool IsConnected { get; private set; }
        public string LastInboundOp { get; private set; } = "-";
        public string LastError { get; private set; } = "-";
        public string LastChatLine { get; private set; } = "No chat yet.";
        public string BearerToken { get; private set; } = string.Empty;
        public bool IsAuthenticated => !string.IsNullOrWhiteSpace(BearerToken);
        public string LoginStatus { get; private set; } = "Demo mode active.";
        public string ActiveChatChannel { get; private set; } = "all";
        public IReadOnlyList<ChatLine> ChatLines => _chatLines;

        private readonly List<ChatLine> _chatLines = new();
        private const int MaxChatLines = 24;

        public void SetUrls(string wsUrl, string httpBaseUrl)
        {
            WsUrl = string.IsNullOrWhiteSpace(wsUrl) ? "-" : wsUrl.Trim();
            HttpBaseUrl = string.IsNullOrWhiteSpace(httpBaseUrl) ? "-" : httpBaseUrl.Trim();
            NotifyChanged();
        }

        public void SetConnectionState(bool isConnected)
        {
            IsConnected = isConnected;
            if (!isConnected)
            {
                RoomId = "-";
            }
            NotifyChanged();
        }

        public void SetLastInboundOp(string op)
        {
            LastInboundOp = string.IsNullOrWhiteSpace(op) ? "-" : op.Trim();
            NotifyChanged();
        }

        public void SetLastError(string error)
        {
            LastError = string.IsNullOrWhiteSpace(error) ? "-" : error.Trim();
            NotifyChanged();
        }

        public void ApplyWsError(string code, string detail = null)
        {
            var safeCode = string.IsNullOrWhiteSpace(code) ? "ws_error" : code.Trim();
            LastError = string.IsNullOrWhiteSpace(detail) ? safeCode : $"{safeCode}: {detail.Trim()}";
            ApplySystemNotice($"WS error: {safeCode}.");
            NotifyChanged();
        }

        public void ApplyWelcome(string sessionId, string displayName, string shardId = null)
        {
            if (!string.IsNullOrWhiteSpace(sessionId)) SessionId = sessionId.Trim();
            if (!string.IsNullOrWhiteSpace(displayName)) DisplayName = displayName.Trim();
            if (!string.IsNullOrWhiteSpace(shardId)) ShardId = shardId.Trim();
            NotifyChanged();
        }

        public void ApplyHelloAck(string shardId)
        {
            if (!string.IsNullOrWhiteSpace(shardId))
            {
                ShardId = shardId.Trim();
            }
            NotifyChanged();
        }

        public void ApplyWhereAmI(string shardId, string roomId)
        {
            if (!string.IsNullOrWhiteSpace(shardId)) ShardId = shardId.Trim();
            RoomId = string.IsNullOrWhiteSpace(roomId) ? "(unattached)" : roomId.Trim();
            NotifyChanged();
        }

        public void ApplyRoomJoined(string roomId)
        {
            RoomId = string.IsNullOrWhiteSpace(roomId) ? "lobby" : roomId.Trim();
            ApplySystemNotice($"Joined chat room: {RoomId}.");
            NotifyChanged();
        }

        public void ApplyRoomLeft()
        {
            RoomId = "(unattached)";
            ApplySystemNotice("Left chat room.");
            NotifyChanged();
        }

        public void ApplyChat(string channelId, string channelLabel, string from, string text)
        {
            var safeChannelId = string.IsNullOrWhiteSpace(channelId) ? "room" : channelId.Trim();
            var safeChannelLabel = string.IsNullOrWhiteSpace(channelLabel) ? Cap(safeChannelId) : channelLabel.Trim();
            var safeText = string.IsNullOrWhiteSpace(text) ? "(empty)" : text.Trim();
            var safeFrom = string.IsNullOrWhiteSpace(from) ? string.Empty : from.Trim();

            var line = new ChatLine
            {
                ChannelId = safeChannelId,
                ChannelLabel = safeChannelLabel,
                From = safeFrom,
                Text = safeText,
                TimestampUtc = DateTime.UtcNow
            };

            _chatLines.Add(line);
            if (_chatLines.Count > MaxChatLines)
            {
                _chatLines.RemoveAt(0);
            }

            LastChatLine = $"{safeChannelLabel}: {(string.IsNullOrWhiteSpace(safeFrom) ? safeText : $"{safeFrom}: {safeText}")}";
            NotifyChanged();
        }

        public void SetActiveChatChannel(string channelId)
        {
            var normalized = string.IsNullOrWhiteSpace(channelId) ? "all" : channelId.Trim().ToLowerInvariant();
            ActiveChatChannel = normalized;
            NotifyChanged();
        }

        public IReadOnlyList<ChatLine> GetVisibleChatLines()
        {
            if (_chatLines.Count == 0)
            {
                return Array.Empty<ChatLine>();
            }

            IEnumerable<ChatLine> source = _chatLines;
            if (!string.Equals(ActiveChatChannel, "all", StringComparison.OrdinalIgnoreCase))
            {
                source = source.Where(line => string.Equals(line.ChannelId, ActiveChatChannel, StringComparison.OrdinalIgnoreCase));
            }

            return source.TakeLast(6).ToArray();
        }


        public void ApplySystemNotice(string text)
        {
            var safeText = string.IsNullOrWhiteSpace(text) ? "(empty)" : text.Trim();
            var line = new ChatLine
            {
                ChannelId = "system",
                ChannelLabel = "System",
                From = string.Empty,
                Text = safeText,
                TimestampUtc = DateTime.UtcNow
            };

            _chatLines.Add(line);
            if (_chatLines.Count > MaxChatLines)
            {
                _chatLines.RemoveAt(0);
            }

            LastChatLine = $"System: {safeText}";
            NotifyChanged();
        }

        public void ApplyLogin(string token, string displayName, string sessionStatus = null)
        {
            BearerToken = token?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(displayName))
            {
                DisplayName = displayName.Trim();
            }

            LoginStatus = string.IsNullOrWhiteSpace(BearerToken)
                ? "Demo mode active."
                : (string.IsNullOrWhiteSpace(sessionStatus) ? $"Authenticated as {DisplayName}." : sessionStatus.Trim());
            ApplySystemNotice(string.IsNullOrWhiteSpace(BearerToken) ? "Demo summary mode active." : $"Signed in as {DisplayName}.");
            NotifyChanged();
        }

        public void ClearLogin()
        {
            BearerToken = string.Empty;
            LoginStatus = "Demo mode active.";
            SessionId = "-";
            DisplayName = "Anon";
            NotifyChanged();
        }

        public void SetLoginStatus(string status)
        {
            LoginStatus = string.IsNullOrWhiteSpace(status) ? LoginStatus : status.Trim();
            NotifyChanged();
        }

        private void NotifyChanged()
        {
            Changed?.Invoke();
        }

        private static string Cap(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value ?? string.Empty;
            return char.ToUpperInvariant(value[0]) + value[1..];
        }
    }
}
