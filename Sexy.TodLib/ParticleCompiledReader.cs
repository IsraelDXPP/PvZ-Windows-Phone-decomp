using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework;
using Sexy;

namespace Sexy.TodLib;

// Lector de las definiciones de particulas en formato ".xml.compiled" (DefinitionReadCompiledFile
// en Sexy.TodLib/Definition.cpp del mod).
//
// El archivo es un volcado de la estructura original: NO guarda punteros utilizables, sino que
// duplica el bloque plano del struct (datos no-punteria validos, punteros basura) seguido de un
// stream "cache" guiado por el DefMap que reparte strings, imagenes, tracks y arrays anidados.
// Los punteros basura del volcado se ignoran; solo se leen del bloque plano los campos escalares.
internal delegate Image CompiledImageResolver(string name);

internal static class ParticleCompiledReader
{
	private const uint COMPILED_MAGIC = 0xDEADFED4u;

	// sizeof(TodEmitterDefinition) en x86: ultimo campo AnimationRate en 0x15C (FloatParameterTrack = ptr+int = 8).
	private const int EMITTER_RAW_SIZE = 0x164;

	// Offsets dentro del bloque plano del emitter (ver gEmitterDefFields en Definition.cpp).
	private const int RAW_IMAGE_COL = 0x4;
	private const int RAW_IMAGE_ROW = 0x8;
	private const int RAW_IMAGE_FRAMES = 0xC;
	private const int RAW_ANIMATED = 0x10;
	private const int RAW_PARTICLE_FLAGS = 0x14;
	private const int RAW_EMITTER_TYPE = 0x18;
	private const int RAW_FIELD_COUNT = 0xD8;
	private const int RAW_SYSTEM_FIELD_COUNT = 0xE0;

	// sizeof(ParticleField): FieldType(0x0), mX(0x4), mY(0xC).
	private const int FIELD_RAW_SIZE = 0x14;

	// sizeof(FloatParameterTrackNode): mTime, mLowValue, mHighValue, mCurveType, mDistribution.
	private const int TRACK_NODE_SIZE = 20;

	// Orden exacto de gEmitterDefFields (Definition.cpp:50-101). OJO: el orden del stream NO sigue
	// el orden de los offsets en memoria (p.ej. EmitterPath 0x8C se serializa antes que
	// EmitterSkewX 0x7C, y SystemRed..SystemBrightness antes que LaunchSpeed/LaunchAngle).
	// Los escalares (DT_INT/DT_ENUM/DT_FLAGS) vienen del bloque plano y no se leen del stream.
	private static readonly string[] sEmitterSchema =
	{
		"Image",
		"Name",
		"SystemDuration",
		"OnDuration",
		"CrossFadeDuration",
		"SpawnRate",
		"SpawnMinActive",
		"SpawnMaxActive",
		"SpawnMaxLaunched",
		"EmitterRadius",
		"EmitterOffsetX",
		"EmitterOffsetY",
		"EmitterBoxX",
		"EmitterBoxY",
		"EmitterPath",
		"EmitterSkewX",
		"EmitterSkewY",
		"ParticleDuration",
		"LaunchSpeed",
		"LaunchAngle",
		"SystemRed",
		"SystemGreen",
		"SystemBlue",
		"SystemAlpha",
		"SystemBrightness",
		"Field",
		"SystemField",
		"ParticleRed",
		"ParticleGreen",
		"ParticleBlue",
		"ParticleAlpha",
		"ParticleBrightness",
		"ParticleSpinAngle",
		"ParticleSpinSpeed",
		"ParticleScale",
		"ParticleStretch",
		"CollisionReflect",
		"CollisionSpin",
		"ClipTop",
		"ClipBottom",
		"ClipLeft",
		"ClipRight",
		"AnimationRate"
	};

	public static bool LoadCompiledParticle(string filePath, ref TodParticleDefinition def)
	{
		byte[] inflated;
		if (!ReadAndInflate(filePath, out inflated))
		{
			def = null;
			return false;
		}
		TodParticleDefinition parsed;
		int endOffset;
		string error;
		if (!ParseInflated(inflated, filePath, LoadImageByName, out parsed, out endOffset, out error))
		{
			def = null;
			return false;
		}
		def = parsed;
		return true;
	}

	internal static bool ParseInflated(byte[] inflated, string filePath, CompiledImageResolver resolver, out TodParticleDefinition def, out int endOffset, out string error)
	{
		def = null;
		endOffset = 0;
		error = null;
		try
		{
			// uint aCashHash (schema hash, solo informativo) + bloque plano de TodParticleDefinition.
			// El struct es { TodEmitterDefinition* mEmitterDefs; int mEmitterDefCount; } -> 8 bytes.
			if (inflated.Length < 4 + 8)
			{
				return false;
			}
			Reader r = new Reader(inflated, 4);
			r.Skip(4);
			int emitterCount = r.ReadInt();

			// DefReadFromCacheArray: int aDefSize debe coincidir con sizeof(TodEmitterDefinition).
			int defSize = r.ReadInt();
			if (defSize != EMITTER_RAW_SIZE)
			{
				SexyAppBase.LoadLog(string.Format("ParticleCompiled defSize inesperado ({0}): {1}", defSize, filePath));
				return false;
			}
			if (emitterCount < 0 || emitterCount > 4096)
			{
				return false;
			}

			// DefReadFromCacheArray lee el bloque plano de TODOS los elementos de una vez y solo despues
			// aplica los fixups de cada elemento, en orden. Por eso los raw van todos juntos al
			// principio y no intercalados con el stream de fixups.
			byte[][] raws = new byte[emitterCount][];
			for (int i = 0; i < emitterCount; i++)
			{
				raws[i] = r.ReadBytes(EMITTER_RAW_SIZE);
			}

			TodEmitterDefinition[] emitters = new TodEmitterDefinition[emitterCount];
			for (int i = 0; i < emitterCount; i++)
			{
				emitters[i] = ReadEmitter(r, raws[i], resolver);
			}

			TodParticleDefinition result = new TodParticleDefinition();
			result.mEmitterDefs = emitters;
			result.mEmitterDefCount = emitters.Length;
			ApplyDefaults(result);
			def = result;
			endOffset = r.Offset;
			return true;
		}
		catch (Exception ex)
		{
			error = ex.GetType().Name + ": " + ex.Message;
			SexyAppBase.LoadLog("ParticleCompiled fallo: " + filePath + " " + error);
			def = null;
			endOffset = 0;
			return false;
		}
	}

	private static TodEmitterDefinition ReadEmitter(Reader r, byte[] raw, CompiledImageResolver resolver)
	{
		TodEmitterDefinition def = new TodEmitterDefinition();
		def.mImageCol = BitConverter.ToInt32(raw, RAW_IMAGE_COL);
		def.mImageRow = BitConverter.ToInt32(raw, RAW_IMAGE_ROW);
		def.mImageFrames = BitConverter.ToInt32(raw, RAW_IMAGE_FRAMES);
		def.mAnimated = BitConverter.ToInt32(raw, RAW_ANIMATED);
		def.mParticleFlags = BitConverter.ToInt32(raw, RAW_PARTICLE_FLAGS);
		def.mEmitterType = (EmitterType)BitConverter.ToInt32(raw, RAW_EMITTER_TYPE);
		int fieldCount = BitConverter.ToInt32(raw, RAW_FIELD_COUNT);
		int systemFieldCount = BitConverter.ToInt32(raw, RAW_SYSTEM_FIELD_COUNT);

		for (int i = 0; i < sEmitterSchema.Length; i++)
		{
			switch (sEmitterSchema[i])
			{
			case "Raw":
				break;
			case "Image":
				def.mImage = resolver(r.ReadString());
				break;
			case "Name":
				def.mName = r.ReadString();
				break;
			case "OnDuration":
				def.mOnDuration = r.ReadString();
				break;
			case "Field":
				def.mParticleFields = ReadFieldArray(r, fieldCount);
				def.mParticleFieldCount = def.mParticleFields.Length;
				break;
			case "SystemField":
				def.mSystemFields = ReadFieldArray(r, systemFieldCount);
				def.mSystemFieldCount = def.mSystemFields.Length;
				break;
			default:
				SetTrack(def, sEmitterSchema[i], r.ReadTrack());
				break;
			}
		}
		return def;
	}

	private static ParticleField[] ReadFieldArray(Reader r, int count)
	{
		int defSize = r.ReadInt();
		if (defSize != FIELD_RAW_SIZE)
		{
			throw new InvalidDataException("ParticleCompiled field defSize inesperado: " + defSize);
		}
		if (count <= 0)
		{
			return new ParticleField[0];
		}
		byte[] block = r.ReadBytes(FIELD_RAW_SIZE * count);
		ParticleField[] fields = new ParticleField[count];
		for (int i = 0; i < count; i++)
		{
			ParticleField f = new ParticleField();
			f.mFieldType = (ParticleFieldType)BitConverter.ToInt32(block, FIELD_RAW_SIZE * i);
			f.mX = r.ReadTrack();
			f.mY = r.ReadTrack();
			fields[i] = f;
		}
		return fields;
	}

	private static void SetTrack(TodEmitterDefinition def, string name, FloatParameterTrack track)
	{
		switch (name)
		{
		case "SystemDuration": def.mSystemDuration = track; break;
		case "CrossFadeDuration": def.mCrossFadeDuration = track; break;
		case "SpawnRate": def.mSpawnRate = track; break;
		case "SpawnMinActive": def.mSpawnMinActive = track; break;
		case "SpawnMaxActive": def.mSpawnMaxActive = track; break;
		case "SpawnMaxLaunched": def.mSpawnMaxLaunched = track; break;
		case "EmitterRadius": def.mEmitterRadius = track; break;
		case "EmitterOffsetX": def.mEmitterOffsetX = track; break;
		case "EmitterOffsetY": def.mEmitterOffsetY = track; break;
		case "EmitterBoxX": def.mEmitterBoxX = track; break;
		case "EmitterBoxY": def.mEmitterBoxY = track; break;
		case "EmitterPath": def.mEmitterPath = track; break;
		case "EmitterSkewX": def.mEmitterSkewX = track; break;
		case "EmitterSkewY": def.mEmitterSkewY = track; break;
		case "ParticleDuration": def.mParticleDuration = track; break;
		case "SystemRed": def.mSystemRed = track; break;
		case "SystemGreen": def.mSystemGreen = track; break;
		case "SystemBlue": def.mSystemBlue = track; break;
		case "SystemAlpha": def.mSystemAlpha = track; break;
		case "SystemBrightness": def.mSystemBrightness = track; break;
		case "LaunchSpeed": def.mLaunchSpeed = track; break;
		case "LaunchAngle": def.mLaunchAngle = track; break;
		case "ParticleRed": def.mParticleRed = track; break;
		case "ParticleGreen": def.mParticleGreen = track; break;
		case "ParticleBlue": def.mParticleBlue = track; break;
		case "ParticleAlpha": def.mParticleAlpha = track; break;
		case "ParticleBrightness": def.mParticleBrightness = track; break;
		case "ParticleSpinAngle": def.mParticleSpinAngle = track; break;
		case "ParticleSpinSpeed": def.mParticleSpinSpeed = track; break;
		case "ParticleScale": def.mParticleScale = track; break;
		case "ParticleStretch": def.mParticleStretch = track; break;
		case "CollisionReflect": def.mCollisionReflect = track; break;
		case "CollisionSpin": def.mCollisionSpin = track; break;
		case "ClipTop": def.mClipTop = track; break;
		case "ClipBottom": def.mClipBottom = track; break;
		case "ClipLeft": def.mClipLeft = track; break;
		case "ClipRight": def.mClipRight = track; break;
		case "AnimationRate": def.mAnimationRate = track; break;
		default: throw new InvalidDataException("ParticleCompiled track desconocida: " + name);
		}
	}

	// TodParticleLoadADef / FloatTrackSetDefault: solo rellena si el track quedo vacio.
	private static void ApplyDefaults(TodParticleDefinition def)
	{
		for (int i = 0; i < def.mEmitterDefCount; i++)
		{
			TodEmitterDefinition a = def.mEmitterDefs[i];
			TrackSetDefault(a.mSystemDuration, 0f);
			TrackSetDefault(a.mSpawnRate, 0f);
			TrackSetDefault(a.mSpawnMinActive, -1f);
			TrackSetDefault(a.mSpawnMaxActive, -1f);
			TrackSetDefault(a.mSpawnMaxLaunched, -1f);
			TrackSetDefault(a.mEmitterRadius, 0f);
			TrackSetDefault(a.mEmitterOffsetX, 0f);
			TrackSetDefault(a.mEmitterOffsetY, 0f);
			TrackSetDefault(a.mEmitterBoxX, 0f);
			TrackSetDefault(a.mEmitterBoxY, 0f);
			TrackSetDefault(a.mEmitterSkewX, 0f);
			TrackSetDefault(a.mEmitterSkewY, 0f);
			TrackSetDefault(a.mParticleDuration, 100f);
			TrackSetDefault(a.mLaunchSpeed, 0f);
			TrackSetDefault(a.mSystemRed, 1f);
			TrackSetDefault(a.mSystemGreen, 1f);
			TrackSetDefault(a.mSystemBlue, 1f);
			TrackSetDefault(a.mSystemAlpha, 1f);
			TrackSetDefault(a.mSystemBrightness, 1f);
			TrackSetDefault(a.mLaunchAngle, 0f);
			TrackSetDefault(a.mCrossFadeDuration, 0f);
			TrackSetDefault(a.mParticleRed, 1f);
			TrackSetDefault(a.mParticleGreen, 1f);
			TrackSetDefault(a.mParticleBlue, 1f);
			TrackSetDefault(a.mParticleAlpha, 1f);
			TrackSetDefault(a.mParticleBrightness, 1f);
			TrackSetDefault(a.mParticleSpinAngle, 0f);
			TrackSetDefault(a.mParticleSpinSpeed, 0f);
			TrackSetDefault(a.mParticleScale, 1f);
			TrackSetDefault(a.mParticleStretch, 1f);
			TrackSetDefault(a.mCollisionReflect, 0f);
			TrackSetDefault(a.mCollisionSpin, 0f);
			TrackSetDefault(a.mClipTop, 0f);
			TrackSetDefault(a.mClipBottom, 0f);
			TrackSetDefault(a.mClipLeft, 0f);
			TrackSetDefault(a.mClipRight, 0f);
			TrackSetDefault(a.mAnimationRate, 0f);
		}
	}

	private static void TrackSetDefault(FloatParameterTrack track, float value)
	{
		if (track.mCountNodes == 0 && value != 0f)
		{
			track.mCountNodes = 1;
			track.mNodes = new FloatParameterTrackNode[1];
			track.mNodes[0] = new FloatParameterTrackNode
			{
				mTime = 0f,
				mLowValue = value,
				mHighValue = value,
				mCurveType = TodCurves.CURVE_CONSTANT,
				mDistribution = TodCurves.CURVE_CONSTANT
			};
		}
	}

	// DefinitionLoadImage: el nombre del XML es el simbolo de Resources (p.ej. "IMAGE_FOG").
	private static Image LoadImageByName(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		// 1) Mapa de recursos, como DefinitionLoadImage (Definition.cpp:254).
		try
		{
			FieldInfo field = typeof(Resources).GetField(name, BindingFlags.Public | BindingFlags.Static);
			if (field != null && typeof(Image).IsAssignableFrom(field.FieldType))
			{
				Image image = field.GetValue(null) as Image;
				if (image != null)
				{
					return image;
				}
			}
		}
		catch
		{
		}
		// 2) Fallback por ruta quitando el prefijo (gDefLoadResPaths, Definition.cpp:140).
		//    El prefijo mas largo se prueba primero porque "IMAGE_REANIM_" empieza por "IMAGE_".
		string baseName = null;
		if (name.Length > "IMAGE_REANIM_".Length && name.StartsWith("IMAGE_REANIM_", StringComparison.Ordinal))
		{
			baseName = name.Substring("IMAGE_REANIM_".Length);
		}
		else if (name.Length > "IMAGE_".Length && name.StartsWith("IMAGE_", StringComparison.Ordinal))
		{
			baseName = name.Substring("IMAGE_".Length);
		}
		if (baseName != null)
		{
			Image loose = LooseImageLoader.Load(baseName, 1, 1);
			if (loose != null)
			{
				return loose;
			}
		}
		SexyAppBase.LoadLog("ParticleCompiled sin imagen: " + name);
		return null;
	}

	private static bool ReadAndInflate(string filePath, out byte[] inflated)
	{
		inflated = null;
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
			SexyAppBase.LoadLog("ParticleCompiled magia invalida: " + filePath);
			return false;
		}
		int expectedSize = BitConverter.ToInt32(file, 4);
		if (file[8] != 0x78)
		{
			SexyAppBase.LoadLog("ParticleCompiled sin zlib: " + filePath);
			return false;
		}
		using (MemoryStream raw = new MemoryStream(file, 10, file.Length - 10))
		{
			using (System.IO.Compression.DeflateStream ds = new System.IO.Compression.DeflateStream(raw, System.IO.Compression.CompressionMode.Decompress))
			{
				using (MemoryStream outMs = new MemoryStream(expectedSize > 0 ? expectedSize : 65536))
				{
					ds.CopyTo(outMs);
					inflated = outMs.ToArray();
				}
			}
		}
		return inflated != null && inflated.Length > 8;
	}

	private sealed class Reader
	{
		private readonly byte[] mData;
		private int mPos;

		public Reader(byte[] data, int pos)
		{
			mData = data;
			mPos = pos;
		}

		public int Offset
		{
			get { return mPos; }
		}

		public int Remaining
		{
			get { return mData.Length - mPos; }
		}

		public void Skip(int count)
		{
			mPos += count;
		}

		public int ReadInt()
		{
			int v = BitConverter.ToInt32(mData, mPos);
			mPos += 4;
			return v;
		}

		public byte[] ReadBytes(int count)
		{
			if (count < 0 || Remaining < count)
			{
				throw new InvalidDataException("ParticleCompiled buffer agotado");
			}
			byte[] result = new byte[count];
			Array.Copy(mData, mPos, result, 0, count);
			mPos += count;
			return result;
		}

		// DefReadFromCacheString: int aLen + aLen bytes. aLen == 0 -> "".
		public string ReadString()
		{
			int len = ReadInt();
			if (len == 0)
			{
				return string.Empty;
			}
			if (len < 0 || len > 100000)
			{
				throw new InvalidDataException("ParticleCompiled string len invalida: " + len);
			}
			return System.Text.Encoding.UTF8.GetString(ReadBytes(len));
		}

		// DefReadFromCacheFloatTrack: int mCountNodes + mCountNodes * sizeof(FloatParameterTrackNode).
		public FloatParameterTrack ReadTrack()
		{
			FloatParameterTrack track = new FloatParameterTrack();
			int count = ReadInt();
			if (count <= 0)
			{
				track.mCountNodes = 0;
				track.mNodes = null;
				return track;
			}
			if (count > 100000)
			{
				throw new InvalidDataException("ParticleCompiled track count invalido: " + count);
			}
			byte[] block = ReadBytes(count * TRACK_NODE_SIZE);
			track.mCountNodes = count;
			track.mNodes = new FloatParameterTrackNode[count];
			for (int i = 0; i < count; i++)
			{
				int o = TRACK_NODE_SIZE * i;
				track.mNodes[i] = new FloatParameterTrackNode
				{
					mTime = BitConverter.ToSingle(block, o),
					mLowValue = BitConverter.ToSingle(block, o + 4),
					mHighValue = BitConverter.ToSingle(block, o + 8),
					mCurveType = (TodCurves)BitConverter.ToInt32(block, o + 12),
					mDistribution = (TodCurves)BitConverter.ToInt32(block, o + 16)
				};
			}
			return track;
		}
	}
}