#include "mip.h"

#include <omp.h>

#include <algorithm>
#include <cmath>
#include <iostream>
#include <map>
#include <vector>

#include "mip.h"
#include "window_parameters.h"

// MIP画像生成
std::vector<unsigned char> EulerAngles::generate_mip_image(const std::vector<unsigned char>& raw_data, int width,
                                                           int height, int depth, const EulerAngles& angles,
                                                           const std::map<std::string, double>& spacing,
                                                           const WindowParameters& window_params) {
    // ボクセル間隔を取得
    double spacing_x = spacing.at("ElementSpacingX");
    double spacing_y = spacing.at("ElementSpacingY");
    double spacing_z = spacing.at("ElementSpacingZ");

    // RAWデータの物理サイズを計算
    double physical_width = width * spacing_x;
    double physical_height = height * spacing_y;
    double physical_depth = depth * spacing_z;

    std::cout << "物理サイズ: " << physical_width << " x " << physical_height << " x " << physical_depth << std::endl;

    // 球の半径を設定
    double r = std::max({physical_width, physical_height, physical_depth}) / 2.0;

    // MIP画像のサイズを計算
    double pixel_size = std::min({spacing_x, spacing_y});
    int mip_size = static_cast<int>(2 * r / pixel_size);

    // MIP画像の初期化
    std::vector<unsigned char> mip_image(mip_size * mip_size, 0);

    // 中心座標を計算
    double center_x = physical_width / 2.0;
    double center_y = physical_height / 2.0;
    double center_z = physical_depth / 2.0;

    // 回転行列の計算
    double cx = std::cos(angles.get_phi()), sx = std::sin(angles.get_phi());
    double cy = std::cos(angles.get_theta()), sy = std::sin(angles.get_theta());
    double cz = std::cos(angles.get_psi()), sz = std::sin(angles.get_psi());

    double rotation_matrix[3][3] = {{cy * cz, -cy * sz, sy},
                                    {sx * sy * cz + cx * sz, -sx * sy * sz + cx * cz, -sx * cy},
                                    {-cx * sy * cz + sx * sz, cx * sy * sz + sx * cz, cx * cy}};

    // 投影処理
    try {
#pragma omp parallel for
        for (int y = 0; y < mip_size; ++y) {
            double ty = (y - mip_size / 2.0) * pixel_size;

            for (int x = 0; x < mip_size; ++x) {
                double tx = (x - mip_size / 2.0) * pixel_size;

                // 球の内部でない場合はスキップ
                if (tx * tx + ty * ty > r * r) {
                    continue;
                }

                unsigned char max_value = 0;

                for (double z = -r; z <= r; z += spacing_z) {
                    double tz = z;

                    // 投影経路上の座標を回転
                    double world_x =
                        tx * rotation_matrix[0][0] + ty * rotation_matrix[0][1] + tz * rotation_matrix[0][2];
                    double world_y =
                        tx * rotation_matrix[1][0] + ty * rotation_matrix[1][1] + tz * rotation_matrix[1][2];
                    double world_z =
                        tx * rotation_matrix[2][0] + ty * rotation_matrix[2][1] + tz * rotation_matrix[2][2];

                    // 回転後の座標を元の座標に変換
                    world_x = (world_x + center_x) / spacing_x;
                    world_y = (world_y + center_y) / spacing_y;
                    world_z = (world_z + center_z) / spacing_z;

                    int ix = static_cast<int>(std::round(world_x));
                    int iy = static_cast<int>(std::round(world_y));
                    int iz = static_cast<int>(std::round(world_z));

                    // 画像範囲内か確認
                    if (ix >= 0 && ix < width && iy >= 0 && iy < height && iz >= 0 && iz < depth) {
                        size_t index = (iz * height + iy) * width + ix;
                        max_value = std::max(max_value, raw_data[index]);
                    }
                }

                // MIP画像に最大値を格納
                mip_image[y * mip_size + x] = max_value;
            }
        }

        // ウィンドウ処理
        if (window_params.get_processing()) {
            window_params.apply_window_processing(mip_image);
        }

        return mip_image;
    } catch (const std::exception& e) {
        std::cerr << "MIP画像生成エラー: " << e.what() << std::endl;
        return mip_image;
    }
}
