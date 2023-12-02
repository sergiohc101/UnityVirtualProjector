using UnityEngine;


public class virtualProjector : MonoBehaviour
{
    public bool MOVE_CAM = true;
    public bool DRAW_LINES = true;
    public Color camColor;
    public int lineRendererWidth = 10;
    public int lineRendererOffset = -1;
    public Vector3 camLookAt;

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

    // public bool DRAW_SHAPE = true;
    // public Color shapeColor = Color.green;
    // public TextAsset textAsset;
    // Vector3[] shape;

    [Header("Projector Properties")]
    public float f = 1000.0f;
    public float u = 640.0f;
    public float v = 480.0f;

    //Setup camera matrix K
    public float[,] K = new float[3, 3];

    public float timeToCompleteCircle = 5.0f; // in seconds
    public float camTrajectoryRadius = 300;
    public float currentAngleDeg;


    //Wall points wrt origin
    Vector3[] Wall = new[] {
        new Vector3( -50.0f,  50.0f, 0.0f),
        new Vector3( -50.0f, -50.0f, 0.0f),
        new Vector3(  50.0f, -50.0f, 0.0f),
        new Vector3(  50.0f,  50.0f, 0.0f),
    };

    //Wall points wrt origin // FIXME : is this needed?
    // Vector3[] CROSS = new[] {
    //     new Vector3(    0.0f,  250.0f, 0.0f),
    //     new Vector3( -250.0f,    0.0f, 0.0f),
    //     new Vector3(    0.0f, -250.0f, 0.0f),
    //     new Vector3(  250.0f,    0.0f, 0.0f),
    // };

    GameObject rayTracerManager;
    GameObject triangleRenderer;
    GameObject polyRenderer;
    GameObject shapeRenderer;

    LineRenderer line;
    LineRenderer triLine;
    LineRenderer polyLine;
    LineRenderer shapeLine;


    Vector3[] triShape = new[] {
        new Vector3( 0.0f,      190.0f, 0.0f),
        new Vector3( -192.0f,   -144.0f, 0.0f),
        new Vector3( 192.0f,    -144.0f, 0.0f),
        new Vector3( 0.0f,      190.0f, 0.0f),
        new Vector3( 0.0f,      140.0f, 0.0f),
        new Vector3( -142.0f,   -114.0f, 0.0f),
        new Vector3( 142.0f,    -114.0f, 0.0f),
        new Vector3( 0.0f,      140.0f, 0.0f)
    };

    public bool DEBUG_LOGS = false;
    void DEBUG(string str) { if (DEBUG_LOGS) Debug.Log(str); }

    // Initialization
    void Start()
    {
        //Set up camera matrix
        K[0, 0] = K[1, 1] = f;
        K[0, 2] = -u / 2.0f;
        K[1, 2] = -v / 2.0f;
        K[2, 2] = -1;

        ///////////////////////////////////////////////////
        // Ray Tracer Manager
        ///////////////////////////////////////////////////
        rayTracerManager = GameObject.Find("rayTracerManager");
        if (!rayTracerManager) rayTracerManager = new GameObject("rayTracerManager");

        line = this.gameObject.AddComponent<LineRenderer>(); // FIXME : move this to a different gameobject if necesary
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = line.endColor = quadColor;
        line.startWidth = line.endWidth = lineRendererWidth;
        line.loop = true;
        line.positionCount = 4;
        //GameObject.Destroy(line, duration);

        ///////////////////////////////////////////////////
        // TRIANGLE
        ///////////////////////////////////////////////////
        triangleRenderer = new GameObject(gameObject.name + "_triangleRenderer");
        triangleRenderer.transform.parent = rayTracerManager.transform;
        triangleRenderer.AddComponent<LineRenderer>();
        triLine = triangleRenderer.GetComponent<LineRenderer>();
        triLine.material = new Material(Shader.Find("Sprites/Default"));
        triLine.startColor = triLine.endColor = triColor;
        triLine.startWidth = triLine.endWidth = lineRendererWidth;
        triLine.positionCount = triShape.Length;

        ///////////////////////////////////////////////////
        // POLYGON
        ///////////////////////////////////////////////////
        polyRenderer = new GameObject(gameObject.name + "_polyRenderer");
        polyRenderer.transform.parent = rayTracerManager.transform;
        polyRenderer.AddComponent<LineRenderer>();
        polyLine = polyRenderer.GetComponent<LineRenderer>();
        polyLine.material = new Material(Shader.Find("Sprites/Default"));
        polyLine.loop = true;
        polyLine.startColor = polyLine.endColor = polyColor;
        polyLine.startWidth = polyLine.endWidth = lineRendererWidth;
        polyLine.positionCount = polyVerts;


        ///////////////////////////////////////////////////
        // SVG Shape
        ///////////////////////////////////////////////////
        // string[] lines = textAsset.text.Split("\n"[0]); // gets all lines into separate strings
        // print(" NLines= " + lines.Length);
        // shape = new Vector3[lines.Length];

        // for (var i = 0; i < lines.Length; i++)
        // {
        //     var pt = lines[i].Split(","[0]); // gets 3 parts of the vector into separate strings
        //     var x = float.Parse(pt[0]);
        //     var y = float.Parse(pt[1]);
        //     var z = 0.0f;   //float.Parse(pt[2]);
        //     shape[i] = new Vector3(x, y, z);
        //     //print( "V["+i+"]= " + shape[i] );
        // }
        // shapeRenderer = new GameObject("shapeRenderer");
        // shapeRenderer.AddComponent<LineRenderer>();
        // shapeLine = shapeRenderer.GetComponent<LineRenderer>();
        // shapeLine.material = new Material(Shader.Find("Sprites/Default"));
        // shapeLine.loop = true;
        // shapeLine.startColor = polyColor; shapeLine.endColor = polyColor;
        // shapeLine.SetWidth(10.0f, 10.0f);
        // shapeLine.positionCount = polyVerts;

    }


    // Update is called once per frame
    void Update()
    {
        // Camera obscura model
        // lp = K [ R|t ] P ,  or
        // lp = K R [ I| -c ] P
        // looking at the origin, up vector is [0 1 0]

        //Change projector color
        this.GetComponent<Renderer>().material.color = camColor;

        // Projector circular movement
        if (MOVE_CAM)
        {
            float camSpeed = Mathf.PI * 2.0f * Mathf.Rad2Deg / timeToCompleteCircle;
            currentAngleDeg += Time.deltaTime * camSpeed;
            if (currentAngleDeg >= Mathf.PI * 2.0f * Mathf.Rad2Deg) currentAngleDeg = 0.0f;

            float Cx = camTrajectoryRadius * Mathf.Cos(currentAngleDeg * Mathf.Deg2Rad);
            float Cy = camTrajectoryRadius * Mathf.Sin(currentAngleDeg * Mathf.Deg2Rad);
            transform.position = new Vector3(Cx, Cy, transform.position.z);
        }
        //Projector looks at plane origin after moving
        transform.LookAt(camLookAt);
        // Line from projector to origin
        if (DRAW_LINES) Debug.DrawLine(transform.position, camLookAt, camColor);

        //////////////////////////////////////////////////////////////////////////////

        Matrix4x4 L = transform.localToWorldMatrix;
        //DEBUG("L= \n" + L );

        Vector3 scale = transform.localScale;
        //DEBUG("ScaleVector= " + scale);

        Vector3 t = L.transpose.MultiplyVector(-transform.position) / 50.0f;
        //DEBUG("Pos= " + (transform.position) );
        DEBUG("|Pos|= " + transform.position.magnitude);
        DEBUG("t= " + t);

        // Copy origial view matrix
        Matrix4x4 Rt = L;
        //DEBUG("Rt= \n" + Rt );
        // Remove matrix scale	// FIXME : place Cube gameobject elsewhere to remove scale factor from main transform component (this)
        Rt.SetColumn(0, Rt.GetColumn(0) / scale[0]);
        Rt.SetColumn(1, Rt.GetColumn(1) / scale[1]);
        Rt.SetColumn(2, Rt.GetColumn(2) / scale[2]);
        // Transpose Matrix
        Rt = Rt.transpose;
        DEBUG("Rt^T= \n" + Rt);
        // Translation vector
        t = Rt.MultiplyVector(-transform.position);
        //DEBUG("t= " + t );

        // Compute plane normal wrt the camera
        Vector3 P1_ = -Rt.MultiplyVector(transform.position - Wall[0]);
        DEBUG("\t->P1= " + P1_);
        Vector3 P2_ = -Rt.MultiplyVector(transform.position - Wall[1]);
        DEBUG("\t->P2= " + P2_);
        Vector3 P3_ = -Rt.MultiplyVector(transform.position - Wall[2]);
        DEBUG("\t->P3= " + P3_);
        Vector3 P4_ = -Rt.MultiplyVector(transform.position - Wall[3]);
        DEBUG("\t->P4= " + P4_);
        Vector3 P21 = P2_ - P1_;
        Vector3 P41 = P4_ - P1_;
        //DEBUG("\t\t P_21= " + P21 );
        //DEBUG("\t\t P_41= " + P41 );

        //float planeAngle = Vector3.Angle(P21, P41);
        //DEBUG("\t\t Plane_angle=  " + planeAngle );

        Vector3 planeNormal = Vector3.Cross(P21, P41).normalized;
        DEBUG("\t\t Plane_normal=  " + planeNormal);

        Vector3 rayPosition = Vector3.zero;
        Vector3 planePoint = t;

        // Test ray from projector to plane origin
        {
            rayTracer raytracer = new rayTracer(Rt, DEBUG_LOGS);
            raytracer.setup(planePoint, planeNormal, rayPosition);
            Vector3 rayDirection = t;
            Vector3 pPlane = raytracer.trace(rayDirection) + transform.position;
            Debug.DrawLine(transform.position, pPlane, Color.red);
        }

        /////////////////////////////////////////
        // Project QUAD Shape wrt the Camera
        /////////////////////////////////////////
        line.enabled = DRAW_QUAD;
        if (DRAW_QUAD)
        {
            line.startColor = line.endColor = quadColor;
            line.startWidth = line.endWidth = lineRendererWidth;
            rayTracer raytracer = new rayTracer(Rt, DEBUG_LOGS);
            raytracer.setup(planePoint, planeNormal, rayPosition);
            Vector3[] wallShape = new Vector3[Wall.Length];
            for (int i = 0; i < Wall.Length; i++)
            {
                wallShape[i] = Wall[i] + new Vector3(QUAD_OFFSET.x, QUAD_OFFSET.y, f);
            }
            raytracer.traceShape(wallShape, transform.position, line, lineRendererOffset, DRAW_LINES);
        }


        /////////////////////////////////////////
        // Project TRIANGLE Shape wrt the Camera
        /////////////////////////////////////////
        triLine.enabled = DRAW_TRI;
        if (DRAW_TRI)
        {
            triLine.startColor = triLine.endColor = triColor;
            triLine.startWidth = triLine.endWidth = lineRendererWidth;
            rayTracer raytracer = new rayTracer(Rt, DEBUG_LOGS);
            raytracer.setup(planePoint, planeNormal, rayPosition);
            Vector3[] TRIShape = new Vector3[triShape.Length];

            for (int i = 0; i < triShape.Length; i++)
            {
                TRIShape[i] = triShape[i] + new Vector3(TRI_OFFSET.x, TRI_OFFSET.y, f);
            }
            raytracer.traceShape(TRIShape, transform.position, triLine, lineRendererOffset, DRAW_LINES);
        }


        /////////////////////////////////////////////////////////
        // Project POLYGON Shape wrt the Camera
        /////////////////////////////////////////////////////////
        polyLine.enabled = DRAW_POLY;
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

                    Vector3 P1 = -Rt.MultiplyVector(transform.position - Wall[k] * 5);
                    Vector3 P2 = -Rt.MultiplyVector(transform.position - Wall[(k + 1) % 4] * 5);
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

            polyLine.positionCount = Mathf.Abs(polyVerts);
            polyLine.startColor = polyLine.endColor = polyColor;
            polyLine.startWidth = polyLine.endWidth = lineRendererWidth;

            rayTracer raytracer = new rayTracer(Rt, DEBUG_LOGS);
            raytracer.setup(planePoint, planeNormal, rayPosition);
            Vector3[] polyShape = new Vector3[polyVerts];

            float angleStep = (Mathf.PI * 2.0f) / polyVerts;
            for (int i = 0; i < polyVerts; i++)
            {
                //DEBUG("\t\t PolyVert[" + i + "]");
                // Limit size based either on polyRadius input or focal distance
                float angle = i * angleStep;
                float Px = polyRadius * Mathf.Cos(angle);
                float Py = polyRadius * Mathf.Sin(angle);

                polyShape[i] = new Vector3(Px + POLY_OFFSET.x, Py + POLY_OFFSET.y, f);
            }
            raytracer.traceShape(polyShape, transform.position, polyLine, lineRendererOffset, DRAW_LINES);
        }


        /////////////////////////////////////////////////////////
        ///Project SVG Shape wrt the Camera
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
