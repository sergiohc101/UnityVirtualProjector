﻿using UnityEngine;

public class test_ray2 : MonoBehaviour
{

    // This is never called, trust no one
    static bool ContainsPoint(Vector3[] polyPoints, Vector3 p)
    {
        //Debug.Log("----->Hitting=" + p);

        var j = polyPoints.Length - 1;
        var inside = false;
        for (int i = 0; i < polyPoints.Length; j = i++)
        {
            Vector3 pi = polyPoints[i];
            Vector3 pj = polyPoints[j];
            if (((pi.y <= p.y && p.y < pj.y) || (pj.y <= p.y && p.y < pi.y)) &&
                (p.x < (pj.x - pi.x) * (p.y - pi.y) / (pj.y - pi.y) + pi.x))
                inside = !inside;
        }

        Bounds B = new Bounds(new Vector3(0, 0, 0), new Vector3(500, 500, 1));
        Vector3 result = B.ClosestPoint(p);

        Debug.Log("----- resultClosestP= " + result);
        Debug.Log("----- Contains= " + B.Contains(p));
        Debug.Log("----- isInside= " + inside);

        return inside;
    }


    static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        Vector3 dir = point - pivot; // get point direction relative to pivot
        dir = Quaternion.Euler(angles) * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point; // return it
    }


    public Transform wall;
    public Transform wall1;
    public Transform wall2;
    public Transform wall3;

    public float ent = 0.0f;
    public float ent1 = 0.0f;
    public float ent2 = 0.0f;

    void Update()
    {
        // Transform wall;
        Vector3 mousePos = Input.mousePosition;
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(mousePos);

            for (int i = 0; i < 3; i++)
            {
                //ent = 100.0f;

                if (i == 0) wall = wall1;
                if (i == 1) wall = wall2;
                if (i == 2) wall = wall3;

                //Debug.Log("Hitting= " + wall.gameObject.name);

                Plane plane = new Plane(); //(wall.transform.position, -wall.up.normalized); //Plane Constructor does not work!!?
                plane.SetNormalAndPosition(-wall.up.normalized, wall.transform.position);

                Debug.Log("Hitting=" + wall.gameObject.name + " , WALL.transform.position" + wall.transform.position + "// wallNrml" + wall.up.normalized + " || planeNrml" + plane.normal + "// thisTransform= " + transform.position);

                Debug.DrawRay(wall.transform.position, wall.up.normalized * 1000, Color.yellow);


                if (plane.Raycast(ray, out ent))
                {

                    Vector3 hitPoint = ray.GetPoint(ent);
                    Debug.Log("Raycast hit Plane at distance: " + ent);
                    Debug.Log("hitPoint: " + hitPoint); // WRT to WORLD

                    if (i == 0) ent1 = ent;
                    else ent2 = ent;

                    GameObject planeManager = GameObject.Find("planeManager");
                    GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);

                    go.transform.parent = planeManager.transform;
                    go.transform.position = hitPoint;
                    go.transform.transform.rotation = wall.transform.rotation;
                    go.transform.localScale = new Vector3(50.0f, 50.0f, 50.0f);

                    //Debug.Log("RAY>>: origin: " + ray.origin + " // dir: " +  (ray.direction * ent));


                    Vector3 scale = wall.transform.localScale;
                    Matrix4x4 Rt = wall.transform.localToWorldMatrix;
                    // Remove matrix scale	// FIXME : place Cube gameobject elsewhere to remove scale factor from main transform component (this)
                    Rt.SetColumn(0, Rt.GetColumn(0) / scale[0]);
                    Rt.SetColumn(1, Rt.GetColumn(1) / scale[1]);
                    Rt.SetColumn(2, Rt.GetColumn(2) / scale[2]);
                    // Transpose Matrix
                    Rt = Rt.transpose;

                    //Wall points wrt to its origin
                    Vector3[] wallPts = new[] {
                        new Vector3(  250.0f,  250.0f, 0.0f),
                        new Vector3( -250.0f,  250.0f, 0.0f),
                        new Vector3( -250.0f, -250.0f, 0.0f),
                        new Vector3(  250.0f, -250.0f, 0.0f),
                    };

                    // Compute plane normal wrt the camera
                    //Vector3 P1_ = -Rt.MultiplyVector(wall.transform.position);
                    Debug.Log("Plane_hitPoint: " + (hitPoint - wall.transform.position));

                    Vector3 t = Rt.MultiplyPoint3x4(hitPoint - wall.transform.position); // / 50.0f;

                    Debug.Log("Hitpoint wrt to planeOrigin [t]:  " + t);

                    Vector3 hitPointInPlane = RotatePointAroundPivot(
                        t,
                        Vector3.zero,
                        new Vector3(-90.0f, 0.0f, 0.0f));

                    Debug.Log("Point_on_Plane:  " + hitPointInPlane);

                    // bool wallHit = ContainsPoint(wallPts, hitPointInPlane);

                    Bounds wallBounds = new Bounds(new Vector3(0, 0, 0), new Vector3(500, 500, 1));
                    bool wallHit = wallBounds.Contains(hitPointInPlane);
                    Vector3 result = wallBounds.ClosestPoint(hitPointInPlane);

                    Vector3 rotatedResult = RotatePointAroundPivot(
                        result,
                        Vector3.zero,
                        new Vector3(90.0f, 0.0f, 0.0f));

                    Debug.Log("ClosestPoint(hitPointInPlane):  " + result);
                    Debug.Log("ROT_ClosestPoint(hitPointInPlane):  " + rotatedResult);
                    Vector3 closestPointForHitInWorld = Rt.transpose.MultiplyPoint3x4(rotatedResult);
                    Debug.Log("closestPointForHitInWorld():  " + closestPointForHitInWorld);

                    GameObject hitNear = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    hitNear.transform.parent = planeManager.transform;
                    hitNear.transform.position = closestPointForHitInWorld;
                    hitNear.name = "Nearest";
                    hitNear.transform.localScale = new Vector3(50.0f, 50.0f, 50.0f);
                    hitNear.GetComponent<Renderer>().material.color = Color.magenta;



                    if (i == 0 && wallHit)
                    {
                        go.GetComponent<Renderer>().material.color = Color.green;
                        Debug.DrawRay(ray.origin, wall.position, Color.green);

                    }
                    else if (i == 1 && wallHit)
                    {
                        go.GetComponent<Renderer>().material.color = Color.blue;
                        Debug.DrawRay(ray.origin, wall.position, Color.blue);

                    }
                    else if (i == 2 && wallHit)
                    {
                        go.GetComponent<Renderer>().material.color = Color.yellow;
                    }
                    else
                    {
                        go.name = "Miss";
                        go.GetComponent<Renderer>().material.color = Color.red;

                        Debug.DrawRay(ray.origin, ray.direction * ent, Color.red);  // FIXME
                    }


                }
                else
                {
                    Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red);
                    Debug.Log("No intersection");

                    //plane.ClosestPointOnPlane() is available in newer SDKs

                }
            }

        }
    }


}
