// using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public class lineClipper : MonoBehaviour
{

    const float SCALE = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        // Example usage:
        Vector2 rectCenter = new Vector2(0, 0);
        float rectWidth = 50f * 2 * SCALE;
        float rectHeight = 50f * 2 * SCALE;
        var line = new Vector4(-50, 75, 75, -50) * SCALE;

        Debug.Log($"Original Line: {line}");
        Vector4 clippedLine = ClipLineToRectangle(line.x , line.y , line.z , line.w , rectCenter, rectWidth, rectHeight);
        Debug.Log($"Clipped Line Segment: {clippedLine.x}, {clippedLine.y}, {clippedLine.z}, {clippedLine.w}");


        // renderLine(new Vector4(-50, 50, 50, 50) * SCALE , Color.green );
        // renderLine(new Vector4(-50, -50, 50, -50) * SCALE , Color.green );
        // renderLine(new Vector4(-50, 50, 50, -50) * SCALE , Color.yellow );

        renderLine(line , Color.cyan, "original_line" );
        renderLine(clippedLine, Color.red, "clipped_line");
    }

    void renderLine(Vector4 lineCoordinates,  Color lineColor, string lineName = "Line")
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
        // lineRenderer.material.color = lineColor;
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


        // Check if endpoints are inside the rectangle
        bool inside1 = rectLeft <= x1 && x1 <= rectRight && rectBottom <= y1 && y1 <= rectTop;
        bool inside2 = rectLeft <= x2 && x2 <= rectRight && rectBottom <= y2 && y2 <= rectTop;
        Debug.Log($"Endpoints are inside: {inside1}, {inside2}");

        if (inside1 && inside2)
        {
            // Both endpoints are inside, return the original line
            return new Vector4(x1, y1, x2, y2);
        }
        else if (inside1)
        {
            // Clip the line to the rectangle boundaries from the inside
            Vector2 clipped = ClipToRectangle(x1, y1, x2, y2, rectLeft, rectRight, rectBottom, rectTop);
            return new Vector4(x1, y1, clipped.x, clipped.y);
        }
        else if (inside2)
        {
            // Clip the line to the rectangle boundaries from the inside
            Vector2 clipped = ClipToRectangle(x2, y2, x1, y1, rectLeft, rectRight, rectBottom, rectTop);
            return new Vector4(clipped.x, clipped.y, x2, y2);
        }
        else
        {
            // Both endpoints are outside, check for intersection points
            List<Vector2> intersections = FindIntersections(x1, y1, x2, y2, rectLeft, rectRight, rectBottom, rectTop);
            if (intersections.Count == 0)
            {
                // No intersection, return Vector4.zero or handle as needed
                return Vector4.zero;
            }
            else
            {
                // Clip the line to include only the portion inside the rectangle
                return new Vector4(intersections[0].x, intersections[0].y, intersections[1].x, intersections[1].y);
            }
        }
    }

    Vector2 ClipToRectangle(float x1, float y1, float x2, float y2, float rectLeft, float rectRight, float rectBottom, float rectTop)
    {
        // Clip the line to the rectangle boundaries
        float clippedX = Mathf.Clamp(x1, rectLeft, rectRight);
        float clippedY = Mathf.Clamp(y1, rectBottom, rectTop);
        return new Vector2(clippedX, clippedY);
    }

    List<Vector2> FindIntersections(float x1, float y1, float x2, float y2, float rectLeft, float rectRight, float rectBottom, float rectTop)
    {
        // Find intersection points between the line and rectangle edges
        List<Vector2> intersections = new List<Vector2>();

        // Check left edge
        Vector2 leftIntersection = LineIntersection(x1, y1, x2, y2, rectLeft, rectBottom, rectLeft, rectTop);
        if (leftIntersection != Vector2.zero)
        {
            intersections.Add(leftIntersection);
        }

        // Check right edge
        Vector2 rightIntersection = LineIntersection(x1, y1, x2, y2, rectRight, rectBottom, rectRight, rectTop);
        if (rightIntersection != Vector2.zero)
        {
            intersections.Add(rightIntersection);
        }

        // Check bottom edge
        Vector2 bottomIntersection = LineIntersection(x1, y1, x2, y2, rectLeft, rectBottom, rectRight, rectBottom);
        if (bottomIntersection != Vector2.zero)
        {
            intersections.Add(bottomIntersection);
        }

        // Check top edge
        Vector2 topIntersection = LineIntersection(x1, y1, x2, y2, rectLeft, rectTop, rectRight, rectTop);
        if (topIntersection != Vector2.zero)
        {
            intersections.Add(topIntersection);
        }

        return intersections;
    }

    Vector2 LineIntersection(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
{
    // Calculate the intersection point between two lines
    float den = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
    if (Mathf.Approximately(den, 0f))
    {
        // Lines are parallel or coincident, return midpoint as a potential intersection
        return new Vector2((x1 + x2) / 2f, (y1 + y2) / 2f);
    }

    float px = ((x1 * y2 - y1 * x2) * (x3 - x4) - (x1 - x2) * (x3 * y4 - y3 * x4)) / den;
    float py = ((x1 * y2 - y1 * x2) * (y3 - y4) - (y1 - y2) * (x3 * y4 - y3 * x4)) / den;
    return new Vector2(px, py);
}

}
