using System;
using Sexy;
using Sexy.TodLib;

namespace Lawn;

internal class TitleScreen : Widget, ButtonListener
{
	private const int NUM_TRIGGERS = 5;

	public HyperlinkWidget mStartButton;

	public float mCurBarWidth;

	public float mTotalBarWidth;

	public float mBarVel;

	public float mBarStartProgress;

	public bool mRegisterClicked;

	public bool mLoadingThreadComplete;

	public int mTitleAge;

	public KeyCode mQuickLoadKey;

	public bool mNeedRegister;

	public bool mNeedShowRegisterBox;

	public bool mNeedToInit;

	public float mPrevLoadingPercent;

	public TitleState mTitleState;

	public int mTitleStateCounter;

	public int mTitleStateDuration;

	public bool mLoaderScreenIsLoaded;

	public bool mNeedToUnpackAtlas;

	public bool mDrawnYet;

	public bool mDisplayPartnerLogo;

	public int mNextImageIndex;

	public LawnApp mApp;

	private static Image[] PreflightImages;

	private float[] aTriggerPoint = new float[5];

	public void ButtonPress(int theId, int theClickCount)
	{
	}

	public void ButtonDownTick(int theId)
	{
	}

	public void ButtonMouseEnter(int theId)
	{
	}

	public void ButtonMouseLeave(int theId)
	{
	}

	public void ButtonMouseMove(int theId, int theX, int theY)
	{
	}

	public TitleScreen(LawnApp theApp)
	{
		mApp = theApp;
		mCurBarWidth = 0f;
		mTotalBarWidth = 314f;
		mBarVel = 0.2f;
		mBarStartProgress = 0f;
		mPrevLoadingPercent = 0f;
		mTitleAge = 0;
		mNeedRegister = false;
		mRegisterClicked = false;
		mNeedShowRegisterBox = false;
		mLoadingThreadComplete = false;
		mNeedToInit = true;
		mQuickLoadKey = KeyCode.KEYCODE_UNKNOWN;
		mTitleState = TitleState.TITLESTATE_WAITING_FOR_FIRST_DRAW;
		mTitleStateDuration = 0;
		mTitleStateCounter = 0;
		mDrawnYet = false;
		mDisplayPartnerLogo = mApp.GetBoolean("DisplayPartnerLogo", false);
		mLoaderScreenIsLoaded = false;
		mNeedToUnpackAtlas = true;
		mStartButton = new HyperlinkWidget(0, this);
		mStartButton.mColor = new SexyColor(218, 184, 33);
		mStartButton.mOverColor = new SexyColor(250, 90, 15);
		mStartButton.mUnderlineSize = 0;
		mStartButton.mDisabled = true;
		mStartButton.mVisible = false;
		mNextImageIndex = 0;
		PreflightImages = new Image[22]
		{
			Resources.IMAGE_PLANTSZOMBIES,
			Resources.IMAGE_CHARREDZOMBIES,
			Resources.IMAGE_ALMANACUI,
			Resources.IMAGE_SEEDATLAS,
			Resources.IMAGE_DAVE,
			Resources.IMAGE_CACHED,
			Resources.IMAGE_DIALOG,
			Resources.IMAGE_PARTICLES,
			Resources.IMAGE_CONVEYORBELT_BACKDROP,
			Resources.IMAGE_CONVEYORBELT_BELT,
			Resources.IMAGE_QUICKPLAY,
			Resources.IMAGE_SPEECHBUBBLE,
			Resources.IMAGE_GOODIES,
			Resources.IMAGE_ZOMBIE_NOTE_SMALL,
			Resources.IMAGE_SELECTORSCREEN_ACHIEVEMENTS_TOP_BACKGROUND,
			Resources.IMAGE_SELECTORSCREEN_MOREGAMES_BACKGROUND,
			Resources.IMAGE_SELECTORSCREEN_MAIN_BACKGROUND,
			Resources.IMAGE_SELECTORSCREEN_ACHIEVEMENTS_HOLE,
			Resources.IMAGE_SELECTORSCREEN_ACHIEVEMENTS_HOLE_CHINA,
			Resources.IMAGE_SELECTORSCREEN_QUICKPLAY_BACKGROUND,
			Resources.IMAGE_REANIM_ZOMBIESWON,
			Resources.IMAGE_SCARY_POT
		};
		aTriggerPoint[0] = 0.11f * mTotalBarWidth;
		aTriggerPoint[1] = 0.32f * mTotalBarWidth;
		aTriggerPoint[2] = 0.54f * mTotalBarWidth;
		aTriggerPoint[3] = 0.72f * mTotalBarWidth;
		aTriggerPoint[4] = 0.91f * mTotalBarWidth;
		mApp.mDetails = "[DISCORD_TITLE_SCREEN]";
	}

	public override void Dispose()
	{
		if (Main.LOW_MEMORY_DEVICE)
		{
			mApp.mResourceManager.DeleteResources("Init");
			mApp.mResourceManager.DeleteResources("LoaderBar");
		}
		mStartButton.Dispose();
	}

	public override void Update()
	{
		base.Update();
		if (mApp.mShutdown)
		{
			return;
		}
		mApp.Set3DAcclerated(true);
		mApp.mIs3dAccel = true;
		MarkDirty();
		if (!mDrawnYet)
		{
			return;
		}
		if (mTitleState == TitleState.TITLESTATE_WAITING_FOR_FIRST_DRAW)
		{
			mApp.mMusic.MusicTitleScreenInit();
			mApp.StartLoadingThread();
			mTitleState = TitleState.TITLESTATE_POPCAP_LOGO;
			if (mDisplayPartnerLogo)
			{
				mTitleStateDuration = 150;
			}
			else
			{
				mTitleStateDuration = 200;
			}
			mTitleStateCounter = mTitleStateDuration;
		}
		if (mQuickLoadKey != KeyCode.KEYCODE_UNKNOWN && mTitleState != TitleState.TITLESTATE_SCREEN)
		{
			mTitleState = TitleState.TITLESTATE_SCREEN;
			mTitleStateDuration = 0;
			mTitleStateCounter = 100;
		}
		mTitleAge += 3;
		if (mTitleStateCounter > 0)
		{
			mTitleStateCounter -= 3;
		}
		if (mTitleState == TitleState.TITLESTATE_POPCAP_LOGO)
		{
			if (mTitleStateCounter <= 0)
			{
				if (mDisplayPartnerLogo)
				{
					mTitleState = TitleState.TITLESTATE_SCREEN;
					mTitleStateDuration = 200;
					mTitleStateCounter = 200;
				}
				else
				{
					mTitleState = TitleState.TITLESTATE_SCREEN;
					mTitleStateDuration = 100;
					mTitleStateCounter = 100;
				}
			}
			return;
		}
		else if (mTitleState == TitleState.TITLESTATE_PARTNER_LOGO)
		{
			if (mTitleStateCounter <= 0)
			{
				mTitleState = TitleState.TITLESTATE_SCREEN;
				mTitleStateDuration = 100;
				mTitleStateCounter = 100;
			}
			return;
		}
		else
		{
			if (!mLoaderScreenIsLoaded)
			{
				return;
			}
			if (mNeedToUnpackAtlas)
			{
				mNeedToUnpackAtlas = false;
			}
			float num = (float)mApp.GetLoadingThreadProgress();
			if (mNeedToInit)
			{
				mNeedToInit = false;
				mStartButton.mLabel = TodStringFile.TodStringTranslate("[LOADING]");
				mStartButton.SetFont(Resources.FONT_BRIANNETOD16);
				mStartButton.Resize(mWidth / 2 - Resources.IMAGE_LOADBAR_DIRT.mWidth / 2, 650, (int)mTotalBarWidth, 50);
				mStartButton.mVisible = true;
				float num2 = ((!(num > 1E-06f)) ? 3000f : ((float)mTitleAge / num));
				float num3 = num2 * (1f - num);
				num3 = TodCommon.ClampFloat(num3, 100f, 3000f);
				mBarVel = mTotalBarWidth / num3;
				mBarStartProgress = Math.Min(num, 0.9f);
			}
			float num4 = (num - mBarStartProgress) / (1f - mBarStartProgress);
			int theY = ((mTitleStateCounter <= 10) ? TodCommon.TodAnimateCurve(10, 0, mTitleStateCounter, 534 + Constants.Board_Background_Offset_Y, 529 + Constants.Board_Background_Offset_Y, TodCurves.CURVE_BOUNCE) : TodCommon.TodAnimateCurve(60, 10, mTitleStateCounter, Constants.BOARD_HEIGHT + Constants.Board_Background_Offset_Y, 534 + Constants.Board_Background_Offset_Y, TodCurves.CURVE_EASE_IN));
			mStartButton.Resize(mStartButton.mX, theY, (int)mTotalBarWidth, mStartButton.mHeight);
			if (mTitleStateCounter > 0)
			{
				return;
			}
			mApp.mEffectSystem.Update();
			float num5 = mCurBarWidth;
			mCurBarWidth += mBarVel;
			if (!mLoadingThreadComplete)
			{
				if (mCurBarWidth > mTotalBarWidth * 0.99f)
				{
					mCurBarWidth = mTotalBarWidth * 0.99f;
				}
			}
			else if (mCurBarWidth > mTotalBarWidth)
			{
				if (mApp.mRestoreLocation == RestoreLocation.RESTORE_BOARD)
				{
					mApp.LoadingCompleted();
				}
				else if (mCurBarWidth >= mTotalBarWidth && mStartButton.mLabel != TodStringFile.TodStringTranslate("[CLICK_TO_START]"))
				{
					mStartButton.mLabel = TodStringFile.TodStringTranslate("[CLICK_TO_START]");
					SexyAppBase.LoadLog("READY");
					mCurBarWidth = mTotalBarWidth;
				}
				else if (mCurBarWidth > mTotalBarWidth)
				{
					mStartButton.mLabel = TodStringFile.TodStringTranslate("[CLICK_TO_START]");
					mCurBarWidth = mTotalBarWidth;
				}
			}
			if (num4 > mPrevLoadingPercent + 0.01f || mLoadingThreadComplete)
			{
				float num6 = TodCommon.TodAnimateCurveFloatTime(0f, 1f, num4, 0f, mTotalBarWidth, TodCurves.CURVE_EASE_IN);
				float num7 = num6 - mCurBarWidth;
				float num8 = TodCommon.TodAnimateCurveFloatTime(0f, 1f, num4, 0.0001f, 1E-05f, TodCurves.CURVE_LINEAR);
				if (mLoadingThreadComplete)
				{
					num8 = 0.0001f;
				}
				mBarVel += num7 * Math.Abs(num7) * num8;
				float num9 = TodCommon.TodAnimateCurveFloatTime(0f, 1f, num4, 0.2f, 0.01f, TodCurves.CURVE_LINEAR);
				float num10 = 2f;
				if (mApp.mTodCheatKeys)
				{
					num9 = 0f;
					num10 = 100f;
				}
				if (mBarVel < num9)
				{
					mBarVel = num9;
				}
				else if (mBarVel > num10)
				{
					mBarVel = num10;
				}
				mPrevLoadingPercent = num4;
			}
			if (GameConstants.TESTING_LOAD_BAR && mBarVel > 0.5f)
			{
				mBarVel = 0.5f;
			}
			if (!mLoadingThreadComplete && mApp.mLoadingThreadCompleted)
			{
				LawnApp.PreallocateMemory();
				mLoadingThreadComplete = true;
				mStartButton.SetDisabled(isDisabled: false);
				if (mQuickLoadKey == KeyCode.KEYCODE_ASCIIEND)
				{
					mApp.FastLoad(GameMode.GAMEMODE_CHALLENGE_ZEN_GARDEN);
				}
				else if (mQuickLoadKey == (KeyCode)77)
				{
					mApp.LoadingCompleted();
				}
				else if (mQuickLoadKey == (KeyCode)83)
				{
					mApp.LoadingCompleted();
					mApp.KillGameSelector();
					mApp.ShowChallengeScreen(ChallengePage.CHALLENGE_PAGE_SURVIVAL);
				}
				else if (mQuickLoadKey == (KeyCode)67)
				{
					mApp.LoadingCompleted();
					mApp.KillGameSelector();
					mApp.ShowChallengeScreen(ChallengePage.CHALLENGE_PAGE_CHALLENGE);
				}
				else if (mQuickLoadKey == (KeyCode)85)
				{
					mApp.LoadingCompleted();
					mApp.KillGameSelector();
					mApp.PreNewGame(GameMode.GAMEMODE_UPSELL, theLookForSavedGame: false);
				}
				else if (mQuickLoadKey == (KeyCode)73)
				{
					mApp.LoadingCompleted();
					mApp.KillGameSelector();
					mApp.PreNewGame(GameMode.GAMEMODE_INTRO, theLookForSavedGame: false);
				}
				else if (mQuickLoadKey == (KeyCode)80)
				{
					mApp.LoadingCompleted();
					mApp.KillGameSelector();
					mApp.ShowChallengeScreen(ChallengePage.CHALLENGE_PAGE_PUZZLE);
				}
				else if (mQuickLoadKey == (KeyCode)82)
				{
					mApp.LoadingCompleted();
					mApp.KillGameSelector();
					mApp.ShowCreditScreen();
				}
				else if (mApp.mTodCheatKeys && mApp.mPlayerInfo != null && mQuickLoadKey == (KeyCode)84)
				{
					mApp.FastLoad(GameMode.GAMEMODE_ADVENTURE);
				}
				else
				{
					mStartButton.SetVisible(isVisible: true);
				}
			}
			for (int i = 0; i < 5; i++)
			{
				if (!(aTriggerPoint[i] <= num5) && !(aTriggerPoint[i] > mCurBarWidth))
				{
					ReanimationType theReanimationType = ReanimationType.REANIM_LOADBAR_SPROUT;
					if (i == 4)
					{
						theReanimationType = ReanimationType.REANIM_LOADBAR_ZOMBIEHEAD;
					}
					float num11 = (float)Constants.TitleScreen_ReanimStart_X + aTriggerPoint[i];
					float num12 = 511f + Constants.Board_Background_Offset_Y;
					Reanimation reanimation = mApp.AddReanimation(num11, num12, 0, theReanimationType, theDoScalePos: false);
					reanimation.mLoopType = ReanimLoopType.REANIM_PLAY_ONCE_AND_HOLD;
					reanimation.mAnimRate = 18f;
					switch (i)
					{
					case 1:
					case 3:
						reanimation.OverrideScale(-1f, 1f);
						break;
					case 2:
						reanimation.SetPosition(num11, num12 - 5f);
						reanimation.OverrideScale(1.1f, 1.3f);
						break;
					case 4:
						reanimation.SetPosition(num11 - 20f, num12);
						break;
					}
					if (i == 4)
					{
						mApp.PlaySample(Resources.SOUND_LOADINGBAR_FLOWER);
						mApp.PlaySample(Resources.SOUND_LOADINGBAR_ZOMBIE);
					}
					else
					{
						mApp.PlaySample(Resources.SOUND_LOADINGBAR_FLOWER);
					}
				}
			}
		}
	}

	public override void Draw(Graphics g)
	{
		g.SetLinearBlend(linear: true);
		base.Draw(g);
		if (mTitleState == TitleState.TITLESTATE_WAITING_FOR_FIRST_DRAW)
		{
			g.SetColor(SexyColor.Black);
			g.FillRect(0, 0, mWidth, mHeight);
			if (!mDrawnYet)
			{
				mDrawnYet = true;
			}
			return;
		}
		if (mTitleState == TitleState.TITLESTATE_POPCAP_LOGO)
		{
			g.SetColor(SexyColor.Black);
			g.FillRect(0, 0, mWidth, mHeight);
			int anAlpha = 255;
			if (mTitleStateCounter < mTitleStateDuration - 50)
			{
				if (!mDisplayPartnerLogo)
				{
					anAlpha = TodCommon.TodAnimateCurve(50, 0, mTitleStateCounter, 255, 0, TodCurves.CURVE_LINEAR);
				}
			}
			else
			{
				anAlpha = TodCommon.TodAnimateCurve(mTitleStateDuration, mTitleStateDuration - 50, mTitleStateCounter, 0, 255, TodCurves.CURVE_LINEAR);
			}
			g.SetColorizeImages(colorizeImages: true);
			g.SetColor(new SexyColor(255, 255, 255, anAlpha));
			g.DrawImage(Resources.IMAGE_POPCAP_LOGO, (mWidth - Resources.IMAGE_POPCAP_LOGO.mWidth) / 2, (mHeight - Resources.IMAGE_POPCAP_LOGO.mHeight) / 2);
			g.SetColorizeImages(colorizeImages: false);
			return;
		}
		if (!mLoaderScreenIsLoaded)
		{
			g.SetColor(SexyColor.Black);
			g.FillRect(0, 0, mWidth, mHeight);
			return;
		}
		PreflightNextImage(g);
		g.DrawImage(Resources.IMAGE_TITLESCREEN, 0, 0);
		if (mNeedToInit)
		{
			return;
		}
		int aLogoY;
		if (mTitleStateCounter > 60)
		{
			aLogoY = TodCommon.TodAnimateCurve(100, 60, mTitleStateCounter, -Resources.IMAGE_PVZ_LOGO.mHeight, 10 + Constants.Board_Background_Offset_Y, TodCurves.CURVE_EASE_IN);
		}
		else
		{
			aLogoY = TodCommon.TodAnimateCurve(60, 50, mTitleStateCounter, 10 + Constants.Board_Background_Offset_Y, 15 + Constants.Board_Background_Offset_Y, TodCurves.CURVE_BOUNCE);
		}
		g.DrawImage(Resources.IMAGE_PVZ_LOGO, mWidth / 2 - Resources.IMAGE_PVZ_LOGO.mWidth / 2, aLogoY);
		int num2 = mStartButton.mX;
		int num3 = mStartButton.mY - 17;
		g.DrawImage(Resources.IMAGE_LOADBAR_DIRT, num2, (float)num3 + 18f);
		if (mCurBarWidth >= mTotalBarWidth)
		{
			g.DrawImage(Resources.IMAGE_LOADBAR_GRASS, num2, num3);
			if (mLoadingThreadComplete)
			{
				DrawToPreload(g);
			}
		}
		else
		{
			Graphics graphics = Graphics.GetNew(g);
			graphics.ClipRect(240 + Constants.Board_Additional_Width, num3, (int)mCurBarWidth, Resources.IMAGE_LOADBAR_GRASS.mHeight);
			graphics.DrawImage(Resources.IMAGE_LOADBAR_GRASS, num2, num3);
			float num4 = mCurBarWidth * 0.94f;
			float rad = (0f - num4) / 180f * (float)Math.PI * 2f;
			float num5 = TodCommon.TodAnimateCurveFloatTime(0f, mTotalBarWidth, mCurBarWidth, 1f, 0.5f, TodCurves.CURVE_LINEAR);
			SexyTransform2D sexyTransform2D = default(SexyTransform2D);
			TodCommon.TodScaleRotateTransformMatrix(ref sexyTransform2D.mMatrix, (float)num2 + 11f + num4, (float)num3 - 3f - 35f * num5 + 35f, rad, num5, num5);
			TodCommon.TodBltMatrix(theSrcRect: new TRect(0, 0, Resources.IMAGE_REANIM_LOAD_SODROLLCAP.mWidth, Resources.IMAGE_REANIM_LOAD_SODROLLCAP.mHeight), g: g, theImage: Resources.IMAGE_REANIM_LOAD_SODROLLCAP, theTransform: sexyTransform2D.mMatrix, theClipRect: ref g.mClipRect, theColor: SexyColor.White, theDrawMode: g.mDrawMode);
			graphics.PrepareForReuse();
		}
		foreach (Reanimation mReanimation in mApp.mEffectSystem.mReanimationHolder.mReanimations)
		{
			mReanimation.Draw(g);
		}
	}

	public override void Resize(int theX, int theY, int theWidth, int theHeight)
	{
		base.Resize(theX, theY, theWidth, theHeight);
	}

	public override void AddedToManager(WidgetManager theWidgetManager)
	{
		base.AddedToManager(theWidgetManager);
		theWidgetManager.AddWidget(mStartButton);
	}

	public override void RemovedFromManager(WidgetManager theWidgetManager)
	{
		base.RemovedFromManager(theWidgetManager);
		theWidgetManager.RemoveWidget(mStartButton);
	}

	public virtual void ButtonPress(int theId)
	{
		mApp.PlaySample(Resources.SOUND_BUTTONCLICK);
	}

	public override bool BackButtonPress()
	{
		mApp.AppExit();
		return true;
	}

	public virtual void ButtonDepress(int theId)
	{
		switch (theId)
		{
		case 0:
			mApp.LoadingCompleted();
			break;
		case 1:
			mRegisterClicked = true;
			break;
		}
	}

	public override void MouseDown(int x, int y, int theClickCount)
	{
		if (mLoadingThreadComplete)
		{
			mApp.PlaySample(Resources.SOUND_BUTTONCLICK);
			mApp.LoadingCompleted();
		}
	}

	public override void KeyDown(KeyCode theKey)
	{
		if (mLoadingThreadComplete)
		{
			mApp.PlaySample(Resources.SOUND_BUTTONCLICK);
			mApp.LoadingCompleted();
		}
		else if (mApp.mTodCheatKeys && mApp.mPlayerInfo != null)
		{
			mQuickLoadKey = theKey;
		}
	}

	public void SetRegistered()
	{
	}

	public void DrawToPreload(Graphics g)
	{
		g.DrawImage(Resources.IMAGE_PLANTSHADOW, 1000, -50);
	}

	public void PreflightNextImage(Graphics g)
	{
		if (mNextImageIndex < PreflightImages.Length && PreflightImages[mNextImageIndex] != null)
		{
			g.DrawImage(PreflightImages[mNextImageIndex], 0, 0, new TRect(0, 0, 1, 1));
			mNextImageIndex++;
		}
	}
}
