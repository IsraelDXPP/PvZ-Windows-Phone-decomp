using System;
using Sexy;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Directorio de trabajo = donde está el exe (para que encuentre Content/)
        try { Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory; } catch { }
        using (var game = new Sexy.Main())
        {
            game.Run();
        }
    }
}
