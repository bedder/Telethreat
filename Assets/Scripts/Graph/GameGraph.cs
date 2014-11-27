using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameGraph {

	SortedDictionary<int,Node> nodes;
	Dictionary<Vector2,Node> nodesByPosition;

	public GameGraph(){
		nodes = new SortedDictionary<int,Node> ();
		nodesByPosition = new Dictionary<Vector2,Node> ();
	}

	public void addNode(Node n){
		nodes.Add (n.id, n);
		nodesByPosition.Add (n.coords, n);
	}

	public List<Node> Nodes(){
		return nodes.Values.ToList ();
	}

	public Node getNode(Vector2 coord){
		if (nodesByPosition.ContainsKey (coord)) {
			return nodesByPosition[coord];
		}
		return null;
	}

	public Node getNode(int id){
		if (nodes.ContainsKey (id)) {
			return nodes[id];
		}
		return null;
	}

	//Ordinary breadth-first search returning the path from a given start- to a given goal node
	public List<Node> BFS(Node start, Node goal)
	{	
		List<int> visited = new List<int> ();
		Queue<List<Node>> queue = new Queue<List<Node>>();
		List<Node> path = new List<Node> ();
		path.Add (start);

		queue.Enqueue (path);
		visited.Add(start.id);
		while(queue.Count>0)
		{
			path = queue.Dequeue();
			Node node = path[path.Count-1];

			if(node.id == goal.id)
			{
				return path;
			}
			foreach (Node child in node.getAdjacentCells())
			{
				if(!visited.Contains(child.id)){
					List<Node> newPath = new List<Node>(path);
					newPath.Add (child);
					queue.Enqueue(newPath);
					visited.Add (child.id);
				}
			}
		}
		return null;
	} 

	//Prim's algorithm for finding the minimum spanning tree of the graph
	public List<int[]> PrimMinSpanningTree(){
		HashSet<int> v = new HashSet<int> ();
		for (int i=0; i<this.Nodes().Count; i++) {
			v.Add(Nodes()[i].id);
		}

		List<int[]> edgeNew = new List<int[]> ();
		List<int> vNewList = new List<int> ();
		HashSet<int> vNew = new HashSet<int> ();
		vNew.Add (this.Nodes()[0].id);
		vNewList.Add (this.Nodes()[0].id);
		//Node lastNode = this.Nodes()[0];
		int index = 0;
		while (!vNew.SetEquals(v)) {
			bool foundAdj = false;
			Node lastNode = this.getNode(vNewList[index]);
			foreach(Node adj in lastNode.getAdjacentCells()){
				if(!vNew.Contains(adj.id)){
					foundAdj = true;
					vNew.Add(adj.id);
					vNewList.Add (adj.id);
					edgeNew.Add (new int[]{lastNode.id,adj.id});
					index = vNewList.Count()-1;
					break;
				}
			}
			if(!foundAdj){
				index--;
			}
		}
		return edgeNew;
	}

}
