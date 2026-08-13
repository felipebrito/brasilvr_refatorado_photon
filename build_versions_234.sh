#!/bin/bash
UNITY_EXEC="/Applications/Unity/Hub/Editor/6000.3.6f1/Unity.app/Contents/MacOS/Unity"
DIR="/Users/brito/Desktop/brasilvr_refatorado_photon"

echo "=== Building Versions 2, 3, 4 of Oculus VR Player ==="

for i in 2 3 4; do
  PKG="com.Vortex.BrasilVR$i"
  echo "Building $PKG..."
  
  # Replace package name in ProjectSettings.asset
  python3 "$DIR/update_pkg.py" "$DIR/Oculus_VR_Player/ProjectSettings/ProjectSettings.asset" "$PKG"
  
  # Run Unity build
  "$UNITY_EXEC" -quit -batchmode -projectPath "$DIR/Oculus_VR_Player" -executeMethod BuildOculosAndroid.Build -logFile "$DIR/build_oculus_version_$i.log"
  
  # Check if build succeeded and rename the output apk
  if [ -f "$DIR/Oculus_VR_Player/Builds/final_ VR_evento_oculos.apk" ]; then
     mv "$DIR/Oculus_VR_Player/Builds/final_ VR_evento_oculos.apk" "$DIR/Oculus_VR_Player/Builds/BrasilVR$i.apk"
     echo "Build $i successful: BrasilVR$i.apk"
  else
     echo "Build $i failed! Check build_oculus_version_$i.log"
  fi
done

echo "=== Done! ==="
