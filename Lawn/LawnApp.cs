using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Media;
using XnaMediaPlayer = Microsoft.Xna.Framework.Media.MediaPlayer;
using Sexy;
using Sexy.TodLib;

namespace Lawn;

internal class LawnApp : SexyAppBase
{
	private class TableTmp
	{
		public int mNormalImageId;

		public int mAdditiveImageId;

		public TableTmp(int aNormal, int aAdditive)
		{
			mNormalImageId = aNormal;
			mAdditiveImageId = aAdditive;
		}
	}

	private const string PLACEHOLDER_PLAYER = "{PLAYER_NAME}";

	private const string PLACEHOLDER_MONEY = "{MONEY}";

	private const string PLACEHOLDER_UPGRADECOST = "{UPGRADE_COST}";

	private const string PLACEHOLDER_CRAZYDAVE_0 = "[CRAZY_DAVE_{0}]";

	public static string AppVersionNumber = "1.4";

	public Board mBoard;

	public TitleScreen mTitleScreen;

	public GameSelector mGameSelector;

	public SeedChooserScreen mSeedChooserScreen;

	public AwardScreen mAwardScreen;

	public CreditScreen mCreditScreen;

	public TodFoley mSoundSystem;

	public LinkedList<ButtonWidget> mControlButtonList = new LinkedList<ButtonWidget>();

	public LinkedList<Image> mCreatedImageList = new LinkedList<Image>();

	public string mReferId;

	public string mRegisterLink;

	public string mMod;

	public bool mRegisterResourcesLoaded;
#if ANDROID
	private static int updateFramesLogCount;
#endif

	public bool mTodCheatKeys;

	public GameMode mGameMode;

	public GameScenes mGameScene;

	public bool mLoadingZombiesThreadCompleted;

	public bool mFirstTimeGameSelector;

	public int mGamesPlayed;

	public int mMaxExecutions;

	public int mMaxPlays;

	public int mMaxTime;

	public bool mEasyPlantingCheat;

	public ZenGarden mZenGarden;

	public EffectSystem mEffectSystem;

	public ReanimatorCache mReanimatorCache;

	public ProfileMgr mProfileMgr;

	private PlayerInfo _playerInfo;

	public LevelStats mLastLevelStats;

	public bool mCloseRequest;

	public int mAppCounter;

	public Music mMusic;

	public Reanimation mCrazyDaveReanimID;

	public CrazyDaveState mCrazyDaveState;

	public int mCrazyDaveBlinkCounter;

	public Reanimation mCrazyDaveBlinkReanimID;

	public int mCrazyDaveMessageIndex;

	public string mCrazyDaveMessageText = string.Empty;

	public int mAppRandSeed;

	public int mSessionID;

	public int mPlayTimeActiveSession;

	public int mPlayTimeInactiveSession;

	public BoardResult mBoardResult;

	public bool mKilledYetiAndRestarted;

	public TypingCheck mKonamiCheck;

	public TypingCheck mMustacheCheck;

	public TypingCheck mMoustacheCheck;

	public TypingCheck mSuperMowerCheck;

	public TypingCheck mSuperMowerCheck2;

	public TypingCheck mFutureCheck;

	public TypingCheck mPinataCheck;

	public TypingCheck mDanceCheck;

	public TypingCheck mDaisyCheck;

	public TypingCheck mSukhbirCheck;

	public bool mMustacheMode;

	public bool mSuperMowerMode;

	public bool mFutureMode;

	public bool mPinataMode;

	public bool mDanceMode;

	public bool mDaisyMode;

	public bool mSukhbirMode;

	public TrialType mTrialType;

	public bool mDebugTrialLocked;

	public bool mMuteSoundsForCutscene;

	private string mLastBackgroundResGroupLoaded;

	private string mLastZombieNoteResGroupLoaded;

	private string mLastStoreResGroupLoaded;

	private string mLastPaperGroupLoaded;

	private string mLastZenGardenResourceLoaded;

	private string mLastUpsellResGroupLoaded;

	public GameMode mRestoreGameMode;

	public RestoreLocation mRestoreLocation;

	public bool checkGiveAchievements;

	public AchievementId achievementToCheck = AchievementId.MAX_ACHIEVEMENTS;

	private Dictionary<int, string> cachedStageStrings = new Dictionary<int, string>();

	public ChallengeScreen mChallengeScreen;

	public LeaderboardScreen mLeaderboardScreen;

	private static Dictionary<string, string> noClickStringCache = new Dictionary<string, string>(10);

	public LawnMessageBoxListener mLawnMessageBoxListener;

	public Widget mOldFocus;

	private static Dictionary<int, string> moneyStrings = new Dictionary<int, string>();

	private static Dictionary<int, string> cachedIntToString = new Dictionary<int, string>();

	private bool leaderboardLoaded;

	private bool gamePlayLoaded;

	private bool cachedLoaded;

	private bool zenGardenLoaded;

	private bool mainMenuLoaded;

	private bool pileLoaded;

	public PlayerInfo mPlayerInfo
	{
		get
		{
			return _playerInfo;
		}
		set
		{
			_playerInfo = value;
			if (mPlayerInfo != null)
			{
				SetMusicVolume(mPlayerInfo.mMusicVolume);
				SetSfxVolume(mPlayerInfo.mSoundVolume);
			}
		}
	}

	public static void CenterDialog(Dialog theDialog, int theWidth, int theHeight)
	{
		theDialog.Resize((Constants.BOARD_WIDTH - theWidth) / 2, (Constants.BOARD_HEIGHT - theHeight) / 2, theWidth, theHeight);
	}

	public LawnApp(Main m)
		: base(m)
	{
		mBoard = null;
		mGameSelector = null;
		mSeedChooserScreen = null;
		mAwardScreen = null;
		mCreditScreen = null;
		mTitleScreen = null;
		mSoundSystem = null;
		mKonamiCheck = null;
		mMustacheCheck = null;
		mMoustacheCheck = null;
		mSuperMowerCheck = null;
		mSuperMowerCheck2 = null;
		mFutureCheck = null;
		mPinataCheck = null;
		mDanceCheck = null;
		mDaisyCheck = null;
		mSukhbirCheck = null;
		mMustacheMode = false;
		mSuperMowerMode = false;
		mFutureMode = false;
		mPinataMode = false;
		mDanceMode = false;
		mDaisyMode = false;
		mSukhbirMode = false;
		mGameScene = GameScenes.SCENE_LOADING;
		mZenGarden = null;
		mEffectSystem = null;
		mReanimatorCache = null;
		mCloseRequest = false;
		mWidth = Constants.BOARD_WIDTH;
		mHeight = Constants.BOARD_HEIGHT;
		mAppCounter = 0;
		mAppRandSeed = DateTime.UtcNow.Millisecond;
		mTrialType = TrialType.TRIAL_NONE;
		mDebugTrialLocked = false;
		mMuteSoundsForCutscene = false;
		base.mMusicVolume = 0.85;
		mSfxVolume = 0.85;
		mAutoStartLoadingThread = false;
		mProdName = "PlantsVsZombies";
		string text = "Plants vs. Zombies";
		mTitle = text;
		mPlayerInfo = null;
		mLastLevelStats = new LevelStats();
		mFirstTimeGameSelector = true;
		mGameMode = GameMode.GAMEMODE_ADVENTURE;
		mEasyPlantingCheat = false;
		mLoadingZombiesThreadCompleted = true;
		mGamesPlayed = 0;
		mMaxExecutions = 0;
		mMaxPlays = 0;
		mMaxTime = 0;
		mCompletedLoadingThreadTasks = 0;
		mProfileMgr = new ProfileMgr();
		mRegisterResourcesLoaded = false;
		mTodCheatKeys = false;
		mCrazyDaveReanimID = null;
		mCrazyDaveState = CrazyDaveState.CRAZY_DAVE_OFF;
		mCrazyDaveBlinkCounter = 0;
		mCrazyDaveBlinkReanimID = null;
		mCrazyDaveMessageIndex = -1;
		mLawnMessageBoxListener = null;
		ReportAchievement.AchievementsChanged += ReportAchievement_AchievementsChanged;
	}

	private void ReportAchievement_AchievementsChanged()
	{
		if (mPlayerInfo != null)
		{
			mPlayerInfo.UpdateAchievementInfo();
		}
	}

	public override void Dispose()
	{
		if (mBoard != null)
		{
			WriteCurrentUserConfig();
		}
		if (mBoard != null)
		{
			mBoardResult = BoardResult.BOARDRESULT_QUIT_APP;
			mBoard.TryToSaveGame();
			mWidgetManager.RemoveWidget(mBoard);
			mBoard.Dispose();
			mBoard = null;
		}
		if (mTitleScreen != null)
		{
			mWidgetManager.RemoveWidget(mTitleScreen);
			mTitleScreen.Dispose();
		}
		if (mGameSelector != null)
		{
			mWidgetManager.RemoveWidget(mGameSelector);
			mGameSelector.Dispose();
		}
		if (mSeedChooserScreen != null)
		{
			mWidgetManager.RemoveWidget(mSeedChooserScreen);
			mSeedChooserScreen.Dispose();
		}
		if (mAwardScreen != null)
		{
			mWidgetManager.RemoveWidget(mAwardScreen);
			mAwardScreen.Dispose();
		}
		if (mCreditScreen != null)
		{
			mWidgetManager.RemoveWidget(mCreditScreen);
			mCreditScreen.Dispose();
		}
		mProfileMgr.Dispose();
		mResourceManager.DeleteResources("");
	}

	public bool KillNewOptionsDialog()
	{
		NewOptionsDialog newOptionsDialog = (NewOptionsDialog)GetDialog(Dialogs.DIALOG_NEWOPTIONS);
		if (newOptionsDialog == null)
		{
			return false;
		}
		KillDialog(2);
		return true;
	}

	public bool KillLeaderboardDialog()
	{
		LeaderboardDialog leaderboardDialog = (LeaderboardDialog)GetDialog(Dialogs.DIALOG_LEADERBOARD);
		if (leaderboardDialog == null)
		{
			return false;
		}
		KillDialog(59);
		mLeaderboardScreen.SetGrayed(aGray: false);
		return true;
	}

	public override void GotFocus()
	{
		if (mSoundManager != null)
		{
			mSoundManager.Enable(enable: true);
		}
		if (mMusicInterface != null)
		{
			mMusicInterface.Enable(mMusicEnabled);
		}
		if (mCreditScreen != null)
		{
			mCreditScreen.AppGotFocus();
		}
		if (checkGiveAchievements && !SexyAppBase.IsInTrialMode)
		{
			checkGiveAchievements = false;
			ReportAchievement.GiveAchievement(achievementToCheck);
		}
		base.GotFocus();
	}

	public override void LostFocus()
	{
		base.LostFocus();
		if (mSoundManager != null)
		{
			mSoundManager.StopAllSounds();
		}
		if (mBoard != null && mBoard.mBoardFadeOutCounter > 0)
		{
			mBoard.mBoardFadeOutCounter = 3;
		}
		if (!mTodCheatKeys && CanPauseNow())
		{
			if (mBoard != null)
			{
				mBoard.RefreshSeedPacketFromCursor();
			}
			DoPauseDialog();
		}
	}

	public override void AppEnteredBackground()
	{
		WriteRestoreInfo();
		if (mBoard != null)
		{
			mBoard.TryToSaveGame();
		}
		WriteCurrentUserConfig();
	}

	public override void InitHook()
	{
	}

	public override void WriteToRegistry()
	{
		if (mPlayerInfo != null)
		{
			RegistryWriteString("CurUser", mPlayerInfo.mName);
			mPlayerInfo.SaveDetails();
		}
		base.WriteToRegistry();
	}

	public override void LoadingThreadProc()
	{
		SexyAppBase.LoadLog("step GameConstants.Init");
		GameConstants.Init();
		SexyAppBase.LoadLog("step TodLoadResources LoaderBar");
		if (!TodCommon.TodLoadResources("LoaderBar") || !TodCommon.TodLoadResources("LoaderBarFont"))
		{
			SexyAppBase.LoadLog("FAIL LoaderBar group (ShowResourceError?)");
			return;
		}
		SexyAppBase.LoadLog("step ExtractLoaderBar");
		Resources.ExtractLoaderBarFontResources(mResourceManager);
		Resources.ExtractLoaderBarResources(mResourceManager);
		AtlasResources.mAtlasResources.UnpackLoadingAtlasImages();
		Resources.LinkUpResArray();
		ReanimationParams[] theReanimationParamArray = new ReanimationParams[2]
		{
			new ReanimationParams(ReanimationType.REANIM_LOADBAR_SPROUT, "reanim/loadbar_sprout", 1),
			new ReanimationParams(ReanimationType.REANIM_LOADBAR_ZOMBIEHEAD, "reanim/loadbar_zombiehead", 1)
		};
		SexyAppBase.LoadLog("step ReanimatorLoadDefinitions loadbar");
		ReanimatorXnaHelpers.ReanimatorLoadDefinitions(ref theReanimationParamArray, theReanimationParamArray.Length);
		SexyAppBase.LoadLog("step TodStringListLoad");
		TodStringFile.TodStringListLoad("LawnStrings_" + Constants.LanguageSubDir + ".txt");
		mTitleScreen.mLoaderScreenIsLoaded = true;
		SexyAppBase.LoadLog("step loaderScreenIsLoaded=true, delay loads");
		mNumLoadingThreadTasks += mResourceManager.GetNumResources("LoadingFonts") * 54;
		mNumLoadingThreadTasks += mResourceManager.GetNumResources("LoadingImages") * 9;
		mNumLoadingThreadTasks += mResourceManager.GetNumResources("LoadingSounds") * 54;
		mNumLoadingThreadTasks += 612;
		mNumLoadingThreadTasks += 8092;
		mNumLoadingThreadTasks += 360500;
		mNumLoadingThreadTasks += GetNumPreloadingTasks();
		mNumLoadingThreadTasks += mMusic.GetNumLoadingTasks();
		if (!Main.LOW_MEMORY_DEVICE)
		{
			DelayLoadGamePlayResources(doLoad: true);
			SexyAppBase.LoadLog("step delay gameplay ok");
			DelayLoadLeaderboardResource(doLoad: true);
			SexyAppBase.LoadLog("step delay leaderboard ok");
			DelayLoadCachedResources(doLoad: true);
			SexyAppBase.LoadLog("step delay cached ok");
			DelayLoadZenGardenResources(doLoad: true);
			SexyAppBase.LoadLog("step delay zengarden ok");
		}
		DelayLoadMainMenuResource(doLoad: true);
		SexyAppBase.LoadLog("step delay mainmenu ok, LoadAllResources");
		mResourceManager.LoadAllResources();
		SexyAppBase.LoadLog("step LoadAllResources ok, ExtractResources");
		Resources.ExtractResources(mResourceManager, AtlasResources.mAtlasResources);
		AtlasResources.mAtlasResources.ExtractResources();
		SexyAppBase.LoadLog("step ExtractResources ok, lawn reanims");
		ReanimatorXnaHelpers.ReanimatorLoadDefinitions(ref GameConstants.gLawnReanimationArray, 119);
		TodStringFile.TodStringListSetColors(GameConstants.gLawnStringFormats, GameConstants.gLawnStringFormatCount);
		SexyAppBase.LoadLog("step reanims ok");
		if (!mLoadingFailed && !mShutdown && !mCloseRequest)
		{
			SexyAppBase.LoadLog("step MusicInit");
			mMusic.MusicInit();
			SexyAppBase.LoadLog("step MusicInit ok");
			mZenGarden = new ZenGarden();
			mReanimatorCache = new ReanimatorCache();
			mReanimatorCache.ReanimatorCacheInitialize();
			SexyAppBase.LoadLog("step reanimcache ok");
			TodFoley.TodFoleyInitialize(null, 103);
			SexyAppBase.LoadLog("step foley ok");
			GlobalMembersTrail.TrailLoadDefinitions(GameConstants.gLawnTrailArray, 1);
			SexyAppBase.LoadLog("step trails ok");
			TodParticleGlobal.TodParticleLoadDefinitions(ref GameConstants.gLawnParticleArray, 102);
			SexyAppBase.LoadLog("step particles ok");
			PreloadForUser();
			SexyAppBase.LoadLog("step preload ok DONE");
			EnsureOfflineAchievements();
			SexyAppBase.LoadLog("step offline achievements ok count=" + Sexy.Achievements.GetNumberOfAchievements());
			if (!mLoadingFailed && !mShutdown)
			{
				_ = mCloseRequest;
			}
		}
	}

	public virtual void LoadingCompleted()
	{
		mWidgetManager.RemoveWidget(mTitleScreen);
		SafeDeleteWidget(mTitleScreen);
		mTitleScreen = null;
		mResourceManager.DeleteImage("IMAGE_TITLESCREEN");
		if (mRestoreLocation != RestoreLocation.RESTORE_BOARD || !RestoreGame())
		{
			ShowGameSelector();
			if (Main.LOW_MEMORY_DEVICE)
			{
				mResourceManager.UnloadInitResources();
			}
		}
	}

	public static void PreallocateMemory()
	{
		GC.Collect();
		ReanimatorTransform.PreallocateMemory();
		Reanimation.PreallocateMemory();
		ReanimatorTrackInstance.PreallocateMemory();
		Attachment.PreallocateMemory();
		Plant.PreallocateMemory();
		Zombie.PreallocateMemory();
		Projectile.PreallocateMemory();
		XNASoundInstance.PreallocateMemory();
		TodParticle.PreallocateMemory();
		RenderItem.PreallocateMemory();
		TodParticleEmitter.PreallocateMemory();
		SexyAppBase.XnaGame.CompensateForSlowUpdate();
	}

	public override void LoadingThreadCompleted()
	{
		PropertiesParser propertiesParser = new PropertiesParser(this);
		if (propertiesParser.ParsePropertiesFile("properties/content.xml"))
		{
			string text = GetString("ContentUpdateSitePrefix");
			if (!string.IsNullOrEmpty(text))
			{
				Debug.OutputDebug(Common.StrFormat_("Content Update: URL={0}\n", text));
			}
			else
			{
				Debug.OutputDebug("Content Update: Failed to find property 'ContentUpdateSitePrefix'.\n");
			}
		}
		else
		{
			Debug.OutputDebug(Common.StrFormat_("Content Update: Failed to parse properties file: {0}\n", propertiesParser.GetErrorText()));
		}
		GC.Collect();
		SexyAppBase.XnaGame.CompensateForSlowUpdate();
	}

	public virtual bool DebugKeyDown(int theKey)
	{
		return false;
	}

	public virtual void HandleCmdLineParam(string theParamName, string theParamValue)
	{
	}

	public override void PlaySample(int theSoundNum)
	{
		if (!mMuteSoundsForCutscene)
		{
			base.PlaySample(theSoundNum);
		}
	}

	public void ConfirmQuit()
	{
		string theDialogLines = TodStringFile.TodStringTranslate("[QUIT_MESSAGE]");
		string theDialogHeader = TodStringFile.TodStringTranslate("[QUIT_HEADER]");
		LawnDialog lawnDialog = DoDialog(13, isModal: true, theDialogHeader, theDialogLines, "", 2);
		lawnDialog.mLawnYesButton.mLabel = TodStringFile.TodStringTranslate("[QUIT_BUTTON]");
		CenterDialog(lawnDialog, lawnDialog.mWidth, lawnDialog.mHeight);
	}

	public void ConfirmCheckForUpdates()
	{
	}

	public void CheckForUpdates()
	{
	}

	public void DoUserDialog()
	{
		KillDialog(29);
		UserDialog userDialog = new UserDialog(this);
		CenterDialog(userDialog, userDialog.mWidth, userDialog.mHeight);
		AddDialog(29, userDialog);
		mWidgetManager.SetFocus(userDialog);
	}

	public void FinishUserDialog(bool isYes)
	{
		UserDialog userDialog = (UserDialog)GetDialog(Dialogs.DIALOG_USERDIALOG);
		if (userDialog == null)
		{
			return;
		}
		if (isYes)
		{
			PlayerInfo profile = mProfileMgr.GetProfile(userDialog.GetSelName());
			if (profile != null)
			{
				mPlayerInfo = profile;
				mWidgetManager.MarkAllDirty();
				if (mGameSelector != null)
				{
					mGameSelector.SyncProfile(theShowLoading: true);
				}
			}
		}
		KillDialog(29);
	}

	private string GetSuggestedPlayerName()
	{
		string text = Environment.UserName;
		int backslash = text.LastIndexOf('\\');
		if (backslash >= 0)
		{
			text = text.Substring(backslash + 1);
		}
		return text.Trim();
	}

	public void DoCreateUserDialog(bool isOnlyUser)
	{
		KillDialog(30);
		NewUserDialog newUserDialog = new NewUserDialog(this, allowCancel: !isOnlyUser);
		newUserDialog.Move(mWidth / 2 - newUserDialog.mWidth / 2, (int)Constants.InvertAndScale(20f));
		newUserDialog.SetName(GetSuggestedPlayerName());
		AddDialog(30, newUserDialog);
	}

	public void DoCheatDialog()
	{
		KillDialog(35);
		CheatDialog cheatDialog = new CheatDialog(this);
		CenterDialog(cheatDialog, cheatDialog.mWidth, cheatDialog.mHeight);
		AddDialog(35, cheatDialog);
	}

	public void FinishCheatDialog(bool isYes)
	{
		CheatDialog cheatDialog = (CheatDialog)GetDialog(Dialogs.DIALOG_CHEAT);
		if (cheatDialog != null && (!isYes || cheatDialog.ApplyCheat()))
		{
			KillDialog(35);
			if (isYes)
			{
				mMusic.StopAllMusic();
				mBoardResult = BoardResult.BOARDRESULT_CHEAT;
				PreNewGame(mGameMode, theLookForSavedGame: false);
			}
		}
	}

	public void FinishCreateUserDialog(bool isYes)
	{
		NewUserDialog newUserDialog = (NewUserDialog)GetDialog(30);
		string text = (newUserDialog != null) ? newUserDialog.GetName() : string.Empty;
		text = text.Trim();
		string theDialogLines = "[ENTER_NEW_USER]";
		if (isYes && text.empty() && mPlayerInfo != null)
		{
			KillDialog(30);
			return;
		}
		if (mPlayerInfo == null && (!isYes || text.empty()))
		{
			DoDialog(33, isModal: true, "[ENTER_YOUR_NAME]", theDialogLines, "[DIALOG_BUTTON_OK]", 3);
			return;
		}
		if (!isYes)
		{
			KillDialog(30);
			return;
		}
		if (mProfileMgr.GetProfile(text) != null)
		{
			DoDialog(33, isModal: true, "[NAME_CONFLICT]", "[ENTER_UNIQUE_PLAYER_NAME]", "[DIALOG_BUTTON_OK]", 3);
			return;
		}
		PlayerInfo playerInfo = mProfileMgr.AddProfile(text);
		mProfileMgr.Save();
		mPlayerInfo = playerInfo;
		KillDialog(29);
		KillDialog(30);
		mWidgetManager.MarkAllDirty();
		if (mGameSelector != null)
		{
			mGameSelector.SyncProfile(theShowLoading: true);
		}
	}

	public void DoConfirmDeleteUserDialog(string theName)
	{
		KillDialog(31);
		DoDialog(31, isModal: true, "[ARE_YOU_SURE]", Common.StrFormat_(TodStringFile.TodStringTranslate("[DELETE_USER_WARNING]"), theName), "", 1);
	}

	public void FinishConfirmDeleteUserDialog(bool isYes)
	{
		KillDialog(31);
		UserDialog userDialog = (UserDialog)GetDialog(Dialogs.DIALOG_USERDIALOG);
		if (userDialog == null)
		{
			return;
		}
		mWidgetManager.SetFocus(userDialog);
		if (!isYes)
		{
			return;
		}
		string text = ((mPlayerInfo != null) ? mPlayerInfo.mName : "");
		string selName = userDialog.GetSelName();
		if (selName == text)
		{
			mPlayerInfo = null;
		}
		mProfileMgr.DeleteProfile(selName);
		userDialog.FinishDeleteUser();
		if (mPlayerInfo == null)
		{
			mPlayerInfo = mProfileMgr.GetProfile(userDialog.GetSelName());
			if (mPlayerInfo == null)
			{
				mPlayerInfo = mProfileMgr.GetAnyProfile();
			}
		}
		mProfileMgr.Save();
		if (mPlayerInfo == null)
		{
			DoCreateUserDialog(isOnlyUser: true);
		}
		mWidgetManager.MarkAllDirty();
		if (mGameSelector != null)
		{
			mGameSelector.SyncProfile(theShowLoading: true);
		}
	}

	public void DoRenameUserDialog(string theName)
	{
		KillDialog(32);
		NewUserDialog newUserDialog = new NewUserDialog(this, isRename: true, allowCancel: true);
		newUserDialog.Move(mWidth / 2 - newUserDialog.mWidth / 2, (int)Constants.InvertAndScale(20f));
		newUserDialog.SetName(theName);
		AddDialog(32, newUserDialog);
	}

	public void FinishRenameUserDialog(bool isYes)
	{
		UserDialog userDialog = (UserDialog)GetDialog(29);
		if (!isYes)
		{
			KillDialog(32);
			mWidgetManager.SetFocus(userDialog);
			return;
		}
		NewUserDialog newUserDialog = (NewUserDialog)GetDialog(32);
		if (userDialog == null || newUserDialog == null)
		{
			return;
		}
		string selName = userDialog.GetSelName();
		string name = newUserDialog.GetName();
		if (string.IsNullOrEmpty(name))
		{
			KillDialog(32);
			mWidgetManager.SetFocus(userDialog);
			return;
		}
		bool flag = mProfileMgr.GetProfile(selName) == mPlayerInfo;
		if (!mProfileMgr.RenameProfile(selName, name))
		{
			DoDialog(34, isModal: true, "[NAME_CONFLICT]", "[ENTER_UNIQUE_PLAYER_NAME]", "[DIALOG_BUTTON_OK]", 3);
			return;
		}
		mProfileMgr.Save();
		if (flag)
		{
			mPlayerInfo = mProfileMgr.GetProfile(name);
		}
		userDialog.FinishRenameUser(name);
		mWidgetManager.MarkAllDirty();
		KillDialog(32);
		mWidgetManager.SetFocus(userDialog);
	}

	public void FinishNameError(int theId)
	{
		KillDialog(theId);
		NewUserDialog newUserDialog = (NewUserDialog)GetDialog(Dialogs.DIALOG_CREATEUSER);
		if (newUserDialog != null)
		{
			mWidgetManager.SetFocus(newUserDialog.mNameEditWidget);
		}
	}

	public void FinishRestartConfirmDialog()
	{
		mKilledYetiAndRestarted = mBoard.mKilledYeti;
		KillDialog(37);
		KillDialog(39);
		KillBoard();
		PreNewGame(mGameMode, theLookForSavedGame: false);
	}

	public void FinishInGameRestartConfirmDialog(bool isYes)
	{
		KillDialog(23);
		if (isYes)
		{
			mMusic.StopAllMusic();
			mSoundSystem.CancelPausedFoley();
			KillNewOptionsDialog();
			mBoardResult = BoardResult.BOARDRESULT_RESTART;
			if (mBoard != null)
			{
				mKilledYetiAndRestarted = mBoard.mKilledYeti;
			}
			PreNewGame(mGameMode, theLookForSavedGame: false);
		}
	}

	public void FinishAboutDialog(bool isYes)
	{
		KillDialog(52);
	}

	public void FinishRestartWarningDialog(bool isYes)
	{
		KillDialog(53);
	}

	public void FinishPacketSlotPurchaseDialog(bool isYes)
	{
		KillDialog(51);
		if (isYes)
		{
			int itemCost = StoreScreen.GetItemCost(StoreItem.STORE_ITEM_PACKET_UPGRADE);
			mPlayerInfo.AddCoins(-itemCost);
			mPlayerInfo.mPurchases[21]++;
			WriteCurrentUserConfig();
			mBoard.mSeedBank.UpdateHeight();
			if (mCrazyDaveMessageIndex == 1503)
			{
				CrazyDaveTalkIndex(1510);
			}
			else if (mCrazyDaveMessageIndex == 1553)
			{
				CrazyDaveTalkIndex(1560);
			}
		}
		else
		{
			mPlayerInfo.mDidntPurchasePacketUpgrade++;
			if (mCrazyDaveMessageIndex == 1503)
			{
				CrazyDaveTalkIndex(1520);
			}
			else if (mCrazyDaveMessageIndex == 1553)
			{
				CrazyDaveTalkIndex(1570);
			}
		}
	}

	public void FinishTimesUpDialog()
	{
		KillDialog(42);
	}

	public void FinishPlantSale(bool isYes)
	{
		mZenGarden.DoPlantSale(isYes);
		KillDialog(48);
	}

	public void FinishLawnDialogMessageBox(bool isYes)
	{
		if (mLawnMessageBoxListener != null)
		{
			mLawnMessageBoxListener.LawnMessageBoxDone(isYes ? 1000 : 1001);
			mLawnMessageBoxListener = null;
			mWidgetManager.SetFocus(mOldFocus);
			mOldFocus = null;
		}
	}

	public void KillBoard()
	{
		FinishModelessDialogs();
		KillSeedChooserScreen();
		if (mBoard != null)
		{
			mBoard.DisposeBoard();
			mWidgetManager.RemoveWidget(mBoard);
			SafeDeleteWidget(mBoard);
			mBoard = null;
		}
	}

	public void MakeNewBoard()
	{
		KillBoard();
		mBoard = new Board(this);
		mBoard.Resize(Constants.Board_Offset_AspectRatio_Correction, 0, mWidth, mHeight);
		mWidgetManager.AddWidget(mBoard);
		mWidgetManager.BringToBack(mBoard);
		mWidgetManager.SetFocus(mBoard);
		GC.Collect();
		SexyAppBase.XnaGame.CompensateForSlowUpdate();
	}

	public void StartPlaying()
	{
		KillSeedChooserScreen();
		mBoard.StartLevel();
		mGameScene = GameScenes.SCENE_PLAYING;
	}

	public bool TryLoadGame()
	{
		string savedGameName = LawnCommon.GetSavedGameName(mGameMode, (int)mPlayerInfo.mId);
		mMusic.StopAllMusic();
		if (FileExists(savedGameName))
		{
			MakeNewBoard();
			if (mBoard.LoadGame(savedGameName))
			{
				mFirstTimeGameSelector = false;
				DoContinueDialog();
				return true;
			}
			KillBoard();
		}
		return false;
	}

	internal override void NewGame()
	{
		mFirstTimeGameSelector = false;
		MakeNewBoard();
		mBoard.InitLevel();
		mBoardResult = BoardResult.BOARDRESULT_NONE;
		mGameScene = GameScenes.SCENE_LEVEL_INTRO;
		ShowSeedChooserScreen();
		mBoard.mCutScene.StartLevelIntro();
	}

	public bool RestoreGame()
	{
		string savedGameName = LawnCommon.GetSavedGameName(mRestoreGameMode, (int)mPlayerInfo.mId);
		mMusic.StopAllMusic();
		if (FileExists(savedGameName))
		{
			mGameMode = mRestoreGameMode;
			MakeNewBoard();
			if (mBoard.LoadGame(savedGameName))
			{
				if (mBoard.mNextSurvivalStageCounter != 1)
				{
					string savedGameName2 = LawnCommon.GetSavedGameName(mRestoreGameMode, (int)mPlayerInfo.mId);
					EraseFile(savedGameName2);
				}
				mBoard.Pause(thePause: false);
				DoPauseDialog();
				return true;
			}
			KillBoard();
		}
		return false;
	}

	public void RestartLoopingSounds()
	{
		if (mGameMode == GameMode.GAMEMODE_CHALLENGE_RAINING_SEEDS || IsStormyNightLevel())
		{
			PlayFoley(FoleyType.FOLEY_RAIN);
		}
		int count = mBoard.mZombies.Count;
		for (int i = 0; i < count; i++)
		{
			Zombie zombie = mBoard.mZombies[i];
			if (!zombie.mDead && zombie.mPlayingSong)
			{
				zombie.mPlayingSong = false;
				zombie.StartZombieSound();
			}
		}
	}

	public void PreNewGame(GameMode theGameMode, bool theLookForSavedGame)
	{
		PreNewGame(theGameMode, theLookForSavedGame, checkForTutorialCompletion: true);
	}

	public void PreNewGame(GameMode theGameMode, bool theLookForSavedGame, bool checkForTutorialCompletion)
	{
		if (NeedRegister())
		{
			ShowGameSelector();
			return;
		}
		DelayLoadMainMenuResource(doLoad: false);
		if (theGameMode == GameMode.GAMEMODE_CHALLENGE_ZEN_GARDEN)
		{
			DelayLoadZenGardenResources(doLoad: true);
		}
		else
		{
			DelayLoadZenGardenResources(doLoad: false);
		}
		GC.Collect();
		GC.WaitForPendingFinalizers();
		DelayLoadGamePlayResources(doLoad: true);
		if (SexyAppBase.IsInTrialMode && mPlayerInfo.mLevel >= 7 && theGameMode != GameMode.GAMEMODE_CHALLENGE_ZEN_GARDEN)
		{
			if (mPlayerInfo.mNeedsTrialLevelReset)
			{
				mPlayerInfo.SetLevel(1);
				mPlayerInfo.mNeedsTrialLevelReset = false;
			}
			else
			{
				theGameMode = GameMode.GAMEMODE_UPSELL;
			}
		}
		if (SexyAppBase.IsInTrialMode && checkForTutorialCompletion && theGameMode == GameMode.GAMEMODE_ADVENTURE && mPlayerInfo.mLevel <= 3 && mPlayerInfo.mHasFinishedTutorial && mPlayerInfo.mFinishedAdventure == 0)
		{
			LawnDialog theDialog = DoDialog(58, isModal: true, string.Empty, "[SKIP_TUTORIAL_MESSAGE]", string.Empty, 1);
			CenterDialog(theDialog, (int)Constants.InvertAndScale(400f), (int)Constants.InvertAndScale(200f));
			return;
		}
		mGameMode = theGameMode;
		if (!theLookForSavedGame || !TryLoadGame())
		{
			string savedGameName = LawnCommon.GetSavedGameName(mGameMode, (int)mPlayerInfo.mId);
			EraseFile(savedGameName);
			NewGame();
			if (mGameMode == GameMode.GAMEMODE_CHALLENGE_ZEN_GARDEN && !mPlayerInfo.mZenGardenTutorialComplete)
			{
				mZenGarden.SetupForZenTutorial();
			}
		}
	}

	public void ShowGameSelectorWithOptions()
	{
		ShowGameSelector();
		DoNewOptions(theFromGameSelector: true);
	}

	public void ShowGameSelector()
	{
		KillBoard();
		UpdateRegisterInfo();
		DelayLoadGamePlayResources(doLoad: false);
		DelayLoadMainMenuResource(doLoad: true);
		if (mGameSelector != null)
		{
			mWidgetManager.RemoveWidget(mGameSelector);
			SafeDeleteWidget(mGameSelector);
		}
		mGameScene = GameScenes.SCENE_MENU;
		mGameSelector = new GameSelector(this);
		mGameSelector.Resize(0, 0, Constants.GameSelector_Width, Constants.GameSelector_Height);
		mWidgetManager.AddWidget(mGameSelector);
		mWidgetManager.BringToBack(mGameSelector);
		mWidgetManager.SetFocus(mGameSelector);
		if (NeedRegister())
		{
			DoNeedRegisterDialog();
		}
	}

	public void KillGameSelector()
	{
		if (mGameSelector != null)
		{
			mWidgetManager.RemoveWidget(mGameSelector);
			SafeDeleteWidget(mGameSelector);
			mGameSelector = null;
		}
	}

	public void ShowGameSelectorQuickPlay(bool theDoFadeIn, GameSelectorButtons theButton)
	{
		ShowGameSelector();
		mGameSelector.mNeedToPlayRollIn = false;
		mGameSelector.MoveToQuickplay(theDoFadeIn, theButton);
	}

	public void ShowGameSelectorQuickPlay(bool theDoFadeIn)
	{
		ShowGameSelectorQuickPlay(theDoFadeIn, GameSelectorButtons.GameSelector_MiniGames);
	}

	protected override void ShowUpdateMessage()
	{
		SexyAppBase.UseLiveServers = false;
		if (!Guide.IsVisible)
		{
			Guide.BeginShowMessageBox(TodStringFile.TodStringTranslate("[UPDATE]"), TodStringFile.TodStringTranslate("[UPDATE_REQUIRED]"), new string[2]
			{
				TodStringFile.TodStringTranslate("[BUTTON_YES]"),
				TodStringFile.TodStringTranslate("[BUTTON_NO]")
			}, 0, MessageBoxIcon.None, GameUpdateMessageClosed, null);
			wantToShowUpdateMessage = false;
		}
	}

	protected override bool ShowRunWhenLockedMessage()
	{
		if (Guide.IsVisible)
		{
			return false;
		}
		if (!TodStringFile.StringsLoaded)
		{
			return false;
		}
		Guide.BeginShowMessageBox(TodStringFile.TodStringTranslate("[ALLOW_RUN_WHEN_LOCKED_HEADING]"), TodStringFile.TodStringTranslate("[ALLOW_RUN_WHEN_LOCKED]"), new string[2]
		{
			TodStringFile.TodStringTranslate("[BUTTON_YES]"),
			TodStringFile.TodStringTranslate("[BUTTON_NO]")
		}, 0, MessageBoxIcon.None, RunWhenLockedMessageClosed, null);
		return true;
	}

	private void RunWhenLockedMessageClosed(IAsyncResult result)
	{
		int? num = Guide.EndShowMessageBox(result);
		int? num2 = num;
		Main.RunWhenLocked = num2.GetValueOrDefault() == 0 && num2.HasValue;
		if (mPlayerInfo != null)
		{
			mPlayerInfo.mRunWhileLocked = Main.RunWhenLocked;
		}
	}

	private void GameUpdateMessageClosed(IAsyncResult result)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		int? num = Guide.EndShowMessageBox(result);
		int? num2 = num;
		if (num2.GetValueOrDefault() == 0 && num2.HasValue)
		{
			if (Main.IsInTrialMode)
			{
				Guide.ShowMarketplace(PlayerIndex.One);
				return;
			}
			MarketplaceDetailTask val = new MarketplaceDetailTask();
			val.ContentType = (MarketplaceContentType)1;
			val.Show();
		}
		else
		{
			SexyAppBase.UseLiveServers = false;
		}
	}

	public void ShowAwardScreen(AwardType theAwardType, bool theShowAchievements)
	{
		mGameScene = GameScenes.SCENE_AWARD;
		mAwardScreen = new AwardScreen(this, theAwardType, theShowAchievements);
		mAwardScreen.Resize(0, 0, mWidth, mHeight);
		mWidgetManager.AddWidget(mAwardScreen);
		mWidgetManager.BringToBack(mAwardScreen);
		mWidgetManager.SetFocus(mAwardScreen);
	}

	public void KillAwardScreen()
	{
		if (mAwardScreen != null)
		{
			mWidgetManager.RemoveWidget(mAwardScreen);
			SafeDeleteWidget(mAwardScreen);
			mAwardScreen = null;
		}
	}

	public void ShowSeedChooserScreen()
	{
		Debug.ASSERT(mSeedChooserScreen == null);
		mSeedChooserScreen = new SeedChooserScreen();
		mSeedChooserScreen.Resize(0, 0, mWidth, mHeight);
		mWidgetManager.AddWidget(mSeedChooserScreen);
		mWidgetManager.BringToBack(mSeedChooserScreen);
	}

	public void KillSeedChooserScreen()
	{
		if (mSeedChooserScreen != null)
		{
			mWidgetManager.RemoveWidget(mSeedChooserScreen);
			SafeDeleteWidget(mSeedChooserScreen);
			mSeedChooserScreen = null;
		}
	}

	public void DoBackToMain()
	{
		DoBackToMain(stopMusic: true);
	}

	public void DoBackToMain(bool stopMusic)
	{
		if (stopMusic)
		{
			mMusic.StopAllMusic();
		}
		mSoundSystem.CancelPausedFoley();
		mSoundManager.StopAllSounds();
		WriteCurrentUserConfig();
		KillNewOptionsDialog();
		KillBoard();
		ShowGameSelector();
		mZenGarden.UnloadBackdrop();
	}

	public void DoConfirmBackToMain()
	{
		LawnDialog lawnDialog = DoDialog(22, isModal: true, "[LEAVE_GAME_HEADER]", "[LEAVE_GAME]", "", 1);
		lawnDialog.mLawnYesButton.mLabel = TodStringFile.TodStringTranslate("[LEAVE_BUTTON]");
		lawnDialog.mLawnNoButton.mLabel = TodStringFile.TodStringTranslate("[DIALOG_BUTTON_CANCEL]");
		lawnDialog.CalcSize(0, 0);
	}

	public void DoNewOptions(bool theFromGameSelector)
	{
		FinishModelessDialogs();
		NewOptionsDialog newOptionsDialog = new NewOptionsDialog(this, theFromGameSelector);
		int theWidth = (int)Constants.InvertAndScale(420f);
		int preferredHeight = newOptionsDialog.GetPreferredHeight(theWidth);
		CenterDialog(newOptionsDialog, theWidth, preferredHeight);
		AddDialog(2, newOptionsDialog);
		mWidgetManager.SetFocus(newOptionsDialog);
	}

	public void ShowLeaderboardDialog(LeaderBoardType aType)
	{
		FinishModelessDialogs();
		LeaderboardDialog leaderboardDialog = new LeaderboardDialog(this, aType);
		int theWidth = (int)Constants.InvertAndScale(420f);
		int preferredHeight = leaderboardDialog.GetPreferredHeight(theWidth);
		CenterDialog(leaderboardDialog, theWidth, preferredHeight);
		AddDialog(59, leaderboardDialog);
		mLeaderboardScreen.SetGrayed(aGray: true);
		mWidgetManager.SetFocus(leaderboardDialog);
	}

	public void DoRegister()
	{
	}

	public void DoRegisterError()
	{
		DoDialog(9, isModal: true, "[INVALID_CODE]", "[INVALID_CODE_MESSAGE]", "[DIALOG_BUTTON_OK]", 3);
	}

	public bool CanDoRegisterDialog()
	{
		return true;
	}

	public bool WriteCurrentUserConfig()
	{
		if (mPlayerInfo != null)
		{
			mPlayerInfo.SaveDetails();
		}
		return true;
	}

	public void WriteRestoreInfo()
	{
		RestoreLocation theValue = RestoreLocation.RESTORE_TITLESCREEN;
		if (mGameSelector != null)
		{
			theValue = RestoreLocation.RESTORE_MAINMENU;
		}
		else if (mBoard != null)
		{
			theValue = RestoreLocation.RESTORE_BOARD;
			RegistryWriteInteger("RestoreGameMode", (int)mGameMode);
		}
		RegistryWriteInteger("RestoreLocation", (int)theValue);
	}

	public void ReadRestoreInfo()
	{
		mRestoreLocation = RestoreLocation.RESTORE_MAINMENU;
		mRestoreLocation = (RestoreLocation)RegistryReadInteger("RestoreLocation");
		if (mRestoreLocation == RestoreLocation.RESTORE_BOARD)
		{
			mRestoreGameMode = (GameMode)RegistryReadInteger("RestoreGameMode");
		}
	}

	public void DoLockedAchievementDialog(AchievementId theId)
	{
		string fmt = TodStringFile.TodStringTranslate("[UNLOCK_TO_EARN]");
		string theDialogLines = Common.StrFormat_(fmt, Achievements.GetAchievementItem(theId).Name);
		LawnDialog lawnDialog = DoDialog(55, isModal: true, "[ACHIEVEMENT_UNLOCKED]", theDialogLines, "", 2);
		lawnDialog.mLawnYesButton.mLabel = TodStringFile.TodStringTranslate("[CONTINUE_BUTTON]");
		lawnDialog.mLawnNoButton.mLabel = TodStringFile.TodStringTranslate("[GET_FULL_VERSION_BUTTON]");
		lawnDialog.CalcSize(200, 10);
		CenterDialog(lawnDialog, lawnDialog.mWidth, lawnDialog.mHeight);
	}

	public void DoNeedRegisterDialog()
	{
		LawnDialog lawnDialog = DoDialog(5, isModal: true, "[REGISTER_HEADER]", "[REGISTER]", "", 2);
		lawnDialog.mLawnYesButton.mLabel = TodStringFile.TodStringTranslate("[REGISTER_BUTTON]");
		lawnDialog.mLawnNoButton.mLabel = TodStringFile.TodStringTranslate("[QUIT_BUTTON]");
	}

	public void DoContinueDialog()
	{
		ContinueDialog continueDialog = new ContinueDialog(this);
		CenterDialog(continueDialog, continueDialog.mWidth, continueDialog.mHeight);
		AddDialog(37, continueDialog);
	}

	public void DoPauseDialog()
	{
		mBoard.Pause(thePause: true);
		FinishModelessDialogs();
		LawnDialog lawnDialog = DoDialog(19, isModal: true, "[GAME_PAUSED]", "", "[RESUME_GAME]", 3);
		int num = Math.Max(Resources.FONT_DWARVENTODCRAFT15.StringWidth(TodStringFile.TodStringTranslate("[GAME_PAUSED]")), Resources.FONT_DWARVENTODCRAFT15.StringWidth(TodStringFile.TodStringTranslate("[RESUME_GAME]")));
		if ((float)num < Constants.InvertAndScale(125f))
		{
			num = (int)Constants.InvertAndScale(125f);
		}
		// La reanimacion del dialogo suma su posicion dos veces (traslacion
		// del widget + matriz overlay de la reanim). Para posicionarla de
		// forma explicita se aparca el widget en el origen y se coloca solo
		// el overlay una vez conocido el ancho real del dialogo: X centrada
		// con el texto PAUSA y el boton, Y original debajo del header.
		lawnDialog.mReanimation.AddReanimation(0f, 0f, ReanimationType.REANIM_ZOMBIE_NEWSPAPER);
		lawnDialog.mSpaceAfterHeader = (int)Constants.InvertAndScale(65f);
		lawnDialog.CalcSize((int)Constants.InvertAndScale(20f), (int)Constants.InvertAndScale(10f), num);
		CenterDialog(lawnDialog, lawnDialog.mWidth, lawnDialog.mHeight);
		lawnDialog.mReanimation.mPosX = 0f;
		lawnDialog.mReanimation.mPosY = 0f;
		// El arte del zombie carga a la derecha de su origen: micro-ajuste
		// medido para centrarlo visualmente con PAUSA y el boton.
		lawnDialog.mReanimation.mReanim.SetPosition((float)lawnDialog.mWidth / 2f - 50f, 2f * Constants.InvertAndScale(30f));
		lawnDialog.Resize(lawnDialog.mX, lawnDialog.mY, lawnDialog.mWidth, lawnDialog.mHeight);
		lawnDialog.mSpaceAfterHeader = (int)Constants.InvertAndScale(65f);
		lawnDialog.CalcSize((int)Constants.InvertAndScale(20f), (int)Constants.InvertAndScale(10f), num);
		CenterDialog(lawnDialog, lawnDialog.mWidth, lawnDialog.mHeight);
		if (mBoard.mCursorObject.mCursorType == CursorType.CURSOR_TYPE_HAMMER)
		{
			EnforceCursor();
		}
	}

	public void FinishModelessDialogs()
	{
	}

	public LawnDialog DoDialog(int theDialogId, bool isModal, string theDialogHeader, string theDialogLines, string theDialogFooter, int theButtonMode)
	{
		TodStringFile.TodStringTranslate(theDialogHeader);
		TodStringFile.TodStringTranslate(theDialogLines);
		TodStringFile.TodStringTranslate(theDialogFooter);
		LawnDialog lawnDialog = new LawnDialog(this, null, theDialogId, isModal, theDialogHeader, theDialogLines, theDialogFooter, theButtonMode);
		base.DoDialog(lawnDialog, theDialogId);
		if (mWidgetManager.mFocusWidget == null)
		{
			mWidgetManager.mFocusWidget = lawnDialog;
		}
		return lawnDialog;
	}

	public LawnDialog DoDialogDelay(int theDialogId, bool isModal, string theDialogHeader, string theDialogLines, string theDialogFooter, int theButtonMode)
	{
		LawnDialog lawnDialog = new LawnDialog(this, null, theDialogId, isModal, theDialogHeader, theDialogLines, theDialogFooter, theButtonMode);
		base.DoDialog(lawnDialog, theDialogId);
		lawnDialog.SetButtonDelay(30);
		return lawnDialog;
	}

	public override void Shutdown()
	{
		if (!mLoadingThreadCompleted)
		{
			mLoadingFailed = true;
		}
		else if (!mShutdown)
		{
			for (int i = 0; i < 60; i++)
			{
				KillDialog(i);
			}
			if (mBoard != null)
			{
				mBoardResult = BoardResult.BOARDRESULT_QUIT_APP;
				mWidgetManager.mDownButtons = 0;
				mBoard.TryToSaveGame();
				KillBoard();
				WriteCurrentUserConfig();
			}
			mProfileMgr.Save();
			ProcessSafeDeleteList();
			if (mZenGarden != null)
			{
				mZenGarden = null;
			}
			if (mEffectSystem != null)
			{
				mEffectSystem.EffectSystemDispose();
				mEffectSystem.Dispose();
				mEffectSystem = null;
			}
			if (mReanimatorCache != null)
			{
				mReanimatorCache.ReanimatorCacheDispose();
				mReanimatorCache = null;
			}
			TodParticleGlobal.TodParticleFreeDefinitions();
			ReanimatorXnaHelpers.ReanimatorFreeDefinitions();
			Coin.CoinFreeTextures();
			GlobalMembersTrail.TrailFreeDefinitions();
			UpdateRegisterInfo();
			base.Shutdown();
		}
	}

	public override void Init()
	{
		_ = mTodCheatKeys;
		mSessionID = (int)DateTime.UtcNow.Ticks;
		mPlayTimeActiveSession = 0;
		mPlayTimeInactiveSession = 0;
		mBoardResult = BoardResult.BOARDRESULT_NONE;
		mKilledYetiAndRestarted = false;
		base.Init();
		ReadRestoreInfo();
		bool parsed = mResourceManager.ParseResourcesFile("resources.xml");
#if ANDROID
		try { Android.Util.Log.Info("PVZ", "Init ParseResources=" + parsed); } catch { }
#endif
		if (!parsed)
		{
			ShowResourceError(true);
			return;
		}
		if (Constants.Language != Constants.LanguageIndex.de)
		{
			bool loaded = TodCommon.TodLoadResources("Init");
#if ANDROID
			try { Android.Util.Log.Info("PVZ", "Init TodLoadResources=" + loaded); } catch { }
#endif
			if (!loaded)
			{
				return;
			}
		}
		else if (!TodCommon.TodLoadResources("InitRegistered"))
		{
			return;
		}
		Resources.ExtractInitResources(mResourceManager);
		PerfTimer perfTimer = default(PerfTimer);
		perfTimer.Start();
		mProfileMgr.Load();
		string empty = string.Empty;
		if (mPlayerInfo == null && RegistryReadString("CurUser", empty) != null)
		{
			mPlayerInfo = mProfileMgr.GetProfile(empty);
		}
		try
		{
			if (XnaMediaPlayer.GameHasControl)
			{
				XnaMediaPlayer.Play(mContentManager.Load<Song>(GlobalStaticVars.GetResourceDir() + "music/crazydave"));
			}
		}
		catch (Exception)
		{
		}
		if (mPlayerInfo == null)
		{
			PlayerInfo anyProfile = mProfileMgr.GetAnyProfile();
			if (!XnaMediaPlayer.GameHasControl)
			{
				anyProfile.mMusicVolume = XnaMediaPlayer.Volume;
			}
			mPlayerInfo = anyProfile;
		}
		mMaxExecutions = GetInteger("MaxExecutions", 0);
		mMaxPlays = GetInteger("MaxPlays", 0);
		mMaxTime = GetInteger("MaxTime", 60);
		mTitleScreen = new TitleScreen(this);
		mTitleScreen.Resize(0, 0, mWidth, mHeight);
		mWidgetManager.AddWidget(mTitleScreen);
		mWidgetManager.SetFocus(mTitleScreen);
		perfTimer.Start();
		mMusic = new Music();
		mSoundSystem = new TodFoley();
		mEffectSystem = new EffectSystem();
		mEffectSystem.EffectSystemInitialize();
		mKonamiCheck = new TypingCheck();
		mKonamiCheck.AddKeyCode(KeyCode.KEYCODE_UP);
		mKonamiCheck.AddKeyCode(KeyCode.KEYCODE_UP);
		mKonamiCheck.AddKeyCode(KeyCode.KEYCODE_DOWN);
		mKonamiCheck.AddKeyCode(KeyCode.KEYCODE_DOWN);
		mKonamiCheck.AddKeyCode(KeyCode.KEYCODE_LEFT);
		mKonamiCheck.AddKeyCode(KeyCode.KEYCODE_RIGHT);
		mKonamiCheck.AddKeyCode(KeyCode.KEYCODE_LEFT);
		mKonamiCheck.AddKeyCode(KeyCode.KEYCODE_RIGHT);
		mKonamiCheck.AddChar('b');
		mKonamiCheck.AddChar('a');
		mMustacheCheck = new TypingCheck("mustache");
		mMoustacheCheck = new TypingCheck("moustache");
		mSuperMowerCheck = new TypingCheck("trickedout");
		mSuperMowerCheck2 = new TypingCheck("tricked out");
		mFutureCheck = new TypingCheck("future");
		mPinataCheck = new TypingCheck("pinata");
		mDanceCheck = new TypingCheck("dance");
		mDaisyCheck = new TypingCheck("daisies");
		mSukhbirCheck = new TypingCheck("sukhbir");
		perfTimer.Start();
		perfTimer.Start();
	}

	public override void Start()
	{
		if (!mLoadingFailed)
		{
			base.Start();
		}
	}

	public string RegistryReadString(string key, string value)
	{
		return null;
	}

	public virtual Dialog NewDialog(int theDialogId, bool isModal, string theDialogHeader, string theDialogLines, string theDialogFooter, int theButtonMode)
	{
		LawnDialog lawnDialog = new LawnDialog(this, null, theDialogId, isModal, theDialogHeader, theDialogLines, theDialogFooter, theButtonMode);
		if (lawnDialog.mWidth < 380)
		{
			lawnDialog.mWidth = 380;
		}
		if (lawnDialog.mHeight > 320)
		{
			lawnDialog.mHeight = 320;
		}
		CenterDialog(lawnDialog, lawnDialog.mWidth, lawnDialog.mHeight);
		return lawnDialog;
	}

	public override bool KillDialog(int theDialogId)
	{
		if (base.KillDialog(theDialogId))
		{
			if (mDialogMap.Count == 0)
			{
				if (mBoard != null)
				{
					mWidgetManager.SetFocus(mBoard);
				}
				else if (mGameSelector != null)
				{
					mWidgetManager.SetFocus(mGameSelector);
				}
			}
			if (mBoard != null && !NeedPauseGame())
			{
				mBoard.Pause(thePause: false);
			}
			return true;
		}
		return false;
	}

	public override void ModalOpen()
	{
		if (mBoard != null && NeedPauseGame())
		{
			mBoard.Pause(thePause: true);
		}
	}

	public override void ModalClose()
	{
	}

	public void PreDisplayHook()
	{
		PreDisplayHook();
	}

	public bool ChangeDirHook(string theIntendedPath)
	{
		return false;
	}

	public bool NeedRegister()
	{
		return false;
	}

	public void UpdateRegisterInfo()
	{
	}

	public override void ButtonPress(int theId)
	{
	}

	public override void ButtonDepress(int theId)
	{
		if (theId % 10000 >= 2000 && theId % 10000 < 3000)
		{
			switch (theId - 2000)
			{
			case 0:
				KillDialog(0);
				ShowGameSelector();
				break;
			case 13:
				KillDialog(13);
				CloseRequestAsync();
				break;
			case 2:
				KillNewOptionsDialog();
				if (mBoard != null)
				{
					RestartLoopingSounds();
				}
				break;
			case 15:
				KillDialog(15);
				DoRegister();
				break;
			case 5:
				DoRegister();
				break;
			case 55:
				KillDialog(55);
				break;
			case 7:
				KillDialog(7);
				CheckForUpdates();
				break;
			case 20008:
				KillDialog(20008);
				KillDialog(8);
				break;
			case 20:
				KillDialog(20);
				mBoard.AddSunMoney(100);
				break;
			case 21:
				KillDialog(21);
				break;
			case 19:
				KillDialog(19);
				break;
			case 16:
				KillDialog(16);
				break;
			case 22:
				KillDialog(22);
				mBoardResult = BoardResult.BOARDRESULT_QUIT;
				mBoard.TryToSaveGame();
				DoBackToMain();
				break;
			case 29:
				FinishUserDialog(isYes: true);
				break;
			case 30:
				FinishCreateUserDialog(isYes: true);
				break;
			case 35:
				FinishCheatDialog(isYes: true);
				break;
			case 32:
				FinishRenameUserDialog(isYes: true);
				break;
			case 31:
				FinishConfirmDeleteUserDialog(isYes: true);
				break;
			case 33:
				FinishNameError(theId - 2000);
				break;
			case 34:
				FinishNameError(theId - 2000);
				break;
			case 39:
				FinishRestartConfirmDialog();
				break;
			case 23:
				FinishInGameRestartConfirmDialog(isYes: true);
				break;
			case 52:
				FinishAboutDialog(isYes: true);
				break;
			case 53:
				FinishRestartWarningDialog(isYes: true);
				break;
			case 51:
				FinishPacketSlotPurchaseDialog(isYes: true);
				break;
			case 46:
				((StoreScreen)GetDialog(4)).PurchasePendingItem();
				break;
			case 47:
				((StoreScreen)GetDialog(4)).FinishTreeOfWisdomDialog(isYes: true);
				break;
			case 28:
			case 49:
				KillDialog(theId - 2000);
				FinishLawnDialogMessageBox(isYes: true);
				break;
			case 42:
				FinishTimesUpDialog();
				break;
			case 48:
				FinishPlantSale(isYes: true);
				break;
			case 58:
				KillDialog(theId - 2000);
				PreNewGame(GameMode.GAMEMODE_ADVENTURE, theLookForSavedGame: true, checkForTutorialCompletion: false);
				mPlayerInfo.mHasFinishedTutorial = false;
				break;
			default:
				KillDialog(theId - 2000);
				break;
			case 6:
				break;
			}
		}
		else if (theId % 10000 >= 3000 && theId % 10000 < 4000)
		{
			switch (theId - 3000)
			{
			case 6:
				KillDialog(6);
				break;
			case 10008:
				KillDialog(10008);
				KillDialog(8);
				break;
			case 5:
				KillDialog(5);
				Shutdown();
				break;
			case 55:
				checkGiveAchievements = true;
				BuyGame();
				break;
			case 29:
				FinishUserDialog(isYes: false);
				break;
			case 30:
				FinishCreateUserDialog(isYes: false);
				break;
			case 35:
				FinishCheatDialog(isYes: false);
				break;
			case 32:
				FinishRenameUserDialog(isYes: false);
				break;
			case 31:
				FinishConfirmDeleteUserDialog(isYes: false);
				break;
			case 42:
				FinishTimesUpDialog();
				break;
			case 48:
				FinishPlantSale(isYes: false);
				break;
			case 23:
				FinishInGameRestartConfirmDialog(isYes: false);
				break;
			case 51:
				FinishPacketSlotPurchaseDialog(isYes: false);
				break;
			case 47:
				((StoreScreen)GetDialog(4)).FinishTreeOfWisdomDialog(isYes: false);
				break;
			case 28:
			case 49:
				KillDialog(theId - 3000);
				FinishLawnDialogMessageBox(isYes: false);
				break;
			case 58:
				KillDialog(theId - 3000);
				mPlayerInfo.SetLevel(4);
				PreNewGame(GameMode.GAMEMODE_ADVENTURE, theLookForSavedGame: false, checkForTutorialCompletion: false);
				break;
			default:
				KillDialog(theId - 3000);
				break;
			}
		}
	}

	public override void LeftTrialMode()
	{
		base.LeftTrialMode();
		if (mGameSelector != null)
		{
			mGameSelector.SyncButtons();
		}
	}

	public override void UpdateFrames()
	{
#if ANDROID
		if (updateFramesLogCount < 5) { try { Android.Util.Log.Info("PVZ", "LawnApp.UpdateFrames " + updateFramesLogCount + " IsLoading=" + LoadingScreen.IsLoading + " widgets=" + mWidgetManager.mWidgets.Count); } catch { } updateFramesLogCount++; }
#endif
		if (wantToShowUpdateMessage)
		{
			ShowUpdateMessage();
		}
		if (LoadingScreen.IsLoading)
		{
			LoadingScreen.gLoadingScreen.Update();
			return;
		}
		UpdatePlayTimeStats();
		int num = 1;
		if (GlobalStaticVars.gSlowMo)
		{
			GlobalStaticVars.gSlowMoCounter++;
			if (GlobalStaticVars.gSlowMoCounter >= 4)
			{
				GlobalStaticVars.gSlowMoCounter = 0;
			}
			else
			{
				num = 0;
			}
		}
		else if (GlobalStaticVars.gFastMo)
		{
			num = 20;
		}
		for (int i = 0; i < num; i++)
		{
			mAppCounter++;
			if (mBoard != null)
			{
				mBoard.ProcessDeleteQueue();
			}
			base.UpdateFrames();
			if (mLoadingThreadCompleted && mEffectSystem != null)
			{
				mEffectSystem.ProcessDeleteQueue();
			}
			CheckForGameEnd();
		}
	}

	public bool IsAdventureMode()
	{
		if (mGameMode == GameMode.GAMEMODE_ADVENTURE)
		{
			return true;
		}
		return false;
	}

	public bool IsQuickPlayMode()
	{
		if (mGameMode >= GameMode.GAMEMODE_QUICKPLAY_1 && mGameMode <= GameMode.GAMEMODE_QUICKPLAY_50)
		{
			return true;
		}
		return false;
	}

	public bool IsSurvivalMode()
	{
		if (mGameMode >= GameMode.GAMEMODE_SURVIVAL_NORMAL_STAGE_1 && mGameMode <= GameMode.GAMEMODE_SURVIVAL_ENDLESS_STAGE_5)
		{
			return true;
		}
		return false;
	}

	public bool IsContinuousChallenge()
	{
		if (IsArtChallenge() || IsSlotMachineLevel() || IsFinalBossLevel() || mGameMode == GameMode.GAMEMODE_CHALLENGE_BEGHOULED || mGameMode == GameMode.GAMEMODE_UPSELL || mGameMode == GameMode.GAMEMODE_INTRO || mGameMode == GameMode.GAMEMODE_CHALLENGE_BEGHOULED_TWIST)
		{
			return true;
		}
		return false;
	}

	public bool IsArtChallenge()
	{
		if (mBoard == null)
		{
			return false;
		}
		if (mGameMode == GameMode.GAMEMODE_CHALLENGE_ART_CHALLENGE_1 || mGameMode == GameMode.GAMEMODE_CHALLENGE_ART_CHALLENGE_2 || mGameMode == GameMode.GAMEMODE_CHALLENGE_SEEING_STARS)
		{
			return true;
		}
		return false;
	}

	public bool NeedPauseGame()
	{
		if (mDialogList.Count == 0)
		{
			return false;
		}
		int num = 0;
		if (mDialogList.Count == 1 && mDialogList.First.Value.mId != 0)
		{
			num = mDialogList.First.Value.mId;
		}
		if (num == 28 || num == 51 || num == 50)
		{
			return false;
		}
		if (mBoard != null && mGameMode == GameMode.GAMEMODE_CHALLENGE_ZEN_GARDEN)
		{
			return false;
		}
		if (mBoard != null && mGameMode == GameMode.GAMEMODE_TREE_OF_WISDOM)
		{
			return false;
		}
		return true;
	}

	public void ShowResourceError()
	{
		ShowResourceError(false);
	}

	public override void ShowResourceError(bool doExit)
	{
		base.ShowResourceError(doExit);
	}

	public void ToggleSlowMo()
	{
		GlobalStaticVars.gSlowMoCounter = 0;
		GlobalStaticVars.gSlowMo = !GlobalStaticVars.gSlowMo;
		GlobalStaticVars.gFastMo = false;
	}

	public void ToggleFastMo()
	{
		GlobalStaticVars.gFastMo = !GlobalStaticVars.gFastMo;
		GlobalStaticVars.gSlowMo = false;
	}

	public void PlayFoley(FoleyType theFoleyType)
	{
		if (!mMuteSoundsForCutscene)
		{
			mSoundSystem.PlayFoley(theFoleyType);
		}
	}

	public void PlayFoleyPitch(FoleyType theFoleyType, float aPitch)
	{
		if (!mMuteSoundsForCutscene)
		{
			mSoundSystem.PlayFoleyPitch(theFoleyType, aPitch);
		}
	}

	public void FastLoad(GameMode theGameMode)
	{
		if (!mShutdown)
		{
			mWidgetManager.RemoveWidget(mTitleScreen);
			SafeDeleteWidget(mTitleScreen);
			mTitleScreen = null;
			PreNewGame(theGameMode, theLookForSavedGame: false);
		}
	}

	public string GetStageString(int theLevel)
	{
		if (!cachedStageStrings.TryGetValue(theLevel, out var value))
		{
			int num = TodCommon.ClampInt((theLevel - 1) / 10 + 1, 1, 6);
			int num2 = theLevel - (num - 1) * 10;
			value = Common.StrFormat_(TodStringFile.TodStringTranslate("[STAGE_STRING]"), num, num2);
			cachedStageStrings.Add(theLevel, value);
		}
		return value;
	}

	public void KillChallengeScreen()
	{
		if (mChallengeScreen != null)
		{
			mWidgetManager.RemoveWidget(mChallengeScreen);
			SafeDeleteWidget(mChallengeScreen);
			mChallengeScreen = null;
		}
	}

	public void ShowChallengeScreen(ChallengePage thePage)
	{
		mGameScene = GameScenes.SCENE_CHALLENGE;
		mChallengeScreen = new ChallengeScreen(this, thePage);
		mChallengeScreen.Resize(0, 0, mWidth, mHeight);
		mWidgetManager.AddWidget(mChallengeScreen);
		mWidgetManager.BringToBack(mChallengeScreen);
		mWidgetManager.SetFocus(mChallengeScreen);
	}

	public void ShowLeaderboardScreen()
	{
		mGameScene = GameScenes.SCENE_LEADERBOARD;
		mLeaderboardScreen = new LeaderboardScreen(this);
		mLeaderboardScreen.Resize(0, 0, mWidth, mHeight);
		mWidgetManager.AddWidget(mLeaderboardScreen);
		mWidgetManager.BringToBack(mLeaderboardScreen);
		mWidgetManager.SetFocus(mLeaderboardScreen);
	}

	public void KillLeaderboardScreen()
	{
		if (mLeaderboardScreen != null)
		{
			mLeaderboardScreen.UnloadResources();
			mWidgetManager.RemoveWidget(mLeaderboardScreen);
			SafeDeleteWidget(mLeaderboardScreen);
			mLeaderboardScreen = null;
		}
	}

	public void CheckForGameEnd()
	{
		if (mBoard == null || !mBoard.mLevelComplete)
		{
			return;
		}
		bool flag = mBoard.CheckForPostGameAchievements();
		flag = false;
		UpdatePlayerProfileForFinishingLevel();
		if (IsAdventureMode())
		{
			int mLevel = mBoard.mLevel;
			KillBoard();
			if (IsFirstTimeAdventureMode() && mLevel < 50)
			{
				ShowAwardScreen(AwardType.AWARD_FOR_LEVEL, flag);
				return;
			}
			switch (mLevel)
			{
			case 50:
				if (mPlayerInfo.mFinishedAdventure != 1)
				{
					ShowAwardScreen(AwardType.AWARD_FOR_LEVEL, flag);
				}
				else
				{
					ShowAwardScreen(AwardType.AWARD_PRE_CREDITS_ZOMBIE_NOTE, flag);
				}
				break;
			case 9:
			case 19:
			case 29:
			case 39:
			case 49:
				ShowAwardScreen(AwardType.AWARD_FOR_LEVEL, flag);
				break;
			default:
				if (flag)
				{
					ShowAwardScreen(AwardType.AWARD_ACHIEVEMENT_ONLY, theShowAchievements: true);
				}
				else
				{
					PreNewGame(mGameMode, theLookForSavedGame: false);
				}
				break;
			}
		}
		else if (IsQuickPlayMode())
		{
			KillBoard();
			if (flag)
			{
				ShowAwardScreen(AwardType.AWARD_ACHIEVEMENT_ONLY, flag);
			}
			else
			{
				ShowGameSelectorQuickPlay(theDoFadeIn: false);
			}
		}
		else if (IsSurvivalMode())
		{
			if (mBoard.IsFinalSurvivalStage())
			{
				KillBoard();
				ShowGameSelectorQuickPlay(theDoFadeIn: false);
			}
			else
			{
				mBoard.mChallenge.mSurvivalStage++;
				KillSeedChooserScreen();
				mBoard.InitSurvivalStage();
			}
		}
		else if (IsPuzzleMode())
		{
			bool flag2 = IsIZombieLevel();
			KillBoard();
			if (flag2)
			{
				ShowGameSelectorQuickPlay(theDoFadeIn: false, GameSelectorButtons.GameSelector_IZombie);
			}
			else
			{
				ShowGameSelectorQuickPlay(theDoFadeIn: false, GameSelectorButtons.GameSelector_Vasebreaker);
			}
		}
		else
		{
			KillBoard();
			ShowGameSelectorQuickPlay(theDoFadeIn: false);
		}
	}

	public virtual void CloseRequestAsync()
	{
		mCloseRequest = true;
	}

	public bool IsChallengeWithoutSeedBank()
	{
		if (mGameMode == GameMode.GAMEMODE_CHALLENGE_RAINING_SEEDS || mGameMode == GameMode.GAMEMODE_UPSELL || mGameMode == GameMode.GAMEMODE_INTRO || IsWhackAZombieLevel() || IsSquirrelLevel() || IsScaryPotterLevel() || mGameMode == GameMode.GAMEMODE_CHALLENGE_ZEN_GARDEN || mGameMode == GameMode.GAMEMODE_TREE_OF_WISDOM)
		{
			return true;
		}
		return false;
	}

	public AlmanacDialog DoAlmanacDialog(SeedType theSeedType, ZombieType theZombieType, AlmanacListener theListener)
	{
		default(PerfTimer).Start();
		FinishModelessDialogs();
		AlmanacDialog almanacDialog = new AlmanacDialog(this, theListener);
		almanacDialog.Resize(0, 0, Constants.BackBufferSize.Y, Constants.BackBufferSize.X);
		AddDialog(3, almanacDialog);
		mWidgetManager.SetFocus(almanacDialog);
		if (theSeedType != SeedType.SEED_NONE)
		{
			almanacDialog.ShowPlant(theSeedType);
		}
		else if (theZombieType != ZombieType.ZOMBIE_INVALID)
		{
			almanacDialog.ShowZombie(theZombieType);
		}
		return almanacDialog;
	}

	public bool KillAlmanacDialog()
	{
		AlmanacDialog almanacDialog = (AlmanacDialog)GetDialog(Dialogs.DIALOG_ALMANAC);
		if (almanacDialog == null)
		{
			return false;
		}
		KillDialog(3);
		return true;
	}

	public int GetSeedsAvailable()
	{
		int level = mPlayerInfo.GetLevel();
		if (HasFinishedAdventure() || level > 50)
		{
			return 49;
		}
		int awardSeedForLevel = (int)GetAwardSeedForLevel(level);
		return Math.Min(49, awardSeedForLevel);
	}

	public Reanimation AddReanimation(float theX, float theY, int aRenderOrder, ReanimationType theReanimationType)
	{
		return AddReanimation(theX, theY, aRenderOrder, theReanimationType, theDoScalePos: true);
	}

	public Reanimation AddReanimation(float theX, float theY, int aRenderOrder, ReanimationType theReanimationType, bool theDoScalePos)
	{
		if (theDoScalePos)
		{
			theX *= Constants.S;
			theY *= Constants.S;
		}
		return mEffectSystem.mReanimationHolder.AllocReanimation(theX, theY, aRenderOrder, theReanimationType);
	}

	public TodParticleSystem AddTodParticle(float theX, float theY, int aRenderOrder, ParticleEffect theEffect)
	{
		return mEffectSystem.mParticleHolder.AllocParticleSystem(theX, theY, aRenderOrder, theEffect);
	}

	public TodParticleSystem ParticleGetID(TodParticleSystem theParticle)
	{
		return theParticle;
	}

	public TodParticleSystem ParticleGet(TodParticleSystem theParticleID)
	{
		return theParticleID;
	}

	public TodParticleSystem ParticleTryToGet(TodParticleSystem theParticleID)
	{
		if (theParticleID == null || !theParticleID.mActive)
		{
			return null;
		}
		return theParticleID;
	}

	public Reanimation ReanimationGetID(Reanimation theReanimation)
	{
		if (theReanimation == null || theReanimation.mDead)
		{
			return null;
		}
		return theReanimation;
	}

	public Reanimation ReanimationGet(Reanimation theReanimID)
	{
		if (theReanimID == null || !theReanimID.mActive)
		{
			return null;
		}
		return theReanimID;
	}

	public Reanimation ReanimationTryToGet(Reanimation theReanimID)
	{
		if (theReanimID == null || !theReanimID.mActive)
		{
			return null;
		}
		return theReanimID;
	}

	public void RemoveReanimation(ref Reanimation theReanimationID)
	{
		ReanimationTryToGet(theReanimationID)?.ReanimationDie();
		theReanimationID = null;
	}

	public void RemoveParticle(TodParticleSystem theParticleID)
	{
		ParticleTryToGet(theParticleID)?.ParticleSystemDie();
	}

	public StoreScreen ShowStoreScreen(StoreListener theListener)
	{
		DelayLoadGamePlayResources(doLoad: false);
		DelayLoadCachedResources(doLoad: true);
		DelayLoadMainMenuResource(doLoad: true);
		DelayLoadZenGardenResources(doLoad: true);
		FinishModelessDialogs();
		Debug.ASSERT(GetDialog(Dialogs.DIALOG_STORE) == null);
		StoreScreen storeScreen = new StoreScreen(this, theListener);
		AddDialog(4, storeScreen);
		mWidgetManager.SetFocus(storeScreen);
		return storeScreen;
	}

	public UpsellScreen ShowUpsellScreen()
	{
		FinishModelessDialogs();
		UpsellScreen upsellScreen = new UpsellScreen(this);
		AddDialog(54, upsellScreen);
		mWidgetManager.SetFocus(upsellScreen);
		return upsellScreen;
	}

	public void KillStoreScreen()
	{
		AlmanacDialog almanacDialog = (AlmanacDialog)GetDialog(Dialogs.DIALOG_STORE);
		if (almanacDialog != null)
		{
			KillDialog(4);
		}
	}

	public bool HasSeedType(SeedType theSeedType)
	{
		if (theSeedType == SeedType.SEED_GATLINGPEA)
		{
			return mPlayerInfo.mPurchases[0] > 0;
		}
		if (IsTrialStageLocked() && theSeedType >= SeedType.SEED_JALAPENO)
		{
			return false;
		}
		switch (theSeedType)
		{
		case SeedType.SEED_TWINSUNFLOWER:
			return mPlayerInfo.mPurchases[1] > 0;
		case SeedType.SEED_GLOOMSHROOM:
			return mPlayerInfo.mPurchases[2] > 0;
		case SeedType.SEED_CATTAIL:
			return mPlayerInfo.mPurchases[3] > 0;
		case SeedType.SEED_WINTERMELON:
			return mPlayerInfo.mPurchases[4] > 0;
		case SeedType.SEED_GOLD_MAGNET:
			return mPlayerInfo.mPurchases[5] > 0;
		case SeedType.SEED_SPIKEROCK:
			return mPlayerInfo.mPurchases[6] > 0;
		case SeedType.SEED_COBCANNON:
			return mPlayerInfo.mPurchases[7] > 0;
		case SeedType.SEED_IMITATER:
			return mPlayerInfo.mPurchases[8] > 0;
		default:
			if ((int)theSeedType >= GetSeedsAvailable())
			{
				return false;
			}
			return true;
		}
	}

	public void EndLevel()
	{
		KillBoard();
		if (IsAdventureMode())
		{
			NewGame();
		}
	}

	public bool IsIceDemo()
	{
		return false;
	}

	public bool IsShovelLevel()
	{
		if (mBoard == null)
		{
			return false;
		}
		if (mGameMode == GameMode.GAMEMODE_CHALLENGE_SHOVEL)
		{
			return true;
		}
		return false;
	}

	public bool IsWallnutBowlingLevel()
	{
		if (mBoard == null)
		{
			return false;
		}
		if (mGameMode == GameMode.GAMEMODE_CHALLENGE_WALLNUT_BOWLING || mGameMode == GameMode.GAMEMODE_CHALLENGE_WALLNUT_BOWLING_2)
		{
			return true;
		}
		if ((IsAdventureMode() && mPlayerInfo.mLevel == 5) || mGameMode == GameMode.GAMEMODE_QUICKPLAY_5)
		{
			return true;
		}
		return false;
	}

	public bool IsMiniBossLevel()
	{
		if (mBoard == null)
		{
			return false;
		}
		if ((IsAdventureMode() && mPlayerInfo.mLevel == 10) || mGameMode == GameMode.GAMEMODE_QUICKPLAY_10)
		{
			return true;
		}
		if ((IsAdventureMode() && mPlayerInfo.mLevel == 20) || mGameMode == GameMode.GAMEMODE_QUICKPLAY_20)
		{
			return true;
		}
		if ((IsAdventureMode() && mPlayerInfo.mLevel == 30) || mGameMode == GameMode.GAMEMODE_QUICKPLAY_30)
		{
			return true;
		}
		return false;
	}

	public bool IsSlotMachineLevel()
	{
		if (mBoard == null)
		{
			return false;
		}
		if (mGameMode == GameMode.GAMEMODE_CHALLENGE_SLOT_MACHINE)
		{
			return true;
		}
		return false;
	}

	public bool IsLittleTroubleLevel()
	{
		if (mBoard == null)
		{
			return false;
		}
		if (mGameMode == GameMode.GAMEMODE_CHALLENGE_LITTLE_TROUBLE)
		{
			return true;
		}
		if ((IsAdventureMode() && mPlayerInfo.mLevel == 25) || mGameMode == GameMode.GAMEMODE_QUICKPLAY_25)
		{
			return true;
		}
		return false;
	}

	public bool IsStormyNightLevel()
	{
		if (mBoard == null)
		{
			return false;
		}
		if (mGameMode == GameMode.GAMEMODE_CHALLENGE_STORMY_NIGHT)
		{
			return true;
		}
		if ((IsAdventureMode() && mPlayerInfo.mLevel == 40) || mGameMode == GameMode.GAMEMODE_QUICKPLAY_40)
		{
			return true;
		}
		return false;
	}

	public bool IsFinalBossLevel()
	{
		if (mBoard == null)
		{
			return false;
		}
		if (mGameMode == GameMode.GAMEMODE_CHALLENGE_FINAL_BOSS)
		{
			return true;
		}
		if ((IsAdventureMode() && mPlayerInfo.mLevel == 50) || mGameMode == GameMode.GAMEMODE_QUICKPLAY_50)
		{
			return true;
		}
		return false;
	}

	public bool IsBungeeBlitzLevel()
	{
		if (mBoard == null)
		{
			return false;
		}
		if (mGameMode == GameMode.GAMEMODE_CHALLENGE_BUNGEE_BLITZ)
		{
			return true;
		}
		if ((IsAdventureMode() && mPlayerInfo.mLevel == 45) || mGameMode == GameMode.GAMEMODE_QUICKPLAY_45)
		{
			return true;
		}
		return false;
	}

	public SeedType GetAwardSeedForLevel(int theLevel)
	{
		int num = (theLevel - 1) / 10 + 1;
		int num2 = (theLevel - 1) % 10 + 1;
		int num3 = (num - 1) * 8 + num2;
		if (num2 >= 10)
		{
			num3 -= 2;
		}
		else if (num2 >= 5)
		{
			num3--;
		}
		if (num3 > 40)
		{
			num3 = 40;
		}
		return (SeedType)num3;
	}

	public string GetCrazyDaveText(int theMessageIndex)
	{
		string theText = Common.StrFormat_("[CRAZY_DAVE_{0}]", theMessageIndex);
		theText = TodCommon.TodReplaceString(theText, "{PLAYER_NAME}", mPlayerInfo.mName);
		theText = TodCommon.TodReplaceString(theText, "{MONEY}", GetMoneyString(mPlayerInfo.mCoins));
		int itemCost = StoreScreen.GetItemCost(StoreItem.STORE_ITEM_PACKET_UPGRADE);
		return TodCommon.TodReplaceString(theText, "{UPGRADE_COST}", GetMoneyString(itemCost));
	}

	public bool CanShowAlmanac()
	{
		if (IsIceDemo())
		{
			return false;
		}
		if (mPlayerInfo == null)
		{
			return false;
		}
		if (HasFinishedAdventure())
		{
			return true;
		}
		if (mPlayerInfo.mLevel < 15)
		{
			return false;
		}
		return true;
	}

	public bool IsNight()
	{
		if (mBoard != null && mBoard.StageIsNight())
		{
			return true;
		}
		if (IsIceDemo())
		{
			return false;
		}
		if (mPlayerInfo == null)
		{
			return false;
		}
		if (mPlayerInfo.mLevel >= 11 && mPlayerInfo.mLevel <= 20)
		{
			return true;
		}
		if (mPlayerInfo.mLevel >= 31 && mPlayerInfo.mLevel <= 40)
		{
			return true;
		}
		if (mPlayerInfo.mLevel == 50)
		{
			return true;
		}
		return false;
	}

	public bool CanShowStore()
	{
		if (IsIceDemo())
		{
			return false;
		}
		if (mPlayerInfo == null)
		{
			return false;
		}
		if (HasFinishedAdventure())
		{
			return true;
		}
		if (mPlayerInfo.mHasSeenUpsell)
		{
			return true;
		}
		if (mPlayerInfo.mLevel < 25)
		{
			return false;
		}
		return true;
	}

	public bool HasBeatenChallenge(GameMode theGameMode)
	{
		if (mPlayerInfo == null)
		{
			return false;
		}
		int num = (int)(theGameMode - 1);
		Debug.ASSERT(num >= 0 && num < 122);
		if (IsSurvivalNormal(theGameMode))
		{
			if (mPlayerInfo.mChallengeRecords[num] >= 5)
			{
				return true;
			}
			return false;
		}
		if (IsSurvivalHard(theGameMode))
		{
			if (mPlayerInfo.mChallengeRecords[num] >= 10)
			{
				return true;
			}
			return false;
		}
		if (IsSurvivalEndless(theGameMode))
		{
			return false;
		}
		if (IsEndlessScaryPotter(theGameMode))
		{
			return false;
		}
		if (IsEndlessIZombie(theGameMode))
		{
			return false;
		}
		if (mPlayerInfo.mChallengeRecords[num] > 0)
		{
			return true;
		}
		return false;
	}

	public PottedPlant GetPottedPlantByIndex(int thePottedPlantIndex)
	{
		Debug.ASSERT(thePottedPlantIndex >= 0 && thePottedPlantIndex < mPlayerInfo.mNumPottedPlants);
		return mPlayerInfo.mPottedPlant[thePottedPlantIndex];
	}

	public bool IsSurvivalNormal(GameMode theGameMode)
	{
		if (theGameMode >= GameMode.GAMEMODE_SURVIVAL_NORMAL_STAGE_1 && theGameMode <= GameMode.GAMEMODE_SURVIVAL_NORMAL_STAGE_5)
		{
			return true;
		}
		return false;
	}

	public bool IsSurvivalHard(GameMode theGameMode)
	{
		if (theGameMode >= GameMode.GAMEMODE_SURVIVAL_HARD_STAGE_1 && theGameMode <= GameMode.GAMEMODE_SURVIVAL_HARD_STAGE_5)
		{
			return true;
		}
		return false;
	}

	public bool IsSurvivalEndless(GameMode theGameMode)
	{
		if (theGameMode >= GameMode.GAMEMODE_SURVIVAL_ENDLESS_STAGE_1 && theGameMode <= GameMode.GAMEMODE_SURVIVAL_ENDLESS_STAGE_5)
		{
			return true;
		}
		return false;
	}

	public bool HasFinishedAdventure()
	{
		if (mPlayerInfo != null && mPlayerInfo.mFinishedAdventure > 0)
		{
			return true;
		}
		return false;
	}

	public bool IsFirstTimeAdventureMode()
	{
		if (IsAdventureMode() && !HasFinishedAdventure())
		{
			return true;
		}
		return false;
	}

	public bool CanSpawnYetis()
	{
		ZombieDefinition zombieDefinition = Zombie.GetZombieDefinition(ZombieType.ZOMBIE_YETI);
		if (!HasFinishedAdventure())
		{
			return false;
		}
		if (mPlayerInfo.mFinishedAdventure >= 2)
		{
			return true;
		}
		if (mPlayerInfo.mLevel >= zombieDefinition.mStartingLevel)
		{
			return true;
		}
		return false;
	}

	public void CrazyDaveEnter()
	{
		Debug.ASSERT(mCrazyDaveState == CrazyDaveState.CRAZY_DAVE_OFF);
		Debug.ASSERT(ReanimationTryToGet(mCrazyDaveReanimID) == null);
		Reanimation reanimation = AddReanimation(0f, 0f, 0, ReanimationType.REANIM_CRAZY_DAVE);
		reanimation.mIsAttachment = true;
		reanimation.SetBasePoseFromAnim(GlobalMembersReanimIds.ReanimTrackId_anim_idle_handing);
		mCrazyDaveReanimID = ReanimationGetID(reanimation);
		reanimation.PlayReanim(GlobalMembersReanimIds.ReanimTrackId_anim_enter, ReanimLoopType.REANIM_PLAY_ONCE_AND_HOLD, 0, 24f);
		mCrazyDaveState = CrazyDaveState.CRAZY_DAVE_ENTERING;
		mCrazyDaveMessageIndex = -1;
		mCrazyDaveMessageText = string.Empty;
		mCrazyDaveBlinkCounter = TodCommon.RandRangeInt(400, 800);
		if (mGameScene == GameScenes.SCENE_LEVEL_INTRO && IsStormyNightLevel())
		{
			reanimation.mColorOverride = new SexyColor(64, 64, 64);
		}
	}

	public void FinishZenGardenTutorial()
	{
		mZenGarden.mIsTutorial = false;
		mPlayerInfo.mZenGardenTutorialComplete = true;
		mPlayerInfo.mIsInZenTutorial = false;
		mBoardResult = BoardResult.BOARDRESULT_WON;
		KillBoard();
		PreNewGame(GameMode.GAMEMODE_ADVENTURE, theLookForSavedGame: false);
	}

	public void UpdateCrazyDave()
	{
		Reanimation theAttachReanim = ReanimationTryToGet(mCrazyDaveReanimID);
		if (theAttachReanim == null)
		{
			return;
		}
		if (mCrazyDaveState == CrazyDaveState.CRAZY_DAVE_ENTERING || mCrazyDaveState == CrazyDaveState.CRAZY_DAVE_TALKING)
		{
			if (theAttachReanim.mLoopCount > 0)
			{
				theAttachReanim.PlayReanim(GlobalMembersReanimIds.ReanimTrackId_anim_idle, ReanimLoopType.REANIM_LOOP, 20, 12f);
				mCrazyDaveState = CrazyDaveState.CRAZY_DAVE_IDLING;
			}
		}
		else if (mCrazyDaveState == CrazyDaveState.CRAZY_DAVE_HANDING_TALKING)
		{
			if (theAttachReanim.mLoopCount > 0)
			{
				theAttachReanim.PlayReanim(GlobalMembersReanimIds.ReanimTrackId_anim_idle_handing, ReanimLoopType.REANIM_LOOP, 20, 12f);
				mCrazyDaveState = CrazyDaveState.CRAZY_DAVE_HANDING_IDLING;
			}
		}
		else if (mCrazyDaveState == CrazyDaveState.CRAZY_DAVE_LEAVING && theAttachReanim.mLoopCount > 0)
		{
			CrazyDaveDie();
		}
		if (mCrazyDaveState == CrazyDaveState.CRAZY_DAVE_IDLING || mCrazyDaveState == CrazyDaveState.CRAZY_DAVE_HANDING_IDLING)
		{
			if (mCrazyDaveMessageText.IndexOf("{MOUTH_BIG_SMILE}") != -1)
			{
				theAttachReanim.SetImageOverride(GlobalMembersReanimIds.ReanimTrackId_dave_mouths, AtlasResources.IMAGE_REANIM_CRAZYDAVE_MOUTH1);
				mCrazyDaveMessageText = mCrazyDaveMessageText.Replace("{MOUTH_BIG_SMILE}", "");
			}
			else if (mCrazyDaveMessageText.IndexOf("{MOUTH_SMALL_SMILE}") != -1)
			{
				theAttachReanim.SetImageOverride(GlobalMembersReanimIds.ReanimTrackId_dave_mouths, AtlasResources.IMAGE_REANIM_CRAZYDAVE_MOUTH5);
				mCrazyDaveMessageText = mCrazyDaveMessageText.Replace("{MOUTH_SMALL_SMILE}", "");
			}
			else if (mCrazyDaveMessageText.IndexOf("{MOUTH_BIG_OH}") != -1)
			{
				theAttachReanim.SetImageOverride(GlobalMembersReanimIds.ReanimTrackId_dave_mouths, AtlasResources.IMAGE_REANIM_CRAZYDAVE_MOUTH4);
				mCrazyDaveMessageText = mCrazyDaveMessageText.Replace("{MOUTH_BIG_OH}", "");
			}
			else if (mCrazyDaveMessageText.IndexOf("{MOUTH_SMALL_OH}") != -1)
			{
				theAttachReanim.SetImageOverride(GlobalMembersReanimIds.ReanimTrackId_dave_mouths, AtlasResources.IMAGE_REANIM_CRAZYDAVE_MOUTH6);
				mCrazyDaveMessageText = mCrazyDaveMessageText.Replace("{MOUTH_SMALL_OH}", "");
			}
		}
		Reanimation reanimation;
		if (mCrazyDaveState == CrazyDaveState.CRAZY_DAVE_IDLING || mCrazyDaveState == CrazyDaveState.CRAZY_DAVE_TALKING || mCrazyDaveState == CrazyDaveState.CRAZY_DAVE_HANDING_TALKING || mCrazyDaveState == CrazyDaveState.CRAZY_DAVE_HANDING_IDLING)
		{
			mCrazyDaveBlinkCounter--;
			if (mCrazyDaveBlinkCounter <= 0)
			{
				mCrazyDaveBlinkCounter = TodCommon.RandRangeInt(400, 800);
				reanimation = AddReanimation(0f, 0f, 0, ReanimationType.REANIM_CRAZY_DAVE);
				reanimation.SetFramesForLayer(GlobalMembersReanimIds.ReanimTrackId_anim_blink);
				reanimation.mLoopType = ReanimLoopType.REANIM_PLAY_ONCE_FULL_LAST_FRAME_AND_HOLD;
				reanimation.mAnimRate = 15f;
				reanimation.AttachToAnotherReanimation(ref theAttachReanim, GlobalMembersReanimIds.ReanimTrackId_dave_head);
				reanimation.mColorOverride = theAttachReanim.mColorOverride;
				theAttachReanim.AssignRenderGroupToTrack(GlobalMembersReanimIds.ReanimTrackId_dave_eye, -1);
				mCrazyDaveBlinkReanimID = ReanimationGetID(reanimation);
			}
		}
		reanimation = ReanimationTryToGet(mCrazyDaveBlinkReanimID);
		if (reanimation != null && reanimation.mLoopCount > 0)
		{
			theAttachReanim.AssignRenderGroupToTrack(GlobalMembersReanimIds.ReanimTrackId_dave_eye, 0);
			RemoveReanimation(ref mCrazyDaveBlinkReanimID);
			mCrazyDaveBlinkReanimID = null;
		}
		theAttachReanim.Update();
	}

	public void CrazyDaveTalkIndex(int theMessageIndex)
	{
		mCrazyDaveMessageIndex = theMessageIndex;
		string crazyDaveText = GetCrazyDaveText(theMessageIndex);
		CrazyDaveTalkMessage(crazyDaveText);
	}

	public void CrazyDaveTalkMessage(string theMessage)
	{
		Reanimation reanimation = ReanimationGet(mCrazyDaveReanimID);
		bool flag = false;
		if (theMessage.IndexOf("{HANDING}") != -1)
		{
			flag = true;
			theMessage = theMessage.Replace("{HANDING}", string.Empty);
		}
		if ((mCrazyDaveState == CrazyDaveState.CRAZY_DAVE_HANDING_TALKING || mCrazyDaveState == CrazyDaveState.CRAZY_DAVE_HANDING_IDLING) && !flag)
		{
			CrazyDaveDoneHanding();
		}
		bool flag2 = true;
		if (theMessage.IndexOf("{NO_SOUND}") != -1)
		{
			flag2 = false;
			theMessage = theMessage.Replace("{NO_SOUND}", "");
		}
		else
		{
			CrazyDaveStopSound();
		}
		int num = 0;
		bool flag3 = false;
		for (int i = 0; i < theMessage.length(); i++)
		{
			if (theMessage[i] == '{')
			{
				flag3 = true;
			}
			else if (theMessage[i] == '}')
			{
				flag3 = false;
			}
			else if (!flag3)
			{
				num++;
			}
		}
		Image theImage = null;
		reanimation.SetImageOverride(GlobalMembersReanimIds.ReanimTrackId_dave_mouths, theImage);
		if (mCrazyDaveState != CrazyDaveState.CRAZY_DAVE_TALKING || flag2)
		{
			if (flag)
			{
				reanimation.PlayReanim(GlobalMembersReanimIds.ReanimTrackId_anim_talk_handing, ReanimLoopType.REANIM_LOOP, 50, 12f);
				if (flag2 && theMessage.IndexOf("{SHORT_SOUND}") != -1)
				{
					PlayFoley(FoleyType.FOLEY_CRAZYDAVESHORT);
					theMessage = theMessage.Replace("{SHORT_SOUND}", "");
				}
				else if (flag2 && theMessage.IndexOf("{SCREAM}") != -1)
				{
					PlayFoley(FoleyType.FOLEY_CRAZYDAVESCREAM);
					theMessage = theMessage.Replace("{SCREAM}", "");
				}
				else if (flag2)
				{
					PlayFoley(FoleyType.FOLEY_CRAZYDAVELONG);
				}
				mCrazyDaveState = CrazyDaveState.CRAZY_DAVE_HANDING_TALKING;
			}
			else if (theMessage.IndexOf("{SHAKE}") != -1)
			{
				reanimation.PlayReanim(GlobalMembersReanimIds.ReanimTrackId_anim_crazy, ReanimLoopType.REANIM_PLAY_ONCE_AND_HOLD, 50, 12f);
				if (flag2)
				{
					PlayFoley(FoleyType.FOLEY_CRAZYDAVECRAZY);
				}
				mCrazyDaveState = CrazyDaveState.CRAZY_DAVE_TALKING;
				theMessage = theMessage.Replace("{SHAKE}", "");
			}
			else if (theMessage.IndexOf("{SCREAM}") != -1)
			{
				reanimation.PlayReanim(GlobalMembersReanimIds.ReanimTrackId_anim_smalltalk, ReanimLoopType.REANIM_PLAY_ONCE_AND_HOLD, 50, 12f);
				if (flag2)
				{
					PlayFoley(FoleyType.FOLEY_CRAZYDAVESCREAM);
				}
				mCrazyDaveState = CrazyDaveState.CRAZY_DAVE_TALKING;
				theMessage = theMessage.Replace("{SCREAM}", "");
			}
			else if (theMessage.IndexOf("{SCREAM2}") != -1)
			{
				reanimation.PlayReanim(GlobalMembersReanimIds.ReanimTrackId_anim_mediumtalk, ReanimLoopType.REANIM_PLAY_ONCE_AND_HOLD, 50, 12f);
				if (flag2)
				{
					PlayFoley(FoleyType.FOLEY_CRAZYDAVESCREAM2);
				}
				mCrazyDaveState = CrazyDaveState.CRAZY_DAVE_TALKING;
				theMessage = theMessage.Replace("{SCREAM2}", "");
			}
			else if (theMessage.IndexOf("{SHOW_WALLNUT}") != -1)
			{
				reanimation.PlayReanim(GlobalMembersReanimIds.ReanimTrackId_anim_talk_handing, ReanimLoopType.REANIM_LOOP, 50, 12f);
				Reanimation reanimation2 = AddReanimation(0f, 0f, 0, ReanimationType.REANIM_WALLNUT);
				reanimation2.PlayReanim(GlobalMembersReanimIds.ReanimTrackId_anim_idle, ReanimLoopType.REANIM_LOOP, 0, 12f);
				ReanimatorTrackInstance trackInstanceByName = reanimation.GetTrackInstanceByName(GlobalMembersReanimIds.ReanimTrackId_dave_handinghand);
				AttachEffect attachEffect = GlobalMembersAttachment.AttachReanim(ref trackInstanceByName.mAttachmentID, reanimation2, 100f * Constants.S, 393f * Constants.S);
				attachEffect.mOffset.mMatrix.M11 = 1.2f;
				attachEffect.mOffset.mMatrix.M22 = 1.2f;
				reanimation.Update();
				if (flag2)
				{
					PlayFoley(FoleyType.FOLEY_CRAZYDAVESCREAM2);
				}
				mCrazyDaveState = CrazyDaveState.CRAZY_DAVE_HANDING_TALKING;
				theMessage = theMessage.Replace("{SHOW_WALLNUT}", "");
			}
			else if (theMessage.IndexOf("{SHOW_HAMMER}") != -1)
			{
				reanimation.PlayReanim(GlobalMembersReanimIds.ReanimTrackId_anim_talk_handing, ReanimLoopType.REANIM_LOOP, 50, 12f);
				Reanimation reanimation3 = AddReanimation(0f, 0f, 0, ReanimationType.REANIM_HAMMER);
				reanimation3.PlayReanim(GlobalMembersReanimIds.ReanimTrackId_anim_whack_zombie, ReanimLoopType.REANIM_PLAY_ONCE_AND_HOLD, 0, 24f);
				reanimation3.mAnimTime = 1f;
				ReanimatorTrackInstance trackInstanceByName2 = reanimation.GetTrackInstanceByName(GlobalMembersReanimIds.ReanimTrackId_dave_handinghand);
				AttachEffect attachEffect2 = GlobalMembersAttachment.AttachReanim(ref trackInstanceByName2.mAttachmentID, reanimation3, 62f * Constants.S, 445f * Constants.S);
				attachEffect2.mOffset.mMatrix.M11 = 1.5f;
				attachEffect2.mOffset.mMatrix.M22 = 1.5f;
				reanimation.Update();
				if (flag2)
				{
					PlayFoley(FoleyType.FOLEY_CRAZYDAVELONG);
				}
				mCrazyDaveState = CrazyDaveState.CRAZY_DAVE_HANDING_TALKING;
				theMessage = theMessage.Replace("{SHOW_HAMMER}", "");
			}
			else if (num < 23)
			{
				reanimation.PlayReanim(GlobalMembersReanimIds.ReanimTrackId_anim_smalltalk, ReanimLoopType.REANIM_PLAY_ONCE_AND_HOLD, 50, 12f);
				if (flag2)
				{
					PlayFoley(FoleyType.FOLEY_CRAZYDAVESHORT);
				}
				mCrazyDaveState = CrazyDaveState.CRAZY_DAVE_TALKING;
			}
			else if (num < 52)
			{
				reanimation.PlayReanim(GlobalMembersReanimIds.ReanimTrackId_anim_mediumtalk, ReanimLoopType.REANIM_PLAY_ONCE_AND_HOLD, 50, 12f);
				if (flag2)
				{
					PlayFoley(FoleyType.FOLEY_CRAZYDAVELONG);
				}
				mCrazyDaveState = CrazyDaveState.CRAZY_DAVE_TALKING;
			}
			else
			{
				reanimation.PlayReanim(GlobalMembersReanimIds.ReanimTrackId_anim_blahblah, ReanimLoopType.REANIM_PLAY_ONCE_AND_HOLD, 50, 12f);
				if (flag2)
				{
					PlayFoley(FoleyType.FOLEY_CRAZYDAVEEXTRALONG);
				}
				mCrazyDaveState = CrazyDaveState.CRAZY_DAVE_TALKING;
			}
		}
		mCrazyDaveMessageText = theMessage;
	}

	public void CrazyDaveLeave()
	{
		Reanimation reanimation = ReanimationTryToGet(mCrazyDaveReanimID);
		if (reanimation != null)
		{
			if (mCrazyDaveState == CrazyDaveState.CRAZY_DAVE_HANDING_TALKING || mCrazyDaveState == CrazyDaveState.CRAZY_DAVE_HANDING_IDLING)
			{
				CrazyDaveDoneHanding();
			}
			reanimation.PlayReanim(GlobalMembersReanimIds.ReanimTrackId_anim_leave, ReanimLoopType.REANIM_PLAY_ONCE_AND_HOLD, 20, 24f);
			Image theImage = null;
			reanimation.SetImageOverride(GlobalMembersReanimIds.ReanimTrackId_dave_mouths, theImage);
			mCrazyDaveState = CrazyDaveState.CRAZY_DAVE_LEAVING;
			mCrazyDaveMessageIndex = -1;
			mCrazyDaveMessageText = string.Empty;
			CrazyDaveStopSound();
		}
	}

	public void DrawCrazyDave(Graphics g)
	{
		DrawCrazyDave(g, theUseSmallFont: false);
	}

	public void DrawCrazyDave(Graphics g, bool theUseSmallFont)
	{
		Reanimation reanimation = ReanimationTryToGet(mCrazyDaveReanimID);
		if (reanimation == null)
		{
			return;
		}
		int theWidth = Constants.RetardedDave_Bubble_Size;
		if (!mCrazyDaveMessageText.empty())
		{
			int num = (int)Constants.InvertAndScale(285f);
			int num2 = (int)Constants.InvertAndScale(80f);
			Image iMAGE_STORE_SPEECHBUBBLE = AtlasResources.IMAGE_STORE_SPEECHBUBBLE;
			if (GetDialog(Dialogs.DIALOG_STORE) != null)
			{
				num += Constants.RetardedDave_Bubble_Offset_Shop.X;
				num2 += Constants.RetardedDave_Bubble_Offset_Shop.Y;
				theWidth = (int)Constants.InvertAndScale(150f);
				int num3 = (int)Constants.InvertAndScale(105f);
				g.DrawImage(iMAGE_STORE_SPEECHBUBBLE, num, num2, new TRect(0, 0, (int)Constants.InvertAndScale(64f), iMAGE_STORE_SPEECHBUBBLE.mHeight));
				g.DrawImage(iMAGE_STORE_SPEECHBUBBLE, num + (int)Constants.InvertAndScale(64f), num2, new TRect(iMAGE_STORE_SPEECHBUBBLE.mWidth - num3, 0, num3, iMAGE_STORE_SPEECHBUBBLE.mHeight));
			}
			else if (mGameMode == GameMode.GAMEMODE_CHALLENGE_ZEN_GARDEN)
			{
				num += Constants.ZenGarden_RetardedDaveBubble_Pos.X;
				num2 += Constants.ZenGarden_RetardedDaveBubble_Pos.Y;
				g.DrawImage(iMAGE_STORE_SPEECHBUBBLE, num, num2);
			}
			else
			{
				num += Constants.RetardedDave_Bubble_Offset_Board.X;
				num2 += Constants.RetardedDave_Bubble_Offset_Board.Y;
				g.DrawImage(iMAGE_STORE_SPEECHBUBBLE, num, num2);
			}
			g.DrawImage(AtlasResources.IMAGE_STORE_SPEECHBUBBLE_TIP, num + (int)Constants.InvertAndScale(30f), num2 - Constants.RetardedDave_Bubble_Tip_Offset + iMAGE_STORE_SPEECHBUBBLE.mHeight);
			string text = mCrazyDaveMessageText;
			TRect theRect = new TRect(num + Constants.RetardedDave_Bubble_Rect.mX, num2 + Constants.RetardedDave_Bubble_Rect.mY, theWidth, Constants.RetardedDave_Bubble_Rect.mHeight);
			int mX = theRect.mX;
			if (text.IndexOf("{SHAKE}") != -1)
			{
				text = TodCommon.TodReplaceString(text, "{SHAKE}", "");
				theRect.mX += RandomNumbers.NextNumber() % 2;
				theRect.mY += RandomNumbers.NextNumber() % 2;
			}
			bool flag = true;
			if (mGameMode == GameMode.GAMEMODE_UPSELL)
			{
				flag = false;
			}
			else if (text.IndexOf("{NO_CLICK}") != -1)
			{
				if (!noClickStringCache.TryGetValue(text, out var value))
				{
					value = TodCommon.TodReplaceString(text, "{NO_CLICK}", string.Empty);
					noClickStringCache.Add(text, value);
				}
				text = value;
				flag = false;
			}
			g.SetColor(SexyColor.Black);
			g.SetFont(theUseSmallFont ? Resources.FONT_BRIANNETOD12 : Resources.FONT_BRIANNETOD16);
			TodStringFile.TodDrawStringWrapped(g, text, theRect, theUseSmallFont ? Resources.FONT_BRIANNETOD12 : Resources.FONT_BRIANNETOD16, SexyColor.Black, DrawStringJustification.DS_ALIGN_CENTER_VERTICAL_MIDDLE);
			if (flag)
			{
				TodCommon.TodDrawString(g, "[TAP_TO_CONTINUE]", mX + theRect.mWidth / 2, num2 + Constants.RetardedDave_Bubble_TapToContinue_Y, Resources.FONT_PICO129, SexyColor.Black, DrawStringJustification.DS_ALIGN_CENTER);
			}
		}
		reanimation.Draw(g);
	}

	public void CrazyDaveDie()
	{
		Reanimation reanimation = ReanimationTryToGet(mCrazyDaveReanimID);
		if (reanimation != null)
		{
			reanimation.ReanimationDie();
			mCrazyDaveState = CrazyDaveState.CRAZY_DAVE_OFF;
			mCrazyDaveReanimID = null;
			mCrazyDaveMessageIndex = -1;
			mCrazyDaveMessageText = string.Empty;
			CrazyDaveStopSound();
		}
	}

	public void DoUpsellScreen()
	{
	}

	public void CrazyDaveStopTalking()
	{
		bool flag = true;
		if (mGameMode == GameMode.GAMEMODE_UPSELL)
		{
			flag = false;
		}
		if (flag && mCrazyDaveState == CrazyDaveState.CRAZY_DAVE_HANDING_TALKING)
		{
			CrazyDaveDoneHanding();
		}
		Image theImage = null;
		Reanimation reanimation = ReanimationGet(mCrazyDaveReanimID);
		reanimation.SetImageOverride(GlobalMembersReanimIds.ReanimTrackId_dave_mouths, theImage);
		if (mCrazyDaveState == CrazyDaveState.CRAZY_DAVE_HANDING_TALKING && !flag)
		{
			reanimation.PlayReanim(GlobalMembersReanimIds.ReanimTrackId_anim_idle_handing, ReanimLoopType.REANIM_LOOP, 20, 12f);
			mCrazyDaveState = CrazyDaveState.CRAZY_DAVE_HANDING_IDLING;
		}
		else if (mCrazyDaveState == CrazyDaveState.CRAZY_DAVE_TALKING || mCrazyDaveState == CrazyDaveState.CRAZY_DAVE_HANDING_TALKING)
		{
			reanimation.PlayReanim(GlobalMembersReanimIds.ReanimTrackId_anim_idle, ReanimLoopType.REANIM_LOOP, 20, 12f);
			mCrazyDaveState = CrazyDaveState.CRAZY_DAVE_IDLING;
		}
		mCrazyDaveMessageIndex = -1;
		mCrazyDaveMessageText = string.Empty;
		CrazyDaveStopSound();
	}

	public void PreloadForUser()
	{
		int num = mCompletedLoadingThreadTasks + GetNumPreloadingTasks();
		if (mTitleScreen != null && mTitleScreen.mQuickLoadKey != KeyCode.KEYCODE_UNKNOWN)
		{
			mCompletedLoadingThreadTasks = num;
			return;
		}
		ReanimatorXnaHelpers.ReanimatorEnsureDefinitionLoaded(ReanimationType.REANIM_PUFF, theIsPreloading: true);
		ReanimatorXnaHelpers.ReanimatorEnsureDefinitionLoaded(ReanimationType.REANIM_LAWN_MOWERED_ZOMBIE, theIsPreloading: true);
		ReanimatorXnaHelpers.ReanimatorEnsureDefinitionLoaded(ReanimationType.REANIM_READYSETPLANT, theIsPreloading: true);
		mCompletedLoadingThreadTasks += 68;
		ReanimatorXnaHelpers.ReanimatorEnsureDefinitionLoaded(ReanimationType.REANIM_FINAL_WAVE, theIsPreloading: true);
		ReanimatorXnaHelpers.ReanimatorEnsureDefinitionLoaded(ReanimationType.REANIM_SUN, theIsPreloading: true);
		ReanimatorXnaHelpers.ReanimatorEnsureDefinitionLoaded(ReanimationType.REANIM_TEXT_FADE_ON, theIsPreloading: true);
		mCompletedLoadingThreadTasks += 68;
		ReanimatorXnaHelpers.ReanimatorEnsureDefinitionLoaded(ReanimationType.REANIM_ZOMBIE, theIsPreloading: true);
		mCompletedLoadingThreadTasks += 68;
		ReanimatorXnaHelpers.ReanimatorEnsureDefinitionLoaded(ReanimationType.REANIM_ZOMBIE_NEWSPAPER, theIsPreloading: true);
		mCompletedLoadingThreadTasks += 68;
		ReanimatorXnaHelpers.ReanimatorEnsureDefinitionLoaded(ReanimationType.REANIM_SELECTOR_SCREEN, theIsPreloading: true);
		mCompletedLoadingThreadTasks += 340;
		mCompletedLoadingThreadTasks += 68;
		if (mPlayerInfo != null)
		{
			for (int i = 0; i < 53; i++)
			{
				SeedType theSeedType = (SeedType)i;
				if (HasSeedType(theSeedType) || HasFinishedAdventure())
				{
					Plant.PreloadPlantResources(theSeedType);
					if (mCompletedLoadingThreadTasks < num)
					{
						mCompletedLoadingThreadTasks += 68;
					}
					if (mTitleScreen != null && mTitleScreen.mQuickLoadKey != KeyCode.KEYCODE_UNKNOWN)
					{
						mCompletedLoadingThreadTasks = num;
						return;
					}
					if (mShutdown || mCloseRequest)
					{
						return;
					}
				}
			}
			for (int j = 0; j < 33; j++)
			{
				ZombieType zombieType = (ZombieType)j;
				if (!HasFinishedAdventure())
				{
					ZombieDefinition zombieDefinition = Zombie.GetZombieDefinition(zombieType);
					if (mPlayerInfo.mLevel < zombieDefinition.mStartingLevel)
					{
						continue;
					}
				}
				if (zombieType != ZombieType.ZOMBIE_BOSS && zombieType != ZombieType.ZOMBIE_CATAPULT && zombieType != ZombieType.ZOMBIE_GARGANTUAR && zombieType != ZombieType.ZOMBIE_DIGGER && zombieType != ZombieType.ZOMBIE_ZAMBONI)
				{
					Zombie.PreloadZombieResources(zombieType);
					if (mCompletedLoadingThreadTasks < num)
					{
						mCompletedLoadingThreadTasks += 68;
					}
					if (mTitleScreen != null && mTitleScreen.mQuickLoadKey != KeyCode.KEYCODE_UNKNOWN)
					{
						mCompletedLoadingThreadTasks = num;
						return;
					}
					if (mShutdown || mCloseRequest)
					{
						return;
					}
				}
			}
		}
		if (mCompletedLoadingThreadTasks != num)
		{
			mCompletedLoadingThreadTasks = num;
		}
	}

	public void PreloadLoadingThreadReanimations()
	{
	}

	public void PreloadReanimation(ReanimationType theReanimType)
	{
	}

	public int GetNumPreloadingTasks()
	{
		int num = 10;
		if (mPlayerInfo != null)
		{
			for (int i = 0; i < 53; i++)
			{
				SeedType theSeedType = (SeedType)i;
				if (HasSeedType(theSeedType) || HasFinishedAdventure())
				{
					num++;
				}
			}
			for (int j = 0; j < 33; j++)
			{
				ZombieType zombieType = (ZombieType)j;
				if (!HasFinishedAdventure())
				{
					ZombieDefinition zombieDefinition = Zombie.GetZombieDefinition(zombieType);
					if (mPlayerInfo.mLevel < zombieDefinition.mStartingLevel)
					{
						continue;
					}
				}
				if (zombieType != ZombieType.ZOMBIE_BOSS && zombieType != ZombieType.ZOMBIE_CATAPULT && zombieType != ZombieType.ZOMBIE_GARGANTUAR && zombieType != ZombieType.ZOMBIE_DIGGER && zombieType != ZombieType.ZOMBIE_ZAMBONI)
				{
					num++;
				}
			}
		}
		return num * 68;
	}

	public void LawnMessageBox(int theDialogId, string theHeaderName, string theLinesName, string theButton1Name, string theButton2Name, int theButtonMode, LawnMessageBoxListener theListener)
	{
		mOldFocus = mWidgetManager.mFocusWidget;
		mLawnMessageBoxListener = theListener;
		LawnDialog lawnDialog = DoDialog(theDialogId, isModal: true, theHeaderName, theLinesName, theButton1Name, theButtonMode);
		if (lawnDialog.mLawnYesButton != null)
		{
			lawnDialog.mLawnYesButton.mLabel = TodStringFile.TodStringTranslate(theButton1Name);
		}
		if (lawnDialog.mLawnNoButton != null)
		{
			lawnDialog.mLawnNoButton.mLabel = TodStringFile.TodStringTranslate(theButton2Name);
		}
		lawnDialog.CalcSize(0, 0, (int)Constants.InvertAndScale(400f));
		mWidgetManager.SetFocus(lawnDialog);
	}

	public virtual void EnforceCursor()
	{
	}

	public void ShowCreditScreen()
	{
		mCreditScreen = new CreditScreen(this);
		mCreditScreen.Resize(0, 0, mWidth, mHeight);
		mWidgetManager.AddWidget(mCreditScreen);
		mWidgetManager.BringToBack(mCreditScreen);
		mWidgetManager.SetFocus(mCreditScreen);
	}

	public void KillCreditScreen()
	{
		if (mCreditScreen != null)
		{
			mWidgetManager.RemoveWidget(mCreditScreen);
			SafeDeleteWidget(mCreditScreen);
			mCreditScreen = null;
		}
	}

	public string Pluralize(int theCount, string theSingular, string thePlural)
	{
		if (theCount == 1)
		{
			return TodCommon.TodReplaceNumberString(theSingular, "{COUNT}", theCount);
		}
		return TodCommon.TodReplaceNumberString(thePlural, "{COUNT}", theCount);
	}

	public int GetNumTrophies(ChallengePage thePage)
	{
		int num = 0;
		for (int i = 1; i < 69; i++)
		{
			ChallengeDefinition challengeDefinition = ChallengeScreen.gChallengeDefs[i - 1];
			if (thePage == challengeDefinition.mPage && HasBeatenChallenge(challengeDefinition.mChallengeMode))
			{
				num++;
			}
		}
		return num;
	}

	public bool EarnedGoldTrophy()
	{
		if (!HasFinishedAdventure())
		{
			return false;
		}
		if (TrophiesNeedForGoldSunflower() > 0)
		{
			return false;
		}
		return true;
	}

	public bool IsRegistered()
	{
		return false;
	}

	public bool IsTrialStageLocked()
	{
		return false;
	}

	public bool IsDRMConnected()
	{
		return false;
	}

	public bool IsScaryPotterLevel()
	{
		if (mGameMode >= GameMode.GAMEMODE_SCARY_POTTER_1 && mGameMode <= GameMode.GAMEMODE_SCARY_POTTER_ENDLESS)
		{
			return true;
		}
		if ((IsAdventureMode() && mPlayerInfo.mLevel == 35) || mGameMode == GameMode.GAMEMODE_QUICKPLAY_35)
		{
			return true;
		}
		return false;
	}

	public bool IsEndlessScaryPotter(GameMode theGameMode)
	{
		if (theGameMode == GameMode.GAMEMODE_SCARY_POTTER_ENDLESS)
		{
			return true;
		}
		return false;
	}

	public bool IsSquirrelLevel()
	{
		if (mBoard == null)
		{
			return false;
		}
		if (mGameMode == GameMode.GAMEMODE_CHALLENGE_SQUIRREL)
		{
			return true;
		}
		return false;
	}

	public bool IsIZombieLevel()
	{
		if (mBoard == null)
		{
			return false;
		}
		if (mGameMode == GameMode.GAMEMODE_PUZZLE_I_ZOMBIE_1 || mGameMode == GameMode.GAMEMODE_PUZZLE_I_ZOMBIE_2 || mGameMode == GameMode.GAMEMODE_PUZZLE_I_ZOMBIE_3 || mGameMode == GameMode.GAMEMODE_PUZZLE_I_ZOMBIE_4 || mGameMode == GameMode.GAMEMODE_PUZZLE_I_ZOMBIE_5 || mGameMode == GameMode.GAMEMODE_PUZZLE_I_ZOMBIE_6 || mGameMode == GameMode.GAMEMODE_PUZZLE_I_ZOMBIE_7 || mGameMode == GameMode.GAMEMODE_PUZZLE_I_ZOMBIE_8 || mGameMode == GameMode.GAMEMODE_PUZZLE_I_ZOMBIE_9 || mGameMode == GameMode.GAMEMODE_PUZZLE_I_ZOMBIE_ENDLESS)
		{
			return true;
		}
		return false;
	}

	public bool CanShowZenGarden()
	{
		if (mPlayerInfo == null)
		{
			return false;
		}
		if (IsTrialStageLocked())
		{
			return false;
		}
		if (HasFinishedAdventure())
		{
			return true;
		}
		if (mPlayerInfo.mLevel < 45)
		{
			return false;
		}
		return true;
	}

	public static string GetMoneyString(int theAmount)
	{
		int theValue = theAmount * 10;
		if (!moneyStrings.TryGetValue(theAmount, out var value))
		{
			value = TodCommon.TodReplaceString(TodStringFile.TodStringTranslate("[CURRENCY_STRING]"), "{CURRENCY_SYMBOL}", TodStringFile.TodStringTranslate("[CURRENCY_SYMBOL]"));
			value = TodCommon.TodReplaceString(value, "{AMOUNT}", Common.CommaSeperate(theValue));
			moneyStrings.Add(theAmount, value);
		}
		return value;
	}

	public static string ToString(int i)
	{
		if (!cachedIntToString.TryGetValue(i, out var value))
		{
			value = i.ToString();
			cachedIntToString.Add(i, value);
		}
		return value;
	}

	public bool AdvanceCrazyDaveText()
	{
		int num = mCrazyDaveMessageIndex + 1;
		string theString = Common.StrFormat_("[CRAZY_DAVE_{0}]", num);
		if (!TodStringFile.TodStringListExists(theString))
		{
			return false;
		}
		CrazyDaveTalkIndex(num);
		return true;
	}

	public bool IsWhackAZombieLevel()
	{
		if (mBoard == null)
		{
			return false;
		}
		if (mGameMode == GameMode.GAMEMODE_CHALLENGE_WHACK_A_ZOMBIE)
		{
			return true;
		}
		if ((IsAdventureMode() && mPlayerInfo.mLevel == 15) || mGameMode == GameMode.GAMEMODE_QUICKPLAY_15)
		{
			return true;
		}
		return false;
	}

	public void UpdatePlayTimeStats()
	{
	}

	public bool CanPauseNow()
	{
		if (mBoard == null)
		{
			return false;
		}
		if (mSeedChooserScreen != null && mSeedChooserScreen.mMouseVisible)
		{
			return false;
		}
		if (mBoard.mBoardFadeOutCounter >= 0)
		{
			return false;
		}
		if (mCrazyDaveState != CrazyDaveState.CRAZY_DAVE_OFF)
		{
			return false;
		}
		if (mGameMode == GameMode.GAMEMODE_CHALLENGE_ZEN_GARDEN || mGameMode == GameMode.GAMEMODE_TREE_OF_WISDOM)
		{
			return false;
		}
		if (GetDialogCount() > 0)
		{
			return false;
		}
		return true;
	}

	public bool IsPuzzleMode()
	{
		if (mGameMode >= GameMode.GAMEMODE_SCARY_POTTER_1 && mGameMode <= GameMode.GAMEMODE_SCARY_POTTER_ENDLESS)
		{
			return true;
		}
		if (mGameMode >= GameMode.GAMEMODE_PUZZLE_I_ZOMBIE_1 && mGameMode <= GameMode.GAMEMODE_PUZZLE_I_ZOMBIE_ENDLESS)
		{
			return true;
		}
		return false;
	}

	public bool IsChallengeMode()
	{
		if (!IsAdventureMode() && !IsQuickPlayMode() && !IsPuzzleMode() && !IsSurvivalMode())
		{
			return true;
		}
		return false;
	}

	public bool IsEndlessIZombie(GameMode theGameMode)
	{
		if (theGameMode == GameMode.GAMEMODE_PUZZLE_I_ZOMBIE_ENDLESS)
		{
			return true;
		}
		return false;
	}

	public void CrazyDaveDoneHanding()
	{
		Reanimation reanimation = ReanimationGet(mCrazyDaveReanimID);
		ReanimatorTrackInstance trackInstanceByName = reanimation.GetTrackInstanceByName(GlobalMembersReanimIds.ReanimTrackId_dave_handinghand);
		GlobalMembersAttachment.AttachmentDie(ref trackInstanceByName.mAttachmentID);
	}

	public int TrophiesNeedForGoldSunflower()
	{
		return 0;
	}

	public int GetCurrentChallengeIndex()
	{
		return (int)(mGameMode - 1);
	}

	public void LoadGroup(string theGroupName, int theGroupAveMsToLoad)
	{
		PerfTimer perfTimer = default(PerfTimer);
		perfTimer.Start();
		mResourceManager.StartLoadResources(theGroupName);
		while (!mShutdown && !mCloseRequest && !mLoadingFailed && TodCommon.TodLoadNextResource())
		{
			mCompletedLoadingThreadTasks += theGroupAveMsToLoad;
		}
		if (!mShutdown && !mCloseRequest)
		{
			if (mResourceManager.HadError())
			{
				ShowResourceError();
				mLoadingFailed = true;
			}
			else if (!Resources.ExtractResourcesByName(mResourceManager, theGroupName))
			{
				ShowResourceError();
				mLoadingFailed = true;
			}
			else
			{
				int theTotalGroupWeigth = mResourceManager.GetNumResources(theGroupName) * theGroupAveMsToLoad;
				int theGroupTime = Math.Max((int)perfTimer.GetDuration(), 0);
				TraceLoadGroup(theGroupName, theGroupTime, theTotalGroupWeigth, theGroupAveMsToLoad);
			}
		}
	}

	public void TraceLoadGroup(string theGroupName, int theGroupTime, int theTotalGroupWeigth, int theTaskWeight)
	{
	}

	public void DelayLoadBackgroundResource(string theGroupName)
	{
		if (mLastBackgroundResGroupLoaded != theGroupName)
		{
			if (!string.IsNullOrEmpty(mLastBackgroundResGroupLoaded))
			{
				mResourceManager.UnloadBackground(mLastBackgroundResGroupLoaded);
			}
			if (!string.IsNullOrEmpty(theGroupName))
			{
				TodCommon.TodLoadResources(theGroupName);
			}
			mLastBackgroundResGroupLoaded = theGroupName;
		}
	}

	private static void LoadResourceInThread(string resourceGroup)
	{
		if (Main.LOW_MEMORY_DEVICE)
		{
			Thread thread = new Thread(Load);
			LoadingScreen.IsLoading = true;
			thread.IsBackground = true;
			thread.Start(resourceGroup);
		}
		else
		{
			Load(resourceGroup);
		}
	}

	private static void Load(object resourceGroup)
	{
		string theGroup = resourceGroup as string;
		TodCommon.TodLoadResources(theGroup);
	}

	public void DelayLoadLeaderboardResource(bool doLoad)
	{
		if (Main.LOW_MEMORY_DEVICE || !leaderboardLoaded)
		{
			if (doLoad)
			{
				DelayLoadMainMenuResource(doLoad: false);
				DelayLoadGamePlayResources(doLoad: false);
			}
			if (doLoad && !leaderboardLoaded)
			{
				LoadResourceInThread("DelayLoad_Leaderboard");
			}
			else if (leaderboardLoaded && !doLoad)
			{
				mResourceManager.UnloadBackground("DelayLoad_Leaderboard");
			}
			DelayLoadPileResource(doLoad);
			if (!doLoad)
			{
				DelayLoadMainMenuResource(doLoad: true);
			}
			leaderboardLoaded = doLoad;
		}
	}

	public void DelayLoadGamePlayResources(bool doLoad)
	{
		if (!Main.LOW_MEMORY_DEVICE && gamePlayLoaded)
		{
			return;
		}
		if (doLoad && !gamePlayLoaded)
		{
			GC.Collect();
			TodCommon.TodLoadResources("DelayLoad_GamePlay");
			AtlasResources.mAtlasResources.UnpackPlantsZombiesAtlasImages();
			AtlasResources.mAtlasResources.UnpackParticlesAtlasImages();
			if (Main.LOW_MEMORY_DEVICE)
			{
				ResourceManager.mReanimContentManager.Unload();
				ReanimatorXnaHelpers.ReanimatorLoadDefinitions(ref GameConstants.gLawnReanimationArray, 119);
				ResourceManager.mParticleContentManager.Unload();
				TodParticleGlobal.TodParticleLoadDefinitions(ref GameConstants.gLawnParticleArray, 102);
			}
		}
		else if (gamePlayLoaded && !doLoad)
		{
			mResourceManager.UnloadBackground("DelayLoad_GamePlay");
		}
		gamePlayLoaded = doLoad;
	}

	private void DelayLoadCachedResources(bool doLoad)
	{
		if (Main.LOW_MEMORY_DEVICE || !cachedLoaded)
		{
			if (doLoad && !cachedLoaded)
			{
				GC.Collect();
				TodCommon.TodLoadResources("DelayLoad_Cached");
				AtlasResources.mAtlasResources.UnpackCachedAtlasImages();
			}
			else if (cachedLoaded && !doLoad)
			{
				mResourceManager.UnloadBackground("DelayLoad_Cached");
			}
			cachedLoaded = doLoad;
		}
	}

	public void DelayLoadZenGardenResources(bool doLoad)
	{
		if (!Main.LOW_MEMORY_DEVICE && zenGardenLoaded)
		{
			return;
		}
		if (doLoad && !zenGardenLoaded)
		{
			GC.Collect();
			TodCommon.TodLoadResources("DelayLoad_ZenGarden");
			AtlasResources.mAtlasResources.UnpackZengardenAtlasImages();
			if (Main.LOW_MEMORY_DEVICE)
			{
				ResourceManager.mReanimContentManager.Unload();
				ReanimatorXnaHelpers.ReanimatorLoadDefinitions(ref GameConstants.gLawnReanimationArray, 119);
				ResourceManager.mParticleContentManager.Unload();
				TodParticleGlobal.TodParticleLoadDefinitions(ref GameConstants.gLawnParticleArray, 102);
			}
		}
		else if (zenGardenLoaded && !doLoad)
		{
			mResourceManager.UnloadBackground("DelayLoad_ZenGarden");
		}
		zenGardenLoaded = doLoad;
	}

	public void DelayLoadMainMenuResource(bool doLoad)
	{
		if (Main.LOW_MEMORY_DEVICE || !mainMenuLoaded)
		{
			if (doLoad && !mainMenuLoaded)
			{
				TodCommon.TodLoadResources("DelayLoad_MainMenu");
				AtlasResources.mAtlasResources.UnpackGoodiesAtlasImages();
				AtlasResources.mAtlasResources.UnpackQuickplayAtlasImages();
				AtlasResources.mAtlasResources.UnpackMiniGamesAtlasImages();
			}
			else if (mainMenuLoaded && !doLoad)
			{
				mResourceManager.UnloadBackground("DelayLoad_MainMenu");
			}
			mainMenuLoaded = doLoad;
		}
	}

	public void DelayLoadPileResource(bool doLoad)
	{
		if (Main.LOW_MEMORY_DEVICE || !pileLoaded)
		{
			if (doLoad && !pileLoaded)
			{
				TodCommon.TodLoadResources("DelayLoad_Pile");
				AtlasResources.mAtlasResources.UnpackPileAtlasImages();
			}
			else if (pileLoaded && !doLoad)
			{
				mResourceManager.UnloadBackground("DelayLoad_Pile");
			}
			pileLoaded = doLoad;
		}
	}

	public void DelayLoadZombieNoteResource(string theGroupName)
	{
		if (mLastZombieNoteResGroupLoaded != theGroupName)
		{
			if (!string.IsNullOrEmpty(mLastZombieNoteResGroupLoaded))
			{
				mResourceManager.UnloadBackground(mLastZombieNoteResGroupLoaded);
			}
			TodCommon.TodLoadResources(theGroupName);
			mLastZombieNoteResGroupLoaded = theGroupName;
			Resources.ExtractResourcesByName(mResourceManager, theGroupName);
		}
	}

	public void DelayLoadZenGardenBackground(string theGroupName)
	{
		if (mLastZenGardenResourceLoaded != theGroupName)
		{
			if (!string.IsNullOrEmpty(mLastZenGardenResourceLoaded))
			{
				mResourceManager.UnloadBackground(mLastZenGardenResourceLoaded);
			}
			TodCommon.TodLoadResources(theGroupName);
			mLastZenGardenResourceLoaded = theGroupName;
			Resources.ExtractResourcesByName(mResourceManager, theGroupName);
		}
	}

	public void DelayLoadZombieNotePaperResource(string theGroupName)
	{
		if (mLastPaperGroupLoaded != theGroupName)
		{
			if (!string.IsNullOrEmpty(mLastPaperGroupLoaded))
			{
				mResourceManager.UnloadBackground(mLastPaperGroupLoaded);
			}
			TodCommon.TodLoadResources(theGroupName);
			mLastPaperGroupLoaded = theGroupName;
			Resources.ExtractResourcesByName(mResourceManager, theGroupName);
		}
	}

	public void DelayLoadUpsellResource(string theGroupName)
	{
		if (mLastStoreResGroupLoaded != theGroupName)
		{
			if (!string.IsNullOrEmpty(mLastUpsellResGroupLoaded))
			{
				mResourceManager.UnloadBackground(mLastUpsellResGroupLoaded);
			}
			TodCommon.TodLoadResources(theGroupName);
			mLastUpsellResGroupLoaded = theGroupName;
			Resources.ExtractResourcesByName(mResourceManager, theGroupName);
		}
	}

	public void DelayLoadStoreResource(string theGroupName)
	{
		if (mLastStoreResGroupLoaded != theGroupName)
		{
			if (!string.IsNullOrEmpty(mLastStoreResGroupLoaded))
			{
				mResourceManager.UnloadBackground(mLastStoreResGroupLoaded);
			}
			TodCommon.TodLoadResources(theGroupName);
			mLastStoreResGroupLoaded = theGroupName;
		}
	}

	public void CrazyDaveStopSound()
	{
		mSoundSystem.StopFoley(FoleyType.FOLEY_CRAZYDAVESHORT);
		mSoundSystem.StopFoley(FoleyType.FOLEY_CRAZYDAVELONG);
		mSoundSystem.StopFoley(FoleyType.FOLEY_CRAZYDAVEEXTRALONG);
		mSoundSystem.StopFoley(FoleyType.FOLEY_CRAZYDAVECRAZY);
	}

	public bool UpdatePlayerProfileForFinishingLevel()
	{
		bool flag = false;
		if (IsAdventureMode())
		{
			int mLevel = mBoard.mLevel;
			if (mLevel == 50)
			{
				if (mPlayerInfo.mIZombieUnlocked == 3 && HasBeatenChallenge(GameMode.GAMEMODE_PUZZLE_I_ZOMBIE_3))
				{
					mPlayerInfo.mIZombieUnlocked++;
				}
				if (mPlayerInfo.mVasebreakerUnlocked == 3 && HasBeatenChallenge(GameMode.GAMEMODE_SCARY_POTTER_3))
				{
					mPlayerInfo.mVasebreakerUnlocked++;
				}
				mPlayerInfo.SetLevel(1);
				mPlayerInfo.mFinishedAdventure++;
				if (mPlayerInfo.mFinishedAdventure == 1)
				{
					mPlayerInfo.mNeedsMessageOnGameSelector = true;
					mPlayerInfo.mMiniGamesUnlockable = 19;
					int num = 0;
					if (HasBeatenChallenge(GameMode.GAMEMODE_CHALLENGE_WAR_AND_PEAS))
					{
						num++;
					}
					if (HasBeatenChallenge(GameMode.GAMEMODE_CHALLENGE_WALLNUT_BOWLING))
					{
						num++;
					}
					if (HasBeatenChallenge(GameMode.GAMEMODE_CHALLENGE_SLOT_MACHINE))
					{
						num++;
					}
					mPlayerInfo.mMiniGamesUnlocked += num;
				}
			}
			else
			{
				mPlayerInfo.SetLevel(mLevel + 1);
			}
			if (!HasFinishedAdventure() && mLevel == 34)
			{
				mPlayerInfo.mNeedsMagicTacoReward = true;
			}
			if (!HasFinishedAdventure() && mLevel == 44)
			{
				mPlayerInfo.mNeedsMagicBaconReward = true;
			}
			if ((mLevel >= 22 || mPlayerInfo.mFinishedAdventure > 0) && !mPlayerInfo.mHasUnlockedMinigames)
			{
				mPlayerInfo.UnlockFirstMiniGames();
			}
			if ((mLevel >= 36 || mPlayerInfo.mFinishedAdventure > 0) && !mPlayerInfo.mHasUnlockedPuzzleMode)
			{
				mPlayerInfo.UnlockPuzzleMode();
			}
		}
		else if (!IsQuickPlayMode())
		{
			if (IsSurvivalMode())
			{
				if (mBoard.IsFinalSurvivalStage())
				{
					flag = !HasBeatenChallenge(mGameMode);
					mBoard.SurvivalSaveScore();
					if (flag && HasFinishedAdventure())
					{
						int numTrophies = GetNumTrophies(ChallengePage.CHALLENGE_PAGE_SURVIVAL);
						if (numTrophies != 8 && numTrophies != 9)
						{
							mPlayerInfo.mHasNewSurvival = true;
						}
					}
				}
			}
			else if (IsPuzzleMode())
			{
				flag = !HasBeatenChallenge(mGameMode);
				int currentChallengeIndex = GetCurrentChallengeIndex();
				mPlayerInfo.mChallengeRecords[currentChallengeIndex]++;
				if (!HasFinishedAdventure() && (mGameMode == GameMode.GAMEMODE_SCARY_POTTER_3 || mGameMode == GameMode.GAMEMODE_PUZZLE_I_ZOMBIE_3))
				{
					flag = false;
				}
				if (flag)
				{
					if (IsScaryPotterLevel())
					{
						mPlayerInfo.mHasNewVasebreaker = true;
						mPlayerInfo.mVasebreakerUnlocked++;
						if (mPlayerInfo.mFinishedAdventure == 0 && mPlayerInfo.mVasebreakerUnlocked > 3)
						{
							mPlayerInfo.mVasebreakerUnlocked = 3;
						}
						if (mPlayerInfo.mVasebreakerUnlocked > 10)
						{
							mPlayerInfo.mVasebreakerUnlocked = 10;
						}
					}
					else
					{
						mPlayerInfo.mHasNewIZombie = true;
						mPlayerInfo.mIZombieUnlocked++;
						if (mPlayerInfo.mFinishedAdventure == 0 && mPlayerInfo.mIZombieUnlocked > 3)
						{
							mPlayerInfo.mIZombieUnlocked = 3;
						}
						if (mPlayerInfo.mIZombieUnlocked > 10)
						{
							mPlayerInfo.mIZombieUnlocked = 10;
						}
					}
				}
			}
			else
			{
				flag = !HasBeatenChallenge(mGameMode);
				int currentChallengeIndex2 = GetCurrentChallengeIndex();
				mPlayerInfo.mChallengeRecords[currentChallengeIndex2]++;
				if (mPlayerInfo.mMiniGamesUnlocked < mPlayerInfo.mMiniGamesUnlockable)
				{
					mPlayerInfo.mMiniGamesUnlocked++;
				}
				if (flag && HasFinishedAdventure())
				{
					int numTrophies2 = GetNumTrophies(ChallengePage.CHALLENGE_PAGE_CHALLENGE);
					if (numTrophies2 <= 17)
					{
						mPlayerInfo.mHasNewMiniGame = true;
					}
				}
			}
		}
		int numTrophies3 = GetNumTrophies(ChallengePage.CHALLENGE_PAGE_CHALLENGE);
		if (numTrophies3 == 19)
		{
			ReportAchievement.GiveAchievement(AchievementId.BeyondTheGrave);
		}
		WriteCurrentUserConfig();
		return flag;
	}

	public bool CanDoDaisyMode()
	{
		return false;
	}

	public bool CanDoPinataMode()
	{
		return false;
	}

	public bool CanDoDanceMode()
	{
		return false;
	}

	public override void SetSfxVolume(double theVolume)
	{
		base.SetSfxVolume(theVolume);
		mPlayerInfo.mSoundVolume = theVolume;
	}

	public override void SetMusicVolume(double theVolume)
	{
		base.SetMusicVolume(theVolume);
		if (mPlayerInfo != null)
		{
			mPlayerInfo.mMusicVolume = theVolume;
		}
	}

	public bool SaveFileExists()
	{
		string savedGameName = LawnCommon.GetSavedGameName(GameMode.GAMEMODE_ADVENTURE, (int)mPlayerInfo.mId);
		if (FileExists(savedGameName))
		{
			return true;
		}
		return false;
	}

	public void Vibrate()
	{
		if (mPlayerInfo != null && mPlayerInfo.mDoVibration)
		{
			DoVibration();
		}
	}

	public override void MoviePlayerContentPreloadDidFinish(bool succeeded)
	{
		if (mCreditScreen != null)
		{
			mCreditScreen.VideoLoaded(succeeded);
		}
	}

	public override void MoviePlayerPlaybackDidFinish()
	{
		if (mCreditScreen != null)
		{
			mCreditScreen.VideoFinished();
		}
	}

	public int GetAchievementIcon(AchievementId theAchievement)
	{
		return GameConstants.AchievementInfo[(int)theAchievement].mImageId;
	}

	public string GetAchievementName(AchievementId theAchievement)
	{
		return TodStringFile.TodStringTranslate(GameConstants.AchievementInfo[(int)theAchievement].mName);
	}

	public string GetAchievementDescription(AchievementId theAchievement)
	{
		return TodStringFile.TodStringTranslate(GameConstants.AchievementInfo[(int)theAchievement].mDesc);
	}

	public void EnsureOfflineAchievements()
	{
		try
		{
			for (int i = 0; i < 18; i++)
			{
				AchievementId id = (AchievementId)i;
				if (Sexy.Achievements.GetAchievementItem(id) != null)
				{
					continue;
				}
				bool earned = mPlayerInfo != null && i < mPlayerInfo.mEarnedAchievements.Length && mPlayerInfo.mEarnedAchievements[i];
				string name;
				string desc;
				try
				{
					name = GetAchievementName(id);
					desc = GetAchievementDescription(id);
				}
				catch
				{
					name = Sexy.Achievements.GetAchievementKey(id);
					desc = string.Empty;
				}
				Sexy.Achievements.AddAchievement(new Sexy.AchievementItem(id, Sexy.Achievements.GetAchievementKey(id), name, desc, 0, earned, AtlasResources.IMAGE_TROPHY_HI_RES));
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine("EnsureOfflineAchievements: " + ex.Message);
		}
	}

	public override bool ShouldAutorotateToInterfaceOrientation(UI_ORIENTATION theOrientation)
	{
		if (base.ShouldAutorotateToInterfaceOrientation(theOrientation))
		{
			if (theOrientation != UI_ORIENTATION.UI_ORIENTATION_LANDSCAPE_LEFT)
			{
				return theOrientation == UI_ORIENTATION.UI_ORIENTATION_LANDSCAPE_RIGHT;
			}
			return true;
		}
		return false;
	}
}
