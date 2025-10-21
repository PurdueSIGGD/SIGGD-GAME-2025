using UnityEngine;
using UnityEditor;
using ProceduralAnimation.Runtime;

namespace ProceduralAnimation.Editor {
    public static class ProcAnimEditorUtils {
        /// <summary>
        /// Generate a colour with all channels the same.
        /// </summary>
        /// <param name="v">The brightness.</param>
        /// <param name="a">The transparency</param>
        /// <returns>A greyscale colour.</returns>
        public static Color Grey(float v, float a = 255f) {
            Color grey = Color.white * v / 255f;
            grey.a = a;
            return grey;
        }

        /// <summary>
        /// Make points be inside of the specified rect.
        /// </summary>
        /// <param name="pos">Position that needs to be fit.</param>
        /// <param name="rect">The rect that contains the point.</param>
        /// <param name="min">The minimum x value.</param>
        /// <param name="max">The maxmimum x value.</param>
        /// <param name="yOffset">Offset on y axis independent of scaling.</param>
        /// <returns>Position within rect.</returns>
        public static Vector2 FitInRect(Vector2 pos, Rect rect, float min, float max, float yOffset = 0) {
            pos.x *= rect.width / (max - min);
            pos.y *= -rect.width / (max - min);

            pos.x += rect.xMin;
            pos.y += rect.yMax;

            pos.y += yOffset;

            if (pos.y > rect.yMin && pos.y < rect.yMax)
                return pos;
            return new Vector3(float.NaN, float.NaN); //    Make a break in the graph
        }

        // The following functions are used for SecondOrderDynamics related Editor scripts
        public class SecondOrderFloatSystem {
            float k1, k2, k3;
            float xp, x0, y, yd;

            /// <summary>
            /// A system to interpolate values procedurally given some variables.
            /// </summary>
            /// <param name="f">How quick the system responds to changes.</param>
            /// <param name="z">The damping of the system. If 1 > z > 0, the system can overshoot. If z > 1, it will not overshoot.</param>
            /// <param name="r">The initial response of the system. If r is negative, the system will try and anticipate movements. If r is positive, it will overshoot then compensate.</param>
            /// <param name="x0">The starting vector of the system.</param>
            public SecondOrderFloatSystem(float f, float z, float r, float _x0) {
                ComputeKValues(f, z, r);

                x0 = _x0;

                xp = x0;
                y = x0;
                yd = 0;
            }

            /// <summary>
            /// A system to interpolate values procedurally given some variables.
            /// </summary>
            /// <param name="settings">Scriptable Object containing the variables needed to setup the class.</param>
            /// <param name="x0">The starting vector of the system.</param>
            public SecondOrderFloatSystem(SecondOrderSettings settings, float _x0) : this(settings.f, settings.z, settings.r, _x0) { }

            /// <summary>
            /// Computes the internal coefficients to be used in the calculations.
            /// </summary>
            /// <param name="f">How quick the system responds to changes.</param>
            /// <param name="z">The damping of the system. If 1 > z > 0, the system can overshoot, if z > 1, it will not overshoot.</param>
            /// <param name="r">The initial response of the system, if r is negative, the system will try and anticipate movements. If r is positive, it will overshoot then compensate.</param>
            public void ComputeKValues(float f, float z, float r) {
                f = Mathf.Max(f, 0.001f);
                z = Mathf.Max(z, 0.001f);

                k1 = z / (Mathf.PI * f);
                k2 = 1 / Mathf.Pow(2 * Mathf.PI * f, 2);
                k3 = r * z / (2 * Mathf.PI * f);
            }

            /// <summary>
            /// Updates the system using the calculated values.
            /// </summary>
            /// <param name="T">Time step, calculated using assumed time elapsed and steps.</param>
            /// <param name="x">The float value of whats to be interpolated.</param>
            /// <param name="xd">The velocity of whats to be interpolated.</param>
            /// <returns></returns>
            public float Update(float T, float x) {
                float k2_stable = Mathf.Max(k2, T * T / 2 + T * k1 / 2, T * k1);
                float xd = (x - xp) / T;
                xp = x;
                y = y + T * yd;
                yd += T * (x + k3 * xd - y - k1 * yd) / k2_stable;
                return y;
            }

        }

        /// <summary>
        /// The function to demonstrate the system on.
        /// </summary>
        /// <param name="x">The input, should be 0 < x < 1000.</param>
        /// <returns>The y value given x.</returns>
        public static float F(float x) {
            if (x < 100)
                return 0;
            else if (x < 150)
                return x - 100;
            else if (x < 200)
                return 50;
            else if (x < 210)
                return -10 * x + 2050;
            else if (x < 250)
                return -50;
            else if (x < 400)
                return 2f / 5f * x - 150f;
            else if (x < 500)
                return 10;
            else if (x < 570)
                return Mathf.Pow((x - 500) / 10, 2) + 10f;
            else if (x < 650)
                return 59;
            else if (x < 177f / 2f * Mathf.PI + 650)
                return 59 * Mathf.Cos((x - 650) / 59);
            else if (x < 197f / 2f * Mathf.PI + 650)
                return 10 * Mathf.Sin(0.1f * (x - 177f / 2f * Mathf.PI - 650));
            else
                return 0;
        }
    }
}