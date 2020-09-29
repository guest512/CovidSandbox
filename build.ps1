param (
    [switch] $Docker,
    [switch] $RunOnly
)

function Write-Log-String ($stringToWrite) {
    Write-Host $stringToWrite -ForegroundColor DarkBlue
}

function Write-Log-Error ($stringToWrite) {
    Write-Host $stringToWrite -ForegroundColor DarkRed  
}

function Confirm-Last-Result ($res) {
    if ($res) {
        Write-Log-String("SUCCESS")
    }
    
    if (!$res) {
        Write-Log-Error("FAIL")
        throw "Last command failed."
    }
}

function Stop-And-Remove-Previous-Container ($container_name) {
    Write-Log-String("Looking for existing containers from previous runs")
    $containers = docker ps -af "name=$container_name" --format "{{.ID}} {{.Names}}" | ConvertFrom-String -PropertyNames ID, Name

    if ($containers.Name -and $containers.Name.Contains($container_name)) {
        Write-Log-String("Stopping and removing container")
        docker stop $container_name
        docker rm $container_name
    }

}

function Build-Docker {
    $today = Get-Date -Format "yy.MM.dd"

    Write-Log-String("Build docker images...")

    Write-Log-String("Data preparation image...")
    docker build --target data_preparation -t covid_sandbox_prepare:$today -t covid_sandbox_prepare:latest .
    Confirm-Last-Result $?
    
    Write-Log-String("Reports generator image...")
    docker build --target reports_generator -t covid_sandbox_generator:$today -t covid_sandbox_generator:latest .
    Confirm-Last-Result $?

    Write-Log-String("Reports processing image...")
    docker build --target reports_processor -t covid_sandbox_processing:$today -t covid_sandbox_processing:latest .
    Confirm-Last-Result $?
}

function Start-Docker {
    Write-Log-String("Prepare data ...")
    docker run -v ${pwd}/Data:/work/Data -v ${pwd}/Data/temp:/work/bin/Release/Data --name covid_sandbox_prepare_afd876 --rm covid_sandbox_prepare

    Write-Log-String("Generate reports...")
    docker run -v ${pwd}/Data/temp:/work/Data -v ${pwd}/ReportsProcessing/reports:/work/reports --name covid_sandbox_generator_afd876 --rm covid_sandbox_generator

    Write-Log-String("Run processing reports image ...")
    Stop-And-Remove-Previous-Container("covid_sandbox_processing_afd876")
    docker run -d -p 8888:8888 -v ${pwd}/ReportsProcessing:/work --name covid_sandbox_processing_afd876 covid_sandbox_processing
    Start-Sleep 1
    Write-Log-String("Image started")
    
    Write-Log-String (docker logs covid_sandbox_processing_afd876)
}

function Build-Local {
    Write-Log-String("Build util for reports conversion")
    dotnet.exe msbuild .\build\build.proj /t:"Build;PrepareData" /p:Configuration=Release
    Confirm-Last-Result $?

    dotnet.exe test --no-build -c:Release
    Confirm-Last-Result $?

    Write-Log-String("Run util to convert reports")
    Set-Location .\bin\Release
    dotnet.exe .\CovidSandbox.dll
    Confirm-Last-Result $?

    Set-Location ..\..

    Write-Log-String("Copy reports to reports processing folder")
    New-Item .\ReportsProcessing\ -Name "reports" -ItemType "directory" -Force
    Copy-Item .\bin\Release\reports .\ReportsProcessing -Recurse -Force
}

if ($Docker) {
    if (!$RunOnly) {
        Build-Docker
    }

    Write-Log-String("Remove previous reports...")
    Remove-Item .\ReportsProcessing\reports -Recurse -Force
    Start-Docker
    Write-Log-String("Clean temp data...")
    Remove-Item .\Data\temp -Recurse -Force
}
else {
    Build-Local
}