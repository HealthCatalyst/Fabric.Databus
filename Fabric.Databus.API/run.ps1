#
# run.ps1
#

$here = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host "Open your browser to http://localhost:5000/api/values"

dotnet "$here\bin\debug\netcoreapp2.1\Fabric.Databus.API.dll"
