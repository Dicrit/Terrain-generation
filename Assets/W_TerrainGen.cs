using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Threading;

public class W_TerrainGen : MonoBehaviour
{
    [SerializeField]
    public string seedstr;
    private List<Seed> seed = new List<Seed>();



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
    private Color32[] getColorMap(int resolution, int width, int height) {
        Color32[] cols = new Color32[width * height];
        for (int i = 0; i < resolution; i++)
        {
            for (int k = 0; k < resolution; k++)
            {
                float f = getPointHeight2(i,k);
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
        if (seedstr == "" || seedstr.Length % 6 != 0) { seedstr = "101111011111"; Debug.LogWarning("seed is uncorrect. Using default seed"); }
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
                Debug.LogWarning("seed is uncorrect. Using default seed");
                seed = new List<Seed>();
                InitializeSeed("101111011111");
            }
        }
    }
    void Start()
    {
        InitializeSeed(seedstr);
        setUpTerrain(2048,2048);
        
    }
    void setUpTerrain(int width, int height)
    {
        int resolution = width;

        Texture2D texture = new Texture2D(width, height);
        Color32[] cols = getColorMap(resolution,width,height);
        texture.SetPixels32(cols);
        texture.Apply();

        float[,] heights = new float[resolution, resolution];
        for (int i = 0; i < resolution; i++)
        {
            for (int k = 0; k < resolution; k++)
            {
                heights[i, k] = texture.GetPixel(i, k).grayscale * 0.03f;
            }
        }

        Terrain terrain = FindObjectOfType<Terrain>();
        terrain.terrainData.size = new Vector3(width, width, height);
        terrain.terrainData.heightmapResolution = resolution;
        terrain.terrainData.SetHeights(0, 0, heights);
    }
}
