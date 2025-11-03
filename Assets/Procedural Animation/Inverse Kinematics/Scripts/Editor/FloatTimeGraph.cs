using System.Collections.Generic;
using UnityEngine;
using System;

namespace ProceduralAnimation.Editor {
    public class FloatTimeGraph {
        public Func<float> getFunc;
        List<Vector2> points;
        float creationTime;
        public FloatTimeGraph(Func<float> _getFunc, float _t) {
            points = new List<Vector2>();
            getFunc = _getFunc;
            creationTime = _t;
        }

        /// <summary>
        /// Adds a new point given Time.time
        /// </summary>
        /// <param name="t">Time.time passed through any type of loop.</param>
        public void Update(float t) {
            points.Add(new Vector2(t, getFunc()));
        }

        /// <summary>
        /// Fit all the x values between [0, 1]
        /// </summary>
        /// <param name="endTime">End graph time.</param>
        // public void NormalizePoints(float endTime) {
        //     // float maxY = ;

        //     for (int i = 0; i < points.Count; i++) {
        //         float normX = (points[i].x - creationTime) / (endTime - creationTime);
        //         float normY = points[i].y / maxY;

        //         points[i] = new Vector2(normX, normY);
        //     }
        // }


    }
}
