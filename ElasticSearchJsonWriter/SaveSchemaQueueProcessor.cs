using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ElasticSearchApiCaller;
using ElasticSearchSqlFeeder.Interfaces;
using ElasticSearchSqlFeeder.Shared;
using static System.String;

namespace ElasticSearchJsonWriter
{
    public class SaveSchemaQueueProcessor : BaseQueueProcessor<SaveSchemaQueueItem, MappingUploadQueueItem>
    {
        private readonly string _uploadUrl;
        private readonly string _mainMappingUploadRelativeUrl;
        private readonly string _secondaryMappingUploadRelativeUrl;
        private readonly string _bulkUploadRelativeUrl;

        public SaveSchemaQueueProcessor(QueueContext queueContext) : base(queueContext)
        {
            _uploadUrl = Config.Urls.First();
            _mainMappingUploadRelativeUrl = queueContext.MainMappingUploadRelativeUrl;
            _secondaryMappingUploadRelativeUrl = queueContext.SecondaryMappingUploadRelativeUrl;
            _bulkUploadRelativeUrl = queueContext.BulkUploadRelativeUrl;

            if (_uploadUrl.Last() == '/')
                _uploadUrl = _uploadUrl.Substring(1, _uploadUrl.Length-1);

            if (_mainMappingUploadRelativeUrl.First() != '/')
                _mainMappingUploadRelativeUrl = '/' + _mainMappingUploadRelativeUrl;
        }

        protected override void Handle(SaveSchemaQueueItem workitem)
        {
            //first send the base mapping
            //SendMapping(workitem.Mappings.First(m => IsNullOrEmpty(m.Key)));

            foreach (var mapping in workitem.Mappings.OrderBy(m => m.SequenceNumber).ToList())
            {
                SendMapping(mapping);
            }

            if (Config.WriteTemporaryFilesToDisk)
            {
                //WriteWindowsBatchFile();

                WriteBashBatchFile();
            }
        }

        protected override void Begin(bool isFirstThreadForThisTask)
        {
        }

        private void SendMapping(MappingItem mapping)
        {
            var stream = new MemoryStream(); // do not use using since we'll pass it to next queue

            var propertyPath = mapping.PropertyPath == Empty ? null : mapping.PropertyPath;

            using (var textWriter = new StreamWriter(stream, Encoding.UTF8, 1024, true))
            {
                EsJsonWriter.WriteMappingToStream(mapping.Columns, propertyPath, textWriter, mapping.PropertyType, Config.EntityType);
            }

            AddToOutputQueue(new MappingUploadQueueItem
            {
                PropertyName = mapping.PropertyPath,
                SequenceNumber = mapping.SequenceNumber,
                Stream = stream,
            });

            if (Config.WriteTemporaryFilesToDisk)
            {
                string path = Path.Combine(Config.LocalSaveFolder, (propertyPath != null ? $@"mapping-{mapping.SequenceNumber}-{propertyPath}.json" : "mainmapping.json"));
                using (var fileStream = File.Create(path))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(fileStream);

                    fileStream.Flush();
                }
            }

        }

#if FALSE
        private void WriteWindowsBatchFile()
        {
            string contents = $@"
SET ESURL={_uploadUrl}
SET PWD=changeme
SET USR=elastic

ECHO %ESURL%

:: delete index
curl -XDELETE -u %USR%:%PWD% %ESURL%/{_index}

:: wait 6 seconds
ping 127.0.0.1 -n 6 > nul

curl -XPUT -u %USR%:%PWD% %ESURL%{_mainMappingUploadRelativeUrl} --data-binary @mainmapping.json

:: wait 6 seconds
ping 127.0.0.1 -n 6 > nul


curl -XPUT -u %USR%:%PWD% %ESURL%/{_index}/_settings --data ""{{ \""index\"" : {{\""refresh_interval\"" : \""-1\"" }} }}""

time /T

for /f %%f in ('dir /b .\mapping*') do curl -XPUT -u %USR%:%PWD% %ESURL%{_secondaryMappingUploadRelativeUrl} --data-binary @%%f

time /T

for /f %%f in ('dir /b .\data*') do curl -XPOST -u %USR%:%PWD% %ESURL%{_bulkUploadRelativeUrl} --data-binary @%%f
time /T

curl -XPUT -u %USR%:%PWD% %ESURL%/{_index}/_settings --data ""{{ \""index\"" : {{\""refresh_interval\"" : \""1s\"" }} }}""

curl -XPOST -u %USR%:%PWD% %ESURL%/{_index}/_forcemerge?max_num_segments=5

:: remove alias
curl -XPOST -u %USR%:%PWD% %ESURL%/_aliases?pretty --data ""{{\""actions\"" : [{{ \""remove\"" : {{ \""index\"" : \""{_index}\"", \""alias\"" : \""{_alias}\"" }} }}]}}""

curl -XPOST -u %USR%:%PWD% %ESURL%/_aliases?pretty --data ""{{\""actions\"" : [{{ \""add\"" : {{ \""index\"" : \""{_index}\"", \""alias\"" : \""{_alias}\"" }} }}]}}""
";

            string path = Path.Combine(_folder, "loadOnWindows.cmd");
            File.WriteAllText(path, contents);
        }
#endif

        private void WriteBashBatchFile()
        {
            string contents = $@"#
set -e
set -x
#ESURL=""172.17.0.2:9200""
ESURL=""{_uploadUrl}""
USR=""elastic""
PWD=""changeme""

echo $ESURL

# delete index
curl -XDELETE -u $USR:$PWD $ESURL/{Config.Index}

# wait 6 seconds
sleep 6s;

curl -XPUT -u $USR:$PWD $ESURL{_mainMappingUploadRelativeUrl} --data-binary @mainmapping.json

# wait 6 seconds
sleep 6s;


curl -XPUT -u $USR:$PWD $ESURL/{Config.Index}/_settings -d '{{ ""index"" : {{ ""refresh_interval"" : ""-1""  }} }}'

shopt -s nullglob
for f in mapping*.*
do
    curl -XPUT -u $USR:$PWD $ESURL{_secondaryMappingUploadRelativeUrl} --data-binary @$f
done

for f in data*.*
do
"
+
(Config.CompressFiles
? $@"
curl -X POST -u $USR:$PWD -H 'Accept-Encoding: gzip' -H 'Content-Encoding: gzip' -H 'Content-Type: application/json' $ESURL'{_bulkUploadRelativeUrl}' --data-binary @$f  --compressed
"
: $@"
curl -X POST -u $USR:$PWD -H 'Content-Type: application/json' $ESURL'{_bulkUploadRelativeUrl}' --data-binary @$f
")
+ $@"
done

curl -XPUT -u $USR:$PWD $ESURL/{Config.Index}/_settings -d '{{ ""index"" : {{ ""refresh_interval"" : ""1s""  }} }}'

curl -XPOST -u $USR:$PWD $ESURL/{Config.Index}/_forcemerge'?max_num_segments=5'

# remove alias
curl -XPOST -u $USR:$PWD $ESURL/'_aliases?pretty' -H 'Content-Type: application/json' -d'{{ ""actions"" : [ {{ ""remove"" : {{ ""index"" : ""{Config.Index}"", ""alias"" : ""{Config.Alias}"" }} }}]}}'

# add alias
curl -XPOST -u $USR:$PWD $ESURL/'_aliases?pretty' -H 'Content-Type: application/json' -d'{{ ""actions"" : [ {{ ""add"" : {{ ""index"" : ""{Config.Index}"", ""alias"" : ""{Config.Alias}"" }} }}]}}'

";

            string path = Path.Combine(Config.LocalSaveFolder, "loadOnMacOrLinux.sh");
            contents = contents.Replace("\r", "");
            File.WriteAllText(path, contents, Encoding.UTF8);
        }


        protected override void Complete(string queryId, bool isLastThreadForThisTask)
        {
        }

        protected override string GetId(SaveSchemaQueueItem workitem)
        {
            return workitem.QueryId;
        }

        protected override string LoggerName => "SaveSchema";
    }

    public class SaveSchemaQueueItem : IQueueItem
    {
        public string QueryId { get; set; }
        public string PropertyName { get; set; }
        public List<MappingItem> Mappings { get; set; }
    }

}