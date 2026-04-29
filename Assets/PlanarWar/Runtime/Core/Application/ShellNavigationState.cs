using System;

namespace PlanarWar.Client.Core.Application
{
    public enum ShellScreen
    {
        Summary,
        City,
        BlackMarket,
        Heroes,
        Social,
        Guide
    }

    public sealed class ShellNavigationState
    {
        public event Action Changed;
        public ShellScreen ActiveScreen { get; private set; } = ShellScreen.Summary;

        public void SetActive(ShellScreen screen)
        {
            if (ActiveScreen == screen) return;
            ActiveScreen = screen;
            Changed?.Invoke();
        }
    }
}
