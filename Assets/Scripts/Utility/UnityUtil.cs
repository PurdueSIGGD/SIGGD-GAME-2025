using UnityEngine;

namespace Utility
{
    /**
     * A static utility class containing extension methods for common Unity-related functions.
     */
    public static class UnityUtil
    {

        #region Vector3 Extensions

        public static Vector3 SetX(this Vector3 v, float x) => new Vector3(x, v.y, v.z);

        public static Vector3 SetY(this Vector3 v, float y) => new Vector3(v.x, y, v.z);

        public static Vector3 SetZ(this Vector3 v, float z) => new Vector3(v.x, v.y, z);


        /**
         * <summary>
         * Converts a Vector3 to a 2D vector by zeroing out one of its components.
         * </summary>
         *
         * <param name="v">The original Vector3.</param>
         * <param name="ignore">The component to ignore ('x', 'y', or 'z'). Defaults to 'y'.</param>
         * <returns>A new Vector3 with the specified component set to zero.</returns>
         */
        public static Vector2 ToVector2(this Vector3 v, char ignore = 'y')
        {
            return ignore switch
            {
                'x' => new Vector2(v.y, v.z),
                'y' => new Vector2(v.x, v.z),
                'z' => new Vector2(v.x, v.y),
                _ => v
            };
        }


        /**
         * <summary>
         * Converts a Vector2 to a 3D vector by adding a zero component.
         * </summary>
         *
         * <param name="v">The original Vector2.</param>
         * <param name="ignore">The component to set to zero ('x', 'y', or 'z'). Defaults to 'y'.</param>
         * <returns>A new Vector3 with the specified component set to zero.</returns>
         */
        public static Vector3 ToVector3(this Vector2 v, char ignore = 'y')
        {
            return ignore switch
            {
                'x' => new Vector3(0, v.x, v.y),
                'y' => new Vector3(v.x, 0, v.y),
                'z' => new Vector3(v.x, v.y, 0),
                _ => v
            };
        }

        #endregion

        // Damping methods that use exponential decay to smoothly interpolate between values
        // (please never try to lerp with deltaTime directly it will not be frame rate independent)

        #region Damping

        /**
         *
         * <summary>
         * Damps a float value towards a target value using exponential decay.
         * </summary>
         *
         * <param name="a">The current value.</param>
         * <param name="b">The target value.</param>
         * <param name="damping">The damping factor (higher values result in faster damping).</param>
         * <param name="dt">The delta time (time since last frame).</param>
         */
        public static float Damp(float a, float b, float damping, float dt)
        {
            return Mathf.Lerp(a, b, BaseDamp(damping, dt));
        }


        /**
         *
         * <summary>
         * Damps an angle value towards a target angle using exponential decay, correctly handling wrap-around.
         * </summary>
         *
         * <param name="a">The current angle in degrees.</param>
         * <param name="b">The target angle in degrees.</param>
         * <param name="damping">The damping factor (higher values result in faster damping).</param>
         * <param name="dt">The delta time (time since last frame).</param>
         */
        public static float DampAngle(float a, float b, float damping, float dt)
        {
            return Mathf.LerpAngle(a, b, BaseDamp(damping, dt));
        }


        /**
         *
         * <summary>
         * Damps a quaternion value towards a target quaternion using spherical linear interpolation (slerp).
         * </summary>
         *
         * <param name="a">The current quaternion.</param>
         * <param name="b">The target quaternion.</param>
         * <param name="damping">The damping factor (higher values result in faster damping).</param>
         * <param name="dt">The delta time (time since last frame).</param>
         */
        public static Quaternion DampQuaternion(Quaternion a, Quaternion b, float damping, float dt)
        {
            return Quaternion.Slerp(a, b, BaseDamp(damping, dt));
        }

        /**
         *
         * <summary>
         * Damps a Vector2 value towards a target Vector2 using linear interpolation (lerp).
         * </summary>
         *
         * <param name="a">The current Vector2.</param>
         * <param name="b">The target Vector2.</param>
         * <param name="damping">The damping factor (higher values result in faster damping).</param>
         * <param name="dt">The delta time (time since last frame).</param>
         */
        public static Vector2 DampVector2(Vector2 a, Vector2 b, float damping, float dt)
        {
            return Vector2.Lerp(a, b, BaseDamp(damping, dt));
        }

        /**
         *
         * <summary>
         * Damps a Vector2 value towards a target Vector2 using linear interpolation (lerp) for each component separately.
         * </summary>
         *
         * <param name="a">The current Vector2.</param>
         * <param name="b">The target Vector2.</param>
         * <param name="damping">A Vector2 containing the damping factors for each component (higher values result in faster damping).</param>
         * <param name="dt">The delta time (time since last frame).</param>
         */
        public static Vector2 DampVector2(Vector2 a, Vector2 b, Vector2 damping, float dt)
        {
            return new Vector2(Damp(a.x, b.x, damping.x, dt), Damp(a.y, b.y, damping.y, dt));
        }

        /**
         *
         * <summary>
         * Damps a Vector3 value towards a target Vector3 using linear interpolation (lerp).
         * </summary>
         *
         * <param name="a">The current Vector3.</param>
         * <param name="b">The target Vector3.</param>
         * <param name="damping">The damping factor (higher values result in faster damping).</param>
         * <param name="dt">The delta time (time since last frame).</param>
         */
        public static Vector3 DampVector3(Vector3 a, Vector3 b, float damping, float dt)
        {
            return Vector3.Lerp(a, b, BaseDamp(damping, dt));
        }

        /**
         *
         * <summary>
         * Damps a Vector3 value towards a target Vector3 using linear interpolation (lerp) for each component separately.
         * </summary>
         *
         * <param name="a">The current Vector3.</param>
         * <param name="b">The target Vector3.</param>
         * <param name="damping">A Vector3 containing the damping factors for each component (higher values result in faster damping).</param>
         * <param name="dt">The delta time (time since last frame).</param>
         */
        public static Vector3 DampVector3(Vector3 a, Vector3 b, Vector3 damping, float dt)
        {
            return new Vector3(
                Damp(a.x, b.x, damping.x, dt),
                Damp(a.y, b.y, damping.y, dt),
                Damp(a.z, b.z, damping.z, dt)
            );
        }

        /**
         *
         * <summary>
         * Calculates the base damping factor using exponential decay.
         * </summary>
         *
         * <param name="damping">The damping factor (higher values result in faster damping).</param>
         * <param name="dt">The delta time (time since last frame).</param>
         * <returns>A value between 0 and 1 representing the interpolation factor for the current frame.</returns>
         */
        static float BaseDamp(float damping, float dt)
        {
            return 1 - Mathf.Exp(-damping * dt);
        }

        #endregion
    }
}
