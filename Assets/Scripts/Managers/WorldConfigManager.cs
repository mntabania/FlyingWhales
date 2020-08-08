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
    [SerializeField] private bool _isDemoBuild = false;
    // [SerializeField] private List<SPELL_TYPE> _availableSpellsInDemoBuild;
    // [SerializeField] private List<TutorialManager.Tutorial> _demoTutorials;

    [Header("Testing")] 
    [SerializeField] private bool _disableLogs;
    public MapGenerationData mapGenerationData;

    private List<SPELL_TYPE> _allSpells;
    
    #region Getters
    public bool isTutorialWorld => WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial;
    public List<SPELL_TYPE> availableSpellsInTutorial => _allSpells; //_availableSpellsInDemoBuild;
#if UNITY_EDITOR
    private bool disableLogs => _disableLogs;
#else
    public bool disableLogs => true;
#endif
    public bool isDemoBuild => _isDemoBuild;
    #endregion
    
    private void Awake() {
        if (disableLogs) {
            Debug.unityLogger.logEnabled = false; 
        }
        if (Instance == null) {
            Instance = this;
            _allSpells = UtilityScripts.CollectionUtilities.GetEnumValues<SPELL_TYPE>().ToList();
        } else if (Instance != this) {
            Destroy(Instance.gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }
}