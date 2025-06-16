using System;
using System.Globalization;
using UnityEngine;
using TMPro;
using Unity.Entities;
using MyDLL;

public class UIMonoBehaviour : MonoBehaviour
{
    //reference to the text objects in the ui
    [SerializeField] private TextMeshProUGUI fpsText;
    
    private void Update()
    {
        //update the UI
        UpdateUI();
    }
    

    private void UpdateUI()
    {
        //update fps
        float fps = 1f / Time.deltaTime;
        int roundedFps = Mathf.RoundToInt(fps);
        string fpsString = "Fps " + roundedFps.ToString(CultureInfo.CurrentCulture);
        fpsText.text = fpsString;
    }
}
