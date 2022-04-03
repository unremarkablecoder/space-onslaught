using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace {
    public class WaveConfig {
        public int minNumGroups = 1;
        public int maxNumGroups = 1;
        public int minNumPerGroup = 3;
        public int maxNumPerGroup = 3;
        public int minTicksUntilNext = 600;
        public int maxTicksUntilNext = 600;
    }

    public class Wave {
        public int numGroups;
        public int numPerGroup;
        public int ticksUntilNext;
    }
    
    public class EnemyWaves {
        public int currentWaveIndex = -1;
        
        private int lastWaveTicks = 0;
        
        private Wave currentWave;
        private Game game;
        
        public EnemyWaves(Game game) {
            this.game = game;
        }

        private WaveConfig NextConfig() {
            currentWaveIndex++;

            var cfg = new WaveConfig();
            cfg.minNumGroups = 1 + Math.Min(20, currentWaveIndex) / 2;
            cfg.maxNumGroups = cfg.minNumGroups + cfg.minNumGroups * 25 / 100;
            cfg.minNumPerGroup = 2 + Math.Min(20, currentWaveIndex) / 2;
            cfg.maxNumPerGroup = cfg.minNumPerGroup + cfg.minNumPerGroup * 50 / 100;
            cfg.minTicksUntilNext = cfg.maxTicksUntilNext = 600;
            

            return cfg;
        }
        
        private Wave SpawnWave() {
            var config = NextConfig();
            currentWave = new Wave();
            currentWave.numGroups = Random.Range(config.minNumGroups, config.maxNumGroups + 1);
            currentWave.numPerGroup = Random.Range(config.minNumPerGroup, config.maxNumPerGroup + 1);
            currentWave.ticksUntilNext = Random.Range(config.minTicksUntilNext, config.maxTicksUntilNext + 1);

            Vector3 hqPos = game.CoordToPos(new Vector2Int(game.GetMapSize()/2, game.GetMapSize()/2));
            float enemyPower = 1.0f * (Mathf.Pow(1.1f, Math.Max(0, currentWaveIndex - 20)));
            for (int group = 0; group < currentWave.numGroups; ++group) {
                Vector3 dir = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0).normalized;
                Vector3 area = hqPos + dir * Random.Range(20, 22);

                for (int i = 0; i < currentWave.numPerGroup; ++i) {
                    float offset = Mathf.Min(8.0f, currentWave.numPerGroup * 0.5f);
                    var pos = area + new Vector3(Random.Range(-offset, offset),
                        Random.Range(-offset, offset), 0);
                    game.CreateEnemyUnit(pos, enemyPower);
                }
            }

            lastWaveTicks = game.GetCurrentTick();
            
            Debug.Log("Spawning wave " + currentWaveIndex + ", power: " + enemyPower);

            return currentWave;
        }

        public void TickSpawning() {
            int currentTick = game.GetCurrentTick();

            if (currentWaveIndex == -1) {
                SpawnWave();
                return;
            }

            if (currentTick - lastWaveTicks < currentWave.ticksUntilNext) {
                return;
            }

            SpawnWave();

        }
    }
}