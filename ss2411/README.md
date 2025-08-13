# 多言語連携・顔認識アプリケーション

このプロジェクトは、**C#・C++・Python**の複数言語を連携し、カメラ映像の取得・顔検出・矩形表示をリアルタイムで行うアプリケーションです。
各言語間はソケット通信でデータをやり取りし、C#のGUI上で表示・操作できます。

---

## 使用技術・主なライブラリ

- **使用言語**
  - C#（.NET Framework 4.7.2以上、Windows Forms）
  - C++（OpenCV利用、カメラ映像取得・画像処理）
  - Python（OpenCV, dlib, socket通信、顔認識・矩形描画）

- **主なライブラリ・技術**
  - C#
    - OpenCvSharp（C#用OpenCVラッパー、カメラ映像の取得・画像処理）
    - System.IO.Ports（シリアル通信）
    - System.Windows.Forms（GUI）
    - System.Net.Sockets（ソケット通信）
  - C++
    - OpenCV（画像処理・カメラ映像取得）
    - CMake（ビルド管理）
  - Python
    - OpenCV (`cv2`)
    - dlib（顔検出・ランドマーク抽出）
    - socket（通信）
    - numpy

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

## 主な機能

- C#フォームアプリによるGUI・仮想マウス・センサー・カメラ連携
- C++/Pythonによるカメラ映像取得・画像処理
- Python（receiveCamera.py）による顔認識・矩形描画（dlib, OpenCV使用）
- ソケット通信による多言語間データ連携
- カメラ映像のリアルタイム表示

---

## 実行方法

1. 各言語のプロジェクトをビルド
   - C#はVisual Studioで`cameraApp.csproj`または`connectDifferentLanguage.csproj`を開いてビルド
   - C++は`CMakeLists.txt`または`ss2411-1.vcxproj`でビルド
   - Pythonは必要なライブラリ（`opencv-python`, `dlib`, `numpy`など）をインストール
     ```sh
     pip install opencv-python dlib numpy
     ```
2. Python側で `receiveCamera.py` を起動（`shape_predictor_68_face_landmarks.dat`が必要）
3. C++/Python側でカメラ映像取得サーバを起動
4. C#フォームアプリ（`cameraApp.csproj`）を起動
5. ソケット通信でカメラ映像・顔認識結果（矩形付き画像）を受信し、GUI上で表示

---

## 注意事項

- .NET Framework 4.7.2 以上が必要です（C#側）
- OpenCvSharpやシリアル通信関連のDLLが必要な場合は、事前にインストールしてください
- C++/Python側はOpenCV・dlib等のライブラリが必要です
- `shape_predictor_68_face_landmarks.dat` ファイルが必要です（dlib用）
- ソケット通信のポート番号やIPアドレスは環境に合わせて設定してください

---

## ライセンス

このプロジェクトは [MITライセンス](https://opensource.org/licenses/MIT) の下で公開されています．
