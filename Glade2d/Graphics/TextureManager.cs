using System;
using System.Collections.Generic;
using System.IO;
using Glade2d.Services;
using Meadow;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;

namespace Glade2d.Graphics;

/// <summary>
/// Manages loading, unloading, and utilization of textures
/// </summary>
public class TextureManager
{
    private readonly Dictionary<string, BufferRgb565> _textures = new();
    
    /// <summary>
    /// Loads a new texture
    /// </summary>
    public void Load(string textureName)
    {
        if (_textures.ContainsKey(textureName))
        {
            return;
        }

        var image = LoadBitmapFile(textureName);
        _textures.Add(textureName, image);
    }

    /// <summary>
    /// Removes a texture from the cache
    /// </summary>
    public void Unload(string textureName)
    {
        _textures.Remove(textureName);
    }

    /// <summary>
    /// Gets the texture with the specified name. If the texture with that name hasn't been loaded yet, it will
    /// attempt to load it.
    /// </summary>
    public BufferRgb565 GetTexture(string textureName)
    {
        if (!_textures.TryGetValue(textureName, out var buffer))
        {
            buffer = LoadBitmapFile(textureName);
            _textures.Add(textureName, buffer);
        }

        return buffer;
    }
    
    /// <summary>
    /// Loads a bitmap file from disk and creates an IDisplayBuffer
    /// </summary>
    /// <param name="name">The bitmap file path</param>
    /// <returns>An IDisplayBuffer containing bitmap data</returns>
    private static BufferRgb565 LoadBitmapFile(string name)
    {
        LogService.Log.Trace($"Attempting to LoadBitmapFile: {name}");
        var filePath = Path.Combine(MeadowOS.FileSystem.UserFileSystemRoot, name);

        try
        {
            var img = Image.LoadFromFile(filePath);

            // Always make sure that the texture is formatted in the same color mode as the display
            var imgBuffer = new BufferRgb565(img.Width, img.Height);
            imgBuffer.WriteBuffer(0, 0, img.DisplayBuffer);
            LogService.Log.Trace($"{name} loaded to buffer of type {imgBuffer.GetType()}");
            return imgBuffer;
        }
        catch (Exception exception)
        {
            LogService.Log.Error($"Failed to load {filePath}: The file should be a 24bit bmp, in the root " +
                                 $"directory with BuildAction = Content, and Copy if Newer!",
                exception);
            
            throw;
        }
    }
}