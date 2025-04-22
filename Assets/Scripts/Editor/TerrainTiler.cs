using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class TerrainTiler : MonoBehaviour
{
    [MenuItem("Tools/Generate Terrain Tiles")]
    static void GenerateTiles()
    {
        int tileCountX = 10;  // 横方向のタイル数
        int tileCountZ = 10;  // 縦方向のタイル数
        int tileSize = 500;

        for (int z = 0; z < tileCountZ; z++)  // 縦方向のループ
        {
            for (int x = 0; x < tileCountX; x++)  // 横方向のループ
            {
                string sceneName = $"Terrain_Tile_{z}_{x}";
                var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

                TerrainData terrainData = new TerrainData
                {
                    heightmapResolution = 513,
                    size = new Vector3(tileSize, 100, tileSize)
                };

                GameObject terrainGO = Terrain.CreateTerrainGameObject(terrainData);
                terrainGO.transform.position = new Vector3(x * tileSize, 0, z * tileSize);
                terrainGO.name = $"Terrain_{z}_{x}";

                // 保存
                string scenePath = $"Assets/Scenes/TerrainTiles/{sceneName}.unity";
                EditorSceneManager.SaveScene(newScene, scenePath);
                EditorSceneManager.CloseScene(newScene, true);
            }
        }

        Debug.Log("Terrain tiles generated and saved as scenes.");
    }
}
