#!/bin/bash

__source_dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
cd $__source_dir/../..


function _log_string() {
    no_color='\033[0m' # No Color
    printf "${1}%s${no_color}\n" "${2}"
}

function log_message() {
    color='\033[1;34m'
    _log_string $color "${1}"
}

function log_warning() {
    color='\033[1;33m'
    _log_string $color "${1}"
}

function log_error() {
    color='\033[31m'
    _log_string $color "${1}"
}

function check_result() {
    if [[ ${1} -eq 0 ]]; then
        log_message 'SUCCESS'
    else
        log_error 'FAIL'
        exit 1
    fi
    
}

function stop_and_remove_previous_container() {
    log_message 'Looking for existing containers from previous runs'
    existing_containers_count=$(docker ps -af "name=${1}" --format "{{.ID}} {{.Names}}" | grep -o ${1} | wc -l)

    if [[ "$existing_containers_count" -gt 0 ]]; then
        log_message 'Stopping and removing container'
        docker stop ${1}
        docker rm ${1}
    fi
}

function build_docker() {
    today=$(date +"%y.%m.%d")

    log_message 'Build docker images...'

    log_message 'Data preparation image...'
    docker build --target data_preparation -t covid_sandbox_prepare:$today -t covid_sandbox_prepare:latest -f ./scripts/docker/Dockerfile.tools .
    check_result $?

    log_message 'Reports generator image...'
    docker build --target reports_generator -t covid_sandbox_generator:$today -t covid_sandbox_generator:latest -f ./scripts/docker/Dockerfile.tools .
    check_result $?

    log_message 'Reports processing image...'
    docker build --target reports_processor -t covid_sandbox_processing:$today -t covid_sandbox_processing:latest -f ./scripts/docker/Dockerfile.processor .
    check_result $?
}

function start_docker() {
    log_message 'Prepare data ...'
    docker run -v $PWD/3rdparty/DataSources:/work/dataSources -v $PWD/Data:/work/data  --name covid_sandbox_prepare_afd876 covid_sandbox_prepare
    res=$?
    docker cp covid_sandbox_prepare_afd876:/work/out $PWD/temp
    docker rm covid_sandbox_prepare_afd876
    check_result res

    log_message 'Generate reports...'
    docker run -v $PWD/temp:/work/Data -v $PWD/ReportsProcessing/data:/work/out --name covid_sandbox_generator_afd876 --rm covid_sandbox_generator
    check_result $?

    log_message 'Clean temp data...'
    rm -rf $PWD/temp
    check_result $?

    log_message 'Run processing reports image ...'
    stop_and_remove_previous_container 'covid_sandbox_processing_afd876'
    docker run -d -p 8888:8888 -v $PWD/ReportsProcessing:/work --name covid_sandbox_processing_afd876 covid_sandbox_processing
    check_result $?
    log_message 'Image started'

    log_message 'Create script for video assets generation...'
    docker exec covid_sandbox_processing_afd876 jupyter nbconvert --to script --output temp_maps_generator AnimatedMapsGenerator.ipynb
    check_result $?

    log_message 'Generate video assets...'
    docker exec covid_sandbox_processing_afd876 python3 ./temp_maps_generator.py --min
    check_result $?

    log_message 'Remove temporary file...'
    docker exec covid_sandbox_processing_afd876 rm ./temp_maps_generator.py
    check_result $?

    log_message 'Generate final report...'
    New-Item .\ReportsProcessing\ -Name "out" -ItemType "directory" -Force
    docker exec covid_sandbox_processing_afd876 jupyter nbconvert --to html --no-input --execute --output ./out/index.html ReportsProcessing.ipynb
    check_result $?

    log_message 'Move results to wwwroot directory...'
    log_message '  Move assets...'
    cp $__source_dir/../ReportsProcessing/assets/* cp $__source_dir/../wwwroot/assets -r
    check_result $?
    log_message '  Move pages...'
    cp $__source_dir/../ReportsProcessing/out/* cp $__source_dir/../wwwroot -r
    check_result $?

    printf '\n'
    log_message '================================================================================'
    printf '\n'
    log_message 'You can continue to work with reports on the following address:'
    log_message 'http://127.0.0.1:8888?token=my_secure_token'
    printf '\n'
    log_message '================================================================================'
    printf '\n'
    printf '\n'
}

function build_local() {
    log_message 'Build util for reports conversion'
    dotnet msbuild ./Tools/build/build.proj /t:Build /p:Configuration=Release -v:n
    check_result $?

    dotnet test ./Tools/src --no-build -c:Release
    check_result $?
}

function start_local() {
    log_message 'Prepare data...'
    dotnet msbuild ./Tools/build/build.proj /t:PrepareData /p:Configuration=Release -v:n
    check_result $?

    log_message 'Run util to convert reports'
    cd bin/Release
    dotnet ./ReportsGenerator.dll
    cd $__source_dir/../..

    log_message 'Copy reports to reports processing folder'
    cp bin/Release/out ReportsProcessing/data -r
}

run_only=false
build_only=false
docker=false

while :; do
    case $1 in
        --run-only)
            if [ "$build_only" = false]; then
                run_only=true
            else
                log_warning "run_only and build_only options sets simultaneously. run_only is ignored."
            fi
            ;;
        --build-only)
            build_only=true
            if [ "$run_only" = true]; then
                log_warning "run_only and build_only options sets simultaneously. run_only is ignored."
                run_only=false
            fi
            ;;
        --docker)
            docker=true
            ;;
        -?*)
            log_warning "Unknown option (ignored): ${1}"
            ;;
        *)               # Default case: No more options, so break out of the loop.
            break
    esac
    shift
done

if [ "$run_only" = false ]; then
    if [ "$docker" = true ]; then
        build_docker    
    else
        build_local
    fi
fi

if [ "$build_only" = true ]; then
    exit 0
fi

if [ -d $__source_dir/../ReportsProcessing/reports ]; then
    log_message 'Remove previous reports...'
    rm -rf $__source_dir/../ReportsProcessing/reports
    check_result $?
fi

if [ -d $__source_dir/../ReportsProcessing/temp ]; then
    log_message 'Clean temp...'
    rm -rf $__source_dir/../ReportsProcessing/temp
    check_result $?
fi

if [ -d $__source_dir/../ReportsProcessing/assets ]; then
    log_message 'Remove generated assets...'
    rm -rf $__source_dir/../ReportsProcessing/assets
    check_result $?
fi

if [ ! -d $__source_dir/../ReportsProcessing/maps ]; then
    log_message 'Prepare maps...'
    cp $__source_dir/../3rdparty/Maps/NaturalEarth/10m_cultural $__source_dir/../ReportsProcessing/maps -r
fi

if [ "$docker" = true ]; then
    start_docker
else 
    start_local
fi