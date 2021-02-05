using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilityScripts;
namespace Inner_Maps.Location_Structures {
    public class Dwelling : ManMadeStructure {

        //public List<Character> residents { get; private set; }

        #region getters
        public override bool isDwelling => true;
        #endregion

        //facilities

        public Dwelling(Region location) : base(STRUCTURE_TYPE.DWELLING, location) {
            //residents = new List<Character>();
            maxResidentCapacity = 2;
            SetMaxHPAndReset(3500);
        }

        public Dwelling(Region location, SaveDataManMadeStructure data) : base(location, data) {
            //residents = new List<Character>();
            maxResidentCapacity = 2;
            SetMaxHP(3500);
        }

        #region Overrides
        protected override void OnAddResident(Character newResident) {
            base.OnAddResident(newResident);
            if (GameManager.Instance.gameHasStarted) {
                //only Update owners on add residency if resident is not from initial generation.
                ProcessAllTileObjects(t => {
                    if (t.isPreplaced) {
                        t.UpdateOwners();
                    }
                });
                //List<TileObject> objs = ProcessAllTileObjects();
                //for (int i = 0; i < objs.Count; i++) {
                //    TileObject obj = objs[i];
                //    if (obj.isPreplaced) {
                //        //only update owners of objects that were preplaced
                //        obj.UpdateOwners();    
                //    }
                //}    
            }
        }

        //Removed this because the unowning of items is in the virtual function now
        //protected override void OnRemoveResident(Character newResident) {
        //    base.OnRemoveResident(newResident);
        //    List<TileObject> objs = GetTileObjects();
        //    for (int i = 0; i < objs.Count; i++) {
        //        TileObject obj = objs[i];
        //        if (obj.isPreplaced) {
        //            //only update owners of objects that were preplaced
        //            obj.UpdateOwners();
        //        }
        //    }
        //}
        public override bool CanBeResidentHere(Character character) {
            if (residents.Count == 0) {
                return true;
            } else {
                for (int i = 0; i < residents.Count; i++) {
                    Character currResident = residents[i];
                    List<RELATIONSHIP_TYPE> rels = currResident.relationshipContainer.GetRelationshipDataWith(character)?.relationships ?? null;
                    if (rels != null && rels.Contains(RELATIONSHIP_TYPE.LOVER)) {
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion

        #region Misc
        public override string GetNameRelativeTo(Character character) {
            if (character.homeStructure == this) {
                //- Dwelling where Actor Resides: "at [his/her] home"
                return $"{UtilityScripts.Utilities.GetPronounString(character.gender, PRONOUN_TYPE.POSSESSIVE, false)} home";
            } else if (residents.Count > 0) {
                //- Dwelling where Someone else Resides: "at [Resident Name]'s home"
                string residentSummary = residents[0].name;
                for (int i = 1; i < residents.Count; i++) {
                    if (i + 1 == residents.Count) {
                        residentSummary = $"{residentSummary} and ";
                    } else {
                        residentSummary = $"{residentSummary}, ";
                    }
                    residentSummary = $"{residentSummary}{residents[i].name}";
                }
                if (residentSummary.Last() == 's') {
                    return $"{residentSummary}' home";
                }
                return $"{residentSummary}'s home";
            } else {
                //- Dwelling where no one resides: "at an empty house"
                return "an empty house";
            }
        }
        #endregion
    }
}