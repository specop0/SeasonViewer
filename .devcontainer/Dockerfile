# Image with Node.js
FROM mcr.microsoft.com/dotnet/sdk:8.0

# Default User
ARG USER_NAME=vscode
ARG USER_UID=1000
ARG USER_GID=1000
RUN groupadd --gid $USER_GID $USER_NAME && useradd --uid $USER_UID --gid $USER_GID -m $USER_NAME
USER $USER_NAME