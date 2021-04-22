using System.Collections.Generic;
using Tutorial;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tutorial Data", menuName = "Scriptable Objects/Tutorial Data")]
public class TutorialScriptableObjectData : ScriptableObject {
    public TutorialManager.Tutorial_Type tutorialType;
    public string tutorialNameKey;
    public List<TutorialPage> pages;
}

[System.Serializable]
public class TutorialPage {
    public Sprite imgTutorial;
    public string description;
}
