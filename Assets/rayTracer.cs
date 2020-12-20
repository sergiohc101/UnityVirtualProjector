using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rayTracer : MonoBehaviour {

	public bool MOVE_CAM = true;
	public bool DRAW_LINES = true;
	public Color camColor;


	public bool DRAW_QUAD = true;
	public Color quadColor = Color.magenta;

	public bool DRAW_TRI = true;
	public Color triColor = Color.cyan;

	public bool DRAW_POLY = true;
	public Color polyColor = Color.red;
	public int polyVerts = 6;
	public int polyRadius = 100;

	/*
	public bool DRAW_SHAPE = true;
	public Color shapeColor = Color.green;
	public TextAsset textAsset;
	Vector3[] shape;
	*/


	public float f = 1000.0f;
	public float u = 640.0f;
	public float v = 480.0f;
	 
	//public Vector3 up = Vector3.up;

	public float[,] K = new float[3,3];

	//public Matrix4x4 R;

	public float timeToCompleteCircle = 5.0f;
	public float radius = 300;
	public float currentAngle;


	//Wall points wrt origin
	Vector3[] Wall = new [] { 
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


	///*
	Vector3 [] triShape = new [] {
		new Vector3( 0.0f, 		190.0f, 0.0f),
		new Vector3( -192.0f, 	-144.0f, 0.0f),
		new Vector3( 192.0f,	-144.0f, 0.0f),
		new Vector3( 0.0f, 		190.0f, 0.0f),
		new Vector3( 0.0f, 		140.0f, 0.0f),
		new Vector3( -142.0f, 	-114.0f, 0.0f),
		new Vector3( 142.0f, 	-114.0f, 0.0f),
		new Vector3( 0.0f, 		140.0f, 0.0f)
	};
	//*/

	// Use this for initialization
	void Start () {
		//Change camera Gameobject color
		this.GetComponent<Renderer>().material.color = camColor;


		//Set up camera matrix
		K[0,0] = K[1,1] = f;
		K[0,2] = -u/2.0f;
		K[1,2] = -v/2.0f;
		K[2,2] = -1;


		//GameObject myLine = new GameObject();
		//myLine.transform.position = start;
		line = this.gameObject.AddComponent<LineRenderer>();
		//line = this.gameObject.GetComponent<LineRenderer>();
		//lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
		line.material = new Material(Shader.Find("Sprites/Default"));
		Color color = Color.magenta;
		line.SetColors(color, color);
		//line.startColor = Color.red;	line.endColor = Color.red;
		line.SetWidth(10.0f, 10.0f);
		line.loop = true;
		line.positionCount = 4;

		/*
		Vector3 ofs = new Vector3( 0.0f, 0.0f, -10.0f);
		line.SetPosition(0, Wall[0] *5.0f + ofs );
		line.SetPosition(1, Wall[1] *5.0f + ofs );
		line.SetPosition(2, Wall[2] *5.0f + ofs );
		line.SetPosition(3, Wall[3] *5.0f + ofs );
		*/

		triangleRenderer = new GameObject( "triangleRenderer" );
		//triangleRenderer.transform.parent = this.gameObject.transform;
		//triangleRenderer.transform.position = this.gameObject.transform.position;
		//myLine.transform.position = start;
		triangleRenderer.AddComponent<LineRenderer>();
		triLine = triangleRenderer.GetComponent<LineRenderer>();
		triLine.material = new Material(Shader.Find("Sprites/Default"));
		Color colorT = Color.green;
		//triLine.SetColors(colorT, colorT);
		triLine.startColor = Color.cyan;	triLine.endColor = Color.cyan;
		triLine.SetWidth(10.0f, 10.0f);
		triLine.positionCount = triShape.Length;
		//GameObject.Destroy(myLine, duration);


		///////////POLYGON
		polyRenderer = new GameObject( "polyRenderer" );
		polyRenderer.AddComponent<LineRenderer>();
		polyLine = polyRenderer.GetComponent<LineRenderer>();
		polyLine.material = new Material(Shader.Find("Sprites/Default"));
		polyLine.loop = true;
		polyLine.startColor = polyColor;	triLine.endColor = polyColor;
		polyLine.SetWidth(10.0f, 10.0f);
		polyLine.positionCount = polyVerts;
		//GameObject.Destroy(myLine, duration);


		/*
		////// SVG Shape
		string[] lines = textAsset.text.Split("\n"[0]); // gets all lines into separate strings
		print(" NLines= " + lines.Length );
		shape = new Vector3[ lines.Length ];

		for (var i = 0; i < lines.Length; i++){
			var pt = lines[i].Split(","[0]); // gets 3 parts of the vector into separate strings
			var x = float.Parse(pt[0]);
			var y = float.Parse(pt[1]);
			var z = 0.0f; 	//float.Parse(pt[2]);
			shape[i] = new Vector3(x,y,z);

			//print( "V["+i+"]= " + shape[i] );
		}
		shapeRenderer = new GameObject( "shapeRenderer" );
		shapeRenderer.AddComponent<LineRenderer>();
		shapeLine = shapeRenderer.GetComponent<LineRenderer>();
		shapeLine.material = new Material(Shader.Find("Sprites/Default"));
		shapeLine.loop = true;
		shapeLine.startColor = polyColor;	shapeLine.endColor = polyColor;
		shapeLine.SetWidth(10.0f, 10.0f);
		shapeLine.positionCount = polyVerts;
		*/

	}


	// Update is called once per frame
	void Update () {


		// Camera obscura model 
		// lp = K [ R|t ] P, o
		// lp = K R [ I| -c ] P
		// looking at the origin, up vector is [0 1 0]

		//float timeToCompleteCircle = 5.0f;
		float speed = (Mathf.PI * 2.0f) / timeToCompleteCircle;
		//float Cx;
		//float Cy;
		//float radius = 300;
		if (MOVE_CAM){
			currentAngle += Time.deltaTime * speed;
			if(currentAngle >= Mathf.PI * 2.0f) currentAngle = 0.0f;

			float Cx = radius * Mathf.Cos (currentAngle);
			float Cy = radius * Mathf.Sin (currentAngle);
		//if (MOVE_CAM)
			transform.position = new Vector3 (Cx, Cy, transform.position.z);
		}
		//triangleRenderer.transform.position = this.gameObject.transform.position* 100.0f;



		transform.LookAt( Vector3.zero );

		if(DRAW_LINES)
			Debug.DrawLine(Vector3.zero, transform.position , Color.red);
		

		/* // Draw Lines
		line.SetPosition(0, start);
		line.SetPosition(1, Vector3.zero );
		line.SetPosition(2, this.transform.position );
		line.SetPosition(3, end);
		//*/



		////////////////////////////////////
		Matrix4x4 L = transform.localToWorldMatrix;
		//Debug.Log( "L= \n" + L );


		Vector3 scale = transform.localScale;
		//Debug.Log("ScaleVector= " + scale);

		Vector3 t = L.transpose.MultiplyVector( -transform.position ) / 50.0f;
		//Debug.Log( "Pos= " + (transform.position) );
		Debug.Log( "|Pos|= " + transform.position.magnitude );

		Debug.Log( "t= " + t );

		//Copy origial view matrix
		Matrix4x4 Rt = L;
		//Debug.Log( "Rt= \n" + Rt );
		//Remove matrix scale
		Rt.SetColumn( 0, Rt.GetColumn(0)/scale[0] );
		Rt.SetColumn( 1, Rt.GetColumn(1)/scale[1] );
		Rt.SetColumn( 2, Rt.GetColumn(2)/scale[2] );
		//Transpose Matrix
		Rt = Rt.transpose;
		Debug.Log( "Rt^T= \n" + Rt );
			t = Rt.MultiplyVector( -transform.position ) ;
				//Debug.Log( "nt= " + t );
		//*
		Vector3 P1_ = -Rt.MultiplyVector( transform.position - Wall[0] ) ;
			Debug.Log( "\t->P1= " + P1_ );

		Vector3 P2_ = -Rt.MultiplyVector( transform.position - Wall[1] );
			Debug.Log( "\t->P2= " + P2_ );

		Vector3 P3_ = -Rt.MultiplyVector( transform.position - Wall[2] );
			Debug.Log( "\t->P3= " + P3_ );
			
		Vector3 P4_ = -Rt.MultiplyVector( transform.position - Wall[3] );
			Debug.Log( "\t->P4= " + P4_ );
		//*/


		//Compute plane normal wrt the camera
		Vector3 P21 = P2_ - P1_;
		Vector3 P41 = P4_ - P1_;
		//Debug.Log( "\t\t P_21= " + P21 );
		//Debug.Log( "\t\t P_41= " + P41 );

		//float planeAngle = Vector3.Angle(P21, P41);
		//Debug.Log( "\t\t Plane_angle=  " + planeAngle );

		Vector3 planeNormal = Vector3.Cross(P21, P41).normalized;
		//planeNormal.z *= -1.0f;
		Debug.Log( "\t\t Plane_normal=  " + planeNormal );

			//Debug.DrawLine(Vector3.zero, -planeNormal * 500.0f , Color.cyan);
			//Debug.DrawLine(transform.position, transform.position - planeNormal * 500.0f , Color.cyan);

		/*
		//Define ray from camera into plane
		Ray ray = new Ray( transform.position, Vector3.zero - transform.position );
		Debug.Log( " >Ray= " + ray.ToString() );
		// create a plane at 0,0,0 whose normal points to -Z:
		//Plane hPlane = new Plane( new Vector3(0.0f, 0.0f, -1.0f) , plane.transform.position);
		//Plane hPlane = new Plane( new Vector3( 0.0f, 0.0f, -1.0f) , new Vector3(0.0f, 0.0f, -10.0f) );//Vector3.zero);
		Plane hPlane = new Plane( Vector3.back, Vector3.zero );
		Debug.Log( " >Plane= " + hPlane.normal + ", " + hPlane.distance );
		
		// Plane.Raycast stores the distance from ray.origin to the hit point in this variable:
		float distance = 0; 
		if( hPlane.Raycast(ray, out distance) ){
			Vector3 hitPoint = ray.GetPoint(distance);
			Debug.Log( " [!] Hit at=  " + hitPoint );
		}
		else Debug.Log( " NO HIT ):  d= "+ distance );
		*/

		/*
		///Project Shape 
		short i = 0;
		foreach (var P in Wall) {

			Vector3 imgPoint = this.transform.forward;
			imgPoint.x += P.x;
			imgPoint.y += P.y;
			imgPoint.z = 100.0f;

			//Define ray from camera into plane
			//ray = new Ray( transform.position, this.transform.forward ); //Shoots stright into Z axis
			ray = new Ray( this.transform.position, imgPoint );
			//Debug.Log( " >Ray= " + ray.ToString() );

			distance = 0; 
			// if the ray hits the plane...
			if( hPlane.Raycast(ray, out distance) ){
				// get the hit point:
				Vector3 hitPoint = ray.GetPoint(distance);

				Debug.Log( " [! "+ i +" !] Hit at=  " + hitPoint );

				Debug.DrawLine(transform.position, hitPoint, Color.blue);

			}
			else
				Debug.Log( " NO HIT ):  d= "+ distance );

			i++;
		}
		*/


		//A plane can be defined as:
		//a point representing how far the plane is from the world origin
		//Vector3 planePoint = Vector3.zero;
		Vector3 planePoint = t;
		//a normal (defining the orientation of the plane), should be negative if we are firing the ray from above
		//Vector3 planeNormal = Vector3.back;
		//We are intrerested in calculating a point in this plane called p
		//The vector between p and p0 and the normal is always perpendicular: (p - p_0) . n = 0

		//A ray to point p can be defined as: rayPos + rayDirection * rayDistance = p, where:
		//the origin of the ray
		//This should be the coordinate system origin
		Vector3 rayPosition = Vector3.zero;  //this.transform.position;
		//l is the direction of the ray
		Vector3 rayDirection = t; //P4_;
		//t is the length of the ray, which we can get by combining the above equations:
		//t = ((p_0 - l_0) . n) / (l . n)

		Debug.Log( " \t >Plane point= \t " + planePoint );
		Debug.Log( " \t >Plane normal= \t " + planeNormal );

		Debug.Log( " \t >Ray origin= \t " + rayPosition );
		Debug.Log( " \t >Ray direction= \t " + rayDirection );

		//But there's a chance that the line doesn't intersect with the plane, and we can check this by first
		//calculating the denominator and see if it's not small. 
		//We are also checking that the denominator is positive or we are looking in the opposite direction
		float denominator_ = Vector3.Dot(rayDirection, planeNormal);

		if (denominator_ > 0.000001f)
		{
			//The distance to the plane
			float rayDistance = Vector3.Dot(planePoint - rayPosition, planeNormal) / denominator_;

			//Where the ray intersects with a plane
			Vector3 p = rayPosition + rayDirection * rayDistance; //Hit wrt Camera

				Debug.Log( " \t[!!] Hit at [wrt Camera]=  " + p );
				Debug.Log( "\t\t Hit_angle=  " + Vector3.Angle( p - P1_ , planeNormal) );
				

			Vector3 pPlane = Rt.transpose.MultiplyVector( p );	
				Debug.Log( " \t[!!] Hit Plane at =  " + pPlane );


			pPlane = Rt.transpose.MultiplyVector( p ) + this.transform.position; //Hit wrt Plane
				Debug.Log( " \t[!!C] Hit Plane at [wrt Origin] =  " + pPlane );

				//Debug.DrawLine(transform.position, pPlane , Color.red);

		}
		else Debug.Log(" [!!] No intersection");

		///*
		///Project Shape wrt the Camera 
		short i = 0;
		foreach (var P in Wall) {

			//Debug.Log( " \t\t [" + i + "]");
		
			Vector3 imgPoint = Vector3.zero; //rayPosition;
			imgPoint.x += P.x;
			imgPoint.y += P.y;
			imgPoint.z += f;  //500.0f;	//focal distance

			rayDirection = imgPoint;
			/*
			Debug.Log( " \t >Plane point= \t " + planePoint );
			Debug.Log( " \t >Plane normal= \t " + planeNormal );

			Debug.Log( " \t >Ray origin= \t " + rayPosition );
			Debug.Log( " \t >Ray direction= \t " + rayDirection );
			*/
			float denominator = Vector3.Dot(rayDirection, planeNormal);

			if (denominator > 0.000001f)
			{
				//The distance to the plane
				float rayDistance = Vector3.Dot(planePoint - rayPosition, planeNormal) / denominator;

				//Where the ray intersects with a plane
				Vector3 p = rayPosition + rayDirection * rayDistance; //Hit wrt Camera

					//Debug.Log( " \t[!!] Hit at [wrt Camera]=  " + p );
				//Debug.Log( "\t\t Hit_angle=  " + Vector3.Angle( p - P1_ , planeNormal) );

				//Vector3 pPlane = Rt.transpose.MultiplyVector( p );	
				//Debug.Log( " \t[!!] Hit Plane at =  " + pPlane );

				Vector3 pPlane = Rt.transpose.MultiplyVector( p ) + this.transform.position; //Hit wrt Plane

				//Vector3 pPlane = Rt.transpose.MultiplyVector( p ) + this.transform.position; //Hit wrt Plane
				//pPlane = Rt.transpose.MultiplyVector( p ); //Hit wrt Plane
				//pPlane += transform.position;


					Debug.Log( " \t[!!C_"+i+"] Hit Plane at =  " + pPlane );

				if(DRAW_QUAD){
					if(DRAW_LINES)
						//Debug.DrawLine(transform.position, pPlane , Color.yellow);
						Debug.DrawLine(transform.position, pPlane , Color.magenta);

					line.enabled = true;
					line.SetColors(quadColor,quadColor);
					//line.SetPosition( i , pPlane );
					line.SetPosition( i , new Vector3(pPlane.x, pPlane.y, -10.0f ) );
				}
				else line.enabled = false;

			}
			//else Debug.Log("No intersection");

			i++;

			//break;
		}
		//*/


		///*
		///Project TRIANGLE Shape wrt the Camera
		/// 				
		if(DRAW_TRI){
		triLine.enabled = true;
		triLine.SetColors(triColor,triColor); 
		i = 0;
		foreach (var P in triShape) {

			//Debug.Log( " \t\t [" + i + "]");

			Vector3 imgPoint = Vector3.zero; //rayPosition;
			imgPoint.x += P.x;
			imgPoint.y += P.y;
			imgPoint.z += f;  //500.0f;	//focal distance

			rayDirection = imgPoint;
			/*
			Debug.Log( " \t >Plane point= \t " + planePoint );
			Debug.Log( " \t >Plane normal= \t " + planeNormal );

			Debug.Log( " \t >Ray origin= \t " + rayPosition );
			Debug.Log( " \t >Ray direction= \t " + rayDirection );
			*/
			float denominator = Vector3.Dot(rayDirection, planeNormal);

			if (denominator > 0.000001f)
			{
				//The distance to the plane
				float rayDistance = Vector3.Dot(planePoint - rayPosition, planeNormal) / denominator;

				//Where the ray intersects with a plane
				Vector3 p = rayPosition + rayDirection * rayDistance; //Hit wrt Camera

				//Debug.Log( " \t[!!] Hit at [wrt Camera]=  " + p );
				//Debug.Log( "\t\t Hit_angle=  " + Vector3.Angle( p - P1_ , planeNormal) );

				//Vector3 pPlane = Rt.transpose.MultiplyVector( p );	
				//Debug.Log( " \t[!!] Hit Plane at =  " + pPlane );

				Vector3 pPlane = Rt.transpose.MultiplyVector( p ) + this.transform.position; //Hit wrt Plane

				//Vector3 pPlane = Rt.transpose.MultiplyVector( p ) + this.transform.position; //Hit wrt Plane
				//pPlane = Rt.transpose.MultiplyVector( p ); //Hit wrt Plane
				//pPlane += transform.position;


				//Debug.Log( " \t[!!C_"+i+"] Hit Plane at =  " + pPlane );
				if(DRAW_LINES)
					Debug.DrawLine(transform.position, pPlane , triColor);


				//triLine.SetPosition( i , new Vector3(pPlane.x, pPlane.y, -10.0f ) );

					triLine.SetPosition( i , new Vector3(pPlane.x, pPlane.y, -10.0f ) );
				



			}
			//else Debug.Log("No intersection");

			i++;

			//break;
		}
		}
		else triLine.enabled = false;
		//*/


		//////////////////////////////////////////////////////////*
		///Project POLYGON Shape wrt the Camera
		if(DRAW_POLY){
			//Wall points wrt origin
			Vector3[] CROSS = new [] { 
				new Vector3(    0.0f,  250.0f, 0.0f), 
				new Vector3( -250.0f,    0.0f, 0.0f), 
				new Vector3(    0.0f, -250.0f, 0.0f), 
				new Vector3(  250.0f,    0.0f, 0.0f), 
			};


			//Limit size based on focal distance
			///Debug.DrawLine( this.transform.position, new Vector3(-250,250,0) , Color.green);
			Debug.Log("FocalDist= " + f);

			float MIN = Mathf.Infinity;

			for (int k = 0; k < 4; k++) {
				Debug.DrawLine( this.transform.position, Wall[k]*5 , Color.green);


				Vector3 P1 = -Rt.MultiplyVector( transform.position - Wall[k]*5 ) ;
				Debug.Log( "\t->P1["+k+"]= " + P1 );

				float u1 = (f * P1.x) / P1.z;
				float v1 = (f * P1.y) / P1.z;	
				Debug.Log(" 1-(u,v)= " + u1 +" , " + v1 );

				Vector3 P2 = -Rt.MultiplyVector( transform.position - Wall[ (k+1)%4 ]*5 ) ;
				Debug.Log( "\t->P2["+k+"]= " + P2 );

				float u2 = (f * P2.x) / P2.z;
				float v2 = (f * P2.y) / P2.z;	
				Debug.Log(" 2-(u,v)= " + u2 +" , " + v2 );

				//https://en.wikipedia.org/wiki/Distance_from_a_point_to_a_line
				//simplified since p=(x0,y0)=(0,0)
				float distToEdge = Mathf.Abs( u2*v1 - v2*u1) / Mathf.Sqrt( (v2-v1)*(v2-v1) + (u2-u1)*(u2-u1) );

				if( distToEdge < MIN) MIN = distToEdge;


				//*
				//Vector3 c_W1_ = Rt.transpose.MultiplyVector( W1_ ) + this.transform.position; //Hit wrt Plane 
				//Debug.Log( "\t->t_W["+k+"]= " + c_W1_ );
			}

							/*
							Debug.DrawLine( new Vector3(xmin,ymax,0), new Vector3(xmin,ymin,0), Color.red  );
							Debug.DrawLine( new Vector3(xmin,ymin,0), new Vector3(xmax,ymin,0), Color.red  );
							Debug.DrawLine( new Vector3(xmax,ymin,0), new Vector3(xmax,ymax,0), Color.red  );
							Debug.DrawLine( new Vector3(xmax,ymax,0), new Vector3(xmin,ymax,0), Color.red  );

							polyLine.positionCount = 4;
							polyLine.SetPosition( 0 , new Vector3(xmin,ymax, -1.0f ) );
							polyLine.SetPosition( 1 , new Vector3(xmin,ymin, -1.0f ) );
							polyLine.SetPosition( 2 , new Vector3(xmax,ymin, -1.0f ) );
							polyLine.SetPosition( 3 , new Vector3(xmax,ymax, -1.0f ) );
							*/


			float limit = MIN;
			Debug.Log( "LimitMin= " + MIN);

			polyLine.enabled = true;
			polyLine.positionCount = Mathf.Abs(polyVerts);
			polyLine.SetColors(polyColor,polyColor);
		 
		float angleStep = (Mathf.PI * 2.0f) / polyVerts;
		for (int j = 0; j < polyVerts; j++) {
			//Debug.Log( " \t\t [" + i + "]");

			//float rad = 100.0f;
			float angle = j * angleStep;
			
			//float Px = polyRadius * Mathf.Cos (angle);
			//float Py = polyRadius * Mathf.Sin (angle);

			float Px = limit * Mathf.Cos (angle);
			float Py = limit * Mathf.Sin (angle);

			Vector3 imgPoint = Vector3.zero; //rayPosition;
			imgPoint.x += Px;
			imgPoint.y += Py;
			imgPoint.z += f;  //500.0f;	//focal distance

			rayDirection = imgPoint;
			/*
			Debug.Log( " \t >Plane point= \t " + planePoint );
			Debug.Log( " \t >Plane normal= \t " + planeNormal );

			Debug.Log( " \t >Ray origin= \t " + rayPosition );
			Debug.Log( " \t >Ray direction= \t " + rayDirection );
			*/
			float denominator = Vector3.Dot(rayDirection, planeNormal);

			if (denominator > 0.000001f)
			{
				//The distance to the plane
				float rayDistance = Vector3.Dot(planePoint - rayPosition, planeNormal) / denominator;

				//Where the ray intersects with a plane
				Vector3 p = rayPosition + rayDirection * rayDistance; //Hit wrt Camera

				//Debug.Log( " \t[!!] Hit at [wrt Camera]=  " + p );
				//Debug.Log( "\t\t Hit_angle=  " + Vector3.Angle( p - P1_ , planeNormal) );

				//Vector3 pPlane = Rt.transpose.MultiplyVector( p );	
				//Debug.Log( " \t[!!] Hit Plane at =  " + pPlane );

				Vector3 pPlane = Rt.transpose.MultiplyVector( p ) + this.transform.position; //Hit wrt Plane

				//Vector3 pPlane = Rt.transpose.MultiplyVector( p ) + this.transform.position; //Hit wrt Plane
				//pPlane = Rt.transpose.MultiplyVector( p ); //Hit wrt Plane
				//pPlane += transform.position;


				//Debug.Log( " \t[!!POLY_"+j+"] Hit Plane at =  " + pPlane );
				if(DRAW_LINES)
					Debug.DrawLine(transform.position, pPlane , polyColor);


				
					polyLine.SetPosition( j , new Vector3(pPlane.x, pPlane.y, -1.0f ) );
				
			}
			//else Debug.Log("No intersection");


		}
		}
		else polyLine.enabled = false;
		//////////////////////////////////////*/
		/// 
		/// 















		/*
		//////////////////////////////////////////////////////////*
		///Project POLYGON Shape wrt the Camera
		if(DRAW_SHAPE){
			shapeLine.enabled = true;
			shapeLine.positionCount = shape.Length;
			shapeLine.SetColors(shapeColor,shapeColor);


			for (int j = 0; j < shape.Length; j++) {
				//Debug.Log( " \t\t [" + i + "]");

				Vector3 imgPoint = Vector3.zero; //rayPosition;
				imgPoint.x += shape[j].x;
				imgPoint.y += shape[j].y;
				imgPoint.z += f;  //500.0f;	//focal distance

				rayDirection = imgPoint;

				float denominator = Vector3.Dot(rayDirection, planeNormal);

				if (denominator > 0.000001f)
				{
					//The distance to the plane
					float rayDistance = Vector3.Dot(planePoint - rayPosition, planeNormal) / denominator;

					//Where the ray intersects with a plane
					Vector3 p = rayPosition + rayDirection * rayDistance; //Hit wrt Camera

					Vector3 pPlane = Rt.transpose.MultiplyVector( p ) + this.transform.position; //Hit wrt Plane

					//Debug.Log( " \t[!!POLY_"+j+"] Hit Plane at =  " + pPlane );
					if(DRAW_LINES)
						Debug.DrawLine(transform.position, pPlane , shapeColor);

					shapeLine.SetPosition( j , new Vector3(pPlane.x, pPlane.y, -10.0f ) );

				}
				//else Debug.Log("No intersection");

			}
		}
		else shapeLine.enabled = false;
		//////////////////////////////////////*/




		/*///
		Vector3 c = transform.position;
		Debug.Log( "C: " + c);

		Vector3 c_0 = c-Vector3.zero;
		//Vector3 z = c_0 / c_0.magnitude; 
		Vector3 z = c_0.normalized; 
		Debug.Log( "\t Z: " + z);
		//Debug.Log( "C_0: " + c_0.normalized);

		Vector3 x = Vector3.Cross(Vector3.up, z).normalized; 
		Debug.Log( "\t X: " + x);

		Vector3 y = Vector3.Cross(z, x).normalized; 
		Debug.Log( "	 Y: " + y);

		//2nd Method
		Matrix4x4 R = Matrix4x4.identity;
		R.SetRow( 0 , -new Vector4( x[0], x[1], x[2] ));
		R.SetRow( 1 , new Vector4( y[0], y[1], y[2] ));
		R.SetRow( 2 , -new Vector4( z[0], z[1], z[2] ));
		Debug.Log( "R= " + R );

		Vector3 t0 = R.MultiplyVector( -c );
		Debug.Log( "t0= " + t0 );

			Vector3 P1 = -R.MultiplyVector( c - new Vector3(-50.0f, 50.0f, 0.0f) );
			Debug.Log( "\t P1= " + P1 );

			Vector3 P2 = -R.MultiplyVector( c - new Vector3(-50.0f, -50.0f, 0.0f) );
			Debug.Log( "\t P2= " + P2 );

			Vector3 P3 = -R.MultiplyVector( c - new Vector3(50.0f, -50.0f, 0.0f) );
			Debug.Log( "\t P3= " + P3 );

			Vector3 P4 = -R.MultiplyVector( c - new Vector3(50.0f, 50.0f, 0.0f) );
			Debug.Log( "\t P4= " + P4 );


		//3rd Method a)
		Matrix4x4 R1 = Matrix4x4.LookAt(Vector3.zero, c, Vector3.up);
		Debug.Log( "R1= " + R1 );

		//3rd Method b)
		Matrix4x4 R2 = Matrix4x4.Inverse(R1);
		R2.m20 *= -1f;
		R2.m21 *= -1f;
		R2.m22 *= -1f;
		R2.m23 *= -1f;
		Debug.Log( "RR= " + R2 );


		//Vector3 t0 = R.MultiplyVector( -c );
		//Debug.Log( "t0= " + t0 );

		Vector3 t1 = R1.transpose.MultiplyVector( -c );
		Debug.Log( "t1= " + t1 );

		Vector3 t2 = R2.MultiplyVector( -c );
		Debug.Log( "t2= " + t2 );

		//*/


		//Translate upwards
		//Debug.Log( "UP: " + up[0] + "," + up[1] + "," + up[2] );
		//Debug.Log( "UP: " + up);
		//transform.position += Vector3.up * 100 * Time.deltaTime;
	}




}
