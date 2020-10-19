#!/bin/sh

kernel="$(cd .. && pwd)/kernel"

echo ""
echo "kernel path: ${kernel}"
echo "current path: $(pwd)";
echo ""
echo "Removing current files..."
rm -rfv ./public/default-profile;
rm -rfv ./public/website.js;
rm -rfv ./public/loader;
rm -rfv ./public/systems;
rm -rfv ./public/unity;
rm -rfv ./public/voice-chat-codec;

echo ""
echo ""
echo "Linking to kernel..."
ln -sv "${kernel}/static/default-profile" ./public
ln -sv "${kernel}/static/dist/website.js" ./public
ln -sv "${kernel}/static/loader" ./public
ln -sv "${kernel}/static/systems" ./public
ln -sv "${kernel}/static/unity" ./public
ln -sv "${kernel}/static/voice-chat-codec" ./public
echo ""
echo ""
echo "READY TO DEV 💪💪💪"
