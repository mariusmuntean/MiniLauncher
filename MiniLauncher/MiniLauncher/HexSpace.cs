using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms.Internals;

namespace MiniLauncher
{
    public class HexSpace
    {
        private readonly Dictionary<Hex, object> _space = new Dictionary<Hex, object>();

        private readonly Hex[] _neighborDirections = new[]
        {
            new Hex(1, 0),
            new Hex(1, -1),
            new Hex(0, -1),
            new Hex(-1, 0),
            new Hex(-1, 1),
            new Hex(0, 1)
        };


        public Hex Add(object payload)
        {
            // If empty or first free then immediately add
            var first = new Hex(0, 0);
            if (_space.Count == 0 || !_space.ContainsKey(first))
            {
                _space.Add(first, payload);
                return first;
            }

            // Add to the first empty slot
            var neighborsToVisit = new Queue<Hex>(GenerateNeighbors(first));
            while (neighborsToVisit.Any())
            {
                var currentHex = neighborsToVisit.Dequeue();
                if (!_space.ContainsKey(currentHex))
                {
                    _space.Add(currentHex, payload);
                    return currentHex;
                }

                // Enqueue the neighbors of the current hex, but only consider those that aren't already enqueued
                var neighborsToEnqueue = GenerateNeighbors(currentHex).Where(h => !neighborsToVisit.Contains(h));
                neighborsToEnqueue.ForEach(h => neighborsToVisit.Enqueue(h));
            }

            throw new Exception("Couldn't find a empty slot to add the payload. This is unexpected!");
        }

        public void Remove(Hex h)
        {
            if (_space.ContainsKey(h))
            {
                _space.Remove(h);
            }
        }

        public bool Contains(Hex h) => _space.ContainsKey(h);
        public object GetPayload(Hex h) => _space[h];
        public int Count => _space.Count;

        private Hex[] GenerateNeighbors(Hex h) => _neighborDirections.Select(dir => new Hex(h.Q + dir.Q, h.R + dir.R)).ToArray();
    }
}