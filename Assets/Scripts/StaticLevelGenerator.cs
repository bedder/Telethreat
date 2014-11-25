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

    private List<GameObject> m_enemies;
	
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
        StaticDemo();

        m_enemies = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
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
		/*
		sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		sphere.transform.position = new Vector3(m_points [m_points.Count - 1].x-m_mapWidth/2,0.5f,m_points [m_points.Count - 1].y-m_mapHeight/2);
		*/

		//Set adjacent cells in graphCells
		foreach(Node n in graphCells.Nodes ()) {
			List<Vector2> neighbors = v.NeighborSitesForSite (n.coords);
			foreach(Vector2 neighborCoords in neighbors){
				Node nNeighbor = graphCells.getNode(neighborCoords);
				List<Vector2> edge = commonLine(n,nNeighbor,v);
				n.addAdjacent(nNeighbor, edge[0], edge[1]);
				nNeighbor.addAdjacent(n, edge[0], edge[1]);
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

			List<Vector2> commonEdge = graphCells.getNode(n1.id).getEdgeToAdjacent(n2);
			n1.addAdjacent(n2,commonEdge[0],commonEdge[1]);
			n2.addAdjacent(n1,commonEdge[0],commonEdge[1]);
			
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
		remaining.Add (8);
		remaining.Add (9);
		for(int i=1; i<remaining.Count; i++){
			//Cells that are connected by a teleporter will be represented as adjacent in graphTele
			Node n1 = graphTele.getNode(remaining[i-1]);
			Node n2 = graphTele.getNode(remaining[i]);

			List<Vector2> commonEdge = graphCells.getNode(n1.id).getEdgeToAdjacent(n2);
			n1.addAdjacent(n2,commonEdge[0],commonEdge[1]);
			n2.addAdjacent(n1,commonEdge[0],commonEdge[1]);

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

		getTeleportAreas (graphCells.getNode(m_points[0]), v, graphCells, graphTele);


		transform.localPosition = new Vector3 (0.0f, 0.0f, 0.0f);
		
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

    private void spawnMonsters(GameGraph graphCells)
    {
        foreach (Node node in graphCells.Nodes())
        {
            GameObject enemy = getRandomEnemy();
            Instantiate(enemy, new Vector3(node.coords.x, 0, node.coords.y), new Quaternion());
        }
    }

    private GameObject getRandomEnemy()
    {
        return m_enemies[(int)(Random.value % m_enemies.Count)];
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

	private Dictionary<List<Vector2>,Node> getTeleportAreas(Node n, Delaunay.Voronoi v, GameGraph graphCells, GameGraph graphTele){
		Vector2 middle = n.coords;
		List<List<Vector2>> ordinaryTris = new List<List<Vector2>> ();
		Dictionary<List<Vector2>,Node> subdivision = new Dictionary<List<Vector2>,Node> ();

		List<Node> adjacentCells = graphCells.getNode (middle).getAdjacent ();
		foreach(Node adj in adjacentCells){
			List<Vector2> edge = graphTele.getNode(middle).getEdgeToAdjacent(adj);
			if(edge !=null){
				edge.Add(middle);
				subdivision.Add(edge, adj);
			}else{
				edge = graphCells.getNode(middle).getEdgeToAdjacent(adj);
				edge.Add (middle);
				ordinaryTris.Add (edge);
			}
		}

		adjacentCells = graphTele.getNode (middle).getAdjacent ();
		foreach(List<Vector2> tri in ordinaryTris){
			float minDistance = float.MaxValue;
			Node targetCell = null;
			foreach (Node adj in adjacentCells) {
				List<Vector2> edge = graphTele.getNode(middle).getEdgeToAdjacent(adj);
				Vector2 edgeMiddle = (edge[0] + edge[1])/2.0f;
				Vector2 triMiddle = (tri[0]+tri[1]+tri[2])/3.0f;
				float distance = Vector2.Distance(edgeMiddle,triMiddle);
				if(distance<minDistance){
					minDistance=distance;
					targetCell = adj;
				}
			}
			subdivision.Add(tri, targetCell);
		}
		List<Vector2> t = subdivision.Keys.ToList ()[0];

		/*
		Mesh msh = new Mesh();
		msh.vertices = new Vector3[]{new Vector3(t[0].x,0.0f,t[0].y),new Vector3(t[1].x,0.0f,t[1].y),new Vector3(t[2].x,0.0f,t[2].y)};
		msh.triangles = new int[]{0,1,2};
		msh.RecalculateNormals();
		msh.RecalculateBounds();
		
		// Set up game object with mesh;
		gameObject.AddComponent(typeof(MeshRenderer));
		//MeshFilter filter = gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
		//filter.mesh = msh;
		gameObject.AddComponent<MeshFilter>().mesh=msh;
*/
		Mesh mesh = new Mesh ();
		if(!transform.GetComponent<MeshFilter> () || !transform.GetComponent<MeshRenderer> () ) //If you will havent got any meshrenderer or filter
		{
			transform.gameObject.AddComponent<MeshFilter>();
			transform.gameObject.AddComponent<MeshRenderer>();
		}

		Vector3 p0 = new Vector3(0,0,0);
		Vector3 p1 = new Vector3(1,0,0);
		Vector3 p2 = new Vector3(0.5f,0,Mathf.Sqrt(0.75f));
		Vector3 p3 = new Vector3(0.5f,Mathf.Sqrt(0.75f),Mathf.Sqrt(0.75f)/3);

		mesh.Clear();
		mesh.vertices = new Vector3[]{new Vector3(t[0].x,0.0f,t[0].y),new Vector3(t[1].x,0.0f,t[1].y),new Vector3(t[2].x,0.0f,t[2].y)};
		mesh.triangles = new int[]{
			2,1,0
		};
		
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		mesh.Optimize();
		
		GameObject triangle = new GameObject ();
		triangle.name = "Triangle";
		triangle.AddComponent (typeof(MeshFilter));
		triangle.AddComponent (typeof(MeshRenderer));
		triangle.GetComponent<MeshFilter> ().mesh = mesh;

		Material myNewMaterial = new Material(Shader.Find("Transparent/Diffuse"));
		myNewMaterial.SetColor ("_Color", new Color (0.0f, 0.0f, 1.0f, 0.7f)); //Semi-transparent blue
		triangle.renderer.material = myNewMaterial;
		//triangle.transform.localRotation = Quaternion.Euler(180,0.0f,0.0f);

		Vector2 triMid=	(t [0] + t [1] + t [2]) / 3.0f;

		gameObject.transform.localPosition=new Vector3(triMid.x, 0.0f, triMid.y);

		triangle.transform.parent = transform;

		
		/*
		Vector2 triMid = (t[0]+t[1]+t[2])/(1/3.0f);
		gameObject.transform.position=new Vector3(triMid.x, 0.0f, triMid.y);
		gameObject.transform.parent = transform;*/
		
		return subdivision;
	}
	
	
}