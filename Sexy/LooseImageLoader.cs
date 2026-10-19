using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sexy
{
	// Sustituye a AtlasResources: en la version sin atlas (referencia: Resources.cpp del mod)
	// no hay Mammoth ni atlas por resolucion, solo imagenes sueltas declaradas en el
	// resources.xml. Este helper cubre el segundo intento de DefinitionLoadImage, que
	// resuelve una imagen por su nombre de archivo en Content/images en vez de por recurso.
	internal static class LooseImageLoader
	{
		private static readonly HashSet<string> sLooseMissingLogged = new HashSet<string>();

		public static Image Load(string baseName, int rows, int cols)
		{
			try
			{
				string rel = null;
				foreach (string ext in new string[2] { "png", "jpg" })
				{
					string cand = ("Content/images" + Constants.ImageSubPath + baseName + "." + ext).Replace('\\', '/');
					try
					{
						using (Stream s = TitleContainer.OpenStream(cand))
						{
						}
						rel = cand;
						break;
					}
					catch
					{
					}
				}
				if (rel == null)
				{
					if (sLooseMissingLogged.Add(baseName))
					{
						SexyAppBase.LoadLog("LooseImageLoader sin archivo: " + baseName);
					}
					return null;
				}
				GraphicsDevice gd = GlobalStaticVars.g.GraphicsDevice;
				Texture2D tex;
				using (Stream stream = TitleContainer.OpenStream(rel))
				{
					tex = Texture2D.FromStream(gd, stream);
				}
				Color[] px = new Color[tex.Width * tex.Height];
				tex.GetData(px);
				for (int i = 0; i < px.Length; i++)
				{
					Color c = px[i];
					c.R = (byte)((float)(c.R * c.A) / 255f);
					c.G = (byte)((float)(c.G * c.A) / 255f);
					c.B = (byte)((float)(c.B * c.A) / 255f);
					px[i] = c;
				}
				tex.SetData(px);
				Image img = new Image(tex, 0, 0, tex.Width, tex.Height);
				img.mNumRows = rows;
				img.mNumCols = cols;
				return img;
			}
			catch (Exception ex)
			{
				if (sLooseMissingLogged.Add(baseName))
				{
					SexyAppBase.LoadLog("LooseImageLoader fallo: " + baseName + " " + ex.Message);
				}
				return null;
			}
		}
	}
}