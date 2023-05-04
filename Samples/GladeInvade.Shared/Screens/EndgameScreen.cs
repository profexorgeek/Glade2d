using Glade2d;
using Glade2d.Graphics.Layers;
using Glade2d.Screens;
using Glade2d.Services;
using GladeInvade.Shared.Services;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GladeInvade.Shared.Screens
{
    public class EndgameScreen : Screen
    {
        const float SecondsToIgnoreInput = 4f;

        readonly int _screenHeight, _screenWidth;
        Layer _mainTextLayer, _inputPromptText;
        float _secondsUntilInputEnabled;
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
            base.Activity();
        }


        void CreateTextLayers()
        {
            _mainTextLayer = Layer.Create(new Glade2d.Dimensions(_screenWidth, _screenHeight));
            _mainTextLayer.BackgroundColor = GameConstants.BackgroundColor;
            _mainTextLayer.DrawLayerWithTransparency = false;
            _mainTextLayer.Clear();
            _game.LayerManager.AddLayer(_mainTextLayer, -1);


            // draw gameover text
            DrawCenteredText(_mainTextLayer, 8, "Game Over", GameConstants.RedTextColor, new Font6x8());

            // draw score text
            var scoreTextString = $"Score: {ProgressionService.Instance.Score}";
            DrawCenteredText(_mainTextLayer, _screenHeight / 4 * 1, scoreTextString, GameConstants.WhiteTextColor);

            // draw level text
            var levelTextString = $"Level: {ProgressionService.Instance.CurrentLevel}";
            DrawCenteredText(_mainTextLayer, _screenHeight / 4 * 2, levelTextString, GameConstants.WhiteTextColor);

            // draw kills text
            var killsTextString = $"Kills: {ProgressionService.Instance.Kills}";
            DrawCenteredText(_mainTextLayer, _screenHeight / 4 * 3, killsTextString, GameConstants.WhiteTextColor);
        }

        void DrawCenteredText(Layer layer, int yPos, string text, Color color, IFont font = null)
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
