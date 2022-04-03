using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class Game {
    private Tile[] tiles;
    private int mapSize;

    private int currentTick = 0;

    public int minerals = 100;

    private Dictionary<int, Unit> units = new Dictionary<int, Unit>();

    private HashSet<int> selectedUnitIds = new HashSet<int>();
    private Dictionary<Vector2Int, Building> buildings = new Dictionary<Vector2Int, Building>();
    private Vector2Int selectedBuildingCoord = new Vector2Int(-1, -1);

    private List<int>[] unitSectors;
    private int sectorSize = 4;
    private int sectorsPerRow;

    private int spawnIncreaseInterval = 400;
    private int spawnLevel = 0;
    private int spawnInterval = 60;
    private int spawnAmount = 1;

    private int supplyLimit = 2;
    private int supply = 2;

    public bool hqAlive = true;

    public int kills = 0;

    private EnemyWaves waves;

    public Game(int size) {
        waves = new EnemyWaves(this);
        mapSize = size;
        tiles = new Tile[size * size];
        for (int y = 0; y < mapSize; ++y) {
            for (int x = 0; x < mapSize; ++x) {
                var tile = new Tile();
                tile.coord = new Vector2Int(x, y);
                tiles[x + y * mapSize] = tile;
            }
        }

        sectorsPerRow = mapSize / sectorSize;
        unitSectors = new List<int>[sectorsPerRow * sectorsPerRow];
        for (int i = 0; i < sectorsPerRow * sectorsPerRow; ++i) {
            unitSectors[i] = new List<int>();
        }

        var centerCoord = new Vector2Int(size / 2, size / 2);
        BuildBuilding(BuildingType.HQ, centerCoord);
        var centerPos = CoordToPos(centerCoord);
        CreatePlayerUnit(centerPos + new Vector3(-2,0,0));
        CreatePlayerUnit(centerPos + new Vector3(2, 0,0 ));
    }

    public int GetCurrentTick() {
        return currentTick;
    }

    public Tile[] GetTiles() {
        return tiles;
    }

    public int GetMapSize() {
        return mapSize;
    }

    public PlayerUnit CreatePlayerUnit(Vector3 pos) {
        var unit = new PlayerUnit();
        unit.pos = pos;
        unit.targetPos = pos;
        units[unit.Id] = unit;
        GetSector(pos)?.Add(unit.Id);
        TryToMoveUnit(unit, pos);
        return unit;
    }

    public EnemyUnit CreateEnemyUnit(Vector3 pos, float power) {
        var unit = new EnemyUnit(power);
        unit.pos = pos;
        unit.targetPos = pos;
        units[unit.Id] = unit;
        GetSector(pos)?.Add(unit.Id);
        TryToMoveUnit(unit, pos);
        return unit;
    }

    public Dictionary<int, Unit> GetUnits() {
        return units;
    }

    public Unit GetUnit(int id) {
        if (units.TryGetValue(id, out var unit)) {
            return unit;
        }

        return null;
    }

    public void Tick() {
        if (!hqAlive) {
            return;
        }
        TickBuildings();
        TickUnits();
        TickSpawning();
        ++currentTick;
    }

    private void TickBuildings() {
        HashSet<Vector2Int> coordsToRemove = new HashSet<Vector2Int>();

        bool newHqAlive = false;
        int newSupplyLimit = 0;
        foreach (var pair in buildings) {
            var coord = pair.Key;
            var building = pair.Value;

            if (building.hp <= 0) {
                coordsToRemove.Add(building.coord);
            }

            var buildingPos = CoordToPos(building.coord);

            switch (building.buildingType) {
                case BuildingType.HQ:
                    newHqAlive = true;
                    newSupplyLimit += 2;
                    if (currentTick - building.lastGenerationTick > 60) {
                        minerals += 2;
                        building.lastGenerationTick = currentTick;
                    }

                    break;
                case BuildingType.Drill:
                    if (currentTick - building.lastGenerationTick > 60) {
                        minerals += 7;
                        building.lastGenerationTick = currentTick;
                    }

                    break;
                case BuildingType.Barracks:
                    newSupplyLimit += 3;
                    if (currentTick - building.lastGenerationTick > 300 && supply < supplyLimit) {
                        var pos = building.rallyPoint + new Vector3(Random.Range(-0.1f, 0.1f),
                            Random.Range(-0.1f, 0.1f), 0);
                        var spawnPos = CoordToPos(building.coord);
                        spawnPos += (pos - spawnPos).normalized;

                        var unit = CreatePlayerUnit(spawnPos);
                        unit.targetPos = pos;
                        ++supply;

                        building.lastGenerationTick = currentTick;
                    }

                    break;
                case BuildingType.Turret: {
                    if (currentTick - building.lastGenerationTick >= 30) {
                        int enemyId = GetClosestEnemyUnitId(buildingPos, 8.0f);
                        if (enemyId > 0) {
                            var enemy = GetUnit(enemyId);
                            if (enemy != null) {
                                building.lastGenerationTick = currentTick;
                                Vector3 toEnemyDir = (enemy.pos - buildingPos).normalized;
                                enemy.lastDamageDir = toEnemyDir;
                                if (DamageUnit(enemy, 1.6f)) { }
                            }

                        }
                    }
                }
                    break;
                default:
                    break;
            }
        }
        supplyLimit = newSupplyLimit;
        hqAlive = newHqAlive;

        foreach (var coord in coordsToRemove) {
            RemoveBuilding(buildings[coord]);
        }
    }

    private void TickUnits() {
        bool checkForAttack = currentTick % 4 == 0;
        supply = 0;

        HashSet<int> idsToRemove = new HashSet<int>();
        foreach (var pair in units) {
            int unitId = pair.Key;
            var unit = pair.Value;
            bool isPlayerUnit = unit.IsPlayerUnit();

            if (isPlayerUnit) {
                supply++;
            }

            if (unit.hp <= 0) {
                idsToRemove.Add(unit.Id);
                continue;
            }

            //player unit try to attack
            if (isPlayerUnit && checkForAttack && (currentTick - unit.lastAttack) >= unit.attackInterval) {
                int closestEnemyId = GetClosestEnemyUnitId(unit.pos, unit.attackRange);
                if (closestEnemyId > 0) {
                    //stop and attack

                    var enemy = GetUnit(closestEnemyId);
                    unit.targetId = enemy.Id;
                    Vector3 toEnemyDir = (enemy.pos - unit.pos).normalized;
                    unit.dir = toEnemyDir;
                    enemy.lastDamageDir = toEnemyDir;
                    if (DamageUnit(enemy, unit.damage)) {
                        unit.targetId = 0;
                        idsToRemove.Add(enemy.Id);
                    }

                    unit.lastAttack = currentTick;

                }
            }

            //enmey unit try to attack
            if (!isPlayerUnit && checkForAttack && (currentTick - unit.lastAttack) >= unit.attackInterval) {
                Vector2Int closestBuildingCoord = GetClosestBuilding(unit.pos);
                Vector3 toBuilding = CoordToPos(closestBuildingCoord) - unit.pos;
                bool attackingUnit = false;
                int closestPlayerId = GetClosestPlayerUnitId(unit.pos, 20.0f);
                if (closestPlayerId > 0) {
                    var targetUnit = GetUnit(closestPlayerId);
                    unit.targetId = targetUnit.Id;
                    unit.targetPos = targetUnit.pos;
                    Vector3 toAttackTarget = (targetUnit.pos - unit.pos);
                    if (toAttackTarget.sqrMagnitude < toBuilding.sqrMagnitude) {
                        attackingUnit = true;
                        Vector3 toTargetDir = toAttackTarget.normalized;
                        unit.dir = toTargetDir;
                        if (toAttackTarget.sqrMagnitude < unit.attackRange * unit.attackRange) {
                            if (DamageUnit(targetUnit, unit.damage)) {
                                unit.targetId = 0;
                                idsToRemove.Add(targetUnit.Id);
                            }

                            unit.lastAttack = currentTick;
                        }
                    }
                }

                if (!attackingUnit) {
                    unit.targetPos = CoordToPos(closestBuildingCoord);
                    var building = GetBuilding(closestBuildingCoord);
                    if (building != null && building.hp > 0) {
                        Vector3 toAttackTarget = (unit.targetPos - unit.pos);
                        Vector3 toTargetDir = toAttackTarget.normalized;
                        unit.dir = toTargetDir;
                        float range = unit.attackRange + 0.6f;
                        if (toAttackTarget.sqrMagnitude < range * range) {
                            if (DamageBuilding(building, unit.damage)) { }

                            unit.lastAttack = currentTick;
                        }

                    }
                }
            }


            Vector3 toTarget = unit.targetPos - unit.pos;
            if (toTarget.sqrMagnitude > 0.05f && (currentTick - unit.lastAttack) >= 20) {
                unit.dir = toTarget.normalized;
                Vector3 newPos = unit.pos + unit.dir * unit.speed;
                TryToMoveUnit(unit, newPos);

            }
        }

        foreach (var id in idsToRemove) {
            RemoveUnit(units[id]);
        }
    }

    private bool TryToMoveUnit(Unit unit, Vector3 pos) {
        //check against buildings
        var building = GetBuilding(PosToCoord(pos));    //need to check more neighboring buildings
        if (building != null) {
            var buildingPos = CoordToPos(building.coord);
            float buildingHalf = 0.5f;
            if (unit.IsPlayerUnit()) {
                buildingHalf = Building.GetBuildingHalfSize(building.buildingType);
            }
            buildingHalf += unit.radius;
            Rect rect = new Rect(buildingPos.x - buildingHalf, buildingPos.y - buildingHalf,
                buildingHalf * 2, buildingHalf * 2);
            if (rect.Contains(pos)) {
                Vector3 toBuildingCenter = buildingPos - pos;
                if (Mathf.Abs(toBuildingCenter.x) > Mathf.Abs(toBuildingCenter.y)) {
                    pos.x = buildingPos.x - buildingHalf * Mathf.Sign(toBuildingCenter.x);
                }
                else {
                    pos.y = buildingPos.y - buildingHalf * Mathf.Sign(toBuildingCenter.y);
                }
            }
            
        }

        //check against all units in this sector and possible next sector
        var fromSector = GetSector(unit.pos);
        var toSector = GetSector(pos);

        foreach (var otherId in fromSector) {
            if (otherId == unit.Id) {
                continue;
            }

            var other = GetUnit(otherId);
            if (other == null) {
                continue;
            }

            Vector3 dist = other.pos - pos;
            float totalRadius = unit.radius + other.radius;
            if (dist.sqrMagnitude < totalRadius * totalRadius) {
                pos = other.pos - dist.normalized * totalRadius;
            }
        }

        if (fromSector != toSector) {
            foreach (var otherId in toSector) {
                if (otherId == unit.Id) {
                    continue;
                }

                var other = GetUnit(otherId);
                if (other == null) {
                    continue;
                }

                Vector3 dist = other.pos - pos;
                float totalRadius = unit.radius + other.radius;
                if (dist.sqrMagnitude < totalRadius * totalRadius) {
                    pos = other.pos - dist.normalized * totalRadius;
                }
            }

            fromSector.Remove(unit.Id);
            toSector.Add(unit.Id);
        }

        unit.pos = pos;
        return true;
    }

    private int GetClosestEnemyUnitId(Vector3 pos, float minRange) {
        float closestSq = 999999f;
        int closestId = 0;
        float minRangeSq = minRange * minRange;

        foreach (var pair in units) {
            var unit = pair.Value;
            if (unit.IsPlayerUnit()) {
                continue;
            }

            if (unit.hp <= 0) {
                continue;
            }

            Vector3 diff = pos - unit.pos;
            float sqDist = diff.sqrMagnitude;
            if (sqDist < closestSq && sqDist < minRangeSq) {
                closestSq = sqDist;
                closestId = unit.Id;
            }
        }

        return closestId;
    }

    private Vector2Int GetClosestBuilding(Vector3 pos) {
        float closestSq = 999999f;
        Vector2Int closestCoord = new Vector2Int(mapSize / 2, mapSize / 2);

        foreach (var pair in buildings) {
            var building = pair.Value;

            if (building.hp <= 0) {
                continue;
            }

            Vector3 diff = pos - new Vector3(building.coord.x, building.coord.y);
            float sqDist = diff.sqrMagnitude;
            if (sqDist < closestSq) {
                closestSq = sqDist;
                closestCoord = building.coord;
            }
        }

        return closestCoord;
    }

    private int GetClosestPlayerUnitId(Vector3 pos, float minRange) {
        float closestSq = 999999f;
        int closestId = 0;
        float minRangeSq = minRange * minRange;

        foreach (var pair in units) {
            var unit = pair.Value;
            if (!unit.IsPlayerUnit()) {
                continue;
            }

            if (unit.hp <= 0) {
                continue;
            }

            Vector3 diff = pos - unit.pos;
            float sqDist = diff.sqrMagnitude;
            if (sqDist < closestSq && sqDist < minRangeSq) {
                closestSq = sqDist;
                closestId = unit.Id;
            }
        }

        return closestId;
    }

    public void SelectUnitOrBuilding(Vector3 pos) {
        Deselect();

        foreach (var pair in units) {
            var unit = pair.Value;
            if (!unit.IsPlayerUnit()) {
                continue;
            }

            Vector3 diff = pos - unit.pos;
            if (diff.sqrMagnitude > unit.radius * unit.radius) {
                continue;
            }

            selectedUnitIds.Add(unit.Id);
            return;
        }

        //select building
        var coord = PosToCoord(pos);
        var building = GetBuilding(coord);
        if (building == null) {
            return;
        }

        selectedBuildingCoord = coord;
    }

    public void SellBuilding(Vector3 pos) {
        //select building
        var coord = PosToCoord(pos);
        var building = GetBuilding(coord);
        if (building == null || building.buildingType == BuildingType.HQ) {
            return;
        }

        minerals += Mathf.CeilToInt((building.hp / building.maxHp) * Building.GetCost(building.buildingType) * 0.75f);
        RemoveBuilding(building);
    }

    public void Deselect() {
        selectedBuildingCoord = new Vector2Int(-1, -1);
        selectedUnitIds.Clear();
    }
    public void SelectUnits(Rect rect) {
        Deselect();

        foreach (var pair in units) {
            var unit = pair.Value;
            if (!unit.IsPlayerUnit()) {
                continue;
            }

            if (!rect.Contains(unit.pos)) {
                continue;
            }

            selectedUnitIds.Add(unit.Id);
        }
    }

    public bool IsUnitSelected(int id) {
        return selectedUnitIds.Contains(id);
    }

    public void IssueRightClickCommand(Vector3 pos) {
        foreach (var id in selectedUnitIds) {
            var unit = GetUnit(id);
            unit.targetPos = pos;
        }

        if (selectedBuildingCoord.x != -1) {
            var building = GetBuilding(selectedBuildingCoord);
            if (building != null) {
                building.rallyPoint = pos;
            }
        }

    }

    //returns true if died
    public bool DamageUnit(Unit unit, float damage) {
        unit.hp -= damage;
        unit.lastDamageTick = currentTick;
        if (unit.hp <= 0.0f) {
            if (unit.unitType == UnitType.Crawler) {
                ++kills;
            }
            return true;
        }

        return false;
    }

    public void RemoveUnit(Unit unit) {
        GetSector(unit.pos).Remove(unit.Id);
        selectedUnitIds.Remove(unit.Id);
        units.Remove(unit.Id);

    }

    public bool BuildBuilding(BuildingType buildingType, Vector2Int coord) {
        if (buildings.ContainsKey(coord)) {
            return false;
        }

        int cost = Building.GetCost(buildingType);
        if (minerals < cost) {
            return false;
        }

        minerals -= cost;

        var building = new Building(buildingType, coord);

        building.rallyPoint = CoordToPos(coord) + new Vector3(1.0f, 0, 0);
        buildings[coord] = building;
        return true;
    }

    public Dictionary<Vector2Int, Building> GetBuildings() {
        return buildings;
    }

    //returns true if died
    public bool DamageBuilding(Building building, float damage) {
        building.hp -= damage;
        if (building.hp <= 0.0f) {
            return true;
        }

        return false;
    }

    public void RemoveBuilding(Building building) {
        buildings.Remove(building.coord);
    }

    public Vector2Int PosToCoord(Vector3 pos) {
        return new Vector2Int((int) pos.x, (int) pos.y);
    }

    public Vector3 CoordToPos(Vector2Int coord) {
        return new Vector3(coord.x + 0.5f, coord.y + 0.5f);
    }

    public Building GetBuilding(Vector2Int coord) {
        if (buildings.TryGetValue(coord, out var building)) {
            return building;
        }

        return null;
    }

    private List<int> GetSector(Vector3 pos) {
        var coord = PosToCoord(pos);
        if (coord.x < 0 || coord.y < 0 ||
            coord.x >= mapSize || coord.y >= mapSize) {
            return null;
        }
        int x = coord.x / sectorSize;
        int y = coord.y / sectorSize;

        return unitSectors[x + y * sectorsPerRow];
    }

    private void TickSpawning() {
        waves.TickSpawning();
    }

    public int GetSupplyLimit() {
        return supplyLimit;
    }

    public int GetSupply() {
        return supply;
    }

    public Building GetSelectedBuilding() {
        if (selectedBuildingCoord.x < 0) {
            return null;
        }

        return GetBuilding(selectedBuildingCoord);
    }
}
