// window_parameters.cpp
#include "window_parameters.h"

#include <omp.h>

#include <iostream>

// ウィンドウ処理の適用
void WindowParameters::apply_window_processing(std::vector<unsigned char>& image_data) const {
    if (!_processing) {
        std::cerr << "ウィンドウ処理が無効です" << std::endl;
        return;
    }

    const int min_value = _level - _width / 2;
    const int max_value = _level + _width / 2;

    std::cout << "ウィンドウ処理: " << min_value << " ~ " << max_value << std::endl;
#pragma omp parallel for
    for (size_t i = 0; i < image_data.size(); ++i) {
        int pixel_value = static_cast<int>(image_data[i]);
        if (pixel_value <= min_value) {
            pixel_value = 0;
        } else if (pixel_value >= max_value) {
            pixel_value = 255;
        } else {
            pixel_value = static_cast<int>((pixel_value - min_value) * 255.0 / _width);
        }
        image_data[i] = static_cast<unsigned char>(pixel_value);
    }
}
