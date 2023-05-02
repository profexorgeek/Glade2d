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

        private uint enemyKills = 0;
        private uint currentLevel = 1;
        private uint score = 0;


        public uint CurrentLevel => currentLevel;
        public uint Score => score;

        public int CurrentEnemySpeed => (int)Math.Round(baseEnemySpeed * (levelSpeedMultiplier * currentLevel));

        public static ProgressionService Instance => instance ?? (instance = new ProgressionService());

        private ProgressionService() { }

        public void Restart()
        {
            currentLevel = 1;
            score = 0;
            enemyKills = 0;
        }

        public void IncreaseDifficultyLevel()
        {
            currentLevel += 1;

            LogService.Log.Info($"Game level is now: {score}");
        }

        public void AwardEnemyKill()
        {
            score += (pointsPerEnemy * CurrentLevel);
            LogService.Log.Info($"Enemy killed! Score: {score} Kills: {enemyKills}");
        }
    }
}
