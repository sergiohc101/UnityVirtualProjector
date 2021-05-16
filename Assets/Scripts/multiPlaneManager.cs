using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class multiPlaneManager : MonoBehaviour
{

    public bool hideMissedHits;
    public bool clear;

    public Transform[] Planes;

    public bool generateFromFile = false; // TODO


    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (clear)
        {
            //delete all children objects in transform
            Transform[] children = GetComponentsInChildren<Transform>(true);
            int count = 0;
            foreach (Transform go in children)
            {
                if (++count > 1) // Exclude this planeManager at position [0] and child planes
                    if (go.name == "Cube" || go.name == "Nearest" || go.name == "Miss")
                        Destroy(go.gameObject);
            }
            clear = false;
            return;
        }


        Transform[] allHits = GetComponentsInChildren<Transform>(true);
        foreach (Transform hit in allHits)
        {
            hit.gameObject.SetActive(!(hit.name == "Miss") || !hideMissedHits);
        }

    }

    public Transform[] getPlanes()
    {
        return Planes;
    }
}
