FROM ubuntu

ARG VERSION=0.0.0
ARG PORT=8090
ENV BINNER_VERSION=${VERSION}
ENV BINNER_CONFIG=/config/appsettings.json
ENV BINNER_NLOGCONFIG=/config/nlog.config

LABEL org.opencontainers.image.version=${VERSION}
LABEL org.opencontainers.image.source="https://github.com/replaysMike/Binner"
LABEL org.opencontainers.image.vendor="Binner"
LABEL org.opencontainers.image.url="https://binner.io"
LABEL org.opencontainers.image.authors="Michael Brown - support@binner.io"
LABEL org.opencontainers.image.licenses="GPL-3.0"
LABEL org.opencontainers.image.documentation="https://github.com/replaysMike/Binner/blob/master/README.md"
LABEL org.opencontainers.image.description="Binner is a free open-source parts inventory tracking system you can run locally in Windows or Unix environments."

RUN echo "Building image for Binner ${VERSION} on default port ${PORT}"

EXPOSE ${PORT}
WORKDIR /app

#install tools
RUN apt-get -y update; apt-get -y install sudo sed curl wget libicu-dev apt-transport-https ca-certificates

RUN echo "Downloading Binner ${VERSION}"

# download the latest release
#RUN cd /app && curl -sS https://api.github.com/repos/replaysMike/Binner/releases/latest | grep "browser_download_url.*Binner_linux-x64" | cut -d : -f 2,3 | xargs wget -O Binner_linux-x64.tar.gz && tar zxfp ./Binner_linux-x64.tar.gz && chmod +x ./Binner.Web && chmod +x ./install-as-service.sh && chmod +x ./install-certificate.sh

# download a local release
RUN cd /app && wget -nv -O Binner_linux-x64.tar.gz http://192.168.1.183:8080/Binner_linux-x64.tar.gz && tar zxfp ./Binner_linux-x64.tar.gz && chmod +x ./Binner.Web && chmod +x ./install-as-service.sh && chmod +x ./install-certificate.sh

# download a specified release
#RUN cd /app && wget -nv -O Binner_linux-x64.tar.gz https://github.com/replaysMike/Binner/releases/download/v${VERSION}/Binner_linux-x64-${VERSION}.tar.gz && tar zxfp ./Binner_linux-x64.tar.gz && chmod +x ./Binner.Web && chmod +x ./install-as-service.sh && chmod +x ./install-certificate.sh

RUN echo "$(date "+%d.%m.%Y %T") Built from Binner ${VERSION}" > /build_date.info # buildkit

RUN useradd app

RUN chown -R app /app && ls -al /app
# create app configuration
RUN mkdir /config && mv -vn /app/appsettings.json /config && mv -vn /app/nlog.config /config

# create database location (Sqlite only)
RUN mv /app/db /data
RUN sed -i -e 's/.\/db\/binner.db/\/data\/binner.db/g' /config/appsettings.json

# create userfiles location
RUN mv /app/UserFiles /userfiles
RUN sed -i -e 's/.\/UserFiles/\/userfiles/g' /config/appsettings.json

# create app certificates location
RUN mv /app/Certificates /certificates
RUN sed -i -e 's/.\/Certificates\/localhost.pfx/\/certificates\/localhost.pfx/g' /config/appsettings.json

 # create a symlink for the logs
RUN ln -s /app/logs /logs
RUN sed -i -e 's/.\/logs\//\/logs\//g' /config/nlog.config

RUN chown -R app /app /config /data /userfiles /certificates /logs

VOLUME ["/config", "/data", "/userfiles", "/certificates", "/logs"]

USER app

ENTRYPOINT ["./Binner.Web"]