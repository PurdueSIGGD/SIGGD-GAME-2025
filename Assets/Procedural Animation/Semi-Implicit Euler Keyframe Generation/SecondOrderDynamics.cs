using UnityEngine;

public class SecondOrderDynamics {
    float k1, k2, k3;
    Vector3 xp; // x prev
    Vector3 y, yd; // state variables -> y and its derivative

    // ---- PARAMETER EXPLANATIONS ----
    // f -> the frequency of which the damping occurs
    // z -> how much its damped, if 1 > z > 0, the function will eventually settle, if z > 1, it will not overshoot at all
    // r -> how long it takes to respond, if r > 0, the function take a little before reacting, if r < 0, the function will anticipate the movement 
    public SecondOrderDynamics(float f, float z, float r, Vector3 x0) {
        ComputerKValues(f, z, r);

        // initialize vars
        xp = x0;
        y = x0;
        yd = Vector3.zero;
    }

    public void ComputerKValues(float f, float z, float r) {
        // compute k's
        k1 = z / (Mathf.PI * f);
        k2 = 1 / Mathf.Pow(2 * Mathf.PI * f, 2);
        k3 = r * z / (2 * Mathf.PI * f);
    }

    public Vector3 Update(float T, Vector3 x) {
        return Update(T, x, (x - xp) / T);
    }

    public Vector3 Update(float T, Vector3 x, Vector3 xd) {
        xp = x;
        y = y + T * yd;
        yd += T * (x + k3 * xd - y - k1 * yd) / k2;
        return y;
    }
}