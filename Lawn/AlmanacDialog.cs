using System;
using Sexy;
using Sexy.TodLib;

namespace Lawn;

internal class AlmanacDialog : LawnDialog, SliderListener
{
	public static readonly float ALMANAC_PLANT_POSITION_X = 578f + Constants.Board_Additional_Width;

	public static readonly float ALMANAC_PLANT_POSITION_Y = 140f + Constants.Board_Background_Offset_Y;

	public static readonly float ALMANAC_ZOMBIE_POSITION_X = 559f + Constants.Board_Additional_Width;

	public static readonly float ALMANAC_ZOMBIE_POSITION_Y = 175f + Constants.Board_Background_Offset_Y;

	public static readonly int ALMANAC_INDEXPLANT_POSITION_X = 167 + Constants.Board_Additional_Width;

	public static readonly int ALMANAC_INDEXPLANT_POSITION_Y = 255 + Constants.Board_Background_Offset_Y;

	public static readonly float ALMANAC_INDEXZOMBIE_POSITION_X = 535f + Constants.Board_Additional_Width;

	public static readonly float ALMANAC_INDEXZOMBIE_POSITION_Y = 215f + Constants.Board_Background_Offset_Y;

	public const int ALMANAC_DESCRIPTION_MIN_HEIGHT = 20;

	private const int ALMANAC_BUTTON_CLOSE = 0;

	private const int ALMANAC_BUTTON_PLANT = 1;

	private const int ALMANAC_BUTTON_ZOMBIE = 2;

	private const int ALMANAC_BUTTON_INDEX = 3;

	private const int cSeedPacketRows = 8;

	private const int cSeedPacketYOffset = 8;

	private const int cSeedPacketYStartOffset = 14;

	private const int cZombieHeight = 80;

	private const int cZombieYStartOffset = 6;

	private const int cZombieRows = 5;

	private const float mBaseScrollSpeed = 1f;

	private const float mScrollAccel = 0.1f;

	private static readonly string[] cWeirdCharacters = new string[1] { "®" };

	public static bool[] gZombieDefeated = new bool[(int)ZombieType.NUM_ZOMBIE_TYPES];

	public new LawnApp mApp;

	public GameButton mCloseButton;

	public GameButton mIndexButton;

	public GameButton mPlantButton;

	public GameButton mZombieButton;

	public Slider mPlantSlider;

	public Slider mZombieSlider;

	public AlmanacPage mOpenPage;

	public SeedType mSelectedSeed;

	public ZombieType mSelectedZombie;

	public Plant mPlant;

	public Zombie mZombie;

	public Zombie[] mZombiePerfTest = new Zombie[400];

	public float mScrollPosition;

	public float mScrollAmount;

	public float mMaxScrollPosition;

	public int mLastMouseX;

	public int mLastMouseY;

	public bool mIsOverDescription;

	public int mDescriptionLineSpacing;

	public float mDescriptionScroll;

	public float mDescriptionMaxScroll;

	public float mDescriptionOffsetScroll;

	public float mDescriptionYOffset;

	public bool mDescriptionOverfill;

	public TRect mDescriptionRect;

	public TRect mDescriptionSliderRect;

	public bool mDescriptionSliderDragging;

	private TRect cSeedClipRect;

	private TRect cZombieClipRect;

	public static void AlmanacInitForPlayer()
	{
		for (int i = 0; i < (int)ZombieType.NUM_ZOMBIE_TYPES; i++)
		{
			gZombieDefeated[i] = false;
		}
	}

	public static void AlmanacPlayerDefeatedZombie(ZombieType theZombieType)
	{
		gZombieDefeated[(int)theZombieType] = true;
	}

	public AlmanacDialog(LawnApp theApp)
		: base(theApp, null, (int)Dialogs.DIALOG_ALMANAC, isModal: true, "Almanac", "", "", 0)
	{
		mApp = (LawnApp)GlobalStaticVars.gSexyAppBase;
		mOpenPage = AlmanacPage.ALMANAC_PAGE_INDEX;
		mSelectedSeed = SeedType.SEED_PEASHOOTER;
		mSelectedZombie = ZombieType.ZOMBIE_NORMAL;
		mZombie = null;
		mPlant = null;
		mDrawStandardBack = false;
		mScrollAmount = 0f;
		mScrollPosition = 0f;
		mDescriptionScroll = 0f;
		mDescriptionOverfill = false;
		mDescriptionSliderDragging = false;
		mMaxScrollPosition = 0f;
		cSeedClipRect = new TRect(0, Constants.SEED_PACKET_HEIGHT + cSeedPacketYOffset + cSeedPacketYStartOffset + Constants.Board_Background_Offset_Y, Constants.BOARD_WIDTH, 460);
		cZombieClipRect = new TRect(0, cZombieHeight + cZombieYStartOffset + Constants.Board_Background_Offset_Y, Constants.BOARD_WIDTH, 474);
		TodCommon.TodLoadResources("DelayLoad_Almanac");
		for (int i = 0; i < mZombiePerfTest.Length; i++)
		{
			mZombiePerfTest[i] = null;
		}
		Resize(0, 0, Constants.BOARD_WIDTH, Constants.BOARD_HEIGHT);
		SexyColor aColor = new SexyColor(42, 42, 90);
		mCloseButton = new GameButton(ALMANAC_BUTTON_CLOSE, null);
		mCloseButton.mLabel = "[CLOSE_BUTTON]";
		mCloseButton.mButtonImage = Resources.IMAGE_ALMANAC_CLOSEBUTTON;
		mCloseButton.mOverImage = Resources.IMAGE_ALMANAC_CLOSEBUTTONHIGHLIGHT;
		mCloseButton.mDownImage = null;
		mCloseButton.SetFont(Resources.FONT_BRIANNETOD12);
		mCloseButton.mColors[0] = aColor;
		mCloseButton.mColors[1] = aColor;
		mCloseButton.Resize(676 + Constants.Board_Additional_Width, 567 + Constants.Board_Background_Offset_Y, 89, 26);
		mCloseButton.mTextOffsetX = -8;
		mCloseButton.mTextOffsetY = 1;
		mCloseButton.mParentWidget = this;
		mIndexButton = new GameButton(ALMANAC_BUTTON_INDEX, null);
		mIndexButton.mLabel = "[ALMANAC_INDEX]";
		mIndexButton.mButtonImage = Resources.IMAGE_ALMANAC_INDEXBUTTON;
		mIndexButton.mOverImage = Resources.IMAGE_ALMANAC_INDEXBUTTONHIGHLIGHT;
		mIndexButton.mDownImage = null;
		mIndexButton.SetFont(Resources.FONT_BRIANNETOD12);
		mIndexButton.mColors[0] = aColor;
		mIndexButton.mColors[1] = aColor;
		mIndexButton.Resize(32 + Constants.Board_Additional_Width, 567 + Constants.Board_Background_Offset_Y, 164, 26);
		mIndexButton.mTextOffsetX = 8;
		mIndexButton.mTextOffsetY = 1;
		mIndexButton.mParentWidget = this;
		mPlantButton = new GameButton(ALMANAC_BUTTON_PLANT, null);
		mPlantButton.mLabel = "[VIEW_PLANTS]";
		mPlantButton.mButtonImage = Resources.IMAGE_SEEDCHOOSER_BUTTON;
		mPlantButton.mOverImage = null;
		mPlantButton.mDownImage = null;
		mPlantButton.mDisabledImage = Resources.IMAGE_SEEDCHOOSER_BUTTON_DISABLED;
		mPlantButton.mOverOverlayImage = Resources.IMAGE_SEEDCHOOSER_BUTTON_GLOW;
		mPlantButton.SetFont(Resources.FONT_DWARVENTODCRAFT18YELLOW);
		mPlantButton.mColors[0] = SexyColor.White;
		mPlantButton.mColors[1] = SexyColor.White;
		mPlantButton.Resize(130 + Constants.Board_Additional_Width, 345 + Constants.Board_Background_Offset_Y, 156, 42);
		mPlantButton.mTextOffsetY = -1;
		mPlantButton.mParentWidget = this;
		mZombieButton = new GameButton(ALMANAC_BUTTON_ZOMBIE, null);
		mZombieButton.mLabel = "[VIEW_ZOMBIES]";
		mZombieButton.Resize(487 + Constants.Board_Additional_Width, 345 + Constants.Board_Background_Offset_Y, 210, 48);
		mZombieButton.mDrawStoneButton = true;
		mZombieButton.mParentWidget = this;
		mPlantSlider = new Slider(Resources.IMAGE_OPTIONS_SLIDERSLOT_PLANT, Resources.IMAGE_OPTIONS_SLIDERKNOB_PLANT, 0, this);
		mPlantSlider.SetValue(Math.Max(0.0, Math.Min(mMaxScrollPosition, mScrollPosition)));
		mPlantSlider.mHorizontal = false;
		mPlantSlider.Resize(10 + Constants.Board_Additional_Width, cSeedClipRect.mY, 20, cSeedClipRect.mHeight);
		mPlantSlider.mThumbOffsetX = -5;
		mPlantSlider.mVisible = false;
		mZombieSlider = new Slider(Resources.IMAGE_CHALLENGE_SLIDERSLOT, Resources.IMAGE_OPTIONS_SLIDERKNOB2, 0, this);
		mZombieSlider.SetValue(Math.Max(0.0, Math.Min(mMaxScrollPosition, mScrollPosition)));
		mZombieSlider.mHorizontal = false;
		mZombieSlider.Resize(10 + Constants.Board_Additional_Width, cZombieClipRect.mY, 20, cZombieClipRect.mHeight);
		mZombieSlider.mThumbOffsetX = -1;
		mZombieSlider.mVisible = false;
		AddWidget(mPlantSlider);
		AddWidget(mZombieSlider);
		SetPage(AlmanacPage.ALMANAC_PAGE_INDEX);
		if (mApp.mBoard == null || !mApp.mBoard.mPaused)
		{
			mApp.mMusic.MakeSureMusicIsPlaying(MusicTune.MUSIC_TUNE_CHOOSE_YOUR_SEEDS);
		}
	}

	public override void Dispose()
	{
		mCloseButton.Dispose();
		mIndexButton.Dispose();
		mPlantButton.Dispose();
		mZombieButton.Dispose();
		ClearObjects();
		RemoveAllWidgets(doDelete: true);
		base.Dispose();
	}

	public void ClearObjects()
	{
		if (mPlant != null)
		{
			mPlant.Die();
			mPlant.PrepareForReuse();
			mPlant = null;
		}
		if (mZombie != null)
		{
			mZombie.DieNoLoot(giveAchievements: false);
			mZombie.Dispose();
			mZombie = null;
		}
		for (int i = 0; i < mZombiePerfTest.Length; i++)
		{
			if (mZombiePerfTest[i] != null)
			{
				mZombiePerfTest[i].DieNoLoot(giveAchievements: false);
				mZombiePerfTest[i].Dispose();
				mZombiePerfTest[i] = null;
			}
		}
	}

	public override void RemovedFromManager(WidgetManager theWidgetManager)
	{
		base.RemovedFromManager(theWidgetManager);
		ClearObjects();
		RemoveWidget(mPlantSlider);
		RemoveWidget(mZombieSlider);
	}

	public void SetupPlant()
	{
		ClearObjects();
		float aPosX = ALMANAC_PLANT_POSITION_X;
		float aPosY = ALMANAC_PLANT_POSITION_Y;
		if (mSelectedSeed == SeedType.SEED_TALLNUT)
		{
			aPosY += 18f;
		}
		else if (mSelectedSeed == SeedType.SEED_COBCANNON)
		{
			aPosX -= 40f;
		}
		else if (mSelectedSeed == SeedType.SEED_FLOWERPOT)
		{
			aPosY -= 20f;
		}
		else if (mSelectedSeed == SeedType.SEED_INSTANT_COFFEE)
		{
			aPosY += 20f;
		}
		else if (mSelectedSeed == SeedType.SEED_GRAVEBUSTER)
		{
			aPosY += 55f;
		}
		mPlant = Plant.GetNewPlant();
		mPlant.mBoard = null;
		mPlant.mIsOnBoard = false;
		mPlant.PlantInitialize(0, 0, mSelectedSeed, SeedType.SEED_NONE);
		mPlant.mX = (int)aPosX;
		mPlant.mY = (int)aPosY;
	}

	public void SetupZombie()
	{
		ClearObjects();
		mZombie = Zombie.GetNewZombie();
		mZombie.mBoard = null;
		mZombie.ZombieInitialize(0, mSelectedZombie, theVariant: false, null, GameConstants.ZOMBIE_WAVE_UI);
		mZombie.mPosX = (int)ALMANAC_ZOMBIE_POSITION_X;
		mZombie.mPosY = (int)ALMANAC_ZOMBIE_POSITION_Y;
	}

	public void SetPage(AlmanacPage thePage)
	{
		mOpenPage = thePage;
		mPlantSlider.SetValue(0.1);
		mZombieSlider.SetValue(0.1);
		ClearObjects();
		if (mOpenPage == AlmanacPage.ALMANAC_PAGE_INDEX)
		{
			ClearObjects();
			mPlant = Plant.GetNewPlant();
			mPlant.mBoard = null;
			mPlant.mIsOnBoard = false;
			mPlant.PlantInitialize(0, 0, SeedType.SEED_SUNFLOWER, SeedType.SEED_NONE);
			mPlant.mX = ALMANAC_INDEXPLANT_POSITION_X;
			mPlant.mY = ALMANAC_INDEXPLANT_POSITION_Y;
			mZombie = Zombie.GetNewZombie();
			mZombie.mBoard = null;
			mZombie.ZombieInitialize(0, ZombieType.ZOMBIE_NORMAL, theVariant: false, null, GameConstants.ZOMBIE_WAVE_UI);
			mZombie.mPosX = (int)ALMANAC_INDEXZOMBIE_POSITION_X;
			mZombie.mPosY = (int)ALMANAC_INDEXZOMBIE_POSITION_Y;
			mIndexButton.mBtnNoDraw = true;
			mPlantButton.mBtnNoDraw = false;
			mZombieButton.mBtnNoDraw = false;
		}
		else
		{
			if (mOpenPage == AlmanacPage.ALMANAC_PAGE_PLANTS)
			{
				SetupPlant();
			}
			else if (mOpenPage == AlmanacPage.ALMANAC_PAGE_ZOMBIES)
			{
				SetupZombie();
			}
			else
			{
				return;
			}
			mIndexButton.mBtnNoDraw = false;
			mPlantButton.mBtnNoDraw = true;
			mZombieButton.mBtnNoDraw = true;
		}
	}

	public void ShowPlant(SeedType theSeedType)
	{
		mSelectedSeed = theSeedType;
		SetPage(AlmanacPage.ALMANAC_PAGE_PLANTS);
	}

	public void ShowZombie(ZombieType theZombieType)
	{
		mSelectedZombie = theZombieType;
		SetPage(AlmanacPage.ALMANAC_PAGE_ZOMBIES);
	}

	public override void Update()
	{
		mLastMouseX = mApp.mWidgetManager.mLastMouseX;
		mLastMouseY = mApp.mWidgetManager.mLastMouseY;
		mCloseButton.Update();
		mIndexButton.Update();
		mPlantButton.Update();
		mZombieButton.Update();
		if (mPlant != null)
		{
			mPlant.Update();
		}
		if (mZombie != null)
		{
			mZombie.Update();
		}
		if (mOpenPage == AlmanacPage.ALMANAC_PAGE_PLANTS)
		{
			mMaxScrollPosition = Math.Max(0, ((((int)SeedType.NUM_SEEDS_IN_CHOOSER - 2) / cSeedPacketRows) * (Constants.SEED_PACKET_HEIGHT + cSeedPacketYOffset)) + Constants.SEED_PACKET_HEIGHT - cSeedClipRect.mHeight);
			float aScrollSpeed = mBaseScrollSpeed + Math.Abs(mScrollAmount) * mScrollAccel;
			mScrollPosition = Math.Max(0f, Math.Min(mMaxScrollPosition, mScrollPosition += mScrollAmount * aScrollSpeed));
			mScrollAmount *= 1f - mScrollAccel;
			mPlantSlider.mVisible = mMaxScrollPosition != 0;
		}
		else if (mOpenPage == AlmanacPage.ALMANAC_PAGE_ZOMBIES)
		{
			mMaxScrollPosition = Math.Max(0, (((GameConstants.NUM_ZOMBIES_IN_ALMANAC - 1) / cZombieRows) * cZombieHeight) + cZombieHeight - cZombieYStartOffset - cZombieClipRect.mHeight);
			float aScrollSpeed = mBaseScrollSpeed + Math.Abs(mScrollAmount) * mScrollAccel;
			mScrollPosition += mScrollAmount * aScrollSpeed;
			mScrollPosition = Math.Max(0f, Math.Min(mMaxScrollPosition, mScrollPosition));
			mScrollAmount *= 1f - mScrollAccel;
			mZombieSlider.mVisible = mMaxScrollPosition != 0;
		}
		else
		{
			mScrollAmount = 0f;
			mScrollPosition = 0f;
			mPlantSlider.mVisible = false;
			mZombieSlider.mVisible = false;
		}
		if (mMaxScrollPosition != 0f)
		{
			double aValue = Math.Max(0f, Math.Min(mMaxScrollPosition, mScrollPosition)) / mMaxScrollPosition;
			mPlantSlider.SetValue(aValue);
			mZombieSlider.SetValue(aValue);
		}
		for (int i = 0; i < mZombiePerfTest.Length; i++)
		{
			if (mZombiePerfTest[i] != null)
			{
				mZombiePerfTest[i].Update();
			}
		}
		if (!(mPlantSlider.mIsOver || mPlantSlider.mDragging) && !(mZombieSlider.mIsOver || mZombieSlider.mDragging))
		{
			ZombieType aZombieType = ZombieHitTest(mLastMouseX, mLastMouseY);
			if (SeedHitTest(mLastMouseX, mLastMouseY) != SeedType.SEED_NONE || (aZombieType != ZombieType.ZOMBIE_INVALID && ZombieIsShown(aZombieType)) || mCloseButton.IsMouseOver() || mIndexButton.IsMouseOver() || mPlantButton.IsMouseOver() || mZombieButton.IsMouseOver())
			{
				mApp.SetCursor(SexyAppBase.CURSOR_HAND);
			}
			else
			{
				mApp.SetCursor(SexyAppBase.CURSOR_POINTER);
			}
		}
		mApp.mPoolEffect.PoolEffectUpdate();
		MarkDirty();
	}

	public static ZombieType GetZombieType(int theIndex)
	{
		return theIndex < (int)ZombieType.NUM_ZOMBIE_TYPES ? (ZombieType)theIndex : ZombieType.ZOMBIE_INVALID;
	}

	public void DrawIndex(Graphics g)
	{
		g.DrawImage(Resources.IMAGE_ALMANAC_INDEXBACK, 0f, 0f);
		TodCommon.TodDrawString(g, "[SUBURBAN_ALMANAC_INDEX]", Constants.BOARD_WIDTH / 2, 60 + Constants.Board_Background_Offset_Y, Resources.FONT_HOUSEOFTERROR28, new SexyColor(220, 220, 220), DrawStringJustification.DS_ALIGN_CENTER);
		if (mPlant != null)
		{
			Graphics aPlantGraphics = Graphics.GetNew(g);
			aPlantGraphics.mTransX += mPlant.mX;
			aPlantGraphics.mTransY += mPlant.mY;
			mPlant.Draw(aPlantGraphics);
			aPlantGraphics.PrepareForReuse();
		}
		if (mZombie != null)
		{
			Graphics aZombieGraphics = Graphics.GetNew(g);
			aZombieGraphics.mTransX += (int)mZombie.mPosX;
			aZombieGraphics.mTransY += (int)mZombie.mPosY;
			mZombie.Draw(aZombieGraphics);
			aZombieGraphics.PrepareForReuse();
		}
	}

	public void DrawPlants(Graphics g)
	{
		g.DrawImage(Resources.IMAGE_ALMANAC_PLANTBACK, 0f, 0f);
		TodCommon.TodDrawString(g, "[SUBURBAN_ALMANAC_PLANTS]", Constants.BOARD_WIDTH / 2, 48 + Constants.Board_Background_Offset_Y, Resources.FONT_HOUSEOFTERROR20, new SexyColor(213, 159, 43), DrawStringJustification.DS_ALIGN_CENTER);
		SeedType aSeedMouseOn = SeedHitTest(mLastMouseX, mLastMouseY);
		for (SeedType aSeedType = SeedType.SEED_PEASHOOTER; aSeedType < SeedType.NUM_SEEDS_IN_CHOOSER; aSeedType++)
		{
			GetSeedPosition(aSeedType, out int aPosX, out int aPosY);
			if (!mApp.SeedTypeAvailable(aSeedType))
			{
				if (aSeedType != SeedType.SEED_IMITATER)
				{
					g.SetClipRect(cSeedClipRect);
					g.DrawImage(Resources.IMAGE_ALMANAC_PLANTBLANK, aPosX, aPosY);
				}
				g.ClearClipRect();
			}
			else if (aSeedType == SeedType.SEED_IMITATER)
			{
				g.ClearClipRect();
				if (aSeedType == aSeedMouseOn)
				{
					g.DrawImage(Resources.IMAGE_ALMANAC_IMITATER, aPosX, aPosY);
				}
				g.DrawImage(Resources.IMAGE_ALMANAC_IMITATER, aPosX, aPosY);
			}
			else
			{
				g.SetClipRect(cSeedClipRect);
				SeedPacket.DrawSeedPacket(g, aPosX, aPosY, aSeedType, SeedType.SEED_NONE, 0f, 255, true, false);
				if (aSeedType == aSeedMouseOn)
				{
					g.DrawImage(Resources.IMAGE_SEEDPACKETFLASH, aPosX, aPosY);
				}
			}
		}
		g.ClearClipRect();
		if (mSelectedSeed == SeedType.SEED_LILYPAD || mSelectedSeed == SeedType.SEED_TANGLEKELP || mSelectedSeed == SeedType.SEED_CATTAIL || mSelectedSeed == SeedType.SEED_SEASHROOM)
		{
			bool aNight = mSelectedSeed == SeedType.SEED_SEASHROOM;
			g.DrawImage(aNight ? Resources.IMAGE_ALMANAC_GROUNDNIGHTPOOL : Resources.IMAGE_ALMANAC_GROUNDPOOL, 521 + Constants.Board_Additional_Width, 107 + Constants.Board_Background_Offset_Y);
			if (mApp.Is3DAccelerated())
			{
				g.SetClipRect(475, 0, 397, 500);
				g.mTransY -= 85;
				g.mTransX += Constants.Board_Additional_Width;
				mApp.mPoolEffect.PoolEffectDraw(g, aNight);
				g.mTransY += 85;
				g.mTransX -= Constants.Board_Additional_Width;
				g.ClearClipRect();
			}
		}
		else if (Plant.IsNocturnal(mSelectedSeed) || mSelectedSeed == SeedType.SEED_GRAVEBUSTER || mSelectedSeed == SeedType.SEED_PLANTERN)
		{
			g.DrawImage(Resources.IMAGE_ALMANAC_GROUNDNIGHT, 521 + Constants.Board_Additional_Width, 107 + Constants.Board_Background_Offset_Y);
		}
		else if (mSelectedSeed == SeedType.SEED_FLOWERPOT)
		{
			g.DrawImage(Resources.IMAGE_ALMANAC_GROUNDROOF, 521 + Constants.Board_Additional_Width, 107 + Constants.Board_Background_Offset_Y);
		}
		else
		{
			g.DrawImage(Resources.IMAGE_ALMANAC_GROUNDDAY, 521 + Constants.Board_Additional_Width, 107 + Constants.Board_Background_Offset_Y);
		}
		if (mPlant != null)
		{
			Graphics aPlantGraphics = Graphics.GetNew(g);
			aPlantGraphics.mTransX += mPlant.mX;
			aPlantGraphics.mTransY += mPlant.mY;
			mPlant.Draw(aPlantGraphics);
			aPlantGraphics.PrepareForReuse();
		}
		g.DrawImage(Resources.IMAGE_ALMANAC_PLANTCARD, 459 + Constants.Board_Additional_Width, 86 + Constants.Board_Background_Offset_Y);
		PlantDefinition aPlantDef = Plant.GetPlantDefinition(mSelectedSeed);
		string aName = Plant.GetNameString(mSelectedSeed, SeedType.SEED_NONE);
		TodCommon.TodDrawString(g, aName, 617 + Constants.Board_Additional_Width, 288 + Constants.Board_Background_Offset_Y, Resources.FONT_DWARVENTODCRAFT18YELLOW, SexyColor.White, DrawStringJustification.DS_ALIGN_CENTER);
		Font descriptionFont = Resources.FONT_BRIANNETOD12;
		SexyColor descriptionColor = new SexyColor(40, 50, 90);
		mDescriptionRect = new TRect(485 + Constants.Board_Additional_Width, 309 + Constants.Board_Background_Offset_Y, 258, 210);
		DrawStringJustification descriptionJustification = DrawStringJustification.DS_ALIGN_LEFT;
		string descriptionHeader = TranslateAndSanitize(Common.StrFormat_("[{0}_DESCRIPTION_HEADER]", aPlantDef.mPlantName));
		TodStringFile.TodDrawStringWrapped(g, descriptionHeader, mDescriptionRect, descriptionFont, descriptionColor, descriptionJustification);
		int textSpacing = TodStringFile.TodDrawStringWrappedHelper(g, descriptionHeader, mDescriptionRect, descriptionFont, descriptionColor, descriptionJustification, drawString: false, widowSuppression: false);
		mDescriptionRect.mY += textSpacing;
		mDescriptionRect.mHeight -= textSpacing;
		string description = TranslateAndSanitize(Common.StrFormat_("[{0}_DESCRIPTION]", aPlantDef.mPlantName));
		textSpacing = TodStringFile.TodDrawStringWrappedHelper(g, description, mDescriptionRect, descriptionFont, descriptionColor, descriptionJustification, drawString: false, widowSuppression: false);
		int rectHeight;
		if (mDescriptionRect.mHeight < textSpacing)
		{
			mIsOverDescription = mDescriptionRect.Contains(mLastMouseX, mLastMouseY);
			mDescriptionLineSpacing = descriptionFont.GetLineSpacing();
			int barWidth = 8;
			int barX = mDescriptionRect.mX + mDescriptionRect.mWidth - barWidth / 2;
			mDescriptionRect.mWidth -= barWidth;
			textSpacing = TodStringFile.TodDrawStringWrappedHelper(g, description, mDescriptionRect, descriptionFont, descriptionColor, descriptionJustification, drawString: false, widowSuppression: false);
			g.SetColor(new SexyColor(143, 67, 27, 75));
			g.FillRect(new TRect(barX, mDescriptionRect.mY, barWidth, mDescriptionRect.mHeight));
			mDescriptionMaxScroll = textSpacing - mDescriptionRect.mHeight;
			g.SetColor(new SexyColor(143, 67, 27));
			int barHeight = mDescriptionRect.mHeight - (int)mDescriptionMaxScroll;
			float posY = mDescriptionScroll;
			mDescriptionOverfill = barHeight < ALMANAC_DESCRIPTION_MIN_HEIGHT;
			if (mDescriptionOverfill)
			{
				barHeight = ALMANAC_DESCRIPTION_MIN_HEIGHT;
				posY = mDescriptionScroll / mDescriptionMaxScroll * (mDescriptionRect.mHeight - barHeight);
			}
			mDescriptionSliderRect = new TRect(barX, mDescriptionRect.mY + (int)posY, barWidth, barHeight);
			g.FillRect(mDescriptionSliderRect);
			rectHeight = textSpacing;
		}
		else
		{
			mIsOverDescription = false;
			mDescriptionLineSpacing = 0;
			mDescriptionScroll = 0f;
			mDescriptionMaxScroll = 0f;
			rectHeight = mDescriptionRect.mHeight;
		}
		g.SetClipRect(mDescriptionRect);
		TodStringFile.TodDrawStringWrapped(g, description, new TRect(mDescriptionRect.mX, mDescriptionRect.mY - (int)mDescriptionScroll, mDescriptionRect.mWidth, rectHeight), descriptionFont, descriptionColor, descriptionJustification);
		g.ClearClipRect();
		if (mSelectedSeed != SeedType.SEED_IMITATER)
		{
			string aCostStr = TodCommon.TodReplaceString(Common.StrFormat_("{KEYWORD}{COST}:{STAT} %d", aPlantDef.mSeedCost), "{COST}", "[COST]");
			TodStringFile.TodDrawStringWrapped(g, aCostStr, new TRect(485 + Constants.Board_Additional_Width, 520 + Constants.Board_Background_Offset_Y, 134, 50), Resources.FONT_BRIANNETOD12, SexyColor.White, DrawStringJustification.DS_ALIGN_LEFT);
			string aRechargeStr = TodCommon.TodReplaceString("{KEYWORD}{WAIT_TIME}:{STAT} {WAIT_TIME_LENGTH}", "{WAIT_TIME_LENGTH}", aPlantDef.mRefreshTime == 750 ? "[WAIT_TIME_SHORT]" : aPlantDef.mRefreshTime == 3000 ? "[WAIT_TIME_LONG]" : "[WAIT_TIME_VERY_LONG]");
			aRechargeStr = TodCommon.TodReplaceString(aRechargeStr, "{WAIT_TIME}", "[WAIT_TIME]");
			TodStringFile.TodDrawStringWrapped(g, aRechargeStr, new TRect(600 + Constants.Board_Additional_Width, 520 + Constants.Board_Background_Offset_Y, 139, 50), Resources.FONT_BRIANNETOD12, descriptionColor, DrawStringJustification.DS_ALIGN_RIGHT);
		}
	}

	public void DrawZombies(Graphics g)
	{
		g.DrawImage(Resources.IMAGE_ALMANAC_ZOMBIEBACK, 0f, 0f);
		TodCommon.TodDrawString(g, "[SUBURBAN_ALMANAC_ZOMBIES]", Constants.BOARD_WIDTH / 2, 54 + Constants.Board_Background_Offset_Y, Resources.FONT_DWARVENTODCRAFT24, new SexyColor(0, 196, 0), DrawStringJustification.DS_ALIGN_CENTER);
		ZombieType aZombieMouseOn = ZombieHitTest(mLastMouseX, mLastMouseY);
		g.SetClipRect(cZombieClipRect);
		for (int i = 0; i < GameConstants.NUM_ZOMBIES_IN_ALMANAC; i++)
		{
			ZombieType aZombieType = GetZombieType(i);
			GetZombiePosition(aZombieType, out int aPosX, out int aPosY);
			if (aZombieType != ZombieType.ZOMBIE_INVALID)
			{
				if (!ZombieIsShown(aZombieType))
				{
					g.DrawImage(Resources.IMAGE_ALMANAC_ZOMBIEBLANK, aPosX, aPosY);
				}
				else
				{
					g.DrawImage(Resources.IMAGE_ALMANAC_ZOMBIEWINDOW, aPosX, aPosY);
					if (aZombieType == aZombieMouseOn)
					{
						g.SetDrawMode(Graphics.DrawMode.DRAWMODE_ADDITIVE);
						g.SetColor(new SexyColor(255, 255, 255, 48));
						g.SetColorizeImages(colorizeImages: true);
						g.DrawImage(Resources.IMAGE_ALMANAC_ZOMBIEWINDOW, aPosX, aPosY);
						g.SetDrawMode(Graphics.DrawMode.DRAWMODE_NORMAL);
						g.SetColorizeImages(colorizeImages: false);
					}
					ZombieType aZombieTypeToDraw = aZombieType;
					Graphics aZombieGraphics = Graphics.GetNew(g);
					aZombieGraphics.ClipRect(aPosX + 2, aPosY + 2, 72, 72);
					aZombieGraphics.Translate(aPosX + 1, aPosY - 6);
					aZombieGraphics.mScaleX = 0.5f;
					aZombieGraphics.mScaleY = 0.5f;
					switch (aZombieType)
					{
					case ZombieType.ZOMBIE_POLEVAULTER:
						aZombieGraphics.Translate(2, -3);
						aZombieTypeToDraw = ZombieType.ZOMBIE_CACHED_POLEVAULTER_WITH_POLE;
						break;
					case ZombieType.ZOMBIE_FLAG:
						aZombieGraphics.Translate(2, 10);
						break;
					case ZombieType.ZOMBIE_TRAFFIC_CONE:
					case ZombieType.ZOMBIE_TALLNUT_HEAD:
						aZombieGraphics.Translate(0, 12);
						break;
					case ZombieType.ZOMBIE_PAIL:
						aZombieGraphics.Translate(0, 9);
						break;
					case ZombieType.ZOMBIE_FOOTBALL:
						aZombieGraphics.Translate(-8, 5);
						break;
					case ZombieType.ZOMBIE_ZAMBONI:
						aZombieGraphics.Translate(0, 3);
						break;
					case ZombieType.ZOMBIE_DOLPHIN_RIDER:
						aZombieGraphics.Translate(-2, -10);
						break;
					case ZombieType.ZOMBIE_POGO:
						aZombieGraphics.Translate(0, -3);
						break;
					case ZombieType.ZOMBIE_GARGANTUAR:
					case ZombieType.ZOMBIE_REDEYE_GARGANTUAR:
						aZombieGraphics.Translate(15, 17);
						break;
					case ZombieType.ZOMBIE_IMP:
						aZombieGraphics.Translate(-8, -7);
						break;
					case ZombieType.ZOMBIE_BUNGEE:
						aZombieGraphics.Translate(-4, 3);
						break;
					case ZombieType.ZOMBIE_BACKUP_DANCER:
						aZombieGraphics.Translate(-8, 5);
						break;
					case ZombieType.ZOMBIE_SNORKEL:
						aZombieGraphics.Translate(-10, 0);
						break;
					case ZombieType.ZOMBIE_YETI:
						aZombieGraphics.Translate(0, 4);
						break;
					case ZombieType.ZOMBIE_CATAPULT:
						aZombieGraphics.Translate(-24, -1);
						break;
					case ZombieType.ZOMBIE_BOBSLED:
						aZombieGraphics.Translate(0, -8);
						break;
					case ZombieType.ZOMBIE_LADDER:
						aZombieGraphics.Translate(0, -3);
						break;
					}
					if (ZombieHasSilhouette(aZombieType))
					{
						aZombieGraphics.SetColor(new SexyColor(0, 0, 0, 40));
						aZombieGraphics.SetColorizeImages(colorizeImages: true);
					}
					mApp.mReanimatorCache.DrawCachedZombie(aZombieGraphics, 0f, 0f, aZombieTypeToDraw);
					aZombieGraphics.SetColorizeImages(colorizeImages: false);
					g.DrawImage(Resources.IMAGE_ALMANAC_ZOMBIEWINDOW2, aPosX, aPosY);
					if (aZombieType == aZombieMouseOn)
					{
						g.SetDrawMode(Graphics.DrawMode.DRAWMODE_ADDITIVE);
						g.SetColor(new SexyColor(255, 255, 255, 48));
						g.SetColorizeImages(colorizeImages: true);
						g.DrawImage(Resources.IMAGE_ALMANAC_ZOMBIEWINDOW2, aPosX, aPosY);
						g.SetDrawMode(Graphics.DrawMode.DRAWMODE_NORMAL);
						g.SetColorizeImages(colorizeImages: false);
					}
				}
			}
		}
		g.ClearClipRect();
		g.DrawImage(mZombie == null || (mZombie.mZombieType != ZombieType.ZOMBIE_ZAMBONI && mZombie.mZombieType != ZombieType.ZOMBIE_BOBSLED) ? Resources.IMAGE_ALMANAC_GROUNDDAY : Resources.IMAGE_ALMANAC_GROUNDICE, 518 + Constants.Board_Additional_Width, 110 + Constants.Board_Background_Offset_Y);
		if (mZombie != null && !ZombieHasSilhouette(mZombie.mZombieType))
		{
			Graphics aZombieGraphics = Graphics.GetNew(g);
			aZombieGraphics.mTransX += (int)mZombie.mPosX;
			aZombieGraphics.mTransY += (int)mZombie.mPosY;
			aZombieGraphics.SetClipRect(-42, -51, 197, 187);
			switch (mZombie.mZombieType)
			{
			case ZombieType.ZOMBIE_ZAMBONI:
				aZombieGraphics.Translate(-30, 5);
				break;
			case ZombieType.ZOMBIE_GARGANTUAR:
			case ZombieType.ZOMBIE_REDEYE_GARGANTUAR:
				aZombieGraphics.Translate(0, 30);
				break;
			case ZombieType.ZOMBIE_FOOTBALL:
				aZombieGraphics.Translate(-17, 5);
				break;
			case ZombieType.ZOMBIE_BALLOON:
				aZombieGraphics.Translate(0, -20);
				break;
			case ZombieType.ZOMBIE_BUNGEE:
				aZombieGraphics.Translate(15, 0);
				break;
			case ZombieType.ZOMBIE_CATAPULT:
				aZombieGraphics.Translate(-10, 0);
				break;
			case ZombieType.ZOMBIE_BOSS:
				aZombieGraphics.Translate(-540, -175);
				break;
			}
			if (mZombie.mZombieType != ZombieType.ZOMBIE_BUNGEE && mZombie.mZombieType != ZombieType.ZOMBIE_BOSS && mZombie.mZombieType != ZombieType.ZOMBIE_ZAMBONI && mZombie.mZombieType != ZombieType.ZOMBIE_CATAPULT)
			{
				mZombie.DrawShadow(aZombieGraphics);
			}
			mZombie.Draw(aZombieGraphics);
			aZombieGraphics.PrepareForReuse();
		}
		g.DrawImage(Resources.IMAGE_ALMANAC_ZOMBIECARD, 455 + Constants.Board_Additional_Width, 78 + Constants.Board_Background_Offset_Y);
		ZombieDefinition aZombieDef = Zombie.GetZombieDefinition(mSelectedZombie);
		string aName = ZombieHasSilhouette(mSelectedZombie) ? "???" : Common.StrFormat_("[{0}]", aZombieDef.mZombieName);
		TodCommon.TodDrawString(g, aName, 613 + Constants.Board_Additional_Width, 362 + Constants.Board_Background_Offset_Y, Resources.FONT_DWARVENTODCRAFT18GREENINSET, new SexyColor(190, 255, 235, 255), DrawStringJustification.DS_ALIGN_CENTER);
		Font descriptionFont = Resources.FONT_BRIANNETOD12;
		for (int i = 0; i < GameConstants.gLawnStringFormatCount; i++)
		{
			TodStringListFormat aFormat = GameConstants.gLawnStringFormats[i];
			if (TodCommon.TestBit(aFormat.mFormatFlags, 1))
			{
				if (mApp.HasSeedType(SeedType.SEED_MAGNETSHROOM))
				{
					aFormat.mNewColor.mAlpha = 255;
					aFormat.mLineSpacingOffset = 0;
				}
				else
				{
					aFormat.mNewColor.mAlpha = 0;
					aFormat.mLineSpacingOffset = -(descriptionFont.GetLineSpacing() / 2);
				}
			}
		}
		SexyColor descriptionColor = new SexyColor(40, 50, 90);
		mDescriptionRect = new TRect(485 + Constants.Board_Additional_Width, 377 + Constants.Board_Background_Offset_Y, 257, 160);
		if (ZombieHasDescription(mSelectedZombie))
		{
			DrawStringJustification descriptionJustification = DrawStringJustification.DS_ALIGN_LEFT;
			string descriptionHeader = TranslateAndSanitize(Common.StrFormat_("[{0}_DESCRIPTION_HEADER]", aZombieDef.mZombieName));
			TodStringFile.TodDrawStringWrapped(g, descriptionHeader, mDescriptionRect, descriptionFont, descriptionColor, descriptionJustification);
			int textSpacing = TodStringFile.TodDrawStringWrappedHelper(g, descriptionHeader, mDescriptionRect, descriptionFont, descriptionColor, descriptionJustification, drawString: false, widowSuppression: false);
			mDescriptionRect.mY += textSpacing;
			mDescriptionRect.mHeight -= textSpacing;
			string description = TranslateAndSanitize(Common.StrFormat_("[{0}_DESCRIPTION]", aZombieDef.mZombieName));
			textSpacing = TodStringFile.TodDrawStringWrappedHelper(g, description, mDescriptionRect, descriptionFont, descriptionColor, descriptionJustification, drawString: false, widowSuppression: false);
			int rectHeight;
			if (mDescriptionRect.mHeight < textSpacing)
			{
				mIsOverDescription = mDescriptionRect.Contains(mLastMouseX, mLastMouseY);
				mDescriptionLineSpacing = descriptionFont.GetLineSpacing();
				int barWidth = 8;
				int barX = mDescriptionRect.mX + mDescriptionRect.mWidth - barWidth / 2;
				mDescriptionRect.mWidth -= barWidth;
				textSpacing = TodStringFile.TodDrawStringWrappedHelper(g, description, mDescriptionRect, descriptionFont, descriptionColor, descriptionJustification, drawString: false, widowSuppression: false);
				g.SetColor(new SexyColor(95, 97, 129, 75));
				g.FillRect(new TRect(barX, mDescriptionRect.mY, barWidth, mDescriptionRect.mHeight));
				mDescriptionMaxScroll = textSpacing - mDescriptionRect.mHeight;
				g.SetColor(new SexyColor(95, 97, 129));
				int barHeight = mDescriptionRect.mHeight - (int)mDescriptionMaxScroll;
				float posY = mDescriptionScroll;
				mDescriptionOverfill = barHeight < ALMANAC_DESCRIPTION_MIN_HEIGHT;
				if (mDescriptionOverfill)
				{
					barHeight = ALMANAC_DESCRIPTION_MIN_HEIGHT;
					posY = mDescriptionScroll / mDescriptionMaxScroll * (mDescriptionRect.mHeight - barHeight);
				}
				mDescriptionSliderRect = new TRect(barX, mDescriptionRect.mY + (int)posY, barWidth, barHeight);
				g.FillRect(mDescriptionSliderRect);
				rectHeight = textSpacing;
			}
			else
			{
				mIsOverDescription = false;
				mDescriptionLineSpacing = 0;
				mDescriptionScroll = 0f;
				mDescriptionMaxScroll = 0f;
				rectHeight = mDescriptionRect.mHeight;
			}
			g.SetClipRect(mDescriptionRect);
			TodStringFile.TodDrawStringWrapped(g, description, new TRect(mDescriptionRect.mX, mDescriptionRect.mY - (int)mDescriptionScroll, mDescriptionRect.mWidth, rectHeight), descriptionFont, descriptionColor, descriptionJustification);
			g.ClearClipRect();
		}
		else
		{
			TodStringFile.TodDrawStringWrapped(g, "[NOT_ENCOUNTERED_YET]", mDescriptionRect, descriptionFont, descriptionColor, DrawStringJustification.DS_ALIGN_CENTER_VERTICAL_MIDDLE);
		}
	}

	public override void Draw(Graphics g)
	{
		g.SetLinearBlend(linear: true);
		if (mOpenPage == AlmanacPage.ALMANAC_PAGE_INDEX)
		{
			DrawIndex(g);
		}
		else if (mOpenPage == AlmanacPage.ALMANAC_PAGE_PLANTS)
		{
			DrawPlants(g);
		}
		else if (mOpenPage == AlmanacPage.ALMANAC_PAGE_ZOMBIES)
		{
			DrawZombies(g);
		}
		for (int i = 0; i < mZombiePerfTest.Length; i++)
		{
			if (mZombiePerfTest[i] != null)
			{
				Graphics aTestGraphics = Graphics.GetNew(g);
				mZombiePerfTest[i].Draw(aTestGraphics);
				aTestGraphics.PrepareForReuse();
			}
		}
		mCloseButton.Draw(g);
		mIndexButton.Draw(g);
		mPlantButton.Draw(g);
		mZombieButton.Draw(g);
	}

	public void GetSeedPosition(SeedType theSeedType, out int x, out int y)
	{
		SeedType aPlantIndex = theSeedType;
		if (aPlantIndex > SeedType.SEED_IMITATER)
		{
			aPlantIndex = aPlantIndex - 1;
		}
		if (aPlantIndex == SeedType.SEED_IMITATER)
		{
			x = 20;
			y = 23;
		}
		else
		{
			int aSeedPacketWidth = Constants.SEED_PACKET_WIDTH + 2;
			int aSeedPacketHeight = Constants.SEED_PACKET_HEIGHT + cSeedPacketYOffset;
			x = (int)aPlantIndex % cSeedPacketRows * aSeedPacketWidth + aSeedPacketWidth / 2 + Constants.Board_Additional_Width;
			y = (int)aPlantIndex / cSeedPacketRows * aSeedPacketHeight + aSeedPacketHeight + cSeedPacketYStartOffset - (int)mScrollPosition + Constants.Board_Background_Offset_Y;
		}
	}

	public SeedType SeedHitTest(int x, int y)
	{
		if (mMouseVisible && mOpenPage == AlmanacPage.ALMANAC_PAGE_PLANTS)
		{
			for (SeedType aSeedType = SeedType.SEED_PEASHOOTER; aSeedType < SeedType.NUM_SEEDS_IN_CHOOSER; aSeedType++)
			{
				if (mApp.SeedTypeAvailable(aSeedType))
				{
					GetSeedPosition(aSeedType, out int aSeedX, out int aSeedY);
					TRect aSeedRect = aSeedType != SeedType.SEED_IMITATER ? new TRect(aSeedX, aSeedY, Constants.SEED_PACKET_WIDTH, Constants.SEED_PACKET_HEIGHT) : new TRect(aSeedX, aSeedY, Resources.IMAGE_ALMANAC_IMITATER.GetWidth(), Resources.IMAGE_ALMANAC_IMITATER.GetHeight());
					if ((cSeedClipRect.Contains(x, y) || aSeedType == SeedType.SEED_IMITATER) && aSeedRect.Contains(x, y))
					{
						return aSeedType;
					}
				}
			}
		}
		return SeedType.SEED_NONE;
	}

	public bool ZombieHasSilhouette(ZombieType theZombieType)
	{
		if (theZombieType != ZombieType.ZOMBIE_YETI || mApp.CanSpawnYetis())
		{
			return false;
		}
		return mApp.HasFinishedAdventure() || mApp.mPlayerInfo.GetLevel() > Zombie.GetZombieDefinition(ZombieType.ZOMBIE_YETI).mStartingLevel;
	}

	public bool ZombieIsShown(ZombieType theZombieType)
	{
		if (mApp.IsTrialStageLocked() && theZombieType > ZombieType.ZOMBIE_SNORKEL)
		{
			return false;
		}
		if (theZombieType == ZombieType.ZOMBIE_YETI)
		{
			return mApp.CanSpawnYetis() || ZombieHasSilhouette(ZombieType.ZOMBIE_YETI);
		}
		if (theZombieType <= ZombieType.ZOMBIE_BOSS)
		{
			if (mApp.HasFinishedAdventure())
			{
				return true;
			}
			int aLevel = mApp.mPlayerInfo.GetLevel();
			int aStart = Zombie.GetZombieDefinition(theZombieType).mStartingLevel;
			return aStart <= aLevel && (aStart != aLevel || !Board.IsZombieTypeSpawnedOnly(theZombieType) || gZombieDefeated[(int)theZombieType]);
		}
		if (theZombieType > ZombieType.ZOMBIE_BOSS)
		{
			return true;
		}
		return false;
	}

	public bool ZombieHasDescription(ZombieType theZombieType)
	{
		int aLevel = mApp.mPlayerInfo.GetLevel();
		int aStart = Zombie.GetZombieDefinition(theZombieType).mStartingLevel;
		if (theZombieType == ZombieType.ZOMBIE_YETI)
		{
			if (!mApp.CanSpawnYetis())
			{
				return false;
			}
			if (mApp.mPlayerInfo.mFinishedAdventure >= 2)
			{
				return true;
			}
		}
		else if (mApp.HasFinishedAdventure())
		{
			return true;
		}
		return aStart <= aLevel && (aStart != aLevel || gZombieDefeated[(int)theZombieType]);
	}

	public void GetZombiePosition(ZombieType theZombieType, out int x, out int y)
	{
		x = (int)theZombieType % cZombieRows * 85 + 22 + Constants.Board_Additional_Width;
		y = (int)theZombieType / cZombieRows * cZombieHeight + cZombieHeight + cZombieYStartOffset - (int)mScrollPosition + Constants.Board_Background_Offset_Y;
	}

	public ZombieType ZombieHitTest(int x, int y)
	{
		if (mMouseVisible && mOpenPage == AlmanacPage.ALMANAC_PAGE_ZOMBIES)
		{
			for (int i = 0; i < GameConstants.NUM_ZOMBIES_IN_ALMANAC; i++)
			{
				ZombieType aZombieType = GetZombieType(i);
				if (aZombieType != ZombieType.ZOMBIE_INVALID)
				{
					GetZombiePosition(aZombieType, out int aZombieX, out int aZombieY);
					TRect aZombieRect = new TRect(aZombieX, aZombieY, 76, 76);
					if (aZombieRect.Contains(x, y) && cZombieClipRect.Contains(x, y))
					{
						return aZombieType;
					}
				}
			}
		}
		return ZombieType.ZOMBIE_INVALID;
	}

	public override void MouseUp(int x, int y, int theClickCount)
	{
		if (mDescriptionSliderDragging)
		{
			mDescriptionSliderDragging = false;
			return;
		}
		if (mPlantButton.IsMouseOver())
		{
			SetPage(AlmanacPage.ALMANAC_PAGE_PLANTS);
		}
		else if (mZombieButton.IsMouseOver())
		{
			SetPage(AlmanacPage.ALMANAC_PAGE_ZOMBIES);
		}
		else if (mCloseButton.IsMouseOver())
		{
			mApp.KillAlmanacDialog();
		}
		else if (mIndexButton.IsMouseOver())
		{
			SetPage(AlmanacPage.ALMANAC_PAGE_INDEX);
		}
	}

	public override void MouseDown(int x, int y, int theClickCount)
	{
		if (mDescriptionSliderRect.Contains(x, y))
		{
			mDescriptionYOffset = y - (mDescriptionOverfill ? mDescriptionScroll / mDescriptionMaxScroll * (mDescriptionRect.mHeight - ALMANAC_DESCRIPTION_MIN_HEIGHT) : 0f);
			mDescriptionOffsetScroll = mDescriptionScroll;
			mDescriptionSliderDragging = true;
			return;
		}
		if (mPlantButton.IsMouseOver() || mCloseButton.IsMouseOver() || mIndexButton.IsMouseOver())
		{
			mApp.PlaySample(Resources.SOUND_TAP);
		}
		if (mZombieButton.IsMouseOver())
		{
			mApp.PlaySample(Resources.SOUND_GRAVEBUTTON);
		}
		SeedType aSeedType = SeedHitTest(x, y);
		if (aSeedType != SeedType.SEED_NONE && aSeedType != mSelectedSeed)
		{
			mSelectedSeed = aSeedType;
			SetupPlant();
			mApp.PlaySample(Resources.SOUND_TAP);
		}
		ZombieType aZombieType = ZombieHitTest(x, y);
		if (aZombieType != ZombieType.ZOMBIE_INVALID && aZombieType != mSelectedZombie && ZombieIsShown(aZombieType))
		{
			mSelectedZombie = aZombieType;
			SetupZombie();
			mApp.PlaySample(Resources.SOUND_TAP);
		}
	}

	public override void MouseDrag(int x, int y)
	{
		if (mDescriptionSliderDragging)
		{
			if (mDescriptionOverfill)
			{
				mDescriptionScroll = (y - mDescriptionYOffset) / (mDescriptionRect.mHeight - ALMANAC_DESCRIPTION_MIN_HEIGHT) * mDescriptionMaxScroll;
			}
			else
			{
				mDescriptionScroll = y - (mDescriptionYOffset - mDescriptionOffsetScroll);
			}
			if (mDescriptionScroll < 0f)
			{
				mDescriptionScroll = 0f;
			}
			else if (mDescriptionScroll > mDescriptionMaxScroll)
			{
				mDescriptionScroll = mDescriptionMaxScroll;
			}
		}
	}

	public virtual void KeyChar(char theChar)
	{
	}

	public void SliderVal(int theId, double theVal)
	{
		switch (theId)
		{
		case 0:
			mScrollPosition = (float)theVal * mMaxScrollPosition;
			break;
		}
	}

	public override void MouseWheel(int theDelta)
	{
		if (mIsOverDescription && !mDescriptionSliderDragging)
		{
			mDescriptionScroll -= mDescriptionLineSpacing * theDelta;
			if (mDescriptionScroll < 0f)
			{
				mDescriptionScroll = 0f;
			}
			else if (mDescriptionScroll > mDescriptionMaxScroll)
			{
				mDescriptionScroll = mDescriptionMaxScroll;
			}
		}
		else
		{
			mScrollAmount -= mBaseScrollSpeed * theDelta;
			mScrollAmount -= mScrollAmount * mScrollAccel;
		}
	}

	public string TranslateAndSanitize(string str)
	{
		string ret = TodStringFile.TodStringTranslate(str);
		for (int i = 0; i < cWeirdCharacters.Length; i++)
		{
			char weirdChar = cWeirdCharacters[i][0];
			int pos = 0;
			while ((pos = ret.IndexOf(weirdChar, pos)) != -1)
			{
				if (pos > 0)
				{
					ret = ret.Remove(pos - 1, 1);
					pos--;
				}
				pos++;
			}
		}
		return ret;
	}
}
