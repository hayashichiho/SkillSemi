#pragma once
#include <string>
#include <vector>
#include <map>
#include <fstream>
#include <iostream>
#include <sstream>
#include <filesystem>

class Path
{
public:
  // パラメータファイルの読み込み
  void load_from_file(const std::string &filepath);

  // パラメータファイルの内容表示
  void print_txt_file(const std::string &filepath) const;

  // 入力MHDファイル名の取得
  std::string get_input_mhd_file(const std::string &dir) const;

  // MHDファイルの読み込み
  void load_mhd_file(const std::string &filepath);

  // MHDファイルの内容表示
  void print_mhd_file(const std::string &filepath) const;

  // MHDファイル情報の取得
  const std::map<std::string, std::string> &get_mhd_info() const;

  // パラメータ情報の取得
  const std::vector<std::string> &get_parameters() const;

  // RAW画像データの読み込み
  std::vector<short> load_image_data(const std::string &filepath) const;

  // RAW画像データの保存
  void save_raw(const std::string &filepath, const std::vector<char> &data) const;

  // MHDファイルの保存
  void save_mhd(const std::string &mhd_path, const std::string &raw_path, const std::map<std::string, std::string> &mhd_info, bool is_transformed) const;

private:
  std::map<std::string, std::string> mhd_info;
  std::vector<std::string> parameters;
};
