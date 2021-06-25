#!/bin/sh

cp static/package.json build/
cd build
npm i
npx ts-node -T ../../kernel/scripts/npmPublish.ts
