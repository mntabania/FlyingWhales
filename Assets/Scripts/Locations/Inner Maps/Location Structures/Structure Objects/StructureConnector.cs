using System;
using Inner_Maps.Location_Structures;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;
namespace Inner_Maps.Location_Structures {
    public class StructureConnector : MonoBehaviour {

        private bool _isOpen;
        private string _ownerID; //the persistent ID of the structure that owns this connector.
        private LocationGridTile _tileLocation;
        
        #region getters
        public bool isOpen => _isOpen;
        public LocationStructure ownerStructure => string.IsNullOrEmpty(_ownerID) ? null : DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(_ownerID);
        #endregion
        
        #region Monobehaviours
        private void Awake() {
            _isOpen = true;
        }
        private void OnDrawGizmos() {
            Gizmos.color = isOpen ? Color.green : Color.red;
            Gizmos.DrawWireCube(transform.position, new Vector3(1, 1, 1));
        }
        #endregion

        public void SetOwner([NotNull]LocationStructure owner) {
            _ownerID = owner.persistentID;
        }
        public void SetOpenState(bool state) {
            _isOpen = state;
        }
        public LocationGridTile GetLocationGridTileGivenCurrentPosition(InnerTileMap innerTileMap) {
            Vector3Int coordinates = innerTileMap.groundTilemap.WorldToCell(transform.position);
            return innerTileMap.GetTileFromMapCoordinates(coordinates.x, coordinates.y);
        }

        public void OnPlaceConnector(InnerTileMap innerTileMap) {
            _tileLocation = GetLocationGridTileGivenCurrentPosition(innerTileMap);
            if (_tileLocation != null) {
                _tileLocation.AddConnector();
                Messenger.AddListener<LocationGridTile>(Signals.STRUCTURE_CONNECTOR_PLACED, OnStructureConnectorPlaced);
                Messenger.AddListener<LocationGridTile>(Signals.STRUCTURE_CONNECTOR_REMOVED, OnStructureConnectorRemoved);
                Messenger.Broadcast(Signals.STRUCTURE_CONNECTOR_PLACED, _tileLocation);
            }
        }

        #region Listeners
        private void OnStructureConnectorPlaced(LocationGridTile placedOnTile) {
            if (placedOnTile == _tileLocation) {
                if (placedOnTile.connectorsOnTile > 1) {
                    SetOpenState(false);
                }
            }
        }
        private void OnStructureConnectorRemoved(LocationGridTile placedOnTile) {
            if (placedOnTile == _tileLocation) {
                if (placedOnTile.connectorsOnTile == 1) {
                    SetOpenState(true); //this is the only connector left on that tile.
                }
            }
        }
        #endregion

        #region Loading
        public void LoadReferences(SaveDataStructureConnector saveData, InnerTileMap innerTileMap) {
            _isOpen = saveData.isOpen;
            _ownerID = saveData.ownerID;
            _tileLocation = GetLocationGridTileGivenCurrentPosition(innerTileMap); //no need too add connector to tile, since that number is saved on SaveDataLocationGridTile.
        }
        #endregion
        
        #region Cleanup
        public void Reset() {
            if (_tileLocation != null) {
                Messenger.RemoveListener<LocationGridTile>(Signals.STRUCTURE_CONNECTOR_PLACED, OnStructureConnectorPlaced);
                Messenger.RemoveListener<LocationGridTile>(Signals.STRUCTURE_CONNECTOR_REMOVED, OnStructureConnectorRemoved);
                _tileLocation.RemoveConnector();
                Messenger.Broadcast(Signals.STRUCTURE_CONNECTOR_REMOVED, _tileLocation);    
            }
            _isOpen = true;
            _ownerID = string.Empty;
        }
        #endregion

        public override string ToString() {
            return $"Connector at {transform.position.ToString()}";
        }
    }
}

public class SaveDataStructureConnector : SaveData<StructureConnector> {
    public bool isOpen;
    public string connectedToID;
    public string ownerID;
    
    public override void Save(StructureConnector data) {
        base.Save(data);
        isOpen = data.isOpen;
        if (data.ownerStructure != null) {
            ownerID = data.ownerStructure.persistentID;
        }
    }
}