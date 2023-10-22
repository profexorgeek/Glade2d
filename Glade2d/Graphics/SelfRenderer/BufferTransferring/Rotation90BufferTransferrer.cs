using System;
using Meadow.Foundation.Graphics.Buffers;

namespace Glade2d.Graphics.SelfRenderer.BufferTransferring;

public class Rotation90BufferTransferrer : IBufferTransferrer
{
    public void Transfer(BufferRgb565 source, BufferRgb565 target, int scale)
    {
        var sourceWidth = source.Width;
        var sourceHeight = source.Height;
        var targetWidth = target.Width;
        var targetHeight = target.Height;
        var targetRowByteLength = targetWidth * GladeSelfRenderer.BytesPerPixel;
        var sourceRowByteLength = sourceWidth * GladeSelfRenderer.BytesPerPixel;

        if (sourceWidth * scale != targetHeight || sourceHeight * scale != targetWidth)
        {
            var message = $"Source dimensions at scale {scale} is not compatible with " +
                          $"the target dimensions ({sourceWidth}x{sourceHeight} vs " +
                          $"{targetWidth}x{targetHeight})";

            throw new InvalidOperationException(message);
        }

        unsafe
        {
            fixed (byte* sourceBufferPtr = source.Buffer)
            fixed (byte* targetBufferPtr = target.Buffer)
            {
                // Since we are rotated such that width and height dimensions
                // are flipped, we need to transfer source columns to target
                // rows. Writing to target in rows is important for Array.Copy
                // calls for scaling.

                // A 90 degree clockwise rotation means we read pixels column by 
                // column and write them to the target row by row, starting from
                // the last pixel in the row to the first.
                var targetRowStartIndex = 0;
                var targetByte1 = targetBufferPtr + targetRowByteLength - GladeSelfRenderer.BytesPerPixel;

                for (var sourceCol = 0; sourceCol < sourceWidth; sourceCol++)
                {
                    var sourceByte1 = sourceBufferPtr + sourceCol * GladeSelfRenderer.BytesPerPixel;
                    for (var sourceRow = 0; sourceRow < sourceHeight; sourceRow++)
                    {
                        for (var x = 0; x < scale; x++)
                        {
                            *targetByte1 = *sourceByte1;
                            *(targetByte1 + 1) = *(sourceByte1 + 1);

                            targetByte1 -= GladeSelfRenderer.BytesPerPixel;
                        }

                        sourceByte1 += sourceRowByteLength;
                    }

                    for (var x = 1; x < scale; x++)
                    {
                        Array.Copy(
                            target.Buffer,
                            targetRowStartIndex,
                            target.Buffer,
                            targetRowStartIndex + targetRowByteLength,
                            targetRowByteLength);

                        targetRowStartIndex += targetRowByteLength;
                    }

                    targetRowStartIndex += targetRowByteLength;

                    // Go to the end of the row
                    targetByte1 = targetBufferPtr + targetRowStartIndex + targetRowByteLength - GladeSelfRenderer.BytesPerPixel;
                }
            }
        }
    }
}