using System;
using System.Collections;
using UnityEngine;

public class PickupObject : GameScript
{
    public int ID { get; set; }
    //public string Name { get; set; }
    public string Sprite { get; set; }
    public Vector3 Position { get; set; }
    public int Owner { get; set; }
    public int Holder { get; set; }
    public int ObjectiveForPlayer { get; set; }
    public int ObjectiveIndex { get; set; }
	public bool IsCollected { get; set; }
	public bool IsStolen { get; set; }

    void Start()
    {
        ID = 0;
        //Name = name;
        Sprite = "";
        Position = Vector3.zero;
        Owner = -1;
        ObjectiveForPlayer = -1;
        ObjectiveIndex = -1;
        Holder = -1;
		IsCollected = false;
    }
}
