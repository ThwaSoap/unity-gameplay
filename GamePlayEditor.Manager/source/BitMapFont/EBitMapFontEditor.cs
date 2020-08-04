using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GamePlayEditor.BitMapFont
{
    public class TiyEditorBuildFontWindow : EditorWindow
    {
        Texture2D Texture;
        int LineCount;
        string InputContent;
        [MenuItem("GamePlayTools/BuildFont")]
        static void Init()
        {
            EditorWindow.GetWindow(typeof(TiyEditorBuildFontWindow), false, "Build Font", true).Show();
        }

        private void OnEnable()
        {
            string path = EditorPrefs.GetString("TiyTools.BuildFont.Texture", "");

            if (string.IsNullOrEmpty(path) == false)
                Texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

            LineCount = EditorPrefs.GetInt("TiyTools.BuildFont.LineCount", 1);
            InputContent = EditorPrefs.GetString("TiyTools.BuildFont.Content", "");
        }

        private void OnDisable()
        {
            if (Texture != null)
            {
                EditorPrefs.SetString("TiyTools.BuildFont.Texture", AssetDatabase.GetAssetPath(Texture));
            }
            EditorPrefs.SetString("TiyTools.BuildFont.Content", InputContent);
            EditorPrefs.SetInt("TiyTools.BuildFont.LineCount", LineCount);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Font Texture:", GUILayout.Width(100));
            Texture = (Texture2D)EditorGUILayout.ObjectField(Texture, typeof(Texture2D), false);
            EditorGUILayout.EndHorizontal();

            if (Texture != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Texture Row:", GUILayout.Width(100));
                LineCount = EditorGUILayout.IntField(LineCount);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Font Words:", GUILayout.Width(100));
            InputContent = EditorGUILayout.TextArea(InputContent);
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Build Font"))
            {
                if (Texture != null)
                {
                    string assetPath = AssetDatabase.GetAssetPath(Texture);
                    TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(assetPath);
                    float width = Texture.width;
                    float height = Texture.height;



                    SpriteMetaData[] sheets = importer.spritesheet;

                    if (InputContent.Length != sheets.Length)
                    {
                        Debug.LogErrorFormat("文字个数与图元数量不匹配:文字{0}!={1}图元", InputContent.Length, sheets.Length);
                    }
                    else
                    {
                        int WordH = Texture.height / LineCount;
                        List<CharacterInfo> infos = new List<CharacterInfo>();
                        for (int i = 0; i < sheets.Length; i++)
                        {
                            var v = sheets[i];
                            CharacterInfo info = new CharacterInfo();
                            info.index = Convert.ToInt32(InputContent[i]);
                            int row = (int)v.rect.position.y / WordH * WordH;
                            Vector4 uv = GetUV(new Rect(new Vector2(v.rect.position.x, row), new Vector2(v.rect.size.x, WordH)), width, height);
                            info.minX = 0;
                            info.maxY = 0;
                            info.minY = -WordH;
                            info.maxX = (int)v.rect.size.x;
                            info.advance = (int)v.rect.size.x;

                            info.uvBottomLeft = new Vector2(uv.x, uv.y);
                            info.uvTopRight = new Vector2(uv.x + uv.z, uv.y + uv.w);
                            info.uvTopLeft = new Vector2(uv.x, uv.y + uv.w);
                            info.uvBottomRight = new Vector2(uv.x + uv.z, uv.y);
                            infos.Add(info);
                        }


                        string basePath = GetPath(assetPath);
                        string assetName = GetName(assetPath);
                        string matPath = basePath + assetName + ".mat";
                        string fontPath = basePath + assetName + ".fontsettings";

                        //> 创建更新材质
                        Material material = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                        if (material == null)
                        {
                            material = new Material(Shader.Find("GUI/Text Shader"));
                            material.mainTexture = Texture;
                            AssetDatabase.CreateAsset(material, matPath);
                        }
                        else
                        {
                            material.mainTexture = Texture;
                        }

                        //> 创建更新字体
                        Font font = (Font)AssetDatabase.LoadAssetAtPath(fontPath, typeof(Font));
                        if (font == null)
                        {
                            font = new Font();
                            font.characterInfo = infos.ToArray();
                            font.material = material;

                            AssetDatabase.CreateAsset(font, fontPath);
                        }
                        else
                        {
                            font.material = material;
                            font.characterInfo = infos.ToArray();
                        }
                    }
                }
                Debug.Log("创建字体文件。");
            }
        }

        string GetPath(string _path)
        {
            int index = _path.LastIndexOf('/');
            return _path.Remove(index) + "/";
        }

        string GetName(string _path)
        {
            int index = _path.LastIndexOf('/');
            _path = _path.Substring(index + 1);
            index = _path.IndexOf('.');
            return _path.Remove(index);
        }

        Vector4 GetUV(Rect _rect, float _width, float _height)
        {
            Vector4 v = new Vector4();
            v.x = _rect.position.x / _width;
            v.y = _rect.position.y / _height;
            v.z = _rect.size.x / _width;
            v.w = _rect.size.y / _height;
            return v;
        }
    }
}
