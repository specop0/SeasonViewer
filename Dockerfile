# Build Preperation
FROM mcr.microsoft.com/dotnet/sdk:6.0  as build
WORKDIR /src
COPY SeasonBackend/SeasonBackend.csproj SeasonBackend/SeasonBackend.csproj
COPY SeasonClientLib/SeasonClientLib.csproj SeasonClientLib/SeasonClientLib.csproj
COPY SeasonTests/SeasonTests.csproj SeasonTests/SeasonTests.csproj
COPY SeasonViewer/SeasonViewer.csproj SeasonViewer/SeasonViewer.csproj
COPY SeasonViewer.sln SeasonViewer.sln
RUN dotnet restore SeasonViewer.sln
COPY SeasonBackend SeasonBackend
COPY SeasonClientLib SeasonClientLib
COPY SeasonTests SeasonTests
COPY SeasonViewer SeasonViewer

# Test
FROM build AS test
WORKDIR /src
RUN dotnet test SeasonViewer.sln --no-restore
ENTRYPOINT [ "dotnet", "test", "--logger:trx", "--no-build" ]
