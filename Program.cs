using Raylib_cs;
using Steamworks;
using System.Diagnostics;

namespace TestingSteamworksCreateBrowser
{

    internal class Program
    {
        static void Main(string[] args) {
            SteamAPI.Init();
            Raylib.InitWindow(1600, 900, "ISteamHTMLSurface/CreateBrowser");

            var browser = new RaylibSteamHTMLSurface();
            browser.OnEnable();

            while (!Raylib.WindowShouldClose()) {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);
                SteamAPI.RunCallbacks();
                browser.Render();
                Raylib.DrawFPS(8, 8);
                Raylib.EndDrawing();
            }
            SteamAPI.Shutdown();
        }
    }
}
