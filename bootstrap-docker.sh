#!/bin/bash

function logstring {
    color='\033[1;34m'
    no_color='\033[0m' # No Color
    echo -e "${color}${1}${no_color}"
}

today=$(date +"%y.%m.%d")

logstring 'Build docker images...'

logstring 'Data preparation image...'
docker build --target data_preparation -t covid_sandbox_prepare:$today -t covid_sandbox_prepare:latest .

logstring 'Reports generator image...'
docker build --target reports_generator -t covid_sandbox_generator:$today -t covid_sandbox_generator:latest .
logstring 'Reports processing image...'
docker build --target reports_processor -t covid_sandbox_processing:$today -t covid_sandbox_processing:latest .


./update-reports.sh