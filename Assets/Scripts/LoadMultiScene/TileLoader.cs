using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;

using Doinject;
using Mew.Core.Assets;

public class TileLoader : MonoBehaviour, IInjectableComponent
{
    public Transform player;
    public int tileSize = 500;

    public SceneAssetReference[] tileSceneReference;
    private Vector2Int currentTile = new Vector2Int(int.MinValue, int.MinValue);
    private Dictionary<Vector2Int, SceneContext> loadedTiles = new Dictionary<Vector2Int, SceneContext>();
    
    // ロードする範囲（プレイヤーを中心とした3x3のグリッド）
    private int loadRadius = 1;
    
    // タイルの変更を検出するためのフラグ
    private bool isTileChanged = false;
    
    // アンロード処理のためのフラグ
    private bool isUnloading = false;

    
    SceneContextLoader SceneContextLoader { get; set; }
    [Inject]
    public void Construct(SceneContextLoader sceneContextLoader)
    {
        SceneContextLoader = sceneContextLoader;
    }
    
    private void Update()
    {
        
        if (player == null) return;
        
        Vector2Int newTile = GetPlayerTileCoord();
        
        // 現在のタイルが変わった場合
        if (newTile != currentTile)
        {
            isTileChanged = true;
            currentTile = newTile;
        }
    }
    
    private void LateUpdate()
    {
        // タイルが変更された場合、LoadedTilesを呼び出す
        if (isTileChanged && !isUnloading)
        {
            isTileChanged = false;
            _ = LoadedTiles();
        }
    }

    Vector2Int GetPlayerTileCoord()
    {
        if (player == null) return Vector2Int.zero;
        
        Vector3 playerPosition = player.position;
        int tileX = Mathf.FloorToInt(playerPosition.x / tileSize);
        int tileZ = Mathf.FloorToInt(playerPosition.z / tileSize);

        return new Vector2Int(tileX, tileZ);
    }

    public async ValueTask LoadedTiles()
    {
        if (player == null) return;
        if (tileSceneReference == null || tileSceneReference.Length == 0) return;
        
        // 新しいタイルの周囲9マスをロード
        HashSet<Vector2Int> tilesToLoad = new HashSet<Vector2Int>();
        
        // 3x3のグリッドを生成
        for (int x = -loadRadius; x <= loadRadius; x++)
        {
            for (int y = -loadRadius; y <= loadRadius; y++)
            {
                Vector2Int tileToLoad = new Vector2Int(currentTile.x + x, currentTile.y + y);
                tilesToLoad.Add(tileToLoad);
            }
        }
        
        // アンロードすべきタイルを特定
        List<Vector2Int> tilesToUnload = new List<Vector2Int>();
        foreach (var loadedTile in loadedTiles.Keys)
        {
            if (!tilesToLoad.Contains(loadedTile))
            {
                tilesToUnload.Add(loadedTile);
            }
        }
        
        // アンロードすべきタイルをアンロード
        if (tilesToUnload.Count > 0)
        {
            isUnloading = true;
            Debug.Log($"アンロードするタイル数: {tilesToUnload.Count}");
            
            foreach (var tileToUnload in tilesToUnload)
            {
                if (loadedTiles.TryGetValue(tileToUnload, out SceneContext sceneContext))
                {
                    Debug.Log($"タイル ({tileToUnload.x}, {tileToUnload.y}) をアンロードします");
                    await UnloadTile(sceneContext);
                    loadedTiles.Remove(tileToUnload);
                    await Task.Delay(100);
                }
            }
            
            isUnloading = false;
        }
        
        // 各タイルをロード
        foreach (Vector2Int tile in tilesToLoad)
        {
            // すでにロードされているタイルはスキップ
            if (loadedTiles.ContainsKey(tile)) continue;
            
            // グリッドの範囲を0-9に制限
            int gridX = Mathf.Clamp(tile.x, 0, 9);
            int gridY = Mathf.Clamp(tile.y, 0, 9);
            
            // 一次元配列のインデックスを計算
            int tileIndex = gridX + gridY * 10;
            
            // インデックスが配列の範囲内かチェック
            if (tileIndex >= tileSceneReference.Length) continue;
            
            // シーンのロード
            Debug.Log($"タイル ({tile.x}, {tile.y}) をロードします");
            var sceneContext = await SceneContextLoader.LoadAsync(tileSceneReference[tileIndex], active: true);
            
            // ロードしたタイルを記録
            loadedTiles.Add(tile, sceneContext);

            await Task.Delay(100);
            
        }
    }
    
    // タイルをアンロードするメソッド
    private async ValueTask UnloadTile(SceneContext sceneContext)
    {
        if (sceneContext != null)
        {
            try
            {
                await SceneContextLoader.UnloadAsync(sceneContext);
                Debug.Log("タイルのアンロードが完了しました");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"タイルのアンロード中にエラーが発生しました: {e.Message}");
            }
        }
    }
    
    // シーン遷移時などに呼び出されるメソッド
    private void OnDisable()
    {
        // すべてのタイルをアンロード
        _ = UnloadAllTiles();
    }
    
    // すべてのタイルをアンロードするメソッド
    private async ValueTask UnloadAllTiles()
    {
        if (loadedTiles.Count == 0) return;
        
        isUnloading = true;
        Debug.Log($"すべてのタイル ({loadedTiles.Count}個) をアンロードします");
        
        List<SceneContext> contextsToUnload = new List<SceneContext>(loadedTiles.Values);
        loadedTiles.Clear();
        
        foreach (var context in contextsToUnload)
        {
            await UnloadTile(context);
        }
        
        isUnloading = false;
        Debug.Log("すべてのタイルのアンロードが完了しました");
    }
}
