using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test_ray2 : MonoBehaviour
{


    static bool ContainsPoint(Vector3[] polyPoints, Vector3 p)
    {
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
        return inside;
    }




    //var plane : Plane = new Plane(Vector3.up, Vector3.zero);;
    public Transform wall1;
    public Transform wall2;

    public float ent = 0.0f;
    public float ent1 = 0.0f;
    public float ent2 = 0.0f;

    void Update()
    {
        Transform wall;
        Vector3 mousePos = Input.mousePosition;
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(mousePos);

            for (int i = 0; i < 2; i++)
            {
                //ent = 100.0f;

                if (i == 0) wall = wall1;
                else wall = wall2;

                //Debug.Log("Hitting= " + wall.gameObject.name);

                Plane plane = new Plane(); //(wall.transform.position, -wall.up.normalized); //Plane Constructor does not work!!?
                plane.SetNormalAndPosition(-wall.up.normalized, wall.transform.position);

                Debug.Log("Hitting=" + wall.gameObject.name + " , WALL.transform.position" + wall.transform.position + "// wallNrml" + wall.up.normalized + " || planeNrml" + plane.normal + "// thisTransform= " + transform.position);

                Debug.DrawRay(wall.transform.position, wall.up.normalized * 1000, Color.yellow);


                if (plane.Raycast(ray, out ent))
                {

                    Vector3 hitPoint = ray.GetPoint(ent);
                    Debug.Log("Raycast hit Plane at distance: " + ent);
                    Debug.Log("hitPoint: " + hitPoint);

                    if (i == 0) ent1 = ent;
                    else ent2 = ent;

                    GameObject planeManager = GameObject.Find("planeManager");
                    GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //go.transform.parent = this.transform;

                    go.transform.parent = planeManager.transform;
                    go.transform.position = hitPoint;
                    go.transform.transform.rotation = wall.transform.rotation;
                    go.transform.localScale = new Vector3(50.0f, 50.0f, 50.0f);

                    Debug.Log("hitPoint[C]: " + (go.transform.position));

                    //go.transform.LookAt(this.transform);

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
                        new Vector3(  250.0f,  250.0f, 0.0f) + wall.transform.position,
                        new Vector3( -250.0f,  250.0f, 0.0f) + wall.transform.position,
                        new Vector3( -250.0f, -250.0f, 0.0f) + wall.transform.position,
                        new Vector3(  250.0f, -250.0f, 0.0f) + wall.transform.position,
                    };

                    // Compute plane normal wrt the camera
                    //Vector3 P1_ = -Rt.MultiplyVector(wall.transform.position);
                    Vector3 t = Rt.MultiplyVector(wall.transform.position); // / 50.0f;

                    Debug.Log("Point_in_plane: " + t);

                    Vector3 hitPointInPlane = t;

                    bool wallHit = ContainsPoint(wallPts, hitPointInPlane);

                    // FIXME : transform wallpoints into planeCoordinates , reverse function to vector



                    if (i == 0 && wallHit)
                    {
                        go.GetComponent<Renderer>().material.color = Color.green;
                    }
                    else if (i == 1 && wallHit)
                    {
                        //go.transform.parent = wall.transform;
                        go.GetComponent<Renderer>().material.color = Color.blue;
                    }
                    else
                    {
                        go.GetComponent<Renderer>().material.color = Color.red;
                        Debug.DrawRay(ray.origin, ray.direction * ent, Color.red);
                    }


                }
                else
                {
                    Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red);
                    Debug.Log("No intersection");
                }
            }

        }
    }


}
