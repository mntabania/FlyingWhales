using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine.Assertions;
namespace Characters.Components {
    public class CharacterEventDispatcher {

        public interface ITraitListener {
            void OnCharacterGainedTrait(Character p_character, Trait p_gainedTrait);
            void OnCharacterLostTrait(Character p_character, Trait p_lostTrait, Character p_removedBy);
        }
        public interface ICarryListener {
            void OnCharacterCarried(Character p_character, Character p_carriedBy);
        }
        public interface ILocationListener {
            void OnCharacterLeftStructure(Character p_character, LocationStructure p_leftStructure);
            void OnCharacterArrivedAtStructure(Character p_character, LocationStructure p_leftStructure);
            void OnCharacterArrivedAtSettlement(Character p_character, NPCSettlement p_settlement);
        }
        public interface IDeathListener {
            void OnCharacterSubscribedToDied(Character p_character);
        }
        public interface IHomeStructureListener {
            void OnCharacterSetHomeStructure(Character p_character, LocationStructure p_homeStructure);
            void OnObjectPlacedInHomeDwelling(Character p_character, LocationStructure p_homeStructure, TileObject p_placedObject);
            void OnObjectRemovedFromHomeDwelling(Character p_character, LocationStructure p_homeStructure, TileObject p_removedObject);
        }
        public interface IEquipmentListener {
            void OnWeaponEquipped(Character p_character, EquipmentItem p_weapon);
            void OnWeaponUnequipped(Character p_character, EquipmentItem p_weapon);
            void OnArmorEquipped(Character p_character, EquipmentItem p_weapon);
            void OnArmorUnequipped(Character p_character, EquipmentItem p_weapon);
            void OnAccessoryEquipped(Character p_character, EquipmentItem p_weapon);
            void OnAccessoryUnequipped(Character p_character, EquipmentItem p_weapon);
        }
        public interface IInventoryListener {
            void OnItemObtained(Character p_character, TileObject p_obtainedItem);
            void OnItemLost(Character p_character, TileObject p_lostItem);
        }
        public interface IFactionListener {
            void OnJoinFaction(Character p_character, Faction p_newFaction);
        }

        private System.Action<Character, Trait> _characterGainedTrait;
        private System.Action<Character, Trait, Character> _characterLostTrait;
        private System.Action<Character, Character> _characterCarried;
        private System.Action<Character, LocationStructure> _characterLeftStructure;
        private System.Action<Character, LocationStructure> _characterArrivedAtStructure;
        private System.Action<Character> _characterDied;
        private System.Action<Character, LocationStructure> _characterSetHomeStructure;
        private System.Action<Character, LocationStructure, TileObject> _objectPlacedInCharacterDwelling;
        private System.Action<Character, LocationStructure, TileObject> _objectRemovedFromCharacterDwelling;
        private System.Action<Character, EquipmentItem> _weaponEquipped;
        private System.Action<Character, EquipmentItem> _weaponUnequipped;
        private System.Action<Character, EquipmentItem> _armorEquipped;
        private System.Action<Character, EquipmentItem> _armorUnequipped;
        private System.Action<Character, EquipmentItem> _accessoryEquipped;
        private System.Action<Character, EquipmentItem> _accessoryUnequipped;
        private System.Action<Character, TileObject> _itemObtained;
        private System.Action<Character, TileObject> _itemLost;
        private System.Action<Character, Faction> _joinedFaction;
        private System.Action<Character, NPCSettlement> _characterArrivedAtSettlement;

        #region Gained Trait
        public void SubscribeToCharacterGainedTrait(ITraitListener p_traitListener) {
            _characterGainedTrait += p_traitListener.OnCharacterGainedTrait;
        }
        public void UnsubscribeToCharacterGainedTrait(ITraitListener p_traitListener) {
            _characterGainedTrait -= p_traitListener.OnCharacterGainedTrait;
        }
        public void ExecuteCharacterGainedTrait(Character p_character, Trait p_gainedTrait) {
            _characterGainedTrait?.Invoke(p_character, p_gainedTrait);
        }
        #endregion

        #region Lost Trait
        public void SubscribeToCharacterLostTrait(ITraitListener p_traitListener) {
            _characterLostTrait += p_traitListener.OnCharacterLostTrait;
        }
        public void UnsubscribeToCharacterLostTrait(ITraitListener p_traitListener) {
            _characterLostTrait -= p_traitListener.OnCharacterLostTrait;
        }
        public void ExecuteCharacterLostTrait(Character p_character, Trait p_lostTrait, Character p_removedBy) {
            _characterLostTrait?.Invoke(p_character, p_lostTrait, p_removedBy);
        }
        #endregion

        #region Carried
        public void SubscribeToCharacterCarried(ICarryListener p_carryListener) {
            _characterCarried += p_carryListener.OnCharacterCarried;
        }
        public void UnsubscribeToCharacterCarried(ICarryListener p_carryListener) {
            _characterCarried -= p_carryListener.OnCharacterCarried;
        }
        public void ExecuteCarried(Character p_character, Character p_carriedBy) {
            _characterCarried?.Invoke(p_character, p_carriedBy);
        }
        #endregion
        
        #region Structure
        public void SubscribeToCharacterLeftStructure(ILocationListener p_listener) {
            _characterLeftStructure += p_listener.OnCharacterLeftStructure;
        }
        public void UnsubscribeToCharacterLeftStructure(ILocationListener p_listener) {
            _characterLeftStructure -= p_listener.OnCharacterLeftStructure;
        }
        public void ExecuteCharacterLeftStructure(Character p_character, LocationStructure p_leftStructure) {
            _characterLeftStructure?.Invoke(p_character, p_leftStructure);
        }
        public void SubscribeToCharacterArrivedAtStructure(ILocationListener p_listener) {
            _characterArrivedAtStructure += p_listener.OnCharacterArrivedAtStructure;
        }
        public void UnsubscribeToCharacterArrivedAtStructure(ILocationListener p_listener) {
            _characterArrivedAtStructure -= p_listener.OnCharacterLeftStructure;
        }
        public void ExecuteCharacterArrivedAtStructure(Character p_character, LocationStructure p_arrivedStructure) {
            _characterArrivedAtStructure?.Invoke(p_character, p_arrivedStructure);
        }
        #endregion

        #region Death
        public void SubscribeToCharacterDied(IDeathListener p_listener) {
            _characterDied += p_listener.OnCharacterSubscribedToDied;
        }
        public void UnsubscribeToCharacterDied(IDeathListener p_listener) {
            _characterDied -= p_listener.OnCharacterSubscribedToDied;
        }
        public void ExecuteCharacterDied(Character p_character) {
            _characterDied?.Invoke(p_character);
        }
        #endregion

        #region Home Structure
        public void SubscribeToCharacterSetHomeStructure(IHomeStructureListener p_listener) {
            _characterSetHomeStructure += p_listener.OnCharacterSetHomeStructure;
        }
        public void UnsubscribeToCharacterSetHomeStructure(IHomeStructureListener p_listener) {
            _characterSetHomeStructure -= p_listener.OnCharacterSetHomeStructure;
        }
        public void ExecuteCharacterSetHomeStructure(Character p_character, LocationStructure p_structure) {
            _characterSetHomeStructure?.Invoke(p_character, p_structure);
        }
        public void SubscribeToObjectPlacedInCharactersDwelling(IHomeStructureListener p_listener) {
            _objectPlacedInCharacterDwelling += p_listener.OnObjectPlacedInHomeDwelling;
        }
        public void UnsubscribeToObjectPlacedInCharactersHome(IHomeStructureListener p_listener) {
            _objectPlacedInCharacterDwelling -= p_listener.OnObjectPlacedInHomeDwelling;
        }
        public void ExecuteObjectPlacedInCharactersHome(Character p_character, LocationStructure p_structure, TileObject p_tileObject) {
            // Assert.IsTrue(p_structure == p_character.homeStructure, $"Object placed on home structure event was fired with object {p_tileObject?.nameWithID} at {p_structure?.name} but it is not {p_character?.name}'s home structure which is {p_character?.homeStructure?.name}");
            _objectPlacedInCharacterDwelling?.Invoke(p_character, p_structure, p_tileObject);
        }
        public void SubscribeToObjectRemovedFromCharactersDwelling(IHomeStructureListener p_listener) {
            _objectRemovedFromCharacterDwelling += p_listener.OnObjectRemovedFromHomeDwelling;
        }
        public void UnsubscribeToObjectRemovedFromCharactersHome(IHomeStructureListener p_listener) {
            _objectRemovedFromCharacterDwelling -= p_listener.OnObjectRemovedFromHomeDwelling;
        }
        public void ExecuteObjectRemovedFromCharactersHome(Character p_character, LocationStructure p_structure, TileObject p_tileObject) {
            // Assert.IsTrue(p_structure == p_character.homeStructure, $"Object removed from home structure event was fired with object {p_tileObject?.nameWithID} at {p_structure?.name} but it is not {p_character?.name}'s home structure which is {p_character?.homeStructure?.name}");
            _objectRemovedFromCharacterDwelling?.Invoke(p_character, p_structure, p_tileObject);
        }
        #endregion

        #region Equipment
        public void SubscribeToWeaponEvents(IEquipmentListener p_listener) {
            _weaponEquipped += p_listener.OnWeaponEquipped;
            _weaponUnequipped += p_listener.OnWeaponUnequipped;
            _armorEquipped += p_listener.OnArmorEquipped;
            _armorUnequipped += p_listener.OnArmorUnequipped;
            _accessoryEquipped += p_listener.OnAccessoryEquipped;
            _accessoryUnequipped += p_listener.OnAccessoryUnequipped;
        }
        public void UnsubscribeToWeaponEvents(IEquipmentListener p_listener) {
            _weaponEquipped -= p_listener.OnWeaponEquipped;
            _weaponUnequipped -= p_listener.OnWeaponUnequipped;
            _armorEquipped -= p_listener.OnArmorEquipped;
            _armorUnequipped -= p_listener.OnArmorUnequipped;
            _accessoryEquipped -= p_listener.OnAccessoryEquipped;
            _accessoryUnequipped -= p_listener.OnAccessoryUnequipped;
        }
        public void ExecuteWeaponEquipped(Character p_character, EquipmentItem p_equipment) {
            Assert.IsNotNull(p_equipment);
            _weaponEquipped?.Invoke(p_character, p_equipment);
        }
        public void ExecuteWeaponUnequipped(Character p_character, EquipmentItem p_equipment) {
            _weaponUnequipped?.Invoke(p_character, p_equipment);
        }
        public void ExecuteArmorEquipped(Character p_character, EquipmentItem p_equipment) {
            Assert.IsNotNull(p_equipment);
            _armorEquipped?.Invoke(p_character, p_equipment);
        }
        public void ExecuteArmorUnequipped(Character p_character, EquipmentItem p_equipment) {
            _armorUnequipped?.Invoke(p_character, p_equipment);
        }
        public void ExecuteAccessoryEquipped(Character p_character, EquipmentItem p_equipment) {
            Assert.IsNotNull(p_equipment);
            _accessoryEquipped?.Invoke(p_character, p_equipment);
        }
        public void ExecuteAccessoryUnequipped(Character p_character, EquipmentItem p_equipment) {
            _accessoryUnequipped?.Invoke(p_character, p_equipment);
        }
        #endregion

        #region Items
        public void SubscribeToInventoryEvents(IInventoryListener p_listener) {
            _itemObtained += p_listener.OnItemObtained;
            _itemLost += p_listener.OnItemLost;
        }
        public void UnsubscribeToInventoryEvents(IInventoryListener p_listener) {
            _itemObtained -= p_listener.OnItemObtained;
            _itemLost -= p_listener.OnItemLost;
        }
        public void ExecuteItemObtained(Character p_character, TileObject p_item) {
            _itemObtained?.Invoke(p_character, p_item);
        }
        public void ExecuteItemLost(Character p_character, TileObject p_item) {
            _itemLost?.Invoke(p_character, p_item);
        }
        #endregion

        #region Faction
        public void SubscribeToFactionEvents(IFactionListener p_listener) {
            _joinedFaction += p_listener.OnJoinFaction;
        }
        public void UnsubscribeToFactionEvents(IFactionListener p_listener) {
            _joinedFaction -= p_listener.OnJoinFaction;
        }
        public void ExecuteJoinedFaction(Character p_character, Faction p_faction) {
            _joinedFaction?.Invoke(p_character, p_faction);
        }
        #endregion

        #region Settlement
        public void SubscribeToCharacterArrivedAtSettlement(ILocationListener p_listener) {
            _characterArrivedAtSettlement += p_listener.OnCharacterArrivedAtSettlement;
        }
        public void UnsubscribeToCharacterArrivedAtSettlement(ILocationListener p_listener) {
            _characterArrivedAtSettlement -= p_listener.OnCharacterArrivedAtSettlement;
        }
        public void ExecuteCharacterArrivedAtSettlement(Character p_character, NPCSettlement p_settlement) {
            _characterArrivedAtSettlement?.Invoke(p_character, p_settlement);
        }
        #endregion
    }
}