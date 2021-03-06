﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class Node: IEquatable<Node> {

	public enum CellType{start,goal,ordinary};
		
	public CellType type{ get; set;}
	public Vector2 coords{ get; set; }
	public int id{ get; set; }
	public float minRadius{ get; set;}
	private Dictionary<Node, List<Vector2>> adjacent;

	public Node(Vector2 coords, CellType type, int id, float minRadius){
		this.coords = coords;
		this.type = type;
		this.id = id;
		this.minRadius = minRadius;
		adjacent = new Dictionary<Node, List<Vector2>> ();
	}

	public void addAdjacent(Node n, Vector2 p1, Vector2 p2){
		if(!adjacent.ContainsKey(n)){
			List<Vector2> edge = new List<Vector2>();
			edge.Add(p1);
			edge.Add(p2);
			adjacent.Add(n, edge);
		}
	}

	public List<Node> getAdjacentCells(){
		return adjacent.Keys.ToList();
	}

	public List<List<Vector2>> getAdjacentEdges(){
		return adjacent.Values.ToList();
	}

	public List<Vector2> getEdgeToAdjacent(Node n){
		List<Node> keys = adjacent.Keys.ToList ();
		if(adjacent.ContainsKey(n)){
			return adjacent[n];
		}
		return null;
	}

	public bool Equals(Node other)
	{
		if (other == null) return false;
		return (this.id == other.id);
	}
	
	public override bool Equals(object obj)
	{
		Node emp = obj as Node;
		if (emp != null)
		{
			return this.id == emp.id;
		}
		else
		{
			return false;
		}
	}

	public override int GetHashCode()
	{
		return this.id;		
	}
}
