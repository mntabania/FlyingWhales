using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class NewResidentEvent : LocationEvent {

	public NewResidentEvent() {
        name = "New Resident Event";
        triggerTick = 96;
        triggerChance = 35;
        triggerCondition = Condition;
    }

    private bool Condition(NPCSettlement location) {
        return false;
        //TODO:
        // return !location.region.coreTile.isCorrupted && !location.IsResidentsFull();
    }

    #region Overrides
    public override void TriggerEvent(NPCSettlement location) {
        base.TriggerEvent(location);
        //List<LocationStructure> structures = location.structures[STRUCTURE_TYPE.DWELLING];
        //int numberOfUnoccupiedDwellings = 0;
        //for (int i = 0; i < structures.Count; i++) {
        //    if (!structures[i].IsOccupied()) {
        //        numberOfUnoccupiedDwellings++;
        //    }
        //}
        int numberOfUnoccupiedDwellings = location.GetNumberOfUnoccupiedStructure(STRUCTURE_TYPE.DWELLING);
        int numOfSoonToBeOccupiedDwellings = UnityEngine.Random.Range(1, numberOfUnoccupiedDwellings + 1);
        int maxCouple = 2;
        int currentCouple = 0;
        for (int i = 0; i < numOfSoonToBeOccupiedDwellings; i++) {
            if (currentCouple >= maxCouple) {
                GenerateSingleResident(location);
            } else {
                //GenerateCoupleResidents(location);

                if (UnityEngine.Random.Range(0, 2) == 0) {
                    GenerateCoupleResidents(location);
                    currentCouple++;
                } else {
                    GenerateSingleResident(location);
                }
            }
        }
        PlayerUI.Instance.ShowGeneralConfirmation("New Residents", $"New residents have arrived at {location.name}");
    }
    #endregion
    private void GenerateSingleResident(NPCSettlement location) {
        RACE race = GetRaceForNewResident(location);
        Character newResident = AddNewResident(location, race, location.owner);
        Debug.Log(
            $"{GameManager.Instance.TodayLogString()}Generated new Single Resident {newResident} from New Resident Event");
        //CharacterManager.Instance.CreateNewCharacter(CharacterRole.SOLDIER, race, Utilities.GetRandomGender(), location.region.owner, location.region);
    }
    private void GenerateCoupleResidents(NPCSettlement location) {
        RACE race = GetRaceForNewResident(location);
        //string className = location.locationClassManager.GetCurrentClassToCreate();
        Character spouse1 = AddNewResident(location, race, location.owner);

        race = GetRaceForNewResident(location);
        SEXUALITY sexuality = UtilityScripts.Utilities.GetCompatibleSexuality(spouse1.sexuality);
        GENDER gender = UtilityScripts.Utilities.GetOppositeGender(spouse1.gender);
        if(spouse1.sexuality == SEXUALITY.BISEXUAL) {
            if(sexuality == SEXUALITY.GAY) {
                gender = spouse1.gender;
            }else if (sexuality == SEXUALITY.BISEXUAL) {
                if(UnityEngine.Random.Range(0, 2) == 0) {
                    gender = spouse1.gender;
                }
            }
        } else if (spouse1.sexuality == SEXUALITY.GAY) {
            if (sexuality == SEXUALITY.GAY) {
                gender = spouse1.gender;
            }
        }
        //className = location.locationClassManager.GetNextClassToCreate();
        Character spouse2 = AddNewResident(location, race, gender, sexuality, location.owner);

        RelationshipManager.Instance.CreateNewRelationshipBetween(spouse1, spouse2, RELATIONSHIP_TYPE.LOVER);

        //if (location.region != null) {
        //    spouse1.ownParty.icon.SetPosition(location.region.coreTile.transform.position);
        //    spouse1.MigrateHomeTo(location.region, null, false);
        //    location.region.AddCharacterToLocation(spouse1);

        //    spouse2.ownParty.icon.SetPosition(location.region.coreTile.transform.position);
        //    spouse2.MigrateHomeTo(location.region, null, false);
        //    location.region.AddCharacterToLocation(spouse2);
        //}

        //location.PlaceNewResidentInInnerMap(spouse1);
        //location.PlaceNewResidentInInnerMap(spouse2);

        Debug.Log(
            $"{GameManager.Instance.TodayLogString()}Generated new Couple Resident {spouse1} and {spouse2} from New Resident Event");
    }
    private RACE GetRaceForNewResident(NPCSettlement location) {
        if(location.owner != null && location.owner.leader != null) {
            int chance = UnityEngine.Random.Range(0, 100);
            if(chance < 75) {
                return location.owner.leader.race;
            }
        }
        return UnityEngine.Random.Range(0, 2) == 0 ? RACE.HUMANS : RACE.ELVES;
    }
    
    public Character AddNewResident(NPCSettlement location, RACE race, Faction faction) {
        string className = location.classManager.GetCurrentClassToCreate();
        Character citizen = CharacterManager.Instance.CreateNewCharacter(className, race, UtilityScripts.Utilities.GetRandomGender(), faction, location);
        // PlaceNewResidentInInnerMap(citizen);
        return citizen;
    }
    public Character AddNewResident(NPCSettlement location, RACE race, GENDER gender, SEXUALITY sexuality, Faction faction) {
        string className = location.classManager.GetCurrentClassToCreate();
        Character citizen = CharacterManager.Instance.CreateNewCharacter(className, race, gender, sexuality, faction, location);
        // PlaceNewResidentInInnerMap(citizen);
        return citizen;
    }
    // public void PlaceNewResidentInInnerMap(Character newResident) {
    //     LocationGridTile mainEntrance = innerMap.GetRandomUnoccupiedEdgeTile();
    //     newResident.CreateMarker();
    //     newResident.InitialCharacterPlacement(mainEntrance);
    // }
}
