#include "path.h"
#include <fstream>
#include <sstream>
#include <stdexcept>
#include <vector>
#include <iostream>
#include <filesystem>

// MHDファイルを読み込む
void Path::load_mhd_file(const std::string &filepath)
{
    std::ifstream file(filepath);
    if (!file.is_open())
    {
        throw std::runtime_error("MHDファイル: " + filepath + " が開けません");
    }

    std::string line;
    while (std::getline(file, line))
    {
        std::istringstream iss(line);
        std::string key, value;
        if (std::getline(iss, key, '=') && std::getline(iss, value))
        {
            key.erase(key.find_last_not_of(" \n\r\t") + 1);     // 末尾の空白を削除
            value.erase(0, value.find_first_not_of(" \n\r\t")); // 先頭の空白を削除
            value.erase(value.find_last_not_of(" \n\r\t") + 1); // 末尾の空白を削除

            if (key == "DimSize")
            {
                _mhd_info["DimSize"] = value;
                std::istringstream dim_stream(value);
                dim_stream >> _mhd_info["DimSizeX"] >> _mhd_info["DimSizeY"] >> _mhd_info["DimSizeZ"];
            }
            else if (key == "ElementSpacing")
            {
                _mhd_info["ElementSpacing"] = value;
                std::istringstream spacing_stream(value);
                spacing_stream >> _mhd_info["ElementSpacingX"] >> _mhd_info["ElementSpacingY"] >> _mhd_info["ElementSpacingZ"];
            }
            else
            {
                _mhd_info[key] = value;
            }
        }
    }
    file.close();
}

// RAWファイルを読み込む
std::vector<short> Path::load_raw_file(const std::string &filepath, size_t size)
{
    std::ifstream file(filepath, std::ios::binary);
    if (!file.is_open())
    {
        throw std::runtime_error("RAWファイル: " + filepath + " が開けません");
    }

    std::vector<short> data(size);
    file.read(reinterpret_cast<char *>(data.data()), size * sizeof(short));
    if (!file)
    {
        throw std::runtime_error("RAWファイルの読み込みに失敗しました");
    }

    file.close();
    return data;
}

// RAWファイルを保存する
void Path::save_raw_file(const std::string &filepath, const std::vector<short> &data)
{
    std::filesystem::path out_path = std::filesystem::absolute(filepath);
    std::filesystem::path input_dir = out_path.parent_path().parent_path() / "output";
    if (!std::filesystem::exists(input_dir))
    {
        std::filesystem::create_directories(input_dir);
    }
    std::filesystem::path save_path = input_dir / out_path.filename();

    std::ofstream file(save_path, std::ios::binary);
    if (!file.is_open())
    {
        throw std::runtime_error("RAWファイル: " + save_path.string() + " が開けません");
    }

    file.write(reinterpret_cast<const char *>(data.data()), data.size() * sizeof(short));
    file.close();
}

// MHDファイルを保存する
void Path::save_mhd_file(const std::string &filepath, const std::map<std::string, std::string> &mhd_info)
{
    std::filesystem::path out_path = std::filesystem::absolute(filepath);
    std::filesystem::path input_dir = out_path.parent_path().parent_path() / "output";
    if (!std::filesystem::exists(input_dir))
    {
        std::filesystem::create_directories(input_dir);
    }
    std::filesystem::path save_path = input_dir / out_path.filename();

    std::ofstream file(save_path);
    if (!file.is_open())
    {
        throw std::runtime_error("MHDファイル: " + save_path.string() + " が開けません");
    }

    for (const auto &kv : mhd_info)
    {
        if (kv.first == "DimSizeX" || kv.first == "DimSizeY" || kv.first == "DimSizeZ" || kv.first == "DimSize" ||
            kv.first == "ElementSpacingX" || kv.first == "ElementSpacingY" || kv.first == "ElementSpacingZ" || kv.first == "ElementSpacing")
        {
            continue; // DimSizeとElementSpacingは後で記述
        }
        file << kv.first << " = " << kv.second << "\n";
    }

    // DimSizeとElementSpacingを記述
    file << "DimSize = " << mhd_info.at("DimSize") << "\n";
    file << "ElementSpacing = " << mhd_info.at("ElementSpacing") << "\n";

    file.close();
}

// MHD情報を取得する
const std::map<std::string, std::string> &Path::get_mhd_info() const
{
    return _mhd_info;
}
