namespace MiniLauncher
{
    public class Hex2Pix
    {
        private const double sqrt3 = 1.73205080757;
        private const double half_sqrt3 = 0.86602540378;
        private const double one_and_a_half = 1.5;

        public (double x, double y) ToPix(Hex hex, double size)
        {
            return (
                size * (sqrt3 * hex.Q + half_sqrt3 * hex.R),
                size * one_and_a_half * hex.R
            );
        }
    }
}