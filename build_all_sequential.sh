#!/bin/bash
DIR="/Users/brito/Desktop/brasilvr_refatorado_photon"
UNITY_EXEC="/Applications/Unity/Hub/Editor/6000.3.6f1/Unity.app/Contents/MacOS/Unity"

for i in 4 1 2; do
  echo "=== Building BrasilVR$i.apk ==="
  sed -i '' -E "s/Android: com\.Vortex\.BrasilVR[0-9]/Android: com.Vortex.BrasilVR$i/" "$DIR/Oculus_VR_Player/ProjectSettings/ProjectSettings.asset"
  "$UNITY_EXEC" -quit -batchmode -projectPath "$DIR/Oculus_VR_Player" -executeMethod BuildOculosAndroid.Build -logFile "$DIR/build_vr$i.log"
  cp "$DIR/Oculus_VR_Player/Builds/final_ VR_evento_oculos.apk" "$DIR/Oculus_VR_Player/Builds/BrasilVR$i.apk"
  cp "$DIR/Oculus_VR_Player/Builds/final_ VR_evento_oculos.apk" "/Users/brito/Desktop/Embratur/APKs/BrasilVR$i.apk"
  echo "BrasilVR$i.apk READY!"
done
