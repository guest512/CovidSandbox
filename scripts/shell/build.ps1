param (
    [switch] $Docker,
    [switch] $RunOnly,
    [switch] $BuildOnly
)

Set-Location ${PSScriptRoot}/../..

function Write-Log-String ($stringToWrite) {
    Write-Host $stringToWrite -ForegroundColor DarkBlue
}

function Write-Log-Warning ($stringToWrite) {
    Write-Host $stringToWrite -ForegroundColor DarkYellow
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
    docker build --pull --target data_preparation -t covid_sandbox_prepare:$today -t covid_sandbox_prepare:latest -f .\scripts\docker\Dockerfile.tools .
    Confirm-Last-Result $?

    Write-Log-String("Cache updater image...")
    docker build --pull --target cache_updater -t covid_sandbox_cache_updater:$today -t covid_sandbox_cache_updater:latest -f .\scripts\docker\Dockerfile.tools .
    Confirm-Last-Result $?

    Write-Log-String("Reports generator image...")
    docker build --pull --target reports_generator -t covid_sandbox_generator:$today -t covid_sandbox_generator:latest -f .\scripts\docker\Dockerfile.tools .
    Confirm-Last-Result $?

    Write-Log-String("Reports processing image...")
    docker build --pull --target reports_processor -t covid_sandbox_processing:$today -t covid_sandbox_processing:latest -f .\scripts\docker\Dockerfile.processor .
    Confirm-Last-Result $?
}

function Start-Docker {
    Write-Log-String("Prepare data...")
    docker run -v ${pwd}/3rdparty/DataSources:/work/dataSources -v ${pwd}/Data:/work/data --name covid_sandbox_prepare_afd876 covid_sandbox_prepare
    $res=$?
    docker cp covid_sandbox_prepare_afd876:/work/out ${pwd}/temp
    docker rm covid_sandbox_prepare_afd876
    Confirm-Last-Result $res

    Write-Log-String("Generate reports...")
    docker run -v ${pwd}/temp:/work/Data -v ${pwd}/ReportsProcessing/data:/work/out --name covid_sandbox_generator_afd876 --rm covid_sandbox_generator
    Confirm-Last-Result $?

    Write-Log-String("Update repo cache...")
    docker run -v ${pwd}/temp:/work/out -v ${pwd}/Data:/work/data --name covid_sandbox_cache_updater_afd876 --rm covid_sandbox_cache_updater
    Confirm-Last-Result $?

    Write-Log-String("Clean temp data...")
    Remove-Item temp -Recurse -Force
    Confirm-Last-Result $?

    Write-Log-String("Run processing reports image ...")
    Stop-And-Remove-Previous-Container("covid_sandbox_processing_afd876")
    docker run -d -p 8888:8888 -v ${pwd}/ReportsProcessing:/work --name covid_sandbox_processing_afd876 covid_sandbox_processing
    Confirm-Last-Result $?
    Write-Log-String("Image started")

    Write-Log-String("Create script for video assets generation...")
    docker exec covid_sandbox_processing_afd876 jupyter nbconvert --to script --output temp_maps_generator AnimatedMapsGenerator.ipynb
    Confirm-Last-Result $?

    Write-Log-String("Generate video assets...")
    docker exec covid_sandbox_processing_afd876 python3 ./temp_maps_generator.py --min
    Confirm-Last-Result $?

    Write-Log-String("Remove temporary file...")
    docker exec covid_sandbox_processing_afd876 rm ./temp_maps_generator.py
    Confirm-Last-Result $?

    Write-Log-String("Generate final report...")
    New-Item .\ReportsProcessing\ -Name "out" -ItemType "directory" -Force
    docker exec covid_sandbox_processing_afd876 jupyter nbconvert --to html --no-input --execute --output ./out/index.html ReportsProcessing.ipynb
    Confirm-Last-Result $?

    Write-Log-String("Move results to 'wwwroot' directory...")
    Write-Log-String("  Move assets...")
    If (!(Test-Path .\wwwroot\assets)) {
        New-Item .\wwwroot -Name "assets" -ItemType "directory"
    }
    Copy-Item .\ReportsProcessing\assets\* .\wwwroot\assets -Recurse -Force
    Confirm-Last-Result $?
    Write-Log-String("  Move pages...")
    Copy-Item .\ReportsProcessing\out\* .\wwwroot\ -Recurse -Force
    Confirm-Last-Result $?
    
    Write-Host You can continue to work with reports on the following address: http://127.0.0.1:8888?token=my_secure_token
}

function Build-Local {
    Write-Log-String("Build util for reports conversion")
    dotnet.exe msbuild Tools/build/build.proj /t:Build /p:Configuration=Release -v:n
    Confirm-Last-Result $?

    dotnet.exe test Tools/src --no-build -c:Release /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput="$PWD/bin/TestReports/"
    Confirm-Last-Result $?
}

function Start-Local {
    Write-Log-String("Prepare data...")
    dotnet msbuild Tools/build/build.proj /t:PrepareData /p:Configuration=Release -v:n
    Confirm-Last-Result $?

    Write-Log-String("Run util to convert reports")
    Set-Location ./bin/Release
    dotnet.exe .\ReportsGenerator.dll
    Confirm-Last-Result $?

    Set-Location ${PSScriptRoot}/../..

    Write-Log-String("Copy reports to reports processing folder")
    New-Item .\ReportsProcessing\ -Name "data" -ItemType "directory" -Force
    Copy-Item .\bin\Release\out\* .\ReportsProcessing\data -Recurse -Force
}

if ($BuildOnly -And $RunOnly) {
    Write-Log-Warning("RunOnly and BuildOnly switches used simultaneously. RunOnly is ignored.")
    $RunOnly = $False
}

if (!$RunOnly) {
    if ($Docker) {
        Build-Docker
    }
    else {
        Build-Local
    }
}

if ($BuildOnly) {
    exit 0
}


if (Test-Path .\ReportsProcessing\data) {
    Write-Log-String("Remove previous reports...")
    Remove-Item .\ReportsProcessing\data -Recurse -Force
    Confirm-Last-Result $?
}

if (Test-Path .\ReportsProcessing\assets) {
    Write-Log-String("Remove generated assets...")
    Remove-Item .\ReportsProcessing\assets -Recurse -Force
    Confirm-Last-Result $?
}

if (Test-Path .\ReportsProcessing\temp) {
    Write-Log-String("Clean temp...")
    Remove-Item .\ReportsProcessing\temp -Recurse -Force
    Confirm-Last-Result $?
}

if (!(Test-Path .\ReportsProcessing\maps)) {
    Write-Log-String("Prepare maps...")
    Copy-Item .\3rdparty\Maps\NaturalEarth\10m_cultural .\ReportsProcessing\maps -Recurse
    Confirm-Last-Result $?
}

if ($Docker) {
    Start-Docker
}
else {
    Start-Local
}