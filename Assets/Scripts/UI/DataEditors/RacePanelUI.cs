using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.UI;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class RacePanelUI : MonoBehaviour {
    public static RacePanelUI Instance;

    public Dropdown raceOptions;
    public Dropdown traitOptions;

    public InputField attackModifierInput;
    public InputField speedModifierInput;
    public InputField hpModifierInput;
    public InputField hpPerLevelInput;
    public InputField attackPerLevelInput;
    public InputField neutralSpawnLevelModInput;
    public InputField runSpeedInput;
    public InputField walkSpeedInput;


    public GameObject traitsGO;
    public GameObject hpPerLevelGO;
    public GameObject attackPerLevelGO;
    public GameObject raceStringButtonPrefab;

    public ScrollRect traitsScrollRect;
    public ScrollRect hpPerLevelScrollRect;
    public ScrollRect attackPerLevelScrollRect;

    [NonSerialized] public RaceStringButton currentSelectedTraitButton;
    [NonSerialized] public RaceStringButton currentSelectedHPPerLevelButton;
    [NonSerialized] public RaceStringButton currentSelectedAttackPerLevelButton;

    private List<string> _traitNames;
    private List<int> _hpPerLevel;
    private List<int> _attackPerLevel;

    #region getters/setters
    public List<string> traitNames {
        get { return _traitNames; }
    }
    public List<int> hpPerLevel {
        get { return _hpPerLevel; }
    }
    public List<int> attackPerLevel {
        get { return _attackPerLevel; }
    }
    #endregion

    void Awake() {
        Instance = this;   
    }

    #region Utilities
    public void UpdateTraitOptions() {
        traitOptions.ClearOptions();
        traitOptions.AddOptions(TraitPanelUI.Instance.allTraits);
        traitOptions.AddOptions(TraitManager.instancedTraits.ToList());
    }
    public void LoadAllData() {
        _traitNames = new List<string>();
        _hpPerLevel = new List<int>();
        _attackPerLevel = new List<int>();

        attackModifierInput.text = "0";
        speedModifierInput.text = "0";
        hpModifierInput.text = "0";
        hpPerLevelInput.text = "0";
        attackPerLevelInput.text = "0";
        neutralSpawnLevelModInput.text = "0";
        runSpeedInput.text = "0";
        walkSpeedInput.text = "0";

        raceOptions.ClearOptions();

        string[] races = System.Enum.GetNames(typeof(RACE));

        raceOptions.AddOptions(races.ToList());
    }
    private void ClearData() {
        currentSelectedTraitButton = null;
        currentSelectedHPPerLevelButton = null;
        currentSelectedAttackPerLevelButton = null;

        attackModifierInput.text = "0";
        speedModifierInput.text = "0";
        hpModifierInput.text = "0";
        hpPerLevelInput.text = "0";
        attackPerLevelInput.text = "0";
        neutralSpawnLevelModInput.text = "0";
        runSpeedInput.text = "0";
        walkSpeedInput.text = "0";

        raceOptions.value = 0;
        traitOptions.value = 0;

        _traitNames.Clear();
        _hpPerLevel.Clear();
        _attackPerLevel.Clear();
        UtilityScripts.Utilities.DestroyChildren(traitsScrollRect.content);
        UtilityScripts.Utilities.DestroyChildren(hpPerLevelScrollRect.content);
        UtilityScripts.Utilities.DestroyChildren(attackPerLevelScrollRect.content);
    }
    private void SaveRace() {
        if (raceOptions.value == 0) {
#if UNTIY_EDITOR
            EditorUtility.DisplayDialog("Error", "Please specify a Class Name", "OK");
            return;
#endif
        }
        string path =
            $"{UtilityScripts.Utilities.dataPath}RaceSettings/{raceOptions.options[raceOptions.value].text}.json";
        if (UtilityScripts.Utilities.DoesFileExist(path)) {
#if UNITY_EDITOR
            if (EditorUtility.DisplayDialog("Overwrite Race",
                $"A race with name {raceOptions.options[raceOptions.value].text} already exists. Replace with this race?", "Yes", "No")) {
                File.Delete(path);
                SaveRaceJson(path);
            }
#endif
        } else {
            SaveRaceJson(path);
        }
    }
    private void SaveRaceJson(string path) {
        RaceSetting newRace = new RaceSetting();

        newRace.SetDataFromRacePanelUI();

        string jsonString = JsonUtility.ToJson(newRace);

        System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
        writer.WriteLine(jsonString);
        writer.Close();

#if UNITY_EDITOR
        //Re-import the file to update the reference in the editor
        UnityEditor.AssetDatabase.ImportAsset(path);
#endif
        Debug.Log($"Successfully saved race at {path}");
    }

    private void LoadRace() {
#if UNITY_EDITOR
        string filePath = EditorUtility.OpenFilePanel("Select Race",
            $"{UtilityScripts.Utilities.dataPath}RaceSettings/", "json");

        if (!string.IsNullOrEmpty(filePath)) {
            string dataAsJson = File.ReadAllText(filePath);

            RaceSetting raceSetting = JsonUtility.FromJson<RaceSetting>(dataAsJson);
            ClearData();
            LoadRaceDataToUI(raceSetting);
        }
#endif
    }
    private void LoadRaceDataToUI(RaceSetting raceSetting) {
        raceOptions.value = GetDropdownIndex(raceOptions, raceSetting.race.ToString());
        //attackModifierInput.text = raceSetting.attackPowerModifier.ToString();
        //speedModifierInput.text = raceSetting.speedModifier.ToString();
        runSpeedInput.text = raceSetting.runSpeed.ToString();
        walkSpeedInput.text = raceSetting.walkSpeed.ToString();
        //hpModifierInput.text = raceSetting.hpModifier.ToString();
        //neutralSpawnLevelModInput.text = raceSetting.neutralSpawnLevelModifier.ToString();

        for (int i = 0; i < raceSetting.traitNames.Length; i++) {
            string traitName = raceSetting.traitNames[i];
            _traitNames.Add(traitName);
            GameObject go = GameObject.Instantiate(raceStringButtonPrefab, traitsScrollRect.content);
            go.GetComponent<RaceStringButton>().SetText(traitName, "trait");
        }
        //for (int i = 0; i < raceSetting.hpPerLevel.Length; i++) {
        //    int hpPerLevel = raceSetting.hpPerLevel[i];
        //    _hpPerLevel.Add(hpPerLevel);
        //    GameObject go = GameObject.Instantiate(raceStringButtonPrefab, hpPerLevelScrollRect.content);
        //    go.GetComponent<RaceStringButton>().SetText(hpPerLevel.ToString(), "hpperlevel");
        //}
        //for (int i = 0; i < raceSetting.attackPerLevel.Length; i++) {
        //    int attackPerLevel = raceSetting.attackPerLevel[i];
        //    _attackPerLevel.Add(attackPerLevel);
        //    GameObject go = GameObject.Instantiate(raceStringButtonPrefab, attackPerLevelScrollRect.content);
        //    go.GetComponent<RaceStringButton>().SetText(attackPerLevel.ToString(), "attackperlevel");
        //}
    }
    private int GetDropdownIndex(Dropdown options, string name) {
        for (int i = 0; i < options.options.Count; i++) {
            if (options.options[i].text == name) {
                return i;
            }
        }
        return 0;
    }
    #endregion

    #region Button Clicks
    public void OnAddNewRace() {
        ClearData();
    }
    public void OnSaveRace() {
        SaveRace();
    }
    public void OnEditRace() {
        LoadRace();
    }
    public void OnAddTrait() {
        string traitToAdd = traitOptions.options[traitOptions.value].text;
        if (!_traitNames.Contains(traitToAdd)) {
            _traitNames.Add(traitToAdd);
            GameObject go = GameObject.Instantiate(raceStringButtonPrefab, traitsScrollRect.content);
            go.GetComponent<RaceStringButton>().SetText(traitToAdd, "trait");
        }
    }
    public void OnRemoveTrait() {
        if (currentSelectedTraitButton != null) {
            string traitToRemove = currentSelectedTraitButton.buttonText.text;
            if (_traitNames.Remove(traitToRemove)) {
                GameObject.Destroy(currentSelectedTraitButton.gameObject);
                currentSelectedTraitButton = null;
            }
        }
    }
    public void OnAddHPPerLevel() {
        string hpToAdd = hpPerLevelInput.text;
        _hpPerLevel.Add(int.Parse(hpToAdd));
        GameObject go = GameObject.Instantiate(raceStringButtonPrefab, hpPerLevelScrollRect.content);
        go.GetComponent<RaceStringButton>().SetText(hpToAdd, "hpperlevel");
    }
    public void OnRemoveHPPerLevel() {
        if (currentSelectedHPPerLevelButton != null) {
            int index = currentSelectedHPPerLevelButton.gameObject.transform.GetSiblingIndex();
            _hpPerLevel.RemoveAt(index);
            GameObject.Destroy(currentSelectedHPPerLevelButton.gameObject);
            currentSelectedHPPerLevelButton = null;
        }
    }
    public void OnAddAttackPerLevel() {
        string attackToAdd = attackPerLevelInput.text;
        _attackPerLevel.Add(int.Parse(attackToAdd));
        GameObject go = GameObject.Instantiate(raceStringButtonPrefab, attackPerLevelScrollRect.content);
        go.GetComponent<RaceStringButton>().SetText(attackToAdd, "attackperlevel");
    }
    public void OnRemoveAttackPerLevel() {
        if (currentSelectedAttackPerLevelButton != null) {
            int index = currentSelectedAttackPerLevelButton.gameObject.transform.GetSiblingIndex();
            _attackPerLevel.RemoveAt(index);
            GameObject.Destroy(currentSelectedAttackPerLevelButton.gameObject);
            currentSelectedAttackPerLevelButton = null;
        }
    }
    public void OnClickTraitsTab() {
        traitsGO.SetActive(true);
        hpPerLevelGO.SetActive(false);
        attackPerLevelGO.SetActive(false);
    }
    public void OnClickHPPerLevelTab() {
        traitsGO.SetActive(false);
        hpPerLevelGO.SetActive(true);
        attackPerLevelGO.SetActive(false);
    }
    public void OnClickAttackPerLevelTab() {
        traitsGO.SetActive(false);
        hpPerLevelGO.SetActive(false);
        attackPerLevelGO.SetActive(true);
    }
    #endregion
}
