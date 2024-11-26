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
			Raylib.SetWindowState(ConfigFlags.ResizableWindow);
            var browser = new RaylibSteamHTMLSurface();
            browser.OnEnable();

			Raylib.SetTraceLogLevel(TraceLogLevel.Warning);

			int lastX = Raylib.GetScreenWidth(), lastY = Raylib.GetScreenHeight();
            while (!Raylib.WindowShouldClose()) {
				int x = Raylib.GetScreenWidth(), y = Raylib.GetScreenHeight();
				if(x != lastX || y != lastY) {
					lastX = x;
					lastY = y;
					browser.Resize(x, y);
				}
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
