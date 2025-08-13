# arucoマーカを用いた距離計測アプリケーション

このプロジェクトは，arucoマーカーを活用し，GUI上で計測・キャリブレーション・データ管理を行うプログラムです．

---

## 実装内容

- arucoマーカーを用いたカメラキャリブレーション
- 画像からの座標計測・距離計算
- 計測データの保存・読み込み
- GUIによる操作（フォームアプリケーション）
- 設定ファイル（App.config）によるパラメータ管理

---

## 使用技術・必要なライブラリ

- **言語・環境**:
  - C# (.NET Framework 4.7.2 以上)
  - Windows Forms（GUIアプリケーション）
- **画像処理・マーカー検出**:
  - OpenCV（C#ラッパー：OpenCvSharpなど）
  - aruco（OpenCVのマーカー検出モジュール）

### 必要なライブラリ・インストール方法

- **OpenCvSharp**（C#用OpenCVラッパー）
  - NuGetでインストール
    ```
    Install-Package OpenCvSharp4
    Install-Package OpenCvSharp4.Windows
    ```
- **.NET Framework 4.7.2**
  - Visual Studioのプロジェクト設定で指定

---

## ディレクトリ構成

```
ss2409/
├── .gitignore                   # Git管理用
├── App.config                   # アプリケーション設定ファイル
├── README.md                    # この説明ファイル
├── ss2409-01.csproj             # プロジェクトファイル
├── aruco.cs                     # arucoマーカー処理
├── Calibration.cs               # キャリブレーション処理
├── Form1.cs                     # メインフォーム
├── Form1.Designer.cs            # フォームデザイン
├── ico.py                       # アイコン生成（Pythonスクリプト）
├── loadData.cs                  # データ読み込み処理
├── MainForm.cs                  # メインフォーム処理
├── measure.cs                   # 計測処理
├── Program.cs                   # エントリポイント
├── .Designer.cs                 # デザイナー自動生成ファイル
├── bin/                         # 実行ファイル（ビルド成果物）
│   └── Debug/
├── obj/                         # 一時ファイル
│   └── Debug/
└── .vs/                         # Visual Studio管理ファイル
```

---

## 各ファイルの説明

- `Program.cs`: アプリケーションのエントリポイント
- `Form1.cs` / `MainForm.cs`: メイン画面・GUI処理
- `aruco.cs`: arucoマーカー検出・処理
- `Calibration.cs`: カメラキャリブレーション処理
- `measure.cs`: 画像計測・距離計算
- `loadData.cs`: 計測データの読み込み・保存
- `App.config`: 設定ファイル（パラメータ管理）
- `ico.py`: アイコン生成用スクリプト（必要に応じて利用）

---

## 使用方法

1. Visual Studioでプロジェクト（`ss2409-01.csproj`）を開く
2. NuGetでOpenCvSharpなど必要なライブラリをインストール
3. `bin/Debug/` フォルダにビルドし、実行ファイルを起動
4. GUI上で画像ファイルを選択し、キャリブレーションや計測を実行
5. 計測結果やキャリブレーションデータは保存・読み込み可能

---

## 注意事項

- .NET Framework 4.7.2 以上が必要です
- OpenCvSharpやaruco関連のDLLが必要な場合は、事前にインストールしてください
- 計測やキャリブレーションの手順はGUI上の説明やマニュアルを参照してください

---

## ライセンス

このプロジェクトは [MITライセンス](https://opensource.org/licenses/MIT) の下で公開されています．
