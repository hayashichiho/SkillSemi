#include "opencv2/opencv.hpp"
#include <opencv2/core/core.hpp>
#include <opencv2/highgui/highgui.hpp>
#include "zmq.hpp"
#include <stdlib.h>
#include <stdio.h>
#include <iostream>
#include <vector>
#include <thread>
#include <atomic>

// メッセージ送信後にメモリを解放するメソッド
void my_free(void* data, void* hint)
{
    free(data);
}

// 画像を取得してZMQで送信するメソッド
void send_image(cv::VideoCapture& cap, zmq::socket_t& socket, int command)
{
    try {
        cv::Mat image; // 取得したフレーム

        // フレームを取得
        if (cap.read(image)) {
            // 画像情報
            int32_t info[2];
            info[0] = static_cast<int32_t>(image.rows);
            info[1] = static_cast<int32_t>(image.cols);

            // 画像情報とコマンドを送信
            zmq::message_t command_msg(&command, sizeof(command));
            socket.send(command_msg, zmq::send_flags::sndmore);

            for (int i = 0; i < 2; i++) {
                zmq::message_t msg(&info[i], sizeof(int32_t), nullptr);
                socket.send(msg, zmq::send_flags::sndmore);
            }

            // ピクセルデータを送信
            std::vector<uchar> data(image.total() * image.elemSize());
            memcpy(data.data(), image.data, data.size());

            // メッセージオブジェクトの作成
            zmq::message_t msg2(data.data(), data.size(), nullptr);

            // メッセージの送信
            socket.send(msg2, zmq::send_flags::none);

            // レスポンスを受信
            zmq::message_t reply;
            socket.recv(reply, zmq::recv_flags::none);
            std::string reply_str(static_cast<char*>(reply.data()), reply.size());
        }
        else {
            std::cerr << "フレームの取得に失敗しました" << std::endl;
        }
    }
    catch (const std::exception& e) {
        std::cerr << "send_image関数内で例外が発生しました: " << e.what() << std::endl;
    }
}

// メイン関数
int main(int argc, char* argv[]) {
    zmq::context_t context(1);
    zmq::socket_t socket(context, ZMQ_REQ);
    zmq::socket_t control_socket(context, ZMQ_PULL);

    try {
        std::cout << "プログラム開始" << std::endl;

        // ZMQコネクションを作成
        socket.connect("tcp://localhost:5555");
        std::cout << "localhost:5555に接続しました" << std::endl;

        control_socket.bind("tcp://localhost:5557");
        std::cout << "localhost:5557にバインドしました" << std::endl;

        cv::VideoCapture cap(0, cv::CAP_DSHOW); // デバイスのオープン

        // カメラデバイスが正常にオープンしたか確認
        if (!cap.isOpened()) {
            std::cerr << "カメラを起動できませんでした" << std::endl;
            return -1;
        }

        // カメラのプロパティを設定
        cap.set(cv::CAP_PROP_FRAME_WIDTH, 640);
        cap.set(cv::CAP_PROP_FRAME_HEIGHT, 480);
		cap.set(cv::CAP_PROP_FPS, 30);

        std::atomic<bool> is_running(true); // プロジェクト進行中フラグ
        std::atomic<bool> is_sending(false); // c#に画像を送るフラグ
        int command;

        std::thread control_thread([&]() {
            while (is_running) {
                // C#からのフラグに対応
                zmq::message_t message;
                control_socket.recv(message, zmq::recv_flags::none);
                command = *static_cast<int*>(message.data());
                std::cout << "メッセージを受信しました: " << command << std::endl;

                if (command == 1) { // start
                    is_sending = true;
                }
                else if (command == 0) { // stop
                    is_sending = false;
                }
                else if (command == -1) { // exit
                    is_sending = false;

					std::cout << "5555ポートに終了コマンドを送信します．" << std::endl;
                    // 5555ポートに終了コマンドを送信
                    send_image(cap, socket, command);

                    // プログラム終了時の処理
                    std::cout << "プログラム終了" << std::endl;

                    // ポートを解放
                    std::cout << "ソケットを閉じます..." << std::endl;
                    socket.close();
                    control_socket.close();
                    context.close();
                    std::cout << "ソケットが閉じられました。" << std::endl;

                    is_running = false;
                }
            }
        });

        try {
            while (is_running) {
                // Pythonに画像とフラグを送る
                if (is_sending) {
                    send_image(cap, socket, command);
                }
                // 適切なフレームレートを維持するために待機
                cv::waitKey(30);
            }
        }
        catch (const std::exception& e) {
            std::cerr << "ZMQのエラーが発生しました: " << e.what() << std::endl;
            return -1;
        }

        control_thread.join();
    }
    catch (const std::exception& e) {
        std::cerr << "例外が発生しました: " << e.what() << std::endl;
        return -1;
    }

    return 0;
}
