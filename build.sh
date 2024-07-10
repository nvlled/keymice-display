#!/bin/bash

mkdir -p build/{linux,windows}/keymice

godot --headless --export-release linux   build/linux/keymice/keymice.x86_64
godot --headless --export-release windows build/windows/keymice/keymice.exe

rm -f build/*.zip

cd build/linux
zip -r ../keymice-linux.zip keymice

cd -
cd build/windows
zip -r ../keymice-windows.zip keymice
