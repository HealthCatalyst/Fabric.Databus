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

    public delegate void NavigatingEventHandler(object sender, NavigatingEventArgs e);
    public delegate void NavigatedEventHandler(object sender, NavigatedEventArgs e);
    public delegate void TransientErrorEventHandler(object sender, TransientErrorEventArgs e);
}