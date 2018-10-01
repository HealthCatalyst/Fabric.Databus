// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IElasticSearchUploaderFactory.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IElasticSearchUploaderFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ElasticSearchSqlFeeder.Interfaces
{
    using Serilog;

    /// <summary>
    /// The FileUploaderFactory interface.
    /// </summary>
    public interface IElasticSearchUploaderFactory
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
        IElasticSearchUploader Create(string userName, string password, bool keepIndexOnline);
    }
}
