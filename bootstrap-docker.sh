#!/bin/bash

today=$(date +"%y.%m.%d")

echo 'Build docker image'
docker build -t covid_sandbox:$today -t covid_sandbox:latest .

echo 'Looking for existing containers from previous runs'
existing_containers_count=$(docker ps -af "name=covid_sandbox_afd876" --format "{{.ID}} {{.Names}}" | grep -o 'covid_sandbox_afd876' | wc -l)

if [[ "$existing_containers_count" -gt 0 ]]; then
    echo 'Stopping and removing container'
    docker stop covid_sandbox_afd876
    docker rm covid_sandbox_afd876
fi

echo 'Run new image'
docker run -d -p 8888:8888 --name covid_sandbox_afd876 covid_sandbox
sleep 1
echo 'Image started'

docker logs covid_sandbox_afd876
