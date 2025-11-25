using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TatehamaTTC_v1bata.Model
{
    public class TrackCircuitData : IEquatable<TrackCircuitData>
    {
        public string Last { get; init; } // 軌道回路を踏んだ列車の名前
        public required string Name { get; init; }
        public bool Lock { get; init; }
        public bool On { get; init; }

        public override string ToString()
        {
            return $"{Name}/{Last}/{On}";
        }

        public bool Equals(TrackCircuitData? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name == other.Name;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as TrackCircuitData);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
