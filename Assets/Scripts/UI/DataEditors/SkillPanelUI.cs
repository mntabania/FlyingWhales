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


public class SkillPanelUI : MonoBehaviour {
    public static SkillPanelUI Instance;

    public InputField skillNameInput;
    public InputField skillDescInput;
    //public InputField powerInput;
    //public InputField spCostInput;
    //public InputField actWeightInput;
    //public InputField rangeInput;
    //public InputField cellAmountInput;

    //public Dropdown attackTypeOptions;
    public Dropdown skillTypeOptions;
    public Dropdown elementOptions;
    public Dropdown targetTypeOptions;
    //public Dropdown allowedWeaponsOptions;

    public GameObject cellAmountGO;
    public GameObject weaponTypeBtnGO;

    public Transform contentTransform;

    [NonSerialized] public WeaponTypeButton currentSelectedWeaponTypeButton;
    [NonSerialized] public List<string> allSkills;
    //private string[] _attackCategories;
    //private string[] _elements;
    //private string[] _targetTypes;
    //private string[] _weaponTypes;

    //private List<string> _allowedWeaponTypes;

    void Awake() {
        Instance = this;
    }

    #region Utilities
    public void LoadAllData() {
        //attackTypeOptions.ClearOptions();
        elementOptions.ClearOptions();
        targetTypeOptions.ClearOptions();
        skillTypeOptions.ClearOptions();
        //allowedWeaponsOptions.ClearOptions();

        //string[] attackCategories = System.Enum.GetNames(typeof(ATTACK_CATEGORY));
        //string[] elements = System.Enum.GetNames(typeof(ELEMENT));
        //string[] targetTypes = System.Enum.GetNames(typeof(TARGET_TYPE));
        //string[] skillTypes = System.Enum.GetNames(typeof(SKILL_TYPE));

        //attackTypeOptions.AddOptions(attackCategories.ToList());
        //elementOptions.AddOptions(elements.ToList());
        //targetTypeOptions.AddOptions(targetTypes.ToList());
        //skillTypeOptions.AddOptions(skillTypes.ToList());
        //allowedWeaponsOptions.AddOptions(weaponTypes.ToList());

        allSkills = new List<string>();
        UpdateSkillList();
    }

    private void ClearData() {
        currentSelectedWeaponTypeButton = null;
        skillNameInput.text = string.Empty;
        skillDescInput.text = string.Empty;

        //powerInput.text = "0";
        //spCostInput.text = "0";
        //actWeightInput.text = "0";
        //rangeInput.text = "0";

        //attackTypeOptions.value = 0;
        elementOptions.value = 0;
        targetTypeOptions.value = 0;
        skillTypeOptions.value = 0;
        //allowedWeaponsOptions.value = 0;

        //_allowedWeaponTypes.Clear();
        //foreach (Transform child in contentTransform) {
        //    GameObject.Destroy(child.gameObject);
        //}
    }
    private void SaveSkill() {
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(skillNameInput.text)) {
            EditorUtility.DisplayDialog("Error", "Please specify a Skill Name", "OK");
            return;
        }
        string path = $"{UtilityScripts.Utilities.dataPath}Skills/{skillNameInput.text}.json";
        if (UtilityScripts.Utilities.DoesFileExist(path)) {
            if (EditorUtility.DisplayDialog("Overwrite Skill",
                $"A skill with name {skillNameInput.text} already exists. Replace with this skill?", "Yes", "No")) {
                File.Delete(path);
                SaveSkillJson(path);
            }
        } else {
            SaveSkillJson(path);
        }
#endif
    }
    private void SaveSkillJson(string path) {
        
    }
    public void LoadSkill() {
//#if UNITY_EDITOR
//        string filePath = EditorUtility.OpenFilePanel("Select Skill", Utilities.dataPath + "Skills/", "json");

//        if (!string.IsNullOrEmpty(filePath)) {
//            string dataAsJson = File.ReadAllText(filePath);
//        }
//#endif
    }
    private int GetOptionIndex(string name, Dropdown ddOptions) {
        for (int i = 0; i < ddOptions.options.Count; i++) {
            if (ddOptions.options[i].text.ToLower() == name.ToLower()) {
                return i;
            }
        }
        return 0;
    }
    private void UpdateSkillList() {
        allSkills.Clear();
        string path = $"{UtilityScripts.Utilities.dataPath}Skills/";
        foreach (string file in Directory.GetFiles(path, "*.json")) {
            allSkills.Add(Path.GetFileNameWithoutExtension(file));
        }

        //ClassPanelUI.Instance.UpdateSkillOptions();
        MonsterPanelUI.Instance.UpdateSkillList();
    }
    #endregion

    #region Button Clicks
    public void OnAddNewSkill() {
        ClearData();
    }
    public void OnAddWeaponType() {
        //string weaponTypeToAdd = allowedWeaponsOptions.options[allowedWeaponsOptions.value].text;
        //if (weaponTypeToAdd != "NONE" && !_allowedWeaponTypes.Contains(weaponTypeToAdd)) {
        //    _allowedWeaponTypes.Add(weaponTypeToAdd);
        //    GameObject go = GameObject.Instantiate(weaponTypeBtnGO, contentTransform);
        //    go.GetComponent<WeaponTypeButton>().buttonText.text = weaponTypeToAdd;
        //    go.GetComponent<WeaponTypeButton>().panelName = "skill";
        //}
    }
    public void OnRemoveWeaponType() {
        //if(currentSelectedWeaponTypeButton != null) {
        //    string weaponTypeToRemove = currentSelectedWeaponTypeButton.buttonText.text;
        //    if (_allowedWeaponTypes.Remove(weaponTypeToRemove)) {
        //        GameObject.Destroy(currentSelectedWeaponTypeButton.gameObject);
        //        currentSelectedWeaponTypeButton = null;
        //    }
        //}
    }
    public void OnSaveSkill() {
        SaveSkill();
    }
    public void OnEditSkill() {
        LoadSkill();
    }
    #endregion
}
