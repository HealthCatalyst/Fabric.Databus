FROM microsoft/dotnet:2.1-aspnetcore-runtime
WORKDIR /app
EXPOSE 5000
COPY ${source:-obj/Docker/publish} .

ADD ${source:-obj/Docker/publish}/ca.crt /usr/local/share/ca-certificates
RUN /usr/sbin/update-ca-certificates

RUN mkdir -p /home/hcadmin/json

ENTRYPOINT ["dotnet", "Fabric.Databus.API.dll"]
