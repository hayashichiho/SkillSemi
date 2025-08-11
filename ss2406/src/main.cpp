#include "image_processing.h"
#include "path.h"
#include <iostream>
#include <vector>
#include <string>
#include <variant>
#include <filesystem>
using namespace std;
namespace fs = std::filesystem;

// 文字列の前後の空白・改行を除去する関数
string trim(const string &s)
{
    auto start = s.find_first_not_of(" \n\r\t");
    auto end = s.find_last_not_of(" \n\r\t");
    if (start == string::npos || end == string::npos)
        return "";
    return s.substr(start, end - start + 1);
}

int main(int argc, char *argv[])
{
    // コマンドライン引数が足りない場合は使い方を表示して終了
    if (argc < 2)
    {
        cerr << "Usage: " << argv[0] << " <parameter file>" << endl;
        return 1;
    }

    // パラメータファイルのパスを取得
    string txt_path = argv[1];
    Path path_obj;

    // パラメータファイルの読み込み
    path_obj.load_from_file(txt_path);

    // 入力MHDファイルのパスを取得し、MHDファイルを読み込む
    string input_mhd_file = path_obj.get_input_mhd_file(fs::path(txt_path).parent_path().string());
    path_obj.load_mhd_file(input_mhd_file);

    // MHDファイルから画像データファイル名を取得
    string element_data_file;
    try
    {
        element_data_file = trim(path_obj.get_mhd_info().at("ElementDataFile"));
    }
    catch (const out_of_range &)
    {
        cerr << "ElementDataFile not found in mhd file" << endl;
        return 1;
    }

    // 画像データファイルのパスを生成し、RAW画像データを読み込む
    fs::path image_data_path = fs::path(txt_path).parent_path() / element_data_file;
    vector<short> image_data = path_obj.load_image_data(image_data_path.string());

    // 画像処理（フィルタ・階調変換など）を実行
    bool is_transformed = false;
    auto processed_data = process_image_data(image_data, path_obj.get_parameters(), path_obj.get_mhd_info(), is_transformed);

    // 出力ファイル名をパラメータファイルから取得
    string output_filename;
    for (const auto &line : path_obj.get_parameters())
    {
        if (line.find("Output") == 0)
        {
            size_t eq_pos = line.find('=');
            if (eq_pos != std::string::npos)
            {
                output_filename = trim(line.substr(eq_pos + 1));
            }
            break;
        }
    }
    if (output_filename.empty())
    {
        cerr << "Output not found in parameters" << endl;
        return 1;
    }

    // 出力先ディレクトリを作成
    fs::path output_dir = fs::absolute(fs::path(txt_path)).parent_path().parent_path() / "output";
    if (!fs::exists(output_dir))
    {
        fs::create_directory(output_dir);
    }

    // 出力ファイルのフルパスを生成
    string raw_path = (output_dir / (output_filename + ".raw")).string();
    string mhd_path = (output_dir / (output_filename + ".mhd")).string();

    // 処理結果をRAWファイルとして保存
    if (is_transformed && holds_alternative<vector<unsigned char>>(processed_data))
    {
        // 階調変換済み（unsigned char型）の場合
        const auto &transformed_data = get<vector<unsigned char>>(processed_data);
        vector<char> raw_data(transformed_data.begin(), transformed_data.end());
        path_obj.save_raw(raw_path, raw_data);
    }
    else if (holds_alternative<vector<short>>(processed_data))
    {
        // フィルタのみ（short型）の場合
        const auto &not_transformed_data = get<vector<short>>(processed_data);
        vector<char> raw_data(reinterpret_cast<const char *>(not_transformed_data.data()),
                              reinterpret_cast<const char *>(not_transformed_data.data()) + not_transformed_data.size() * sizeof(short));
        path_obj.save_raw(raw_path, raw_data);
    }
    else
    {
        cerr << "Processed data type error" << endl;
        return 1;
    }

    // 処理結果のMHDファイルを保存
    path_obj.save_mhd(mhd_path, raw_path, path_obj.get_mhd_info(), is_transformed);

    return 0;
}
