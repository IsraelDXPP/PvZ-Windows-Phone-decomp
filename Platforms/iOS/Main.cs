using System;
using Foundation;
using UIKit;

namespace LAWN;

[Register("AppDelegate")]
class Program : UIApplicationDelegate
{
    private static Sexy.Main game;

    internal static void RunGame()
    {
        game = new Sexy.Main();
        game.Run();
    }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static void Main(string[] args)
    {
        UIApplication.Main(args, null, typeof(Program));
    }

    public override void FinishedLaunching(UIApplication app)
    {
        RunGame();
    }
}
