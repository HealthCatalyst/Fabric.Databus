docker stop fabricdatabus
docker rm fabricdatabus
docker pull healthcatalyst/fabric.databus

docker run -p 5000:5000 --net=host -it --rm --name fabricdatabus healthcatalyst/fabricdatabus arg1 arg2
