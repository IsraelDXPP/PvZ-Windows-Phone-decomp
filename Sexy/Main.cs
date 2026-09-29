using System;
using System.Collections.Generic;
using System.Globalization;
using Lawn;
using Microsoft.Phone.Info;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Sexy;

public class Main : Game
{
	private static SexyTransform2D orientationTransform;

	private static UI_ORIENTATION orientationUsed;

	private static bool newOrientation;

	public static GamerServicesComponent GamerServicesComp;

	public static bool trialModeChecked = false;

	private static bool trialModeCachedValue = true;

	internal static Graphics graphics;

	private int mFrameCnt;

	private static int letterboxLogCount;

#if !ANDROID && !IOS
	private static readonly HashSet<Keys> previousKeys = new HashSet<Keys>();
#endif

	private static bool startedProfiler;

	private static bool wantToSuppressDraw;

	private GamePadState previousGamepadState = default(GamePadState);

	private MouseState previousMouseState;
	private bool mouseDown;
#if ANDROID
	private static int drawLogCount;
	private static int updateLogCount;
	private static int measureLogCount;
	private static int touchLogCount;
#endif

	// Letterbox: el juego siempre piensa en 800x480; en pantallas mayores
	// se centra un viewport 800x480. OJO Android: MonoGame recorta
	// ClientBounds con su propio aspect-fit (ResetClientBounds): con
	// Preferred 800x480 en display 1600x900 deja ClientBounds=(50,0,1500,900)
	// y ADEMAS resta ese origen (X,Y) a cada toque en
	// AndroidTouchEventManager.UpdateTouchPosition. Por eso el touch llegaba
	// 50px a la izquierda: hay que compensar ese origen.
	private int letterOffX;
	private int letterOffY;
	private int viewOffX;
	private int viewOffY;

	private void UpdateLetterbox()
	{
		int cbX = 0, cbY = 0, cbW = 0, cbH = 0;
		try
		{
			var cb = base.Window.ClientBounds;
			cbX = cb.X; cbY = cb.Y; cbW = cb.Width; cbH = cb.Height;
		}
		catch
		{
		}
		int ppw = 0;
		int pph = 0;
		try
		{
			var gd = GraphicsState.mGraphicsDeviceManager.GraphicsDevice;
			if (gd != null)
			{
				ppw = gd.PresentationParameters.BackBufferWidth;
				pph = gd.PresentationParameters.BackBufferHeight;
			}
		}
		catch
		{
		}
		// Fuente de tamano: ClientBounds si es valido, si no backbuffer.
		// (En Android ClientBounds trae el recorte aspect-fit de MonoGame;
		// en Windows coinciden con el backbuffer y daba igual.)
		int bbw = (cbW > 0) ? cbW : ppw;
		int bbh = (cbH > 0) ? cbH : pph;
		// Forzar el recalculo de la escala tactil de MonoGame en CADA frame.
		// MonoGame calcula _touchScale = Display / ClientBounds solo dentro
		// del setter de Display, y lo hace una vez (DeviceReset) cuando
		// ClientBounds aun puede ser el tamano inicial (ej. MuMu: 1600) y
		// luego ClientBounds se corrige al surface real (1500) sin que la
		// escala se recalcule -> queda 1500/1600=0.9375 baked y todo toque X
		// llega comprimido (~50px a la izquierda en 750). El setter SIEMPRE
		// recalcula aunque el valor sea el mismo, asi que asignar sin
		// condicion lo mantiene sincronizado (barato: 2 sets/frame).
		try
		{
			if (bbw > 0 && bbh > 0)
			{
				TouchPanel.DisplayWidth = bbw;
				TouchPanel.DisplayHeight = bbh;
			}
		}
		catch
		{
		}
		if (bbw <= 0) bbw = Constants.BOARD_WIDTH;
		if (bbh <= 0) bbh = Constants.BOARD_HEIGHT;
#if ANDROID
		if (measureLogCount < 3) { try
		{
			float msx = 1f, msy = 1f;
			var gg = GlobalStaticVars.g;
			if (gg != null) { msx = gg.mScaleX; msy = gg.mScaleY; }
			int tpdw = 0, tpdh = 0;
			try { tpdw = TouchPanel.DisplayWidth; tpdh = TouchPanel.DisplayHeight; } catch { }
			Android.Util.Log.Info("PVZ", $"letterbox bb={bbw}x{bbh} pp={ppw}x{pph} client={cbX},{cbY},{cbW}x{cbH} off={letterOffX},{letterOffY} view={viewOffX},{viewOffY} gscale={msx},{msy} touchdisp={tpdw}x{tpdh}");
		} catch { } measureLogCount++; }
#endif
		// Offset INTERNO (tamano): el touch raw ya trae restado el origen
		// (cbX,cbY) por MonoGame, asi que aqui solo se resta lo que sobra por
		// centrar el 800x480 dentro del area. El viewport de Draw, en cambio,
		// va en coords absolutas de surface (= vista): incluye el origen.
		int innerX = (bbw - Constants.BOARD_WIDTH) / 2;
		int innerY = (bbh - Constants.BOARD_HEIGHT) / 2;
		if (innerX < 0) innerX = 0;
		if (innerY < 0) innerY = 0;
		// cbX/cbY (origen del ClientBounds) SOLO cuenta en Android, donde
		// MonoGame recorta la surface a ese rect y ademas se lo resta a cada
		// toque. En Windows ClientBounds.X/Y son la POSICION de la ventana en
		// el escritorio (p.ej. 560,300), no un origen de cliente: sumarlos
		// empujaba el viewport fuera del backbuffer y el juego salia en la
		// esquina inferior derecha. En escritorio el viewport va en coords de
		// backbuffer, asi que el origen es 0.
		int baseX = 0;
		int baseY = 0;
#if ANDROID
		baseX = cbX;
		baseY = cbY;
#endif
		letterOffX = innerX;
		letterOffY = innerY;
		viewOffX = baseX + innerX;
		viewOffY = baseY + innerY;
		if (letterboxLogCount < 6)
		{
			letterboxLogCount++;
			SexyAppBase.LoadLog(string.Format("lb bb={0}x{1} pp={2}x{3} client={4},{5},{6}x{7} inner={8},{9} base={10},{11} view={12},{13} board={14}x{15}", bbw, bbh, ppw, pph, cbX, cbY, cbW, cbH, innerX, innerY, baseX, baseY, viewOffX, viewOffY, Constants.BOARD_WIDTH, Constants.BOARD_HEIGHT));
		}
	}

	private float ScreenToGameX(float x) => x - letterOffX;
	private float ScreenToGameY(float y) => y - letterOffY;

	public static bool RunWhenLocked
	{
		get
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Invalid comparison between Unknown and I4
			return (int)PhoneApplicationService.Current.ApplicationIdleDetectionMode == 1;
		}
		set
		{
			try
			{
				PhoneApplicationService.Current.ApplicationIdleDetectionMode = (IdleDetectionMode)(value ? 1 : 0);
			}
			catch
			{
			}
		}
	}

	public static bool LOW_MEMORY_DEVICE { get; private set; }

	public static bool DO_LOW_MEMORY_OPTIONS { get; private set; }

	public static bool IsInTrialMode => trialModeCachedValue;

	public Main()
	{
#if ANDROID
		try { Android.Util.Log.Info("PVZ", "Main.ctor"); } catch { }
#endif
		SetupTileSchedule();
		graphics = Graphics.GetNew(this);
		SetLowMem();
#if ANDROID || IOS
		graphics.IsFullScreen = true;
#else
		graphics.IsFullScreen = false;
		base.IsMouseVisible = true;
		base.Window.AllowUserResizing = true;
#endif
		Guide.SimulateTrialMode = false;
		graphics.PreferredBackBufferWidth = 800;
		graphics.PreferredBackBufferHeight = 480;
		GraphicsState.mGraphicsDeviceManager.SupportedOrientations = Constants.SupportedOrientations;
		GraphicsState.mGraphicsDeviceManager.DeviceCreated += graphics_DeviceCreated;
		GraphicsState.mGraphicsDeviceManager.DeviceReset += graphics_DeviceReset;
		GraphicsState.mGraphicsDeviceManager.PreparingDeviceSettings += mGraphicsDeviceManager_PreparingDeviceSettings;
		base.TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 30.0);
		base.Exiting += Main_Exiting;
		PhoneApplicationService.Current.UserIdleDetectionMode = (IdleDetectionMode)0;
		PhoneApplicationService.Current.Launching += Game_Launching;
		PhoneApplicationService.Current.Activated += Game_Activated;
		PhoneApplicationService.Current.Closing += Current_Closing;
		PhoneApplicationService.Current.Deactivated += Current_Deactivated;
	}

	private void Current_Deactivated(object sender, DeactivatedEventArgs e)
	{
		GlobalStaticVars.gSexyAppBase.Tombstoned();
	}

	private void Current_Closing(object sender, ClosingEventArgs e)
	{
		PhoneApplicationService.Current.State.Clear();
	}

	private void Game_Activated(object sender, ActivatedEventArgs e)
	{
	}

	private void Game_Launching(object sender, LaunchingEventArgs e)
	{
		PhoneApplicationService.Current.State.Clear();
	}

	private static void SetupTileSchedule()
	{
	}

	private void mGraphicsDeviceManager_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
	{
	}

	private void graphics_DeviceReset(object sender, EventArgs e)
	{
	}

	private void graphics_DeviceCreated(object sender, EventArgs e)
	{
		base.GraphicsDevice.PresentationParameters.PresentationInterval = PresentInterval.Immediate;
	}

	private void Main_Exiting(object sender, EventArgs e)
	{
		GlobalStaticVars.gSexyAppBase.AppExit();
	}

	protected override void Initialize()
	{
		base.Window.OrientationChanged += Window_OrientationChanged;
		GamerServicesComp = new GamerServicesComponent(this);
		ReportAchievement.Initialise();
		base.Initialize();
	}

	protected override void LoadContent()
	{
#if ANDROID
		try { Android.Util.Log.Info("PVZ", "LoadContent start"); } catch { }
#endif
		GraphicsState.Init();
#if ANDROID
		try { Android.Util.Log.Info("PVZ", "GraphicsState.Init ok"); } catch { }
#endif
		SetupForResolution();
#if ANDROID
		try { Android.Util.Log.Info("PVZ", "SetupForResolution ok"); } catch { }
#endif
		GlobalStaticVars.initialize(this);
#if ANDROID
		try { Android.Util.Log.Info("PVZ", "initialize ok"); } catch { }
#endif
		GlobalStaticVars.mGlobalContent.LoadSplashScreen();
		GlobalStaticVars.gSexyAppBase.StartLoadingThread();
#if ANDROID
		try { Android.Util.Log.Info("PVZ", "LoadContent done"); } catch { }
#endif
	}

	protected override void UnloadContent()
	{
		GlobalStaticVars.mGlobalContent.cleanUp();
	}

	protected override void BeginRun()
	{
		base.BeginRun();
	}

	public void CompensateForSlowUpdate()
	{
		ResetElapsedTime();
	}

	protected override void Update(GameTime gameTime)
	{
#if ANDROID
		if (updateLogCount < 3) { try { Android.Util.Log.Info("PVZ", "Update frame " + updateLogCount + " active=" + base.IsActive); } catch { } updateLogCount++; }
#endif
		mFrameCnt++;
		if (mFrameCnt % 300 == 0)
		{
			SexyAppBase.LoadLog("heartbeat IsActive=" + base.IsActive);
		}
		if (!base.IsActive)
		{
			return;
		}
		if (GlobalStaticVars.gSexyAppBase.WantsToExit)
		{
#if IOS
			GlobalStaticVars.gSexyAppBase.WantsToExit = false;
#else
			Exit();
#endif
		}
		HandleInput(gameTime);
		GlobalStaticVars.gSexyAppBase.UpdateApp();
		if (!trialModeChecked)
		{
			trialModeChecked = true;
			bool flag = trialModeCachedValue;
			SetLowMem();
			trialModeCachedValue = Guide.IsTrialMode;
			if (flag != trialModeCachedValue && flag)
			{
				LeftTrialMode();
			}
		}
		try
		{
			base.Update(gameTime);
		}
		catch (GameUpdateRequiredException)
		{
			GlobalStaticVars.gSexyAppBase.ShowUpdateRequiredMessage();
		}
	}

	private static void SetLowMem()
	{
		object obj = default(object);
		DeviceExtendedProperties.TryGetValue("DeviceTotalMemory", ref obj);
		DO_LOW_MEMORY_OPTIONS = (LOW_MEMORY_DEVICE = (long)obj / 1024 / 1024 <= 256);
		LOW_MEMORY_DEVICE = false;
	}

	private void LeftTrialMode()
	{
		if (GlobalStaticVars.gSexyAppBase != null)
		{
			GlobalStaticVars.gSexyAppBase.LeftTrialMode();
		}
		Window_OrientationChanged(null, null);
	}

	public static void SuppressNextDraw()
	{
		wantToSuppressDraw = true;
	}

	public static SignedInGamer GetGamer()
	{
		if (Gamer.SignedInGamers.Count == 0)
		{
			return null;
		}
		return Gamer.SignedInGamers[PlayerIndex.One];
	}

	public static void NeedToSetUpOrientationMatrix(UI_ORIENTATION orientation)
	{
		orientationUsed = orientation;
		newOrientation = true;
	}

	private static void SetupOrientationMatrix(UI_ORIENTATION orientation)
	{
		newOrientation = false;
	}

	private void Window_OrientationChanged(object sender, EventArgs e)
	{
		SetupInterfaceOrientation();
	}

	private void SetupInterfaceOrientation()
	{
		if (GlobalStaticVars.gSexyAppBase != null)
		{
			if (base.Window.CurrentOrientation == DisplayOrientation.LandscapeLeft || base.Window.CurrentOrientation == DisplayOrientation.LandscapeRight)
			{
				GlobalStaticVars.gSexyAppBase.InterfaceOrientationChanged(UI_ORIENTATION.UI_ORIENTATION_LANDSCAPE_LEFT);
			}
			else
			{
				GlobalStaticVars.gSexyAppBase.InterfaceOrientationChanged(UI_ORIENTATION.UI_ORIENTATION_PORTRAIT);
			}
		}
	}

	protected override void Draw(GameTime gameTime)
	{
#if ANDROID
		if (drawLogCount < 3) { try { Android.Util.Log.Info("PVZ", "Draw frame " + drawLogCount); } catch { } drawLogCount++; }
#endif
		if (newOrientation)
		{
			SetupOrientationMatrix(orientationUsed);
		}
		lock (ResourceManager.DrawLocker)
		{
			UpdateLetterbox();
			var gd = base.GraphicsDevice;
			// Viewport 800x480 centrado en coords absolutas de surface: el
			// batch usa la proyeccion del viewport, asi todo (sprites y
			// texto) cae en espacio 800x480.
			gd.Viewport = new Viewport(viewOffX, viewOffY, Constants.BOARD_WIDTH, Constants.BOARD_HEIGHT);
			gd.Clear(Color.Black);
			GlobalStaticVars.gSexyAppBase.DrawGame(gameTime);
			base.Draw(gameTime);
		}
	}

	public void HandleInput(GameTime gameTime)
	{
		if (LoadingScreen.IsLoading)
		{
			return;
		}
		// Refrescar el offset ANTES de mapear (antes solo se actualizaba en
		// Draw, asi el touch usaba el offset del frame anterior tras
		// rotar/redimensionar).
		UpdateLetterbox();
		GamePadState state = GamePad.GetState(PlayerIndex.One);
		if (state.Buttons.Back == ButtonState.Pressed && previousGamepadState.Buttons.Back == ButtonState.Released)
		{
			GlobalStaticVars.gSexyAppBase.BackButtonPress();
		}
		TouchCollection state2 = TouchPanel.GetState();
		bool flag = false;
		foreach (TouchLocation item in state2)
		{
			_Touch touch = new _Touch
			{
				location =
				{
					mX = ScreenToGameX(item.Position.X),
					mY = ScreenToGameY(item.Position.Y)
				}
			};
			if (item.TryGetPreviousLocation(out var previousLocation))
			{
				touch.previousLocation = new CGPoint(ScreenToGameX(previousLocation.Position.X), ScreenToGameY(previousLocation.Position.Y));
			}
			else
			{
				touch.previousLocation = touch.location;
			}
			touch.timestamp = gameTime.TotalGameTime.TotalSeconds;
			if (item.State == TouchLocationState.Pressed && !flag)
			{
#if ANDROID
				if (touchLogCount < 30) { try { Android.Util.Log.Info("PVZ", string.Format(System.Globalization.CultureInfo.InvariantCulture, "touch raw={0:F1},{1:F1} game={2:F1},{3:F1} off={4},{5}", item.Position.X, item.Position.Y, touch.location.mX, touch.location.mY, letterOffX, letterOffY)); } catch { } touchLogCount++; }
#endif
				GlobalStaticVars.gSexyAppBase.TouchBegan(touch);
				flag = true;
			}
			else if (item.State == TouchLocationState.Moved)
			{
				GlobalStaticVars.gSexyAppBase.TouchMoved(touch);
			}
			else if (item.State == TouchLocationState.Released)
			{
				GlobalStaticVars.gSexyAppBase.TouchEnded(touch);
			}
			else if (item.State == TouchLocationState.Invalid)
			{
				GlobalStaticVars.gSexyAppBase.TouchesCanceled();
			}
		}
		HandleMouseAsTouch(gameTime);
		HandleKeyboardInput();
		previousGamepadState = state;
	}

#if !ANDROID && !IOS
	private static Keys[] keyValues;

	[System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetKeyState")]
	private static extern short WinGetKeyState(int nVirtKey);

	private static bool IsCapsLockOn()
	{
		try
		{
			return (WinGetKeyState(20) & 1) != 0;
		}
		catch
		{
			return false;
		}
	}

	private static char? KeyToChar(Keys theKey, bool shift, bool capsLock)
	{
		int num = (int)theKey;
		if (num >= (int)Keys.A && num <= (int)Keys.Z)
		{
			// Shift ^ BloqMayus: con caps-lock activo las letras entran en
			// mayuscula aunque no se pulse Shift (antes se ignoraba y todo
			// quedaba en minuscula).
			bool upper = shift ^ capsLock;
			return upper ? (char)num : (char)(num + 32);
		}
		if (num >= (int)Keys.D0 && num <= (int)Keys.D9)
		{
			return (char)num;
		}
		switch (theKey)
		{
		case Keys.Space:
			return ' ';
		case Keys.OemMinus:
		case Keys.Subtract:
			return '-';
		case Keys.OemPeriod:
		case Keys.Decimal:
			return '.';
		default:
			return null;
		}
	}

	// El port no traia NINGUNA entrada de teclado (nada llamaba a
	// WidgetManager.KeyDown/KeyChar), asi que ningun dialogo podia
	// escribirse. Se traduce el teclado de MonoGame a los codigos que el
	// juego ya espera (KeyCode con valores de VK de Windows) y se enruta
	// solo al widget con el foco.
	private void HandleKeyboardInput()
	{
		if (GlobalStaticVars.gSexyAppBase == null)
		{
			return;
		}
		KeyboardState current;
		try
		{
			current = Keyboard.GetState();
		}
		catch
		{
			return;
		}
		if (current == null)
		{
			return;
		}
		if (keyValues == null)
		{
			keyValues = (Keys[])Enum.GetValues(typeof(Keys));
		}
		bool shift = current.IsKeyDown(Keys.LeftShift) || current.IsKeyDown(Keys.RightShift);
		bool capsLock = IsCapsLockOn();
		foreach (Keys k in keyValues)
		{
			if (k == Keys.None)
			{
				continue;
			}
			bool down = current.IsKeyDown(k);
			if (down == previousKeys.Contains(k))
			{
				continue;
			}
			int num = (int)k;
			if (num <= 0 || num > 255)
			{
				previousKeys.Remove(k);
				continue;
			}
			if (!down)
			{
				previousKeys.Remove(k);
				GlobalStaticVars.gSexyAppBase.KeyUp((KeyCode)num);
				continue;
			}
			previousKeys.Add(k);
			char? c = KeyToChar(k, shift, capsLock);
			if (c.HasValue)
			{
				GlobalStaticVars.gSexyAppBase.KeyChar(new SexyChar(c.Value));
			}
			GlobalStaticVars.gSexyAppBase.KeyDown((KeyCode)num);
		}
	}
#endif

	private void HandleMouseAsTouch(GameTime gameTime)
	{
#if ANDROID || IOS
		// En movil el touch ya viene por TouchPanel.GetState (espacio de
		// backbuffer). Mouse.GetState en Android puede espejar el ultimo touch
		// con otra escala -> doble TouchBegan fantasma ligeramente desfazado.
		// Solo escritorio usa el raton como touch.
		return;
#else
		try
		{
			MouseState mouse = Mouse.GetState();
			// El raton viene en coords de ventana (ClientBounds), no de
			// backbuffer: convertir antes de restar el offset del letterbox
			// (en Windows normal coinciden, escala 1, sin cambios).
			float scaleX = 1f, scaleY = 1f;
			try
			{
				int cw = base.Window.ClientBounds.Width;
				int ch = base.Window.ClientBounds.Height;
				var gd = GraphicsState.mGraphicsDeviceManager.GraphicsDevice;
				if (gd != null && cw > 0 && ch > 0)
				{
					scaleX = gd.PresentationParameters.BackBufferWidth / (float)cw;
					scaleY = gd.PresentationParameters.BackBufferHeight / (float)ch;
				}
			}
			catch
			{
			}
			float mx = ScreenToGameX(mouse.X * scaleX);
			float my = ScreenToGameY(mouse.Y * scaleY);
			bool pressed = mouse.LeftButton == ButtonState.Pressed;
			bool wasPressed = previousMouseState.LeftButton == ButtonState.Pressed;
			double ts = gameTime.TotalGameTime.TotalSeconds;
			if (pressed && !wasPressed)
			{
				mouseDown = true;
				_Touch touch = new _Touch
				{
					location = new CGPoint(mx, my),
					previousLocation = new CGPoint(mx, my),
					timestamp = ts
				};
				GlobalStaticVars.gSexyAppBase.TouchBegan(touch);
			}
			else if (pressed && wasPressed && mouseDown)
			{
				if (mouse.X != previousMouseState.X || mouse.Y != previousMouseState.Y)
				{
					_Touch touch = new _Touch
					{
						location = new CGPoint(mx, my),
						previousLocation = new CGPoint(ScreenToGameX(previousMouseState.X * scaleX), ScreenToGameY(previousMouseState.Y * scaleY)),
						timestamp = ts
					};
					GlobalStaticVars.gSexyAppBase.TouchMoved(touch);
				}
			}
			else if (!pressed && wasPressed && mouseDown)
			{
				mouseDown = false;
				_Touch touch = new _Touch
				{
					location = new CGPoint(mx, my),
					previousLocation = new CGPoint(mx, my),
					timestamp = ts
				};
				GlobalStaticVars.gSexyAppBase.TouchEnded(touch);
			}
			previousMouseState = mouse;
		}
		catch
		{
		}
#endif
	}

	protected override void OnActivated(object sender, EventArgs args)
	{
		trialModeChecked = false;
		if (GlobalStaticVars.gSexyAppBase != null)
		{
			GlobalStaticVars.gSexyAppBase.GotFocus();
			if (!GlobalStaticVars.gSexyAppBase.mMusicInterface.isStopped)
			{
				GlobalStaticVars.gSexyAppBase.mMusicInterface.ResumeMusic();
			}
		}
		base.OnActivated(sender, args);
	}

	protected override void OnDeactivated(object sender, EventArgs args)
	{
		GlobalStaticVars.gSexyAppBase.LostFocus();
		if (!GlobalStaticVars.gSexyAppBase.mMusicInterface.isStopped)
		{
			GlobalStaticVars.gSexyAppBase.mMusicInterface.PauseMusic();
		}
		GlobalStaticVars.gSexyAppBase.AppEnteredBackground();
		base.OnDeactivated(sender, args);
	}

	private void GameSpecificCheatInputCheck()
	{
	}

	private static void SetupForResolution()
	{
		Strings.Culture = CultureInfo.CurrentCulture;
		if (Strings.Culture.TwoLetterISOLanguageName == "fr")
		{
			Constants.Language = Constants.LanguageIndex.fr;
		}
		else if (Strings.Culture.TwoLetterISOLanguageName == "de")
		{
			Constants.Language = Constants.LanguageIndex.de;
		}
		else if (Strings.Culture.TwoLetterISOLanguageName == "es")
		{
			Constants.Language = Constants.LanguageIndex.es;
		}
		else if (Strings.Culture.TwoLetterISOLanguageName == "it")
		{
			Constants.Language = Constants.LanguageIndex.it;
		}
		else
		{
			Constants.Language = Constants.LanguageIndex.en;
		}
		if ((graphics.GraphicsDevice.PresentationParameters.BackBufferWidth == 480 && graphics.GraphicsDevice.PresentationParameters.BackBufferHeight == 800) || (graphics.GraphicsDevice.PresentationParameters.BackBufferWidth == 800 && graphics.GraphicsDevice.PresentationParameters.BackBufferHeight == 480))
		{
			AtlasResources.mAtlasResources = new AtlasResources_480x800();
			Constants.Load480x800();
			return;
		}
		// Cualquier otra resolucion (telefonos): se usan los assets 480x800 y
		// el juego se dibuja con letterbox en Draw(). Antes lanzaba
		// "Unsupported Resolution" y crasheaba al arrancar.
		SexyAppBase.LoadLog(string.Format("SetupForResolution fallback: {0}x{1}",
			graphics.GraphicsDevice.PresentationParameters.BackBufferWidth,
			graphics.GraphicsDevice.PresentationParameters.BackBufferHeight));
		AtlasResources.mAtlasResources = new AtlasResources_480x800();
		Constants.Load480x800();
	}
}
