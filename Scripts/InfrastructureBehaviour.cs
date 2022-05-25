
using System.Collections.Generic;
using UnityEngine;
[RequireComponent (typeof(Reader))]

public abstract class InfrastructureBehaviour: MonoBehaviour
{

    protected Reader map;

    void Awake() {
        map = GetComponent<Reader>();
    }
}
