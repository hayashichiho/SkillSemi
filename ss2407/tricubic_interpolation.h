#ifndef TRICUBIC_INTERPOLATION_H
#define TRICUBIC_INTERPOLATION_H

#include <vector>

class TricubicInterpolation {
public:
    float h(float t, float a); // h(t) 関数
    float cubic_interpolate(const float src[4], float t, float a); // 1次元Cubic補間
    float bicubic_interpolate(const float src[4][4], float t_x, float t_y, float a); // 2次元Bicubic補間
    float tricubic_interpolate(const float src[4][4][4], float t_x, float t_y, float t_z, float a); // 3次元Tricubic補間
    float interpolate(const std::vector<short>& data, int width, int height, int depth, float x, float y, float z, float a); // 3次元Tricubic補間のメイン関数
    float matrix_interpolate(const float src[4][4], float t_x, float t_y, float a); // 1次元Cubic補間を実施する関数
};

#endif
