using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class OpinionData {
    public Dictionary<string, int> allOpinions;
    /// <summary>
    /// Getting compatibility value must be gotten from RelationshipManager, DO NOT CALL THIS DIRECTLY!
    /// </summary>
    public int compatibilityValue;

    #region getters
    public int totalOpinion => allOpinions.Sum(x => x.Value);
    #endregion

    public OpinionData() {
        allOpinions = new Dictionary<string, int>();
        compatibilityValue = -1;
    }
    
    public void RandomizeBaseOpinionBasedOnCompatibility() {
        switch (compatibilityValue) {
            case 0:
                SetOpinion("Base", Random.Range(-100, 11));
                break;
            case 1:
                SetOpinion("Base", Random.Range(-50, 21));
                break;
            case 2:
                SetOpinion("Base", Random.Range(-20, 21));
                break;
            case 3:
                SetOpinion("Base", Random.Range(0, 51));
                break;
            case 4:
                SetOpinion("Base", Random.Range(20, 71));
                break;
            case 5:
                SetOpinion("Base", Random.Range(50, 100));
                break;
        }
    }
    
    public void AdjustOpinion(string text, int value) {
        if (allOpinions.ContainsKey(text)) {
            allOpinions[text] += value;
        } else {
            allOpinions.Add(text, value);
        }
    }
    public void SetOpinion(string text, int value) {
        if (allOpinions.ContainsKey(text)) {
            allOpinions[text] = value;
        } else {
            allOpinions.Add(text, value);
        }
    }
    public bool RemoveOpinion(string text) {
        if (allOpinions.ContainsKey(text)) {
            return allOpinions.Remove(text);
        }
        return false;
    }
    public bool HasOpinion(string text) {
        return allOpinions.ContainsKey(text);
    }
    public void SetCompatibilityValue(int value) {
        compatibilityValue = value;
        Assert.IsTrue(compatibilityValue >= RelationshipManager.MinCompatibility 
                      && compatibilityValue <= RelationshipManager.MaxCompatibility, 
            $"Compatibility value exceeds the min/max compatibility. Set Value is {compatibilityValue.ToString()}");
    }

    #region Object Pool
    public void Initialize() { }
    public void Reset() {
        allOpinions.Clear();
        compatibilityValue = 0;
    }
    #endregion
}
