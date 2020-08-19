param (
    [Switch] $Docker
)

function LogString ($stringToWrite) {
    Write-Host $stringToWrite -ForegroundColor DarkBlue
}

function StopAndRemovePreviousContainer {
    LogString("Looking for existing containers from previous runs")
    $containers = docker ps -af "name=covid_sandbox_afd876" --format "{{.ID}} {{.Names}}" | ConvertFrom-String -PropertyNames ID, Name

    if ($containers.Name -and $containers.Name.Contains("covid_sandbox_afd876")) {
        LogString("Stopping and removing container")
        docker stop covid_sandbox_afd876
        docker rm covid_sandbox_afd876
    }

}

function RunDocker {
    $today = Get-Date -Format "yy.MM.dd"

    LogString("Build docker image")
    docker build -t covid_sandbox:$today -t covid_sandbox:latest .
    StopAndRemovePreviousContainer
    LogString("Run new image ...")
    docker run -d -p 8888:8888 --name covid_sandbox_afd876 covid_sandbox
    Start-Sleep 1
    LogString("Image started")
    Write-Host (docker logs covid_sandbox_afd876)
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