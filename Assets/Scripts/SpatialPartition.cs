using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

public static class SpatialPartition
{
    private static QuadTree RootQuadTree = new QuadTree(0, 0, 1000, 1000, 5);
}

public class QuadTree
{
    public QuadTree(float x, float y, float width, float height, int maxObjects)
    {
        
    }
}
