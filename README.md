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

