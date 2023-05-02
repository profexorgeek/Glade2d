![](/media/glade2d-logo.png)

# Glade2d

Glade2d is a 2D rendering/game engine intended to run on a [Wilderness Labs Meadow Board](https://www.wildernesslabs.co/). Clearly this is silly because a microcontroller is not well-suited for game development and the board is intended for IoT applications. But it can draw to a screen and therefore a game engine should exist for it.

A "glade" is an open space in the forest, aka a meadow, which is what Glade2d was designed to run on.

![image](https://user-images.githubusercontent.com/711100/167543124-df31e10e-ee33-4441-bcf5-95a89e12e3fd.png)

## Getting Started

Currently, Glade2d early in development and does not have official releases or packages. To experiment with Glade2d:

1. Clone the repository and open the Glade2d.sln in Visual Studio
1. If you have a Meadow installed on a ProjectLab board v2.e or better, deploy the `SampleProjectLab` application
1. If you do not have a Meadow device or just want to test on your dev machine, run the `SampleGtk` application

The sample application loads a spritesheet, defines multiple entities based on sprites from the spritesheet, and composes the entities into a tiny scene while displaying the FPS. This is a basic example of how to load sprites and composite them into a single buffer. We hope to add more demos as we add features to the engine!

## Documentation

See the [docs index page in this repo](/docs/index.md) to get started.

## Contributing

File an issue, write a doc, submit a PR, or reach out to [@profexorgeek](https://twitter.com/profexorgeek) on Twitter.

## Roadmap

The roadmap/backlog has now moved to a [Trello board](https://trello.com/b/YuEifteL/glade2d)!

