using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;

public class ActionEditorWindow : EditorWindow
{
    private const float frameDuration = 1f / 60f;         // 60FPS固定.

    private AttackData attackData;                  // データ用の変数.
    private SerializedObject serializedAttackData;  // 技の情報が入ったスクリプタブルオブジェクト.
    private int currentFrame = 0;                   // 現在のフレーム.
    private EditorCoroutine playCoroutine;          // アニメーション用のコルーチン.
    private bool isPlaying = false;                 // アニメーションが再生されているかどうか.
    private Vector2 scrollPos;                      // エディターのスクロールに使う変数.
    private float previewScale = 2f;                // キャラクターのサイズ.

    [MenuItem("Window/Action Editor")]

    public static void Open()
    {
        GetWindow<ActionEditorWindow>("Action Editor");
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        // AttackData選択.
        AttackData newData = (AttackData)EditorGUILayout.ObjectField("AttackData", attackData, typeof(AttackData), false);
        // 選択したデータと保存されているデータが違ったら.
        if (newData != attackData)
        {
            attackData = newData;   // データを代入.
            // attackDataがnullでないなら代入する.
            serializedAttackData = (attackData != null) ? new SerializedObject(attackData) : null;
        }

        // attackDataかserializedAttackDataがnullなら値を返す.
        if (attackData == null || serializedAttackData == null)
        {
            EditorGUILayout.EndScrollView();
            return;
        }

        GUILayout.Space(10);

        // AttackDataの編集フィールド.
        serializedAttackData.Update();

        // EditorGUILayout.PropertyField(serializedAttackData.FindProperty("attackName"));
        EditorGUILayout.PropertyField(serializedAttackData.FindProperty("attribute"));
        EditorGUILayout.PropertyField(serializedAttackData.FindProperty("attackStart"));
        EditorGUILayout.PropertyField(serializedAttackData.FindProperty("attackDuration"));
        EditorGUILayout.PropertyField(serializedAttackData.FindProperty("attackRecovery"));
        EditorGUILayout.PropertyField(serializedAttackData.FindProperty("landingRecovery"));
        EditorGUILayout.PropertyField(serializedAttackData.FindProperty("hitStop"));
        EditorGUILayout.PropertyField(serializedAttackData.FindProperty("damege"));
        EditorGUILayout.PropertyField(serializedAttackData.FindProperty("shieldDamage"));
        EditorGUILayout.PropertyField(serializedAttackData.FindProperty("eHitRecovery"));
        EditorGUILayout.PropertyField(serializedAttackData.FindProperty("eGuardRecovery"));
        EditorGUILayout.PropertyField(serializedAttackData.FindProperty("hitBack"));
        EditorGUILayout.PropertyField(serializedAttackData.FindProperty("guardBack"));
        EditorGUILayout.PropertyField(serializedAttackData.FindProperty("startUpSprite"), true);
        EditorGUILayout.PropertyField(serializedAttackData.FindProperty("activeAttackSprite"), true);
        EditorGUILayout.PropertyField(serializedAttackData.FindProperty("recoverySprite"), true);

        serializedAttackData.ApplyModifiedProperties(); // 保存.

        GUILayout.Space(10);

        DrawTimeline(); // タイムライン.
        GUILayout.Space(10);
        DrawControls();

        DrawPreview();

        GUILayout.Space(10);

        serializedAttackData.Update();
        DrawHitBoxEditor();
        serializedAttackData.ApplyModifiedProperties();

        EditorGUILayout.EndScrollView();

    }

    // フレームメーターの表示.
    private void DrawTimeline()
    {
        Rect rect = GUILayoutUtility.GetRect(400, 40);

        EditorGUI.DrawRect(rect, Color.gray);

        float frameWidth = rect.width / attackData.totalFrame;

        // クリックしてフレーム移動.
        if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
        {
            float clickX = Event.current.mousePosition.x - rect.x;
            currentFrame = Mathf.Clamp(Mathf.FloorToInt(clickX / frameWidth), 0, attackData.totalFrame - 1);
            Repaint();
        }

        // 発生.
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, attackData.attackStart * frameWidth, rect.height), Color.yellow);

        // 判定（効果時間）.
        EditorGUI.DrawRect(new Rect(rect.x + attackData.attackStart * frameWidth, rect.y, attackData.attackDuration * frameWidth, rect.height), Color.red);

        // 硬直.
        EditorGUI.DrawRect(new Rect(rect.x + (attackData.attackStart + attackData.attackDuration) * frameWidth, rect.y, attackData.attackRecovery * frameWidth, rect.height), Color.blue);

        // フレームごとの区切り線.
        for (int i = 0; i <= attackData.totalFrame; i++)
        {
            float x = rect.x + i * frameWidth;
            EditorGUI.DrawRect(new Rect(x, rect.y, 1, rect.height), Color.black);
        }

        // 再生ヘッド.
        float headX = rect.x + currentFrame * frameWidth;
        EditorGUI.DrawRect(new Rect(headX, rect.y, 2, rect.height), Color.white);
    }

    // ヒットボックス調整用のボタンなどを表示.
    private void DrawHitBoxEditor()
    {
        GUILayout.Label("HitBox Editor", EditorStyles.boldLabel);

        var frameData = serializedAttackData.FindProperty("frameBoxes").GetArrayElementAtIndex(currentFrame);
        var boxesProp = frameData.FindPropertyRelative("boxes");

        GUILayout.Space(10);

        // 追加ボタン.
        if (GUILayout.Button("Add HitBox"))
        {
            boxesProp.arraySize++;
        }

        GUILayout.Space(10);

        // ボックスリスト表示.
        for (int i = 0; i < boxesProp.arraySize; i++)
        {
            var box = boxesProp.GetArrayElementAtIndex(i);

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(box.FindPropertyRelative("boxType"));
            EditorGUILayout.PropertyField(box.FindPropertyRelative("offset"));
            EditorGUILayout.PropertyField(box.FindPropertyRelative("size"));

            GUILayout.Space(5);
            if (GUILayout.Button("Delete"))
            {
                boxesProp.DeleteArrayElementAtIndex(i);
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(5);
        }
    }

    // アニメーション用の画像表示.
    private void DrawPreview()
    {
        GUILayout.Label("Sprite Preview", EditorStyles.boldLabel);

        Sprite sprite = GetSpriteForFrame(currentFrame);
        if (sprite == null) return;

        float previewWidth = sprite.rect.width * previewScale;
        float previewHeight = sprite.rect.height * previewScale;


        Texture2D tex = sprite.texture;
        Rect spriteRect = sprite.rect;

        Rect rect = GUILayoutUtility.GetRect(previewWidth, previewHeight, GUILayout.ExpandWidth(false));
        rect.width = previewWidth;
        rect.height = previewHeight;

        rect.x = (position.width - rect.width) / 2f;

        // スプライトのUV座標.
        Rect texCoords = new Rect(
            spriteRect.x / tex.width,
            spriteRect.y / tex.height,
            spriteRect.width / tex.width,
            spriteRect.height / tex.height
        );

        // 枠内にスプライトを描画（縦横比を維持してフィット）.
        GUI.DrawTextureWithTexCoords(rect, tex, texCoords, true);

        DrawHitBoxesOnSprite(rect, sprite);
    }

    // フレームに対応した画像の取得用関数.
    private Sprite GetSpriteForFrame(int frame)
    {
        if (attackData == null) return null;

        int activeStart = attackData.attackStart;
        int activeEnd = attackData.attackStart + attackData.attackDuration;

        if (frame < activeStart)
            return attackData.startUpSprite.Count > 0
                ? attackData.startUpSprite[Mathf.Min(frame, attackData.startUpSprite.Count - 1)]
                : null;

        if (frame < activeEnd)
            return attackData.activeAttackSprite.Count > 0
                ? attackData.activeAttackSprite[Mathf.Min(frame - activeStart, attackData.activeAttackSprite.Count - 1)]
                : null;

        int recoveryStart = activeEnd;
        if (frame < recoveryStart + attackData.attackRecovery)
            return attackData.recoverySprite.Count > 0
                ? attackData.recoverySprite[Mathf.Min(frame - recoveryStart, attackData.recoverySprite.Count - 1)]
                : null;

        return null;
    }

    // アニメーション用のボタン.
    private void DrawControls()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("◀")) currentFrame = Mathf.Max(0, currentFrame - 1);

        if (GUILayout.Button(isPlaying ? "■ Stop" : "▶ Play"))
        {
            if (isPlaying) StopPlayback();
            else StartPlayback();
        }

        if (GUILayout.Button("▶"))
        {
            currentFrame = Mathf.Min(currentFrame + 1, attackData.totalFrame - 1);
        }

        GUILayout.EndHorizontal();
    }

    // アニメーションの再生処理.
    private void StartPlayback()
    {
        if (attackData == null) return;

        isPlaying = true;
        currentFrame = 0;

        playCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(PlayRoutine());
    }

    // アニメーションの止める処理.
    private void StopPlayback()
    {
        isPlaying = false;
        if (playCoroutine != null)
        {
            EditorCoroutineUtility.StopCoroutine(playCoroutine);
            playCoroutine = null;
        }
    }

    // アニメーションの再生.
    private System.Collections.IEnumerator PlayRoutine()
    {
        while (isPlaying && currentFrame < attackData.totalFrame - 1)
        {
            yield return new EditorWaitForSeconds(frameDuration);

            currentFrame++;
            Repaint();
        }

        currentFrame = attackData.totalFrame - 1;
        isPlaying = false;
        playCoroutine = null;
        Repaint();
    }

    // フレームに対応した当たり判定の表示.
    private void DrawHitBoxesOnSprite(Rect spriteRect, Sprite sprite)
    {
        var frameData = attackData.frameBoxes[currentFrame];
        if (sprite == null) return;

        float ppu = sprite.pixelsPerUnit;

        // pivot (0〜1) をpixelに変換.
        Vector2 pivotPixel = sprite.pivot;

        foreach (var box in frameData.boxes)
        {
            Color c = (box.boxType == BoxData.BoxType.Hit) ? new Color(1, 0, 0, 0.3f) : new Color(0, 0, 1, 0.3f);
            Handles.color = c;

            Vector2 pixelSize = box.size * ppu;
            Vector2 pixelOffset = box.offset * ppu;

            // pivotを基準にオフセット.
            Rect r = new Rect(
             spriteRect.x + pivotPixel.x * previewScale + pixelOffset.x * previewScale - (pixelSize.x * previewScale) / 2,
             spriteRect.y + (spriteRect.height - pivotPixel.y * previewScale) - pixelOffset.y * previewScale - (pixelSize.y * previewScale) / 2,
             pixelSize.x * previewScale,
             pixelSize.y * previewScale
             );

            // 塗り矩形.
            EditorGUI.DrawRect(r, c);

            // 枠線.
            Handles.DrawSolidRectangleWithOutline(r, Color.clear, Color.white);
        }
    }
}