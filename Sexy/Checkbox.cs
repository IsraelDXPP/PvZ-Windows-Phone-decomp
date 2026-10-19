using Sexy.TodLib;

namespace Sexy;

internal class Checkbox : Widget
{
	protected CheckboxListener mListener;

	public int mId;

	public bool mChecked;

	public Image mUncheckedImage;

	public Image mCheckedImage;

	public TRect mCheckedRect;

	public TRect mUncheckedRect;

	public SexyColor mOutlineColor;

	public SexyColor mBkgColor;

	public SexyColor mCheckColor;

	public Checkbox(Image theUncheckedImage, Image theCheckedImage, int theId, CheckboxListener theCheckboxListener)
	{
		mUncheckedImage = theUncheckedImage;
		mCheckedImage = theCheckedImage;
		mId = theId;
		mListener = theCheckboxListener;
		mChecked = false;
		mOutlineColor = SexyColor.White;
		mBkgColor = new SexyColor(80, 80, 80);
		mCheckColor = new SexyColor(255, 255, 0);
		mDoFinger = true;
	}

	public virtual void SetChecked(bool isChecked, bool tellListener = true)
	{
		mChecked = isChecked;
		if (tellListener && mListener != null)
		{
			mListener.CheckboxChecked(mId, mChecked);
		}
		MarkDirty();
	}

	public virtual bool IsChecked()
	{
		return mChecked;
	}

	public override void Draw(Graphics g)
	{
		base.Draw(g);
		if (mCheckedRect.mWidth == 0 && mCheckedImage != null && mUncheckedImage != null)
		{
			if (mChecked)
			{
				g.DrawImage(mCheckedImage, 0, 0);
			}
			else
			{
				g.DrawImage(mUncheckedImage, 0, 0);
			}
		}
		else if (mCheckedRect.mWidth != 0 && mUncheckedImage != null)
		{
			if (mChecked)
			{
				g.DrawImage(mUncheckedImage, 0, 0, mCheckedRect);
			}
			else
			{
				g.DrawImage(mUncheckedImage, 0, 0, mUncheckedRect);
			}
		}
		else if (mUncheckedImage == null && mCheckedImage == null)
		{
			g.SetColor(mOutlineColor);
			g.FillRect(0, 0, mWidth, mHeight);
			g.SetColor(mBkgColor);
			g.FillRect(1, 1, mWidth - 2, mHeight - 2);
			if (mChecked)
			{
				g.SetColor(mCheckColor);
				g.DrawLine(1, 1, mWidth - 2, mHeight - 2);
				g.DrawLine(mWidth - 1, 1, 1, mHeight - 2);
			}
		}
	}

	public override void MouseDown(int x, int y, int theBtnNum, int theClickCount)
	{
		base.MouseDown(x, y, theBtnNum, theClickCount);
		mChecked = !mChecked;
		if (mListener != null)
		{
			mListener.CheckboxChecked(mId, mChecked);
		}
		MarkDirty();
	}
}
