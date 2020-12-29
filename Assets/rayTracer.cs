using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class rayTracer : MonoBehaviour
{
    public bool DEBUG_LOGS = false;
    public bool MOVE_CAM = true;
    public bool DRAW_LINES = true;
    public Color camColor;
    public int lineRendererWidth = 10;
    public int lineRendererOffset = -1;

    public bool DRAW_QUAD = true;
    public Color quadColor = Color.magenta;

    public bool DRAW_TRI = true;
    public Color triColor = Color.cyan;

    public bool DRAW_POLY = true;
    public Color polyColor = Color.red;
    public bool LIMIT_RADIUS = true;
    public int polyVerts = 6;
    public int polyRadius = 100;

    // public bool DRAW_SHAPE = true;
    // public Color shapeColor = Color.green;
    // public TextAsset textAsset;
    // Vector3[] shape;

    public float f = 1000.0f;
    public float u = 640.0f;
    public float v = 480.0f;

    //Setup camera matrix K
    public float[,] K = new float[3, 3];

    //public Matrix4x4 R;

    public float timeToCompleteCircle = 5.0f; // in seconds
    public float radius = 300;
    public float currentAngleDeg;


    //Wall points wrt origin
    Vector3[] Wall = new[] {
        new Vector3( -50.0f,  50.0f, 0.0f),
        new Vector3( -50.0f, -50.0f, 0.0f),
        new Vector3(  50.0f, -50.0f, 0.0f),
        new Vector3(  50.0f,  50.0f, 0.0f),
    };

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

    void DEBUG(string str) { if (DEBUG_LOGS) Debug.Log(str); }

    // Initialization
    void Start()
    {
        //Change projector color
        this.GetComponent<Renderer>().material.color = camColor;

        //Set up camera matrix
        K[0, 0] = K[1, 1] = f;
        K[0, 2] = -u / 2.0f;
        K[1, 2] = -v / 2.0f;
        K[2, 2] = -1;

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
        triangleRenderer = new GameObject("triangleRenderer");
        triangleRenderer.AddComponent<LineRenderer>();
        triLine = triangleRenderer.GetComponent<LineRenderer>();
        triLine.material = new Material(Shader.Find("Sprites/Default"));
        triLine.startColor = triLine.endColor = triColor;
        triLine.startWidth = triLine.endWidth = lineRendererWidth;
        triLine.positionCount = triShape.Length;

        ///////////////////////////////////////////////////
        // POLYGON
        ///////////////////////////////////////////////////
        polyRenderer = new GameObject("polyRenderer");
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

        // Projector circular movement 
        if (MOVE_CAM)
        {
            float speed = Mathf.PI * 2.0f * Mathf.Rad2Deg / timeToCompleteCircle;
            currentAngleDeg += Time.deltaTime * speed;
            if (currentAngleDeg >= Mathf.PI * 2.0f * Mathf.Rad2Deg) currentAngleDeg = 0.0f;

            float Cx = radius * Mathf.Cos(currentAngleDeg * Mathf.Deg2Rad);
            float Cy = radius * Mathf.Sin(currentAngleDeg * Mathf.Deg2Rad);
            transform.position = new Vector3(Cx, Cy, transform.position.z);
        }
        //Projector looks at origin after moving
        transform.LookAt(Vector3.zero);
        // Line from projector to origin
        if (DRAW_LINES) Debug.DrawLine(Vector3.zero, transform.position, camColor);

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
        // Remove matrix scale
        Rt.SetColumn(0, Rt.GetColumn(0) / scale[0]);
        Rt.SetColumn(1, Rt.GetColumn(1) / scale[1]);
        Rt.SetColumn(2, Rt.GetColumn(2) / scale[2]);
        // Transpose Matrix
        Rt = Rt.transpose;
        DEBUG("Rt^T= \n" + Rt);
        t = Rt.MultiplyVector(-transform.position);
        //DEBUG("t= " + t );

        Vector3 P1_ = -Rt.MultiplyVector(transform.position - Wall[0]);
        DEBUG("\t->P1= " + P1_);
        Vector3 P2_ = -Rt.MultiplyVector(transform.position - Wall[1]);
        DEBUG("\t->P2= " + P2_);
        Vector3 P3_ = -Rt.MultiplyVector(transform.position - Wall[2]);
        DEBUG("\t->P3= " + P3_);
        Vector3 P4_ = -Rt.MultiplyVector(transform.position - Wall[3]);
        DEBUG("\t->P4= " + P4_);

        // Compute plane normal wrt the camera
        Vector3 P21 = P2_ - P1_;
        Vector3 P41 = P4_ - P1_;
        //DEBUG("\t\t P_21= " + P21 );
        //DEBUG("\t\t P_41= " + P41 );

        //float planeAngle = Vector3.Angle(P21, P41);
        //DEBUG("\t\t Plane_angle=  " + planeAngle );

        Vector3 planeNormal = Vector3.Cross(P21, P41).normalized;
        DEBUG("\t\t Plane_normal=  " + planeNormal);

        // A plane can be defined as:
        // a point representing how far the plane is from the world origin
        //Vector3 planePoint = Vector3.zero;
        Vector3 planePoint = t;
        // a normal (defining the orientation of the plane), should be negative if we are firing the ray from above
        // We are intrerested in calculating a point in this plane called p
        // The vector between p and p0 and the normal is always perpendicular: (p - p_0) . n = 0

        // A ray to point p can be defined as: rayPos + rayDirection * rayDistance = p
        // This should be the coordinate system origin
        Vector3 rayPosition = Vector3.zero;

        {
            // Test ray from camera to plane origin
            Vector3 rayDirection = t;

            DEBUG("\t >Plane point= \t " + planePoint);
            DEBUG("\t >Plane normal= \t " + planeNormal);
            DEBUG("\t >Ray origin= \t " + rayPosition);
            DEBUG("\t >Ray direction= \t " + rayDirection);

            // But there's a chance that the line doesn't intersect with the plane, and we can check this by first
            // calculating the denominator and see if it's not small. 
            // We are also checking that the denominator is positive or we are looking in the opposite direction
            float denominator_ = Vector3.Dot(rayDirection, planeNormal);
            if (denominator_ > 0.000001f)
            {
                //The distance to the plane
                float rayDistance = Vector3.Dot(planePoint - rayPosition, planeNormal) / denominator_;

                //Where the ray intersects with a plane
                Vector3 p = rayPosition + rayDirection * rayDistance; //Hit wrt Camera
                DEBUG("\t[!!] Hit at [wrt Camera]: " + p);
                DEBUG("\t\t Hit_angle=  " + Vector3.Angle(p - P1_, planeNormal));

                Vector3 pPlane = Rt.transpose.MultiplyVector(p);
                DEBUG("\t[!!] Hit Plane at: " + pPlane);

                pPlane = Rt.transpose.MultiplyVector(p) + this.transform.position; //Hit wrt Plane
                DEBUG("\t[!!C] Hit Plane at [wrt Origin]: " + pPlane);

                Debug.DrawLine(transform.position, pPlane, Color.red);
            }
            else DEBUG("[!!] No intersection");
        }

        /////////////////////////////////////////
        // Project QUAD Shape wrt the Camera
        /////////////////////////////////////////
        line.enabled = DRAW_QUAD;
        if (DRAW_QUAD)
        {
            line.startColor = line.endColor = quadColor;
            line.startWidth = line.endWidth = lineRendererWidth;
            int i = 0;
            foreach (var P in Wall)
            {
                // DEBUG("\t QUAD[" + i + "]");
                Vector3 rayDirection = Vector3.zero;
                rayDirection.x = P.x;
                rayDirection.y = P.y;
                rayDirection.z = f;    //focal distance

                // DEBUG("\t >Plane point= \t " + planePoint );
                // DEBUG("\t >Plane normal= \t " + planeNormal );
                // DEBUG("\t >Ray origin= \t " + rayPosition );
                // DEBUG("\t >Ray direction= \t " + rayDirection );

                float denominator = Vector3.Dot(rayDirection, planeNormal);
                if (denominator > 0.000001f)
                {
                    //The distance to the plane
                    float rayDistance = Vector3.Dot(planePoint - rayPosition, planeNormal) / denominator;

                    //Where the ray intersects with a plane
                    Vector3 p = rayPosition + rayDirection * rayDistance; //Hit wrt Camera
                    //DEBUG("\t[!!] Hit at [wrt Camera]=  " + p );
                    //DEBUG("\t\t Hit_angle=  " + Vector3.Angle( p - P1_ , planeNormal) );

                    Vector3 pPlane = Rt.transpose.MultiplyVector(p) + this.transform.position; //Hit wrt Plane
                    DEBUG("\t[C_" + i + "] Hit Plane at =  " + pPlane);

                    line.SetPosition(i, new Vector3(pPlane.x, pPlane.y, lineRendererOffset));

                    if (DRAW_LINES) Debug.DrawLine(transform.position, pPlane, quadColor);
                }
                //else DEBUG("No intersection");
                i++;
            }
        }


        /////////////////////////////////////////
        // Project TRIANGLE Shape wrt the Camera
        /////////////////////////////////////////
        triLine.enabled = DRAW_TRI;
        if (DRAW_TRI)
        {
            triLine.startColor = triLine.endColor = triColor;
            triLine.startWidth = triLine.endWidth = lineRendererWidth;
            int i = 0;
            foreach (var P in triShape)
            {
                // DEBUG("\t\t TRI[" + i + "]");
                Vector3 rayDirection = Vector3.zero;
                rayDirection.x = P.x;
                rayDirection.y = P.y;
                rayDirection.z = f;    //focal distance

                // DEBUG("\t >Plane point= \t " + planePoint);
                // DEBUG("\t >Plane normal= \t " + planeNormal);
                // DEBUG("\t >Ray origin= \t " + rayPosition);
                // DEBUG("\t >Ray direction= \t " + rayDirection);

                float denominator = Vector3.Dot(rayDirection, planeNormal);

                if (denominator > 0.000001f)
                {
                    // The distance to the plane
                    float rayDistance = Vector3.Dot(planePoint - rayPosition, planeNormal) / denominator;

                    // Where the ray intersects with a plane
                    Vector3 p = rayPosition + rayDirection * rayDistance; //Hit wrt Camera
                    //DEBUG("\t[!!] Hit at [wrt Camera]=  " + p );
                    //DEBUG("\t\t Hit_angle=  " + Vector3.Angle( p - P1_ , planeNormal) );

                    Vector3 pPlane = Rt.transpose.MultiplyVector(p) + this.transform.position; //Hit wrt Plane
                    //DEBUG("\t[C_" + i + "] Hit Plane at =  " + pPlane);

                    triLine.SetPosition(i, new Vector3(pPlane.x, pPlane.y, lineRendererOffset));

                    if (DRAW_LINES) Debug.DrawLine(transform.position, pPlane, triColor);
                }
                //else DEBUG("No intersection");
                i++;
            }
        }


        /////////////////////////////////////////////////////////
        // Project POLYGON Shape wrt the Camera
        /////////////////////////////////////////////////////////
        polyLine.enabled = DRAW_POLY;
        if (DRAW_POLY)
        {
            //Wall points wrt origin // FIXME : is this needed?
            // Vector3[] CROSS = new[] {
            //     new Vector3(    0.0f,  250.0f, 0.0f),
            //     new Vector3( -250.0f,    0.0f, 0.0f),
            //     new Vector3(    0.0f, -250.0f, 0.0f),
            //     new Vector3(  250.0f,    0.0f, 0.0f),
            // };

            //Limit radius size based on focal distance
            if (LIMIT_RADIUS)
            {
                DEBUG("FocalDist= " + f);

                float minRadius = Mathf.Infinity;

                for (int k = 0; k < 4; k++)
                {
                    Debug.DrawLine(this.transform.position, Wall[k] * 5, camColor); // FIXME : move this somewhere else so it always executes and fix the scale issue

                    Vector3 P1 = -Rt.MultiplyVector(transform.position - Wall[k] * 5);
                    DEBUG("\t->P1[" + k + "]= " + P1);

                    float u1 = (f * P1.x) / P1.z;
                    float v1 = (f * P1.y) / P1.z;
                    DEBUG("1-(u,v)= " + u1 + " , " + v1);

                    Vector3 P2 = -Rt.MultiplyVector(transform.position - Wall[(k + 1) % 4] * 5);
                    DEBUG("\t->P2[" + k + "]= " + P2);

                    float u2 = (f * P2.x) / P2.z;
                    float v2 = (f * P2.y) / P2.z;
                    DEBUG("2-(u,v)= " + u2 + " , " + v2);

                    // https://en.wikipedia.org/wiki/Distance_from_a_point_to_a_line
                    // simplified since p=(x0,y0)=(0,0)
                    float distToEdge = Mathf.Abs(u2 * v1 - v2 * u1) / Mathf.Sqrt((v2 - v1) * (v2 - v1) + (u2 - u1) * (u2 - u1));
                    if (distToEdge < minRadius) minRadius = distToEdge;

                    //Vector3 c_W1_ = Rt.transpose.MultiplyVector( W1_ ) + this.transform.position; //Hit wrt Plane 
                    //DEBUG("\t->t_W["+k+"]= " + c_W1_ );
                }

                // Debug.DrawLine( new Vector3(xmin,ymax,0), new Vector3(xmin,ymin,0), Color.red  );
                // Debug.DrawLine( new Vector3(xmin,ymin,0), new Vector3(xmax,ymin,0), Color.red  );
                // Debug.DrawLine( new Vector3(xmax,ymin,0), new Vector3(xmax,ymax,0), Color.red  );
                // Debug.DrawLine( new Vector3(xmax,ymax,0), new Vector3(xmin,ymax,0), Color.red  );

                // polyLine.positionCount = 4;
                // polyLine.SetPosition( 0 , new Vector3(xmin,ymax, -1.0f ) );
                // polyLine.SetPosition( 1 , new Vector3(xmin,ymin, -1.0f ) );
                // polyLine.SetPosition( 2 , new Vector3(xmax,ymin, -1.0f ) );
                // polyLine.SetPosition( 3 , new Vector3(xmax,ymax, -1.0f ) );

                polyRadius = (int)minRadius;
                DEBUG("LimitMin= " + minRadius);
            }

            polyLine.positionCount = Mathf.Abs(polyVerts);
            polyLine.startColor = polyLine.endColor = polyColor;
            polyLine.startWidth = polyLine.endWidth = lineRendererWidth;

            float angleStep = (Mathf.PI * 2.0f) / polyVerts;
            for (int j = 0; j < polyVerts; j++)
            {
                //DEBUG("\t\t [" + i + "]");
                float angle = j * angleStep;
                //Limit size based either on polyRadius input or focal distance
                float Px = polyRadius * Mathf.Cos(angle);
                float Py = polyRadius * Mathf.Sin(angle);

                Vector3 rayDirection = Vector3.zero;
                rayDirection.x = Px;
                rayDirection.y = Py;
                rayDirection.z = f;    //focal distance

                // DEBUG("\t >Plane point= \t " + planePoint );
                // DEBUG("\t >Plane normal= \t " + planeNormal );
                // DEBUG("\t >Ray origin= \t " + rayPosition );
                // DEBUG("\t >Ray direction= \t " + rayDirection );

                float denominator = Vector3.Dot(rayDirection, planeNormal);
                if (denominator > 0.000001f)
                {
                    //The distance to the plane
                    float rayDistance = Vector3.Dot(planePoint - rayPosition, planeNormal) / denominator;

                    //Where the ray intersects with a plane
                    Vector3 p = rayPosition + rayDirection * rayDistance; //Hit wrt Camera
                    //DEBUG("\t[!!] Hit at [wrt Camera]=  " + p );
                    //DEBUG("\t\t Hit_angle=  " + Vector3.Angle( p - P1_ , planeNormal) );

                    Vector3 pPlane = Rt.transpose.MultiplyVector(p) + this.transform.position; //Hit wrt Plane
                    // DEBUG("\t[POLY_" + j + "] Hit Plane at =  " + pPlane);

                    polyLine.SetPosition(j, new Vector3(pPlane.x, pPlane.y, lineRendererOffset));

                    if (DRAW_LINES) Debug.DrawLine(transform.position, pPlane, polyColor);
                }
                //else DEBUG("No intersection");
            }
        }


        /////////////////////////////////////////////////////////
        ///Project SVG Shape wrt the Camera
        /////////////////////////////////////////////////////////
        // shapeLine.enabled = DRAW_SHAPE;
        // if (DRAW_SHAPE)
        // {
        //     shapeLine.positionCount = shape.Length;
        //     shapeLine.SetColors(shapeColor, shapeColor);

        //     for (int j = 0; j < shape.Length; j++)
        //     {
        //         //DEBUG("\t\t [" + i + "]");
        //         Vector3 rayDirection = Vector3.zero;
        //         rayDirection.x = shape[j].x;
        //         rayDirection.y = shape[j].y;
        //         rayDirection.z = f;     //focal distance

        //         float denominator = Vector3.Dot(rayDirection, planeNormal);
        //         if (denominator > 0.000001f)
        //         {
        //             //The distance to the plane
        //             float rayDistance = Vector3.Dot(planePoint - rayPosition, planeNormal) / denominator;

        //             //Where the ray intersects with a plane
        //             Vector3 p = rayPosition + rayDirection * rayDistance; //Hit wrt Camera
        //             Vector3 pPlane = Rt.transpose.MultiplyVector(p) + this.transform.position; //Hit wrt Plane

        //             //DEBUG("\t[Shape_"+j+"] Hit Plane at:   " + pPlane );
        //             if (DRAW_LINES) Debug.DrawLine(transform.position, pPlane, shapeColor);

        //             shapeLine.SetPosition(j, new Vector3(pPlane.x, pPlane.y, -10.0f));
        //         }
        //         //else DEBUG("No intersection");
        //     }
        // }


        // Translate upwards
        //transform.position += Vector3.up * 100 * Time.deltaTime;
    }


}
