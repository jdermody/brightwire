using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.Models.Bayesian
{
    /// <summary>
    /// A markov model state transition
    /// </summary>
    /// <typeparam name="T">The data type</typeparam>
    public class MarkovModelStateTransition<T>
    {
        /// <summary>
        /// The next state
        /// </summary>
        public T NextState { get; set; }

        /// <summary>
        /// The probability of this next state
        /// </summary>
        public float Probability { get; set; }

        public override string ToString() => $"{NextState} ({Probability})";
    }

    /// <summary>
    /// A markov model observation based on the preceding two items
    /// </summary>
    /// <typeparam name="T">The data type</typeparam>
    public class MarkovModelObservation2<T>
    {
        /// <summary>
        /// The second last preceding item
        /// </summary>
        public T Item1 { get; set; }

        /// <summary>
        /// The last preceding item
        /// </summary>
        public T Item2 { get; set; }

        /// <summary>
        /// The list of possible transitions from this state
        /// </summary>
        public MarkovModelStateTransition<T>[] Transition { get; set; }

        internal MarkovModelObservation2() { }
        internal MarkovModelObservation2(T item1, T item2, MarkovModelStateTransition<T>[] transition)
        {
            Item1 = item1;
            Item2 = item2;
            Transition = transition;
        }

        /// <summary>
        /// Equals overide
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is MarkovModelObservation2<T> other)
                return Object.Equals(other.Item1, Item1) && Object.Equals(other.Item2, Item2);
            return false;
        }

        /// <summary>
        /// Hash code override
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (Item1?.GetHashCode() ?? 0) ^ (Item2?.GetHashCode() ?? 0);
        }

        public override string ToString() => $"({Item1}, {Item2}) => {Transition.Length:N0}...";
    }

    /// <summary>
    /// A markov model based on observing two items at a time
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MarkovModel2<T>
    {
        /// <summary>
        /// The list of observations
        /// </summary>
        public MarkovModelObservation2<T>[] Observation { get; set; }

        /// <summary>
        /// Converts the model to a dictionary
        /// </summary>
        public Dictionary<MarkovModelObservation2<T>, MarkovModelStateTransition<T>[]> AsDictionary
        {
            get
            {
                return Observation.ToDictionary(o => o, o => o.Transition);
            }
        }
    }

    /// <summary>
    /// A markov model observation based on the preceding three instances
    /// </summary>
    /// <typeparam name="T">The data type</typeparam>
    public class MarkovModelObservation3<T>
    {
        /// <summary>
        /// The third last item
        /// </summary>
        public T Item1 { get; set; }

        /// <summary>
        /// The second last item
        /// </summary>
        public T Item2 { get; set; }

        /// <summary>
        /// The third last item
        /// </summary>
        public T Item3 { get; set; }

        /// <summary>
        /// The list of associated transitions
        /// </summary>
        public MarkovModelStateTransition<T>[] Transition { get; set; }

        internal MarkovModelObservation3() { }
        internal MarkovModelObservation3(T item1, T item2, T item3, MarkovModelStateTransition<T>[] transition)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Transition = transition;
        }

        /// <summary>
        /// Equals override
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is MarkovModelObservation3<T> other)
                return Object.Equals(other.Item1, Item1) && Object.Equals(other.Item2, Item2) && Object.Equals(other.Item3, Item3);
            return false;
        }

        /// <summary>
        /// Hashcode override
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (Item1?.GetHashCode() ?? 0) ^ (Item2?.GetHashCode() ?? 0) ^ (Item3?.GetHashCode() ?? 0);
        }

        public override string ToString() => $"({Item1}, {Item2}, {Item3}) => {Transition.Length:N0}...";
    }

    /// <summary>
    /// A markov model based on observing the last three observations
    /// </summary>
    /// <typeparam name="T">The data type</typeparam>
    public class MarkovModel3<T>
    {
        /// <summary>
        /// The list of observations
        /// </summary>
        public MarkovModelObservation3<T>[] Observation { get; set; }

        /// <summary>
        /// Converts the model to a dictionary
        /// </summary>
        public Dictionary<MarkovModelObservation3<T>, MarkovModelStateTransition<T>[]> AsDictionary
        {
            get
            {
                return Observation.ToDictionary(o => o, o => o.Transition);
            }
        }
    }
}
