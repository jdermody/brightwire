﻿using System;
using System.Diagnostics;
using System.Text;

namespace BrightData.Helper
{
    /// <summary>
    /// Writes progress notifications to the console
    /// </summary>
    public class ConsoleProgressNotification : INotifyOperationProgress
    {
        const char PROGRESS_CHAR = '\u2588';
        int _progress = 0;
        readonly Stopwatch _stopWatch = new();

        /// <inheritdoc />
        public void OnStartOperation(Guid operationId, string? msg)
        {
            if (msg is not null) {
                Console.WriteLine();
                Console.WriteLine(msg);
            }

            _progress = -1;
            _stopWatch.Restart();
        }

        /// <inheritdoc />
        public void OnOperationProgress(Guid operationId, float progressPercent)
        {
            WriteProgress(progressPercent, ref _progress, _stopWatch);
        }

        /// <inheritdoc />
        public void OnCompleteOperation(Guid operationId, bool wasCancelled)
        {
            _stopWatch.Stop();
            //Console.WriteLine();
        }

        /// <inheritdoc />
        public void OnMessage(string msg)
        {
            Console.WriteLine(msg);
        }

        /// <summary>
        /// Writes a progress bar to the console
        /// </summary>
        /// <param name="newProgress">New progress</param>
        /// <param name="oldProgress">Current progress</param>
        /// <param name="max">Max progress amount (default 100)</param>
        /// <returns>True if the progress has increased</returns>
        public static bool WriteProgress(int newProgress, ref int oldProgress, int max = 100)
        {
            if (newProgress > oldProgress) {
                var sb = new StringBuilder();
                sb.Append('\r');
                for (var i = 0; i < max; i++)
                    sb.Append(i < newProgress ? PROGRESS_CHAR : '_');
                sb.Append($" ({oldProgress = newProgress}%)");
                Console.Write(sb.ToString());
                return true;
            }

            return false;
        }

        /// <summary>
        /// Writes a progress bar to the console
        /// </summary>
        /// <param name="progress">New progress (between 0 and 1)</param>
        /// <param name="previousPercentage">Current progress percentage (max 100)</param>
        /// <param name="sw">Stopwatch since start of operation</param>
        /// <returns>True if the progress has increased</returns>
        public static bool WriteProgress(float progress, ref int previousPercentage, Stopwatch sw)
        {
            var curr = Convert.ToInt32(progress * 100);
            if (curr > previousPercentage) {
                var sb = new StringBuilder();
                sb.Append('\r');
                var i = 0;
                for (; i < curr; i++)
                    sb.Append(PROGRESS_CHAR);
                var fore = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write(sb.ToString());

                sb.Clear();
                for (; i < 100; i++)
                    sb.Append(PROGRESS_CHAR);
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.Write(sb.ToString());
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write($" {progress:P0}");

                Console.ForegroundColor = fore;
                Console.Write($" {sw.Elapsed.Minutes:00}:{sw.Elapsed.Seconds:00}:{sw.Elapsed.Milliseconds:0000}");

                previousPercentage = curr;
                return true;
            }

            return false;
        }
    }
}
