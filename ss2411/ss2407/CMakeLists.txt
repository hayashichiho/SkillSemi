cmake_minimum_required(VERSION 3.10)
project(ImageProcessing)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

add_executable(ImageProcessing main.cpp path.cpp tricubic_processing.cpp tricubic_interpolation.cpp)
