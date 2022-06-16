# 環境構築

## 1. CONFIG値を設定する
`Assets/Resources/ApplicationConfigs` を Unity で開きます。

![ApplicationConfigs](ENVIRONMENT_1.webp)

`Version Config` と `Firebase Config` を適切なものに設定します。

---

その後、ファイルが git 検知されないように、差分検知を無効にします。
```shell
git update-index --assume-unchanged Assets/Resources/ApplicationConfigs.asset
```

## 2. Firebaseの設定ファイルを読み込む
配布された `google-services.json` と `GoogleService-Info.plist` を
`Assets` フォルダ内にドラッグ&ドロップします。

![GoogleService](ENVIRONMENT_2.webp)

## 3. KeyStoreを設定する
`File > Build Settings > Player Settings` を開き、
`Player > Android > Publishing Settings > Project KeyStore` を入力する。

![ProjectKeyStore](ENVIRONMENT_3.webp)
