/****************************************************************************
 * Copyright ©2021 Khoa Nguyen and Quan Dang. Adapted from CSE 457 Modeler by
 * Brian Curless. All rights reserved. Permission is hereby granted to
 * students registered for University of Washington CSE 457.
 * No other use, copying, distribution, or modification is permitted without
 * prior written consent. Copyrights for third-party components of this work
 * must be honored.  Instructors interested in reusing these course materials
 * should contact the authors below.
 * Khoa Nguyen: https://github.com/akkaneror
 * Quan Dang: https://github.com/QuanGary
 ****************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Mathf;

/// <summary>
/// SurfaceOfRevolution is responsible for generating a mesh given curve points.
/// </summary>

#if (UNITY_EDITOR)
public class SurfaceOfRevolution : MonoBehaviour
{
    private Mesh mesh;

    private List<Vector2> curvePoints;
    private int _mode;
    private int _numCtrlPts;
    private readonly string _curvePointsFile = "curvePoints.txt";
    private Vector3[] normals;
    private int[] triangles;
    private Vector2[] UVs;
    private Vector3[] vertices;

    private int subdivisions;
    public TextMeshProUGUI subdivisionText;

    private void Start()
    {
        subdivisions = 16;
        subdivisionText.text = "Subdivision: " + subdivisions.ToString();
    }

    private void Update()
    {
    }

    public void Initialize()
    {
        // Create an empty mesh
        mesh = new Mesh();
        mesh.indexFormat =
            UnityEngine.Rendering.IndexFormat.UInt32; // Set Unity's max number of vertices for a mesh to be ~4 billion
        GetComponent<MeshFilter>().mesh = mesh;

        // Load curve points
        ReadCurveFile(_curvePointsFile);

        // Invalid number of control points
        if (_mode == 0 && _numCtrlPts < 4 || _mode == 1 && _numCtrlPts < 2) return;
        
        // Calculate and draw mesh
        ComputeMeshData();
        UpdateMeshData();
    }

    
    /// <summary>
    /// Computes the surface revolution mesh given the curve points and the number of radial subdivisions.
    /// 
    /// Inputs:
    /// curvePoints : the list of sampled points on the curve.
    /// subdivisions: the number of radial subdivisions
    /// 
    /// Outputs:
    /// vertices : a list of `Vector3` containing the vertex positions
    /// normals  : a list of `Vector3` containing the vertex normals. The normal should be pointing out of
    ///            the mesh.
    /// UVs      : a list of `Vector2` containing the texture coordinates of each vertex
    /// triangles: an integer array containing vertex indices (of the `vertices` list). The first three
    ///            elements describe the first triangle, the fourth to sixth elements describe the second
    ///            triangle, and so on. The vertex must be oriented counterclockwise when viewed from the 
    ///            outside.
    /// </summary>
    private void ComputeMeshData()
    {
        // TODO: Compute and set vertex positions, normals, UVs, and triangle faces
        // You will want to use curvePoints and subdivisions variables, and you will
        // want to change the size of these arrays
        vertices = new Vector3[(subdivisions + 1) * curvePoints.Count];
        normals = new Vector3[(subdivisions + 1) * curvePoints.Count];
        UVs = new Vector2[(subdivisions + 1) * curvePoints.Count];
        triangles = new int[subdivisions * (curvePoints.Count - 1) * 2 * 3];
        
        // compute the vertices
        List<Vector3> verticeList = new List<Vector3>();
        for (int i = 0; i < subdivisions; i++)
        {
            foreach (Vector2 point in curvePoints)
            {
                float theta = 0 + i * 2 * PI / subdivisions;
                Vector3 point3D = new Vector3(point.x, point.y, 0.0f);
                Vector3 x = Vector3.Scale(new Vector3(Cos(theta), 0, Sin(theta)), point3D);
                Vector3 y = Vector3.Scale(new Vector3(0, 1, 0), point3D);
                Vector3 z = Vector3.Scale(new Vector3(-Sin(theta), 0, Cos(theta)), point3D);
                verticeList.Add(new Vector3(x.x + x.y + x.z, y.x + y.y + y.z, z.x + z.y + z.z));
            }
        }
        
        foreach (Vector2 point in curvePoints)
        {
            verticeList.Add(new Vector3(point.x, point.y, 0));
        }
        vertices = verticeList.ToArray();
        
        // compute the triangles
        List<int> triangleList = new List<int>();
        for (int i = 0; i < subdivisions; i++)
        {
            for (int j = 0; j < curvePoints.Count - 1; j++)
            {
                int nextCurve = i + 1;
                // triangle 1 (two points on the main curve, one point on the next curve)
                triangleList.Add(j + i * curvePoints.Count); // vertex 1 of the first triangle
                triangleList.Add(j + 1 + i * curvePoints.Count); // vertex 2 of the first triangle
                triangleList.Add(j + nextCurve * curvePoints.Count); // vertex 3 of the first triangle
                
                // triangle 2 (one point on the main curve, two points on the next curve)
                triangleList.Add(j + nextCurve * curvePoints.Count); // vertex 1 of the second triangle
                triangleList.Add(j + 1 + i * curvePoints.Count); //vertex 2 of the second triangle
                triangleList.Add(j + 1 + nextCurve * curvePoints.Count); // vertex 3 of the second triangle
            }
        }
        triangles = triangleList.ToArray();

        // compute the normals
        // compute the normals on the first curve
        List<Vector3> normalListOneCurve = new List<Vector3>();
        for (int i = 0; i < curvePoints.Count - 1; i++)
        {
            Vector3 vectorA = vertices[i + curvePoints.Count] - vertices[i];
            Vector3 vectorB = vertices[i + 1] - vertices[i];
            normalListOneCurve.Add(Vector3.Cross(vectorA, vectorB));
        }
        Vector3 finalVectorA = vertices[(curvePoints.Count - 1) + curvePoints.Count * (subdivisions - 1)] - vertices[curvePoints.Count - 1];
        Vector3 finalVectorB = vertices[curvePoints.Count - 2] - vertices[curvePoints.Count - 1];
        normalListOneCurve.Add(Vector3.Cross(finalVectorA, finalVectorB));
        
        // rotate all the normals to get the rest
        List<Vector3> normalList = new List<Vector3>();
        for (int i = 0; i < subdivisions; i++)
        {
            foreach (Vector3 normal in normalListOneCurve)
            {
                float theta = 0 + i * 2 * PI / subdivisions;
                Vector3 x = Vector3.Scale(new Vector3(Cos(theta), 0, Sin(theta)), normal);
                Vector3 y = Vector3.Scale(new Vector3(0, 1, 0), normal);
                Vector3 z = Vector3.Scale(new Vector3(-Sin(theta), 0, Cos(theta)), normal);
                normalList.Add(new Vector3(x.x + x.y + x.z, y.x + y.y + y.z, z.x + z.y + z.z));
            }
        }
        foreach (Vector3 normal in normalListOneCurve)
        {
            normalList.Add(normal);
        }
        normals = normalList.ToArray();
        
        // compute UVs
        // compute the Vs of the first curve
        List<float> dSums = new List<float>{0f}; // aggregated distances
        float dSum = 0;
        List<float> vs = new List<float>();
        List<Vector2> UVList = new List<Vector2>();
        for (int i = 0; i < curvePoints.Count - 1; i++)
        {
            float d = Sqrt(Pow(vertices[i + 1].x - vertices[i].x, 2) + Pow(vertices[i + 1].y - vertices[i].y, 2) + Pow(vertices[i + 1].z - vertices[i].z, 2));
            dSums.Add(d + dSum);
            dSum += d;
        }

        foreach (float d in dSums)
        {
            vs.Add(d / dSum);
        }
        
        // compute the Us and append to UVs
        for (int i = 0; i < subdivisions + 1; i++)
        {
            float theta = 0 + i * 2 * PI / subdivisions;
            float u = 1 - theta / (2 * PI);
            foreach (float v in vs)
            {
                UVList.Add(new Vector2(u, v));
            }
        }

        UVs = UVList.ToArray();
    }

    private void UpdateMeshData()
    {
        // Assign data to mesh
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.triangles = triangles;
        mesh.uv = UVs;
    }

    // Export mesh as an asset
    public void ExportMesh()
    {
        string path = EditorUtility.SaveFilePanel("Save Mesh Asset", "Assets/ExportedMesh/", mesh.name, "asset");
        if (string.IsNullOrEmpty(path)) return;
        path = FileUtil.GetProjectRelativePath(path);
        AssetDatabase.CreateAsset(mesh, path);
        AssetDatabase.SaveAssets();
    }

    public void SubdivisionValueChanged(Slider slider)
    {
        subdivisions = (int)slider.value;
        subdivisionText.text = "Subdivision: " + subdivisions.ToString();
    }
    
    private void ReadCurveFile(string file)
    {
        curvePoints = new List<Vector2>();
        string line;

        var f =
            new StreamReader(file);
        if ((line = f.ReadLine()) != null)
        {
            var curveData = line.Split(' ');
            _mode = Convert.ToInt32(curveData[0]);
            _numCtrlPts = Convert.ToInt32(curveData[1]);
        }

        while ((line = f.ReadLine()) != null)
        {
            var curvePoint = line.Split(' ');
            var x = float.Parse(curvePoint[0]);
            var y = float.Parse(curvePoint[1]);
            curvePoints.Add(new Vector2(x, y));
        }

        f.Close();
    }
}
#endif
