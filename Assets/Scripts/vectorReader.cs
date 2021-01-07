using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vectorReader : MonoBehaviour
{
    public TextAsset textAsset;

    // Initialization
    void Start()
    {
        print("# Processing content: \n" + textAsset.text);

        string[] lines = textAsset.text.Split("\n"[0]); // gets all lines into separate strings

        print("# NLines= " + lines.Length);

        // Print all lines one by one
        // for (int i = 0; i < lines.Length; i++) {
        // 	print( lines[i] );
        // }

        // Store the points into a Vector3 array
        Vector3[] vectors = new Vector3[lines.Length];
        for (var i = 0; i < lines.Length; i++)
        {
            var pt = lines[i].Split(","[0]); // gets 3 parts of the vector into separate strings
            var x = float.Parse(pt[0]);
            var y = float.Parse(pt[1]);
            var z = 0.0f;   //float.Parse(pt[2]);
            vectors[i] = new Vector3(x, y, z);

            print("# V[" + i + "]= " + vectors[i]);
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}
