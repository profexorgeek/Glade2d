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
        const int baseEnemySpeed = 7;
        const float levelSpeedMultiplier = 1.5f;
        const uint pointsPerEnemy = 1;
        public const int EnemyVerticalMovementAmount = 3;

        private uint _enemyKills = 0;
        private uint _currentLevel = 1;
        private uint _score = 0;
        private uint _lives = 3;


        /// <summary>
        /// The current difficulty of the game, affects enemy speed
        /// </summary>
        public uint CurrentLevel => _currentLevel;
        
        /// <summary>
        /// The player's current score
        /// </summary>
        public uint Score => _score;

        /// <summary>
        /// The player's current lives
        /// </summary>
        public uint Lives => _lives;

        /// <summary>
        /// How many enemy creatures the player has destroyed
        /// </summary>
        public uint Kills => _enemyKills;

        /// <summary>
        /// Calculated property that determines enemy speed based
        /// on a base speed and the current difficulty level
        /// </summary>
        public int CurrentEnemySpeed => (int)Math.Round(baseEnemySpeed * (levelSpeedMultiplier * _currentLevel));

        /// <summary>
        /// Singleton accessor
        /// </summary>
        public static ProgressionService Instance => instance ?? (instance = new ProgressionService());

        private ProgressionService() { }

        /// <summary>
        /// Resets all scoring to the starting state
        /// </summary>
        public void Restart()
        {
            _currentLevel = 1;
            _score = 0;
            _enemyKills = 0;
            _lives = 3;
        }

        /// <summary>
        /// Raises the game's difficulty level
        /// </summary>
        public void IncreaseDifficultyLevel()
        {
            _currentLevel += 1;

            LogService.Log.Info($"Game level is now: {_score}");
        }

        /// <summary>
        /// Updates the player's kill count and score
        /// </summary>
        public void AwardEnemyKill()
        {
            _enemyKills += 1;
            _score += (pointsPerEnemy * CurrentLevel);
            LogService.Log.Info($"Enemy killed! Score: {_score} Kills: {_enemyKills}");
        }

        /// <summary>
        /// Removes a life from the player
        /// </summary>
        public void RemoveLife()
        {
            _lives--;
        }
    }
}
