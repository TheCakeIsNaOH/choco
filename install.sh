#!/bin/bash
set -euo pipefail

cp ./docker/choco_wrapper /usr/local/bin/choco
cp ./docker/choco_wrapper /usr/local/bin/choco.exe

[ -d /opt/chocolatey ] || mkdir /opt/chocolatey
cp ./code_drop/chocolatey/console/* /opt/chocolatey/

echo "Add 'export ChocolateyInstall=/opt/chocolatey' to your profile"