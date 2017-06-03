using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightWire.TrainingData.Helper
{
    /// <summary>
    /// Finds strings of words and numbers in a larger string
    /// </summary>
    public static class SimpleTokeniser
    {
        enum ParseState
        {
            None,
            InWord,
            InNumber
        }

        /// <summary>
        /// Splits the text into smaller word and number strings. Punctuation characters become single character strings.
        /// </summary>
        /// <param name="text">The text to tokenise</param>
        public static IEnumerable<string> Tokenise(string text)
        {
            var sb = new StringBuilder();
            var parseState = ParseState.None;

            foreach (var ch in text) {
                var isWord = Char.IsLetter(ch);
                var isNumber = Char.IsNumber(ch);
                var isSpace = Char.IsWhiteSpace(ch);

                // punctuation characters become single tokens
                if (!isSpace && !isWord && !isNumber) {
                    if (sb.Length > 0) {
                        yield return sb.ToString();
                        sb.Clear();
                    }
                    yield return new string(ch, 1);
                    parseState = ParseState.None;
                    continue;
                }

                // otherwise try to form a multi character token
                if ((isWord && parseState != ParseState.InWord) || (isNumber && parseState != ParseState.InNumber) || (isSpace && parseState != ParseState.None)) {
                    if (sb.Length > 0) {
                        yield return sb.ToString();
                        sb.Clear();
                    }
                    parseState = isWord ? ParseState.InWord : isNumber ? ParseState.InNumber : ParseState.None;
                }
                if (!isSpace)
                    sb.Append(ch);
            }
            if (sb.Length > 0)
                yield return sb.ToString();
        }

        /// <summary>
        /// Simple token modification following "not".
        /// Double not or punctuation stops the not mode
        /// </summary>
        /// <param name="tokenList">The list of tokens</param>
        /// <returns>A sequence of modified tokens</returns>
        public static IEnumerable<string> JoinNegations(IEnumerable<string> tokenList)
        {
            var inNot = false;

            foreach (var item in tokenList) {
                var isNot = item == "not";
                if (!isNot)
                    yield return inNot ? "not_" + item : item;

                if (isNot)
                    inNot = !inNot;
                else if (!Char.IsLetterOrDigit(item[0]))
                    inNot = false;
            }
        }

        /// <summary>
        /// Finds sentences from a list of strings
        /// </summary>
        /// <param name="stringList">The list of strings</param>
        public static IEnumerable<IReadOnlyList<string>> FindSentences(IEnumerable<string> stringList)
        {
            var curr = new List<string>();
            foreach (var item in stringList) {
                curr.Add(item);
                if (IsEndOfSentence(item)) {
                    yield return curr.ToArray();
                    curr.Clear();
                }
            }
            if (curr.Any())
                yield return curr.ToArray();
        }

        /// <summary>
        /// Checks if the string is an end of sentence token
        /// </summary>
        /// <param name="str">The string to check</param>
        public static bool IsEndOfSentence(string str)
        {
            return str == "." || str == "!" || str == "?";
        }
    }
}
