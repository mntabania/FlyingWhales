using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Interrupts;

public class RumorComponent {
    public Character owner { get; private set; }

    private List<string> _rumorPool;
    private List<IPointOfInterest> _rumorTargetPool;

    public RumorComponent(Character owner) {
        this.owner = owner;
        _rumorPool = new List<string>();
        _rumorTargetPool = new List<IPointOfInterest>();
    }

    #region General
    public Rumor GenerateNewRandomRumor(Character spreadTargetCharacter, Character rumoredCharacter) {
        _rumorPool.Clear();
        _rumorPool.AddRange(CharacterManager.Instance.rumorWorthyActions);
        string chosenRumor = string.Empty;
        IPointOfInterest chosenTargetOfRumoredCharacter = null;
        while (_rumorPool.Count > 0 && chosenTargetOfRumoredCharacter == null) {
            string potentialRumor = _rumorPool[UnityEngine.Random.Range(0, _rumorPool.Count)];
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
                return _rumorTargetPool[UnityEngine.Random.Range(0, _rumorTargetPool.Count)];
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
                    Character chosenCharacter = _rumorTargetPool[UnityEngine.Random.Range(0, _rumorTargetPool.Count)] as Character;
                    return chosenCharacter.ownedItems[UnityEngine.Random.Range(0, chosenCharacter.ownedItems.Count)];
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
                    return _rumorTargetPool[UnityEngine.Random.Range(0, _rumorTargetPool.Count)];
                }
            }
        } else if (identifier == CharacterManager.Place_Trap) {
            for (int i = 0; i < owner.relationshipContainer.charactersWithOpinion.Count; i++) {
                Character potentialCharacter = owner.relationshipContainer.charactersWithOpinion[i];
                if (potentialCharacter != spreadTargetCharacter && potentialCharacter != rumoredCharacter) {
                    if (potentialCharacter.ownedItems.Count > 0) {
                        for (int j = 0; j < potentialCharacter.ownedItems.Count; j++) {
                            TileObject ownedItem = potentialCharacter.ownedItems[j];
                            if (ownedItem.gridTileLocation != null && owner.gridTileLocation != null && ownedItem.gridTileLocation.structure.location == owner.gridTileLocation.structure.location) {
                                _rumorTargetPool.Add(ownedItem);
                            }
                        }
                    }
                }
                if (_rumorTargetPool.Count > 0) {
                    return _rumorTargetPool[UnityEngine.Random.Range(0, _rumorTargetPool.Count)];
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
                return _rumorTargetPool[UnityEngine.Random.Range(0, _rumorTargetPool.Count)];
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
                return _rumorTargetPool[UnityEngine.Random.Range(0, _rumorTargetPool.Count)];
            }
        } else if (identifier == CharacterManager.Transform_To_Wolf) {
            return rumoredCharacter;
        }
        return null;
    }
    public Character GetRandomSpreadRumorTarget(Character rumoredCharacter) {
        Character chosenCharacter = null;
        int charactersWithOpinionCount = owner.relationshipContainer.charactersWithOpinion.Count;
        if(charactersWithOpinionCount > 2) {
            while (chosenCharacter == null) {
                Character potentialCharacter = owner.relationshipContainer.charactersWithOpinion[UnityEngine.Random.Range(0, owner.relationshipContainer.charactersWithOpinion.Count)];
                if (potentialCharacter != rumoredCharacter) {
                    chosenCharacter = potentialCharacter;
                }
            }
        } else if (charactersWithOpinionCount == 1) {
            Character potentialCharacter = owner.relationshipContainer.charactersWithOpinion[0];
            if (potentialCharacter != rumoredCharacter) {
                chosenCharacter = potentialCharacter;
            }
        } else if (charactersWithOpinionCount == 2) {
            for (int i = 0; i < owner.relationshipContainer.charactersWithOpinion.Count; i++) {
                Character potentialCharacter = owner.relationshipContainer.charactersWithOpinion[i];
                if (potentialCharacter != rumoredCharacter) {
                    chosenCharacter = potentialCharacter;
                    break;
                }
            }
        }
        return chosenCharacter;
    }
    public Rumor CreateNewRumor(Character rumoredCharacter, IPointOfInterest targetOfRumoredCharacter, string identifier) {
        IRumorable rumorable = null;
        if (identifier == CharacterManager.Flirt || identifier == CharacterManager.Transform_To_Wolf) {
            Interrupt interrupt = null;
            Log effectLog = null;
            if (identifier == CharacterManager.Flirt) {
                interrupt = InteractionManager.Instance.GetInterruptData(INTERRUPT.Flirt);
                effectLog = interrupt.CreateEffectLog(rumoredCharacter, targetOfRumoredCharacter, "flirted_back");
            } else if (identifier == CharacterManager.Transform_To_Wolf) {
                interrupt = InteractionManager.Instance.GetInterruptData(INTERRUPT.Transform_To_Wolf);
                effectLog = interrupt.CreateEffectLog(rumoredCharacter, targetOfRumoredCharacter);
            }
            rumorable = new InterruptHolder(interrupt, rumoredCharacter, targetOfRumoredCharacter, effectLog);
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
            rumorable = new ActualGoapNode(InteractionManager.Instance.goapActionData[actionType], rumoredCharacter, targetOfRumoredCharacter, null, 0);
        }
        if(rumorable != null) {
            return new Rumor(owner, rumorable);
        }
        throw new System.Exception("Cannot create new rumor for identifier " + identifier + " because rumorable is null!");
    }
    #endregion
}
