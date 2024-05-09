using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlannedMode : MonoBehaviour
{

    public Button modeButton;
    public Button secondButton;
    public bool randomMode;
    void Start()
    {
        modeButton.onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick()
    {
        GameManager.instance.currentBoss.randomRackAdvancement = randomMode;
        GameManager.instance.currentState = GameManager.GameState.GAMEPLAY;
        this.gameObject.SetActive(false);
        secondButton.gameObject.SetActive(false);
    }
}
