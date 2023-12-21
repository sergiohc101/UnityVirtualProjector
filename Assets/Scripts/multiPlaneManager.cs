using UnityEngine;

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


    public Transform[] getChildren(bool includeInactive)
    {
        return GetComponentsInChildren<Transform>(includeInactive);
    }


    public void clearAllHits(Transform[] hitsContainer)
    {
        // Destroy all children hit objects
        foreach (Transform go in hitsContainer)
        {
            // Exclude planeManager at position [0] and child planes
            if (go.name == "Cube" || go.name == "Hit" || go.name == "Nearest" || go.name == "Miss")
                Destroy(go.gameObject);
        }
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
