// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InitializerExtensions.cs" company="">
//   
// </copyright>
// <summary>
//   The initializer extensions.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PipelineRunnerTests
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The initializer extensions.
    /// </summary>
    public static class InitializerExtensions
    {
        /// <summary>
        /// The add.
        /// </summary>
        /// <param name="target">
        /// The target.
        /// </param>
        /// <param name="item1">
        /// The item 1.
        /// </param>
        /// <param name="item2">
        /// The item 2.
        /// </param>
        /// <typeparam name="T1">first param
        /// </typeparam>
        /// <typeparam name="T2">second param
        /// </typeparam>
        /// <exception cref="ArgumentNullException">argument is null
        /// </exception>
        public static void Add<T1, T2>(this ICollection<KeyValuePair<T1, T2>> target, T1 item1, T2 item2)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            target.Add(new KeyValuePair<T1, T2>(item1, item2));
        }
    }
}