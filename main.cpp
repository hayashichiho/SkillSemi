#include "path.h"
#include "mip.h"
#include <iostream>
#include <vector>
#include <map>
#include <string>
#include <filesystem>
#include <cmath>

int main(int argc, char *argv[])
{
    if (argc < 4)
    {
        std::cerr << "Usage: " << argv[0] << " <input.mhd> <output.mhd> <text.txt>" << std::endl;
        return 1;
    }

    std::string input_mhd = argv[1];
    std::string output_mhd = argv[2];
    std::string text_file = argv[3];

    try
    {
        Path path;

        // テキストファイルの読み込み
        path.load_text_file(text_file);
        const auto &text_info = path.get_text_info();

        // MHDファイルの読み込み
        path.load_mhd_file(input_mhd);
        const auto &mhd_info = path.get_mhd_info();

        // RAWデータの読み込み
        size_t size = std::stoi(mhd_info.at("DimSizeX")) *
                      std::stoi(mhd_info.at("DimSizeY")) *
                      std::stoi(mhd_info.at("DimSizeZ"));
        std::string raw_file = std::filesystem::path(input_mhd).replace_extension(".raw").string();
        std::vector<unsigned char> raw_data = path.load_raw_file(raw_file, size);

        EulerAngles angles(0, 60, 0);

        // MIP画像を生成
        int width = std::stoi(mhd_info.at("DimSizeX"));
        int height = std::stoi(mhd_info.at("DimSizeY"));
        int depth = std::stoi(mhd_info.at("DimSizeZ"));
        std::vector<unsigned char> mip_image = generate_mip_image(raw_data, width, height, depth, angles);

        // 結果の保存
        path.save_raw_file(std::filesystem::path(output_mhd).replace_extension(".raw").string(), mip_image);
        path.save_mhd_file(output_mhd, mhd_info);
    }
    catch (const std::exception &e)
    {
        std::cerr << "Error: " << e.what() << std::endl;
        return 1;
    }

    return 0;
}
