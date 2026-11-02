#!/bin/sh
REPORTTOOL=""
dotnet tool list -g dotnet-reportgenerator-globaltool
if [ 0 -eq $? ]; then
	REPORTTOOL=reportgenerator
else
	dotnet tool restore
	REPORTTOOL=dotnet reportgenerator
fi
dotnet build
dotnet test --no-build --maxcpucount:1
$REPORTTOOL -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:TestResults/report -reporttypes:Html -assemblyfilters:"-*.Tests"
OPEN=$(which xdg-open || which open || which gnome-open)
if [ -z $OPEN ]; then
    echo "Open browser manually and navigage to 'TestResults/report/index.html'"
else
    $OPEN TestResults/report/index.html
fi
