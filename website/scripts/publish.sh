#!/bin/sh

cp static/package.json build/
cd build
npx ts-node -T ../scripts/npmPublish.ts
