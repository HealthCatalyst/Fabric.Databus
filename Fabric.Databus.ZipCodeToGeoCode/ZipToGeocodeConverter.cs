// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ZipToGeocodeConverter.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the ZipToGeocodeConverter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.ZipCodeToGeoCode
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The zip to geocode converter.
    /// </summary>
    public class ZipToGeocodeConverter
    {
        /// <summary>
        /// The zip to geocode lookup.
        /// </summary>
        private static readonly Dictionary<string, GeoCode> ZipToGeocodeLookup = new Dictionary<string, GeoCode>();

        /// <summary>
        /// The zip 3 to geocode lookup.
        /// </summary>
        private static readonly Dictionary<string, GeoCode> Zip3ToGeocodeLookup = new Dictionary<string, GeoCode>();

        /// <summary>
        /// The semaphore slim.
        /// </summary>
        private static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        /// <summary>
        /// The convert zipcode to geocode async.
        /// </summary>
        /// <param name="zipCode">
        /// The zip code.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<GeoCode> ConvertZipcodeToGeocodeAsync(string zipCode)
        {
            if (!ZipToGeocodeLookup.Any())
            {
                await this.ReadEmbeddedFileAsync();
            }

            var containsKey = ZipToGeocodeLookup.ContainsKey(zipCode);

            return containsKey
                ? ZipToGeocodeLookup[zipCode]
                : null;
        }

        /// <summary>
        /// The convert 3 digit zipcode to geocode async.
        /// </summary>
        /// <param name="zipCodePrefix">
        /// The zip code prefix.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<GeoCode> Convert3DigitZipcodeToGeocodeAsync(string zipCodePrefix)
        {
            if (!ZipToGeocodeLookup.Any())
            {
                await this.ReadEmbeddedFileAsync();
            }

            if (string.IsNullOrWhiteSpace(zipCodePrefix) || zipCodePrefix.Any(c => !char.IsDigit(c))
                                                         || zipCodePrefix == "000")
            {
                return null;
            }

            return Zip3ToGeocodeLookup.ContainsKey(zipCodePrefix) ? Zip3ToGeocodeLookup[zipCodePrefix]
                : null;
        }

        /// <summary>
        /// The read embedded file.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task ReadEmbeddedFileAsync()
        {
            // Asynchronously wait to enter the Semaphore. If no-one has been granted access to the Semaphore, code execution will proceed, otherwise this thread waits here until the semaphore is released 
            await semaphoreSlim.WaitAsync();
            try
            {
                //var assembly = Assembly.GetEntryAssembly();
                //var resourceName = "ZipCodeToGeoCodeConverter.2016_Gaz_zcta_national.txt";
                //var separator = '\t';

                // from http://www.unitedstateszipcodes.org/zip-code-database/
                var resourceName = "ZipCodeToGeoCodeConverter.zip_code_database.csv";
                var separator = ',';
                var zipCodeColumnIndex = 0;
                var latitudeColumnIndex = 8;
                var longitudeColumnIndex = 9;

                var assembly = typeof(GeoCode).GetTypeInfo().Assembly;

                var resources = assembly.GetManifestResourceNames();
                // resourceName = resources[0];

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                using (StreamReader reader = new StreamReader(stream))
                {
                    // skip first line
                    reader.ReadLine();

                    string line = null;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        var strings = line.Split(separator);
                        var latitudeAsText = strings[latitudeColumnIndex];
                        var longitudeAsText = strings[longitudeColumnIndex];

                        if (!string.IsNullOrWhiteSpace(latitudeAsText) && !string.IsNullOrWhiteSpace(longitudeAsText))
                        {
                            ZipToGeocodeLookup.Add(
                                strings[zipCodeColumnIndex],
                                new GeoCode
                                {
                                    ZipCode = strings[zipCodeColumnIndex],
                                    lat = Convert.ToDecimal(latitudeAsText),
                                    lon = Convert.ToDecimal(longitudeAsText)
                                });
                        }
                    }

                    // string result = reader.ReadToEnd();
                }

                // create the zip3 version
                ZipToGeocodeLookup.ToList().ForEach(
                    item =>
                    {
                        var zip3 = item.Key.Substring(0, 3);
                        if (!Zip3ToGeocodeLookup.ContainsKey(zip3))
                        {
                            Zip3ToGeocodeLookup.Add(zip3, item.Value);
                        }
                    });
            }
            finally
            {
                // When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                // This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                semaphoreSlim.Release();
            }
        }
    }
}
