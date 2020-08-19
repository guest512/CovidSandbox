
FROM alpine:3.11 as base_builder

ENV dotnet_install_dir=/usr/share/dotnet
WORKDIR work

RUN apk add --no-cache bash wget libstdc++ libintl icu libcurl zlib

RUN wget https://dot.net/v1/dotnet-install.sh 
RUN chmod +x ./dotnet-install.sh 

RUN ./dotnet-install.sh -c Current -InstallDir $dotnet_install_dir
ENV DOTNET_ROOT=$dotnet_install_dir
ENV PATH=$PATH:$dotnet_install_dir





FROM ubuntu:20.04 as base_runner
WORKDIR work
RUN apt-get update -y && apt-get install python3-pip -y && pip3 install pip --upgrade && apt-get clean
RUN pip3 install jupyter numpy sympy pandas geopandas matplotlib

# Add Tini. Tini operates as a process subreaper for jupyter. This prevents kernel crashes.
ENV TINI_VERSION v0.6.0
ADD https://github.com/krallin/tini/releases/download/${TINI_VERSION}/tini /usr/bin/tini
RUN chmod +x /usr/bin/tini
ENTRYPOINT ["/usr/bin/tini", "--"]

CMD ["jupyter", "notebook", "--port=8888", "--no-browser", "--ip=0.0.0.0", "--allow-root"]




FROM base_builder as builder

COPY build build
COPY CovidSandbox CovidSandbox
COPY CovidSandbox.Tests CovidSandbox.Tests
COPY Data Data

COPY CovidSandbox.sln CovidSandbox.sln

RUN dotnet msbuild build/build.proj -v:n /p:Configuration=Release /t:PrepareData
RUN dotnet test -v:n -c:Release

WORKDIR bin/Release
RUN dotnet CovidSandbox.dll





FROM base_runner as runner

COPY --from=builder work/bin/Release/reports reports
COPY ReportsProcessing .
