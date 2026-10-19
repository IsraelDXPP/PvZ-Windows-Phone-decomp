using System;
using Microsoft.Xna.Framework;
using Sexy;
using Sexy.TodLib;

namespace Lawn;

internal enum SelectorAnimState
{
	SELECTOR_OPEN,
	SELECTOR_NEW_USER,
	SELECTOR_SHOW_SIGN,
	SELECTOR_SUB_MENU,
	SELECTOR_IDLE
}

internal class GameSelectorOverlay : Widget
{
	public GameSelector mParent;

	public GameSelectorOverlay(GameSelector theGameSelector)
	{
		mParent = theGameSelector;
		mMouseVisible = false;
		mHasAlpha = true;
	}

	public override void Draw(Graphics g)
	{
		mParent.DrawOverlay(g);
	}
}

internal class GameSelector : Widget, ButtonListener, StoreListener, LawnMessageBoxListener
{
	public bool mWaitingTrialConfirm;
	public LawnApp mApp;

	public NewLawnButton mAdventureButton;

	public NewLawnButton mMinigameButton;

	public NewLawnButton mPuzzleButton;

	public NewLawnButton mOptionsButton;

	public NewLawnButton mQuitButton;

	public NewLawnButton mHelpButton;

	public NewLawnButton mStoreButton;

	public NewLawnButton mAlmanacButton;

	public NewLawnButton mZenGardenButton;

	public NewLawnButton mAchievementButton;

	public NewLawnButton mQuickPlayButton;

	public NewLawnButton mCreditsButton;

	public NewLawnButton mSurvivalButton;

	public NewLawnButton mChangeUserButton;

	public Widget mOverlayWidget;

	public bool mStartingGame;

	public int mStartingGameCounter;

	public bool mMinigamesLocked;

	public bool mPuzzleLocked;

	public bool mSurvivalLocked;

	public bool mShowStartButton;

	public TodParticleSystem mTrophyParticleID;

	public Reanimation mSelectorReanimID;

	public Reanimation[] mCloudReanimID = new Reanimation[6];

	public int[] mCloudCounter = new int[6];

	public Reanimation[] mFlowerReanimID = new Reanimation[3];

	public Reanimation mLeafReanimID;

	public Reanimation mHandReanimID;

	public int mLeafCounter;

	public SelectorAnimState mSelectorState;

	public int mLevel;

	public bool mLoading;

	public ToolTipWidget mToolTip;

	public bool mHasTrophy;

	public bool mUnlockSelectorCheat;

	public bool mDebugText;

	public bool mDebugKeysEnabled;

	public int mMovementTimer;

	public int mCurrentY;

	public int mDestinationY;

	public int mCurrentX;

	public int mDestinationX;

	public bool mEnableButtonsTransition;

	public bool mInUserDialog;

	public bool mDoNewGameAfterStore;

	private static float[,] gFlowerCenter = new float[3, 2] { { 765f, 483f }, { 663f, 455f }, { 701f, 439f } };

	public GameSelector(LawnApp theApp)
	{
		mApp = theApp;
		mLevel = 1;
		mLoading = false;
		mHasTrophy = false;
		mToolTip = new ToolTipWidget();
		mDebugText = false;
		mDebugKeysEnabled = GlobalStaticVars.gIsPartnerBuild;
		mInUserDialog = false;
		mDoNewGameAfterStore = false;
		mMovementTimer = 0;
		mCurrentY = mY;
		mDestinationY = mCurrentY;
		mCurrentX = mX;
		mDestinationX = mCurrentX;
		mEnableButtonsTransition = false;
		mAdventureButton = GameButton.MakeNewButton(100, this, "", null, Resources.IMAGE_REANIM_SELECTORSCREEN_ADVENTURE_BUTTON, Resources.IMAGE_REANIM_SELECTORSCREEN_ADVENTURE_HIGHLIGHT, Resources.IMAGE_REANIM_SELECTORSCREEN_ADVENTURE_HIGHLIGHT);
		mAdventureButton.Resize(0, 0, Resources.IMAGE_REANIM_SELECTORSCREEN_ADVENTURE_BUTTON.mWidth, 125);
		mAdventureButton.mClip = false;
		mAdventureButton.mMouseVisible = false;
		mAdventureButton.mPolygonShape[0] = new SexyVector2(7f, 1f);
		mAdventureButton.mPolygonShape[1] = new SexyVector2(328f, 30f);
		mAdventureButton.mPolygonShape[2] = new SexyVector2(314f, 125f);
		mAdventureButton.mPolygonShape[3] = new SexyVector2(1f, 78f);
		mAdventureButton.mUsePolygonShape = true;
		mMinigameButton = GameButton.MakeNewButton(101, this, "", null, Resources.IMAGE_REANIM_SELECTORSCREEN_SURVIVAL_BUTTON, Resources.IMAGE_REANIM_SELECTORSCREEN_SURVIVAL_HIGHLIGHT, Resources.IMAGE_REANIM_SELECTORSCREEN_SURVIVAL_HIGHLIGHT);
		mMinigameButton.Resize(0, 0, Resources.IMAGE_REANIM_SELECTORSCREEN_SURVIVAL_BUTTON.mWidth, 130);
		mMinigameButton.mClip = false;
		mMinigameButton.mBtnNoDraw = true;
		mMinigameButton.mMouseVisible = false;
		mMinigameButton.mPolygonShape[0] = new SexyVector2(4f, 2f);
		mMinigameButton.mPolygonShape[1] = new SexyVector2(312f, 51f);
		mMinigameButton.mPolygonShape[2] = new SexyVector2(296f, 130f);
		mMinigameButton.mPolygonShape[3] = new SexyVector2(7f, 77f);
		mMinigameButton.mUsePolygonShape = true;
		mPuzzleButton = GameButton.MakeNewButton(102, this, "", null, Resources.IMAGE_REANIM_SELECTORSCREEN_CHALLENGES_BUTTON, Resources.IMAGE_REANIM_SELECTORSCREEN_CHALLENGES_HIGHLIGHT, Resources.IMAGE_REANIM_SELECTORSCREEN_CHALLENGES_HIGHLIGHT);
		mPuzzleButton.Resize(0, 0, Resources.IMAGE_REANIM_SELECTORSCREEN_CHALLENGES_BUTTON.mWidth, 121);
		mPuzzleButton.mClip = false;
		mPuzzleButton.mBtnNoDraw = true;
		mPuzzleButton.mMouseVisible = false;
		mPuzzleButton.mPolygonShape[0] = new SexyVector2(2f, 0f);
		mPuzzleButton.mPolygonShape[1] = new SexyVector2(281f, 55f);
		mPuzzleButton.mPolygonShape[2] = new SexyVector2(268f, 121f);
		mPuzzleButton.mPolygonShape[3] = new SexyVector2(3f, 60f);
		mPuzzleButton.mUsePolygonShape = true;
		mSurvivalButton = GameButton.MakeNewButton(110, this, "", null, Resources.IMAGE_REANIM_SELECTORSCREEN_VASEBREAKER_BUTTON, Resources.IMAGE_REANIM_SELECTORSCREEN_VASEBREAKER_HIGHLIGHT, Resources.IMAGE_REANIM_SELECTORSCREEN_VASEBREAKER_HIGHLIGHT);
		mSurvivalButton.Resize(0, 0, Resources.IMAGE_REANIM_SELECTORSCREEN_VASEBREAKER_BUTTON.mWidth, 124);
		mSurvivalButton.mClip = false;
		mSurvivalButton.mBtnNoDraw = true;
		mSurvivalButton.mMouseVisible = false;
		mSurvivalButton.mPolygonShape[0] = new SexyVector2(7f, 1f);
		mSurvivalButton.mPolygonShape[1] = new SexyVector2(267f, 62f);
		mSurvivalButton.mPolygonShape[2] = new SexyVector2(257f, 124f);
		mSurvivalButton.mPolygonShape[3] = new SexyVector2(7f, 57f);
		mSurvivalButton.mUsePolygonShape = true;
		mZenGardenButton = GameButton.MakeNewButton(109, this, "", null, Resources.IMAGE_SELECTORSCREEN_ZENGARDEN, Resources.IMAGE_SELECTORSCREEN_ZENGARDENHIGHLIGHT, Resources.IMAGE_SELECTORSCREEN_ZENGARDENHIGHLIGHT);
		mZenGardenButton.Resize(0, 0, 130, 130);
		mZenGardenButton.mMouseVisible = false;
		mZenGardenButton.mClip = false;
		mOptionsButton = GameButton.MakeNewButton(103, this, "", null, Resources.IMAGE_SELECTORSCREEN_OPTIONS1, Resources.IMAGE_SELECTORSCREEN_OPTIONS2, Resources.IMAGE_SELECTORSCREEN_OPTIONS2);
		mOptionsButton.Resize(0, 0, Resources.IMAGE_SELECTORSCREEN_OPTIONS1.mWidth, Resources.IMAGE_SELECTORSCREEN_OPTIONS1.mHeight + 23);
		mOptionsButton.mBtnNoDraw = true;
		mOptionsButton.mMouseVisible = false;
		mOptionsButton.mButtonOffsetY = 15;
		mOptionsButton.mClip = false;
		mHelpButton = GameButton.MakeNewButton(104, this, "", null, Resources.IMAGE_SELECTORSCREEN_HELP1, Resources.IMAGE_SELECTORSCREEN_HELP2, Resources.IMAGE_SELECTORSCREEN_HELP2);
		mHelpButton.Resize(0, 0, Resources.IMAGE_SELECTORSCREEN_HELP1.mWidth, Resources.IMAGE_SELECTORSCREEN_HELP1.mHeight + 33);
		mHelpButton.mBtnNoDraw = true;
		mHelpButton.mMouseVisible = false;
		mHelpButton.mButtonOffsetY = 30;
		mHelpButton.mClip = false;
		mQuitButton = GameButton.MakeNewButton(105, this, "", null, Resources.IMAGE_SELECTORSCREEN_QUIT1, Resources.IMAGE_SELECTORSCREEN_QUIT2, Resources.IMAGE_SELECTORSCREEN_QUIT2);
		mQuitButton.Resize(0, 0, Resources.IMAGE_SELECTORSCREEN_QUIT1.mWidth + 10, Resources.IMAGE_SELECTORSCREEN_QUIT1.mHeight + 10);
		mQuitButton.mBtnNoDraw = true;
		mQuitButton.mMouseVisible = false;
		mQuitButton.mButtonOffsetX = 5;
		mQuitButton.mButtonOffsetY = 5;
		mQuitButton.mClip = false;
		mChangeUserButton = GameButton.MakeNewButton(106, this, "", null, Resources.IMAGE_BLANK, Resources.IMAGE_BLANK, Resources.IMAGE_BLANK);
		mChangeUserButton.Resize(0, 0, 250, 30);
		mChangeUserButton.mBtnNoDraw = true;
		mChangeUserButton.mMouseVisible = false;
		mOverlayWidget = new GameSelectorOverlay(this);
		mOverlayWidget.Resize(0, 0, Constants.BOARD_WIDTH, Constants.BOARD_HEIGHT);
		mStoreButton = GameButton.MakeNewButton(107, this, "", null, Resources.IMAGE_SELECTORSCREEN_STORE, Resources.IMAGE_SELECTORSCREEN_STOREHIGHLIGHT, Resources.IMAGE_SELECTORSCREEN_STOREHIGHLIGHT);
		mStoreButton.Resize(405, 484, Resources.IMAGE_SELECTORSCREEN_STORE.mWidth, Resources.IMAGE_SELECTORSCREEN_STORE.mHeight);
		mStoreButton.mMouseVisible = false;
		mStoreButton.mClip = false;
		mAlmanacButton = GameButton.MakeNewButton(108, this, "", null, Resources.IMAGE_SELECTORSCREEN_ALMANAC, Resources.IMAGE_SELECTORSCREEN_ALMANACHIGHLIGHT, Resources.IMAGE_SELECTORSCREEN_ALMANACHIGHLIGHT);
		mAlmanacButton.Resize(327, 428, Resources.IMAGE_SELECTORSCREEN_ALMANAC.mWidth, Resources.IMAGE_SELECTORSCREEN_ALMANAC.mHeight);
		mAlmanacButton.mMouseVisible = false;
		mAlmanacButton.mClip = false;
		mAchievementButton = GameButton.MakeNewButton(111, this, "", null, Resources.IMAGE_ACHIEVEMENT_BUTTON, Resources.IMAGE_ACHIEVEMENT_HIGHLIGHT, Resources.IMAGE_ACHIEVEMENT_HIGHLIGHT);
		mAchievementButton.Resize(20, mApp.mHeight - Resources.IMAGE_ACHIEVEMENT_BUTTON.mHeight - 35, Resources.IMAGE_ACHIEVEMENT_BUTTON.mWidth, Resources.IMAGE_ACHIEVEMENT_BUTTON.mHeight);
		mAchievementButton.mClip = false;
		mAchievementButton.mMouseVisible = false;
		mQuickPlayButton = GameButton.MakeNewButton(112, this, "", null, Resources.IMAGE_QUICKPLAY_BUTTON, Resources.IMAGE_QUICKPLAY_BUTTON_HIGHLIGHT, Resources.IMAGE_QUICKPLAY_BUTTON_HIGHLIGHT);
		mQuickPlayButton.Resize(300, mApp.mHeight - Resources.IMAGE_QUICKPLAY_BUTTON.mHeight - 100, Resources.IMAGE_QUICKPLAY_BUTTON.mWidth, Resources.IMAGE_QUICKPLAY_BUTTON.mHeight);
		mQuickPlayButton.mClip = false;
		mQuickPlayButton.mMouseVisible = false;
		mQuickPlayButton.SetDisabled(false);
		mQuickPlayButton.mVisible = true;
		mCreditsButton = GameButton.MakeNewButton(113, this, "", null, Resources.IMAGE_BLANK, Resources.IMAGE_BLANK, Resources.IMAGE_BLANK);
		mCreditsButton.Resize(0, 0, Resources.IMAGE_REANIM_SELECTORSCREEN_WOODSIGN3_PRESS.mWidth, Resources.IMAGE_REANIM_SELECTORSCREEN_WOODSIGN3_PRESS.mHeight);
		mCreditsButton.mBtnNoDraw = true;
		mCreditsButton.mMouseVisible = false;
		mApp.mMusic.MakeSureMusicIsPlaying(MusicTune.MUSIC_TUNE_TITLE_CRAZY_DAVE_MAIN_THEME);
		mStartingGame = false;
		mStartingGameCounter = 0;
		mMinigamesLocked = false;
		mPuzzleLocked = false;
		mSurvivalLocked = false;
		mUnlockSelectorCheat = false;
		mTrophyParticleID = null;
		mShowStartButton = false;
		Reanimation aSelectorReanim = mApp.AddReanimation(0.5f + Constants.Board_Additional_Width, 0.5f + Constants.Board_Background_Offset_Y, 0, ReanimationType.REANIM_SELECTOR_SCREEN);
		aSelectorReanim.PlayReanim("anim_open", ReanimLoopType.REANIM_PLAY_ONCE_AND_HOLD, 0, 30f);
		aSelectorReanim.AssignRenderGroupToPrefix("flower", ReanimatorXnaHelpers.RENDER_GROUP_HIDDEN);
		aSelectorReanim.AssignRenderGroupToPrefix("leaf", ReanimatorXnaHelpers.RENDER_GROUP_HIDDEN);
		aSelectorReanim.AssignRenderGroupToTrack("SelectorScreen_BG", 1);
		aSelectorReanim.AssignRenderGroupToPrefix("SelectorScreen_Adventure_", ReanimatorXnaHelpers.RENDER_GROUP_NORMAL);
		aSelectorReanim.AssignRenderGroupToPrefix("SelectorScreen_StartAdventure_", ReanimatorXnaHelpers.RENDER_GROUP_HIDDEN);
		mSelectorReanimID = mApp.ReanimationGetID(aSelectorReanim);
		mSelectorState = SelectorAnimState.SELECTOR_OPEN;
		short aFrameStart, aFrameCount;
		aSelectorReanim.GetFramesForLayer("anim_sign", out aFrameStart, out aFrameCount);
		aSelectorReanim.mFrameBasePose = (short)(aFrameStart + aFrameCount - 1);
		for (int i = 0; i < 6; i++)
		{
			Reanimation aCloudReanim = mApp.AddReanimation(0.5f, 0.5f, 0, ReanimationType.REANIM_SELECTOR_SCREEN);
			string aAnimName = Common.StrFormat_("anim_cloud{0}", i > 1 ? i + 2 : i + 1);
			aCloudReanim.PlayReanim(aAnimName, ReanimLoopType.REANIM_PLAY_ONCE_AND_HOLD, 0, 0f);
			mCloudReanimID[i] = mApp.ReanimationGetID(aCloudReanim);
			mCloudCounter[i] = TodCommon.RandRangeInt(-6000, 2000);
			if (mCloudCounter[i] < 0)
			{
				aCloudReanim.mAnimTime = (float)(-mCloudCounter[i]) / 6000f;
				aCloudReanim.mAnimRate = 0.5f;
				mCloudCounter[i] = 0;
			}
			else
			{
				aCloudReanim.mAnimRate = 0f;
			}
		}
		for (int i = 0; i < 3; i++)
		{
			Reanimation aFlowerReanim = mApp.AddReanimation(0.5f + Constants.Board_Additional_Width, 0.5f + Constants.Board_Background_Offset_Y, 0, ReanimationType.REANIM_SELECTOR_SCREEN);
			string aAnimName = Common.StrFormat_("anim_flower{0}", i + 1);
			aFlowerReanim.PlayReanim(aAnimName, ReanimLoopType.REANIM_PLAY_ONCE_AND_HOLD, 0, 0f);
			aFlowerReanim.mAnimRate = 0f;
			Reanimation selReanim = mApp.ReanimationGet(mSelectorReanimID);
			aFlowerReanim.AttachToAnotherReanimation(ref selReanim, "SelectorScreen_BG_Right");
			aFlowerReanim.mIsAttachment = false;
			mFlowerReanimID[i] = mApp.ReanimationGetID(aFlowerReanim);
		}
		Reanimation aLeafReanim = mApp.AddReanimation(0.5f + Constants.Board_Additional_Width, 0.5f + Constants.Board_Background_Offset_Y, 0, ReanimationType.REANIM_SELECTOR_SCREEN);
		aLeafReanim.PlayReanim("anim_grass", ReanimLoopType.REANIM_LOOP, 0, 6f);
		aLeafReanim.mAnimRate = 0f;
		mLeafReanimID = mApp.ReanimationGetID(aLeafReanim);
		mLeafCounter = 50;
		mApp.mDetails = "[DISCORD_MAIN_MENU]";
		SyncProfile(theShowLoading: false);
		mApp.PlaySample(Resources.SOUND_ROLL_IN);
	}

	public override void Dispose()
	{
		RemoveAllWidgets(doDelete: true, recursive: true);
		mApp.UpdateDiscordState();
	}

	public void SyncButtons()
	{
		bool aAlmanacAvailable = mApp.CanShowAlmanac() || mUnlockSelectorCheat;
		bool aStoreOpen = mApp.CanShowStore() || mUnlockSelectorCheat;
		bool aZenGardenOpen = mApp.CanShowZenGarden() || mUnlockSelectorCheat;
		mAlmanacButton.mDisabled = !aAlmanacAvailable;
		mAlmanacButton.mVisible = aAlmanacAvailable;
		mQuickPlayButton.mDisabled = !mApp.HasFinishedAdventure();
		mQuickPlayButton.mVisible = mApp.HasFinishedAdventure();
		mStoreButton.mDisabled = !aStoreOpen;
		mStoreButton.mVisible = aStoreOpen;
		Reanimation aSelectorReanim = mApp.ReanimationGet(mSelectorReanimID);
		if (aAlmanacAvailable)
		{
			aSelectorReanim.AssignRenderGroupToPrefix("almanac_key_shadow", ReanimatorXnaHelpers.RENDER_GROUP_NORMAL);
			if (aStoreOpen)
			{
				aSelectorReanim.SetImageOverride("almanac_key_shadow", null);
			}
			else
			{
				aSelectorReanim.SetImageOverride("almanac_key_shadow", Resources.IMAGE_REANIM_SELECTORSCREEN_ALMANAC_SHADOW);
			}
		}
		else if (aStoreOpen)
		{
			aSelectorReanim.AssignRenderGroupToPrefix("almanac_key_shadow", ReanimatorXnaHelpers.RENDER_GROUP_NORMAL);
			aSelectorReanim.SetImageOverride("almanac_key_shadow", Resources.IMAGE_REANIM_SELECTORSCREEN_KEY_SHADOW);
		}
		else
		{
			aSelectorReanim.AssignRenderGroupToPrefix("almanac_key_shadow", ReanimatorXnaHelpers.RENDER_GROUP_HIDDEN);
		}
		mZenGardenButton.mDisabled = !aZenGardenOpen;
		mZenGardenButton.mVisible = aZenGardenOpen;
		if (mMinigamesLocked)
		{
			mMinigameButton.mOverImage = Resources.IMAGE_REANIM_SELECTORSCREEN_SURVIVAL_BUTTON;
			mMinigameButton.mDownImage = Resources.IMAGE_REANIM_SELECTORSCREEN_SURVIVAL_BUTTON;
			mMinigameButton.SetColor((int)GameButton.ButtonColours.COLOR_BKG, new SexyColor(128, 128, 128));
		}
		else
		{
			mMinigameButton.mOverImage = Resources.IMAGE_REANIM_SELECTORSCREEN_SURVIVAL_HIGHLIGHT;
			mMinigameButton.mDownImage = Resources.IMAGE_REANIM_SELECTORSCREEN_SURVIVAL_HIGHLIGHT;
			mMinigameButton.SetColor((int)GameButton.ButtonColours.COLOR_BKG, SexyColor.White);
		}
		if (mPuzzleLocked)
		{
			mPuzzleButton.mOverImage = Resources.IMAGE_REANIM_SELECTORSCREEN_CHALLENGES_BUTTON;
			mPuzzleButton.mDownImage = Resources.IMAGE_REANIM_SELECTORSCREEN_CHALLENGES_BUTTON;
			mPuzzleButton.SetColor((int)GameButton.ButtonColours.COLOR_BKG, new SexyColor(128, 128, 128));
		}
		else
		{
			mPuzzleButton.mOverImage = Resources.IMAGE_REANIM_SELECTORSCREEN_CHALLENGES_HIGHLIGHT;
			mPuzzleButton.mDownImage = Resources.IMAGE_REANIM_SELECTORSCREEN_CHALLENGES_HIGHLIGHT;
			mPuzzleButton.SetColor((int)GameButton.ButtonColours.COLOR_BKG, SexyColor.White);
		}
		if (mSurvivalLocked)
		{
			mSurvivalButton.mOverImage = Resources.IMAGE_REANIM_SELECTORSCREEN_VASEBREAKER_BUTTON;
			mSurvivalButton.mDownImage = Resources.IMAGE_REANIM_SELECTORSCREEN_VASEBREAKER_BUTTON;
			mSurvivalButton.SetColor((int)GameButton.ButtonColours.COLOR_BKG, new SexyColor(128, 128, 128));
		}
		else
		{
			mSurvivalButton.mOverImage = Resources.IMAGE_REANIM_SELECTORSCREEN_VASEBREAKER_HIGHLIGHT;
			mSurvivalButton.mDownImage = Resources.IMAGE_REANIM_SELECTORSCREEN_VASEBREAKER_HIGHLIGHT;
			mSurvivalButton.SetColor((int)GameButton.ButtonColours.COLOR_BKG, SexyColor.White);
		}
		ReanimatorTrackInstance aMinigameTrack = aSelectorReanim.GetTrackInstanceByName("SelectorScreen_Survival_button");
		ReanimatorTrackInstance aPuzzleTrack = aSelectorReanim.GetTrackInstanceByName("SelectorScreen_Challenges_button");
		ReanimatorTrackInstance aSurvivalTrack = aSelectorReanim.GetTrackInstanceByName("SelectorScreen_ZenGarden_button");
		aMinigameTrack.mTrackColor = new SexyColor(mMinigameButton.GetColor((int)GameButton.ButtonColours.COLOR_BKG));
		aPuzzleTrack.mTrackColor = new SexyColor(mPuzzleButton.GetColor((int)GameButton.ButtonColours.COLOR_BKG));
		aSurvivalTrack.mTrackColor = new SexyColor(mSurvivalButton.GetColor((int)GameButton.ButtonColours.COLOR_BKG));
		if (mShowStartButton)
		{
			mAdventureButton.mButtonImage = Resources.IMAGE_REANIM_SELECTORSCREEN_STARTADVENTURE_BUTTON;
			mAdventureButton.mOverImage = Resources.IMAGE_REANIM_SELECTORSCREEN_STARTADVENTURE_HIGHLIGHT;
			mAdventureButton.mDownImage = Resources.IMAGE_REANIM_SELECTORSCREEN_STARTADVENTURE_HIGHLIGHT;
		}
		else
		{
			mAdventureButton.mButtonImage = Resources.IMAGE_REANIM_SELECTORSCREEN_ADVENTURE_BUTTON;
			mAdventureButton.mOverImage = Resources.IMAGE_REANIM_SELECTORSCREEN_ADVENTURE_HIGHLIGHT;
			mAdventureButton.mDownImage = Resources.IMAGE_REANIM_SELECTORSCREEN_ADVENTURE_HIGHLIGHT;
		}
	}

	public void AddTrophySparkle()
	{
		Debug.ASSERT(mTrophyParticleID == null);
		TodParticleSystem aTrophyParticle = mApp.AddTodParticle(85f + Constants.Board_Additional_Width, 380f + Constants.Board_Background_Offset_Y, 1, ParticleEffect.PARTICLE_TROPHY_SPARKLE);
		mTrophyParticleID = mApp.ParticleGetID(aTrophyParticle);
	}

	public void SyncProfile(bool theShowLoading)
	{
		if (theShowLoading)
		{
			mLoading = true;
			mApp.DoDialog((int)Dialogs.DIALOG_MESSAGE, true, "[LOADING]", "", "", (int)Dialog.DialogButtons.BUTTONS_NONE);
			mApp.PreloadForUser();
			mApp.KillDialog((int)Dialogs.DIALOG_MESSAGE);
			mLoading = false;
		}
		TodParticleSystem aTrophyParticle = mApp.ParticleTryToGet(mTrophyParticleID);
		if (aTrophyParticle != null)
		{
			aTrophyParticle.ParticleSystemDie();
			mTrophyParticleID = null;
		}
		mLevel = 1;
		if (mApp.mPlayerInfo != null)
		{
			mLevel = mApp.mPlayerInfo.mLevel;
		}
		mShowStartButton = true;
		mMinigamesLocked = true;
		mPuzzleLocked = true;
		mSurvivalLocked = true;
		if (mApp.mPlayerInfo != null && !mApp.IsIceDemo())
		{
			if (mLevel >= 2)
			{
				mShowStartButton = false;
			}
			if (mApp.HasFinishedAdventure())
			{
				mMinigamesLocked = false;
				mSurvivalLocked = false;
				mPuzzleLocked = false;
				mShowStartButton = false;
			}
			if (mApp.mPlayerInfo.mHasUnlockedMinigames)
			{
				mMinigamesLocked = false;
			}
			if (mApp.mPlayerInfo.mHasUnlockedPuzzleMode)
			{
				mPuzzleLocked = false;
			}
			if (mApp.mPlayerInfo.mHasUnlockedSurvivalMode)
			{
				mSurvivalLocked = false;
			}
			if (mApp.IsTrialStageLocked())
			{
				mPuzzleLocked = true;
				mSurvivalLocked = true;
			}
		}
		if (mApp.HasFinishedAdventure() && !mApp.IsTrialStageLocked())
		{
			mHasTrophy = true;
		}
		else
		{
			mHasTrophy = false;
		}
		if (mHasTrophy && mSelectorState != SelectorAnimState.SELECTOR_OPEN)
		{
			AddTrophySparkle();
		}
		SyncButtons();
		AlmanacDialog.AlmanacInitForPlayer();
		Board.BoardInitForPlayer();
	}

	public override void Draw(Graphics g)
	{
		if (mApp.GetDialog(Dialogs.DIALOG_STORE) != null || mApp.GetDialog(Dialogs.DIALOG_ALMANAC) != null)
		{
			return;
		}
		g.SetLinearBlend(true);
		Reanimation aSelectorReanim = mApp.ReanimationGet(mSelectorReanimID);
		aSelectorReanim.DrawRenderGroup(g, 1);
		for (int i = 0; i < 6; i++)
		{
			mApp.ReanimationGet(mCloudReanimID[i]).Draw(g);
		}
		aSelectorReanim.DrawRenderGroup(g, ReanimatorXnaHelpers.RENDER_GROUP_NORMAL);
		if (mSelectorState == SelectorAnimState.SELECTOR_OPEN)
		{
			int aBGIdx = aSelectorReanim.FindTrackIndex("SelectorScreen_BG_Right");
			ReanimatorTransform aTransform;
			aSelectorReanim.GetCurrentTransform(aBGIdx, out aTransform, false);
			float aFractionalOffsetX = aTransform.mTransX % 1f;
			float aFractionalOffsetY = aTransform.mTransY % 1f;
			g.DrawImageF(mOptionsButton.mButtonImage, mOptionsButton.mX + mOptionsButton.mButtonOffsetX + aFractionalOffsetX, mOptionsButton.mY + mOptionsButton.mButtonOffsetY + aFractionalOffsetY);
			g.DrawImageF(mQuitButton.mButtonImage, mQuitButton.mX + mQuitButton.mButtonOffsetX + aFractionalOffsetX, mQuitButton.mY + mQuitButton.mButtonOffsetY + aFractionalOffsetY);
			g.DrawImageF(mHelpButton.mButtonImage, mHelpButton.mX + mHelpButton.mButtonOffsetX + aFractionalOffsetX, mHelpButton.mY + mHelpButton.mButtonOffsetY + aFractionalOffsetY);
		}
		if (mApp.mPlayerInfo != null && mApp.mPlayerInfo.mName.Length != 0 && mSelectorState != SelectorAnimState.SELECTOR_OPEN && mSelectorState != SelectorAnimState.SELECTOR_NEW_USER)
		{
			string aWelcomeStr = mApp.mPlayerInfo.mName + "!";
			int aSignIdx = aSelectorReanim.FindTrackIndex("woodsign1");
			SexyTransform2D aOverlayMatrix;
			aSelectorReanim.GetAttachmentOverlayMatrix(aSignIdx, out aOverlayMatrix);
			float aStringWidth = Resources.FONT_BRIANNETOD16.StringWidth(aWelcomeStr);
			SexyTransform2D aOffsetMatrix = default(SexyTransform2D);
			aOffsetMatrix.Translate(170.5f - (int)(aStringWidth * 0.5f) + mX, 102.5f + mY);
			TodCommon.TodDrawStringMatrix(g, Resources.FONT_BRIANNETOD16, (aOverlayMatrix * aOffsetMatrix).mMatrix, aWelcomeStr, new SexyColor(255, 245, 200));
		}
		if (mDebugText && mApp.mPlayerInfo != null)
		{
			string aText = "ACHIEVEMENTS DEBUG\n";
			for (int i = 0; i < Achievements.NUM_ACHIEVEMENTS; i++)
			{
				aText += "ACHIEVEMENT " + i + ": ";
				aText += i < mApp.mPlayerInfo.mEarnedAchievements.Length && mApp.mPlayerInfo.mEarnedAchievements[i] ? "TRUE\n" : "FALSE\n";
			}
			g.SetFont(Resources.FONT_BRIANNETOD12);
			TRect aShadowRect = new TRect(11, 91, 500, 400);
			TRect aDebugRect = new TRect(10, 89, 500, 400);
			g.SetColor(new SexyColor(0, 0, 0));
			g.WriteWordWrapped(aShadowRect, aText, 0, -1);
			g.SetColor(SexyColor.White);
			g.WriteWordWrapped(aDebugRect, aText, 0, -1);
		}
	}

	public void DrawOverlay(Graphics g)
	{
		g.SetLinearBlend(true);
		if (mApp.mPlayerInfo == null)
		{
			return;
		}
		if (!mApp.IsIceDemo() && !mShowStartButton)
		{
			int aOffsetX = 0;
			int aOffsetY = 0;
			if (mAdventureButton.mIsDown && mAdventureButton.mIsOver)
			{
				aOffsetX = 1;
				aOffsetY = 1;
			}
			Reanimation aSelectorReanim = mApp.ReanimationGet(mSelectorReanimID);
			int aRightIdx = aSelectorReanim.FindTrackIndex("SelectorScreen_BG_Right");
			ReanimatorTransform aTransform;
			aSelectorReanim.GetCurrentTransform(aRightIdx, out aTransform, false);
			float aTransAreaX = aTransform.mTransX + aOffsetX + Constants.Board_Additional_Width;
			float aTransAreaY = aTransform.mTransY + aOffsetY + Constants.Board_Background_Offset_Y;
			float aTransSubX = aTransAreaX;
			float aTransSubY = aTransAreaY;
			int aStage = TodCommon.ClampInt((mLevel - 1) / 10 + 1, 1, 6);
			int aSub = mLevel - (aStage - 1) * 10;
			if (mApp.IsTrialStageLocked() && (mLevel >= 25 || mApp.HasFinishedAdventure()))
			{
				aStage = 3;
				aSub = 4;
			}
			else
			{
				if (aStage == 1)
				{
					aTransAreaY += 1f;
				}
				else if (aStage == 4)
				{
					aTransAreaX -= 1f;
				}
				if (aSub == 3)
				{
					aTransSubX -= 1f;
				}
			}
			g.SetColorizeImages(true);
			g.SetColor(mAdventureButton.GetColor((int)GameButton.ButtonColours.COLOR_BKG));
			TodCommon.TodDrawImageCelF(g, Resources.IMAGE_SELECTORSCREEN_LEVELNUMBERS, aTransAreaX + 486f + 111f, aTransAreaY + 125f - 77.5f + 80f, aStage, 0);
			if (aSub < 10)
			{
				TodCommon.TodDrawImageCelF(g, Resources.IMAGE_SELECTORSCREEN_LEVELNUMBERS, aTransSubX + 512f + 111f, aTransSubY + 128f - 77.5f + 80f, aSub, 0);
			}
			else if (aSub == 10)
			{
				TodCommon.TodDrawImageCelF(g, Resources.IMAGE_SELECTORSCREEN_LEVELNUMBERS, aTransSubX + 506f + 111f, aTransSubY + 128f - 77.5f + 80f, 1, 0);
				TodCommon.TodDrawImageCelF(g, Resources.IMAGE_SELECTORSCREEN_LEVELNUMBERS, aTransSubX + 515f + 111f, aTransSubY + 129f - 77.5f + 80f, 0, 0);
			}
			g.SetColorizeImages(false);
			int aLeftIdx = aSelectorReanim.FindTrackIndex("SelectorScreen_BG_Left");
			ReanimatorTransform aTransformLeft;
			aSelectorReanim.GetCurrentTransform(aLeftIdx, out aTransformLeft, false);
			if (mHasTrophy)
			{
				int aAchievementOffset = GameConstants.HAS_ACHIEVEMENTS ? 0 : 45;
				if (mApp.EarnedGoldTrophy())
				{
					TodCommon.TodDrawImageCelF(g, Resources.IMAGE_SUNFLOWER_TROPHY, aTransformLeft.mTransX + 10f + Constants.Board_Additional_Width + 370f, aTransformLeft.mTransY + 350f + Constants.Board_Background_Offset_Y + 60f + aAchievementOffset, 1, 0);
				}
				else
				{
					TodCommon.TodDrawImageCelF(g, Resources.IMAGE_SUNFLOWER_TROPHY, aTransformLeft.mTransX + 10f + Constants.Board_Additional_Width + 370f, aTransformLeft.mTransY + 350f + Constants.Board_Background_Offset_Y + 60f + aAchievementOffset, 0, 0);
				}
				TodParticleSystem aTrophyParticle = mApp.ParticleTryToGet(mTrophyParticleID);
				if (aTrophyParticle != null)
				{
					aTrophyParticle.Draw(g, doScale: true);
				}
			}
		}
		if (mZenGardenButton.mVisible && mApp.mZenGarden.PlantsNeedWater())
		{
			g.DrawImage(Resources.IMAGE_PLANTSPEECHBUBBLE, mZenGardenButton.mX + 106, mZenGardenButton.mY + 36);
			g.DrawImage(Resources.IMAGE_WATERDROP, mZenGardenButton.mX + 123, mZenGardenButton.mY + 45);
		}
		Reanimation aHandReanim = mApp.ReanimationTryToGet(mHandReanimID);
		if (aHandReanim != null)
		{
			g.SetClipRect(0, 0, Constants.BOARD_WIDTH, Constants.BOARD_HEIGHT - 40);
			aHandReanim.Draw(g);
			g.ClearClipRect();
		}
		mApp.ReanimationGet(mLeafReanimID).Draw(g);
		for (int i = 0; i < 3; i++)
		{
			Reanimation aFlowerReanim = mApp.ReanimationGet(mFlowerReanimID[i]);
			if (aFlowerReanim.mTrackInstances[i] != null && aFlowerReanim.mTrackInstances[i].mBlendTransform != null && aFlowerReanim.mTrackInstances[i].mBlendTransform.mFrame == aFlowerReanim.mFrameCount)
			{
				break;
			}
			aFlowerReanim.Draw(g);
		}
		mToolTip.Draw(g);
	}

	public void UpdateTooltip()
	{
		if (!mApp.HasFinishedAdventure() || mApp.GetDialog(Dialogs.DIALOG_MESSAGE) != null)
		{
			return;
		}
		if (mHasTrophy)
		{
			int aMouseX = mApp.mWidgetManager.mLastMouseX;
			int aMouseY = mApp.mWidgetManager.mLastMouseY;
			if (new TRect(50 + Constants.Board_Additional_Width, 325 + Constants.Board_Background_Offset_Y, 85, GameConstants.HAS_ACHIEVEMENTS ? 142 : 225).Contains(aMouseX, aMouseY))
			{
				if (mApp.EarnedGoldTrophy())
				{
					mToolTip.SetLabel(mApp.Pluralize(mApp.mPlayerInfo.mFinishedAdventure, "[GOLD_SUNFLOWER_TOOLTIP]", "[GOLD_SUNFLOWER_TOOLTIP_PLURAL]"));
					mToolTip.mX = 32 + Constants.Board_Additional_Width;
					mToolTip.mY = 510 + Constants.Board_Background_Offset_Y;
					mToolTip.mVisible = true;
				}
				else
				{
					mToolTip.SetLabel("[SILVER_SUNFLOWER_TOOLTIP]");
					mToolTip.mX = 20 + Constants.Board_Additional_Width;
					mToolTip.mY = 495 + Constants.Board_Background_Offset_Y;
					mToolTip.mVisible = true;
				}
				return;
			}
		}
		mToolTip.mVisible = false;
		mToolTip.Update();
	}

	public override void Update()
	{
		base.Update();
		MarkDirty();
		UpdateTooltip();
		mApp.mZenGarden.UpdatePlantNeeds();
		string aState = string.Empty;
		if (mSelectorState == SelectorAnimState.SELECTOR_SUB_MENU)
		{
			if (mApp.mAchievementScreen != null)
			{
				aState = "[DISCORD_ACHIEVEMENTS]";
			}
		}
		else
		{
			aState = "[DISCORD_GAME_SELECTOR]";
		}
		mApp.UpdateDiscordState(aState);
		if (mMovementTimer > 0)
		{
			int aPosY = CalcYPos(mCurrentY, mDestinationY);
			int aPosX = CalcXPos(mCurrentX, mDestinationX);
			mY = aPosY;
			mX = aPosX;
			if (mApp.mAchievementScreen != null)
			{
				mApp.mAchievementScreen.mY = aPosY + mApp.mHeight - 1;
			}
			mOverlayWidget.mX = aPosX;
			mOverlayWidget.mY = aPosY;
			mAdventureButton.SetButtonOffset(aPosX, aPosY);
			mMinigameButton.SetButtonOffset(aPosX, aPosY);
			mPuzzleButton.SetButtonOffset(aPosX, aPosY);
			mOptionsButton.SetButtonOffset(aPosX, aPosY + 15);
			mQuitButton.SetButtonOffset(aPosX + 5, aPosY + 5);
			mHelpButton.SetButtonOffset(aPosX, aPosY + 30);
			mStoreButton.SetButtonOffset(aPosX, aPosY);
			mAlmanacButton.SetButtonOffset(aPosX, aPosY);
			mZenGardenButton.SetButtonOffset(aPosX, aPosY);
			mSurvivalButton.SetButtonOffset(aPosX, aPosY);
			mChangeUserButton.SetButtonOffset(aPosX, aPosY);
			mCreditsButton.SetButtonOffset(aPosX, aPosY);
			mAchievementButton.SetButtonOffset(aPosX, aPosY);
			mQuickPlayButton.SetButtonOffset(aPosX, aPosY);
			mAchievementButton.MarkDirty();
			mQuickPlayButton.MarkDirty();
			mOptionsButton.MarkDirty();
			mHelpButton.MarkDirty();
			mQuitButton.MarkDirty();
			mStoreButton.MarkDirty();
			mMovementTimer--;
		}
		else if (mMovementTimer == 0)
		{
			mCurrentY = mY;
			mCurrentX = mX;
			if (mEnableButtonsTransition)
			{
				mAdventureButton.SetDisabled(false);
				mMinigameButton.SetDisabled(false);
				mPuzzleButton.SetDisabled(false);
				mOptionsButton.SetDisabled(false);
				mQuitButton.SetDisabled(false);
				mHelpButton.SetDisabled(false);
				mChangeUserButton.SetDisabled(false);
				mCreditsButton.SetDisabled(false);
				mStoreButton.SetDisabled(false);
				mAlmanacButton.SetDisabled(false);
				mSurvivalButton.SetDisabled(false);
				mZenGardenButton.SetDisabled(false);
				mAchievementButton.SetDisabled(false);
				mQuickPlayButton.SetDisabled(false);
				mEnableButtonsTransition = false;
			}
		}
		TodParticleSystem aParticle = mApp.ParticleTryToGet(mTrophyParticleID);
		if (aParticle != null)
		{
			aParticle.Update();
		}
		if (mStartingGame)
		{
			mStartingGameCounter++;
			if (mStartingGameCounter > 450)
			{
				mApp.KillGameSelector();
				if (mApp.IsIceDemo())
				{
					mApp.PreNewGame(GameMode.GAMEMODE_CHALLENGE_ICE, theLookForSavedGame: false);
					return;
				}
				if (mApp.IsFirstTimeAdventureMode() && mLevel == 1)
				{
					mApp.PreNewGame(GameMode.GAMEMODE_INTRO, theLookForSavedGame: false);
					return;
				}
				if (ShouldDoZenTuturialBeforeAdventure())
				{
					mApp.PreNewGame(GameMode.GAMEMODE_CHALLENGE_ZEN_GARDEN, theLookForSavedGame: false);
					mApp.mZenGarden.SetupForZenTutorial();
					return;
				}
				mApp.PreNewGame(GameMode.GAMEMODE_ADVENTURE, theLookForSavedGame: true);
				return;
			}
			mAdventureButton.SetColor((int)GameButton.ButtonColours.COLOR_BKG, mStartingGameCounter % 20 < 10 ? new SexyColor(80, 80, 80) : SexyColor.White);
		}
		Reanimation aSelectorReanim = mApp.ReanimationGet(mSelectorReanimID);
		switch (mSelectorState)
		{
		case SelectorAnimState.SELECTOR_OPEN:
			if (mWidgetManager != null)
			{
				mWidgetManager.RehupMouse();
			}
			if (aSelectorReanim.mLoopCount > 0)
			{
				aSelectorReanim.AssignRenderGroupToTrack("SelectorScreen_Adventure_button", ReanimatorXnaHelpers.RENDER_GROUP_HIDDEN);
				aSelectorReanim.AssignRenderGroupToTrack("SelectorScreen_StartAdventure_button", ReanimatorXnaHelpers.RENDER_GROUP_HIDDEN);
				aSelectorReanim.AssignRenderGroupToTrack("SelectorScreen_Survival_button", ReanimatorXnaHelpers.RENDER_GROUP_HIDDEN);
				aSelectorReanim.AssignRenderGroupToTrack("SelectorScreen_Challenges_button", ReanimatorXnaHelpers.RENDER_GROUP_HIDDEN);
				aSelectorReanim.AssignRenderGroupToTrack("SelectorScreen_ZenGarden_button", ReanimatorXnaHelpers.RENDER_GROUP_HIDDEN);
				mMinigameButton.mBtnNoDraw = false;
				mPuzzleButton.mBtnNoDraw = false;
				mSurvivalButton.mBtnNoDraw = false;
				mZenGardenButton.mBtnNoDraw = false;
				mHelpButton.mBtnNoDraw = false;
				mOptionsButton.mBtnNoDraw = false;
				mQuitButton.mBtnNoDraw = false;
				mQuickPlayButton.mBtnNoDraw = false;
				mAdventureButton.mMouseVisible = true;
				mMinigameButton.mMouseVisible = true;
				mPuzzleButton.mMouseVisible = true;
				mSurvivalButton.mMouseVisible = true;
				mZenGardenButton.mMouseVisible = true;
				mHelpButton.mMouseVisible = true;
				mOptionsButton.mMouseVisible = true;
				mQuitButton.mMouseVisible = true;
				mStoreButton.mMouseVisible = true;
				mAlmanacButton.mMouseVisible = true;
				mChangeUserButton.mMouseVisible = true;
				mCreditsButton.mMouseVisible = true;
				mAchievementButton.mMouseVisible = true;
				mQuickPlayButton.mMouseVisible = true;
			if (mApp.mPlayerInfo == null)
			{
				mApp.DoCreateUserDialog(isOnlyUser: true);
				if (GlobalStaticVars.gIsPartnerBuild)
				{
					AddPreviewProfiles();
				}
				mSelectorState = SelectorAnimState.SELECTOR_NEW_USER;
			}
				else
				{
					aSelectorReanim.PlayReanim("anim_sign", ReanimLoopType.REANIM_PLAY_ONCE_AND_HOLD, 0, 30f);
					mSelectorState = SelectorAnimState.SELECTOR_IDLE;
				}
				if (mHasTrophy)
				{
					AddTrophySparkle();
				}
		if (mApp.mPlayerInfo != null && mApp.mPlayerInfo.mNeedsMessageOnGameSelector)
		{
			mApp.mPlayerInfo.mNeedsMessageOnGameSelector = false;
					mApp.WriteCurrentUserConfig();
					mApp.LawnMessageBox((int)Dialogs.DIALOG_MESSAGE, "[ADVENTURE_COMPLETE_HEADER]", "[ADVENTURE_COMPLETE_BODY]", "[DIALOG_BUTTON_OK]", "", (int)Dialog.DialogButtons.BUTTONS_FOOTER, this);
				}
			}
			break;
		case SelectorAnimState.SELECTOR_NEW_USER:
			if (mApp.GetDialog(Dialogs.DIALOG_CREATEUSER) == null)
			{
				aSelectorReanim.PlayReanim("anim_sign", ReanimLoopType.REANIM_PLAY_ONCE_AND_HOLD, 0, 30f);
				mSelectorState = SelectorAnimState.SELECTOR_SHOW_SIGN;
			}
			break;
		case SelectorAnimState.SELECTOR_SHOW_SIGN:
			if (aSelectorReanim.mLoopCount > 0)
			{
				mSelectorState = SelectorAnimState.SELECTOR_IDLE;
			}
			break;
		}
		for (int i = 0; i < 6; i++)
		{
			Reanimation aCloudReanim = mApp.ReanimationGet(mCloudReanimID[i]);
			if (mCloudCounter[i] > 0)
			{
				if (--mCloudCounter[i] == 0)
				{
					aCloudReanim.mLoopCount = 0;
					aCloudReanim.mAnimTime = 0f;
					aCloudReanim.mAnimRate = 0.5f;
				}
			}
			else if (aCloudReanim.mLoopCount > 0)
			{
				mCloudCounter[i] = TodCommon.RandRangeInt(2000, 4000);
			}
		}
		aSelectorReanim.Update();
		Reanimation aLeafReanim = mApp.ReanimationGet(mLeafReanimID);
		aLeafReanim.Update();
		int aLeafTrackIndex = aSelectorReanim.FindTrackIndex("SelectorScreen_BG_Right");
		ReanimatorTransform aLeafTransform;
		aSelectorReanim.GetCurrentTransform(aLeafTrackIndex, out aLeafTransform, false);
		aLeafReanim.SetPosition(aLeafTransform.mTransX - 71f + Constants.Board_Additional_Width, aLeafTransform.mTransY - 41f + Constants.Board_Background_Offset_Y);
		if (--mLeafCounter == 0)
		{
			float aRate = TodCommon.RandRangeFloat(3f, 12f);
			aLeafReanim.PlayReanim("anim_grass", ReanimLoopType.REANIM_LOOP, 20, aRate);
			mLeafCounter = TodCommon.RandRangeInt(200, 400);
		}
		for (int i = 0; i < 6; i++)
		{
			mApp.ReanimationGet(mCloudReanimID[i]).Update();
		}
		Reanimation aHandReanim = mApp.ReanimationTryToGet(mHandReanimID);
		if (aHandReanim != null)
		{
			aHandReanim.Update();
		}
		if (mSelectorState != SelectorAnimState.SELECTOR_SUB_MENU)
		{
			if (mMovementTimer == 0)
			{
				mApp.KillAchievementScreen();
				mApp.KillQuickPlayScreen();
			}
			TrackButton(mAdventureButton, mShowStartButton ? "SelectorScreen_StartAdventure_button" : "SelectorScreen_Adventure_button", Constants.Board_Additional_Width, Constants.Board_Background_Offset_Y);
			TrackButton(mMinigameButton, "SelectorScreen_Survival_button", Constants.Board_Additional_Width, Constants.Board_Background_Offset_Y);
			TrackButton(mPuzzleButton, "SelectorScreen_Challenges_button", Constants.Board_Additional_Width, Constants.Board_Background_Offset_Y);
			TrackButton(mSurvivalButton, "SelectorScreen_ZenGarden_button", Constants.Board_Additional_Width, Constants.Board_Background_Offset_Y);
			TrackButton(mZenGardenButton, "SelectorScreen_BG_Right", 100f + Constants.Board_Additional_Width + Constants.GameSelector_Extra.X, 360f + Constants.Board_Background_Offset_Y + Constants.GameSelector_Extra.Y);
			TrackButton(mOptionsButton, "SelectorScreen_BG_Right", 494f + Constants.Board_Additional_Width + Constants.GameSelector_Extra.X, 434f + Constants.Board_Background_Offset_Y + Constants.GameSelector_Extra.Y);
			TrackButton(mQuitButton, "SelectorScreen_BG_Right", 644f + Constants.Board_Additional_Width + Constants.GameSelector_Extra.X, 469f + Constants.Board_Background_Offset_Y + Constants.GameSelector_Extra.Y);
			TrackButton(mHelpButton, "SelectorScreen_BG_Right", 576f + Constants.Board_Additional_Width + Constants.GameSelector_Extra.X, 458f + Constants.Board_Background_Offset_Y + Constants.GameSelector_Extra.Y);
			TrackButton(mAlmanacButton, "SelectorScreen_BG_Right", 256f + Constants.Board_Additional_Width + Constants.GameSelector_Extra.X, 387f + Constants.Board_Background_Offset_Y + Constants.GameSelector_Extra.Y);
			TrackButton(mStoreButton, "SelectorScreen_BG_Right", 334f + Constants.Board_Additional_Width + Constants.GameSelector_Extra.X, 441f + Constants.Board_Background_Offset_Y + Constants.GameSelector_Extra.Y);
			TrackButton(mChangeUserButton, "woodsign2", 24f + Constants.Board_Additional_Width, 10f + Constants.Board_Background_Offset_Y);
			TrackButton(mCreditsButton, "woodsign3", Constants.Board_Additional_Width, Constants.Board_Background_Offset_Y);
			TrackButton(mAchievementButton, "SelectorScreen_BG_Left", Constants.GameSelector_Achievement_Off.X + Constants.Board_Additional_Width, Constants.GameSelector_Achievement_Off.Y + Constants.Board_Background_Offset_Y);
			TrackButton(mQuickPlayButton, "SelectorScreen_BG_Right", Constants.GameSelector_QuickPlay_Off.X + Constants.Board_Additional_Width, Constants.GameSelector_QuickPlay_Off.Y + Constants.Board_Background_Offset_Y);
			aSelectorReanim.SetImageOverride("woodsign2", mChangeUserButton.mIsOver || mChangeUserButton.mIsDown ? Resources.IMAGE_REANIM_SELECTORSCREEN_WOODSIGN2_PRESS : null);
			aSelectorReanim.SetImageOverride("woodsign3", mCreditsButton.mIsOver || mCreditsButton.mIsDown ? Resources.IMAGE_REANIM_SELECTORSCREEN_WOODSIGN3_PRESS : null);
		}
	}

	public void TrackButton(NewLawnButton theButton, string theTrackName, float theOffsetX, float theOffsetY)
	{
		Reanimation aSelectorReanim = mApp.ReanimationGet(mSelectorReanimID);
		int aTrackIndex = aSelectorReanim.FindTrackIndex(theTrackName);
		ReanimatorTransform aTransform;
		aSelectorReanim.GetCurrentTransform(aTrackIndex, out aTransform, false);
		theButton.mX = (int)(aTransform.mTransX + theOffsetX);
		theButton.mY = (int)(aTransform.mTransY + theOffsetY);
	}

	public override void AddedToManager(WidgetManager theWidgetManager)
	{
		base.AddedToManager(theWidgetManager);
		theWidgetManager.AddWidget(mAdventureButton);
		theWidgetManager.AddWidget(mMinigameButton);
		theWidgetManager.AddWidget(mPuzzleButton);
		theWidgetManager.AddWidget(mOptionsButton);
		theWidgetManager.AddWidget(mQuitButton);
		theWidgetManager.AddWidget(mHelpButton);
		theWidgetManager.AddWidget(mStoreButton);
		theWidgetManager.AddWidget(mAlmanacButton);
		theWidgetManager.AddWidget(mSurvivalButton);
		theWidgetManager.AddWidget(mZenGardenButton);
		theWidgetManager.AddWidget(mChangeUserButton);
		theWidgetManager.AddWidget(mCreditsButton);
		theWidgetManager.AddWidget(mOverlayWidget);
		if (GameConstants.HAS_ACHIEVEMENTS)
		{
			theWidgetManager.AddWidget(mAchievementButton);
		}
		if (GameConstants.HAS_QUICKPLAY)
		{
			theWidgetManager.AddWidget(mQuickPlayButton);
		}
	}

	public override void RemovedFromManager(WidgetManager theWidgetManager)
	{
		base.RemovedFromManager(theWidgetManager);
		theWidgetManager.RemoveWidget(mAdventureButton);
		theWidgetManager.RemoveWidget(mMinigameButton);
		theWidgetManager.RemoveWidget(mPuzzleButton);
		theWidgetManager.RemoveWidget(mOptionsButton);
		theWidgetManager.RemoveWidget(mQuitButton);
		theWidgetManager.RemoveWidget(mHelpButton);
		theWidgetManager.RemoveWidget(mStoreButton);
		theWidgetManager.RemoveWidget(mAlmanacButton);
		theWidgetManager.RemoveWidget(mSurvivalButton);
		theWidgetManager.RemoveWidget(mZenGardenButton);
		theWidgetManager.RemoveWidget(mChangeUserButton);
		theWidgetManager.RemoveWidget(mCreditsButton);
		theWidgetManager.RemoveWidget(mOverlayWidget);
		if (GameConstants.HAS_ACHIEVEMENTS)
		{
			theWidgetManager.RemoveWidget(mAchievementButton);
		}
		if (GameConstants.HAS_QUICKPLAY)
		{
			theWidgetManager.RemoveWidget(mQuickPlayButton);
		}
	}

	public override void OrderInManagerChanged()
	{
		mWidgetManager.PutInfront(mOverlayWidget, this);
		mWidgetManager.PutInfront(mAlmanacButton, this);
		mWidgetManager.PutInfront(mStoreButton, this);
		mWidgetManager.PutInfront(mHelpButton, this);
		mWidgetManager.PutInfront(mQuitButton, this);
		mWidgetManager.PutInfront(mOptionsButton, this);
		mWidgetManager.PutInfront(mAdventureButton, this);
		mWidgetManager.PutInfront(mMinigameButton, this);
		mWidgetManager.PutInfront(mPuzzleButton, this);
		mWidgetManager.PutInfront(mZenGardenButton, this);
		mWidgetManager.PutInfront(mSurvivalButton, this);
		mWidgetManager.PutInfront(mChangeUserButton, this);
		mWidgetManager.PutInfront(mCreditsButton, this);
		if (GameConstants.HAS_ACHIEVEMENTS)
		{
			mWidgetManager.PutInfront(mAchievementButton, this);
		}
		if (GameConstants.HAS_QUICKPLAY)
		{
			mWidgetManager.PutInfront(mQuickPlayButton, this);
		}
	}

	public override void KeyDown(KeyCode theKey)
	{
		if (mApp.mKonamiCheck.Check(theKey))
		{
			mApp.PlayFoley(FoleyType.FOLEY_DROP);
			return;
		}
		if (mApp.mMustacheCheck.Check(theKey) || mApp.mMoustacheCheck.Check(theKey))
		{
			mApp.PlayFoley(FoleyType.FOLEY_POLEVAULT);
			mApp.mMustacheMode = !mApp.mMustacheMode;
			return;
		}
		if (mApp.mSuperMowerCheck.Check(theKey) || mApp.mSuperMowerCheck2.Check(theKey))
		{
			mApp.PlayFoley(FoleyType.FOLEY_ZAMBONI);
			mApp.mSuperMowerMode = !mApp.mSuperMowerMode;
			return;
		}
		if (mApp.mFutureCheck.Check(theKey))
		{
			mApp.PlaySample(Resources.SOUND_BOING);
			mApp.mFutureMode = !mApp.mFutureMode;
			return;
		}
		if (mApp.mPinataCheck.Check(theKey))
		{
			if (mApp.CanDoPinataMode())
			{
				mApp.PlayFoley(FoleyType.FOLEY_JUICY);
				mApp.mPinataMode = !mApp.mPinataMode;
			}
			else
			{
				mApp.PlaySample(Resources.SOUND_BUZZER);
			}
			return;
		}
		if (mApp.mDanceCheck.Check(theKey))
		{
			if (mApp.CanDoDanceMode())
			{
				mApp.PlayFoley(FoleyType.FOLEY_DANCER);
				mApp.mDanceMode = !mApp.mDanceMode;
			}
			else
			{
				mApp.PlaySample(Resources.SOUND_BUZZER);
			}
			return;
		}
		if (mApp.mDaisyCheck.Check(theKey))
		{
			if (mApp.CanDoDaisyMode())
			{
				mApp.PlaySample(Resources.SOUND_LOADINGBAR_FLOWER);
				mApp.mDaisyMode = !mApp.mDaisyMode;
			}
			else
			{
				mApp.PlaySample(Resources.SOUND_BUZZER);
			}
			return;
		}
		if (mApp.mSukhbirCheck.Check(theKey))
		{
			mApp.PlaySample(Resources.SOUND_SUKHBIR4);
			mApp.mSukhbirMode = !mApp.mSukhbirMode;
			return;
		}
	}

	public override void KeyChar(SexyChar theChar)
	{
		if (mStartingGame)
		{
			return;
		}
		if ((GlobalStaticVars.gIsPartnerBuild || mDebugKeysEnabled) && theChar.value_type == 'u' && mApp.mPlayerInfo != null)
		{
			mApp.mPlayerInfo.mFinishedAdventure = 2;
			mApp.mPlayerInfo.AddCoins(50000);
			mApp.mPlayerInfo.mHasUsedCheatKeys = true;
			mApp.mPlayerInfo.mHasUnlockedMinigames = true;
			mApp.mPlayerInfo.mHasUnlockedPuzzleMode = true;
			mApp.mPlayerInfo.mHasUnlockedSurvivalMode = true;
			for (int i = 1; i < 100; i++)
			{
				GameMode gameMode = (GameMode)i;
				if (gameMode != GameMode.GAMEMODE_TREE_OF_WISDOM && gameMode != GameMode.GAMEMODE_SCARY_POTTER_ENDLESS && gameMode != GameMode.GAMEMODE_PUZZLE_I_ZOMBIE_ENDLESS && gameMode != GameMode.GAMEMODE_SURVIVAL_ENDLESS_STAGE_3)
				{
					mApp.mPlayerInfo.mChallengeRecords[i - 1] = 20;
				}
			}
			SyncProfile(theShowLoading: true);
			mApp.EraseFile(LawnCommon.GetSavedGameName(GameMode.GAMEMODE_ADVENTURE, (int)mApp.mPlayerInfo.mId));
		}
		if (mDebugKeysEnabled)
		{
			if (theChar.value_type == 'c' || theChar.value_type == 'C')
			{
				mMinigamesLocked = false;
				mPuzzleLocked = false;
				mSurvivalLocked = false;
				SyncButtons();
			}
			if (theChar.value_type == 'a')
			{
				mDebugText = !mDebugText;
				return;
			}
			if (theChar.value_type == 'p')
			{
				AddPreviewProfiles();
			}
		}
	}

	public override void MouseDown(int x, int y, int theClickCount)
	{
		for (int i = 0; i < 3; i++)
		{
			Reanimation aFlowerReanim = mApp.ReanimationGet(mFlowerReanimID[i]);
			if (aFlowerReanim.mAnimRate <= 0f && TodCommon.Distance2D(x - Constants.Board_Additional_Width, y - Constants.Board_Background_Offset_Y, gFlowerCenter[i, 0], gFlowerCenter[i, 1]) < 20f)
			{
				aFlowerReanim.mAnimRate = 24f;
				mApp.PlayFoley(FoleyType.FOLEY_LIMBS_POP);
			}
		}
		if (mApp.mTodCheatKeys && mStartingGame && mStartingGameCounter < 450)
		{
			mStartingGameCounter = 450;
		}
	}

	public void ButtonMouseEnter(int theId)
	{
		if ((theId == 101 && mMinigamesLocked) || (theId == 102 && mPuzzleLocked) || (theId == 110 && mSurvivalLocked))
		{
			return;
		}
		mApp.PlayFoley(FoleyType.FOLEY_BLEEP);
	}

	public virtual void ButtonPress(int theId)
	{
		if (theId == 100 || theId == 101 || theId == 102 || theId == 110)
		{
			mApp.PlaySample(Resources.SOUND_GRAVEBUTTON);
		}
		else
		{
			mApp.PlaySample(Resources.SOUND_TAP);
		}
	}

	public virtual void ButtonPress(int theId, int theClickCount)
	{
		ButtonPress(theId);
	}

	public virtual void ButtonDownTick(int theId)
	{
	}

	public virtual void ButtonMouseLeave(int theId)
	{
	}

	public virtual void ButtonMouseMove(int theId, int theX, int theY)
	{
	}

	public void ClickedAdventure()
	{
		if (mApp.IsTrialStageLocked() && (mLevel >= 25 || mApp.HasFinishedAdventure()))
		{
			mWaitingTrialConfirm = true;
			mApp.LawnMessageBox((int)Dialogs.DIALOG_MESSAGE, "[REPLAY_LEVEL_HEADER]", "[REPLAY_LEVEL_BODY]", "[DIALOG_BUTTON_YES]", "[DIALOG_BUTTON_NO]", (int)Dialog.DialogButtons.BUTTONS_YES_NO, this);
			return;
		}
		StartAdventureAfterConfirm();
	}

	public virtual void LawnMessageBoxDone(int theResult)
	{
		if (!mWaitingTrialConfirm)
		{
			return;
		}
		mWaitingTrialConfirm = false;
		if (theResult == (int)Dialog.DialogResult.ID_NO)
		{
			return;
		}
		mApp.mPlayerInfo.SetLevel(24);
		mApp.mPlayerInfo.mFinishedAdventure = 0;
		mApp.EraseFile(LawnCommon.GetSavedGameName(GameMode.GAMEMODE_ADVENTURE, (int)mApp.mPlayerInfo.mId));
		StartAdventureAfterConfirm();
	}

	public void StartAdventureAfterConfirm()
	{
		mApp.mMusic.StopAllMusic();
		mApp.PlaySample(Resources.SOUND_LOSEMUSIC);
		mStartingGame = true;
		mAdventureButton.SetDisabled(true);
		mMinigameButton.SetDisabled(true);
		mPuzzleButton.SetDisabled(true);
		mOptionsButton.SetDisabled(true);
		mQuitButton.SetDisabled(true);
		mHelpButton.SetDisabled(true);
		mChangeUserButton.SetDisabled(true);
		mCreditsButton.SetDisabled(true);
		mStoreButton.SetDisabled(true);
		mAlmanacButton.SetDisabled(true);
		mSurvivalButton.SetDisabled(true);
		mZenGardenButton.SetDisabled(true);
		mAchievementButton.SetDisabled(true);
		mQuickPlayButton.SetDisabled(true);
		Reanimation aHandReanim = mApp.AddReanimation(-70f + Constants.Board_Additional_Width, 10f + Constants.Board_Background_Offset_Y, 0, ReanimationType.REANIM_ZOMBIE_HAND);
		aHandReanim.mLoopType = ReanimLoopType.REANIM_PLAY_ONCE_AND_HOLD;
		mHandReanimID = mApp.ReanimationGetID(aHandReanim);
		mApp.PlayFoley(FoleyType.FOLEY_DIRT_RISE);
		for (int i = 0; i < aHandReanim.mDefinition.mTrackCount; i++)
		{
			if (aHandReanim.mDefinition.mTracks[i].mName.StartsWith("rock", StringComparison.OrdinalIgnoreCase))
			{
				aHandReanim.mTrackInstances[i].mIgnoreClipRect = true;
			}
		}
	}

	public bool ShouldDoZenTuturialBeforeAdventure()
	{
		if (!mApp.HasFinishedAdventure() && mApp.mPlayerInfo.mLevel == 45 && mApp.mPlayerInfo.mNumPottedPlants == 0)
		{
			return true;
		}
		return false;
	}

	public virtual void ButtonDepress(int theId)
	{
		if (theId == 101 && mMinigamesLocked)
		{
			mApp.LawnMessageBox((int)Dialogs.DIALOG_MESSAGE, "[MODE_LOCKED]", "[MINIGAME_LOCKED_MESSAGE]", "[DIALOG_BUTTON_OK]", "", (int)Dialog.DialogButtons.BUTTONS_FOOTER, this);
			return;
		}
		if (theId == 102 && mPuzzleLocked)
		{
			mApp.LawnMessageBox((int)Dialogs.DIALOG_MESSAGE, "[MODE_LOCKED]", "[PUZZLE_LOCKED_MESSAGE]", "[DIALOG_BUTTON_OK]", "", (int)Dialog.DialogButtons.BUTTONS_FOOTER, this);
			return;
		}
		if (theId == 110 && mSurvivalLocked)
		{
			mApp.LawnMessageBox((int)Dialogs.DIALOG_MESSAGE, "[MODE_LOCKED]", "[SURVIVAL_LOCKED_MESSAGE]", "[DIALOG_BUTTON_OK]", "", (int)Dialog.DialogButtons.BUTTONS_FOOTER, this);
			return;
		}
		switch (theId)
		{
		case 100:
			ClickedAdventure();
			break;
		case 101:
			mApp.KillGameSelector();
			mApp.ShowChallengeScreen(ChallengePage.CHALLENGE_PAGE_CHALLENGE);
			break;
		case 102:
			mApp.KillGameSelector();
			mApp.ShowChallengeScreen(ChallengePage.CHALLENGE_PAGE_PUZZLE);
			break;
		case 110:
			mApp.KillGameSelector();
			mApp.ShowChallengeScreen(ChallengePage.CHALLENGE_PAGE_SURVIVAL);
			break;
		case 105:
			mApp.ConfirmQuit();
			break;
		case 104:
			mApp.KillGameSelector();
			mApp.ShowAwardScreen(AwardType.AWARD_HELP_ZOMBIE_NOTE, theShowAchievements: false);
			break;
		case 103:
			mApp.DoNewOptions(theFromGameSelector: true);
			break;
		case 106:
			mApp.DoUserDialog();
			break;
	{
			mApp.mMusic.MakeSureMusicIsPlaying(MusicTune.MUSIC_TUNE_TITLE_CRAZY_DAVE_MAIN_THEME);
			break;
	}
		case 108:
			mApp.DoAlmanacDialog().WaitForResult(true);
			mApp.mMusic.MakeSureMusicIsPlaying(MusicTune.MUSIC_TUNE_TITLE_CRAZY_DAVE_MAIN_THEME);
			break;
		case 111:
			mMovementTimer = 75;
			mDestinationY = -mApp.mHeight;
			mSelectorState = SelectorAnimState.SELECTOR_SUB_MENU;
			mApp.ShowAchievementScreen();
			mAdventureButton.SetDisabled(true);
			mMinigameButton.SetDisabled(true);
			mPuzzleButton.SetDisabled(true);
			mOptionsButton.SetDisabled(true);
			mQuitButton.SetDisabled(true);
			mHelpButton.SetDisabled(true);
			mChangeUserButton.SetDisabled(true);
			mCreditsButton.SetDisabled(true);
			mStoreButton.SetDisabled(true);
			mAlmanacButton.SetDisabled(true);
			mSurvivalButton.SetDisabled(true);
			mZenGardenButton.SetDisabled(true);
			mAchievementButton.SetDisabled(true);
			mQuickPlayButton.SetDisabled(true);
			break;
		case 112:
			mMovementTimer = 75;
			mDestinationX = -mApp.mWidth;
			mSelectorState = SelectorAnimState.SELECTOR_SUB_MENU;
			mApp.ShowQuickPlayScreen();
			mAdventureButton.SetDisabled(true);
			mMinigameButton.SetDisabled(true);
			mPuzzleButton.SetDisabled(true);
			mOptionsButton.SetDisabled(true);
			mQuitButton.SetDisabled(true);
			mHelpButton.SetDisabled(true);
			mChangeUserButton.SetDisabled(true);
			mCreditsButton.SetDisabled(true);
			mStoreButton.SetDisabled(true);
			mAlmanacButton.SetDisabled(true);
			mSurvivalButton.SetDisabled(true);
			mZenGardenButton.SetDisabled(true);
			mAchievementButton.SetDisabled(true);
			mQuickPlayButton.SetDisabled(true);
			break;
		case 109:
			mApp.KillGameSelector();
			mApp.PreNewGame(GameMode.GAMEMODE_CHALLENGE_ZEN_GARDEN, theLookForSavedGame: false);
			if (ShouldDoZenTuturialBeforeAdventure())
			{
				mApp.mZenGarden.SetupForZenTutorial();
			}
			break;
		case 113:
			mApp.KillGameSelector();
			mApp.ShowCreditScreen();
			break;
		}
	}

	public int CalcYPos(int ogY, int theY)
	{
		mDestinationY = theY;
		return TodCommon.TodAnimateCurve(75, 0, mMovementTimer, mCurrentY, mDestinationY, TodCurves.CURVE_EASE_IN_OUT);
	}

	public int CalcXPos(int ogX, int theX)
	{
		mDestinationX = theX;
		return TodCommon.TodAnimateCurve(75, 0, mMovementTimer, mCurrentX, mDestinationX, TodCurves.CURVE_EASE_IN_OUT);
	}

	public void AddPreviewProfiles()
	{
		PlayerInfo aProfile;
		aProfile = mApp.mProfileMgr.AddProfile("2 Night");
		if (aProfile != null)
		{
			aProfile.SetLevel(11);
			aProfile.SaveDetails();
		}
		aProfile = mApp.mProfileMgr.AddProfile("3 Pool");
		if (aProfile != null)
		{
			aProfile.SetLevel(21);
			aProfile.mHasUnlockedMinigames = true;
			aProfile.mCoins = 400;
			aProfile.mPurchases[(int)StoreItem.STORE_ITEM_PACKET_UPGRADE] = 1;
			aProfile.SaveDetails();
		}
		aProfile = mApp.mProfileMgr.AddProfile("4 Fog");
		if (aProfile != null)
		{
			aProfile.SetLevel(31);
			aProfile.mHasUnlockedMinigames = true;
			aProfile.mHasUnlockedSurvivalMode = true;
			aProfile.mPurchases[(int)StoreItem.STORE_ITEM_PACKET_UPGRADE] = 2;
			aProfile.mPurchases[(int)StoreItem.STORE_ITEM_POOL_CLEANER] = 1;
			aProfile.mCoins = 400;
			aProfile.SaveDetails();
		}
		aProfile = mApp.mProfileMgr.AddProfile("5 Roof");
		if (aProfile != null)
		{
			aProfile.SetLevel(41);
			aProfile.mHasUnlockedMinigames = true;
			aProfile.mHasUnlockedPuzzleMode = true;
			aProfile.mHasUnlockedSurvivalMode = true;
			aProfile.mPurchases[(int)StoreItem.STORE_ITEM_PACKET_UPGRADE] = 2;
			aProfile.mPurchases[(int)StoreItem.STORE_ITEM_POOL_CLEANER] = 1;
			aProfile.mCoins = 500;
			aProfile.SaveDetails();
		}
		aProfile = mApp.mProfileMgr.AddProfile("Complete");
		if (aProfile != null)
		{
			aProfile.SetLevel(1);
			aProfile.mFinishedAdventure = 1;
			aProfile.mHasUnlockedMinigames = true;
			aProfile.mHasUnlockedPuzzleMode = true;
			aProfile.mHasUnlockedSurvivalMode = true;
			aProfile.mPurchases[(int)StoreItem.STORE_ITEM_PACKET_UPGRADE] = 2;
			aProfile.mPurchases[(int)StoreItem.STORE_ITEM_POOL_CLEANER] = 1;
			aProfile.mPurchases[(int)StoreItem.STORE_ITEM_ROOF_CLEANER] = 1;
			aProfile.mCoins = 1000;
			aProfile.SaveDetails();
		}
		aProfile = mApp.mProfileMgr.AddProfile("Full Unlock");
		if (aProfile != null)
		{
			aProfile.SetLevel(1);
			aProfile.mFinishedAdventure = 2;
			aProfile.AddCoins(50000);
			aProfile.mPurchases[(int)StoreItem.STORE_ITEM_FERTILIZER] = 1000 + 5;
			aProfile.mPurchases[(int)StoreItem.STORE_ITEM_BUG_SPRAY] = 1000 + 5;
			aProfile.mPurchases[(int)StoreItem.STORE_ITEM_CHOCOLATE] = 1000 + 5;
			aProfile.mPurchases[(int)StoreItem.STORE_ITEM_TREE_FOOD] = 1000 + 5;
			aProfile.mHasUnlockedMinigames = true;
			aProfile.mHasUnlockedPuzzleMode = true;
			aProfile.mPurchases[(int)StoreItem.STORE_ITEM_PLANT_GATLINGPEA] = 1;
			aProfile.mPurchases[(int)StoreItem.STORE_ITEM_PLANT_TWINSUNFLOWER] = 1;
			aProfile.mPurchases[(int)StoreItem.STORE_ITEM_PLANT_GLOOMSHROOM] = 1;
			aProfile.mPurchases[(int)StoreItem.STORE_ITEM_PLANT_CATTAIL] = 1;
			aProfile.mPurchases[(int)StoreItem.STORE_ITEM_PLANT_WINTERMELON] = 1;
			aProfile.mPurchases[(int)StoreItem.STORE_ITEM_PLANT_GOLD_MAGNET] = 1;
			aProfile.mPurchases[(int)StoreItem.STORE_ITEM_PLANT_SPIKEROCK] = 1;
			aProfile.mPurchases[(int)StoreItem.STORE_ITEM_PLANT_COBCANNON] = 1;
			aProfile.mPurchases[(int)StoreItem.STORE_ITEM_PLANT_IMITATER] = 1;
			aProfile.mPurchases[(int)StoreItem.STORE_ITEM_PACKET_UPGRADE] = 3;
			aProfile.mPurchases[(int)StoreItem.STORE_ITEM_POOL_CLEANER] = 1;
			aProfile.mPurchases[(int)StoreItem.STORE_ITEM_ROOF_CLEANER] = 1;
			aProfile.mPurchases[(int)StoreItem.STORE_ITEM_PHONOGRAPH] = 1;
			aProfile.mPurchases[(int)StoreItem.STORE_ITEM_GARDENING_GLOVE] = 1;
			aProfile.mPurchases[(int)StoreItem.STORE_ITEM_MUSHROOM_GARDEN] = 1;
			aProfile.mPurchases[(int)StoreItem.STORE_ITEM_WHEEL_BARROW] = 1;
			aProfile.mPurchases[(int)StoreItem.STORE_ITEM_AQUARIUM_GARDEN] = 1;
			aProfile.mPurchases[(int)StoreItem.STORE_ITEM_TREE_OF_WISDOM] = 1;
			aProfile.mChallengeRecords[(int)GameMode.GAMEMODE_TREE_OF_WISDOM - 1] = 1;
			for (int i = 1; i < 100; i++)
			{
				GameMode gameMode = (GameMode)i;
				if (gameMode != GameMode.GAMEMODE_TREE_OF_WISDOM && gameMode != GameMode.GAMEMODE_SCARY_POTTER_ENDLESS && gameMode != GameMode.GAMEMODE_PUZZLE_I_ZOMBIE_ENDLESS && gameMode != GameMode.GAMEMODE_SURVIVAL_ENDLESS_STAGE_3)
				{
					aProfile.mChallengeRecords[i - 1] = 20;
				}
			}
			aProfile.SaveDetails();
		}
	}

	public void BackFromStore()
	{
		mApp.KillDialog(4);
		if (mDoNewGameAfterStore)
		{
			mApp.PreNewGame(GameMode.GAMEMODE_ADVENTURE, theLookForSavedGame: false);
		}
		else
		{
			mApp.mMusic.MakeSureMusicIsPlaying(MusicTune.MUSIC_TUNE_TITLE_CRAZY_DAVE_MAIN_THEME);
		}
	}

}
