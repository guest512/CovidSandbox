#!/bin/bash

echo 'Build util for reports conversion'
dotnet msbuild ./build/build.proj /t:"Build;PrepareData" /p:Configuration=Release
dotnet test --no-build -c:Release

echo 'Run util to convert reports'
cd bin/Release
dotnet ./CovidSandbox.dll
cd ../..

echo 'Copy reports to reports processing folder'
cp bin/Release/reports ReportsProcessing/reports -r