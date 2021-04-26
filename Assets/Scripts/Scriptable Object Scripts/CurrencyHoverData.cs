using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "Currency Hover Data", menuName = "Scriptable Objects/Currencies/Currency Hover Data")]
public class CurrencyHoverData : ScriptableObject {
    [Header("Chaotic Energy")]
    public int minAmountHover;
    public int maxAmountHover;

    [Header("Max Chaotic Per Portal Level")]
    public List<int> maxChaoticPerValues = new List<int>();

    [Header("Portal")] 
    public Cost releaseAbilitiesRerollCost;
    private void OnValidate() {
        releaseAbilitiesRerollCost.name = releaseAbilitiesRerollCost.ToString();
    }
}