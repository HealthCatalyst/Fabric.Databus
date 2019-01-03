docker stop fabricdatabus
docker rm fabricdatabus
cd Fabric.Databus.API
dotnet publish --output obj/Docker/publish
copy .\ca.crt obj\Docker\publish\
docker build -t fabricdatabus .
cd ..

# use host.docker.internal to connect to host from container

docker run -p 5000:5000 -it --rm --name fabricdatabus fabricdatabus arg1 arg2