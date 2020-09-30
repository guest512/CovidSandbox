#!/bin/bash

function logstring {
    no_color='\033[0m' # No Color
    echo -e "${1}${2}${no_color}"
}

function logmessage {
    color = '\033[1;34m'
    logmessage color ${1}
}

function logerror {
    color='\033[31m'
    logmessage color ${1}
}

function checkresult {
    if [ ${1} -eq 0]; then
        logmessage 'SUCCESS'
    else
        logerror 'FAIL'
        exit 1
    fi
    
}

today=$(date +"%y.%m.%d")

logmessage 'Build docker images...'

logmessage 'Data preparation image...'
docker build --target data_preparation -t covid_sandbox_prepare:$today -t covid_sandbox_prepare:latest .
checkresult $?

logmessage 'Reports generator image...'
docker build --target reports_generator -t covid_sandbox_generator:$today -t covid_sandbox_generator:latest .
checkresult $?

logmessage 'Reports processing image...'
docker build --target reports_processor -t covid_sandbox_processing:$today -t covid_sandbox_processing:latest .
checkresult $?

chmod +x ./update-reports.sh
./update-reports.sh