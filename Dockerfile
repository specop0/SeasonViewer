# Build Preperation
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY SeasonViewer/SeasonViewer.csproj SeasonViewer/SeasonViewer.csproj
COPY SeasonViewer.Tests/SeasonViewer.Tests.csproj SeasonViewer.Tests/SeasonViewer.Tests.csproj
COPY SeasonViewer.sln SeasonViewer.sln
RUN dotnet restore SeasonViewer.sln

COPY SeasonViewer SeasonViewer
COPY SeasonViewer.Tests SeasonViewer.Tests

# Test
FROM build AS test
WORKDIR /src
RUN dotnet build SeasonViewer.sln --no-restore
ENTRYPOINT [ "dotnet", "test", "SeasonViewer.sln", "--no-build", "--logger:trx"]

# Publish
FROM build AS publish
WORKDIR /src
RUN dotnet publish SeasonViewer/SeasonViewer.csproj --no-restore --configuration Release --output publish

# Run
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy-chiseled-extra
WORKDIR /app
COPY --from=publish /src/publish .
ENTRYPOINT ["dotnet", "SeasonViewer.dll"]
EXPOSE 5020
