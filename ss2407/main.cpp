#include "path.h"
#include "tricubic_processing.h"
#include <iostream>
#include <vector>
#include <filesystem>

using namespace std;

int main(int argc, char* argv[]) {
    if (argc < 3) {
        cerr << "Usage: " << argv[0] << " <input.mhd> <output.mhd> [resolution]" << endl;
        return 1;
    }

    string input_mhd = argv[1];
    string output_mhd = argv[2];
    float target_resolution = argc > 3 ? stof(argv[3]) : 1.0f;
    if(target_resolution <= 0) {
        throw std::runtime_error("分解能は0より大きい値である必要があります.");
    }

    Path path;
    path.load_mhd_file(input_mhd); // MHDファイルを読み込む
    map<string, string> mhd_info = path.get_mhd_info(); // MHDファイル情報を取得

    int width = stoi(mhd_info.at("DimSizeX"));
    int height = stoi(mhd_info.at("DimSizeY"));
    int depth = stoi(mhd_info.at("DimSizeZ"));
    float spacing_x = stof(mhd_info.at("ElementSpacingX"));
    float spacing_y = stof(mhd_info.at("ElementSpacingY"));
    float spacing_z = stof(mhd_info.at("ElementSpacingZ"));

    // 新しいZ軸のボクセル数を計算
    int new_depth = static_cast<int>(depth * (spacing_z / target_resolution));

    size_t size = static_cast<size_t>(width) * height * depth;
    string raw_file = filesystem::path(input_mhd).replace_extension(".raw").string();
    vector<short> input_data = path.load_raw_file(raw_file, size); // RAWファイルを読み込む

    // Tricubic補間を使って等方化
    vector<short> output_data = TricubicProcessing::perform_isotropic_resampling(
        input_data, width, height, depth, width, height, new_depth, -0.5f, 0);

    // 出力データを保存
    string output_raw = filesystem::path(output_mhd).replace_extension(".raw").string();
    path.save_raw_file(output_raw, output_data);

    // MHDファイルの更新
    mhd_info["DimSize"] = to_string(width) + " " + to_string(height) + " " + to_string(new_depth);
    mhd_info["ElementSpacing"] = to_string(target_resolution) + " " +
                                to_string(target_resolution) + " " +
                                to_string(target_resolution);
    mhd_info["ElementDataFile"] = filesystem::path(output_raw).filename().string();

    path.save_mhd_file(output_mhd, mhd_info); // MHDファイルを保存

    cout << "MHDファイル: " << output_mhd << endl;
    cout << "RAWファイル: " << output_raw << endl;
    return 0;
}
