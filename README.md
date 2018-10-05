# Architecture
Databus is based on pipelines and queues.

Each component of the pipeline is called a PipelineStep.  It receives an input QueueItem and writes to an output QueueItem.  The output QueueItem of one PipelineStep is the input QueueItem of the next PipelineStep.

The underlying queue can either be an in-memory queue or a distributed queue like rabbitmq.

One example of a queue would:

SqlGetSchemaPipelineStep (reads the schema from a set of sql queries) 

SaveSchemaPipelineStep (saves the schema to files)

MappingUploadPipelineStep (uploads schema to elasticsearch)

SqlJobPipelineStep (processes a sqljob)

SqlBatchPipelineStep (creates batches of data)

SqlImportPipelineStep (reads data from sql queries)

ConvertDatabaseRowToJsonPipelineStep (converts data to json)

JsonDocumentMergerPipelineStep (merges json from multiple queries into one)

CreateBatchItemsPipelineStep (creates batch files to send to REST API)

SaveBatchPipelineStep (saves batch files)

FileUploadPipelineStep (uploads json to REST API)


Benefits:

The queues can be monitored independently of the PipelineStep.

Each PipelineStep has a defined queue item coming and going out so the QPipelineSteps are very pluggable

You can specify the number of instances of each PipelineStep to use multiple threads

The queues allow work to flow without each PipelineStep completing

The queue manager controls how much data is loaded into memory

Batching allows handling of large amounts of data by only loading small subsets in memory

We use Unity so various modules can be replaced and unit testing is easy

# Running via console
You can run ElasticSearchSqlFeederConsole and pass it a xml file.  There is a sample fhir.xml.

# Running Fabric.Databus via REST API and Docker

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

# Creating nested json

Patients table

| PatientID  | PatientLastNM |
| ------------- | ------------- |
| 1  | Jones  |
| 2  | McConnell  |


PatientDiagnosis table

| DiagnosisID  | PatientID |  DiagnosisCD |
| ------------- | ------------- | ---------- |
| 1  | 1  |  E11.3 |
| 2  | 1  | E 13.4 !




