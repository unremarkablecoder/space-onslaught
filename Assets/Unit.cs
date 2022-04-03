using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitType {
    Soldier,
    Crawler,
}

public class Unit {
    private int _id;
    public int Id => _id;
    private static int nextId = 1;

    private UnitType _unitType = UnitType.Soldier;
    public UnitType unitType => _unitType;
    
    public Vector3 pos;
    public Vector3 dir;

    public Vector3 targetPos;

    public float radius = 0.25f;
    public float speed = 0.035f;
    public float attackRange = 5.0f;
    public float damage = 1.0f;

    public float hp;
    public float maxHp;

    public int attackInterval = 30;
    public int lastAttack = -999;

    public int targetId = 0;

    public int lastAvoidTurn = 0;
    public int lastDamageTick = -999;
    public Vector3 lastDamageDir;

    public Unit(UnitType unitType, float hp) {
        _id = nextId++;
        this.maxHp = this.hp = hp;
        _unitType = unitType;
    }

    public bool IsPlayerUnit() {
        return unitType == UnitType.Soldier;
    }

}
