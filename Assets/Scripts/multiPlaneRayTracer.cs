//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
// FIXME CLEAN ALL

public class multiPlaneRayTracer
{
    float EPS = 0.000001f;
    Matrix4x4 Rt;
    Vector3 rayPosition = Vector3.zero; // FIXME : Is this ever nonzero?
    bool DEBUG_LOGS = false;

    GameObject mPlaneManagerGO;
    GameObject rayTracerManager;

    void setDebugLogs(bool debug) { DEBUG_LOGS = debug; }
    void setRtMatrix(Matrix4x4 M) { Rt = M; }
    void DEBUG(string str) { if (DEBUG_LOGS) Debug.Log(str); }

    public multiPlaneRayTracer(Matrix4x4 M, bool debug)
    {
        setRtMatrix(M);
        setDebugLogs(debug);

        // Get reference to PlaneManager
        mPlaneManagerGO = GameObject.Find("planeManager");
        if (!mPlaneManagerGO) Debug.LogError("No reference to planeManager in Scene");

        // Get reference to Ray Tracer Manager
        rayTracerManager = GameObject.Find("rayTracerManager");
        if (!rayTracerManager) rayTracerManager = new GameObject("rayTracerManager");
    }

    static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        Vector3 dir = point - pivot; // get point direction relative to pivot
        dir = Quaternion.Euler(angles) * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point; // return it
    }


    public void multiPlaneTraceShape(Vector3 rayOrigin, Vector3[] shape, bool DRAW_LINES)
    {

        // FIXME : handle shape name from parameters
        GameObject lineRenderer = new GameObject("_lineRenderer");
        lineRenderer.transform.parent = rayTracerManager.transform;
        lineRenderer.AddComponent<LineRenderer>();
        LineRenderer line = lineRenderer.GetComponent<LineRenderer>();
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = line.endColor = Color.red;
        line.startWidth = line.endWidth = 5.0f; //lineRendererWidth;
        line.positionCount = shape.Length;
        line.loop = true;

        int i = 0;
        foreach (var rayDirection in shape)
        {

            Vector3 worldRayDirection = Rt.MultiplyVector(-rayDirection);

            //Debug.Log("\t worldRayDirection: " + worldRayDirection);

            Vector3 pointInNearestPlane = multiPlaneTrace(rayOrigin, worldRayDirection); // bool DRAW_LINES

            Debug.Log("\t Shape[" + i + "] : rayDir: " + rayDirection + "\t pointInNearestPlane " + pointInNearestPlane);

            line.SetPosition(i, pointInNearestPlane);
            //new Vector3(pointInNearestPlane.x, pointInNearestPlane.y, -1.0f));//mlineRendererOffset));

            //if (DRAW_LINES) 
            //Debug.DrawLine(rayOrigin, pPlane, mlineRenderer.startColor);

            i++;
        }
    }


    // This function calls the trace function for a point intersecting all plane references handled by the PlaneManager
    public Vector3 multiPlaneTrace(Vector3 rayOrigin, Vector3 rayDirection)
    {
        Ray ray = new Ray(rayOrigin, rayDirection);
        //Debug.Log("Ray origin:" + ray.origin + " :: direction: " + ray.direction);


        multiPlaneManager[] mPlaneManager = mPlaneManagerGO.GetComponents<multiPlaneManager>();

        Transform[] planes = mPlaneManager[0].getPlanes();
        //Debug.Log("PlanesSz: " + planes.Length);

        int i = 0;
        Vector3 closestHitpoint = Vector3.zero;
        float closestPlaneDist = float.MaxValue;
        foreach (var wall in planes)
        {
            Plane plane = new Plane(); //(wall.transform.position, -wall.up.normalized); //Plane Constructor does not work!!?
            plane.SetNormalAndPosition(-wall.up.normalized, wall.transform.position);

            //Debug.Log("Hitting=" + wall.gameObject.name + " , WALL.transform.position" + wall.transform.position + "// wallNrml" + wall.up.normalized + " || planeNrml" + plane.normal); // + "// thisTransform= " + transform.position);

            Debug.DrawRay(wall.transform.position, wall.up.normalized * 500, Color.yellow); // Print plane normals

            float ent;
            if (plane.Raycast(ray, out ent))
            {

                Vector3 hitPoint = ray.GetPoint(ent);
                //Debug.Log("Raycast hit Plane at distance: " + ent);
                Debug.Log("hitPoint wrt origin: " + hitPoint);

                // FIXME : move all
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.transform.parent = mPlaneManagerGO.transform;
                go.transform.position = hitPoint;
                go.transform.transform.rotation = wall.transform.rotation;
                go.transform.localScale = new Vector3(30.0f, 30.0f, 30.0f);


                Debug.Log(wall.gameObject.name + "_hitPoint: " + (hitPoint - wall.transform.position));


                Vector3 scale = wall.transform.localScale;
                Matrix4x4 Rt = wall.transform.localToWorldMatrix;
                // Remove matrix scale	// FIXME :
                Rt.SetColumn(0, Rt.GetColumn(0) / scale[0]);
                Rt.SetColumn(1, Rt.GetColumn(1) / scale[1]);
                Rt.SetColumn(2, Rt.GetColumn(2) / scale[2]);
                // Transpose Matrix
                Rt = Rt.transpose;

                Vector3 t = Rt.MultiplyPoint3x4(hitPoint - wall.transform.position);

                //Debug.Log("Hitpoint wrt to " + wall.gameObject.name + "Origin [t]:  " + t);

                Vector3 hitPointInPlane = RotatePointAroundPivot(
                    t,
                    Vector3.zero,
                    new Vector3(-90.0f, 0.0f, 0.0f));

                //Debug.Log("Point_on_Plane" + wall.gameObject.name + ":  " + hitPointInPlane);


                //plane.ClosestPointOnPlane() is available in newer SDKs
                Bounds wallBounds = new Bounds(new Vector3(0, 0, 0), new Vector3(500, 500, 1)); // FIXME : move this to planeManager ??
                bool hitWithinBounds = wallBounds.Contains(hitPointInPlane);

                if (hitWithinBounds)
                {
                    if (ent < closestPlaneDist)
                    {
                        closestPlaneDist = ent;
                        closestHitpoint = hitPoint;
                    }

                    if (wall == planes[0])
                    {
                        go.GetComponent<Renderer>().material.color = Color.green;
                        //Debug.DrawRay(ray.origin, wall.position, Color.green);

                    }
                    else if (wall == planes[1])
                    {
                        go.GetComponent<Renderer>().material.color = Color.blue;
                        //Debug.DrawRay(ray.origin, wall.position, Color.blue);

                    }
                    else if (wall == planes[2])
                    {
                        go.GetComponent<Renderer>().material.color = Color.yellow;
                    }
                    else
                    {
                        // FIXME
                    }
                }
                else
                {

                    go.name = "Miss";
                    go.GetComponent<Renderer>().material.color = Color.magenta;

                    Debug.DrawRay(ray.origin, ray.direction * ent, Color.magenta);  // FIXME

                    //////////////
                    Vector3 result = wallBounds.ClosestPoint(hitPointInPlane);

                    Vector3 rotatedResult = RotatePointAroundPivot(
                        result,
                        Vector3.zero,
                        new Vector3(90.0f, 0.0f, 0.0f));

                    //Debug.Log("ClosestPoint(hitPointInPlane):  " + result);
                    //Debug.Log("ROT_ClosestPoint(hitPointInPlane):  " + rotatedResult);
                    Vector3 closestPointForHitInWorld = Rt.transpose.MultiplyPoint3x4(rotatedResult);
                    //Debug.Log("closestPointForHitInWorld():  " + closestPointForHitInWorld);

                    GameObject hitNear = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    hitNear.transform.parent = mPlaneManagerGO.transform;
                    hitNear.transform.position = closestPointForHitInWorld;
                    hitNear.name = "Nearest";
                    hitNear.transform.localScale = new Vector3(25.0f, 25.0f, 25.0f);
                    hitNear.GetComponent<Renderer>().material.color = Color.magenta;
                }



            }
            else
            {
                Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red);
                //Debug.Log("No intersection");
            }

        }

        return closestHitpoint;
    }



    public void traceShape(Vector3[] shape, Vector3 camPosition, bool DRAW_LINES)
    {
        int i = 0;
        foreach (var rayDirection in shape)
        {
            DEBUG("\t Shape[" + i + "]");

            // Get with wrt to the plane
            //Vector3 pPlane = multiPlaneTrace(rayDirection); // FIXME + camPosition;
            //DEBUG("\t[C_" + i + "] Hit Plane at =  " + pPlane);

            //mlineRenderer.SetPosition(i, new Vector3(pPlane.x, pPlane.y, mlineRendererOffset));

            //if (DRAW_LINES) Debug.DrawLine(camPosition, pPlane, mlineRenderer.startColor);

            i++;
        }
    }

}
