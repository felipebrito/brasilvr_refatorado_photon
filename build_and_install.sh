#!/bin/bash
UNITY_EXEC="/Applications/Unity/Hub/Editor/6000.3.6f1/Unity.app/Contents/MacOS/Unity"
DIR="/Users/brito/Desktop/brasilvr_refatorado_photon"

echo "=== Building Tablet ==="
"$UNITY_EXEC" -quit -batchmode -projectPath "$DIR/Tablet_Controller" -executeMethod BuildTabletApk.PerformBuild -logFile "$DIR/build_tablet.log"
if [ $? -eq 0 ]; then
    echo "Tablet build successful. Installing..."
    adb -s RX2Y401NAXE install -r "$DIR/Tablet_Controller/Builds/BrasilVRController-tablet.apk"
    adb -s RX2Y401NAXE shell monkey -p com.vortexplay.final_vr_evento -c android.intent.category.LAUNCHER 1
else
    echo "Tablet build failed. Check $DIR/build_tablet.log"
fi

echo "=== Building Oculus ==="
"$UNITY_EXEC" -quit -batchmode -projectPath "$DIR/Oculus_VR_Player" -executeMethod BuildOculosAndroid.Build -logFile "$DIR/build_oculus.log"
if [ $? -eq 0 ]; then
    echo "Oculus build successful. Installing..."
    adb -s 230YC01D8202J6 install -r "$DIR/Oculus_VR_Player/Builds/final_ VR_evento_oculos.apk"
    adb -s 230YC01D8202J6 shell monkey -p com.vortexplay.final_vr_evento_oculos -c android.intent.category.LAUNCHER 1
else
    echo "Oculus build failed. Check $DIR/build_oculus.log"
fi

echo "=== Build and Install Process Complete ==="
