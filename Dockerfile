
FROM alpine:3.11 as base_builder

ENV dotnet_install_dir=/usr/share/dotnet
ENV DOTNET_ROOT=$dotnet_install_dir
ENV PATH=$PATH:$dotnet_install_dir

WORKDIR /work

RUN apk add --no-cache bash wget libstdc++ libintl icu libcurl zlib

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
 pip3 install jupyter numpy sympy pandas geopandas matplotlib





FROM base_runner as reports_processor

VOLUME [ "/work" ]

# Add Tini. Tini operates as a process subreaper for jupyter. This prevents kernel crashes.
ENV TINI_VERSION v0.6.0
ADD https://github.com/krallin/tini/releases/download/${TINI_VERSION}/tini /usr/bin/tini
RUN chmod +x /usr/bin/tini
ENTRYPOINT ["/usr/bin/tini", "--"]

CMD ["jupyter", "notebook", "--port=8888", "--no-browser", "--ip=0.0.0.0", "--allow-root"]





FROM base_builder as base_builder_dotnet

RUN ./dotnet-install.sh -c Current -InstallDir $dotnet_install_dir





FROM base_builder_dotnet as data_preparation

VOLUME [ "/work/Data" ]
VOLUME [ "/work/bin/Release/Data" ]

COPY build build/

CMD [ "dotnet", "msbuild", "./build/build.proj", "/t:PrepareData", "/p:Configuration=Release" ]





FROM base_builder_dotnet as builder

COPY build build/
COPY CovidSandbox CovidSandbox/
COPY CovidSandbox.Tests CovidSandbox.Tests/
COPY CovidSandbox.sln ./
COPY Data/Misc bin/Release/Data/Misc/

RUN dotnet test -c:Release && dotnet publish --self-contained true -c:Release -p:PublishTrimmed=true -r alpine.3.11-x64 CovidSandbox/ -v:n




FROM base_builder as reports_generator

VOLUME [ "/work/Data" ]
VOLUME [ "/work/reports" ]

COPY --from=builder work/bin/Release/alpine.3.11-x64/publish .

CMD ["./CovidSandbox"]
