#!/bin/bash
UNITY_EXEC="/Applications/Unity/Hub/Editor/6000.3.6f1/Unity.app/Contents/MacOS/Unity"
DIR="/Users/brito/Desktop/brasilvr_refatorado_photon"

echo "=== Building Versions 1, 2, 4 of Oculus VR Player PROPERLY ==="

for i in 1 2 4; do
  PKG="com.Vortex.BrasilVR$i"
  SLOT=$((i-1))
  
  echo "Setting up for $PKG (Slot $SLOT)..."
  
  python3 "$DIR/update_pkg.py" "$DIR/Oculus_VR_Player/ProjectSettings/ProjectSettings.asset" "$PKG"
  
  python3 -c "
import sys, re
path = '$DIR/Oculus_VR_Player/Assets/Scripts/UserStatusSend.cs'
with open(path, 'r') as f: code = f.read()
new_code = re.sub(r'int slotIndex = \d+; // FORCANDO PARA PLAYER \d+ \(Slot \d+\)', f'int slotIndex = {SLOT}; // FORCANDO PARA PLAYER {sys.argv[1]} (Slot {SLOT})', code)
with open(path, 'w') as f: f.write(new_code)
" $i
  
  echo "Building $PKG..."
  "$UNITY_EXEC" -quit -batchmode -projectPath "$DIR/Oculus_VR_Player" -executeMethod BuildOculosAndroid.Build -logFile "$DIR/build_oculus_version_$i.log"
  
  if [ -f "$DIR/Oculus_VR_Player/Builds/final_ VR_evento_oculos.apk" ]; then
     mv "$DIR/Oculus_VR_Player/Builds/final_ VR_evento_oculos.apk" "$DIR/Oculus_VR_Player/Builds/BrasilVR$i.apk"
     echo "Build $i successful: BrasilVR$i.apk"
  else
     echo "Build $i failed! Check build_oculus_version_$i.log"
  fi
done
