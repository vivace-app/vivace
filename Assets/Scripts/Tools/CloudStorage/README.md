# CloudStorageHandler

## 使い方

```csharp
// クラスの初期化
var cs = new CloudStorageHandler();

// エラー発生時に実行する処理を登録
cs.OnErrorOccured += error =>
{
    // TODO: エラーをユーザに伝える
    Debug.Log(error);
};
```

## メソッド

---

### (Uri) パスからCloudStorageのURLを発行する

```csharp
public IEnumerator GenerateDownloadURL(string path)
```

使用例

```csharp
var path = "asset_bundle/burning_heart/v2/ios/burning_heart";

// Coroutine を格納
var iEnumerator = fs.GenerateDownloadURL(path);

// 非同期処理の実行を待機
yield return iEnumerator;

// 実行結果を格納
var url = (Uri)ie.Current;
```
