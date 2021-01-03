using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xamarin.Forms.Internals;
using Xunit;

namespace MiniLauncher.Test
{
    public class HexSpaceTest
    {
        [Fact]
        public void AddToEmptySpace()
        {
            // Given
            var space = new HexSpace<Item>();

            // When
            var payload = new Item();
            var h = space.Add(payload);

            // Then
            space.Contains(h).Should().BeTrue();
            space.GetPayload(h).Should().Be(payload);
            space.Count.Should().Be(1);
        }

        [Fact]
        public void FillUpFirstTwoRings()
        {
            // Given
            var space = new HexSpace<Item>();

            // When
            var payloads = Enumerable.Range(0, 7).Select(i => new Item());
            var hexes = new List<Hex>();
            foreach (var payload in payloads)
            {
                space.Add(payload);
            }

            // Then
            space.Count.Should().Be(payloads.Count());
            hexes.ForEach(h =>
            {
                var p = space.GetPayload(h);
                payloads.Should().Contain(p);
            });
        }

        class Item : IItem
        {
        }
    }
}