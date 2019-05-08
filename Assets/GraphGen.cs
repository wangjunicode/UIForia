using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class GraphGen : MonoBehaviour
{

    public Color color_a, color_b, color_c, color_d;

    MeshFilter meshFilter;
    MeshBits meshBits;
    Mesh mesh;

    private class MeshBits
    {

        public Color[] colors;

        int count = 0;
        public List<Vector3> verts = new List<Vector3>();
        public List<int> indexes = new List<int>();
        public List<Vector4> uvs = new List<Vector4>();
        float topDelay = 0;

        public void clear()
        {
            verts.Clear();
            indexes.Clear();
            uvs.Clear();
            count = 0;
        }

        public void makeWave(float totalWidth, float offset = 0)
        {
            clear();

            AddQuad(Vector3.zero - Vector3.up * 0.1f - Vector3.right * 0.1f, totalWidth + 0.1f, 0.05f, 0.05f);
            AddQuad(Vector3.zero - Vector3.right * 0.1f, 0.05f, 1, 1);

            float zoff = offset / 10;
            float top = 0;
            const int samples = 320;
            float sampleWidth = totalWidth / samples;
            float scale = 3;
            for (int i=0;i<samples;i++)
            {
                float x = sampleWidth * i;

                float p1, p2;

                p1 = Mathf.Abs(Mathf.PerlinNoise(x * scale + offset, zoff));
                p2 = Mathf.Abs(Mathf.PerlinNoise((x + sampleWidth) * scale + offset, zoff));

                AddQuad(Vector3.zero + Vector3.right * x, sampleWidth, p1, p2);

                if (p1 > top)
                    top = p1;
                if (p2 > top)
                    top = p2;

            }

            AddQuad(Vector3.zero + Vector3.up * topDelay, totalWidth, 0.02f, 0.02f);
            AddQuad(Vector3.zero + Vector3.up * top, totalWidth, 0.01f, 0.02f);

            if (topDelay < top) topDelay = top;
            if (topDelay > 0)
            {
                topDelay -= Time.deltaTime / 10;
                if (topDelay < 0) topDelay = 0;
            }

        }

        [StructLayout(LayoutKind.Explicit)]
        public struct Union
        {
            [FieldOffset(0)] public float asFloat;
            [FieldOffset(0)] public int asInt;
        }

        public float colorToFloat(Color c)
        {
            int color = (int)(c.r * 255) | (int)(c.g * 255) << 8 | (int)(c.b * 255) << 16 | (int)(c.a * 255) << 24;

            Union color2Float;
            color2Float.asFloat = 0;
            color2Float.asInt = color;

            return color2Float.asFloat;
        }


        public void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            verts.Add(a);
            verts.Add(b);
            verts.Add(c);
            verts.Add(d);

            Vector4 colorVec = new Vector4(colorToFloat(colors[0]), colorToFloat(colors[1]), colorToFloat(colors[2]), colorToFloat(colors[3]));
            uvs.Add(colorVec);
            uvs.Add(colorVec);
            uvs.Add(colorVec);
            uvs.Add(colorVec);


            indexes.Add(count++);
            indexes.Add(count++);
            indexes.Add(count++);
            indexes.Add(count++);
        }

        public void AddQuad(Vector3 origin, float width, float h1, float h2)
        {
            AddQuad(origin,
            origin + Vector3.up * h1,
            origin + Vector3.right * width + Vector3.up * h2,
            origin + Vector3.right * width);
        }

        public void push(Mesh mesh, MeshFilter mf)
        {
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetIndices(indexes.ToArray(), MeshTopology.Quads, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mf.mesh = mesh;
        }
    }
    
    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        meshBits = new MeshBits();
        meshBits.colors = new Color[4];
        meshBits.colors[0] = color_a;
        meshBits.colors[1] = color_b;
        meshBits.colors[2] = color_c;
        meshBits.colors[3] = color_d;

    }

    float clock = 0;
    
    void Update()
    {
        clock += Time.deltaTime * 3;

        meshBits.colors[0] = color_a;
        meshBits.colors[1] = color_b;
        meshBits.colors[2] = color_c;
        meshBits.colors[3] = color_d;

        meshBits.makeWave(3, clock + transform.position.y + transform.position.x + transform.position.z);

        meshBits.push(mesh, meshFilter);
    }
}
