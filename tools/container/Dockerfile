# Use the base Debian image
FROM debian:bookworm-slim

# Set environment variable to avoid interactive prompts during package installation
ENV DEBIAN_FRONTEND=noninteractive

# Install necessary tools and dependencies
RUN apt-get update && apt-get install -y curl wget build-essential procps

# Add the Microsoft package signing key
RUN wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    rm packages-microsoft-prod.deb

# Install .NET SDK
RUN apt-get update && apt-get install -y dotnet-sdk-8.0

# install specific version of Node.js (20.10.0) and npm
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs=20.16.0-1nodesource1 && \
    npm install -g npm@10.8.2

# # FIXME: Install Wine for running Windows applications
# RUN dpkg --add-architecture i386 && \
#     apt-get update && \
#     apt-get install -y wine wine32

# # FIXME: Download and install Inno Setup using Wine
# RUN wget https://jrsoftware.org/download.php/is.exe -O is.exe
# RUN Xvfb :1 & \
#     export DISPLAY=:1 && \
#     wine is.exe /SILENT && \
#     rm is.exe

COPY *.sh /usr/local/bin/
RUN chmod a+x /usr/local/bin/*.sh

ENTRYPOINT [ "/usr/local/bin/entrypoint.sh" ]
