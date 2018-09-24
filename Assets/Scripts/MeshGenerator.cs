using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour {

    public SquareGrid squareGrid;
    List<Vector3> vertices;
    List<int> triangles;

    Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();
    public void GenerateMesh(int[,] map, float squareSize)
    {

        // We want a grid of squares complete with functioning control nodes and half-way nodes.
        // First make control nodes out of all our map positions 
        //      - Make a 2D array of appropriate size filled with items of the ControlNode type without constructing them - null value
        //      - Invoke ControlNode constructor to fill each coordinate (position, whether or not it's a wall)
        // Then use the nodes to make squares
        //      - Make another 2D array - Squares type, null value for now
        //      - Invoke constructor in loops to set value
        squareGrid = new SquareGrid(map, squareSize);


        // Vertices are, by themselves, just the jumble of points that edges are somehow drawn between
        // Triangles is a list of integers that relates directly to vertices - 
        //      they're index taken three at a time by Unity
        //      that tell Unity which vertices to connect-the-dots with to form triangles.
        // We populate them in the next loops.
        vertices = new List<Vector3>();
        triangles = new List<int>();


        // Squares have a configuration recorded based on which control nodes are active.
        // TriangulateSquare calls MeshFromPoints differently based on this configuration, passing in the active nodes from the square objects as points.
        // MeshFromPoints first calls **AssignVertices** on all these points,
        //      assigning a vertexIndex to the point/node itself based on how many vertices are already in our List<Vector3>,
        //      and adding the Vector3 position of each point to that list.
        // MeshFromPoints then calls **CreateTriangle**,
        //      which accesses each point to grab its newly assigned vertexIndex,
        //      and adds these indexes in the correct order to our triangle list. See the above description of triangles.
        //      It then does things with the dictionary we haven't really gotten to yet
        for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
        {
            for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
            {
                TriangulateSquare(squareGrid.squares[x, y]);
            }
        }

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // Array of coordinate points - (x, y, z) (built from Vector3.position)
        mesh.vertices = vertices.ToArray();

        // Array of integers - Unity strips these out in groups of 3 when it uses them.
        // It takes these groups of three and plays connect-the-dots, in order, between your vertices.
        // These triangles it draws and fills in are only visible from one direction, 
        // so if you have the numbers ordered oddly, you might get upside-down triangles visible only from underneath.
        // I presume this is why clipping inside a wall still lets you see everything behind you instead of a reversed wall.
        mesh.triangles = triangles.ToArray(); 
        //mesh.RecalculateNormals();
    }






    void TriangulateSquare(Square square)
    {
        switch (square.configuration)
        {
            case 0:
                break;

            // 1 point:
            case 1:
                MeshFromPoints(square.centerLeft, square.centerBottom, square.bottomLeft);
                break;
            case 2:
                MeshFromPoints(square.bottomRight, square.centerBottom, square.centerRight);
                break;
            case 4:
                MeshFromPoints(square.topRight, square.centerRight, square.centerTop);
                break;
            case 8:
                MeshFromPoints(square.topLeft, square.centerTop, square.centerLeft);
                break;

            // 2 points:
            case 3:
                MeshFromPoints(square.centerRight, square.bottomRight, square.bottomLeft, square.centerLeft);
                break;
            case 6:
                MeshFromPoints(square.centerTop, square.topRight, square.bottomRight, square.centerBottom);
                break;
            case 9:
                MeshFromPoints(square.topLeft, square.centerTop, square.centerBottom, square.bottomLeft);
                break;
            case 12:
                MeshFromPoints(square.topLeft, square.topRight, square.centerRight, square.centerLeft);
                break;
            case 5:
                MeshFromPoints(square.centerTop, square.topRight, square.centerRight, square.centerBottom, square.bottomLeft, square.centerLeft);
                break;
            case 10:
                MeshFromPoints(square.topLeft, square.centerTop, square.centerRight, square.bottomRight, square.centerBottom, square.centerLeft);
                break;

            // 3 point:
            case 7:
                MeshFromPoints(square.centerTop, square.topRight, square.bottomRight, square.bottomLeft, square.centerLeft);
                break;
            case 11:
                MeshFromPoints(square.topLeft, square.centerTop, square.centerRight, square.bottomRight, square.bottomLeft);
                break;
            case 13:
                MeshFromPoints(square.topLeft, square.topRight, square.centerRight, square.centerBottom, square.bottomLeft);
                break;
            case 14:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centerBottom, square.centerLeft);
                break;

            // 4 points:
            case 15:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
                break;


        }
    }

    // "params" keyword means our quantity of arguments is unknown
    void MeshFromPoints(params Node[] points)
    {
        AssignVertices(points);   

        if (points.Length >= 3)
        {
            CreateTriangle(points[0], points[1], points[2]);
        }
        if (points.Length >= 4)
        {
            CreateTriangle(points[0], points[2], points[3]);
        }
        if (points.Length >= 5)
        {
            CreateTriangle(points[0], points[3], points[4]);
        }
        if (points.Length >= 6)
        {
            CreateTriangle(points[0], points[4], points[5]);
        }
    }

    void AssignVertices(Node[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].vertexIndex == -1)
            {
                points[i].vertexIndex = vertices.Count;
                vertices.Add(points[i].position);
            }
        }
    }

    void CreateTriangle(Node a, Node b, Node c)
    {
        triangles.Add(a.vertexIndex);
        triangles.Add(b.vertexIndex);
        triangles.Add(c.vertexIndex);

        Triangle triangle = new Triangle(a.vertexIndex, b.vertexIndex, c.vertexIndex);
        AddTriangleToDictionary(triangle.vertexIndexA, triangle);
        AddTriangleToDictionary(triangle.vertexIndexB, triangle);
        AddTriangleToDictionary(triangle.vertexIndexC, triangle);

    }

    void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle)
    {
        if (triangleDictionary.ContainsKey(vertexIndexKey))
        {
            triangleDictionary[vertexIndexKey].Add(triangle);
        }
        else
        {
            List<Triangle> triangleList = new List<Triangle>();
            triangleList.Add(triangle);
            triangleDictionary.Add(vertexIndexKey, triangleList);
        }
    }

    //bool IsOutlineEdge(int)

    struct Triangle
    {
        public int vertexIndexA;
        public int vertexIndexB;
        public int vertexIndexC;

        public Triangle(int a, int b, int c)
        {
            vertexIndexA = a;
            vertexIndexB = b;
            vertexIndexC = c;
        }
    }




    
    public class SquareGrid
    {
        public Square[,] squares;

        public SquareGrid(int[,] map, float squareSize)
        {
            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1);
            float mapWidth = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;

            // This is not invoking ControlNode constructor, I think. 
            // Just making a 2D array that's filled with unctonstructed objects of the type?
            // Seems the value is null. But it's a ControlNode type null. Right type, value to be assigned in the following loop.
            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];
            //Debug.Log(controlNodes[1, 2]);
            for (int x = 0; x < nodeCountX; x++)
            {
                for (int y = 0; y < nodeCountY; y++)
                {
                    Vector3 pos = new Vector3(-mapWidth/2 + x * squareSize + squareSize/2, 0, -mapHeight/2 + y * squareSize + squareSize/2);
                    controlNodes[x, y] = new ControlNode(pos, map[x, y] == 1, squareSize);

                }
            }

            squares = new Square[nodeCountX - 1, nodeCountY - 1];

            for (int x = 0; x < nodeCountX-1; x++)
            {
                for (int y = 0; y < nodeCountY-1; y++)
                {
                    squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);

                    // Why not this version? Aren't we starting in the top left?
                    //squares[x, y] = new Square(controlNodes[x, y], controlNodes[x + 1, y], controlNodes[x + 1, y + 1], controlNodes[x, y + 1]);

                    // Yes. We are. But You're coming from Javascript canvas where (0, 0) is the top left like how books read and positive y moves down.
                    // It was a bit weird for you there but positive y is up here. Unity is taking positive y as up, I think. Maybe C# does too?
                    // ...seems like most video/screen APIs do it the way Javascript's canvas does? Just roll with whatever your tools do I suppose.
                    
                }
            }
        }
    }

    public class Square
    {
        public ControlNode topLeft, topRight, bottomRight, bottomLeft;
        public Node centerTop, centerRight, centerBottom, centerLeft;
        public int configuration;


        public Square(ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomRight, ControlNode _bottomLeft)
        {
            topLeft = _topLeft;
            topRight = _topRight;
            bottomRight = _bottomRight;
            bottomLeft = _bottomLeft;

            centerTop = topLeft.right;
            centerRight = bottomRight.above;
            centerBottom = bottomLeft.right;
            centerLeft = bottomLeft.above;

            if (topLeft.active)
                configuration += 8;
            if (topRight.active)
                configuration += 4;
            if (bottomRight.active)
                configuration += 2;
            if (bottomLeft.active)
                configuration += 1;
        }
    }


    // A thing in a spot. Just a vector position. To be used later.
	public class Node
    {
        public Vector3 position;
        public int vertexIndex = -1;

        public Node(Vector3 _pos)
        {
            position = _pos;
        }
    }

    // A thing in a spot, that also toggles active.
    // Expected: toggling active triggers the regular nodes it controls in mesh building.
    public class ControlNode : Node
    {
        public bool active;
        public Node above, right;

        public ControlNode(Vector3 _pos, bool _active, float squareSize) : base(_pos)
        {
            active = _active;
            above = new Node(position + Vector3.forward * squareSize / 2f);
            right = new Node(position + Vector3.right * squareSize / 2f);
        }
    }
}
