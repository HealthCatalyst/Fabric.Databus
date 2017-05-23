using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using static System.String;

namespace ZipCodeToGeoCodeConverter
{
    public class ZipToGeocodeConverter
    {
        private static readonly Dictionary<string, GeoCode> ZipToGeocodeLookup = new Dictionary<string, GeoCode>();

        private static readonly Dictionary<string, GeoCode> Zip3ToGeocodeLookup = new Dictionary<string, GeoCode>();

        private static readonly object LockObject = new object();

        public GeoCode ConvertZipcodeToGeocode(string zipCode)
        {
            if (!ZipToGeocodeLookup.Any())
            {
                ReadEmbeddedFile();
            }

            var containsKey = ZipToGeocodeLookup.ContainsKey(zipCode);

            return containsKey
                ? ZipToGeocodeLookup[zipCode]
                : null;
        }

        public GeoCode Convert3DigitZipcodeToGeocode(string zipCodePrefix)
        {
            if (!ZipToGeocodeLookup.Any())
            {
                ReadEmbeddedFile();
            }

            if (IsNullOrWhiteSpace(zipCodePrefix)
                || zipCodePrefix.Any(c => !char.IsDigit(c))
                || zipCodePrefix == "000")
                return null;

            return Zip3ToGeocodeLookup.ContainsKey(zipCodePrefix) ? Zip3ToGeocodeLookup[zipCodePrefix]
                : null;
        }


        private void ReadEmbeddedFile()
        {
            lock (LockObject)
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
                //resourceName = resources[0];

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                using (StreamReader reader = new StreamReader(stream))
                {
                    //skip first line
                    reader.ReadLine();

                    string line = null;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var strings = line.Split(separator);
                        var latitudeAsText = strings[latitudeColumnIndex];
                        var longitudeAsText = strings[longitudeColumnIndex];

                        if (!IsNullOrWhiteSpace(latitudeAsText) && !IsNullOrWhiteSpace(longitudeAsText))
                        {
                            ZipToGeocodeLookup.Add(strings[zipCodeColumnIndex], new GeoCode
                            {
                                ZipCode = strings[zipCodeColumnIndex],
                                lat = Convert.ToDecimal(latitudeAsText),
                                lon = Convert.ToDecimal(longitudeAsText)
                            }
                        );
                        }
                    }

                    //string result = reader.ReadToEnd();
                }

                //create the zip3 version
                ZipToGeocodeLookup.ToList()
                    .ForEach(item =>
                    {
                        var zip3 = item.Key.Substring(0, 3);
                        if (!Zip3ToGeocodeLookup.ContainsKey(zip3))
                            Zip3ToGeocodeLookup.Add(zip3, item.Value);
                    });
            }
        }
    }

    public class GeoCode
    {
        public decimal lat { get; set; }
        public decimal lon { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public string ZipCode { get; internal set; }
    }
}
