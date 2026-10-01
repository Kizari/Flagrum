namespace Flagrum.Application.Features.EbonySnapsManager.Crypto
{
    internal class SharedCryptoFunctions
    {
        public static int[] GenerateKeyStack(CryptoVariables cryptoVars, int bufferSize)
        {
            var keyStack = new int[bufferSize];
            var mt = new MersenneTwister(cryptoVars.Seed);

            for (var i = bufferSize - 1; i > 1; i--)
            {
                while (true)
                {
                    var value = mt.NextUInt();

                    if (value < (uint)~(uint.MaxValue % i))
                    {
                        keyStack[i] = (int)(value % i);
                        break;
                    }
                }
            }

            return keyStack;
        }
    }
}