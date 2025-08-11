#pragma once
#include <vector>
#include <map>
#include <string>
#include <variant>

// 画像処理関数群
std::variant<std::vector<short>, std::vector<unsigned char>>
process_image_data(const std::vector<short> &image_data,
                   const std::vector<std::string> &parameters,
                   const std::map<std::string, std::string> &mhd_info,
                   bool &is_transformed);

std::vector<short> apply_sobel_x(const std::vector<short> &data, int width, int height);
std::vector<short> apply_sobel_y(const std::vector<short> &data, int width, int height);
std::vector<short> apply_moving_average(const std::vector<short> &data, int width, int height, int kernel_size);
std::vector<short> apply_median_filter(const std::vector<short> &data, int width, int height, int kernel_size);
std::vector<unsigned char> window_transform(const std::vector<short> &data, int window_level, int window_width);
