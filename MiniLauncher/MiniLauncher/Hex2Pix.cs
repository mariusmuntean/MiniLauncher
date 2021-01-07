using System;

namespace MiniLauncher
{
    public class Hex2Pix
    {
        private const double sqrt3 = 1.73205080757;
        private const double Sqrt3_by_3 = 0.57735026919d;
        private const double half_sqrt3 = 0.86602540378;
        private const double one_and_a_half = 1.5;

        public (double x, double y) ToPix(Hex hex, double size)
        {
            return (
                size * (sqrt3 * hex.Q + half_sqrt3 * hex.R),
                size * one_and_a_half * hex.R
            );
        }

        public Hex ToHex(double x, double y, double size)
        {
            var fractionalQ = (Sqrt3_by_3 * x - 0.3333 * y) / size;
            var fractionalR = 0.6666 * y / size;

            var q = (int) Math.Truncate(fractionalQ);
            var r = (int) Math.Truncate(fractionalR);
            return new Hex(q, r);
        }

        public (double width, double height) ComputeHexDimensions(double Size)
        {
            var w = sqrt3 * Size;
            var h = 2 * Size;
            return (w, h);
        }
    }
}