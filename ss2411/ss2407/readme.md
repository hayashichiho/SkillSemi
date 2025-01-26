# 課題内容
3次元CT画像を読み込んで，Tricubicで等方化を行う．

## 使用方法

1. `build`ディレクトリを作成し，移動する．
    ```sh
    mkdir build
    cd build
    ```

2. CMakeを使用してプロジェクトを構成する．
    ```sh
    cmake ..
    ```

3. プロジェクトをビルドする．
    ```sh
    cmake --build .
    ```

4. プログラムを実行します．
    ```sh
    ./ImageProcessing /home/hayashi/SkillSemi2024/SkillSemi2024/ss2407/ss2407_data/ChestCT.mhd /home/hayashi/SkillSemi2024/SkillSemi2024/ss2407/ss2407_data/ChestCT_output.mhd 分解能（0より大きい値，任意）
    ```


## ファイルの説明

- `CMakeLists.txt`: CMakeファイル
- `build/`: ビルドした結果が格納されるディレクトリ
- `main.cpp`: メインファイル
- `path.cpp` / `path.h`: パス関連の処理を行うファイル
- `tricubic_interpolation.cpp` / `tricubic_interpolation.h`: トリキュービック補間の計算ファイル
- `tricubic_processing.cpp` / `tricubic_processing.h`: トリキュービック補間の実装ファイル

## クラスと関数の説明

### TricubicInterpolation クラス

- `float h(float t, float a)`: h(t)関数の実装．
- `float matrix_interpolate(const float src[4][4], float t_x, float t_y, float a)`: 1次元Cubic補間を実施する関数．
- `float interpolate(const std::vector<short>& data, int width, int height, int depth, float x, float y, float z, float a)`: 3次元Tricubic補間のメイン関数．

### TricubicProcessing クラス

- `static std::vector<short> perform_isotropic_resampling(const std::vector<short>& input_data, int width, int height, int depth, float new_width, float new_height, float new_depth, float a, int interpolation_method)`: トリキュービック補間の実装をする関数．