# CMakeLists.txt
cmake_minimum_required(VERSION 3.10)
project(MIPGenerator)

# C++17の設定
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# コンパイラ最適化フラグの設定
set(CMAKE_CXX_FLAGS_RELEASE "-O3 -march=native -DNDEBUG")
set(CMAKE_BUILD_TYPE Release)

# OpenMPの検索
find_package(OpenMP REQUIRED)

# ソースファイルの設定
set(SOURCES
    main.cpp
    mip.cpp
    path.cpp
)

# 実行ファイルの設定
add_executable(MIPGenerator ${SOURCES})

# OpenMPのリンク
target_link_libraries(MIPGenerator
    PRIVATE
        OpenMP::OpenMP_CXX
)
