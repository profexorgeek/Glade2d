using Meadow.Foundation.Graphics.Buffers;

namespace Glade2d.Graphics.BufferTransferring;

/// <summary>
/// Transfers RGB565 data from one buffer to another
/// </summary>
internal interface IBufferTransferrer
{
    void Transfer(BufferRgb565 source, BufferRgb565 target, int scale);
}