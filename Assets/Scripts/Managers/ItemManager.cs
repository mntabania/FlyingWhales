using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class ItemManager : MonoBehaviour {

    public static ItemManager Instance = null;

    public static List<string> lootChestNames = new List<string> {
        "Tier 1 Armor Chest",
        "Tier 1 Weapon Chest"
    };

    private Dictionary<string, ECS.Item> _allItems;
	private Dictionary<string, ECS.Weapon> allWeapons;
	private Dictionary<string, ECS.Armor> allArmors;

    private Dictionary<WEAPON_TYPE, ECS.WeaponType> _weaponTypeData;
    private Dictionary<ARMOR_TYPE, ECS.ArmorType> _armorTypeData;

	private Dictionary<ITEM_TYPE, List<EQUIPMENT_TYPE>> _equipmentTypes;

    private Dictionary<WEAPON_PREFIX, WeaponPrefix> _weaponPrefixes;
    private Dictionary<WEAPON_SUFFIX, WeaponSuffix> _weaponSuffixes;

    private Dictionary<ARMOR_PREFIX, ArmorPrefix> _armorPrefixes;
    private Dictionary<ARMOR_SUFFIX, ArmorSuffix> _armorSuffixes;

    //private List<List<ECS.Item>> _itemTiers;
    //private List<List<ECS.Weapon>> _weaponTiers;
    //private List<List<ECS.Armor>> _armorTiers;

    //   public int crudeWeaponPowerModifier;
    //public int exceptionalWeaponPowerModifier;
    //public int crudeWeaponDurabilityModifier;
    //public int exceptionalWeaponDurabilityModifier;

    //public int crudeArmorMitigationModifier;
    //public int exceptionalArmorMitigationModifier;
    //public int crudeArmorDurabilityModifier;
    //public int exceptionalArmorDurabilityModifier;

    //public List<TextAssetListWrapper> armorTiersAsset;
    //public List<TextAssetListWrapper> weaponTiersAsset;

    #region getters/setters
    public Dictionary<ARMOR_TYPE, ECS.ArmorType> armorTypeData{
		get { return _armorTypeData; }
	}
	public Dictionary<WEAPON_TYPE, ECS.WeaponType> weaponTypeData{
		get { return _weaponTypeData; }
	}
    public Dictionary<WEAPON_PREFIX, WeaponPrefix> weaponPrefixes {
        get { return _weaponPrefixes; }
    }
    public Dictionary<WEAPON_SUFFIX, WeaponSuffix> weaponSuffixes {
        get { return _weaponSuffixes; }
    }
    public Dictionary<ARMOR_PREFIX, ArmorPrefix> armorPrefixes {
        get { return _armorPrefixes; }
    }
    public Dictionary<ARMOR_SUFFIX, ArmorSuffix> armorSuffixes {
        get { return _armorSuffixes; }
    }
    public Dictionary<string, ECS.Item> allItems {
        get { return _allItems; }
    }
    #endregion

    private void Awake() {
        Instance = this;
    }
	internal void Initialize(){
        _weaponPrefixes = new Dictionary<WEAPON_PREFIX, WeaponPrefix>();
        _weaponSuffixes = new Dictionary<WEAPON_SUFFIX, WeaponSuffix>();
        _armorPrefixes = new Dictionary<ARMOR_PREFIX, ArmorPrefix>();
        _armorSuffixes = new Dictionary<ARMOR_SUFFIX, ArmorSuffix>();
        _equipmentTypes = new Dictionary<ITEM_TYPE, List<EQUIPMENT_TYPE>> ();
		_equipmentTypes.Add (ITEM_TYPE.WEAPON, new List<EQUIPMENT_TYPE> ());
		_equipmentTypes.Add (ITEM_TYPE.ARMOR, new List<EQUIPMENT_TYPE> ());
		_equipmentTypes.Add (ITEM_TYPE.ITEM, new List<EQUIPMENT_TYPE> ());
		ConstructItemsDictionary();
        ConstructWeaponTypeData();
        ConstructArmorTypeData();
        CreateWeaponPrefix(WEAPON_PREFIX.NONE);
        CreateWeaponSuffix(WEAPON_SUFFIX.NONE);
        CreateArmorPrefix(ARMOR_PREFIX.NONE);
        CreateArmorSuffix(ARMOR_SUFFIX.NONE);
		//ConstructWeaponTiers ();
		//ConstructArmorTiers ();
		//ConstructItemTiers ();

	}
    private void ConstructItemsDictionary() {
        _allItems = new Dictionary<string, ECS.Item>();
		allWeapons = new Dictionary<string, ECS.Weapon>();
		allArmors = new Dictionary<string, ECS.Armor> ();
        string path = Utilities.dataPath + "Items/";
        string[] directories = Directory.GetDirectories(path);
        for (int i = 0; i < directories.Length; i++) {
            string currDirectory = directories[i];
            string itemType = new DirectoryInfo(currDirectory).Name;
            ITEM_TYPE currItemType = (ITEM_TYPE)Enum.Parse(typeof(ITEM_TYPE), itemType);
            string[] files = Directory.GetFiles(currDirectory, "*.json");
            for (int k = 0; k < files.Length; k++) {
                string currFilePath = files[k];
                string dataAsJson = File.ReadAllText(currFilePath);
                switch (currItemType) {
				case ITEM_TYPE.WEAPON:
					ECS.Weapon newWeapon = JsonUtility.FromJson<ECS.Weapon> (dataAsJson);
					_allItems.Add (newWeapon.itemName, newWeapon);
					allWeapons.Add (newWeapon.itemName, newWeapon);
                    CreateWeaponPrefix(newWeapon.prefixType);
                    CreateWeaponSuffix(newWeapon.suffixType);
                    break;
                case ITEM_TYPE.ARMOR:
                    ECS.Armor newArmor = JsonUtility.FromJson<ECS.Armor>(dataAsJson);
                    _allItems.Add(newArmor.itemName, newArmor);
					allArmors.Add (newArmor.itemName, newArmor);
                    CreateArmorPrefix(newArmor.prefixType);
                    CreateArmorSuffix(newArmor.suffixType);
                    break;
                default:
					ECS.Item newItem = JsonUtility.FromJson<ECS.Item>(dataAsJson);
					_allItems.Add(newItem.itemName, newItem);
                    break;
                }
            }
        }
    }
    public ECS.Item CreateNewItemInstance(string itemName) {
        if (_allItems.ContainsKey(itemName)) {
            return _allItems[itemName].CreateNewCopy();
        }
        throw new System.Exception("There is no item type called " + itemName);
    }

    public ECS.Item CreateNewItemInstance(MATERIAL itemMaterial, EQUIPMENT_TYPE equipmentType) {
        string itemName = Utilities.NormalizeString(itemMaterial.ToString()) + " " + Utilities.NormalizeString(equipmentType.ToString());
        if (_allItems.ContainsKey(itemName)) {
            return _allItems[itemName].CreateNewCopy();
        }
        throw new System.Exception("There is no item type called " + itemName);
    }
    private void ConstructWeaponTypeData() {
        _weaponTypeData = new Dictionary<WEAPON_TYPE, ECS.WeaponType>();
        string path = Utilities.dataPath + "WeaponTypes/";
        string[] files = Directory.GetFiles(path, "*.json");
        for (int i = 0; i < files.Length; i++) {
            string currFilePath = files[i];
            string dataAsJson = File.ReadAllText(currFilePath);
            ECS.WeaponType data = JsonUtility.FromJson<ECS.WeaponType>(dataAsJson);
            _weaponTypeData.Add(data.weaponType, data);
			_equipmentTypes [ITEM_TYPE.WEAPON].Add ((EQUIPMENT_TYPE)data.weaponType);
			_equipmentTypes [ITEM_TYPE.ITEM].Add ((EQUIPMENT_TYPE)data.weaponType);
        }
    }
    private void ConstructArmorTypeData() {
        _armorTypeData = new Dictionary<ARMOR_TYPE, ECS.ArmorType>();
        string path = Utilities.dataPath + "ArmorTypes/";
        string[] files = Directory.GetFiles(path, "*.json");
        for (int i = 0; i < files.Length; i++) {
            string currFilePath = files[i];
            string dataAsJson = File.ReadAllText(currFilePath);
            ECS.ArmorType data = JsonUtility.FromJson<ECS.ArmorType>(dataAsJson);
            _armorTypeData.Add(data.armorType, data);
			_equipmentTypes [ITEM_TYPE.ARMOR].Add ((EQUIPMENT_TYPE)data.armorType);
			_equipmentTypes [ITEM_TYPE.ITEM].Add ((EQUIPMENT_TYPE)data.armorType);
        }
    }

    private void CreateWeaponPrefix(WEAPON_PREFIX prefix) {
        if(!_weaponPrefixes.ContainsKey(prefix)) {
            switch (prefix) {
                case WEAPON_PREFIX.NONE:
                _weaponPrefixes.Add(prefix, new WeaponPrefix(prefix));
                break;
            }
        }
    }
    private void CreateWeaponSuffix(WEAPON_SUFFIX suffix) {
        if (!_weaponSuffixes.ContainsKey(suffix)) {
            switch (suffix) {
                case WEAPON_SUFFIX.NONE:
                _weaponSuffixes.Add(suffix, new WeaponSuffix(suffix));
                break;
            }
        }

    }

    private void CreateArmorPrefix(ARMOR_PREFIX prefix) {
        if (!_armorPrefixes.ContainsKey(prefix)) {
            switch (prefix) {
                case ARMOR_PREFIX.NONE:
                _armorPrefixes.Add(prefix, new ArmorPrefix(prefix));
                break;
            }
        }

    }
    private void CreateArmorSuffix(ARMOR_SUFFIX suffix) {
        if (!_armorSuffixes.ContainsKey(suffix)) {
            switch (suffix) {
                case ARMOR_SUFFIX.NONE:
                _armorSuffixes.Add(suffix, new ArmorSuffix(suffix));
                break;
            }
        }

    }

    //private void ConstructArmorTiers(){
    //	_armorTiers = new List<List<ECS.Armor>> ();
    //	for (int i = 0; i < armorTiersAsset.Count; i++) {
    //		List<ECS.Armor> armorList = new List<ECS.Armor> ();
    //		for (int j = 0; j < armorTiersAsset[i].list.Count; j++) {
    //			armorList.Add (allArmors[armorTiersAsset [i].list [j].name]);
    //		}
    //		_armorTiers.Add (armorList);
    //	}
    //}
    //private void ConstructWeaponTiers(){
    //	_weaponTiers = new List<List<ECS.Weapon>> ();
    //	for (int i = 0; i < weaponTiersAsset.Count; i++) {
    //		List<ECS.Weapon> weaponList = new List<ECS.Weapon> ();
    //		for (int j = 0; j < weaponTiersAsset[i].list.Count; j++) {
    //			weaponList.Add (allWeapons[weaponTiersAsset [i].list [j].name]);
    //		}
    //		_weaponTiers.Add (weaponList);
    //	}
    //}
    //private void ConstructItemTiers(){
    //	_itemTiers = new List<List<ECS.Item>> ();
    //	int maxCount = _weaponTiers.Count;
    //	if(_armorTiers.Count > _weaponTiers.Count){
    //		maxCount = _armorTiers.Count;
    //	}
    //	for (int i = 0; i < maxCount; i++) {
    //		List<ECS.Item> itemAssets = new List<ECS.Item> ();
    //		if(i < _armorTiers.Count){
    //			for (int j = 0; j < _armorTiers[i].Count; j++) {
    //				itemAssets.Add (_armorTiers [i][j]);
    //			}
    //		}
    //		if(i < _weaponTiers.Count){
    //			for (int j = 0; j < _weaponTiers[i].Count; j++) {
    //				itemAssets.Add (_weaponTiers [i][j]);
    //			}
    //		}
    //		_itemTiers.Add (itemAssets);
    //	}
    //}


    internal ECS.WeaponType GetWeaponTypeData(WEAPON_TYPE weaponType) {
        if (_weaponTypeData.ContainsKey(weaponType)) {
            return _weaponTypeData[weaponType];
        }
        return null;
    }
    internal ECS.ArmorType GetArmorTypeData(ARMOR_TYPE armorType) {
        if (_armorTypeData.ContainsKey(armorType)) {
            return _armorTypeData[armorType];
        }
        return null;
    }
    internal int GetGoldCostOfItem(ITEM_TYPE itemType, MATERIAL material) {
        if (itemType == ITEM_TYPE.ARMOR) {
            return MaterialManager.Instance.materialsLookup[material].armorData.cost;
        } else if (itemType == ITEM_TYPE.WEAPON) {
            return MaterialManager.Instance.materialsLookup[material].weaponData.cost;
        }
        return -1;
    }

    internal bool CanMaterialBeUsedForWeapon(MATERIAL material) {
		if (ProductionManager.Instance.weaponMaterials.Contains(material)) {
			return true;
		}
        return false;
    }

    internal bool CanMaterialBeUsedForArmor(MATERIAL material) {
		if (ProductionManager.Instance.armorMaterials.Contains(material)) {
			return true;
		}
        return false;
    }

	internal ECS.Weapon GetRandomWeapon(){
		int index = UnityEngine.Random.Range (0, allWeapons.Count);
		int count = 0;
		foreach (ECS.Weapon weapon in allWeapons.Values) {
			if(index == count){
				return (ECS.Weapon)CreateNewItemInstance (weapon.itemName);
			}else{
				count++;
			}
		}
		return null;
	}
	internal ECS.Armor GetRandomArmor(){
		int index = UnityEngine.Random.Range (0, allArmors.Count);
		int count = 0;
		foreach (ECS.Armor armor in allArmors.Values) {
			if(index == count){
				return (ECS.Armor)CreateNewItemInstance (armor.itemName);
			}else{
				count++;
			}
		}
		return null;
	}

	internal EQUIPMENT_TYPE GetRandomEquipmentTypeByItemType(ITEM_TYPE itemType){
		return _equipmentTypes [itemType] [UnityEngine.Random.Range (0, _equipmentTypes [itemType].Count)];
	}

	//internal ECS.Weapon GetRandomWeaponTier(int tier){
	//	if(tier > 0){
	//		int index = tier - 1;
	//		ECS.Weapon weaponAsset = _weaponTiers [index] [UnityEngine.Random.Range (0, _weaponTiers [index].Count)];
	//		return (ECS.Weapon)CreateNewItemInstance (weaponAsset.itemName);
	//	}
	//	return null;
	//}

	//internal ECS.Armor GetRandomArmorTier(int tier){
	//	if(tier > 0){
	//		int index = tier - 1;
	//		ECS.Armor armorAsset = _armorTiers [index] [UnityEngine.Random.Range (0, _armorTiers [index].Count)];
	//		return (ECS.Armor)CreateNewItemInstance (armorAsset.itemName);
	//	}
	//	return null;
	//}

	//internal ECS.Item GetRandomItemTier(int tier){
	//	if(tier > 0){
	//		int index = tier - 1;
	//		ECS.Item itemAsset = _itemTiers [index] [UnityEngine.Random.Range (0, _itemTiers [index].Count)];
	//		return CreateNewItemInstance (itemAsset.itemName);
	//	}
	//	return null;
	//}
	//internal ECS.Item GetRandomTier(int tier, ITEM_TYPE itemType){
	//	if(itemType == ITEM_TYPE.WEAPON){
	//		return GetRandomWeaponTier (tier);
	//	}else if(itemType == ITEM_TYPE.ARMOR){
	//		return GetRandomArmorTier (tier);
	//	}else if(itemType == ITEM_TYPE.ITEM){
	//		return GetRandomItemTier (tier);
	//	}
	//	return null;
	//}

	//internal List<ECS.Weapon> GetWeaponTierList(int tier){
	//	if(tier > 0){
	//		int index = tier - 1;
	//		return _weaponTiers [index];
	//	}
	//	return null;
	//}

	//internal List<ECS.Armor> GetArmorTierList(int tier){
	//	if(tier > 0){
	//		int index = tier - 1;
	//		return _armorTiers [index];
	//	}
	//	return null;
	//}

	//internal List<ECS.Item> GetItemTierList(int tier){
	//	if(tier > 0){
	//		int index = tier - 1;
	//		return _itemTiers [index];
	//	}
	//	return null;
	//}

 //   public bool IsLootChest(ECS.Item item) {
 //       if (lootChestNames.Contains(item.itemName)) {
 //           return true;
 //       }
 //       return false;
 //   }
}
