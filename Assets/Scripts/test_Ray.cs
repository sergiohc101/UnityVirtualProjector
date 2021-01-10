using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Draws a line in the scene view going through a point 200 pixels
// from the lower-left corner of the screen

public class test_Ray : MonoBehaviour
{
    /*
     * Camera camera;

    void Start()
    {
        camera = GetComponent<Camera>();
    }

    void Update()
    {
        Ray ray = camera.ScreenPointToRay(new Vector3(500, 500, 0));
        Debug.DrawRay(ray.origin, ray.direction * 1000, Color.yellow);
    }
    */

    public LineRenderer line;
    public Transform youTrans;
    public Transform planeTrans;
    void Start()
    {
    }


    void Update()
    {

        //line = gameObject.GetComponent<LineRenderer>();
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = line.endColor = Color.yellow;
        line.startWidth = line.endWidth = 10;


        //Math from http://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-plane-and-ray-disk-intersection

        //A plane can be defined as:
        //a point representing how far the plane is from the world origin
        Vector3 p_0 = planeTrans.position;
        //a normal (defining the orientation of the plane), should be negative if we are firing the ray from above
        Vector3 n = -planeTrans.up;
        //We are intrerested in calculating a point in this plane called p
        //The vector between p and p0 and the normal is always perpendicular: (p - p_0) . n = 0

        //A ray to point p can be defined as: l_0 + l * t = p, where:
        //the origin of the ray
        Vector3 l_0 = youTrans.position;
        //l is the direction of the ray
        Vector3 l = youTrans.forward;
        //t is the length of the ray, which we can get by combining the above equations:
        //t = ((p_0 - l_0) . n) / (l . n)

        //But there's a chance that the line doesn't intersect with the plane, and we can check this by first
        //calculating the denominator and see if it's not small. 
        //We are also checking that the denominator is positive or we are looking in the opposite direction
        float denominator = Vector3.Dot(l, n);

        if (denominator > 0.00001f)
        {
            //Debug.Log("Ok");

            //The distance to the plane
            float t = Vector3.Dot(p_0 - l_0, n) / denominator;

            //Where the ray intersects with a plane
            Vector3 p = l_0 + l * t;

            Debug.Log("Plane scale: " + planeTrans.localScale);
            Debug.Log("Plane position: " + planeTrans.position);
            //Debug.Log("Plane : " + planeTrans.);//
            Debug.Log("Ray: " + p);

            //Display the ray with a line renderer
            line.SetPosition(0, p);
            line.SetPosition(1, l_0);

        }
        else
        {
            line.SetPosition(0, Vector3.zero);
            line.SetPosition(1, Vector3.zero);
            Debug.Log("No intersection");
        }

    }


}