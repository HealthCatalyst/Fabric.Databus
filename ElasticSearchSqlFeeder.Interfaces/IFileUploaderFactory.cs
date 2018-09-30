// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IFileUploaderFactory.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IFileUploaderFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ElasticSearchSqlFeeder.Interfaces
{
    using Serilog;

    /// <summary>
    /// The FileUploaderFactory interface.
    /// </summary>
    public interface IFileUploaderFactory
    {
        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="userName">
        /// The config elastic search user name.
        /// </param>
        /// <param name="password">
        /// The config elastic search password.
        /// </param>
        /// <param name="keepIndexOnline">
        /// The config keep index online.
        /// </param>
        /// <returns>
        /// The <see cref="FileUploader"/>.
        /// </returns>
        IFileUploader Create(string userName, string password, bool keepIndexOnline);
    }
}
