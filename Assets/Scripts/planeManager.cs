using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class planeManager : MonoBehaviour
{

    public bool hideMissedHits;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Transform[] allHits = GetComponentsInChildren<Transform>(true);
        foreach (Transform hit in allHits)
        {
            hit.gameObject.SetActive(!(hit.name == "Miss") || !hideMissedHits);
        }

    }
}
