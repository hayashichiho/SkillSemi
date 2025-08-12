# 多言語連携・仮想マウスプログラム（ソケット通信＋センサー＋カメラ＋顔認識）

このプロジェクトは、C#・C++・Pythonなど複数言語を連携し、画面上で顔検出，矩形表示を行うアプリケーションです。
各言語間はソケット通信でデータをやり取りし、GUI上でリアルタイムに表示可能です。

---

## 実装内容

- C#フォームアプリによるGUI・仮想マウス・センサー・カメラ連携
- C++/Pythonによるカメラ映像取得・画像処理
- **Python（receiveCamera.py）による顔認識・矩形描画（OpenFace使用）**
- ソケット通信による多言語間データ連携
- シリアル通信によるセンサー値取得
- カメラ映像のリアルタイム表示
- 仮想マウス描画・計測データ管理
- 計測データの保存・読み込み

---

## ディレクトリ構成

```
ss2411/
├── cameraApp.csproj                # C#メインプロジェクトファイル
├── connectDifferentLanguage.csproj # 多言語連携用C#プロジェクト
├── cameraApp.cs                    # C#カメラ連携処理
├── cameraApp.Designer.cs           # C#フォームデザイン
├── Form1.cs                        # メインフォーム（仮想マウス・センサー・カメラ連携）
├── Form1.Designer.cs               # フォームデザイン
├── Program.cs                      # エントリポイント
├── getCamera.cpp                   # C++カメラ取得処理
├── opencv.cpp                      # C++画像処理
├── receiveCamera.py                # Pythonカメラ受信・顔認識・矩形描画
├── CMakeLists.txt                  # C++ビルド設定
├── ss2411-1.vcxproj                # Visual Studio C++プロジェクト
├── README.md                       # この説明ファイル
├── bin/                            # 実行ファイル（ビルド成果物）
│   └── Debug/
├── obj/                            # 一時ファイル
│   └── Debug/
```

---

## 各ファイルの説明

- `cameraApp.cs` / `Form1.cs`: C#によるGUI・仮想マウス・センサー・カメラ連携処理
- `getCamera.cpp` / `opencv.cpp`: C++によるカメラ映像取得・画像処理
- `receiveCamera.py`: **Pythonによるカメラ映像受信・顔認識・矩形描画（OpenFace使用）**
    - 顔検出し、顔部分に矩形を描画してC#側に送信
- `Program.cs`: C#アプリケーションのエントリポイント
- `CMakeLists.txt`, `ss2411-1.vcxproj`: C++ビルド設定
- `cameraApp.csproj`, `connectDifferentLanguage.csproj`: C#プロジェクトファイル

---

## 使用方法

1. 各言語のプロジェクトをビルド（C#はVisual Studio、C++はCMake/VS、Pythonは必要なライブラリをインストール）
2. Python側で `receiveCamera.py` を起動（OpenFaceのshape_predictor_68_face_landmarks.datが必要）
3. C++/Python側でカメラ映像取得サーバを起動
4. C#フォームアプリ（`cameraApp.csproj`）を起動
5. ソケット通信でカメラ映像・センサー値・顔認識結果（矩形付き画像）を受信し、GUI上で仮想マウス描画・計測を操作
6. 計測データの保存・読み込みも可能

---

## 顔認識・矩形表示について

- `receiveCamera.py` でOpenFaceの `AlignDlib` を使い、顔部分を検出
- 検出した顔領域に `cv2.rectangle` で矩形を描画
- 矩形付き画像をC#側に送信し、GUI上で表示

---

## 注意事項

- .NET Framework 4.7.2 以上が必要です（C#側）
- OpenCvSharpやシリアル通信関連のDLLが必要な場合は、事前にインストールしてください
- C++/Python側はOpenCV・OpenFace等のライブラリが必要です
- `shape_predictor_68_face_landmarks.dat` ファイルが必要です（OpenFace用）
- ソケット通信のポート番号やIPアドレスは環境に合わせて設定してください

---

## ライセンス

このプロジェクトは [MITライセンス](https://opensource.org/licenses/MIT) の下で公開されています．
