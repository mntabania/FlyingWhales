using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationClassManager {
    public string[] characterClassOrder { get; private set; }
    public int currentIndex { get;  private set; }

    private int startLoopIndex;
    private int numberOfRotations;
    private Dictionary<string, LocationClassNumberGuide> characterClassGuide;

    public LocationClassManager() {
        currentIndex = 0;
        startLoopIndex = 5;
        numberOfRotations = 0;
        CreateCharacterClassOrderAndGuide();
    }

    public string GetCurrentClassToCreate() {
        return GetClassToCreate(currentIndex);
    }
    public string GetNextClassToCreate() {
        int nextIndex = currentIndex + 1;
        if (nextIndex >= characterClassOrder.Length) {
            nextIndex = startLoopIndex;
        }
        return GetClassToCreate(nextIndex);
    }
    private string GetClassToCreate(int index) {
        string currentClass = characterClassOrder[index];
        if (currentClass == "Combatant") {
            List<CharacterClass> classes = CharacterManager.Instance.GetNormalCombatantClasses();
            currentClass = classes[UnityEngine.Random.Range(0, classes.Count)].className;
        } else if (currentClass == "Civilian") {
            int i = UnityEngine.Random.Range(0, 3);
            if (i == 0) {
                currentClass = "Miner";
            } else if (i == 0) {
                currentClass = "Peasant";
            } else {
                currentClass = "Craftsman";
            }
        }
        return currentClass;
    }

    private void CreateCharacterClassOrderAndGuide() {
        characterClassOrder = new string[] {
            //"Leader",
            "Craftsman",
            "Peasant",
            "Combatant",
            "Peasant",

            "Civilian",
            "Combatant",
            "Combatant",
            "Noble",
            "Combatant",
        };

        characterClassGuide = new Dictionary<string, LocationClassNumberGuide>() {
            //{ "Leader", new LocationClassNumberGuide() { supposedNumber = 0, currentNumber = 0, } },
            { "Peasant", new LocationClassNumberGuide() { supposedNumber = 0, currentNumber = 0, } },
            { "Combatant", new LocationClassNumberGuide() { supposedNumber = 0, currentNumber = 0, }},
            { "Craftsman", new LocationClassNumberGuide() { supposedNumber = 0, currentNumber = 0, }},
            { "Civilian", new LocationClassNumberGuide() { supposedNumber = 0, currentNumber = 0, }},
            { "Noble", new LocationClassNumberGuide() { supposedNumber = 0, currentNumber = 0, }},
        };
    }

    public void OnAddResident(Character residentAdded) {
        string currentClassRequirement = characterClassOrder[currentIndex];

        if (!DoesCharacterClassFitCurrentClass(residentAdded)) {
            throw new System.Exception(
                $"New resident {residentAdded.name}'s class which is {residentAdded.characterClass.className} does not match current location class requirement: {currentClassRequirement}");
        }

        LocationClassNumberGuide temp = characterClassGuide[currentClassRequirement];
        temp.supposedNumber++;
        temp.currentNumber++;
        characterClassGuide[currentClassRequirement] = temp;

        currentIndex++;
        if(currentIndex >= characterClassOrder.Length) {
            currentIndex = startLoopIndex;
            numberOfRotations++;
        }
    }
    public void OnRemoveResident(Character residentRemoved) {
        string residentClassName = residentRemoved.characterClass.className;

        if(residentClassName == "Miner") {
            if(characterClassGuide["Civilian"].currentNumber > 0) {
                AdjustCurrentNumberOfClass("Civilian", -1);
            } else {
                throw new System.Exception(
                    $"Wrong location class requirement data! Removal of resident{residentClassName} {residentRemoved.name} but current number of Civilian is {characterClassGuide["Civilian"].currentNumber} (supposed number: {characterClassGuide["Civilian"].supposedNumber})");
            }
        } else if(residentClassName == "Peasant" || residentClassName == "Craftsman") {
            if (characterClassGuide[residentClassName].currentNumber > 0) {
                AdjustCurrentNumberOfClass(residentClassName, -1);
            } else if (characterClassGuide["Civilian"].currentNumber > 0) {
                AdjustCurrentNumberOfClass("Civilian", -1);
            } else {
                throw new System.Exception(
                    $"Wrong location class requirement data! Removal of resident{residentClassName} {residentRemoved.name} but current number of Civilian is {characterClassGuide["Civilian"].currentNumber} (supposed number: {characterClassGuide["Civilian"].supposedNumber}) and current number of {residentClassName} is {characterClassGuide[residentClassName].currentNumber} (supposed number: {characterClassGuide[residentClassName].supposedNumber})");
            }
        } else if (residentClassName == "Noble") {
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
        } else if (residentRemoved.traitContainer.HasTrait("Combatant")) {
            if (characterClassGuide["Combatant"].currentNumber > 0) {
                AdjustCurrentNumberOfClass("Combatant", -1);
            } else {
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
        string previousClassName = previousClass.className;
        string currentClassName = currentClass.className;

        if (previousClassName == "Miner") {
            if (characterClassGuide["Civilian"].currentNumber > 0) {
                AdjustCurrentNumberOfClass("Civilian", -1);
            }
        } else if (previousClassName == "Peasant" || previousClassName == "Craftsman") {
            if (characterClassGuide[previousClassName].currentNumber > 0) {
                AdjustCurrentNumberOfClass(previousClassName, -1);
            } else if (characterClassGuide["Civilian"].currentNumber > 0) {
                AdjustCurrentNumberOfClass("Civilian", -1);
            } 
        } else if (previousClassName == "Noble") {
            if (characterClassGuide[previousClassName].currentNumber > 0) {
                AdjustCurrentNumberOfClass(previousClassName, -1);
            }
            // else if (characterClassGuide["Combatant"].currentNumber > 0) {
            //     AdjustCurrentNumberOfClass("Combatant", -1);
            // }
        } else if (previousClass.IsCombatant()) {
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
        } else if (currentClassName == "Peasant" || currentClassName == "Craftsman") {
            if (characterClassGuide[currentClassName].currentNumber < characterClassGuide[currentClassName].supposedNumber) {
                AdjustCurrentNumberOfClass(currentClassName, 1);
            } else { //if (characterClassGuide["Civilian"].currentNumber > 0) 
                AdjustCurrentNumberOfClass("Civilian", 1);
            }
        } else if (currentClassName == "Noble") {
            if (characterClassGuide[currentClassName].currentNumber < characterClassGuide[currentClassName].supposedNumber) {
                AdjustCurrentNumberOfClass(currentClassName, 1);
            } 
            // else { // if (characterClassGuide["Combatant"].currentNumber > 0)
            //     AdjustCurrentNumberOfClass("Combatant", 1);
            // }
        } else if (currentClass.IsCombatant()) {
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
        if(currentIndex >= startLoopIndex) {
            currentIndex--;
            if(numberOfRotations > 0 && currentIndex < startLoopIndex) {
                currentIndex = characterClassOrder.Length - 1;
                numberOfRotations--;
            }
        } else {
            currentIndex--;
            if(currentIndex < 0) {
                throw new System.Exception("Wrong data! Current index cannot be less than zero");
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
    private bool DoesCharacterClassFitCurrentClass(Character character) {
        string className = characterClassOrder[currentIndex];
        if(className == "Combatant") {
            return character.traitContainer.HasTrait("Combatant");
        }else if (className == "Civilian") {
            return character.characterClass.className == "Miner" || character.characterClass.className == "Peasant" || character.characterClass.className == "Craftsman";
        } else {
            return character.characterClass.className == className;
        }
    }
    public void LogLocationRequirementsData(string regionName) {
        string log = $"Location Character Class Requirements Data For {regionName}";
        foreach (KeyValuePair<string, LocationClassNumberGuide> kvp in characterClassGuide) {
            log +=
                $"\n{kvp.Key} - Supposed Number: {kvp.Value.supposedNumber}, Current Number: {kvp.Value.currentNumber}"; 
        }
        Debug.Log(log);
    }
    public bool IsClassASurplus(CharacterClass charClass) {
        string className = charClass.className;

        if (className == "Miner") {
            LocationClassNumberGuide numberGuide = characterClassGuide["Civilian"];
            return numberGuide.currentNumber > numberGuide.supposedNumber;
        } else if (className == "Peasant" || className == "Craftsman") {
            LocationClassNumberGuide numberGuide = characterClassGuide[className];
            if (numberGuide.currentNumber > numberGuide.supposedNumber) {
                return true;
            } else {
                numberGuide = characterClassGuide["Civilian"];
                if (numberGuide.currentNumber > numberGuide.supposedNumber) {
                    return true;
                }
            }
        } else if (className == "Noble") {
            LocationClassNumberGuide numberGuide = characterClassGuide[className];
            if (numberGuide.currentNumber > numberGuide.supposedNumber) {
                return true;
            } 
            // else {
            //     numberGuide = characterClassGuide["Combatant"];
            //     if (numberGuide.currentNumber > numberGuide.supposedNumber) {
            //         return true;
            //     }
            // }
        } else if (charClass.IsCombatant()) {
            LocationClassNumberGuide numberGuide = characterClassGuide["Combatant"];
            return numberGuide.currentNumber > numberGuide.supposedNumber;
        } 
        //else {
        //    LocationClassNumberGuide numberGuide = characterClassGuide[className];
        //    return numberGuide.currentNumber > numberGuide.supposedNumber;
        //}
        return false;
    }

    public bool IsClassADeficit(CharacterClass charClass) {
        string className = charClass.className;

        if (className == "Miner") {
            LocationClassNumberGuide numberGuide = characterClassGuide["Civilian"];
            return numberGuide.currentNumber < numberGuide.supposedNumber;
        } else if (className == "Peasant" || className == "Craftsman") {
            LocationClassNumberGuide numberGuide = characterClassGuide[className];
            if (numberGuide.currentNumber < numberGuide.supposedNumber) {
                return true;
            } else {
                numberGuide = characterClassGuide["Civilian"];
                if (numberGuide.currentNumber < numberGuide.supposedNumber) {
                    return true;
                }
            }
        } else if (className == "Noble") {
            LocationClassNumberGuide numberGuide = characterClassGuide[className];
            if (numberGuide.currentNumber < numberGuide.supposedNumber) {
                return true;
            } 
            // else {
            //     numberGuide = characterClassGuide["Combatant"];
            //     if (numberGuide.currentNumber < numberGuide.supposedNumber) {
            //         return true;
            //     }
            // }
        } else if (charClass.IsCombatant()) {
            LocationClassNumberGuide numberGuide = characterClassGuide["Combatant"];
            return numberGuide.currentNumber < numberGuide.supposedNumber;
        } 
        //else {
        //    LocationClassNumberGuide numberGuide = characterClassGuide[className];
        //    return numberGuide.currentNumber < numberGuide.supposedNumber;
        //}
        return false;
    }
}

public struct LocationClassNumberGuide {
    public int supposedNumber;
    public int currentNumber;
}