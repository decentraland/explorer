#!/bin/sh

cp static/package.json build/
cd build
echo $(ls ../)
node ../../kernel/scripts/npmPublish.js
