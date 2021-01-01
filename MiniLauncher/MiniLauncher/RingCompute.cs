using System;

namespace MiniLauncher
{
    public class RingCompute
    {
        public int ComputeRingIndex(int elementIndex)
        {
            if (elementIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(elementIndex), "The argument must be grater than or equal to zero");
            }

            if (elementIndex == 0)
            {
                return 0;
            }

            var currentRing = 0;
            var nextRingStartingIndex = 1;
            var i = 0;

            do
            {
                i++;
                if (i == nextRingStartingIndex)
                {
                    currentRing++;
                    nextRingStartingIndex = i + 6 * currentRing;
                }
            } while (i < elementIndex);

            return currentRing;
        }
    }
}