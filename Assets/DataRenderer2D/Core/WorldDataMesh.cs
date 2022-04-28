using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace geniikw.DataRenderer2D
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public abstract class WorldDataMesh : MonoBehaviour
    {
        public bool updateInUpdate = true;
        public bool updateInStart = false;

        IEnumerable<IMesh> _mesh;

        IEnumerable<IMesh> Mesh { get { return _mesh ?? (_mesh = MeshFactory); } }

        int bufferSize = 100;
        Vector3[] vBuffer;
        Vector2[] uvBuffer;
        Color[] colorBuffer;
        int[] tBuffer;

        bool m_geometryUpdateFlag = false;

        abstract protected IEnumerable<IMesh> MeshFactory { get; }

        MeshFilter m_mf;
        MeshFilter Mf
        {
            get
            {
                return m_mf ?? (m_mf = GetComponent<MeshFilter>());
            }
        }

        protected virtual void Awake()
        {
            AllocateBuffer(bufferSize);
            Reset();
        }

        public void Start() 
        {
            if (updateInStart) GeometyUpdateFlagUp();
        }

        public void Reset()
        {
            var mf = GetComponent<MeshFilter>();

            if (mf.sharedMesh == null)
            {
                MakeNewMesh();
            }
        }

        private void Update()
        {
            if (updateInUpdate) GeometyUpdateFlagUp();
        }
        
        public void LateUpdate()
        {
            if (m_geometryUpdateFlag)
            {
                UpdateGeometry();
                m_geometryUpdateFlag = false;
            }
        }

        public void MakeNewMesh()
        {
            var mf = GetComponent<MeshFilter>();

            var mesh = new Mesh();
            mesh.name = name;
            mf.sharedMesh = mesh;
        }


        public void UpdateGeometry()
        {
            var mf = GetComponent<MeshFilter>();

            mf.sharedMesh.Clear();

            var vc = 0;
            var tc = 0;

            foreach (var mesh in Mesh)
            {
                foreach (var t in mesh.Triangles)
                {
                    if (tc > tBuffer.Length - 1)
                        ExtendBuffer();

                    tBuffer[tc] = vc + t;
                    tc++;
                }

                foreach (var v in mesh.Vertices)
                {
                    if (vc > vBuffer.Length - 1)
                        ExtendBuffer();

                    vBuffer[vc] = v.position;
                    uvBuffer[vc] = v.uv;
                    colorBuffer[vc] = v.color;
                    vc++;
                }
            }

            while (vc < vBuffer.Length)
            {
                vBuffer[vc] = vBuffer[0];// Vector3.zero; - breaks collider bounds
                uvBuffer[vc] = Vector2.zero;
                colorBuffer[vc] = Color.white;
                vc++;
            }
            while (tc < vBuffer.Length)
            {
                tBuffer[tc] = 0;
                tc++;
            }

            mf.sharedMesh.vertices = vBuffer;

            mf.sharedMesh.uv = uvBuffer;
            mf.sharedMesh.triangles = tBuffer;
            mf.sharedMesh.colors = colorBuffer;
            mf.sharedMesh.RecalculateNormals();
            mf.sharedMesh.RecalculateTangents();
            mf.sharedMesh.RecalculateBounds();

            var collider = GetComponent<MeshCollider>();
            if (collider != null) collider.sharedMesh = mf.sharedMesh;
        }
        
        private void AllocateBuffer(int size)
        {
            vBuffer = new Vector3[size];
            uvBuffer = new Vector2[size];
            colorBuffer = new Color[size];
            tBuffer = new int[size * 6];
        }
        
        private void ExtendBuffer()
        {
            var tempV = vBuffer;
            var tempUV = uvBuffer;
            var tempC = colorBuffer;
            var tempT = tBuffer;
            //Debug.Log("Size double up : " + bufferSize);
            bufferSize *= 2;
            AllocateBuffer(bufferSize);

            Array.Copy(tempV, vBuffer, tempV.Length);
            Array.Copy(tempUV, uvBuffer, tempUV.Length);
            Array.Copy(tempC, colorBuffer, tempC.Length);
            Array.Copy(tempT, tBuffer, tempT.Length);
        }

        
        public void GeometyUpdateFlagUp()
        {
            m_geometryUpdateFlag = true;
        }


    }
}