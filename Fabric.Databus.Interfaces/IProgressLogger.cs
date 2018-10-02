// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IProgressLogger.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IProgressLogger type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces
{
    /// <summary>
    /// The ProgressLogger interface.
    /// </summary>
    public interface IProgressLogger
    {
        /// <summary>
        /// The reset.
        /// </summary>
        void Reset();

        /// <summary>
        /// The get log.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string GetLog();

        /// <summary>
        /// The log progress monitor item.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="progressMonitorItem">
        /// The progress monitor item.
        /// </param>
        void LogProgressMonitorItem(int key, ProgressMonitorItem progressMonitorItem);

        /// <summary>
        /// The log header.
        /// </summary>
        void LogHeader();
    }
}