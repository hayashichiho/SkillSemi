import threading
import time

import cv2
import numpy as np
import torch
import torch.nn as nn
import torchvision.transforms as transforms
import zmq
from PIL import Image as PILImage
from torchvision.models import resnet18

########################################
# グローバル変数 & 設定
########################################
running = True
latest_frame = None  # 最新の画像フレームを保存するグローバル変数
frame_lock = threading.Lock()  # latest_frame を保護するためのロック

SPEED_MAX = 100.0  # 速度を正規化している場合、推論後に SPEED_MAX を掛け戻す

########################################
# ZMQ 初期化
########################################
ctx = zmq.Context()

# 画像を受信するためのソケット
camera_sock = ctx.socket(zmq.PULL)
camera_sock.bind("tcp://*:5555")
# 注意: Raspberry Pi 側では同じポートで connect("tcp://<PCのIP>:5555") を使用

# 速度コマンドを送信するためのソケット
command_sock = ctx.socket(zmq.PUSH)
command_sock.bind("tcp://*:5556")
# 注意: Raspberry Pi 側では同じポートで connect("tcp://<PCのIP>:5556") を使用

# 最新の画像フレームのみを保持するように設定し、データの蓄積を防ぐ
camera_sock.setsockopt(zmq.RCVHWM, 1)
# camera_sock.setsockopt(zmq.CONFLATE, 1)

print(
    "ZMQ ソケットが設定されました: ポート 5555 で画像を受信し、ポート 5556 でコマンドを送信します。"
)


########################################
# モデルの定義と読み込み
########################################
class DrivingModel(nn.Module):
    def __init__(self):
        super(DrivingModel, self).__init__()
        self.model = resnet18(pretrained=True)
        self.model.fc = nn.Linear(
            self.model.fc.in_features, 2
        )  # 左右の速度 (left_speed, right_speed) を出力

    def forward(self, x):
        return self.model(x)


model = DrivingModel()
state_dict = torch.load("best_model.pth", map_location=torch.device("cpu"))
model.load_state_dict(state_dict)
model.eval()

print("モデルが読み込まれ、推論モードに設定されました。")

########################################
# 画像の前処理
########################################
transform = transforms.Compose(
    [
        transforms.Resize((224, 224)),
        transforms.ToTensor(),
        transforms.Normalize(mean=[0.5, 0.5, 0.5], std=[0.5, 0.5, 0.5]),
    ]
)


def predict_image(img_bgr: np.ndarray):
    """
    入力: OpenCV の BGR 画像 (H, W, 3)
    出力: 左右の車輪速度 (left_speed, right_speed)
    """
    # BGR を RGB に変換し、PIL Image に変換
    img_rgb = cv2.cvtColor(img_bgr, cv2.COLOR_BGR2RGB)
    pil_img = PILImage.fromarray(img_rgb)

    with torch.no_grad():
        input_tensor = transform(pil_img).unsqueeze(0)  # [1, C, H, W]
        outputs = model(input_tensor)  # 出力の形状: [1, 2]
    left_speed, right_speed = outputs[0].tolist()

    left_speed = int(left_speed * SPEED_MAX)
    right_speed = int(right_speed * SPEED_MAX)
    return left_speed, right_speed


########################################
# スレッド 1: 画像を受信
########################################
def receiver_thread():
    global latest_frame
    while running:
        try:
            # 画像データを受信 (ブロッキング)
            jpeg_data = camera_sock.recv()
        except zmq.ZMQError:
            if not running:
                break
            continue

        # JPEG データをデコード
        jpeg_array = np.frombuffer(jpeg_data, dtype=np.uint8)
        img_bgr = cv2.imdecode(jpeg_array, cv2.IMREAD_COLOR)

        if img_bgr is None:
            print("画像のデコードに失敗しました。")
            continue

        # 最新のフレームを更新 (スレッドセーフ)
        with frame_lock:
            latest_frame = img_bgr


########################################
# スレッド 2: 推論とコマンド送信
########################################
def inference_thread():
    while running:
        # 最新のフレームをコピーして取得
        with frame_lock:
            if latest_frame is None:
                continue
            frame_copy = latest_frame.copy()
        left_speed, right_speed = predict_image(frame_copy)
        print(f"[推論結果] => 左={left_speed}, 右={right_speed}")
        command_sock.send_string(f"{left_speed},{right_speed}")
        # 推論間隔を調整 (必要に応じて変更可能)
        time.sleep(0.05)


########################################
# メインループ: 画像を表示
########################################
def main_loop():
    cv2.namedWindow("Camera", cv2.WINDOW_AUTOSIZE)
    while running:
        # 最新のフレームをコピーして表示
        with frame_lock:
            if latest_frame is not None:
                display_frame = latest_frame.copy()
            else:
                display_frame = None

        if display_frame is not None:
            cv2.imshow("Camera", display_frame)

        key = cv2.waitKey(10)
        if key == 27:  # ESC キーで終了
            break

        time.sleep(0.05)  # CPU 使用率を下げるためのスリープ

    cv2.destroyAllWindows()


########################################
# エントリーポイント
########################################
if __name__ == "__main__":
    try:
        t_recv = threading.Thread(target=receiver_thread, daemon=True)
        t_infer = threading.Thread(target=inference_thread, daemon=True)

        t_recv.start()
        t_infer.start()

        # メインスレッドで画像を表示
        main_loop()
    except KeyboardInterrupt:
        print("ユーザーによる中断 (KeyboardInterrupt)。")
    finally:
        running = False
        t_recv.join()
        t_infer.join()

        camera_sock.close()
        command_sock.close()
        ctx.term()
        print("すべて完了しました。終了します。")
