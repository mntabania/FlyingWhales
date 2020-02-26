using UnityEngine;
using System.Collections;

public class UIMenuSettings {

    public InfoUIBase @base;
    public object data;

    public UIMenuSettings(InfoUIBase @base, object data) {
        this.@base = @base;
        this.data = data;
    }
}
