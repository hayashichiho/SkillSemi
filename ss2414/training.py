#
# import struct
# import threading
#
# import cv2
# import numpy as np
# import torch
# import torch.nn as nn
# import torchvision.transforms as transforms
# import zmq
# from PIL import Image as PILImage
# from torchvision.models import resnet18
#
# # グローバル変数
# image = np.zeros((480, 640, 3), dtype=np.uint8)
# running = True
#
# # ソケット設定
# ctx = zmq.Context()
# command_sock = ctx.socket(zmq.PUSH)  # コマンド送信用
# command_sock.bind("tcp://*:5556")  # ポート5556で待機
#
# camera_sock = ctx.socket(zmq.PULL)  # カメラ受信用
# camera_sock.bind("tcp://*:5555")  # ポート5555で待機
#
# class DrivingModel(nn.Module):
#     def __init__(self):  # コンストラクタ
#         super(DrivingModel, self).__init__()  # 親クラスのコンストラクタ
#         self.backbone = resnet18(pretrained=True)  # ResNet18の読み込み
#         self.backbone.fc = nn.Linear(
#             self.backbone.fc.in_features, 4
#         )  # 全結合層の出力を4に変更（4クラス分類）
#
#     def forward(self, x):  # 順伝播
#         return self.backbone(x)
#
#
# # モデルの読み込み
# model = DrivingModel()
# model.load_state_dict(torch.load("best_model.pth"))  # 学習済みモデルをロード
# model.eval()
#
# # 画像変換の定義
# transform = transforms.Compose(
#     [
#         transforms.Resize((224, 224)),  # モデルの入力サイズにリサイズ
#         transforms.ToTensor(),  # Tensorに変換
#         transforms.Normalize(mean=[0.5, 0.5, 0.5], std=[0.5, 0.5, 0.5]),  # 正規化
#     ]
# )
#
#
# # # 推論関数
# # def predict(image):
# #     model.eval()
# #     with torch.no_grad():
# #         image = transform(image).unsqueeze(0)  # バッチ次元を追加
# #         output = model(image)
# #         left_speed, right_speed = output[0].tolist()  # 出力をリストに変換
# #         return int(left_speed), int(right_speed)
# def predict(image):
#     model.eval()
#     with torch.no_grad():
#         image = transform(image).unsqueeze(0)  # バッチ次元を追加
#         output = model(image)  # 模型输出
#         predicted_class = torch.argmax(output, dim=1).item()  # 获取预测的类别
#         # 根据类别映射到速度值
#         if predicted_class == 0:  # up
#             left_speed, right_speed = 50, 50
#         elif predicted_class == 1:  # down
#             left_speed, right_speed = -50, -50
#         elif predicted_class == 2:  # left
#             left_speed, right_speed = 20, 40
#         else:  # right
#             left_speed, right_speed = 40, 20
#         return int(left_speed), int(right_speed)
#
# # 画像受信スレッド
# def image_receiver_thread():
#     global image
#     while running:
#         try:
#             # 画像データを受信
#             byte_rows, byte_cols, byte_ndim, jpeg_data = camera_sock.recv_multipart()
#             row = struct.unpack("i", byte_rows)[0]
#             cols = struct.unpack("i", byte_cols)[0]
#             ndim = struct.unpack("i", byte_ndim)[0]
#
#             print(f"Received image shape: {row}x{cols}, Channels: {ndim}")
#
#             # JPEG データをデコード
#             jpeg_array = np.frombuffer(jpeg_data, dtype=np.uint8)
#             img = cv2.imdecode(
#                 jpeg_array, cv2.IMREAD_COLOR if ndim == 3 else cv2.IMREAD_GRAYSCALE
#             )
#             img = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
#             img = cv2.resize(img, (224, 224))  # モデルの入力サイズにリサイズ
#
#             # 推論
#             img = PILImage.fromarray(img)
#             left_speed, right_speed = predict(img)
#             print(f"Predicted speeds: Left={left_speed}, Right={right_speed}")
#
#             # コマンドを送信
#             command_sock.send_string(f"{left_speed},{right_speed}")
#             print("Command sent successfully")
#         except Exception as e:
#             print(f"Error in image_receiver_thread: {e}")
#
#
# # メインループ
# try:
#     thread1 = threading.Thread(target=image_receiver_thread)
#     thread1.start()
#     thread1.join()
# except KeyboardInterrupt:
#     print("Stopped by user")
# finally:
#     command_sock.close()
#     ctx.term()
#     camera_sock.close()
#     print("Sockets closed and resources cleaned up")


# 回帰モデル
import struct
import threading

import cv2
import numpy as np
import torch
import torch.nn as nn
import torchvision.transforms as transforms
import zmq
from PIL import Image as PILImage
from torchvision.models import resnet18

image = np.zeros((480, 640, 3), dtype=np.uint8)
running = True

# ソケット設定
ctx = zmq.Context()
command_sock = ctx.socket(zmq.PUSH)  # コマンド送信用
command_sock.bind("tcp://*:5556")  # ポート5556で待機

camera_sock = ctx.socket(zmq.PULL)  # カメラ受信用
camera_sock.bind("tcp://*:5555")  # `teaching.py` のアドレスとポートに接続


# モデルの定義
class DrivingModel(nn.Module):
    def __init__(self):
        super(DrivingModel, self).__init__()
        self.model = resnet18(pretrained=True)
        self.model.fc = nn.Linear(self.model.fc.in_features, 2)  # 出力を2つに変更

    def forward(self, x):
        return self.model(x)


# モデルの読み込み
model = DrivingModel()
model.load_state_dict(torch.load("best_model.pth"))
model.eval()

# 画像変換の定義
transform = transforms.Compose(
    [
        transforms.Resize((224, 224)),
        transforms.ToTensor(),
        transforms.Normalize(mean=[0.5, 0.5, 0.5], std=[0.5, 0.5, 0.5]),
    ]
)


# 推論関数
def predict(image):
    model.eval()
    with torch.no_grad():
        image = transform(image).unsqueeze(0)  # バッチ次元を追加
        output = model(image)
        left_speed, right_speed = output[0].tolist()
        return int(left_speed), int(right_speed)


#  画像受信スレッド
def image_receiver_thread():
    global image
    while running:
        byte_rows, byte_cols, byte_ndim, jpeg_data = camera_sock.recv_multipart()
        row = struct.unpack("i", byte_rows)[0]
        cols = struct.unpack("i", byte_cols)[0]
        ndim = struct.unpack("i", byte_ndim)[0]

        print(f"Received image shape: {row}x{cols}, Channels: {ndim}")

        # JPEG データをデコード
        jpeg_array = np.frombuffer(jpeg_data, dtype=np.uint8)
        img = cv2.imdecode(
            jpeg_array, cv2.IMREAD_COLOR if ndim == 3 else cv2.IMREAD_GRAYSCALE
        )
        img = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
        img = cv2.resize(img, (224, 224))  # モデルの入力サイズにリサイズ

        # 推論
        img = PILImage.fromarray(img)
        left_speed, right_speed = predict(img)
        print(f"Predicted speeds: Left={left_speed}, Right={right_speed}")

        # コマンドを送信
        # control_thread(left_speed, right_speed)
        command_sock.send_string(f"{left_speed},{right_speed}")
        print("send ok")


# メインループ
try:
    while True:
        # コマンドを受信
        thread1 = threading.Thread(target=image_receiver_thread)
        # thread2 = threading.Thread(target=control_thread, args=(left_speed, right_speed))
        thread1.start()
        # thread2.start()
        thread1.join()
        # thread2.join()
except KeyboardInterrupt:
    print("Stopped by user")
finally:
    command_sock.close()
    ctx.term()
    camera_sock.close()
