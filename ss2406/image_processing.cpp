#include "image_processing.h"
#include <algorithm>
#include <cmath>
#include <iostream>
#include <sstream>
#include <map>
#include <variant>
#include <vector>
#include <memory>

using namespace std;

/* 画像処理クラス */
ImageProcessing::ImageProcessing(const string& filter_type, int kernel_size)
    : filter_type(filter_type), kernel_size(kernel_size) {}

/* 画像処理を適用 */
vector<short> ImageProcessing::apply(const vector<short>& image_data, int width, int height) {
    if (filter_type == "SobelX") {
        return apply_sobel_x(image_data, width, height);
    } else if (filter_type == "SobelY") {
        return apply_sobel_y(image_data, width, height);
    } else if (filter_type == "MovingAverage") {
        return apply_moving_average(image_data, width, height);
    } else if (filter_type == "Median") {
        return apply_median_filter(image_data, width, height);
    } else {
        return image_data;
    }
}

/* SobelXフィルタを適用 */
vector<short> ImageProcessing::apply_sobel_x(const vector<short>& image_data, int width, int height) {
    vector<short> result(image_data.size());
    int sobel_x[3][3] = {
        {-1, 0, 1},
        {-2, 0, 2},
        {-1, 0, 1}
    };

    for (int y = 1; y < height - 1; ++y) {
        for (int x = 1; x < width - 1; ++x) {
            int sum = 0;
            for (int ky = -1; ky <= 1; ++ky) {
                for (int kx = -1; kx <= 1; ++kx) {
                    sum += image_data[(y + ky) * width + (x + kx)] * sobel_x[ky + 1][kx + 1];
                }
            }
            result[y * width + x] = sum;
        }
    }

    return result;
}

/* SobelYフィルタを適用 */
vector<short> ImageProcessing::apply_sobel_y(const vector<short>& image_data, int width, int height) {
    vector<short> result(image_data.size());
    int sobel_y[3][3] = {
        {-1, -2, -1},
        {0, 0, 0},
        {1, 2, 1}
    };

    for (int y = 1; y < height - 1; ++y) {
        for (int x = 1; x < width - 1; ++x) {
            int sum = 0;
            for (int ky = -1; ky <= 1; ++ky) {
                for (int kx = -1; kx <= 1; ++kx) {
                    sum += image_data[(y + ky) * width + (x + kx)] * sobel_y[ky + 1][kx + 1];
                }
            }
            result[y * width + x] = sum;
        }
    }

    return result;
}

/* 移動平均フィルタを適用 */
vector<short> ImageProcessing::apply_moving_average(const vector<short>& image_data, int width, int height) {
    if(kernel_size % 2 == 0) {
        cerr << "Kernel size must be an odd number" << endl;
        exit(1);
        throw invalid_argument("Kernel size must be an odd number");
    }
    vector<short> result(image_data.size());
    int half_kernel = kernel_size / 2;
    
    for (int y = half_kernel; y < height - half_kernel; ++y) {
        for (int x = half_kernel; x < width - half_kernel; ++x) {
            int sum = 0;
            for (int ky = -half_kernel; ky <= half_kernel; ++ky) {
                for (int kx = -half_kernel; kx <= half_kernel; ++kx) {
                    sum += image_data[(y + ky) * width + (x + kx)];
                }
            }
            result[y * width + x] = sum / (kernel_size * kernel_size);
        }
    }

    return result;
}

/* メディアンフィルタを適用 */
vector<short> ImageProcessing::apply_median_filter(const vector<short>& image_data, int width, int height) {
    if(kernel_size % 2 == 0) {
        cerr << "Kernel size must be an odd number" << endl;
        exit(1);
        throw invalid_argument("Kernel size must be an odd number");
    }
    vector<short> result(image_data.size());
    int half_kernel = kernel_size / 2;

    for (int y = half_kernel; y < height - half_kernel; ++y) {
        for (int x = half_kernel; x < width - half_kernel; ++x) {
            vector<short> neighborhood;
            for (int ky = -half_kernel; ky <= half_kernel; ++ky) {
                for (int kx = -half_kernel; kx <= half_kernel; ++kx) {
                    neighborhood.push_back(image_data[(y + ky) * width + (x + kx)]);
                }
            }
            sort(neighborhood.begin(), neighborhood.end());
            result[y * width + x] = neighborhood[neighborhood.size() / 2];
        }
    }

    return result;
}

/* 階調変換 */
vector<unsigned char> window_transform(const vector<short>& image_data, int window_level, int window_width) {
    vector<unsigned char> transformed_data(image_data.size());
    int min_val = window_level - window_width / 2;
    int max_val = window_level + window_width / 2;

    for (size_t i = 0; i < image_data.size(); ++i) {
        if (image_data[i] <= min_val) {
            transformed_data[i] = 0;
        } else if (image_data[i] >= max_val) {
            transformed_data[i] = 255;
        } else {
            transformed_data[i] = static_cast<unsigned char>(255.0 * (image_data[i] - min_val) / window_width);
        }
    }
    return transformed_data;
}

/* 画像データの処理 */
variant<vector<short>, vector<unsigned char>> process_image_data(
    const vector<short>& image_data,
    const vector<string>& parameters,
    const map<string, string>& mhd_info,
    bool& is_transformed
) {
    bool window_processing = false;
    int window_level = 0;
    int window_width = 0;
    string image_processing;
    int moving_average_kernel_size = 3;
    int median_filter_kernel_size = 3;

    /* パラメータの値を取得 */
    for (const auto& param : parameters) {
        istringstream iss(param);
        string key, value;
        if (getline(iss, key, '=') && getline(iss, value)) {
            key.erase(key.find_last_not_of(" \n\r\t") + 1); // 余分なスペースを削除
            value.erase(0, value.find_first_not_of(" \n\r\t")); // 先頭のスペースを削除
            value.erase(value.find_last_not_of(" \n\r\t") + 1); // 末尾のスペースを削除

            if (key == "WindowProcessing" && value == "true") {
                window_processing = true;
            } else if (key == "WindowLevel") {
                window_level = stoi(value);
            } else if (key == "WindowWidth") {
                window_width = stoi(value);
            } else if (key == "ImageProcessing") {
                image_processing = value;
            } else if (key == "MovingAverageFilterKernel") {
                moving_average_kernel_size = stoi(value);
            } else if (key == "MedianFilterKernel") {
                median_filter_kernel_size = stoi(value);
            }
        }
    }

    /* mhd情報を取得 */
    int width = 0;
    int height = 0;
    try {
        istringstream iss(mhd_info.at("DimSize"));
        iss >> width >> height;
    } catch (const out_of_range& e) {
        cerr << "DimSize not found in mhd file" << endl;
        throw;
    } catch (const invalid_argument& e) {
        cerr << "Invalid DimSize value in mhd file" << endl;
        throw;
    }

    /* 画像処理を適用 */
    int kernel_size = (image_processing == "MovingAverage") ? moving_average_kernel_size : median_filter_kernel_size;
    ImageProcessing processor(image_processing, kernel_size);
    vector<short> processed_image_data = processor.apply(image_data, width, height);

    /* 階調変換を行う */
    if (window_processing) {
        is_transformed = true;
        return window_transform(processed_image_data, window_level, window_width);
    } else {
        is_transformed = false;
        return processed_image_data;
    }
}