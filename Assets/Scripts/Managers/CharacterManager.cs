using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ECS;

public class CharacterManager : MonoBehaviour {

    public static CharacterManager Instance = null;

    public GameObject characterIconPrefab;
    public Transform characterIconsParent;

    public GameObject armyIconPrefab;
    public Transform armyIconsParent;

    public int maxLevel;
    public List<CharacterType> characterTypes;
    public List<Trait> traitSetup;
    private Dictionary<TRAIT, string> traitDictionary;
    private Dictionary<string, CharacterClass> _classesDictionary;
    private Dictionary<ELEMENT, float> _elementsChanceDictionary;
    private List<Character> allCharacters;

	public Sprite heroSprite;
	public Sprite villainSprite;
	public Sprite hermitSprite;
	public Sprite beastSprite;
	public Sprite banditSprite;
	public Sprite chieftainSprite;

    [Header("Character Portrait Assets")]
    public List<HairSetting> hairSettings;
    public List<Sprite> noseSprites;
    public List<Sprite> mouthSprites;
    public List<Sprite> eyeSprites;
    public List<Sprite> eyeBrowSprites;
    public List<Color> hairColors;

    #region getters/setters
    //public Dictionary<int, HashSet<Citizen>> elligibleCitizenAgeTable {
    //    get { return citizenAgeTable.Where(x => x.Value.Any()).ToDictionary(x => x.Key, v => v.Value); }
    //}
    public Dictionary<string, CharacterClass> classesDictionary {
        get { return _classesDictionary; }
    }
    public Dictionary<ELEMENT, float> elementsChanceDictionary {
        get { return _elementsChanceDictionary; }
    }
    #endregion

    private void Awake() {
        Instance = this;
        allCharacters = new List<Character>();
    }

    public void Initialize() {
        ConstructTraitDictionary();
        ConstructAllClasses();
        ConstructElementChanceDictionary();
    }

    #region ECS.Character Types
    internal CharacterType GetRandomCharacterType() {
        return characterTypes[Random.Range(0, characterTypes.Count)];
    }
    #endregion

    #region Characters
    /*
     Create a new character, given a role, class and race.
         */
	public ECS.Character CreateNewCharacter(CHARACTER_ROLE charRole, string className, RACE race, Faction faction = null) {
		if(className == "None"){
            className = "Classless";
		}
        ECS.CharacterSetup setup = ECS.CombatManager.Instance.GetBaseCharacterSetup(className, race);
        if (setup == null) {
            Debug.LogError("THERE IS NO CLASS WITH THE NAME: " + className + "!");
            return null;
        }
		ECS.Character newCharacter = new ECS.Character(setup);
        if (faction != null) {
            newCharacter.SetFaction(faction);
        }
        if(charRole != CHARACTER_ROLE.NONE) {
            newCharacter.AssignRole(charRole);
        }
        allCharacters.Add(newCharacter);
        return newCharacter;
    }
	public ECS.Character CreateNewCharacter(CHARACTER_ROLE charRole, ECS.CharacterSetup setup) {
		if (setup == null) {
			return null;
		}
		ECS.Character newCharacter = new ECS.Character(setup);
		newCharacter.AssignRole(charRole);
        allCharacters.Add(newCharacter);
        Messenger.Broadcast(Signals.CHARACTER_CREATED, newCharacter);
        return newCharacter;
	}
    /*
     Create a new character, given filename.
         */
    public ECS.Character CreateNewCharacter(string fileName, Faction faction = null) {
        ECS.CharacterSetup setup = ECS.CombatManager.Instance.GetBaseCharacterSetup(fileName);
        if (setup == null) {
            Debug.LogError("THERE IS NO CLASS WITH THE NAME: " + fileName + "!");
            return null;
        }
        ECS.Character newCharacter = new ECS.Character(setup);
        if (faction != null) {
            newCharacter.SetFaction(faction);
        }
        if (setup.optionalRole != CHARACTER_ROLE.NONE) {
            newCharacter.AssignRole(setup.optionalRole);
        }
        allCharacters.Add(newCharacter);
        Messenger.Broadcast(Signals.CHARACTER_CREATED, newCharacter);
        return newCharacter;
    }
    private void ConstructAllClasses() {
        _classesDictionary = new Dictionary<string, CharacterClass>();
        string path = Utilities.dataPath + "CharacterClasses/";
        string[] classes = System.IO.Directory.GetFiles(path, "*.json");
        for (int i = 0; i < classes.Length; i++) {
            CharacterClass currentClass = JsonUtility.FromJson<CharacterClass>(System.IO.File.ReadAllText(classes[i]));
            //CharacterClass currentClass = new CharacterClass();
            currentClass.ConstructSkills();
            _classesDictionary.Add(currentClass.className, currentClass);
        }
    }
    #endregion

    #region Traits
    // internal Trait CreateNewTraitForCitizen(TRAIT traitType, Citizen citizen) {
    //     Trait createdTrait = null;
    //     switch (traitType) {
    //         case TRAIT.OPPORTUNIST:
    //             createdTrait = JsonUtility.FromJson<Opportunist>(traitDictionary[traitType]);
    //             break; 
    //         case TRAIT.DECEITFUL:
    //             createdTrait = JsonUtility.FromJson<Deceitful>(traitDictionary[traitType]);
    //             break; 
    //         case TRAIT.IMPERIALIST:
    //             createdTrait = JsonUtility.FromJson<Imperialist>(traitDictionary[traitType]);
    //             break; 
    //         case TRAIT.HOSTILE:
    //             createdTrait = JsonUtility.FromJson<Hostile>(traitDictionary[traitType]);
    //             break;
    //         case TRAIT.PACIFIST:
    //             createdTrait = JsonUtility.FromJson<Pacifist>(traitDictionary[traitType]);
    //             break;
    //         case TRAIT.SCHEMING:
    //             createdTrait = JsonUtility.FromJson<Scheming>(traitDictionary[traitType]);
    //             break;
    //         case TRAIT.DIPLOMATIC:
    //             createdTrait = JsonUtility.FromJson<Diplomatic>(traitDictionary[traitType]);
    //             break;
    //         case TRAIT.BENEVOLENT:
    //             createdTrait = JsonUtility.FromJson<Benevolent>(traitDictionary[traitType]);
    //             break;
    //case TRAIT.RUTHLESS:
    //	createdTrait = JsonUtility.FromJson<Ruthless>(traitDictionary[traitType]);
    //	break;
    //     }
    //     if(citizen != null && createdTrait != null) {
    //         createdTrait.AssignCitizen(citizen);
    //     }
    //     return createdTrait;
    // }
    internal Trait CreateNewTraitForCharacter(TRAIT traitType, ECS.Character character) {
        if(traitDictionary == null) {
            ConstructTraitDictionary();
        }
        Trait createdTrait = null;
        switch (traitType) {
            case TRAIT.IMPERIALIST:
                createdTrait = JsonUtility.FromJson<Imperialist>(traitDictionary[traitType]);
                break;
            case TRAIT.HOSTILE:
                createdTrait = JsonUtility.FromJson<Hostile>(traitDictionary[traitType]);
                break;
            case TRAIT.PACIFIST:
                createdTrait = JsonUtility.FromJson<Pacifist>(traitDictionary[traitType]);
                break;
            case TRAIT.SCHEMING:
                createdTrait = JsonUtility.FromJson<Scheming>(traitDictionary[traitType]);
                break;
            case TRAIT.OPPORTUNIST:
                createdTrait = JsonUtility.FromJson<Opportunist>(traitDictionary[traitType]);
                break;
            case TRAIT.EFFICIENT:
                createdTrait = JsonUtility.FromJson<Efficient>(traitDictionary[traitType]);
                break;
            case TRAIT.INEPT:
                createdTrait = JsonUtility.FromJson<Inept>(traitDictionary[traitType]);
                break;
            case TRAIT.MEDDLER:
                createdTrait = JsonUtility.FromJson<Meddler>(traitDictionary[traitType]);
                break;
            case TRAIT.SMART:
                createdTrait = JsonUtility.FromJson<Smart>(traitDictionary[traitType]);
                break;
            case TRAIT.DUMB:
                createdTrait = JsonUtility.FromJson<Dumb>(traitDictionary[traitType]);
                break;
            case TRAIT.CHARISMATIC:
                createdTrait = JsonUtility.FromJson<Charismatic>(traitDictionary[traitType]);
                break;
            case TRAIT.REPULSIVE:
                createdTrait = JsonUtility.FromJson<Repulsive>(traitDictionary[traitType]);
                break;
            case TRAIT.RUTHLESS:
                createdTrait = JsonUtility.FromJson<Ruthless>(traitDictionary[traitType]);
                break;
            case TRAIT.DECEITFUL:
                createdTrait = JsonUtility.FromJson<Deceitful>(traitDictionary[traitType]);
                break;
            case TRAIT.BENEVOLENT:
                createdTrait = JsonUtility.FromJson<Benevolent>(traitDictionary[traitType]);
                break;
            case TRAIT.DIPLOMATIC:
                createdTrait = JsonUtility.FromJson<Diplomatic>(traitDictionary[traitType]);
                break;
            case TRAIT.DEFENSIVE:
                createdTrait = JsonUtility.FromJson<Defensive>(traitDictionary[traitType]);
                break;
            case TRAIT.HONEST:
                createdTrait = JsonUtility.FromJson<Honest>(traitDictionary[traitType]);
                break;
            case TRAIT.RACIST:
                createdTrait = JsonUtility.FromJson<Racist>(traitDictionary[traitType]);
                break;
            case TRAIT.ROBUST:
                createdTrait = JsonUtility.FromJson<Robust>(traitDictionary[traitType]);
                break;
            case TRAIT.FRAGILE:
                createdTrait = JsonUtility.FromJson<Fragile>(traitDictionary[traitType]);
                break;
            case TRAIT.STRONG:
                createdTrait = JsonUtility.FromJson<Strong>(traitDictionary[traitType]);
                break;
            case TRAIT.WEAK:
                createdTrait = JsonUtility.FromJson<Weak>(traitDictionary[traitType]);
                break;
            case TRAIT.CLUMSY:
                createdTrait = JsonUtility.FromJson<Clumsy>(traitDictionary[traitType]);
                break;
            case TRAIT.AGILE:
                createdTrait = JsonUtility.FromJson<Agile>(traitDictionary[traitType]);
                break;
            default:
                break;
        }
        //if (character != null && createdTrait != null) {
        //    createdTrait.AssignCitizen(citizen);
        //}
        return createdTrait;
    }
    internal Trait GetTrait(TRAIT trait) {
        for (int i = 0; i < traitSetup.Count; i++) {
            Trait currTrait = traitSetup[i];
            if (currTrait.trait == trait) {
                return currTrait;
            }
        }
        return null;
    }

    public void ResetTraitSetup() {
        traitSetup.Clear();
        TRAIT[] allTraits = Utilities.GetEnumValues<TRAIT>();
        for (int i = 0; i < allTraits.Length; i++) {
            TRAIT currTrait = allTraits[i];
            string jsonStringOfTrait = GetJsonStringOfTrait(currTrait);
            if (!string.IsNullOrEmpty(jsonStringOfTrait)) {
                Trait traitFromFile = JsonUtility.FromJson<Trait>(jsonStringOfTrait);
                traitSetup.Add(traitFromFile);
            }
        }
    }
#if UNITY_EDITOR
    public void ApplyTraitSetup() {
        for (int i = 0; i < traitSetup.Count; i++) {
            Trait currTrait = traitSetup[i];
            SaveTraitJson(currTrait.traitName, currTrait);
        }
    }
    private void SaveTraitJson(string fileName, Trait traitSetup) {
        string path = Utilities.dataPath + "Traits/" + fileName + ".json";

        string jsonString = JsonUtility.ToJson(traitSetup);

        System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
        writer.WriteLine(jsonString);
        writer.Close();

        //Re-import the file to update the reference in the editor
        UnityEditor.AssetDatabase.ImportAsset(path);
        //TextAsset asset = Resources.Load("Data/Traits/" + fileName + ".json") as TextAsset;

        //Print the text from the file
        Debug.Log(GetJsonStringOfTrait(traitSetup.trait));
    }
#endif
    private string GetJsonStringOfTrait(TRAIT trait) {
        string path = Utilities.dataPath + "Traits/" + Utilities.NormalizeString(trait.ToString()) + ".json";
        string jsonString = string.Empty;
        try {
            //Read the text from directly from the test.txt file
            System.IO.StreamReader reader = new System.IO.StreamReader(path);
            jsonString = reader.ReadToEnd();
            reader.Close();
        } catch {
            //Do nothing
        }
        
        return jsonString;
    }
    internal void ConstructTraitDictionary() {
        traitDictionary = new Dictionary<TRAIT, string>();
        TRAIT[] allTraits = Utilities.GetEnumValues<TRAIT>();
        for (int i = 0; i < allTraits.Length; i++) {
            TRAIT currTrait = allTraits[i];
            string jsonStringOfTrait = GetJsonStringOfTrait(currTrait);
            if (!string.IsNullOrEmpty(jsonStringOfTrait)) {
                traitDictionary.Add(currTrait, jsonStringOfTrait);
            }
        }
    }
    #endregion

    #region Relationships
    /*
     Create a new relationship between 2 characters,
     then add add a reference to that relationship, to both of the characters.
         */
    public Relationship CreateNewRelationshipBetween(ECS.Character character1, ECS.Character character2) {
        Relationship newRel = new Relationship(character1, character2);
        character1.AddNewRelationship(character2, newRel);
        character2.AddNewRelationship(character1, newRel);
		return newRel;
    }
    /*
     Utility Function for getting the relationship between 2 characters,
     this just adds a checking for data consistency if, the 2 characters have the
     same reference to their relationship.
     NOTE: This is probably more performance intensive because of the additional checking.
     User can opt to use each characters GetRelationshipWith() instead.
         */
    public Relationship GetRelationshipBetween(ECS.Character character1, ECS.Character character2) {
        if(character1 == null || character2 == null) {
            return null;
        }
        Relationship char1Rel = character1.GetRelationshipWith(character2);
        Relationship char2Rel = character2.GetRelationshipWith(character1);
        if(char1Rel == char2Rel) {
            return char1Rel;
        }
        throw new System.Exception(character1.name + " does not have the same relationship object as " + character2.name + "!");
    }
    #endregion

	#region Prisoner Conversion
	public void SchedulePrisonerConversion(){
		GameDate newSched = GameManager.Instance.Today ();
		newSched.AddDays (7);
		SchedulingManager.Instance.AddEntry (newSched, () => PrisonerConversion ());
	}
	private void PrisonerConversion(){
		int allPrisonersWorldwide = FactionManager.Instance.allFactions.Sum (x => x.settlements.Sum (y => y.prisoners.Count));
		if(allPrisonersWorldwide > 0){
			int chance = UnityEngine.Random.Range (0, 100);
			float value = (float)allPrisonersWorldwide * 0.2f;

			if(chance < value){
				Faction faction = FactionManager.Instance.allFactions [UnityEngine.Random.Range (0, FactionManager.Instance.allFactions.Count)];
				Settlement settlement = faction.settlements [UnityEngine.Random.Range (0, faction.settlements.Count)];
				ECS.Character characterToBeConverted = settlement.prisoners [UnityEngine.Random.Range (0, settlement.prisoners.Count)];
				characterToBeConverted.ConvertToFaction ();
			}
		}

		SchedulePrisonerConversion ();
	}
    #endregion

    #region Utilities
    public void EquipCharacterWithBestGear(Settlement village, ECS.Character character) {
        //MATERIAL matForArmor = village.GetMaterialFor(PRODUCTION_TYPE.ARMOR);
        //MATERIAL matForWeapon = village.GetMaterialFor(PRODUCTION_TYPE.WEAPON);
        //EquipCharacterWithBestAvailableArmor(character, matForArmor, village);
        //EquipCharacterWithBestAvailableWeapon(character, matForWeapon, village);
    }
    private void EquipCharacterWithBestAvailableArmor(ECS.Character character, MATERIAL material, Settlement village) {
        foreach (ARMOR_TYPE armorType in ItemManager.Instance.armorTypeData.Keys) {
            TECHNOLOGY neededTech = Utilities.GetTechnologyForEquipment((EQUIPMENT_TYPE)armorType);
            if (village.HasTechnology(neededTech)) {
                string armorName = Utilities.NormalizeString(material.ToString()) + " " + Utilities.NormalizeString(armorType.ToString());
                int armorQuantityToCreate = 1;
                if (armorType == ARMOR_TYPE.BOOT || armorType == ARMOR_TYPE.BRACER) {
                    armorQuantityToCreate = 2; //Create a pair of boots or bracer
                }
                for (int i = 0; i < armorQuantityToCreate; i++) {
                    ECS.Item item = ItemManager.Instance.CreateNewItemInstance(armorName);
                    character.EquipItem(item);
                }
            }
        }
    }
    private void EquipCharacterWithBestAvailableWeapon(ECS.Character character, MATERIAL material, Settlement village) {
        for (int i = 0; i < character.characterClass.allowedWeaponTypes.Count; i++) {
            WEAPON_TYPE weaponType = character.characterClass.allowedWeaponTypes[i];
            TECHNOLOGY neededTech = Utilities.GetTechnologyForEquipment((EQUIPMENT_TYPE)weaponType);
            if (village.HasTechnology(neededTech)) {
                string weaponName = Utilities.NormalizeString(material.ToString()) + " " + Utilities.NormalizeString(weaponType.ToString());
                ECS.Item item = ItemManager.Instance.CreateNewItemInstance(weaponName);
                character.EquipItem(item);
                break;
            }
        }
    }
    public Character GetCharacterByID(int id) {
        for (int i = 0; i < allCharacters.Count; i++) {
            Character currChar = allCharacters[i];
            if (currChar.id == id) {
                return currChar;
            }
        }
        return null;
    }
    public Character GetCharacterByName(string name) {
        for (int i = 0; i < allCharacters.Count; i++) {
            Character currChar = allCharacters[i];
            if (currChar.name.Equals(name)) {
                return currChar;
            }
        }
        return null;
    }
    public void GenerateCharactersForTesting(int number) {
        List<Settlement> allOwnedSettlements = new List<Settlement>();
        for (int i = 0; i < FactionManager.Instance.allTribes.Count; i++) {
            allOwnedSettlements.AddRange(FactionManager.Instance.allTribes[i].settlements);
        }
        WeightedDictionary<CHARACTER_ROLE> characterRoleProductionDictionary = LandmarkManager.Instance.GetCharacterRoleProductionDictionary();

        for (int i = 0; i < number; i++) {
            Settlement chosenSettlement = allOwnedSettlements[Random.Range(0, allOwnedSettlements.Count)];
            //WeightedDictionary<CHARACTER_CLASS> characterClassProductionDictionary = LandmarkManager.Instance.GetCharacterClassProductionDictionary(chosenSettlement);

            //CHARACTER_CLASS chosenClass = characterClassProductionDictionary.PickRandomElementGivenWeights();
            CHARACTER_CLASS chosenClass = CHARACTER_CLASS.WARRIOR;
            CHARACTER_ROLE chosenRole = characterRoleProductionDictionary.PickRandomElementGivenWeights();
            ECS.Character newChar = chosenSettlement.CreateNewCharacter(RACE.HUMANS, chosenRole, Utilities.NormalizeString(chosenClass.ToString()), false);
            //Initial Character tags
            newChar.AssignInitialTags();
            CharacterManager.Instance.EquipCharacterWithBestGear(chosenSettlement, newChar);
        }
    }
    #endregion

	#region Avatars
	public Sprite GetSpriteByRole(CHARACTER_ROLE role){
		switch(role){
		case CHARACTER_ROLE.HERO:
			return heroSprite;
		case CHARACTER_ROLE.VILLAIN:
			return villainSprite;
		case CHARACTER_ROLE.HERMIT:
			return hermitSprite;
		case CHARACTER_ROLE.BEAST:
			return beastSprite;
		case CHARACTER_ROLE.BANDIT:
			return banditSprite;
		case CHARACTER_ROLE.CHIEFTAIN:
			return chieftainSprite;
		}
		return null;
	}
    #endregion

    #region Character Portraits
    public PortraitSettings GenerateRandomPortrait(GENDER gender) {
        PortraitSettings ps = new PortraitSettings();
        //ps.gender = gender;
        ps.eyesIndex = Random.Range(0, eyeSprites.Count);
        ps.eyeBrowIndex = Random.Range(0, eyeBrowSprites.Count);
        //List<HairSettings> hairsToUse = maleHairSettings;
        //if (gender == GENDER.FEMALE) {
        //    hairsToUse = femaleHairSettings;
        //}
        ps.hairIndex = Random.Range(0, hairSettings.Count);
        ps.noseIndex = Random.Range(0, noseSprites.Count);
        ps.mouthIndex = Random.Range(0, mouthSprites.Count);
        ps.hairColor = hairColors[Random.Range(0, hairColors.Count)];
        return ps;
    }
    #endregion

    #region Elements
    private void ConstructElementChanceDictionary() {
        _elementsChanceDictionary = new Dictionary<ELEMENT, float>();
        ELEMENT[] elements = (ELEMENT[]) System.Enum.GetValues(typeof(ELEMENT));
        for (int i = 0; i < elements.Length; i++) {
            _elementsChanceDictionary.Add(elements[i], 0f);
        }
    }
    #endregion
}
