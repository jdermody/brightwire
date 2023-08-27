using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.Models.Bayesian
{
    /// <summary>
    /// A markov model state transition
    /// </summary>
    /// <typeparam name="T">The data type</typeparam>
    public class MarkovModelStateTransition<T> where T: notnull
    {
        /// <summary>
        /// The next state
        /// </summary>
        public T NextState { get; }

        /// <summary>
        /// The probability of this next state
        /// </summary>
        public float Probability { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nextState"></param>
        /// <param name="probability"></param>
        public MarkovModelStateTransition(T nextState, float probability)
        {
            NextState = nextState;
            Probability = probability;
        }

        /// <inheritdoc />
        public override string ToString() => $"{NextState} ({Probability})";
    }

    /// <summary>
    /// A markov model observation based on the preceding two items
    /// </summary>
    /// <typeparam name="T">The data type</typeparam>
    public class MarkovModelObservation2<T> where T : notnull
    {
        /// <summary>
        /// The second last preceding item
        /// </summary>
        public T Item1 { get; }

        /// <summary>
        /// The last preceding item
        /// </summary>
        public T Item2 { get; }

        /// <summary>
        /// The list of possible transitions from this state
        /// </summary>
        public MarkovModelStateTransition<T>[]? Transition { get; set; }

        internal MarkovModelObservation2(T item1, T item2, IEnumerable<MarkovModelStateTransition<T>>? transition = null)
        {
            Item1 = item1;
            Item2 = item2;
            Transition = transition?.ToArray();
        }

        /// <summary>
        /// Equals override
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
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
            return Item1.GetHashCode() ^ Item2.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString() => $"({Item1}, {Item2}) => {Transition?.Length ?? 0:N0}...";
    }

    /// <summary>
    /// A markov model based on observing two items at a time
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MarkovModel2<T> where T : notnull
    {
        /// <summary>
        /// The list of observations
        /// </summary>
        public MarkovModelObservation2<T>[] Observations { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="observations"></param>
        public MarkovModel2(IEnumerable<MarkovModelObservation2<T>> observations) => Observations = observations.ToArray();

        /// <summary>
        /// Converts the model to a dictionary
        /// </summary>
        public Dictionary<MarkovModelObservation2<T>, MarkovModelStateTransition<T>[]?> AsDictionary
        {
            get
            {
                return Observations.ToDictionary(o => o, o => o.Transition);
            }
        }
    }

    /// <summary>
    /// A markov model observation based on the preceding three instances
    /// </summary>
    /// <typeparam name="T">The data type</typeparam>
    public class MarkovModelObservation3<T> where T: notnull
    {
        /// <summary>
        /// The third last item
        /// </summary>
        public T Item1 { get; }

        /// <summary>
        /// The second last item
        /// </summary>
        public T Item2 { get; }

        /// <summary>
        /// The third last item
        /// </summary>
        public T Item3 { get; }

        /// <summary>
        /// The list of associated transitions
        /// </summary>
        public MarkovModelStateTransition<T>[]? Transitions { get; set; }

        internal MarkovModelObservation3(T item1, T item2, T item3, IEnumerable<MarkovModelStateTransition<T>>? transitions = null)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Transitions = transitions?.ToArray();
        }

        /// <summary>
        /// Equals override
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
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
            return Item1.GetHashCode() ^ Item2.GetHashCode() ^ Item3.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString() => $"({Item1}, {Item2}, {Item3}) => {Transitions?.Length ?? 0:N0}...";
    }

    /// <summary>
    /// A markov model based on observing the last three observations
    /// </summary>
    /// <typeparam name="T">The data type</typeparam>
    public class MarkovModel3<T> where T : notnull
    {
        /// <summary>
        /// The list of observations
        /// </summary>
        public MarkovModelObservation3<T>[] Observations { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="observations"></param>
        public MarkovModel3(IEnumerable<MarkovModelObservation3<T>> observations) => Observations = observations.ToArray();

        /// <summary>
        /// Converts the model to a dictionary
        /// </summary>
        public Dictionary<MarkovModelObservation3<T>, MarkovModelStateTransition<T>[]?> AsDictionary
        {
            get
            {
                return Observations.ToDictionary(o => o, o => o.Transitions);
            }
        }
    }
}
