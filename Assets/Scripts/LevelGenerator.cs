using UnityEngine;
using System.Collections.Generic;
using Delaunay;
using Delaunay.Geo;

public class Rectangle{
	public float width;
	public float height;
	public Vector2 upperLeft;
	public Vector2 upperRight;
	public Vector2 lowerRight;
	public Vector2 lowerLeft;
	public Vector2 middle;
	
	public Rectangle(Vector2 middle, float width, float height){
		this.middle = middle;
		this.width = width;
		this.height = height;
		this.upperLeft = new Vector2 (middle.x - width / 2, middle.y - height / 2);
		this.upperRight = new Vector2 (middle.x + width / 2, middle.y - height / 2);
		this.lowerRight = new Vector2 (middle.x + width / 2, middle.y + height / 2);
		this.lowerLeft = new Vector2 (middle.x - width / 2, middle.y + height / 2);
	}

	public bool isWithin(Vector2 point){
		if(point.x >= upperLeft.x && point.x<=upperRight.x){
			if(point.y >= upperLeft.y && point.y<=lowerLeft.y){
				return true;
			}
		}
		return false;
	}
}

public class LevelGenerator : MonoBehaviour
{
	[SerializeField]
	private int	m_pointCount = 100;
	private List<Vector2> m_points;
	private float m_mapWidth = 100;
	private float m_mapHeight = 50;
	private List<LineSegment> m_edges = null;
	private List<LineSegment> m_spanningTree;
	private List<LineSegment> m_delaunayTriangulation;
	List<Rectangle> rects;

	void Awake ()
	{
		StaticDemo ();
	}

	void Update ()
	{
		/*
		if (Input.anyKeyDown) {
			Demo ();
		}*/
	}

	private void StaticDemo(){
		//Fixed point set for demo
		m_points = new List<Vector2> ();
		m_points.Add (new Vector2(0.1f*m_mapWidth,0.2f*m_mapHeight));
		m_points.Add (new Vector2(0.3f*m_mapWidth,0.2f*m_mapHeight));
		m_points.Add (new Vector2(0.3f*m_mapWidth,0.5f*m_mapHeight));
		m_points.Add (new Vector2(0.5f*m_mapWidth,0.2f*m_mapHeight));
		m_points.Add (new Vector2(0.8f*m_mapWidth,0.3f*m_mapHeight));
		m_points.Add (new Vector2(0.4f*m_mapWidth,0.4f*m_mapHeight));
		m_points.Add (new Vector2(0.4f*m_mapWidth,0.7f*m_mapHeight));
		m_points.Add (new Vector2(0.5f*m_mapWidth,0.9f*m_mapHeight));
		m_points.Add (new Vector2(0.8f*m_mapWidth,0.1f*m_mapHeight));		
		m_points.Add (new Vector2(0.7f*m_mapWidth,0.5f*m_mapHeight));

		Delaunay.Voronoi v = new Delaunay.Voronoi (m_points, null, new Rect (0, 0, m_mapWidth, m_mapHeight));
		List<Vector2> sites = v.SiteCoords ();

		m_edges = v.VoronoiDiagram ();
		
		createWalls ();

		GameObject topWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
		topWall.transform.localScale = new Vector3(m_mapWidth,1.0f,0.1f);
		topWall.transform.position = new Vector3(0.0f,0.5f,m_mapHeight/2);

		GameObject bottomWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
		bottomWall.transform.localScale = new Vector3(m_mapWidth,1.0f,0.1f);
		bottomWall.transform.position = new Vector3(0.0f,0.5f,-m_mapHeight/2);

		GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
		leftWall.transform.localScale = new Vector3(0.1f,1.0f,m_mapHeight);
		leftWall.transform.position = new Vector3(-m_mapWidth/2,0.5f,0.0f);

		GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
		rightWall.transform.localScale = new Vector3(0.1f,1.0f,m_mapHeight);
		rightWall.transform.position = new Vector3(m_mapWidth/2,0.5f,0.0f);


		for (int i=0; i<m_points.Count; i++) {
			GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			dot.transform.position = new Vector3(m_points[i].x-m_mapWidth/2,0.0f,m_points[i].y-m_mapHeight/2);
			dot.transform.parent = this.transform;
		}

		//transform.localRotation = Quaternion.Euler (-90.0f, 0.0f, 0.0f);
		//transform.position = new Vector3 (-45.3f,1.5f,31.0f);
		
		GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
		plane.transform.localScale = new Vector3 (10.0f,1.0f,5.0f);

	}

	private void Demo (){
		rects = createBoundaries3 ();
		m_points = createRandomSeed ();

		Delaunay.Voronoi v = new Delaunay.Voronoi (m_points, null, new Rect (0, 0, m_mapWidth, m_mapHeight));
		m_edges = v.VoronoiDiagram ();

		createWalls ();

		transform.localRotation = Quaternion.Euler (90.0f, 0.0f, 0.0f);
		transform.position = new Vector3 (-m_mapWidth/2,0.5f,-m_mapHeight/2);

		GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
		plane.transform.localScale = new Vector3 (m_mapWidth/10,1.0f,m_mapHeight/5);
		plane.transform.position = new Vector3 (0.0f, 0.0f, 0.0f);


		/*
		List<uint> colors = new List<uint> ();

		//Calculate voronoi diagram and extract edges
		Delaunay.Voronoi v = new Delaunay.Voronoi (m_points, colors, new Rect (0, 0, m_mapWidth, m_mapHeight));
		m_edges = v.VoronoiDiagram ();

		List<LineSegment> edges = v.VoronoiDiagram ();


		/*
		List<Edge> edges = v.Edges ();

		List<Site> corruptedSites = new List<Site>();
		for (int i = 0; i< edges.Count; i++) {
			Vector2 left = (Vector2)edges[i].VoronoiEdge().p0;
			Vector2 right = (Vector2)edges[i].VoronoiEdge().p1;
			bool leftIn = false;
			bool rightIn = false;
			foreach(Rectangle r in rects){
				if(r.isWithin(left)){
					leftIn = true;
					break;
				}
			}
			if(!leftIn){
				corruptedSites.Add(edges[i].leftSite);
				corruptedSites.Add(edges[i].rightSite);

				continue;
			}
			foreach(Rectangle r in rects){
				if(r.isWithin(right)){
					rightIn = true;
					break;
				}
			}
			if(!rightIn){
				corruptedSites.Add(edges[i].leftSite);
				corruptedSites.Add(edges[i].rightSite);
			}
		}

		for (int i=0; i<edges.Count; i++) {
			if(corruptedSites.Contains(edges[i].leftSite)|| corruptedSites.Contains(edges[i].rightSite)){
				edges.RemoveAt(i);
				i--;
			}
		}*/

		
		//Ziel: Entferne alle edges, die aus Quadraten herausragen
		//Problem: Es können lose Edges einer Site übrig bleiben
		//Lösung: Entferne in einem zweiten Durchlauf alle Edges, denen mindestens 1 Nachbar fehlt
		//->Alternativ: Entferne alle Sites, fpr die mindestens eine Edge herausragt. Sammle dann alle Edges über die übrigen Sites. 
		//->Abschließend: Prüfe, ob es einen Weg vom äußerst linken- zum äußerst rechten Punkt gibt. Eine Verknüpfung existiert nur, wenn zwei Sites anliegen.

		//m_spanningTree = v.SpanningTree (KruskalType.MINIMUM);
		//m_delaunayTriangulation = v.DelaunayTriangulation ();
	}

	private void createWalls(){
		//Create walls from cubes
		foreach (LineSegment e in m_edges) {
			Vector2 p0=(Vector2)e.p0;
			Vector2 p1=(Vector2)e.p1;

			/*
			GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			dot.transform.position = new Vector3(p0.x-m_mapWidth/2,0.0f,p0.y-m_mapHeight/2);
			dot.transform.parent = this.transform;

			dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			dot.transform.position = new Vector3(p1.x-m_mapWidth/2,0.0f,p1.y-m_mapHeight/2);
			dot.transform.parent = this.transform;*/

			Vector2 midpoint = (p0+p1)*0.5f;
			float length = Vector2.Distance(p0,p1);
			float angle = -Mathf.Atan2(p0.y - p1.y, p0.x - p1.x) * (180 / Mathf.PI) + 90;
			GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			cube.transform.localScale = new Vector3(0.1f,1.0f,length);
			cube.transform.position = new Vector3(midpoint.x-m_mapWidth/2,0.5f,midpoint.y-m_mapHeight/2);
			cube.transform.localRotation = Quaternion.Euler(new Vector3(0.0f,angle,0.0f));
			cube.transform.parent = this.transform;
		}
	}

	private List<Vector2> createRandomSeed(){
		//Create vector of random seed points in euclidean space
		List<Vector2> points = new List<Vector2> (); 
		while (points.Count < m_pointCount) {
			Vector2 point = new Vector2 (Random.Range (0, m_mapWidth), Random.Range (0, m_mapHeight));
			foreach (Rectangle r in rects) {
				if (r.isWithin (point)) {
					points.Add (point);
					break;
				}
			}
		}
		return points;
	}
	
	private List<Rectangle> createBoundaries(){

		float minEdgeX = 5.0f;
		float maxEdgeX = 20.0f;
		float minEdgeY = 5.0f;
		float maxEdgeY = 20.0f;
		float minOverlap = 1.0f;

		rects = new List<Rectangle> ();
		Rectangle last = null;

		//First Rectangle: Position on left edge of the parent boundary square, choose random y
		float x = UnityEngine.Random.Range(minEdgeX,maxEdgeX);
		float y = UnityEngine.Random.Range(minEdgeY,maxEdgeY);
		rects.Add (new Rectangle (new Vector2(0, Random.Range (0, m_mapWidth-maxEdgeY)), x, y));

		//Following rectangles must overlap with preceding one
		last = rects[rects.Count-1];
		while(last.upperRight.x < m_mapWidth && last.upperRight.y>0 && last.lowerRight.y<m_mapHeight){

			x = UnityEngine.Random.Range(minEdgeX,maxEdgeX);
			y = UnityEngine.Random.Range(minEdgeY,maxEdgeY);
			float pX = Random.Range (last.lowerRight.x-x, last.lowerRight.x-minOverlap);
			if(last.upperRight.y > m_mapHeight/2){
				//Align at lower left, i.e. overlaps at top
				float pY = Random.Range (last.upperRight.y+minOverlap, last.lowerRight.y)-y;
				rects.Add (new Rectangle(new Vector2(pX,pY),x,y));
			}else{
				//Align at upper left, i.e. overlaps at bottom
				float pY = Random.Range (last.upperRight.y+minOverlap, last.lowerRight.y-y-minOverlap);
				rects.Add (new Rectangle(new Vector2(pX,pY),x,y));
			}
			last = rects[rects.Count-1];
		};
		rects.RemoveAt (rects.Count - 1);
		return rects;
	}

	private List<Rectangle> createBoundaries3(){

		List<Rectangle> rs = new List<Rectangle> ();
		int nRectangles = 10;
		float compartment = m_mapWidth / nRectangles;
		float minHeight = 20;
		float minOverlap = 20;
		float width = m_mapWidth / nRectangles;
		float x = compartment / 2;

		float y = m_mapHeight / 2;
		float height = Random.Range (m_mapHeight/2,m_mapHeight);
		rs.Add (new Rectangle (new Vector2 (x,y), width, height));

		for (int i=1; i<nRectangles; i++) {
			x = compartment*i + compartment / 2;
			height = Random.Range (minHeight,m_mapHeight);

			float lowerBoundary = minHeight = rs[i-1].middle.y-(rs[i-1].height/2-minOverlap)-height/2;
			float upperBoundary = rs[i-1].middle.y+(rs[i-1].height/2-minOverlap)+height/2;

			y = Random.Range (Mathf.Max(height/2,lowerBoundary), Mathf.Min(m_mapHeight-height/2,upperBoundary)); 
			rs.Add (new Rectangle (new Vector2 (x,y), width, height));
		}

		return rs;
	}

	private void OnDrawGizmos ()
	{
		//Draw rectangles
		Gizmos.color = Color.white;
		if(rects != null){
			foreach (Rectangle r in rects) {
				Gizmos.DrawLine ((Vector3)r.upperLeft, (Vector3)r.upperRight);
				Gizmos.DrawLine ((Vector3)r.upperRight, (Vector3)r.lowerRight);
				Gizmos.DrawLine ((Vector3)r.lowerRight, (Vector3)r.lowerLeft);
				Gizmos.DrawLine ((Vector3)r.lowerLeft, (Vector3)r.upperLeft);
			}
		}

		//Draw seed points
		Gizmos.color = Color.red;
		if (m_points != null){
			for (int i = 0; i < m_points.Count; i++) {
				Gizmos.DrawSphere (m_points [i], 0.2f);
			}
		}

		//Draw voronoi edges
		if (m_edges != null) {
			Gizmos.color = Color.white;


			for (int i = 0; i< m_edges.Count; i++) {
				Vector2 left = (Vector2)m_edges [i].p0;
				Vector2 right = (Vector2)m_edges [i].p1;
				Gizmos.DrawLine ((Vector3)left, (Vector3)right);
			}
		}
	


		/*
		Gizmos.color = Color.magenta;
		if (m_delaunayTriangulation != null) {
			for (int i = 0; i< m_delaunayTriangulation.Count; i++) {
				Vector2 left = (Vector2)m_delaunayTriangulation [i].p0;
				Vector2 right = (Vector2)m_delaunayTriangulation [i].p1;
				Gizmos.DrawLine ((Vector3)left, (Vector3)right);
			}
		}

		if (m_spanningTree != null) {
			Gizmos.color = Color.green;
			for (int i = 0; i< m_spanningTree.Count; i++) {
				LineSegment seg = m_spanningTree [i];				
				Vector2 left = (Vector2)seg.p0;
				Vector2 right = (Vector2)seg.p1;
				Gizmos.DrawLine ((Vector3)left, (Vector3)right);
			}
		}*/

		//Draw boundaries
		Gizmos.color = Color.yellow;
		Gizmos.DrawLine (new Vector2 (0, 0), new Vector2 (0, m_mapHeight));
		Gizmos.DrawLine (new Vector2 (0, 0), new Vector2 (m_mapWidth, 0));
		Gizmos.DrawLine (new Vector2 (m_mapWidth, 0), new Vector2 (m_mapWidth, m_mapHeight));
		Gizmos.DrawLine (new Vector2 (0, m_mapHeight), new Vector2 (m_mapWidth, m_mapHeight));
	}
}