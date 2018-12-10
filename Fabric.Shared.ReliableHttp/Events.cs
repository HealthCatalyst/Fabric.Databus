// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Events.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the NavigatingEventHandler type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Shared.ReliableHttp
{
    using Fabric.Shared.ReliableHttp.Events;

    /// <summary>
    /// The navigating event handler.
    /// </summary>
    /// <param name="sender">
    /// The sender.
    /// </param>
    /// <param name="e">
    /// The e.
    /// </param>
    public delegate void NavigatingEventHandler(object sender, NavigatingEventArgs e);

    /// <summary>
    /// The navigated event handler.
    /// </summary>
    /// <param name="sender">
    /// The sender.
    /// </param>
    /// <param name="e">
    /// The e.
    /// </param>
    public delegate void NavigatedEventHandler(object sender, NavigatedEventArgs e);

    /// <summary>
    /// The transient error event handler.
    /// </summary>
    /// <param name="sender">
    /// The sender.
    /// </param>
    /// <param name="e">
    /// The e.
    /// </param>
    public delegate void TransientErrorEventHandler(object sender, TransientErrorEventArgs e);
}