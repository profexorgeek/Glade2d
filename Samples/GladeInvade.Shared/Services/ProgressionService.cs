using Glade2d.Services;
using Meadow.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace GladeInvade.Shared.Services
{
    /// <summary>
    /// Manages the game progression across screens. Provides
    /// way to store current level, enemy speed, and other
    /// values that are critical to the game state
    /// </summary>
    public class ProgressionService
    {
        private static ProgressionService instance;
        const int baseEnemySpeed = 5;
        const float levelSpeedMultiplier = 1.5f;
        const uint pointsPerEnemy = 1;
        public const int EnemyVerticalMovementAmount = 4;


        private uint _enemyKills = 0;
        private uint _currentLevel = 1;
        private uint _score = 0;
        private uint _lives = 3;



        public uint CurrentLevel => _currentLevel;
        public uint Score => _score;
        public uint Lives => _lives;

        public int CurrentEnemySpeed => (int)Math.Round(baseEnemySpeed * (levelSpeedMultiplier * _currentLevel));

        public static ProgressionService Instance => instance ?? (instance = new ProgressionService());

        private ProgressionService() { }

        public void Restart()
        {
            _currentLevel = 1;
            _score = 0;
            _enemyKills = 0;
            _lives = 3;
        }

        public void IncreaseDifficultyLevel()
        {
            _currentLevel += 1;

            LogService.Log.Info($"Game level is now: {_score}");
        }

        public void AwardEnemyKill()
        {
            _score += (pointsPerEnemy * CurrentLevel);
            LogService.Log.Info($"Enemy killed! Score: {_score} Kills: {_enemyKills}");
        }

        public void RemoveLife()
        {
            _lives--;
        }
    }
}
