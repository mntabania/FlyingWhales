using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Traits;
namespace Inner_Maps.Location_Structures {
    public class Hospice : ManMadeStructure, TileObjectEventDispatcher.IDestroyedListener {

        public List<BedClinic> beds { get; private set; }

        #region getters
        public override Type serializedData => typeof(SaveDataHospice);
        #endregion
        
        public Hospice(Region location) : base(STRUCTURE_TYPE.HOSPICE, location) {
            SetMaxHPAndReset(8000);
            beds = new List<BedClinic>();
        }
        public Hospice(Region location, SaveDataManMadeStructure data) : base(location, data) {
            SetMaxHP(8000);
            beds = new List<BedClinic>();
        }

        #region Loading
        public override void LoadReferences(SaveDataLocationStructure saveDataLocationStructure) {
            base.LoadReferences(saveDataLocationStructure);
            SaveDataHospice saveDataHospice = saveDataLocationStructure as SaveDataHospice;
            if (saveDataHospice.beds != null) {
                for (int i = 0; i < saveDataHospice.beds.Length; i++) {
                    string bedID = saveDataHospice.beds[i];
                    if (DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(bedID) is BedClinic bedClinic) {
                        AddBed(bedClinic);
                    }
                }
            }
        }
        #endregion

        #region Beds
        public void AddBed(BedClinic p_bed) {
            if (!beds.Contains(p_bed)) {
                beds.Add(p_bed);
                p_bed.eventDispatcher.SubscribeToTileObjectDestroyed(this);
            }
        }
        private void RemoveBed(BedClinic p_bed) {
            if (beds.Remove(p_bed)) {
                p_bed.eventDispatcher.UnsubscribeToTileObjectDestroyed(this);
            }
        }
        #endregion

        #region IDestroyedListener Implementation
        public void OnTileObjectDestroyed(TileObject p_tileObject) {
            if (p_tileObject is BedClinic bedClinic) {
                RemoveBed(bedClinic);
            }
        }
        #endregion

        #region Listeners
        protected override void SubscribeListeners() {
            base.SubscribeListeners();
            Messenger.AddListener<TileObject, Character, LocationGridTile>(GridTileSignals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
            Messenger.AddListener<TileObject, LocationGridTile>(GridTileSignals.TILE_OBJECT_PLACED, OnTileObjectPlaced);
        }
        protected override void UnsubscribeListeners() {
            base.UnsubscribeListeners();
            Messenger.RemoveListener<TileObject, Character, LocationGridTile>(GridTileSignals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
            Messenger.RemoveListener<TileObject, LocationGridTile>(GridTileSignals.TILE_OBJECT_PLACED, OnTileObjectPlaced);
        }
        private void OnTileObjectPlaced(TileObject p_tileObject, LocationGridTile p_tile) {
            if (p_tile.structure == this && p_tileObject is BedClinic bedClinic) {
                AddBed(bedClinic);
            }
        }
        private void OnTileObjectRemoved(TileObject p_tileObject, Character p_removedBy, LocationGridTile p_removedFrom) {
            if (p_tileObject is BedClinic bedClinic) {
                RemoveBed(bedClinic);    
            }
        }
        #endregion

        #region For Testing
        public override string GetTestingInfo() {
            string info = base.GetTestingInfo();
            info = $"{info}\nBeds: {beds.ComafyList()}";
            return info;
        }
        #endregion

        protected override void ProcessWorkStructureJobsByWorker(Character p_worker, out JobQueueItem producedJob) {
            producedJob = null;
            Character targetVillager = GetVillagerToBeCured(p_worker);
            if (targetVillager != null) {
                p_worker.jobComponent.TriggerHealerCureCharacter(targetVillager, out producedJob);
                if(producedJob != null) {
                    return;
                }    
            }

            targetVillager = GetVillagerToBeDispelledForVampirism(p_worker);
            if (targetVillager != null) {
                p_worker.jobComponent.TriggerCureMagicalAffliction(targetVillager, "Vampire", out producedJob);
                if (producedJob != null) {
                    return;
                }
            }

            targetVillager = GetVillagerToBeDispelledForLycan(p_worker);
            if (targetVillager != null) {
                p_worker.jobComponent.TriggerCureMagicalAffliction(targetVillager, "Lycanthrope", out producedJob);
                if (producedJob != null) {
                    return;
                }
            }

            List<TileObject> plants = GetTileObjectsOfType(TILE_OBJECT_TYPE.HERB_PLANT);
            
            if (plants == null || plants.Count < 3) {
                HerbPlant plant = p_worker.homeSettlement.SettlementResources.GetAvailableHerbPlant();
                if (plant != null) {
                    p_worker.jobComponent.TriggerGatherHerb(plant as TileObject, out producedJob);
                    if(producedJob != null) {
                        return;
					}
                }
            }

            List<TileObject> potions = GetTileObjectsOfType(TILE_OBJECT_TYPE.HEALING_POTION);
            if (potions == null || potions.Count <= 0) {
                if (plants != null && plants.Count > 1) {
                    p_worker.jobComponent.TriggerCraftHospicePotion(plants[0], out producedJob);
                    if(producedJob != null) {
                        return;
					}
                }
                
            }

            List<TileObject> antidotes = GetTileObjectsOfType(TILE_OBJECT_TYPE.ANTIDOTE);
            if (antidotes == null || antidotes.Count <= 0) {
                if (plants != null && plants.Count > 1) {
                    p_worker.jobComponent.TriggerCraftHospiceAntidote(plants[0], out producedJob);
                    if (producedJob != null) {
                        return;
                    }
                }
     
            }
        }

        Character GetVillagerToBeCured(Character p_worker) {
            if (p_worker.talentComponent.GetTalent(CHARACTER_TALENT.Healing_Magic).level <= 1) {
                return null;
            }
            for(int x = 0; x < beds.Count; ++x) {
                if (beds[x].users.Length > 0) {
                    var villagerToBeCured = beds[x].users[0];
                    if (villagerToBeCured != null) {
                        if (p_worker.talentComponent.GetTalent(CHARACTER_TALENT.Healing_Magic).level >= 3) {
                            if (villagerToBeCured.traitContainer.HasTrait("Injured") || villagerToBeCured.traitContainer.HasTrait("Poison") || villagerToBeCured.traitContainer.HasTrait("Plagued")) {
                                return villagerToBeCured;
                            }
                        } else if (p_worker.talentComponent.GetTalent(CHARACTER_TALENT.Healing_Magic).level >= 2) {
                            if (villagerToBeCured.traitContainer.HasTrait("Injured") || villagerToBeCured.traitContainer.HasTrait("Poison")) {
                                return villagerToBeCured;
                            }
                        }    
                    }
                }
			}
            return null;
        }
        Character GetVillagerToBeDispelledForLycan(Character p_worker) {
            if (p_worker.talentComponent.GetTalent(CHARACTER_TALENT.Healing_Magic).level < 5) {
                return null;
            }
            for (int x = 0; x < beds.Count; ++x) {
                if (beds[x].users.Length > 0) {
                    Character villagerToBeDispelled = beds[x].users[0];
                    if (villagerToBeDispelled != null) {
                        if (villagerToBeDispelled.isLycanthrope && villagerToBeDispelled.lycanData.dislikesBeingLycan) {
                            return villagerToBeDispelled;
                        }
                    }
                }
            }
            for (int x = 0; x < charactersHere.Count; ++x) {
                Character villagerToBeDispelled = charactersHere[x];
                if (villagerToBeDispelled != null) {
                    if (villagerToBeDispelled.isLycanthrope && villagerToBeDispelled.lycanData.dislikesBeingLycan) {
                        return villagerToBeDispelled;
                    }
                }
            }
            return null;
        }

        Character GetVillagerToBeDispelledForVampirism(Character p_worker) {
            if (p_worker.talentComponent.GetTalent(CHARACTER_TALENT.Healing_Magic).level < 5) {
                return null;
            }
            for (int x = 0; x < beds.Count; ++x) {
                if (beds[x].users.Length > 0) {
                    Character villagerToBeDispelled = beds[x].users[0];
                    if (villagerToBeDispelled != null) {
                        if (villagerToBeDispelled.traitContainer.HasTrait("Vampire")) {
                            Vampire vampire = villagerToBeDispelled.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
                            if (vampire.dislikedBeingVampire) {
                                return villagerToBeDispelled;
                            }

                        }
                    }
                }
            }
            for (int x = 0; x < charactersHere.Count; ++x) {
                Character villagerToBeDispelled = charactersHere[x];
                if (villagerToBeDispelled != null) {
                    if (villagerToBeDispelled.traitContainer.HasTrait("Vampire")) {
                        Vampire vampire = villagerToBeDispelled.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
                        if (vampire.dislikedBeingVampire) {
                            return villagerToBeDispelled;
                        }
                    }
                }
            }
            return null;
        }
        public BedClinic GetFirstUnoccupiedBed() {
            for (int i = 0; i < beds.Count; i++) {
                BedClinic bedClinic = beds[i];
                if (bedClinic.IsAvailable()) {
                    return bedClinic;
                }
            }
            return null;
        }
    }
}

#region Save Data
public class SaveDataHospice : SaveDataManMadeStructure {
    public string[] beds;
    public override void Save(LocationStructure locationStructure) {
        base.Save(locationStructure);
        Hospice hospice = locationStructure as Hospice;
        if (hospice.beds.Count > 0) {
            beds = new string[hospice.beds.Count];
            for (int i = 0; i < hospice.beds.Count; i++) {
                BedClinic bedClinic = hospice.beds[i];
                beds[i] = bedClinic.persistentID;
            }
        }
    }
}
#endregion