using Raylib_cs;
using Steamworks;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using Color = Raylib_cs.Color;
using Rectangle = Raylib_cs.Rectangle;

public class RaylibSteamHTMLSurface
{
    const int WidthOffset = 400;
    const int HeightOffset = 100;

    private bool m_Init;
    private HHTMLBrowser m_HHTMLBrowser;
    private string m_URL;
    private Image? m_Texture;
    public bool IsReady => m_Texture.HasValue && Raylib.IsImageReady(m_Texture.Value);
    public Image Image => m_Texture ?? throw new Exception();
    private uint m_Width = 1600;
    private uint m_Height = 900;
    private bool m_CanGoBack;
    private bool m_CanGoForward;
    private Rectangle m_Rect;
    private Vector2 m_LastMousePos;
    private uint m_VerticalScrollMax;
    private uint m_VeritcalScrollCurrent;
    private uint m_HorizontalScrollMax;
    private uint m_HorizontalScrollCurrent;
    private bool m_SetKeyFocus;
    private string m_Find;
    private bool m_CurrentlyInFind;
    private float m_ScaleFactor;
    private bool m_BackgroundMode;

    protected Callback<HTML_NeedsPaint_t> m_HTML_NeedsPaint;
    protected Callback<HTML_StartRequest_t> m_HTML_StartRequest;
    protected Callback<HTML_CloseBrowser_t> m_HTML_CloseBrowser;
    protected Callback<HTML_URLChanged_t> m_HTML_URLChanged;
    protected Callback<HTML_FinishedRequest_t> m_HTML_FinishedRequest;
    protected Callback<HTML_OpenLinkInNewTab_t> m_HTML_OpenLinkInNewTab;
    protected Callback<HTML_ChangedTitle_t> m_HTML_ChangedTitle;
    protected Callback<HTML_SearchResults_t> m_HTML_SearchResults;
    protected Callback<HTML_CanGoBackAndForward_t> m_HTML_CanGoBackAndForward;
    protected Callback<HTML_HorizontalScroll_t> m_HTML_HorizontalScroll;
    protected Callback<HTML_VerticalScroll_t> m_HTML_VerticalScroll;
    protected Callback<HTML_LinkAtPosition_t> m_HTML_LinkAtPosition;
    protected Callback<HTML_JSAlert_t> m_HTML_JSAlert;
    protected Callback<HTML_JSConfirm_t> m_HTML_JSConfirm;
    protected Callback<HTML_FileOpenDialog_t> m_HTML_FileOpenDialog;
    protected Callback<HTML_NewWindow_t> m_HTML_NewWindow;
    protected Callback<HTML_SetCursor_t> m_HTML_SetCursor;
    protected Callback<HTML_StatusText_t> m_HTML_StatusText;
    protected Callback<HTML_ShowToolTip_t> m_HTML_ShowToolTip;
    protected Callback<HTML_UpdateToolTip_t> m_HTML_UpdateToolTip;
    protected Callback<HTML_HideToolTip_t> m_HTML_HideToolTip;
    protected Callback<HTML_BrowserRestarted_t> m_HTML_BrowserRestarted;

    private CallResult<HTML_BrowserReady_t> OnHTML_BrowserReadyCallResult;

    public void OnEnable() {
        m_HHTMLBrowser = HHTMLBrowser.Invalid;
        m_URL = "https://steamcommunity.com/";
        m_Texture = null;
        m_Find = "Steamworks";
        m_CurrentlyInFind = false;
        m_ScaleFactor = 0f;
        m_BackgroundMode = false;

        m_Init = SteamHTMLSurface.Init();
        Console.WriteLine("SteamHTMLSurface.Init() : " + m_Init);

        m_HTML_NeedsPaint = Callback<HTML_NeedsPaint_t>.Create(OnHTML_NeedsPaint);
        m_HTML_StartRequest = Callback<HTML_StartRequest_t>.Create(OnHTML_StartRequest);
        m_HTML_CloseBrowser = Callback<HTML_CloseBrowser_t>.Create(OnHTML_CloseBrowser);
        m_HTML_URLChanged = Callback<HTML_URLChanged_t>.Create(OnHTML_URLChanged);
        m_HTML_FinishedRequest = Callback<HTML_FinishedRequest_t>.Create(OnHTML_FinishedRequest);
        m_HTML_OpenLinkInNewTab = Callback<HTML_OpenLinkInNewTab_t>.Create(OnHTML_OpenLinkInNewTab);
        m_HTML_ChangedTitle = Callback<HTML_ChangedTitle_t>.Create(OnHTML_ChangedTitle);
        m_HTML_SearchResults = Callback<HTML_SearchResults_t>.Create(OnHTML_SearchResults);
        m_HTML_CanGoBackAndForward = Callback<HTML_CanGoBackAndForward_t>.Create(OnHTML_CanGoBackAndForward);
        m_HTML_HorizontalScroll = Callback<HTML_HorizontalScroll_t>.Create(OnHTML_HorizontalScroll);
        m_HTML_VerticalScroll = Callback<HTML_VerticalScroll_t>.Create(OnHTML_VerticalScroll);
        m_HTML_LinkAtPosition = Callback<HTML_LinkAtPosition_t>.Create(OnHTML_LinkAtPosition);
        m_HTML_JSAlert = Callback<HTML_JSAlert_t>.Create(OnHTML_JSAlert);
        m_HTML_JSConfirm = Callback<HTML_JSConfirm_t>.Create(OnHTML_JSConfirm);
        m_HTML_FileOpenDialog = Callback<HTML_FileOpenDialog_t>.Create(OnHTML_FileOpenDialog);
        m_HTML_NewWindow = Callback<HTML_NewWindow_t>.Create(OnHTML_NewWindow);
        m_HTML_SetCursor = Callback<HTML_SetCursor_t>.Create(OnHTML_SetCursor);
        m_HTML_StatusText = Callback<HTML_StatusText_t>.Create(OnHTML_StatusText);
        m_HTML_ShowToolTip = Callback<HTML_ShowToolTip_t>.Create(OnHTML_ShowToolTip);
        m_HTML_UpdateToolTip = Callback<HTML_UpdateToolTip_t>.Create(OnHTML_UpdateToolTip);
        m_HTML_HideToolTip = Callback<HTML_HideToolTip_t>.Create(OnHTML_HideToolTip);
        m_HTML_BrowserRestarted = Callback<HTML_BrowserRestarted_t>.Create(OnHTML_BrowserRestarted);

        OnHTML_BrowserReadyCallResult = CallResult<HTML_BrowserReady_t>.Create(OnHTML_BrowserReady);
        SteamAPICall_t handle = SteamHTMLSurface.CreateBrowser("SpaceWars Test", null);
        OnHTML_BrowserReadyCallResult.Set(handle);
    }

    public void OnDisable() {
        RemoveBrowser();
        SteamHTMLSurface.Shutdown();
    }

    void RemoveBrowser() {
        if (m_HHTMLBrowser != HHTMLBrowser.Invalid) {
            Console.WriteLine("SteamHTMLSurface.RemoveBrowser(" + m_HHTMLBrowser + ")");
            SteamHTMLSurface.RemoveBrowser(m_HHTMLBrowser);
            m_HHTMLBrowser = HHTMLBrowser.Invalid;
        }
        m_Texture = null;
    }


    void OnHTML_BrowserReady(HTML_BrowserReady_t pCallback, bool bIOFailure) {
        //Console.WriteLine("[" + HTML_BrowserReady_t.k_iCallback + " - HTML_BrowserReady] - " + pCallback.unBrowserHandle);

        m_HHTMLBrowser = pCallback.unBrowserHandle;

		Resize((int)m_Width, (int)m_Height);
        SteamHTMLSurface.LoadURL(m_HHTMLBrowser, m_URL, null);
        //SteamHTMLSurface.OpenDeveloperTools(m_HHTMLBrowser);
    }
	public void Resize(int w, int h) {
		m_Width = (uint)w;
		m_Height = (uint)h;
		SteamHTMLSurface.SetSize(m_HHTMLBrowser, m_Width, m_Height);
	}

    private bool __dirtyTex = false;
    private Texture2D __tex;
    private Vector2 lastMousePos = Vector2.Zero;
    public void Render() {
        if (IsReady && __dirtyTex) {
            if (Raylib.IsTextureReady(__tex))
                Raylib.UnloadTexture(__tex);
            __tex = Raylib.LoadTextureFromImage(Image);
            __dirtyTex = false;
        }

        Raylib.DrawTexture(__tex, 0, 0, Color.White);

        Vector2 mousePos = Raylib.GetMousePosition();
        if(mousePos != lastMousePos) {
            SteamHTMLSurface.MouseMove(m_HHTMLBrowser, (int)mousePos.X, (int)mousePos.Y);
        }

		if (Raylib.IsMouseButtonPressed(MouseButton.Left)) SteamHTMLSurface.MouseDown(m_HHTMLBrowser, EHTMLMouseButton.eHTMLMouseButton_Left);
		if (Raylib.IsMouseButtonPressed(MouseButton.Middle)) SteamHTMLSurface.MouseDown(m_HHTMLBrowser, EHTMLMouseButton.eHTMLMouseButton_Middle);
		if (Raylib.IsMouseButtonPressed(MouseButton.Right)) SteamHTMLSurface.MouseDown(m_HHTMLBrowser, EHTMLMouseButton.eHTMLMouseButton_Right);
		
		if (Raylib.IsMouseButtonReleased(MouseButton.Left)) SteamHTMLSurface.MouseUp(m_HHTMLBrowser, EHTMLMouseButton.eHTMLMouseButton_Left);
		if (Raylib.IsMouseButtonReleased(MouseButton.Middle)) SteamHTMLSurface.MouseUp(m_HHTMLBrowser, EHTMLMouseButton.eHTMLMouseButton_Middle);
		if (Raylib.IsMouseButtonReleased(MouseButton.Right)) SteamHTMLSurface.MouseUp(m_HHTMLBrowser, EHTMLMouseButton.eHTMLMouseButton_Right);

		if (Raylib.GetMouseWheelMoveV().Y != 0) SteamHTMLSurface.MouseWheel(m_HHTMLBrowser, (int)Raylib.GetMouseWheelMoveV().Y * 50);
		while (true) {
            int inp = Raylib.GetKeyPressed();
            if (inp == 0)
                break;
            if (inp == 340) continue;
            if (inp == 341) continue;
            if (inp == 342) continue;
            if (inp == 344) continue;
            if (inp == 345) continue;
            if (inp == 346) continue;

            EHTMLKeyModifiers modifiers = EHTMLKeyModifiers.k_eHTMLKeyModifier_None;

            if (Raylib.IsKeyDown(KeyboardKey.LeftAlt) || Raylib.IsKeyDown(KeyboardKey.RightAlt)) 
                modifiers |= EHTMLKeyModifiers.k_eHTMLKeyModifier_AltDown;
            if (Raylib.IsKeyDown(KeyboardKey.LeftShift) || Raylib.IsKeyDown(KeyboardKey.RightShift)) 
                modifiers |= EHTMLKeyModifiers.k_eHTMLKeyModifier_ShiftDown;
            if (Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.IsKeyDown(KeyboardKey.RightControl)) 
                modifiers |= EHTMLKeyModifiers.k_eHTMLKeyModifier_CtrlDown;

            SteamHTMLSurface.KeyDown(m_HHTMLBrowser, (uint)inp, modifiers);
        }
    }

	unsafe byte* pDt;
	uint lastDataSize = 0;

    unsafe void OnHTML_NeedsPaint(HTML_NeedsPaint_t pCallback) {
        //Console.WriteLine("[" + HTML_NeedsPaint_t.k_iCallback + " - HTML_NeedsPaint] - " + pCallback.unBrowserHandle + " -- " + pCallback.pBGRA + " -- " + pCallback.unWide + " -- " + pCallback.unTall + " -- " + pCallback.unUpdateX + " -- " + pCallback.unUpdateY + " -- " + pCallback.unUpdateWide + " -- " + pCallback.unUpdateTall + " -- " + pCallback.unScrollX + " -- " + pCallback.unScrollY + " -- " + pCallback.flPageScale + " -- " + pCallback.unPageSerial);

        int dataSize = (int)(pCallback.unWide * pCallback.unTall * 4);

		if (lastDataSize != dataSize) {
			if (m_Texture.HasValue) {
				Raylib.UnloadImage(m_Texture.Value);
			}
			pDt = Raylib.New<byte>((uint)dataSize);

			m_Texture = new() {
				Width = (int)pCallback.unWide,
				Height = (int)pCallback.unTall,
				Mipmaps = 1,
				Format = PixelFormat.UncompressedR8G8B8A8,
				Data = pDt
			};

			lastDataSize = (uint)dataSize;
		}

		byte* bytes = (byte*)pCallback.pBGRA;
		for (int i = 0; i < dataSize; i+=4) {
            pDt[i] = bytes[i + 2];
            pDt[i + 1] = bytes[i + 1];
            pDt[i + 2] = bytes[i];
            pDt[i + 3] = bytes[i + 3];
        }

        __dirtyTex = true;
    }

    void OnHTML_StartRequest(HTML_StartRequest_t pCallback) {
        //Console.WriteLine("[" + HTML_StartRequest_t.k_iCallback + " - HTML_StartRequest] - " + pCallback.unBrowserHandle + " -- " + pCallback.pchURL + " -- " + pCallback.pchTarget + " -- " + pCallback.pchPostData + " -- " + pCallback.bIsRedirect);

        SteamHTMLSurface.AllowStartRequest(pCallback.unBrowserHandle, true);
        SteamHTMLSurface.AddHeader(m_HHTMLBrowser, "Cookie", "");
        Console.WriteLine("SteamHTMLSurface.AllowStartRequest(pCallback.unBrowserHandle, true)");
    }

    void OnHTML_CloseBrowser(HTML_CloseBrowser_t pCallback) {
        //Console.WriteLine("[" + HTML_CloseBrowser_t.k_iCallback + " - HTML_CloseBrowser] - " + pCallback.unBrowserHandle);

        m_HHTMLBrowser = HHTMLBrowser.Invalid;
    }

    void OnHTML_URLChanged(HTML_URLChanged_t pCallback) {
        //Console.WriteLine("[" + HTML_URLChanged_t.k_iCallback + " - HTML_URLChanged] - " + pCallback.unBrowserHandle + " -- " + pCallback.pchURL + " -- " + pCallback.pchPostData + " -- " + pCallback.bIsRedirect + " -- " + pCallback.pchPageTitle + " -- " + pCallback.bNewNavigation);
        SteamHTMLSurface.AddHeader(m_HHTMLBrowser, "Clear-Site-Data", "*");
    }

    void OnHTML_FinishedRequest(HTML_FinishedRequest_t pCallback) {
        //Console.WriteLine("[" + HTML_FinishedRequest_t.k_iCallback + " - HTML_FinishedRequest] - " + pCallback.unBrowserHandle + " -- " + pCallback.pchURL + " -- " + pCallback.pchPageTitle);
    }

    void OnHTML_OpenLinkInNewTab(HTML_OpenLinkInNewTab_t pCallback) {
        //Console.WriteLine("[" + HTML_OpenLinkInNewTab_t.k_iCallback + " - HTML_OpenLinkInNewTab] - " + pCallback.unBrowserHandle + " -- " + pCallback.pchURL);
    }

    void OnHTML_ChangedTitle(HTML_ChangedTitle_t pCallback) {
        //Console.WriteLine("[" + HTML_ChangedTitle_t.k_iCallback + " - HTML_ChangedTitle] - " + pCallback.unBrowserHandle + " -- " + pCallback.pchTitle);
    }

    void OnHTML_SearchResults(HTML_SearchResults_t pCallback) {
        //Console.WriteLine("[" + HTML_SearchResults_t.k_iCallback + " - HTML_SearchResults] - " + pCallback.unBrowserHandle + " -- " + pCallback.unResults + " -- " + pCallback.unCurrentMatch);
    }

    void OnHTML_CanGoBackAndForward(HTML_CanGoBackAndForward_t pCallback) {
        //Console.WriteLine("[" + HTML_CanGoBackAndForward_t.k_iCallback + " - HTML_CanGoBackAndForward] - " + pCallback.unBrowserHandle + " -- " + pCallback.bCanGoBack + " -- " + pCallback.bCanGoForward);

        m_CanGoBack = pCallback.bCanGoBack;
        m_CanGoForward = pCallback.bCanGoForward;
    }

    void OnHTML_HorizontalScroll(HTML_HorizontalScroll_t pCallback) {
        //Console.WriteLine("[" + HTML_HorizontalScroll_t.k_iCallback + " - HTML_HorizontalScroll] - " + pCallback.unBrowserHandle + " -- " + pCallback.unScrollMax + " -- " + pCallback.unScrollCurrent + " -- " + pCallback.flPageScale + " -- " + pCallback.bVisible + " -- " + pCallback.unPageSize);

        m_HorizontalScrollMax = pCallback.unScrollMax;
        m_HorizontalScrollCurrent = pCallback.unScrollCurrent;
    }

    void OnHTML_VerticalScroll(HTML_VerticalScroll_t pCallback) {
        //Console.WriteLine("[" + HTML_VerticalScroll_t.k_iCallback + " - HTML_VerticalScroll] - " + pCallback.unBrowserHandle + " -- " + pCallback.unScrollMax + " -- " + pCallback.unScrollCurrent + " -- " + pCallback.flPageScale + " -- " + pCallback.bVisible + " -- " + pCallback.unPageSize);

        m_VerticalScrollMax = pCallback.unScrollMax;
        m_VeritcalScrollCurrent = pCallback.unScrollCurrent;
    }

    void OnHTML_LinkAtPosition(HTML_LinkAtPosition_t pCallback) {
        //Console.WriteLine("[" + HTML_LinkAtPosition_t.k_iCallback + " - HTML_LinkAtPosition] - " + pCallback.unBrowserHandle + " -- " + pCallback.x + " -- " + pCallback.y + " -- " + pCallback.pchURL + " -- " + pCallback.bInput + " -- " + pCallback.bLiveLink);
    }

    void OnHTML_JSAlert(HTML_JSAlert_t pCallback) {
        //Console.WriteLine("[" + HTML_JSAlert_t.k_iCallback + " - HTML_JSAlert] - " + pCallback.unBrowserHandle + " -- " + pCallback.pchMessage);

        SteamHTMLSurface.JSDialogResponse(pCallback.unBrowserHandle, true);
        Console.WriteLine("SteamHTMLSurface.JSDialogResponse(pCallback.unBrowserHandle, true)");
    }

    void OnHTML_JSConfirm(HTML_JSConfirm_t pCallback) {
        //Console.WriteLine("[" + HTML_JSConfirm_t.k_iCallback + " - HTML_JSConfirm] - " + pCallback.unBrowserHandle + " -- " + pCallback.pchMessage);

        SteamHTMLSurface.JSDialogResponse(pCallback.unBrowserHandle, true);
        //Console.WriteLine("SteamHTMLSurface.JSDialogResponse(pCallback.unBrowserHandle, true)");
    }

    void OnHTML_FileOpenDialog(HTML_FileOpenDialog_t pCallback) {
        //Console.WriteLine("[" + HTML_FileOpenDialog_t.k_iCallback + " - HTML_FileOpenDialog] - " + pCallback.unBrowserHandle + " -- " + pCallback.pchTitle + " -- " + pCallback.pchInitialFile);

        // TODO: Valve has no example usage of this.
        SteamHTMLSurface.FileLoadDialogResponse(pCallback.unBrowserHandle, System.IntPtr.Zero);
    }

    void OnHTML_NewWindow(HTML_NewWindow_t pCallback) {
       // Console.WriteLine("[" + HTML_NewWindow_t.k_iCallback + " - HTML_NewWindow] - " + pCallback.unBrowserHandle + " -- " + pCallback.pchURL + " -- " + pCallback.unX + " -- " + pCallback.unY + " -- " + pCallback.unWide + " -- " + pCallback.unTall + " -- " + pCallback.unNewWindow_BrowserHandle_IGNORE);
    }

    void OnHTML_SetCursor(HTML_SetCursor_t pCallback) {
        //Console.WriteLine("[" + HTML_SetCursor_t.k_iCallback + " - HTML_SetCursor] - " + pCallback.unBrowserHandle + " -- " + pCallback.eMouseCursor);

        Raylib.SetMouseCursor(pCallback.eMouseCursor switch {
            1 => MouseCursor.Arrow,
            2 => MouseCursor.Arrow,
            20 => MouseCursor.PointingHand,
            3 => MouseCursor.IBeam,
            _ => MouseCursor.Arrow,
        });
    }

    void OnHTML_StatusText(HTML_StatusText_t pCallback) {
        //Console.WriteLine("[" + HTML_StatusText_t.k_iCallback + " - HTML_StatusText] - " + pCallback.unBrowserHandle + " -- " + pCallback.pchMsg);
    }

    void OnHTML_ShowToolTip(HTML_ShowToolTip_t pCallback) {
        //Console.WriteLine("[" + HTML_ShowToolTip_t.k_iCallback + " - HTML_ShowToolTip] - " + pCallback.unBrowserHandle + " -- " + pCallback.pchMsg);
    }

    void OnHTML_UpdateToolTip(HTML_UpdateToolTip_t pCallback) {
       // Console.WriteLine("[" + HTML_UpdateToolTip_t.k_iCallback + " - HTML_UpdateToolTip] - " + pCallback.unBrowserHandle + " -- " + pCallback.pchMsg);
    }

    void OnHTML_HideToolTip(HTML_HideToolTip_t pCallback) {
        //Console.WriteLine("[" + HTML_HideToolTip_t.k_iCallback + " - HTML_HideToolTip] - " + pCallback.unBrowserHandle);
    }

    void OnHTML_BrowserRestarted(HTML_BrowserRestarted_t pCallback) {
        //Console.WriteLine("[" + HTML_BrowserRestarted_t.k_iCallback + " - HTML_BrowserRestarted] - " + pCallback.unBrowserHandle + " -- " + pCallback.unOldBrowserHandle);
    }
}