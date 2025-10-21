using UnityEngine;

namespace ProceduralAnimation.Runtime.Dynamics {
    public class SecondOrderDynamics {
        float k1, k2, k3;
        Vector3 xp; //  Previous value of x
        Vector3 y, yd; //   State variables -> y and its derivative

        /// <summary>
        /// A system to interpolate values procedurally given some variables.
        /// </summary>
        /// <param name="f">How quick the system responds to changes.</param>
        /// <param name="z">The damping of the system. If 1 > z > 0, the system can overshoot. If z > 1, it will not overshoot.</param>
        /// <param name="r">The initial response of the system. If r is negative, the system will try and anticipate movements. If r is positive, it will overshoot then compensate.</param>
        /// <param name="x0">The starting vector of the system.</param>
        public SecondOrderDynamics(float f, float z, float r, Vector3 x0) {
            ComputeKValues(f, z, r);

            //  Initialize variables
            xp = x0;
            y = x0;
            yd = Vector3.zero;
        }

        /// <summary>
        /// A system to interpolate values procedurally given some variables.
        /// </summary>
        /// <param name="settings">Scriptable Object containing the variables needed to setup the class.</param>
        /// <param name="x0">The starting vector of the system.</param>
        public SecondOrderDynamics(SecondOrderSettings settings, Vector3 x0) : this(settings.f, settings.z, settings.r, x0) { }

        /// <summary>
        /// Computes the internal coefficients to be used in the calculations.
        /// </summary>
        /// <param name="f">How quick the system responds to changes.</param>
        /// <param name="z">The damping of the system. If 1 > z > 0, the system can overshoot, if z > 1, it will not overshoot.</param>
        /// <param name="r">The initial response of the system, if r is negative, the system will try and anticipate movements. If r is positive, it will overshoot then compensate.</param>
        public void ComputeKValues(float f, float z, float r) {
            //  Prevent the system from going haywire
            f = Mathf.Min(f, 0.001f);
            z = Mathf.Min(z, 0.001f);

            //  Compute k's
            k1 = z / (Mathf.PI * f);
            k2 = 1 / Mathf.Pow(2 * Mathf.PI * f, 2);
            k3 = r * z / (2 * Mathf.PI * f);
        }

        /// <summary>
        /// Updates the system with the new vector and the time step.
        /// </summary>
        /// <param name="T">Time since last frame.</param>
        /// <param name="x">The current value of the starting vector.</param>
        /// <returns>The interpolated position.</returns>
        public Vector3 Update(float T, Vector3 x) {
            return Update(T, x, (x - xp) / T);
        }

        /// <summary>
        /// Overload method where the velocity is known.
        /// </summary>
        /// <param name="T">Time since last frame.</param>
        /// <param name="x">The current value of the starting vector.</param>
        /// <param name="xd">The velocity of the vector from last frame to this frame.</param>
        /// <returns>The interpolated position</returns>
        public Vector3 Update(float T, Vector3 x, Vector3 xd) {
            //   Clamp to stabilize system and prevent jitter
            float k2_stable = Mathf.Max(k2, T * T / 2 + T * k1 / 2, T * k1);
            xp = x;
            y = y + T * yd;
            yd += T * (x + k3 * xd - y - k1 * yd) / k2_stable;
            return y;
        }
    }
}