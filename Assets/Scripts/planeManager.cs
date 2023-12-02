using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class planeManager : MonoBehaviour
{

    public bool hideMissedHits;
    public bool clear;

    public Transform[] planes;

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
                if (++count > 1) // Exclude this planeManager at position [0]
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
}
