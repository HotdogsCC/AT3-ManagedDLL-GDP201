using System;
using System.Globalization;
using UnityEngine;
using TMPro;
using Unity.Entities;

public class UIMonoBehaviour : MonoBehaviour
{
    //reference to the text objects in the ui
    [SerializeField] private TextMeshProUGUI agentsText;
    [SerializeField] private TextMeshProUGUI fpsText;

    //how many agents are currently in the scene
    private static uint agentsInScene = 0;

    // Start is called before the first frame update
    void Start()
    {
        SpawnerSystem.OnSpawnEntity += OnSpawnEntity;
    }

    private void Update()
    {
        //update the UI
        UpdateUI();
    }

    private static void OnSpawnEntity()
    {
        //update the amount agents in the scene
        agentsInScene++;
    }

    private void UpdateUI()
    {
        agentsInScene = UI.agentsSpawned;
        //update agent text
        agentsText.text = "Agents: " + agentsInScene;

        //update fps
        float fps = 1f / Time.deltaTime;
        int roundedFps = Mathf.RoundToInt(fps);
        string fpsString = "Fps " + roundedFps.ToString(CultureInfo.CurrentCulture);
        fpsText.text = fpsString;
    }
}
