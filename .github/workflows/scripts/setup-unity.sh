#!/usr/bin/env bash

set -e
set -x
mkdir -p /root/.cache/unity3d
mkdir -p /root/.local/share/unity3d/Unity/
set +x

LICENSE=$(echo "${UNITY_LICENSE_CONTENT_BASE64}" | base64 -d | tr -d '\r')

echo "Writing LICENSE to license file /root/.local/share/unity3d/Unity/Unity_lic.ulf"
echo "$LICENSE" > /root/.local/share/unity3d/Unity/Unity_lic.ulf

set -x
