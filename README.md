[![Build](https://github.com/guest512/CovidSandbox/actions/workflows/build.yml/badge.svg)](https://github.com/guest512/CovidSandbox/actions/workflows/build.yml)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=guest512_CovidSandbox&metric=code_smells)](https://sonarcloud.io/dashboard?id=guest512_CovidSandbox)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=guest512_CovidSandbox&metric=coverage)](https://sonarcloud.io/dashboard?id=guest512_CovidSandbox)

# CovidSandbox

This is a small project to visualize changes and trends for the COVID-19 pandemic and improve my technologies skills.

## Prerequsites

General prerequisites for all scenarios is an installed command processor. PowerShell v.5.1 or newer for Windows, bash - for Linux/Mac.

### Local build

To build a project locally, you need the preinstalled .NET Core SDK 3.1. You can find information about how to install it [here](https://docs.microsoft.com/dotnet/core/install/).

To run workbooks, you need a Jupyter server with additional python modules:
- [numpy](https://pypi.org/project/numpy/1.19.2)
- [pandas](https://pypi.org/project/pandas/1.1.4)
- [matplotlib](https://pypi.org/project/matplotlib/3.3.2)
- [seaborn](https://pypi.org/project/seaborn/0.11.0)
- [geopandas](https://pypi.org/project/geopandas/0.8.1)
- [descartes](https://pypi.org/project/descartes/1.1.0)
- [imageio](https://pypi.org/project/imageio/2.9.0)
- [imageio-ffmpeg](https://pypi.org/project/imageio-ffmpeg/0.4.2)
- [multiprocess](https://pypi.org/project/multiprocess/0.70.11.1)
- [tqdm ](https://pypi.org/project/tqdm/4.51.0)

### Docker build

To build and run a project with a docker, you need an installed [Docker Desktop](https://docs.docker.com/engine/install/)

## How to use

This repo contains submodules, so it's highly recommended to clone it with the following command:
```bash
git clone --recursive --jobs 8 https://github.com/guest512/CovidSandbox.git 
```

There are four scripts for each platform:
- `build-and-run-local` - builds all necessary tools and runs them. After the script finish, go to the ReportsProcessing folder and open (create new) a jupyter workbook to process the results.
- `build-and-run-docker` - creates all necessary containers and runs them. After the script finish, open a link from the script output and open (create new) a jupyter workbook to process the results.
- `run-local` - same as `build-and-run-local` but don't build anything. Useful when you update data source and want to regenerate/update reports.
- `run-docker` - same as previous but for docker containers.

**NOTE:** At the moment `build-and-run-docker` and `run-docker` scripts not only build tools and prepare data, but also builds web-site and move results to `wwwroot` folder in the project root.

There are two data sources for the moment:
- JHopkins - a submodule from the [COVID-19 repository for CSSE at Johns Hopkins University](https://github.com/CSSEGISandData/COVID-19) repository. Can be updated with the following command:
```bash
git submodule update --remote 3rdparty/DataSources/JHopkins
```
- Yandex - a manually updated file grabbed from the public [Yandex Dashboard](https://datalens.yandex/7o7is1q6ikh23?tab=X1). There is no need to update it anymore since all new cases are stored in the JHopkins data source. However, it's useful to get more detailed data about Russia cases at the beginning of the pandemic.
