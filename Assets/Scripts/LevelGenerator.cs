using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using Delaunay;
using Delaunay.Geo; 

public class LevelGenerator : MonoBehaviour
{
    private GameController gameController;

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

	private List<Vector2> m_points;
	private List<LineSegment> m_edges;

	private GameObject gameObject_triangle;
	private GameObject gameObject_walls;
	private GameObject gameObject_floor;
	private GameObject gameObject_teleporters;
	private GameObject gameObject_enemies;
	private GameObject gameObject_players;
	private GameObject gameObject_obstacles;
	private GameObject gameObject_chargers;
	private GameObject gameObject_alarmLights;

	
	List<Rectangle> rects;
	public List<GameObject> prefab_enemies;
	public GameObject prefab_goal;
	public GameObject prefab_player;
	public GameObject prefab_teleporter;
	public GameObject prefab_wall;
	public GameObject prefab_obstacle;
	public GameObject prefab_charger;
	public GameObject prefab_warnLight;
	public Material material_floor;
	public int m_pointCount = 100;
	public float m_mapWidth = 100;
	public float m_mapHeight = 50;
	public float m_wallHeight=1;
	public float m_minCellDistance = 5;
	public int m_teleporterCount = 25;
	public int m_obstaclesCount = 20;
	public int m_chargerCount = 5;
	public int m_monsterCount = 50;
	public float m_minDistanceBetweenObjects = 3;
	public Dictionary<int,Dictionary<List<Vector2>,Node>> teleportAreas {get; set;}
	public Dictionary<int,List<Vector2>> objectPositions;

	private Delaunay.Voronoi v;
	private GameGraph graphTele;
	public GameGraph getTeleportGraph(){
		return graphTele;
	}
	private List<AlarmLight> alarmLights;
	public List<AlarmLight> getAlarmLights(){
		return alarmLights;
	}
	
	void Awake ()
	{
        gameController = GameObject.FindObjectOfType<GameController>();
        prefab_player = (GameObject)gameController.playerController;

		//Initialize objects
		Vector2[] startGoalCells;
		GameGraph graphCells;
		List<Node> path = null;
		for (int i=0; i<moreColors.Count(); i++) {
				colors.Add (moreColors [i]);
		}
		gameObject_triangle = new GameObject ("Triangles");
		gameObject_triangle.transform.parent = transform;
		gameObject_floor = new GameObject ("Floor");
		gameObject_floor.transform.parent = transform;
		gameObject_teleporters = new GameObject ("Teleporters");
		gameObject_teleporters.transform.parent = transform;
		gameObject_walls = new GameObject ("Walls");
		gameObject_walls.transform.parent = transform;
		gameObject_enemies = new GameObject ("Enemies");
		gameObject_enemies.transform.parent = transform;
		gameObject_players = new GameObject ("Players");
		gameObject_players.transform.parent = transform;
		gameObject_obstacles = new GameObject ("Obstacles");
		gameObject_obstacles.transform.parent = transform;
		gameObject_chargers = new GameObject ("Chargers");
		gameObject_chargers.transform.parent = transform;
		gameObject_alarmLights = new GameObject ("Alarm Lights");
		gameObject_alarmLights.transform.parent = transform;

		//Create level and check if path from start- to goal cell exists (left to right). Otherwise, repeat.
		Vector2 startCell;
		Vector2 goalCell;
		do {
			startGoalCells = GenerateLevel ();
			startCell = startGoalCells [0];
			goalCell = startGoalCells [1];

			//Place goal object at goal
			GameObject goalObject = Instantiate(prefab_goal) as GameObject;
			goalObject.transform.position = new Vector3(goalCell.x - m_mapWidth / 2.0f, 0.0f, goalCell.y - m_mapHeight / 2.0f);
			goalObject.transform.parent = transform;

			//Create game graph with nodes representing cells
			graphCells = createGameGraph (v, startCell, goalCell);

			path = graphCells.BFS (graphCells.getNode (startCell), graphCells.getNode (goalCell));
		} while(path==null);

		//Create teleporters and calculate/highlight areas of influence
		graphTele = createTeleporters (graphCells, path, v);
		teleportAreas = new Dictionary<int,Dictionary<List<Vector2>,Node>> ();
		foreach (Node n in graphCells.Nodes()) {
			Dictionary<List<Vector2>,Node> areas = getTeleportAreas (n, v, graphCells, graphTele);
			teleportAreas.Add (n.id,areas);
		}

		//Create meshes and store the positions of those that might collide in "objectsLocation" (per cell)
		objectPositions = new Dictionary<int,List<Vector2>> ();
	
		createWalls ();

		createFloor ();

		createObstacles ();

		createChargers ();

		createAlarmLights ();

		transform.localPosition = new Vector3 (0.0f, 0.0f, 0.0f);

		//First monsters, then players. This order is crucial to allow players to determine the monsters in the same cell
		spawnMonsters(graphCells);
		spawnPlayer(graphCells.getNode(startCell));

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

	private void createFloor(){

		for (int k=0; k<m_points.Count(); k++) {
			Vector2 cell = m_points [k];		

			List<Vector2> regions = v.Region (cell);
			Vector3[] vertices = new Vector3[regions.Count + 1];
			vertices [0] = new Vector3 (cell.x, 0.0f, cell.y);
			for (int j=0; j<regions.Count(); j++) {
					vertices [j + 1] = new Vector3 (regions [j].x, 0.0f, regions [j].y);
			}

			List<LineSegment> segments = v.VoronoiBoundaryForSite (cell);
			List<Vector3> outerRing = new List<Vector3> ();
			outerRing.Add (new Vector3 (segments [0].p0.Value.x, 0.0f, segments [0].p0.Value.y));
			outerRing.Add (new Vector3 (segments [0].p1.Value.x, 0.0f, segments [0].p1.Value.y));
			segments.RemoveAt (0);
			while (segments.Count>0) {
					Vector3 last = outerRing [outerRing.Count - 1];
					for (int i=0; i<segments.Count(); i++) {
							Vector3 p0 = new Vector3 (segments [i].p0.Value.x, 0.0f, segments [i].p0.Value.y);
							Vector3 p1 = new Vector3 (segments [i].p1.Value.x, 0.0f, segments [i].p1.Value.y);
							if (Vector3.Distance (p0, last) < 0.001) {
									outerRing.Add (p1);
									segments.RemoveAt (i);
									break;
							} else if (Vector3.Distance (p1, last) < 0.001) {
									outerRing.Add (p0);
									segments.RemoveAt (i);
									break;
							}
					}
			}

			int[] triangles = new int[v.VoronoiBoundaryForSite (cell).Count () * 3];
			int x = 0;
			for (int i=0; i<outerRing.Count-1; i++) {

					int ip0, ip1;
					for (ip0=0; ip0<vertices.Count(); ip0++) {
							if (Vector3.Distance (outerRing [i], vertices [ip0]) < 0.001) {
									break;
							}
					}
					for (ip1=0; ip1<vertices.Count(); ip1++) {
							if (Vector3.Distance (outerRing [i + 1], vertices [ip1]) < 0.001) {
									break;
							}
					}

					triangles [x] = 0;
					triangles [x + 1] = ip1;
					triangles [x + 2] = ip0;
					x += 3;
			}

			//Create triangle mesh representing the area of teleporter influence
			Mesh mesh = new Mesh ();
			mesh.Clear ();
			mesh.vertices = vertices;
			mesh.triangles = triangles;
			mesh.RecalculateNormals ();

			Vector2[] uvs = new Vector2[vertices.Length];
			for (int i = 0; i < uvs.Count(); i++) {
					uvs [i] = new Vector2 (vertices [i].x, vertices [i].z);
			}
			mesh.uv = uvs;

			mesh.RecalculateBounds ();
			mesh.Optimize ();

			GameObject cellFloor = new GameObject ();
			cellFloor.name = "CellFloor";
			cellFloor.AddComponent (typeof(MeshFilter));
			cellFloor.AddComponent (typeof(MeshRenderer));
			cellFloor.AddComponent (typeof(MeshCollider));
			cellFloor.GetComponent<MeshFilter> ().mesh = mesh;
			cellFloor.GetComponent<MeshCollider>().sharedMesh=mesh;
			cellFloor.renderer.material = material_floor;
			cellFloor.transform.localPosition = new Vector3 (-m_mapWidth / 2, 0.0f, -m_mapHeight / 2);
			cellFloor.transform.parent = gameObject_floor.transform;
		}
	}
	
	private GameGraph createTeleporters(GameGraph graphCells, List<Node> path, Delaunay.Voronoi v){

		//Create new graph with nodes, but blank adjacency matrix
		GameGraph tele = new GameGraph ();
		foreach (Node n in graphCells.Nodes()) {
			Node newNode = new Node(n.coords,n.type,n.id, n.minRadius);
			tele.addNode(newNode);
		}

		//Assing teleporters along minimum spanning tree, so each cell can be accessed
		List<int[]> cells = graphCells.PrimMinSpanningTree ();
		for (int i=0; i<cells.Count(); i++) {
			Vector2 coordsSite1 = graphCells.getNode(cells[i][0]).coords;
			Vector2 coordsSite2 = graphCells.getNode(cells[i][1]).coords;
			
			//Cells that are connected by a teleporter will be represented as adjacent in graphTele
			Node n1 = tele.getNode(coordsSite1);
			Node n2 = tele.getNode(coordsSite2);
			
			List<Vector2> commonEdge = graphCells.getNode(n1.id).getEdgeToAdjacent(n2);
			n1.addAdjacent(n2,commonEdge[0],commonEdge[1]);
			n2.addAdjacent(n1,commonEdge[0],commonEdge[1]);
			
			//Instantiate teleporter at wall
			createTeleporter((Vector2)commonEdge[0],(Vector2)commonEdge[1]);
			m_teleporterCount--;
		}

		/*
		//Assing teleporters along shortest path from start to goal cell
		for (int i=1; i<path.Count; i++) {
			Vector2 coordsSite1 = path [i - 1].coords;
			Vector2 coordsSite2 = path [i].coords;
			
			//Cells that are connected by a teleporter will be represented as adjacent in graphTele
			Node n1 = tele.getNode(coordsSite1);
			Node n2 = tele.getNode(coordsSite2);

			List<Vector2> commonEdge = graphCells.getNode(n1.id).getEdgeToAdjacent(n2);
			n1.addAdjacent(n2,commonEdge[0],commonEdge[1]);
			n2.addAdjacent(n1,commonEdge[0],commonEdge[1]);

			//Instantiate teleporter at wall
			createTeleporter((Vector2)commonEdge[0],(Vector2)commonEdge[1],teleporter);
			m_teleporterCount--;
		}*/

		//Assing more teleporters on random points, if there are any teleporters left to distribute
		for (int i=0; i<m_teleporterCount; i++) {
			int rnd = UnityEngine.Random.Range(0,graphCells.Nodes().Count()-1);
			Node rndNode = graphCells.Nodes()[rnd];
			if(rndNode.getAdjacentCells().Count()>0){
				rnd = UnityEngine.Random.Range(0,rndNode.getAdjacentCells().Count()-1);
				Node rndAdjacent = rndNode.getAdjacentCells()[rnd];
				if(!tele.getNode(rndNode.id).getAdjacentCells().Contains(rndAdjacent)){
					List<Vector2> commonEdge = graphCells.getNode(rndNode.id).getEdgeToAdjacent(rndAdjacent);
					rndNode.addAdjacent(rndAdjacent,commonEdge[0],commonEdge[1]);
					rndAdjacent.addAdjacent(rndNode,commonEdge[0],commonEdge[1]);
					
					//Instantiate teleporter at wall
					createTeleporter((Vector2)commonEdge[0],(Vector2)commonEdge[1]);
					m_teleporterCount--;
				}
			}
		}

		return tele;
		
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
			List<LineSegment> edges = v.VoronoiBoundaryForSite(m_points[i]);
			List<Vector2> midpoints = new List<Vector2>();
			foreach(LineSegment edge in edges){
				midpoints.Add ((Vector2)(edge.p0+edge.p1)/2f);
			}
			float minRadius = float.MaxValue;
			foreach(Vector2 midpoint in midpoints){
				float dist = Vector2.Distance(midpoint,m_points[i]);
				if(dist<minRadius){
					minRadius = dist;
				}
			}
			graphCells.addNode(new Node(m_points[i], Node.CellType.ordinary, id++, minRadius));
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
		
		if (graphTele.getNode (middle).getAdjacentCells().Count == 0) {
			return subdivision;		
		}
		
		int colorIndex = 0;
		Dictionary<Node,Color> colorDict = new Dictionary<Node,Color> ();

		//Distinguish edges next to teleporters and ordinary edges
		List<List<Vector2>> teleporterEdges = new List<List<Vector2>> ();
		List<Node> adjacentCells = graphCells.getNode(middle).getAdjacentCells();
		foreach (Node adj in adjacentCells)
		{
			//Teleporter edge
			List<Vector2> edge = graphTele.getNode(middle).getEdgeToAdjacent(adj);
			if (edge != null)
			{
				teleporterEdges.Add (edge);
				edge.Add(middle);
				subdivision.Add(edge, adj);
				colorDict.Add(adj, colors[colorIndex]);
				colorIndex++;
			}
		}

		float epsilon = 0.001f;
		List<List<Vector2>> ordinaryEdges = new List<List<Vector2>> ();
		foreach (LineSegment ls in v.VoronoiBoundaryForSite (middle)) {
			List<Vector2> edge = new List<Vector2>();
			edge.Add ((Vector2)ls.p0);
			edge.Add ((Vector2)ls.p1);
			ordinaryEdges.Add (edge);
		}

		for(int i=0; i<ordinaryEdges.Count(); i++){
			List<Vector2> edgeOrd = ordinaryEdges[i];
			foreach(List<Vector2> edge in teleporterEdges){
				if((Vector2.Distance(edge[0],edgeOrd[0])<epsilon && Vector2.Distance(edge[1],edgeOrd[1])<epsilon)||(Vector2.Distance(edge[0],edgeOrd[1])<epsilon && Vector2.Distance(edge[1],edgeOrd[0])<epsilon)){
					ordinaryEdges.RemoveAt(i);
					i--;
					break;
				}
			}
		}

		foreach(List<Vector2> ordTri in ordinaryEdges){
			ordTri.Add (middle);
			ordinaryTris.Add (ordTri);
		}

		//Assign triangle to closest teleporter relative to the distance of the triangle midpoint to the middle of the teleporter edg
		adjacentCells = graphTele.getNode (middle).getAdjacentCells ();
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
			Vector3 midpoint = new Vector3((tri[0].x+tri[1].x+tri[2].x)/3.0f,0.0f,(tri[0].y+tri[1].y+tri[2].y)/3.0f);
			Color c = colorDict[entry.Value];
			createTriangle (new Vector3 (tri[0].x, 0.0f, tri[0].y), new Vector3 (tri[1].x, 0.0f, tri[1].y), new Vector3 (tri[2].x, 0.0f, tri[2].y), midpoint, c);
		}
		
		return subdivision;
	}
	
	private GameObject getRandomEnemy()
	{
		if (prefab_enemies.Count == 0)
		{
			Debug.LogWarning("No enemies loaded! :(");
			return null;
		}
		
		return prefab_enemies[(int)UnityEngine.Random.Range(0, prefab_enemies.Count)];
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
		float height = UnityEngine.Random.Range (m_mapHeight/2,m_mapHeight);
		rs.Add (new Rectangle (new Vector2 (x,y), width, height));
		
		for (int i=1; i<nRectangles; i++) {
			x = compartment*i + compartment / 2;
			height = UnityEngine.Random.Range (minHeight,m_mapHeight);
			
			float lowerBoundary = minHeight = rs[i-1].middle.y-(rs[i-1].height/2-minOverlap)-height/2;
			float upperBoundary = rs[i-1].middle.y+(rs[i-1].height/2-minOverlap)+height/2;
			
			y = UnityEngine.Random.Range (Mathf.Max(height/2,lowerBoundary), Mathf.Min(m_mapHeight-height/2,upperBoundary)); 
			rs.Add (new Rectangle (new Vector2 (x,y), width, height));
		}
		
		return rs;
	}
	
	// Create a random seed of points in the euclidean space of the rectangles
	private List<Vector2> createRandomSeed(){
		//Create vector of random seed points in euclidean space
		List<Vector2> points = new List<Vector2> (); 
		while (points.Count < m_pointCount) {
			Vector2 point = new Vector2 (UnityEngine.Random.Range (0, m_mapWidth), UnityEngine.Random.Range (0, m_mapHeight));
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
			wall.transform.localScale = new Vector3(0.3f,1.0f*m_wallHeight,length);
			wall.transform.parent = gameObject_walls.transform;

		}
	}
	
	private void createAlarmLights(){
		alarmLights = new List<AlarmLight> ();
		foreach (Node n in graphTele.Nodes ()) {
			GameObject warnLight = Instantiate (prefab_warnLight, new Vector3 (n.coords.x - m_mapWidth / 2, 0.5f, n.coords.y - m_mapHeight / 2), Quaternion.identity) as GameObject;
			AlarmLight alarmLight = warnLight.GetComponent<AlarmLight> ();
			alarmLight.transform.parent = gameObject_alarmLights.transform;
			alarmLights.Add(alarmLight);
		}
	}
	
	// Create teleporter from prefab on specified line
	private void createTeleporter(Vector2 p0, Vector2 p1){
		
		Vector2 midpoint = (p0+p1)*0.5f;
		float length = Vector2.Distance(p0,p1)*0.5f;
		float angle = -Mathf.Atan2(p0.y - p1.y, p0.x - p1.x) * (180 / Mathf.PI) + 90;
		GameObject teleporter = Instantiate(prefab_teleporter,new Vector3(midpoint.x-m_mapWidth/2,0.5f*m_wallHeight,midpoint.y-m_mapHeight/2),Quaternion.Euler(new Vector3(0.0f,angle,0.0f))) as GameObject; 
		teleporter.transform.localScale = new Vector3(0.5f,0.8f*m_wallHeight,length);
		teleporter.transform.parent = gameObject_teleporters.transform;
		
	}

	//TODO: Merge create Obstacles / Chargers into one method
	private void createObstacles(){
		for (int i=0; i<m_obstaclesCount; i++) {
			int nodeIndex = UnityEngine.Random.Range(0,graphTele.Nodes ().Count-1);
			Node n = graphTele.Nodes()[nodeIndex];
			//Do not place obstacles in the start node
			if(n.type==Node.CellType.start){
				i--;
				continue;
			}

			Vector2 position = new Vector2(n.coords.x-m_mapWidth/2f,n.coords.y-m_mapHeight/2f);
			position += UnityEngine.Random.insideUnitCircle*n.minRadius*0.8f;

			if(!objectPositions.ContainsKey(n.id)){
				List<Vector2> positions = new List<Vector2>();
				positions.Add (position);
				objectPositions.Add(n.id,positions);
			}else{
				//Check if collides
				bool collides = false;
				foreach(Vector2 obj in objectPositions[n.id]){
					if(Vector2.Distance(obj, position)<m_minDistanceBetweenObjects){
						collides = true;
						break; 
					}
				}	
				if(!collides){
					objectPositions[n.id].Add (position);
					GameObject obstacle = Instantiate(prefab_obstacle) as GameObject;
					obstacle.transform.position=new Vector3(position.x,1.1f,position.y);
					obstacle.transform.parent = gameObject.transform;
				}else{
					i--;
				}

			}
		}
	}

	private void createChargers(){
		for (int i=0; i<m_chargerCount; i++) {
			int nodeIndex = UnityEngine.Random.Range(0,graphTele.Nodes ().Count-1);
			Node n = graphTele.Nodes()[nodeIndex];
			//Do not place chargers in the start node
			if(n.type==Node.CellType.start){
				i--;
				continue;
			}

			Vector2 position = new Vector2(n.coords.x-m_mapWidth/2f,n.coords.y-m_mapHeight/2f);
			position += UnityEngine.Random.insideUnitCircle*n.minRadius*0.8f;

			if(!objectPositions.ContainsKey(n.id)){
				List<Vector2> positions = new List<Vector2>();
				positions.Add (position);
				objectPositions.Add(n.id, positions);
			}else{
				//Check if collides
				bool collides = false;
				foreach(Vector2 obj in objectPositions[n.id]){
					if(Vector2.Distance(obj, position)<m_minDistanceBetweenObjects){
						collides = true;
						break; 
					}
				}	
				if(!collides){
					objectPositions[n.id].Add (position);
					GameObject charger = Instantiate(prefab_charger) as GameObject;
					charger.transform.position=new Vector3(position.x,0.5f,position.y);
					charger.transform.parent = gameObject_chargers.transform;
				}else{
					i--;
				}
			}
		}
	}

	private void spawnPlayer(Node startNode)
	{
		GameObject newPlayer=Instantiate(prefab_player, new Vector3(startNode.coords.x - m_mapWidth / 2, 0.2f, startNode.coords.y - m_mapHeight / 2), Quaternion.Euler(new Vector3(0.0f, 0.0f, 0.0f))) as GameObject;
		newPlayer.GetComponent<PlayerController>().setCurrentCellId(startNode.id);
		newPlayer.transform.parent = gameObject_players.transform;
	}
	
	private void spawnMonsters(GameGraph graphCells)
	{
		for (int i=0; i<m_monsterCount; i++) {
			
			int nodeIndex = UnityEngine.Random.Range(0,graphTele.Nodes ().Count-1);
			Node node = graphTele.Nodes()[nodeIndex];
			
			//Do not place enemies in the start node
			if(node.type==Node.CellType.start){
				i--;
				continue;
			}
			
			GameObject enemy = getRandomEnemy();
			if (enemy != null)
			{
				Vector2 position = new Vector2(node.coords.x-m_mapWidth/2f,node.coords.y-m_mapHeight/2f);
				position += UnityEngine.Random.insideUnitCircle*node.minRadius*0.8f;

				if(!objectPositions.ContainsKey(node.id)){
					List<Vector2> positions = new List<Vector2>();
					positions.Add (position);
					objectPositions.Add(node.id, positions);
				}else{
					//Check if collides
					bool collides = false;
					foreach(Vector2 obj in objectPositions[node.id]){
						if(Vector2.Distance(obj, position)<m_minDistanceBetweenObjects){
							collides = true;
							break; 
						}
					}	
					if(!collides){
					objectPositions[node.id].Add (position);
					GameObject newEnemy = Instantiate(enemy, new Vector3(position.x, 0.1f, position.y), Quaternion.identity) as GameObject;
					newEnemy.GetComponent<EnemyAI_BasicCollider>().CurrentCellId = node.id;
					newEnemy.transform.parent=gameObject_enemies.transform;
					}else{
						i--;
					}
				}
			}
		}
	}

	//Create a custom mesh representing a triangle from the 3 given points, located at their midpoint and assigned the given (semi-transparent) color
	private void createTriangle(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 midpoint, Color c){

		//Transform to origin
		p0 = p0 - midpoint;
		p1 = p1 - midpoint;
		p2 = p2 - midpoint;

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
		Material myNewMaterial = new Material (Shader.Find("Transparent/Diffuse"));

		myNewMaterial.SetColor ("_Color", c);
		triangle.renderer.material = myNewMaterial;

		triangle.transform.localPosition = new Vector3(midpoint.x-m_mapWidth/2, 0.1f, midpoint.z-m_mapHeight/2);
		triangle.transform.parent = gameObject_triangle.transform;

	}

	/*
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

		//Draw boundaries
		Gizmos.color = Color.yellow;
		Gizmos.DrawLine (new Vector2 (0, 0), new Vector2 (0, m_mapHeight));
		Gizmos.DrawLine (new Vector2 (0, 0), new Vector2 (m_mapWidth, 0));
		Gizmos.DrawLine (new Vector2 (m_mapWidth, 0), new Vector2 (m_mapWidth, m_mapHeight));
		Gizmos.DrawLine (new Vector2 (0, m_mapHeight), new Vector2 (m_mapWidth, m_mapHeight));
	}
	*/
}