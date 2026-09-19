using System;
using UnityEngine;

namespace me.cg360.cubes
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public class CubeTesterSingle : MonoBehaviour
    {

        public Material Material;

        public bool zero;
        public bool one;
        public bool two;
        public bool three;
        public bool four;
        public bool five;
        public bool six;
        public bool seven;
        
        public int currentVal = -1;
        
        private void FixedUpdate()
        {
            
            int val = 0;
            if (zero) val  |= 1;
            if (one) val   |= 2;
            if (two) val   |= 4;
            if (three) val |= 8;
            if (four) val  |= 16;
            if (five) val  |= 32;
            if (six) val   |= 64;
            if (seven) val |= 128;

            // Detect a change.
            if (currentVal == val) return;
            currentVal = val;
            
            Debug.Log("Redraw Start!");
            CubeAssembler.CubeReturn data = CubeAssembler.BuildCubeFromIndex(val);

            Mesh mesh = data.ToSingleMesh();

            if (mesh is not null)
            {
                //mesh.RecalculateBounds();

                //mesh.Optimize();
                
                mesh.bounds.SetMinMax(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, 0.5f, 0.5f));
                mesh.RecalculateNormals();

                GetComponent<MeshRenderer>().material = Material;
                GetComponent<MeshFilter>().mesh = mesh;
                //GetComponent<MeshCollider>().sharedMesh = mesh;
            }
        }
    }
}