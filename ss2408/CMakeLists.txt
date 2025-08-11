cmake_minimum_required(VERSION 3.10)
project(MIPGenerator)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# OpenMPを使用する場合
find_package(OpenMP)
if(OpenMP_CXX_FOUND)
    add_compile_options(-fopenmp)
endif()

# ソースファイル一覧
set(SOURCES
    src/main.cpp
    src/ProcessingInfo.cpp
    src/ImageInfo.cpp
    src/ImageInfo2D.cpp
    src/ImageInfo3D.cpp
    src/Coordinate3D.cpp
    src/ProcessingInfoParser.cpp
    src/MIPGenerator.cpp
    src/Outputs.cpp
)

include_directories(include)

add_executable(MIPGenerator ${SOURCES})

if(OpenMP_CXX_FOUND)
    target_link_libraries(MIPGenerator OpenMP::OpenMP_CXX)
endif()
