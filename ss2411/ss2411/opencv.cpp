#include <iostream>
#include <opencv2/opencv.hpp>

int main(int argc, char* argv[]) {
    cv::VideoCapture cap(0);  // デバイスのオープン

    if (!cap.isOpened())  // カメラデバイスが正常にオープンしたか確認
    {
        // 読み込みに失敗したときの処理
        std::cerr << "Error: Could not open camera" << std::endl;
        return -1;
    }

    cv::Mat frame;           // 取得したフレーム
    while (cap.read(frame))  // 無限ループ
    {
        cv::imshow("win", frame);  // 画像を表示
        const int key = cv::waitKey(1);
        if (key == 'q')  // qボタンが押されたとき
        {
            break;
        }
    }

    cv::destroyAllWindows();
    return 0;
}

// g++ -o my_program opencv.cpp `pkg-config --cflags --libs opencv4`
// ./my_program
