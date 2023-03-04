# FirestoreHandler

## 使い方

```csharp
// クラスの初期化
var fs = new FirestoreHandler();

// エラー発生時に実行する処理を登録
fs.OnErrorOccured += error =>
{
    // TODO: エラーをユーザに伝える
    Debug.Log(error);
};
```

## メソッド

---

### (bool) サポートされているバージョンかどうかを取得する

```csharp
public IEnumerator GetIsSupportedVersionCoroutine(string version)
```

使用例

```csharp
var version = "2.0.0";

// Coroutine を格納
var iEnumerator = fs.GetIsSupportedVersionCoroutine(version);

// 非同期処理の実行を待機
yield return iEnumerator;

// 実行結果を格納
var isValidVersion = iEnumerator.Current != null && (bool)iEnumerator.Current;
```

---

### (Music[]) 楽曲一覧を取得する

```csharp
public IEnumerator GetMusicListCoroutine()
```

使用例

```csharp
// Coroutine を格納
var iEnumerator = fs.GetMusicListCoroutine();

// 非同期処理の実行を待機
yield return StartCoroutine(iEnumerator);

// 実行結果を格納
var musicList = (Music[])iEnumerator.Current;
```
