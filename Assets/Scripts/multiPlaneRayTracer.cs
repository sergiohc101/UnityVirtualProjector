using System.Collections;
using UnityEngine;

public class multiPlaneRayTracer
{
    float EPS = 0.000001f;
    Matrix4x4 Rt;
    Vector3 rayPosition = Vector3.zero; // FIXME : Is this ever nonzero?
    bool DEBUG_LOGS = false;

    static ArrayList mShapes = new ArrayList();

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


    public void multiPlaneTraceShape(Vector3 rayOrigin, Vector3[] shape, bool DRAW_LINES, string shapeName = "noname")
    {
        LineRenderer line = null;

        //Look up shapeName in list
        if (mShapes.Contains(shapeName))
        {
            Debug.Log("A shape exists already for " + shapeName);

            bool lineRendererFound = false;
            Transform[] children = rayTracerManager.GetComponentsInChildren<Transform>(true);
            foreach (var go in children)
            {
                if (go.name == "_lineRenderer_" + shapeName)
                {
                    line = go.gameObject.GetComponent<LineRenderer>();
                    Debug.Log("Got lineRenderer: " + go.name);
                    lineRendererFound = true;
                    break;
                }
            }
            if (!lineRendererFound) Debug.LogError("This should not happen!");

            Transform[] childrenP = mPlaneManagerGO.GetComponentsInChildren<Transform>(true);
            foreach (var go in childrenP)
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
        else if (shapeName != "noname")
        {
            Debug.Log("Adding shape" + shapeName);
            mShapes.Add(shapeName);

            GameObject lineRenderer = new GameObject();
            lineRenderer.name = "_lineRenderer_" + (shapeName != "noname" ? shapeName : "");
            lineRenderer.transform.parent = rayTracerManager.transform;

            lineRenderer.AddComponent<LineRenderer>();
            line = lineRenderer.GetComponent<LineRenderer>();

            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = line.endColor = Color.red;
            line.startWidth = line.endWidth = 5.0f; //lineRendererWidth; // FIXME
            line.loop = true;


            GameObject hitsObject = new GameObject();
            hitsObject.name = "_hits_" + (shapeName != "noname" ? shapeName : "");
            hitsObject.transform.parent = mPlaneManagerGO.transform;
        }

        if (line != null)
            line.positionCount = shape.Length;
        else
            Debug.Log("LineRenderer was not set! ");

        int i = 0;
        foreach (var rayDirection in shape)
        {
            Vector3 worldRayDirection = Rt.MultiplyVector(-rayDirection);
            //Debug.Log("\t worldRayDirection: " + worldRayDirection);

            Vector3 pointInNearestPlane = multiPlaneTrace(rayOrigin, worldRayDirection, shapeName); // bool DRAW_LINES

            Debug.Log("\t " + shapeName + "[" + i + "] :: rayDir: " + rayDirection + "\t pointInNearestPlane:" + pointInNearestPlane);

            if (line != null)
                line.SetPosition(i, new Vector3(pointInNearestPlane.x, pointInNearestPlane.y, pointInNearestPlane.z - 5)); //pointInNearestPlane);
            else
                Debug.Log("LineRenderer was not set! ");
            //new Vector3(pointInNearestPlane.x, pointInNearestPlane.y, -1.0f));//mlineRendererOffset));

            //if (DRAW_LINES)
            //Debug.DrawLine(rayOrigin, pPlane, mlineRenderer.startColor);

            i++;
        }
    }


    // This function calls the trace function for a point intersecting all plane references handled by the PlaneManager
    public Vector3 multiPlaneTrace(Vector3 rayOrigin, Vector3 rayDirection, string shapeName = "noname")
    {
        GameObject parent = mPlaneManagerGO;
        Transform[] childrenP = mPlaneManagerGO.GetComponentsInChildren<Transform>(true);
        foreach (var go in childrenP)
        {
            if (go.name == "_hits_" + shapeName)
            {
                parent = go.gameObject;
                break;
            }
        }

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

            Debug.DrawRay(wall.transform.position, wall.up.normalized * 100, Color.yellow); // Print plane normals

            float ent;
            if (plane.Raycast(ray, out ent))
            {

                Vector3 hitPoint = ray.GetPoint(ent);
                //Debug.Log("Raycast hit Plane at distance: " + ent);
                Debug.Log("hitPoint wrt origin: " + hitPoint);
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

                if (ent < closestPlaneDist)
                {
                    closestPlaneDist = ent;
                    closestHitpoint = hitPoint;
                }

                //--------------------------------------------------------
                // FIXME : move all
                //plane.ClosestPointOnPlane() is available in newer SDKs
                Bounds wallBounds = new Bounds(new Vector3(0, 0, 0), new Vector3(500, 500, 1)); // FIXME : move this to planeManager ??
                bool hitWithinBounds = wallBounds.Contains(hitPointInPlane);

                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.transform.parent = parent.transform; //mPlaneManagerGO.transform;
                go.transform.position = hitPoint;
                go.transform.transform.rotation = wall.transform.rotation;
                float GS = 25.0f;
                go.transform.localScale = new Vector3(GS, GS, GS);

                if (hitWithinBounds)
                {
                    if (wall == planes[0])
                    {
                        go.GetComponent<Renderer>().material.color = Color.green;
                    }
                    else if (wall == planes[1])
                    {
                        go.GetComponent<Renderer>().material.color = Color.blue;
                    }
                    else if (wall == planes[2])
                    {
                        go.GetComponent<Renderer>().material.color = Color.yellow;
                    }
                }
                // FIXME : dont generate miss if not needed
                else
                {
                    go.name = "Miss";
                    go.GetComponent<Renderer>().material.color = Color.magenta;

                    Debug.DrawRay(ray.origin, ray.direction * ent, Color.magenta);  // FIXME

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
                    hitNear.transform.parent = parent.transform; // mPlaneManagerGO.transform;
                    hitNear.transform.position = closestPointForHitInWorld;
                    hitNear.name = "Nearest";
                    float MS = 15.0f;
                    hitNear.transform.localScale = new Vector3(MS, MS, MS);
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


}
