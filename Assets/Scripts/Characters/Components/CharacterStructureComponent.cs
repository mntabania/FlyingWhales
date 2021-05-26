using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;

public class CharacterStructureComponent : CharacterComponent {
    private const string Dwelling = "Dwelling";
    private const string Survival_Structures = "Survival Structures"; //Hunter's Lodge/Apothecary/Mage Quarters
    private const string Utility_Structures = "Utility Structures"; //Inn/Warehouse/Cemetery/Prison/Granary/Miner's Camp
    private const string Combat_Structures = "Combat Structures"; //Smithy/Barracks/Raider's Camp/Assassin's Guild

    //private const string Human_Survival_Structures = "Human Survival Structures"; //Warehouse/Cemetery/Prison/Smithy/Barracks/Apothecary
    //private const string Human_Utility_Structures = "Human Utility Structures"; //Granary/Miner's Camp/Inn
    //private const string Human_Combat_Structures = "Human Combat Structures"; //Raider's Camp/Assassin's Guild/Hunter's Lodge/Mage Quarters
    public int currentIndex { get; private set; }

    public int startLoopIndex { get; private set; }

    private List<STRUCTURE_TYPE> survivalStructures;
    private List<STRUCTURE_TYPE> utilityStructures;
    private List<STRUCTURE_TYPE> combatStructures;
    //private Dictionary<string, BuildStructureRequirementNumberGuide> structureRequirementNumberGuide;
    private List<STRUCTURE_TYPE> missingStructures;
    private List<string> buildStructureOrder;

    public ManMadeStructure workPlaceStructure { get; private set; } //Do not save this because the loading of this is handled in ManMadeStructure - LoadReferences

    public CharacterStructureComponent() {
        startLoopIndex = 0;
        currentIndex = 0;
        survivalStructures = new List<STRUCTURE_TYPE>();
        utilityStructures = new List<STRUCTURE_TYPE>();
        combatStructures = new List<STRUCTURE_TYPE>();
        missingStructures = new List<STRUCTURE_TYPE>();
    }
    public CharacterStructureComponent(SaveDataCharacterStructureComponent data) {
        currentIndex = data.currentIndex;
        startLoopIndex = data.startLoopIndex;
        survivalStructures = new List<STRUCTURE_TYPE>();
        utilityStructures = new List<STRUCTURE_TYPE>();
        combatStructures = new List<STRUCTURE_TYPE>();
        missingStructures = new List<STRUCTURE_TYPE>();
    }
    public override void SetOwner(Character owner) {
        base.SetOwner(owner);
        if(this.owner != null) {
            ResetCategorizedStructures("Survival");
            ResetCategorizedStructures("Utility");
            ResetCategorizedStructures("Combat");
            AssignBuildOrder();
        }
    }

    #region General
    public STRUCTURE_TYPE GetCurrentStructureToBuild() {
        if(missingStructures.Count > 0) {
            return missingStructures[0];
        }
        return GetStructureToBuild(currentIndex);
    }
    private STRUCTURE_TYPE GetStructureToBuild (int index) {
        STRUCTURE_TYPE structureToBuild = STRUCTURE_TYPE.NONE;
        while(structureToBuild == STRUCTURE_TYPE.NONE) {
            string structureCategory = buildStructureOrder[index];
            if (structureCategory == Survival_Structures) {
                if (survivalStructures.Count > 0) {
                    structureToBuild = survivalStructures[UnityEngine.Random.Range(0, survivalStructures.Count)];
                } else {
                    buildStructureOrder.RemoveAt(index);
                }
            } else if (structureCategory == Utility_Structures) {
                if (utilityStructures.Count > 0) {
                    structureToBuild = utilityStructures[UnityEngine.Random.Range(0, utilityStructures.Count)];
                } else {
                    buildStructureOrder.RemoveAt(index);
                }
            } else if (structureCategory == Combat_Structures) {
                if (combatStructures.Count > 0) {
                    structureToBuild = combatStructures[UnityEngine.Random.Range(0, combatStructures.Count)];
                } else {
                    buildStructureOrder.RemoveAt(index);
                }
            } else if (structureCategory == Dwelling) {
                structureToBuild = STRUCTURE_TYPE.DWELLING;
            }
        }
        return structureToBuild;
    }
    public void OnCreateBlueprint(STRUCTURE_TYPE blueprintType) {
        if(missingStructures.Count > 0 && missingStructures[0] == blueprintType) {
            missingStructures.RemoveAt(0);
            return;
        }
        if (survivalStructures.Contains(blueprintType)) {
            survivalStructures.Remove(blueprintType);
        }else if(utilityStructures.Contains(blueprintType)) {
            utilityStructures.Remove(blueprintType);
        }else if (combatStructures.Contains(blueprintType)) {
            combatStructures.Remove(blueprintType);
        }
        currentIndex++;
        if (currentIndex >= buildStructureOrder.Count) {
            currentIndex = startLoopIndex;
        }
    }
    public void OnDestroyStructure(STRUCTURE_TYPE destroyedStructureType) {
        missingStructures.Add(destroyedStructureType);
    }
    public void SetWorkPlaceStructure(ManMadeStructure p_structure) {
        workPlaceStructure = p_structure;
    }
    public bool HasWorkPlaceStructure() {
        return workPlaceStructure != null;
    }
    #endregion

    #region Categorized Structures
    private void ResetCategorizedStructures(string category) {
        STRUCTURE_TYPE[] referenceArray = LandmarkManager.Instance.GetRaceStructureRequirements(owner.race, category);
        List<STRUCTURE_TYPE> usedList = null;
        if(category == "Survival") {
            usedList = survivalStructures;
        } else if (category == "Utility") {
            usedList = utilityStructures;
        } else if (category == "Combat") {
            usedList = combatStructures;
        }
        for (int i = 0; i < referenceArray.Length; i++) {
            usedList.Add(referenceArray[i]);
        }
    }
    #endregion

    #region Build Orders
    private void AssignBuildOrder() {
        if(owner.race == RACE.ELVES) {
            buildStructureOrder = ElfBuildOrder();
        } else if (owner.race == RACE.HUMANS) {
            buildStructureOrder = HumanBuildOrder();
        } else {
            buildStructureOrder = HumanBuildOrder();
        }
        //structureRequirementNumberGuide = new Dictionary<string, BuildStructureRequirementNumberGuide>() {
        //    { Dwelling, new BuildStructureRequirementNumberGuide() { supposedNumber = 0, currentNumber = 0, } },
        //    { Survival_Structures, new BuildStructureRequirementNumberGuide() { supposedNumber = 0, currentNumber = 0, } },
        //    { Utility_Structures, new BuildStructureRequirementNumberGuide() { supposedNumber = 0, currentNumber = 0, }},
        //    { Combat_Structures, new BuildStructureRequirementNumberGuide() { supposedNumber = 0, currentNumber = 0, }},
        //};
    }
    private List<string> ElfBuildOrder() {
        return new List<string> {
            //Dwelling,
            //Dwelling,
            //Dwelling,
            Survival_Structures,
            Dwelling,
            Utility_Structures,
            Dwelling,
            Survival_Structures,
            Dwelling,
            Combat_Structures,
            Dwelling,
            Utility_Structures,
            Dwelling,
        };
    }
    private List<string> HumanBuildOrder() {
        return new List<string> {
            //Dwelling,
            //Dwelling,
            //Dwelling,
            Utility_Structures,
            Dwelling,
            Survival_Structures,
            Dwelling,
            Dwelling,
            Combat_Structures,
            Dwelling,
            Dwelling,
        };
    }
    #endregion

    #region Loading
    public void LoadReferences(SaveDataCharacterStructureComponent data) {
        //Currently N/A
    }
    #endregion
}

[System.Serializable]
public class SaveDataCharacterStructureComponent : SaveData<CharacterStructureComponent> {
    public int currentIndex;
    public int startLoopIndex;

    #region Overrides
    public override void Save(CharacterStructureComponent data) {
        currentIndex = data.currentIndex;
        startLoopIndex = data.startLoopIndex;
    }

    public override CharacterStructureComponent Load() {
        CharacterStructureComponent component = new CharacterStructureComponent(this);
        return component;
    }
    #endregion
}