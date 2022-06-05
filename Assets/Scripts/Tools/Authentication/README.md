# AuthenticationHandler

## 使い方

```csharp
// クラスの初期化
private readonly AuthenticationHandler _auth = new();

private void Start()
{
    // Start()で呼び出す必要あり
    _auth.Start();

    // エラー発生時に実行する処理を登録
    _auth.OnErrorOccured += error =>
    {
        // TODO: エラーをユーザに伝える
        Debug.Log(error);
    };
}

private void Update()
{
    // Update()で呼び出す必要あり
    _auth.Update();
}

private void OnDestroy()
{
    // OnDestroy()で呼び出す必要あり
    _auth.OnDestroy();
}
```

## メソッド

---

### (void) Appleでサインイン

```csharp
public void SignInWithApple()
```

使用例

```csharp
_auth.SignInWithApple();
```

---

### (void) Googleでサインイン

```csharp
public void SignInWithGoogle()
```

使用例

```csharp
_auth.SignInWithGoogle();
```

---

### (void) 匿名認証でサインイン

```csharp
public void SignInWithAnonymously()
```

使用例

```csharp
_auth.SignInWithAnonymously();
```

---

### (void) ユーザ名を更新する

```csharp
public void UpdateDisplayName(string displayName)
```

使用例

```csharp
_auth.UpdateDisplayName("太郎");
```

---

### (void) サインアウト

```csharp
public void SignOut()
```

使用例

```csharp
_auth.SignOut();
```

---

### (FirebaseUser) 認証済みのユーザを取得する

※ ユーザ認証後に呼び出す必要があります

```csharp
public FirebaseUser GetUser()
```

使用例

```csharp
var user = _auth.GetUser();
```
