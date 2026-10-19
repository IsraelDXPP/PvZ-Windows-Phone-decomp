using System;
using Sexy;
using Sexy.TodLib;

namespace Lawn;

internal class ChallengePagesDialog : LawnDialog, SliderListener
{
	private const float mBaseScrollSpeed = 1f;

	private const float mScrollAccel = 0.1f;

	public LawnApp mApp;

	public float mScrollAmount;

	public float mScrollPosition;

	public float mMaxScrollPosition;

	public Slider mSlider;

	public LawnStoneButton[] mPageButtons = new LawnStoneButton[4];

	public TRect[] mPageButtonRects = new TRect[4];

	public TRect mClipRect;

	public ChallengePagesDialog(LawnApp theApp)
		: base(theApp, null, (int)Dialogs.DIALOG_CHALLENGE_PAGES, isModal: true, "[PAGE_SELECTION_HEADER]", "", "[CLOSE_PAGE_SELECTION]", (int)Dialog.DialogButtons.BUTTONS_FOOTER)
	{
		mApp = theApp;
		CalcSize(150, 350);
		mClipRect = new TRect(45, 120, mWidth - 100, mHeight - 230);
		mScrollAmount = 0f;
		mScrollPosition = 0f;
		mMaxScrollPosition = 0f;
		mSlider = new Slider(Resources.IMAGE_CHALLENGE_SLIDERSLOT, Resources.IMAGE_OPTIONS_SLIDERKNOB2, -1, this);
		mSlider.mVisible = false;
		mSlider.SetValue(Math.Max(0.0, Math.Min(mMaxScrollPosition, mScrollPosition)));
		mSlider.mHorizontal = false;
		mSlider.Resize(mWidth - 70, mClipRect.mY, 20, mClipRect.mHeight);
		mSlider.mThumbOffsetX = -1;
		mSlider.mNoDraw = true;
		for (int aPage = 0; aPage < 4; aPage++)
		{
			LawnStoneButton aPageButton = GameButton.MakeButton(aPage, this, mApp.mChallengeScreen.GetPageTitle((ChallengePage)aPage));
			mPageButtons[aPage] = aPageButton;
			aPageButton.mBtnNoDraw = true;
		}
	}

	public override void AddedToManager(WidgetManager theWidgetManager)
	{
		base.AddedToManager(theWidgetManager);
		theWidgetManager.AddWidget(mSlider);
		for (int i = 0; i < 4; i++)
		{
			theWidgetManager.AddWidget(mPageButtons[i]);
		}
	}

	public override void RemovedFromManager(WidgetManager theWidgetManager)
	{
		base.RemovedFromManager(theWidgetManager);
		theWidgetManager.RemoveWidget(mSlider);
		for (int i = 0; i < 4; i++)
		{
			theWidgetManager.RemoveWidget(mPageButtons[i]);
		}
	}

	public override void Draw(Graphics g)
	{
		base.Draw(g);
		g.SetClipRect(mClipRect);
		int totalHidden = 0;
		int maxScroll = 0;
		for (int aPage = 0; aPage < 4; aPage++)
		{
			bool isSelected = aPage == (int)mApp.mChallengeScreen.mPageIndex;
			LawnStoneButton aPageButton = mPageButtons[aPage];
			aPageButton.mDisabled = isSelected;
			aPageButton.mMouseVisible = mClipRect.Contains(mWidgetManager.mLastMouseX - mX, mWidgetManager.mLastMouseY - mY);
			aPageButton.mVisible = mApp.mChallengeScreen.IsPageUnlocked((ChallengePage)aPage);
			int aHeight = 46;
			int aOffset = 3;
			mPageButtonRects[aPage] = new TRect(mClipRect.mX, mClipRect.mY + (aPage - totalHidden) * (aHeight + aOffset) - (int)mScrollPosition, mClipRect.mWidth - (mSlider.mVisible ? mSlider.mWidth : 0), aHeight);
			TRect aPageButtonRect = mPageButtonRects[aPage];
			aPageButton.Resize(aPageButtonRect.mX, aPageButtonRect.mY, aPageButtonRect.mWidth, aPageButtonRect.mHeight);
			if (aPageButton.mVisible)
			{
				GameButton.DrawStoneButton(g, aPageButtonRect.mX, aPageButtonRect.mY, aPageButtonRect.mWidth, aPageButtonRect.mHeight, (aPageButton.mIsDown && aPageButton.mIsOver && !aPageButton.mDisabled) ^ aPageButton.mInverted, aPageButton.mIsOver, TodStringFile.TodStringTranslate(aPageButton.mLabel), isSelected ? 175 : 255);
				maxScroll += aHeight + aOffset;
			}
			else
			{
				totalHidden++;
			}
		}
		mMaxScrollPosition = Math.Max(0, maxScroll - mClipRect.mHeight);
		g.ClearClipRect();
		mSlider.SliderDraw(g);
	}

	public override void Update()
	{
		base.Update();
		mScrollPosition = TodCommon.ClampFloat(mScrollPosition += mScrollAmount * (mBaseScrollSpeed + Math.Abs(mScrollAmount) * mScrollAccel), 0f, mMaxScrollPosition);
		mScrollAmount *= 1f - mScrollAccel;
		mSlider.SetValue(Math.Max(0.0, Math.Min(mMaxScrollPosition, mScrollPosition)) / mMaxScrollPosition);
		mSlider.mVisible = mMaxScrollPosition != 0f;
	}

	public void ButtonDepress(int theId)
	{
		if (theId == 1000)
		{
			mApp.KillDialog((int)Dialogs.DIALOG_CHALLENGE_PAGES);
			mApp.PlaySample(Resources.SOUND_BUTTONCLICK);
		}
		else
		{
			mApp.mChallengeScreen.mPageIndex = (ChallengePage)theId;
			mApp.mChallengeScreen.mSlider.SetValue(0.0);
			mApp.mChallengeScreen.mScrollPosition = 0f;
			mApp.mChallengeScreen.UpdateButtons();
			for (int aPage = 0; aPage < 4; aPage++)
			{
				mPageButtons[aPage].mIsOver = false;
			}
			mApp.SetCursor(SexyAppBase.CURSOR_POINTER);
		}
	}

	public void SliderVal(int theId, double theVal)
	{
		switch (theId)
		{
		case -1:
			mScrollPosition = (float)(theVal * mMaxScrollPosition);
			break;
		}
	}

	public override void MouseWheel(int theDelta)
	{
		mScrollAmount -= mBaseScrollSpeed * theDelta;
		mScrollAmount -= mScrollAmount * mScrollAccel;
	}

	public override void KeyDown(KeyCode theKey)
	{
		if (theKey == KeyCode.KEYCODE_ESCAPE)
		{
			mApp.KillDialog((int)Dialogs.DIALOG_CHALLENGE_PAGES);
			mApp.PlaySample(Resources.SOUND_BUTTONCLICK);
		}
	}
}
