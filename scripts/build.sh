#!/bin/bash

__source_dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
cd $__source_dir/..


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
    docker build --target data_preparation -t covid_sandbox_prepare:$today -t covid_sandbox_prepare:latest .
    check_result $?

    logmessage 'Reports generator image...'
    docker build --target reports_generator -t covid_sandbox_generator:$today -t covid_sandbox_generator:latest .
    check_result $?

    log_message 'Reports processing image...'
    docker build --target reports_processor -t covid_sandbox_processing:$today -t covid_sandbox_processing:latest .
    check_result $?
}

function start_docker() {
    log_message 'Prepare data ...'
    docker run -v $PWD/3rdparty/DataSources:/work/dataSources -v $PWD/Data:/work/data -v $PWD/temp:/work/out --name covid_sandbox_prepare_afd876 --rm covid_sandbox_prepare
    check_result $?

    log_message 'Generate reports...'
    docker run -v $PWD/temp:/work/Data -v $PWD/ReportsProcessing/reports:/work/reports --name covid_sandbox_generator_afd876 --rm covid_sandbox_generator
    check_result $?

    log_message 'Clean temp data...'
    ls -l  $PWD/temp
    chown -R root $PWD/temp
    rm -rf $PWD/temp
    check_result $?

    log_message 'Run processing reports image ...'
    stop_and_remove_previous_container 'covid_sandbox_processing_afd876'
    docker run -d -p 8888:8888 -v $PWD/ReportsProcessing:/work --name covid_sandbox_processing_afd876 covid_sandbox_processing
    sleep 1
    log_message 'Image started'

    log_message 'Server is available on the following address: http://127.0.0.1:8888?token=my_secure_token'
}

function build_local() {
    log_message 'Build util for reports conversion'
    dotnet msbuild ./Tools/build/build.proj /t:"Build;PrepareData" /p:Configuration=Release
    check_result $?

    dotnet test ./Tools/src --no-build -c:Release
    check_result $?
}

function start_local() {
    log_message 'Run util to convert reports'
    cd bin/Release
    dotnet ./ReportsGenerator.dll
    cd $__source_dir/..

    log_message 'Copy reports to reports processing folder'
    cp bin/Release/reports ReportsProcessing/reports -r
}

run_only=false
docker=false

while :; do
    case $1 in
        --run-only)
            run_only=true
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

if [ -d $__source_dir/../ReportsProcessing/reports ]; then
    log_message 'Remove previous reports...'
    rm -rf $__source_dir/../ReportsProcessing/reports
    check_result $?
fi

if [ "$docker" = true ]; then
    start_docker
else 
    start_local
fi