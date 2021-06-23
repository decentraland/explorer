#!/bin/sh

cp static/package.json build/
cd build
echo $(ls ../../kernel/scripts)
node ../../kernel/scripts/npmPublish.js
