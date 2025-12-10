using System;

namespace RVParking.Utilities
{
    public static class StringExtensions
    {
        /// <summary>
        /// Returns the first whitespace-delimited word from <paramref name="input"/>.
        /// Returns an empty string for null/empty/whitespace input.
        /// </summary>
        public static string FirstWord(this string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            input = input.TrimStart();

            int i = 0;
            while (i < input.Length && !char.IsWhiteSpace(input[i]))
                i++;

            return input.Substring(0, i);
        }

        /// <summary>
        /// Span-based version (alloc-free) that returns a slice containing the first word.
        /// </summary>
        public static ReadOnlySpan<char> FirstWordSpan(this ReadOnlySpan<char> span)
        {
            span = span.TrimStart();
            int i = 0;
            while (i < span.Length && !char.IsWhiteSpace(span[i]))
                i++;
            return span.Slice(0, i);
        }
    }
}