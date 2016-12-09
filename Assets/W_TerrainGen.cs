using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Diagnostics;
using System.Threading;

public class W_TerrainGen : MonoBehaviour
{
    [SerializeField]
    public string seedstr;
    private List<Seed> seed = new List<Seed>();
    private int width = 2048;
    private Stopwatch watch, watch1;

    private class Seed
    {
        private int a, b, d;
        private float c;
        public Vector2 getVector(int i, int k){
            return new Vector2((i*a+k*b)/c, d);
        }
        public Seed(int _a, int _b, float _c, int _d)
        {
            a = _a;
            b = _b;
            c = _c;
            d = _d;
        }
    }


    //calculating color array
    private Color32[] getColorMap(int resolution) {
        Color32[] cols = new Color32[width * width];
        for (int i = 0; i < resolution; i++)
        {
            for (int k = 0; k < resolution; k++)
            {
                float f = getPointHeight1(i,k);
                cols[resolution * i + k] = new Color(f, f, f);
            }
        }
        return cols;
    }


    //function that returns height by position (from 0 to 1)
    private float getPointHeight1(int i , int k)
    {
        float b = (Mathf.Sin(i / 19f) + Mathf.Sin(k / 17f) + 2) / 4f;
        float f = (Mathf.Sin(i / 56f) + Mathf.Sin(k / 47f) + 2) / 4f;
        f = (4 * f + b) / 5f;
        return f;
    }

    //function that returns height by position (from 0 to 1) by seed
    private float getPointHeight2(int i, int k)
    {
        //if (seedstr.Length == 0 || seedstr.Length % 6 != 0) {return doMagic(new Vector2(i / 200f, 3), new Vector2(k / 77f, 1), new Vector2(i / 156f, 7), new Vector2(k / 47f, 4), new Vector2((i + k) / 99f, 10)); }
        List<Vector2> list = new List<Vector2>();
        foreach (Seed s in seed)
        {
            list.Add(s.getVector(i,k));
        }
        
    return doMagic(list.ToArray());
    }

    //Do Magic!!! getHeight using vectors
    private float doMagic(params Vector2[] values)
    {
        if (values.Length == 0) return 0;
        float magicNumber = 0;
        float magicDivider = 0;
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i].y <= 0) throw new Exception("multiplier must grater than 0");
            magicNumber += (Mathf.Sin(values[i].x)+1f)*values[i].y;
            magicDivider += values[i].y;
        }
        return magicNumber /= (magicDivider*2);
    }


    private void InitializeSeed(string seedstr)
    {
        if (seedstr == "" || seedstr.Length % 6 != 0) { seedstr = "101111011111"; UnityEngine.Debug.LogWarning("seed is uncorrect. Using default seed"); }
        for (int j = 0; j < seedstr.Length; j += 6)
        {
            try
            {
                int a = Int32.Parse(seedstr[0 + j].ToString());
                int b = Int32.Parse(seedstr[1 + j].ToString());
                float c = float.Parse(seedstr[2 + j].ToString() + seedstr[3 + j].ToString() + seedstr[4 + j].ToString());
                int d = Int32.Parse(seedstr[5 + j].ToString());
                seed.Add(new Seed(a, b, c, d));
            }
            catch
            {
                UnityEngine.Debug.LogWarning("seed is uncorrect. Using default seed");
                seed = new List<Seed>();
                InitializeSeed("101111011111");
            }
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Build();
        }
    }
    void Build()
    {
        watch = new Stopwatch();
        watch1 = new Stopwatch();
        watch.Reset();
        watch.Start();
        watch1.Reset();
        watch1.Start();
        InitializeSeed(seedstr);
        print("Seed creating time: " + watch1.ElapsedMilliseconds);
        setUpTerrain();
        print("Total time: " + watch.ElapsedMilliseconds);
    }
    Texture2D textureFromColorMap(Color32[] cols)
    {
        Texture2D texture = new Texture2D(width, width);
        texture.SetPixels32(cols);
        texture.Apply();
        return texture;
    }
    void setUpTerrain()
    {
        int resolution = width;

        watch1.Reset();
        watch1.Start();
        Color32[] cols = getColorMap(resolution);
        print("generating color array time: " + watch1.ElapsedMilliseconds);

        watch1.Reset();
        watch1.Start();
        Texture2D texture = textureFromColorMap(cols);
        print("Generating texture time: " + watch1.ElapsedMilliseconds);

        watch1.Reset();
        watch1.Start();
        float[,] heights = new float[resolution, resolution];
        for (int i = 0; i < resolution; i++)
        {
            for (int k = 0; k < resolution; k++)
            {
                heights[i, k] = texture.GetPixel(i, k).grayscale * 0.03f;
            }
        }
        print("Generating heights array time: " + watch1.ElapsedMilliseconds);
        watch1.Reset();
        watch1.Start();
        Terrain terrain = FindObjectOfType<Terrain>();
        terrain.terrainData.size = new Vector3(width, width, width);
        terrain.terrainData.heightmapResolution = resolution;
        terrain.terrainData.SetHeights(0, 0, heights);
        print("Terrain Applying time: "+watch1.ElapsedMilliseconds);
    }
}
