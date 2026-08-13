#!/bin/bash
DIR="/Users/brito/Desktop/brasilvr_refatorado_photon"
UNITY_EXEC="/Applications/Unity/Hub/Editor/6000.3.6f1/Unity.app/Contents/MacOS/Unity"
LOG_FILE="$DIR/build_vr1.log"
PKG="com.Vortex.BrasilVR1"

echo "Configuring ProjectSettings for $PKG..."
sed -i '' -E "s/Android: com\.Vortex\.BrasilVR[0-9]/Android: $PKG/" "$DIR/Oculus_VR_Player/ProjectSettings/ProjectSettings.asset"

echo "Building $PKG..."
"$UNITY_EXEC" -quit -batchmode -projectPath "$DIR/Oculus_VR_Player" -executeMethod BuildOculosAndroid.Build -logFile "$LOG_FILE"
cp "$DIR/Oculus_VR_Player/Builds/final_ VR_evento_oculos.apk" "$DIR/Oculus_VR_Player/Builds/BrasilVR1.apk"
echo "Done"
