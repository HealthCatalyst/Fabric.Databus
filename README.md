# Fabric.Databus

Run the project (or get it from docker: curl -sSL https://healthcatalyst.github.io/InstallScripts/installfabricdatabus.txt | sh)

To test it:
Get Status of running jobs:
curl http://localhost:5000/jobstatus

You'll need a bearer token to validate and queue jobs. 
To get an access token (replace the url and client ID as needed, you'll need to obtain the client_secret):
curl -k https://fabric-identity.azurewebsites.net/connect/token --data "client_id=fabric-installer&grant_type=client_credentials" --data-urlencode "client_secret<client secret>"

To validate a job (make sure to include the bearer token in an authorization header - e.g. -H "Authorization:Bearer <bearer token>"):
curl -XPOST http://localhost:5000/validate --data-binary @job.xml


To post a new job (make sure to include the bearer token in an authorization header - e.g. -H "Authorization:Bearer <bearer token>"):
curl -XPOST http://localhost:5000/job --data-binary @cjob.xml

There is a sample config file in the configs/localhost folder.

# Architecture
Databus is based on pipelines and queues.

Each component of the pipeline is called a QueueProcessor.  It receives an input QueueItem and writes to an outpunt QueueItem.  The output QueueItem of one QueueProcessor is the input QueueItem of the next QueueProcessor.

The underlying queue can either be an in-memory queue or a distributed queue like rabbitmq.

One example of a queue would:
SqlGetSchemaQueueProcessor (reads the schema from a set of sql queries)
SaveSchemaQueueProcessor (saves the schema to files)
MappingUploadQueueProcessor (uploads schema to elasticsearch)
SqlJobQueueProcessor (processes a sqljob)
SqlBatchQueueProcessor (creates batches of data)
SqlImportQueueProcessor (reads data from sql queries)
ConvertDatabaseRowToJsonQueueProcessor (converts data to json)
JsonDocumentMergerQueueProcessor (merges json from multiple queries into one)
CreateBatchItemsQueueProcessor (creates batch files to send to REST API)
SaveBatchQueueProcessor (saves batch files)
FileUploadQueueProcessor (uploads json to REST API)

Benefits:
The queues can be monitored independently of the QueueProcessors.
Each QueueProcessor has a defined queue item coming and going out so the QueueProcessors are very pluggable
You can specify the number of instances of each QueueProcessor to use multiple threads
The queues allow work to flow without each QueueProcessor completing
The queue manager controls how much data is loaded into memory
Batching allows handling of large amounts of data by only loading small subsets in memory
We use Unity so various modules can be replaced and unit testing is easy
