﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrightData.Buffer.Operations
{
    /// <summary>
    /// Mutates values from a buffer and writes to a destination
    /// </summary>
    /// <typeparam name="FT"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="mutator"></param>
    internal class OneToOneMutation<FT, T>(IReadOnlyBuffer<FT> from, IAppendToBuffer<T> to, OneToOneMutation<FT, T>.IWriteMutatedBlocks mutator)
        : IOperation
        where FT : notnull
        where T : notnull
    {
        internal interface IWriteMutatedBlocks
        {
            void Write(ReadOnlySpan<FT> from, IAppendToBuffer<T> to);
        }

        public Task Execute(INotifyOperationProgress? notify = null, string? msg = null, CancellationToken ct = default)
        {
            return notify is not null 
                ? from.ForEachWithProgressNotification(notify, x => mutator.Write(x, to), msg, ct) 
                : from.ForEachBlock(x => mutator.Write(x, to), ct)
            ;
        }
    }
}
