#!/bin/bash

function logstring {
    color='\033[1;34m'
    no_color='\033[0m' # No Color
    echo -e "${color}${1}${no_color}"
}

logstring 'Build util for reports conversion'
dotnet msbuild ./build/build.proj /t:"Build;PrepareData" /p:Configuration=Release
dotnet test --no-build -c:Release

logstring 'Run util to convert reports'
cd bin/Release
dotnet ./CovidSandbox.dll
cd ../..

logstring 'Copy reports to reports processing folder'
cp bin/Release/reports ReportsProcessing/reports -r