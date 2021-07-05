using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using UtilityScripts;

namespace Inner_Maps {
    public class GridTileTileObjectComponent : LocationGridTileComponent {
        public TileObject objHere { get; private set; }
        public TileObject hiddenObjHere { get; private set; }
        public GenericTileObject genericTileObject { get; private set; }
        public List<ThinWall> walls { get; private set; }
        public bool hasLandmine { get; private set; }
        public bool hasFreezingTrap { get; private set; }
        public bool hasSnareTrap { get; private set; }
        public bool isSeenByEyeWard { get; private set; }
        public bool isFreezingTrapPlayerSource { get; private set; }
        public bool isSnareTrapPlayerSource { get; private set; }

        public RACE[] freezingTrapExclusions { get; private set; }

        private GameObject _landmineEffect;
        private GameObject _freezingTrapEffect;
        private GameObject _snareTrapEffect;
        public GridTileTileObjectComponent() {
            walls = new List<ThinWall>();
        }
        public GridTileTileObjectComponent(SaveDataGridTileTileObjectComponent data) {
            walls = new List<ThinWall>();
            hasLandmine = data.hasLandmine;
            hasFreezingTrap = data.hasFreezingTrap;
            hasSnareTrap = data.hasSnareTrap;
            freezingTrapExclusions = data.freezingTrapExclusions;
            isSeenByEyeWard = data.isSeenByEyeWard;
            isFreezingTrapPlayerSource = data.isFreezingTrapPlayerSource;
            isSnareTrapPlayerSource = data.isSnareTrapPlayerSource;
        }

        #region Object Here
        public void SetOccupyingObject(TileObject p_object) {
            objHere = p_object;
            owner.mouseEventsComponent.UpdateHasMouseEventsForSelfAndAllNeighbours();
        }
        public void SetObjectHere(TileObject poi) {
            if (poi.isHidden) {
                hiddenObjHere = poi;
                poi.SetGridTileLocation(owner);
                poi.OnPlacePOI();
            } else {
                bool isPassablePreviously = owner.IsPassable();
                if (poi.OccupiesTile()) {
                    SetOccupyingObject(poi);
                }
                //if (poi is TileObject tileObject) {
                //    if (tileObject.OccupiesTile()) {
                //        objHere = poi;
                //    }
                //} else {
                //    objHere = poi;
                //}

                poi.SetGridTileLocation(owner);
                poi.OnPlacePOI();
                owner.SetTileState(LocationGridTile.Tile_State.Occupied);
                if (!owner.IsPassable()) {
                    owner.structure.RemovePassableTile(owner);
                    owner.area.gridTileComponent.RemovePassableTile(owner);
                } else if (owner.IsPassable() && !isPassablePreviously) {
                    owner.structure.AddPassableTile(owner);
                    owner.area.gridTileComponent.AddPassableTile(owner);
                }
                Messenger.Broadcast(GridTileSignals.OBJECT_PLACED_ON_TILE, owner, poi);
            }
        }
        public void LoadObjectHere(TileObject poi) {
            if (poi.isHidden) {
                hiddenObjHere = poi;
                poi.SetGridTileLocation(owner);
                poi.OnLoadPlacePOI();
            } else {
                bool isPassablePreviously = owner.IsPassable();
                if (poi is TileObject tileObject) {
                    if (tileObject.OccupiesTile()) {
                        SetOccupyingObject(poi);
                    }
                } else {
                    SetOccupyingObject(poi);
                }

                poi.SetGridTileLocation(owner);
                poi.OnLoadPlacePOI();
                owner.SetTileState(LocationGridTile.Tile_State.Occupied);
                if (!owner.IsPassable()) {
                    owner.structure.RemovePassableTile(owner);
                } else if (owner.IsPassable() && !isPassablePreviously) {
                    owner.structure.AddPassableTile(owner);
                }
                Messenger.Broadcast(GridTileSignals.OBJECT_PLACED_ON_TILE, owner, poi);
            }
        }
        public TileObject RemoveObjectHere(Character removedBy, bool isPlayerSource) {
            if (objHere != null) {
                TileObject removedObj = objHere;
                SetOccupyingObject(null);
                removedObj.RemoveTileObject(removedBy);
                //if (removedObj is TileObject tileObject) {
                //    //if the object in this tile is a tile object and it was removed by a character, use tile object specific function
                //    tileObject.RemoveTileObject(removedBy);
                //} else {
                //    removedObj.SetGridTileLocation(null);
                //    removedObj.OnDestroyPOI();
                //}
                owner.SetTileState(LocationGridTile.Tile_State.Empty);

                if(removedBy != null && removedBy.faction != null && removedBy.faction.isPlayerFaction) {
                    isPlayerSource = true;
                }

                if (isPlayerSource) {
                    if (removedObj is ResourcePile) {
                        PlayerManager.Instance?.player?.retaliationComponent.ResourcePileRetaliation(removedObj, owner);
                    }
                }

                Messenger.Broadcast(CharacterSignals.STOP_CURRENT_ACTION_TARGETING_POI, removedObj);
                Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, removedObj as IPlayerActionTarget);
                removedObj.DestroyPermanently();
                return removedObj;
            }
            return null;
        }
        public TileObject RemoveHiddenObjectHere(Character removedBy) {
            if (hiddenObjHere != null) {
                TileObject removedObj = hiddenObjHere;
                hiddenObjHere = null;
                removedObj.RemoveTileObject(removedBy);
                Messenger.Broadcast(CharacterSignals.STOP_CURRENT_ACTION_TARGETING_POI, removedObj);
                Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, removedObj as IPlayerActionTarget);
                removedObj.DestroyPermanently();
                return removedObj;
            }
            return null;
        }
        public TileObject RemoveObjectHereWithoutDestroying() {
            if (objHere != null) {
                TileObject removedObj = objHere;
                //LocationGridTile gridTile = objHere.gridTileLocation;
                objHere.SetGridTileLocation(null);
                SetOccupyingObject(null);
                removedObj.previousTile?.area.OnRemovePOIInHex(removedObj);
                owner.SetTileState(LocationGridTile.Tile_State.Empty);
                removedObj.OnRemoveTileObject(null, owner, false, false);
                //if (removedObj is TileObject tileObject) {
                //    tileObject.OnRemoveTileObject(null, gridTile, false, false);
                //}
                removedObj.SetPOIState(POI_STATE.INACTIVE);

                Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, removedObj as IPlayerActionTarget);
                return removedObj;
            }
            return null;
        }
        public TileObject RemoveObjectHereDestroyVisualOnly(Character remover = null) {
            if (objHere != null) {
                TileObject removedObj = objHere;
                //LocationGridTile gridTile = objHere.gridTileLocation;
                objHere.SetGridTileLocation(null);
                SetOccupyingObject(null);
                removedObj.previousTile?.area.OnRemovePOIInHex(removedObj);
                owner.SetTileState(LocationGridTile.Tile_State.Empty);
                removedObj.OnRemoveTileObject(null, owner, false, false);
                removedObj.DestroyMapVisualGameObject();
                //if (removedObj is TileObject removedTileObj) {
                //    removedTileObj.OnRemoveTileObject(null, gridTile, false, false);
                //    removedTileObj.DestroyMapVisualGameObject();
                //}
                removedObj.SetPOIState(POI_STATE.INACTIVE);

                bool isPlayerSource = false;
                if (remover != null && remover.faction != null && remover.faction.isPlayerFaction) {
                    isPlayerSource = true;
                }
                if (isPlayerSource) {
                    if (removedObj is ResourcePile) {
                        PlayerManager.Instance?.player?.retaliationComponent.ResourcePileRetaliation(removedObj, owner);
                    }
                }
                Messenger.Broadcast(CharacterSignals.STOP_CURRENT_ACTION_TARGETING_POI_EXCEPT_ACTOR, removedObj, remover);
                Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, removedObj as IPlayerActionTarget);
                return removedObj;
            }
            return null;
        }
        #endregion

        #region Generic Tile Object
        public void CreateGenericTileObject() {
            genericTileObject = new GenericTileObject(owner);
        }
        public void LoadGenericTileObject(GenericTileObject genericTileObject) {
            this.genericTileObject = genericTileObject;
        }
        #endregion

        #region Walls
        public void AddWallObject(ThinWall structureWallObject) {
            walls.Add(structureWallObject);
            //if(objHere is BlockWall) {
            //    //Thin walls cannot co-exist with block walls, so if a thin wall is placed, all block walls must be destroyed
            //    objHere.AdjustHP(-objHere.maxHP, ELEMENTAL_TYPE.Normal, true);
            //}
        }
        public void ClearWallObjects() {
            walls.Clear();
        }
        public bool HasWalls() {
            if(objHere is BlockWall || objHere is OreVein) {
                return true;
            } else {
                if(walls.Count > 0) {
                    for (int i = 0; i < walls.Count; i++) {
                        if (walls[i].currentHP > 0) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public TileObject GetFirstWall() {
            if(objHere is BlockWall || objHere is OreVein) {
                return objHere;
            } else if (walls.Count > 0) {
                for (int i = 0; i < walls.Count; i++) {
                    if (walls[i].currentHP > 0) {
                        return walls[i];
                    }
                }
            }
            return null;
        }
        #endregion
        #region Landmine
        public void SetHasLandmine(bool state) {
            if (hasLandmine != state) {
                owner.SetIsDefault(false);
                hasLandmine = state;
                if (hasLandmine) {
                    _landmineEffect = GameManager.Instance.CreateParticleEffectAt(owner, PARTICLE_EFFECT.Landmine, InnerMapManager.DetailsTilemapSortingOrder - 1);
                } else {
                    ObjectPoolManager.Instance.DestroyObject(_landmineEffect);
                    _landmineEffect = null;
                }
            }
        }
        public IEnumerator TriggerLandmine(Character triggeredBy) {
            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "InterventionAbility", "Landmine", "trap_activated", null, LOG_TAG.Player);
            log.AddToFillers(triggeredBy, triggeredBy.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddLogToDatabase();
            PlayerManager.Instance.player.ShowNotificationFrom(triggeredBy, log, true);
            
            GameManager.Instance.CreateParticleEffectAt(owner, PARTICLE_EFFECT.Landmine_Explosion);
            genericTileObject.traitContainer.AddTrait(genericTileObject, "Danger Remnant");
            SetHasLandmine(false);
            if (triggeredBy.isNormalAndNotAlliedWithPlayer) {
                Messenger.Broadcast(PlayerSkillSignals.ON_TRAP_ACTIVATED_ON_VILLAGER, triggeredBy);
            }
            yield return new WaitForSeconds(0.5f);
            List<LocationGridTile> tiles = RuinarchListPool<LocationGridTile>.Claim();
            owner.PopulateTilesInRadius(tiles, 3, includeCenterTile: true, includeTilesInDifferentStructure: true);
            SkillData landmineData = PlayerSkillManager.Instance.GetSpellData(PLAYER_SKILL_TYPE.LANDMINE);
            int processedDamage = -PlayerSkillManager.Instance.GetDamageBaseOnLevel(landmineData);
            float piercing = PlayerSkillManager.Instance.GetAdditionalPiercePerLevelBaseOnLevel(landmineData);

            for (int i = 0; i < tiles.Count; i++) {
                LocationGridTile tile = tiles[i];
                List<IPointOfInterest> pois = RuinarchListPool<IPointOfInterest>.Claim();
                tile.PopulatePOIsOnTile(pois);
                for (int j = 0; j < pois.Count; j++) {
                    IPointOfInterest poi = pois[j];
                    if (poi.gridTileLocation == null) {
                        continue; //skip
                    }
                    if (poi is TileObject obj) {
                        if (obj.tileObjectType != TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT) {
                            obj.AdjustHP(processedDamage, ELEMENTAL_TYPE.Normal, true, showHPBar: true, piercingPower: piercing, isPlayerSource: true, source : landmineData);
                        } else {
                            CombatManager.Instance.ApplyElementalDamage(0, ELEMENTAL_TYPE.Normal, obj, setAsPlayerSource: true);
                        }
                    } else if (poi is Character character) {
                        character.AdjustHP(processedDamage, ELEMENTAL_TYPE.Normal, true, showHPBar: true, piercingPower: piercing, isPlayerSource: true, source: landmineData);
                        Messenger.Broadcast(PlayerSignals.PLAYER_HIT_CHARACTER_VIA_SPELL, character, processedDamage);
                        if (character.isDead && character.skillCauseOfDeath == PLAYER_SKILL_TYPE.NONE) {
                            character.skillCauseOfDeath = PLAYER_SKILL_TYPE.LANDMINE;
                            //Messenger.Broadcast(PlayerSignals.CREATE_SPIRIT_ENERGY, character.deathTilePosition.centeredWorldLocation, 1, character.deathTilePosition.parentMap);
                            //Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, character.deathTilePosition.centeredWorldLocation, 1, character.deathTilePosition.parentMap);
                        }
                    } else {
                        poi.AdjustHP(processedDamage, ELEMENTAL_TYPE.Normal, true, showHPBar: true, piercingPower: piercing, isPlayerSource: true, source: landmineData);
                    }
                }
                RuinarchListPool<IPointOfInterest>.Release(pois);
            }
            RuinarchListPool<LocationGridTile>.Release(tiles);
        }
        #endregion

        #region Freezing Trap
        public void SetHasFreezingTrap(bool state, bool isPlayerSource, params RACE[] freezingTrapExclusions) {
            isFreezingTrapPlayerSource = isPlayerSource;
            if (hasFreezingTrap != state) {
                owner.SetIsDefault(false);
                hasFreezingTrap = state;
                if (hasFreezingTrap) {
                    owner.area.AddFreezingTrapInArea();
                    this.freezingTrapExclusions = freezingTrapExclusions;
                    _freezingTrapEffect = GameManager.Instance.CreateParticleEffectAt(owner, PARTICLE_EFFECT.Freezing_Trap, InnerMapManager.DetailsTilemapSortingOrder - 1);
                } else {
                    owner.area.RemoveFreezingTrapInArea();
                    ObjectPoolManager.Instance.DestroyObject(_freezingTrapEffect);
                    _freezingTrapEffect = null;
                    this.freezingTrapExclusions = null;
                }
            }
        }
        public void TriggerFreezingTrap(Character triggeredBy) {
            bool willTrigger = false;
            int baseChance = 100;
            SkillData trapData = PlayerSkillManager.Instance.GetSpellData(PLAYER_SKILL_TYPE.FREEZING_TRAP);
            FreezingTrapSkillData trapSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<FreezingTrapSkillData>(PLAYER_SKILL_TYPE.FREEZING_TRAP);
            RESISTANCE resistanceType = trapSkillData.resistanceType;
            float piercing = PlayerSkillManager.Instance.GetAdditionalPiercePerLevelBaseOnLevel(trapData);
            float resistanceValue = triggeredBy.piercingAndResistancesComponent.GetResistanceValue(resistanceType);

            if (triggeredBy.traitContainer.HasTrait("Recently Trapped")) {
                resistanceValue += 50f;
            }

            CombatManager.ModifyValueByPiercingAndResistance(ref baseChance, piercing, resistanceValue);
            string debugLog = string.Empty;
#if DEBUG_LOG
            debugLog += "Freezing Trap Chance";
#endif
            if (GameUtilities.RollChance(baseChance, ref debugLog)) {
                willTrigger = true;
            }
#if DEBUG_LOG
            triggeredBy.logComponent.PrintLogIfActive(debugLog);
#endif

            bool isFreezingTrapPlayerSource = false;
            int duration = PlayerSkillManager.Instance.GetDurationBonusPerLevel(trapData);
            if (willTrigger) {
                if (triggeredBy is Summon summon) {
                    if (summon.summonType == SUMMON_TYPE.Kobold) {
                        duration = PlayerSkillManager.Instance.GetDurationBonusPerLevel(trapData, 0);
                    }
                } else {
                    if (triggeredBy.isNormalAndNotAlliedWithPlayer) {
                        Messenger.Broadcast(PlayerSkillSignals.ON_TRAP_ACTIVATED_ON_VILLAGER, triggeredBy);
                    }
                }
                GameManager.Instance.CreateParticleEffectAt(triggeredBy, PARTICLE_EFFECT.Freezing_Trap_Explosion);
                AudioManager.Instance.TryCreateAudioObject(trapSkillData.trapExplosionSound, owner, 1, false);

                isFreezingTrapPlayerSource = this.isFreezingTrapPlayerSource;
            }

            SetHasFreezingTrap(false, false);

            if (willTrigger) {
                triggeredBy.traitContainer.RemoveStatusAndStacks(triggeredBy, "Freezing");
                triggeredBy.traitContainer.AddTrait(triggeredBy, "Frozen", bypassElementalChance: true, overrideDuration: duration);
                Frozen frozen = triggeredBy.traitContainer.GetTraitOrStatus<Frozen>("Frozen");
                frozen?.SetIsPlayerSource(isFreezingTrapPlayerSource);
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "InterventionAbility", "Freezing Trap", "trap_activated", null, LOG_TAG.Player);
                log.AddToFillers(triggeredBy, triggeredBy.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddLogToDatabase();
                PlayerManager.Instance.player.ShowNotificationFrom(triggeredBy, log, true);

                triggeredBy.traitContainer.AddTrait(triggeredBy, "Recently Trapped");
            } else {
                if (isFreezingTrapPlayerSource) {
                    triggeredBy.reactionComponent.ResistRuinarchPower();
                } else {
                    triggeredBy.reactionComponent.PlayResistVFX();
                }
            }
        }
        #endregion

        #region Snare Trap
        public void SetHasSnareTrap(bool state, bool isPlayerSource) {
            isSnareTrapPlayerSource = isPlayerSource;
            if (hasSnareTrap != state) {
                owner.SetIsDefault(false);
                hasSnareTrap = state;
                if (hasSnareTrap) {
                    _snareTrapEffect = GameManager.Instance.CreateParticleEffectAt(owner, PARTICLE_EFFECT.Snare_Trap, InnerMapManager.DetailsTilemapSortingOrder - 1);
                } else {
                    ObjectPoolManager.Instance.DestroyObject(_snareTrapEffect);
                    _snareTrapEffect = null;
                }
            }
        }
        public void TriggerSnareTrap(Character triggeredBy) {
            bool willTrigger = false;
            int baseChance = 100;
            SkillData trapData = PlayerSkillManager.Instance.GetSpellData(PLAYER_SKILL_TYPE.SNARE_TRAP);
            PlayerSkillData trapSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(PLAYER_SKILL_TYPE.SNARE_TRAP);
            RESISTANCE resistanceType = trapSkillData.resistanceType;
            float piercing = PlayerSkillManager.Instance.GetAdditionalPiercePerLevelBaseOnLevel(trapData);
            float resistanceValue = triggeredBy.piercingAndResistancesComponent.GetResistanceValue(resistanceType);

            if (triggeredBy.traitContainer.HasTrait("Recently Trapped")) {
                resistanceValue += 50f;
            }

            CombatManager.ModifyValueByPiercingAndResistance(ref baseChance, piercing, resistanceValue);
            string debugLog = string.Empty;
#if DEBUG_LOG
            debugLog += "Snare Trap Chance";
#endif
            if (GameUtilities.RollChance(baseChance, ref debugLog)) {
                willTrigger = true;
            }
#if DEBUG_LOG
            triggeredBy.logComponent.PrintLogIfActive(debugLog);
#endif
            bool isSnareTrapPlayerSource = false;
            if (willTrigger) {
                GameManager.Instance.CreateParticleEffectAt(triggeredBy, PARTICLE_EFFECT.Snare_Trap_Explosion);
                isSnareTrapPlayerSource = this.isSnareTrapPlayerSource;
            }

            SetHasSnareTrap(false, false);

            if (willTrigger) {
                //int duration = TraitManager.Instance.allTraits["Ensnared"].ticksDuration + PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.SNARE_TRAP);
                int duration = PlayerSkillManager.Instance.GetDurationBonusPerLevel(trapData);
                triggeredBy.traitContainer.AddTrait(triggeredBy, "Ensnared", overrideDuration: duration);
                Ensnared ensnared = triggeredBy.traitContainer.GetTraitOrStatus<Ensnared>("Ensnared");
                ensnared?.SetIsPlayerSource(isSnareTrapPlayerSource);
                if (triggeredBy.isNormalAndNotAlliedWithPlayer) {
                    Messenger.Broadcast(PlayerSkillSignals.ON_TRAP_ACTIVATED_ON_VILLAGER, triggeredBy);
                }
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "InterventionAbility", "Snare Trap", "trap_activated", null, LOG_TAG.Player);
                log.AddToFillers(triggeredBy, triggeredBy.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddLogToDatabase();
                PlayerManager.Instance.player.ShowNotificationFrom(triggeredBy, log, true);

                triggeredBy.traitContainer.AddTrait(triggeredBy, "Recently Trapped");
            } else {
                if (isSnareTrapPlayerSource) {
                    triggeredBy.reactionComponent.ResistRuinarchPower();
                } else {
                    triggeredBy.reactionComponent.PlayResistVFX();
                }
            }
        }
        #endregion

        #region Loading
        public void LoadSecondWave() {
            if (hasLandmine) {
                hasLandmine = false; //Set to false first so that the SetHasLandmine will be called
                SetHasLandmine(true);
            }
            if (hasFreezingTrap) {
                hasFreezingTrap = false; //Set to false first so that the SetHasFreezingTrap will be called
                SetHasFreezingTrap(true, isFreezingTrapPlayerSource, freezingTrapExclusions);
            }
            if (hasSnareTrap) {
                hasSnareTrap = false; //Set to false first so that the SetHasSnareTrap will be called
                SetHasSnareTrap(true, isSnareTrapPlayerSource);
            }
        }
        #endregion

        #region Eye Ward
        public void SetIsSeenByEyeWard(bool state) {
            isSeenByEyeWard = state;
        }
        #endregion

        #region Clean Up
        public void CleanUp() {
            objHere = null;
            genericTileObject = null;
            walls?.Clear();
            walls = null;
        }
        #endregion
    }
    public class SaveDataGridTileTileObjectComponent : SaveData<GridTileTileObjectComponent> {
        public string genericTileObjectID;
        public bool hasLandmine;
        public bool hasFreezingTrap;
        public bool hasSnareTrap;
        public bool isSeenByEyeWard;
        public bool isFreezingTrapPlayerSource;
        public bool isSnareTrapPlayerSource;
        public RACE[] freezingTrapExclusions;
        public override void Save(GridTileTileObjectComponent data) {
            base.Save(data);
            genericTileObjectID = data.genericTileObject.persistentID;
            hasLandmine = data.hasLandmine;
            hasFreezingTrap = data.hasFreezingTrap;
            hasSnareTrap = data.hasSnareTrap;
            freezingTrapExclusions = data.freezingTrapExclusions;
            isSeenByEyeWard = data.isSeenByEyeWard;
            isFreezingTrapPlayerSource = data.isFreezingTrapPlayerSource;
            isSnareTrapPlayerSource = data.isSnareTrapPlayerSource;
        }
        public override GridTileTileObjectComponent Load() {
            GridTileTileObjectComponent component = new GridTileTileObjectComponent(this);
            return component;
        }
    }
}