## How to debug with local test parcels and preview scenes

### Running Test Scenes

You can test and debug components and SDK functionalities with many test-scenes located at `kernel/public/test-scenes/`, running the client in `debug` mode.

1. Start the regular kernel server in a terminal at '/kernel' and leave it watching for changes

    `make watch`

2. In **another terminal** in '/kernel' run the following command to build the test scenes and keep watching changes on them (changes will be visible when refreshing the browser):

    `make watch-only-test-scenes`

Note that the test-scenes building may take a while (10 mins or more).

3. To run the client in `debug` mode, append the following query parameter to the URL:

    http://localhost:3000/?DEBUG_MODE

4. To spawn at the specific coordinates of a test scene append the position query paramter:

    http://localhost:3000/?DEBUG_MODE&position=-1,27

NOTE(optional): If you don't need to modify the test-scenes and just build them once, you may only run once `make test-scenes` (it takes a while) and that'd be it.

### Creating New Test Scenes

1. Create a new folder in `kernel/public/test-scenes`.
2. Go to the new created folder.
3. Run `dcl init`. This will create the basic files structure with an example scene.
4. Run `dcl start`. A new tab in your browser will be open with your new scene in preview mode.

### Preview Mode Scenes

#### Play a scene in Preview Mode

1. Go to the scene folder.
2. Compile the scene dependencies:
   a. For working with the @latest Kernel version, run `decentraland-ecs@latest`.
   b. For working with the @next Kernel version, run `decentraland-ecs@next`.
3. Run `dcl start`. A new tab in your browser will be open with the scene in preview mode.

#### Play a scene in Preview Mode using the local Kernel version

1. Go to the kernel folder. 
2. Run `make watch` and wait for the process ends.
3. Kill the server, run `make npm-link` and wait for the process ends.
4. Go to the scene folder.
5. Run `npm link decentraland-ecs`.
6. Run `dcl start`. A new tab in your browser will be open with the scene in preview mode.

#### Debugging a scene in Preview Mode from Unity Editor

1. Go to the scene folder.
2. Compile the scene dependencies:
   a. For working with the @latest Kernel version, run `decentraland-ecs@latest`.
   b. For working with the @next Kernel version, run `decentraland-ecs@next`.
3. Run `dcl start`. A new tab in your browser will be open with the scene in preview mode.
4. In Unity, deactivate the "Open Browser When Start" flag (`WSSController` game object in the `InitialScene` scene).
5. Click on PLAY button.
6. Go to the browser tab that was opened by the previous `dcl start` command and append the following query parameter to the url: `&ws=ws://localhost:4999/dcl`.
7. Notice that the Unity editor starts loading the scene.

#### Debugging a scene in Preview Mode from Unity Editor and using the local Kernel version

1. Go to the kernel folder. 
2. Run `make watch` and wait for the process ends.
3. Kill the server, run `make npm-link` and wait for the process ends.
4. Go to the scene folder.
5. Run `npm link decentraland-ecs`.
6. Run `dcl start`. A new tab in your browser will be open with the scene in preview mode.
7. In Unity, deactivate the "Open Browser When Start" flag (`WSSController` game object in the `InitialScene` scene).
8. Click on PLAY button.
9. Go to the browser tab that was opened by the previous `dcl start` command and append the following query parameter to the url: `&ws=ws://localhost:4999/dcl`.
10. Notice that the Unity editor starts loading the scene.
