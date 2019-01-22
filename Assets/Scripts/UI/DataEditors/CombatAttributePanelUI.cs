﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.IO;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CombatAttributePanelUI : MonoBehaviour {
    public static CombatAttributePanelUI Instance;

    //General
    public InputField nameInput;
    public InputField descriptionInput;
    public InputField durationInput;
    public Dropdown traitTypeOptions;
    public Dropdown traitEffectOptions;

    //Effects
    public InputField amountInput;
    public InputField traitEffectDescriptionInput;
    public Dropdown statOptions;
    public Dropdown requirementTargetOptions;
    public Dropdown requirementTypeOptions;
    public Dropdown requirementSeparatorOptions;
    public Dropdown requirementOptions;
    public Toggle percentageToggle;
    public Toggle hasRequirementToggle;
    public Toggle isNotToggle;
    public ScrollRect requirementsScrollRect;
    public ScrollRect effectsScrollRect;
    public GameObject traitEffectBtnGO;
    public GameObject requirementBtnGO;
    public GameObject requirementsParentGO;
    [NonSerialized] public TraitEffectButton currentSelectedTraitEffectButton;
    [NonSerialized] public RequirementButton currentSelectedRequirementButton;

    private List<string> _allCombatAttributes;
    private List<TraitEffect> _effects;
    private List<string> _requirements;

    #region getters/setters
    public List<string> allCombatAttributes {
        get { return _allCombatAttributes; }
    }
    #endregion
    private void Awake() {
        Instance = this;
    }
    private void UpdateCombatAttributes() {
        _allCombatAttributes.Clear();
        string path = Utilities.dataPath + "CombatAttributes/";
        foreach (string file in Directory.GetFiles(path, "*.json")) {
            _allCombatAttributes.Add(Path.GetFileNameWithoutExtension(file));
        }
        ItemPanelUI.Instance.UpdateAttributeOptions();
        CharacterPanelUI.Instance.UpdateCombatAttributeOptions();
        ClassPanelUI.Instance.UpdateTraitOptions();
        RacePanelUI.Instance.UpdateTraitOptions();
    }
    public void LoadAllData() {
        _allCombatAttributes = new List<string>();
        _requirements = new List<string>();
        _effects = new List<TraitEffect>();

        statOptions.ClearOptions();
        traitTypeOptions.ClearOptions();
        traitEffectOptions.ClearOptions();
        requirementTypeOptions.ClearOptions();
        requirementOptions.ClearOptions();
        requirementTargetOptions.ClearOptions();
        requirementSeparatorOptions.ClearOptions();

        string[] stats = System.Enum.GetNames(typeof(STAT));
        string[] traitTypes = System.Enum.GetNames(typeof(TRAIT_TYPE));
        string[] traitEffects = System.Enum.GetNames(typeof(TRAIT_EFFECT));
        string[] requirementTypes = System.Enum.GetNames(typeof(TRAIT_REQUIREMENT));
        string[] requirementTargets = System.Enum.GetNames(typeof(TRAIT_REQUIREMENT_TARGET));
        string[] requirementSeparators = System.Enum.GetNames(typeof(TRAIT_REQUIREMENT_SEPARATOR));

        statOptions.AddOptions(stats.ToList());
        traitTypeOptions.AddOptions(traitTypes.ToList());
        traitEffectOptions.AddOptions(traitEffects.ToList());
        requirementTypeOptions.AddOptions(requirementTypes.ToList());
        requirementTargetOptions.AddOptions(requirementTargets.ToList());
        requirementSeparatorOptions.AddOptions(requirementSeparators.ToList());

        //requirementTypeOptions.value = 0;
        OnRequirementTypeChange(requirementTypeOptions.value);

        UpdateCombatAttributes();
    }
    private void ClearData() {
        currentSelectedTraitEffectButton = null;
        currentSelectedRequirementButton = null;

        statOptions.value = 0;
        traitTypeOptions.value = 0;
        traitEffectOptions.value = 0;
        requirementTypeOptions.value = 0;
        requirementOptions.value = 0;
        requirementTargetOptions.value = 0;
        requirementSeparatorOptions.value = 0;

        nameInput.text = string.Empty;
        descriptionInput.text = string.Empty;
        traitEffectDescriptionInput.text = string.Empty;
        amountInput.text = "0";
        durationInput.text = "0";

        percentageToggle.isOn = false;
        hasRequirementToggle.isOn = false;
        isNotToggle.isOn = false;

        _effects.Clear();
        _requirements.Clear();
        effectsScrollRect.content.DestroyChildren();
        requirementsScrollRect.content.DestroyChildren();
    }

    private void SaveCombatAttribute() {
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(nameInput.text)) {
            EditorUtility.DisplayDialog("Error", "Please specify a Combat Attribute Name", "OK");
            return;
        }
        string path = Utilities.dataPath + "CombatAttributes/" + nameInput.text + ".json";
        if (Utilities.DoesFileExist(path)) {
            if (EditorUtility.DisplayDialog("Overwrite Combat Attribute", "A combat attribute with name " + nameInput.text + " already exists. Replace with this combat attribute?", "Yes", "No")) {
                File.Delete(path);
                SaveCombatAttributeJson(path);
            }
        } else {
            SaveCombatAttributeJson(path);
        }
#endif
    }

    private void SaveCombatAttributeJson(string path) {
        float amountInp = 0f;
        if (!string.IsNullOrEmpty(amountInput.text)) {
            amountInp = float.Parse(amountInput.text);
        }
        Trait newTrait = new Trait {
            name = nameInput.text,
            description = descriptionInput.text,
            type = (TRAIT_TYPE) System.Enum.Parse(typeof(TRAIT_TYPE), traitTypeOptions.options[traitTypeOptions.value].text),
            effect = (TRAIT_EFFECT) System.Enum.Parse(typeof(TRAIT_EFFECT), traitEffectOptions.options[traitEffectOptions.value].text),
            daysDuration = int.Parse(durationInput.text),
            effects = _effects
        };
        string jsonString = JsonUtility.ToJson(newTrait);

        System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
        writer.WriteLine(jsonString);
        writer.Close();

#if UNITY_EDITOR
        //Re-import the file to update the reference in the editor
        UnityEditor.AssetDatabase.ImportAsset(path);
#endif
        Debug.Log("Successfully saved trait at " + path);

        UpdateCombatAttributes();
    }

    private void LoadCombatAttribute() {
#if UNITY_EDITOR
        string filePath = EditorUtility.OpenFilePanel("Select Trait", Utilities.dataPath + "CombatAttributes/", "json");
        if (!string.IsNullOrEmpty(filePath)) {
            string dataAsJson = File.ReadAllText(filePath);
            Trait attribute = JsonUtility.FromJson<Trait>(dataAsJson);
            ClearData();
            LoadCombatAttributeDataToUI(attribute);
        }
#endif
    }
    private void LoadCombatAttributeDataToUI(Trait trait) {
        nameInput.text = trait.name;
        descriptionInput.text = trait.description;
        traitTypeOptions.value = GetOptionIndex(trait.type.ToString(), traitTypeOptions);
        traitEffectOptions.value = GetOptionIndex(trait.effect.ToString(), traitEffectOptions);
        durationInput.text = trait.daysDuration.ToString();

        for (int i = 0; i < trait.effects.Count; i++) {
            TraitEffect traitEffect = trait.effects[i];
            _effects.Add(traitEffect);
            GameObject go = GameObject.Instantiate(traitEffectBtnGO, effectsScrollRect.content);
            go.GetComponent<TraitEffectButton>().SetTraitEffect(traitEffect);
        }
    }
    private void PopulateRequirements(List<string> requirements) {
        if(requirements != null) {
            requirementOptions.ClearOptions();
            requirementOptions.AddOptions(requirements);
        }
    }
    private int GetOptionIndex(string identifier, Dropdown options) {
        for (int i = 0; i < options.options.Count; i++) {
            if (options.options[i].text.ToLower() == identifier.ToLower()) {
                return i;
            }
        }
        return 0;
    }
    private List<string> GetCombatRequirementsByType(TRAIT_REQUIREMENT requirementType) {
        //if (requirementType == TRAIT_REQUIREMENT.ATTRIBUTE) {
        //    return System.Enum.GetNames(typeof(ATTRIBUTE)).ToList();
        //} else if (requirementType == TRAIT_REQUIREMENT.CLASS) {
        //    return ClassPanelUI.Instance.allClasses;
        //} else if (requirementType == TRAIT_REQUIREMENT.ELEMENT) {
        //    return System.Enum.GetNames(typeof(ELEMENT)).ToList();
        //}
        if (requirementType == TRAIT_REQUIREMENT.RACE) {
            return System.Enum.GetNames(typeof(RACE)).ToList();
        }else if (requirementType == TRAIT_REQUIREMENT.TRAIT) {
            return _allCombatAttributes;
        }
        return null;
    }

    public void OnRequirementTypeChange(int index) {
        TRAIT_REQUIREMENT requirementType = (TRAIT_REQUIREMENT) System.Enum.Parse(typeof(TRAIT_REQUIREMENT), requirementTypeOptions.options[index].text);
        List<string> requirements = GetCombatRequirementsByType(requirementType);
        PopulateRequirements(requirements);
    }
    public void OnToggleRequirement(bool state) {
        RequirementOptionsActivation(state);
    }
    private void RequirementOptionsActivation(bool state) {
        requirementsParentGO.SetActive(state);
    }
    #region Button Clicks
    public void OnClickAddNewAttribute() {
        ClearData();
    }
    public void OnClickEditAttribute() {
        LoadCombatAttribute();
    }
    public void OnClickSaveAttribute() {
        SaveCombatAttribute();
    }
    public void OnClickAddRequirement() {
        string requirementToAdd = requirementOptions.options[requirementOptions.value].text;
        if (!_requirements.Contains(requirementToAdd)) {
            _requirements.Add(requirementToAdd);
            GameObject go = GameObject.Instantiate(requirementBtnGO, requirementsScrollRect.content);
            go.GetComponent<RequirementButton>().SetRequirement(requirementToAdd);
        }
    }
    public void OnClickRemoveRequirement() {
        if (currentSelectedRequirementButton != null) {
            string requirementToRemove = currentSelectedRequirementButton.requirement;
            if (_requirements.Remove(requirementToRemove)) {
                GameObject.Destroy(currentSelectedRequirementButton.gameObject);
                currentSelectedRequirementButton = null;
            }
        }
    }
    public void OnClickAddTraitEffect() {
        TraitEffect traitEffect = new TraitEffect {
            stat = (STAT) System.Enum.Parse(typeof(STAT), statOptions.options[statOptions.value].text),
            amount = float.Parse(amountInput.text),
            isPercentage = percentageToggle.isOn,
            target = (TRAIT_REQUIREMENT_TARGET) System.Enum.Parse(typeof(TRAIT_REQUIREMENT_TARGET), requirementTargetOptions.options[requirementTargetOptions.value].text),
            description = traitEffectDescriptionInput.text,
            hasRequirement = hasRequirementToggle.isOn,
            isNot = isNotToggle.isOn,
            requirementType = (TRAIT_REQUIREMENT) System.Enum.Parse(typeof(TRAIT_REQUIREMENT), requirementTypeOptions.options[requirementTypeOptions.value].text),
            requirementSeparator = (TRAIT_REQUIREMENT_SEPARATOR) System.Enum.Parse(typeof(TRAIT_REQUIREMENT_SEPARATOR), requirementSeparatorOptions.options[requirementSeparatorOptions.value].text),
            requirements = new List<string>(_requirements)
        };
        _effects.Add(traitEffect);
        GameObject go = GameObject.Instantiate(traitEffectBtnGO, effectsScrollRect.content);
        go.GetComponent<TraitEffectButton>().SetTraitEffect(traitEffect);
    }
    public void OnClickRemoveTraitEffect() {
        if (currentSelectedTraitEffectButton != null) {
            TraitEffect traitEffectToRemove = currentSelectedTraitEffectButton.traitEffect;
            if (_effects.Remove(traitEffectToRemove)) {
                GameObject.Destroy(currentSelectedTraitEffectButton.gameObject);
                currentSelectedTraitEffectButton = null;
            }
        }
    }
    #endregion
}
