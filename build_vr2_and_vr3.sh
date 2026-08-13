#!/bin/bash
DIR="/Users/brito/Desktop/brasilvr_refatorado_photon"
UNITY_EXEC="/Applications/Unity/Hub/Editor/6000.3.6f1/Unity.app/Contents/MacOS/Unity"

# Build VR2
echo "=== Building BrasilVR2.apk ==="
sed -i '' -E "s/Android: com\.Vortex\.BrasilVR[0-9]/Android: com.Vortex.BrasilVR2/" "$DIR/Oculus_VR_Player/ProjectSettings/ProjectSettings.asset"
"$UNITY_EXEC" -quit -batchmode -projectPath "$DIR/Oculus_VR_Player" -executeMethod BuildOculosAndroid.Build -logFile "$DIR/build_vr2.log"
cp "$DIR/Oculus_VR_Player/Builds/final_ VR_evento_oculos.apk" "$DIR/Oculus_VR_Player/Builds/BrasilVR2.apk"

# Build VR3
echo "=== Building BrasilVR3.apk ==="
sed -i '' -E "s/Android: com\.Vortex\.BrasilVR[0-9]/Android: com.Vortex.BrasilVR3/" "$DIR/Oculus_VR_Player/ProjectSettings/ProjectSettings.asset"
"$UNITY_EXEC" -quit -batchmode -projectPath "$DIR/Oculus_VR_Player" -executeMethod BuildOculosAndroid.Build -logFile "$DIR/build_vr3.log"
cp "$DIR/Oculus_VR_Player/Builds/final_ VR_evento_oculos.apk" "$DIR/Oculus_VR_Player/Builds/BrasilVR3.apk"

echo "=== BrasilVR2.apk and BrasilVR3.apk Ready! ==="
