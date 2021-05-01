//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;


public class rayTracer
{
    float EPS = 0.000001f;
    Matrix4x4 Rt;
    Vector3 planePoint, planeNormal;
    Vector3 rayPosition; // FIXME : Is this ever nonzero?
    bool DEBUG_LOGS = false;

    void setDebugLogs(bool debug) { DEBUG_LOGS = debug; }
    void setRtMatrix(Matrix4x4 M) { Rt = M; }
    void DEBUG(string str) { if (DEBUG_LOGS) Debug.Log(str); }

    public rayTracer(Matrix4x4 M, bool debug)
    {
        setRtMatrix(M);
        setDebugLogs(debug);
    }

    public void setup(Vector3 _planePoint, Vector3 _planeNormal, Vector3 _rayPosition)
    {
        planePoint = _planePoint;
        planeNormal = _planeNormal;
        rayPosition = _rayPosition;
    }

    public Vector3 trace(Vector3 rayDirection)
    {
        // This function computes the ray vs plane collition
        // A plane can be defined as:
        // a point (p0) representing how far the plane is from the world origin
        // a normal (n) defines the orientation of the plane (should be negative if we are firing the ray from above)
        // We are interested in calculating a point (p) in the the plane defined by (p0) with normal (n)
        // The vector defined by (p) and (p0) is always perpendicular to the normal: (p0 - p) . n = 0

        // A ray to point (p) can be defined as: rayPosition + rayDirection * rayDistance = p
        // In this case the rayPosition is the coordinate system origin
        DEBUG("\t >Plane point= \t " + planePoint);
        DEBUG("\t >Plane normal= \t " + planeNormal);
        DEBUG("\t >Ray origin= \t " + rayPosition);
        DEBUG("\t >Ray direction= \t " + rayDirection);

        // There is a chance that the line does not intersect with the plane, 
        // we can check this by first calculating the inner product between the rayDirection and the planeNormal
        // to check that the denominator is not smaller than some epsilon (EPS)
        // We also check that the denominator is positive else the ray is pointing towards the opposite direction
        Vector3 pPlane = Vector3.zero;
        float denominator = Vector3.Dot(rayDirection, planeNormal);
        if (denominator > EPS)
        {
            //The distance to the plane
            float rayDistance = Vector3.Dot(planePoint - rayPosition, planeNormal) / denominator;

            //Where the ray intersects with a plane
            Vector3 p = rayPosition + rayDirection * rayDistance; //Hit wrt Camera
            //DEBUG("\t[!!] Hit at [wrt Camera]: " + p);

            pPlane = Rt.transpose.MultiplyVector(p);
            //DEBUG("\t[!!] Hit Plane at: " + pPlane);

            //pPlane = Rt.transpose.MultiplyVector(p) + transform.position; //Hit wrt Plane
            //Debug.DrawLine(transform.position, pPlane, Color.red);
        }
        else DEBUG("[!!] No intersection");
        return pPlane;
    }

    public void traceShape(Vector3[] shape, Vector3 camPosition, LineRenderer lineRenderer, int lineRendererOffset, bool DRAW_LINES)
    {
        int i = 0;
        foreach (var rayDirection in shape)
        {
            DEBUG("\t Shape[" + i + "]");

            // Get with wrt to the plane
            Vector3 pPlane = trace(rayDirection) + camPosition;
            //DEBUG("\t[C_" + i + "] Hit Plane at =  " + pPlane);

            lineRenderer.SetPosition(i, new Vector3(pPlane.x, pPlane.y, lineRendererOffset));

            if (DRAW_LINES) Debug.DrawLine(camPosition, pPlane, lineRenderer.startColor);

            i++;
        }
    }

}
