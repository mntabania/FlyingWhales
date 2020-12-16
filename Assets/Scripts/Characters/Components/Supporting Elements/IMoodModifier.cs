using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMoodModifier {
	
	string modifierName { get; }
	int moodModifier { get; }

	Log GetMoodEffectFlavorText(Character p_characterResponsible);
}
