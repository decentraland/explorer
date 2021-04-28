#!/bin/sh

BRANCH=$1

if [ -z "${BRANCH}" ]; then
    echo "Usage: $0 <branch>"
    exit
fi

echo "Starting update renderer branch=${BRANCH}"

NPM_PACKAGE_TAG=$(echo "${BRANCH}" | sed -e 's/[^a-zA-Z0-9-]/-/g')

if [ "${BRANCH}" = "master" ]; then
    echo "Updating renderer master"
    npm install --no-save decentraland-renderer@^1.0.0
else
    NPM_PACKAGE_EXISTS=$(npm view decentraland-renderer | grep -a ".-${NPM_PACKAGE_TAG}")

    if [ -z "${NPM_PACKAGE_EXISTS}" ]; then
        echo "Branch package not found, installing master instead..."
        npm install --no-save decentraland-renderer@^1.0.0
    else
        echo "Branch package found, installing..."
        npm install --no-save decentraland-renderer@${NPM_PACKAGE_TAG}
    fi
fi

cp node_modules/decentraland-renderer/*.unityweb static/unity/Build/
echo "Update renderer OK"