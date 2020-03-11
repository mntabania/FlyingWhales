using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptableObjectsManager : MonoBehaviour {
    public static ScriptableObjectsManager Instance;
    
    [Header("Artifacts")]
    public ArtifactDataDictionary artifactDataDictionary;

    [Header("Elemental Damage")]
    public ElementalDamageDataDictionary elementalDamageDataDictionary;

    void Awake() {
        Instance = this;
    }
    public ArtifactData GetArtifactData(ARTIFACT_TYPE type) {
        if (artifactDataDictionary.ContainsKey(type)) {
            return artifactDataDictionary[type];
        }
        return null;
    }
    public ElementalDamageData GetElementalDamageData(ELEMENTAL_TYPE type) {
        if (elementalDamageDataDictionary.ContainsKey(type)) {
            return elementalDamageDataDictionary[type];
        }
        return null;
    }
}
