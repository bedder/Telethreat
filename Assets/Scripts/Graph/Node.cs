using UnityEngine;
using System.Collections.Generic;

public class Node {

	public enum CellType{start,goal,ordinary};
		
	public CellType type;
	public Vector2 coords{ get; set; }
	public int id;
	private List<Node> adjacent;

	public Node(Vector2 coords, CellType type, int id){
		this.coords = coords;
		this.type = type;
		this.id = id;

		adjacent = new List<Node> ();
	}

	public void addAdjacent(Node n){
		if(!adjacent.Contains(n)){
			adjacent.Add(n);
		}
	}

	public List<Node> getAdjacent(){
		return adjacent;
	}
}
