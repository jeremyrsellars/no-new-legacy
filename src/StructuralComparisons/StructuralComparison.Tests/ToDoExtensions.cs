using System;

namespace Sellars
{
    internal static class ToDoExtensions
    {
        [Obsolete("To-do")]
        public static T ToDo<T>(this T value, string reason = null) => value;

        [Obsolete("Optimize")]
        public static T Optimize<T>(this T value, string reason = "because it's slow!") => value;
    }
}
