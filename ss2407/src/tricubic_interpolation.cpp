#include "tricubic_interpolation.h"
#include <cmath>
#include <vector>
#include <algorithm>

// 範囲外の値をクランプする関数
template <typename T>
T clamp(T v, T lo, T hi) {
    return (v < lo) ? lo : (v > hi) ? hi : v;
}

// h(t) 関数の実装
float TricubicInterpolation::h(float t, float a) {
    t = std::abs(t); // tの絶対値を取得
    if (t <= 1) {
        return (a + 2) * t * t * t - (a + 3) * t * t + 1;
    } else if (t <= 2) {
        return a * t * t * t - 5 * a * t * t + 8 * a * t - 4 * a;
    } else {
        return 0.0f; // |t| > 2 の場合
    }
}

// 1次元Cubic補間を実施する関数
float TricubicInterpolation::matrix_interpolate(const float src[4][4], float t_x, float t_y, float a) {
    // h(t) のベクトルを生成
    float h_x[4], h_y[4];
    for (int i = 0; i < 4; ++i) {
        h_x[i] = h(t_x - i + 1, a);
        h_y[i] = h(t_y - i + 1, a);
    }

    // 行列計算 (h_y) * (src) * (h_x)^T
    float temp[4] = {0.0f};
    for (int i = 0; i < 4; ++i) {
        for (int j = 0; j < 4; ++j) {
            temp[i] += src[i][j] * h_x[j];
        }
    }

    float result = 0.0f;
    for (int i = 0; i < 4; ++i) {
        result += h_y[i] * temp[i];
    }

    return result;
}

// 3次元Tricubic補間のメイン関数
float TricubicInterpolation::interpolate(
    const std::vector<short>& data, int width, int height, int depth, float x, float y, float z, float a) {

    int x0 = static_cast<int>(std::floor(x));
    int y0 = static_cast<int>(std::floor(y));
    int z0 = static_cast<int>(std::floor(z));
    float t_x = x - x0;
    float t_y = y - y0;
    float t_z = z - z0;

    // 周囲の値を取得
    float src[4][4][4];
    for (int z = 0; z < 4; ++z) {
        for (int y = 0; y < 4; ++y) {
            for (int x = 0; x < 4; ++x) {
                int xi = clamp(x0 - 1 + x, 0, width - 1);
                int yi = clamp(y0 - 1 + y, 0, height - 1);
                int zi = clamp(z0 - 1 + z, 0, depth - 1);
                src[z][y][x] = static_cast<float>(data[(zi * height + yi) * width + xi]);
            }
        }
    }

    // Z方向を固定し、XY平面での行列補間を実施
    float temp[4];
    for (int i = 0; i < 4; ++i) {
        temp[i] = matrix_interpolate(src[i], t_x, t_y, a);
    }

    // Z方向の補間を実施
    float h_z[4];
    for (int i = 0; i < 4; ++i) {
        h_z[i] = h(t_z - i + 1, a);
    }

    float result = 0.0f;
    for (int i = 0; i < 4; ++i) {
        result += h_z[i] * temp[i];
    }

    return result;
} 