using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Models
{
    [ProtoContract]
    public class MarkovModelStateTransition<T>
    {
        [ProtoMember(1)]
        public T NextState { get; set; }

        [ProtoMember(2)]
        public float Probability { get; set; }
    }

    [ProtoContract]
    public class MarkovModelObservation2<T>
    {
        [ProtoMember(1)]
        public T Item1 { get; set; }

        [ProtoMember(2)]
        public T Item2 { get; set; }

        [ProtoMember(3)]
        public List<MarkovModelStateTransition<T>> Transition { get; set; }

        internal MarkovModelObservation2() { }
        internal MarkovModelObservation2(T item1, T item2, List<MarkovModelStateTransition<T>> transition)
        {
            Item1 = item1;
            Item2 = item2;
            Transition = transition;
        }

        public override bool Equals(object obj)
        {
            var other = obj as MarkovModelObservation2<T>;
            if (other != null)
                return Object.Equals(other.Item1, Item1) && Object.Equals(other.Item2, Item2);
            return false;
        }

        public override int GetHashCode()
        {
            return (Item1?.GetHashCode() ?? 0) ^ (Item2?.GetHashCode() ?? 0);
        }
    }

    [ProtoContract]
    public class MarkovModel2<T>
    {
        [ProtoMember(1)]
        public MarkovModelObservation2<T>[] Observation { get; set; }

        public Dictionary<MarkovModelObservation2<T>, List<MarkovModelStateTransition<T>>> AsDictionary
        {
            get
            {
                return Observation.ToDictionary(o => o, o => o.Transition);
            }
        }
    }

    [ProtoContract]
    public class MarkovModelObservation3<T>
    {
        [ProtoMember(1)]
        public T Item1 { get; set; }

        [ProtoMember(2)]
        public T Item2 { get; set; }

        [ProtoMember(3)]
        public T Item3 { get; set; }

        [ProtoMember(4)]
        public List<MarkovModelStateTransition<T>> Transition { get; set; }

        internal MarkovModelObservation3() { }
        internal MarkovModelObservation3(T item1, T item2, T item3, List<MarkovModelStateTransition<T>> transition)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Transition = transition;
        }

        public override bool Equals(object obj)
        {
            var other = obj as MarkovModelObservation3<T>;
            if (other != null)
                return Object.Equals(other.Item1, Item1) && Object.Equals(other.Item2, Item2) && Object.Equals(other.Item3, Item3);
            return false;
        }

        public override int GetHashCode()
        {
            return (Item1?.GetHashCode() ?? 0) ^ (Item2?.GetHashCode() ?? 0) ^ (Item3?.GetHashCode() ?? 0);
        }
    }

    [ProtoContract]
    public class MarkovModel3<T>
    {
        [ProtoMember(1)]
        public MarkovModelObservation3<T>[] Observation { get; set; }

        public Dictionary<MarkovModelObservation3<T>, List<MarkovModelStateTransition<T>>> AsDictionary
        {
            get
            {
                return Observation.ToDictionary(o => o, o => o.Transition);
            }
        }
    }
}
