// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileUploaderFactory.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the FileUploaderFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ElasticSearchApiCaller
{
    /// <summary>
    /// The file uploader factory.
    /// </summary>
    public class FileUploaderFactory : IFileUploaderFactory
    {
        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="userName">
        ///     The config elastic search user name.
        /// </param>
        /// <param name="password">
        ///     The config elastic search password.
        /// </param>
        /// <param name="keepIndexOnline">
        ///     The config keep index online.
        /// </param>
        /// <returns>
        /// The <see cref="FileUploader"/>.
        /// </returns>
        public IFileUploader Create(string userName, string password, bool keepIndexOnline)
        {
            return new FileUploader(userName, password, keepIndexOnline);
        }
    }
}
