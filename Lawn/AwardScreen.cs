using System;
using Sexy;
using Sexy.TodLib;

namespace Lawn;

internal class AwardScreen : Widget, StoreListener, ButtonListener, SliderListener
{
	private const int AwardScreen_Start = 100;

	private const int AwardScreen_Menu = 101;

	private const int AwardScreen_Slider = 102;

	private const float mBaseScrollSpeed = 1f;

	private const float mScrollAccel = 0.1f;

	public GameButton mStartButton;

	public GameButton mMenuButton;

	public LawnApp mApp;

	public int mFadeInCounter;

	public int mAchievementCounter;

	public AwardType mAwardType;

	public bool mShowAchievements;

	public string mState = string.Empty;

	public bool mWasDrawn;

	public float mScrollAmount;

	public float mScrollPosition;

	public float mMaxScrollPosition;

	public Slider mSlider;

	private static TRect AchievementClipRect()
	{
		return new TRect(0, 95 + Constants.Board_Background_Offset_Y, Constants.BOARD_WIDTH, 405);
	}

	public AwardScreen(LawnApp theApp, AwardType theAwardType, bool theShowAchievements)
	{
		mApp = theApp;
		mClip = false;
		mFadeInCounter = 180;
		mAchievementCounter = 360;
		mScrollAmount = 0f;
		mScrollPosition = 0f;
		mMaxScrollPosition = 0f;
		mAwardType = theAwardType;
		mShowAchievements = theShowAchievements;
		LoadAchievements();
		int aLevel = mApp.mPlayerInfo.mLevel;
		if (mAwardType == AwardType.AWARD_CREDITS_ZOMBIE_NOTE)
		{
			mApp.DelayLoadBackgroundResource("DelayLoad_Background6");
			mApp.DelayLoadZombieNotePaperResource("DelayLoad_ZombieNote");
			mApp.DelayLoadZombieNoteResource("DelayLoad_Credits");
		}
		else if (mAwardType == AwardType.AWARD_HELP_ZOMBIE_NOTE)
		{
			mApp.DelayLoadBackgroundResource("DelayLoad_Background1");
			mApp.DelayLoadZombieNotePaperResource("DelayLoad_ZombieNote");
			mApp.DelayLoadZombieNoteResource("DelayLoad_ZombieNoteHelp");
		}
		else if (mApp.IsAdventureMode())
		{
			if (aLevel == 10)
			{
				mApp.DelayLoadBackgroundResource("DelayLoad_Background1");
				mApp.DelayLoadZombieNotePaperResource("DelayLoad_ZombieNote");
				mApp.DelayLoadZombieNoteResource("DelayLoad_ZombieNote1");
			}
			else if (aLevel == 20)
			{
				mApp.DelayLoadBackgroundResource("DelayLoad_Background2");
				mApp.DelayLoadZombieNotePaperResource("DelayLoad_ZombieNote");
				mApp.DelayLoadZombieNoteResource("DelayLoad_ZombieNote2");
			}
			else if (aLevel == 30)
			{
				mApp.DelayLoadBackgroundResource("DelayLoad_Background1");
				mApp.DelayLoadZombieNotePaperResource("DelayLoad_ZombieNote");
				mApp.DelayLoadZombieNoteResource("DelayLoad_ZombieNote3");
			}
			else if (aLevel == 40)
			{
				mApp.DelayLoadBackgroundResource("DelayLoad_Background2");
				mApp.DelayLoadZombieNotePaperResource("DelayLoad_ZombieNote");
				mApp.DelayLoadZombieNoteResource("DelayLoad_ZombieNote4");
			}
			else if (aLevel == 50)
			{
				mApp.DelayLoadBackgroundResource("DelayLoad_Background1");
				mApp.DelayLoadZombieNotePaperResource("DelayLoad_ZombieNote");
				mApp.DelayLoadZombieNoteResource("DelayLoad_ZombieFinalNote");
			}
		}
		mStartButton = new GameButton(AwardScreen_Start, this);
		mStartButton.mButtonImage = Resources.IMAGE_SEEDCHOOSER_BUTTON;
		mStartButton.mOverImage = null;
		mStartButton.mDownImage = null;
		mStartButton.mDisabledImage = Resources.IMAGE_SEEDCHOOSER_BUTTON_DISABLED;
		mStartButton.mOverOverlayImage = Resources.IMAGE_SEEDCHOOSER_BUTTON_GLOW;
		mStartButton.SetFont(Resources.FONT_DWARVENTODCRAFT15);
		mStartButton.mColors[(int)GameButton.ButtonColours.COLOR_LABEL] = new SexyColor(213, 159, 43);
		mStartButton.mColors[(int)GameButton.ButtonColours.COLOR_LABEL_HILITE] = new SexyColor(213, 159, 43);
		mStartButton.Resize(324 + Constants.Board_Additional_Width, 500 + Constants.Board_Background_Offset_Y, 156, 42);
		mStartButton.mTextOffsetY = -1;
		mStartButton.mParentWidget = this;
		mMenuButton = new GameButton(AwardScreen_Menu, this);
		mMenuButton.SetLabel("[AWARD_MAIN_MENU_BUTTON]");
		mMenuButton.mButtonImage = Resources.IMAGE_SEEDCHOOSER_BUTTON2;
		mMenuButton.mOverImage = Resources.IMAGE_SEEDCHOOSER_BUTTON2_GLOW;
		mMenuButton.mDownImage = null;
		mMenuButton.SetFont(Resources.FONT_BRIANNETOD12);
		mMenuButton.mColors[(int)GameButton.ButtonColours.COLOR_LABEL] = new SexyColor(42, 42, 90);
		mMenuButton.mColors[(int)GameButton.ButtonColours.COLOR_LABEL_HILITE] = new SexyColor(42, 42, 90);
		mMenuButton.Resize(677 + 2 * Constants.Board_Additional_Width, 16, 111, 26);
		mMenuButton.mTextOffsetY = 1;
		mMenuButton.mParentWidget = this;
		if (!mApp.HasFinishedAdventure() && aLevel <= 3)
		{
			mMenuButton.mBtnNoDraw = true;
			mMenuButton.mDisabled = true;
		}
		if (mAwardType == AwardType.AWARD_CREDITS_ZOMBIE_NOTE)
		{
			mStartButton.SetLabel("[ROLL_CREDITS]");
			mStartButton.mButtonImage = Resources.IMAGE_CREDITS_PLAYBUTTON;
			mStartButton.mOverImage = null;
			mStartButton.mDownImage = null;
			mStartButton.mDisabledImage = null;
			mStartButton.mOverOverlayImage = null;
			mStartButton.SetFont(Resources.FONT_HOUSEOFTERROR16);
			mStartButton.mColors[(int)GameButton.ButtonColours.COLOR_LABEL] = SexyColor.White;
			mStartButton.mColors[(int)GameButton.ButtonColours.COLOR_LABEL_HILITE] = new SexyColor(213, 159, 43);
			mStartButton.Resize(325, 505, 190, 73);
			mStartButton.mTextOffsetX = 33;
			mStartButton.mTextOffsetY = -2;
			mStartButton.mButtonOffsetX = -2;
			mStartButton.mButtonOffsetY = 8;
		}
		else if (mAwardType == AwardType.AWARD_HELP_ZOMBIE_NOTE)
		{
			mStartButton.SetLabel("[MAIN_MENU_BUTTON]");
			mMenuButton.mBtnNoDraw = true;
			mMenuButton.mDisabled = true;
		}
		else if (!mApp.IsAdventureMode())
		{
			mStartButton.SetLabel("[MAIN_MENU_BUTTON]");
			mMenuButton.mBtnNoDraw = true;
			mMenuButton.mDisabled = true;
		}
		else if (aLevel == 1)
		{
			mStartButton.SetLabel("[CONTINUE_BUTTON]");
			mMenuButton.mBtnNoDraw = true;
			mMenuButton.mDisabled = true;
		}
		else if (aLevel == 15)
		{
			mStartButton.SetLabel("[VIEW_ALMANAC_BUTTON]");
		}
		else if (aLevel == 25 || aLevel == 35 || aLevel == 45)
		{
			mStartButton.SetLabel("[CONTINUE_BUTTON]");
		}
		else
		{
			mStartButton.SetLabel("[NEXT_LEVEL_BUTTON]");
		}
		if (mApp.IsFirstTimeAdventureMode() && aLevel == 25 && mApp.IsTrialStageLocked() && !mApp.mPlayerInfo.mHasSeenUpsell)
		{
			mMenuButton.mBtnNoDraw = true;
			mMenuButton.mDisabled = true;
		}
		else if (mApp.HasFinishedAdventure())
		{
			mStartButton.SetLabel("[CONTINUE_BUTTON]");
			mMenuButton.mBtnNoDraw = true;
			mMenuButton.mDisabled = true;
		}
		if (!mShowAchievements)
		{
			StartSounds();
		}
		mSlider = new Slider(Resources.IMAGE_OPTIONS_SLIDERSLOT_PLANT, Resources.IMAGE_OPTIONS_SLIDERKNOB_PLANT, AwardScreen_Slider, this);
		mSlider.SetValue(Math.Max(0.0, Math.Min(mMaxScrollPosition, mScrollPosition)));
		mSlider.mHorizontal = false;
		mSlider.Resize(180 + Constants.Board_Additional_Width, AchievementClipRect().mY, 20, AchievementClipRect().mHeight);
		mSlider.mThumbOffsetX = -5;
		mSlider.mNoDraw = true;
		mWasDrawn = mMenuButton.mBtnNoDraw;
		mApp.mDetails = "[DISCORD_AWARD_SCREEN]";
	}

	public override void Dispose()
	{
		mApp.DelayLoadBackgroundResource(string.Empty);
		mApp.DelayLoadZombieNotePaperResource(string.Empty);
		mApp.DelayLoadZombieNoteResource(string.Empty);
		mStartButton.Dispose();
		mMenuButton.Dispose();
		mSlider.Dispose();
		RemoveAllWidgets();
		mApp.UpdateDiscordState();
	}

	public void LoadAchievements()
	{
		if (mShowAchievements)
		{
			for (int i = 0; i < Achievements.NUM_ACHIEVEMENTS; i++)
			{
				if (mApp.mPlayerInfo.mEarnedAchievements[i] && !mApp.mPlayerInfo.mShownAchievements[i] && mApp.mAchievements.ReturnShowInAwards(i))
				{
					mShowAchievements = true;
					break;
				}
				else
				{
					mShowAchievements = false;
				}
			}
			mApp.WriteCurrentUserConfig();
		}
	}

	public bool IsPaperNote()
	{
		if (mAwardType == AwardType.AWARD_CREDITS_ZOMBIE_NOTE || mAwardType == AwardType.AWARD_HELP_ZOMBIE_NOTE)
		{
			return true;
		}
		int aLevel = mApp.mPlayerInfo.mLevel;
		return mApp.IsAdventureMode() && (aLevel == 10 || aLevel == 20 || aLevel == 30 || aLevel == 40 || aLevel == 50);
	}

	public void DrawBottom(Graphics g, string theTitle, string theAward, string theMessage)
	{
		g.DrawImage(Resources.IMAGE_AWARDSCREEN_BACK, 0, 0);
		TodCommon.TodDrawString(g, theTitle, Constants.BOARD_WIDTH / 2, 58 + Constants.Board_Background_Offset_Y, Resources.FONT_DWARVENTODCRAFT18, new SexyColor(213, 159, 43), DrawStringJustification.DS_ALIGN_CENTER);
		TodCommon.TodDrawString(g, theAward, Constants.BOARD_WIDTH / 2, 326 + Constants.Board_Background_Offset_Y, Resources.FONT_DWARVENTODCRAFT18, SexyColor.White, DrawStringJustification.DS_ALIGN_CENTER);
		g.SetFont(Resources.FONT_BRIANNETOD16);
		g.SetColor(new SexyColor(40, 50, 90));
		g.WriteWordWrapped(new TRect(285 + Constants.Board_Additional_Width, 360 + Constants.Board_Background_Offset_Y, 230, 90), TodStringFile.TodStringTranslate(theMessage), 0, 0, centerVertically: true);
		mState = theAward;
	}

	public void DrawAwardSeed(Graphics g)
	{
		SeedType aSeedType = mApp.GetAwardSeedForLevel(mApp.mPlayerInfo.mLevel - 1);
		string aAward = Plant.GetNameString(aSeedType, SeedType.SEED_NONE);
		string aMessage;
		if (mApp.IsTrialStageLocked() && (int)aSeedType >= (int)SeedType.SEED_SQUASH && aSeedType != SeedType.SEED_TANGLEKELP)
		{
			aMessage = "[AVAILABLE_IN_FULL_VERSION]";
		}
		else
		{
			aMessage = Plant.GetToolTip(aSeedType);
		}
		DrawBottom(g, "[NEW_PLANT]", aAward, aMessage);
		g.mScaleX = 2f;
		g.mScaleY = 2f;
		g.mScaleOrigX = 350f;
		g.mScaleOrigY = 129f;
		SeedPacket.DrawSeedPacket(g, 350 + Constants.Board_Additional_Width, 129 + Constants.Board_Background_Offset_Y, aSeedType, SeedType.SEED_NONE, 0f, 255, theDrawCost: true, theUseCurrentCost: false);
		g.mScaleX = 1f;
		g.mScaleY = 1f;
		g.mScaleOrigX = 0f;
		g.mScaleOrigY = 0f;
	}

	public override void Draw(Graphics g)
	{
		g.SetLinearBlend(linear: true);
		int aLevel = mApp.mPlayerInfo.mLevel;
		if (mAwardType == AwardType.AWARD_CREDITS_ZOMBIE_NOTE)
		{
			g.SetColor(new SexyColor(125, 200, 255, 255));
			g.SetColorizeImages(colorizeImages: true);
			g.DrawImage(Resources.IMAGE_BACKGROUND6BOSS, -900, -400, 2800, 1200);
			g.SetColorizeImages(colorizeImages: false);
			g.SetColor(new SexyColor(0, 0, 0, 64));
			g.FillRect(0, 525, Constants.BOARD_WIDTH, Constants.BOARD_HEIGHT);
			g.DrawImage(Resources.IMAGE_ZOMBIE_NOTE, 75 + Constants.Board_Additional_Width, 60 + Constants.Board_Background_Offset_Y);
			g.DrawImage(Resources.IMAGE_CREDITS_ZOMBIENOTE, 149 + Constants.Board_Additional_Width, 103 + Constants.Board_Background_Offset_Y, 475, 325);
			mState = "[DISCORD_CREDITS_NOTE]";
		}
		else if (mAwardType == AwardType.AWARD_HELP_ZOMBIE_NOTE)
		{
			g.DrawImage(Resources.IMAGE_BACKGROUND1, -700, -300, 2800, 1200);
			g.DrawImage(Resources.IMAGE_ZOMBIE_NOTE, 80 + Constants.Board_Additional_Width, 80 + Constants.Board_Background_Offset_Y);
			g.DrawImage(Resources.IMAGE_ZOMBIE_NOTE_HELP, 131 + Constants.Board_Additional_Width, 132 + Constants.Board_Background_Offset_Y);
			mState = "[DISCORD_HELP_NOTE]";
		}
		else
		{
			string aZombieNote = TodCommon.TodReplaceString("[DISCORD_ZOMBIE_NOTE]", "{LEVEL}", mApp.GetStageString(aLevel).Remove(0, 1));
			if (!mApp.IsAdventureMode())
			{
				if (mApp.EarnedGoldTrophy())
				{
					DrawBottom(g, "[BEAT_GAME_MESSAGE1]", "[GOLD_SUNFLOWER_TROPHY]", "[BEAT_GAME_MESSAGE2]");
					TodCommon.TodDrawImageCelCenterScaledF(g, Resources.IMAGE_SUNFLOWER_TROPHY, 325 + Constants.Board_Additional_Width, 65 + Constants.Board_Background_Offset_Y, 1, 0.6f, 0.6f);
				}
				else
				{
					string aMsgChar;
					if (mApp.IsSurvivalMode())
					{
						int aNumTrophies = mApp.GetNumTrophies(ChallengePage.CHALLENGE_PAGE_SURVIVAL);
						if (aNumTrophies <= 7)
						{
							aMsgChar = "[YOU_UNLOCKED_A_SURVIVAL]";
						}
						else if (aNumTrophies == 10)
						{
							aMsgChar = "[YOU_UNLOCKED_ENDLESS_SURVIVAL]";
						}
						else
						{
							aMsgChar = "[EARN_MORE_TROPHIES_FOR_ENDLESS_SURVIVAL]";
						}
					}
					else if (mApp.IsScaryPotterLevel())
					{
						aMsgChar = "[UNLOCKED_VASEBREAKER_LEVEL]";
					}
					else if (mApp.IsPuzzleMode())
					{
						aMsgChar = "[UNLOCKED_I_ZOMBIE_LEVEL]";
					}
					else
					{
						aMsgChar = mApp.GetNumTrophies(ChallengePage.CHALLENGE_PAGE_CHALLENGE) <= 17 ? "[CHALLENGE_UNLOCKED]" : "[GET_MORE_TROPHIES]";
					}
					DrawBottom(g, "[GOT_TROPHY]", "[TROPHY]", aMsgChar);
					g.DrawImage(Resources.IMAGE_TROPHY_HI_RES, Constants.BOARD_WIDTH / 2 - Resources.IMAGE_TROPHY_HI_RES.mWidth / 2, 137 + Constants.Board_Background_Offset_Y);
				}
			}
			else if (aLevel == 5)
			{
				DrawBottom(g, "[GOT_SHOVEL]", "[SHOVEL]", "[SHOVEL_DESCRIPTION]");
				g.DrawImage(Resources.IMAGE_SHOVEL_HI_RES, Constants.BOARD_WIDTH / 2 - Resources.IMAGE_SHOVEL_HI_RES.mWidth / 2, 137 + Constants.Board_Background_Offset_Y);
			}
			else if (aLevel == 10)
			{
				g.DrawImage(Resources.IMAGE_BACKGROUND1, -700 + Constants.Board_Additional_Width, -300 + Constants.Board_Background_Offset_Y, 2800, 1200);
				g.DrawImage(Resources.IMAGE_ZOMBIE_NOTE, 80 + Constants.Board_Additional_Width, 80 + Constants.Board_Background_Offset_Y);
				g.DrawImage(Resources.IMAGE_ZOMBIE_NOTE1, 131 + Constants.Board_Additional_Width, 132 + Constants.Board_Background_Offset_Y);
				TodCommon.TodDrawString(g, "[FOUND_NOTE]", Constants.BOARD_WIDTH / 2, 70 + Constants.Board_Background_Offset_Y, Resources.FONT_DWARVENTODCRAFT18, new SexyColor(255, 200, 0, 255), DrawStringJustification.DS_ALIGN_CENTER);
				mState = aZombieNote;
			}
			else if (aLevel == 15)
			{
				DrawBottom(g, "[FOUND_SUBURBAN_ALMANAC]", "[SUBURBAN_ALMANAC]", "[SUBURBAN_ALMANAC_DESCRIPTION]");
				g.DrawImage(Resources.IMAGE_ALMANAC, Constants.BOARD_WIDTH / 2 - Resources.IMAGE_ALMANAC.mWidth / 2, 160 + Constants.Board_Background_Offset_Y);
			}
			else if (aLevel == 20)
			{
				g.DrawImage(Resources.IMAGE_BACKGROUND2, -700 + Constants.Board_Additional_Width, -300 + Constants.Board_Background_Offset_Y, 2800, 1200);
				g.DrawImage(Resources.IMAGE_ZOMBIE_NOTE, 80 + Constants.Board_Additional_Width, 80 + Constants.Board_Background_Offset_Y);
				g.DrawImage(Resources.IMAGE_ZOMBIE_NOTE2, 133 + Constants.Board_Additional_Width, 127 + Constants.Board_Background_Offset_Y);
				TodCommon.TodDrawString(g, "[FOUND_NOTE]", Constants.BOARD_WIDTH / 2, 70 + Constants.Board_Background_Offset_Y, Resources.FONT_DWARVENTODCRAFT18, new SexyColor(255, 200, 0, 255), DrawStringJustification.DS_ALIGN_CENTER);
				mState = aZombieNote;
			}
			else if (aLevel == 25)
			{
				DrawBottom(g, "[FOUND_KEYS]", "[KEYS]", "[KEYS_DESCRIPTION]");
				g.DrawImage(Resources.IMAGE_CARKEYS, Constants.BOARD_WIDTH / 2 - Resources.IMAGE_CARKEYS.mWidth / 2, 160 + Constants.Board_Background_Offset_Y);
			}
			else if (aLevel == 30)
			{
				g.DrawImage(Resources.IMAGE_BACKGROUND1, -700 + Constants.Board_Additional_Width, -300 + Constants.Board_Background_Offset_Y, 2800, 1200);
				g.DrawImage(Resources.IMAGE_ZOMBIE_NOTE, 80 + Constants.Board_Additional_Width, 80 + Constants.Board_Background_Offset_Y);
				g.DrawImage(Resources.IMAGE_ZOMBIE_NOTE3, 120 + Constants.Board_Additional_Width, 117 + Constants.Board_Background_Offset_Y);
				TodCommon.TodDrawString(g, "[FOUND_NOTE]", Constants.BOARD_WIDTH / 2, 70 + Constants.Board_Background_Offset_Y, Resources.FONT_DWARVENTODCRAFT18, new SexyColor(255, 200, 0, 255), DrawStringJustification.DS_ALIGN_CENTER);
				mState = aZombieNote;
			}
			else if (aLevel == 35)
			{
				DrawBottom(g, "[FOUND_TACO]", "[TACO]", "[TACO_DESCRIPTION]");
				g.DrawImage(Resources.IMAGE_TACO, Constants.BOARD_WIDTH / 2 - Resources.IMAGE_TACO.mWidth / 2, 160 + Constants.Board_Background_Offset_Y);
			}
			else if (aLevel == 40)
			{
				g.DrawImage(Resources.IMAGE_BACKGROUND2, -700 + Constants.Board_Additional_Width, -300 + Constants.Board_Background_Offset_Y, 2800, 1200);
				g.DrawImage(Resources.IMAGE_ZOMBIE_NOTE, 80 + Constants.Board_Additional_Width, 80 + Constants.Board_Background_Offset_Y);
				g.DrawImage(Resources.IMAGE_ZOMBIE_NOTE4, 102 + Constants.Board_Additional_Width, 117 + Constants.Board_Background_Offset_Y);
				TodCommon.TodDrawString(g, "[FOUND_NOTE]", Constants.BOARD_WIDTH / 2, 70 + Constants.Board_Background_Offset_Y, Resources.FONT_DWARVENTODCRAFT18, new SexyColor(255, 200, 0, 255), DrawStringJustification.DS_ALIGN_CENTER);
				mState = aZombieNote;
			}
			else if (aLevel == 45)
			{
				DrawBottom(g, "[FOUND_WATERING_CAN]", "[WATERING_CAN]", "[WATERING_CAN_DESCRIPTION]");
				g.DrawImage(Resources.IMAGE_WATERINGCAN, Constants.BOARD_WIDTH / 2 - Resources.IMAGE_WATERINGCAN.mWidth / 2, 160 + Constants.Board_Background_Offset_Y);
			}
			else if (aLevel == 50)
			{
				g.DrawImage(Resources.IMAGE_BACKGROUND1, -700 + Constants.Board_Additional_Width, -300 + Constants.Board_Background_Offset_Y, 2800, 1200);
				g.DrawImage(Resources.IMAGE_ZOMBIE_NOTE, 80 + Constants.Board_Additional_Width, 80 + Constants.Board_Background_Offset_Y);
				g.DrawImage(Resources.IMAGE_ZOMBIE_FINAL_NOTE, 114 + Constants.Board_Additional_Width, 138 + Constants.Board_Background_Offset_Y);
				TodCommon.TodDrawString(g, "[FOUND_NOTE]", Constants.BOARD_WIDTH / 2, 70 + Constants.Board_Background_Offset_Y, Resources.FONT_DWARVENTODCRAFT18, new SexyColor(255, 200, 0, 255), DrawStringJustification.DS_ALIGN_CENTER);
				mState = aZombieNote;
			}
			else if (aLevel == 1 && mApp.HasFinishedAdventure())
			{
				DrawBottom(g, "[WIN_MESSAGE1]", "[SILVER_SUNFLOWER_TROPHY]", "[WIN_MESSAGE2]");
				TodCommon.TodDrawImageCelCenterScaledF(g, Resources.IMAGE_SUNFLOWER_TROPHY, 325 + Constants.Board_Additional_Width, 65 + Constants.Board_Background_Offset_Y, 0, 0.6f, 0.6f);
			}
			else
			{
				DrawAwardSeed(g);
			}
		}
		if (mShowAchievements)
		{
			g.DrawImage(Resources.IMAGE_CHALLENGE_BACKGROUND, 0, 0);
			mStartButton.SetLabel("[CONTINUE_BUTTON]");
			TodCommon.TodDrawString(g, "ACHIEVEMENTS", 400 + Constants.Board_Additional_Width, 58 + Constants.Board_Background_Offset_Y, Resources.FONT_HOUSEOFTERROR16, new SexyColor(220, 220, 220), DrawStringJustification.DS_ALIGN_CENTER);
			mMenuButton.mBtnNoDraw = true;
			g.SetClipRect(AchievementClipRect());
			int aTotalShown = 0;
			int aMaxScroll = 0;
			for (int i = 0; i < Achievements.NUM_ACHIEVEMENTS; i++)
			{
				if (mApp.mPlayerInfo.mEarnedAchievements[i] && !mApp.mPlayerInfo.mShownAchievements[i] && mApp.mAchievements.ReturnShowInAwards(i))
				{
					aTotalShown++;
					string aAchievementName = "[ACHIEVEMENT_" + mApp.mAchievements.ReturnAchievementName(i) + "_TITLE]";
					string aAchievementDesc = "[ACHIEVEMENT_" + mApp.mAchievements.ReturnAchievementName(i) + "_DESCRIPTION]";
					TRect aTextRect = new TRect(80, 30, 320, 230);
					int aXPos = Constants.BOARD_WIDTH / 2 - aTextRect.mWidth / 2 - 35;
					int aOffset = 75;
					int aYPos = 20 + aTotalShown * aOffset - (int)mScrollPosition + Constants.Board_Background_Offset_Y;
					aTextRect.mX += aXPos;
					aTextRect.mY += aYPos - 2;
					g.DrawImageCel(Resources.IMAGE_ACHIEVEMENTS_PORTRAITS, aXPos, aYPos, i);
					TodCommon.TodDrawString(g, aAchievementName, aTextRect.mX, aYPos + 23, Resources.FONT_DWARVENTODCRAFT18, new SexyColor(255, 200, 0, 255), DrawStringJustification.DS_ALIGN_LEFT);
					g.SetFont(Resources.FONT_BRIANNETOD12);
					g.SetColor(SexyColor.White);
					g.WriteWordWrapped(aTextRect, TodStringFile.TodStringTranslate(aAchievementDesc), 0, 1, centerVertically: false);
					aMaxScroll += aOffset;
				}
			}
			mMaxScrollPosition = Math.Max(0, aMaxScroll - AchievementClipRect().mHeight - 5);
			g.ClearClipRect();
			mSlider.SliderDraw(g);
		}
		else
		{
			if (mAwardType == AwardType.AWARD_HELP_ZOMBIE_NOTE)
			{
				mStartButton.SetLabel("[MAIN_MENU_BUTTON]");
			}
			else if (!mApp.IsAdventureMode())
			{
				mStartButton.SetLabel("[MAIN_MENU_BUTTON]");
			}
			else if (aLevel == 1)
			{
				mStartButton.SetLabel("[CONTINUE_BUTTON]");
			}
			else if (aLevel == 15)
			{
				mStartButton.SetLabel("[VIEW_ALMANAC_BUTTON]");
			}
			else if (aLevel == 25 || aLevel == 35 || aLevel == 45)
			{
				mStartButton.SetLabel("[CONTINUE_BUTTON]");
			}
			else
			{
				mStartButton.SetLabel("[NEXT_LEVEL_BUTTON]");
			}
			if (mApp.HasFinishedAdventure())
			{
				mStartButton.SetLabel("[CONTINUE_BUTTON]");
			}
			mMenuButton.mBtnNoDraw = mWasDrawn;
		}
		mMenuButton.Draw(g);
		mStartButton.Draw(g);
		int aFadeInAlpha = TodCommon.TodAnimateCurve(180, 0, mFadeInCounter, 255, 0, TodCurves.CURVE_LINEAR);
		g.SetColor(IsPaperNote() ? new SexyColor(0, 0, 0, aFadeInAlpha) : new SexyColor(255, 255, 255, aFadeInAlpha));
		g.FillRect(0, 0, Constants.BOARD_WIDTH, Constants.BOARD_HEIGHT);
	}

	public override void Update()
	{
		base.Update();
		mApp.UpdateDiscordState(mState);
		if (mApp.GetDialogCount() > 0)
		{
			return;
		}
		mStartButton.Update();
		mMenuButton.Update();
		if (!(mSlider.mIsOver || mSlider.mDragging))
		{
			mApp.SetCursor(mStartButton.IsMouseOver() || mMenuButton.IsMouseOver() ? SexyAppBase.CURSOR_HAND : SexyAppBase.CURSOR_POINTER);
		}
		if (mFadeInCounter > 0)
		{
			mFadeInCounter -= 3;
			if (mFadeInCounter < 0)
			{
				mFadeInCounter = 0;
			}
		}
		if (mAchievementCounter > 0)
		{
			mAchievementCounter -= 3;
			if (mAchievementCounter < 0)
			{
				mAchievementCounter = 0;
			}
		}
		mScrollPosition = TodCommon.ClampFloat(mScrollPosition += mScrollAmount * (mBaseScrollSpeed + Math.Abs(mScrollAmount) * mScrollAccel), 0f, mMaxScrollPosition);
		mScrollAmount *= 1f - mScrollAccel;
		mSlider.SetValue(Math.Max(0.0, Math.Min(mMaxScrollPosition, mScrollPosition)) / mMaxScrollPosition);
		mSlider.mVisible = mMaxScrollPosition != 0f && mShowAchievements;
		MarkDirty();
	}

	public override void AddedToManager(WidgetManager theWidgetManager)
	{
		base.AddedToManager(theWidgetManager);
		theWidgetManager.AddWidget(mSlider);
	}

	public override void RemovedFromManager(WidgetManager theWidgetManager)
	{
		base.RemovedFromManager(theWidgetManager);
		theWidgetManager.RemoveWidget(mSlider);
	}

	public override void KeyChar(SexyChar theChar)
	{
		if (theChar.value_type == ' ' || theChar.value_type == '\r' || theChar.value_type == '\u001B')
		{
			StartButtonPressed();
		}
	}

	public void ExitScreen()
	{
		if (mApp.mPlayingQuickplay)
		{
			mApp.KillGameSelector();
			mApp.ShowGameSelector();
		}
		else if (mAwardType == AwardType.AWARD_CREDITS_ZOMBIE_NOTE)
		{
			mApp.KillAwardScreen();
			mApp.ShowCreditScreen();
		}
		else if (mAwardType == AwardType.AWARD_HELP_ZOMBIE_NOTE)
		{
			mApp.KillAwardScreen();
			mApp.ShowGameSelector();
		}
		else if (mApp.IsSurvivalMode())
		{
			mApp.KillAwardScreen();
			mApp.ShowChallengeScreen(ChallengePage.CHALLENGE_PAGE_SURVIVAL);
		}
		else if (mApp.IsPuzzleMode())
		{
			mApp.KillAwardScreen();
			mApp.ShowChallengeScreen(ChallengePage.CHALLENGE_PAGE_PUZZLE);
		}
		else if (mApp.IsChallengeMode())
		{
			mApp.KillAwardScreen();
			mApp.ShowChallengeScreen(ChallengePage.CHALLENGE_PAGE_CHALLENGE);
		}
		else
		{
			int aLevel = mApp.mPlayerInfo.mLevel;
			if (aLevel == 1)
			{
				mApp.KillAwardScreen();
				if (mApp.HasFinishedAdventure())
				{
					mApp.ShowAwardScreen(AwardType.AWARD_CREDITS_ZOMBIE_NOTE, theShowAchievements: false);
				}
				else
				{
					mApp.PreNewGame(GameMode.GAMEMODE_ADVENTURE, theLookForSavedGame: false);
				}
			}
			else
			{
				if (aLevel == 15)
				{
					mApp.DoAlmanacDialog().WaitForResult();
				}
				else if (aLevel == 25)
				{
mApp.ShowStoreScreen(this);
				}
				else if (aLevel == 35)
				{
mApp.ShowStoreScreen(this);
				}
				else if (aLevel == 42)
				{
mApp.ShowStoreScreen(this);
				}
				else if (aLevel == 45)
				{
					mApp.KillAwardScreen();
					mApp.PreNewGame(GameMode.GAMEMODE_CHALLENGE_ZEN_GARDEN, theLookForSavedGame: false);
					mApp.mZenGarden.SetupForZenTutorial();
					return;
				}
				else
				{
					mApp.KillAwardScreen();
					mApp.PreNewGame(GameMode.GAMEMODE_ADVENTURE, theLookForSavedGame: false);
					return;
				}
			}
		}
	}

	public void StartButtonPressed()
	{
		if (mApp.GetDialog(Dialogs.DIALOG_STORE) != null)
		{
			return;
		}
		if (mShowAchievements)
		{
			for (int i = 0; i < Achievements.NUM_ACHIEVEMENTS; i++)
			{
				if (mApp.mPlayerInfo.mEarnedAchievements[i] && !mApp.mPlayerInfo.mShownAchievements[i])
				{
					mApp.mPlayerInfo.mShownAchievements[i] = true;
				}
			}
			mShowAchievements = false;
			mFadeInCounter = 180;
			if (mAwardType == AwardType.AWARD_ACHIEVEMENT_ONLY)
			{
				ExitScreen();
			}
			else
			{
				StartSounds();
			}
			return;
		}
		if (mAwardType == AwardType.AWARD_PRE_CREDITS_ZOMBIE_NOTE)
		{
			mApp.KillAwardScreen();
			mApp.ShowAwardScreen(AwardType.AWARD_CREDITS_ZOMBIE_NOTE, theShowAchievements: false);
			return;
		}
		if (mAwardType == AwardType.AWARD_ACHIEVEMENT_ONLY)
		{
			mApp.KillAwardScreen();
			if (mApp.IsQuickPlayMode())
			{
				mApp.ShowGameSelectorQuickPlay(theDoFadeIn: false);
			}
			else
			{
				mApp.PreNewGame(GameMode.GAMEMODE_ADVENTURE, theLookForSavedGame: false);
			}
			return;
		}
		ExitScreen();
	}

	public override void MouseDown(int x, int y, int theClickCount)
	{
		if (theClickCount == 1 && (mStartButton.IsMouseOver() || mMenuButton.IsMouseOver()))
		{
			mApp.PlaySample(Resources.SOUND_TAP);
		}
	}

	public override void MouseUp(int x, int y, int theClickCount)
	{
		if (theClickCount == 1)
		{
			if (mStartButton.IsMouseOver())
			{
				StartButtonPressed();
			}
			if (mMenuButton.IsMouseOver())
			{
				for (int i = 0; i < Achievements.NUM_ACHIEVEMENTS; i++)
				{
					if (mApp.mPlayerInfo.mEarnedAchievements[i] && !mApp.mPlayerInfo.mShownAchievements[i])
					{
						mApp.mPlayerInfo.mShownAchievements[i] = true;
					}
				}
				mApp.KillAwardScreen();
				mApp.ShowGameSelector();
			}
		}
	}

	public void SliderVal(int theId, double theVal)
	{
		if (!mShowAchievements)
		{
			return;
		}
		switch (theId)
		{
		case AwardScreen_Slider:
			mScrollPosition = (float)(theVal * mMaxScrollPosition);
			break;
		}
	}

	public override void MouseWheel(int theDelta)
	{
		if (!mShowAchievements)
		{
			return;
		}
		mScrollAmount -= mBaseScrollSpeed * theDelta;
		mScrollAmount -= mScrollAmount * mScrollAccel;
	}

	public void StartSounds()
	{
		if (IsPaperNote())
		{
			mApp.mMusic.StopAllMusic();
			mStartButton.mY += 20;
			mApp.PlayFoley(FoleyType.FOLEY_PAPER);
		}
		else
		{
			mApp.mMusic.MakeSureMusicIsPlaying(MusicTune.MUSIC_TUNE_ZEN_GARDEN);
		}
	}

	public override bool BackButtonPress()
	{
		if (mAwardType == AwardType.AWARD_HELP_ZOMBIE_NOTE)
		{
			mApp.KillAwardScreen();
			mApp.ShowGameSelectorWithOptions();
			return true;
		}
		if (mAwardType == AwardType.AWARD_FOR_LEVEL || mAwardType == AwardType.AWARD_PRE_CREDITS_ZOMBIE_NOTE || mAwardType == AwardType.AWARD_CREDITS_ZOMBIE_NOTE)
		{
			mApp.KillAwardScreen();
			mApp.ShowGameSelector();
			return true;
		}
		ButtonPress(mStartButton.mId);
		ButtonDepress(mStartButton.mId);
		return true;
	}

	public virtual void ButtonPress(int theId)
	{
		mApp.PlaySample(Resources.SOUND_TAP);
	}

	public virtual void ButtonDepress(int theId)
	{
		StartButtonPressed();
	}

	public void BackFromStore()
	{
		mApp.KillDialog(4);
		mApp.KillAwardScreen();
		mApp.PreNewGame(GameMode.GAMEMODE_ADVENTURE, theLookForSavedGame: false);
	}

	public void ButtonMouseMove(int id, int x, int y)
	{
	}

	public void ButtonMouseLeave(int id)
	{
	}

	public void ButtonMouseEnter(int id)
	{
	}

	public void ButtonMouseTick(int id)
	{
	}

	public void ButtonPress(int id, int id2)
	{
	}

	public void ButtonDownTick(int id)
	{
	}
}
