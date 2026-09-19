using System;
using System.Collections.Generic;
using me.cg360.cubes.data;
using UnityEngine;

namespace me.cg360.cubes
{
    public class CubeAssembler
    {

        public static int GetCubeIndex(GridPoint[] data, float threshold)
        {
            int val = 0;

            for (int i = 0; i < 8; i++)
            {
                if (CorrectCubeWeightPoint(data[i].Weight) >= threshold)
                {
                    int mask = (int) Math.Pow(2, i);
                    val |= mask;
                }
            }

            return val;
        }

        public static float CorrectCubeWeightPoint(float data)
        {
            return (-data) + 1f;  //-Math.Abs(data);
        }
        
        //TODO: Add a "neighbours" handling method which allows existing vertices to be used and swapped in.
        public static CubeReturn BuildCubeFromIndex(int lookupIndex, float threshold = 0.5f, GridPoint[] points = null)
        {
            int encodedEdges = VertexReference.EdgeTable[lookupIndex];
            
            if (encodedEdges == 0) return new CubeReturn
            {
                Triangles = Array.Empty<ReferenceTriangle>(),
                Vertices = Array.Empty<Vertex?>(),
                UniqueVertexCount = 0
            };
            
            Vertex?[] vertices = new Vertex?[12];
            int uniqueVertexCount = 0;

            for (int p = 0; p < 12; p++)
            {
                int mask = (int) Math.Pow(2, p);
                if ((encodedEdges & mask) == 0) continue;
                
                vertices[p] = ParseVertex(p, threshold, points);
                uniqueVertexCount += 1;
            }

            List<ReferenceTriangle> tris = new List<ReferenceTriangle>();
            ReferenceTriangle currentTri = new ReferenceTriangle
            {
                Vertices = new int[3]
            };
            
            for (int i = 0; i < 12; i++)
            {
                sbyte vertex = VertexReference.TriangleTable[lookupIndex, i];
                if(vertex < 0) break;

                currentTri.Vertices[i % 3] = vertex;
                if ((i % 3) == 2)
                {
                    tris.Add(currentTri);
                    currentTri = new ReferenceTriangle
                    {
                        Vertices = new int[3]
                    };
                }
            }

            return new CubeReturn()
            {
                Triangles = tris.ToArray(),
                Vertices = vertices,
                UniqueVertexCount = uniqueVertexCount
            };
        }


        //TODO: Pass through weights
        private static Vertex ParseVertex(int edge, float weightThreshold, GridPoint[] data = null)
        {
            int vertexIndex1 = VertexReference.EdgeVertices[edge, 0];
            int vertexIndex2 = VertexReference.EdgeVertices[edge, 1];

            Vector3 vPos1 = VertexReference.CornerPoints[vertexIndex1];
            Vector3 vPos2 = VertexReference.CornerPoints[vertexIndex2];

            Vector3 position;
            Color color;
            
            if (data is null)
            {
                position = Vector3.Lerp(vPos1, vPos2, 0.5f);
                color = Color.white;
            }
            else
            {
                float weight1 = CorrectCubeWeightPoint(data[vertexIndex1].Weight);
                float weight2 = CorrectCubeWeightPoint(data[vertexIndex2].Weight);
                
                
                float t = (weightThreshold - data[vertexIndex1].Weight) / (data[vertexIndex2].Weight - data[vertexIndex1].Weight);
                position = vPos1 + (t * (vPos2 - vPos1));

                //Debug.Log($"Weights: {weight1}, {weight2}");
                
                if (weight1 == 0.0f) color = data[vertexIndex1].Color;
                else if (weight2 == 0.0f) color = data[vertexIndex2].Color;
                else color = Color.Lerp(data[vertexIndex1].Color, data[vertexIndex2].Color, t);
            }
            
            return new Vertex()
            {
                Position = position,
                Color = color
            };
        }
        
        
        public struct CubeReturn
        {
            public ReferenceTriangle[] Triangles;
            public Vertex?[] Vertices;
            public int UniqueVertexCount;

            public Mesh ToSingleMesh()
            {
                if (UniqueVertexCount == 0) return null;
                
                //Debug.Log($"Unique Verts: {UniqueVertexCount}, Tris: {Triangles.Length}");
                Vector3[] verts = new Vector3[UniqueVertexCount];
                Color[] colours = new Color[UniqueVertexCount];
                int[] triangles = new int[Triangles.Length * 3];

                Dictionary<int, int> remapper = new Dictionary<int, int>();

                int currentVertex = 0;
                
                for (int i = 0; i < 12; i++)
                {
                    Vertex? v = Vertices[i];
                    if (v.HasValue)
                    {
                        remapper[i] = currentVertex;
                        colours[currentVertex] = v.Value.Color;
                        verts[currentVertex] = v.Value.Position;
                        //Debug.Log($"Vertex: {currentVertex} @ {v.Value.Position}");
                        currentVertex++;
                    }
                }

                for (int triIndex = 0; triIndex < Triangles.Length; triIndex++)
                {
                    ReferenceTriangle tri = Triangles[triIndex];
                    for (int i = 0; i < 3; i++)
                        triangles[(triIndex * 3) + i] = remapper[tri.Vertices[i]];
                    //Debug.Log($"Tri: {triangles[(triIndex * 3) + 0]}, {triangles[(triIndex * 3) + 1]}, {triangles[(triIndex * 3) + 2]}");
                }

                return new Mesh
                {
                    vertices = verts,
                    triangles = triangles,
                    colors = colours,
                };
            }
        }

    }
}