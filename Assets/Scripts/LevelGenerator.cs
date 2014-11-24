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

	private List<Vector2> m_points;
	private List<LineSegment> m_edges = null;
	private List<LineSegment> m_spanningTree;
	private List<LineSegment> m_delaunayTriangulation;
	List<Rectangle> rects;

	public GameObject prefab_teleporter;
	public GameObject prefab_wall;
	public int m_pointCount = 100;
	public float m_mapWidth = 100;
	public float m_mapHeight = 50;
	public float m_wallHeight=1;

	private GameGraph graphTele;

	public GameGraph getTeleportGraph(){
		return graphTele;
	}

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
		//Create fixed point set for demo
		m_points = new List<Vector2> ();
		m_points.Add (new Vector2(0.1f*m_mapWidth,0.2f*m_mapHeight));
		m_points.Add (new Vector2(0.3f*m_mapWidth,0.2f*m_mapHeight));
		m_points.Add (new Vector2(0.3f*m_mapWidth,0.5f*m_mapHeight));
		m_points.Add (new Vector2(0.5f*m_mapWidth,0.2f*m_mapHeight));
		m_points.Add (new Vector2(0.4f*m_mapWidth,0.4f*m_mapHeight));
		m_points.Add (new Vector2(0.4f*m_mapWidth,0.7f*m_mapHeight));
		m_points.Add (new Vector2(0.5f*m_mapWidth,0.9f*m_mapHeight));
		m_points.Add (new Vector2(0.8f*m_mapWidth,0.1f*m_mapHeight));		
		m_points.Add (new Vector2(0.7f*m_mapWidth,0.5f*m_mapHeight));
		m_points.Add (new Vector2(0.8f*m_mapWidth,0.3f*m_mapHeight));

		//Calculate voronoi tesselation
		Delaunay.Voronoi v = new Delaunay.Voronoi (m_points, null, new Rect (0, 0, m_mapWidth, m_mapHeight));
		m_edges = v.VoronoiDiagram ();

		//Create graphCells with nodes representing cells
		int id = 0;
		GameGraph graphCells = new GameGraph ();
		graphCells.addNode (new Node (m_points [0], Node.CellType.start, id++));
		for (int i=1; i<m_points.Count-1; i++) {
			graphCells.addNode(new Node(m_points[i], Node.CellType.ordinary, id++));
		}
		graphCells.addNode (new Node (m_points [m_points.Count - 1], Node.CellType.goal, id++));

		//Set adjacent cells in graphCells
		foreach(Node n in graphCells.Nodes ()) {
			List<Vector2> neighbors = v.NeighborSitesForSite (n.coords);
			foreach(Vector2 neighborCoords in neighbors){
				Node nNeighbor = graphCells.getNode(neighborCoords);
				n.addAdjacent(nNeighbor);
				nNeighbor.addAdjacent(n);
			}
		}

		//Create new graph with nodes, but blank adjacency matrix
		graphTele = new GameGraph ();
		foreach (Node n in graphCells.Nodes()) {
			Node newNode = new Node(n.coords,n.type,n.id);
			graphTele.addNode(newNode);
		}

		//Assing teleporters along shortest path from start to goal cell
		List<Node> path = graphCells.BFS (graphCells.getNode(m_points [0]), graphCells.getNode(m_points [m_points.Count - 1]));
		for (int i=1; i<path.Count; i++) {
			Vector2 coordsSite1 = path [i - 1].coords;
			Vector2 coordsSite2 = path [i].coords;

			//Cells that are connected by a teleporter will be represented as adjacent in graphTele
			Node n1 = graphTele.getNode(coordsSite1);
			Node n2 = graphTele.getNode(coordsSite2);
			n1.addAdjacent(n2);
			n2.addAdjacent(n1);

			//Instantiate teleporter at wall
			foreach (LineSegment line in m_edges) {
				if(this.FasterLineSegmentIntersection(coordsSite1,coordsSite2,(Vector2)line.p0,(Vector2)line.p1)){
					createTeleporter((Vector2)line.p0,(Vector2)line.p1);
					break;
				}
			}
		}
		/*
		foreach(Node n in graphTele.Nodes()){
			Debug.Log(n.id.ToString() + ": \n");
			for(int j=0; j<n.getAdjacent().Count; j++){
				Debug.Log(n.getAdjacent()[j].id.ToString() + ", ");
			}
		}*/
		
		//Fixed: assing teleporters along remaining cells
		List<int> remaining = new List<int> ();
		remaining.Add (2);
		remaining.Add (4);
		remaining.Add (8);
		remaining.Add (5);
		remaining.Add (6);
		remaining.Add (9);
		for(int i=1; i<remaining.Count; i++){
		//Cells that are connected by a teleporter will be represented as adjacent in graphTele
			Node n1 = graphTele.getNode(remaining[i-1]);
			Node n2 = graphTele.getNode(remaining[i]);
			n1.addAdjacent(n2);
			n2.addAdjacent(n1);
			
			//Instantiate teleporter at wall
			foreach (LineSegment line in m_edges) {
				if(this.FasterLineSegmentIntersection(n1.coords,n2.coords,(Vector2)line.p0,(Vector2)line.p1)){
					createTeleporter((Vector2)line.p0,(Vector2)line.p1);
					break;
				}
			}
		}

		//Assign more teleporters at random adjacent cells
		/*
		 * TODO: Bug - overwrites existing teleporters
		int m_additionalTeleporters = 5;
		int j=0;
		while (j<Mathf.Min (m_additionalTeleporters,graphCells.Nodes().Count)) {
			int nodeIndex = Random.Range(0,graphCells.Nodes().Count-1);
			Node n = graphCells.Nodes ()[nodeIndex];
			int adjacentIndex = Random.Range (0,n.getAdjacent().Count-1);
			Node n1 = n.getAdjacent()[adjacentIndex];
			if(!graphTele.getNode(n.id).getAdjacent().Contains(n1) && !graphTele.getNode(n1.id).getAdjacent().Contains(n)){
				graphTele.getNode(n.id).addAdjacent(n1);
				graphTele.getNode(n1.id).addAdjacent(n);

				foreach (LineSegment line in m_edges) {
					if(this.FasterLineSegmentIntersection(n.coords,n1.coords,(Vector2)line.p0,(Vector2)line.p1)){
						createTeleporter((Vector2)line.p0,(Vector2)line.p1);
						j++;
						break;
					}
				}
			}
		}*/

		//Create cell walls from edges
		createWalls ();

		//Create boundary walls
		GameObject topWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
		topWall.transform.localScale = new Vector3(m_mapWidth,1.0f*m_wallHeight,0.1f);
		topWall.transform.position = new Vector3(0.0f,0.5f,m_mapHeight/2);

		GameObject bottomWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
		bottomWall.transform.localScale = new Vector3(m_mapWidth,1.0f*m_wallHeight,0.1f);
		bottomWall.transform.position = new Vector3(0.0f,0.5f,-m_mapHeight/2);

		GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
		leftWall.transform.localScale = new Vector3(0.1f,1.0f*m_wallHeight,m_mapHeight);
		leftWall.transform.position = new Vector3(-m_mapWidth/2,0.5f,0.0f);

		GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
		rightWall.transform.localScale = new Vector3(0.1f,1.0f*m_wallHeight,m_mapHeight);
		rightWall.transform.position = new Vector3(m_mapWidth/2,0.5f,0.0f);

		//Create floor
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
		plane.transform.localScale = new Vector3 (m_mapWidth/10,1.0f*m_wallHeight,m_mapHeight/5);
		plane.transform.position = new Vector3 (0.0f, 0.0f, 0.0f);


		//m_spanningTree = v.SpanningTree (KruskalType.MINIMUM);
		//m_delaunayTriangulation = v.DelaunayTriangulation ();
	}

	//Quickly compute if two lines intersect. First two arguments = first line, last two arguments = second line
	private bool FasterLineSegmentIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4) {
		
		Vector2 a = p2 - p1;
		Vector2 b = p3 - p4;
		Vector2 c = p1 - p3;
		
		float alphaNumerator = b.y*c.x - b.x*c.y;
		float alphaDenominator = a.y*b.x - a.x*b.y;
		float betaNumerator  = a.x*c.y - a.y*c.x;
		float betaDenominator  = alphaDenominator; /*2013/07/05, fix by Deniz*/
		
		bool doIntersect = true;
		
		if (alphaDenominator == 0 || betaDenominator == 0) {
			doIntersect = false;
		} else {
			
			if (alphaDenominator > 0) {
				if (alphaNumerator < 0 || alphaNumerator > alphaDenominator) {
					doIntersect = false;
				}
			} else if (alphaNumerator > 0 || alphaNumerator < alphaDenominator) {
				doIntersect = false;
			}
			
			if (doIntersect && betaDenominator > 0) {
				if (betaNumerator < 0 || betaNumerator > betaDenominator) {
					doIntersect = false;
				}
			} else if (betaNumerator > 0 || betaNumerator < betaDenominator) {
				doIntersect = false;
			}
		}
		
		return doIntersect;
	}

	// Create teleporter from prefab on specified line
	private void createTeleporter(Vector2 p0, Vector2 p1){
		
		Vector2 midpoint = (p0+p1)*0.5f;
		float length = Vector2.Distance(p0,p1)*0.5f;
		float angle = -Mathf.Atan2(p0.y - p1.y, p0.x - p1.x) * (180 / Mathf.PI) + 90;
		GameObject teleporter = Instantiate(prefab_teleporter,new Vector3(midpoint.x-m_mapWidth/2,0.5f,midpoint.y-m_mapHeight/2),Quaternion.Euler(new Vector3(0.0f,angle,0.0f))) as GameObject; 
		teleporter.transform.localScale = new Vector3(0.5f,0.8f*m_wallHeight,length);

	}

	// Create walls from prefab between cells 
	private void createWalls(){
		foreach (LineSegment e in m_edges) {
			Vector2 p0=(Vector2)e.p0;
			Vector2 p1=(Vector2)e.p1;

			Vector2 midpoint = (p0+p1)*0.5f;
			float length = Vector2.Distance(p0,p1);
			float angle = -Mathf.Atan2(p0.y - p1.y, p0.x - p1.x) * (180 / Mathf.PI) + 90;
			GameObject wall = Instantiate(prefab_wall,new Vector3(midpoint.x-m_mapWidth/2,0.5f,midpoint.y-m_mapHeight/2),Quaternion.Euler(new Vector3(0.0f,angle,0.0f))) as GameObject;
			wall.transform.localScale = new Vector3(0.1f,1.0f*m_wallHeight,length);
		}
	}

	// Create a random seed of points in the euclidean space of the rectangles
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

	// Create a number of rectangles that can be traversed horizontally
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
	// Create a number of rectangles that can be traversed horizontally
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

	// Drawing method for debugging
	private void OnDrawGizmos ()
	{

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
		*/
		//Draw boundaries
		Gizmos.color = Color.yellow;
		Gizmos.DrawLine (new Vector2 (0, 0), new Vector2 (0, m_mapHeight));
		Gizmos.DrawLine (new Vector2 (0, 0), new Vector2 (m_mapWidth, 0));
		Gizmos.DrawLine (new Vector2 (m_mapWidth, 0), new Vector2 (m_mapWidth, m_mapHeight));
		Gizmos.DrawLine (new Vector2 (0, m_mapHeight), new Vector2 (m_mapWidth, m_mapHeight));
	}
}