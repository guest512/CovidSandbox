#!/bin/bash

function logstring {
    color='\033[1;34m'
    no_color='\033[0m' # No Color
    echo -e "${color}${1}${no_color}"
}

function stop_and_remove_previous_container {
    logstring 'Looking for existing containers from previous runs'
    existing_containers_count=$(docker ps -af "name=${1}" --format "{{.ID}} {{.Names}}" | grep -o ${1} | wc -l)

    if [[ "$existing_containers_count" -gt 0 ]]; then
        logstring 'Stopping and removing container'
        docker stop ${1}
        docker rm ${1}
    fi
}


logstring 'Prepare data ...'
docker run -v $PWD/Data:/work/Data -v $PWD/Data/temp:/work/bin/Release/Data --name covid_sandbox_prepare_afd876 --rm covid_sandbox_prepare

logstring 'Generate reports...'
docker run -v $PWD/Data/temp:/work/Data -v $PWD/ReportsProcessing/reports:/work/reports --name covid_sandbox_generator_afd876 --rm covid_sandbox_generator

logstring 'Run processing reports image ...'
stop_and_remove_previous_container 'covid_sandbox_processing_afd876'
docker run -d -p 8888:8888 -v $PWD/ReportsProcessing:/work --name covid_sandbox_processing_afd876 covid_sandbox_processing
sleep 1
logstring 'Image started'

logstring 'Server is available on the following address: http://127.0.0.1:8888?token=my_secure_token'