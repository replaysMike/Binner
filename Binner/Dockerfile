FROM ubuntu

ARG VERSION=0.0.0
ARG PORT=8090
ARG CERTPASSWORD=password
ENV BINNER_VERSION=${VERSION}

LABEL org.opencontainers.image.version=${VERSION}
LABEL org.opencontainers.image.source="https://github.com/replaysMike/Binner"
LABEL org.opencontainers.image.vendor="Binner"
LABEL org.opencontainers.image.url="https://binner.io"
LABEL org.opencontainers.image.authors="Michael Brown - support@binner.io"
LABEL org.opencontainers.image.licenses="GPL-3.0"
LABEL org.opencontainers.image.documentation="https://github.com/replaysMike/Binner/blob/master/README.md"
LABEL org.opencontainers.image.description="Binner is a free open-source parts inventory tracking system you can run locally in Windows or Unix environments."

RUN /bin/sh -c echo "Building image for Binner ${VERSION} on default port ${PORT}"

EXPOSE ${PORT}
WORKDIR /app

#install tools
RUN apt-get -y update; apt-get -y install sudo sed curl wget libicu-dev apt-transport-https ca-certificates

RUN /bin/sh -c echo "Downloading Binner ${VERSION}"

# download the latest release
#RUN /bin/sh -c cd /app && curl -sS https://api.github.com/repos/replaysMike/Binner/releases/latest | grep "browser_download_url.*Binner_linux-x64" | cut -d : -f 2,3 | xargs wget -O Binner_linux-x64.tar.gz && tar zxfp ./Binner_linux-x64.tar.gz && chmod +x ./Binner.Web && chmod +x ./install-as-service.sh && chmod +x ./install-certificate.sh

# download a local release
#RUN cd /app && wget -nv -O Binner_linux-x64.tar.gz http://192.168.1.183:8080/Binner_linux-x64.tar.gz && tar zxfp ./Binner_linux-x64.tar.gz && chmod +x ./Binner.Web && chmod +x ./install-as-service.sh && chmod +x ./install-certificate.sh

# download a specified release
RUN cd /app && wget -nv -O Binner_linux-x64.tar.gz https://github.com/replaysMike/Binner/releases/download/v${VERSION}/Binner_linux-x64-${VERSION}.tar.gz && tar zxfp ./Binner_linux-x64.tar.gz && chmod +x ./Binner.Web && chmod +x ./install-as-service.sh && chmod +x ./install-certificate.sh

# generate a new unique ssl cert for the container, with a password of 'password'
RUN openssl req -x509 -newkey rsa:4096 -sha256 -keyout /app/Certificates/binner-docker.key -out /app/Certificates/binner-docker.crt -passout "pass:${CERTPASSWORD}" -subj "/O=Binner/CN=localhost" -addext "subjectAltName=DNS:localhost,DNS:binner.localhost,IP:127.0.0.1,IP:0.0.0.0" -days 3650
RUN openssl pkcs12 -export -password "pass:${CERTPASSWORD}" -passin "pass:${CERTPASSWORD}" -inkey /app/Certificates/binner-docker.key -in /app/Certificates/binner-docker.crt -name "Binner" -out /app/Certificates/binner-docker.pfx
RUN chmod +r /app/Certificates/binner-docker.key && chmod +r /app/Certificates/binner-docker.crt && chmod +r /app/Certificates/binner-docker.pfx

# change the config to use this certificate
RUN sed -i -e 's/.\/Certificates\/localhost-unix.pfx/\/app\/Certificates\/binner-docker.pfx/g' /app/appsettings.json

# remove the bundled certificate not used on this environment
RUN rm /app/Certificates/localhost-unix.crt && rm /app/Certificates/localhost-unix.key && rm /app/Certificates/localhost-unix.pfx

# register the cert in the container
RUN cp /app/Certificates/binner-docker.crt /usr/local/share/ca-certificates && cp /app/Certificates/binner-docker.crt /usr/local/share/certificates && ls -l /usr/local/share/ca-certificates && ls -l /app/Certificates && head -n 20 /app/appsettings.json
RUN update-ca-certificates

RUN /bin/sh -c echo "$(date "+%d.%m.%Y %T") Built from Binner ${VERSION}" >> /build_date.info # buildkit

RUN useradd app

# create a symlink for the app configuration and database
RUN mkdir -p /data \
 && chown app /data \
 && ln -s /app/appsettings.json /data/appsettings.json \
 && touch /app/binner.db \
 && chown app /app/binner.db \
 && ln -s /app/binner.db /data/binner.db

 # create a symlink for the certificates
RUN ln -s /app/Certificates /Certificates

# create symlinks for the app logs
RUN mkdir -p /logs \
&& chown app /logs \
&& touch /app/Binner.log \
&& touch /app/Binner.Microsoft.log \
&& touch /app/Binner.MissingLocaleKeys.log \
&& chown app /app/Binner.log \
&& chown app /app/Binner.Microsoft.log \
&& chown app /app/Binner.MissingLocaleKeys.log \
&& ln -s /app/Binner.log /logs/Binner.log \
&& ln -s /app/Binner.Microsoft.log /logs/Binner.Microsoft.log \
&& ln -s /app/Binner.MissingLocaleKeys.log /logs/Binner.MissingLocaleKeys.log

# volume labels should be specified by the user or using compose
VOLUME ["/data", "/Certificates", "/logs"]

USER app

ENTRYPOINT ["./Binner.Web"]