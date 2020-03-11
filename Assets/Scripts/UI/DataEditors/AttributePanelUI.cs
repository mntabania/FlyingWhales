using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AttributePanelUI : MonoBehaviour {
    public static AttributePanelUI Instance;

    public Dropdown categoryOptions;
    public Dropdown tickBehaviorOptions;
    public Dropdown addBehaviorOptions;
    public Dropdown removeBehaviorOptions;

    public InputField nameInput;
    public Toggle hiddenToggle;

    public Transform tickBehaviorContentTransform;
    public Transform addBehaviorContentTransform;
    public Transform removeBehaviorContentTransform;

    public GameObject behaviorBtnPrefab;

    private BehaviorBtn _currentSelectTickBtn;
    private BehaviorBtn _currentSelectAddBtn;
    private BehaviorBtn _currentSelectRemoveBtn;

    private List<string> _allItemAttributes;
    private List<string> _tickBehaviors;
    private List<string> _addBehaviors;
    private List<string> _removeBehaviors;
    
    #region getters/setters
    public List<string> allItemAttributes {
        get { return _allItemAttributes; }
    }
    public List<string> tickBehaviors {
        get { return _tickBehaviors; }
    }
    public List<string> addBehaviors {
        get { return _addBehaviors; }
    }
    public List<string> removeBehaviors {
        get { return _removeBehaviors; }
    }
    #endregion

    void Awake() {
        Instance = this;
    }

    #region Utilities
    private void UpdateItemAttributes() {
        _allItemAttributes.Clear();
        string path = $"{UtilityScripts.Utilities.dataPath}Attributes/ITEM/";
        foreach (string file in Directory.GetFiles(path, "*.json")) {
            _allItemAttributes.Add(Path.GetFileNameWithoutExtension(file));
        }
        ItemPanelUI.Instance.UpdateAttributeOptions();
    }
    public void LoadAllData() {
        _allItemAttributes = new List<string>();
        _tickBehaviors = new List<string>();
        _addBehaviors = new List<string>();
        _removeBehaviors = new List<string>();

        categoryOptions.ClearOptions();
        tickBehaviorOptions.ClearOptions();
        addBehaviorOptions.ClearOptions();
        removeBehaviorOptions.ClearOptions();

        string[] categories = System.Enum.GetNames(typeof(ATTRIBUTE_CATEGORY));
        string[] behaviors = System.Enum.GetNames(typeof(ATTRIBUTE_BEHAVIOR));

        categoryOptions.AddOptions(categories.ToList());
        tickBehaviorOptions.AddOptions(behaviors.ToList());
        addBehaviorOptions.AddOptions(behaviors.ToList());
        removeBehaviorOptions.AddOptions(behaviors.ToList());

        UpdateItemAttributes();
    }
    private void ClearData() {
        categoryOptions.value = 0;
        tickBehaviorOptions.value = 0;
        addBehaviorOptions.value = 0;
        removeBehaviorOptions.value = 0;
        nameInput.text = string.Empty;
        hiddenToggle.isOn = false;


        _tickBehaviors.Clear();
        _addBehaviors.Clear();
        _removeBehaviors.Clear();

        foreach (Transform child in tickBehaviorContentTransform) {
            GameObject.Destroy(child.gameObject);
        }
        foreach (Transform child in addBehaviorContentTransform) {
            GameObject.Destroy(child.gameObject);
        }
        foreach (Transform child in removeBehaviorContentTransform) {
            GameObject.Destroy(child.gameObject);
        }
    }
    private void SaveAttribute() {
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(nameInput.text)) {
            EditorUtility.DisplayDialog("Error", "Please specify an Attribute Name", "OK");
            return;
        }
        string path =
            $"{UtilityScripts.Utilities.dataPath}Attributes/{categoryOptions.options[categoryOptions.value].text}/{nameInput.text}.json";
        if (UtilityScripts.Utilities.DoesFileExist(path)) {
            if (EditorUtility.DisplayDialog("Overwrite Attribute",
                $"An attribute with name {nameInput.text} already exists. Replace with this attribute?", "Yes", "No")) {
                File.Delete(path);
                SaveAttributeJson(path);
            }
        } else {
            SaveAttributeJson(path);
        }
#endif
    }
    private void SaveAttributeJson(string path) {
//        Attribute newAttribute = new Attribute();

//        newAttribute.SetDataFromAttributePanelUI();

//        string jsonString = JsonUtility.ToJson(newAttribute);

//        System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
//        writer.WriteLine(jsonString);
//        writer.Close();

//#if UNITY_EDITOR
//        //Re-import the file to update the reference in the editor
//        UnityEditor.AssetDatabase.ImportAsset(path);
//#endif
//        Debug.Log("Successfully saved attribute at " + path);
//        UpdateItemAttributes();
    }
    private void LoadAttribute() {
#if UNITY_EDITOR
        string filePath = EditorUtility.OpenFilePanel("Select Attribute",
            $"{UtilityScripts.Utilities.dataPath}Attributes/", "json");
        if (!string.IsNullOrEmpty(filePath)) {
            string dataAsJson = File.ReadAllText(filePath);
            CharacterAttribute attribute = JsonUtility.FromJson<CharacterAttribute>(dataAsJson);
            ClearData();
            LoadAttributeDataToUI(attribute);
        }
#endif
    }
    private void LoadAttributeDataToUI(CharacterAttribute attribute) {
        nameInput.text = attribute.name;
        hiddenToggle.isOn = attribute.isHidden;
        categoryOptions.value = GetCategoryIndex(attribute.category.ToString());

        //for (int i = 0; i < attribute.tickBehaviors.Count; i++) {
        //    string behavior = attribute.tickBehaviors[i].ToString();
        //    _tickBehaviors.Add(behavior);
        //    GameObject go = GameObject.Instantiate(behaviorBtnPrefab, tickBehaviorContentTransform);
        //    go.GetComponent<BehaviorBtn>().buttonText.text = behavior;
        //}
        //for (int i = 0; i < attribute.onAddedBehaviors.Count; i++) {
        //    string behavior = attribute.onAddedBehaviors[i].ToString();
        //    _addBehaviors.Add(behavior);
        //    GameObject go = GameObject.Instantiate(behaviorBtnPrefab, addBehaviorContentTransform);
        //    go.GetComponent<BehaviorBtn>().buttonText.text = behavior;
        //}
        //for (int i = 0; i < attribute.onRemovedBehaviors.Count; i++) {
        //    string behavior = attribute.onRemovedBehaviors[i].ToString();
        //    _removeBehaviors.Add(behavior);
        //    GameObject go = GameObject.Instantiate(behaviorBtnPrefab, removeBehaviorContentTransform);
        //    go.GetComponent<BehaviorBtn>().buttonText.text = behavior;
        //}
    }
    private int GetCategoryIndex(string categoryName) {
        for (int i = 0; i < categoryOptions.options.Count; i++) {
            if (categoryOptions.options[i].text == categoryName) {
                return i;
            }
        }
        return 0;
    }
    public void SetCurrentSelectBehaviorBtn(BehaviorBtn btn) {
        if(btn.gameObject.transform.parent == tickBehaviorContentTransform) {
            _currentSelectTickBtn = btn;
        }else if (btn.gameObject.transform.parent == addBehaviorContentTransform) {
            _currentSelectAddBtn = btn;
        }else if (btn.gameObject.transform.parent == removeBehaviorContentTransform) {
            _currentSelectRemoveBtn = btn;
        }
    }
    #endregion

    #region Button Clicks
    public void OnClickAddNewAttribute() {
        ClearData();
    }
    public void OnClickEditAttribute() {
        LoadAttribute();
    }
    public void OnClickSaveAttribute() {
        SaveAttribute();
    }
    public void OnAddTickBehavior() {
        string behaviorToAdd = tickBehaviorOptions.options[tickBehaviorOptions.value].text;
        if (behaviorToAdd != "NONE" && !_tickBehaviors.Contains(behaviorToAdd)) {
            _tickBehaviors.Add(behaviorToAdd);
            GameObject go = GameObject.Instantiate(behaviorBtnPrefab, tickBehaviorContentTransform);
            go.GetComponent<BehaviorBtn>().buttonText.text = behaviorToAdd;
        }
    }
    public void OnRemoveTickBehavior() {
        if (_currentSelectTickBtn != null) {
            string behaviorToRemove = _currentSelectTickBtn.buttonText.text;
            if (_tickBehaviors.Remove(behaviorToRemove)) {
                GameObject.Destroy(_currentSelectTickBtn.gameObject);
                _currentSelectTickBtn = null;
            }
        }
    }
    public void OnAddAddedBehavior() {
        string behaviorToAdd = addBehaviorOptions.options[addBehaviorOptions.value].text;
        if (behaviorToAdd != "NONE" && !_addBehaviors.Contains(behaviorToAdd)) {
            _addBehaviors.Add(behaviorToAdd);
            GameObject go = GameObject.Instantiate(behaviorBtnPrefab, addBehaviorContentTransform);
            go.GetComponent<BehaviorBtn>().buttonText.text = behaviorToAdd;
        }
    }
    public void OnRemoveAddedBehavior() {
        if (_currentSelectAddBtn != null) {
            string behaviorToRemove = _currentSelectAddBtn.buttonText.text;
            if (_addBehaviors.Remove(behaviorToRemove)) {
                GameObject.Destroy(_currentSelectAddBtn.gameObject);
                _currentSelectAddBtn = null;
            }
        }
    }
    public void OnAddRemovedBehavior() {
        string behaviorToAdd = removeBehaviorOptions.options[removeBehaviorOptions.value].text;
        if (behaviorToAdd != "NONE" && !_removeBehaviors.Contains(behaviorToAdd)) {
            _removeBehaviors.Add(behaviorToAdd);
            GameObject go = GameObject.Instantiate(behaviorBtnPrefab, removeBehaviorContentTransform);
            go.GetComponent<BehaviorBtn>().buttonText.text = behaviorToAdd;
        }
    }
    public void OnRemoveRemovedBehavior() {
        if (_currentSelectRemoveBtn != null) {
            string behaviorToRemove = _currentSelectRemoveBtn.buttonText.text;
            if (_removeBehaviors.Remove(behaviorToRemove)) {
                GameObject.Destroy(_currentSelectRemoveBtn.gameObject);
                _currentSelectRemoveBtn = null;
            }
        }
    }
    #endregion
}
