using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlaneSpawnZone : MonoBehaviour
{
    public int Id;
    public string Description;

    public Bounds GetBounds()
    {
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
            return renderer.bounds;

        return new Bounds(transform.position, transform.localScale);
    }
}