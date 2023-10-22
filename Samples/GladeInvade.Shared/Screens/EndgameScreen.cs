using Glade2d;
using Glade2d.Graphics.Layers;
using Glade2d.Input;
using Glade2d.Screens;
using Glade2d.Services;
using GladeInvade.Shared.Services;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using Glade2d.Graphics;

namespace GladeInvade.Shared.Screens
{
    public class EndgameScreen : Screen
    {
        const float SecondsToIgnoreInput = 4f;
        const int ScreenEdgePadding = 4;

        readonly int _screenHeight, _screenWidth;
        ILayer _mainTextLayer, _inputPromptLayer;
        double _secondsUntilInputEnabled = SecondsToIgnoreInput;
        readonly Game _game;

        public EndgameScreen()
        {
            _game = GameService.Instance.GameInstance;
            _screenHeight = _game.Renderer.Height;
            _screenWidth =  _game.Renderer.Width;

            CreateTextLayers();
        }

        public override void Activity()
        {
            // countdown until it's time to show the input prompt. Then
            // show the input prompt and start listening for input
            _secondsUntilInputEnabled -= GameService.Instance.Time.FrameDelta;
            if (_secondsUntilInputEnabled <= 0)
            {
                if(_game.LayerManager.ContainsLayer(_inputPromptLayer) == false)
                {
                    _game.LayerManager.AddLayer(_inputPromptLayer, 1);
                }

                DoPlayerInput();
            }

            base.Activity();
        }

        /// <summary>
        /// Listen and react to player input
        /// </summary>
        void DoPlayerInput()
        {
            if(_game.InputManager.GetButtonState(nameof(GameInputs.ActionButton)) == ButtonState.Pressed)
            {
                _game.TransitionToScreen(() => new TitleScreen());
            }
        }

        /// <summary>
        /// Create all of the text layers and draw text to them
        /// </summary>
        void CreateTextLayers()
        {
            _mainTextLayer = GameService.Instance.GameInstance.Renderer.CreateLayer(new Glade2d.Dimensions(_screenWidth, _screenHeight));
            _mainTextLayer.BackgroundColor = GameConstants.BackgroundColor;
            _mainTextLayer.DrawLayerWithTransparency = false;
            _mainTextLayer.Clear();
            _game.LayerManager.AddLayer(_mainTextLayer, -1);

            // draw gameover text
            DrawCenteredText(_mainTextLayer, ScreenEdgePadding, "Game Over", GameConstants.RedTextColor, new Font6x8());

            // draw score text
            var scoreTextString = $"Score: {ProgressionService.Instance.Score}";
            DrawCenteredText(_mainTextLayer, _screenHeight / 4 * 1, scoreTextString, GameConstants.WhiteTextColor);

            // draw level text
            var levelTextString = $"Level: {ProgressionService.Instance.CurrentLevel}";
            DrawCenteredText(_mainTextLayer, _screenHeight / 4 * 2, levelTextString, GameConstants.WhiteTextColor);

            // draw kills text
            var killsTextString = $"Kills: {ProgressionService.Instance.Kills}";
            DrawCenteredText(_mainTextLayer, _screenHeight / 4 * 3, killsTextString, GameConstants.WhiteTextColor);



            var inputPromptFont = new Font4x6();
            var inputPromptString = "Press [Action] to continue!";
            _inputPromptLayer = GameService.Instance.GameInstance.Renderer.CreateLayer(new Glade2d.Dimensions(_screenWidth, inputPromptFont.Height));
            _inputPromptLayer.BackgroundColor = GameConstants.BackgroundColor;
            _inputPromptLayer.DrawLayerWithTransparency = false;
            _inputPromptLayer.Clear();
            _inputPromptLayer.CameraOffset = new Point(0, _screenHeight - _inputPromptLayer.Height - ScreenEdgePadding);
            // NOTE: we don't add this layer to the layer manager yet because we don't want it to show!

            DrawCenteredText(_inputPromptLayer, 0, inputPromptString, GameConstants.RedTextColor);
        }

        /// <summary>
        /// Draws centered text at the specified Y coordinate with the provided 
        /// color and font, on the provided layer
        /// </summary>
        /// <param name="layer">The layer to draw to</param>
        /// <param name="yPos">The Y position of the text</param>
        /// <param name="text">The text to draw</param>
        /// <param name="color">The color of the text</param>
        /// <param name="font">The font to use</param>
        void DrawCenteredText(ILayer layer, int yPos, string text, Color color, IFont font = null)
        {
            font = font ?? layer.DefaultFont;
            var textWidth = font.Width * text.Length;
            var xPos = (int)((_screenWidth / 2f) - (textWidth / 2f));

            layer.DrawText(
                position: new Point(xPos, yPos),
                text: text,
                font: font,
                color: color);
        }


    }
}
