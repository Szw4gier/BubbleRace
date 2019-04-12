using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class main : MonoBehaviour {
    int[,] nodes;

    public Text textInfo;

    int nodesAmountX = 0;
    int nodesAmountY = 0;

    int startCoordX = 0;
    int startCoordY = 0;

    int endCoordX = 0;
    int endCoordY = 0;

    // Use this for initialization
    void Start()
    {
        setSize();
        setStartEnd();
        generate();
        setPlayer(startCoordX, startCoordY);
        setOpponent(startCoordX, startCoordY);
        generateGraph();

        initialize();
        computeShortestPath();

        StartCoroutine("idleStart");   
    }

    void setSize() {
        //set size of playground
        nodesAmountX = Random.Range(10, 21);
        nodesAmountY = Random.Range(10, 21);

        //PRESENTATION/DEBUG
        //nodesAmountX = 4;
        //nodesAmountY = 4;

        //allocate nodes to memory
        nodes = new int[nodesAmountX, nodesAmountY];

        //innitialize nodes with its types: 0-start, 1-walkable terrain, 2-obsticle, 3-end

        float scale = Random.Range(0.0F, 1.0F);

        //set type for every node that will be generated
        for (int x = 0; x < nodesAmountX; x++)
        {
            for (int y = 0; y < nodesAmountY; y++)
            {
                float sampleX = x / scale;
                float sampleY = y / scale;

                float noiseVal = Mathf.PerlinNoise(sampleX, sampleY);

                if (noiseVal < 0.35)
                {
                    nodes[x, y] = 2;
                }
                else
                {
                    nodes[x, y] = 1;
                }
            }
        }

        //PRESENTATION/DEBUG
        //for (int x = 0; x < nodesAmountX; x++)
        //{
        //    for (int y = 0; y < nodesAmountY; y++)
        //    {
        //        nodes[x, y] = 1;
        //    }
        //}
        //nodes[1, 0] = 2;
        //nodes[1, 1] = 2;
        //nodes[2, 2] = 2;
        //nodes[2, 3] = 2;
    }

    //set start node and end node
    void setStartEnd() {
        int flip = Random.Range(0, 4);

        switch (flip)
        {
            case 0:
                startCoordX = Random.Range(0, 3);
                startCoordY = Random.Range(0, 3);
                endCoordX = nodesAmountX - Random.Range(0, 3) - 1;
                endCoordY = nodesAmountY - Random.Range(0, 3) - 1;
                nodes[startCoordX, startCoordY] = 0;
                nodes[endCoordX, endCoordY] = 3;
                break;
            case 1:
                startCoordX = Random.Range(nodesAmountX - 4, nodesAmountX - 1);
                startCoordY = Random.Range(0, 3);
                endCoordX = nodesAmountX - Random.Range(nodesAmountX - 4, nodesAmountX - 1) - 1;
                endCoordY = nodesAmountY - Random.Range(0, 3) - 1;
                nodes[startCoordX, startCoordY] = 0;
                nodes[endCoordX, endCoordY] = 3;
                break;
            case 2:
                startCoordX = Random.Range(0, 3);
                startCoordY = Random.Range(0, 3);
                endCoordX = nodesAmountX - Random.Range(nodesAmountX - 4, nodesAmountX - 1) - 1;
                endCoordY = nodesAmountY - Random.Range(0, 3) - 1;
                nodes[startCoordX, startCoordY] = 0;
                nodes[endCoordX, endCoordY] = 3;
                break;
            case 3:
                startCoordX = Random.Range(nodesAmountX - 4, nodesAmountX - 1);
                startCoordY = Random.Range(0, 3);
                endCoordX = nodesAmountX - Random.Range(0, 3) - 1;
                endCoordY = nodesAmountY - Random.Range(0, 3) - 1;
                nodes[startCoordX, startCoordY] = 0;
                nodes[endCoordX, endCoordY] = 3;
                break;
            default:
                break;
        }

        //PRESENTATION/DEBUG
        //startCoordX = 0;
        //startCoordY = 0;
        //endCoordX = 3;
        //endCoordY = 3;


        //nodes[startCoordX, startCoordY] = 0;
        //nodes[endCoordX, endCoordY] = 3;
    }

    public nodeType[] nodeTypes;

    Dictionary<Pair<int, int>, GameObject> walkableNodes = new Dictionary<Pair<int, int>, GameObject>();
    List<Pair<int, int>> walkableCoords = new List<Pair<int, int>>();

    Dictionary<Pair<int, int>, GameObject> notWalkableNodes = new Dictionary<Pair<int, int>, GameObject>();
    List<Pair<int, int>> notWalkableCoords = new List<Pair<int, int>>();

    //method that's generating nodes
    void generate()
    {
        for (int x = 0; x < nodesAmountX; x++)
        {
            for (int y = 0; y < nodesAmountY; y++)
            {
                if (nodeTypes[nodes[x,y]].isWalkable > 0)
                {                    
                    GameObject newWalk = (GameObject)Instantiate(nodeTypes[nodes[x, y]].nodePrefab, new Vector3(x, 0, y), Quaternion.identity);
                    Pair<int, int> newCoords = new Pair<int, int>(x,y);
                    if ((newCoords.first != startCoordX || newCoords.second != startCoordY)
                        && (newCoords.first != endCoordX || newCoords.second != endCoordY))
                    {
                        walkableCoords.Add(newCoords);
                        walkableNodes.Add(newCoords, newWalk);
                    }
                }
                else if (nodeTypes[nodes[x, y]].isWalkable == 0) 
                {
                    GameObject newNotWalk = (GameObject)Instantiate(nodeTypes[nodes[x, y]].nodePrefab, new Vector3(x, 0.5f, y), Quaternion.identity);
                    Pair<int, int> newNotWalkableCoords = new Pair<int, int>(x, y);

                    notWalkableCoords.Add(newNotWalkableCoords);
                    notWalkableNodes.Add(newNotWalkableCoords, newNotWalk);
                }
            }
        }
    }

    nodesGraph[,] graph;

    //generating graph nodes with neighbours
    void generateGraph()
    {
        graph = new nodesGraph[nodesAmountX, nodesAmountY];

        for (int x = 0; x < nodesAmountX; x++)
        {
            for (int y = 0; y < nodesAmountY; y++)
            {
                graph[x, y] = new nodesGraph();

                graph[x, y].x = x;
                graph[x, y].y = y;
            }
        }

        for (int x = 0; x < nodesAmountX; x++)
        {
            for (int y = 0; y < nodesAmountY; y++)
            {
                if (y < nodesAmountY - 1)
                {
                    graph[x, y].nodesList.Add(graph[x, y + 1]);
                }
                if (x < nodesAmountX - 1)
                {
                    graph[x, y].nodesList.Add(graph[x + 1, y]);
                }
                if (y > 0)
                {
                    graph[x, y].nodesList.Add(graph[x, y - 1]);
                }
                if (x > 0)
                {
                    graph[x, y].nodesList.Add(graph[x - 1, y]);
                }
            }
        }
    }

    //player data and spawn
    public playerData playerUnit = new playerData();

    void setPlayer(int sx, int sy)
    {
        playerUnit.actualX = sx;
        playerUnit.actualY = sy;
        playerUnit.Player = (GameObject)Instantiate(playerUnit.Player, new Vector3(sx, 0, sy), Quaternion.identity);
    }

    //opponent data and spawn
    public opponentData opponentUnit = new opponentData();

    void setOpponent(int sx, int sy)
    {
        opponentUnit.actualX = sx;
        opponentUnit.actualY = sy;
        opponentUnit.Opponent = (GameObject)Instantiate(opponentUnit.Opponent, new Vector3(sx, 0, sy), Quaternion.identity);
    }

    //start countdown
    bool pause = true;

    IEnumerator idleStart() 
    {
        for (int i = 3; i > 0; i--)
        {
            textInfo.text = "Info:" + i.ToString();
            yield return new WaitForSeconds(1f);
        }
        textInfo.text = "Info: Start!";
        StartCoroutine("opponentMovement");
        pause = false;       
    }

    //stop simulation when one of the units reaches goal
    void finishCondition() {
        if (playerUnit.actualX == endCoordX && playerUnit.actualY == endCoordY)
        {
            textInfo.text = "Info: wygrywa gracz";
            Time.timeScale = 0;
        }

        if (opponentUnit.actualX == endCoordX && opponentUnit.actualY == endCoordY)
        {
            textInfo.text = "Info: wygrywa przeciwnik";
            Time.timeScale = 0;
        }
    }

    bool waitForChanges = true;
    float nextspawn = 0;
    float nextremove = 0;

    // Update is called once per frame
    void Update()
    {   
        if (waitForChanges == false)
        {
            computeShortestPath();
            waitForChanges = true;
        }

        if (pause == false)
        {
            playerMovement();

            if (Time.time > nextspawn && walkableNodes.Count > 0)
            {
                nextspawn = Time.time + 1.5f;
                StartCoroutine("newObsticle", walkableCoords);
                
            }
            if (Time.time > nextremove && notWalkableNodes.Count > 0)
            {
                nextremove = Time.time + 2.5f;
                StartCoroutine("newRemovedObsticle", notWalkableCoords);
            }
        }

        finishCondition();
    }

    //movement of an opponnent
    bool targetControl = false;
    bool targetNotFound = false;
    
    IEnumerator opponentMovement()
    {
        while(startCoordX != endCoordX || startCoordY != endCoordY)
        {
            if (graph[startCoordX, startCoordY].g == System.Single.PositiveInfinity && targetNotFound == false)
            {
                textInfo.text = "Info: Cel nieosiągalny, czekam na zmiany pól";
                targetNotFound = true;
                targetControl = true;
                yield return null;
            }
            else if (graph[startCoordX, startCoordY].g != System.Single.PositiveInfinity)
            {
                yield return new WaitForSeconds(0.5f);
                localStart(graph[startCoordX, startCoordY]);

                opponentUnit.actualX = startCoordX;
                opponentUnit.actualY = startCoordY;
                opponentUnit.Opponent.transform.position = new Vector3(opponentUnit.actualX, 0, opponentUnit.actualY);

                if (targetControl == true)
                {
                    textInfo.text = "Info:";
                    targetNotFound = false;
                    targetControl = false;
                }
            }
            else 
            {
                yield return null;
            }
        }   
    }


    //movement of a player unit
    public void playerMovement()
    {
        if (Input.GetKey(KeyCode.UpArrow) && running == false)
        {
            StartCoroutine("playerMoveDir", KeyCode.UpArrow);
        }
        if (Input.GetKey(KeyCode.RightArrow) && running == false)
        {
            StartCoroutine("playerMoveDir", KeyCode.RightArrow);
        }
        if (Input.GetKey(KeyCode.DownArrow) && running == false)
        {
            StartCoroutine("playerMoveDir", KeyCode.DownArrow);
        }
        if (Input.GetKey(KeyCode.LeftArrow) && running == false)
        {
            StartCoroutine("playerMoveDir", KeyCode.LeftArrow);
        }
    }

    //player movement directions, dependant on which key is pressed
    bool running = false;
    IEnumerator playerMoveDir(KeyCode keyCode) {
        running = true;

        if (keyCode == KeyCode.UpArrow
            && (playerUnit.actualY + 1 < nodesAmountY)
            && nodeTypes[nodes[playerUnit.actualX, playerUnit.actualY + 1]].isWalkable == 1)
        {
            playerUnit.actualY = playerUnit.actualY + 1;
            playerUnit.Player.transform.position = new Vector3(playerUnit.actualX, 0, playerUnit.actualY);
        }
        if (keyCode == KeyCode.RightArrow
            && (playerUnit.actualX + 1 < nodesAmountX)
            && nodeTypes[nodes[playerUnit.actualX + 1, playerUnit.actualY]].isWalkable == 1)
        {
            playerUnit.actualX = playerUnit.actualX + 1;
            playerUnit.Player.transform.position = new Vector3(playerUnit.actualX, 0, playerUnit.actualY);
        }
        if (keyCode == KeyCode.DownArrow
            && (playerUnit.actualY - 1 >= 0)
            && nodeTypes[nodes[playerUnit.actualX, playerUnit.actualY - 1]].isWalkable == 1)
        {
            playerUnit.actualY = playerUnit.actualY - 1;
            playerUnit.Player.transform.position = new Vector3(playerUnit.actualX, 0, playerUnit.actualY);
        }
        if (keyCode == KeyCode.LeftArrow
            && (playerUnit.actualX - 1 >= 0)
            && nodeTypes[nodes[playerUnit.actualX - 1, playerUnit.actualY]].isWalkable == 1)
        {
            playerUnit.actualX = playerUnit.actualX - 1;
            playerUnit.Player.transform.position = new Vector3(playerUnit.actualX, 0, playerUnit.actualY);
        }

        yield return new WaitForSeconds(0.5f);
        running = false;
    }
    
    //random node pick from chosen collection
    Pair<int, int> pickNode(List<Pair<int, int>> coords)
    {
        System.Random rand = new System.Random();
        int index = rand.Next(coords.Count);

        return coords[index];
    }

    //generating new obsticles
    nodesGraph newObsticleData = new nodesGraph();
    IEnumerator newObsticle()
    {
        float control = 0;
        float duration = 1;

        Pair<int, int> chosenNodeCoords = pickNode(walkableCoords);

        Material clr = walkableNodes[chosenNodeCoords].GetComponent<Renderer>().material;

        Color defaultClr = clr.color;
        Color newClr = Color.red;

        while (control < 1)
        {
            clr.color = Color.Lerp(defaultClr, newClr, control);

            control += Time.deltaTime / duration;
            yield return null;
        }

        Destroy(walkableNodes[chosenNodeCoords]);
        nodes[chosenNodeCoords.first, chosenNodeCoords.second] = 4;

        walkableCoords.Remove(chosenNodeCoords);
        walkableNodes.Remove(chosenNodeCoords);

        GameObject newNotWalk = (GameObject)Instantiate(nodeTypes[nodes[chosenNodeCoords.first, chosenNodeCoords.second]].nodePrefab, new Vector3(chosenNodeCoords.first, 0, chosenNodeCoords.second), Quaternion.identity);

        notWalkableCoords.Add(chosenNodeCoords);
        notWalkableNodes.Add(chosenNodeCoords, newNotWalk);

        graph[chosenNodeCoords.first, chosenNodeCoords.second].g = System.Single.PositiveInfinity;
        graph[chosenNodeCoords.first, chosenNodeCoords.second].rhs = System.Single.PositiveInfinity;
        newObsticleData = graph[chosenNodeCoords.first, chosenNodeCoords.second];
        
        updateEdgeCost(newObsticleData);
        updateKeys();
        waitForChanges = false;
    }

    //removing obsticles
    nodesGraph newRemovedObsticleData = new nodesGraph();
    IEnumerator newRemovedObsticle() {
        float control = 0;
        float duration = 2;

        Pair<int, int> chosenNodeCoords = pickNode(notWalkableCoords);

        Material clr = notWalkableNodes[chosenNodeCoords].GetComponent<Renderer>().material;

        Color defaultClr = clr.color;
        Color newClr = Color.blue;

        while (control < 1)
        {
            clr.color = Color.Lerp(defaultClr, newClr, control);

            control += Time.deltaTime / duration;
            yield return null;
        }

        Destroy(notWalkableNodes[chosenNodeCoords]);
        nodes[chosenNodeCoords.first, chosenNodeCoords.second] = 1;

        notWalkableCoords.Remove(chosenNodeCoords);
        notWalkableNodes.Remove(chosenNodeCoords);

        GameObject newWalk = (GameObject)Instantiate(nodeTypes[nodes[chosenNodeCoords.first, chosenNodeCoords.second]].nodePrefab, new Vector3(chosenNodeCoords.first, 0, chosenNodeCoords.second), Quaternion.identity);

        walkableCoords.Add(chosenNodeCoords);
        walkableNodes.Add(chosenNodeCoords, newWalk);

        graph[chosenNodeCoords.first, chosenNodeCoords.second].g = System.Single.PositiveInfinity;
        graph[chosenNodeCoords.first, chosenNodeCoords.second].rhs = System.Single.PositiveInfinity;
        newRemovedObsticleData = graph[chosenNodeCoords.first, chosenNodeCoords.second];

        updateEdgeCost(newRemovedObsticleData);
        updateKeys();
        waitForChanges = false;
    }

    //D* Lite

    Dictionary<nodesGraph, Pair<float, float>> priorityQ = new Dictionary<nodesGraph, Pair<float, float>>();
    void initialize() {
        foreach (nodesGraph v in graph)
        {
            v.g = System.Single.PositiveInfinity;
            v.rhs = System.Single.PositiveInfinity;
        }
        graph[endCoordX, endCoordY].rhs = 0;
        priorityQ.Add(graph[endCoordX, endCoordY], calcKey(graph[endCoordX, endCoordY]));
    }

    Pair<float,float> calcKey(nodesGraph s) { 
        Pair<float,float> pair = new Pair<float,float>(
            Mathf.Min(s.g, (s.rhs + Mathf.Sqrt((Mathf.Pow(startCoordX - s.x, 2)) + (Mathf.Pow(startCoordY - s.y, 2))))), //first
            Mathf.Min(s.g,s.rhs) //second
        );

        return pair;
    }

    void computeShortestPath() {
        nodesGraph u = new nodesGraph();
        float key1 = System.Single.PositiveInfinity;
        float key2 = System.Single.PositiveInfinity;

        do
        {
            key1 = System.Single.PositiveInfinity;
            key2 = System.Single.PositiveInfinity;

            foreach (var v in priorityQ)
            {
                if (v.Value.first < key1)
                {
                    key1 = v.Value.first;
                    key2 = v.Value.second;
                    u = v.Key;
                }
                else if (v.Value.first == key1)
                {
                    if (v.Value.second < key2)
                    {
                        key2 = v.Value.second;
                        u = v.Key;
                    }
                }
            }

            priorityQ.Remove(u);

            if (u.g > u.rhs)
            {
                u.g = u.rhs;
                foreach (nodesGraph s in u.nodesList)
                {
                    if (nodeTypes[nodes[s.x, s.y]].isWalkable == 0)
                    {
                        continue;
                    }
                    else
                    {
                        updateVertex(s);
                    }
                }
            }
            else
            {
                u.g = System.Single.PositiveInfinity;
                foreach (nodesGraph s in u.nodesList)
                {
                    if (nodeTypes[nodes[s.x, s.y]].isWalkable == 0)
                    {
                        continue;
                    }
                    else
                    {
                        updateVertex(s);
                    }
                }
                updateVertex(u);
            }
        } while (key1 < calcKey(graph[startCoordX, startCoordY]).first
            || key2 < calcKey(graph[startCoordX, startCoordY]).second
            || graph[startCoordX, startCoordY].rhs != graph[startCoordX, startCoordY].g);
    }

    void updateVertex(nodesGraph u)
    {
        int noNeighbours = 0;
        if (u != graph[endCoordX, endCoordY])
        {
            float TMP = System.Single.PositiveInfinity;
            foreach (nodesGraph v in u.nodesList)
            {
                if (nodeTypes[nodes[v.x, v.y]].isWalkable == 0)
                {
                    noNeighbours++;
                    continue;
                }
                else if (v.g <= TMP)
                {
                    u.rhs = v.g + 1;
                    TMP = v.g;
                }
            }
            if (noNeighbours == u.nodesList.Count)
            {
                u.rhs = System.Single.PositiveInfinity;
            }
        }
        if (priorityQ.ContainsKey(u))
        {
            priorityQ.Remove(u);
        }
        if (u.g != u.rhs)
        {
            priorityQ.Add(u,calcKey(u));
        }
    }

    void updateEdgeCost(nodesGraph u) {
        if (nodeTypes[nodes[u.x, u.y]].isWalkable > 0)
        {
            updateVertex(u);
        }

        foreach (nodesGraph v in u.nodesList)
        {
            if (nodeTypes[nodes[v.x, v.y]].isWalkable == 0)
            {
                continue;
            }
            else
            {
                updateVertex(v);
            }
        }
    }

    void updateKeys() {
        List<nodesGraph> coords = new List<nodesGraph>();

        foreach (var s in priorityQ)
        {
            coords.Add(s.Key);
        }

        foreach (var s in coords)
        {
            Pair<float, float> k = calcKey(s);
            if (priorityQ[s] != k)
            {
                priorityQ[s] = k;
            }
        }
    }

    void localStart(nodesGraph s) {
        int localStartX = startCoordX;
        int localStartY = startCoordY;

        float TMPG = s.g;
        foreach (nodesGraph v in s.nodesList)
        {
            if (v.g < TMPG)
            {
                localStartX = v.x;
                localStartY = v.y;
                TMPG = v.g;
            }
        }

        startCoordX = localStartX;
        startCoordY = localStartY;
    }
}
