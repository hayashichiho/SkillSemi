cmake_minimum_required(VERSION 3.10)
project(ImageProcessing)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# ヘッダファイルのディレクトリを追加
include_directories(${CMAKE_SOURCE_DIR}/include)

# 実行ファイルの生成
add_executable(ImageProcessing
    src/main.cpp
    src/path.cpp
    src/image_processing.cpp
)
