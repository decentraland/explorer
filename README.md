# Decentraland Explorer

### Before you start

1. [Contribution Guidelines](.github/CONTRIBUTING.md)
2. [Coding Guidelines](unity-client/style-guidelines.md)
3. Code Review Standards

## Running the Explorer

This repo requires `git lfs` to track images and other binary files. https://git-lfs.github.com/ .
So, before anything make sure you have it installed by typing:

    git lfs install
    git lfs pull

Explorer is composed of two main projects, Kernel (ts) and Renderer (Unity). The Unity part needs its Kernel ts-based counterpart for many [responsibilities](https://docs.google.com/document/d/1_lzi3V5IDaVRJbTKNsNEcaG0L21VPydiUx5uamiyQnY/edit).
However, if you only intend to make changes related to Unity only you can skip the Kernel build by connecting Unity Editor with a deployed explorer build via websocket. More on this on the following section.

---

### Debug using Unity only

#### Why you should care

Take this path if you intend to contribute on features without the need of modifying Kernel.
This is the recommended path for artists.

#### Steps

1. Download and install Unity 2019.4.0f1
2. Open the scene named `InitialScene`
3. Within the scene, select the `WSSController` GameObject.
4. On `WSSController` inspector, make sure that `Base url mode` is set to `Custom` 
and `Base url custom` is set to `https://play.decentraland.zone/?`
5. Run the Initial Scene in the Unity editor
6. A browser tab with `explorer` should open automatically and steal your focus, don't close it!. Login with your wallet, go back to Unity and explorer should start running on the `Game View`.
7. As you can see, `WSSController` has other special options like the starting position, etc. You are welcome to use them as you see fit, but you'll have to close the tab and restart the scene for them to make effect.

#### Troubleshooting

##### Missing git lfs extension
If while trying to compile the Unity project you get an error regarding some libraries that can not be added (for instance Newtonsoft
Json.NET or Google Protobuf), please execute the following command in the root folder:

    git lfs install
    git lfs pull

Then, on the Unity editor, click on `Assets > Reimport All`

---

### Debug using Kernel only

#### Why you should care

TBD

#### Steps

Make sure you have the following dependencies:
- Latest version of GNU make, install it using `brew install make`
- If you are using Windows 10, you must enable the Linux subsystem and install a Linux distro from Windows Store like Ubuntu. Then install all tools and dependecies like nodejs, npm, typescript, make, et cetera.
- Node v10 or compatible installed via `sudo apt install nodejs` or [nvm](https://github.com/nvm-sh/nvm)
- yarn installed globally via `npm install yarn -g`

---
**IMPORTANT:** If your path has spaces the build process will fail. Make sure to clone this repo in a properly named path.

---
When all the dependencies are in place, you can start building the project.

First off, we need the npm packages for **website** and **kernel**. In most of the cases this should be done only once:

    cd website
    npm install
    cd kernel
    npm install

By now, you can run and watch a server with the kernel build by typing:

    make watch

The make process will take a while. When its finished, you can start debugging the browser's explorer by going to http://localhost:3000/

Note that the Unity version used by this approach will be the latest version deployed to `master` branch. If you need a local Unity build, check out the **advanced debugging scenarios**.

#### Run kernel tests

To see test logs/errors directly in the browser, run:

    make watch

Now, navigate to [http://localhost:8080/test](http://localhost:8080/test)

#### Troubleshooting

##### Missing xcrun (macOS)
If you get the "missing xcrun" error when trying to run the `make watch` command, you should download the latest command line tools for macOS, either by downloading them from https://developer.apple.com/download/more/?=command%20line%20tools or by re-installing XCode

---

## Advanced debugging scenarios

### Debug with Unity Editor + local Kernel

#### Why you should care

TBD

#### Steps

* Make sure you have the proper Unity version up and running
* Make sure you are running kernel through `make watch` command.
* Back in unity editor, open the `WSSController` component inspector of `InitialScene`
* Make sure that is setup correctly

### Debug with browsers + local Unity build

#### Why you should care

TBD

#### Steps

1. Make sure you have the proper Unity version up and running
2. Make sure you are running kernel through `make watch` command.
3. Produce a Unity wasm targeted build using the Build menu.
4. When the build finishes, only copy all the files with the `file1` and `file2` extensions to `static/unity/Build` folder within the `kernel` project. Do not copy the `unity loader` and `unity.json` files.
5. Run the browser explorer through `localhost:3000`. Now, it should use your local Unity build.

## Technical how-to guides and explainers

- [How to create new SDK components]()
- [How to debug with local test parcels and preview scenes]()
- [How to use Unity visual tests]()
- [How to profile a local Unity build remotely]()
- [Kernel-unity native interface explainer and maintenance guide]()
- [Create typescript worker how-to guide]()

## Copyright info

This repository is protected with a standard Apache 2 license. See the terms and conditions in the [LICENSE](https://github.com/decentraland/unity-client/blob/master/LICENSE) file.
