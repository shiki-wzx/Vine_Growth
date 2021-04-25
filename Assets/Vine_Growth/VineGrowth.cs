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
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class VineFeature
{
    public FeatureAnimation prefab;
    public float minDistance = 0.01f;
    public float maxDistance = 1.0f;
    public float offsetMin = 0.01f;
    public float offsetMax = 0.1f;
    public float scaleMin = 0.4f;
    public float scaleMax = 1.5f;

    [HideInInspector] public Vector3 lastPosition;
    [HideInInspector] public float nextDistance;

    public void Init(Vector3 position)
    {
        lastPosition = position;
        nextDistance = Mathf.Lerp(minDistance, maxDistance, Random.value);
    }
}

[RequireComponent(typeof(MeshFilter))]
public class VineGrowth : MonoBehaviour
{
    private MeshFilter _meshFilter;
    private Mesh _mesh;

    public List<VineFeature> features;

    public bool showVerticesGizmos = false;
    public float timeScale = 2.0f;
    public Vector3 velocity = Vector3.up;
    public Vector3 velocityScale = Vector3.one;
    public float grouthLength = 10.0f;

    public Vector3 Direction => velocity.normalized;
    public float radius = 1.0f;
    public float minRadius = 0.1f;

    public Vector3 endPosition = Vector3.zero;

    // resolution should not changed after start
    public int loopResolution = 6;

    private void OnDrawGizmosSelected()
    {
        var endPosWorldSpace = transform.TransformPoint(endPosition);
        var directionWorldSpace = transform.TransformDirection(velocity).normalized;
        Gizmos.DrawWireSphere(endPosWorldSpace, radius);
        Gizmos.DrawLine(endPosWorldSpace, endPosWorldSpace + directionWorldSpace * 2.0f);
        Gizmos.DrawLine(endPosWorldSpace, endPosWorldSpace + directionWorldSpace * 2.0f);

        if (!showVerticesGizmos) return;

        Gizmos.color = Color.red;
        foreach (var vertex in _vertices)
        {
            Gizmos.DrawSphere(transform.TransformPoint(vertex), 0.1f);
        }
    }

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
    }


    private readonly List<Vector3> _vertices = new List<Vector3>();
    private readonly List<int> _triangles = new List<int>();

    private struct ControlPoint
    {
        public Vector3 position;
        public Vector3 velocity;
    }

    private readonly List<ControlPoint> _controlPoints = new List<ControlPoint>();

    private Vector3 GetRight(Vector3 direction)
    {
        return Vector3.Cross(direction, new Vector3(1.0f, 2.0f, 3.0f)).normalized;
    }


    private void AddQuadFace(int a1, int b1, int a2, int b2)
    {
        var faces = new[]
        {
            a1, b1, a2,
            b1, b2, a2
        };
        _triangles.AddRange(faces);
    }

    private void ConnectLoopWithPrevious()
    {
        var lastStartIndex = _vertices.Count - 2 * loopResolution;
        var currentStartIndex = _vertices.Count - loopResolution;
        if (lastStartIndex < 0 || currentStartIndex < 0)
        {
            return;
        }

        for (var i = 0; i < loopResolution; i++)
        {
            AddQuadFace(
                lastStartIndex + i,
                lastStartIndex + (i + 1) % loopResolution,
                currentStartIndex + i,
                currentStartIndex + (i + 1) % loopResolution
            );
        }
    }

    private Vector3[] _loopBuffer;

    private Vector3[] GetLoopVertices(Vector3 position, Vector3 direction, float r)
    {
        if (_loopBuffer == null || loopResolution != _loopBuffer.Length)
        {
            _loopBuffer = new Vector3[loopResolution];
        }

        var right = GetRight(direction);
        var back = Vector3.Normalize(Vector3.Cross(right, direction));

        var deltaAngle = Mathf.PI * 2.0f / (float) loopResolution;
        for (var i = 0; i < loopResolution; i++)
        {
            var angle = i * deltaAngle;
            var dir = back * Mathf.Cos(angle) + right * Mathf.Sin(angle);
            _loopBuffer[i] = position + dir * r;
        }

        return _loopBuffer;
    }

    private Vector3[] GetLoopVertices()
    {
        return GetLoopVertices(endPosition, Direction, radius);
    }

    private void AddLoop()
    {
        var loop = GetLoopVertices();
        _vertices.AddRange(loop);
        ConnectLoopWithPrevious();
    }

    private void AddControlPoint()
    {
        var controlPoint = new ControlPoint
        {
            position = endPosition,
            velocity = velocity
        };
        _controlPoints.Add(controlPoint);
        AddLoop();
    }

    private void UpdateLastControlPoint()
    {
        if (_controlPoints.Count == 0) return;
        _controlPoints[_controlPoints.Count - 1] = new ControlPoint
        {
            position = endPosition,
            velocity = velocity
        };
    }

    private float RadiusEase(float t)
    {
        return t * t;
    }


    // TODO use vertex shader for optimization
    private void UpdateVerticesForControlPoints()
    {
        var distance = 0.0f;
        for (var currentIndex = _controlPoints.Count - 1; currentIndex >= 0; currentIndex--)
        {
            var lastIndex = currentIndex + 1;
            if (lastIndex < _controlPoints.Count)
            {
                distance += Vector3.Distance(_controlPoints[currentIndex].position, _controlPoints[lastIndex].position);
            }

            var currentRadius = Mathf.Lerp(minRadius, radius, RadiusEase(Mathf.Clamp01(distance / grouthLength)));
            var loop = GetLoopVertices(_controlPoints[currentIndex].position,
                _controlPoints[currentIndex].velocity.normalized, currentRadius);
            for (var i = 0; i < loopResolution; i++)
            {
                _vertices[currentIndex * loopResolution + i] = loop[i];
            }
        }
    }

    private void UpdateMesh()
    {
        UpdateVerticesForControlPoints();
        _mesh.vertices = _vertices.ToArray();
        _mesh.triangles = _triangles.ToArray();
        _mesh.RecalculateNormals();
    }

    private Vector3 _lastDirection;
    private float _lastRadius;

    private Vector3 SampleNormal()
    {
        var right = GetRight(Direction);
        var back = Vector3.Cross(right, Direction);
        var angle = Random.value * Mathf.PI;
        return (right * Mathf.Cos(angle) + back * Mathf.Sin(angle)).normalized;
    }

    private void SwapnFeature(VineFeature feature)
    {
        if (Vector3.Distance(feature.lastPosition, endPosition) > feature.nextDistance)
        {
            var obj = Instantiate(feature.prefab, transform);
            var normal = SampleNormal();
            var offset = Mathf.Lerp(feature.offsetMin, feature.offsetMin, Random.value);
            var worldNormal = transform.TransformDirection(normal);
            worldNormal /= worldNormal.y;
            var angle = Mathf.Atan2(worldNormal.z, worldNormal.x);
            obj.transform.rotation = Quaternion.Euler(
                Mathf.Lerp(30.0f, 45.0f, Random.value),
                Mathf.Lerp(-10.0f, 10.0f, Random.value) + angle * Mathf.Rad2Deg - 90.0f,
                Mathf.Lerp(-20.0f, 20.0f, Random.value)
            );
            obj.targetScale = Vector3.one * Mathf.Lerp(feature.scaleMin, feature.scaleMax, Random.value);
            obj.transform.localScale = Vector3.zero;
            obj.transform.localPosition = endPosition + normal * offset;
            feature.lastPosition = endPosition;
            feature.nextDistance = Mathf.Lerp(feature.minDistance, feature.maxDistance, Random.value);
        }
    }

    private void Grouth(float dt)
    {
        var dotDirection = Vector3.Dot(Direction, _lastDirection);
        var deltaPos = Vector3.Scale(dt * velocity, velocityScale);
        endPosition += deltaPos;
        var shouldAddVertices = dotDirection < Mathf.Cos(Mathf.Deg2Rad * 2.0f) && deltaPos.magnitude > 1e-3f;
        var relativeDeltaRadius = Mathf.Abs(radius - _lastRadius) / _lastRadius;
        shouldAddVertices = shouldAddVertices || relativeDeltaRadius > 0.1;
        if (shouldAddVertices)
        {
            AddControlPoint();
            _lastDirection = Direction;
            _lastRadius = radius;
        }
        else
        {
            UpdateLastControlPoint();
        }

        UpdateMesh();

        foreach (var feature in features)
        {
            SwapnFeature(feature);
        }
    }

    private void Start()
    {
        _mesh = new Mesh();
        _meshFilter.mesh = _mesh;
        _lastDirection = Direction;
        _lastRadius = radius;
        foreach (var feature in features)
        {
            feature.Init(endPosition);
        }

        AddControlPoint();
        AddControlPoint();
        UpdateMesh();
    }

    private void Update()
    {
        Grouth(Time.deltaTime * timeScale);
    }
}