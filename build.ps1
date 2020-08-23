param (
    [Switch] $Docker
)

function LogString ($stringToWrite) {
    Write-Host $stringToWrite -ForegroundColor DarkBlue
}

function StopAndRemovePreviousContainer ($container_name)  {
    LogString("Looking for existing containers from previous runs")
    $containers = docker ps -af "name=$container_name" --format "{{.ID}} {{.Names}}" | ConvertFrom-String -PropertyNames ID, Name

    if ($containers.Name -and $containers.Name.Contains($container_name)) {
        LogString("Stopping and removing container")
        docker stop $container_name
        docker rm $container_name
    }

}

function RunDocker {
    $today = Get-Date -Format "yy.MM.dd"

    LogString("Build docker images...")

    LogString("Data preparation image...")
    docker build --target data_preparation -t covid_sandbox_prepare:$today -t covid_sandbox_prepare:latest .
    LogString("Reports generator image...")
    docker build --target reports_generator -t covid_sandbox_generator:$today -t covid_sandbox_generator:latest .
    LogString("Reports processing image...")
    docker build --target reports_processor -t covid_sandbox_processing:$today -t covid_sandbox_processing:latest .

    LogString("Prepare data ...")
    StopAndRemovePreviousContainer("covid_sandbox_prepare_afd876")
    docker run -v ${pwd}/Data:/work/Data -v ${pwd}/Data/temp:/work/bin/Release/Data --name covid_sandbox_prepare_afd876 covid_sandbox_prepare

    LogString("Generate reports...")
    StopAndRemovePreviousContainer("covid_sandbox_generator_afd876")
    docker run -v ${pwd}/Data/temp:/work/Data -v ${pwd}/ReportsProcessing/reports:/work/reports --name covid_sandbox_generator_afd876 covid_sandbox_generator

    LogString("Run processing reports image ...")
    StopAndRemovePreviousContainer("covid_sandbox_processing_afd876")
    docker run -d -p 8888:8888 -v ${pwd}/ReportsProcessing:/work --name covid_sandbox_processing_afd876 covid_sandbox_processing
    Start-Sleep 1
    LogString("Image started")
    
    Write-Host (docker logs covid_sandbox_processing_afd876)
}

function RunLocal {
    LogString("Build util for reports conversion")
    dotnet.exe msbuild .\build\build.proj /t:"Build;PrepareData" /p:Configuration=Release
    dotnet.exe test --no-build -c:Release

    LogString("Run util to convert reports")
    Set-Location .\bin\Release
    dotnet.exe .\CovidSandbox.dll
    Set-Location ..\..

    LogString("Copy reports to reports processing folder")
    New-Item .\ReportsProcessing\ -Name "reports" -ItemType "directory" -Force
    Copy-Item .\bin\Release\reports .\ReportsProcessing -Recurse -Force
}

if ($Docker) {
    RunDocker
}
else {
    RunLocal
}