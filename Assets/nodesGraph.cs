using UnityEngine;
using System.Collections.Generic;

public class nodesGraph{

	public List<nodesGraph> nodesList;
    public int x;
    public int y;

    public float g;
    public float rhs;

    public nodesGraph()
    {
        nodesList = new List<nodesGraph>();
    }
}
