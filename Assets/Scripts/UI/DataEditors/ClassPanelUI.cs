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


public class ClassPanelUI : MonoBehaviour {
    public static ClassPanelUI Instance;

    public InputField classNameInput;
    public InputField identifierInput;
    public InputField tamedTraitInput;
    public InputField baseAttackPowerInput;
    //public InputField attackPowerPerLevelInput;
    public InputField baseSpeedInput;
    //public InputField speedPerLevelInput;
    public InputField baseHPInput;
    //public InputField hpPerLevelInput;
    //public InputField baseSPInput;
    //public InputField spPerLevelInput;
    //public InputField recruitmentCostInput;
    public InputField baseAttackSpeedInput;
    public InputField attackRangeInput;
    public InputField inventoryCapacityInput;
    public InputField interestedItemNamesInput;
    public InputField staminaReductionInput;
    public Dropdown traitOptions;
    public Dropdown ableJobsOptions;


    public Dropdown elementalTypeOptions;

    public Dropdown attackTypeOptions;
    public Dropdown rangeTypeOptions;
    public GameObject traitsGO;
    public GameObject relatedStructuresGO;
    public GameObject priorityJobsGO;
    public GameObject secondaryJobsGO;
    public GameObject ableJobsGO;

    public GameObject traitBtnGO;
    public GameObject relatedStructuresBtnGO;
    public GameObject priorityJobBtnPrefab;

    public ScrollRect traitsScrollRect;
    public ScrollRect relatedStructuresScrollRect;
    public ScrollRect priorityJobsScrollRect;
    public ScrollRect secondaryJobsScrollRect;
    public ScrollRect ableJobsScrollRect;

    [NonSerialized] public ClassTraitButton currentSelectedClassTraitButton;
    [NonSerialized] public StructureTypeButton currentSelectedRelatedStructuresButton;
    [NonSerialized] public PriorityJobButton currentSelectedPriorityJobButton;
    [NonSerialized] public PriorityJobButton currentSelectedSecondaryJobButton;
    [NonSerialized] public PriorityJobButton currentSelectedAbleJobButton;

    [NonSerialized] public List<string> allClasses;

    private List<string> _traitNames;
    private List<JOB_TYPE> _ableJobs;

    #region getters/setters
    public List<string> traitNames {
        get { return _traitNames; }
    }
    public List<JOB_TYPE> ableJobs {
        get { return _ableJobs; }
    }
    #endregion

    void Awake() {
        Instance = this;
    }

    #region Utilities
    private void UpdateClassList() {
        allClasses.Clear();
        string path = $"{UtilityScripts.Utilities.dataPath}CharacterClasses/";
        foreach (string file in Directory.GetFiles(path, "*.json")) {
            allClasses.Add(Path.GetFileNameWithoutExtension(file));
        }
        CharacterPanelUI.Instance.UpdateClassOptions();
    }
    //public void UpdateSkillOptions() {
    //    skillOptions.ClearOptions();
    //    skillOptions.AddOptions(SkillPanelUI.Instance.allSkills);
    //}
    //public void UpdateItemOptions() {
        //weaponsOptions.ClearOptions();
        //armorsOptions.ClearOptions();
        //relatedStructuresOptions.ClearOptions();

        //weaponsOptions.AddOptions(ItemPanelUI.Instance.allWeapons);
        //armorsOptions.AddOptions(ItemPanelUI.Instance.allArmors);
        //relatedStructuresOptions.AddOptions(ItemPanelUI.Instance.allItems);
    //}
    public void UpdateTraitOptions() {
        traitOptions.ClearOptions();
        traitOptions.AddOptions(TraitPanelUI.Instance.allTraits);
        traitOptions.AddOptions(TraitManager.instancedTraitsAndStatuses.ToList());
    }
    public void LoadAllData() {
        allClasses = new List<string>();
        //_weaponTiers = new List<string>();
        //_armorTiers = new List<string>();
        //_accessoryTiers = new List<string>();
        _traitNames = new List<string>();
        _ableJobs = new List<JOB_TYPE>();

        //recruitmentCostInput.text = "0";
        elementalTypeOptions.ClearOptions();
        //combatPositionOptions.ClearOptions();
        //combatTargetOptions.ClearOptions();
        attackTypeOptions.ClearOptions();
        rangeTypeOptions.ClearOptions();
        ableJobsOptions.ClearOptions();

        //damageTypeOptions.ClearOptions();
        //occupiedTileOptions.ClearOptions();
        //roleOptions.ClearOptions();
        //jobTypeOptions.ClearOptions();
        //recruitmentCostOptions.ClearOptions();

        string[] elementalTypes = System.Enum.GetNames(typeof(ELEMENTAL_TYPE));
        //string[] combatPositions = System.Enum.GetNames(typeof(COMBAT_POSITION));
        //string[] combatTargets = System.Enum.GetNames(typeof(COMBAT_TARGET));
        string[] attackTypes = System.Enum.GetNames(typeof(ATTACK_TYPE));
        string[] rangeTypes = System.Enum.GetNames(typeof(RANGE_TYPE));
        string[] structureTypes = System.Enum.GetNames(typeof(STRUCTURE_TYPE));
        string[] jobTypes = System.Enum.GetNames(typeof(JOB_TYPE));
        //string[] occupiedTiles = System.Enum.GetNames(typeof(COMBAT_OCCUPIED_TILE));
        //string[] roles = System.Enum.GetNames(typeof(CHARACTER_ROLE));
        //string[] jobs = System.Enum.GetNames(typeof(JOB));
        //string[] cost = System.Enum.GetNames(typeof(CURRENCY));

        elementalTypeOptions.AddOptions(elementalTypes.ToList());
        //combatPositionOptions.AddOptions(combatPositions.ToList());
        //combatTargetOptions.AddOptions(combatTargets.ToList());
        attackTypeOptions.AddOptions(attackTypes.ToList());
        rangeTypeOptions.AddOptions(rangeTypes.ToList());
        ableJobsOptions.AddOptions(jobTypes.ToList());

        //damageTypeOptions.AddOptions(damageTypes.ToList());
        //occupiedTileOptions.AddOptions(occupiedTiles.ToList());
        //roleOptions.AddOptions(roles.ToList());
        //jobTypeOptions.AddOptions(jobs.ToList());
        //recruitmentCostOptions.AddOptions(cost.ToList());
        UpdateClassList();
    }
    private void ClearData() {
        //latestLevel = 0;
        //currentSelectedWeaponButton = null;
        //currentSelectedArmorButton = null;
        //currentSelectedAccessoryButton = null;
        currentSelectedClassTraitButton = null;

        classNameInput.text = string.Empty;
        identifierInput.text = string.Empty;
        interestedItemNamesInput.text = string.Empty;
        tamedTraitInput.text = string.Empty;

        //nonCombatantToggle.isOn = false;

        baseAttackPowerInput.text = "0";
        //attackPowerPerLevelInput.text = "0";
        baseSpeedInput.text = "0";
        //speedPerLevelInput.text = "0";
        baseHPInput.text = "0";
        //hpPerLevelInput.text = "0";
        //baseSPInput.text = "0";
        //spPerLevelInput.text = "0";
        //recruitmentCostInput.text = "0";
        baseAttackSpeedInput.text = "1";
        attackRangeInput.text = "1";
        inventoryCapacityInput.text = "0";
        staminaReductionInput.text = "0";

        //weaponsOptions.value = 0;
        //armorsOptions.value = 0;
        traitOptions.value = 0;
        elementalTypeOptions.value = 0;
        //combatPositionOptions.value = 0;
        //combatTargetOptions.value = 0;
        attackTypeOptions.value = 0;
        rangeTypeOptions.value = 0;
        //damageTypeOptions.value = 0;
        //occupiedTileOptions.value = 0;
        //skillOptions.value = 0;
        //roleOptions.value = 0;
        //jobTypeOptions.value = 0;
        //recruitmentCostOptions.value = 0;

        //_weaponTiers.Clear();
        //_armorTiers.Clear();
        //_accessoryTiers.Clear();
        _traitNames.Clear();
        _ableJobs.Clear();

        //UtilityScripts.Utilities.DestroyChildren(weaponsContentTransform);
        //UtilityScripts.Utilities.DestroyChildren(armorsContentTransform);
        //UtilityScripts.Utilities.DestroyChildren(accessoriesContentTransform);
        UtilityScripts.Utilities.DestroyChildren(traitsScrollRect.content);
        UtilityScripts.Utilities.DestroyChildren(relatedStructuresScrollRect.content);
        UtilityScripts.Utilities.DestroyChildren(priorityJobsScrollRect.content);
        UtilityScripts.Utilities.DestroyChildren(secondaryJobsScrollRect.content);
        UtilityScripts.Utilities.DestroyChildren(ableJobsScrollRect.content);
    }
    private void SaveClass() {
        if (string.IsNullOrEmpty(classNameInput.text)) {
#if UNTIY_EDITOR
            EditorUtility.DisplayDialog("Error", "Please specify a Class Name", "OK");
            return;
#endif
        }
        if (string.IsNullOrEmpty(identifierInput.text)) {
#if UNTIY_EDITOR
            EditorUtility.DisplayDialog("Error", "Please specify an Identifier", "OK");
            return;
#endif
        }
        string path = $"{UtilityScripts.Utilities.dataPath}CharacterClasses/{classNameInput.text}.json";
        if (UtilityScripts.Utilities.DoesFileExist(path)) {
#if UNITY_EDITOR
            if (EditorUtility.DisplayDialog("Overwrite Class",
                $"A class with name {classNameInput.text} already exists. Replace with this class?", "Yes", "No")) {
                File.Delete(path);
                SaveClassJson(path);
            }
#endif
        } else {
            SaveClassJson(path);
        }
    }
    private void SaveClassJson(string path) {
        CharacterClass newClass = new CharacterClass();

        newClass.SetDataFromClassPanelUI();

        string jsonString = JsonUtility.ToJson(newClass);

        System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
        writer.WriteLine(jsonString);
        writer.Close();

#if UNITY_EDITOR
        //Re-import the file to update the reference in the editor
        UnityEditor.AssetDatabase.ImportAsset(path);
#endif
        Debug.Log($"Successfully saved class at {path}");

        UpdateClassList();
    }

    private void LoadClass() {
#if UNITY_EDITOR
        string filePath = EditorUtility.OpenFilePanel("Select Class",
            $"{UtilityScripts.Utilities.dataPath}CharacterClasses/", "json");

        if (!string.IsNullOrEmpty(filePath)) {
            string dataAsJson = File.ReadAllText(filePath);

            CharacterClass characterClass = JsonUtility.FromJson<CharacterClass>(dataAsJson);
            ClearData();
            LoadClassDataToUI(characterClass);
        }
#endif
    }

    private void LoadClassDataToUI(CharacterClass characterClass) {
        classNameInput.text = characterClass.className;
        identifierInput.text = characterClass.identifier;
        tamedTraitInput.text = characterClass.traitNameOnTamedByPlayer;
        //nonCombatantToggle.isOn = characterClass.isNormalNonCombatant;
        baseAttackPowerInput.text = characterClass.baseAttackPower.ToString();
        //attackPowerPerLevelInput.text = characterClass.attackPowerPerLevel.ToString();
        //baseSpeedInput.text = characterClass.baseSpeed.ToString();
        //speedPerLevelInput.text = characterClass.speedPerLevel.ToString();
        //armyCountInput.text = characterClass.armyCount.ToString();
        baseHPInput.text = characterClass.baseHP.ToString();
        //hpPerLevelInput.text = characterClass.hpPerLevel.ToString();
        baseAttackSpeedInput.text = characterClass.baseAttackSpeed.ToString();
        attackRangeInput.text = characterClass.attackRange.ToString();
        staminaReductionInput.text = characterClass.staminaReduction.ToString();
        //interestedItemNamesInput.text = characterClass.runSpeedMod.ToString();
        inventoryCapacityInput.text = characterClass.inventoryCapacity.ToString();
        interestedItemNamesInput.text = UtilityScripts.Utilities.ConvertArrayToString(characterClass.interestedItemNames, ',');

        elementalTypeOptions.value = GetDropdownIndex(elementalTypeOptions, characterClass.elementalType.ToString());
        //combatPositionOptions.value = GetDropdownIndex(combatPositionOptions, characterClass.combatPosition.ToString());
        //combatTargetOptions.value = GetDropdownIndex(combatTargetOptions, characterClass.combatTarget.ToString());
        attackTypeOptions.value = GetDropdownIndex(attackTypeOptions, characterClass.attackType.ToString());
        rangeTypeOptions.value = GetDropdownIndex(rangeTypeOptions, characterClass.rangeType.ToString());
        //damageTypeOptions.value = GetDropdownIndex(damageTypeOptions, characterClass.damageType.ToString());
        //occupiedTileOptions.value = GetDropdownIndex(occupiedTileOptions, characterClass.occupiedTileType.ToString());

        //roleOptions.value = GetDropdownIndex(roleOptions, characterClass.roleType.ToString());
        //skillOptions.value = GetDropdownIndex(skillOptions, characterClass.skillName.ToString());
        //jobTypeOptions.value = GetDropdownIndex(jobTypeOptions, characterClass.jobType.ToString());
        for (int i = 0; i < characterClass.traitNames.Length; i++) {
            string traitName = characterClass.traitNames[i];
            _traitNames.Add(traitName);
            GameObject go = GameObject.Instantiate(traitBtnGO, traitsScrollRect.content);
            go.GetComponent<ClassTraitButton>().SetTraitName(traitName);
        }
        if (characterClass.ableJobs != null) {
            for (int i = 0; i < characterClass.ableJobs.Length; i++) {
                JOB_TYPE jobType = characterClass.ableJobs[i];
                OnAddJob(jobType, "able");
            }
        }
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
    public void OnAddNewClass() {
        ClearData();
    }
    public void OnSaveClass() {
        SaveClass();
    }
    public void OnEditClass() {
        LoadClass();
    }
    //public void OnAddWeapon() {
    //    string weaponTypeToAdd = weaponsOptions.options[weaponsOptions.value].text;
    //    if (!_weaponTiers.Contains(weaponTypeToAdd)) {
    //        _weaponTiers.Add(weaponTypeToAdd);
    //        GameObject go = GameObject.Instantiate(weaponTypeBtnGO, weaponsContentTransform);
    //        go.GetComponent<WeaponTypeButton>().buttonText.text = weaponTypeToAdd;
    //        go.GetComponent<WeaponTypeButton>().panelName = "class";
    //        go.GetComponent<WeaponTypeButton>().categoryName = "weapon";
    //    }
    //}
    //public void OnRemoveWeapon() {
    //    if (currentSelectedWeaponButton != null) {
    //        string weaponTypeToRemove = currentSelectedWeaponButton.buttonText.text;
    //        if (_weaponTiers.Remove(weaponTypeToRemove)) {
    //            GameObject.Destroy(currentSelectedWeaponButton.gameObject);
    //            currentSelectedWeaponButton = null;
    //        }
    //    }
    //}
    //public void OnAddArmor() {
    //    string armorToAdd = armorsOptions.options[armorsOptions.value].text;
    //    if (!_armorTiers.Contains(armorToAdd)) {
    //        _armorTiers.Add(armorToAdd);
    //        GameObject go = GameObject.Instantiate(weaponTypeBtnGO, armorsContentTransform);
    //        go.GetComponent<WeaponTypeButton>().buttonText.text = armorToAdd;
    //        go.GetComponent<WeaponTypeButton>().panelName = "class";
    //        go.GetComponent<WeaponTypeButton>().categoryName = "armor";
    //    }
    //}
    //public void OnRemoveArmor() {
    //    if (currentSelectedArmorButton != null) {
    //        string armorToRemove = currentSelectedArmorButton.buttonText.text;
    //        if (_armorTiers.Remove(armorToRemove)) {
    //            GameObject.Destroy(currentSelectedArmorButton.gameObject);
    //            currentSelectedArmorButton = null;
    //        }
    //    }
    //}
    //public void OnAddAccessory() {
    //    string accessoryToAdd = relatedStructuresOptions.options[relatedStructuresOptions.value].text;
    //    if (!_accessoryTiers.Contains(accessoryToAdd)) {
    //        _accessoryTiers.Add(accessoryToAdd);
    //        GameObject go = GameObject.Instantiate(weaponTypeBtnGO, accessoriesContentTransform);
    //        go.GetComponent<WeaponTypeButton>().buttonText.text = accessoryToAdd;
    //        go.GetComponent<WeaponTypeButton>().panelName = "class";
    //        go.GetComponent<WeaponTypeButton>().categoryName = "accessory";
    //    }
    //}
    //public void OnRemoveAccessory() {
    //    if (currentSelectedAccessoryButton != null) {
    //        string accessoryToRemove = currentSelectedAccessoryButton.buttonText.text;
    //        if (_accessoryTiers.Remove(accessoryToRemove)) {
    //            GameObject.Destroy(currentSelectedAccessoryButton.gameObject);
    //            currentSelectedAccessoryButton = null;
    //        }
    //    }
    //}
    public void OnAddTrait() {
        string traitToAdd = traitOptions.options[traitOptions.value].text;
        if (!_traitNames.Contains(traitToAdd)) {
            _traitNames.Add(traitToAdd);
            GameObject go = GameObject.Instantiate(traitBtnGO, traitsScrollRect.content);
            go.GetComponent<ClassTraitButton>().SetTraitName(traitToAdd);
        }
    }
    public void OnRemoveTrait() {
        if (currentSelectedClassTraitButton != null) {
            string traitToRemove = currentSelectedClassTraitButton.buttonText.text;
            if (_traitNames.Remove(traitToRemove)) {
                GameObject.Destroy(currentSelectedClassTraitButton.gameObject);
                currentSelectedClassTraitButton = null;
            }
        }
    }
    public void OnAddAbleJob() {
        string job = ableJobsOptions.options[ableJobsOptions.value].text;
        JOB_TYPE jobType = (JOB_TYPE) System.Enum.Parse(typeof(JOB_TYPE), job);
        OnAddJob(jobType, "able");
    }
    public void OnRemovePriorityJob() {
        OnRemoveJob("priority");
    }
    public void OnRemoveSecondaryJob() {
        OnRemoveJob("secondary");
    }
    public void OnRemoveAbleJob() {
        OnRemoveJob("able");
    }
    private void OnAddJob(JOB_TYPE jobType, string identifier) {
        bool hasBeenAdded = false;
        ScrollRect scrollRect = null;
        if (identifier == "able") {
            scrollRect = ableJobsScrollRect;
            if (!_ableJobs.Contains(jobType)) {
                _ableJobs.Add(jobType);
                hasBeenAdded = true;
            }
        }
        if (hasBeenAdded) {
            GameObject go = GameObject.Instantiate(priorityJobBtnPrefab, scrollRect.content);
            go.GetComponent<PriorityJobButton>().SetJobType(jobType, identifier);
        }
    }
    public void OnRemoveJob(string identifier) {
        if (identifier == "able") {
            if (currentSelectedAbleJobButton != null) {
                JOB_TYPE jobType = currentSelectedAbleJobButton.jobType;
                if (_ableJobs.Remove(jobType)) {
                    GameObject.Destroy(currentSelectedAbleJobButton.gameObject);
                    currentSelectedAbleJobButton = null;
                }
            }
        }
        
    }
    public void OnClickTraitsTab() {
        relatedStructuresGO.SetActive(false);
        traitsGO.SetActive(true);
        priorityJobsGO.SetActive(false);
        secondaryJobsGO.SetActive(false);
        ableJobsGO.SetActive(false);
    }
    public void OnClickRelatedStructuresTab() {
        relatedStructuresGO.SetActive(true);
        traitsGO.SetActive(false);
        priorityJobsGO.SetActive(false);
        secondaryJobsGO.SetActive(false);
        ableJobsGO.SetActive(false);
    }
    public void OnClickPriorityJobsTab() {
        relatedStructuresGO.SetActive(false);
        traitsGO.SetActive(false);
        priorityJobsGO.SetActive(true);
        secondaryJobsGO.SetActive(false);
        ableJobsGO.SetActive(false);
    }
    public void OnClickSecondaryJobsTab() {
        relatedStructuresGO.SetActive(false);
        traitsGO.SetActive(false);
        priorityJobsGO.SetActive(false);
        secondaryJobsGO.SetActive(true);
        ableJobsGO.SetActive(false);
    }
    public void OnClickAbleJobsTab() {
        relatedStructuresGO.SetActive(false);
        traitsGO.SetActive(false);
        priorityJobsGO.SetActive(false);
        secondaryJobsGO.SetActive(false);
        ableJobsGO.SetActive(true);
    }
    #endregion
}
