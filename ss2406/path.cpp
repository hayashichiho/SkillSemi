#include "path.h"
#include <fstream>
#include <iostream>
#include <sstream>
#include <iterator>
#include <algorithm>

using namespace std;

/* mhdファイル読み込み */
void Path::load_mhd_file(const string& filepath) {
    ifstream file(filepath);
    if (!file.is_open()) {
        cerr << "Failed to open mhd file: " << filepath << endl;
        return;
    }
    string line;
    while (getline(file, line)) {
        istringstream iss(line);
        string key, value;
        if (getline(iss, key, '=') && getline(iss, value)) {
            if (key == "DimSize") {
                istringstream dim_iss(value);
                vector<int> dim_sizes;
                int dim;
                while (dim_iss >> dim) {
                    dim_sizes.push_back(dim);
                }
                if (dim_sizes.size() == 3) {
                    mhd_info["DimSizeX"] = to_string(dim_sizes[0]);
                    mhd_info["DimSizeY"] = to_string(dim_sizes[1]);
                    mhd_info["DimSizeZ"] = to_string(dim_sizes[2]);
                } else {
                    cerr << "Invalid DimSize in mhd file" << endl;
                    throw runtime_error("Invalid DimSize in mhd file");
                }
            } else {
                mhd_info[key] = value;
            }
        }
        key.erase(key.find_last_not_of(" \n\r\t") + 1);
            value.erase(0, value.find_first_not_of(" \n\r\t")); // 先頭のスペースを削除
            value.erase(value.find_last_not_of(" \n\r\t") + 1); // 末尾のスペースを削除
            mhd_info[key] = value;
    }
    file.close();

    // DimSizeの存在を確認
    if (mhd_info.find("DimSizeX") == mhd_info.end() ||
        mhd_info.find("DimSizeY") == mhd_info.end() ||
        mhd_info.find("DimSizeZ") == mhd_info.end()) {
        cerr << "DimSize not found in mhd file" << endl;
        throw out_of_range("DimSize not found in mhd file");
    }
}


/* rawファイル読み込み */
vector<float> Path::load_raw_file(const string& filepath, size_t size) {
    ifstream file(filepath, ios::binary);
    if (!file.is_open()) {
        cerr << "Failed to open raw file: " << filepath << endl;
        throw runtime_error("Failed to open raw file");
    }

    vector<float> data(size);
    file.read(reinterpret_cast<char*>(data.data()), size * sizeof(float));
    if (!file) {
        cerr << "Failed to read raw file: " << filepath << endl;
        throw runtime_error("Failed to read raw file");
    }

    file.close();
    return data;
}

/* mhdファイル保存 */
void Path::save_mhd_file(const string& filepath, const map<string, string>& mhd_info) {
    ofstream file(filepath);
    if (!file.is_open()) {
        cerr << "Failed to open mhd file for writing: " << filepath << endl;
        throw runtime_error("Failed to open mhd file for writing");
    }

    for (const auto& kv : mhd_info) {
        file << kv.first << " = " << kv.second << endl;
    }

    file.close();
}

/* rawファイル保存 */
void Path::save_raw_file(const string& filepath, const vector<float>& data) {
    ofstream file(filepath, ios::binary);
    if (!file.is_open()) {
        cerr << "Failed to open raw file for writing: " << filepath << endl;
        throw runtime_error("Failed to open raw file for writing");
    }

    file.write(reinterpret_cast<const char*>(data.data()), data.size() * sizeof(float));
    if (!file) {
        cerr << "Failed to write raw file: " << filepath << endl;
        throw runtime_error("Failed to write raw file");
    }

    file.close();
}