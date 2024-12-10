#include "path.h"

#include <algorithm>
#include <fstream>
#include <sstream>
#include <stdexcept>

// テキストファイルの読み込み
void Path::load_text_file(const std::string& filepath) {
    std::ifstream file(filepath);
    if (!file.is_open()) {
        throw std::runtime_error("テキストファイル: " + filepath + " が開けません");
    }

    // テキスト情報のクリア
    _text_info.clear();

    std::string line;
    while (std::getline(file, line)) {
        std::istringstream iss(line);
        std::string key, value;
        if (std::getline(iss, key, '=') && std::getline(iss, value)) {
            // 空白の削除
            key.erase(key.find_last_not_of(" \n\r\t") + 1);
            value.erase(0, value.find_first_not_of(" \n\r\t"));
            value.erase(value.find_last_not_of(" \n\r\t") + 1);

            if (key == "ViewAngle") {
                std::istringstream angle_stream(value);
                std::string phi, theta, psi;
                if (angle_stream >> phi >> theta >> psi) {
                    _text_info["Phi"] = phi;      // X軸周りの回転角
                    _text_info["Theta"] = theta;  // Y軸周りの回転角
                    _text_info["Psi"] = psi;      // Z軸周りの回転角
                } else {
                    throw std::runtime_error("ViewAngleの値が不正です: " + value);
                }
            } else {
                _text_info[key] = value;
            }
        }
    }
    file.close();
}

// MHDファイルの読み込み
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
            }
            if (key == "ElementSpacing") {
                _mhd_info["ElementSpacing"] = value;
                std::istringstream dim_stream(value);
                std::string x, y, z;
                dim_stream >> x >> y >> z;
                _mhd_info["ElementSpacingX"] = x;
                _mhd_info["ElementSpacingY"] = y;
                _mhd_info["ElementSpacingZ"] = z;
            } else {
                _mhd_info[key] = value;
            }
        }
    }
    file.close();
}

// RAWファイルの読み込み
std::vector<unsigned char> Path::load_raw_file(const std::string& filepath, size_t size) {
    std::ifstream file(filepath, std::ios::binary);
    if (!file.is_open()) {
        throw std::runtime_error("RAWファイル: " + filepath + " が開けません");
    }

    // ファイルサイズを確認
    file.seekg(0, std::ios::end);
    std::streampos file_size = file.tellg();
    file.seekg(0, std::ios::beg);

    // ElementTypeの確認
    std::string element_type = _mhd_info.at("ElementType");

    if (element_type == "MET_SHORT") {
        std::vector<short> short_data(size);
        if (!file.read(reinterpret_cast<char*>(short_data.data()), size * sizeof(short))) {
            throw std::runtime_error("RAWファイルの読み込みに失敗しました: " + filepath);
        }

        // HU値の範囲
        const short hu_min = -1024;
        const short hu_max = 3072;

        std::vector<unsigned char> data(size);
#pragma omp parallel for
        for (size_t i = 0; i < size; ++i) {
            // HU値の範囲で正規化
            double normalized = (short_data[i] - hu_min) / static_cast<double>(hu_max - hu_min);
            normalized = std::clamp(normalized, 0.0, 1.0);
            data[i] = static_cast<unsigned char>(normalized * 255);
        }
        return data;
    } else if (element_type == "MET_UCHAR") {
        std::vector<unsigned char> data(size);
        if (!file.read(reinterpret_cast<char*>(data.data()), size)) {
            throw std::runtime_error("RAWファイルの読み込みに失敗しました: " + filepath);
        }
        return data;
    } else {
        throw std::runtime_error("未対応のElementType: " + element_type);
    }
}

// RAWファイルの保存
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

// MHDファイルの保存
void Path::save_mhd_file(const std::string& filepath, const std::map<std::string, std::string>& mhd_info) {
    std::ofstream file(filepath);
    if (!file.is_open()) {
        throw std::runtime_error("MHDファイル: " + filepath + " が作成できません");
    }

    for (const auto& kv : mhd_info) {
        if (kv.first == "DimSizeX" || kv.first == "DimSizeY" || kv.first == "DimSizeZ" || kv.first == "DimSize" ||
            kv.first == "ElementSpacingX" || kv.first == "ElementSpacingY" || kv.first == "ElementSpacingZ" ||
            kv.first == "ElementSpacing") {
            continue;  // DimSizeとElementSpacingは後で記述
        }
        file << kv.first << " = " << kv.second << "\n";
    }

    // DimSizeとElementSpacingを記述
    file << "DimSize = " << mhd_info.at("DimSize");

    file.close();
}

// MHD情報の取得
const std::map<std::string, std::string>& Path::get_mhd_info() const {
    return _mhd_info;
}

// テキスト情報の取得
const std::map<std::string, std::string>& Path::get_text_info() const {
    return _text_info;
}
