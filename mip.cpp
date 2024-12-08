// mip.cpp
#include "mip.h"
#include <limits>
#include <cmath>

std::vector<unsigned char> generate_mip_image(
    const std::vector<unsigned char>& raw_data,
    int width, int height, int depth,
    const EulerAngles& angles) {
    
    std::vector<unsigned char> mip_image(width * height, 0);
    
    // 回転行列の計算
    double cx = cos(angles.phi);
    double sx = sin(angles.phi);
    double cy = cos(angles.theta);
    double sy = sin(angles.theta);
    double cz = cos(angles.psi);
    double sz = sin(angles.psi);
    
    // 中心座標
    double center_x = width / 2.0;
    double center_y = height / 2.0;
    double center_z = depth / 2.0;

    // MIP処理
    for (int y = 0; y < height; ++y) {
        for (int x = 0; x < width; ++x) {
            // 画像中心を原点とした座標に変換
            double dx = x - center_x;
            double dy = y - center_y;
            
            unsigned char max_val = 0;
            for (int z = 0; z < depth; ++z) {
                double dz = z - center_z;
                
                // 回転変換
                double rx = dx;
                double ry = cy * dy - sy * dz;
                double rz = sy * dy + cy * dz;
                
                // 座標を元の範囲に戻す
                int ix = static_cast<int>(rx + center_x);
                int iy = static_cast<int>(ry + center_y);
                int iz = static_cast<int>(rz + center_z);
                
                // 範囲チェック
                if (ix >= 0 && ix < width && iy >= 0 && iy < height && iz >= 0 && iz < depth) {
                    int index = iz * (width * height) + iy * width + ix;
                    max_val = std::max(max_val, raw_data[index]);
                }
            }
            
            mip_image[y * width + x] = max_val;
        }
    }
    
    return mip_image;
}