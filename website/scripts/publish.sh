#!/bin/sh

cp static/package.json build/
cd build
echo $(ls ../../kernel/scripts)
npx tsc --build ../../kernel/scripts/tsconfig.json
node ../../kernel/scripts/npmPublish.js
