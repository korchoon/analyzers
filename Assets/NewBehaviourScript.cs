using System;
using UnityEngine;

public sealed class NewBehaviourScript : MonoBehaviour {
    ref int ByRef () => throw new NotImplementedException ();

    void Start () {
        ref var ok = ref ByRef ();
        var notOk = ByRef ();
    }
}