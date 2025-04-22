import queue
import struct
import threading
import time

import cv2
import numpy as np
import torch
import torch.nn as nn
import zmq
from torchvision import transforms
from torchvision.models import ResNet18_Weights, resnet18

# グローバル変数
image_queue = queue.Queue(maxsize=1)  # 最新の画像のみ処理
running = True

# ソケット設定
ctx = zmq.Context()
command_sock = ctx.socket(zmq.PUSH)  # コマンド送信用
command_sock.bind("tcp://*:5556")  # ポート5556で待機

camera_sock = ctx.socket(zmq.PULL)  # カメラ受信用
camera_sock.bind("tcp://*:5555")  # ポート5555で待機
camera_sock.setsockopt(zmq.RCVHWM, 1)  # 受信バッファを 1 に設定
camera_sock.setsockopt(zmq.CONFLATE, 1)  # 最新のデータのみ保持

# 受信バッファをクリア
while camera_sock.poll(0):
    camera_sock.recv_multipart(zmq.NOBLOCK)


# モデル定義
class ClassificationModel(nn.Module):
    def __init__(self, num_classes, dropout_rate):
        super(ClassificationModel, self).__init__()
        self.feature_extractor = resnet18(weights=ResNet18_Weights.DEFAULT)
        self.feature_extractor.fc = nn.Sequential(
            nn.Linear(512, 256),
            nn.ReLU(),
            nn.Dropout(dropout_rate),
            nn.Linear(256, num_classes),
        )

    def forward(self, x):
        return self.feature_extractor(x)


# モデルの読み込み
device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
num_classes = 3
dropout_rate = 0.2
model = ClassificationModel(num_classes=num_classes, dropout_rate=dropout_rate).to(
    device
)
model.load_state_dict(
    torch.load("best_model_lr0.001_bs16_dr0.2.pth", map_location=device)
)
model.eval()


# 推論関数
def predict(image):
    try:
        model.eval()
        with torch.no_grad():
            # 画像を前処理
            image = cv2.resize(image, (224, 224))  # モデルの入力サイズにリサイズ
            image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)  # RGBに変換
            image = (
                transforms.ToTensor()(image).unsqueeze(0).to(device)
            )  # トランスフォーム適用

            # 推論
            output = model(image)
            print(f"Raw model output: {output}")  # 推論結果を出力
            _, predicted = torch.max(output, 1)
            predicted_class = predicted.item()
            print(f"Predicted class: {predicted_class}")  # 予測クラスを出力

            # クラスラベルに基づいて速度を決定
            if predicted_class == 0:
                left_speed, right_speed = 52, 50
            elif predicted_class == 1:
                left_speed, right_speed = 20, 40
            elif predicted_class == 2:
                left_speed, right_speed = 40, 20
            else:
                left_speed, right_speed = 0, 0  # デフォルト値

            return left_speed, right_speed
    except Exception as e:
        print(f"Error in predict function: {e}")
        return 0, 0


# 画像受信スレッド
def image_receiver_thread():
    while running:
        try:
            if camera_sock.poll(100):
                byte_rows, byte_cols, byte_ndim, jpeg_data = (
                    camera_sock.recv_multipart()
                )
                row = struct.unpack("i", byte_rows)[0]
                cols = struct.unpack("i", byte_cols)[0]
                ndim = struct.unpack("i", byte_ndim)[0]

                print(f"Received image shape: {row}x{cols}, Channels: {ndim}")

                jpeg_array = np.frombuffer(jpeg_data, dtype=np.uint8)
                img = cv2.imdecode(jpeg_array, cv2.IMREAD_COLOR)

                if img is not None:
                    if not image_queue.full():
                        image_queue.put(img)
                        cv2.imshow("Received Image", img)
                        cv2.waitKey(1)
                    else:
                        image_queue.get()
                        image_queue.put(img)

        except Exception as e:
            print(f"Error in image_receiver_thread: {e}")
            time.sleep(0.02)


# 推論スレッド
def process_image_thread():
    while running:
        img = image_queue.get()
        left_speed, right_speed = predict(img)
        print(f"Predicted speeds: Left={left_speed}, Right={right_speed}")
        command_sock.send_string(f"{left_speed},{right_speed}")


# メインループ
try:
    thread1 = threading.Thread(target=image_receiver_thread, daemon=True)
    thread2 = threading.Thread(target=process_image_thread, daemon=True)

    thread1.start()
    thread2.start()

    thread1.join()
    thread2.join()
except KeyboardInterrupt:
    print("Stopped by user")
finally:
    command_sock.close()
    ctx.term()
    camera_sock.close()
