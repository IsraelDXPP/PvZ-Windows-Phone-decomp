using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sexy;

namespace Lawn;

internal class PoolEffect
{
	public const int CAUSTIC_IMAGE_WIDTH = 128;

	public const int CAUSTIC_IMAGE_HEIGHT = 64;

	private const int GRID_SQUARES_X = 15;

	private const int GRID_SQUARES_Y = 5;

	private const int NUM_TRIANGLES = GRID_SQUARES_X * GRID_SQUARES_Y * 2;

	private const int NUM_VERTEX_LAYERS = 3;

	private static readonly SamplerState sPoolWrapSamplerState = new SamplerState
	{
		AddressU = TextureAddressMode.Wrap,
		AddressV = TextureAddressMode.Wrap,
		AddressW = TextureAddressMode.Wrap,
		Filter = TextureFilter.Linear
	};

	public byte[] mCausticGrayscaleImage;

	public MemoryImage mCausticImage;

	public LawnApp mApp;

	public int mPoolCounter;

	private uint[] mCausticBits;

	private TriVertex[][,] mVertArray;

	public void PoolEffectInitialize()
	{
		mApp = GlobalStaticVars.gLawnApp;

		mCausticImage = new MemoryImage();
		mCausticImage.Create(CAUSTIC_IMAGE_WIDTH, CAUSTIC_IMAGE_HEIGHT);
		mCausticBits = new uint[CAUSTIC_IMAGE_WIDTH * CAUSTIC_IMAGE_HEIGHT];
		for (int i = 0; i < mCausticBits.Length; i++)
		{
			mCausticBits[i] = uint.MaxValue;
		}
		UploadCausticImage();

		mVertArray = new TriVertex[NUM_VERTEX_LAYERS][,];
		for (int aLayer = 0; aLayer < NUM_VERTEX_LAYERS; aLayer++)
		{
			mVertArray[aLayer] = new TriVertex[NUM_TRIANGLES, 3];
		}

		mCausticGrayscaleImage = new byte[256 * 256];
		Color[] aCausticGrayscaleImage = new Color[256 * 256];
		Resources.IMAGE_POOL_CAUSTIC_EFFECT.Texture.GetData(aCausticGrayscaleImage);
		int index = 0;
		for (int x = 0; x < 256; x++)
		{
			for (int y = 0; y < 256; y++)
			{
				mCausticGrayscaleImage[index] = aCausticGrayscaleImage[index].B;
				index++;
			}
		}
	}

	public void PoolEffectDispose()
	{
		mCausticImage?.Dispose();
		mCausticImage = null;
		mCausticGrayscaleImage = null;
		mCausticBits = null;
		mVertArray = null;
	}

	private void UploadCausticImage()
	{
		GlobalStaticVars.g.GraphicsDevice.Textures[0] = null;
		mCausticImage.RenderTarget.SetData(mCausticBits);
	}

	public uint BilinearLookupFixedPoint(uint u, uint v)
	{
		uint timeU = u & 0xFFFF0000u;
		uint timeV = v & 0xFFFF0000u;
		uint factorU1 = ((u - timeU) & 0x0000FFFEu) + 1u;
		uint factorV1 = ((v - timeV) & 0x0000FFFEu) + 1u;
		uint factorU0 = 65536u - factorU1;
		uint factorV0 = 65536u - factorV1;
		uint indexU0 = (timeU >> 16) % 256u;
		uint indexU1 = ((timeU >> 16) + 1u) % 256u;
		uint indexV0 = (timeV >> 16) % 256u;
		uint indexV1 = ((timeV >> 16) + 1u) % 256u;

		return (((factorU0 * factorV1) / 65536u) * mCausticGrayscaleImage[indexV1 * 256u + indexU0]) / 65536u
			+ (((factorU1 * factorV1) / 65536u) * mCausticGrayscaleImage[indexV1 * 256u + indexU1]) / 65536u
			+ (((factorU0 * factorV0) / 65536u) * mCausticGrayscaleImage[indexV0 * 256u + indexU0]) / 65536u
			+ (((factorU1 * factorV0) / 65536u) * mCausticGrayscaleImage[indexV0 * 256u + indexU1]) / 65536u;
	}

	public void UpdateWaterEffect(Graphics g)
	{
		int idx = 0;
		for (int y = 0; y < CAUSTIC_IMAGE_HEIGHT; y++)
		{
			int timeV1 = (256 - y) << 17;
			int timeV0 = y << 17;

			for (int x = 0; x < CAUSTIC_IMAGE_WIDTH; x++)
			{
				int timeU = x << 17;
				int timePool0 = mPoolCounter << 16;
				int timePool1 = ((mPoolCounter & 65535) + 1) << 16;
				int a1 = (int)(byte)BilinearLookupFixedPoint((uint)(timeU - timePool1 / 6), (uint)(timeV1 + timePool0 / 8));
				int a0 = (int)(byte)BilinearLookupFixedPoint((uint)(timeU + timePool0 / 10), (uint)timeV0);
				byte a = (byte)((a0 + a1) / 2);

				byte alpha;
				if (a >= 160u)
				{
					alpha = (byte)(255 - 2 * (a - 160u));
				}
				else if (a >= 128u)
				{
					alpha = (byte)(5 * (a - 128u));
				}
				else
				{
					alpha = 0;
				}

				mCausticBits[idx] = (mCausticBits[idx] & 0x00FFFFFFu) + (uint)((int)alpha / 3 << 24);
				idx++;
			}
		}

		UploadCausticImage();
	}

	public void PoolEffectDraw(Graphics g, bool theIsNight)
	{
		if (!mApp.Is3DAccelerated())
		{
			if (theIsNight)
			{
				g.DrawImage(Resources.IMAGE_POOL_NIGHT, 34, 278);
			}
			else
			{
				g.DrawImage(Resources.IMAGE_POOL, 34, 278);
			}
			return;
		}

		float aGridSquareX = Resources.IMAGE_POOL.GetWidth() / 15.0f;
		float aGridSquareY = Resources.IMAGE_POOL.GetHeight() / 5.0f;
		float[,,,] aOffsetArray = new float[NUM_VERTEX_LAYERS, GRID_SQUARES_X + 1, GRID_SQUARES_Y + 1, 2];
		for (int x = 0; x <= GRID_SQUARES_X; x++)
		{
			for (int y = 0; y <= GRID_SQUARES_Y; y++)
			{
				aOffsetArray[2, x, y, 0] = x / 15.0f;
				aOffsetArray[2, x, y, 1] = y / 5.0f;
				if (x != 0 && x != GRID_SQUARES_X && y != 0 && y != GRID_SQUARES_Y)
				{
					float aPoolPhase = mPoolCounter * 2 * (float)Math.PI;
					float aWaveTime1 = aPoolPhase / 800.0f;
					float aWaveTime2 = aPoolPhase / 150.0f;
					float aWaveTime3 = aPoolPhase / 900.0f;
					float aWaveTime4 = aPoolPhase / 800.0f;
					float aWaveTime5 = aPoolPhase / 110.0f;
					float xPhase = x * 3.0f * 2 * (float)Math.PI / 15.0f;
					float yPhase = y * 3.0f * 2 * (float)Math.PI / 5.0f;

					aOffsetArray[0, x, y, 0] = (float)Math.Sin(yPhase + aWaveTime2) * 0.002f + (float)Math.Sin(yPhase + aWaveTime1) * 0.005f;
					aOffsetArray[0, x, y, 1] = (float)Math.Sin(xPhase + aWaveTime5) * 0.01f + (float)Math.Sin(xPhase + aWaveTime3) * 0.015f + (float)Math.Sin(xPhase + aWaveTime4) * 0.005f;
					aOffsetArray[1, x, y, 0] = (float)Math.Sin(yPhase * 0.2f + aWaveTime2) * 0.015f + (float)Math.Sin(yPhase * 0.2f + aWaveTime1) * 0.012f;
					aOffsetArray[1, x, y, 1] = (float)Math.Sin(xPhase * 0.2f + aWaveTime5) * 0.005f + (float)Math.Sin(xPhase * 0.2f + aWaveTime3) * 0.015f + (float)Math.Sin(xPhase * 0.2f + aWaveTime4) * 0.02f;
					aOffsetArray[2, x, y, 0] += (float)Math.Sin(yPhase + aWaveTime1 * 1.5f) * 0.004f + (float)Math.Sin(yPhase + aWaveTime2 * 1.5f) * 0.005f;
					aOffsetArray[2, x, y, 1] += (float)Math.Sin(xPhase * 4.0f + aWaveTime5 * 2.5f) * 0.005f + (float)Math.Sin(xPhase * 2.0f + aWaveTime3 * 2.5f) * 0.04f + (float)Math.Sin(xPhase * 3.0f + aWaveTime4 * 2.5f) * 0.02f;
				}
				else
				{
					aOffsetArray[0, x, y, 0] = 0.0f;
					aOffsetArray[0, x, y, 1] = 0.0f;
					aOffsetArray[1, x, y, 0] = 0.0f;
					aOffsetArray[1, x, y, 1] = 0.0f;
				}
			}
		}

		int[] aIndexOffsetX = new int[6] { 0, 0, 1, 0, 1, 1 };
		int[] aIndexOffsetY = new int[6] { 0, 1, 1, 0, 1, 0 };

		for (int x = 0; x < GRID_SQUARES_X; x++)
		{
			for (int y = 0; y < GRID_SQUARES_Y; y++)
			{
				for (int aLayer = 0; aLayer < NUM_VERTEX_LAYERS; aLayer++)
				{
					TriVertex[,] aLayerVerts = mVertArray[aLayer];
					int aTriIndex = x * 10 + y * 2;
					for (int aVertIndex = 0; aVertIndex < 6; aVertIndex++)
					{
						int aIndexX = x + aIndexOffsetX[aVertIndex];
						int aIndexY = y + aIndexOffsetY[aVertIndex];
						if (aLayer == 2)
						{
							float aVertX = (704.0f / 15.0f) * aIndexX + 45.0f;
							float aVertY = 30.0f * aIndexY + 288.0f;
							float aU = aOffsetArray[2, aIndexX, aIndexY, 0] + aIndexX / 15.0f;
							float aV = aOffsetArray[2, aIndexX, aIndexY, 1] + aIndexY / 5.0f;
							Color aColor;
							if (!ClipRectContains(g, aVertX, aVertY))
							{
								aColor = new Color(255, 255, 255, 0);
							}
							else if (aIndexX == 0 || aIndexX == GRID_SQUARES_X || aIndexY == 0)
							{
								aColor = new Color(255, 255, 255, 0x20);
							}
							else if (theIsNight)
							{
								aColor = new Color(255, 255, 255, 0x30);
							}
							else
							{
								aColor = new Color(255, 255, 255, aIndexX <= 7 ? 0xC0 : 0x80);
							}
							aLayerVerts[aTriIndex, aVertIndex] = new TriVertex(aVertX, aVertY, aU, aV, aColor);
						}
						else
						{
							float aVertX = aIndexX * aGridSquareX + 35.0f;
							float aVertY = aIndexY * aGridSquareY + 279.0f;
							float aU = aOffsetArray[aLayer, aIndexX, aIndexY, 0] + aIndexX / 15.0f;
							float aV = aOffsetArray[aLayer, aIndexX, aIndexY, 1] + aIndexY / 5.0f;
							Color aColor = new Color(255, 255, 255, 255);
							if (!ClipRectContains(g, aVertX, aVertY))
							{
								aColor = new Color(255, 255, 255, 0);
							}
							aLayerVerts[aTriIndex, aVertIndex] = new TriVertex(aVertX, aVertY, aU, aV, aColor);
						}
					}
				}
			}
		}

		if (theIsNight)
		{
			g.DrawTrianglesTex(Resources.IMAGE_POOL_BASE_NIGHT, mVertArray[0], NUM_TRIANGLES);
			g.DrawTrianglesTex(Resources.IMAGE_POOL_SHADING_NIGHT, mVertArray[1], NUM_TRIANGLES);
		}
		else
		{
			g.DrawTrianglesTex(Resources.IMAGE_POOL_BASE, mVertArray[0], NUM_TRIANGLES);
			g.DrawTrianglesTex(Resources.IMAGE_POOL_SHADING, mVertArray[1], NUM_TRIANGLES);
		}

		UpdateWaterEffect(g);
		g.DrawTrianglesTex(sPoolWrapSamplerState, mCausticImage, mVertArray[2], NUM_TRIANGLES);
	}

	private static bool ClipRectContains(Graphics g, float theX, float theY)
	{
		TRect aClipRect = g.mClipRect;
		return theX >= aClipRect.mX && theX < aClipRect.mX + aClipRect.mWidth && theY >= aClipRect.mY && theY < aClipRect.mY + aClipRect.mHeight;
	}

	public void PoolEffectUpdate()
	{
		++mPoolCounter;
	}
}
