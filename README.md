![Build](https://github.com/guest512/CovidSandbox/workflows/Build/badge.svg)

# CovidSandbox

A small project to visualize changes and trends for COVID-19 pandemic and to improve skills in various technologies.

## Prerequsites

General prerequsites for all scenarios is an installed command processor. PowerShell v.5.1 or newer for Windows, bash - for Linux/Mac.

### Local build

To build project locally you need the preinstalled .NET Core SDK 3.1. You can find information how to install it [here](https://docs.microsoft.com/dotnet/core/install/)

To run workbooks you need a Jupyter server with additional python modules:
- numpy
- pandas
- matplotlib
- seaborn

### Docker build

To build and run project with docker you need an installed [Docker Desktop](https://docs.docker.com/engine/install/)

## How to use

This repo contains submodules, so it's highly recommended to clone it with the following command:
```bash
git clone --recursive --jobs 8 [URL to Git repo] 
```

There are four scripts for each platform:
- build-and-run-local - builds all necessary tools and runs them. After the script finish go to ReportsProcessing folder and open (create new) a jupyter workbook to process the results.
- build-and-run-docker - creates all necessary containers and runs them. After the script finish open a link from the script output and open (create new) a jupyter workbook to process the results.
- run-local - Same as `build-and-run-local` but don't build anything. Useful when you update data source and want to regenerate/update reports.
- run-docker - Same as previous but for docker containers.


There are two data sources for the moment:
- JHopkins - a submodule from [COVID-19 repository for CSSE at Johns Hopkins University](https://github.com/CSSEGISandData/COVID-19) repository. Can be updated with the following command:
```bash
git submodule update --remote 3rdparty/DataSources/JHopkins
```
- Yandex - a manually updated file grabbed from public [Yandex Dashboard](https://datalens.yandex/7o7is1q6ikh23?tab=X1). There is no need to update it anymore, since all new cases are stored in JHopkins data source. However it's useful to get more detailed data about cases in Russia at the beginning of the pandemic.