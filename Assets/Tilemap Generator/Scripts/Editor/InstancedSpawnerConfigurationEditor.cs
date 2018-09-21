using System.IO;
using UnityEngine;
using UnityEditor;
using TilemapGenerator;

namespace TilemapGeneratorEditor
{
    [CustomEditor(typeof(InstancedSpawnerConfiguration))]
    public class InstancedSpawnerConfigurationEditor : Editor
    {

        InstancedSpawnerConfiguration instance;

        private void OnEnable()
        {
            instance = (InstancedSpawnerConfiguration) target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Generate Packed Image"))
            {
                if (instance.Sprites.Length == 0)
                {
                    Debug.LogError("Add some images into the Source Images");
                    return;
                }
                string basePath = AssetDatabase.GetAssetPath(instance.Sprites[0]);
                if (string.IsNullOrEmpty(basePath) || string.IsNullOrWhiteSpace(basePath))
                {
                    Debug.LogError("Can't get base path");
                    return;
                }
                int index = basePath.LastIndexOf('/');
                basePath = basePath.Substring(0, index);
                string path = EditorUtility.SaveFilePanel("Save To", basePath, instance.Sprites[0].name, "asset");
                if (path.Contains(Application.dataPath))
                {
                    path = path.Substring(Application.dataPath.Length - 6);
                    Texture3D results = BuildTexture(instance.Sprites);
                    if (results == null)
                    {
                        Debug.LogError("Error generating texture");
                        return;
                    }
                    AssetDatabase.CreateAsset(results, path);
                    AssetDatabase.Refresh();
                    results = AssetDatabase.LoadAssetAtPath<Texture3D>(path);
                    instance.PackedTexture = results;
                    EditorUtility.SetDirty(target);
                }
            }
        }

        public Texture3D BuildTexture(Sprite[] sprites)
        {
            int width = 0;
            int height = 0;
            int depth = sprites.Length;
            float ppu = sprites[0].pixelsPerUnit;

            for (int i = 0; i < depth; i++)
            {
                if (sprites[i].textureRect.width > width)
                {
                    width = (int) sprites[i].textureRect.width;
                }
                if (sprites[i].textureRect.height > height)
                {
                    height = (int) sprites[i].textureRect.height;
                }
            }
            int texSize = width * height;
            Texture3D sourceImage = new Texture3D(width, height, depth, TextureFormat.ARGB32, false);
            sourceImage.anisoLevel = 0;
            sourceImage.wrapMode = TextureWrapMode.Clamp;
            Color[] textureColors = new Color[texSize * depth];
            Color[] transparent = new Color[texSize];
            Texture2D blank = new Texture2D(width, height, TextureFormat.ARGB32, false);
            for (int i = 0; i < texSize; i++)
            {
                transparent[i] = new Color(0, 0, 0, 0);
            }
            for (int i = 0; i < depth; i++)
            {
                int offset = i * texSize;
                blank.SetPixels(transparent);
                blank.Apply();
                Graphics.CopyTexture(sprites[i].texture, 0, 0, (int) sprites[i].textureRect.xMin, (int) sprites[i].textureRect.yMin, (int) sprites[i].textureRect.width, (int) sprites[i].textureRect.height, blank, 0, 0, 0, 0);
                blank.Apply();
                System.Array.Copy(blank.GetPixels(), 0, textureColors, offset, texSize);
            }
            sourceImage.SetPixels(textureColors);
            sourceImage.Apply();
            return sourceImage;
        }
    }
}
