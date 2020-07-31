#!/bin/bash

DIR=$(dirname "$(readlink -f "$0")")

if [ -f /usr/bin/lxterminal ]; then
  lxterminal --working-directory=$DIR/SeasonBackend -e "dotnet run" &
  lxterminal --working-directory=$DIR/SeasonViewer -e "dotnet run" &
elif [ -f /bin/xfce4-terminal ]; then
  xfce4-terminal --working-directory=$DIR/SeasonBackend -e "dotnet run" &
  xfce4-terminal --working-directory=$DIR/SeasonViewer -e "dotnet run" &
else
  echo "ERROR: Could not find a supported terminal."
  exit 1
fi

sleep 5
xdg-open http://localhost:5000
