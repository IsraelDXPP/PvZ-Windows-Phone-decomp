using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;

namespace LAWN;

[Activity(
    Label = "@string/app_name",
    Icon = "@mipmap/ic_launcher",
    RoundIcon = "@mipmap/ic_launcher_round",
    MainLauncher = true,
    AlwaysRetainTaskState = true,
    LaunchMode = LaunchMode.SingleInstance,
    ScreenOrientation = ScreenOrientation.Landscape,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.ScreenLayout | ConfigChanges.UiMode
)]
public class MainActivity : AndroidGameActivity
{
    private Sexy.Main _game;
    private View _view;

    protected override void OnCreate(Bundle bundle)
    {
        global::Android.Util.Log.Info("PVZ", "MainActivity.OnCreate");
        base.OnCreate(bundle);

        _game = new Sexy.Main();
        global::Android.Util.Log.Info("PVZ", "Game created");
        _view = _game.Services.GetService(typeof(View)) as View;

        SetContentView(_view);
        global::Android.Util.Log.Info("PVZ", "Game.Run()");
        _game.Run();
    }
}
