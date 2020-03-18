using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldConfigManager : MonoBehaviour {

    public static WorldConfigManager Instance;

    [Header("Monster Generation")]
    public MonsterGenerationSetting worldWideMonsterGenerationSetting;
    [Header("Item Generation")] 
    public ItemGenerationSetting worldWideItemGenerationSetting;
    public List<ARTIFACT_TYPE> initialArtifactChoices;
    
    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(Instance.gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }
}