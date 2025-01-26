# DICOM画像処理
 
![Python Version](https://img.shields.io/badge/Python-3.7%2B-blue)
 
このプロジェクトは，指定したフォルダのDICOMファイル（*.dcm）を読み込み，3次元データを作成するプログラムを実装したpythonスクリプトである．
 
## 概要
 
DICOM形式は，医用画像の標準規格であり，ファイル内に画素データだけでなく，患者情報や検査情報も含めることができる．

## 課題内容

指定したフォルダのDICOMファイルを読み込み，3次元データを作成する．

## 使用技術

- Python 3.7以上
- Numpy
- pydicom


## 使用方法

コマンドラインから以下の形式で実行する:
python ss2402-01.py 入力フォルダ名 出力ファイル名 --wl（任意） 階調範囲の中心値 --ww（任意） 階調範囲の幅

例：
python ss2402-01.py /home/hayashi/SkillSemi2024/ss2402-01/head.zip out_file --wl 40 --ww 100

この例では，head.zipを読み込み，指定の階調で画像処理をし，out_file.mhdとout_file.rawを出力する．

## エラー処理
スクリプトは以下のような状況で適切なエラーメッセージを表示する：
- 入力フォルダが見つからなかったとき
- DICOMファイルを読み込めなかったとき
- zipファイルが有効でなかったとき
- その他の予期せぬエラー
