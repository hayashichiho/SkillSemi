# HackerRank C++問題解答集

このプロジェクトは，[HackerRank C++ドメイン](https://www.hackerrank.com/domains/cpp)の問題を解いたC++コード集です．
各ファイルは個別の問題に対応しており，標準入力・出力やSTL・例外処理・継承・仮想関数などC++の基本機能を活用しています．

---

## 実装内容

- attribute_parser.cpp：属性パーサ問題の解答
- deque_stl.cpp：STLデック操作問題の解答
- exceptional_server.cpp：例外処理サーバ問題の解答
- inherited_code.cpp：継承コード問題の解答
- vertual_functions.cpp：仮想関数問題の解答

---

## 使用技術

- C++11以上（標準ライブラリ `<iostream>`, `<vector>`, `<string>` など）
- 問題ごとにSTLや例外処理，クラス・継承・仮想関数などC++の基本機能を利用

---

## 使用方法

1. 各ファイル（例：`attribute_parser.cpp`）をC++11以上対応のコンパイラでビルドしてください．

    ```bash
    g++ -std=c++11 attribute_parser.cpp -o attribute_parser
    ```

2. 実行し，問題の入力例に従って標準入力からデータを与えてください．

    ```bash
    ./attribute_parser
    ```

3. 他のファイルも同様にビルド・実行できます．

---

## テスト

- HackerRankの各問題ページに記載された入力例・出力例で動作確認できます．
- 標準入力・出力を使うため，コマンドラインやファイルリダイレクトでテスト可能です．

---

## 注意事項

- 各ファイルは独立した問題の解答です．必要に応じて個別にビルド・実行してください．
- C++11以上のコンパイラが必要です（g++, clang++, MSVCなど）．
- 問題の仕様に合わせて，標準入力・出力やクラス設計がされています．

---

## 必要なライブラリ

- 標準C++ライブラリ（追加インストール不要）

---

## ライセンス

このプロジェクトは [MITライセンス](https://opensource.org/licenses/MIT) の下で公開されています．
