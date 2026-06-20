using SpawnSystem.Map;
using UnityEditor;
using UnityEngine;

namespace SpawnSystem.EditorTools
{
    /// <summary>
    /// <see cref="MapGenerator"/> 커스텀 인스펙터.
    /// 기본 필드(크기/복잡도 등) 아래에 Generate / Bake / Clear 버튼을 노출.
    /// </summary>
    [CustomEditor(typeof(MapGenerator))]
    public class MapGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var map = (MapGenerator)target;

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Map Tools", EditorStyles.boldLabel);

            int obstacles = Mathf.RoundToInt(map.complexity * map.maxObstacles);
            EditorGUILayout.HelpBox(
                $"크기 {map.width:0} x {map.length:0}  |  복잡도 {map.complexity:0.00} → 장애물 최대 {obstacles}개",
                MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Generate (생성 + 베이크)", GUILayout.Height(30)))
                {
                    Undo.RegisterFullObjectHierarchyUndo(map.gameObject, "Generate Map");
                    map.Generate();
                    MarkDirty(map);
                }

                if (GUILayout.Button("Bake NavMesh", GUILayout.Height(30)))
                {
                    map.Bake();
                    MarkDirty(map);
                }

                if (GUILayout.Button("Clear", GUILayout.Height(30)))
                {
                    Undo.RegisterFullObjectHierarchyUndo(map.gameObject, "Clear Map");
                    map.Clear();
                    MarkDirty(map);
                }
            }
        }

        static void MarkDirty(MapGenerator map)
        {
            if (Application.isPlaying)
                return;
            EditorUtility.SetDirty(map);
            if (map.gameObject.scene.IsValid())
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(map.gameObject.scene);
        }
    }
}
