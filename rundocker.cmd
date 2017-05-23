@echo off
docker stop fabricdatabus
docker rm fabricdatabus
cd Fabric.Databus.API
dotnet publish --output obj/Docker/publish
copy .\ca.crt obj\Docker\publish\
docker build -t fabricdatabus .
cd ..

for /f "tokens=1-2 delims=:" %%a in ('ipconfig^|find "IPv4"') do set ip=%%b
set ip=%ip:~1%
echo %ip%

docker run -p 5000:5000 --add-host dockerhost:%ip% -it --rm --name fabricdatabus fabricdatabus arg1 arg2