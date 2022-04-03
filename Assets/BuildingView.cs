using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class BuildingView : MonoBehaviour {
    public Sprite[] sprites;
    public Sprite[] genSprites;
    
    public SpriteRenderer sprite;
    public SpriteRenderer hpBg;
    public SpriteRenderer hpBar;
    public GameObject selectionLine;

    
    public int lastUpdateTick = 0;
    public BuildingType buildingType;

    public void SetInitData(Building building) {
        sprite.sprite = sprites[(int) building.buildingType];
    }
    
    public void SetData(Building building, bool isSelected, int currentTick) {
        lastUpdateTick = currentTick;

        transform.position = new Vector3(building.coord.x + 0.5f, building.coord.y + 0.5f);

        float hpPercent = building.hp / building.maxHp;

        hpBg.gameObject.SetActive(hpPercent < 0.99f);
        hpBar.transform.localScale = new Vector3(hpPercent, 1, 1);
        hpBar.transform.localPosition = new Vector3(-(1.0f - hpPercent) * 0.5f, 0, 0);

        bool showGen = currentTick - building.lastGenerationTick < 10;
        if (showGen) {
            sprite.sprite = genSprites[(int) building.buildingType];
        }
        else {
            sprite.sprite = sprites[(int) building.buildingType];
        }

        selectionLine.SetActive(isSelected);

    }

    public void SetPendingBuildingType(BuildingType buildingType) {
        this.buildingType = buildingType;
        sprite.sprite = sprites[(int)buildingType];

    }

    public void SetPendingCoord(Vector2Int coord) {
        transform.position = new Vector3(coord.x + 0.5f, coord.y + 0.5f);
    }
}
