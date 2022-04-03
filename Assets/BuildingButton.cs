using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingButton : MonoBehaviour {
    public TextMeshProUGUI costText;
    public BuildingType buildingType;
    private Button button;

    void Awake() {
        button = GetComponent<Button>();
    }
    
    public void SetData(int minerals) {
        int cost = Building.GetCost(buildingType);
        costText.text = cost.ToString();

        button.interactable = minerals >= cost;

    }
}
