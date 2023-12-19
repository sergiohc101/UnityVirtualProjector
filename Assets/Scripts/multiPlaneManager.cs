using UnityEngine;

/// <summary>
/// The multiPlaneManager class is meant to hold the plane instances.
/// It also serves as a parent container for all Hit/Miss objects.
/// </summary>
public class multiPlaneManager : MonoBehaviour
{

    public bool hideMissedHits = true;
    public bool clearHits;
    public Transform[] Planes;

    const bool includeInactive = true;
    Transform[] planeManagerChildren;

    // TODO : Generate planes positions and dimensions from file
    // public bool generateFromFile = false;


    void Start()
    {
        Debug.Log("Initializing multiPlaneManager.");
    }


    void Update()
    {
        // Update children once per frame
        planeManagerChildren = GetComponentsInChildren<Transform>(includeInactive);

        if (clearHits)
        {
            clearAllHits();
            clearHits = false;
            return;
        }

        // Hides missed hits
        foreach (Transform go in planeManagerChildren)
        {
            go.gameObject.SetActive(!(go.name == "Miss") || !hideMissedHits);
        }

    }


    public void clearAllHits()
    {
        // Destroy all children hit objects
        foreach (Transform go in planeManagerChildren)
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


    public Transform[] getChildren(bool includeInactive)
    {
        // return GetComponentsInChildren<Transform>(includeInactive);
        return planeManagerChildren;
    }


    public void clearShapeHits(string shapeName)
    {
        // Destroy shape children hit objects
        foreach (var go in planeManagerChildren)
        {
            if (go.name == "_hits_" + shapeName)
            {
                Debug.Log("Destroying children hits: " + go.name);
                for (int k = go.childCount - 1; k > 0; k--)
                {
                    GameObject.Destroy(go.GetChild(k).gameObject);
                }
                break;
            }
        }
    }

}
