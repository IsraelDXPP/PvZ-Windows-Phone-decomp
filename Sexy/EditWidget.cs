using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Sexy.TodLib;

namespace Sexy;

internal class EditWidget : Widget
{
	public enum Colors
	{
		COLOR_BKG,
		COLOR_OUTLINE,
		COLOR_TEXT,
		COLOR_HILITE,
		COLOR_HILITE_TEXT,
		NUM_COLORS
	}

	internal static int[,] gEditWidgetColors = new int[5, 3]
	{
		{ 0, 0, 0 },
		{ 0, 0, 0 },
		{ 250, 250, 250 },
		{ 0, 0, 0 },
		{ 255, 255, 255 }
	};

	public int mId;

	public string mString;

	public string mPasswordDisplayString;

	public Font mFont;

	public EditListener mEditListener;

	public int mMaxChars;

	public string mPasswordChar;

	public bool mEditing;

	public bool mAcceptsEmptyText;

	public string mTitle;

	public string mDescription;

	private bool callbackDone;

	private bool inputCancelled;

	private int mCaret;

	private string SafeText()
	{
		return mString ?? string.Empty;
	}

	private void ClampCaret()
	{
		int num = SafeText().Length;
		if (mCaret < 0)
		{
			mCaret = 0;
		}
		if (mCaret > num)
		{
			mCaret = num;
		}
	}

	public EditWidget(int theId, EditListener theEditListener, string title, string description)
	{
		mTitle = TodStringFile.TodStringTranslate(title);
		mDescription = TodStringFile.TodStringTranslate(description);
		mId = theId;
		mEditListener = theEditListener;
		mFont = null;
		mMaxChars = -1;
		mPasswordChar = " ";
		mEditing = false;
		mAcceptsEmptyText = false;
		mCaret = 0;
		mFont = new Font();
		SetColors(gEditWidgetColors, 5);
	}

	protected string GetDisplayString()
	{
		if (mPasswordChar == " ")
		{
			return mString;
		}
		if (mPasswordDisplayString.Length != mString.Length)
		{
			mPasswordDisplayString = mPasswordDisplayString + mString.Length + mPasswordChar;
		}
		return mPasswordDisplayString;
	}

	public virtual void SetFont(Font theFont)
	{
		mFont.Dispose();
		mFont = theFont.Duplicate();
	}

	public virtual void SetText(string theText)
	{
		mString = theText;
		mCaret = SafeText().Length;
		MarkDirty();
	}

	public override void Resize(TRect frame)
	{
		base.Resize(frame);
	}

	public override void Resize(int theX, int theY, int theWidth, int theHeight)
	{
		base.Resize(theX, theY, theWidth, theHeight);
		RehupBounds();
	}

	public override void Draw(Graphics g)
	{
		// OJO: DrawAll ya transladó g al origen de este widget, asi que aqui
		// todo va en coordenadas LOCALES (usar mX/mY duplicaba el offset y el
		// texto quedaba fuera del editbox, recortado por el clip).
		// El IMAGE_EDITBOX es claro, asi que el texto va en negro (el blanco
		// de gEditWidgetColors sobre fondo blanco era invisible).
		if (mFont == null)
		{
			return;
		}
		string text = SafeText();
		int padX = (int)Constants.InvertAndScale(4f);
		int padY = (int)Constants.InvertAndScale(2f);
		g.SetFont(mFont);
		g.SetColor(new SexyColor(0, 0, 0));
		g.DrawString(text, padX, padY);
		ClampCaret();
		// Caret con parpadeo (~2 Hz): indica donde se esta escribiendo.
		if (mHasFocus && (mUpdateCnt / 20) % 2 == 0)
		{
			int caretX = padX;
			if (mCaret > 0)
			{
				caretX += mFont.StringWidth(text.Substring(0, Math.Min(mCaret, text.Length)));
			}
			int caretH = mFont.GetLineSpacing();
			g.SetColor(new SexyColor(0, 0, 0));
			g.FillRect(caretX, padY, (int)Constants.InvertAndScale(2f), caretH);
		}
	}

	public override void Update()
	{
		if (callbackDone)
		{
			DoKeyboardCallback();
		}
		base.Update();
	}

	public override bool WantsFocus()
	{
		return true;
	}

	public override void GotFocus()
	{
		base.GotFocus();
		ClampCaret();
		try
		{
			// En escritorio el teclado virtual de la Guide no existe: solo
			// tiene sentido en Xbox. Cualquier fallo aqui no debe tumbar el
			// campo de texto.
			if (Guide.IsVisible)
			{
				return;
			}
			Guide.BeginShowKeyboardInput(PlayerIndex.One, mTitle, mDescription, mString, KeyboardCallback, null);
		}
		catch
		{
		}
	}

	public override void KeyChar(SexyChar theChar)
	{
		base.KeyChar(theChar);
		char c = theChar.value_type;
		if (c < ' ' || c > '~')
		{
			return;
		}
		if (mEditListener != null && !mEditListener.AllowChar(mId, c))
		{
			return;
		}
		string text = SafeText();
		if (mMaxChars >= 0 && text.Length >= mMaxChars)
		{
			return;
		}
		ClampCaret();
		mString = text.Insert(mCaret, c.ToString());
		mCaret++;
		mEditing = true;
		MarkDirty();
	}

	public override void KeyDown(KeyCode theKey)
	{
		base.KeyDown(theKey);
		string text = SafeText();
		ClampCaret();
		switch (theKey)
		{
		case KeyCode.KEYCODE_BACK:
			if (mCaret > 0 && text.Length > 0)
			{
				mString = text.Remove(mCaret - 1, 1);
				mCaret--;
			}
			break;
		case KeyCode.KEYCODE_DELETE:
			if (mCaret < text.Length)
			{
				mString = text.Remove(mCaret, 1);
			}
			break;
		case KeyCode.KEYCODE_LEFT:
			if (mCaret > 0)
			{
				mCaret--;
			}
			break;
		case KeyCode.KEYCODE_RIGHT:
			if (mCaret < text.Length)
			{
				mCaret++;
			}
			break;
		case KeyCode.KEYCODE_HOME:
			mCaret = 0;
			break;
		case KeyCode.KEYCODE_END:
			mCaret = text.Length;
			break;
		case KeyCode.KEYCODE_RETURN:
			mCaret = text.Length;
			EditingEnded(SafeText());
			return;
		default:
			return;
		}
		mEditing = true;
		MarkDirty();
	}

	private void KeyboardCallback(IAsyncResult result)
	{
		string text = Guide.EndShowKeyboardInput(result);
		inputCancelled = text == null;
		if (!inputCancelled)
		{
			mString = text;
		}
		callbackDone = true;
	}

	private void DoKeyboardCallback()
	{
		callbackDone = false;
		if (mString == null)
		{
			LostFocus();
			EditingEnded(mString);
		}
		else
		{
			EditingEnded(mString);
		}
	}

	public override void LostFocus()
	{
		base.LostFocus();
	}

	public virtual void RehupBounds()
	{
	}

	public virtual void EditingEnded(string theString)
	{
		mEditing = false;
		mString = theString;
		mEditListener.EditWidgetText(mId + (inputCancelled ? 1000 : 0), mString);
	}

	public virtual bool ShouldChangeCharacters(int theRangeStart, int theRangeLength, string theReplacementChars)
	{
		return true;
	}

	public virtual bool ShouldClear()
	{
		bool flag = mEditListener.ShouldClear();
		if (flag)
		{
			mString = "";
		}
		return flag;
	}

	public override void Dispose()
	{
		mFont = null;
	}
}
