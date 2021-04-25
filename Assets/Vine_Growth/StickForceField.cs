/*
 * MIT License
 * 
 * Copyright (c) 2020 huisedenanhai
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using UnityEngine;

public class StickForceField : ForceFiled
{
    [Range(0, 1)] public float raiseForce;
    [Range(0, 1)] public float noise;
    [Range(0, 1)] public float vortex;
    [Range(0, 1)] public float attraction;
    [Range(0, 5)] public float attractionHeightFactor;
    [Range(0, 1)] public float friction;

    private Vector3 AttractionForce(Vector3 position)
    {
        var planePos = new Vector3(position.x, 0, position.z);
        return -attraction * planePos * (1.0f + position.z * attractionHeightFactor);
    }

    private Vector3 RaiseForce(Vector3 position)
    {
        return raiseForce * Vector3.up;
    }

    private Vector3 VortexForce(Vector3 position)
    {
        var planePos = new Vector3(position.x, 0, position.z);
        return vortex * new Vector3(-position.z, 0, position.x).normalized
                      * Mathf.Exp(-planePos.magnitude * 5.0f);
    }

    private Vector3 NoiseForce(Vector3 position)
    {
        return noise * new Vector3(Mathf.PerlinNoise(
                position.x + position.z * 0.5f, position.y
            ) * 5.0f,
            Mathf.PerlinNoise(
                0.9f * position.x, 1.2f * position.y + position.z
            ) * 5.0f,
            Mathf.PerlinNoise(
                0.8f * position.x + position.z * 0.5f, position.y * 1.1f
            )) * 5.0f;
    }

    public override Vector3 GetForce(Vector3 position, Vector3 velocity)
    {
        return AttractionForce(position) + VortexForce(position) + RaiseForce(position) + NoiseForce(position) -
               velocity * friction;
    }
}