using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tutorial;
using UnityEngine;

public class WorldConfigManager : MonoBehaviour {

    public static WorldConfigManager Instance;

    [Header("Monster Generation")]
    public MonsterGenerationSetting worldWideMonsterGenerationSetting;
    [Header("Item Generation")] 
    public ItemGenerationSetting worldWideItemGenerationSetting;
    public List<ARTIFACT_TYPE> initialArtifactChoices;

    [Header("Demo")]
    [SerializeField] private bool _isDemoWorld;
    [SerializeField] private List<SPELL_TYPE> _availableSpellsInDemoBuild;
    [SerializeField] private List<TutorialManager.Tutorial> _demoTutorials;
    
    #region Getters
    public bool isDemoWorld => _isDemoWorld;
    public List<SPELL_TYPE> availableSpellsInDemoBuild => _availableSpellsInDemoBuild;
    public List<TutorialManager.Tutorial> demoTutorials => _demoTutorials;
    #endregion
    
    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(Instance.gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }
}