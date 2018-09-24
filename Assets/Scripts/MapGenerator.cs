using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MapGenerator : MonoBehaviour {

    // 1) Generate random mess of coordinate points
    // 2) Apply the cellular automata smoothing thing (reassign based on neighboring wall count)
    // 3) Begin treating coordinates as the corners of squares, define them as control nodes (with non-control nodes defined half way between these)
    // 4) Define square objects out of these nodes - each control node is in charge of the half-way node to its upward and rightward position
    // 5) Form a full grid from these squares
    // 6) ???? witchcraft draws a mesh out of magic triangles

    public int width;
    public int height;

    public string seed;
    public bool useRandomSeed;

    [Range(0, 100)]
    public int randomFillPercent;

    int[,] map;


    private void Start()
    {
        GenerateMap();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GenerateMap();
            Debug.Log("Generating New");
        }
    }



    void GenerateMap()
    {
        map = new int[width, height];
        RandomFillMap();

        for (int i = 0; i < 5; i++)
        {
            SmoothMap();
        }

        int borderSize = 5;
        int[,] borderedMap = new int[width + borderSize*2, height + borderSize*2];

        for (int x = 0; x < borderedMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize)
                {
                    borderedMap[x, y] = map[x - borderSize, y - borderSize];
                }
                else
                {
                    borderedMap[x, y] = 1;
                }
            }
        }

        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen.GenerateMesh(borderedMap, 1);
    }



    void RandomFillMap()
    {
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }

        System.Random pseudoRandGen = new System.Random(seed.GetHashCode());

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // We added a border generation bit later, so this isn't necessary nymore.
                //if (x == 0 || x == width-1 || y == 0 || y == height - 1)
                //{
                //    map[x, y] = 1;
                //}
                //else
                //{
                map[x, y] = (pseudoRandGen.Next(0, 100) <= randomFillPercent) ? 1 : 0;
                //}

            }
        }
    }

    void SmoothMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighborWallTiles = GetSurroundingWallCount(x, y);

                if (neighborWallTiles > 4)
                {
                    map[x, y] = 1;
                }
                if (neighborWallTiles < 4)
                {
                    map[x, y] = 0;
                }
                else if (neighborWallTiles == 4)
                {
                    // Remain as is.
                }
            }
        }
    }

    int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;

        // Check within 3x3 grid around the point passed in
        for (int neighborX = gridX - 1; neighborX <= gridX + 1; neighborX++)
        {
            for (int neighborY = gridY - 1; neighborY <= gridY + 1; neighborY++)
            {
                // Check we're not off the edges of the map
                if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height)
                {
                    // Check we're not examining the point in args
                    if (neighborX != gridX || neighborY != gridY)
                    {
                        // This will be 0 or 1
                        wallCount += map[neighborX, neighborY];
                    }
                }
                // We're looking off the map. But encouraging walls is desireable
                else
                {
                    wallCount++;
                }
            }
        }

        return wallCount;
    }

    //private void OnDrawGizmos()
    //{
    //    if (map!= null)
    //    {
    //        for (int x = 0; x < width; x++)
    //        {
    //            for (int y = 0; y < height; y++)
    //            {
    //                Gizmos.color = (map[x,y] == 1) ? Color.gray : Color.white;
    //                Vector3 pos = new Vector3(-width / 2 + x + .5f, 0,  -height + y + .5f);
    //                Gizmos.DrawCube(pos, Vector3.one);
    //            }
    //        }
    //    }
    //}

}
