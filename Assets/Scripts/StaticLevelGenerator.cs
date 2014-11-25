using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Delaunay;
using Delaunay.Geo; 

public class StaticLevelGenerator : MonoBehaviour
{
	[SerializeField]
	
	private List<Vector2> m_points;
	private List<LineSegment> m_edges = null;
	private List<LineSegment> m_spanningTree;
	private List<LineSegment> m_delaunayTriangulation;
	List<Rectangle> rects;
	
	public GameObject prefab_teleporter;
	public GameObject prefab_wall;
	public GameObject prefab_floor;
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

		GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		sphere.transform.position = new Vector3(m_points [0].x-m_mapWidth/2,0.5f,m_points [0].y-m_mapHeight/2);

		sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		sphere.transform.position = new Vector3(m_points [m_points.Count - 1].x-m_mapWidth/2,0.5f,m_points [m_points.Count - 1].y-m_mapHeight/2);
		
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
			
			/*
			foreach (LineSegment line in m_edges) {
				if(this.FasterLineSegmentIntersection(coordsSite1,coordsSite2,(Vector2)line.p0,(Vector2)line.p1)){
					createTeleporter((Vector2)line.p0,(Vector2)line.p1);
					break;
				}
			}*/

			//Instantiate teleporter at wall
			List<Vector2> commonPoints = commonLine(n1, n2, v);
			if(commonPoints.Count==2){
				createTeleporter((Vector2)commonPoints[0],(Vector2)commonPoints[1]);
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
		remaining.Add (1);
		remaining.Add (2);
		remaining.Add (4);
		remaining.Add (8);
		remaining.Add (5);
		remaining.Add (6);
		remaining.Add (9);
		remaining.Add (8);
		for(int i=1; i<remaining.Count; i++){
			//Cells that are connected by a teleporter will be represented as adjacent in graphTele
			Node n1 = graphTele.getNode(remaining[i-1]);
			Node n2 = graphTele.getNode(remaining[i]);
			n1.addAdjacent(n2);
			n2.addAdjacent(n1);

			//Instantiate teleporter at wall
			List<Vector2> commonPoints = commonLine(n1, n2, v);
			if(commonPoints.Count==2){
				createTeleporter((Vector2)commonPoints[0],(Vector2)commonPoints[1]);
			}
		}

		//Create cell walls from edges
		createWalls ();
		
		//Create boundary walls
		GameObject topWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
		topWall.transform.localScale = new Vector3(m_mapWidth,1.0f*m_wallHeight,0.1f);
		topWall.transform.position = new Vector3(0.0f,0.5f*m_wallHeight,m_mapHeight/2);
		topWall.transform.parent = transform;
		
		GameObject bottomWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
		bottomWall.transform.localScale = new Vector3(m_mapWidth,1.0f*m_wallHeight,0.1f);
		bottomWall.transform.position = new Vector3(0.0f,0.5f*m_wallHeight,-m_mapHeight/2);
		bottomWall.transform.parent = transform;

		GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
		leftWall.transform.localScale = new Vector3(0.1f,1.0f*m_wallHeight,m_mapHeight);
		leftWall.transform.position = new Vector3(-m_mapWidth/2,0.5f*m_wallHeight,0.0f);
		leftWall.transform.parent = transform;
		
		GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
		rightWall.transform.localScale = new Vector3(0.1f,1.0f*m_wallHeight,m_mapHeight);
		rightWall.transform.position = new Vector3(m_mapWidth/2,0.5f*m_wallHeight,0.0f);
		rightWall.transform.parent = transform;
		
		//Create floor
		GameObject floor = Instantiate(prefab_floor) as GameObject; 
		floor.transform.localScale = new Vector3 (10.0f,1.0f,5.0f);
		floor.transform.position = new Vector3 (0.0f, 0.0f, 0.0f);
		floor.transform.parent = transform;
		
	}

	private List<Vector2> commonLine(Node site1, Node site2, Delaunay.Voronoi v){
		List<Vector2> r1 = v.Region(site1.coords);
		List<Vector2> r2 = v.Region(site2.coords);
		List<Vector2> commonPoints = new List<Vector2>();
		float epsilon = 0.01f;
		foreach(Vector2 p1 in r1){
			foreach(Vector2 p2 in r2){
				if(Vector2.Distance(p1,p2)<epsilon){
					commonPoints.Add (p1);
					if(commonPoints.Count==2){
						return commonPoints;
					}
				}
			}
		}
		return commonPoints;
	}

	// Create teleporter from prefab on specified line
	private void createTeleporter(Vector2 p0, Vector2 p1){
		
		Vector2 midpoint = (p0+p1)*0.5f;
		float length = Vector2.Distance(p0,p1)*0.5f;
		float angle = -Mathf.Atan2(p0.y - p1.y, p0.x - p1.x) * (180 / Mathf.PI) + 90;
		GameObject teleporter = Instantiate(prefab_teleporter,new Vector3(midpoint.x-m_mapWidth/2,0.5f*m_wallHeight,midpoint.y-m_mapHeight/2),Quaternion.Euler(new Vector3(0.0f,angle,0.0f))) as GameObject; 
		teleporter.transform.localScale = new Vector3(0.5f,0.8f*m_wallHeight,length);
		teleporter.transform.parent = transform;

	}
	
	// Create walls from prefab between cells 
	private void createWalls(){
		foreach (LineSegment e in m_edges) {
			Vector2 p0=(Vector2)e.p0;
			Vector2 p1=(Vector2)e.p1;
			
			Vector2 midpoint = (p0+p1)*0.5f;
			float length = Vector2.Distance(p0,p1);
			float angle = -Mathf.Atan2(p0.y - p1.y, p0.x - p1.x) * (180 / Mathf.PI) + 90;
			GameObject wall = Instantiate(prefab_wall,new Vector3(midpoint.x-m_mapWidth/2,0.5f*m_wallHeight,midpoint.y-m_mapHeight/2),Quaternion.Euler(new Vector3(0.0f,angle,0.0f))) as GameObject;
			wall.transform.localScale = new Vector3(0.1f,1.0f*m_wallHeight,length);
			wall.transform.parent = transform;

		}
	}

}