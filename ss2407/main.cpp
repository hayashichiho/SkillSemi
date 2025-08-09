#include "path.h"
#include "tricubic_processing.h"
#include <iostream>
#include <vector>
#include <filesystem>
#include <map>
#include <string>

using namespace std;
namespace fs = std::filesystem;

int main(int argc, char *argv[])
{
    // コマンドライン引数のチェック
    if (argc < 3)
    {
        cerr << "Usage: " << argv[0] << " <input.mhd> <output.mhd> [resolution]" << endl;
        return 1;
    }

    string input_mhd = argv[1];
    string output_mhd = argv[2];
    float target_resolution = argc > 3 ? stof(argv[3]) : 1.0f;
    if (target_resolution <= 0)
    {
        cerr << "分解能は0より大きい値である必要があります." << endl;
        return 1;
    }

    Path path;
    // 入力MHDファイルの読み込み
    path.load_mhd_file(input_mhd);
    map<string, string> mhd_info = path.get_mhd_info();

    // 必要な情報の取得
    int width = stoi(mhd_info.at("DimSizeX"));
    int height = stoi(mhd_info.at("DimSizeY"));
    int depth = stoi(mhd_info.at("DimSizeZ"));
    float spacing_x = stof(mhd_info.at("ElementSpacingX"));
    float spacing_y = stof(mhd_info.at("ElementSpacingY"));
    float spacing_z = stof(mhd_info.at("ElementSpacingZ"));

    // 新しいZ軸のボクセル数を計算
    int new_depth = static_cast<int>(depth * (spacing_z / target_resolution));

    // RAWファイルのパス生成
    fs::path raw_file_path = fs::path(input_mhd).replace_extension(".raw");
    size_t size = static_cast<size_t>(width) * height * depth;
    vector<short> input_data = path.load_raw_file(raw_file_path.string(), size);

    // Tricubic補間による等方化
    vector<short> output_data = TricubicProcessing::perform_isotropic_resampling(
        input_data, width, height, depth, width, height, new_depth, -0.5f, 0);

    // 出力先ディレクトリ
    fs::path input_dir = fs::absolute(fs::path(input_mhd)).parent_path();
    fs::path output_dir = input_dir.parent_path() / "output";
    if (!fs::exists(output_dir))
    {
        fs::create_directories(output_dir);
    }

    // 出力RAW/MHDファイルのパス生成（outputフォルダに保存）
    fs::path output_raw_path = output_dir / fs::path(output_mhd).filename().replace_extension(".raw");
    fs::path output_mhd_path = output_dir / fs::path(output_mhd).filename().replace_extension(".mhd");

    path.save_raw_file(output_raw_path.string(), output_data);

    // MHDファイル情報の更新
    mhd_info["DimSize"] = to_string(width) + " " + to_string(height) + " " + to_string(new_depth);
    mhd_info["ElementSpacing"] = to_string(target_resolution) + " " +
                                 to_string(target_resolution) + " " +
                                 to_string(target_resolution);
    mhd_info["ElementDataFile"] = output_raw_path.filename().string();

    path.save_mhd_file(output_mhd_path.string(), mhd_info);

    cout << "MHDファイル: " << output_mhd_path.string() << endl;
    cout << "RAWファイル: " << output_raw_path.string() << endl;
    return 0;
}
