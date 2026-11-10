using System.Runtime.CompilerServices;

namespace Neotales.Extras
{
    /// <summary>
    /// Short names for <see cref="Task.ConfigureAwait(bool)"/>.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>Awaits on the captured context.</summary>
        /// <param name="t">The task to configure.</param>
        /// <returns>The configured awaitable.</returns>
        public static ConfiguredTaskAwaitable Cap(this Task t) => t.ConfigureAwait(true);

        /// <summary>Awaits on the captured context.</summary>
        /// <typeparam name="T">The task result type.</typeparam>
        /// <param name="t">The task to configure.</param>
        /// <returns>The configured awaitable.</returns>
        public static ConfiguredTaskAwaitable<T> Cap<T>(this Task<T> t) => t.ConfigureAwait(true);

        /// <summary>Awaits on the captured context.</summary>
        /// <typeparam name="T">The value-task result type.</typeparam>
        /// <param name="t">The value task to configure.</param>
        /// <returns>The configured awaitable.</returns>
        public static ConfiguredValueTaskAwaitable<T> Cap<T>(this ValueTask<T> t) => t.ConfigureAwait(true);

        /// <summary>Awaits on the captured context.</summary>
        /// <param name="t">The value task to configure.</param>
        /// <returns>The configured awaitable.</returns>
        public static ConfiguredValueTaskAwaitable Cap(this ValueTask t) => t.ConfigureAwait(true);

        /// <summary>Awaits without capturing the context.</summary>
        /// <param name="t">The task to configure.</param>
        /// <returns>The configured awaitable.</returns>
        public static ConfiguredTaskAwaitable NoCap(this Task t) => t.ConfigureAwait(false);

        /// <summary>Awaits without capturing the context.</summary>
        /// <typeparam name="T">The task result type.</typeparam>
        /// <param name="t">The task to configure.</param>
        /// <returns>The configured awaitable.</returns>
        public static ConfiguredTaskAwaitable<T> NoCap<T>(this Task<T> t) => t.ConfigureAwait(false);

        /// <summary>Awaits without capturing the context.</summary>
        /// <param name="t">The value task to configure.</param>
        /// <returns>The configured awaitable.</returns>
        public static ConfiguredValueTaskAwaitable NoCap(this ValueTask t) => t.ConfigureAwait(false);

        /// <summary>Awaits without capturing the context.</summary>
        /// <typeparam name="T">The value-task result type.</typeparam>
        /// <param name="t">The value task to configure.</param>
        /// <returns>The configured awaitable.</returns>
        public static ConfiguredValueTaskAwaitable<T> NoCap<T>(this ValueTask<T> t) => t.ConfigureAwait(false);
    }
}
