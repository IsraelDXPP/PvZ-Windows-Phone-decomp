using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sexy;

namespace Sexy.TodLib
{
	public enum FilterEffect
	{
		FILTER_EFFECT_WASHED_OUT,
		FILTER_EFFECT_LESS_WASHED_OUT,
		FILTER_EFFECT_WHITE,
		NUM_FILTER_EFFECTS
	}

	internal static class FilterEffectHelper
	{
		private static Dictionary<Image, Image>[] gFilterMap;

		private static void RGB_to_HSL(float r, float g, float b, out float h, out float s, out float l)
		{
			float maxval = r > g ? r : g;
			maxval = maxval > b ? maxval : b;
			float minval = r < g ? r : g;
			minval = minval < b ? minval : b;
			l = (minval + maxval) / 2f;
			h = 0f;
			s = 0f;
			if (l <= 0f)
			{
				return;
			}
			float delta = maxval - minval;
			s = delta;
			if (s <= 0f)
			{
				return;
			}
			s /= ((l <= 0.5f) ? (minval + maxval) : (2f - minval - maxval));
			float r2 = (maxval - r) / delta;
			float g2 = (maxval - g) / delta;
			float b2 = (maxval - b) / delta;
			if (maxval == r)
			{
				h = ((g == minval) ? (5f + b2) : (1f - g2));
			}
			else if (maxval == g)
			{
				h = ((b == minval) ? (1f + r2) : (3f - b2));
			}
			else
			{
				h = ((r == minval) ? (3f + g2) : (5f - r2));
			}
			h /= 6f;
		}

		private static void HSL_to_RGB(float h, float sl, float l, out float r, out float g, out float b)
		{
			float v = (l <= 0.5f) ? (l * (1f + sl)) : (l + sl - l * sl);
			if (v <= 0f)
			{
				r = 0f;
				g = 0f;
				b = 0f;
				return;
			}
			float y = 2f * l - v;
			float sv = (v - y) / v;
			h *= 6f;
			int sextant = TodCommon.ClampInt((int)h, 0, 5);
			float vsf = v * sv * (h - sextant);
			float x = y + vsf;
			float z = v - vsf;
			switch (sextant)
			{
			case 0: r = v; g = x; b = y; break;
			case 1: r = z; g = v; b = y; break;
			case 2: r = y; g = v; b = x; break;
			case 3: r = y; g = z; b = v; break;
			case 4: r = x; g = y; b = v; break;
			default: r = v; g = y; b = z; break;
			}
		}

		private static void DoLumSat(Color[] px, float theLum, float theSat)
		{
			for (int i = 0; i < px.Length; i++)
			{
				Color c = px[i];
				float r = c.R / 255f;
				float g = c.G / 255f;
				float b = c.B / 255f;
				float h, s, l;
				RGB_to_HSL(r, g, b, out h, out s, out l);
				s *= theSat;
				l *= theLum;
				HSL_to_RGB(h, s, l, out r, out g, out b);
				px[i] = new Color((byte)TodCommon.ClampInt((int)(r * 255f), 0, 255), (byte)TodCommon.ClampInt((int)(g * 255f), 0, 255), (byte)TodCommon.ClampInt((int)(b * 255f), 0, 255), c.A);
			}
		}

		public static Image FilterEffectGetImage(Image theImage, FilterEffect theFilterEffect)
		{
			if (gFilterMap == null)
			{
				gFilterMap = new Dictionary<Image, Image>[(int)FilterEffect.NUM_FILTER_EFFECTS];
				for (int i = 0; i < gFilterMap.Length; i++)
				{
					gFilterMap[i] = new Dictionary<Image, Image>();
				}
			}
			Dictionary<Image, Image> map = gFilterMap[(int)theFilterEffect];
			Image cached;
			if (map.TryGetValue(theImage, out cached))
			{
				return cached;
			}
			Texture2D src = theImage.Texture;
			Color[] px = new Color[src.Width * src.Height];
			src.GetData(px);
			if (theFilterEffect == FilterEffect.FILTER_EFFECT_WASHED_OUT)
			{
				DoLumSat(px, 1.8f, 0.2f);
			}
			else if (theFilterEffect == FilterEffect.FILTER_EFFECT_LESS_WASHED_OUT)
			{
				DoLumSat(px, 1.2f, 0.3f);
			}
			else
			{
				for (int i = 0; i < px.Length; i++)
				{
					Color c = px[i];
					px[i] = new Color((byte)255, (byte)255, (byte)255, c.A);
				}
			}
			GraphicsDevice gd = GlobalStaticVars.g.GraphicsDevice;
			Texture2D tex = new Texture2D(gd, src.Width, src.Height);
			tex.SetData(px);
			Image img = new Image(tex, 0, 0, tex.Width, tex.Height);
			img.mNumRows = theImage.mNumRows;
			img.mNumCols = theImage.mNumCols;
			map[theImage] = img;
			return img;
		}
	}
}
