using System.Text;

namespace Neotales.Text
{
    /// <summary>
    /// A thread-local <see cref="StringBuilder"/> reused by short-lived formatting.
    /// </summary>
    public static class StringBuilderCache
    {
        private const int MaxBuilderSize = 360;
        private const int DefaultCapacity = 16;

        [ThreadStatic]
        private static StringBuilder? cached;

        /// <summary>Returns a cleared builder, from this thread's cache when it fits.</summary>
        /// <param name="capacity">The capacity the caller needs.</param>
        /// <returns>A builder with at least <paramref name="capacity"/>.</returns>
        public static StringBuilder Acquire(int capacity = DefaultCapacity)
        {
            StringBuilder? builder = cached;
            if (builder != null && capacity <= builder.Capacity && builder.Capacity <= MaxBuilderSize)
            {
                cached = null;
                builder.Clear();
                return builder;
            }

            return new StringBuilder(capacity);
        }

        /// <summary>Returns a builder to this thread's cache when it is small enough to keep.</summary>
        /// <param name="sb">The builder to keep.</param>
        public static void Release(StringBuilder sb)
        {
            if (sb.Capacity <= MaxBuilderSize)
            {
                cached = sb;
            }
        }

        /// <summary>Renders the builder and returns it to the cache.</summary>
        /// <param name="sb">The builder to render.</param>
        /// <returns>The rendered text.</returns>
        public static string GetStringAndRelease(StringBuilder sb)
        {
            string text = sb.ToString();
            Release(sb);
            return text;
        }
    }
}
