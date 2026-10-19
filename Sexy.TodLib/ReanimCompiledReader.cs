using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Sexy;

namespace Sexy.TodLib
{
	internal static class ReanimCompiledReader
	{
		private const uint COMPILED_MAGIC = 0xDEADFED4u;

		private const float COMPILED_PLACEHOLDER = -10000f;

		private const int EXPECTED_STRIDE = 44;

		private const int RECORD_FLOATS = 11;

		private const int MAX_BLOCKS = 4096;

		private const int MAX_RECORDS_PER_BLOCK = 200000;

		private sealed class CompiledBlock
		{
			public string Name;

			public List<float[]> Records = new List<float[]>();

			public List<string> Images = new List<string>();
		}

		public static bool LoadCompiledReanimation(string filePath, ref ReanimatorDefinition def)
		{
			try
			{
				byte[] inflated;
				float fileFps;
				if (!ReadAndInflate(filePath, out inflated, out fileFps))
				{
					return false;
				}
				List<CompiledBlock> blocks = ParseBlocks(inflated, filePath);
				if (blocks == null || blocks.Count == 0)
				{
					SexyAppBase.LoadLog("ReanimCompiled sin bloques: " + filePath);
					return false;
				}
				def = new ReanimatorDefinition();
				def.mFPS = fileFps;
				def.mReanimAtlas = null;
				int maxRecords = 0;
				foreach (CompiledBlock block in blocks)
				{
					if (block.Records.Count > maxRecords)
					{
						maxRecords = block.Records.Count;
					}
				}
				def.mTracks = new ReanimatorTrack[blocks.Count];
				def.mTrackCount = (short)blocks.Count;
				SexyAppBase.LoadLog(string.Format("ReanimCompiled {0}: {1} tracks, {2} frames", filePath, blocks.Count, maxRecords));
				for (int i = 0; i < blocks.Count; i++)
				{
					CompiledBlock block = blocks[i];
					if (block.Records.Count == 0)
					{
						block.Records.Add(new float[RECORD_FLOATS]);
					}
					if (block.Records.Count < maxRecords)
					{
						SexyAppBase.LoadLog(string.Format("ReanimCompiled pad {0} track={1} {2}->{3}", filePath, block.Name, block.Records.Count, maxRecords));
					}
					while (block.Records.Count < maxRecords)
					{
						float[] last = block.Records[block.Records.Count - 1];
						float[] clone = new float[RECORD_FLOATS];
						Array.Copy(last, clone, RECORD_FLOATS);
						block.Records.Add(clone);
					}
					ReanimatorTrack track = new ReanimatorTrack(block.Name, block.Records.Count);
					string imageName = block.Images.Count > 0 ? block.Images[0] : string.Empty;
					if (block.Images.Count > 1)
					{
						SexyAppBase.LoadLog(string.Format("ReanimCompiled multi-imagen (uso primera): {0} track={1} n={2} [{3}]", filePath, block.Name, block.Images.Count, string.Join(",", block.Images.ToArray())));
					}
					for (int j = 0; j < block.Records.Count; j++)
					{
						float[] r = block.Records[j];
						ReanimatorTransform t = ReanimatorTransform.GetReanimatorTransformForLoadingThread();
						t.mTransX = MapValue(r[0]);
						t.mTransY = MapValue(r[1]);
						t.mSkewX = MapValue(r[2]);
						t.mSkewY = MapValue(r[3]);
						t.mScaleX = MapValue(r[4]);
						t.mScaleY = MapValue(r[5]);
						t.mFrame = MapValue(r[6]);
						t.mAlpha = MapAlpha(r[7]);
						t.mImageName = imageName;
						t.mFontName = string.Empty;
						t.mText = string.Empty;
						track.mTransforms[j] = t;
					}
					def.mTracks[i] = track;
				}
				return true;
			}
			catch (Exception ex)
			{
				SexyAppBase.LoadLog("ReanimCompiled fallo: " + filePath + " " + ex.Message);
				return false;
			}
		}

		private static float MapValue(float v)
		{
			if (v == COMPILED_PLACEHOLDER)
			{
				return ReanimatorXnaHelpers.DEFAULT_FIELD_PLACEHOLDER;
			}
			return v;
		}

		private static float MapAlpha(float v)
		{
			return MapValue(v);
		}

		private static bool ReadAndInflate(string filePath, out byte[] inflated, out float fps)
		{
			inflated = null;
			fps = 12f;
			byte[] file;
			using (Stream s = TitleContainer.OpenStream(filePath))
			{
				using (MemoryStream ms = new MemoryStream())
				{
					s.CopyTo(ms);
					file = ms.ToArray();
				}
			}
			if (file.Length < 10 || BitConverter.ToUInt32(file, 0) != COMPILED_MAGIC)
			{
				SexyAppBase.LoadLog("ReanimCompiled magia invalida: " + filePath);
				return false;
			}
			int expectedSize = BitConverter.ToInt32(file, 4);
			if (file[8] != 0x78)
			{
				SexyAppBase.LoadLog("ReanimCompiled sin zlib: " + filePath);
				return false;
			}
			using (MemoryStream raw = new MemoryStream(file, 10, file.Length - 10))
			{
				using (System.IO.Compression.DeflateStream ds = new System.IO.Compression.DeflateStream(raw, System.IO.Compression.CompressionMode.Decompress))
				{
					using (MemoryStream outMs = new MemoryStream(expectedSize > 0 ? Math.Min(expectedSize, 8 * 1024 * 1024) : 65536))
					{
						ds.CopyTo(outMs);
						inflated = outMs.ToArray();
					}
				}
			}
			if (expectedSize > 0 && inflated.Length != expectedSize)
			{
				SexyAppBase.LoadLog(string.Format("ReanimCompiled tam distinto ({0} vs {1}): {2}", inflated.Length, expectedSize, filePath));
			}
			if (inflated.Length >= 16)
			{
				float fileFps = BitConverter.ToSingle(inflated, 12);
				if (fileFps >= 1f && fileFps <= 120f)
				{
					fps = fileFps;
				}
			}
			return inflated.Length > 64;
		}

		private static List<CompiledBlock> ParseBlocks(byte[] data, string filePath)
		{
			List<CompiledBlock> blocks = new List<CompiledBlock>();
			int offset = 0;
			int firstHeader = FindNextBlockHeader(data, 0);
			if (firstHeader < 0)
			{
				return blocks;
			}
			offset = firstHeader;
			while (offset < data.Length && blocks.Count < MAX_BLOCKS)
			{
				while (offset < data.Length && !IsBlockHeaderAt(data, offset))
				{
					offset++;
				}
				if (offset >= data.Length)
				{
					break;
				}
				int nameLen = BitConverter.ToInt32(data, offset);
				string name = Encoding.ASCII.GetString(data, offset + 4, nameLen);
				int stride = BitConverter.ToInt32(data, offset + 4 + nameLen);
				if (stride != EXPECTED_STRIDE)
				{
					SexyAppBase.LoadLog(string.Format("ReanimCompiled stride {0} (esperado 44) en {1}@{2}", stride, filePath, offset));
					return null;
				}
				CompiledBlock block = new CompiledBlock();
				block.Name = name;
				offset += 4 + nameLen + 4;
				int recordCount = 0;
				while (recordCount < MAX_RECORDS_PER_BLOCK)
				{
					if (IsImageEntryAt(data, offset))
					{
						break;
					}
					if (offset + EXPECTED_STRIDE > data.Length)
					{
						int tail = data.Length - offset;
						if (tail > 0)
						{
							bool allZero = true;
							for (int i = 0; i < tail; i++)
							{
								if (data[offset + i] != 0)
								{
									allZero = false;
									break;
								}
							}
							if (!allZero)
							{
								SexyAppBase.LoadLog(string.Format("ReanimCompiled cola parcial no-cero ({0}B) en {1} bloque {2}", tail, filePath, name));
							}
						}
						break;
					}
					if (IsBlockHeaderAt(data, offset))
					{
						break;
					}
					if (IsZeroChunk(data, offset, EXPECTED_STRIDE))
					{
						break;
					}
					float[] record = new float[RECORD_FLOATS];
					for (int i = 0; i < RECORD_FLOATS; i++)
					{
						record[i] = BitConverter.ToSingle(data, offset + i * 4);
					}
					block.Records.Add(record);
					offset += EXPECTED_STRIDE;
				}
				while (offset < data.Length)
				{
					if (IsBlockHeaderAt(data, offset))
					{
						break;
					}
					if (IsImageEntryAt(data, offset))
					{
						int imgLen = BitConverter.ToInt32(data, offset);
						string img = Encoding.ASCII.GetString(data, offset + 4, imgLen);
						block.Images.Add(img);
						offset += 4 + imgLen + 8;
						continue;
					}
					if (IsZeroChunk(data, offset, Math.Min(44, data.Length - offset)))
					{
						offset += 4;
						continue;
					}
					break;
				}
				blocks.Add(block);
			}
			return blocks;
		}

		private static bool IsBlockHeaderAt(byte[] data, int offset)
		{
			if (offset + 8 > data.Length)
			{
				return false;
			}
			int nameLen = BitConverter.ToInt32(data, offset);
			if (nameLen < 1 || nameLen > 128)
			{
				return false;
			}
			if (offset + 4 + nameLen + 4 > data.Length)
			{
				return false;
			}
			for (int i = 0; i < nameLen; i++)
			{
				byte c = data[offset + 4 + i];
				bool ok = (c >= 48 && c <= 57) || (c >= 65 && c <= 90) || (c >= 97 && c <= 122) || c == 95 || c == 32;
				if (!ok)
				{
					return false;
				}
			}
			return BitConverter.ToInt32(data, offset + 4 + nameLen) == EXPECTED_STRIDE;
		}

		private static int FindNextBlockHeader(byte[] data, int start)
		{
			for (int offset = start; offset + 8 < data.Length; offset++)
			{
				if (IsBlockHeaderAt(data, offset))
				{
					return offset;
				}
			}
			return -1;
		}

		private static bool IsImageEntryAt(byte[] data, int offset)
		{
			if (offset + 12 > data.Length)
			{
				return false;
			}
			int nameLen = BitConverter.ToInt32(data, offset);
			if (nameLen < 1 || nameLen > 128)
			{
				return false;
			}
			if (offset + 4 + nameLen + 8 > data.Length)
			{
				return false;
			}
			for (int i = 0; i < nameLen; i++)
			{
				byte c = data[offset + 4 + i];
				bool ok = (c >= 48 && c <= 57) || (c >= 65 && c <= 90) || (c >= 97 && c <= 122) || c == 95 || c == 32;
				if (!ok)
				{
					return false;
				}
			}
			return BitConverter.ToInt32(data, offset + 4 + nameLen) == 0 && BitConverter.ToInt32(data, offset + 4 + nameLen + 4) == 0;
		}

		private static bool IsZeroChunk(byte[] data, int offset, int length)
		{
			if (offset + length > data.Length)
			{
				return false;
			}
			for (int i = 0; i < length; i++)
			{
				if (data[offset + i] != 0)
				{
					return false;
				}
			}
			return true;
		}
	}
}
