import csv
import os
import socket
import struct
import threading
import time

import cv2
import keyboard
import numpy as np
import zmq

# 全局变量
running = True
image = np.zeros((480, 640, 3), dtype=np.uint8)

# ZMQ端口配置
image_port = 5555  # 用于接收图像
command_port = 5556  # 用于发送速度指令

script_dir = os.path.dirname(os.path.abspath(__file__))

# 建议将文件保存到脚本所在目录下的 "img" 文件夹
save_dir = os.path.join(script_dir, "img")
os.makedirs(save_dir, exist_ok=True)

csv_filename = os.path.join(save_dir, "speed_data.csv")

# 若CSV不存在或为空，则写入表头
if not os.path.exists(csv_filename) or os.stat(csv_filename).st_size == 0:
    with open(csv_filename, mode="w", newline="") as file:
        writer = csv.writer(file)
        writer.writerow(["timestamp", "left_speed", "right_speed", "img_name"])

# ZMQ初始化
ctx = zmq.Context()
# PULL: 接收图像
image_sock = ctx.socket(zmq.PULL)
image_sock.bind(f"tcp://*:{image_port}")
# PUSH: 发送速度
command_sock = ctx.socket(zmq.PUSH)
command_sock.bind(f"tcp://*:{command_port}")

motor_l = 0
motor_r = 0


def generate_image_name(timestamp, counter):
    """
    生成唯一图片文件名
    例如: image_20230408_152345_0.jpg
    """
    return f"image_{timestamp}_{counter}.jpg"


def save_image(image_rgb, counter):
    """
    image_rgb: RGB格式的numpy图像
    把它先转回BGR，再用OpenCV保存到硬盘
    """
    # 生成时间戳(到秒)
    timestamp = time.strftime("%Y%m%d_%H%M%S")
    img_name = generate_image_name(timestamp, counter)
    filename = os.path.join(save_dir, img_name)

    # 转回BGR，以便cv2.imwrite不出现颜色偏差
    image_bgr = cv2.cvtColor(image_rgb, cv2.COLOR_RGB2BGR)
    cv2.imwrite(filename, image_bgr)

    return timestamp, img_name


def save_to_csv(timestamp, motor_l, motor_r, img_name):
    """
    往CSV追加一行，包含时间戳、左右轮速与图像名称
    """
    with open(csv_filename, mode="a", newline="") as file:
        writer = csv.writer(file)
        writer.writerow([timestamp, motor_l, motor_r, img_name])


def control_thread():
    global motor_l, motor_r
    while True:
        # 这里每30ms检测一次键盘
        key = cv2.waitKey(30)
        if keyboard.is_pressed("Esc"):
            # Quit command
            command_sock.send_string("quit")
            break

        elif keyboard.is_pressed("up"):
            motor_l = 41
            motor_r = 40
        elif keyboard.is_pressed("down"):
            motor_l = -50
            motor_r = -50

        elif keyboard.is_pressed("left"):
            motor_l = 20
            motor_r = 40
        elif keyboard.is_pressed("a"):
            motor_l = 40
            motor_r = 50
        elif keyboard.is_pressed("q"):
            motor_l = 20
            motor_r = 50

        elif keyboard.is_pressed("right"):
            motor_l = 40
            motor_r = 20
        elif keyboard.is_pressed("d"):
            motor_l = 50
            motor_r = 40
        elif keyboard.is_pressed("e"):
            motor_l = 50
            motor_r = 20
        else:
            motor_l = 0
            motor_r = 0

        # 发送左右速度到小车
        try:
            command_sock.send_string(f"{motor_l},{motor_r}")
        except zmq.error.ZMQError as e:
            print(f"ZMQ Error: {e}")


def image_receiver_thread():
    global running
    global image
    global motor_l, motor_r
    counter = 0

    while running:
        # 阻塞等待相机端发送来的multipart数据
        try:
            byte_rows, byte_cols, byte_ndim, jpeg_data = image_sock.recv_multipart()
            print("Received image data")
        except zmq.error.ZMQError:
            if not running:
                break
            continue

        row = struct.unpack("i", byte_rows)[0]
        cols = struct.unpack("i", byte_cols)[0]
        ndim = struct.unpack("i", byte_ndim)[0]

        print(f"Received image shape: {row}x{cols}, Channels: {ndim}")

        # JPEG解码
        jpeg_array = np.frombuffer(jpeg_data, dtype=np.uint8)
        if ndim == 3:
            temp_img = cv2.imdecode(jpeg_array, cv2.IMREAD_COLOR)
        else:
            temp_img = cv2.imdecode(jpeg_array, cv2.IMREAD_GRAYSCALE)

        if temp_img is None:
            print("Failed to decode image.")
            continue

        # 如果是彩色图像, 转成RGB
        if ndim == 3:
            img_rgb = cv2.cvtColor(temp_img, cv2.COLOR_BGR2RGB)

            # 保存 & 写csv
            timestamp, img_name = save_image(img_rgb, counter)
            save_to_csv(timestamp, motor_l, motor_r, img_name)

            # 更新显示用全局图像
            image = img_rgb

        else:
            # 灰度图像
            image = cv2.cvtColor(temp_img, cv2.COLOR_GRAY2RGB)

        counter += 1


def gui():
    global running
    window_name = "receiver"
    cv2.namedWindow(window_name, cv2.WINDOW_AUTOSIZE)
    print("Press [ESC] in window to quit GUI.")

    while running:
        # 显示图像
        cv2.imshow(window_name, image)
        key = cv2.waitKey(30)

        # ESC键退出
        if key == 27:  # ESC
            running = False

    cv2.destroyAllWindows()


if __name__ == "__main__":
    ip = socket.gethostbyname(socket.gethostname())
    print("Current IP:", ip)

    # 启动接收图像线程
    thread1 = threading.Thread(target=image_receiver_thread)
    thread1.start()
    print("image_receiver_thread running...")

    # 启动控制线程
    thread3 = threading.Thread(target=control_thread)
    thread3.start()
    print("control_thread running...")

    # 进入GUI循环，直到ESC或其他方式退出
    gui()
    print("GUI ended...")

    # 等待线程结束
    thread1.join()
    thread3.join()

    # 清理ZMQ资源
    image_sock.close()
    command_sock.close()
    ctx.term()

    print("All done. Exiting.")
