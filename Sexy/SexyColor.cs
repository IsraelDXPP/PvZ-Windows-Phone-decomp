using Microsoft.Xna.Framework;

namespace Sexy;

public struct SexyColor
{
	public Color Color;

	private static SexyColor mWhite;

	private static SexyColor mBlack;

	public int mRed
	{
		get
		{
			return Color.R;
		}
		set
		{
			Color.R = (byte)value;
		}
	}

	public int mGreen
	{
		get
		{
			return Color.G;
		}
		set
		{
			Color.G = (byte)value;
		}
	}

	public int mBlue
	{
		get
		{
			return Color.B;
		}
		set
		{
			Color.B = (byte)value;
		}
	}

	public int mAlpha
	{
		get
		{
			return Color.A;
		}
		set
		{
			float num = (float)value / 255f;
			Color *= num;
			Color.A = (byte)value;
		}
	}

	public static SexyColor Black => mBlack;

	public static SexyColor White => mWhite;

	public int this[int theIdx] => theIdx switch
	{
		0 => Color.R, 
		1 => Color.G, 
		2 => Color.B, 
		3 => Color.A, 
		_ => 0, 
	};

	public static SexyColor Premultiply(SexyColor col)
	{
		col.mRed = (int)((float)(col.mRed * col.mAlpha) / 255f);
		col.mGreen = (int)((float)(col.mGreen * col.mAlpha) / 255f);
		col.mBlue = (int)((float)(col.mBlue * col.mAlpha) / 255f);
		return col;
	}

	public void PremultiplyAlpha()
	{
		mRed = (int)((float)(mRed * mAlpha) / 255f);
		mGreen = (int)((float)(mGreen * mAlpha) / 255f);
		mBlue = (int)((float)(mBlue * mAlpha) / 255f);
	}

	public SexyColor(int theRed, int theGreen, int theBlue)
	{
		Color = new Color(theRed, theGreen, theBlue);
	}

	public SexyColor(int theRed, int theGreen, int theBlue, int theAlpha)
	{
		this = new SexyColor(theRed, theGreen, theBlue, theAlpha, premultiply: true);
	}

	public SexyColor(int theRed, int theGreen, int theBlue, int theAlpha, bool premultiply)
	{
		Color = new Color(theRed, theGreen, theBlue, theAlpha);
		if (premultiply)
		{
			Color = Color.Multiply(Color, (float)theAlpha / 255f);
			Color.A = (byte)theAlpha;
		}
	}

	public SexyColor(string theElements)
	{
		Color = new Color(theElements[0], theElements[1], theElements[2], 255);
	}

	public SexyColor(Color theColor)
	{
		Color = theColor;
	}

	public static bool operator ==(SexyColor a, SexyColor b)
	{
		return a.Color == b.Color;
	}

	public static bool operator !=(SexyColor a, SexyColor b)
	{
		return !(a == b);
	}

	public override bool Equals(object obj)
	{
		if (obj is SexyColor)
		{
			return Color == ((SexyColor)obj).Color;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Color.GetHashCode();
	}

	public override string ToString()
	{
		return Color.ToString();
	}

	public static implicit operator SexyColor(Color color)
	{
		return new SexyColor
		{
			Color = color
		};
	}

	public static implicit operator Color(SexyColor aColor)
	{
		return aColor.Color;
	}

	public static SexyColor FromColor(Color c)
	{
		return new SexyColor(c);
	}

	internal void CopyFrom(Color c)
	{
		Color = c;
	}

	static SexyColor()
	{
		mWhite = new SexyColor(Color.White);
		mBlack = new SexyColor(Color.Black);
	}
}
