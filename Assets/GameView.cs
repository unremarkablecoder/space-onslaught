using System.Collections;
using System.Collections.Generic;
using System.Threading;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameView : MonoBehaviour {
    public TileView tileViewPrefab;
    public UnitView unitViewSoldierPrefab;
    public UnitView unitViewCrawlerPrefab;
    public BuildingView buildingViewPrefab;
    public GameObject bloodPrefab;
    public GameObject crawlerCorpsePrefab;
    public SoundManager soundManager;

    public GameObject selectionBox;
    public UI ui;
    
    private Game game;
    private Camera cam;

    private TileView[] tileViews;
    private Dictionary<int, UnitView> unitViews = new Dictionary<int, UnitView>();
    private Dictionary<Vector2Int, BuildingView> buildingViews = new Dictionary<Vector2Int, BuildingView>();

    private float accDt;
    private const float tickTime = 1.0f / 60.0f;

    private bool mouseSelecting = false;
    private Vector3 mouseDragStartWorld;
    private bool mousePanning;
    private bool mousePanningMinDrag;
    private Vector3 mouseDragStartScreen;
    private Vector3 camDragStartWorld;

    private BuildingView pendingBuildingView;

    private bool selling = false;
    public GameObject sellingPointer;
    public GameObject cross;

    private bool paused = false;
    
    // Start is called before the first frame update
    void Start() {
        const int size = 64;
        game = new Game(size);
        
        cam = Camera.main;

        tileViews = new TileView[size * size];
        
        UpdateMapTiles();

        pendingBuildingView = Instantiate(buildingViewPrefab, transform);
        pendingBuildingView.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 movement = Vector3.zero;
        float dt = Time.deltaTime;
        const float speed = 20.0f;

        if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.F1)
            || Input.GetKeyUp(KeyCode.Z)) {
            if (paused) {
                Resume();
            }
            else {
                Pause();
            }
        }

        if (paused) {
            ui.UpdateUI(game);
            return;
        }
        
        if (Input.GetKey(KeyCode.A)) {
            movement.x -= speed * dt;
        } else if (Input.GetKey(KeyCode.D)) {
            movement.x += speed * dt;
        }
        if (Input.GetKey(KeyCode.W)) {
            movement.y += speed * dt;
        } else if (Input.GetKey(KeyCode.S)) {
            movement.y -= speed * dt;
        }
        

        var mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        
        /*
        if (Input.GetKeyDown(KeyCode.F)) {
            game.CreatePlayerUnit(mousePos);
        }
        if (Input.GetKey(KeyCode.G)) {
            game.CreateEnemyUnit(mousePos);
        }
        */

        if (Input.GetKey(KeyCode.M)) {
            //game.minerals += 10;
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            SetPendingBuilding(BuildingType.Drill);
        } else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            SetPendingBuilding(BuildingType.Barracks);
        } else if (Input.GetKeyDown(KeyCode.Alpha3)) {
            SetPendingBuilding(BuildingType.Turret);
        } else if (Input.GetKeyDown(KeyCode.Alpha4)) {
            SetPendingBuilding(BuildingType.Wall);
        } else if (Input.GetKeyDown(KeyCode.F)) {
            SetSelling(true);
        }
        
        
        sellingPointer.SetActive(selling);
        cross.SetActive(false);

        if (pendingBuildingView.gameObject.activeSelf) {
            var coord = game.PosToCoord(mousePos);
            pendingBuildingView.SetPendingCoord(coord);
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) {
                if (game.BuildBuilding(pendingBuildingView.buildingType, coord)) {
                    soundManager.Play(soundManager.buildingPlaced);
                }
            }
        }
        else {
            if (selling) {
                sellingPointer.transform.position = mousePos;
                var building = game.GetBuilding(game.PosToCoord(mousePos));
                if (building != null && building.buildingType != BuildingType.HQ) {
                    cross.SetActive(true);
                    cross.transform.position = game.CoordToPos(building.coord);
                }
            }
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) {
                mouseSelecting = true;
                mouseDragStartWorld = mousePos;
            }
        }

        if (Input.GetMouseButtonDown(1)) {
            mousePanning = true;
            mousePanningMinDrag = false;
            mouseDragStartWorld = mousePos;
            mouseDragStartScreen = Input.mousePosition;
            camDragStartWorld = cam.transform.position;
        }

        if (mouseSelecting) {
            Rect rect = new Rect(
                Mathf.Min(mousePos.x, mouseDragStartWorld.x),
                Mathf.Min(mousePos.y, mouseDragStartWorld.y),
                Mathf.Abs(mousePos.x - mouseDragStartWorld.x),
                Mathf.Abs(mousePos.y - mouseDragStartWorld.y));

            
            selectionBox.transform.localPosition = rect.center;
            selectionBox.transform.localScale = new Vector3(rect.width, rect.height, 1);
            if (Input.GetMouseButtonUp(0)) {
                if (rect.size.x < 0.5f && rect.size.y < 0.5f) {
                    if (selling) {
                        game.SellBuilding(mousePos);
                    }
                    else {
                        game.SelectUnitOrBuilding(mousePos);
                    }
                }
                else {
                    game.SelectUnits(rect);    
                }
                mouseSelecting = false;
                
            }
        }
        
        selectionBox.SetActive(mouseSelecting);

        if (mousePanning) {
            Vector3 a = cam.ScreenToWorldPoint(mouseDragStartScreen);
            Vector3 b = cam.ScreenToWorldPoint(Input.mousePosition);

            if (mousePanningMinDrag) {
                cam.transform.position = camDragStartWorld - (b - a);
            }

            bool minDrag = (b - a).sqrMagnitude > 0.5f;
            if (minDrag && !mousePanningMinDrag) {
                mousePanningMinDrag = true;
                mouseDragStartScreen = Input.mousePosition;
            }

        }

        if (Input.GetMouseButtonUp(1)) {
            if (!mousePanningMinDrag) {
                game.IssueRightClickCommand(mousePos);
                if (pendingBuildingView.gameObject.activeSelf) {
                    pendingBuildingView.gameObject.SetActive(false);
                }

                if (selling) {
                    selling = false;
                }
            }
            
            mousePanning = false;
            
        }

        cam.transform.position += movement;

        accDt += dt;
        if (accDt > tickTime * 4) {
            accDt = tickTime * 4;
        }
        while (accDt >= tickTime) {
            Tick();
            accDt -= tickTime;
        }

    }


    void UpdateMapTiles() {
        var tiles = game.GetTiles();
        int mapSize = game.GetMapSize();


        List<Color> colorOffsets = new List<Color> {
            new Color(0.01f, 0.01f, 0.01f, 0),
            new Color(0.01f, 0.0f, 0.01f, 0),
            new Color(0.0f, 0.01f, 0.01f, 0),
            new Color(-0.01f, -0.01f, -0.01f, 0)
        };
        
        for (int y = 0; y < mapSize; ++y) {
            for (int x = 0; x < mapSize; ++x) {
                int index = x + y * mapSize;
                var tileView = tileViews[index];
                if (tileView == null) {
                    tileView = Instantiate(tileViewPrefab, transform);
                }

                
                int colorOffsetIndex = (x ^ y) % colorOffsets.Count;

                //tileView.GetComponent<SpriteRenderer>().color += colorOffsets[colorOffsetIndex]; 
                tileView.SetData(tiles[index]);

                tileViews[index] = tileView;
            }
        }
    }
    
    void Tick() {
        game.Tick();
        UpdateBuildingViews();
        UpdateUnitViews();
        ui.UpdateUI(game);
    }

    UnitView GetUnitView(int id, out bool wasCreated) {
        if (unitViews.ContainsKey(id)) {
            wasCreated = false;
            return unitViews[id];
        }

        var unitView = Instantiate(game.GetUnit(id).unitType == UnitType.Soldier ? unitViewSoldierPrefab : unitViewCrawlerPrefab, transform);
        unitViews[id] = unitView;
        wasCreated = true;
        return unitView;
    }

    void UpdateUnitViews() {
        var units = game.GetUnits();
        foreach (var pair in units) {
            int id = pair.Key;
            var unit = pair.Value;

            var unitView = GetUnitView(id, out bool wasCreated);
            if (wasCreated) {
                unitView.SetInitData(unit);
            }
            bool isSelected = game.IsUnitSelected(id);
            unitView.SetData(unit, isSelected, game.GetCurrentTick());
            if (unit.unitType == UnitType.Crawler && unit.lastDamageTick == game.GetCurrentTick()-1) {
                var blood = Instantiate(bloodPrefab, transform);
                blood.transform.position = unit.pos;
                blood.transform.right = unit.lastDamageDir;
                Destroy(blood, 0.1f);
            }

            if (unit.unitType == UnitType.Soldier && unit.lastAttack == game.GetCurrentTick() - 1) {
                soundManager.Play(soundManager.shoot);
            } else if (unit.unitType == UnitType.Crawler && unit.lastAttack == game.GetCurrentTick() - 1) {
                soundManager.Play(soundManager.spit);
            }
            
        }

        List<int> idsToRemove = new List<int>();
        foreach (var pair in unitViews) {
            var unitView = pair.Value;

            if (game.GetCurrentTick() > unitView.lastUpdateTick) {
                if (unitView.unitType == UnitType.Crawler) {
                    var corpse = Instantiate(crawlerCorpsePrefab, transform);
                    corpse.transform.position = unitView.transform.position;
                    Destroy(corpse, 5.0f);
                }
                idsToRemove.Add(pair.Key);
            } 
        }

        foreach (var id in idsToRemove) {
            Destroy(unitViews[id].gameObject);
            unitViews.Remove(id);
        }
    }
    
    BuildingView GetBuildingView(Vector2Int coord, out bool wasCreated) {
        if (buildingViews.ContainsKey(coord)) {
            wasCreated = false;
            return buildingViews[coord];
        }

        var buildingView = Instantiate(buildingViewPrefab, transform);
        buildingViews[coord] = buildingView;
        wasCreated = true;
        return buildingView;
        
    }
    void UpdateBuildingViews() {
        var buildings = game.GetBuildings();
        var selectedBuilding = game.GetSelectedBuilding();
        foreach (var pair in buildings) {
            Vector2Int coord = pair.Key;
            Building building = pair.Value;

            var buildingView = GetBuildingView(coord, out bool wasCreated);
            if (wasCreated) {
                buildingView.SetInitData(building);
            }
            
            buildingView.SetData(building, selectedBuilding == building, game.GetCurrentTick());

        }
        
        List<Vector2Int> buildingsToRemove = new List<Vector2Int>();
        foreach (var pair in buildingViews) {
            var buildingView = pair.Value;

            if (game.GetCurrentTick() > buildingView.lastUpdateTick) {
                buildingsToRemove.Add(pair.Key);
            } 
        }

        foreach (var coord in buildingsToRemove) {
            soundManager.Play(soundManager.buildingExplode);
            Destroy(buildingViews[coord].gameObject);
            buildingViews.Remove(coord);
        }
        
    }

    public void SetPendingBuilding(BuildingType buildingType) {
        SetSelling(false);
        game.Deselect();
        pendingBuildingView.gameObject.SetActive(true);
        pendingBuildingView.SetPendingBuildingType(buildingType);
    }

    public void SetSelling(bool selling) {
        pendingBuildingView.gameObject.SetActive(false);
        this.selling = selling;
    }

    public void Pause() {
        paused = true;
    }
    public void Resume() {
        paused = false;
    }

    public bool IsPaused() {
        return paused;
    }
}














