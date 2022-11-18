# Glade2d

Glade2d is a simple rendering engine designed to run on a microcontroller. It is currently very slow since it is doing pixel-by-pixel drawing in the software layer, but we have plans to speed that up significantly.

## The Hardware Layer

Before you can start running Glade2d, you must set up your hardware.

If you have a Project Lab board, you can look at the [Project Lab repository](https://github.com/WildernessLabs/Meadow.ProjectLab) for samples and demos to help set up your MeadowApp to connect to the display, buttons, and audio.

If you are connecting custom components to your Meadow, you can start in the [Meadow.Foundation repository](https://github.com/WildernessLabs/Meadow.Foundation) for samples and demos on connecting to various types of hardware.

At a minimum, the Glade2d engine requires an `IGraphicsDisplay` instance to render to.

## The `Game` Class

The Glade2d application lifecycle starts in the `Game.cs` class. The game class is responsible for initializing the renderer, starting the game loop, managing the currently-loaded screen, and updating all game entities each frame.

### Starting Glade2d

The steps to start a Glade2d game instance typically include:

1. Creating a `Game` instance via a call to the empty constructor
1. Initializing the engine using the `Initialize` method and providing a `IGraphicsDisplay`, a rendering scale, mode and rotation
1. Starting the game loop

### Initialization Settings

- `IGraphicsDisplay` - This is an instance of a graphics driver, which Glade2d will use for rendering.
- `DisplayScale` - This field is specified in the `Initialize` call and defines the render scale. A value of `2` will cause the renderer to render at half of the display resolution, or 200% scale. The default is `1` which does not apply any scaling.
- `EngineMode`- This enum value is specified in the `Initialize` call and determines whether the engine runs as fast as possible, or if it renders on demand. Rendering on demand is useful to reduce processor load and battery consumption if you only need to update the display periodically.
- `RotationType` This enum value allows you to change the rotation of the rendering so it aligns with the direction you are viewing the screen.

### Other Settings

- `SleepMilliseconds` - Glade2d will attempt to run as fast as possible by default. This can cause problems with listening for input or performing other non-render tasks. Setting a non-zero value here will force the engine to wait for the specified milliseconds after each frame update.

## The `Renderer` Class

Contributors Needed: Help me document this!

## Using the `Screen` Class

Contributors Needed: Help me document this!

## Using the `Sprite` Class

Contributors Needed: Help me document this!