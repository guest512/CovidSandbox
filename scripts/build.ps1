param (
    [switch] $Docker,
    [switch] $RunOnly
)

Set-Location ${PSScriptRoot}/..

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
    docker run -v ${pwd}/3rdparty/DataSources:/work/dataSources -v ${pwd}/Data:/work/data --name covid_sandbox_prepare_afd876 covid_sandbox_prepare
    $res=$?
    docker cp covid_sandbox_prepare_afd876:/work/out ${pwd}/temp
    docker rm covid_sandbox_prepare_afd876
    Confirm-Last-Result $?

    Write-Log-String("Generate reports...")
    docker run -v ${pwd}/temp:/work/Data -v ${pwd}/ReportsProcessing/data:/work/out --name covid_sandbox_generator_afd876 --rm covid_sandbox_generator
    Confirm-Last-Result $?

    Write-Log-String("Clean temp data...")
    Remove-Item temp -Recurse -Force
    Confirm-Last-Result $?

    Write-Log-String("Run processing reports image ...")
    Stop-And-Remove-Previous-Container("covid_sandbox_processing_afd876")
    docker run -d -p 8888:8888 -v ${pwd}/ReportsProcessing:/work --name covid_sandbox_processing_afd876 covid_sandbox_processing
    Start-Sleep 1
    Write-Log-String("Image started")
    
    Write-Host Server is available on the following address: http://127.0.0.1:8888?token=my_secure_token
}

function Build-Local {
    Write-Log-String("Build util for reports conversion")
    dotnet.exe msbuild Tools/build/build.proj /t:"Build;PrepareData" /p:Configuration=Release
    Confirm-Last-Result $?

    dotnet.exe test Tools/src --no-build -c:Release
    Confirm-Last-Result $?
}

function Start-Local {
    Write-Log-String("Run util to convert reports")
    Set-Location ./bin/Release
    dotnet.exe .\ReportsGenerator.dll
    Confirm-Last-Result $?

    Set-Location ${PSScriptRoot}/..

    Write-Log-String("Copy reports to reports processing folder")
    New-Item .\ReportsProcessing\ -Name "data" -ItemType "directory" -Force
    Copy-Item .\bin\Release\out\* .\ReportsProcessing\data -Recurse -Force
}

if(!$RunOnly){
    if($Docker){
        Build-Docker
    }
    else {
        Build-Local
    }
}

if (Test-Path ${PSScriptRoot}/../ReportsProcessing/reports) {
    Write-Log-String("Remove previous reports...")
    Remove-Item ${PSScriptRoot}/../ReportsProcessing/reports -Recurse -Force
    Confirm-Last-Result $?
}

if ($Docker) {
    Start-Docker
}
else {
    Start-Local
}