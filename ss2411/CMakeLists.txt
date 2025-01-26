cmake_minimum_required(VERSION 3.10)

# プロジェクト名と使用するC++のバージョンを設定
project(ss2411-1)
set(CMAKE_CXX_STANDARD 14)

# OpenCVのパスを設定
set(OpenCV_DIR "C:/opencv/build")

# OpenCVを見つける
find_package(OpenCV REQUIRED)

# ZeroMQのパスを設定
set(ZeroMQ_DIR "C:/Users/Owner/source/repos/SkillSemi2024/ss2411-1/vcpkg/installed/x64-windows")

# ZeroMQを見つける
find_path(ZeroMQ_INCLUDE_DIR zmq.hpp PATHS ${ZeroMQ_DIR}/include)
find_library(ZeroMQ_LIBRARY NAMES libzmq-mt-4_3_5 PATHS ${ZeroMQ_DIR}/lib)

# インクルードディレクトリを設定
include_directories(${OpenCV_INCLUDE_DIRS} ${ZeroMQ_INCLUDE_DIR})

# 実行ファイルを追加
add_executable(ss2411-1 opencv.cpp)

# リンクするライブラリを設定
target_link_libraries(ss2411-1 ${OpenCV_LIBS} ${ZeroMQ_LIBRARY})
