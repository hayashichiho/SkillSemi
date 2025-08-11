#include "tricubic_interpolation.h"
#include "tricubic_processing.h"
#include <cstddef>

// トリキュービック補間を使って等方化
std::vector<short> TricubicProcessing::perform_isotropic_resampling(
    const std::vector<short>& input_data, int width, int height, int depth,
    float new_width, float new_height, float new_depth, float a, int interpolation_method) {

    std::vector<short> output_data(static_cast<size_t>(new_width * new_height * new_depth)); // 出力データ

    TricubicInterpolation interpolator;

    for (int z = 0; z < new_depth; ++z) {
        for (int y = 0; y < new_height; ++y) {
            for (int x = 0; x < new_width; ++x) {
                float old_x = x * (width / new_width); // x座標の変換
                float old_y = y * (height / new_height); // y座標の変換
                float old_z = z * (depth / new_depth); // z座標の変換

                output_data[(z * static_cast<int>(new_height) + y) * static_cast<int>(new_width) + x] =
                    interpolator.interpolate(input_data, width, height, depth, old_x, old_y, old_z, a); // 補間
            }
        }
    }

    return output_data;
}
