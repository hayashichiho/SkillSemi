#include "path.h"
#include <fstream>
#include <sstream>
#include <stdexcept>

void Path::load_text_file(const std::string& filepath) {
    std::ifstream file(filepath);
    if (!file.is_open()) {
        throw std::runtime_error("テキストファイル: " + filepath + " が開けません");
    }

    std::string line;
    while (std::getline(file, line)) {
        std::istringstream iss(line);
        std::string key, value;
        if (std::getline(iss, key, '=') && std::getline(iss, value)) {
            key.erase(key.find_last_not_of(" \n\r\t") + 1);
            value.erase(0, value.find_first_not_of(" \n\r\t"));
            value.erase(value.find_last_not_of(" \n\r\t") + 1);
            _text_info[key] = value;
        }
    }
    file.close();
}

void Path::load_mhd_file(const std::string& filepath) {
    std::ifstream file(filepath);
    if (!file.is_open()) {
        throw std::runtime_error("MHDファイル: " + filepath + " が開けません");
    }

    std::string line;
    while (std::getline(file, line)) {
        std::istringstream iss(line);
        std::string key, value;
        if (std::getline(iss, key, '=') && std::getline(iss, value)) {
            key.erase(key.find_last_not_of(" \n\r\t") + 1);
            value.erase(0, value.find_first_not_of(" \n\r\t"));
            value.erase(value.find_last_not_of(" \n\r\t") + 1);

            if (key == "DimSize") {
                _mhd_info["DimSize"] = value;
                std::istringstream dim_stream(value);
                std::string x, y, z;
                dim_stream >> x >> y >> z;
                _mhd_info["DimSizeX"] = x;
                _mhd_info["DimSizeY"] = y;
                _mhd_info["DimSizeZ"] = z;
            } else {
                _mhd_info[key] = value;
            }
        }
    }
    file.close();
}

std::vector<unsigned char> Path::load_raw_file(const std::string& filepath, size_t size) {
    std::ifstream file(filepath, std::ios::binary);
    if (!file.is_open()) {
        throw std::runtime_error("RAWファイル: " + filepath + " が開けません");
    }

    // ファイルサイズを確認
    file.seekg(0, std::ios::end);
    std::streampos file_size = file.tellg();
    file.seekg(0, std::ios::beg);

    // データ読み込み
    std::vector<unsigned char> data(size);
    if (!file.read(reinterpret_cast<char*>(data.data()), size)) {
        throw std::runtime_error(
            "RAWファイルの読み込みに失敗しました: " + filepath + 
            "\n要求サイズ: " + std::to_string(size) + 
            " bytes\nファイルサイズ: " + std::to_string(file_size) + " bytes"
        );
    }
    file.close();
    return data;
}

void Path::save_raw_file(const std::string& filepath, const std::vector<unsigned char>& data) {
    std::ofstream file(filepath, std::ios::binary);
    if (!file.is_open()) {
        throw std::runtime_error("RAWファイル: " + filepath + " が作成できません");
    }

    if (!file.write(reinterpret_cast<const char*>(data.data()), data.size())) {
        throw std::runtime_error("RAWファイルの書き込みに失敗しました: " + filepath);
    }
    file.close();
}

void Path::save_mhd_file(const std::string& filepath, const std::map<std::string, std::string>& mhd_info) {
    auto updated_info = mhd_info;
    
    // 2D画像用にMHD情報を更新
    updated_info["DimSizeZ"] = "1";  // Z方向を1枚に
    updated_info["DimSize"] = updated_info["DimSizeX"] + " " + 
                             updated_info["DimSizeY"] + " 1";

    std::ofstream file(filepath);
    if (!file.is_open()) {
        throw std::runtime_error("MHDファイル: " + filepath + " が作成できません");
    }

    for (const auto& [key, value] : updated_info) {
        file << key << " = " << value << std::endl;
    }
    file.close();
}

const std::map<std::string, std::string>& Path::get_mhd_info() const {
    return _mhd_info;
}

const std::map<std::string, std::string>& Path::get_text_info() const {
    return _text_info;
}