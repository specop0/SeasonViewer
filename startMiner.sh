#!/bin/bash

DIR=$(dirname "$(readlink -f "$0")")

cd $DIR/SeleniumMiner

java -jar ./target/SeleniumMiner-1.0-SNAPSHOT-jar-with-dependencies.jar

