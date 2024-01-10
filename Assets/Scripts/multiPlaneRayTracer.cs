using System.Collections;
using UnityEngine;

public class multiPlaneRayTracer
{
    Matrix4x4 Rt;
    bool DEBUG_LOGS = false;

    static ArrayList drawnShapes = new ArrayList();

    GameObject planeManagerGO;
    multiPlaneManager planeManager;
    GameObject rayTracerManager;

    void setDebugLogs(bool debug_logs) { DEBUG_LOGS = debug_logs; }
    void setRtMatrix(Matrix4x4 M) { Rt = M; }
    void DEBUG(string str) { if (DEBUG_LOGS) Debug.Log(str); }

    private static readonly float hitScale = 25.0f; // TODO : make configurable

    public multiPlaneRayTracer(Matrix4x4 M, bool debug)
    {
        setRtMatrix(M);
        setDebugLogs(debug);

        // Get reference to PlaneManager
        planeManagerGO = GameObject.Find("planeManager");
        if (!planeManagerGO)
            Debug.LogError("No reference to planeManager in Scene");
        planeManager = planeManagerGO.GetComponent<multiPlaneManager>();
        if (!planeManager)
            Debug.LogError("Could not retrieve multiPlaneManager component.");

        // Get reference to rayTracerManager
        rayTracerManager = GameObject.Find("rayTracerManager");
        if (!rayTracerManager)
        {
            Debug.Log("Creating new rayTracerManager instance.");
            rayTracerManager = new GameObject("rayTracerManager");
        }
    }

    static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        Vector3 dir = point - pivot;            // Get point direction relative to pivot
        dir = Quaternion.Euler(angles) * dir;   // Rotate it
        point = dir + pivot;                    // Compute rotated point
        return point;
    }
    private static int frameCount;
    void drawPlaneNormals()
    {
        int currentFrame = Time.frameCount;
        if(frameCount < currentFrame)
        {
            Debug.Log("(!) drawPlaneNormals new call!.");
            // TODO : Move draw stuff here
        }
         // Update the frame count
        frameCount = currentFrame;
        Debug.Log("called drawPlaneNormals on frame: " + frameCount);
    }

    // TODO: Test on old SDK for retrocompatibility
    public void NEW_multiPlaneTraceShape(Vector3 rayOrigin, Vector3[] shape, bool DRAW_LINES, string shapeName = "noname")
    {
        // Instantiate LineRenderer for shapeName:
        //      Since each line segment can intersect a plane boundaries at most twice,
        //      its assumed that at most [line.positionCount = (shape.Length + 1) * 2].
        //      The extra segment is only needed in case of shape loop.
        //
        // Instantiate _hits_PlaneName container for each Plane inside the _hits_shapeName
        //      This means: [planeManager] > [_hits_shapeName] > [_hits_PlaneName] > [Hit | Miss]
        //
        // Foreach plane, compute raycast for all shape points. Apply line clipping to plane points.
        //

        Debug.Log("|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||");
        Transform[] planes = planeManager.getPlanes();
        GameObject[] planeHits = new GameObject[planes.Length];
        LineRenderer[] shapeRenderers = new LineRenderer[planes.Length];
        Debug.Log($"Tracing shape '{shapeName}' containing [{shape.Length}] points. Using [{planes.Length}] planes.");
        bool foundHitsContainer = false; // TODO: Implement

        // Look up shapeName in list
        if (drawnShapes.Contains(shapeName))
        {
            Debug.Log($"Found existing shape for '{shapeName}'.");

            // Check if the LineRenderer component for this shape exists and retrieve it
            bool lineRendererFound = false;
            bool includeInactive = true;
            Transform[] rayTracerManagerChildren = rayTracerManager.GetComponentsInChildren<Transform>(includeInactive);
            foreach (var go in rayTracerManagerChildren)
            {
                // Get lineRendererContainer
                if (go.name == "_lineRenderer_" + shapeName)
                {
                    lineRendererFound = true;
                    Debug.Log($"Got lineRenderer '{go.name}' containing [{go.childCount}] elements.");
                    for (int k = 0; k < go.childCount; k++)
                    {
                        shapeRenderers[k] = go.GetChild(k).gameObject.GetComponent<LineRenderer>();
                    }
                    break;
                }
            }
            if (!lineRendererFound) Debug.LogError($"lineRenderer '{shapeName}' not found!.");

            // Destroy all previous shape hits from planeManagerGO
            planeManager.clearShapeHits(shapeName);

            // The parent gameobject is either planeManagerGO or _hits_ shapeName; // TODO
            // GameObject parent = planeManagerGO;
            Transform[] planeManagerChildren = planeManager.getChildren(includeInactive);
            foreach (var child in planeManagerChildren)
            {
                if (child.name == "_hits_" + shapeName)
                {
                    Debug.Log($"Got hits container: '{child.name}'.");
                    // parent = child.gameObject;
                    // Retrieve planeHits gameobjects
                    if(child.childCount == planes.Length){
                        Debug.Log($"Got [{child.childCount}] hits containers on '{child.name}'.");
                        foundHitsContainer = true;
                        for (int k = 0; k < child.childCount; k++)
                        {
                            planeHits[k] = child.GetChild(k).gameObject;
                        }
                    }
                    else{
                        Debug.LogError($"Something changed for hits container '{child.name}'.");
                        // return;
                    }
                    break;
                }
            }
        }
        // Check for non-generic shape
        else if (shapeName != "noname")
        {
            Debug.Log($"Adding shape: '{shapeName}'.");
            drawnShapes.Add(shapeName);
            // Create a new GameObject which contains all LineRenderers for the shape
            string lineRendererName = "_lineRenderer_" + (shapeName != "noname" ? shapeName : "");
            GameObject lineRendererContainer = new GameObject(lineRendererName);
            lineRendererContainer.transform.parent = rayTracerManager.transform;
            int R = 0;
            foreach (var wall in planes)
            {
                // Create a LineRenderer for each plane
                GameObject lineRenderer = new GameObject(lineRendererContainer.name + "_" + wall.name , typeof(LineRenderer));
                lineRenderer.transform.parent = lineRendererContainer.transform;

                var line = lineRenderer.GetComponent<LineRenderer>();
                line.material = new Material(Shader.Find("Sprites/Default"));
                line.startColor = line.endColor = Color.red; // TODO: Use some editor color
                line.startWidth = line.endWidth = 5.0f; // TODO: Use lineRendererWidth
                line.loop = true; // TODO : Make configurable
                shapeRenderers[R] = line;
                R++;
            }
            Debug.Log($"Created '{lineRendererName}' LineRenderer.");

            // Create a new GameObject which contains all "hits" for the shape
            GameObject hitsObject = new GameObject("_hits_" + (shapeName != "noname" ? shapeName : ""));
            hitsObject.transform.parent = planeManagerGO.transform;
            // Create _hits_PlaneName GameObjects
            int p = 0;
            foreach (var wall in planes)
            {
                string hitsName = hitsObject.name + "_" + wall.name;
                GameObject hitsWall = new GameObject(hitsName);
                hitsWall.transform.parent = hitsObject.transform;
                planeHits[p] = hitsWall;
                p++;
                Debug.Log($"Created '{hitsName}' hits container.");
            }
        }

        // if (line != null)
            // line.positionCount = shape.Length; // FIXME: Use (shape.Length + 1) * 2; ??
        // if (!line)
            // Debug.LogError($"LineRenderer '{shapeName}' was not set!.");

        // Print plane normals // TODO: Only once per cycle
        if(DRAW_LINES) drawPlaneNormals();

        // Iterate all planes retrieved from the PlaneManager
        int w = 0;
        foreach (var wall in planes)
        {
            // Print plane normals // TODO: Only once per cycle
            if(DRAW_LINES) Debug.DrawRay(wall.transform.position, wall.up.normalized * 100, Color.yellow);

            Debug.Log("###################################################");

            Plane plane = new Plane(); //(wall.transform.position, -wall.up.normalized); // Plane Constructor not working on old sdk.
            plane.SetNormalAndPosition(-wall.up.normalized, wall.transform.position);

            Debug.Log($"Target='{wall.name}' , position={wall.transform.position} , normal={wall.up.normalized} | planeNrml={plane.normal} | rayOrigin={rayOrigin}");

            // Iterate shape points to compute hitsOnPlane
            int i = 0;
            Vector3[] hitsOnPlane = new Vector3[shape.Length];
            Vector3[] hitsOnWorld = new Vector3[shape.Length];
            foreach (var rayDirection in shape)
            {
                Debug.Log("------------------------------------------------------");

                Vector3 worldRayDirection = Rt.MultiplyVector(rayDirection);
                DEBUG("worldRayDirection: " + worldRayDirection);

                Debug.Log($"{shapeName}[{i+1}]: rayDirection: {rayDirection}");

                // The parent gameobject is either planeManagerGO or _hits_ shapeName // TOOO
                GameObject hitsContainer = planeHits[w];

                Ray ray = new Ray(rayOrigin, worldRayDirection);
                Debug.Log($"Ray origin: {ray.origin}, direction: {ray.direction}, rayDirection: {rayDirection}");

                float hit_distance;
                if (plane.Raycast(ray, out hit_distance))
                {
                    Vector3 hitPoint = ray.GetPoint(hit_distance);
                    hitsOnWorld[i] = hitPoint;
                    Debug.Log("Raycast hit Plane at distance: " + hit_distance);
                    Debug.Log("hitPoint wrt origin: " + hitPoint);
                    Debug.Log(wall.name + " hitPoint: " + (hitPoint - wall.transform.position));

                    Vector3 scale = wall.transform.localScale;
                    // Debug.Log("wall.transform.localScale: " + scale);
                    Matrix4x4 wall_Rt = wall.transform.localToWorldMatrix;
                    // Remove matrix scale
                    wall_Rt.SetColumn(0, wall_Rt.GetColumn(0) / scale[0]);
                    wall_Rt.SetColumn(1, wall_Rt.GetColumn(1) / scale[1]);
                    wall_Rt.SetColumn(2, wall_Rt.GetColumn(2) / scale[2]);
                    // Transpose Matrix
                    wall_Rt = wall_Rt.transpose;

                    Vector3 t = wall_Rt.MultiplyPoint3x4(hitPoint - wall.transform.position);

                    // Debug.Log($"Hitpoint wrt to {wall.name}, Origin [t]: " + t);

                    Vector3 hitPointInPlane = RotatePointAroundPivot(
                                                t,
                                                Vector3.zero,
                                                new Vector3(-90.0f, 0.0f, 0.0f));
                    hitsOnPlane[i] = hitPointInPlane;

                    //--------------------------------------------------------

                    Bounds wallBounds = new Bounds(new Vector3(0, 0, 0), new Vector3(500, 500, 1)); // FIXME : move this to planeManager ??
                    bool hitWithinBounds = wallBounds.Contains(hitPointInPlane);

                    GameObject hit = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    hit.transform.parent = hitsContainer.transform;
                    hit.transform.position = hitPoint;
                    hit.transform.transform.rotation = wall.transform.rotation;
                    hit.transform.localScale = new Vector3(hitScale, hitScale, hitScale);

                    // Get the Renderer component attached to the hit GameObject
                    Renderer hitRenderer = hit.GetComponent<Renderer>();

                    if (hitWithinBounds) {
                        hit.name = "Hit" + "_" + (i+1);
                        // TODO: Use wall color
                        if      (wall == planes[0]) { hitRenderer.material.color = Color.green; }
                        else if (wall == planes[1]) { hitRenderer.material.color = Color.blue; }
                        else if (wall == planes[2]) { hitRenderer.material.color = Color.yellow; }
                    }
                    else
                    {
                        hit.name = "Miss" + "_" + (i+1);
                        hit.GetComponent<Renderer>().material.color = Color.magenta;

                        Debug.DrawRay(ray.origin, ray.direction * hit_distance, Color.magenta);
                    }
                    Debug.Log($"[{hit.name}] on '{wall.name}'. -> Point_wrt_Plane: {hitPointInPlane}");

                }
                else
                {
                    Debug.Log($"No intersection for {shapeName}[{i+1}]");
                    Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red);
                }

                i++;
            }

            Debug.Log($" (*) hitsOnWorld[{wall.gameObject.name}]:" +  string.Join(", ", hitsOnWorld));
            Debug.Log($" (*) hitsOnPlane[{wall.gameObject.name}]:" +  string.Join(", ", hitsOnPlane));

            // Clip hitsOnPlane
            float rectWidth = 500;
            float rectHeight = 500;
            // Clip shape to plane
            Vector4[] clippedShape = new Vector4[shape.Length];
            clippedShape = lineClipper.clipShapeToRectangle(hitsOnPlane, rectWidth, rectHeight);
            Debug.Log($" (*) clippedShape[{clippedShape.Length}]:" +  string.Join(", ", clippedShape));

            var line = shapeRenderers[w];
            line.positionCount = clippedShape.Length;
            line.numCornerVertices = line.positionCount;

            float Z = 0.0f;

            for (int k = 0; k < clippedShape.Length; k++)
            {
                int n = (k+1) % clippedShape.Length;
                Vector3 point1 = new Vector3(clippedShape[k].x, clippedShape[k].y, 0 ) ;
                Vector3 point2 = new Vector3(clippedShape[k].z, clippedShape[k].w, 0 ) ;

                if(point1 != Vector3.zero)
                {
                    point1.z = hitsOnWorld[k].z + Z;
                    line.SetPosition(k,point1);
                }
                if(point2 != Vector3.zero)
                {
                    point2.z = hitsOnWorld[n].z + Z;
                    line.SetPosition(n,point2);
                }
            }

            w++;
        }


    }


    public void multiPlaneTraceShape(Vector3 rayOrigin, Vector3[] shape, bool DRAW_LINES, string shapeName = "noname")
    {
        Debug.Log($"Tracing shape '{shapeName}' containing [{shape.Length}] points.");
        LineRenderer line = null;

        // Look up shapeName in list
        if (drawnShapes.Contains(shapeName))
        {
            Debug.Log($"Found existing shape for '{shapeName}'.");

            // Check if the LineRenderer component for this shape exists and retrieve it
            bool lineRendererFound = false;
            bool includeInactive = true;
            Transform[] rayTracerManagerChildren = rayTracerManager.GetComponentsInChildren<Transform>(includeInactive);
            foreach (var go in rayTracerManagerChildren)
            {
                if (go.name == "_lineRenderer_" + shapeName)
                {
                    line = go.gameObject.GetComponent<LineRenderer>();
                    Debug.Log("Got lineRenderer: " + go.name);
                    lineRendererFound = true;
                    break;
                }
            }
            if (!lineRendererFound) Debug.LogError("lineRenderer" + shapeName + "not found!.");

            // Destroy all previous shape hits from planeManagerGO
            planeManager.clearShapeHits(shapeName);
        }
        // Check for non-generic shape
        else if (shapeName != "noname")
        {
            Debug.Log($"Adding shape: '{shapeName}'.");
            drawnShapes.Add(shapeName);

            // Create a new GameObject which contains a LineRenderer component for the shape
            GameObject lineRenderer = new GameObject();
            lineRenderer.name = "_lineRenderer_" + (shapeName != "noname" ? shapeName : "");
            lineRenderer.transform.parent = rayTracerManager.transform;

            lineRenderer.AddComponent<LineRenderer>();
            line = lineRenderer.GetComponent<LineRenderer>();

            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = line.endColor = Color.red;
            line.startWidth = line.endWidth = 5.0f; // TODO: Use lineRendererWidth
            line.loop = true; // TODO : Make configurable

            // Create a new GameObject which contains all "hits" for the shape
            GameObject hitsObject = new GameObject();
            hitsObject.name = "_hits_" + (shapeName != "noname" ? shapeName : "");
            hitsObject.transform.parent = planeManagerGO.transform;
        }

        if (line != null)
            line.positionCount = shape.Length;
        else
            Debug.LogError("LineRenderer" + shapeName + "was not set!.");

        int i = 0;
        foreach (var rayDirection in shape)
        {
            Vector3 worldRayDirection = Rt.MultiplyVector(rayDirection);
            DEBUG($"{shapeName}[{i}] :: worldRayDirection: " + worldRayDirection);

            Vector3 pointInNearestPlane = multiPlaneTrace(rayOrigin, worldRayDirection, shapeName); // bool DRAW_LINES

            Debug.Log($"{shapeName}[{i}] :: rayDirection: {rayDirection} \t pointInNearestPlane: {pointInNearestPlane}");

            if (line != null)
                line.SetPosition(i, new Vector3(pointInNearestPlane.x, pointInNearestPlane.y, pointInNearestPlane.z - 5)); //pointInNearestPlane);
            else
                Debug.Log("LineRenderer was not set! ");

            i++;
        }
    }


    // This function calls the trace function for a point intersecting all plane references handled by the PlaneManager
    public Vector3 multiPlaneTrace(Vector3 rayOrigin, Vector3 rayDirection, string shapeName = "noname")
    {
        // The parent gameobject is either planeManagerGO or _hits_ shapeName;
        GameObject parent = planeManagerGO;
        Transform[] planeManagerChildren = planeManagerGO.GetComponentsInChildren<Transform>(true);
        foreach (var go in planeManagerChildren)
        {
            if (go.name == "_hits_" + shapeName)
            {
                parent = go.gameObject;
                break;
            }
        }

        Ray ray = new Ray(rayOrigin, rayDirection);
        Debug.Log($"Ray origin: {ray.origin}, direction: {ray.direction}");

        Transform[] planes = planeManager.getPlanes();
        Debug.Log("Num Planes: " + planes.Length);

        Vector3 closestHitpoint = Vector3.zero;
        float closestPlaneDist = float.MaxValue;
        // Iterate all planes retrieved from the PlaneManager
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
                Debug.Log(wall.gameObject.name + " hitPoint: " + (hitPoint - wall.transform.position));


                Vector3 scale = wall.transform.localScale;
                // Debug.Log("wall.transform.localScale: " + scale);
                Matrix4x4 Rt = wall.transform.localToWorldMatrix;
                // Remove matrix scale
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

                // plane.ClosestPointOnPlane() is available in newer SDKs
                Bounds wallBounds = new Bounds(new Vector3(0, 0, 0), new Vector3(500, 500, 1)); // FIXME : move this to planeManager ??
                bool hitWithinBounds = wallBounds.Contains(hitPointInPlane);

                GameObject hit = GameObject.CreatePrimitive(PrimitiveType.Cube);
                hit.transform.parent = parent.transform;
                hit.transform.position = hitPoint;
                hit.transform.transform.rotation = wall.transform.rotation;
                const float GS = 25.0f;
                hit.transform.localScale = new Vector3(GS, GS, GS);

                // Get the Renderer component attached to the hit GameObject
                Renderer hitRenderer = hit.GetComponent<Renderer>();

                if (hitWithinBounds) {
                    hit.name = "Hit";
                    // TODO: Use wall color
                    if      (wall == planes[0]) { hitRenderer.material.color = Color.green; }
                    else if (wall == planes[1]) { hitRenderer.material.color = Color.blue; }
                    else if (wall == planes[2]) { hitRenderer.material.color = Color.yellow; }
                }
                // FIXME : dont generate miss if not needed
                else
                {
                    hit.name = "Miss";
                    hitRenderer.material.color = Color.magenta;

                    Debug.DrawRay(ray.origin, ray.direction * ent, Color.magenta);  // FIXME

                    Vector3 closestPointInPlane = wallBounds.ClosestPoint(hitPointInPlane);

                    Vector3 rotatedResult = RotatePointAroundPivot(
                                                closestPointInPlane,
                                                Vector3.zero,
                                                new Vector3(90.0f, 0.0f, 0.0f));

                    //Debug.Log("closestPointInPlane  " + closestPointInPlane);
                    //Debug.Log("ROT_ClosestPoint(hitPointInPlane):  " + rotatedResult);closestPointInPlane
                    Vector3 closestPointForHitInWorld = Rt.transpose.MultiplyPoint3x4(rotatedResult);
                    //Debug.Log("closestPointForHitInWorld():  " + closestPointForHitInWorld);

                    GameObject hitNear = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    hitNear.transform.parent = parent.transform;
                    hitNear.transform.position = closestPointForHitInWorld;
                    hitNear.name = "Nearest";
                    const float MS = 15.0f;
                    hitNear.transform.localScale = new Vector3(MS, MS, MS);
                    hitNear.GetComponent<Renderer>().material.color = Color.magenta;
                }
                Debug.Log($"[{hit.name}] on {wall.name}. Point_wrt_Plane: {hitPointInPlane}");

            }
            else
            {
                Debug.Log("(!) No intersection on");
                Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red);
            }

        }

        return closestHitpoint;
    }


}