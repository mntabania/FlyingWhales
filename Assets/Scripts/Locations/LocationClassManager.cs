using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LocationClassManager {
    public string[] characterClassOrder { get; private set; }
    public int currentIndex { get; private set; }
    private int startLoopIndex;
    private int numberOfRotations;
    private Dictionary<string, LocationClassNumberGuide> characterClassGuide;
    public Dictionary<string, int> combatantClasses { get; }
    public Dictionary<string, int> civilianClasses { get; }
    public LocationClassManager() {
        currentIndex = 0;
        startLoopIndex = 3;
        numberOfRotations = 0;
        combatantClasses = new Dictionary<string, int>();
        civilianClasses = new Dictionary<string, int>() {
            {"Peasant", 1},
            {"Crafter", 1},
            {"Miner", 1},
        };
            
        CreateCharacterClassOrderAndGuide();
    }
    public string GetCurrentClassToCreate() {
        return GetClassToCreate(currentIndex);
    }
    public void AddCombatantClass(string className) {
        if (combatantClasses.ContainsKey(className) == false) {
            combatantClasses.Add(className, 0);
        }
        combatantClasses[className] += 1;
    }
    private string GetClassToCreate(int index) {
        string currentClass = characterClassOrder[index];
        if (currentClass == "Combatant") {
            // List<CharacterClass> classes = CharacterManager.Instance.GetNormalCombatantClasses();
            // currentClass = classes[UnityEngine.Random.Range(0, classes.Count)].className;
            currentClass = UtilityScripts.CollectionUtilities.GetRandomElement(combatantClasses.Keys);
            // int i = UnityEngine.Random.Range(0, 3);
            // if (i == 0) {
            //     currentClass = "Mage";
            // } else if (i == 1) {
            //     currentClass = "Shaman";
            // } else {
            //     currentClass = "Druid";
            // }
        } else if (currentClass == "Civilian") {
            currentClass = UtilityScripts.CollectionUtilities.GetRandomElement(civilianClasses.Keys);
            // int i = UnityEngine.Random.Range(0, 3);
            // if (i == 0) {
            //     currentClass = "Miner";
            // } else if (i == 1) {
            //     currentClass = "Peasant";
            // } else {
            //     currentClass = "Crafter";
            // }
        }
        return currentClass;
    }
    private void CreateCharacterClassOrderAndGuide() {
        characterClassOrder = new string[] {
            //"Leader",
            "Crafter",
            "Peasant",
            "Combatant",
            //"Peasant",

            "Civilian",
            "Combatant",
            "Combatant",
            "Noble",
            "Combatant",
        };

        characterClassGuide = new Dictionary<string, LocationClassNumberGuide>() {
            //{ "Leader", new LocationClassNumberGuide() { supposedNumber = 0, currentNumber = 0, } },
            {"Peasant", new LocationClassNumberGuide() {supposedNumber = 0, currentNumber = 0,}},
            {"Combatant", new LocationClassNumberGuide() {supposedNumber = 0, currentNumber = 0,}},
            {"Crafter", new LocationClassNumberGuide() {supposedNumber = 0, currentNumber = 0,}},
            {"Civilian", new LocationClassNumberGuide() {supposedNumber = 0, currentNumber = 0,}},
            {"Noble", new LocationClassNumberGuide() {supposedNumber = 0, currentNumber = 0,}},
        };
    }
    public void OnAddResident(Character residentAdded) {
        string currentClassRequirement = characterClassOrder[currentIndex];

        //if (!DoesCharacterClassFitCurrentClass(residentAdded)) {
        //    throw new System.Exception(
        //        $"New resident {residentAdded.name}'s class which is {residentAdded.characterClass.className} does not match current location class requirement: {currentClassRequirement}");
        //}

        LocationClassNumberGuide temp = characterClassGuide[currentClassRequirement];
        temp.supposedNumber++;
        temp.currentNumber++;
        characterClassGuide[currentClassRequirement] = temp;

        currentIndex++;
        if (currentIndex >= characterClassOrder.Length) {
            currentIndex = startLoopIndex;
            numberOfRotations++;
        }
    }
    public void OnRemoveResident(Character residentRemoved) {
        return;
        string residentClassName = residentRemoved.characterClass.className;

        if (residentClassName == "Miner") {
            if (characterClassGuide["Civilian"].currentNumber > 0) {
                AdjustCurrentNumberOfClass("Civilian", -1);
            }
            else {
                throw new System.Exception(
                    $"Wrong location class requirement data! Removal of resident{residentClassName} {residentRemoved.name} but current number of Civilian is {characterClassGuide["Civilian"].currentNumber} (supposed number: {characterClassGuide["Civilian"].supposedNumber})");
            }
        }
        else if (residentClassName == "Peasant" || residentClassName == "Crafter") {
            if (characterClassGuide[residentClassName].currentNumber > 0) {
                AdjustCurrentNumberOfClass(residentClassName, -1);
            }
            else if (characterClassGuide["Civilian"].currentNumber > 0) {
                AdjustCurrentNumberOfClass("Civilian", -1);
            }
            else {
                throw new System.Exception(
                    $"Wrong location class requirement data! Removal of resident{residentClassName} {residentRemoved.name} but current number of Civilian is {characterClassGuide["Civilian"].currentNumber} (supposed number: {characterClassGuide["Civilian"].supposedNumber}) and current number of {residentClassName} is {characterClassGuide[residentClassName].currentNumber} (supposed number: {characterClassGuide[residentClassName].supposedNumber})");
            }
        }
        else if (residentClassName == "Noble") {
            if (characterClassGuide[residentClassName].currentNumber > 0) {
                AdjustCurrentNumberOfClass(residentClassName, -1);
            }
            // else if (characterClassGuide["Combatant"].currentNumber > 0) {
            //     AdjustCurrentNumberOfClass("Combatant", -1);
            // }
            else {
                throw new System.Exception(
                    $"Wrong location class requirement data! Removal of resident{residentClassName} {residentRemoved.name} but current number of Combatant is {characterClassGuide["Combatant"].currentNumber} (supposed number: {characterClassGuide["Combatant"].supposedNumber}) and current number of {residentClassName} is {characterClassGuide[residentClassName].currentNumber} (supposed number: {characterClassGuide[residentClassName].supposedNumber})");
            }
        }
        else if (residentRemoved.traitContainer.HasTrait("Combatant")) {
            if (characterClassGuide["Combatant"].currentNumber > 0) {
                AdjustCurrentNumberOfClass("Combatant", -1);
            }
            else {
                throw new System.Exception(
                    $"Wrong location class requirement data! Removal of resident{residentClassName} {residentRemoved.name} but current number of Combatant is {characterClassGuide["Combatant"].currentNumber} (supposed number: {characterClassGuide["Combatant"].supposedNumber})");
            }
        }
        //else {
        //    if (characterClassGuide[residentClassName].currentNumber > 0) {
        //        AdjustCurrentNumberOfClass(residentClassName, -1);
        //    } else {
        //        throw new System.Exception(
        //            $"Wrong location class requirement data! Removal of resident{residentClassName} {residentRemoved.name} but current number of {residentClassName} is {characterClassGuide[residentClassName].currentNumber} (supposed number: {characterClassGuide[residentClassName].supposedNumber})");
        //    }
        //}
        RevertCharacterClassOrderByOne();
    }
    public void OnResidentChangeClass(Character resident, CharacterClass previousClass, CharacterClass currentClass) {
        return;
        string previousClassName = previousClass.className;
        string currentClassName = currentClass.className;

        if (previousClassName == "Miner") {
            if (characterClassGuide["Civilian"].currentNumber > 0) {
                AdjustCurrentNumberOfClass("Civilian", -1);
            }
        }
        else if (previousClassName == "Peasant" || previousClassName == "Crafter") {
            if (characterClassGuide[previousClassName].currentNumber > 0) {
                AdjustCurrentNumberOfClass(previousClassName, -1);
            }
            else if (characterClassGuide["Civilian"].currentNumber > 0) {
                AdjustCurrentNumberOfClass("Civilian", -1);
            }
        }
        else if (previousClassName == "Noble") {
            if (characterClassGuide[previousClassName].currentNumber > 0) {
                AdjustCurrentNumberOfClass(previousClassName, -1);
            }
            // else if (characterClassGuide["Combatant"].currentNumber > 0) {
            //     AdjustCurrentNumberOfClass("Combatant", -1);
            // }
        }
        else if (previousClass.IsCombatant()) {
            if (characterClassGuide["Combatant"].currentNumber > 0) {
                AdjustCurrentNumberOfClass("Combatant", -1);
            }
        }
        //else {
        //    if (characterClassGuide[previousClassName].currentNumber > 0) {
        //        AdjustCurrentNumberOfClass(previousClassName, -1);
        //    }
        //}

        if (currentClassName == "Miner") {
            AdjustCurrentNumberOfClass("Civilian", 1);
        }
        else if (currentClassName == "Peasant" || currentClassName == "Crafter") {
            if (characterClassGuide[currentClassName].currentNumber <
                characterClassGuide[currentClassName].supposedNumber) {
                AdjustCurrentNumberOfClass(currentClassName, 1);
            }
            else {
                //if (characterClassGuide["Civilian"].currentNumber > 0) 
                AdjustCurrentNumberOfClass("Civilian", 1);
            }
        }
        else if (currentClassName == "Noble") {
            if (characterClassGuide[currentClassName].currentNumber <
                characterClassGuide[currentClassName].supposedNumber) {
                AdjustCurrentNumberOfClass(currentClassName, 1);
            }
            // else { // if (characterClassGuide["Combatant"].currentNumber > 0)
            //     AdjustCurrentNumberOfClass("Combatant", 1);
            // }
        }
        else if (currentClass.IsCombatant()) {
            AdjustCurrentNumberOfClass("Combatant", 1);
            //if (characterClassGuide["Combatant"].currentNumber > 0) {
            //    AdjustCurrentNumberOfClass("Combatant", 1);
            //}
        }
        //else {
        //    AdjustCurrentNumberOfClass(currentClassName, 1);
        //    //if (characterClassGuide[previousClassName].currentNumber > 0) {
        //    //    AdjustCurrentNumberOfClass(previousClassName, -1);
        //    //}
        //}
    }
    private void RevertCharacterClassOrderByOne() {
        string currentClassIdentifier = characterClassOrder[currentIndex];
        if (currentIndex >= startLoopIndex) {
            currentIndex--;
            if (numberOfRotations > 0 && currentIndex < startLoopIndex) {
                currentIndex = characterClassOrder.Length - 1;
                numberOfRotations--;
            }
        }
        else {
            currentIndex--;
            if (currentIndex < 0) {
                currentIndex = 0;
                Debug.LogWarning("Wrong data! Current index cannot be less than zero");
            }
        }
        AdjustSupposedNumberOfClass(currentClassIdentifier, -1);
    }
    private void AdjustCurrentNumberOfClass(string classIdentifier, int amount) {
        LocationClassNumberGuide temp = characterClassGuide[classIdentifier];
        temp.currentNumber += amount;
        characterClassGuide[classIdentifier] = temp;
    }
    private void AdjustSupposedNumberOfClass(string classIdentifier, int amount) {
        LocationClassNumberGuide temp = characterClassGuide[classIdentifier];
        temp.supposedNumber += amount;
        characterClassGuide[classIdentifier] = temp;
    }
}

public struct LocationClassNumberGuide {
    public int supposedNumber;
    public int currentNumber;
}