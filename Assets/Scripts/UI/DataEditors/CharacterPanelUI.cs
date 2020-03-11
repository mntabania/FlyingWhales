using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;

using TMPro;


public class CharacterPanelUI : MonoBehaviour {
    public static CharacterPanelUI Instance;

    public Dropdown classOptions;
    public Dropdown raceOptions;
    public Dropdown genderOptions;
    public Dropdown consumableOptions;
    public Dropdown traitOptions;

    public InputField nameInput;
    public InputField levelInput;
    public InputField armyInput;
    //public InputField dHeadInput;
    //public InputField dBodyInput;
    //public InputField dLegsInput;
    //public InputField dHandsInput;
    //public InputField dFeetInput;

    public TextMeshProUGUI hpLbl;
    public TextMeshProUGUI attackPowerLbl;
    public TextMeshProUGUI speedLbl;
    public TextMeshProUGUI spLbl;
    public TextMeshProUGUI skillsLbl;
    public TextMeshProUGUI combatAttributesLbl;
    public TextMeshProUGUI weaponLbl;
    public TextMeshProUGUI armorLbl;
    public TextMeshProUGUI accessoryLbl;

    public Toggle toggleArmy;

    private int _singleHP;
    private int _sp;
    private int _singleAttackPower;
    private int _singleSpeed;
    private int _attackPower;
    private int _speed;
    private int _hp;
    //private string _skillName;
    //private string _weaponName;
    //private string _armorName;
    //private string _accessoryName;
    private List<string> _allCombatAttributeNames;

    #region getters/setters
    //public string skillName {
    //    get { return _skillName; }
    //}
    //public string weaponName {
    //    get { return _weaponName; }
    //}
    //public string armorName {
    //    get { return _armorName; }
    //}
    //public string accessoryName {
    //    get { return _accessoryName; }
    //}
    public int hp {
        get { return _singleHP; }
    }
    public int sp {
        get { return _sp; }
    }
    public int speed {
        get { return _singleSpeed; }
    }
    public int attackPower {
        get { return _singleAttackPower; }
    }
    public List<string> allCombatAttributeNames {
        get { return _allCombatAttributeNames; }
    }
    #endregion

    void Awake() {
        Instance = this;
    }

    #region Utilities
    public void LoadAllData() {
        _allCombatAttributeNames = new List<string>();
        levelInput.text = "1";
        armyInput.text = "1";
        raceOptions.ClearOptions();
        genderOptions.ClearOptions();
        //combatAttributeOptions.ClearOptions();
        //weaponOptions.ClearOptions();
        //armorOptions.ClearOptions();
        //accessoryOptions.ClearOptions();
        //consumableOptions.ClearOptions();
        string[] genders = System.Enum.GetNames(typeof(GENDER));

        //List<string> combatAttributes = new List<string>();
        //string path = Utilities.dataPath + "CombatAttributes/";
        //foreach (string file in Directory.GetFiles(path, "*.json")) {
        //    combatAttributes.Add(Path.GetFileNameWithoutExtension(file));
        //}
        //List<string> weapons = new List<string>();
        //List<string> armors = new List<string>();
        //string path = Utilities.dataPath + "Items/";
        //string[] directories = Directory.GetDirectories(path);
        //for (int i = 0; i < directories.Length; i++) {
        //    string folderName = new DirectoryInfo(directories[i]).Name;
        //    string[] files = Directory.GetFiles(directories[i], "*.json");
        //    for (int j = 0; j < files.Length; j++) {
        //        string fileName = Path.GetFileNameWithoutExtension(files[j]);
        //        if (folderName == "WEAPON") {
        //            weapons.Add(fileName);
        //        } else if (folderName == "ARMOR") {
        //            armors.Add(fileName);
        //        }
        //    }
        //}

        List<string> races = new List<string>();
        string path3 = $"{UtilityScripts.Utilities.dataPath}RaceSettings/";
        foreach (string file in Directory.GetFiles(path3, "*.json")) {
            races.Add(Path.GetFileNameWithoutExtension(file));
        }

        raceOptions.AddOptions(races);
        //combatAttributeOptions.AddOptions(combatAttributes);
        //weaponOptions.AddOptions(weapons);
        genderOptions.AddOptions(genders.ToList());
        //armorOptions.AddOptions(armors);
        //accessoryOptions.AddOptions(armors);
        //consumableOptions.AddOptions(armors);
    }
    public void UpdateClassOptions() {
        classOptions.ClearOptions();
        classOptions.AddOptions(ClassPanelUI.Instance.allClasses);
    }
    public void UpdateTraitOptions() {
        traitOptions.ClearOptions();
        traitOptions.AddOptions(TraitPanelUI.Instance.allTraits);
    }
    public void UpdateItemOptions() {
        //UpdateWeaponOptions();
        //UpdateArmorOptions();
    }
    //private void UpdateWeaponOptions() {
    //    weaponOptions.ClearOptions();
    //    weaponOptions.AddOptions(ItemPanelUI.Instance.allWeapons);
    //}
    //private void UpdateArmorOptions() {
    //    armorOptions.ClearOptions();
    //    armorOptions.AddOptions(ItemPanelUI.Instance.allArmors);
    //}
    private void ClearData() {
        classOptions.value = 0;
        genderOptions.value = 0;
        //weaponOptions.value = 0;
        //armorOptions.value = 0;
        //accessoryOptions.value = 0;
        //consumableOptions.value = 0;

        nameInput.text = string.Empty;
        levelInput.text = "1";
        armyInput.text = "1";

        attackPowerLbl.text = "0";
        speedLbl.text = "0";
        hpLbl.text = "0";
        spLbl.text = "0";

        skillsLbl.text = "None";
        combatAttributesLbl.text = "None";
        weaponLbl.text = "None";
        armorLbl.text = "None";
        accessoryLbl.text = "None";

        toggleArmy.isOn = false;

        //dHeadInput.text = "0";
        //dBodyInput.text = "0";
        //dLegsInput.text = "0";
        //dHandsInput.text = "0";
        //dFeetInput.text = "0";
    }
    private void SaveCharacter() {
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(nameInput.text)) {
            EditorUtility.DisplayDialog("Error", "Please specify a Character Name", "OK");
            return;
        }
        string path = $"{UtilityScripts.Utilities.dataPath}CharacterSims/{nameInput.text}.json";
        if (UtilityScripts.Utilities.DoesFileExist(path)) {
            if (EditorUtility.DisplayDialog("Overwrite Character",
                $"A character with name {nameInput.text} already exists. Replace with this character?", "Yes", "No")) {
                File.Delete(path);
                SaveCharacterJson(path);
            }
        } else {
            SaveCharacterJson(path);
        }
#endif
    }
    private void SaveCharacterJson(string path) {
//        CharacterSim newCharacter = new CharacterSim();

//        newCharacter.SetDataFromCharacterPanelUI();

//        string jsonString = JsonUtility.ToJson(newCharacter);

//        System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
//        writer.WriteLine(jsonString);
//        writer.Close();

//#if UNITY_EDITOR
//        //Re-import the file to update the reference in the editor
//        UnityEditor.AssetDatabase.ImportAsset(path);
//#endif
//        Debug.Log($"Successfully saved character at {path}");

        //CombatSimManager.Instance.UpdateAllCharacters();
    }
    private void LoadCharacter() {
#if UNITY_EDITOR
        //string filePath = EditorUtility.OpenFilePanel("Select Character",
        //    $"{UtilityScripts.Utilities.dataPath}CharacterSims/", "json");

        //if (!string.IsNullOrEmpty(filePath)) {
        //    string dataAsJson = File.ReadAllText(filePath);

        //    //CharacterSim character = JsonUtility.FromJson<CharacterSim>(dataAsJson);
        //    //ClearData();
        //    //LoadCharacterDataToUI(character);
        //}
#endif
    }

    //private void LoadCharacterDataToUI(CharacterSim character) {
    //    nameInput.text = character.name;
    //    levelInput.text = character.level.ToString();
    //    armyInput.text = character.armyCount.ToString();

    //    classOptions.value = GetDropdownIndex(character.className, classOptions);
    //    genderOptions.value = GetDropdownIndex(character.gender.ToString(), genderOptions);
    //    raceOptions.value = GetDropdownIndex(character.raceName, raceOptions);

    //    toggleArmy.isOn = character.isArmy;
    //    //weaponOptions.value = GetDropdownIndex(character.weaponName, weaponOptions);
    //    //armorOptions.value = GetDropdownIndex(character.armorName, armorOptions);
    //    //accessoryOptions.value = GetDropdownIndex(character.accessoryName, accessoryOptions);
    //    //consumableOptions.value = GetDropdownIndex(character.consumableName, consumableOptions);

    //    //dHeadInput.text = character.defHead.ToString();
    //    //dBodyInput.text = character.defBody.ToString();
    //    //dLegsInput.text = character.defLegs.ToString();
    //    //dHandsInput.text = character.defHands.ToString();
    //    //dFeetInput.text = character.defFeet.ToString();

    //    //_singleHP = character.maxHP;
    //    //_sp = character.maxSP;
    //    //_singleAttackPower = character.attackPower;
    //    //_singleSpeed = character.speed;

    //    //_skillNames.Clear();
    //    //for (int i = 0; i < character.skillNames.Count; i++) {
    //    //    string skillName = character.skillNames[i];
    //    //    _skillNames.Add(skillName);
    //    //}

    //    UpdateUI();
    //}
    private int GetDropdownIndex(string armorName, Dropdown ddOptions) {
        for (int i = 0; i < ddOptions.options.Count; i++) {
            if (ddOptions.options[i].text == armorName) {
                return i;
            }
        }
        return 0;
    }
    private CharacterClass GetClass(string className) {
        string path = $"{UtilityScripts.Utilities.dataPath}CharacterClasses/{className}.json";
        CharacterClass currentClass = JsonUtility.FromJson<CharacterClass>(System.IO.File.ReadAllText(path));
        return currentClass;
    }
    private RaceSetting GetRace(string raceName) {
        string path = $"{UtilityScripts.Utilities.dataPath}RaceSettings/{raceName}.json";
        RaceSetting currentRace = JsonUtility.FromJson<RaceSetting>(System.IO.File.ReadAllText(path));
        return currentRace;
    }
    private void AllocateStats(RaceSetting raceSetting) {
        //_singleAttackPower = raceSetting.attackPowerModifier;
        //_singleSpeed = raceSetting.speedModifier;
        //_singleHP = raceSetting.hpModifier;
    }
    private void LevelUp(int level, CharacterClass characterClass, RaceSetting raceSetting) {
        int multiplier = level - 1;
        if(multiplier < 0) {
            multiplier = 0;
        }
        _singleAttackPower += (multiplier * (int)((characterClass.attackPowerPerLevel / 100f) * (float)raceSetting.attackPowerModifier));
        _singleSpeed += (multiplier * (int) ((characterClass.speedPerLevel / 100f) * (float) raceSetting.speedModifier));
        _singleHP += (multiplier * (int) ((characterClass.hpPerLevel / 100f) * (float) raceSetting.hpModifier));

        ////Add stats per level from race
        //if(level > 1) {
        //    int hpIndex = level % raceSetting.hpPerLevel.Length;
        //    hpIndex = hpIndex == 0 ? raceSetting.hpPerLevel.Length : hpIndex;
        //    int attackIndex = level % raceSetting.attackPerLevel.Length;
        //    attackIndex = attackIndex == 0 ? raceSetting.attackPerLevel.Length : attackIndex;

        //    _singleHP += raceSetting.hpPerLevel[hpIndex - 1];
        //    _singleAttackPower += raceSetting.attackPerLevel[attackIndex - 1];
        //}
    }
    private void ArmyModifier(bool state) {
        if (state) {
            _attackPower = _singleAttackPower * int.Parse(armyInput.text);
            _speed = _singleSpeed * int.Parse(armyInput.text);
            _hp = _singleHP * int.Parse(armyInput.text);
        } else {
            _attackPower = _singleAttackPower;
            _speed = _singleSpeed;
            _hp = _singleHP;
        }
    }
    //private void UpdateSkills(CharacterClass characterClass) {
    //    _skillName = characterClass.skillName;
    //}
    //private void UpdateEquipment(CharacterClass characterClass) {
    //    if(characterClass.weaponTierNames != null && characterClass.weaponTierNames.Count > 0) {
    //        _weaponName = characterClass.weaponTierNames[0];
    //    } else {
    //        _weaponName = string.Empty;
    //    }
    //    if (characterClass.armorTierNames != null && characterClass.armorTierNames.Count > 0) {
    //        _armorName = characterClass.armorTierNames[0];
    //    } else {
    //        _armorName = string.Empty;
    //    }
    //    if (characterClass.accessoryTierNames != null && characterClass.accessoryTierNames.Count > 0) {
    //        _accessoryName = characterClass.accessoryTierNames[0];
    //    } else {
    //        _accessoryName = string.Empty;
    //    }
    //}
    private void UpdateUI() {
        attackPowerLbl.text = _attackPower.ToString();
        speedLbl.text = _speed.ToString();
        hpLbl.text = _hp.ToString();
        spLbl.text = _sp.ToString();
        //weaponLbl.text = _weaponName;
        //armorLbl.text = _armorName;
        //accessoryLbl.text = _accessoryName;

        //if(!string.IsNullOrEmpty(_skillName)) {
        //    skillsLbl.text = _skillName;
        //} else {
        //    skillsLbl.text = "None";
        //}
        UpdateTraitsUI();
    }
    private void UpdateTraitsUI() {
        combatAttributesLbl.text = string.Empty;
        if (_allCombatAttributeNames != null && _allCombatAttributeNames.Count > 0) {
            combatAttributesLbl.text += _allCombatAttributeNames[0];
            for (int i = 1; i < _allCombatAttributeNames.Count; i++) {
                combatAttributesLbl.text += $", {_allCombatAttributeNames[i]}";
            }
        }
        if(string.IsNullOrEmpty(combatAttributesLbl.text)) {
            combatAttributesLbl.text = "None";
        }
    }
    private void ClassChange(int index) {
        CharacterClass characterClass = GetClass(classOptions.options[index].text);
        RaceSetting race = GetRace(raceOptions.options[raceOptions.value].text);
        int level = int.Parse(levelInput.text);
        if (level < 1) {
            level = 1;
            levelInput.text = level.ToString();
        } else if (level > 100) {
            level = 100;
            levelInput.text = level.ToString();
        }
        AllocateStats(race);
        LevelUp(level, characterClass, race);
        ArmyModifier(toggleArmy.isOn);
        //UpdateSkills(characterClass);
        //UpdateEquipment(characterClass);
        UpdateUI();
    }
    private void RaceChange(int index) {
        CharacterClass characterClass = GetClass(classOptions.options[classOptions.value].text);
        RaceSetting race = GetRace(raceOptions.options[index].text);
        int level = int.Parse(levelInput.text);
        if (level < 1) {
            level = 1;
            levelInput.text = level.ToString();
        } else if (level > 100) {
            level = 100;
            levelInput.text = level.ToString();
        }
        AllocateStats(race);
        LevelUp(level, characterClass, race);
        ArmyModifier(toggleArmy.isOn);
        //UpdateSkills(characterClass);
        //UpdateEquipment(characterClass);
        UpdateUI();
    }
    #endregion

    #region OnValueChanged
    public void OnClassChange(int index) {
        ClassChange(index);
    }
    public void OnRaceChange(int index) {
        RaceChange(index);
    }
    public void OnToggleArmy(bool state) {
        armyInput.gameObject.SetActive(state);
        ArmyModifier(state);
    }
    #endregion

    #region Button Clicks
    public void OnClickAddNewCharacter() {
        ClearData();
    }
    public void OnClickEditCharacter() {
        LoadCharacter();
    }
    public void OnClickSaveCharacter() {
        SaveCharacter();
    }
    public void OnClickApply() {
        ClassChange(classOptions.value);
    }
    public void OnAddCombatAttribute() {
        if (traitOptions.options.Count > 0 && !_allCombatAttributeNames.Contains(traitOptions.options[traitOptions.value].text)) {
            _allCombatAttributeNames.Add(traitOptions.options[traitOptions.value].text);
        }
        UpdateTraitsUI();
    }
    public void OnRemoveCombatAttribute() {
        if (traitOptions.options.Count > 0 && _allCombatAttributeNames.Remove(traitOptions.options[traitOptions.value].text)) {
            UpdateTraitsUI();
        }
    }
    #endregion
}
