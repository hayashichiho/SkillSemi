#pragma once
#include <map>
#include <string>
#include <vector>

// パス情報の管理
class Path {
   public:
    void load_text_file(const std::string& filepath);  // テキストファイルの読み込み
    void load_mhd_file(const std::string& filepath);   // MHDファイルの読み込み
    std::vector<unsigned char> load_raw_file(const std::string& filepath, size_t size);  // RAWファイルの読み込み
    void save_raw_file(const std::string& filepath, const std::vector<unsigned char>& data);  // RAWファイルの保存
    void save_mhd_file(const std::string& filepath,
                       const std::map<std::string, std::string>& mhd_info);  // MHDファイルの保存
    const std::map<std::string, std::string>& get_mhd_info() const;          // MHD情報の取得
    const std::map<std::string, std::string>& get_text_info() const;         // テキスト情報の取得

   private:
    std::map<std::string, std::string> _text_info;  // テキストファイル情報
    std::map<std::string, std::string> _mhd_info;   // MHDファイル情報
};
