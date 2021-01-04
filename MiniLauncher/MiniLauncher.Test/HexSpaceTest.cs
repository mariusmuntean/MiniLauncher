using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using FluentAssertions;
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

        [Fact]
        public void NearestHexes_from_center()
        {
            // Given
            var space = new HexSpace<Item>();

            var payloads = Enumerable.Range(0, 7).Select(i => new Item());
            var hexes = new List<Hex>();
            foreach (var payload in payloads)
            {
                space.Add(payload);
            }
            
            // When
            var nearestHexes = space.GetNearestHexes(new Hex(0, 0));
            
            // Then
            nearestHexes.Should().NotBeEmpty();
            nearestHexes.Should().HaveCount(6);
        }
        
        [Fact]
        public void NearestHexes_from_side()
        {
            // Given
            var space = new HexSpace<Item>();

            var payloads = Enumerable.Range(0, 7).Select(i => new Item());
            var hexes = new List<Hex>();
            foreach (var payload in payloads)
            {
                space.Add(payload);
            }
            
            // When
            var nearestHexes = space.GetNearestHexes(new Hex(2, 0));
            
            // Then
            nearestHexes.Should().NotBeEmpty();
            nearestHexes.Should().HaveCount(1);
            var singleNearestHex = nearestHexes.Single();
            singleNearestHex.Should().Be(new Hex(1, 0));
        }

        class Item : IItem
        {
        }
    }
}