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

    [Header("Testing")] 
    [SerializeField] private bool _unlimitedCast;
    [SerializeField] private bool _disableLogs;
    
    #region Getters
    public bool isDemoWorld => _isDemoWorld;
    public List<SPELL_TYPE> availableSpellsInDemoBuild => _availableSpellsInDemoBuild;
    public List<TutorialManager.Tutorial> demoTutorials => _demoTutorials;
#if UNITY_EDITOR
    public bool unlimitedCast => _unlimitedCast;
    public bool disableLogs => _disableLogs;
#else
    public bool unlimitedCast => false;
    public bool disableLogs => true;
#endif
    #endregion
    
    private void Awake() {
        if (disableLogs) {
            Debug.unityLogger.logEnabled = false; 
        }
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(Instance.gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }
}