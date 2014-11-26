using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Delaunay;
using Delaunay.Geo; 

public class LevelGenerator : MonoBehaviour
{
	//Colors for teleporter influence areas
	private List<Color> colors = new List<Color> ();
	private Color colorBlue = new Color (0.0f, 0.0f, 1.0f, 0.7f);
	private Color colorGreen = new Color (0.0f, 1.0f, 0.0f, 0.7f);
	private Color colorRed = new Color (1.0f, 0.0f, 0.0f, 0.7f);
	private Color colorYellow = new Color (1.0f, 1.0f, 0.0f, 0.7f);
	private Color colorBlack = new Color (1.0f, 1.0f, 0.0f, 0.7f);
	private Color[] moreColors = new Color[]{
		new Color(0.93f,0.97f,0.98f,0.7f),
		new Color (0.75f, 0.83f, 0.9f,0.7f),
		new Color (0.62f, 0.74f, 0.85f,0.7f),
		new Color (0.55f, 0.59f, 0.78f,0.7f),
		new Color (0.55f, 0.42f, 0.69f,0.7f),
		new Color (0.53f, 0.25f, 0.62f),
		new Color (0.43f, 0.0f, 0.42f),
		new Color (0.13f, 0.4f, 0.67f)};

	private List<GameObject> m_enemies;
	private List<Vector2> m_points;
	private List<LineSegment> m_edges;

	List<Rectangle> rects;

	public GameObject prefab_teleporter;
	public GameObject prefab_wall;
	public GameObject prefab_floor;
	public int m_pointCount = 100;
	public float m_mapWidth = 100;
	public float m_mapHeight = 50;
	public float m_wallHeight=1;
	public float m_minCellDistance = 5;

	private Delaunay.Voronoi v;
	private GameGraph graphTele;
	public GameGraph getTeleportGraph(){
		return graphTele;
	}
	
	void Awake ()
	{
		for (int i=0; i<moreColors.Count(); i++) {
			colors.Add (moreColors[i]);
		}

		Vector2[] startGoalCells;
		GameGraph graphCells;
		List<Node> path = null;

		//Create level and check if path from start- to goal cell exists (left to right). Otherwise, repeat.
		do {
			startGoalCells = GenerateLevel ();
			Vector2 startCell = startGoalCells[0];
			Vector2 goalCell = startGoalCells[1];
		
			//Create graphCells with nodes representing cells
			graphCells = createGameGraph (v,startCell,goalCell);

			path = graphCells.BFS (graphCells.getNode (startCell), graphCells.getNode (goalCell));
		} while(path==null);

		foreach (Node n in path) {
			GameObject sphere = GameObject.CreatePrimitive (PrimitiveType.Sphere);
			sphere.transform.position = new Vector3 (n.coords.x - m_mapWidth / 2, 0.5f, n.coords.y - m_mapHeight / 2);
		}

		populateLevel ();

        m_enemies = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
	}

	private Vector2[] GenerateLevel(){

		//Create bold boundaries of level
		rects = createBoundaries ();

		//Create point seed within boundaries
		m_points = createRandomSeed ();

		//Calculate voronoi tesselation
		v = new Delaunay.Voronoi (m_points, null, new Rect (0, 0, m_mapWidth, m_mapHeight));
		m_edges = v.VoronoiDiagram ();

		//Truncate voronoi: Remove cells that connect to the boundaries
		//TODO: Remove loose ends of length > 1 (recursively?)
		truncateVoronoi (v);

		//Determine start (closestLeft) and goal cell (closestRight)
		Vector2 startCell=new Vector2(m_mapWidth/2,0.0f);
		Vector2 goalCell=new Vector2(m_mapWidth/2,0.0f);
		foreach (Vector2 point in m_points) {
			if(startCell.x>point.x){
				startCell = point;
			}
			if(goalCell.x<point.x){
				goalCell = point;
			}
		}

		return new Vector2[]{startCell,goalCell};

	}

	private void populateLevel(){

		/*
		//Create new graph with nodes, but blank adjacency matrix
		graphTele = new GameGraph ();
		foreach (Node n in graphCells.Nodes()) {
			Node newNode = new Node(n.coords,n.type,n.id);
			graphTele.addNode(newNode);
		}
		
		//Assing teleporters along shortest path from start to goal cell
		for (int i=1; i<path.Count; i++) {
			Vector2 coordsSite1 = path [i - 1].coords;
			Vector2 coordsSite2 = path [i].coords;
			
			//Cells that are connected by a teleporter will be represented as adjacent in graphTele
			Node n1 = graphTele.getNode(coordsSite1);
			Node n2 = graphTele.getNode(coordsSite2);

			List<Vector2> commonEdge = graphCells.getNode(n1.id).getEdgeToAdjacent(n2);
			n1.addAdjacent(n2,commonEdge[0],commonEdge[1]);
			n2.addAdjacent(n1,commonEdge[0],commonEdge[1]);

			//Instantiate teleporter at wall
			List<Vector2> commonEdge = graphCells.getNode (n1).getEdgeToAdjacent (n2);
			createTeleporter((Vector2)commonEdge[0],(Vector2)commonEdge[1]);
		}
*/
		//Create cell walls from edges
		createWalls ();

		//Create floor
		GameObject floor = Instantiate(prefab_floor) as GameObject; 
		floor.transform.localScale = new Vector3 (10.0f,1.0f,5.0f);
		floor.transform.position = new Vector3 (0.0f, 0.0f, 0.0f);
		floor.transform.parent = transform;
	/*
		//Create teleport areas
		foreach (Node n in graphCells.Nodes()) {
			getTeleportAreas (n, v, graphCells, graphTele);
		}
*/
		transform.localPosition = new Vector3 (0.0f, 0.0f, 0.0f);
		
	}

	//Remove cells that connect to the boundaries
	private void truncateVoronoi(Delaunay.Voronoi v){
		HashSet<Vector2> observe = new HashSet<Vector2>();
		for(int i=0; i<m_points.Count(); i++){
			bool atBoundary = false;
			Vector2 site = m_points[i];
			List<Vector2> corners = v.Region(site);
			for(int j=0; j<corners.Count(); j++){
				Vector2 corner = corners[j];
				if(corner.x==0 || corner.x==m_mapWidth || corner.y==0 || corner.y == m_mapHeight){
					atBoundary=true;
					
					for(int k=0; k<m_edges.Count; k++){
						if(m_edges[k].p0==corner){
							observe.Add((Vector2)m_edges[k].p1);
							m_edges.RemoveAt(k);
							k--;
						}else if(m_edges[k].p1==corner){
							observe.Add((Vector2)m_edges[k].p0);
							m_edges.RemoveAt(k);
							k--;
						}
					}/*
					corners.RemoveAt (j);
					j--;*/
				}
			}
			
			if(atBoundary){
				m_points.RemoveAt(i);
				i--;
			}
		}
		
		//Remove loose ends, i.e. if only one line remains with this point as endpoint
		float epsilon = 0.001f;
		foreach(Vector2 point in observe){
			int foundAt = -1;
			int sharedBy = 0;
			for(int i=0; i<m_edges.Count(); i++){
				LineSegment edge = m_edges[i];
				if(Vector2.Distance((Vector2)edge.p0,point)<epsilon || Vector2.Distance((Vector2)edge.p1,point)<epsilon){
					//if(((Vector2)edge.p0)==point || ((Vector2)edge.p1)==point){
					foundAt = i;
					sharedBy++;
				}
			}
			if(sharedBy==1){
				m_edges.RemoveAt(foundAt);
			}
		}
	}

	//Create game graph: One node per cell and adjacency
	public GameGraph createGameGraph(Delaunay.Voronoi v, Vector2 startCell, Vector2 goalCell){

		int id = 0;
		GameGraph graphCells = new GameGraph ();
		for (int i=0; i<m_points.Count; i++) {
			graphCells.addNode(new Node(m_points[i], Node.CellType.ordinary, id++));
		}
		graphCells.getNode (startCell).type = Node.CellType.start;
		graphCells.getNode (goalCell).type = Node.CellType.goal;

		//Set adjacency
		foreach (Node n1 in graphCells.Nodes()) {
			List<Vector2> r1 = v.Region (n1.coords);
			foreach (Node n2 in graphCells.Nodes ()) {
				if (n1.id == n2.id) {
						continue;
				}
				List<Vector2> r2 = v.Region (n2.coords);
				Vector2[] commonPoints = new Vector2[2];
				int i = 0;
				float epsilon = 0.001f;
				foreach (Vector2 p1 in r1) {
					foreach (Vector2 p2 in r2) {
						if (Vector2.Distance (p1, p2) < epsilon) {
							commonPoints [i] = p1;
							i++;
							if (i == 2) {
								int[] nbs = new int[]{n1.id,n2.id};
								n1.addAdjacent(n2, commonPoints[0],commonPoints[1]);
							}
						}
					}
				}
			}
		}

		return graphCells;
	}
	
	//Calculate influence areas of teleporters for a particular node
	private Dictionary<List<Vector2>,Node> getTeleportAreas(Node n, Delaunay.Voronoi v, GameGraph graphCells, GameGraph graphTele){
		Vector2 middle = n.coords;
		List<List<Vector2>> ordinaryTris = new List<List<Vector2>> ();
		Dictionary<List<Vector2>,Node> subdivision = new Dictionary<List<Vector2>,Node> ();
		
		if (graphTele.getNode (middle).getAdjacent ().Count == 0) {
			return subdivision;		
		}
		
		int colorIndex = 0;
		Dictionary<Node,Color> colorDict = new Dictionary<Node,Color> ();
		List<Node> adjacentCells = graphCells.getNode (middle).getAdjacent ();
		foreach(Node adj in adjacentCells){
			List<Vector2> edge = graphTele.getNode(middle).getEdgeToAdjacent(adj);
			if(edge !=null){
				edge.Add(middle);
				subdivision.Add(edge, adj);
				colorDict.Add(adj, colors[colorIndex]);
				colorIndex++;
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
		
		foreach (KeyValuePair<List<Vector2>, Node> entry in subdivision) {
			List<Vector2> tri = entry.Key;
			Color c = colorDict[entry.Value];
			createTriangle (new Vector3 (tri[0].x, 0.0f, tri[0].y), new Vector3 (tri[1].x, 0.0f, tri[1].y), new Vector3 (tri[2].x, 0.0f, tri[2].y), c);
		}
		
		/*
		GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		sphere.transform.position = new Vector3(n.coords.x-m_mapWidth/2,0.5f,n.coords.y-m_mapHeight/2);
		*/
		
		return subdivision;
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

	// Create a number of rectangles that can be traversed horizontally
	private List<Rectangle> createBoundaries(){
		
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
		//Check whether this point is too close to other points
		for (int i=0; i<points.Count(); i++) {
			Vector2 point = points[i];
			for(int j=i+1; j<points.Count(); j++){
				Vector2 otherPoint = points[j];
				if(Vector2.Distance(point, otherPoint)< m_minCellDistance){
					points.RemoveAt(j);
					j--;
				}
			}
		}
		return points;
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

	// Create teleporter from prefab on specified line
	private void createTeleporter(Vector2 p0, Vector2 p1){
		
		Vector2 midpoint = (p0+p1)*0.5f;
		float length = Vector2.Distance(p0,p1)*0.5f;
		float angle = -Mathf.Atan2(p0.y - p1.y, p0.x - p1.x) * (180 / Mathf.PI) + 90;
		GameObject teleporter = Instantiate(prefab_teleporter,new Vector3(midpoint.x-m_mapWidth/2,0.5f*m_wallHeight,midpoint.y-m_mapHeight/2),Quaternion.Euler(new Vector3(0.0f,angle,0.0f))) as GameObject; 
		teleporter.transform.localScale = new Vector3(0.5f,0.8f*m_wallHeight,length);
		teleporter.transform.parent = transform;
		
	}

	//Create a custom mesh representing a triangle from the 3 given points, located at their midpoint and assigned the given (semi-transparent) color
	private void createTriangle(Vector3 p0, Vector3 p1, Vector3 p2, Color c){
		
		//Create triangle mesh representing the area of teleporter influence
		Mesh mesh = new Mesh ();
		
		mesh.Clear();
		mesh.vertices = new Vector3[]{p0,p1,p2};
		mesh.triangles = new int[]{2,1,0};
		mesh.RecalculateNormals();

		if(mesh.normals[0].y<0.0f){
			mesh.triangles = new int[]{0,1,2};
			mesh.RecalculateNormals();
		}

		Vector2[] uvs = new Vector2[3];
		uvs[0] = new Vector2(0,0); //bottom-left
		uvs[1] = new Vector2(1,0); //bottom-right
		uvs[2] = new Vector2(0.5f,1); //top-middle
		mesh.uv = uvs;

		mesh.RecalculateBounds();
		mesh.Optimize();

		GameObject triangle = new GameObject ();
		triangle.name = "Triangle";
		triangle.AddComponent (typeof(MeshFilter));
		triangle.AddComponent (typeof(MeshRenderer));
		triangle.GetComponent<MeshFilter> ().mesh = mesh;
		
		//Material myNewMaterial = new Material(Shader.Find("Transparent/Diffuse"));
		Material myNewMaterial = new Material(Shader.Find("Transparent/Diffuse"));

		myNewMaterial.SetColor ("_Color", c);
		triangle.renderer.material = myNewMaterial;
		
		triangle.transform.localPosition=new Vector3(-m_mapWidth/2, 0.1f, -m_mapHeight/2);
		triangle.transform.parent = transform;

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