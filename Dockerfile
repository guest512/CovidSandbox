
FROM alpine:3.11 as base_builder

ENV dotnet_install_dir=/usr/share/dotnet
ENV DOTNET_ROOT=$dotnet_install_dir
ENV PATH=$PATH:$dotnet_install_dir
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

WORKDIR /work
# Dependencies list can be found here https://docs.microsoft.com/ru-ru/dotnet/core/install/linux-alpine#dependencies
RUN apk add --no-cache bash wget icu-libs krb5-libs libgcc libintl openssl-dev libstdc++ zlib

RUN wget https://dot.net/v1/dotnet-install.sh \
    && chmod +x ./dotnet-install.sh





FROM ubuntu:20.04 as base_runner
WORKDIR /work
RUN apt update -y && \
    apt install python3-pip locales -y && \
    apt-get clean && \
    locale-gen ru_RU && \
    locale-gen ru_RU.UTF-8 && \
    update-locale && \
    pip3 install pip --upgrade && \
    pip3 install jupyter numpy sympy pandas geopandas matplotlib seaborn





FROM base_runner as reports_processor

VOLUME [ "/work" ]

# Add Tini. Tini operates as a process subreaper for jupyter. This prevents kernel crashes.
ENV TINI_VERSION v0.6.0
ADD https://github.com/krallin/tini/releases/download/${TINI_VERSION}/tini /usr/bin/tini
RUN chmod +x /usr/bin/tini
ENTRYPOINT ["/usr/bin/tini", "--"]

CMD [ \
    "jupyter", "notebook", "--port=8888", "--ip=0.0.0.0", \
    "--no-browser", "--allow-root", \
    "--NotebookApp.token='my_secure_token'", "--NotebookApp.password=''" \
    ]





FROM base_builder as base_builder_dotnet

RUN ./dotnet-install.sh -c Current -InstallDir $dotnet_install_dir





FROM base_builder_dotnet as data_preparation

VOLUME [ "/work/dataSources", "/work/data" ]
COPY Tools/build ./

CMD dotnet msbuild build.proj /t:PrepareData \
    /p:DataSourcesRootFolder=$PWD/dataSources \
    /p:DataRootFolder=$PWD/data \
    /p:BinaryDataOutDir=$PWD/out





FROM base_builder_dotnet as builder

COPY Tools ./
COPY Data bin/Data/Misc/

RUN dotnet test src/ -c:Release -p:BinaryOutDir=$PWD/bin && \
    dotnet publish src/ReportsGenerator/ --self-contained true -c:Release -p:BinaryOutDir=$PWD/bin -p:PublishTrimmed=true -r alpine.3.11-x64 -v:n




FROM base_builder as reports_generator

VOLUME [ "/work/Data" , "/work/reports" ]

COPY --from=builder work/bin/alpine.3.11-x64/publish .

CMD ["./ReportsGenerator"]
