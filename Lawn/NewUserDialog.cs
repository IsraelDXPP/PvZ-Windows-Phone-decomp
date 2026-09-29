using Sexy;

namespace Lawn;

internal class NewUserDialog : LawnDialog, EditListener
{
	public EditWidget mNameEditWidget;

	private bool mIsRename;

	public NewUserDialog(LawnApp theApp, bool allowCancel)
		: this(theApp, isRename: false, allowCancel)
	{
	}

	public NewUserDialog(LawnApp theApp, bool isRename, bool allowCancel)
		: base(theApp, null, isRename ? 32 : 30, isModal: true, isRename ? "[RENAME_USER]" : "[NEW_USER]", "[PLEASE_ENTER_NAME]", "[DIALOG_BUTTON_OK]", allowCancel ? 2 : 3)
	{
		mIsRename = isRename;
		mApp = theApp;
		mVerticalCenterText = false;
		mNameEditWidget = LawnCommon.CreateEditWidget(0, this, this, isRename ? "[RENAME_USER]" : "[NEW_USER]", "[PLEASE_ENTER_NAME]");
		mNameEditWidget.mMaxChars = 12;
		mNameEditWidget.mAcceptsEmptyText = true;
		mNameEditWidget.SetFont(Resources.FONT_BRIANNETOD16);
		// Alto extra generoso: en el original el campo vivia en el teclado
		// virtual de la Guide y el dialogo era bajo. Aqui hay que dejar sitio
		// para el campo de texto Y para los botones de abajo.
		CalcSize((int)Constants.InvertAndScale(110f), (int)Constants.InvertAndScale(90f));
	}

	public override int GetPreferredHeight(int theWidth)
	{
		return base.GetPreferredHeight(theWidth) + (int)Constants.InvertAndScale(40f);
	}

	public override void Resize(int theX, int theY, int theWidth, int theHeight)
	{
		base.Resize(theX, theY, theWidth, theHeight);
		// El campo se ancla ENCIMA del boton (nunca se solapa) y se centra en
		// la misma franja horizontal que los botones: antes usaba solo
		// mContentInsets y quedaba descentrado respecto al boton ACEPTAR.
		int fieldH = (int)Constants.InvertAndScale(28f);
		int gap = (int)Constants.InvertAndScale(12f);
		int margin = (int)Constants.InvertAndScale(12f);
		int regionX = mContentInsets.mLeft + mBackgroundInsets.mLeft - (int)Constants.InvertAndScale(5f);
		int regionW = mWidth - mContentInsets.mLeft - mContentInsets.mRight - mBackgroundInsets.mLeft - mBackgroundInsets.mRight + (int)Constants.InvertAndScale(8f);
		int fieldW = regionW - margin * 2;
		int fieldX = regionX + margin;
		int buttonTop = (mLawnYesButton != null) ? mLawnYesButton.mY : mHeight;
		// Justo debajo del texto "INTRODUCE TU NOMBRE:", no centrada en el
		// hueco ni pegada al boton.
		int freeTop = GetTop() + mLinesFont.GetHeight() + gap;
		int freeBottom = buttonTop - gap;
		int fieldY = freeTop;
		if (fieldY + fieldH > freeBottom)
		{
			fieldY = freeBottom - fieldH;
		}
		mNameEditWidget.Resize(fieldX, fieldY, fieldW, fieldH);
	}

	public override void AddedToManager(WidgetManager theWidgetManager)
	{
		// Sin esto no se pintan ni el fondo del dialogo ni los botones OK /
		// Cancel: LawnDialog.AddedToManager es quien los agrega.
		base.AddedToManager(theWidgetManager);
		AddWidget(mNameEditWidget);
		theWidgetManager.SetFocus(mNameEditWidget);
	}

	public override void RemovedFromManager(WidgetManager theWidgetManager)
	{
		base.RemovedFromManager(theWidgetManager);
		RemoveWidget(mNameEditWidget);
	}

	public override void Draw(Graphics g)
	{
		base.Draw(g);
		LawnCommon.DrawEditBox(g, mNameEditWidget);
	}

	public override void Update()
	{
		base.Update();
	}

	public virtual void EditWidgetText(int theId, string theString)
	{
		mApp.ButtonDepress(2000 + mId + theId);
	}

	public override void Dispose()
	{
		mNameEditWidget.Dispose();
		base.Dispose();
	}

	public string GetName()
	{
		if (mNameEditWidget.mString == null)
		{
			return string.Empty;
		}
		return mNameEditWidget.mString;
	}

	public void SetName(string theName)
	{
		mNameEditWidget.SetText(theName);
	}

	public bool AllowChar(int theId, char theChar)
	{
		// El decompile dejo esto en false (residuo del teclado virtual de la
		// Xbox), con lo que el campo rechazaba cualquier caracter.
		return theChar >= ' ' && theChar <= '~';
	}

	public bool AllowText(int theId, ref string theText)
	{
		return true;
	}

	public bool ShouldClear()
	{
		return false;
	}
}
