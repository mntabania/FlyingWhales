using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actionables {
	/// <summary>
	/// Interface for classes that can be targeted by player actions.
	/// </summary>
	public interface IPlayerActionTarget {

		List<Actionables.PlayerAction> actions { get; }

		void ConstructDefaultActions();
		void AddPlayerAction(PlayerAction action);
		void RemovePlayerAction(PlayerAction action);
        void RemovePlayerAction(string actionName);
		void ClearPlayerActions();
	}	
}

