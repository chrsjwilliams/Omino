using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MakeTileSkin
{
    [MenuItem("Assets/Create/TileSkin")]
    public static void Create()
    {
        TileSkin asset = ScriptableObject.CreateInstance<TileSkin>();
        AssetDatabase.CreateAsset(asset, "Assets/NewTileSkin.asset");
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
}
