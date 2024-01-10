using UnityEngine;

public class multiPlaneVirtualProjector : MonoBehaviour
{
    public bool DRAW_LINES = true;
    public bool MOVE_CAM = true;
    public bool LOOK_AROUND = true;
    public Vector3 camLookAt;
    Vector3 camLookAroundAt;

    public float camTrajectoryRadius = 300;
    public Color camColor;

    public bool DRAW_QUAD = true;
    public Color quadColor = Color.magenta;
    public Vector2 QUAD_OFFSET;

    public bool DRAW_TRI = true;
    public Color triColor = Color.cyan;
    public Vector2 TRI_OFFSET;

    public bool DRAW_POLY = true;
    public Color polyColor = Color.red;
    public Vector2 POLY_OFFSET;
    public bool LIMIT_RADIUS = true;
    public int polyVerts = 6;
    public int polyRadius = 100;
    public int lineRendererWidth = 10; // FIXME : adapt
    public int lineRendererOffset = -1;

    // public bool DRAW_SHAPE = true;
    // public Color shapeColor = Color.green;
    // public TextAsset textAsset;
    // Vector3[] shape;

    [Header("Projector Properties")]
    public float f = 1000.0f;
    public float u = 640.0f;
    public float v = 480.0f;

    // Camera matrix K
    // public float[,] K = new float[3, 3];

    public float timeToCompleteCircle = 5.0f; // in seconds
    public float currentAngleDeg;

    // Wall points wrt origin
    Vector3[] Wall = {
        new Vector3( -50.0f,  50.0f, 0.0f), //  (0)-------(3)
        new Vector3( -50.0f, -50.0f, 0.0f), //  |          |
        new Vector3(  50.0f, -50.0f, 0.0f), //  |          |
        new Vector3(  50.0f,  50.0f, 0.0f), //  (1)-------(2)
    };

    Vector3[] triShape = {
        new Vector3(    0.0f,  190.0f, 0.0f),
        new Vector3( -192.0f, -144.0f, 0.0f),
        new Vector3(  192.0f, -144.0f, 0.0f),
        new Vector3(    0.0f,  190.0f, 0.0f),
        new Vector3(    0.0f,  140.0f, 0.0f),
        new Vector3( -142.0f, -114.0f, 0.0f),
        new Vector3(  142.0f, -114.0f, 0.0f),
        new Vector3(    0.0f,  140.0f, 0.0f)
    };

    public bool DEBUG_LOGS = false;
    void DEBUG(string str) { if (DEBUG_LOGS) Debug.Log(str); }

    // Initialization
    void Start()
    {
        // // Set up camera matrix
        // K[0, 0] = K[1, 1] = -f;
        // K[0, 2] = u / 2.0f;
        // K[1, 2] = v / 2.0f;
        // K[2, 2] = -1;

        ///////////////////////////////////////////////////
        // Ray Tracer Manager
        ///////////////////////////////////////////////////
        // FIXME : Refactor multi plane RayTracer, initialize here
    }

    static readonly float TAU = 2.0f * Mathf.PI;
    static readonly float TAU_Deg = TAU * Mathf.Rad2Deg; // 360

    // Update is called once per frame
    void Update()
    {
        // Change projector color
        GetComponentInChildren<MeshRenderer>().material.color = camColor;

        if (MOVE_CAM)
        {
            // Projector circular movement
            float camSpeed = 360.0f / timeToCompleteCircle;
            currentAngleDeg += Time.deltaTime * camSpeed;
            if (Mathf.Abs(currentAngleDeg) >= 360.0f) currentAngleDeg = 0.0f;

            float Cx = camTrajectoryRadius * Mathf.Cos(currentAngleDeg * Mathf.Deg2Rad);
            float Cy = camTrajectoryRadius * Mathf.Sin(currentAngleDeg * Mathf.Deg2Rad);
            transform.position = new Vector3(Cx, Cy, transform.position.z);
        }
        // Projector looks at world origin, up vector is [0 1 0]
        // This can be overriden by camLookAt and LOOK_AROUND public values.
        camLookAroundAt = camLookAt;
        if (LOOK_AROUND)
        {
            // Projector looks around in space
            float camSpeed = 360.0f / timeToCompleteCircle;
            currentAngleDeg += Time.deltaTime * camSpeed;
            if (Mathf.Abs(currentAngleDeg) >= 360.0f) currentAngleDeg = 0.0f;
            camLookAroundAt.x += camTrajectoryRadius * Mathf.Cos(currentAngleDeg * Mathf.Deg2Rad);
        }
        transform.LookAt(camLookAroundAt);

        // Line from projector to origin/ camLookAt position
        if (DRAW_LINES) Debug.DrawLine(transform.position, camLookAroundAt, camColor);

        //////////////////////////////////////////////////////////////////////////////

        Matrix4x4 M = transform.localToWorldMatrix;
        //DEBUG("M= \n" + M );

        // Translation Vector wrt/from Projector to World Origin
        Vector3 t = M.transpose.MultiplyVector(-transform.position);
        DEBUG("Pos= " + (transform.position) );
        DEBUG("|Pos|= " + transform.position.magnitude);
        DEBUG("t= " + t);


        // Test ray from projector to plane origin
        // {
        //     multiPlaneRayTracer raytracer = new multiPlaneRayTracer(M, DEBUG_LOGS);
        //     Vector3 rayDirection = camLookAt - transform.position;
        //     Vector3 pPlane = raytracer.multiPlaneTrace(transform.position, rayDirection);
        //     DEBUG("pointInPlane= " + pPlane);
        //     Debug.DrawLine(transform.position, pPlane, Color.blue);
        // }

        /////////////////////////////////////////
        // Project QUAD Shape wrt the Camera
        /////////////////////////////////////////
        if (DRAW_QUAD)
        {
            multiPlaneRayTracer raytracer = new multiPlaneRayTracer(M, DEBUG_LOGS);
            Vector3[] wallShape = new Vector3[Wall.Length];
            for (int i = 0; i < Wall.Length; i++)
            {
                wallShape[i] = Wall[i] + new Vector3(QUAD_OFFSET.x, QUAD_OFFSET.y, f);
            }

            raytracer.NEW_multiPlaneTraceShape(transform.position, wallShape, DRAW_LINES, "QUAD");
        }


        /////////////////////////////////////////
        // Project TRIANGLE Shape wrt the Camera
        /////////////////////////////////////////
        if (DRAW_TRI)
        {
            multiPlaneRayTracer multiPlaneRayTracer = new multiPlaneRayTracer(M, DEBUG_LOGS);

            Vector3[] TRIShape = new Vector3[triShape.Length];

            for (int i = 0; i < triShape.Length; i++)
            {
                TRIShape[i] = triShape[i] + new Vector3(TRI_OFFSET.x, TRI_OFFSET.y, f);
            }

            multiPlaneRayTracer.multiPlaneTraceShape(transform.position, TRIShape, DRAW_LINES, "TRIANGLE_D");
        }
        else
        {
            //Clean stuff //FIXME
            //triLine.enabled = DRAW_TRI;
        }


        /////////////////////////////////////////////////////////
        // Project POLYGON Shape wrt the Camera
        /////////////////////////////////////////////////////////
        if (DRAW_POLY)
        {
            // Limit polygon radius size based on focal distance
            // This is done by finding the distance to the nearest point
            // for each line that every pair of vertices defines
            if (LIMIT_RADIUS)
            {
                float minRadius = Mathf.Infinity;
                for (int k = 0; k < 4; k++)
                {
                    Debug.DrawLine(transform.position, Wall[k] * 5, camColor); // FIXME : move this somewhere else so it always executes and fix the scale issue

                    Vector3 P1 = -M.MultiplyVector(transform.position - Wall[k] * 5);
                    Vector3 P2 = -M.MultiplyVector(transform.position - Wall[(k + 1) % 4] * 5);
                    DEBUG("\t->P1[" + k + "]= " + P1);
                    DEBUG("\t->P2[" + k + "]= " + P2);

                    float u1 = (f * P1.x) / P1.z;
                    float v1 = (f * P1.y) / P1.z;
                    float u2 = (f * P2.x) / P2.z;
                    float v2 = (f * P2.y) / P2.z;
                    DEBUG("1_(u,v)= " + u1 + " , " + v1);
                    DEBUG("2_(u,v)= " + u2 + " , " + v2);

                    // https://en.wikipedia.org/wiki/Distance_from_a_point_to_a_line
                    // simplified since p=(x0,y0)=(0,0)
                    float distToEdge = Mathf.Abs(u2 * v1 - v2 * u1) /
                                       Mathf.Sqrt((v2 - v1) * (v2 - v1) + (u2 - u1) * (u2 - u1));
                    if (distToEdge < minRadius) minRadius = distToEdge;
                }
                polyRadius = (int)minRadius;
                DEBUG("LimitMin= " + minRadius);

                // Debug.DrawLine( new Vector3(xmin,ymax,0), new Vector3(xmin,ymin,0), Color.red  );
                // Debug.DrawLine( new Vector3(xmin,ymin,0), new Vector3(xmax,ymin,0), Color.red  );
                // Debug.DrawLine( new Vector3(xmax,ymin,0), new Vector3(xmax,ymax,0), Color.red  );
                // Debug.DrawLine( new Vector3(xmax,ymax,0), new Vector3(xmin,ymax,0), Color.red  );

                // polyLine.positionCount = 4;
                // polyLine.SetPosition( 0 , new Vector3(xmin,ymax, -1.0f ) );
                // polyLine.SetPosition( 1 , new Vector3(xmin,ymin, -1.0f ) );
                // polyLine.SetPosition( 2 , new Vector3(xmax,ymin, -1.0f ) );
                // polyLine.SetPosition( 3 , new Vector3(xmax,ymax, -1.0f ) );
            }

            Vector3[] polyShape = new Vector3[Mathf.Abs(polyVerts)];

            float angleStep = TAU / polyVerts;
            for (int i = 0; i < polyVerts; i++)
            {
                // Limit size based either on polyRadius input or focal distance
                float angle = i * angleStep;
                float Px = polyRadius * Mathf.Cos(angle);
                float Py = polyRadius * Mathf.Sin(angle);
                Debug.Log("\t\t PolyVert[" + i + "]" + Px + "," + Py);

                polyShape[i] = new Vector3(Px + POLY_OFFSET.x, Py + POLY_OFFSET.y, f);
            }
            //multiPlaneRayTracer.traceShape(polyShape, transform.position, polyLine, lineRendererOffset, DRAW_LINES);
            multiPlaneRayTracer multiPlaneRayTracer = new multiPlaneRayTracer(M, DEBUG_LOGS);
            multiPlaneRayTracer.NEW_multiPlaneTraceShape(transform.position, polyShape, DRAW_LINES, "POLYGON");

        }


        /////////////////////////////////////////////////////////
        /// Project SVG Shape wrt the Camera
        /////////////////////////////////////////////////////////
        // shapeLine.enabled = DRAW_SHAPE;
        // if (DRAW_SHAPE)
        // {
        //     shapeLine.positionCount = shape.Length;
        //     shapeLine.SetColors(shapeColor, shapeColor);
        // }


        // Translate upwards
        //transform.position += Vector3.up * 100 * Time.deltaTime;
    }


}
