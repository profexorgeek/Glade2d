![](/media/glade2d-logo.png)

# Glade2d

Glade2d is a 2D rendering/game engine intended to run on a [Wilderness Labs Meadow Board](https://www.wildernesslabs.co/). Clearly this is silly because a microcontroller is not well-suited for game development and the board is intended more for IoT applications.

But it can draw to a screen and therefore a game engine should exist for it.

A "glade" is an open space in the forest, aka a meadow ;)

To see an example showing how to configure custom `Sprite` objects and render them in a custom `Screen` check out the [Demo Enitities](https://github.com/profexorgeek/Glade2d/blob/master/Glade2d/Examples/DemoEntities.cs) file or download and run the app!

![image](https://user-images.githubusercontent.com/711100/167543124-df31e10e-ee33-4441-bcf5-95a89e12e3fd.png)


## Using

Currently, Glade2d is very early in development and does not have any releases or packages. Feel free to clone the repository and experiment with it.

Glade2d requires a Meadow board and a display. We have only tested it with an st7789. If you try it on something else, let us know how it goes!

We will publish some documentation on how to set up the hardware when we are a little further in this project.

## Documentation

We intend to publish docs in [the docs folder of this repository as simple markdown](/docs/index.md) but we are not that far yet.

## Contributing

File an issue, write a doc, submit a PR, or reach out to [@profexorgeek](https://twitter.com/profexorgeek) on Twitter.

## Roadmap

- [x] Get texture loading and rendering on a graphics device
- [x] Get a simple renderer that can draw frames from a spritesheet
- [x] Create the core game loop with update delta tracking
- [x] Enable a smaller graphics buffer that can scale after compositing is complete
- [x] Create the concept of a drawable object
- [x] Create a scene graph that holds drawable objects
- [x] Set up the renderer to render the scene graph
- [x] Create a demo scene with example entities
- [ ] Add concept of velocity to sprites
- [ ] Sprites should be destroyable
- [ ] Demo scene should demo velocity and destroying
- [ ] Glade2d should be spun out from MeadowApp
- [ ] Changing sprite Layer after adding to scene should trigger a re-sort
- [ ] Create an input manager that allows buttons to be registered and button presses to be tracked each frame
- [ ] CONSIDERATION: Enable parent child relationships and a tree-shaped scene graph


