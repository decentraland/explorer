#!/usr/bin/env bash

set -x

xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' /opt/Unity/Editor/Unity \
        -batchmode \
        -logFile /dev/stdout \
        -projectPath "$(pwd)" \
        -buildTarget "$BUILD_TARGET" \
        -runTests \
        -testPlatform EditMode \
        -testResults "$(pwd)/editmode-results.xml" \
        -enableCodeCoverage \
        -coverageResultsPath "$(pwd)/CodeCoverage/" \
        -coverageOptions "assemblyFilters:-*unity*" \
        -manualLicenseFile /root/.local/share/unity3d/Unity/Unity_lic.ulf

# Catch exit code
UNITY_EXIT_CODE=$?

# Print unity log output
cat "editmode-results.xml"

# Display results
if [ $UNITY_EXIT_CODE -eq 0 ]; then
  echo "Run succeeded, no failures occurred";
elif [ $UNITY_EXIT_CODE -eq 2 ]; then
  echo "Run succeeded, some tests failed";
elif [ $UNITY_EXIT_CODE -eq 3 ]; then
  echo "Run failure (other failure)";
else
  echo "Unexpected exit code $UNITY_EXIT_CODE";
fi

exit $UNITY_EXIT_CODE
