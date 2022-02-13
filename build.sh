#!/bin/bash

DIR=$(dirname "$(readlink -f "$0")")

# Backend
cd $DIR/SeasonBackend
echo $DIR/SeasonBackend

echo dotnet publish --configuration Release --output bin/publish
dotnet publish --configuration Release --output bin/publish

# Web Frontend
cd $DIR/SeasonViewer
echo $DIR/SeasonViewer

echo dotnet publish --configuration Release --output bin/publish
dotnet publish --configuration Release --output bin/publish

# Miner
cd $DIR/SeleniumMiner
echo $DIR/SeleniumMiner
mvn clean compile assembly:single
