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
			foreach (Node child in node.getAdjacent())
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

}
