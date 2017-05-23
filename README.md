# Fabric.Databus

Run the project (or get it from docker: curl -sSL https://healthcatalyst.github.io/InstallScripts/installfabricdatabus.txt | sh)

To test it:
Get Status of running jobs:
curl http://localhost:5000/jobstatus

To validate a job:
curl -XPOST http://localhost:5000/validate --data-binary @job.xml


To post a new job:
curl -XPOST http://localhost:5000/job --data-binary @cjob.xml

There is a sample cofig file in the configs/localhost folder.

