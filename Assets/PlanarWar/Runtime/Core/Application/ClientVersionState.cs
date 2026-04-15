using System;
using UnityEngine;

using UnityEngine;

namespace PlanarWar.Client.Core.Application
{
    public sealed class ClientVersionState
    {
        public string BuildLabel =>
            string.IsNullOrWhiteSpace(UnityEngine.Application.version)
                ? "v0.0.0-local"
                : UnityEngine.Application.version;

        public string ChannelLabel =>
            UnityEngine.Application.isEditor ? "editor-local" : "live";

        public string UpdateStatus => "Update manifest not wired yet.";
        public string AuthorityHint => "Patch authority will come from the box manifest.";
    }
}
