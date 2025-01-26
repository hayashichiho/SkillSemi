#include "image_processing.h"
#include <iostream>
#include <vector>
#include <string>
#include <variant>
#include <filesystem>

using namespace std;
namespace fs = std::filesystem;

int main(int argc, char* argv[]) {
    if (argc < 2) {
        cerr << "Usage: " << argv[0] << " <parameter file>" << endl;
        return 1;
    }

    /* 読み取るファイルの決定 */
    string txt_path = argv[1];
    Path path_obj;

    /* txtファイル読み込み */
    path_obj.load_from_file(txt_path);

    /* txtファイルの内容を表示 */
    path_obj.print_txt_file(txt_path);

    /* Inputキーの値を取得し、.mhdを付ける */
    string input_mhd_file;
    input_mhd_file = path_obj.get_input_mhd_file(fs::path(txt_path).parent_path().string());

    /* mhdファイル読み込み */
    path_obj.load_mhd_file(input_mhd_file);

    /* mhdファイルの内容を表示 */
    path_obj.print_mhd_file(input_mhd_file);
    
    /* ElementDataFileの値を取得 */
    string element_data_file;
    try {
        element_data_file = path_obj.get_mhd_info().at("ElementDataFile");
    } catch (const out_of_range& e) {
        cerr << "ElementDataFile not found in mhd file" << endl;
        return 1;
    }
    element_data_file.erase(0, element_data_file.find_first_not_of(" \n\r\t")); // 先頭のスペースを削除
    element_data_file.erase(element_data_file.find_last_not_of(" \n\r\t")+1); // 末尾のスペースを削除

    /* 画像データファイルのパスを作成 */
    string image_data_file = txt_path.substr(0, txt_path.find_last_of('/')) + "/" + element_data_file;

    /* 画像データを読み込み */
    vector<short> image_data = path_obj.load_image_data(image_data_file);
    
    /* 画像データの処理 */
    bool is_transformed = false;
    auto processed_data = process_image_data(image_data, path_obj.get_parameters(), path_obj.get_mhd_info(), is_transformed);

    /* 出力ファイル名を取得 */
    string output_filename;
    try {
        output_filename = path_obj.get_parameters().at(1).substr(path_obj.get_parameters().at(1).find('=') + 1);
        output_filename.erase(0, output_filename.find_first_not_of(" \n\r\t")); // 先頭のスペースを削除
        output_filename.erase(output_filename.find_last_not_of(" \n\r\t")+1); // 末尾のスペースを削除
    } catch (const out_of_range& e) {
        cerr << "Output not found in parameters" << endl;
        return 1;
    }

    /* rawファイルとして保存 */
    if (is_transformed) {
        auto transformed_data = get<vector<unsigned char>>(processed_data);
        vector<char> raw_data(transformed_data.begin(), transformed_data.end());
        path_obj.save_raw(output_filename + ".raw", raw_data);
    } else {
        auto not_transformed_data = get<vector<short>>(processed_data);
        vector<char> raw_data(not_transformed_data.begin(), not_transformed_data.end());
        path_obj.save_raw(output_filename + ".raw", raw_data);
    }

    /* mhdファイルとして保存 */
    path_obj.save_mhd(output_filename + ".mhd", output_filename + ".raw", path_obj.get_mhd_info(), is_transformed);

    /* mhdファイルの内容を表示 */
    path_obj.print_mhd_file(output_filename + ".mhd");

    return 0;
}