﻿using Inner_Maps.Location_Structures;
using UnityEngine;

namespace Inner_Maps.Location_Structures {
    public class StructureConnector : MonoBehaviour {

        private bool _isOpen;
        private LocationGridTile _tileLocation;
        
        #region getters
        public bool isOpen => _isOpen;
        public LocationGridTile tileLocation => _tileLocation;
        #endregion

        #region Monobehaviours
        private void Awake() {
            _isOpen = true;
        }
#if UNITY_EDITOR
        private void OnDrawGizmos() {
            Gizmos.color = isOpen ? Color.green : Color.red;
            Gizmos.DrawWireCube(transform.position, new Vector3(1, 1, 1));
        }
#endif
        #endregion
        
        private void SetOpenState(bool state) {
            _isOpen = state;
        }
        public LocationGridTile GetLocationGridTileGivenCurrentPosition(InnerTileMap innerTileMap) {
            Vector3Int coordinates = innerTileMap.groundTilemap.WorldToCell(transform.position);
            return innerTileMap.GetTileFromMapCoordinates(coordinates.x, coordinates.y);
        }

        public void OnPlaceConnector(InnerTileMap innerTileMap) {
            _tileLocation = GetLocationGridTileGivenCurrentPosition(innerTileMap);
            if (_tileLocation != null) {
                _tileLocation.AddConnector(this);
                Messenger.AddListener<LocationGridTile>(StructureSignals.STRUCTURE_CONNECTOR_PLACED, OnStructureConnectorPlaced);
                Messenger.AddListener<LocationGridTile>(StructureSignals.STRUCTURE_CONNECTOR_REMOVED, OnStructureConnectorRemoved);
                Messenger.Broadcast(StructureSignals.STRUCTURE_CONNECTOR_PLACED, _tileLocation);
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
            _tileLocation = GetLocationGridTileGivenCurrentPosition(innerTileMap); //no need too add connector to tile, since that number is saved on SaveDataLocationGridTile.
        }
        #endregion
        
        #region Cleanup
        public void Reset() {
            if (_tileLocation != null) {
                Messenger.RemoveListener<LocationGridTile>(StructureSignals.STRUCTURE_CONNECTOR_PLACED, OnStructureConnectorPlaced);
                Messenger.RemoveListener<LocationGridTile>(StructureSignals.STRUCTURE_CONNECTOR_REMOVED, OnStructureConnectorRemoved);
                _tileLocation.RemoveConnector(this);
                Messenger.Broadcast(StructureSignals.STRUCTURE_CONNECTOR_REMOVED, _tileLocation);    
            }
            _isOpen = true;
            _tileLocation = null;
        }
        #endregion

        public override string ToString() {
            return $"Connector at {_tileLocation}";
        }
    }
}

public class SaveDataStructureConnector : SaveData<StructureConnector> {
    public bool isOpen;
    
    public override void Save(StructureConnector data) {
        base.Save(data);
        isOpen = data.isOpen;
    }
}