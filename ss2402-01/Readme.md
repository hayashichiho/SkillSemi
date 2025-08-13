# DICOMボリュームデータのMHD/RAW変換実装

このプロジェクトは，DICOM画像（CT/MR等）を3次元ボリュームデータとして読み込み，MHD/RAW形式で保存するPythonスクリプトです．

---

## 概要

指定したフォルダまたはZIPファイル内のDICOM画像を読み込み，Instance Number順に並べて3次元ボリュームデータを生成します．
CT画像の場合はHU値変換・ウィンドウ処理を行い，256階調に正規化してMHD/RAWファイルとして保存します．

---

## 実装内容

- 入力: DICOM画像ファイル群（フォルダまたはZIPファイル）
- 出力: 3次元ボリュームデータ（MHD/RAWファイル）
- オプション: ウィンドウレベル（WL），ウィンドウ幅（WW）指定（CT画像のみ）

---

## 使用技術

![Python Version](https://img.shields.io/badge/Python-3.7%2B-blue)

- Python 3.7以上

---

## 使用方法

1. コマンドラインから `main.py` を実行します．
2. 必要な引数を指定してください．

例:
```bash
python main.py HeadCtSample_2022 out_volume --wl 40 --ww 400
```

- 第1引数: 入力フォルダ名またはZIPファイル名（必須）
- 第2引数: 出力ファイル名（拡張子不要、.mhd/.rawが自動付与）（必須）
- `--wl` : ウィンドウレベル（CT画像のみ、デフォルト40.0）
- `--ww` : ウィンドウ幅（CT画像のみ、デフォルト400.0）

---

## テスト

動作確認には，任意のDICOM画像フォルダまたはZIPファイルを用いて上記コマンドを実行してください．
エラー処理として，入力ファイルが存在しない場合やDICOMファイルが見つからない場合は例外が発生します．

## ImageJでの確認方法

1. ImageJを起動します。
2. メニューから「File」→「Import」→「Raw...」を選択します。
3. ダイアログで出力した `.raw` ファイルを選択します。
4. 以下の設定を入力します（MHDファイルの内容に合わせて設定してください）:
    - **Image type**: 8-bit Unsigned
    - **Width**: MHDファイルの `DimSize` の2番目の値
    - **Height**: MHDファイルの `DimSize` の3番目の値
    - **Number of images**: MHDファイルの `DimSize` の1番目の値
    - **Offset**: 0
    - **Little-endian**: チェックを外す（`ElementByteOrderMSB = False` の場合）
    - **Interleaved**: チェックを外す
5. 「OK」を押すと、3次元画像がImageJで表示されます。

※ 詳細な値は `.mhd` ファイルをテキストエディタで開いて確認してください。

---

## 注意事項

- 入力はDICOM画像（拡張子.dcm）である必要があります．
- CT画像の場合はHU値変換・ウィンドウ処理を行います．
- 出力はMHD/RAW形式（ITK/VTK等で利用可能）です．
- ZIPファイル入力時は一時ディレクトリを自動削除します．

---

## 必要なライブラリ

- numpy
- pydicom

インストールコマンド：
```bash
pip install numpy pydicom
```

---

## ライセンス

このプロジェクトは [MITライセンス](https://opensource.org/licenses/MIT) の下で公開されています．
