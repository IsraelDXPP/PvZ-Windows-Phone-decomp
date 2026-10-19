using System;
using Sexy;
using Sexy.TodLib;

namespace Lawn;

internal class SeedChooserScreen : Widget, StoreListener, LawnMessageBoxListener, SliderListener
{
	private const int SeedChooserScreen_Start = 100;

	private const int SeedChooserScreen_Random = 101;

	private const int SeedChooserScreen_ViewLawn = 102;

	private const int SeedChooserScreen_Almanac = 103;

	private const int SeedChooserScreen_Menu = 104;

	private const int SeedChooserScreen_Store = 105;

	private const int SeedChooserScreen_Imitater = 106;

	private const int cSeedPacketYOffset = 2;

	private const int cSeedPacketRows = 8;

	private const float mBaseScrollSpeed = 1f;

	private const float mScrollAccel = 0.1f;

	public GameButton mStartButton;

	public GameButton mRandomButton;

	public GameButton mViewLawnButton;

	public GameButton mStoreButton;

	public GameButton mAlmanacButton;

	public GameButton mMenuButton;

	public ChosenSeed[] mChosenSeeds = new ChosenSeed[(int)SeedType.NUM_SEED_TYPES];

	public LawnApp mApp;

	public Board mBoard;

	public int mSeedChooserAge;

	public int mSeedsInFlight;

	public int mSeedsInBank;

	public ToolTipWidget mToolTip;

	public SeedType mToolTipSeed;

	public int mLastMouseX;

	public int mLastMouseY;

	public SeedChooserState mChooseState;

	public int mViewLawnTime;

	public float mScrollPosition;

	public float mScrollAmount;

	public float mMaxScrollPosition;

	public Slider mSlider;

	public SeedType mPreviousType;

	public bool mDoStartButton;

	public bool[] mPickWarningsWaved = new bool[20];

	public int mPendingWarningId;

	private static TRect SeedClipRect()
	{
		return new TRect(0, 123, Constants.BOARD_WIDTH, 420 + Constants.SEED_CHOOSER_EXTRA_HEIGHT);
	}

	public SeedChooserScreen()
	{
		mApp = (LawnApp)GlobalStaticVars.gSexyAppBase;
		mBoard = mApp.mBoard;
		mClip = false;
		mSeedsInFlight = 0;
		mSeedsInBank = 0;
		mLastMouseX = -1;
		mLastMouseY = -1;
		mChooseState = SeedChooserState.CHOOSE_NORMAL;
		mViewLawnTime = 0;
		mDoStartButton = false;
		mToolTip = new ToolTipWidget();
		mToolTipSeed = SeedType.SEED_NONE;
		mScrollAmount = 0f;
		mScrollPosition = 0f;
		mStartButton = new GameButton(SeedChooserScreen_Start, this);
		mStartButton.SetLabel("[LETS_ROCK_BUTTON]");
		mStartButton.mButtonImage = Resources.IMAGE_SEEDCHOOSER_BUTTON;
		mStartButton.mOverImage = null;
		mStartButton.mDownImage = null;
		mStartButton.mDisabledImage = Resources.IMAGE_SEEDCHOOSER_BUTTON_DISABLED;
		mStartButton.mOverOverlayImage = Resources.IMAGE_SEEDCHOOSER_BUTTON_GLOW;
		mStartButton.SetFont(Resources.FONT_DWARVENTODCRAFT18);
		mStartButton.mColors[(int)GameButton.ButtonColours.COLOR_LABEL] = SexyColor.White;
		mStartButton.mColors[(int)GameButton.ButtonColours.COLOR_LABEL_HILITE] = SexyColor.White;
		mStartButton.Resize(154, 545 + Constants.SEED_CHOOSER_EXTRA_HEIGHT, 156, 42);
		mStartButton.mTextOffsetY = -1;
		EnableStartButton(theEnabled: false);
		mStartButton.mParentWidget = this;
		int aButtonOffsetX = Constants.Board_Additional_Width * 2;
		mMenuButton = new GameButton(SeedChooserScreen_Menu, this);
		mMenuButton.SetLabel("[MENU_BUTTON]");
		mMenuButton.mDrawStoneButton = true;
		mMenuButton.Resize(681 + aButtonOffsetX, -10, 117, 46);
		mMenuButton.mParentWidget = this;
		mRandomButton = new GameButton(SeedChooserScreen_Random, this);
		mRandomButton.SetLabel("[DEBUG_PLAY_BUTTON]");
		mRandomButton.mButtonImage = Resources.IMAGE_BLANK;
		mRandomButton.mOverImage = Resources.IMAGE_BLANK;
		mRandomButton.mDownImage = Resources.IMAGE_BLANK;
		mRandomButton.SetFont(Resources.FONT_BRIANNETOD12);
		mRandomButton.mColors[0] = new SexyColor(255, 240, 0);
		mRandomButton.mColors[1] = new SexyColor(200, 200, 255);
		mRandomButton.Resize(332, 555 + Constants.SEED_CHOOSER_EXTRA_HEIGHT, 100, 30);
		mRandomButton.mParentWidget = this;
		SexyColor aBtnColor = new SexyColor(42, 42, 90);
		Image aBtnImage = Resources.IMAGE_SEEDCHOOSER_BUTTON2;
		Image aOverImage = Resources.IMAGE_SEEDCHOOSER_BUTTON2_GLOW;
		int aImageWidth = aBtnImage.GetWidth();
		int aImageHeight = aOverImage.GetHeight();
		mViewLawnButton = new GameButton(SeedChooserScreen_ViewLawn, this);
		mViewLawnButton.SetLabel("[VIEW_LAWN]");
		mViewLawnButton.mButtonImage = aBtnImage;
		mViewLawnButton.mOverImage = aOverImage;
		mViewLawnButton.mDownImage = null;
		mViewLawnButton.SetFont(Resources.FONT_BRIANNETOD12);
		mViewLawnButton.mColors[0] = aBtnColor;
		mViewLawnButton.mColors[1] = aBtnColor;
		mViewLawnButton.Resize(22, 561 + Constants.SEED_CHOOSER_EXTRA_HEIGHT, aImageWidth, aImageHeight);
		mViewLawnButton.mTextOffsetY = 1;
		mViewLawnButton.mParentWidget = this;
		if (!mBoard.mCutScene.IsSurvivalRepick())
		{
			mViewLawnButton.mBtnNoDraw = true;
			mViewLawnButton.mDisabled = true;
		}
		mAlmanacButton = new GameButton(SeedChooserScreen_Almanac, this);
		mAlmanacButton.SetLabel("[ALMANAC_BUTTON]");
		mAlmanacButton.mButtonImage = aBtnImage;
		mAlmanacButton.mOverImage = aOverImage;
		mAlmanacButton.mDownImage = null;
		mAlmanacButton.SetFont(Resources.FONT_BRIANNETOD12);
		mAlmanacButton.mColors[0] = aBtnColor;
		mAlmanacButton.mColors[1] = aBtnColor;
		mAlmanacButton.Resize(560 + aButtonOffsetX, 572 + Constants.SEED_CHOOSER_EXTRA_HEIGHT, aImageWidth, aImageHeight);
		mAlmanacButton.mTextOffsetY = 1;
		mAlmanacButton.mParentWidget = this;
		mStoreButton = new GameButton(SeedChooserScreen_Store, this);
		mStoreButton.SetLabel("[SHOP_BUTTON]");
		mStoreButton.mButtonImage = aBtnImage;
		mStoreButton.mOverImage = aOverImage;
		mStoreButton.mDownImage = null;
		mStoreButton.SetFont(Resources.FONT_BRIANNETOD12);
		mStoreButton.mColors[0] = aBtnColor;
		mStoreButton.mColors[1] = aBtnColor;
		mStoreButton.Resize(680 + aButtonOffsetX, 572 + Constants.SEED_CHOOSER_EXTRA_HEIGHT, aImageWidth, aImageHeight);
		mStoreButton.mTextOffsetY = 1;
		mStoreButton.mParentWidget = this;
		if (!mApp.CanShowAlmanac())
		{
			mAlmanacButton.mBtnNoDraw = true;
			mAlmanacButton.mDisabled = true;
		}
		if (!mApp.CanShowStore())
		{
			mStoreButton.mBtnNoDraw = true;
			mStoreButton.mDisabled = true;
		}
		for (SeedType aSeedType = SeedType.SEED_PEASHOOTER; aSeedType < SeedType.NUM_SEEDS_IN_CHOOSER; aSeedType++)
		{
			ChosenSeed aChosenSeed = new ChosenSeed();
			aChosenSeed.mSeedType = aSeedType;
			int x, y;
			GetSeedPositionInChooser((int)aSeedType, out x, out y);
			aChosenSeed.mX = x;
			aChosenSeed.mY = y;
			aChosenSeed.mTimeStartMotion = 0;
			aChosenSeed.mTimeEndMotion = 0;
			aChosenSeed.mStartX = aChosenSeed.mX;
			aChosenSeed.mStartY = aChosenSeed.mY;
			aChosenSeed.mEndX = aChosenSeed.mX;
			aChosenSeed.mEndY = aChosenSeed.mY;
			aChosenSeed.mSeedState = ChosenSeedState.SEED_IN_CHOOSER;
			aChosenSeed.mSeedIndexInBank = 0;
			aChosenSeed.mRefreshCounter = 0;
			aChosenSeed.mRefreshing = false;
			aChosenSeed.mImitaterType = SeedType.SEED_NONE;
			aChosenSeed.mCrazyDavePicked = false;
			mChosenSeeds[(int)aSeedType] = aChosenSeed;
		}
		if (mBoard.mCutScene.IsSurvivalRepick())
		{
			for (int anIdx = 0; anIdx < mBoard.mSeedBank.mNumPackets; anIdx++)
			{
				SeedPacket aSeedPacket = mBoard.mSeedBank.mSeedPackets[anIdx];
				ChosenSeed aChosenSeed = mChosenSeeds[(int)aSeedPacket.mPacketType];
				aChosenSeed.mRefreshing = aSeedPacket.mRefreshing;
				aChosenSeed.mRefreshCounter = aSeedPacket.mRefreshCounter;
			}
			mBoard.mSeedBank.mNumPackets = 0;
		}
		if (mApp.mGameMode == GameMode.GAMEMODE_CHALLENGE_SEEING_STARS)
		{
			ChosenSeed aStarFruit = mChosenSeeds[(int)SeedType.SEED_STARFRUIT];
			int aX = mBoard.GetSeedPacketPositionX(0);
			aStarFruit.mX = aX;
			aStarFruit.mY = 8;
			aStarFruit.mStartX = aX;
			aStarFruit.mStartY = 8;
			aStarFruit.mEndX = aX;
			aStarFruit.mEndY = 8;
			aStarFruit.mSeedState = ChosenSeedState.SEED_IN_BANK;
			aStarFruit.mSeedIndexInBank = 0;
			mSeedsInBank++;
		}
		if ((mApp.mCrazySeeds && mApp.mPlayingQuickplay) || (mApp.IsAdventureMode() && !mApp.IsFirstTimeAdventureMode() && !mApp.mPlayingQuickplay))
		{
			CrazyDavePickSeeds();
		}
		mSlider = new Slider(Resources.IMAGE_OPTIONS_SLIDERSLOT_PLANT, Resources.IMAGE_OPTIONS_SLIDERKNOB_PLANT, 0, this);
		mSlider.SetValue(Math.Max(0.0, Math.Min(mMaxScrollPosition, mScrollPosition)));
		mSlider.mHorizontal = false;
		mSlider.mThumbOffsetX = -14;
		mSlider.mNoDraw = true;
		ResizeSlider();
		for (int k = 0; k < mPickWarningsWaved.Length; k++)
		{
			mPickWarningsWaved[k] = false;
		}
	}

	public override void Dispose()
	{
		mStartButton.Dispose();
		mRandomButton.Dispose();
		mViewLawnButton.Dispose();
		mAlmanacButton.Dispose();
		mStoreButton.Dispose();
		mSlider.Dispose();
		mToolTip.Dispose();
		mMenuButton.Dispose();
		RemoveAllWidgets(doDelete: true);
	}

	public static int PickFromWeightedArrayUsingSpecialRandSeed(TodWeightedArray[] theArray, int theCount, MTRand theLevelRNG)
	{
		int aTotalWeight = 0;
		for (int i = 0; i < theCount; i++)
		{
			aTotalWeight += theArray[i].mWeight;
		}
		ulong aRndResult = theLevelRNG.Next((ulong)aTotalWeight);
		int aWeight = 0;
		for (int j = 0; j < theCount; j++)
		{
			aWeight += theArray[j].mWeight;
			if (aWeight > (int)aRndResult)
			{
				return (int)theArray[j].mItem;
			}
		}
		return 0;
	}

	public void CrazyDavePickSeeds()
	{
		TodWeightedArray[] aSeedArray = new TodWeightedArray[(int)SeedType.NUM_SEED_TYPES];
		for (int i = 0; i < aSeedArray.Length; i++)
		{
			aSeedArray[i] = TodWeightedArray.GetNewTodWeightedArray();
		}
		for (SeedType aSeedType = SeedType.SEED_PEASHOOTER; aSeedType < SeedType.NUM_SEEDS_IN_CHOOSER; aSeedType++)
		{
			aSeedArray[(int)aSeedType].mItem = (int)aSeedType;
			uint aRecFlags = SeedNotRecommendedToPick(aSeedType);
			if ((aSeedType == SeedType.SEED_GATLINGPEA && mApp.mPlayerInfo.mPurchases[(int)StoreItem.STORE_ITEM_PLANT_GATLINGPEA] == 0) || !mApp.SeedTypeAvailable(aSeedType) || SeedNotAllowedToPick(aSeedType) || Plant.IsUpgrade(aSeedType) || aSeedType == SeedType.SEED_IMITATER || aSeedType == SeedType.SEED_UMBRELLA || aSeedType == SeedType.SEED_BLOVER || aSeedType == SeedType.SEED_GRAVEBUSTER || aSeedType == SeedType.SEED_PLANTERN || Plant.IsNocturnal(aSeedType) || aSeedType == SeedType.SEED_FLOWERPOT || Plant.IsAquatic(aSeedType))
			{
				aSeedArray[(int)aSeedType].mWeight = 0;
			}
			else
			{
				aSeedArray[(int)aSeedType].mWeight = 1;
			}
			if (mBoard.StageIsNight() && Plant.IsNocturnal(aSeedType) && !Plant.IsUpgrade(aSeedType) && !Plant.IsAquatic(aSeedType))
			{
				aSeedArray[(int)aSeedType].mWeight = 1;
			}
			if (mBoard.StageHasPool())
			{
				if (Plant.IsAquatic(aSeedType) && !Plant.IsUpgrade(aSeedType) && !Plant.IsNocturnal(aSeedType))
				{
					aSeedArray[(int)aSeedType].mWeight = 1;
				}
				else if (mBoard.StageIsNight() && Plant.IsAquatic(aSeedType) && Plant.IsNocturnal(aSeedType) && !Plant.IsUpgrade(aSeedType))
				{
					aSeedArray[(int)aSeedType].mWeight = 1;
				}
			}
		}
		if (mBoard.StageIsNight())
		{
			aSeedArray[(int)SeedType.SEED_INSTANT_COFFEE].mWeight = 0;
		}
		if (mBoard.StageHasGraveStones())
		{
			aSeedArray[(int)SeedType.SEED_GRAVEBUSTER].mWeight = 1;
		}
		if (mBoard.StageHasFog())
		{
			aSeedArray[(int)SeedType.SEED_PLANTERN].mWeight = 1;
		}
		if (mBoard.mZombieAllowed[(int)ZombieType.ZOMBIE_BALLOON] || mBoard.StageHasFog())
		{
			aSeedArray[(int)SeedType.SEED_BLOVER].mWeight = 1;
		}
		if (mBoard.mZombieAllowed[(int)ZombieType.ZOMBIE_BUNGEE] || mBoard.mZombieAllowed[(int)ZombieType.ZOMBIE_CATAPULT])
		{
			aSeedArray[(int)SeedType.SEED_UMBRELLA].mWeight = 1;
		}
		if (mBoard.StageHasRoof())
		{
			aSeedArray[(int)SeedType.SEED_TORCHWOOD].mWeight = 0;
			aSeedArray[(int)SeedType.SEED_SPIKEWEED].mWeight = 0;
			aSeedArray[(int)SeedType.SEED_FLOWERPOT].mWeight = 1;
		}
		MTRand aLevelRNG = new MTRand((ulong)mBoard.GetLevelRandSeed());
		for (int i = 0; i < 3; i++)
		{
			SeedType aPickedSeed = (SeedType)PickFromWeightedArrayUsingSpecialRandSeed(aSeedArray, (int)SeedType.NUM_SEEDS_IN_CHOOSER, aLevelRNG);
			aSeedArray[(int)aPickedSeed].mWeight = 0;
			ChosenSeed aChosenSeed = mChosenSeeds[(int)aPickedSeed];
			int aPosX = mBoard.GetSeedPacketPositionX(i);
			aChosenSeed.mX = aPosX;
			aChosenSeed.mY = 8;
			aChosenSeed.mStartX = aPosX;
			aChosenSeed.mStartY = 8;
			aChosenSeed.mEndX = aPosX;
			aChosenSeed.mEndY = 8;
			aChosenSeed.mSeedState = ChosenSeedState.SEED_IN_BANK;
			aChosenSeed.mSeedIndexInBank = i;
			aChosenSeed.mCrazyDavePicked = true;
			mSeedsInBank++;
		}
	}

	public void GetSeedPositionInChooser(int theIndex, out int x, out int y)
	{
		if (theIndex == (int)SeedType.SEED_IMITATER)
		{
			x = Constants.IMITATER_POS_X + 5;
			y = Constants.IMITATER_POS_Y + 12 + Constants.SEED_CHOOSER_EXTRA_HEIGHT;
		}
		else
		{
			x = theIndex % cSeedPacketRows * 53 + 22;
			y = theIndex / cSeedPacketRows * (Constants.SEED_PACKET_HEIGHT + cSeedPacketYOffset) + (Constants.SEED_PACKET_HEIGHT + 53) - (int)mScrollPosition;
		}
	}

	public void GetSeedPositionInBank(int theIndex, out int x, out int y)
	{
		x = mBoard.mSeedBank.mX - mX + mBoard.GetSeedPacketPositionX(theIndex);
		y = mBoard.mSeedBank.mY - mY + 8;
	}

	public override void RemovedFromManager(WidgetManager theWidgetManager)
	{
		base.RemovedFromManager(theWidgetManager);
		theWidgetManager.RemoveWidget(mSlider);
	}

	public override void AddedToManager(WidgetManager theWidgetManager)
	{
		base.AddedToManager(theWidgetManager);
		theWidgetManager.AddWidget(mSlider);
	}

	public uint SeedNotRecommendedToPick(SeedType theSeedType)
	{
		uint theNumber = mBoard.SeedNotRecommendedForLevel(theSeedType);
		if (TodCommon.TestBit(theNumber, 0) && PickedPlantType(SeedType.SEED_INSTANT_COFFEE))
		{
			TodCommon.SetBit(ref theNumber, 0, 0);
		}
		return theNumber;
	}

	public bool SeedNotAllowedToPick(SeedType theSeedType)
	{
		if (mApp.mGameMode == GameMode.GAMEMODE_CHALLENGE_LAST_STAND && (theSeedType == SeedType.SEED_SUNFLOWER || theSeedType == SeedType.SEED_SUNSHROOM || theSeedType == SeedType.SEED_TWINSUNFLOWER || theSeedType == SeedType.SEED_SEASHROOM || theSeedType == SeedType.SEED_PUFFSHROOM))
		{
			return true;
		}
		return false;
	}

	public bool SeedNotAllowedDuringTrial(SeedType theSeedType)
	{
		return mApp.IsTrialStageLocked() && (theSeedType == SeedType.SEED_SQUASH || theSeedType == SeedType.SEED_THREEPEATER);
	}

	public override void Draw(Graphics g)
	{
		if (mApp.GetDialog(Dialogs.DIALOG_STORE) != null || mApp.GetDialog(Dialogs.DIALOG_ALMANAC) != null)
		{
			return;
		}
		g.SetLinearBlend(linear: true);
		if (!mBoard.ChooseSeedsOnCurrentLevel() || (mBoard.mCutScene != null && mBoard.mCutScene.IsBeforePreloading()))
		{
			return;
		}
		g.DrawImage(Resources.IMAGE_SEEDCHOOSER_BACKGROUND, 0, 87);
		if (mApp.SeedTypeAvailable(SeedType.SEED_IMITATER))
		{
			g.DrawImage(Resources.IMAGE_SEEDCHOOSER_IMITATERADDON, Constants.IMITATER_POS_X, Constants.IMITATER_POS_Y + Constants.SEED_CHOOSER_EXTRA_HEIGHT);
		}
		TodCommon.TodDrawString(g, "[CHOOSE_YOUR_PLANTS]", 229, 110, Resources.FONT_DWARVENTODCRAFT18, SexyColor.White, DrawStringJustification.DS_ALIGN_CENTER);
		mSlider.SliderDraw(g);
		for (SeedType aSeedType = SeedType.SEED_PEASHOOTER; aSeedType < SeedType.NUM_SEEDS_IN_CHOOSER; aSeedType++)
		{
			if (aSeedType != SeedType.SEED_IMITATER)
			{
				g.SetClipRect(SeedClipRect());
			}
			int x, y;
			GetSeedPositionInChooser((int)aSeedType, out x, out y);
			if (mApp.SeedTypeAvailable(aSeedType))
			{
				ChosenSeed aChosenSeed = mChosenSeeds[(int)aSeedType];
				if (aChosenSeed.mSeedState != ChosenSeedState.SEED_IN_CHOOSER)
				{
					SeedPacket.DrawSeedPacket(g, x, y, aSeedType, SeedType.SEED_NONE, 0f, 55, theDrawCost: false, theUseCurrentCost: false);
				}
			}
			else if (aSeedType != SeedType.SEED_IMITATER)
			{
				g.DrawImage(Resources.IMAGE_SEEDPACKETSILHOUETTE, x, y);
			}
			ChosenSeed aChosenSeed2 = mChosenSeeds[(int)aSeedType];
			ChosenSeedState aSeedState = aChosenSeed2.mSeedState;
			if (mApp.SeedTypeAvailable(aSeedType) && aSeedState != ChosenSeedState.SEED_FLYING_TO_BANK && aSeedState != ChosenSeedState.SEED_FLYING_TO_CHOOSER && aSeedState != ChosenSeedState.SEED_PACKET_HIDDEN && (aSeedState == ChosenSeedState.SEED_IN_CHOOSER || mBoard.mCutScene.mSeedChoosing))
			{
				bool aGrayed = false;
				if (((SeedNotRecommendedToPick(aSeedType) != 0 || SeedNotAllowedToPick(aSeedType) || IsImitaterUnselectable(aSeedType)) && aSeedState == ChosenSeedState.SEED_IN_CHOOSER) || SeedNotAllowedDuringTrial(aSeedType))
				{
					aGrayed = true;
				}
				int aPosX, aPosY;
				if (aSeedState == ChosenSeedState.SEED_IN_CHOOSER)
				{
					GetSeedPositionInChooser((int)aSeedType, out aPosX, out aPosY);
				}
				else if (aSeedState == ChosenSeedState.SEED_IN_BANK)
				{
					g.ClearClipRect();
					aPosX = aChosenSeed2.mX;
					aPosY = aChosenSeed2.mY;
				}
				else
				{
					aPosX = aChosenSeed2.mX;
					aPosY = aChosenSeed2.mY;
				}
				if (mChooseState != SeedChooserState.CHOOSE_VIEW_LAWN || (mChooseState == SeedChooserState.CHOOSE_VIEW_LAWN && aSeedState == ChosenSeedState.SEED_IN_CHOOSER))
				{
					SeedPacket.DrawSeedPacket(g, aPosX, aPosY, aChosenSeed2.mSeedType, aChosenSeed2.mImitaterType, 0f, aGrayed ? 115 : 255, theDrawCost: false, theUseCurrentCost: false);
				}
			}
			g.ClearClipRect();
		}
		for (SeedType aSeedType = SeedType.SEED_PEASHOOTER; aSeedType < SeedType.NUM_SEEDS_IN_CHOOSER; aSeedType++)
		{
			ChosenSeed aChosenSeed = mChosenSeeds[(int)aSeedType];
			ChosenSeedState aSeedState = aChosenSeed.mSeedState;
			if (mApp.SeedTypeAvailable(aSeedType) && (aSeedState == ChosenSeedState.SEED_FLYING_TO_BANK || aSeedState == ChosenSeedState.SEED_FLYING_TO_CHOOSER))
			{
				SeedPacket.DrawSeedPacket(g, aChosenSeed.mX, aChosenSeed.mY, aChosenSeed.mSeedType, aChosenSeed.mImitaterType, 0f, 255, theDrawCost: false, theUseCurrentCost: false);
			}
		}
		int aNumSeedsInBank = mBoard.mSeedBank.mNumPackets;
		for (int anIndex = 0; anIndex < aNumSeedsInBank; anIndex++)
		{
			if (FindSeedInBank(anIndex) == SeedType.SEED_NONE)
			{
				int x2, y2;
				GetSeedPositionInBank(anIndex, out x2, out y2);
				g.DrawImage(Resources.IMAGE_SEEDPACKETSILHOUETTE, x2, y2);
			}
		}
		mStartButton.Draw(g);
		mRandomButton.Draw(g);
		mViewLawnButton.Draw(g);
		mAlmanacButton.Draw(g);
		mStoreButton.Draw(g);
		Graphics aBoardFrameG = Graphics.GetNew(g);
		aBoardFrameG.mTransX -= mX;
		aBoardFrameG.mTransY -= mY;
		mMenuButton.Draw(aBoardFrameG);
		aBoardFrameG.PrepareForReuse();
		mToolTip.Draw(g);
	}

	public void UpdateViewLawn()
	{
		if (mChooseState != SeedChooserState.CHOOSE_VIEW_LAWN)
		{
			return;
		}
		mViewLawnTime++;
		if (mViewLawnTime == 100)
		{
			mBoard.DisplayAdviceAgain("[CLICK_TO_CONTINUE]", MessageStyle.MESSAGE_STYLE_HINT_STAY, AdviceType.ADVICE_CLICK_TO_CONTINUE);
		}
		else if (mViewLawnTime == 251)
		{
			mViewLawnTime = 250;
		}
		for (int anIndex = 0; anIndex < mBoard.mSeedBank.mNumPackets; anIndex++)
		{
			SeedType aSeedType = FindSeedInBank(anIndex);
			if (aSeedType == SeedType.SEED_NONE)
			{
				break;
			}
			ChosenSeed aChosenSeed = mChosenSeeds[(int)aSeedType];
			SeedPacket aSeedPacket = mBoard.mSeedBank.mSeedPackets[anIndex];
			aSeedPacket.SetPacketType(aSeedType, aChosenSeed.mImitaterType);
		}
		int aSeedChooserY = Constants.SEED_CHOOSER_OFFSET_Y - Resources.IMAGE_SEEDCHOOSER_BACKGROUND.mHeight - 87;
		int aStreetOffset = Constants.BACKGROUND_IMAGE_WIDTH + Constants.Board_Additional_Width - mApp.mWidth;
		if (mViewLawnTime <= 100)
		{
			mBoard.mRoofPoleOffset = TodCommon.TodAnimateCurve(0, 100, mViewLawnTime, Constants.ROOF_POLE_END, Constants.ROOF_POLE_START, TodCurves.CURVE_EASE_IN_OUT);
			mBoard.mRoofTreeOffset = TodCommon.TodAnimateCurve(0, 100, mViewLawnTime, Constants.ROOF_TREE_END, Constants.ROOF_TREE_START, TodCurves.CURVE_EASE_IN_OUT);
			mBoard.Move(-TodCommon.TodAnimateCurve(0, 100, mViewLawnTime, aStreetOffset, 0, TodCurves.CURVE_EASE_IN_OUT), 0);
			Move(0, TodCommon.TodAnimateCurve(0, 40, mViewLawnTime, aSeedChooserY, Constants.SEED_CHOOSER_OFFSET_Y, TodCurves.CURVE_EASE_IN_OUT));
		}
		else if (mViewLawnTime <= 250)
		{
			mBoard.Move(0, 0);
			Move(0, Constants.SEED_CHOOSER_OFFSET_Y);
		}
		else if (mViewLawnTime <= 350)
		{
			mBoard.ClearAdvice(AdviceType.ADVICE_CLICK_TO_CONTINUE);
			mBoard.mRoofPoleOffset = TodCommon.TodAnimateCurve(250, 350, mViewLawnTime, Constants.ROOF_POLE_START, Constants.ROOF_POLE_END, TodCurves.CURVE_EASE_IN_OUT);
			mBoard.mRoofTreeOffset = TodCommon.TodAnimateCurve(250, 350, mViewLawnTime, Constants.ROOF_TREE_START, Constants.ROOF_TREE_END, TodCurves.CURVE_EASE_IN_OUT);
			mBoard.Move(-TodCommon.TodAnimateCurve(250, 350, mViewLawnTime, 0, aStreetOffset, TodCurves.CURVE_EASE_IN_OUT), 0);
			Move(0, TodCommon.TodAnimateCurve(310, 350, mViewLawnTime, Constants.SEED_CHOOSER_OFFSET_Y, aSeedChooserY, TodCurves.CURVE_EASE_IN_OUT));
		}
		else
		{
			mChooseState = SeedChooserState.CHOOSE_NORMAL;
			mViewLawnTime = 0;
			mMenuButton.mDisabled = false;
			for (int anIndex = 0; anIndex < mBoard.mSeedBank.mNumPackets; anIndex++)
			{
				SeedPacket aSeedPacket = mBoard.mSeedBank.mSeedPackets[anIndex];
				aSeedPacket.SetPacketType(SeedType.SEED_NONE, SeedType.SEED_NONE);
			}
		}
	}

	public void LandFlyingSeed(ref ChosenSeed theChosenSeed)
	{
		if (theChosenSeed.mSeedState == ChosenSeedState.SEED_FLYING_TO_BANK)
		{
			theChosenSeed.mX = theChosenSeed.mEndX;
			theChosenSeed.mY = theChosenSeed.mEndY;
			theChosenSeed.mTimeStartMotion = 0;
			theChosenSeed.mTimeEndMotion = 0;
			theChosenSeed.mSeedState = ChosenSeedState.SEED_IN_BANK;
			mSeedsInFlight--;
		}
		else if (theChosenSeed.mSeedState == ChosenSeedState.SEED_FLYING_TO_CHOOSER)
		{
			theChosenSeed.mX = theChosenSeed.mEndX;
			theChosenSeed.mY = theChosenSeed.mEndY;
			theChosenSeed.mTimeStartMotion = 0;
			theChosenSeed.mTimeEndMotion = 0;
			theChosenSeed.mSeedState = ChosenSeedState.SEED_IN_CHOOSER;
			mSeedsInFlight--;
			if (theChosenSeed.mSeedType == SeedType.SEED_IMITATER)
			{
				theChosenSeed.mImitaterType = SeedType.SEED_NONE;
			}
		}
	}

	public void UpdateCursor()
	{
		if (mApp.GetDialogCount() > 0 || mBoard.mCutScene.IsInShovelTutorial() || mApp.mGameMode == GameMode.GAMEMODE_UPSELL || mSlider.mIsOver || mSlider.mDragging)
		{
			return;
		}
		SeedType aMouseSeedType = SeedHitTest(mLastMouseX, mLastMouseY);
		if (aMouseSeedType != SeedType.SEED_NONE)
		{
			if (IsImitaterUnselectable(aMouseSeedType))
			{
				aMouseSeedType = SeedType.SEED_NONE;
			}
			ChosenSeed aMouseChosenSeed = mChosenSeeds[(int)aMouseSeedType];
			if (aMouseChosenSeed.mSeedState == ChosenSeedState.SEED_IN_BANK && aMouseChosenSeed.mCrazyDavePicked)
			{
				aMouseSeedType = SeedType.SEED_NONE;
			}
		}
		if (mMouseVisible && mChooseState != SeedChooserState.CHOOSE_VIEW_LAWN && ((ZombieHitTest(mLastMouseX, mLastMouseY) != null && mApp.CanShowAlmanac() && !IsOverImitater(mLastMouseX, mLastMouseY)) || (aMouseSeedType != SeedType.SEED_NONE && !SeedNotAllowedToPick(aMouseSeedType)) || mRandomButton.IsMouseOver() || mViewLawnButton.IsMouseOver() || mAlmanacButton.IsMouseOver() || mStoreButton.IsMouseOver() || mMenuButton.IsMouseOver() || mStartButton.IsMouseOver()))
		{
			mApp.SetCursor(SexyAppBase.CURSOR_HAND);
		}
		else
		{
			mApp.SetCursor(SexyAppBase.CURSOR_POINTER);
		}
	}

	public override void Update()
	{
		base.Update();
		mRandomButton.mBtnNoDraw = !mApp.mTodCheatKeys;
		mRandomButton.mDisabled = !mApp.mTodCheatKeys;
		mMaxScrollPosition = Math.Max(0, ((int)SeedType.NUM_SEEDS_IN_CHOOSER - 2) / cSeedPacketRows * (Constants.SEED_PACKET_HEIGHT + cSeedPacketYOffset) + Constants.SEED_PACKET_HEIGHT - SeedClipRect().mHeight);
		mSlider.mVisible = mMaxScrollPosition != 0f;
		if (mSlider.mVisible)
		{
			mScrollPosition = TodCommon.ClampFloat(mScrollPosition += mScrollAmount * (mBaseScrollSpeed + Math.Abs(mScrollAmount) * mScrollAccel), 0f, mMaxScrollPosition);
			mScrollAmount *= 1f - mScrollAccel;
			mSlider.SetValue(Math.Max(0.0, Math.Min(mMaxScrollPosition, mScrollPosition)) / mMaxScrollPosition);
		}
		else
		{
			mScrollPosition = 0f;
			mScrollAmount = 0f;
		}
		mLastMouseX = mApp.mWidgetManager.mLastMouseX;
		mLastMouseY = mApp.mWidgetManager.mLastMouseY;
		mSeedChooserAge += 3;
		for (SeedType aSeedType = SeedType.SEED_PEASHOOTER; aSeedType < SeedType.NUM_SEEDS_IN_CHOOSER; aSeedType++)
		{
			if (mApp.SeedTypeAvailable(aSeedType))
			{
				ChosenSeed aChosenSeed = mChosenSeeds[(int)aSeedType];
				if (aChosenSeed.mSeedState == ChosenSeedState.SEED_FLYING_TO_BANK)
				{
					int aTimeStart = aChosenSeed.mTimeStartMotion;
					int aTimeEnd = aChosenSeed.mTimeEndMotion;
					aChosenSeed.mX = TodCommon.TodAnimateCurve(aTimeStart, aTimeEnd, mSeedChooserAge, aChosenSeed.mStartX, aChosenSeed.mEndX, TodCurves.CURVE_EASE_IN_OUT);
					aChosenSeed.mY = TodCommon.TodAnimateCurve(aTimeStart, aTimeEnd, mSeedChooserAge, aChosenSeed.mStartY, aChosenSeed.mEndY, TodCurves.CURVE_EASE_IN_OUT);
					if (mSeedChooserAge >= aChosenSeed.mTimeEndMotion)
					{
						LandFlyingSeed(ref aChosenSeed);
					}
				}
			}
		}
		ShowToolTip();
		mStartButton.Update();
		mRandomButton.Update();
		mViewLawnButton.Update();
		mAlmanacButton.Update();
		mStoreButton.Update();
		mMenuButton.Update();
		UpdateViewLawn();
		UpdateCursor();
		if (mDoStartButton)
		{
			mDoStartButton = false;
			OnStartButton();
		}
		MarkDirty();
	}

	public override void MouseWheel(int theDelta)
	{
		if (mChooseState != SeedChooserState.CHOOSE_NORMAL)
		{
			return;
		}
		mScrollAmount -= mBaseScrollSpeed * theDelta;
		mScrollAmount -= mScrollAmount * mScrollAccel;
	}

	public bool DisplayRepickWarningDialog(int theWarningId, string theMessage)
	{
		if (mPickWarningsWaved[theWarningId])
		{
			return true;
		}
		mPendingWarningId = theWarningId;
		mApp.LawnMessageBox(28, "[DIALOG_WARNING]", theMessage, "[DIALOG_BUTTON_YES]", "[REPICK_BUTTON]", 1, this);
		return false;
	}

	public bool FlyersAreComing()
	{
		for (int i = 0; i < mBoard.mNumWaves; i++)
		{
			for (int j = 0; j < 50; j++)
			{
				switch (mBoard.mZombiesInWave[i, j])
				{
				case ZombieType.ZOMBIE_BALLOON:
					return true;
				default:
					continue;
				case ZombieType.ZOMBIE_INVALID:
					break;
				}
				break;
			}
		}
		return false;
	}

	public bool FlyProtectionCurrentlyPlanted()
	{
		int count = mBoard.mPlants.Count;
		for (int i = 0; i < count; i++)
		{
			Plant plant = mBoard.mPlants[i];
			if (!plant.mDead && (plant.mSeedType == SeedType.SEED_CATTAIL || plant.mSeedType == SeedType.SEED_CACTUS))
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckSeedUpgrade(int theWarningId, SeedType theSeedTypeTo, SeedType theSeedTypeFrom)
	{
		if (mApp.IsSurvivalMode() || mPickWarningsWaved[theWarningId])
		{
			return true;
		}
		if (PickedPlantType(theSeedTypeTo) && !PickedPlantType(theSeedTypeFrom))
		{
			string theText = TodStringFile.TodStringTranslate("[SEED_CHOOSER_UPGRADE_WARNING]");
			string nameString = Plant.GetNameString(theSeedTypeTo, SeedType.SEED_NONE);
			string nameString2 = Plant.GetNameString(theSeedTypeFrom, SeedType.SEED_NONE);
			theText = TodCommon.TodReplaceString(theText, "{UPGRADE_TO}", nameString);
			theText = TodCommon.TodReplaceString(theText, "{UPGRADE_FROM}", nameString2);
			if (!DisplayRepickWarningDialog(theWarningId, theText))
			{
				return false;
			}
		}
		return true;
	}

	public void OnStartButton()
	{
		if ((mApp.mGameMode == GameMode.GAMEMODE_CHALLENGE_SEEING_STARS && !PickedPlantType(SeedType.SEED_STARFRUIT) && !DisplayRepickWarningDialog(0, "[SEED_CHOOSER_SEEING_STARS_WARNING]")) || (mApp.IsFirstTimeAdventureMode() && mBoard.mLevel == 11 && !PickedPlantType(SeedType.SEED_PUFFSHROOM) && !DisplayRepickWarningDialog(1, "[SEED_CHOOSER_PUFFSHROOM_WARNING]")))
		{
			return;
		}
		if (!PickedPlantType(SeedType.SEED_SUNFLOWER) && !PickedPlantType(SeedType.SEED_TWINSUNFLOWER) && !PickedPlantType(SeedType.SEED_SUNSHROOM) && !mBoard.mCutScene.IsSurvivalRepick() && mApp.mGameMode != GameMode.GAMEMODE_CHALLENGE_LAST_STAND)
		{
			if (mApp.IsFirstTimeAdventureMode() && mBoard.mLevel == 11)
			{
				if (!DisplayRepickWarningDialog(2, "[SEED_CHOOSER_NIGHT_SUN_WARNING]"))
				{
					return;
				}
			}
			else if (!DisplayRepickWarningDialog(3, "[SEED_CHOOSER_SUN_WARNING]"))
			{
				return;
			}
		}
		if (mBoard.StageHasPool() && !PickedPlantType(SeedType.SEED_LILYPAD) && !PickedPlantType(SeedType.SEED_SEASHROOM) && !PickedPlantType(SeedType.SEED_TANGLEKELP) && !mBoard.mCutScene.IsSurvivalRepick())
		{
			if (mApp.IsFirstTimeAdventureMode() && mBoard.mLevel == 21)
			{
				if (!DisplayRepickWarningDialog(4, "[SEED_CHOOSER_LILY_WARNING]"))
				{
					return;
				}
			}
			else if (!DisplayRepickWarningDialog(5, "[SEED_CHOOSER_POOL_WARNING]"))
			{
				return;
			}
		}
		if ((!mBoard.StageHasRoof() || PickedPlantType(SeedType.SEED_FLOWERPOT) || !mApp.HasSeedType(SeedType.SEED_FLOWERPOT) || DisplayRepickWarningDialog(6, "[SEED_CHOOSER_ROOF_WARNING]")) && (mApp.mGameMode != GameMode.GAMEMODE_CHALLENGE_ART_CHALLENGE_1 || PickedPlantType(SeedType.SEED_WALLNUT) || DisplayRepickWarningDialog(7, "[SEED_CHOOSER_ART_WALLNUT_WARNING]")) && (mApp.mGameMode != GameMode.GAMEMODE_CHALLENGE_ART_CHALLENGE_2 || (PickedPlantType(SeedType.SEED_STARFRUIT) && PickedPlantType(SeedType.SEED_UMBRELLA) && PickedPlantType(SeedType.SEED_WALLNUT)) || DisplayRepickWarningDialog(8, "[SEED_CHOOSER_ART_2_WARNING]")) && (!FlyersAreComing() || FlyProtectionCurrentlyPlanted() || PickedPlantType(SeedType.SEED_CATTAIL) || PickedPlantType(SeedType.SEED_CACTUS) || PickedPlantType(SeedType.SEED_BLOVER) || DisplayRepickWarningDialog(9, "[SEED_CHOOSER_FLYER_WARNING]")) && CheckSeedUpgrade(10, SeedType.SEED_GATLINGPEA, SeedType.SEED_REPEATER) && CheckSeedUpgrade(11, SeedType.SEED_WINTERMELON, SeedType.SEED_MELONPULT) && CheckSeedUpgrade(12, SeedType.SEED_TWINSUNFLOWER, SeedType.SEED_SUNFLOWER) && CheckSeedUpgrade(13, SeedType.SEED_SPIKEROCK, SeedType.SEED_SPIKEWEED) && CheckSeedUpgrade(14, SeedType.SEED_COBCANNON, SeedType.SEED_KERNELPULT) && CheckSeedUpgrade(15, SeedType.SEED_GOLD_MAGNET, SeedType.SEED_MAGNETSHROOM) && CheckSeedUpgrade(16, SeedType.SEED_GLOOMSHROOM, SeedType.SEED_FUMESHROOM) && CheckSeedUpgrade(17, SeedType.SEED_CATTAIL, SeedType.SEED_LILYPAD))
		{
			CloseSeedChooser();
		}
	}

	public void PickRandomSeeds()
	{
		for (int anIndex = mSeedsInBank; anIndex < mBoard.mSeedBank.mNumPackets; anIndex++)
		{
			SeedType aSeedType;
			do
			{
				aSeedType = (SeedType)TodCommon.RandRangeInt(0, mApp.GetSeedsAvailable());
			}
			while (!mApp.SeedTypeAvailable(aSeedType) || aSeedType == SeedType.SEED_IMITATER || mChosenSeeds[(int)aSeedType].mSeedState != ChosenSeedState.SEED_IN_CHOOSER);
			ChosenSeed aChosenSeed = mChosenSeeds[(int)aSeedType];
			aChosenSeed.mTimeStartMotion = 0;
			aChosenSeed.mTimeEndMotion = 0;
			aChosenSeed.mStartX = aChosenSeed.mX;
			aChosenSeed.mStartY = aChosenSeed.mY;
			int endX, endY;
			GetSeedPositionInBank(anIndex, out endX, out endY);
			aChosenSeed.mEndX = endX;
			aChosenSeed.mEndY = endY;
			aChosenSeed.mSeedState = ChosenSeedState.SEED_IN_BANK;
			aChosenSeed.mSeedIndexInBank = anIndex;
			mSeedsInBank++;
		}
		for (SeedType aSeedFlying = SeedType.SEED_PEASHOOTER; aSeedFlying < SeedType.NUM_SEEDS_IN_CHOOSER; aSeedFlying++)
		{
			LandFlyingSeed(ref mChosenSeeds[(int)aSeedFlying]);
		}
		CloseSeedChooser();
	}

	public virtual void ButtonDepress(int theId)
	{
		if (mSeedsInFlight > 0 || mChooseState == SeedChooserState.CHOOSE_VIEW_LAWN || !mMouseVisible)
		{
			return;
		}
		if (theId == SeedChooserScreen_ViewLawn)
		{
			mChooseState = SeedChooserState.CHOOSE_VIEW_LAWN;
			mMenuButton.mDisabled = true;
			mViewLawnTime = 0;
		}
		else if (theId == SeedChooserScreen_Almanac)
		{
			mApp.DoAlmanacDialog().WaitForResult(true);
			mApp.mWidgetManager.SetFocus(this);
		}
		else if (theId == SeedChooserScreen_Store)
		{
mApp.ShowStoreScreen(this);
		}
		else if (theId == SeedChooserScreen_Menu)
		{
			mMenuButton.mIsOver = false;
			mMenuButton.mIsDown = false;
			UpdateCursor();
			mApp.DoNewOptions(theFromGameSelector: false);
		}
		else if (mApp.GetSeedsAvailable() >= mBoard.mSeedBank.mNumPackets)
		{
			if (theId == SeedChooserScreen_Start)
			{
				OnStartButton();
			}
			else if (theId == SeedChooserScreen_Random)
			{
				PickRandomSeeds();
			}
		}
	}

	public SeedType SeedHitTest(int x, int y)
	{
		if (mMouseVisible)
		{
			for (SeedType aSeedType = SeedType.SEED_PEASHOOTER; aSeedType < SeedType.NUM_SEEDS_IN_CHOOSER; aSeedType++)
			{
				ChosenSeed aChosenSeed = mChosenSeeds[(int)aSeedType];
				if (!mApp.SeedTypeAvailable(aSeedType) || aChosenSeed.mSeedState == ChosenSeedState.SEED_PACKET_HIDDEN)
				{
					continue;
				}
				if (aChosenSeed.mSeedState == ChosenSeedState.SEED_IN_CHOOSER)
				{
					TRect aChosenSeedRect = new TRect(aChosenSeed.mX, aChosenSeed.mY + (aChosenSeed.mSeedType != SeedType.SEED_IMITATER ? -(int)mScrollPosition : 0), Constants.SEED_PACKET_WIDTH, Constants.SEED_PACKET_HEIGHT);
					if ((aChosenSeed.mSeedType != SeedType.SEED_IMITATER ? SeedClipRect().Contains(x, y) : true) && aChosenSeedRect.Contains(x, y))
					{
						return aSeedType;
					}
				}
				else
				{
					if (new TRect(aChosenSeed.mX, aChosenSeed.mY, Constants.SEED_PACKET_WIDTH, Constants.SEED_PACKET_HEIGHT).Contains(x, y))
					{
						return aSeedType;
					}
				}
			}
		}
		return SeedType.SEED_NONE;
	}

	public SeedType FindSeedInBank(int theIndexInBank)
	{
		for (SeedType aSeedType = SeedType.SEED_PEASHOOTER; aSeedType < SeedType.NUM_SEEDS_IN_CHOOSER; aSeedType++)
		{
			if (mApp.SeedTypeAvailable(aSeedType))
			{
				ChosenSeed aChosenSeed = mChosenSeeds[(int)aSeedType];
				if (aChosenSeed.mSeedState == ChosenSeedState.SEED_IN_BANK && aChosenSeed.mSeedIndexInBank == theIndexInBank)
				{
					return aSeedType;
				}
			}
		}
		return SeedType.SEED_NONE;
	}

	public void EnableStartButton(bool theEnabled)
	{
		mStartButton.SetDisabled(!theEnabled);
		if (theEnabled)
		{
			mStartButton.mColors[(int)GameButton.ButtonColours.COLOR_LABEL] = SexyColor.White;
		}
		else
		{
			mStartButton.mColors[(int)GameButton.ButtonColours.COLOR_LABEL] = new SexyColor(64, 64, 64);
		}
	}

	public void ClickedSeedInBank(ref ChosenSeed theChosenSeed)
	{
		mPreviousType = FindSeedInBank(mSeedsInBank - (theChosenSeed.mSeedIndexInBank == mSeedsInBank - 1 ? 2 : 1));
		for (int anIndex = theChosenSeed.mSeedIndexInBank + 1; anIndex < mBoard.mSeedBank.mNumPackets; anIndex++)
		{
			SeedType aSeedType = FindSeedInBank(anIndex);
			if (aSeedType != SeedType.SEED_NONE)
			{
				ChosenSeed aChosenSeed = mChosenSeeds[(int)aSeedType];
				aChosenSeed.mTimeStartMotion = mSeedChooserAge;
				aChosenSeed.mTimeEndMotion = mSeedChooserAge + 15;
				aChosenSeed.mStartX = aChosenSeed.mX;
				aChosenSeed.mStartY = aChosenSeed.mY;
				int endX, endY;
				GetSeedPositionInBank(anIndex - 1, out endX, out endY);
				aChosenSeed.mEndX = endX;
				aChosenSeed.mEndY = endY;
				aChosenSeed.mSeedState = ChosenSeedState.SEED_FLYING_TO_BANK;
				aChosenSeed.mSeedIndexInBank = anIndex - 1;
				mSeedsInFlight++;
			}
		}
		int cx, cy;
		GetSeedPositionInChooser((int)theChosenSeed.mSeedType, out cx, out cy);
		theChosenSeed.mX = cx;
		theChosenSeed.mY = cy;
		theChosenSeed.mY += theChosenSeed.mSeedType == SeedType.SEED_IMITATER ? 0 : (int)mScrollPosition;
		theChosenSeed.mSeedState = ChosenSeedState.SEED_IN_CHOOSER;
		theChosenSeed.mImitaterType = SeedType.SEED_NONE;
		theChosenSeed.mSeedIndexInBank = 0;
		mSeedsInBank--;
		RemoveToolTip();
		EnableStartButton(theEnabled: false);
		mApp.PlaySample(Resources.SOUND_TAP);
	}

	public void ClickedSeedInChooser(ref ChosenSeed theChosenSeed)
	{
		if (mSeedsInBank == mBoard.mSeedBank.mNumPackets)
		{
			return;
		}
		if (IsImitaterUnselectable(theChosenSeed.mSeedType))
		{
			return;
		}
		if (theChosenSeed.mSeedType == SeedType.SEED_IMITATER)
		{
			theChosenSeed.mImitaterType = FindSeedInBank(mSeedsInBank - 1);
		}
		else
		{
			mPreviousType = theChosenSeed.mSeedType;
		}
		theChosenSeed.mTimeStartMotion = mSeedChooserAge;
		theChosenSeed.mTimeEndMotion = mSeedChooserAge + 25;
		theChosenSeed.mStartX = theChosenSeed.mX;
		theChosenSeed.mStartY = theChosenSeed.mY - (theChosenSeed.mSeedType == SeedType.SEED_IMITATER ? 0 : (int)mScrollPosition);
		int endX, endY;
		GetSeedPositionInBank(mSeedsInBank, out endX, out endY);
		theChosenSeed.mEndX = endX;
		theChosenSeed.mEndY = endY;
		theChosenSeed.mSeedState = ChosenSeedState.SEED_FLYING_TO_BANK;
		theChosenSeed.mSeedIndexInBank = mSeedsInBank;
		mSeedsInFlight++;
		mSeedsInBank++;
		RemoveToolTip();
		mApp.PlaySample(Resources.SOUND_TAP);
		if (mSeedsInBank == mBoard.mSeedBank.mNumPackets)
		{
			EnableStartButton(theEnabled: true);
		}
	}

	public void ShowToolTip()
	{
		if (!mApp.mWidgetManager.mMouseIn || !mApp.mWidgetManager.mHasFocus || mApp.GetDialogCount() > 0 || mChooseState != SeedChooserState.CHOOSE_NORMAL)
		{
			RemoveToolTip();
			return;
		}
		if (mChooseState == SeedChooserState.CHOOSE_VIEW_LAWN)
		{
			RemoveToolTip();
			return;
		}
		Zombie aZombie = ZombieHitTest(mLastMouseX, mLastMouseY);
		if (aZombie == null || aZombie.mFromWave != GameConstants.ZOMBIE_WAVE_CUTSCENE)
		{
			RemoveToolTip();
		}
		else if (!IsOverImitater(mLastMouseX, mLastMouseY))
		{
			string aZombieName = "[" + Zombie.GetZombieDefinition(aZombie.mZombieType).mZombieName + "]";
			mToolTip.SetTitle(aZombieName);
			if (mApp.CanShowAlmanac())
			{
				mToolTip.SetLabel("[CLICK_TO_VIEW]");
			}
			else
			{
				mToolTip.SetLabel("");
			}
			mToolTip.SetWarningText("");
			TRect aRect = aZombie.GetZombieRect();
			mToolTip.mX = aRect.mWidth / 2 + aRect.mX + 5 + mBoard.mX;
			mToolTip.mY = aRect.mHeight + aRect.mY - 10 - mBoard.mY;
			if (aZombie.mZombieType == ZombieType.ZOMBIE_BUNGEE)
			{
				mToolTip.mY = aZombie.mY;
			}
			mToolTip.mCenter = true;
			mToolTip.mVisible = true;
			if (mAlmanacButton.mBtnNoDraw && mStoreButton.mBtnNoDraw)
			{
				mToolTip.mMaxBottom = Constants.BOARD_HEIGHT;
			}
			else
			{
				mToolTip.mMaxBottom = Constants.BOARD_HEIGHT - 30;
			}
			return;
		}
		if (mSeedsInFlight <= 0)
		{
			SeedType aSeedType = SeedHitTest(mLastMouseX, mLastMouseY);
			if (aSeedType == SeedType.SEED_NONE)
			{
				RemoveToolTip();
			}
			else if (aSeedType != mToolTipSeed)
			{
				RemoveToolTip();
				ChosenSeed aChosenSeed = mChosenSeeds[(int)aSeedType];
				uint aRecFlags = SeedNotRecommendedToPick(aSeedType);
				if (SeedNotAllowedToPick(aSeedType))
				{
					mToolTip.SetWarningText("[NOT_ALLOWED_ON_THIS_LEVEL]");
				}
				else if (SeedNotAllowedDuringTrial(aSeedType))
				{
					mToolTip.SetWarningText("[FULL_VERSION_ONLY]");
				}
				else if (aChosenSeed.mSeedState == ChosenSeedState.SEED_IN_BANK && aChosenSeed.mCrazyDavePicked)
				{
					mToolTip.SetWarningText("[CRAZY_DAVE_WANTS]");
				}
				else if (aRecFlags != 0)
				{
					if (TodCommon.TestBit(aRecFlags, 0))
					{
						mToolTip.SetWarningText("[NOCTURNAL_WARNING]");
					}
					else
					{
						mToolTip.SetWarningText("[NOT_RECOMMENDED_FOR_LEVEL]");
					}
				}
				else
				{
					mToolTip.SetWarningText("");
				}
				if (aSeedType == SeedType.SEED_IMITATER)
				{
					mToolTip.SetTitle(Plant.GetNameString(aSeedType, aChosenSeed.mImitaterType));
					mToolTip.SetLabel(Plant.GetToolTip(aChosenSeed.mImitaterType == SeedType.SEED_NONE ? SeedType.SEED_IMITATER : aChosenSeed.mImitaterType));
				}
				else
				{
					mToolTip.SetTitle(Plant.GetNameString(aSeedType, SeedType.SEED_NONE));
					mToolTip.SetLabel(Plant.GetToolTip(aSeedType));
				}
				int aSeedX, aSeedY;
				if (aChosenSeed.mSeedState == ChosenSeedState.SEED_IN_BANK)
				{
					GetSeedPositionInBank(aChosenSeed.mSeedIndexInBank, out aSeedX, out aSeedY);
				}
				else
				{
					GetSeedPositionInChooser((int)aSeedType, out aSeedX, out aSeedY);
				}
				mToolTip.mX = TodCommon.ClampInt((Constants.SEED_PACKET_WIDTH - mToolTip.mWidth) / 2 + aSeedX, 0, Constants.BOARD_WIDTH - mToolTip.mWidth);
				mToolTip.mY = aSeedY + (aSeedType == SeedType.SEED_IMITATER && aChosenSeed.mSeedState == ChosenSeedState.SEED_IN_CHOOSER ? -mToolTip.mHeight : Constants.SEED_PACKET_HEIGHT);
				mToolTip.mVisible = true;
				mToolTipSeed = aSeedType;
			}
			else if (aSeedType == mToolTipSeed)
			{
				ChosenSeed aChosenSeed = mChosenSeeds[(int)aSeedType];
				if (aChosenSeed.mSeedState != ChosenSeedState.SEED_IN_CHOOSER)
				{
					return;
				}
				int aSeedX, aSeedY;
				GetSeedPositionInChooser((int)aSeedType, out aSeedX, out aSeedY);
				mToolTip.mX = TodCommon.ClampInt((Constants.SEED_PACKET_WIDTH - mToolTip.mWidth) / 2 + aSeedX, 0, Constants.BOARD_WIDTH - mToolTip.mWidth);
				mToolTip.mY = aSeedY + 70;
			}
		}
	}

	public void RemoveToolTip()
	{
		mToolTip.mVisible = false;
		mToolTip.mMaxBottom = Constants.BOARD_HEIGHT;
		mToolTip.mCenter = false;
		mToolTipSeed = SeedType.SEED_NONE;
	}

	public void CancelLawnView()
	{
		if (mChooseState == SeedChooserState.CHOOSE_VIEW_LAWN && mViewLawnTime > 100 && mViewLawnTime <= 250)
		{
			mViewLawnTime = 251;
		}
	}

	public bool IsOverImitater(int x, int y)
	{
		return mApp.SeedTypeAvailable(SeedType.SEED_IMITATER) && new TRect(Constants.IMITATER_POS_X, Constants.IMITATER_POS_Y + Constants.SEED_CHOOSER_EXTRA_HEIGHT, Resources.IMAGE_SEEDCHOOSER_IMITATERADDON.mWidth, Resources.IMAGE_SEEDCHOOSER_IMITATERADDON.mHeight).Contains(x, y);
	}

	public override void MouseUp(int x, int y, int theClickCount)
	{
		if (theClickCount == 1)
		{
			if (mMenuButton.IsMouseOver())
			{
				ButtonDepress(SeedChooserScreen_Menu);
			}
			else if (mStartButton.IsMouseOver())
			{
				ButtonDepress(SeedChooserScreen_Start);
			}
			else if (mAlmanacButton.IsMouseOver())
			{
				ButtonDepress(SeedChooserScreen_Almanac);
			}
			else if (mStoreButton.IsMouseOver())
			{
				ButtonDepress(SeedChooserScreen_Store);
			}
		}
	}

	public bool IsImitaterUnselectable(SeedType seedType)
	{
		return seedType == SeedType.SEED_IMITATER && (mSeedsInBank == 0 || mSeedsInBank == mBoard.mSeedBank.mNumPackets || Plant.IsUpgrade(mPreviousType) || SeedNotAllowedToPick(mPreviousType));
	}

	public override void MouseDown(int x, int y, int theClickCount)
	{
		base.MouseDown(x, y, theClickCount);
		if (mSeedsInFlight > 0)
		{
			for (int i = 0; i < (int)SeedType.NUM_SEEDS_IN_CHOOSER; i++)
			{
				ChosenSeed theChosenSeed = mChosenSeeds[i];
				LandFlyingSeed(ref theChosenSeed);
			}
		}
		if (mChooseState == SeedChooserState.CHOOSE_VIEW_LAWN)
		{
			CancelLawnView();
			return;
		}
		if (mRandomButton.IsMouseOver())
		{
			mApp.PlaySample(Resources.SOUND_TAP);
			ButtonDepress(SeedChooserScreen_Random);
			return;
		}
		if (mViewLawnButton.IsMouseOver())
		{
			mApp.PlaySample(Resources.SOUND_TAP);
			ButtonDepress(SeedChooserScreen_ViewLawn);
			return;
		}
		if (mMenuButton.IsMouseOver())
		{
			mApp.PlaySample(Resources.SOUND_GRAVEBUTTON);
		}
		if (mStartButton.IsMouseOver() || mAlmanacButton.IsMouseOver())
		{
			mApp.PlaySample(Resources.SOUND_TAP);
		}
		if (mStoreButton.IsMouseOver())
		{
			mApp.PlaySample(Resources.SOUND_TAP);
			mScrollAmount = 0f;
			mScrollPosition = 0f;
		}
		else
		{
			if (!IsOverImitater(x, y) && !mAlmanacButton.IsMouseOver() && !mStoreButton.IsMouseOver() && mApp.CanShowAlmanac())
			{
				Zombie aZombie = ZombieHitTest(x, y);
				if (aZombie != null && aZombie.mFromWave == GameConstants.ZOMBIE_WAVE_CUTSCENE && aZombie.mZombieType != ZombieType.ZOMBIE_REDEYE_GARGANTUAR)
				{
					mApp.PlaySample(Resources.SOUND_TAP);
					mApp.DoAlmanacDialog(SeedType.SEED_NONE, aZombie.mZombieType).WaitForResult(true);
					mApp.mWidgetManager.SetFocus(this);
					return;
				}
			}
			SeedType aSeedType = SeedHitTest(x, y);
			if (aSeedType != SeedType.SEED_NONE && !SeedNotAllowedToPick(aSeedType))
			{
				if (SeedNotAllowedDuringTrial(aSeedType))
				{
					mApp.PlaySample(Resources.SOUND_TAP);
					mPendingWarningId = 18;
					mApp.LawnMessageBox(54, "[GET_FULL_VERSION_TITLE]", "[GET_FULL_VERSION_BODY]", "[GET_FULL_VERSION_YES_BUTTON]", "[GET_FULL_VERSION_NO_BUTTON]", 1, this);
				}
				else
				{
					ChosenSeed aChosenSeed = mChosenSeeds[(int)aSeedType];
					if (aChosenSeed.mSeedState == ChosenSeedState.SEED_IN_BANK)
					{
						if (aChosenSeed.mCrazyDavePicked)
						{
							mApp.PlaySample(Resources.SOUND_BUZZER);
							mToolTip.FlashWarning();
						}
						else
						{
							ClickedSeedInBank(ref aChosenSeed);
						}
					}
					else if (aChosenSeed.mSeedState == ChosenSeedState.SEED_IN_CHOOSER)
					{
						ClickedSeedInChooser(ref aChosenSeed);
					}
				}
			}
		}
	}

	public bool PickedPlantType(SeedType theSeedType)
	{
		for (SeedType aSeedType = SeedType.SEED_PEASHOOTER; aSeedType < SeedType.NUM_SEEDS_IN_CHOOSER; aSeedType++)
		{
			ChosenSeed aChosenSeed = mChosenSeeds[(int)aSeedType];
			if (aChosenSeed.mSeedState == ChosenSeedState.SEED_IN_BANK || aChosenSeed.mSeedState == ChosenSeedState.SEED_FLYING_TO_BANK)
			{
				if (aChosenSeed.mSeedType == theSeedType || (aChosenSeed.mSeedType == SeedType.SEED_IMITATER && aChosenSeed.mImitaterType == theSeedType))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void CloseSeedChooser()
	{
		Debug.ASSERT(mBoard.mSeedBank.mNumPackets == mBoard.GetNumSeedsInBank());
		for (int anIndex = 0; anIndex < mBoard.mSeedBank.mNumPackets; anIndex++)
		{
			SeedType aSeedType = FindSeedInBank(anIndex);
			ChosenSeed aChosenSeed = mChosenSeeds[(int)aSeedType];
			SeedPacket aSeedPacket = mBoard.mSeedBank.mSeedPackets[anIndex];
			aSeedPacket.SetPacketType(aSeedType, aChosenSeed.mImitaterType);
			if (aChosenSeed.mRefreshing)
			{
				aSeedPacket.mRefreshCounter = aChosenSeed.mRefreshCounter;
				aSeedPacket.mRefreshTime = Plant.GetRefreshTime(aSeedPacket.mPacketType, aSeedPacket.mImitaterType);
				aSeedPacket.mRefreshing = true;
				aSeedPacket.mActive = false;
			}
		}
		mBoard.mCutScene.EndSeedChooser();
	}

	public override void KeyDown(KeyCode theKey)
	{
		mBoard.DoTypingCheck(theKey);
	}

	public override void KeyChar(SexyChar theChar)
	{
		if (mChooseState == SeedChooserState.CHOOSE_VIEW_LAWN && (theChar.value_type == ' ' || theChar.value_type == '\r' || theChar.value_type == '\u001B'))
		{
			CancelLawnView();
		}
		else if (mApp.mTodCheatKeys && theChar.value_type == '\u001B')
		{
			PickRandomSeeds();
		}
		else
		{
			mBoard.KeyChar(theChar);
		}
	}

	public void UpdateAfterPurchase()
	{
		for (SeedType aSeedType = SeedType.SEED_PEASHOOTER; aSeedType < SeedType.NUM_SEEDS_IN_CHOOSER; aSeedType++)
		{
			ChosenSeed aChosenSeed = mChosenSeeds[(int)aSeedType];
			if (aChosenSeed.mSeedState == ChosenSeedState.SEED_IN_BANK)
			{
				int bx, by;
				GetSeedPositionInBank(aChosenSeed.mSeedIndexInBank, out bx, out by);
				aChosenSeed.mX = bx;
				aChosenSeed.mY = by;
			}
			else if (aChosenSeed.mSeedState == ChosenSeedState.SEED_IN_CHOOSER)
			{
				int cx, cy;
				GetSeedPositionInChooser((int)aSeedType, out cx, out cy);
				aChosenSeed.mX = cx;
				aChosenSeed.mY = cy;
			}
			else
			{
				continue;
			}
			aChosenSeed.mStartX = aChosenSeed.mX;
			aChosenSeed.mStartY = aChosenSeed.mY;
			aChosenSeed.mEndX = aChosenSeed.mX;
			aChosenSeed.mEndY = aChosenSeed.mY;
		}
		EnableStartButton(mSeedsInBank == mBoard.mSeedBank.mNumPackets);
		ResizeSlider();
	}

	public Zombie ZombieHitTest(int x, int y)
	{
		Zombie aRecord = null;
		if (mMouseVisible && !new TRect(mBoard.mSeedBank.mX - mX, mBoard.mSeedBank.mY - mY, mBoard.mSeedBank.mWidth, mBoard.mSeedBank.mHeight).Contains(x, y))
		{
			foreach (Zombie aZombie in mBoard.mZombies)
			{
				if (aZombie.mDead || aZombie.IsDeadOrDying() || (int)aZombie.mZombieType >= (int)ZombieType.ZOMBIE_BOSS + 1)
				{
					continue;
				}
				if (aZombie.GetZombieRect().Contains(x - mBoard.mX, y - mBoard.mY))
				{
					if (aRecord == null || aZombie.mY > aRecord.mY)
					{
						aRecord = aZombie;
					}
				}
			}
		}
		return aRecord;
	}

	public void SliderVal(int theId, double theVal)
	{
		switch (theId)
		{
		case 0:
			mScrollPosition = (float)(theVal * mMaxScrollPosition);
			break;
		}
	}

	public override bool BackButtonPress()
	{
		int mId = mMenuButton.mId;
		ButtonPress(mId);
		ButtonDepress(mId);
		return true;
	}

	public virtual void ButtonPress(int theId)
	{
	}

public void BackFromStore()
	{
		mApp.mMusic.MakeSureMusicIsPlaying(MusicTune.MUSIC_TUNE_CHOOSE_YOUR_SEEDS);
	}

	public void LawnMessageBoxDone(int theResult)
	{
		if (theResult == 1000)
		{
			if (mPendingWarningId == 18)
			{
				mApp.DoBackToMain();
				return;
			}
			mPickWarningsWaved[mPendingWarningId] = true;
			mDoStartButton = true;
		}
	}

	public void ResizeSlider()
	{
		mSlider.Resize(472, 92, 40, Resources.IMAGE_SEEDCHOOSER_BACKGROUND.mHeight - (mApp.SeedTypeAvailable(SeedType.SEED_IMITATER) ? Resources.IMAGE_SEEDCHOOSER_IMITATERADDON.mHeight + 4 : 0) - 11);
	}
}
