#!/bin/sh

cp static/package.json build/
cd build
node ../../kernel/scripts/npmPublish.js
