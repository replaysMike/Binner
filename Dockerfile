FROM debian:bookworm-slim

RUN apt-get update && apt-get install -y npm curl wget tar bash libicu-dev ca-certificates

ARG SOURCES

COPY $SOURCES /usr/local/bin/

#extract file
RUN tar -xzf /usr/local/bin/$SOURCES -C /usr/local/bin/ && \
    rm /usr/local/bin/$SOURCES

#make Binner.Web executable
RUN chmod +x /usr/local/bin/Binner.Web

EXPOSE 8090

#execute Binner.Web file
WORKDIR /usr/local/bin/
ENTRYPOINT ["Binner.Web"]
