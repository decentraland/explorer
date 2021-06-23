#!/bin/sh

cp static/package.json build/
cd build
npx -T ../scripts/npmPublish.ts
