using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridElement : MonoBehaviour
{
    [SerializeField] Transform pivot;

    public Vector3 GetBackPosition()
    {
        return pivot.position;
    }
}
