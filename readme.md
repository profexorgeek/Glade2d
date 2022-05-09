![](/media/glade2d-logo.png)

# Glade2d

Glade2d is a 2D rendering/game engine intended to run on a [Wilderness Labs Meadow Board](https://www.wildernesslabs.co/). Clearly this is silly because a microcontroller is not well-suited for game development and the board is intended more for IoT applications.

But it can draw to a screen and therefore a game engine should exist for it.

A "glade" is an open space in the forest, aka a meadow ;)

## Using

Currently, Glade2d is very early in development and does not have any releases or packages. Feel free to clone the repository and experiment with it.

Glade2d requires a Meadow board and a display. We have only tested it with an st7789. If you try it on something else, let us know how it goes!

We will publish some documentation on how to set up the hardware when we are a little further in this project.

## Documentation

We intend to publish docs in [the docs folder of this repository as simple markdown](/docs/index.md) but we are not that far yet.

## Contributing

File an issue, write a doc, submit a PR, or reach out to @profexorgeek on Twitter.

## Roadmap

- [x] Get texture loading and rendering on a graphics device
- [x] Get a simple renderer that can draw frames from a spritesheet
- [x] Create the core game loop with update delta tracking
- [x] Enable a smaller graphics buffer that can scale after compositing is complete
- [ ] Create the concept of a drawable object
- [x] Create a scene graph that holds drawable objects
- [ ] Enable parent child relationships and a tree-shaped scene graph
- [ ] Set up the renderer to render the scene graph

