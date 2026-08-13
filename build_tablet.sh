#!/bin/bash
DIR="/Users/brito/Desktop/brasilvr_refatorado_photon"
UNITY_EXEC="/Applications/Unity/Hub/Editor/6000.3.6f1/Unity.app/Contents/MacOS/Unity"
LOG_FILE="$DIR/build_tablet.log"
echo "Building Tablet Controller..."
"$UNITY_EXEC" -quit -batchmode -projectPath "$DIR/Tablet_Controller" -executeMethod BuildTabletApk.PerformBuild -logFile "$LOG_FILE"
echo "Done"
