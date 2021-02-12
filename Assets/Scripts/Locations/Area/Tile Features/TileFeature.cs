namespace Locations.Tile_Features {
	public class TileFeature  {

		public string name { get; protected set; }
		public string description { get; protected set; }

		#region Virtuals
		public virtual void OnAddFeature(Area tile) { }
		public virtual void OnRemoveFeature(Area tile) { }
		public virtual void OnDemolishLandmark(Area tile, LANDMARK_TYPE demolishedLandmarkType) { }
		public virtual void GameStartActions(Area tile) { }
		#endregion

		#region Loading
		public virtual void LoadedGameStartActions(Area tile) {
			GameStartActions(tile); //by default features will behave the same as normal when loaded, override this if specific feature has a specific implementation for loading
		}
		#endregion

	}
}


