using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System.Runtime.CompilerServices;
using System.Threading;
using Autodesk.Fbx;
using System.Runtime.InteropServices;

namespace SIGGD.Mobs
{
    public class TerritorialBehaviour : MonoBehaviour
    {
        public static float EPSILON = 0.0001f;
        LinkedList<Vec> vecs;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
        struct Vec
        {
            public float xVal;
            public float yVal;
            public float xVec;
            public float yVec;
        }
        public static void CreateTerritory()
        {
           
        }
        public bool IsInTerritory()
        {
            float agentX = transform.position.x;
            float agentY = transform.position.y;
            var vecNode = vecs.First;
            int xCount = 0;
            int yCount = 0;
            while (vecNode != null) 
            {
                if (PlugInX(agentX, vecNode.Value))
                {
                    xCount++;
                }
                if (PlugInY(agentY, vecNode.Value))
                {
                    yCount++;
                }
                vecNode = vecNode.Next;
            }
            if (xCount % 2 == 0 && yCount % 2 == 0)
            {
                return true;
            }
            return false;
        }
        private bool PlugInX(float agentX, Vec vec) {
            float t = (agentX - vec.xVal) / vec.xVec;
            if (t - EPSILON < 1f && t + EPSILON > 0) {
                return true;
            }
            return false;
        }
        private bool PlugInY(float agentY, Vec vec)
        {
            float t = (agentY - vec.yVal) / vec.yVec;
            if (t - EPSILON < 1f && t + EPSILON > 0)
            {
                return true;
            }
            return false;
        }
    }
}