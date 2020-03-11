//using System.Collections;
//using System.Collections.Generic;
//using Traits;
//using UnityEngine;

//[System.Serializable]
//public class SaveDataAlterEgo {
//    public string name;
//    public int factionID;
//    public RACE race;
//    public CHARACTER_ROLE roleType;
//    public string characterClassName;
//    public int level;
//    public int attackPowerMod;
//    public int speedMod;
//    public int maxHPMod;
//    public int attackPowerPercentMod;
//    public int speedPercentMod;
//    public int maxHPPercentMod;

//    //Relationships
//    public List<SaveDataRelationship> relationships;

//    //Traits
//    public List<SaveDataTrait> traits;

//    public void Save(AlterEgoData alterEgo) {
//        name = alterEgo.name;
//        if (alterEgo.faction != null) {
//            factionID = alterEgo.faction.id;
//        } else {
//            factionID = -1;
//        }
//        race = alterEgo.race;
//        roleType = alterEgo.role.roleType;
//        characterClassName = alterEgo.characterClass.className;
//        level = alterEgo.level;
//        attackPowerMod = alterEgo.attackPowerMod;
//        speedMod = alterEgo.speedMod;
//        maxHPMod = alterEgo.maxHPMod;
//        attackPowerPercentMod = alterEgo.attackPowerPercentMod;
//        speedPercentMod = alterEgo.speedPercentMod;
//        maxHPPercentMod = alterEgo.maxHPPercentMod;


//        traits = new List<SaveDataTrait>();
//        if (alterEgo.traits != null) {
//            for (int i = 0; i < alterEgo.traits.Count; i++) {
//                Trait trait = alterEgo.traits[i];
//                SaveDataTrait saveDataTrait = SaveManager.ConvertTraitToSaveDataTrait(trait);
//                if (saveDataTrait != null) {
//                    saveDataTrait.Save(trait);
//                    traits.Add(saveDataTrait);
//                }
//            }
//        }

//        relationships = new List<SaveDataRelationship>();
//        //foreach (CharacterRelationshipData relData in alterEgo.relationshipContainer.relationships.Values) {
//        //    SaveDataRelationship saveDataRelationship = new SaveDataRelationship();
//        //    saveDataRelationship.Save(relData);
//        //    relationships.Add(saveDataRelationship);
//        //}
//    }

//    public void Load(Character character) {
//        AlterEgoData alterEgoData = character.CreateNewAlterEgo(name);
//        if(factionID != -1) {
//            alterEgoData.SetFaction(FactionManager.Instance.GetFactionBasedOnID(factionID));
//        }
//        alterEgoData.SetRace(race);
//        alterEgoData.SetRole(CharacterManager.Instance.GetRoleByRoleType(roleType));
//        alterEgoData.SetCharacterClass(CharacterManager.Instance.CreateNewCharacterClass(characterClassName));
//        alterEgoData.SetLevel(level);
//        alterEgoData.SetAttackPowerMod(attackPowerMod);
//        alterEgoData.SetAttackPowerPercentMod(attackPowerPercentMod);
//        alterEgoData.SetMaxHPMod(maxHPMod);
//        alterEgoData.SetMaxHPPercentMod(maxHPPercentMod);
//        alterEgoData.SetSpeedMod(speedMod);
//        alterEgoData.SetSpeedPercentMod(speedPercentMod);

//        for (int i = 0; i < traits.Count; i++) {
//            Trait trait = traits[i].Load();
//            alterEgoData.AddTrait(trait);
//        }
//    }
//    public void LoadRelationships(Character character) {
//        AlterEgoData alterEgoData = character.GetAlterEgoData(name);
//        for (int i = 0; i < relationships.Count; i++) {
//            relationships[i].Load(alterEgoData);
//        }
//    }
//}
