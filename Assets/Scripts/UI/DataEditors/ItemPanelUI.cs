using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class ItemPanelUI : MonoBehaviour {
    public static ItemPanelUI Instance;

    public Dropdown itemTypeOptions;
    public Dropdown weaponTypeOptions;
    public Dropdown armorTypeOptions;
    public Dropdown weaponPrefixOptions;
    public Dropdown weaponSuffixOptions;
    public Dropdown armorPrefixOptions;
    public Dropdown armorSuffixOptions;
    public Dropdown elementOptions;
    public Dropdown attributeOptions;
    public Dropdown iconOptions;

    public InputField nameInput;
    public InputField descriptionInput;
    public InputField interactionInput;
    public InputField goldCostInput;
    public InputField powerInput;
    public InputField defInput;

    public Toggle stackableToggle;

    public Image iconImg;

    public GameObject weaponFieldsGO;
    public GameObject armorFieldsGO;
    public GameObject attributeBtnPrefab;

    public Transform attributeContentTransform;

    private List<string> _attributes;
    private List<string> _allItems;
    private List<string> _allWeapons;
    private List<string> _allArmors;

    private Dictionary<string, Sprite> _iconSprites;
    private AttributeBtn _currentSelectedButton;

    #region getters/setters
    public List<string> allItems {
        get { return _allItems; }
    }
    public List<string> allWeapons {
        get { return _allWeapons; }
    }
    public List<string> allArmors {
        get { return _allArmors; }
    }
    #endregion

    void Awake() {
        Instance = this;
    }

    #region Utilities
    private void UpdateAllItems() {
        _allItems.Clear();
        _allWeapons.Clear();
        _allArmors.Clear();

        _allArmors.Add("None");
        string path = $"{UtilityScripts.Utilities.dataPath}Items/";
        string[] directories = Directory.GetDirectories(path);
        for (int i = 0; i < directories.Length; i++) {
            string folderName = new DirectoryInfo(directories[i]).Name;
            string[] files = Directory.GetFiles(directories[i], "*.json");
            for (int j = 0; j < files.Length; j++) {
                string fileName = Path.GetFileNameWithoutExtension(files[j]);
                if (folderName == "WEAPON") {
                    _allWeapons.Add(fileName);
                }else if (folderName == "ARMOR") {
                    _allArmors.Add(fileName);
                }
                _allItems.Add(fileName);
            }
        }
        MonsterPanelUI.Instance.UpdateItemDropOptions();
        CharacterPanelUI.Instance.UpdateItemOptions();
        //ClassPanelUI.Instance.UpdateItemOptions();
    }
    public void LoadAllData() {
        _attributes = new List<string>();
        _allItems = new List<string>();
        _allWeapons = new List<string>();
        _allArmors = new List<string>();
        itemTypeOptions.ClearOptions();
        weaponTypeOptions.ClearOptions();
        armorTypeOptions.ClearOptions();
        weaponPrefixOptions.ClearOptions();
        weaponSuffixOptions.ClearOptions();
        armorPrefixOptions.ClearOptions();
        armorSuffixOptions.ClearOptions();
        elementOptions.ClearOptions();
        //attributeOptions.ClearOptions();
        iconOptions.ClearOptions();

        string[] itemTypes = System.Enum.GetNames(typeof(ITEM_TYPE));
        string[] weaponTypes = System.Enum.GetNames(typeof(WEAPON_TYPE));
        string[] armorTypes = System.Enum.GetNames(typeof(ARMOR_TYPE));
        string[] weaponPrefixes = System.Enum.GetNames(typeof(WEAPON_PREFIX));
        string[] weaponSuffixes = System.Enum.GetNames(typeof(WEAPON_SUFFIX));
        string[] armorPrefixes = System.Enum.GetNames(typeof(ARMOR_PREFIX));
        string[] armorSuffixes = System.Enum.GetNames(typeof(ARMOR_SUFFIX));
        string[] elements = System.Enum.GetNames(typeof(ELEMENT));

        //List<string> attributes = new List<string>();
        //string path = Utilities.dataPath + "Attributes/ITEM/";
        //foreach (string file in Directory.GetFiles(path, "*.json")) {
        //    attributes.Add(Path.GetFileNameWithoutExtension(file));
        //}

        Sprite[] icons = Resources.LoadAll<Sprite>("Textures/ItemIcons");
        _iconSprites = new Dictionary<string, Sprite>();
        _iconSprites.Add("None", null);
        for (int i = 0; i < icons.Length; i++) {
            _iconSprites.Add(icons[i].name, icons[i]);
        }

        itemTypeOptions.AddOptions(itemTypes.ToList());
        weaponTypeOptions.AddOptions(weaponTypes.ToList());
        armorTypeOptions.AddOptions(armorTypes.ToList());
        weaponPrefixOptions.AddOptions(weaponPrefixes.ToList());
        weaponSuffixOptions.AddOptions(weaponSuffixes.ToList());
        armorPrefixOptions.AddOptions(armorPrefixes.ToList());
        armorSuffixOptions.AddOptions(armorSuffixes.ToList());
        elementOptions.AddOptions(elements.ToList());
        //attributeOptions.AddOptions(attributes);
        iconOptions.AddOptions(_iconSprites.Keys.ToList());

        UpdateAllItems();
    }
    private void ClearData() {
        itemTypeOptions.value = 0;
        weaponTypeOptions.value = 0;
        armorTypeOptions.value = 0;
        weaponPrefixOptions.value = 0;
        weaponSuffixOptions.value = 0;
        armorPrefixOptions.value = 0;
        armorSuffixOptions.value = 0;
        elementOptions.value = 0;
        attributeOptions.value = 0;
        iconOptions.value = 0;

        nameInput.text = string.Empty;
        descriptionInput.text = string.Empty;
        interactionInput.text = string.Empty;
        goldCostInput.text = "0";
        powerInput.text = "0";
        defInput.text = "0";

        stackableToggle.isOn = false;

        iconImg.sprite = null;

        armorFieldsGO.SetActive(false);

        _attributes.Clear();
        foreach (Transform child in attributeContentTransform) {
            GameObject.Destroy(child.gameObject);
        }
    }
    private void SaveItem() {
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(nameInput.text)) {
            EditorUtility.DisplayDialog("Error", "Please specify an Item Name", "OK");
            return;
        }
        string path =
            $"{UtilityScripts.Utilities.dataPath}Items/{itemTypeOptions.options[itemTypeOptions.value].text}/{nameInput.text}.json";
        if (UtilityScripts.Utilities.DoesFileExist(path)) {
            if (EditorUtility.DisplayDialog("Overwrite Item",
                $"An item with name {nameInput.text} already exists. Replace with this item?", "Yes", "No")) {
                File.Delete(path);
                SaveItemJson(path);
            }
        } else {
            SaveItemJson(path);
        }
#endif
    }
    private void SaveItemJson(string path) {
        ITEM_TYPE itemType = (ITEM_TYPE) System.Enum.Parse(typeof(ITEM_TYPE), itemTypeOptions.options[itemTypeOptions.value].text);
        if (itemType == ITEM_TYPE.WEAPON) {
            SaveWeapon(path);
        } else if (itemType == ITEM_TYPE.ARMOR) {
            SaveArmor(path);
        } else {
            Save(path);
        }
    }
    private void SaveWeapon(string path) {
        
    }
    private void SaveArmor(string path) {
       
    }
    private void Save(string path) {
        
    }
    private int GetItemTypeIndex(string name) {
        for (int i = 0; i < itemTypeOptions.options.Count; i++) {
            if (itemTypeOptions.options[i].text == name) {
                return i;
            }
        }
        return 0;
    }
    private int GetWeaponTypeIndex(string name) {
        for (int i = 0; i < weaponTypeOptions.options.Count; i++) {
            if (weaponTypeOptions.options[i].text == name) {
                return i;
            }
        }
        return 0;
    }
    private int GetWeaponPrefixIndex(string name) {
        for (int i = 0; i < weaponPrefixOptions.options.Count; i++) {
            if (weaponPrefixOptions.options[i].text == name) {
                return i;
            }
        }
        return 0;
    }
    private int GetWeaponSuffixIndex(string name) {
        for (int i = 0; i < weaponSuffixOptions.options.Count; i++) {
            if (weaponSuffixOptions.options[i].text == name) {
                return i;
            }
        }
        return 0;
    }
    private int GetElementIndex(string name) {
        for (int i = 0; i < elementOptions.options.Count; i++) {
            if (elementOptions.options[i].text == name) {
                return i;
            }
        }
        return 0;
    }
    private int GetArmorTypeIndex(string name) {
        for (int i = 0; i < armorTypeOptions.options.Count; i++) {
            if (armorTypeOptions.options[i].text == name) {
                return i;
            }
        }
        return 0;
    }
    private int GetArmorPrefixIndex(string name) {
        for (int i = 0; i < armorPrefixOptions.options.Count; i++) {
            if (armorPrefixOptions.options[i].text == name) {
                return i;
            }
        }
        return 0;
    }
    private int GetArmorSuffixIndex(string name) {
        for (int i = 0; i < armorSuffixOptions.options.Count; i++) {
            if (armorSuffixOptions.options[i].text == name) {
                return i;
            }
        }
        return 0;
    }
    private int GetIconIndex(string name) {
        for (int i = 0; i < iconOptions.options.Count; i++) {
            if (iconOptions.options[i].text == name) {
                return i;
            }
        }
        return 0;
    }
    public void SetCurrentlySelectedButton(AttributeBtn btn) {
        _currentSelectedButton = btn;
    }
    public void UpdateAttributeOptions() {
        attributeOptions.ClearOptions();
        attributeOptions.AddOptions(TraitPanelUI.Instance.allTraits);
    }
    #endregion

    #region OnValueChanged
    public void OnItemTypeChange(int index) {
        weaponFieldsGO.SetActive(false);
        armorFieldsGO.SetActive(false);

        ITEM_TYPE itemType = (ITEM_TYPE) System.Enum.Parse(typeof(ITEM_TYPE), itemTypeOptions.options[index].text);
        if(itemType == ITEM_TYPE.WEAPON) {
            weaponFieldsGO.SetActive(true);
        }else if (itemType == ITEM_TYPE.ARMOR) {
            armorFieldsGO.SetActive(true);
        }
    }
    public void OnIconChange(int index) {
        string iconName = iconOptions.options[index].text;
        Sprite sprite = _iconSprites[iconName];
        iconImg.sprite = sprite;
    }
    #endregion

    #region Button Clicks
    public void OnClickAddNewItem() {
        ClearData();
    }
    public void OnClickEditItem() {
    }
    public void OnClickSaveItem() {
        SaveItem();
    }
    public void OnAddAttribute() {
        string attributeToAdd = attributeOptions.options[attributeOptions.value].text;
        if (!_attributes.Contains(attributeToAdd)) {
            _attributes.Add(attributeToAdd);
            GameObject go = GameObject.Instantiate(attributeBtnPrefab, attributeContentTransform);
            go.GetComponent<AttributeBtn>().buttonText.text = attributeToAdd;
        }
    }
    public void OnRemoveAttribute() {
        if (_currentSelectedButton != null) {
            string attributeToRemove = _currentSelectedButton.buttonText.text;
            if (_attributes.Remove(attributeToRemove)) {
                GameObject.Destroy(_currentSelectedButton.gameObject);
                _currentSelectedButton = null;
            }
        }
    }
    #endregion
}
