using System;
using System.Runtime.CompilerServices;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;

namespace Glade2d.Graphics;

/// <summary>
/// Provides data types and functionality for performing drawing from one pixel buffer
/// to another.
/// </summary>
internal static class Drawing
{
    private const int BytesPerPixel = 2; 
    
    /// <summary>
    /// Defines how pixels should be moved from one buffer to another
    /// </summary>
    public readonly record struct Operation(BufferRgb565 Source,
        BufferRgb565 Target,
        Point SourceStart, 
        Point TargetStart, 
        Dimensions Dimensions,
        Color? TransparencyColor);

    public static void ExecuteOperation(Operation operation)
    {
        if (operation.TransparencyColor != null)
        {
            ExecuteTransparentDraw(operation);
        }
        else
        {
            ExecuteNonTransparentDraw(operation);
        }
    }

    private static void ExecuteNonTransparentDraw(Operation operation)
    {
        var bytesPerRowToCopy = operation.Dimensions.Width * BytesPerPixel;
        var layerBuffer = operation.Source.Buffer;
        var targetBuffer = operation.Target.Buffer;
        for (var row = 0; row < operation.Dimensions.Height; row++)
        {
            var layerRow = row + operation.SourceStart.Y;
            var targetRow = row + operation.TargetStart.Y;
            var layerIndex = GetBufferIndex(operation.SourceStart.X, layerRow, operation.Source.Width);
            var targetIndex = GetBufferIndex(operation.TargetStart.X, targetRow, operation.Target.Width);
            
            Array.Copy(layerBuffer,
                layerIndex,
                targetBuffer,
                targetIndex,
                bytesPerRowToCopy);
        }
    }

    private static void ExecuteTransparentDraw(Operation operation)
    {
        var transparencyShort = operation.TransparencyColor!.Value.Color16bppRgb565;
        var transparencyColorByte1 = (byte)(transparencyShort >> 8);
        var transparencyColorByte2 = (byte)transparencyShort;
        
        var layerBuffer = operation.Source.Buffer;
        var layerWidth = operation.Source.Width;
        var targetWidth = operation.Target.Width;
        var totalHeight = operation.Dimensions.Height;
        var totalWidth = operation.Dimensions.Width;

        unsafe
        {
            fixed (byte* sourceBufferPtr = operation.Source.Buffer)
            fixed (byte* targetBufferPtr = operation.Target.Buffer)
            {
                var sourceStartIndex = GetBufferIndex(operation.SourceStart.X, 
                    operation.SourceStart.Y,
                    operation.Source.Width);

                var targetStartIndex = GetBufferIndex(operation.TargetStart.X,
                    operation.TargetStart.Y,
                    operation.Target.Width);

                var sourceBytesPerRow = operation.Source.Width * BytesPerPixel;
                var targetBytesPerRow = operation.Target.Width * BytesPerPixel;
                
                for (var row = 0; row < totalHeight; row++)
                {
                    var sourceByte1 = sourceBufferPtr + sourceStartIndex + (row * sourceBytesPerRow);
                    var sourceByte2 = sourceByte1 + 1;
                    var targetByte1 = targetBufferPtr + targetStartIndex + (row * targetBytesPerRow);
                    var targetByte2 = targetByte1 + 1;

                    for (var col = 0; col < totalWidth; col++)
                    {
                        if (*sourceByte1 != transparencyColorByte1 || *sourceByte2 != transparencyColorByte2)
                        {
                            *targetByte1 = *sourceByte1;
                            *targetByte2 = *sourceByte2;
                        }

                        sourceByte1 += BytesPerPixel;
                        sourceByte2 += BytesPerPixel;
                        targetByte1 += BytesPerPixel;
                        targetByte2 += BytesPerPixel;
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Gets the index for a specific x and y coordinate in a pixel buffer. All values provided
    /// are pixel based, not byte based.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetBufferIndex(int x, int y, int width)
    {
        return (y * width + x) * BytesPerPixel;
    }
}