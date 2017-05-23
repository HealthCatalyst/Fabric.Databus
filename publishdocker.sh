@echo off
read -n 1 -p 'Are you sure you want to publish to dockerhub?'
echo "(ok)"

echo Logging in to dockerhub as healthcatalyst  
docker login --username healthcatalyst

docker stop fabric.databus
docker rm fabric.databus

cd Fabric.Databus.API
dotnet publish --configuration Release --output obj/Docker/publish
cp ./ca.crt obj/Docker/publish/

docker build -t healthcatalyst/fabric.databus .
docker push healthcatalyst/fabric.databus
cd ..
echo Press any key to exit
read -n 1
