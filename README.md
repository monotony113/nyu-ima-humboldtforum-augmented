### IMNY-9001 Augmenting the Gallery, NYU Berlin, Spring 2019
# Humboldt Forum Augmented

An iOS AR app for indoor exploration in Humboldt Forum, a museum in Berlin, Germany.

It maps locations and exhibits in the museum onto the screen, provides basic descriptions of the objects (obtained from the museum), and supports object filtering and bookmarking.

It locates the user in the library through various means, including motion tracking and the device's built-in compass. Making use of AR Image Anchors from ARKit, the prototype also attempts to locate the user through image recognition. Ideally, location info should be provided through external sensors like Wi-Fi beacons, Bluetooth, etc.

App prototyped in Unity. Requires an iOS device with ARKit 2+ support.

This repo is derived from the course final project, which was a group project with Ava, Sammy, and Angelica. 

## Demo

(If videos are not showing please turn to the repo's [GitHub Page](https://monotony113.github.io/nyu-ima-humboldtforum-augmented/))

Initial screen:
<video width="270" height="480" controls>
    <source src="demo/1-init.MP4" type="video/mp4">
</video>

Filtering objects by topics:
<video width="270" height="480" controls>
    <source src="demo/2-filter.MP4" type="video/mp4">
</video>

Object descriptions:
<video width="270" height="480" controls>
    <source src="demo/3-read-more.MP4" type="video/mp4">
</video>

Simple direction indicator:
<video width="270" height="480" controls>
    <source src="demo/4-navigation.MP4" type="video/mp4">
</video>

Bookmarking:
<video width="270" height="480" controls>
    <source src="demo/5-bookmark.MP4" type="video/mp4">
</video>

Locating the user:
<video width="270" height="480" controls>
    <source src="demo/6-location.MP4" type="video/mp4">
</video>

Complete experience:
<video width="270" height="480" controls>
    <source src="demo/7-flow.MP4" type="video/mp4">
</video>


## Installing

Requirements:
- Unity (source uses version 2018.3.6)
- Xcode 11 or above
- iOS device with ARKit supports

1. Download or clone from repo
2. Open project with Unity
3. Import TextMesh Pro; opening Assets/Scenes/ARScene.unity should trigger the import wizard
4. Fix missing font errors etc. (some fonts are not supplied because they are copyrighted)
5. **Switch build platform to iOS**
6. Build and run

## About Humboldt Forum
[Humboldt Forum](https://www.humboldtforum.org/en) is a new museum in Berlin, residing in the reconstructed Berlin Palace and set to open late 2019/early 2020.

## About Augmenting the Gallery
NYU Berlin spring 2019 course by Pierre Depaz. [Course repo](https://github.com/periode/augmenting-gallery)


<style>
    video {
        padding-bottom: 20px;
    }
</style>
