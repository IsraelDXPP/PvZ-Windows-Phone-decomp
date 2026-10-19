using System.Collections.Generic;
using Sexy;

namespace Lawn;

internal class ToolTipWidget : Widget
{
	public string mLabel = string.Empty;

	public string mTitle = string.Empty;

	public string mWarningText = string.Empty;

	public bool mCenter;

	public int mMinLeft;

	public int mMaxBottom;

	public int mGetsLinesWidth;

	public int mWarningFlashCounter;

	public ToolTipWidget()
	{
		mX = 0;
		mY = 0;
		mWidth = 0;
		mHeight = 0;
		mVisible = true;
		mCenter = false;
		mMinLeft = 0;
		mMaxBottom = Constants.BOARD_HEIGHT;
		mGetsLinesWidth = 0;
		mWarningFlashCounter = 0;
	}

	public void GetLines(List<string> theLines)
	{
		int aLineWidth = 0;
		int aIndexStart = 0;
		int aIndexInLine = 0;
		Font aFont = Resources.FONT_BRIANNETOD12;
		string aLabel = Sexy.TodLib.TodStringFile.TodStringTranslate(mLabel);
		while (aIndexInLine != aLabel.Length)
		{
			while (aIndexInLine < aLabel.Length && aLabel[aIndexInLine] != ' ' && aLabel[aIndexInLine] != '\n')
			{
				aLineWidth += aFont.CharWidth(aLabel[aIndexInLine]);
				aIndexInLine++;
			}
			if (aIndexInLine != aLabel.Length && aLineWidth < mGetsLinesWidth && aLabel[aIndexInLine] != '\n')
			{
				aLineWidth += aFont.CharWidth(aLabel[aIndexInLine]);
				aIndexInLine++;
			}
			else
			{
				string aLine = aLabel.Substring(aIndexStart, aIndexInLine - aIndexStart);
				aLineWidth = 0;
				theLines.Add(aLine);
				if (aIndexInLine < aLabel.Length && aLabel[aIndexInLine] == '\n')
				{
					aIndexInLine++;
				}
				while (aIndexInLine < aLabel.Length && aLabel[aIndexInLine] == ' ')
				{
					aIndexInLine++;
				}
				aIndexStart = aIndexInLine;
			}
		}
	}

	public void CalculateSize()
	{
		List<string> aLines = new List<string>();
		string aTitle = Sexy.TodLib.TodStringFile.TodStringTranslate(mTitle);
		string aWarningText = Sexy.TodLib.TodStringFile.TodStringTranslate(mWarningText);
		Font aTitleFont = Resources.FONT_BRIANNETOD16;
		Font aFont = Resources.FONT_BRIANNETOD12;
		int aTitleWidth = aTitleFont.StringWidth(aTitle);
		int aWarningWidth = aFont.StringWidth(aWarningText);
		int aMaxWidth = aTitleWidth > aWarningWidth ? aTitleWidth : aWarningWidth;
		mGetsLinesWidth = aMaxWidth - 30 > 100 ? aMaxWidth - 30 : 100;
		GetLines(aLines);
		for (int i = 0; i < aLines.Count; i++)
		{
			int aLineWidth = aFont.StringWidth(aLines[i]);
			if (aLineWidth > aMaxWidth)
			{
				aMaxWidth = aLineWidth;
			}
		}
		int aHeight = 6;
		if (aTitle.Length != 0)
		{
			aHeight = aTitleFont.GetAscent() + 8;
		}
		if (aWarningText.Length != 0)
		{
			aHeight += aFont.GetAscent() + 2;
		}
		aHeight += aLines.Count * aFont.GetAscent();
		mWidth = aMaxWidth + 10;
		mHeight = aHeight + aLines.Count * 2 - 2;
	}

	public void SetLabel(string theLabel)
	{
		mLabel = theLabel;
		CalculateSize();
	}

	public void SetTitle(string theTitle)
	{
		mTitle = theTitle;
		CalculateSize();
	}

	public void SetWarningText(string theWarningText)
	{
		mWarningText = theWarningText;
		CalculateSize();
	}

	public void FlashWarning()
	{
		mWarningFlashCounter = 70;
	}

	public void Update()
	{
		if (mWarningFlashCounter > 0)
		{
			mWarningFlashCounter--;
		}
	}

	public void SetPosition(int theX, int theY)
	{
		mX = theX;
		mY = theY;
	}

	public override void Draw(Graphics g)
	{
		if (!mVisible)
		{
			return;
		}
		int aPosX = mX;
		if (mCenter)
		{
			aPosX -= mWidth / 2;
		}
		if (mMinLeft - g.mTransX > aPosX)
		{
			aPosX = mMinLeft - g.mTransX;
		}
		else if (aPosX + mWidth + g.mTransX > Constants.BOARD_WIDTH)
		{
			aPosX = Constants.BOARD_WIDTH - g.mTransX - mWidth;
		}
		int aPosY = mY;
		if (-g.mTransY > aPosY)
		{
			aPosY = -g.mTransY;
		}
		else if (mMaxBottom < mY + mHeight + g.mTransY)
		{
			aPosY = mMaxBottom - g.mTransY - mHeight;
		}
		g.SetColor(new SexyColor(255, 255, 200, 255));
		g.FillRect(aPosX, aPosY, mWidth, mHeight);
		g.SetColor(SexyColor.Black);
		g.DrawRect(aPosX, aPosY, mWidth - 1, mHeight - 1);
		aPosY++;
		Font aTitleFont = Resources.FONT_BRIANNETOD16;
		string aTitle = Sexy.TodLib.TodStringFile.TodStringTranslate(mTitle);
		if (aTitle.Length != 0)
		{
			g.SetFont(aTitleFont);
			g.DrawString(aTitle, aPosX + (mWidth - aTitleFont.StringWidth(aTitle)) / 2, aPosY + aTitleFont.GetAscent());
			aPosY += aTitleFont.GetAscent() + 2;
		}
		Font aWarningFont = Resources.FONT_BRIANNETOD12;
		string aWarningText = Sexy.TodLib.TodStringFile.TodStringTranslate(mWarningText);
		if (aWarningText.Length != 0)
		{
			g.SetFont(aWarningFont);
			int x = aPosX + (mWidth - aWarningFont.StringWidth(aWarningText)) / 2;
			int y = aPosY + aWarningFont.GetAscent();
			SexyColor aWarningColor = new SexyColor(255, 0, 0);
			if (mWarningFlashCounter > 0 && mWarningFlashCounter % 20 < 10)
			{
				aWarningColor = new SexyColor(0, 0, 0);
			}
			g.SetColor(aWarningColor);
			g.DrawString(aWarningText, x, y);
			g.SetColor(SexyColor.Black);
			aPosY += aWarningFont.GetAscent() + 2;
		}
		List<string> aLines = new List<string>();
		GetLines(aLines);
		Font aFont = Resources.FONT_BRIANNETOD12;
		g.SetFont(aFont);
		for (int i = 0; i < aLines.Count; i++)
		{
			string aLine = aLines[i];
			g.DrawString(aLine, aPosX + (mWidth - aFont.StringWidth(aLine)) / 2, aPosY + aFont.GetAscent());
			aPosY += aFont.GetAscent() + 2;
		}
	}
}
