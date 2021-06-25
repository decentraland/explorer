#!/bin/sh

cp static/package.json build/
cd build
npm i
npx oddish
