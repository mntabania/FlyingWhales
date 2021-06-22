using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Interrupts;
using UtilityScripts;
using System.Linq;

public class RumorComponent : CharacterComponent {
    private List<string> _rumorPool;
    private List<ActualGoapNode> _negativeInfoPool;
    private List<IPointOfInterest> _rumorTargetPool;

    private const int Max_Negative_Info = 40;

    #region getters
    public List<ActualGoapNode> negativeInfoPool => _negativeInfoPool;
    #endregion
    
    public RumorComponent() {
        _rumorPool = new List<string>();
        _rumorTargetPool = new List<IPointOfInterest>();
        _negativeInfoPool = new List<ActualGoapNode>();
    }
    public RumorComponent(SaveDataRumorComponent data) {
        _rumorPool = new List<string>();
        _rumorTargetPool = new List<IPointOfInterest>();
        _negativeInfoPool = new List<ActualGoapNode>();
    }

    #region General
    public ActualGoapNode GetRandomKnownNegativeInfo(Character spreadTargetCharacter, Character negativeCharacter) {
        if (_negativeInfoPool.Count == 0) {
            return null;
        }
        List<ActualGoapNode> filteredInfo = RuinarchListPool<ActualGoapNode>.Claim();
        for (int i = 0; i < _negativeInfoPool.Count; i++) {
            ActualGoapNode node = _negativeInfoPool[i];
            if(node.descriptionLog != null) {
                if(node.actor == negativeCharacter && node.poiTarget != spreadTargetCharacter) {
                    filteredInfo.Add(node);
                }
            }
        }
        ActualGoapNode chosen = null;
        if(filteredInfo.Count > 0) {
            chosen = CollectionUtilities.GetRandomElement(filteredInfo);
        }
        //_negativeInfoPool.Clear();
        // for (int i = 0; i < owner.logComponent.history.Count; i++) {
        //     Log history = owner.logComponent.history[i];
        //     if(history.logType == LOG_TYPE.Assumption || history.logType == LOG_TYPE.Witness || history.logType == LOG_TYPE.Informed) {
        //         if(history.node != null && history.node.descriptionLog != null) {
        //             if(history.node.actor == negativeCharacter && history.node.poiTarget != spreadTargetCharacter && history.node.GetReactableEffect(owner) == REACTABLE_EFFECT.Negative) {
        //                 _negativeInfoPool.Add(history.node);
        //             }
        //         }
        //     }
        // }
        RuinarchListPool<ActualGoapNode>.Release(filteredInfo);
        return chosen;
    }
    public void AddAssumedWitnessedOrInformedNegativeInfo(ActualGoapNode node) {
        if (!_negativeInfoPool.Contains(node)) {
            _negativeInfoPool.Add(node);
            node.SetIsNegativeInfo(true);
            if (_negativeInfoPool.Count > Max_Negative_Info) {
                ActualGoapNode previousNode = _negativeInfoPool[0];
                _negativeInfoPool.RemoveAt(0);
                if (previousNode != null) {
                    previousNode.SetIsNegativeInfo(false);
                    if (previousNode.isSupposedToBeInPool) {
                        previousNode.ProcessReturnToPool();
                    }
                }
            }
        }
    }
    public Rumor GenerateNewRandomRumor(Character spreadTargetCharacter, Character rumoredCharacter) {
        _rumorPool.Clear();
        _rumorPool.AddRange(CharacterManager.Instance.rumorWorthyActions);
        string chosenRumor = string.Empty;
        IPointOfInterest chosenTargetOfRumoredCharacter = null;
        while (_rumorPool.Count > 0 && chosenTargetOfRumoredCharacter == null) {
            string potentialRumor = _rumorPool[Random.Range(0, _rumorPool.Count)];
            IPointOfInterest targetOfRumoredCharacter = GetTargetOfRumorCharacter(spreadTargetCharacter, rumoredCharacter, potentialRumor);
            if(targetOfRumoredCharacter != null) {
                chosenRumor = potentialRumor;
                chosenTargetOfRumoredCharacter = targetOfRumoredCharacter;
            }
        }
        if (chosenTargetOfRumoredCharacter != null) {
            return CreateNewRumor(rumoredCharacter, chosenTargetOfRumoredCharacter, chosenRumor);
        }
        return null;
    }
    private IPointOfInterest GetTargetOfRumorCharacter(Character spreadTargetCharacter, Character rumoredCharacter, string identifier) {
        _rumorTargetPool.Clear();
        if(identifier == CharacterManager.Make_Love) {
            for (int i = 0; i < owner.relationshipContainer.charactersWithOpinion.Count; i++) {
                Character potentialCharacter = owner.relationshipContainer.charactersWithOpinion[i];
                if(potentialCharacter != spreadTargetCharacter && potentialCharacter != rumoredCharacter) {
                    if(!rumoredCharacter.relationshipContainer.HasRelationshipWith(potentialCharacter, RELATIONSHIP_TYPE.LOVER)) {
                        _rumorTargetPool.Add(potentialCharacter);
                    }
                }
            }
            if(_rumorTargetPool.Count > 0) {
                return _rumorTargetPool[Random.Range(0, _rumorTargetPool.Count)];
            }
        } else if (identifier == CharacterManager.Steal) {
            for (int i = 0; i < owner.relationshipContainer.charactersWithOpinion.Count; i++) {
                Character potentialCharacter = owner.relationshipContainer.charactersWithOpinion[i];
                if (potentialCharacter != spreadTargetCharacter && potentialCharacter != rumoredCharacter) {
                    if (potentialCharacter.ownedItems.Count > 0) {
                        _rumorTargetPool.Add(potentialCharacter);
                    }
                }
                if (_rumorTargetPool.Count > 0) {
                    Character chosenCharacter = _rumorTargetPool[Random.Range(0, _rumorTargetPool.Count)] as Character;
                    return chosenCharacter.ownedItems[Random.Range(0, chosenCharacter.ownedItems.Count)];
                }
            }
        } else if (identifier == CharacterManager.Poison_Food) {
            for (int i = 0; i < owner.relationshipContainer.charactersWithOpinion.Count; i++) {
                Character potentialCharacter = owner.relationshipContainer.charactersWithOpinion[i];
                if (potentialCharacter != spreadTargetCharacter && potentialCharacter != rumoredCharacter) {
                    if (potentialCharacter.ownedItems.Count > 0) {
                        for (int j = 0; j < potentialCharacter.ownedItems.Count; j++) {
                            TileObject ownedItem = potentialCharacter.ownedItems[j];
                            if(ownedItem.tileObjectType == TILE_OBJECT_TYPE.TABLE && ownedItem.gridTileLocation != null) {
                                _rumorTargetPool.Add(ownedItem);
                            }
                        }
                    }
                }
                if (_rumorTargetPool.Count > 0) {
                    return _rumorTargetPool[Random.Range(0, _rumorTargetPool.Count)];
                }
            }
        } else if (identifier == CharacterManager.Place_Trap) {
            for (int i = 0; i < owner.relationshipContainer.charactersWithOpinion.Count; i++) {
                Character potentialCharacter = owner.relationshipContainer.charactersWithOpinion[i];
                if (potentialCharacter != spreadTargetCharacter && potentialCharacter != rumoredCharacter) {
                    if (potentialCharacter.ownedItems.Count > 0) {
                        for (int j = 0; j < potentialCharacter.ownedItems.Count; j++) {
                            TileObject ownedItem = potentialCharacter.ownedItems[j];
                            if (!(ownedItem is StructureTileObject) && ownedItem.gridTileLocation != null && owner.gridTileLocation != null && ownedItem.gridTileLocation.structure.region == owner.gridTileLocation.structure.region) {
                                _rumorTargetPool.Add(ownedItem);
                            }
                        }
                    }
                }
                if (_rumorTargetPool.Count > 0) {
                    return _rumorTargetPool[Random.Range(0, _rumorTargetPool.Count)];
                }
            }
        } else if (identifier == CharacterManager.Drink_Blood) {
            for (int i = 0; i < owner.relationshipContainer.charactersWithOpinion.Count; i++) {
                Character potentialCharacter = owner.relationshipContainer.charactersWithOpinion[i];
                if (potentialCharacter != spreadTargetCharacter && potentialCharacter != rumoredCharacter) {
                    _rumorTargetPool.Add(potentialCharacter);
                }
            }
            if (_rumorTargetPool.Count > 0) {
                return _rumorTargetPool[Random.Range(0, _rumorTargetPool.Count)];
            }
        } else if (identifier == CharacterManager.Flirt) {
            for (int i = 0; i < owner.relationshipContainer.charactersWithOpinion.Count; i++) {
                Character potentialCharacter = owner.relationshipContainer.charactersWithOpinion[i];
                if (potentialCharacter != spreadTargetCharacter && potentialCharacter != rumoredCharacter) {
                    if (!rumoredCharacter.relationshipContainer.HasRelationshipWith(potentialCharacter, RELATIONSHIP_TYPE.LOVER)) {
                        _rumorTargetPool.Add(potentialCharacter);
                    }
                }
            }
            if (_rumorTargetPool.Count > 0) {
                return _rumorTargetPool[Random.Range(0, _rumorTargetPool.Count)];
            }
        } else if (identifier == CharacterManager.Transform_To_Wolf) {
            return rumoredCharacter;
        }
        return null;
    }
    public Character GetRandomSpreadRumorOrNegativeInfoTarget(Character rumoredCharacter) {
        Character chosenCharacter = null;
        int charactersWithOpinionCount = owner.relationshipContainer.charactersWithOpinion.Count(CanShareInfoTo);
        if(charactersWithOpinionCount > 2) {
            while (chosenCharacter == null) {
                Character potentialCharacter = owner.relationshipContainer.charactersWithOpinion[Random.Range(0, owner.relationshipContainer.charactersWithOpinion.Count)];
                if (potentialCharacter != rumoredCharacter && CanShareInfoTo(potentialCharacter)) {
                    chosenCharacter = potentialCharacter;
                }
            }
        } else if (charactersWithOpinionCount == 1) {
            Character potentialCharacter = owner.relationshipContainer.charactersWithOpinion[0];
            if (potentialCharacter != rumoredCharacter && CanShareInfoTo(potentialCharacter)) {
                chosenCharacter = potentialCharacter;
            }
        } else if (charactersWithOpinionCount == 2) {
            for (int i = 0; i < owner.relationshipContainer.charactersWithOpinion.Count; i++) {
                Character potentialCharacter = owner.relationshipContainer.charactersWithOpinion[i];
                if (potentialCharacter != rumoredCharacter && CanShareInfoTo(potentialCharacter)) {
                    chosenCharacter = potentialCharacter;
                    break;
                }
            }
        }
        return chosenCharacter;
    }
    private bool CanShareInfoTo(Character character) {
        return !character.isDead && !character.traitContainer.HasTrait("Enslaved", "Travelling");
    }
    public Rumor CreateNewRumor(Character rumoredCharacter, IPointOfInterest targetOfRumoredCharacter, string identifier) {
        IRumorable rumorable = null;
        if (identifier == CharacterManager.Flirt || identifier == CharacterManager.Transform_To_Wolf) {
            Interrupt interrupt = null;
            Log effectLog = default;
            if (identifier == CharacterManager.Flirt) {
                interrupt = InteractionManager.Instance.GetInterruptData(INTERRUPT.Flirt);
                effectLog = interrupt.CreateEffectLog(rumoredCharacter, targetOfRumoredCharacter, "flirted_back");
            } else if (identifier == CharacterManager.Transform_To_Wolf) {
                interrupt = InteractionManager.Instance.GetInterruptData(INTERRUPT.Transform_To_Wolf);
                effectLog = interrupt.CreateEffectLog(rumoredCharacter, targetOfRumoredCharacter);
            }
            //Note: This particular interrupt holder, if used, will not be brought back to the object pool because we do not exactly know when this particular rumorable will not be used anymore
            //It is uncertain when this will be not used, so we must not reset its data
            InterruptHolder interruptHolder = ObjectPoolManager.Instance.CreateNewInterrupt();
            interruptHolder.Initialize(interrupt, rumoredCharacter, targetOfRumoredCharacter, string.Empty, string.Empty);
            interruptHolder.SetEffectLog(effectLog);
            rumorable = interruptHolder;
        } else {
            INTERACTION_TYPE actionType = INTERACTION_TYPE.NONE;
            if (identifier == CharacterManager.Make_Love) {
                actionType = INTERACTION_TYPE.MAKE_LOVE;
            } else if (identifier == CharacterManager.Steal) {
                actionType = INTERACTION_TYPE.STEAL;
            } else if (identifier == CharacterManager.Poison_Food) {
                actionType = INTERACTION_TYPE.POISON;
            } else if (identifier == CharacterManager.Place_Trap) {
                actionType = INTERACTION_TYPE.BOOBY_TRAP;
            } else if (identifier == CharacterManager.Drink_Blood) {
                actionType = INTERACTION_TYPE.DRINK_BLOOD;
            }
            ActualGoapNode action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[actionType], rumoredCharacter, targetOfRumoredCharacter, null, 0);
            if (identifier == CharacterManager.Poison_Food) {
                if(targetOfRumoredCharacter.gridTileLocation != null) {
                    action.SetTargetStructure(targetOfRumoredCharacter.gridTileLocation.structure);
                }
            }
            rumorable = action;
        }
        if(rumorable != null) {
            Rumor rumor = new Rumor(owner, rumoredCharacter);
            rumorable.SetAsRumor(rumor);
            return rumor;
        }
        return null;
        //throw new System.Exception("Cannot create new rumor for identifier " + identifier + " because rumorable is null!");
    }
    public Rumor CreateNewRumor(Character rumoredCharacter, IPointOfInterest targetOfRumoredCharacter, INTERACTION_TYPE actionType) {
        if (rumoredCharacter != null && targetOfRumoredCharacter != null) {
            ActualGoapNode action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[actionType], rumoredCharacter, targetOfRumoredCharacter, null, 0);
            if (actionType == INTERACTION_TYPE.POISON) {
                if (targetOfRumoredCharacter.gridTileLocation != null) {
                    action.SetTargetStructure(targetOfRumoredCharacter.gridTileLocation.structure);
                }
            }
            Rumor rumor = new Rumor(owner, rumoredCharacter);
            action.SetAsRumor(rumor);
            return rumor;
        }
        return null;
    }
    #endregion

    #region Loading
    public void LoadReferences(SaveDataRumorComponent data) {
        for (int i = 0; i < data.negativeInfoIDs.Count; i++) {
            string id = data.negativeInfoIDs[i];
            ActualGoapNode node = DatabaseManager.Instance.actionDatabase.GetActionByPersistentID(id);
            if (node.actor == null || node.poiTarget == null) {
                //Do not add negative info if actor or target is no longer in the world or does not exist
            } else {
                _negativeInfoPool.Add(node);
            }
        }
    }
    #endregion
}

[System.Serializable]
public class SaveDataRumorComponent : SaveData<RumorComponent> {

    public List<string> negativeInfoIDs;
    
    #region Overrides
    public override void Save(RumorComponent data) {
        negativeInfoIDs = new List<string>();
        for (int i = 0; i < data.negativeInfoPool.Count; i++) {
            ActualGoapNode node = data.negativeInfoPool[i];
            negativeInfoIDs.Add(node.persistentID);
            SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(node);
        }
    }

    public override RumorComponent Load() {
        RumorComponent component = new RumorComponent(this);
        return component;
    }
    #endregion
}