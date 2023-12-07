using UnityEngine;

public class multiPlaneManager : MonoBehaviour
{

    public bool hideMissedHits = true;
    public bool clear;

    public Transform[] Planes;

    // TODO : Generate planes positions and dimensions from file
    // public bool generateFromFile = false;


    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        bool includeInactive = true;
        Transform[] planeManagerChildren = GetComponentsInChildren<Transform>(includeInactive);
        if (clear)
        {
            // Destroy children objects
            foreach (Transform go in planeManagerChildren)
            {
                // Exclude planeManager at position [0] and child planes
                if (go.name == "Cube" || go.name == "Nearest" || go.name == "Miss")
                    Destroy(go.gameObject);
            }
            clear = false;
            return;
        }

        // Hides missed hits
        foreach (Transform go in planeManagerChildren)
        {
            go.gameObject.SetActive(!(go.name == "Miss") || !hideMissedHits);
        }

    }

    public Transform[] getPlanes()
    {
        return Planes;
    }

    // public void setShapeHits(string shapeName = "noname")
    // {
    //
    // }

    public void removeShapeHits(string shapeName)
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);
        foreach (var go in children)
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
