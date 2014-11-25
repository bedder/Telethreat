using UnityEngine;
using System.Collections.Generic;

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