# 顔画像処理GUIアプリケーション

このプロジェクトは，顔画像の検出・加工・保存をGUIで直感的に操作できるPythonアプリケーションです

---

## 実装内容

- 顔画像の検出
- 顔画像検出結果の保存（PNG画像・CSV座標）
- 顔領域へのモザイク・ぼかし処理
- 顔領域の色黒（日焼け）加工
- 他画像の顔と入れ替え

---

## 使用技術

![Python Version](https://img.shields.io/badge/Python-3.7%2B-blue)

- Python 3.7以上
- numpy
- OpenCV
- tkinter
- Pillow（PIL）

---

## ディレクトリ構成

```
ss2403/
├── face_detection_yunet_2023mar.onnx   # 顔検出モデル
├── face_detector.py                    # 顔検出クラス
├── face_processing.py                  # 顔画像処理クラス
├── face_utils.py                       # 処理・保存ユーティリティ
├── gui_app.py                          # GUIアプリ本体
├── main.py                             # 実行スクリプト
├── Readme.md                           # この説明ファイル
├── images/                             # サンプル画像フォルダ
│   └── image.jpg
├── results/                            # 処理結果保存フォルダ
│   ├── result.png
│   └── result.csv
└── __pycache__/                        # Pythonキャッシュ
```

---

## 使用方法

1. コマンドラインから `main.py` を実行します．

    ```bash
    python main.py
    ```

2. GUI画面が表示されます．

3. [Select Image] ボタンで画像（PNG/JPG/JPEG）を選択します．

4. 各処理（Mosaic, Blur, Blown Skin, Change Face）はスライダーやボタンで操作できます．

5. [Save Results] ボタンで処理結果（画像・座標）を `results` フォルダに保存します．

---

## テスト

- サンプル画像（`images/` フォルダ内）を使って動作確認できます．
- 顔検出・モザイク・ぼかし・色黒・顔入れ替えなど各機能をGUIから試してください．
- 結果は `results/` フォルダに保存されます（PNG画像とCSV座標）．

---

## 注意事項

- 顔検出モデル（`face_detection_yunet_2023mar.onnx`）が必要です．
- 入力画像はPNG/JPG/JPEG形式に対応しています．
- 顔が検出できない場合や画像が読み込めない場合はエラーメッセージが表示されます．
- 処理結果は `results` フォルダに保存されます．フォルダがなければ自動作成されます．

---

## 必要なライブラリ

- numpy
- opencv-python
- pillow

インストールコマンド：
```bash
pip install numpy opencv-python pillow
```

---

## ライセンス

このプロジェクトは [MITライセンス](https://opensource.org/licenses/MIT) の下で公開されています．
