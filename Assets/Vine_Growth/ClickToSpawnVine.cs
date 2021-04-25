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

using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class ClickToSpawnVine : MonoBehaviour
{
    public VineGrowth vine;
    public ForceFiled forceField;
    public Vector3 velocityScaleMin = Vector3.one * 0.5f;
    public Vector3 velocityScaleMax = Vector3.one * 1.2f;
    public float grouthTimeScaleMin = 1.5f;
    public float grouthTimeScaleMax = 2.5f;
    public float heightMin = 1.5f;
    public float heightMax = 4.7f;

    private float SampleRandomHeight()
    {
        return Mathf.Lerp(heightMin, heightMax, Random.value);
    }

    private Vector3 SampleRandomVectorScale()
    {
        return new Vector3(
            Mathf.Lerp(velocityScaleMin.x, velocityScaleMax.x, Random.value),
            Mathf.Lerp(velocityScaleMin.y, velocityScaleMax.y, Random.value),
            Mathf.Lerp(velocityScaleMin.z, velocityScaleMax.z, Random.value)
        );
    }

    private float SampleRandomTimeScale()
    {
        return Mathf.Lerp(grouthTimeScaleMin, grouthTimeScaleMax, Random.value);
    }

    private void Update()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out var hit))
        {
            var hitPoint = hit.point;
            var obj = Instantiate(vine, transform);
            obj.transform.position = hitPoint;
            if (forceField != null)
            {
                obj.timeScale = SampleRandomTimeScale();
                obj.velocityScale = SampleRandomVectorScale();
                obj.GetComponent<GuidGrowthWithForceFiled>().forceFiled = forceField;
                obj.GetComponent<StopGrowthAtHeight>().height = SampleRandomHeight();
            }
        }
    }
}