#include "path.h"
#include <fstream>
#include <iostream>
#include <sstream>
#include <iterator>
#include <algorithm>

using namespace std;

/* ファイルからの読み込み */
void Path::load_from_file(const std::string &filepath)
{
    parameters.clear();
    std::ifstream fin(filepath);
    if (!fin)
    {
        std::cerr << "Failed to open parameter file: " << filepath << std::endl;
        return;
    }
    std::string line;
    while (std::getline(fin, line))
    {
        if (!line.empty())
            parameters.push_back(line);
    }
}

/* 読み込んだパラメータの表示 */
void Path::print_txt_file(const std::string &filepath) const
{
    std::cout << "[Parameter File] " << filepath << std::endl;
    for (const auto &line : parameters)
    {
        std::cout << "  " << line << std::endl;
    }
}

/* 入力MHDファイルの取得 */
std::string Path::get_input_mhd_file(const std::string &dir) const
{
    for (const auto &line : parameters)
    {
        if (line.find("Input") == 0)
        {
            size_t eq_pos = line.find('=');
            if (eq_pos != std::string::npos)
            {
                std::string path = line.substr(eq_pos + 1);
                // 前後の空白除去
                path.erase(0, path.find_first_not_of(" \n\r\t"));
                path.erase(path.find_last_not_of(" \n\r\t") + 1);
                std::filesystem::path p = std::filesystem::path(dir) / path;
                return p.string();
            }
        }
    }
    return "";
}

/* mhdファイル読み込み */
void Path::load_mhd_file(const std::string &filepath)
{
    mhd_info.clear();
    std::ifstream fin(filepath);
    if (!fin)
    {
        std::cerr << "Failed to open mhd file: " << filepath << std::endl;
        return;
    }
    std::string line;
    while (std::getline(fin, line))
    {
        std::istringstream iss(line);
        std::string key, value;
        if (std::getline(iss, key, '='))
        {
            std::getline(iss, value);
            key.erase(key.find_last_not_of(" \n\r\t") + 1);
            value.erase(0, value.find_first_not_of(" \n\r\t"));
            mhd_info[key] = value;
        }
    }
}

/* mhdファイルの表示 */
void Path::print_mhd_file(const std::string &filepath) const
{
    std::cout << "[MHD File] " << filepath << std::endl;
    for (const auto &kv : mhd_info)
    {
        std::cout << "  " << kv.first << " = " << kv.second << std::endl;
    }
}

/* mhd情報の取得 */
const std::map<std::string, std::string> &Path::get_mhd_info() const
{
    return mhd_info;
}

/* パラメータの取得 */
const std::vector<std::string> &Path::get_parameters() const
{
    return parameters;
}

/* 画像データの読み込み */
std::vector<short> Path::load_image_data(const std::string &filepath) const
{
    std::ifstream fin(filepath, std::ios::binary);
    if (!fin)
    {
        std::cerr << "Failed to open raw file: " << filepath << std::endl;
        return {};
    }
    fin.seekg(0, std::ios::end);
    size_t filesize = fin.tellg();
    fin.seekg(0, std::ios::beg);
    std::vector<short> data(filesize / sizeof(short));
    fin.read(reinterpret_cast<char *>(data.data()), filesize);
    return data;
}

/* rawファイル保存 */
void Path::save_raw(const std::string &filepath, const std::vector<char> &data) const
{
    std::ofstream fout(filepath, std::ios::binary);
    if (!fout)
    {
        std::cerr << "Failed to save raw file: " << filepath << std::endl;
        return;
    }
    fout.write(data.data(), data.size());
}

/* mhdファイル保存 */
void Path::save_mhd(const std::string &mhd_path, const std::string &raw_path, const std::map<std::string, std::string> &mhd_info, bool is_transformed) const
{
    std::ofstream fout(mhd_path);
    if (!fout)
    {
        std::cerr << "Failed to save mhd file: " << mhd_path << std::endl;
        return;
    }
    for (const auto &kv : mhd_info)
    {
        if (kv.first == "ElementDataFile")
        {
            fout << kv.first << " = " << raw_path << std::endl;
        }
        else
        {
            fout << kv.first << " = " << kv.second << std::endl;
        }
    }
    if (is_transformed)
    {
        fout << "Transformed = True" << std::endl;
    }
}
