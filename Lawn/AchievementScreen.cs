using System;
using Sexy;
using Sexy.TodLib;

namespace Lawn;

internal class AchievementScreen : Widget, ButtonListener
{
	public LawnApp mApp;

	public NewLawnButton mBackButton;

	public NewLawnButton mRockButton;

	public float mScrollPosition;

	public float mScrollAmount;

	public float mMaxScrollPosition;

	public readonly float mBaseScrollSpeed = 5.5f;

	public readonly float mScrollAccel = 0.2f;

	public int mTweenTimer;

	public bool mGoesDown;

	public AchievementScreen(LawnApp theApp)
	{
		mApp = theApp;
		TodCommon.TodLoadResources("DelayLoad_Achievements");
		mScrollAmount = 0f;
		mScrollPosition = 0f;
		mTweenTimer = 0;
		mGoesDown = false;
		mBackButton = GameButton.MakeNewButton(0, this, "", null, Resources.IMAGE_BLANK, Resources.IMAGE_ACHIEVEMENT_BACK_GLOW, Resources.IMAGE_ACHIEVEMENT_BACK_GLOW);
		mBackButton.Resize(18 + Constants.Board_Additional_Width, 568 + (int)mScrollPosition - Constants.Board_Background_Offset_Y, 111, 26);
		mRockButton = GameButton.MakeNewButton(1, this, "", null, Resources.IMAGE_ACHIEVEMENT_MORE, Resources.IMAGE_ACHIEVEMENT_MORE_HIGHLIGHT, Resources.IMAGE_ACHIEVEMENT_MORE_HIGHLIGHT);
		mRockButton.Resize(710 + Constants.Board_Additional_Width, 470 + (int)mScrollPosition - Constants.Board_Background_Offset_Y, Resources.IMAGE_ACHIEVEMENT_MORE.mWidth, Resources.IMAGE_ACHIEVEMENT_MORE.mHeight);
	}

	public override void Draw(Graphics g)
	{
		int baw = Constants.Board_Additional_Width;
		g.DrawImage(Resources.IMAGE_ACHIEVEMENT_SELECTOR_TILE, 0, Constants.Ach_TileY + (int)mScrollPosition);
		for (int i = 1; i <= 70; i++)
		{
			if (i == 70)
			{
				g.DrawImage(Resources.IMAGE_ACHIEVEMENT_TILE_CHINA, 0, Resources.IMAGE_ACHIEVEMENT_TILE_CHINA.mHeight * i + (int)mScrollPosition + Constants.Ach_ChinaY);
			}
			else
			{
				g.DrawImage(Resources.IMAGE_ACHIEVEMENT_TILE, 0, Resources.IMAGE_ACHIEVEMENT_TILE.mHeight * i + (int)mScrollPosition + Constants.Ach_TileRowY);
			}
		}
		g.DrawImage(Resources.IMAGE_ACHIEVEMENT_ROCK, mRockButton.mX, mRockButton.mY);
		g.DrawImage(Resources.IMAGE_ACHIEVEMENT_BOOKWORM, baw, 1125 + (int)mScrollPosition);
		g.DrawImage(Resources.IMAGE_ACHIEVEMENT_BEJEWELED, baw, 2250 + (int)mScrollPosition);
		g.DrawImage(Resources.IMAGE_ACHIEVEMENT_CHUZZLE, baw, 4500 + (int)mScrollPosition);
		g.DrawImage(Resources.IMAGE_ACHIEVEMENT_PEGGLE, baw, 6750 + (int)mScrollPosition);
		g.DrawImage(Resources.IMAGE_ACHIEVEMENT_PIPE, baw, 9000 + (int)mScrollPosition);
		g.DrawImage(Resources.IMAGE_ACHIEVEMENT_ZUMA, baw, 11250 + (int)mScrollPosition);
		for (int i = 0; i < (int)AchievementId.MAX_ACHIEVEMENTS; i++)
		{
			string achName = ((AchievementId)i).ToString().Replace("ACHIEVEMENT_", "");
			string titleKey = "[ACHIEVEMENT_" + achName + "_TITLE]";
			string descKey = "[ACHIEVEMENT_" + achName + "_DESCRIPTION]";
			int yPos = Constants.Ach_GridY0 + 57 * (i / 2) + (int)mScrollPosition;
			int xPos = 90 + baw;
			if (i % 2 != 0)
			{
				xPos = 380 + baw;
			}
			xPos += 120;
			TodCommon.TodDrawString(g, TodStringFile.TodStringTranslate(titleKey), xPos - 20, yPos + 16, Resources.FONT_DWARVENTODCRAFT15, new SexyColor(21, 175, 0), DrawStringJustification.DS_ALIGN_LEFT);
			g.SetColor(SexyColor.White);
			g.SetFont(Resources.FONT_DWARVENTODCRAFT12);
			g.WriteWordWrapped(new TRect(xPos - 20, yPos + 30, 215, 200), TodStringFile.TodStringTranslate(descKey), 12, 0);
			g.SetColorizeImages(true);
			g.SetColor(mApp.mPlayerInfo.mEarnedAchievements[i] ? new SexyColor(255, 255, 255) : new SexyColor(255, 255, 255, 32));
			int imagePosX = xPos - 90;
			g.SetScale(0.8f, 0.8f, imagePosX, yPos);
			g.DrawImageCel(Resources.IMAGE_ACHIEVEMENTS_PORTRAITS, imagePosX, yPos, i);
			g.SetScale(1f, 1f, 0f, 0f);
			g.SetColorizeImages(false);
		}
	}

	public override void KeyDown(KeyCode theKey)
	{
		if (mApp.mWidgetManager.mFocusWidget != this)
		{
			return;
		}
		if (theKey == KeyCode.KEYCODE_UP)
		{
			mScrollPosition += 15f;
		}
		else if (theKey == KeyCode.KEYCODE_DOWN)
		{
			mScrollPosition -= 15f;
		}
		else if (theKey == KeyCode.KEYCODE_ESCAPE)
		{
			ExitScreen();
		}
	}

	public override void AddedToManager(WidgetManager theWidgetManager)
	{
		base.AddedToManager(theWidgetManager);
		AddWidget(mBackButton);
		AddWidget(mRockButton);
	}

	public override void RemovedFromManager(WidgetManager theWidgetManager)
	{
		base.RemovedFromManager(theWidgetManager);
		RemoveWidget(mBackButton);
		RemoveWidget(mRockButton);
	}

	public virtual void ButtonPress(int theId)
	{
		mApp.PlaySample(Resources.SOUND_BUTTONCLICK);
	}

	public virtual void ButtonPress(int theId, int theClickCount)
	{
		ButtonPress(theId);
	}

	public virtual void ButtonDownTick(int theId)
	{
	}

	public virtual void ButtonMouseEnter(int theId)
	{
	}

	public virtual void ButtonMouseLeave(int theId)
	{
	}

	public virtual void ButtonMouseMove(int theId, int theX, int theY)
	{
	}

	public override void Update()
	{
		base.Update();
		if (mTweenTimer > 0)
		{
			int direction = mGoesDown ? -234 : 0;
			DoButtonMovement(mScrollPosition, direction);
		}
		mMaxScrollPosition = Constants.Ach_MaxScroll;
		float aScrollSpeed = mBaseScrollSpeed + Math.Abs(mScrollAmount) * mScrollAccel;
		mScrollPosition = TodCommon.ClampFloat(mScrollPosition - mScrollAmount * aScrollSpeed, -mMaxScrollPosition, 0f);
		mScrollAmount *= 1f - mScrollAccel;
		mBackButton.Resize(128 + Constants.Board_Additional_Width, 55 + (int)mScrollPosition - Constants.Board_Background_Offset_Y + 30, 130, 80);
		mRockButton.Resize(710 + Constants.Board_Additional_Width + Constants.Ach_RockUpdXExtra, 470 + (int)mScrollPosition - Constants.Board_Background_Offset_Y, Resources.IMAGE_ACHIEVEMENT_MORE.mWidth, Resources.IMAGE_ACHIEVEMENT_MORE.mHeight);
	}

	public virtual void ButtonDepress(int theId)
	{
		if (theId == 0)
		{
			ExitScreen();
		}
		if (theId == 1)
		{
			mTweenTimer = 110;
			mGoesDown = !mGoesDown;
			if (mGoesDown)
			{
				mRockButton.mButtonImage = Resources.IMAGE_ACHIEVEMENT_TOP;
				mRockButton.mDownImage = Resources.IMAGE_ACHIEVEMENT_TOP_HIGHLIGHT;
				mRockButton.mOverImage = Resources.IMAGE_ACHIEVEMENT_TOP_HIGHLIGHT;
			}
			else
			{
				mRockButton.mButtonImage = Resources.IMAGE_ACHIEVEMENT_MORE;
				mRockButton.mDownImage = Resources.IMAGE_ACHIEVEMENT_MORE_HIGHLIGHT;
				mRockButton.mOverImage = Resources.IMAGE_ACHIEVEMENT_MORE_HIGHLIGHT;
			}
		}
	}

	public void DoButtonMovement(float startX, float finalX)
	{
		mScrollPosition = TodCommon.TodAnimateCurve(200, 0, mTweenTimer, (int)startX, (int)finalX, TodCurves.CURVE_EASE_IN_OUT);
		mTweenTimer--;
	}

	public override void MouseWheel(int theDelta)
	{
		if (mApp.mGameSelector.mMovementTimer > 0)
		{
			return;
		}
		mScrollAmount -= mBaseScrollSpeed * theDelta;
		mScrollAmount -= mScrollAmount * mScrollAccel;
	}

	public void ExitScreen()
	{
		mScrollPosition = 0f;
		mApp.mGameSelector.mMovementTimer = 75;
		mApp.mGameSelector.mDestinationY = 0;
		mApp.mGameSelector.mSelectorState = SelectorAnimState.SELECTOR_IDLE;
		mApp.mGameSelector.mEnableButtonsTransition = true;
		mApp.mWidgetManager.SetFocus(mApp.mGameSelector);
		mBackButton.SetDisabled(true);
		mRockButton.SetDisabled(true);
	}
}
