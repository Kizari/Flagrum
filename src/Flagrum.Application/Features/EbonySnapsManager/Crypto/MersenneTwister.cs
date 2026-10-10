public class MersenneTwister
{
    private const int N = 624;
    private const int M = 397;
    private const uint MATRIX_A = 0x9908b0dfU;
    private const uint UPPER_MASK = 0x80000000U;
    private const uint LOWER_MASK = 0x7fffffffU;

    private uint[] mt = new uint[N];
    private int mti = N + 1;

    // Constructor with a default seed
    public MersenneTwister(uint seed = 5489U)
    {
        Initialize(seed);
    }

    // Initialize generator with a seed
    public void Initialize(uint seed)
    {
        mt[0] = seed;
        for (mti = 1; mti < N; mti++)
        {
            mt[mti] = (uint)(1812433253U * (mt[mti - 1] ^ (mt[mti - 1] >> 30)) + mti);
        }
    }

    // Generate the next random integer in [0, 0xffffffff]
    public uint NextUInt()
    {
        uint y;
        uint[] mag01 = { 0U, MATRIX_A };

        if (mti >= N)
        {
            int kk;

            if (mti == N + 1)
                Initialize(5489U);

            for (kk = 0; kk < N - M; kk++)
            {
                y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                mt[kk] = mt[kk + M] ^ (y >> 1) ^ mag01[y & 0x1U];
            }
            for (; kk < N - 1; kk++)
            {
                y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                mt[kk] = mt[kk + (M - N)] ^ (y >> 1) ^ mag01[y & 0x1U];
            }
            y = (mt[N - 1] & UPPER_MASK) | (mt[0] & LOWER_MASK);
            mt[N - 1] = mt[M - 1] ^ (y >> 1) ^ mag01[y & 0x1U];

            mti = 0;
        }

        y = mt[mti++];

        // Tempering
        y ^= (y >> 11);
        y ^= (y << 7) & 0x9d2c5680U;
        y ^= (y << 15) & 0xefc60000U;
        y ^= (y >> 18);

        return y;
    }

    // Generate the next random double in [0,1)
    public double NextDouble()
    {
        return (double)NextUInt() / (uint.MaxValue + 1.0);
    }
}