cmake_minimum_required(VERSION 3.10)
project(MIPGenerator)

# C++17 を使用
set(CMAKE_CXX_STANDARD 17)

# 実行ファイルを追加
add_executable(MIPGenerator main.cpp mip.cpp path.cpp)

find_package(OpenCV REQUIRED)
target_link_libraries(MIPGenerator ${OpenCV_LIBS})
