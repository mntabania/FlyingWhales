using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        }

        #region Object Here
        public void SetObjectHere(TileObject poi) {
            if (poi.isHidden) {
                hiddenObjHere = poi;
                poi.SetGridTileLocation(owner);
                poi.OnPlacePOI();
            } else {
                bool isPassablePreviously = owner.IsPassable();
                if (poi.OccupiesTile()) {
                    objHere = poi;
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
                } else if (owner.IsPassable() && !isPassablePreviously) {
                    owner.structure.AddPassableTile(owner);
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
                        objHere = poi;
                    }
                } else {
                    objHere = poi;
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
        public TileObject RemoveObjectHere(Character removedBy) {
            if (objHere != null) {
                TileObject removedObj = objHere;
                objHere = null;
                removedObj.RemoveTileObject(removedBy);
                //if (removedObj is TileObject tileObject) {
                //    //if the object in this tile is a tile object and it was removed by a character, use tile object specific function
                //    tileObject.RemoveTileObject(removedBy);
                //} else {
                //    removedObj.SetGridTileLocation(null);
                //    removedObj.OnDestroyPOI();
                //}
                owner.SetTileState(LocationGridTile.Tile_State.Empty);
                Messenger.Broadcast(CharacterSignals.STOP_CURRENT_ACTION_TARGETING_POI, removedObj);
                Messenger.Broadcast(SpellSignals.RELOAD_PLAYER_ACTIONS, removedObj as IPlayerActionTarget);
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
                Messenger.Broadcast(SpellSignals.RELOAD_PLAYER_ACTIONS, removedObj as IPlayerActionTarget);
                return removedObj;
            }
            return null;
        }
        public TileObject RemoveObjectHereWithoutDestroying() {
            if (objHere != null) {
                TileObject removedObj = objHere;
                LocationGridTile gridTile = objHere.gridTileLocation;
                objHere.SetGridTileLocation(null);
                objHere = null;
                owner.SetTileState(LocationGridTile.Tile_State.Empty);
                removedObj.OnRemoveTileObject(null, gridTile, false, false);
                //if (removedObj is TileObject tileObject) {
                //    tileObject.OnRemoveTileObject(null, gridTile, false, false);
                //}
                removedObj.SetPOIState(POI_STATE.INACTIVE);
                Messenger.Broadcast(SpellSignals.RELOAD_PLAYER_ACTIONS, removedObj as IPlayerActionTarget);
                return removedObj;
            }
            return null;
        }
        public TileObject RemoveObjectHereDestroyVisualOnly(Character remover = null) {
            if (objHere != null) {
                TileObject removedObj = objHere;
                LocationGridTile gridTile = objHere.gridTileLocation;
                objHere.SetGridTileLocation(null);
                objHere = null;
                owner.SetTileState(LocationGridTile.Tile_State.Empty);
                removedObj.OnRemoveTileObject(null, gridTile, false, false);
                removedObj.DestroyMapVisualGameObject();
                //if (removedObj is TileObject removedTileObj) {
                //    removedTileObj.OnRemoveTileObject(null, gridTile, false, false);
                //    removedTileObj.DestroyMapVisualGameObject();
                //}
                removedObj.SetPOIState(POI_STATE.INACTIVE);
                Messenger.Broadcast(CharacterSignals.STOP_CURRENT_ACTION_TARGETING_POI_EXCEPT_ACTOR, removedObj, remover);
                Messenger.Broadcast(SpellSignals.RELOAD_PLAYER_ACTIONS, removedObj as IPlayerActionTarget);
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
            return walls.Count > 0 || objHere is BlockWall;
        }
        public TileObject GetFirstWall() {
            if(objHere is BlockWall) {
                return objHere;
            } else if (walls.Count > 0) {
                return walls[0];
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
            GameManager.Instance.CreateParticleEffectAt(owner, PARTICLE_EFFECT.Landmine_Explosion);
            genericTileObject.traitContainer.AddTrait(genericTileObject, "Danger Remnant");
            int baseDamage = -500;
            yield return new WaitForSeconds(0.5f);
            SetHasLandmine(false);
            List<LocationGridTile> tiles = owner.GetTilesInRadius(3, includeCenterTile: true, includeTilesInDifferentStructure: true);
            for (int i = 0; i < tiles.Count; i++) {
                LocationGridTile tile = tiles[i];
                List<IPointOfInterest> pois = tile.GetPOIsOnTile();
                for (int j = 0; j < pois.Count; j++) {
                    IPointOfInterest poi = pois[j];
                    if (poi.gridTileLocation == null) {
                        continue; //skip
                    }
                    int processedDamage = baseDamage + PlayerSkillManager.Instance.GetAdditionalDamageBaseOnLevel(PLAYER_SKILL_TYPE.LANDMINE);
                    if (poi is TileObject obj) {
                        if (obj.tileObjectType != TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT) {
                            obj.AdjustHP(processedDamage, ELEMENTAL_TYPE.Normal, true, showHPBar: true);
                        } else {
                            CombatManager.Instance.ApplyElementalDamage(0, ELEMENTAL_TYPE.Normal, obj);
                        }
                    } else if (poi is Character character) {
                        character.AdjustHP(processedDamage, ELEMENTAL_TYPE.Normal, true, showHPBar: true);
                        Messenger.Broadcast(PlayerSignals.PLAYER_HIT_CHARACTER_VIA_SPELL, character, processedDamage);
                        if (character.currentHP <= 0) {
                            character.skillCauseOfDeath = PLAYER_SKILL_TYPE.LANDMINE;
                            Messenger.Broadcast(PlayerSignals.CREATE_SPIRIT_ENERGY, character.marker.transform.position, 1, character.currentRegion.innerMap);
                        }
                    } else {
                        poi.AdjustHP(processedDamage, ELEMENTAL_TYPE.Normal, true, showHPBar: true);
                    }
                }
            }
        }
        #endregion

        #region Freezing Trap
        public void SetHasFreezingTrap(bool state, params RACE[] freezingTrapExclusions) {
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
            GameManager.Instance.CreateParticleEffectAt(triggeredBy, PARTICLE_EFFECT.Freezing_Trap_Explosion);
            AudioManager.Instance.TryCreateAudioObject(PlayerSkillManager.Instance.GetPlayerSkillData<FreezingTrapSkillData>(PLAYER_SKILL_TYPE.FREEZING_TRAP).trapExplosionSound, owner, 1, false);
            PlayerSkillData playerSkillData = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(PLAYER_SKILL_TYPE.FREEZING_TRAP);
            SkillData skillData = PlayerSkillManager.Instance.GetPlayerSkillData(PLAYER_SKILL_TYPE.FREEZING_TRAP);
            SetHasFreezingTrap(false);
            for (int i = 0; i < 3; i++) {
                if (triggeredBy.traitContainer.HasTrait("Frozen")) {
                    break;
                } else {
                    int duration = TraitManager.Instance.allTraits["Freezing"].ticksDuration + PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.FREEZING_TRAP);
                    triggeredBy.traitContainer.AddTrait(triggeredBy, "Freezing", bypassElementalChance: true, overrideDuration: duration);
                }
            }
        }
        #endregion

        #region Snare Trap
        public void SetHasSnareTrap(bool state) {
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
            GameManager.Instance.CreateParticleEffectAt(triggeredBy, PARTICLE_EFFECT.Snare_Trap_Explosion);
            SetHasSnareTrap(false);
            int duration = TraitManager.Instance.allTraits["Ensnared"].ticksDuration + PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.SNARE_TRAP);
            triggeredBy.traitContainer.AddTrait(triggeredBy, "Ensnared", overrideDuration: duration);
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
                SetHasFreezingTrap(true, freezingTrapExclusions);
            }
            if (hasSnareTrap) {
                hasSnareTrap = false; //Set to false first so that the SetHasSnareTrap will be called
                SetHasSnareTrap(true);
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
        public RACE[] freezingTrapExclusions;
        public override void Save(GridTileTileObjectComponent data) {
            base.Save(data);
            genericTileObjectID = data.genericTileObject.persistentID;
            hasLandmine = data.hasLandmine;
            hasFreezingTrap = data.hasFreezingTrap;
            hasSnareTrap = data.hasSnareTrap;
            freezingTrapExclusions = data.freezingTrapExclusions;
            isSeenByEyeWard = data.isSeenByEyeWard;
        }
        public override GridTileTileObjectComponent Load() {
            GridTileTileObjectComponent component = new GridTileTileObjectComponent(this);
            return component;
        }
    }
}