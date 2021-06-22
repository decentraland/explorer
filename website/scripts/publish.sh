#!/bin/sh

cp static/package.json build/
cd build
echo $(env)
node ../../kernel/scripts/npmPublish.js
