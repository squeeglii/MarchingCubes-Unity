using System;
using System.Collections.Generic;
using me.cg360.cubes.data;
using UnityEngine;
using Random = UnityEngine.Random;

namespace me.cg360.cubes
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public class CubeTesterMulti : MonoBehaviour
    {

        public Material Material;

        [Range(1, 100)] public int sizeX = 3;
        [Range(1, 100)] public int sizeY = 3;
        [Range(1, 100)] public int sizeZ = 3;

        public int sizeSeed = -1;
        
        private void FixedUpdate()
        {
            int seed = sizeX + (sizeY * 100) + (sizeZ * 10000);
            if(seed == sizeSeed) return;
            sizeSeed = seed;
            
            GridPoint[,,] dataSet = new GridPoint[sizeX + 1, sizeY + 1, sizeZ + 1];

            for (int x = 0; x < sizeX + 1; x++)
            {
                for (int y = 0; y < sizeY + 1; y++)
                {
                    for (int z = 0; z < sizeZ + 1; z++)
                    {
                        dataSet[x, y, z] = (x != 0 && x != sizeX) && (y != 0 && y != sizeY) && (z != 0 && z != sizeZ) 
                            //? new GridPoint { Weight = 1.0f, Color = new Color(((float) x) / (sizeX + 1), ((float) y) / (sizeY + 1), ((float) z) / (sizeZ + 1)) } // Point has mass (Middle)
                            ? new GridPoint { Weight = Random.Range(0.3f, 1.0f), Color = Color.blue } // Point has mass (Middle)
                            : new GridPoint { Weight = 0.0f, Color = Color.black }; // Point has no mass (Edge)
                    }
                }
            }


            int vertCount = 0;
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Color> colors = new List<Color>();

            // Go through cells
            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    for (int z = 0; z < sizeZ; z++)
                    {
                        GridPoint[] dataPoints =
                        {
                            dataSet[x, y, z], dataSet[x, y, z + 1], dataSet[x + 1, y, z + 1], dataSet[x + 1, y, z], // Bottom
                            dataSet[x, y + 1, z], dataSet[x, y + 1, z + 1], dataSet[x + 1, y + 1, z + 1], dataSet[x + 1, y + 1, z] // Top
                        };

                        int index = CubeAssembler.GetCubeIndex(dataPoints, 0.5f);
                        CubeAssembler.CubeReturn meshGen = CubeAssembler.BuildCubeFromIndex(index, 0.5f, dataPoints);
                        Mesh mesh = meshGen.ToSingleMesh();
                        
                        if(mesh == null) continue;

                        int lastVertCount = vertCount;

                        for (int i = 0; i < mesh.vertexCount; i++)
                        {
                            Vector3 vert = mesh.vertices[i];
                            Color color = mesh.colors[i];
                            vert += new Vector3(x, y, z);
                            vertices.Add(vert);
                            colors.Add(color);
                            vertCount += 1;
                        }
                        
                        for (int i = 0; i < mesh.triangles.Length; i++)
                        {
                            triangles.Add(mesh.triangles[i] + lastVertCount);
                        }
                    }
                }
            }

            Mesh newMesh = vertCount == 0 
                ? null
                : new Mesh()
                {
                    vertices = vertices.ToArray(),
                    triangles = triangles.ToArray(),
                    colors = colors.ToArray()
                };
            
            if (newMesh is not null)
            {
                //mesh.RecalculateBounds();
                //mesh.Optimize();
                
                newMesh.bounds.SetMinMax(new Vector3(-0.5f, -0.5f, -0.5f) * 2, new Vector3(0.5f, 0.5f, 0.5f) * 2);
                newMesh.RecalculateNormals();

                GetComponent<MeshRenderer>().material = Material;
                GetComponent<MeshFilter>().mesh = newMesh;
                //GetComponent<MeshCollider>().sharedMesh = mesh;
            }

        }
    }
}