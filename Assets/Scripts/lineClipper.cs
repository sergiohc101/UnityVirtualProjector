// using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public class lineClipper : MonoBehaviour
{
    const float SCALE = 5.0f;

    // Define region codes for Cohen-Sutherland algorithm
    const int INSIDE = 0; // 0000
    const int LEFT = 1;   // 0001
    const int RIGHT = 2;  // 0010
    const int BOTTOM = 4; // 0100
    const int TOP = 8;    // 1000

    // Start is called before the first frame update
    void Start()
    {
        // Example usage:
        Vector2 rectCenter = new Vector2(0, 0);
        float rectWidth = 50f * 2 * SCALE;
        float rectHeight = 50f * 2 * SCALE;
        var line = new Vector4(-50, 75, 75, -50) * SCALE; // Apply the scale to the line coordinates

        Debug.Log($"Original Line: {line}");
        Vector4 clippedLine = ClipLineToRectangle(line.x, line.y, line.z, line.w, rectCenter, rectWidth, rectHeight);
        Debug.Log($"Clipped Line Segment: {clippedLine.x}, {clippedLine.y}, {clippedLine.z}, {clippedLine.w}");


        // renderLine(new Vector4(-50, 50, 50, 50) * SCALE , Color.green );
        // renderLine(new Vector4(-50, -50, 50, -50) * SCALE , Color.green );
        // renderLine(new Vector4(-50, 50, 50, -50) * SCALE , Color.yellow );

        renderLine(line, Color.cyan, "original_line");
        renderLine(clippedLine, Color.red, "clipped_line");
    }

    void renderLine(Vector4 lineCoordinates, Color lineColor, string lineName = "Line")
    {
        // Create a new GameObject as a child
        GameObject lineObject = new GameObject(lineName);
        lineObject.transform.parent = transform; // Set the current object as the parent

        // Attach LineRenderer component to the child GameObject
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

        // Set line properties
        lineRenderer.startWidth = 5f;
        lineRenderer.endWidth = 5f;
        lineRenderer.positionCount = 2;

        // Set line positions based on the Vector4 coordinates
        lineRenderer.SetPosition(0, new Vector3(lineCoordinates.x, lineCoordinates.y, -1));
        lineRenderer.SetPosition(1, new Vector3(lineCoordinates.z, lineCoordinates.w, -1));

        // Set line color
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineRenderer.endColor = lineColor;
    }

    // // Update is called once per frame
    // void Update()
    // {
    //
    // }

    Vector4 ClipLineToRectangle(float x1, float y1, float x2, float y2, Vector2 rectCenter, float rectWidth, float rectHeight)
    {
        // Determine rectangle corners
        float rectLeft = rectCenter.x - rectWidth / 2f;
        float rectRight = rectCenter.x + rectWidth / 2f;
        float rectTop = rectCenter.y + rectHeight / 2f;
        float rectBottom = rectCenter.y - rectHeight / 2f;

        Debug.Log($"Rectangle center: {rectCenter}");
        Debug.Log($"Rectangle corners: {rectLeft}, {rectRight}, {rectTop}, {rectBottom}");
        renderLine(new Vector4(rectLeft, rectTop, rectRight, rectBottom), Color.magenta , "_Rectangle_corners");
        renderLine(new Vector4(rectLeft, rectTop, rectRight, rectTop), Color.green , "_Rectangle_top");
        renderLine(new Vector4(rectLeft, rectBottom, rectRight, rectBottom), Color.green , "_Rectangle_bottom");
        renderLine(new Vector4(rectLeft, rectTop, rectLeft, rectBottom), Color.green , "_Rectangle_left");
        renderLine(new Vector4(rectRight, rectTop, rectRight, rectBottom), Color.green , "_Rectangle_right");

        // Compute region codes for both endpoints
        int code1 = ComputeRegionCode(x1, y1, rectLeft, rectRight, rectBottom, rectTop);
        int code2 = ComputeRegionCode(x2, y2, rectLeft, rectRight, rectBottom, rectTop);

        // Check if both endpoints are inside the rectangle
        if ((code1 & code2) == 0)
        {
            // Both endpoints are inside or on the boundary, return the original line
            return new Vector4(x1, y1, x2, y2);
        }

        // Both endpoints are outside the same side, line is completely outside
        if ((code1 & code2) != 0)
        {
            return Vector4.zero;
        }

        // Clip the line using Cohen-Sutherland algorithm
        while ((code1 | code2) != 0)
        {
            // Both endpoints are outside the same side, line is completely outside
            if ((code1 & code2) != 0)
            {
                return Vector4.zero;
            }

            float x, y; // Clipped coordinates
            int codeOut = (code1 != 0) ? code1 : code2;

            // Determine intersection point
            if ((codeOut & TOP) != 0)
            {
                x = x1 + (x2 - x1) * (rectTop - y1) / (y2 - y1);
                y = rectTop;
            }
            else if ((codeOut & BOTTOM) != 0)
            {
                x = x1 + (x2 - x1) * (rectBottom - y1) / (y2 - y1);
                y = rectBottom;
            }
            else if ((codeOut & RIGHT) != 0)
            {
                y = y1 + (y2 - y1) * (rectRight - x1) / (x2 - x1);
                x = rectRight;
            }
            else if ((codeOut & LEFT) != 0)
            {
                y = y1 + (y2 - y1) * (rectLeft - x1) / (x2 - x1);
                x = rectLeft;
            }
            else
            {
                // Should not reach here, but to handle unexpected cases
                return Vector4.zero;
            }

            // Replace the outside endpoint with the intersection point
            if (codeOut == code1)
            {
                x1 = x;
                y1 = y;
                code1 = ComputeRegionCode(x1, y1, rectLeft, rectRight, rectBottom, rectTop);
            }
            else
            {
                x2 = x;
                y2 = y;
                code2 = ComputeRegionCode(x2, y2, rectLeft, rectRight, rectBottom, rectTop);
            }
        }

        return new Vector4(x1, y1, x2, y2);
    }

    int ComputeRegionCode(float x, float y, float rectLeft, float rectRight, float rectBottom, float rectTop)
    {
        int code = INSIDE; // Initialize as inside

        if (x < rectLeft)
        {
            code |= LEFT;
        }
        else if (x > rectRight)
        {
            code |= RIGHT;
        }

        if (y < rectBottom)
        {
            code |= BOTTOM;
        }
        else if (y > rectTop)
        {
            code |= TOP;
        }

        return code;
    }
}

