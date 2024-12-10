// window_parameters.h
#pragma once
#include <vector>

class WindowParameters {
   private:
    bool _processing;  // ウィンドウ処理の有無
    int _level;        // ウィンドウレベル
    int _width;        // ウィンドウ幅

   public:
    // コンストラクタ
    WindowParameters(bool processing, int level, int width) : _processing(processing), _level(level), _width(width) {}

    bool get_processing() const {
        return _processing;
    }
    int get_level() const {
        return _level;
    }
    int get_width() const {
        return _width;
    }

    // ウィンドウ処理の適用
    void apply_window_processing(std::vector<unsigned char>& image_data) const;
};
