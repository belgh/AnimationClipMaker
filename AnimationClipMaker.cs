using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class AnimationClipMaker : EditorWindow
{
    /// <summary>
    /// 定数
    /// </summary>
    string basePath = "Assets/";

    /// <summary>
    /// GUI設定値
    /// </summary>
    bool isOverwrite = false;
    string spriteBase = "";
    string mainData = "\n";
    string errorText = "";
    public Vector2 scrollPosition;

    /// <summary>
    /// メニューに設定
    /// </summary>
    [MenuItem("Window/ClipMaker")]
    static void Open()
    {
        GetWindow<AnimationClipMaker>("ClipMaker");
    }

    /// <summary>
    /// Window内容設定
    /// </summary>
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 12;
        style.richText = true;
        GUILayout.Box(@"
<b>◆前提</b>
　・ClipのソースはResourcesフォルダに置く
　・ClipのソースはMultipleのSprite

<b>◆手順</b>
　(1) SpriteをResourcesフォルダの下に作成しておく(必須)
　(2) tsv(タブ区切り)形式で、出力するClip情報を列挙

　ex. <color=blue>Animations/OutputClip SpriteHoge  SpriteFuga   SpriteMoge</color>
", style);
        EditorGUILayout.Space();

        isOverwrite = EditorGUILayout.ToggleLeft("データの上書き", isOverwrite);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("▼Spriteオブジェクト名(必須、Multiple限定、Resources下)");
        spriteBase = EditorGUILayout.TextField(spriteBase);

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        EditorGUILayout.LabelField("▼tsv");
        mainData = EditorGUILayout.TextArea(mainData);
        GUILayout.EndScrollView();

        if (GUILayout.Button("実行"))
            Execute();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(errorText, MessageType.Warning);
    }

    /// <summary>
    /// 実行ボタン押下時処理
    /// </summary>
    void Execute()
    {
        var data = new List<List<String>>();
        var lineRegex = new Regex("\r\n|\r|\n");
        var columnRegex = new Regex("\t");

        // 必須チェック
        if (spriteBase.Trim().Length == 0)
        {
            errorText = "Spriteオブジェクト名を設定して下さい。";
            return;
        }

        // tsvをListに変換する
        foreach (string line in lineRegex.Split(mainData))
        {
            var lineData = new List<String>();
            foreach (string column in columnRegex.Split(line))
            {
                if (column.Trim().Length > 0)
                {
                    lineData.Add(column.Trim());
                }
            }
            // ファイル名 + Spriteで1行最低3カラム以上必要(Sprite1枚だと本ツールにおいては作る意味が無い)
            if (lineData.Count > 2)
            {
                data.Add(lineData);
            }
            else if (lineData.Count > 0)
            {
                Debug.Log("Skip: " + lineData.ToString());
            }
        }
        if (data.Count == 0)
        {
            errorText = "有効な入力データがありません。\nDebug.LogのSkip情報を確認して下さい。";
            return;
        }

        // エラーチェック及び出力
        Sprite[] spriteResources = Resources.LoadAll<Sprite>(spriteBase.Trim());
        if (spriteResources.Length == 0)
        {
            errorText = "Spriteオブジェクトが取得できません。";
            return;
        }
        foreach (var line in data)
        {
            // 最初の列は出力ファイル名
            var clipPath = line[0];
            line.RemoveAt(0);
            if (Regex.Match(clipPath, "\\.anim$").Success)
            {
                errorText = "出力ファイルの拡張子は暗黙に設定されるため、不要です。";
                return;
            }
            clipPath += ".anim";

            if (Regex.Match(clipPath, "\\/").Success)
            {
                var parts = new List<string>(clipPath.Split('/'));
                parts.RemoveAt(parts.Count - 1);
                System.IO.Directory.CreateDirectory(basePath + String.Join("/", parts.ToArray()));
            }
            clipPath = basePath + clipPath;

            // 出力ファイル上書きチェック
            if (File.Exists(clipPath))
            {
                if (isOverwrite)
                {
                    Debug.Log("Overwrite: " + clipPath);
                }
                else
                {
                    errorText = "Clipが既に存在します。\n" + clipPath + "\n上書きしない設定のため、終了します。";
                    return;
                }
            }
            
            // フレームごとに設定するSprite
            var sprites = new List<Sprite>();
            foreach (var column in line)
            {
                Sprite sprite = Array.Find(spriteResources, c => c.name == column);
                if (sprite)
                {
                    sprites.Add(sprite);
                }
                else
                {
                    errorText = "スプライトが見つかりませんでした。(" + column + ")\nデータを見直して下さい。";
                    return;
                }
            }

            // 出力
            PutAnimationClip(clipPath, sprites);
        }
    }

    /// <summary>
    /// Clip出力
    /// </summary>
    /// <param name="path">出力ClipのPath</param>
    /// <param name="sprites">各フレームに割り当てるSpriteの配列</param>
    void PutAnimationClip(string path, List<Sprite> sprites)
    {
        var animation = CreateAnimationClip(sprites.ToArray<Sprite>());
        AssetDatabase.CreateAsset(animation, path);
        EditorUtility.SetDirty(animation);
    }

    /// <summary>
    /// Clip生成実体
    /// </summary>
    /// <param name="sprites">各フレームに割り当てるSpriteの配列</param>
    AnimationClip CreateAnimationClip(params Sprite[] sprites)
    {
        /// Copyright (c) 2015 kyusyukeigo
        /// Released under the MIT license
        /// https://github.com/anchan828/unite2015tokyo/blob/master/LICENSE
        
        var animationClip = new AnimationClip { frameRate = 12 };
        var animationClipSettings = new AnimationClipSettings { loopTime = true };
        AnimationUtility.SetAnimationClipSettings(animationClip, animationClipSettings);
        var objectReferenceKeyframes = new ObjectReferenceKeyframe[sprites.Length];
        for (var i = 0; i < objectReferenceKeyframes.Length; i++)
        {
            objectReferenceKeyframes[i] = new ObjectReferenceKeyframe
            {
                value = sprites[i],
                time = i / animationClip.frameRate
            };
        }
        var editorCurveBinding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(animationClip, editorCurveBinding, objectReferenceKeyframes);
        return animationClip;
    }
}
