using UnityEngine;
using System.IO;
using System.Collections.Generic;

// Sprite sayfalarını StreamingAssets/Sprites/ klasöründen yükler.
// Checker arka planı (rgb ~150 veya ~105 gri) şeffaf yapar.
public static class SpriteLoader
{
    static readonly Dictionary<string, Texture2D> cache = new();

    public static Texture2D LoadSheet(string filename)
    {
        if (cache.TryGetValue(filename, out var hit)) return hit;

        string path = Path.Combine(Application.streamingAssetsPath, "Sprites", filename);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"SpriteLoader: {path} bulunamadı");
            return null;
        }

        byte[] data = File.ReadAllBytes(path);
        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.LoadImage(data);
        RemoveCheckerboard(tex);
        tex.Apply();
        cache[filename] = tex;
        return tex;
    }

    // Sprite'ı sayfadan keser. x,y koordinatları görüntünün sol-üst köşesinden.
    public static Sprite Extract(Texture2D tex, int x, int y, int w, int h, float ppu = 100f)
    {
        if (tex == null) return null;
        int fy = tex.height - y - h;                    // üst-sol → alt-sol dönüşümü
        fy = Mathf.Clamp(fy, 0, tex.height - h);
        w  = Mathf.Min(w, tex.width  - x);
        h  = Mathf.Min(h, tex.height - fy);
        return Sprite.Create(tex, new Rect(x, fy, w, h), new Vector2(0.5f, 0.5f), ppu);
    }

    // Checker arka plan: gri pikseller (~150,150,150 ve ~105,105,105) → alfa=0
    static void RemoveCheckerboard(Texture2D tex)
    {
        var px = tex.GetPixels32();
        for (int i = 0; i < px.Length; i++)
        {
            int r = px[i].r, g = px[i].g, b = px[i].b;
            int maxDiff = System.Math.Max(System.Math.Abs(r - g),
                          System.Math.Max(System.Math.Abs(g - b), System.Math.Abs(r - b)));
            int br = (r + g + b) / 3;
            if (maxDiff < 22 && (System.Math.Abs(br - 150) < 22 || System.Math.Abs(br - 105) < 22))
                px[i].a = 0;
        }
        tex.SetPixels32(px);
    }
}
