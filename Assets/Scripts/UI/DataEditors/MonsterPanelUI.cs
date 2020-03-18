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


public class MonsterPanelUI : MonoBehaviour {
    public static MonsterPanelUI Instance;

    public InputField nameInput;
    public InputField levelInput;
    public InputField expInput;
    public InputField hpInput;
    public InputField spInput;
    public InputField powerInput;
    public InputField speedInput;
    public InputField dodgeInput;
    public InputField hitInput;
    public InputField critInput;
    public InputField itemDropRateInput;
    public InputField armyCountInput;

    public Dropdown typeOptions;
    public Dropdown skillOptions;
    public Dropdown itemDropOptions;

    public Toggle isSleepingOnSpawnToggle;

    public Transform skillContentTransform;
    public Transform itemDropContentTransform;

    public GameObject skillsGO;
    public GameObject itemDropGO;
    public GameObject monsterSkillBtnGO;
    public GameObject itemDropBtnPrefab;

    [NonSerialized] public MonsterSkillButton currentSelectedButton;

    private ItemDropBtn _currentSelectedItemDropBtn;
    private List<string> _allSkills;

    #region getters/setters
    public List<string> allSkills {
        get { return _allSkills; }
    }
    #endregion

    void Awake() {
        Instance = this;
    }

    #region Utilities
    public void UpdateSkillList() {
        skillOptions.ClearOptions();
        skillOptions.AddOptions(SkillPanelUI.Instance.allSkills);
    }
    public void UpdateItemDropOptions() {
        itemDropOptions.ClearOptions();
        itemDropOptions.AddOptions(ItemPanelUI.Instance.allItems);
    }
    public void LoadAllData() {
        _allSkills = new List<string>();
        typeOptions.ClearOptions();
        //itemDropOptions.ClearOptions();

        // string[] monsterTypes = System.Enum.GetNames(typeof(MONSTER_TYPE));

        //List<string> allItems = new List<string>();
        //string path = Utilities.dataPath + "Items/";
        //string[] directories = Directory.GetDirectories(path);
        //for (int i = 0; i < directories.Length; i++) {
        //    string[] files = Directory.GetFiles(directories[i], "*.json");
        //    for (int j = 0; j < files.Length; j++) {
        //        allItems.Add(Path.GetFileNameWithoutExtension(files[j]));
        //    }
        //}

        //itemDropOptions.AddOptions(allItems);
        // typeOptions.AddOptions(monsterTypes.ToList());
    }
    private void ClearData() {
        currentSelectedButton = null;
        nameInput.text = string.Empty;

        levelInput.text = "1";
        expInput.text = "0";
        hpInput.text = "0";
        spInput.text = "0";
        powerInput.text = "0";
        speedInput.text = "0";
        dodgeInput.text = "0";
        hitInput.text = "0";
        critInput.text = "0";
        itemDropRateInput.text = "0";
        armyCountInput.text = "1";

        typeOptions.value = 0;
        skillOptions.value = 0;
        itemDropOptions.value = 0;

        isSleepingOnSpawnToggle.isOn = false;

        _allSkills.Clear();
        foreach (Transform child in skillContentTransform) {
            GameObject.Destroy(child.gameObject);
        }
        foreach (Transform child in itemDropContentTransform) {
            GameObject.Destroy(child.gameObject);
        }
    }
    private void SaveMonster() {
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(nameInput.text)) {
            EditorUtility.DisplayDialog("Error", "Please specify a Monster Name", "OK");
            return;
        }
        string path = $"{UtilityScripts.Utilities.dataPath}Monsters/{nameInput.text}.json";
        if (UtilityScripts.Utilities.DoesFileExist(path)) {
            if (EditorUtility.DisplayDialog("Overwrite Monster",
                $"A monster with name {nameInput.text} already exists. Replace with this monster?", "Yes", "No")) {
                File.Delete(path);
            }
        } 
#endif
    }
    public void SetItemDropBn(ItemDropBtn btn) {
        _currentSelectedItemDropBtn = btn;
    }
    #endregion

    #region Button Clicks
    public void OnClickAddNewMonster() {
        ClearData();
    }
    public void OnClickSaveMonster() {
        SaveMonster();
    }
    public void OnClickAddSkill() {
        string skillToAdd = skillOptions.options[skillOptions.value].text;
        if (!_allSkills.Contains(skillToAdd)) {
            _allSkills.Add(skillToAdd);
            GameObject go = GameObject.Instantiate(monsterSkillBtnGO, skillContentTransform);
            go.GetComponent<MonsterSkillButton>().buttonText.text = skillToAdd;
        }
    }
    public void OnClickRemoveSkill() {
        if (currentSelectedButton != null) {
            string skillToRemove = currentSelectedButton.buttonText.text;
            if (_allSkills.Remove(skillToRemove)) {
                GameObject.Destroy(currentSelectedButton.gameObject);
                currentSelectedButton = null;
            }
        }
    }
    public void OnClickSkills() {
        skillsGO.SetActive(true);
        itemDropGO.SetActive(false);
    }
    public void OnClickItemDrops() {
        skillsGO.SetActive(false);
        itemDropGO.SetActive(true);
    }
    public void OnClickAddItemDrop() {
        string itemToAdd = itemDropOptions.options[itemDropOptions.value].text;
    }
    public void OnClickRemoveItemDrop() {
        if (_currentSelectedItemDropBtn != null) {
            string itemToRemove = _currentSelectedItemDropBtn.name;
        }
    }
    #endregion
}
