#pragma once
#include <vector>
#include <map>
#include <string>

class Path {
public:
    void load_text_file(const std::string& filepath);
    void load_mhd_file(const std::string& filepath);
    std::vector<unsigned char> load_raw_file(const std::string& filepath, size_t size);
    void save_raw_file(const std::string& filepath, const std::vector<unsigned char>& data);
    void save_mhd_file(const std::string& filepath, const std::map<std::string, std::string>& mhd_info);
    const std::map<std::string, std::string>& get_mhd_info() const;
    const std::map<std::string, std::string>& get_text_info() const;

private:
    std::map<std::string, std::string> _text_info;
    std::map<std::string, std::string> _mhd_info;
};