# スキルゼミ2024 Python深層学習 クラス分類 サンプルコード

ResNet18による2次元画像のクラス分類のサンプルコードです。

## 準備するデータ

- 元画像：`PIL`で読込画像形式であればpng画像(RGBカラー、8 bitグレースケールどちらも対応)
  - 学習、テスト(推論)データとも画像ファイルのチャンネル数より `Resnet18` モデルの入力チャンネル数を自動設定するようにしています。

### 画像の配置方法

- `negative` (陰性、正常)、`positive` (陽性、異常) サブフォルダを作成し、それぞれの画像を配置してください（これにより、。

       (data path)
         |- negative
         |    |- image000.png
         |    |- image001.png
         |    |- image002.png
         |
         |- positive
              |- image003.png
              |- image004.png
              |- image005.png

## ResNet-18モデル

- ResNetの論文[He K, CVPR 2016]に準じたモデルです(convolution, batch normalization, ReLUの順番は実装により異ります)。基本的な使い方は以下の通りです。

      from resnet18 import ResNet18
     
      model = ResNet18(in_channels, out_channels)

  - in_channels: 入力チャンネル数(グレースケール画像の場合は1, RGBカラー画像の場合は3)
  - out_channels: 出力チャンネル数(=クラス数)。

## 学習方法

学習は `training_classification.py` を使用します。コマンドの使い方は以下の通りです。

     $ python training_classification.py [学習用データのパス名] [検証用データのパス名] [出力のパス名] (オプション) 

- オプションについて
  - `--gpu_id (-g)`: GPU ID。複数GPU搭載のマシンを使用しない場合は設定不要(デフォルトの0を指定)。詳細は各研究室の先輩、教員に聞いてください。
  - `--learning_rate (-l)`: 学習率(デフォルト:0.001)
  - `--beta_1 (-l)`: Adam(Optimizer)のパラメータbeta_1(デフォルト:0.99、基本的には 0.9 - 0.99 の値を使用します)。
  - `--batch_size (-b)`: バッチサイズ(デフォルト:8)
  - `--max_epoch_num (-m)`: 学習(最大)エポック数(デフォルト:50)
  - `--time_stamp`: 出力されるファイル名に使われるタイムスタンプです。未指定の場合はプログラム開始時の時刻(YYYYMMDDhhmmss形式)が記載されます。

- 出力ファイル
  - `loss_log_(time stamp).csv`
    - 各エポックでの学習データと検証用データに対する損失関数の出力が書かれています(CSV形式)。このデータを使うことで学習曲線を描くことができます。
  - `model_best_(time stamp).pth`
    - 学習後のモデルデータです。検証用データに対する損失関数の出力が最小となったエポックでのモデルが保存されます。
  - `validation_roc_(time stamp).png`
    - 上記モデルが保存されたエポックでの検証用データに対するROC曲線が保存されます。

## テスト(推論)方法

テスト(推論)は `test_classification.py` を使用します。コマンドの使い方は以下の通りです。

     $ python test_classification.py [テスト用データのパス名] [モデルのファイル名] [出力のパス名] (オプション) 

- オプションについて
  - `--gpu_id (-g)`: GPU ID。複数GPU搭載のマシンを使用しない場合は設定不要(デフォルトの0を指定)。詳細は各研究室の先輩、教員に聞いてください。
  - `--time_stamp`: 出力されるファイル名に使われるタイムスタンプです。未指定の場合はプログラム開始時の時刻(YYYYMMDDhhmmss形式)が記載されます。

- 出力ファイル
  - `test_result_th(閾値)_(time stamp).csv`
    - テストに使用した各データの出力マスクと正解マスクとのDice係数が記載されています。最初の列のインデックスは出力マスク画像のインデックスと対応しています。
  - `test_roc_(time stamp).png`
    - 上記モデルが保存されたエポックでの検証用データに対するROC曲線が保存されます。

## フォルダ内のファイルの説明

- `resnet18.py`: ResNet18 モデルのクラスファイルです。
- `roc.py`: ROC解析の関数が書かれています。
- `test_segmentation.py`: テスト(推論)用コードです。
- `training_segmentation.py`: 学習用コードです。

## 課題によって各自で対応が必要な事項

1. ハイパーパラメータ探索
    - ランダムサーチ等で決定するパラメータはtrain()関数実行前に決定してください(再現性確保のため、taing()関数の冒頭で乱数のseedを固定しています)。別モジュールにすることをお勧めします。

1. Data augmentation
    - data augmentationに関するコードは書いていません。必要に応じて追加してください。

1. 評価結果の混同行列などの作成
    - 評価結果の混同行列などを出力する場合はコードの修正が必要です。cutoff pointはYouden indexにより求めています。  
 
