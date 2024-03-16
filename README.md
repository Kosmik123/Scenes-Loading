# Bipolar Scene Management (or Scenes Loading)
[![Unity 2020.4+](https://img.shields.io/badge/unity-2020.4%2B-blue.svg)](https://unity3d.com/get-unity/download)
[![License: MIT](https://img.shields.io/badge/License-MIT-brightgreen.svg)](https://opensource.org/license/mit)

**Bipolar Scene Management (or Scenes Loading, the official name is not decided yet)** is a system for managing scenes in easier and more relevant way than default Unity solution. The whole system is based on ScenesContexts which represent multiple scenes loaded together. 

## Problems solved by the system  
Bipolar Scene Management addresses a few problems with Unity scenes loading.  

### Game lag while loading first scene after splash screen
This problem is quite common and have been seen in many games. By default the first scene starts loading asynchronously while splash screen is visible. However, often the scene is so big that the loading duration exceeds splash screen duration. This can happen when objects in the scene reference many big assets such as textures or meshes. Loading them from the disk to RAM might take a while. 

That's where this module becomes handy. In Bipolar Scene Management the first scene (with build index = 0) is considered Init or Main scene. It is always active and is never unloaded. It discourages developer from containing big assets within the scene and encourages to make it as small as possible, making loading it quick.

### Error-prone managing of scene paths and build indices  
While writing scene changing scripts developers often encounter several problems with scene build indices and paths. If loading scenes by scene path, paths need to be cached somewhere as readonly strings and in case of changing scene directory these cached paths have to be changed as well. If scenes are loaded by index, these indices might also change when scenes are reordered in Build Settings. Developers need to remember about all these changes and dependencies, which makes handling scenes prone to errors. 

To solve these problems (any a few more) ScenesContexts where introduced. Scenes contexts takes away the responsibility of remembering scene paths and build indices from developers and ensure these values are always correctly set. Instead of loading scene by path or build index, developers load scenes context and all referenced scenes are loaded properly even if path or build index were changed.

## Additional helpful features
Bipolar Scene Management also add some features that are often useful while managing scenes in games:
1) Always loaded main scene which can contain persistent player data
2) Tracking of scene loading progress (both reading from disk and loading scene subsystems)
3) Loading screen which is visible while changing scenes (optional feature)
