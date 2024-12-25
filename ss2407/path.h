#ifndef PATH_H
#define PATH_H

#include <string>
#include <vector>
#include <map>

class Path {
public:
    void load_mhd_file(const std::string& filepath); // MHDファイルを読み込む
    std::vector<short> load_raw_file(const std::string& filepath, size_t size); // RAWファイルを読み込む
    void save_raw_file(const std::string& filepath, const std::vector<short>& data); // RAWファイルを保存する
    void save_mhd_file(const std::string& filepath, const std::map<std::string, std::string>& mhd_info); // MHDファイルを保存する
    const std::map<std::string, std::string>& get_mhd_info() const; // MHD情報を取得する

private:
    std::map<std::string, std::string> _mhd_info; // MHDファイルの情報
};

#endif
