using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI : MonoBehaviour {
    public TextMeshProUGUI mineralsText;
    public TextMeshProUGUI supplyText;
    public TextMeshProUGUI killsText;
    
    public GameView gameView;
    public GameObject losePopup;
    public TextMeshProUGUI loseKillsText;
    public GameObject pausePopup;

    public List<BuildingButton> buildingButtons;
    
    public void UpdateUI(Game game) {
        mineralsText.text = game.minerals.ToString();
        supplyText.text = game.GetSupply().ToString() + " / " + game.GetSupplyLimit().ToString();
        killsText.text = game.kills.ToString();

        foreach (var button in buildingButtons) {
            button.SetData(game.minerals);
        }
        
        losePopup.SetActive(!game.hqAlive);
        loseKillsText.text = "KILLS: " + game.kills.ToString();
        
        pausePopup.SetActive(gameView.IsPaused());
    }

    public void OnBuildingButton(int type) {
        gameView.SetPendingBuilding((BuildingType)type);
    }

    public void OnSellButton() {
        gameView.SetSelling(true);
    }

    public void OnRetryButton() {
        SceneManager.LoadScene(1);
    }

    public void OnMainMenuButton() {
        SceneManager.LoadScene(0);
    }

    public void OnResumeButton() {
        gameView.Resume();
    }
}
