using Sexy;

namespace Lawn;

internal class CursorWidget : Widget
{
	public LawnApp mApp;

	public Image mImage;

	public bool mDraw;

	public CursorWidget(LawnApp theApp)
	{
		mApp = theApp;
		mMouseVisible = false;
		mImage = null;
		mDraw = false;
	}

	public override void Draw(Graphics g)
	{
		if (mImage != null && mDraw)
		{
			g.DrawImage(mImage, 0, 0);
		}
	}

	public void SetImage(Image theImage)
	{
		mImage = theImage;
		if (mImage != null)
		{
			Resize(mX, mY, theImage.mWidth, theImage.mHeight);
		}
	}

	public override void Update()
	{
		mX = mApp.mWidgetManager.mLastMouseX;
		mY = mApp.mWidgetManager.mLastMouseY;
		mDraw = mApp.mCursorNum != SexyAppBase.CURSOR_NONE && mApp.mCustomCursor;
	}
}
