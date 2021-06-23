#!/bin/sh

cp static/package.json build/
cd build
echo $(ls ../scripts)
node ../../kernel/scripts/npmPublish.js
