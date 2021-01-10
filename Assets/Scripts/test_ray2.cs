using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test_ray2 : MonoBehaviour
{
    public static class Poly
    {
        public static bool ContainsPoint(Vector2[] polyPoints, Vector2 p)
        {
            var j = polyPoints.Length - 1;
            var inside = false;
            for (int i = 0; i < polyPoints.Length; j = i++)
            {
                Vector2 pi = polyPoints[i];
                Vector2 pj = polyPoints[j];
                if (((pi.y <= p.y && p.y < pj.y) || (pj.y <= p.y && p.y < pi.y)) &&
                    (p.x < (pj.x - pi.x) * (p.y - pi.y) / (pj.y - pi.y) + pi.x))
                    inside = !inside;
            }
            return inside;
        }
    }



    //var plane : Plane = new Plane(Vector3.up, Vector3.zero);;
    public Transform wall1;
    public Transform wall2;

    public float ent = 0.0f;

    void Update()
    {
        Transform wall;
        Vector3 mousePos = Input.mousePosition;
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(mousePos);

            for (int i = 0; i < 2; i++)
            {
                ent = 0.0f;

                if (i == 0) wall = wall1;
                else wall = wall2;

				Debug.Log("Hitting= " + wall.gameObject.name);

				Plane plane = new Plane(wall.transform.position, wall.up.normalized);
				Debug.Log("wallPos=" + wall.transform.position + " wallNrml"+ wall.up.normalized +" thisTransform= " + transform.position);

                if (plane.Raycast(ray, out ent))
                {

                    Vector3 hitPoint = ray.GetPoint(ent);
                    Debug.Log("Raycast hit Plane at distance: " + ent);
                    Debug.Log("hitPoint: " + hitPoint);

					GameObject planeManager = GameObject.Find("planeManager");
                    GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //go.transform.parent = this.transform;

					go.transform.parent = planeManager.transform; //wall.transform; //wall1;
					go.transform.rotation = wall.transform.rotation;
					go.transform.position = hitPoint + wall.position;
					//go.transform.LookAt(wall.up.normalized);
                    go.transform.localScale = new Vector3(50.0f, 50.0f, 50.0f);
					//go.transform.LookAt(this.transform);

                    if (i == 0)
                    {
                        go.GetComponent<Renderer>().material.color = Color.green;
                    }
                    else
                    {
                        //go.transform.parent = wall.transform;
                        go.GetComponent<Renderer>().material.color = Color.blue;
                    }
                    Debug.DrawRay(ray.origin, ray.direction * ent, Color.green);

                }
                else
                {
                    Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red);
                    Debug.Log("No intersection");
                }
            }

        }
    }


}
