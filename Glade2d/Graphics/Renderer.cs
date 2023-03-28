using Glade2d.Services;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using System;
using System.Collections.Generic;
using Glade2d.Graphics.BufferTransferring;
using Glade2d.Graphics.Layers;
using Glade2d.Profiling;

namespace Glade2d.Graphics
{
    public class Renderer : MicroGraphics
    {
        internal const int BytesPerPixel = 2;
        private readonly TextureManager _textureManager;
        private readonly LayerManager _layerManager;
        private readonly Layer _spriteLayer;
        private readonly Profiler _profiler;
        private readonly IBufferTransferrer _bufferTransferrer;
        
        public Color BackgroundColor
        {
            get => _spriteLayer.BackgroundColor;
            set => _spriteLayer.BackgroundColor = value;
        }

        public Color TransparentColor
        {
            get => _spriteLayer.TransparentColor;
            set => _spriteLayer.TransparentColor = value;
        }
        
        public bool ShowPerf { get; set; }
        public int Scale { get; private set; }
        public bool RenderInSafeMode { get; set; } = false;

        public Renderer(IGraphicsDisplay display, 
            TextureManager textureManager,
            LayerManager layerManager,
            Profiler profiler,
            int scale = 1) : base(display)
        {
            if (display.PixelBuffer.ColorMode != ColorMode.Format16bppRgb565)
            {
                var message = $"Only color mode rgb565 is supported, but {display.PixelBuffer.ColorMode} " +
                              "was given.";

                throw new InvalidOperationException(message);
            }

            _layerManager = layerManager;
            _textureManager = textureManager;
            _profiler = profiler;
            Scale = scale;

            // If we are rendering at a different resolution than our
            // device, we need to create a new buffer as our primary drawing buffer
            // so we draw at the scaled resolution
            if (scale > 1)
            {
                var scaledWidth = display.Width / scale;
                var scaledHeight = display.Height / scale;
                
                pixelBuffer = new BufferRgb565(scaledWidth, scaledHeight);
                LogService.Log.Trace($"Initialized renderer with custom buffer: {scaledWidth}x{scaledHeight}");
            }
            else
            {
                // do nothing, the default behavior is to draw
                // directly into the graphics buffer
                LogService.Log.Trace($"Initialized renderer using default display driver buffer: {display.Width}x{display.Height}");
            }

            CurrentFont = new Font4x6();
            
            _spriteLayer = Layer.FromExistingBuffer((BufferRgb565)pixelBuffer);
            _bufferTransferrer = new NoRotationBufferTransferrer();
        }

        public void Reset()
        {
            pixelBuffer.Fill(BackgroundColor);
            _spriteLayer.Clear();
            
            // Should we be clearing the display buffer too???
        }

        /// <summary>
        /// Renders the current scene
        /// </summary>
        public void Render(IReadOnlyList<Sprite> sprites)
        {
            _profiler.StartTiming("Renderer.Render");
            _profiler.StartTiming("LayerManager.RenderBackgroundLayers");
            _layerManager.RenderBackgroundLayers((BufferRgb565)pixelBuffer);
            _profiler.StopTiming("LayerManager.RenderBackgroundLayers");

            _profiler.StartTiming("Renderer.DrawSprites");
            sprites ??= Array.Empty<Sprite>();
            for (var x = 0; x < sprites.Count; x++)
            {
                // Use direct indexing instead of foreach for performance
                // due to IEnumerable allocations.
                DrawSprite(sprites[x]);
            }
            _profiler.StopTiming("Renderer.DrawSprites");
            
            _profiler.StartTiming("LayerManager.RenderForegroundLayers");
            _layerManager.RenderForegroundLayers((BufferRgb565)pixelBuffer);
            _profiler.StopTiming("LayerManager.RenderForegroundLayers");
           
            _profiler.StartTiming("Renderer.RenderToDisplay");
            RenderToDisplay();
            _profiler.StopTiming("Renderer.RenderToDisplay");
            _profiler.StartTiming("Renderer.Render");
        }

        public override void Show()
        {
            GameService.Instance.GameInstance.Profiler.StartTiming("Renderer.Show");
            // If we are doing a scaled draw, we must perform special copy logic
            if (Scale > 1)
            {
                GameService.Instance.GameInstance.Profiler.StartTiming("Renderer.Scale");
                var sourceBuffer = (BufferRgb565)pixelBuffer;
                var targetBuffer = (BufferRgb565)display.PixelBuffer;
                _bufferTransferrer.Transfer(sourceBuffer, targetBuffer, Scale);
                // ScaleIntoDisplayBuffer();
                GameService.Instance.GameInstance.Profiler.StopTiming("Renderer.Scale");
            }
            
            // if we're not doing a scaled draw, our buffers should match
            // draw the pixel buffer to the display
            else if (pixelBuffer != display.PixelBuffer)
            {
                display.PixelBuffer.WriteBuffer(0, 0, pixelBuffer);
            }

            GameService.Instance.GameInstance.Profiler.StartTiming("Micrographics.Show");
            base.Show();
            GameService.Instance.GameInstance.Profiler.StopTiming("Micrographics.Show");
            GameService.Instance.GameInstance.Profiler.StopTiming("Renderer.Show");
        }

        /// <summary>
        /// Renders the contents of the internal buffer to the driver buffer and
        /// then blits the driver buffer to the device
        /// </summary>
        private void RenderToDisplay()
        {
            // draw the FPS counter
            if (ShowPerf)
            {
                DrawRectangle(0, 0, Width, CurrentFont.Height, Color.Black, true);
                DrawText(0, 0, $"{GameService.Instance.Time.FPS:n1}fps", Color.White);
            }

            // send the driver buffer to device
            Show();
        }
        
        /// <summary>
        /// Draws a sprite's CurrentFrame into the graphics buffer
        /// </summary>
        /// <param name="sprite">The sprite to draw</param>
        private void DrawSprite(Sprite sprite)
        {
            if (sprite.CurrentFrame != null)
            {
                var spriteOrigin = new Point((int)sprite.X, (int)sprite.Y);
                var textureOrigin = new Point(sprite.CurrentFrame.X, sprite.CurrentFrame.Y);
                var dimensions = new Dimensions(sprite.CurrentFrame.Width, sprite.CurrentFrame.Height);

                var texture = _textureManager.GetTexture(sprite.CurrentFrame.TextureName);
                _spriteLayer.DrawTexture(texture, textureOrigin, spriteOrigin, dimensions);
            }
        }

        private void ScaleIntoDisplayBuffer()
        {
            // Copy the pixel buffer to the display buffer. Since the pixel buffer is scaled
            // down, we need to scale it up to the display buffer's resolution.
            unsafe
            {
                fixed (byte* displayBufferPtr = display.PixelBuffer.Buffer)
                fixed (byte* pixelBufferPtr = pixelBuffer.Buffer)
                {
                    var sourceWidth = pixelBuffer.Width;
                    var targetWidth = display.Width;

                    for (var sourceRow = 0; sourceRow < pixelBuffer.Height; sourceRow++)
                    {
                        // First copy the source row scaled horizontally
                        var sourceByte1 = pixelBufferPtr + (sourceRow * sourceWidth * BytesPerPixel);
                        var targetByte1 = displayBufferPtr + (sourceRow * targetWidth * Scale * BytesPerPixel);

                        for (var sourceCol = 0; sourceCol < pixelBuffer.Width; sourceCol++)
                        {
                            for (var scale = 0; scale < Scale; scale++)
                            {
                                *targetByte1 = *sourceByte1;
                                *(targetByte1 + 1) = *(sourceByte1 + 1);

                                targetByte1 += BytesPerPixel;
                            }

                            sourceByte1 += BytesPerPixel;
                        }

                    
                        // Next copy the previous pre-scaled target row for all scaled rows
                        var copyFromStartIndex = targetWidth * sourceRow * Scale * BytesPerPixel;
                        
                        for (var scale = 1; scale < Scale; scale++)
                        {
                            var copyToStartIndex = copyFromStartIndex + targetWidth * scale * BytesPerPixel;
                            Array.Copy(
                                display.PixelBuffer.Buffer, 
                                copyFromStartIndex, 
                                display.PixelBuffer.Buffer, 
                                copyToStartIndex,
                                targetWidth * BytesPerPixel);
                        }
                    }
                }
            }
        }
    }
}