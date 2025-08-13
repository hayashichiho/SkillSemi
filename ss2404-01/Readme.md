# 画像分類（ResNet18）

このプロジェクトは，2次元画像をResNet18でクラス分類するPythonサンプルコードです．
学習・推論・ROC解析まで一連の流れを実装しています．

---

## 概要

指定した画像フォルダから画像を読み込み，ResNet18によるクラス分類を行います．
学習・検証・テスト・ROC解析・損失ログ保存など，画像分類の基本的な流れを網羅しています．
画像はRGBカラー・グレースケールどちらにも対応し，入力チャンネル数は自動判定されます．

---

## 実装内容

- 入力: 画像フォルダ（`input/`，ImageFolder形式．クラスごとにサブフォルダ）
- 出力: 分類結果CSV・ROC曲線PNG・損失ログCSV・学習済みモデル（`output/`フォルダに保存）
- 処理:
    - ResNet18による画像分類
    - 学習・検証・テスト・ROC解析
    - 損失ログ・予測結果保存

---

## 使用技術

![Python Version](https://img.shields.io/badge/Python-3.7%2B-blue)

- Python 3.7以上
- numpy
- pandas
- torch（PyTorch）
- torchvision
- scikit-learn
- matplotlib

---

## ディレクトリ構成

```
ss2404-01/
├── loss_log_20241028160044.csv         # 学習損失ログ
├── Readme.md                           # この説明ファイル
├── resnet18.py                         # ResNet18モデル定義
├── roc.py                              # ROC解析・描画用スクリプト
├── test_classification.py              # テスト・推論・ROC解析スクリプト
├── training_classification.py          # 学習・検証スクリプト
├── training_classification2.py         # 拡張・実験用学習スクリプト
├── input/                              # 入力画像フォルダ（ImageFolder形式）
│   ├── class1/
│   └── class2/
├── output/                             # 結果保存フォルダ
│   ├── model_best_xxxxx.pth
│   ├── test_result_xxxxx.csv
│   ├── test_roc_xxxxx.png
│   └── loss_log_xxxxx.csv
```

---

## 使用方法

1. 入力画像は `input/` フォルダにImageFolder形式（クラスごとにサブフォルダ）で配置してください．

2. コマンドラインから `training_classification.py` で学習を実行します．

    ```bash
    python training_classification.py input/train input/val output/
    ```

3. `test_classification.py` でテスト・ROC解析を実行します．

    ```bash
    python test_classification.py input/test output/model_best_xxxxx.pth output/
    ```

- 第1引数: 学習/検証/テスト用画像フォルダ（ImageFolder形式）
- 第2引数: モデルファイル名（テスト時のみ）
- 第3引数: 出力フォルダ

---

## テスト

- サンプル画像フォルダを用意し，ImageFolder形式（クラスごとにサブフォルダ）で配置してください．
- 学習・検証・テストの各スクリプトを実行し，結果CSV・ROC曲線PNGが `output/` に保存されます．
- エラー処理として，画像が存在しない場合やモデルが読み込めない場合は例外が発生します．

---

## 注意事項

- 入力画像はImageFolder形式（クラスごとにサブフォルダ）で配置してください．
- モデルの入力チャンネル数は自動判定されます．
- 学習・検証・テスト・ROC解析の各スクリプトは引数でパスを指定してください．
- 結果ファイル（CSV, PNG, pth）は `output/` フォルダに保存されます．

---

## 必要なライブラリ

- numpy
- pandas
- torch
- torchvision
- scikit-learn
- matplotlib

インストールコマンド：
```bash
pip install numpy pandas torch torchvision scikit-learn matplotlib
```

---

## ライセンス

このプロジェクトは [MITライセンス](https://opensource.org/licenses/MIT) の下で公開されています．
