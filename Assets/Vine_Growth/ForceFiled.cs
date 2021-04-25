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

public class ForceFiled : MonoBehaviour
{
    public bool showGizmos = false;

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        var scale = 2f;
        var range = 5;
        Gizmos.color = Color.red;
        for (var x = -range; x <= range; x++)
        {
            for (var y = -range; y <= range; y++)
            {
                for (var z = -range; z <= range; z++)
                {
                    var localPos = new Vector3(x, y, z) * scale;
                    Gizmos.DrawRay(transform.TransformPoint(localPos),
                        transform.TransformVector(GetForce(localPos, Vector3.zero)));
                }
            }
        }
    }

    // all calculation are in force filed local space
    public virtual Vector3 GetForce(Vector3 position, Vector3 velocity)
    {
        return Vector3.zero;
    }
}