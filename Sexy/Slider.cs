namespace Sexy;

internal class Slider : Widget
{
	public SliderListener mListener;

	public double mVal;

	public int mId;

	public Image mTrackMinImage;

	public Image mTrackMaxImage;

	public Image mTrackImage;

	public Image mThumbImage;

	public bool mDragging;

	public int mRelX;

	public int mRelY;

	public bool mHorizontal = true;

	public int mThumbOffsetX;

	public bool mNoDraw;

	public Slider(int theId, SliderListener theListener, Image trackMinImage, Image trackMaxImage, Image thumbImage)
	{
		mId = theId;
		mListener = theListener;
		mTrackMinImage = trackMinImage;
		mTrackMaxImage = trackMaxImage;
		mThumbImage = thumbImage;
		mTrackImage = null;
		mVal = 0.0;
		mDragging = false;
		mRelX = 0;
		mRelY = 0;
		Resize(0, 0, trackMinImage.GetWidth() + trackMaxImage.GetWidth(), thumbImage.GetHeight());
	}

	public Slider(Image theTrackImage, Image theThumbImage, int theId, SliderListener theListener)
	{
		mTrackImage = theTrackImage;
		mThumbImage = theThumbImage;
		mId = theId;
		mListener = theListener;
		mTrackMinImage = null;
		mTrackMaxImage = null;
		mVal = 0.0;
		mDragging = false;
		mRelX = 0;
		mRelY = 0;
	}

	public virtual void SetValue(double theValue)
	{
		mVal = theValue;
		if (mVal < 0.0)
		{
			mVal = 0.0;
		}
		else if (mVal > 1.0)
		{
			mVal = 1.0;
		}
		MarkDirtyFull();
	}

	public virtual bool HasTransparencies()
	{
		return true;
	}

	public override void Draw(Graphics g)
	{
		if (mNoDraw)
		{
			return;
		}
		SliderDraw(g);
	}

	public void SliderDraw(Graphics g)
	{
		if (mNoDraw)
		{
			if (!mVisible)
			{
				return;
			}
			g.mTransX += mX;
			g.mTransY += mY;
		}
		if (mTrackMinImage != null && mTrackMaxImage != null)
		{
			int width = mThumbImage.GetWidth();
			int height = mThumbImage.GetHeight();
			int num = width / 2;
			int num2 = mWidth - width;
			TPoint tPoint = new TPoint((int)((double)num + mVal * (double)num2), mHeight / 2);
			TRect destRect = new TRect(0, 0, tPoint.mX, mHeight);
			TRect destRect2 = new TRect(tPoint.mX, 0, mWidth - tPoint.mX, mHeight);
			DrawStretchableImage(g, mTrackMinImage, destRect);
			DrawStretchableImage(g, mTrackMaxImage, destRect2);
			g.DrawImage(mThumbImage, tPoint.mX - width / 2, tPoint.mY - height / 2);
			return;
		}
		if (mTrackImage != null)
		{
			int cw = mHorizontal ? mTrackImage.GetWidth() / 3 : mTrackImage.GetWidth();
			int ch = mHorizontal ? mTrackImage.GetHeight() : mTrackImage.GetHeight() / 3;
			if (mHorizontal)
			{
				int num3 = cw;
				int height2 = ch;
				int theY = (mHeight - height2) / 2;
				g.DrawImage(mTrackImage, 0, theY, new TRect(0, 0, num3, height2));
				Graphics graphics = Graphics.GetNew(g);
				graphics.ClipRect(num3, theY, mWidth - num3 * 2, height2);
				for (int i = 0; i < (mWidth - num3 * 2 + num3 - 1) / num3; i++)
				{
					graphics.DrawImage(mTrackImage, num3 + i * num3, theY, new TRect(num3, 0, num3, height2));
				}
				g.DrawImage(mTrackImage, mWidth - num3, theY, new TRect(num3 * 2, 0, num3, height2));
				graphics.PrepareForReuse();
			}
			else
			{
				g.DrawImage(mTrackImage, 0, 0, new TRect(0, 0, cw, ch));
				Graphics graphics = Graphics.GetNew(g);
				graphics.ClipRect(0, ch, cw, mHeight - ch * 2);
				for (int i = 0; i < (mHeight - ch * 2 + ch - 1) / ch; i++)
				{
					graphics.DrawImage(mTrackImage, 0, ch + i * ch, new TRect(0, ch, cw, ch));
				}
				g.DrawImage(mTrackImage, 0, mHeight - ch, new TRect(0, ch * 2, cw, ch));
				graphics.PrepareForReuse();
			}
		}
		g.ClearClipRect();
		if (mHorizontal && mThumbImage != null)
		{
			g.DrawImage(mThumbImage, (int)(mVal * (double)(mWidth - mThumbImage.GetWidth())) + mThumbOffsetX, (mHeight - mThumbImage.GetHeight()) / 2);
		}
		else if (!mHorizontal && mThumbImage != null)
		{
			g.DrawImage(mThumbImage, (mWidth - mThumbImage.GetWidth()) / 2 + mThumbOffsetX, (int)(mVal * (double)(mHeight - mThumbImage.GetHeight())));
		}
		g.Translate(-mX, -mY);
	}

	public void DrawStretchableImage(Graphics g, Image image, TRect destRect)
	{
		int width = image.GetWidth();
		int height = image.GetHeight();
		TRect theSrcRect = new TRect(0, 0, (width - 1) / 2, height);
		TRect theSrcRect2 = new TRect(theSrcRect.mWidth, 0, 1, height);
		TRect theSrcRect3 = new TRect(theSrcRect2.mX + theSrcRect2.mWidth, 0, width - theSrcRect.mWidth - theSrcRect2.mWidth, height);
		int theY = destRect.mY + (destRect.mHeight - height) / 2;
		TRect theDestRect = new TRect(destRect.mX + theSrcRect.mWidth, theY, destRect.mWidth - theSrcRect.mWidth - theSrcRect3.mWidth, height);
		g.DrawImage(image, destRect.mX, theY, theSrcRect);
		g.DrawImage(image, theDestRect, theSrcRect2);
		g.DrawImage(image, destRect.mX + destRect.mWidth - theSrcRect3.mWidth, theY, theSrcRect3);
	}

	public override void MouseMove(int x, int y)
	{
		base.MouseMove(x, y);
		if (mHorizontal)
		{
			int aThumbX = (int)(mVal * (double)(mWidth - mThumbImage.GetWidth())) + mThumbOffsetX;
			if (x >= aThumbX && x < aThumbX + mThumbImage.GetWidth())
			{
				GlobalStaticVars.gSexyAppBase.SetCursor(SexyAppBase.CURSOR_DRAGGING);
			}
			else
			{
				GlobalStaticVars.gSexyAppBase.SetCursor(SexyAppBase.CURSOR_POINTER);
			}
		}
		else
		{
			int aThumbY = (int)(mVal * (double)(mHeight - mThumbImage.GetHeight()));
			int aThumbCenterX = (mWidth - mThumbImage.GetWidth()) / 2 + mThumbOffsetX;
			if (x >= aThumbCenterX && x < aThumbCenterX + mThumbImage.GetWidth() && y >= aThumbY && y < aThumbY + mThumbImage.GetHeight())
			{
				GlobalStaticVars.gSexyAppBase.SetCursor(SexyAppBase.CURSOR_DRAGGING);
			}
			else
			{
				GlobalStaticVars.gSexyAppBase.SetCursor(SexyAppBase.CURSOR_POINTER);
			}
		}
	}

	public override void MouseDown(int x, int y, int theMagicCode)
	{
		if (mHorizontal)
		{
			int num = (int)(mVal * (double)(mWidth - mThumbImage.GetWidth())) + mThumbOffsetX;
			if (x >= num && x < num + mThumbImage.GetWidth())
			{
				GlobalStaticVars.gSexyAppBase.SetCursor(SexyAppBase.CURSOR_DRAGGING);
				mDragging = true;
				mRelX = x - num;
			}
			else
			{
				double value = (double)x / (double)mWidth;
				SetValue(value);
			}
		}
		else
		{
			int aThumbY = (int)(mVal * (double)(mHeight - mThumbImage.GetHeight()));
			int aThumbCenterX = (mWidth - mThumbImage.GetWidth()) / 2 + mThumbOffsetX;
			if (x >= aThumbCenterX && x < aThumbCenterX + mThumbImage.GetWidth() && y >= aThumbY && y < aThumbY + mThumbImage.GetHeight())
			{
				GlobalStaticVars.gSexyAppBase.SetCursor(SexyAppBase.CURSOR_DRAGGING);
				mDragging = true;
				mRelY = y - aThumbY;
			}
			else
			{
				double value2 = (double)y / (double)mHeight;
				SetValue(value2);
			}
		}
	}

	public override void MouseDrag(int x, int y)
	{
		if (mDragging)
		{
			double num = mVal;
			if (mHorizontal)
			{
				mVal = (double)(x - mRelX) / (double)(mWidth - mThumbImage.GetWidth());
			}
			else
			{
				mVal = (double)(y - mRelY) / (double)(mHeight - mThumbImage.GetHeight());
			}
			if (mVal < 0.0)
			{
				mVal = 0.0;
			}
			if (mVal > 1.0)
			{
				mVal = 1.0;
			}
			if (mVal != num)
			{
				mListener.SliderVal(mId, mVal);
				MarkDirtyFull();
			}
		}
	}

	public override void MouseUp(int x, int y, int theMagicCode)
	{
		mDragging = false;
		GlobalStaticVars.gSexyAppBase.SetCursor(SexyAppBase.CURSOR_POINTER);
		mListener.SliderVal(mId, mVal);
	}

	public override void MouseLeave()
	{
		if (!mDragging)
		{
			GlobalStaticVars.gSexyAppBase.SetCursor(SexyAppBase.CURSOR_POINTER);
		}
	}
}
