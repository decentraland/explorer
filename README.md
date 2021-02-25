# Decentraland Explorer

## Contributing

**Please read the [contribution guidelines](.github/CONTRIBUTING.md)**

### Before you start

1. [Pull Request Naming Standards](https://github.com/decentraland/standards/blob/master/standards/git-usage.md)
2. [Architecture Overview](https://docs.google.com/document/d/1_lzi3V5IDaVRJbTKNsNEcaG0L21VPydiUx5uamiyQnY/edit)
3. [Coding Guidelines](unity-client/style-guidelines.md)

This repo requires `git lfs` to track images and other binary files. https://git-lfs.github.com/ and the latest version of GNU make, install it using `brew install make`
If you are using Windows 10 we recommend you to enable the Linux subsystem and install a Linux distro from Windows Store like Ubuntu. Then install all tools and dependecies like nodejs, npm, typescript, make...

## Running the Explorer

### Debug using Unity only
Explorer is composed of two main projects, Kernel (ts) and Renderer (Unity). However, if you only intend to make changes related to Unity only you can skip the Kernel build and connect the Unity build with a deployed kernel build via websocket.

1. Download and install Unity 2019.4.0f1
2. Open the Initial Scene
3. Run the Initial Scene in the Unity editor

#### Troubleshooting

##### Missing git lfs extension
If while trying to compile the Unity project you get an error regarding some libraries that can not be added (for instance Newtonsoft
Json.NET or Google Protobuf), please execute the following command in the root folder:

    git lfs install
    git lfs pull

Then, on the Unity editor, click on `Assets > Reimport All`

---

### Debug using Kernel only

Make sure you have the following dependencies:
- Node v10 or compatible installed via `sudo apt install nodejs` or [nvm](https://github.com/nvm-sh/nvm)
- yarn installed globally via `npm install yarn -g`

IMPORTANT: If your path has spaces the build process will fail. Make sure to clone this repo in a properly named path.

Build the project:

    cd website
    npm install
    cd kernel
    npm install

To run and watch a server with the kernel build, run:

    make watch


#### Run kernel tests

To see test logs/errors directly in the browser, run:

    make watch

Now, navigate to [http://localhost:8080/test](http://localhost:8080/test)

#### Troubleshooting

##### Missing xcrun (macOS)
If you get the "missing xcrun" error when trying to run the `make watch` command, you should download the latest command line tools for macOS, either by downloading them from https://developer.apple.com/download/more/?=command%20line%20tools or by re-installing XCode


### Debug with Unity Editor + local Kernel

* Make sure you have the proper Unity version up and running
* Make sure you are running kernel through `make watch` command.
* Back in unity editor, open the `WSSController` component inspector of `InitialScene`
* Make sure that is setup correctly

### Debug with browsers + local Unity build

1. Make sure you have the proper Unity version up and running
2. Make sure you are running kernel through `make watch` command.
3. Produce a Unity wasm targeted build using the Build menu.
4. When the build finishes, only copy all the files with the `file1` and `file2` extensions to `static/unity/Build` folder within the `kernel` project. Do not copy the `unity loader` and `unity.json` files.
5. Run the browser explorer through `localhost:3000`. Now, it should use your local Unity build.

## Copyright info

This repository is protected with a standard Apache 2 license. See the terms and conditions in the [LICENSE](https://github.com/decentraland/unity-client/blob/master/LICENSE) file.
