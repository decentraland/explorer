# Decentraland VR Explorer Proto

### Requirements

This build only runs in WINDOWS PCs and works with hopefully every VR device (it has been implemented using the SteamVR integration instead of brand-owned packages)

### Current state

This explorer is still in a super early stage, in slow development, not officially released by Decentraland, and based upon the "desktop explorer" branch started by Patricio Bosio at [Desktop Client](https://github.com/decentraland/explorer/tree/poc/desktop-client).

The current manual way of using this app is a bit clunky and annoying, and the world interactivity is limited as the UI usage hasn't been implemented yet.

### Usage instructions

1. Download [DecentralandVRExplorer-Win64.zip](https://github.com/decentraland/explorer/blob/poc/desktop-client/Win64Build/DecentralandVRExplorer-Win64.zip) and unzip it anywhere in your PC.
2. Open the DecentralandUnityClient.exe and you'll see that along with the application, a browser tab will open as well
3. The newly opened browser tab will start loading the world and sending it to the VR client. You'll be able to see a loading bar (if it doesn't work at once, try refreshing the URL)
4. After a while you should see a **green "Connected"** word in the browser, at this moment you can alt-tab into the running app again and put on your headset.

Note: if you are re-opening the application and you already have a tab running decentraland, you should close it, only 1 at a time can be opened to be used with the client.