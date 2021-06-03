using System.Collections.Generic;

namespace Inner_Maps.Location_Structures {
    public class Hospice : ManMadeStructure {

        public List<BedClinic> beds = new List<BedClinic>();
        
        public Hospice(Region location) : base(STRUCTURE_TYPE.HOSPICE, location) {
            SetMaxHPAndReset(8000);
        }
        public Hospice(Region location, SaveDataManMadeStructure data) : base(location, data) {
            SetMaxHP(8000);
        }

        protected override void ProcessWorkStructureJobsByWorker(Character p_worker, out JobQueueItem producedJob) {
            producedJob = null;
            Character targetVillager = GetVillagerToBeCured(p_worker);
            p_worker.jobComponent.TriggerHealerCureCharacter(targetVillager, out producedJob);
            if(producedJob != null) {
                return;
			}
            targetVillager = GetVillagerToBDispelled(p_worker);
            p_worker.jobComponent.TriggerHealerCureCharacter(targetVillager, out producedJob);
            if (producedJob != null) {
                return;
            }

            List<TileObject> plants = GetTileObjectsOfType(TILE_OBJECT_TYPE.HERB_PLANT);
            if (plants == null || plants.Count < 3) {
                HerbPlant plant = p_worker.homeSettlement.SettlementResources.GetAvailableHerbPlant();
                if (plant != null) {
                    p_worker.jobComponent.TriggerGatherHerb(plant, out producedJob);
                    if(producedJob != null) {
                        return;
					}
                }
            }

            List<TileObject> potions = GetTileObjectsOfType(TILE_OBJECT_TYPE.HEALING_POTION);
            if (potions == null || potions.Count <= 0) {
                if (plants.Count > 1) {
                    p_worker.jobComponent.TriggerCraftHospicePotion(plants[0], out producedJob);
                    if(producedJob != null) {
                        return;
					}
                }
                
            }

            List<TileObject> antidotes = GetTileObjectsOfType(TILE_OBJECT_TYPE.ANTIDOTE);
            if (antidotes == null || antidotes.Count <= 0) {
                if (plants.Count > 1) {
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
                    if (p_worker.talentComponent.GetTalent(CHARACTER_TALENT.Healing_Magic).level >= 3) {
                        if (beds[x].users[0].traitContainer.HasTrait("Injured") || beds[x].users[0].traitContainer.HasTrait("Poison") || beds[x].users[0].traitContainer.HasTrait("Plagued")) {
                            return beds[x].users[0];
                        }
                    } else if (p_worker.talentComponent.GetTalent(CHARACTER_TALENT.Healing_Magic).level >= 2) {
                        if (beds[x].users[0].traitContainer.HasTrait("Injured") || beds[x].users[0].traitContainer.HasTrait("Poison")) {
                            return beds[x].users[0];
                        }
                    }
                }
			}
            return null;
        }
        Character GetVillagerToBDispelled(Character p_worker) {
            if (p_worker.talentComponent.GetTalent(CHARACTER_TALENT.Healing_Magic).level <= 1) {
                return null;
            }
            for (int x = 0; x < beds.Count; ++x) {
                if (beds[x].users.Length > 0) {
                    if (p_worker.talentComponent.GetTalent(CHARACTER_TALENT.Healing_Magic).level >= 5) {
                        if (beds[x].users[0].traitContainer.HasTrait("Vampirism") || beds[x].users[0].isLycanthrope) {
                            return beds[x].users[0];
                        }
                    }
                }
            }
            return null;
        }
    }
}