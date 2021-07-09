
using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using Inner_Maps;

public class SummonBoneGolem : GoapAction {

    public SummonBoneGolem() : base(INTERACTION_TYPE.SUMMON_BONE_GOLEM) {
        actionIconString = GoapActionStateDB.Magic_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        logTags = new[] {LOG_TAG.Major, LOG_TAG.Work};
        showNotification = true;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.SUMMON, "Bone Golem", false, GOAP_EFFECT_TARGET.ACTOR));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Summon Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
#endregion

#region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            //Only advertise summon of bone golem if cult altar is in cult temple and is inside the home settlement of actors
            return poiTarget.gridTileLocation != null && poiTarget.gridTileLocation.structure.structureType == STRUCTURE_TYPE.CULT_TEMPLE;
        }
        return false;
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity invalidity = base.IsInvalid(node);
        if (!invalidity.isInvalid) {
            IPointOfInterest poiTarget = node.poiTarget;
            if (poiTarget.gridTileLocation == null || poiTarget.gridTileLocation.structure.structureType != STRUCTURE_TYPE.CULT_TEMPLE) {
                invalidity.isInvalid = true;
            } else {
                OtherData[] otherData = node.otherData;
                if(otherData == null || otherData.Length != 3) {
                    invalidity.isInvalid = true;
                } else {
                    for (int i = 0; i < otherData.Length; i++) {
                        if (otherData[i].obj == null) {
                            invalidity.isInvalid = true;
                            break;
                        } else {
                            Character targetCorpse = otherData[i].obj as Character;
                            if(targetCorpse == null) {
                                invalidity.isInvalid = true;
                                break;
                            } else {
                                if (!targetCorpse.isDead) {
                                    invalidity.isInvalid = true;
                                    break;
                                } else {
                                    IPointOfInterest target = targetCorpse;
                                    if (targetCorpse.grave != null) {
                                        target = targetCorpse.grave;
                                    }
                                    if (target.gridTileLocation == null || target.mapObjectVisual == null || target.isBeingCarriedBy != null || target.isBeingSeized || target.gridTileLocation.structure.structureType != STRUCTURE_TYPE.CULT_TEMPLE) {
                                        invalidity.isInvalid = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        return invalidity;
    }
#endregion

#region State Effects
    public void AfterSummonSuccess(ActualGoapNode goapNode) {
        OtherData[] otherData = goapNode.otherData;
        if (otherData != null && otherData.Length == 3) {
            for (int i = 0; i < otherData.Length; i++) {
                if (otherData[i].obj is Character targetCorpse) {
                    if (targetCorpse != null) {
                        if (!targetCorpse.isDead) {
                            continue;
                        }
                        IPointOfInterest target = targetCorpse;
                        if (targetCorpse.grave != null) {
                            target = targetCorpse.grave;
                        }
                        if(target.gridTileLocation != null && target.mapObjectVisual != null && target.isBeingCarriedBy == null && !target.isBeingSeized) {
                            if (targetCorpse.grave != null) {
                                //if character is at a tombstone, destroy tombstone and character marker.
                                targetCorpse.grave.SetRespawnCorpseOnDestroy(false);
                                targetCorpse.grave.gridTileLocation.structure.RemovePOI(targetCorpse.grave);
                            } else {
                                targetCorpse.DestroyMarker();
                                if (targetCorpse.currentRegion != null) {
                                    targetCorpse.currentRegion.RemoveCharacterFromLocation(targetCorpse);
                                }
                            }
                        }
                    }
                }
            }
        }

        LocationGridTile gridTile = goapNode.actor.gridTileLocation.GetFirstNeighbor();
        if(gridTile == null) {
            gridTile = goapNode.actor.gridTileLocation;
        }
        Summon boneGolem = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Bone_Golem, goapNode.actor.faction, homeLocation: goapNode.actor.homeSettlement, homeRegion: gridTile.parentMap.region, bypassIdeologyChecking: true);
        CharacterManager.Instance.PlaceSummonInitially(boneGolem, gridTile);
    }
#endregion

}