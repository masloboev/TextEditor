using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextEditor.Attributes;

namespace TextEditor.UnitTests.Utils
{
    /// <summary>
    ///     Utility for text generation
    /// </summary>
    public class TextGenerator
    {
        /// <summary>
        /// Generates text based on stringss basis. Produces all combinations with all permutations
        /// </summary>
        /// <param name="strings">Generation basis list</param>
        /// <returns>
        /// Generated text
        /// </returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        [return: NotNull]
        public string GenerateText([NotNull] string[] strings)
        {
            if (strings == null) throw new ArgumentNullException(nameof(strings));

            var sb = new StringBuilder();
            var selection = new List<string>(strings.Length);
            var maxSelection = 1 << strings.Length;
            // combination producing
            for (var i = 1; i < maxSelection; i++)
            {
                selection.AddRange(strings.Where((t, idx) => (i & (1 << idx)) != 0));
                GeneratePermutations(selection, 0, sb);
                selection.Clear();
            }
            return sb.ToString();
        }

        /// <summary>
        ///     Swaps two elements in list
        /// </summary>
        /// <param name="list">List to elements swap in</param>
        /// <param name="i">first element index</param>
        /// <param name="j">second element index</param>
        private static void Swap(IList<string> list, int i, int j)
        {
            var t = list[i];
            list[i] = list[j];
            list[j] = t;
        }

        /// <summary>
        ///     Recursive permutation algorithm
        /// </summary>
        /// <param name="strings">Elements list to make permutations</param>
        /// <param name="k">Start index to make permutations</param>
        /// <param name="sb">String builder to write resulting permutations</param>
        private static void GeneratePermutations(IList<string> strings, int k, StringBuilder sb)
        {
            if (k == strings.Count - 1)
            {
                foreach (var s in strings)
                    sb.Append(s);
                return;
            }
            for (var i = k; i <= strings.Count - 1; i++)
            {
                Swap(strings, k, i);
                GeneratePermutations(strings, k + 1, sb);
                Swap(strings, k, i);
            }
        }
    }
}
