#include "image_processing.h"
#include <algorithm>
#include <cmath>
#include <iostream>
#include <sstream>
#include <map>
#include <variant>
#include <vector>

using namespace std;

// 画像データの処理（フィルタ・階調変換などをパラメータに従って実行）
std::variant<std::vector<short>, std::vector<unsigned char>>
process_image_data(const std::vector<short> &image_data,
                   const std::vector<std::string> &parameters,
                   const std::map<std::string, std::string> &mhd_info,
                   bool &is_transformed)
{
    // パラメータ初期化
    std::string proc_type = "None";
    int window_level = 0, window_width = 0;
    int moving_kernel = 3, median_kernel = 3;
    bool window_proc = false;
    int width = 0, height = 0;

    // パラメータファイルから各種設定値を取得
    for (const auto &param : parameters)
    {
        std::istringstream iss(param);
        std::string key, value;
        if (getline(iss, key, '=') && getline(iss, value))
        {
            // 前後の空白除去
            key.erase(key.find_last_not_of(" \n\r\t") + 1);
            value.erase(0, value.find_first_not_of(" \n\r\t"));
            value.erase(value.find_last_not_of(" \n\r\t") + 1);

            // 各種パラメータの判定
            if (key == "WindowProcessing" && value == "true")
                window_proc = true;
            else if (key == "WindowLevel")
                window_level = stoi(value);
            else if (key == "WindowWidth")
                window_width = stoi(value);
            else if (key == "ImageProcessing")
                proc_type = value;
            else if (key == "MovingAverageFilterKernel")
                moving_kernel = stoi(value);
            else if (key == "MedianFilterKernel")
                median_kernel = stoi(value);
        }
    }
    // MHDファイルから画像サイズを取得
    auto it = mhd_info.find("DimSize");
    if (it != mhd_info.end())
    {
        std::istringstream iss(it->second);
        iss >> width >> height;
    }

    // フィルタ処理の実行
    std::vector<short> result = image_data;
    if (proc_type == "SobelX")
        result = apply_sobel_x(result, width, height);
    else if (proc_type == "SobelY")
        result = apply_sobel_y(result, width, height);
    else if (proc_type == "MovingAverage")
        result = apply_moving_average(result, width, height, moving_kernel);
    else if (proc_type == "Median")
        result = apply_median_filter(result, width, height, median_kernel);

    // 階調変換の有無で返す型を変更
    if (window_proc)
    {
        is_transformed = true;
        return window_transform(result, window_level, window_width);
    }
    else
    {
        is_transformed = false;
        return result;
    }
}

// Sobelフィルタ（X方向）を適用
std::vector<short> apply_sobel_x(const std::vector<short> &data, int width, int height)
{
    std::vector<short> out(data.size(), 0);
    for (int y = 1; y < height - 1; ++y)
    {
        for (int x = 1; x < width - 1; ++x)
        {
            int idx = y * width + x;
            int gx = -data[idx - width - 1] - 2 * data[idx - 1] - data[idx + width - 1] + data[idx - width + 1] + 2 * data[idx + 1] + data[idx + width + 1];
            out[idx] = std::clamp(gx, -32768, 32767);
        }
    }
    return out;
}

// Sobelフィルタ（Y方向）を適用
std::vector<short> apply_sobel_y(const std::vector<short> &data, int width, int height)
{
    std::vector<short> out(data.size(), 0);
    for (int y = 1; y < height - 1; ++y)
    {
        for (int x = 1; x < width - 1; ++x)
        {
            int idx = y * width + x;
            int gy = -data[idx - width - 1] - 2 * data[idx - width] - data[idx - width + 1] + data[idx + width - 1] + 2 * data[idx + width] + data[idx + width + 1];
            out[idx] = std::clamp(gy, -32768, 32767);
        }
    }
    return out;
}

// 移動平均フィルタを適用
std::vector<short> apply_moving_average(const std::vector<short> &data, int width, int height, int kernel_size)
{
    std::vector<short> out(data.size(), 0);
    int k = kernel_size / 2;
    for (int y = k; y < height - k; ++y)
    {
        for (int x = k; x < width - k; ++x)
        {
            int sum = 0;
            for (int dy = -k; dy <= k; ++dy)
                for (int dx = -k; dx <= k; ++dx)
                    sum += data[(y + dy) * width + (x + dx)];
            out[y * width + x] = sum / (kernel_size * kernel_size);
        }
    }
    return out;
}

// メディアンフィルタを適用
std::vector<short> apply_median_filter(const std::vector<short> &data, int width, int height, int kernel_size)
{
    std::vector<short> out(data.size(), 0);
    int k = kernel_size / 2;
    std::vector<short> window;
    for (int y = k; y < height - k; ++y)
    {
        for (int x = k; x < width - k; ++x)
        {
            window.clear();
            for (int dy = -k; dy <= k; ++dy)
                for (int dx = -k; dx <= k; ++dx)
                    window.push_back(data[(y + dy) * width + (x + dx)]);
            std::sort(window.begin(), window.end());
            out[y * width + x] = window[window.size() / 2];
        }
    }
    return out;
}

// 階調変換（ウィンドウ処理）を適用
std::vector<unsigned char> window_transform(const std::vector<short> &data, int window_level, int window_width)
{
    std::vector<unsigned char> out(data.size(), 0);
    int min_val = window_level - window_width / 2;
    int max_val = window_level + window_width / 2;
    for (size_t i = 0; i < data.size(); ++i)
    {
        int v = data[i];
        if (v <= min_val)
            out[i] = 0;
        else if (v >= max_val)
            out[i] = 255;
        else
            out[i] = static_cast<unsigned char>(255.0 * (v - min_val) / window_width);
    }
    return out;
}
