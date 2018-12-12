// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DatabusDataSourceException.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the DatabusDataSourceException type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    using Fabric.Databus.Interfaces.Config;

    /// <inheritdoc />
    public class DatabusDataSourceException : Exception
    {
        private readonly IDataSource dataSource;

        private readonly Exception innerException;

        /// <inheritdoc />
        public DatabusDataSourceException()
        {
        }

        /// <inheritdoc />
        public DatabusDataSourceException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public DatabusDataSourceException(string message, Exception innerException) : base(message, innerException)
        {
        }


        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Fabric.Databus.Interfaces.Exceptions.DatabusDataSourceException" /> class.
        /// </summary>
        /// <param name="dataSource">
        /// The data source.
        /// </param>
        /// <param name="innerException">
        /// The inner exception.
        /// </param>
        public DatabusDataSourceException(IDataSource dataSource, Exception innerException) : base("DataSource=" + dataSource.Path, innerException)
        {
            this.dataSource = dataSource;
            this.innerException = innerException;
        }

        /// <inheritdoc />
        protected DatabusDataSourceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

    }
}
