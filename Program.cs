using System;
using Sexy;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Directorio de trabajo = donde está el exe (para que encuentre Content/)
        try { Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory; } catch { }
        // Perfil 4:3 (800x600, PvZ-Quality-Enhanced): LAWN.exe 800x600
        foreach (string a in args)
        {
            if (a == "800x600" || a == "4:3" || a == "--800x600")
            {
                Sexy.Main.Want800x600 = true;
            }
        }
        using (var game = new Sexy.Main())
        {
            game.Run();
        }
    }
}
