using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class MusicHaterData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.MUSIC_HATER;
    public override string name { get { return "Music Hater"; } }
    public override string description { get { return "Music Hater"; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.AFFLICTION; } }

    public MusicHaterData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }
}