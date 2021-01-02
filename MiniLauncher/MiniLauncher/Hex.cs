using System;

namespace MiniLauncher
{
    public struct Hex : IEquatable<Hex>
    {
        private readonly int _q;
        private readonly int _r;

        public Hex(int q, int r)
        {
            _q = q;
            _r = r;
        }

        public int R => _r;

        public int Q => _q;

        public bool Equals(Hex other)
        {
            return _q == other._q && _r == other._r;
        }

        public override bool Equals(object obj)
        {
            return obj is Hex other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_q * 397) ^ _r;
            }
        }
    }
}