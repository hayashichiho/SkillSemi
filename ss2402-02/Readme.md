# CT/MR画像の体幹抽出（NIfTI形式）

このプロジェクトは，CT/MR画像（NIfTI形式）から体幹領域のみを抽出し，NIfTI形式で保存するPythonスクリプトです．
抽出した体幹領域は画素値255（体幹），0（その他）でnp.uint8型として保存されます．

---

## 概要

指定したNIfTI画像（CT/MR）から体幹領域のみを抽出します．
背面の寝台やノイズ領域を除去し，体幹領域のみを抽出したマスク画像をNIfTI形式で出力します．
抽出処理は各断面ごとに自動閾値処理・モルフォロジー処理・連結要素解析を行い，2Dラベリングで最大領域（体幹）を抽出します．

---

## 実装内容

- 入力: CT/MR画像（NIfTI形式）
- 出力: 体幹領域マスク画像（NIfTI形式, np.uint8, 体幹=255, その他=0）
- 処理: 各断面ごとに自動閾値処理・モルフォロジー処理・連結要素解析（2D最大領域抽出）

---

## 使用技術

![Python Version](https://img.shields.io/badge/Python-3.7%2B-blue)

- Python 3.7以上
- numpy
- scikit-image
- scipy.ndimage
- SimpleITK
- matplotlib（ヒストグラムや中間画像表示用）

---

## 使用方法

1. コマンドラインから `main.py` を実行します．
2. 必要な引数を指定してください．

例:
```bash
python main.py input.nii output.nii
```

- 第1引数: 入力NIfTIファイル名（必須）
- 第2引数: 出力NIfTIファイル名（必須）

---

## テスト

動作確認には，任意のCT/MR画像（NIfTI形式）を用いて上記コマンドを実行してください．
エラー処理として，入力ファイルが存在しない場合や画像の読み込みに失敗した場合は例外が発生します．

---

## ImageJでの確認方法

1. ImageJを起動します．
2. メニューから「File」→「Import」→「NIfTI-Analyze...」を選択します．
3. ダイアログで出力した `.nii` ファイルを選択します．
4. 「OK」を押すと，抽出結果（体幹マスク）がImageJで表示されます．
   - 体幹領域は白（255），その他は黒（0）で表示されます．
   - 表示が暗い場合は「Image → Adjust → Brightness/Contrast」で調整してください．

---

## 注意事項

- 入力画像はNIfTI形式（拡張子 .nii, .nii.gz）である必要があります．
- 出力画像は体幹領域のみを255，それ以外を0としたnp.uint8型のマスク画像です．
- 閾値処理は固定値（例：-700）ですが，画像によって調整が必要な場合があります．
- モルフォロジー処理・ラベリングはscipy.ndimage, scikit-imageを使用しています．
- 中間画像やヒストグラム表示にはmatplotlibを利用できます．

---

## 必要なライブラリ

- numpy
- scikit-image
- scipy
- SimpleITK
- matplotlib

インストールコマンド：
```bash
pip install numpy scikit-image scipy SimpleITK matplotlib
```

---

## ライセンス

このプロジェクトは [MITライセンス](https://opensource.org/licenses/MIT) の下で公開されています．
