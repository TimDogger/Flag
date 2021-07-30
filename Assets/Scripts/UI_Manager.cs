using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    [SerializeField]
    private FlagManager flagManager;
    
    [SerializeField]
    private TextMeshProUGUI total;
    
    [SerializeField]
    private TMP_InputField sizeX;
    
    [SerializeField]
    private TMP_InputField sizeY;
    
    [SerializeField]
    private TMP_InputField delay;

    [SerializeField]
    private Toggle isWaving;

    public void OnFlagBuilderInitialized()
    {
        if (!flagManager) flagManager = FindObjectOfType<FlagManager>();
        
        sizeX.text = flagManager.SizeX.ToString(CultureInfo.InvariantCulture);
        sizeY.text = flagManager.SizeY.ToString(CultureInfo.InvariantCulture);
        
        flagManager.OnTotalUpdated += FlagManagerOnOnTotalUpdated;
        FlagManagerOnOnTotalUpdated(flagManager.Total);
    }

    private void FlagManagerOnOnTotalUpdated(int i)
    {
        total.text = i.ToString();
    }

    public void OnSizeXValueChanged(string s)
    {
        if (int.TryParse(s, NumberStyles.Integer, null, out int sizeX))
        {
            flagManager.SizeX = sizeX;
        }
    }
    
    public void OnSizeYValueChanged(string s)
    {
        if (int.TryParse(s, NumberStyles.Integer, null, out int sizeY))
        {
            flagManager.SizeY = sizeY;
        }
    }
}
