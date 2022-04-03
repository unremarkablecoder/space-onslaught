using UnityEngine;

namespace DefaultNamespace {
    public enum BuildingType {
        HQ,
        Drill,
        Barracks,
        Turret,
        Wall,
    }
    
    public class Building {
        public Vector2Int coord;
        public float hp = 100;
        public float maxHp = 100;
        public BuildingType buildingType = BuildingType.HQ;

        public int lastGenerationTick = 0;

        public Vector3 rallyPoint;

        public Building(BuildingType buildingType, Vector2Int coord) {
            this.buildingType = buildingType;
            this.coord = coord;
            hp = maxHp = GetMaxHp(buildingType);
        }

        public static int GetCost(BuildingType buildingType) {
            switch (buildingType) {
                case BuildingType.HQ:
                    return 0;
                case BuildingType.Barracks:
                    return 150;
                case BuildingType.Drill:
                    return 100;
                case BuildingType.Turret:
                    return 200;
                case BuildingType.Wall:
                    return 50;
                default:
                    return 999;
            }
        }
        
        public static float GetMaxHp(BuildingType buildingType) {
            switch (buildingType) {
                case BuildingType.HQ:
                    return 250.0f;
                case BuildingType.Barracks:
                    return 50;
                case BuildingType.Drill:
                    return 35;
                case BuildingType.Turret:
                    return 80;
                case BuildingType.Wall:
                    return 100;
                default:
                    return 999;
            }
        }
        
        public static float GetBuildingHalfSize(BuildingType buildingType) {
            switch (buildingType) {
                case BuildingType.Wall:
                    return 0.24f;
                default:
                    return 0.24f;
            }
        }
    }
}