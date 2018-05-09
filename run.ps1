docker stop fabricdatabus
docker rm fabricdatabus
cd Fabric.Databus.API
dotnet publish --output obj/Docker/publish
# cp .\ca.crt obj\Docker\publish\
docker build -t fabricdatabus .
cd ..

docker run -p 5000:5000 -it --rm --name fabricdatabus fabricdatabus arg1 arg2
