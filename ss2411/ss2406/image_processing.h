#ifndef IMAGE_PROCESSING_H
#define IMAGE_PROCESSING_H

#include <vector>
#include <string>
#include <map>
#include <variant>

using namespace std;

class ImageProcessing {
public:
    ImageProcessing(const string& filter_type, int kernel_size);
    vector<short> apply(const vector<short>& image_data, int width, int height);

private:
    string filter_type;
    int kernel_size;

    vector<short> apply_sobel_x(const vector<short>& image_data, int width, int height);
    vector<short> apply_sobel_y(const vector<short>& image_data, int width, int height);
    vector<short> apply_moving_average(const vector<short>& image_data, int width, int height);
    vector<short> apply_median_filter(const vector<short>& image_data, int width, int height);
};

vector<unsigned char> window_transform(const vector<short>& image_data, int window_level, int window_width);
variant<vector<short>, vector<unsigned char>> process_image_data(const vector<short>& image_data, const vector<string>& parameters, const map<string, string>& mhd_info, bool& is_transformed);

class Path {
public:
    void load_from_file(const string& filepath);
    void print_txt_file(const string& filepath) const;
    void load_mhd_file(const string& filepath);
    void print_mhd_file(const string& filepath) const;
    string get_input_mhd_file(const string& base_path) const;
    const map<string, string>& get_mhd_info() const;
    const vector<string>& get_parameters() const;
    void save_raw(const string& filename, const vector<char>& data) const;
    void save_mhd(const string& filename, const string& raw_filename, const map<string, string>& mhd_info, bool is_transformed) const;
    vector<short> load_image_data(const string& filepath) const;

private:
    map<string, string> mhd_info;
    vector<string> parameters;
    string input_mhd_file;
};

#endif