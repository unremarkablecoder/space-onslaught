using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitView : MonoBehaviour {
    public SpriteRenderer sprite;
    public GameObject selectionLine;
    public GameObject defaultGo;
    public GameObject attackGo;

    public UnitType unitType;

    public int lastUpdateTick = 0;

    public void SetInitData(Unit unit) {
        //transform.localScale = new Vector3(unit.radius*2, unit.radius*2, 1);
        defaultGo.SetActive(true);
        attackGo.SetActive(false);
        unitType = unit.unitType;
    }
    
    public void SetData(Unit unit, bool selected, int currentTick) {
        transform.localPosition = unit.pos;
        transform.right = unit.dir;
        selectionLine.gameObject.SetActive(selected);
        selectionLine.transform.rotation = Quaternion.identity;
        lastUpdateTick = currentTick;
        if (unit.unitType == UnitType.Soldier) {
            attackGo.SetActive(currentTick - unit.lastAttack < 5);
            defaultGo.SetActive(!attackGo.activeSelf);
        } else if (unit.unitType == UnitType.Crawler) {
            attackGo.SetActive(currentTick - unit.lastAttack < 5);
            defaultGo.SetActive(!attackGo.activeSelf);
        }
    }
}
