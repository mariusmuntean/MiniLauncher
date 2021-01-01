using System;
using FluentAssertions;
using Xunit;

namespace MiniLauncher.Test
{
    public class RingComputeTest
    {
        [Fact]
        public void ArgumentCheck()
        {
            var rc = new RingCompute();
            Assert.Throws<ArgumentOutOfRangeException>(() => rc.ComputeRingIndex(-1));
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(6, 1)]
        [InlineData(7, 2)]
        [InlineData(18, 2)]
        [InlineData(19, 3)]
        [InlineData(20, 3)]
        [InlineData(37, 4)]
        public void ComputeTest(int elementIndex, int expectedRing)
        {
            var rc = new RingCompute();
            rc.ComputeRingIndex(elementIndex).Should().Be(expectedRing);
        }
    }
}