using UnityEngine;
using TMPro;
using Unity.Entities;

public class UIMonoBehaviour : MonoBehaviour
{
    //reference to the text objects in the ui
    [SerializeField] private TextMeshProUGUI agentsText;
    [SerializeField] private TextMeshProUGUI fpsText;

    //how many agents are currently in the scene
    private uint agentsInScene = 0;

    // Start is called before the first frame update
    void Start()
    {
        SpawnerSystem.OnSpawnEntity += OnSpawnEntity;
    }

    private void OnSpawnEntity()
    {
        agentsInScene++;
        UpdateUI();
    }

    private void UpdateUI()
    {
        //update agent text
        agentsText.text = "Agents: " + agentsInScene;

        //update fps
        float fps = 1000f / Time.deltaTime;
        fpsText.text = "Fps: " + fps.ToString();
    }
}
