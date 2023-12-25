using UnityEngine;

public static class lineClipper
{
// Define region codes for Cohen-Sutherland algorithm
const int INSIDE = 0; // 0000
const int LEFT = 1;   // 0001
const int RIGHT = 2;  // 0010
const int BOTTOM = 4; // 0100
const int TOP = 8;    // 1000

static int ComputeRegionCode(float x, float y, float rectLeft, float rectRight, float rectTop, float rectBottom)
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

public static Vector4 ClipLineToRectangle(float x1, float y1, float x2, float y2, Vector2 rectCenter, float rectWidth, float rectHeight)
{
    // Determine rectangle corners
    float rectLeft = rectCenter.x - rectWidth / 2f;
    float rectRight = rectCenter.x + rectWidth / 2f;
    float rectTop = rectCenter.y + rectHeight / 2f;
    float rectBottom = rectCenter.y - rectHeight / 2f;

    Debug.Log($"Rectangle center: {rectCenter}");
    Debug.Log($"Rectangle corners: {rectLeft}, {rectRight}, {rectTop}, {rectBottom}");
    // renderLine(new Vector4(rectLeft, rectTop, rectRight, rectBottom), Color.magenta , "_Rectangle_corners");
    // renderLine(new Vector4(rectLeft, rectTop, rectRight, rectTop), Color.green , "_Rectangle_top");
    // renderLine(new Vector4(rectLeft, rectBottom, rectRight, rectBottom), Color.green , "_Rectangle_bottom");
    // renderLine(new Vector4(rectLeft, rectTop, rectLeft, rectBottom), Color.green , "_Rectangle_left");
    // renderLine(new Vector4(rectRight, rectTop, rectRight, rectBottom), Color.green , "_Rectangle_right");

    // Compute region codes for both endpoints
    int code1 = ComputeRegionCode(x1, y1, rectLeft, rectRight, rectTop, rectBottom);
    int code2 = ComputeRegionCode(x2, y2, rectLeft, rectRight, rectTop, rectBottom);
    Debug.Log($"Region codes: {code1},{code2}");

    // Both endpoints are inside or on the boundary, return the original line
    if ((code1 | code2) == 0)
    {
        Debug.Log("Both endpoints are inside");
        return new Vector4(x1, y1, x2, y2);
    }

    // Both endpoints are outside the same side, line is completely outside
    if ((code1 & code2) != 0)
    {
        Debug.Log("Both endpoints are outside");
        return Vector4.zero;
    }

    // Clip the line using Cohen-Sutherland algorithm
    while ((code1 | code2) != 0)
    {
        // At least one endpoint is outside the clip rectangle; pick it.
        Debug.Log("Clip line segment");

        float x, y; // Clipped coordinates
        int codeOut = code2 > code1 ? code2 : code1;

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
            code1 = ComputeRegionCode(x1, y1, rectLeft, rectRight, rectTop, rectBottom);
        }
        else
        {
            x2 = x;
            y2 = y;
            code2 = ComputeRegionCode(x2, y2, rectLeft, rectRight, rectTop, rectBottom);
        }
    }

    return new Vector4(x1, y1, x2, y2);
}

public static Vector3[] clipShapeToRectangle(Vector3[] shape, Vector2 rectCenter, float rectWidth, float rectHeight)
{
    Debug.Log($"Clipping shape containing [{shape.Length}] points.");

    Vector3[] clippedShape = new Vector3[shape.Length];

    for (int i = 0; i < shape.Length; i++)
    {
        int j = (i+1) % shape.Length;
        Vector4 clippedLine = lineClipper.ClipLineToRectangle(shape[i].x, shape[i].y, shape[j].x, shape[j].y, rectCenter, rectWidth, rectHeight);
        clippedShape[i] = clippedLine;
    }

    return clippedShape;
}

}


public class lineClipperRenderer : MonoBehaviour
{
    const float SCALE = 5.0f;
    // Start is called before the first frame update
    void Start()
    {
        // Example usage:
        Vector2 rectCenter = new Vector2(0, 0);
        float rectWidth = 50f * 2 * SCALE;
        float rectHeight = 50f * 2 * SCALE;
        var line = new Vector4(-50, 75, 75, -50) * SCALE; // Apply the scale to the line coordinates

        Debug.Log($"Original Line: {line}");
        Vector4 clippedLine = lineClipper.ClipLineToRectangle(line.x, line.y, line.z, line.w, rectCenter, rectWidth, rectHeight);
        Debug.Log($"Clipped Line Segment: {clippedLine.x}, {clippedLine.y}, {clippedLine.z}, {clippedLine.w}");
        renderLine(line, Color.cyan, "original_line");
        renderLine(clippedLine, Color.red, "clipped_line");

        var line2 = new Vector4(-70, -20, 75, -50) * SCALE;
        Vector4 clippedLine2 = lineClipper.ClipLineToRectangle(line2.x, line2.y, line2.z, line2.w, rectCenter, rectWidth, rectHeight);
        renderLine(line2, Color.cyan, "original_line2");
        renderLine(clippedLine2, Color.red, "clipped_line2");

        var line3 = new Vector4(-70, 100, -75, -150) * SCALE;
        Vector4 clippedLine3 = lineClipper.ClipLineToRectangle(line3.x, line3.y, line3.z, line3.w, rectCenter, rectWidth, rectHeight);
        renderLine(line3, Color.cyan, "original_line3");
        renderLine(clippedLine3, Color.red, "clipped_line3");

        // Draw a test shape
        renderShape();
    }

    // Update is called once per frame
    void Update() { }

    void renderShape()
    {
        int sides = 6; // Number of sides for the geometric shape
        float radius = 1f; // Radius of the geometric shape
        Vector3[] polygon = new Vector3[sides + 1];

        for (int i = 0; i <= sides; i++)
        {
            float angle = i * 2 * Mathf.PI / sides;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;

            polygon[i] = new Vector3(x, y, 0f);
        }

        ////////////
        // Transformations
        ////////////
        // Translate the polygon (e.g., move it to the right by 2 units)
        Vector3 translation = new Vector3(0f, 00f, 0f);
        for (int i = 0; i <= sides; i++)
        {
            polygon[i] += translation;
        }

        // Rotate the polygon (e.g., rotate it by 45 degrees around the z-axis)
        float rotationAngle = 70f;
        Quaternion rotation = Quaternion.Euler(0f, 0f, rotationAngle);
        for (int i = 0; i <= sides; i++)
        {
            polygon[i] = rotation * polygon[i];
        }

        // Scale the polygon (e.g., scale it uniformly by a factor of 1.5)
        float scaleFactor = 300.0f;
        for (int i = 0; i <= sides; i++)
        {
            polygon[i] *= scaleFactor;
        }
        ////////////

        // Render original shape
        for (int i = 0; i < polygon.Length; i++)
        {
            int j = (i+1) % polygon.Length;
            Vector2 p1 = new Vector2(polygon[i].x, polygon[i].y);
            Vector2 p2 = new Vector2(polygon[j].x, polygon[j].y);
            Vector4 segment = new Vector4(p1.x, p1.y, p2.x, p2.y);
            renderLine(segment,Color.green, "polygon_"+i);
        }

        // Render clipped shape
        Vector2 rectCenter = new Vector2(0, 0);
        float rectWidth = 50f * 2 ; // FIXME
        float rectHeight = 50f * 2 ; // FIXME

        // Clip shape to plane
        lineClipper.clipShapeToRectangle(polygon, rectCenter, rectWidth, rectHeight);
        // Render clipped shape
        for (int i = 0; i < polygon.Length; i++)
        {
            int j = (i+1) % polygon.Length;
            Vector4 clippedLine = lineClipper.ClipLineToRectangle(polygon[i].x, polygon[i].y, polygon[j].x, polygon[j].y, rectCenter, rectWidth, rectHeight);
            renderLine(clippedLine,Color.magenta, "clipped_polygon_"+i);
        }


    }

    static void renderLine(Vector4 lineCoordinates, Color lineColor, string lineName = "Line")
    {
        // Create a new GameObject as a child
        GameObject lineObject = new GameObject(lineName);
        // lineObject.transform.parent = transform; // Set the current object as the parent

        // Attach LineRenderer component to the child GameObject
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

        // Set line properties
        lineRenderer.startWidth = 5f;
        lineRenderer.endWidth = 5f;
        lineRenderer.positionCount = 2;

        float Z = 0.0f;
        if(lineName.StartsWith("clipped")) Z = -1.0f;

        // Set line positions based on the Vector4 coordinates
        lineRenderer.SetPosition(0, new Vector3(lineCoordinates.x, lineCoordinates.y, Z));
        lineRenderer.SetPosition(1, new Vector3(lineCoordinates.z, lineCoordinates.w, Z));

        // Set line color
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineRenderer.endColor = lineColor;
    }


}

