using UnityEngine;
using System;

/// <summary>
/// The multiPlaneManager class is meant to hold the plane instances.
/// It also serves as a parent container for all Hit/Miss objects.
/// </summary>
public class multiPlaneManager : MonoBehaviour
{

    public bool hideMissedHits = true;
    public bool clearHits;

    [Tooltip("Array of plane transforms.")]
    public Transform[] Planes;

    [Tooltip("Array of plane colors.")]
    public Color[] PlanesColors;

    const bool includeInactive = true;

    // TODO : Generate planes positions and dimensions from file
    // public bool generateFromFile = false;


    void Start()
    {
        Debug.Log("Initializing multiPlaneManager.");
    }


    void Update()
    {
        // Retrieve current children gameobjects
        Transform[] planeManagerChildren = getChildren(includeInactive);

        if (clearHits)
        {
            clearAllHits(planeManagerChildren);
            clearHits = false;
            return;
        }

        // Hides missed hits
        foreach (Transform go in planeManagerChildren)
        {
            go.gameObject.SetActive(!(go.name == "Miss") || !hideMissedHits);
        }

    }


     // Define a static readonly array of possible game object names to destroy
    private static readonly string[] ObjectsToDestroy = { "Cube", "Hit", "Nearest", "Miss" };
    void clearAllHits(Transform[] hitsContainer)
    {
        // Destroy all children hit objects
        foreach (Transform go in hitsContainer)
        {
            // Check if the name is in the local array of objects to destroy
            if (Array.Exists(ObjectsToDestroy, name => name == go.name))
            {
                Destroy(go.gameObject);
            }
        }
    }


    public Transform[] getChildren(bool includeInactive)
    {
        return GetComponentsInChildren<Transform>(includeInactive);
    }


    public Transform[] getPlanes()
    {
        return Planes;
    }


    public void clearShapeHits(string shapeName)
    {
        // Destroy shape children hit objects
        Transform[] planeManagerChildren = getChildren(includeInactive);
        foreach (var child in planeManagerChildren)
        {
            if (child.name == "_hits_" + shapeName)
            {
                Debug.Log($"Destroying hits nested on {child.name}.");
                Transform [] nestedElements = child.GetComponentsInChildren<Transform>(includeInactive);
                clearAllHits(nestedElements);
                break;
            }
        }
    }

}
