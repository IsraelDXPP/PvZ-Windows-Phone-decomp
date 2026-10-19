namespace Sexy;

internal class MTRand
{
	private const int N = 624;

	private const int M = 397;

	private const uint MATRIX_A = 0x9908b0dfU;

	private const uint UPPER_MASK = 0x80000000U;

	private const uint LOWER_MASK = 0x7fffffffU;

	private const uint TEMPERING_MASK_B = 0x9d2c5680U;

	private const uint TEMPERING_MASK_C = 0xefc60000U;

	private uint[] mt = new uint[N];

	private int mti;

	public MTRand()
	{
		SRand(4357u);
	}

	public MTRand(ulong seed)
	{
		SRand(seed);
	}

	public void SRand(ulong seed)
	{
		if (seed == 0)
		{
			seed = 4357;
		}
		mt[0] = (uint)(seed & 0xffffffffUL);
		for (mti = 1; mti < N; mti++)
		{
			mt[mti] = (1812433253U * (mt[mti - 1] ^ (mt[mti - 1] >> 30)) + (uint)mti);
		}
	}

	public ulong Next()
	{
		uint y;
		if (mti >= N)
		{
			int kk;
			for (kk = 0; kk < N - M; kk++)
			{
				y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
				mt[kk] = mt[kk + M] ^ (y >> 1) ^ ((y & 0x1U) != 0 ? MATRIX_A : 0U);
			}
			for (; kk < N - 1; kk++)
			{
				y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
				mt[kk] = mt[kk + (M - N)] ^ (y >> 1) ^ ((y & 0x1U) != 0 ? MATRIX_A : 0U);
			}
			y = (mt[N - 1] & UPPER_MASK) | (mt[0] & LOWER_MASK);
			mt[N - 1] = mt[M - 1] ^ (y >> 1) ^ ((y & 0x1U) != 0 ? MATRIX_A : 0U);
			mti = 0;
		}
		y = mt[mti++];
		y ^= (y >> 11);
		y ^= (y << 7) & TEMPERING_MASK_B;
		y ^= (y << 15) & TEMPERING_MASK_C;
		y ^= (y >> 18);
		return y;
	}

	public ulong Next(ulong range)
	{
		if (range == 0)
		{
			return 0;
		}
		return Next() % range;
	}
}
