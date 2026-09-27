using System;
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

	private static bool startedProfiler;

	private static bool wantToSuppressDraw;

	private GamePadState previousGamepadState = default(GamePadState);

	private MouseState previousMouseState;
	private bool mouseDown;
#if ANDROID
	private static int drawLogCount;
	private static int updateLogCount;
#endif

	// Letterbox: el juego siempre piensa en 800x480; si el backbuffer real
	// es otro (telefonos), se renderiza a un target 800x480 y se escala.
	private RenderTarget2D letterboxTarget;
	private float letterScale = 1f;
	private int letterOffX;
	private int letterOffY;

	private void UpdateLetterbox()
	{
		int bbw = GraphicsState.mGraphicsDeviceManager.PreferredBackBufferWidth;
		int bbh = GraphicsState.mGraphicsDeviceManager.PreferredBackBufferHeight;
		try
		{
			var gd = GraphicsState.mGraphicsDeviceManager.GraphicsDevice;
			if (gd != null)
			{
				bbw = gd.PresentationParameters.BackBufferWidth;
				bbh = gd.PresentationParameters.BackBufferHeight;
			}
		}
		catch
		{
		}
		if (bbw <= 0) bbw = Constants.BOARD_WIDTH;
		if (bbh <= 0) bbh = Constants.BOARD_HEIGHT;
		letterScale = Math.Min((float)bbw / Constants.BOARD_WIDTH, (float)bbh / Constants.BOARD_HEIGHT);
		letterOffX = (int)((bbw - Constants.BOARD_WIDTH * letterScale) / 2f);
		letterOffY = (int)((bbh - Constants.BOARD_HEIGHT * letterScale) / 2f);
	}

	private float ScreenToGameX(float x) => (x - letterOffX) / letterScale;
	private float ScreenToGameY(float y) => (y - letterOffY) / letterScale;

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
			bool useLetterbox = letterScale != 1f || letterOffX != 0 || letterOffY != 0;
			if (useLetterbox)
			{
				if (letterboxTarget == null || letterboxTarget.IsDisposed ||
					letterboxTarget.Width != Constants.BOARD_WIDTH || letterboxTarget.Height != Constants.BOARD_HEIGHT)
				{
					letterboxTarget?.Dispose();
					letterboxTarget = new RenderTarget2D(gd, Constants.BOARD_WIDTH, Constants.BOARD_HEIGHT,
						false, SurfaceFormat.Color, DepthFormat.Depth24);
				}
				gd.SetRenderTarget(letterboxTarget);
				gd.Clear(Color.Black);
				GlobalStaticVars.gSexyAppBase.DrawGame(gameTime);
				gd.SetRenderTarget(null);
				gd.Clear(Color.Black);
				var sb = Sexy.Graphics.gSpriteBatch;
				if (sb != null)
				{
					sb.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
					sb.Draw(letterboxTarget,
						new Rectangle(letterOffX, letterOffY,
							(int)(Constants.BOARD_WIDTH * letterScale), (int)(Constants.BOARD_HEIGHT * letterScale)),
						Color.White);
					sb.End();
				}
			}
			else
			{
				gd.Clear(Color.Black);
				GlobalStaticVars.gSexyAppBase.DrawGame(gameTime);
			}
			base.Draw(gameTime);
		}
	}

	public void HandleInput(GameTime gameTime)
	{
		if (LoadingScreen.IsLoading)
		{
			return;
		}
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
		previousGamepadState = state;
	}

	private void HandleMouseAsTouch(GameTime gameTime)
	{
		try
		{
			MouseState mouse = Mouse.GetState();
			UpdateLetterbox();
			float mx = ScreenToGameX(mouse.X);
			float my = ScreenToGameY(mouse.Y);
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
						previousLocation = new CGPoint(ScreenToGameX(previousMouseState.X), ScreenToGameY(previousMouseState.Y)),
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
