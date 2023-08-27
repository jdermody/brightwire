using System;
using System.Diagnostics;
using System.Text;

namespace BrightData.Helper
{
    /// <summary>
    /// Writes progress notifications to the console
    /// </summary>
    public class ConsoleProgressNotification : INotifyUser
    {
        int _progress = 0;
        readonly Stopwatch _stopWatch = new();

        /// <inheritdoc />
        public void OnStartOperation(string operationId, string? msg)
        {
            if(msg is not null)
                Console.WriteLine(msg);

            _progress = -1;
            _stopWatch.Restart();
        }

        /// <inheritdoc />
        public void OnOperationProgress(string operationId, float progressPercent)
        {
            WriteProgress(progressPercent, ref _progress, _stopWatch);
        }

        /// <inheritdoc />
        public void OnCompleteOperation(string operationId, bool wasCancelled)
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
                    sb.Append(i < newProgress ? '█' : '_');
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
                    sb.Append('█');
                var fore = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(sb.ToString());

                sb.Clear();
                for (; i < 100; i++)
                    sb.Append('█');
                sb.Append($" {progress:P0}");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(sb.ToString());

                Console.ForegroundColor = fore;
                Console.Write($" {sw.Elapsed.Minutes:00}:{sw.Elapsed.Seconds:00}:{sw.Elapsed.Milliseconds:0000}");

                previousPercentage = curr;
                return true;
            }

            return false;
        }
    }
}
