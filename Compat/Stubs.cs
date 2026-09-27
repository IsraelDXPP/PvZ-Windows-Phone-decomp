// Stubs de compatibilidad WP7/XNA -> Desktop (MonoGame).
// Solo para que compile en PC. Sin funcionalidad Live/Xbox.
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Framework.GamerServices
{
    public class GameUpdateRequiredException : Exception
    {
        public GameUpdateRequiredException() { }
        public GameUpdateRequiredException(string msg) : base(msg) { }
    }
    public enum MessageBoxIcon { None, Error, Warning, Alert }

    public class GamerServicesComponent : GameComponent
    {
        public GamerServicesComponent(Game game) : base(game) { }
    }

    public class Gamer
    {
        public string Gamertag { get; set; } = "Player";
        public static SignedGamerCollection SignedInGamers { get; } = new SignedGamerCollection();
        public IAsyncResult BeginGetProfile(AsyncCallback cb, object state) => new StubAsyncResult(state);
        public GamerProfile EndGetProfile(IAsyncResult r) => new GamerProfile();
    }

    public class SignedGamerCollection
    {
        private readonly List<SignedInGamer> _list = new List<SignedInGamer>();
        public int Count => _list.Count;
        public SignedInGamer this[PlayerIndex i] => _list.Count > (int)i ? _list[(int)i] : null;
    }

    public class SignedInGamer : Gamer
    {
        public static event EventHandler<SignedInEventArgs> SignedIn;
        public LeaderboardWriter LeaderboardWriter { get; } = new LeaderboardWriter();
        public IAsyncResult BeginGetAchievements(AsyncCallback cb, object state) => new StubAsyncResult(state);
        public AchievementCollection EndGetAchievements(IAsyncResult r) => new AchievementCollection();
        public IAsyncResult BeginAwardAchievement(string key, AsyncCallback cb, object state) => new StubAsyncResult(state);
        public void EndAwardAchievement(IAsyncResult r) { }
    }

    public class SignedInEventArgs : EventArgs
    {
        public SignedInGamer Gamer { get; set; }
    }

    public class Achievement
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int GamerScore { get; set; }
        public string Key { get; set; } = "";
        public bool IsEarned { get; set; }
        public Stream GetPicture() => Stream.Null;
    }

    public class AchievementCollection : List<Achievement> { }

    public class GamerProfile : IDisposable
    {
        public Stream GetGamerPicture() => Stream.Null;
        public void Dispose() { }
    }

    public enum LeaderboardKey { BestScoreLifeTime }

    public class LeaderboardIdentity
    {
        public static LeaderboardIdentity Create(LeaderboardKey key, int number) => new LeaderboardIdentity();
    }

    public class LeaderboardEntry
    {
        public Gamer Gamer { get; set; }
        public long Rating { get; set; }
    }

    public class LeaderboardEntryCollection : List<LeaderboardEntry> { }

    public class LeaderboardReader
    {
        public LeaderboardEntryCollection Entries { get; } = new LeaderboardEntryCollection();
        public int PageStart { get; set; }
        public int TotalLeaderboardSize { get; set; }
        public bool CanPageUp => false;
        public bool CanPageDown => false;
        public static IAsyncResult BeginRead(LeaderboardIdentity id, Gamer gamer, int pageSize, AsyncCallback cb, object state) => new StubAsyncResult(state);
        public static LeaderboardReader EndRead(IAsyncResult r) => new LeaderboardReader();
        public IAsyncResult BeginPageUp(AsyncCallback cb, object state) => new StubAsyncResult(state);
        public IAsyncResult BeginPageDown(AsyncCallback cb, object state) => new StubAsyncResult(state);
    }

    public class LeaderboardWriter
    {
        public LeaderboardEntry GetLeaderboard(LeaderboardIdentity id) => new LeaderboardEntry();
    }

    public static class Guide
    {
        public static bool IsTrialMode => false;
        public static bool IsVisible => false;
        public static bool SimulateTrialMode { get; set; }
        public static void ShowMarketplace(PlayerIndex player) { }
        public static IAsyncResult BeginShowMessageBox(string title, string text, string[] buttons, int focus, MessageBoxIcon icon, AsyncCallback cb, object state) => new StubAsyncResult(state);
        public static int? EndShowMessageBox(IAsyncResult r) => null;
        public static IAsyncResult BeginShowKeyboardInput(PlayerIndex player, string title, string desc, string def, AsyncCallback cb, object state) => new StubAsyncResult(state);
        public static string EndShowKeyboardInput(IAsyncResult r) => null;
    }

    internal class StubAsyncResult : IAsyncResult
    {
        public StubAsyncResult(object state) { AsyncState = state; }
        public object AsyncState { get; }
        public System.Threading.WaitHandle AsyncWaitHandle => null;
        public bool CompletedSynchronously => true;
        public bool IsCompleted => true;
    }
}

namespace Microsoft.Phone.Shell
{
    public enum IdleDetectionMode { Disabled = 0, Enabled = 1 }

    public class LaunchingEventArgs : EventArgs { }
    public class ActivatedEventArgs : EventArgs { }
    public class DeactivatedEventArgs : EventArgs { }
    public class ClosingEventArgs : EventArgs { }

    public sealed class PhoneApplicationService
    {
        private static readonly PhoneApplicationService _current = new PhoneApplicationService();
        public static PhoneApplicationService Current => _current;
        public IdleDetectionMode ApplicationIdleDetectionMode { get; set; }
        public IdleDetectionMode UserIdleDetectionMode { get; set; }
        public IDictionary<string, object> State { get; } = new Dictionary<string, object>();
        public event EventHandler<LaunchingEventArgs> Launching;
        public event EventHandler<ActivatedEventArgs> Activated;
        public event EventHandler<DeactivatedEventArgs> Deactivated;
        public event EventHandler<ClosingEventArgs> Closing;
    }
}

namespace Microsoft.Phone.Info
{
    public static class DeviceExtendedProperties
    {
        public static bool TryGetValue(string name, ref object value)
        {
            // 512MB para no activar low-mem
            value = (long)512 * 1024 * 1024;
            return true;
        }
    }
}

namespace Microsoft.Devices
{
    public class VibrateController
    {
        public static VibrateController Default { get; } = new VibrateController();
        public void Start(TimeSpan duration) { }
    }
}

namespace Microsoft.Phone.Tasks
{
    public enum MediaPlaybackControls { None = 0, All = 31 }
    public enum MediaLocationType { Data = 0, Install = 1 }
    public enum MarketplaceContentType { Applications = 0, Music = 1 }

    public class MediaPlayerLauncher
    {
        public Uri Media { get; set; }
        public MediaLocationType Location { get; set; }
        public MediaPlaybackControls Controls { get; set; }
        public void Show() { }
    }

    public class MarketplaceDetailTask
    {
        public MarketplaceContentType ContentType { get; set; }
        public void Show() { }
    }
}
