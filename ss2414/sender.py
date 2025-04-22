import struct
import threading
import time

import cv2
import RPi.GPIO as GPIO
import zmq
from picamera2 import Picamera2

# 画像送信用のポート
image_conn_str = "tcp://192.168.23.177:5555"
# コマンド送信用のポート
command_conn_str = "tcp://192.168.23.177:5556"

# ソケットの作成
ctx = zmq.Context()
image_sock = ctx.socket(zmq.PUSH)  # 画像送信用
image_sock.connect(image_conn_str)

command_sock = ctx.socket(zmq.PULL)  # コマンド送信用
command_sock.connect(command_conn_str)
print("socket finish")

# カメラの設定
picam2 = Picamera2()
config = picam2.create_still_configuration(main={"size": (640, 480)})
picam2.configure(config)

picam2.start()
print("camera finish")

# GPIO 設置
GPIO.setmode(GPIO.BOARD)
MA_IN1 = 19
MA_IN2 = 21
MA_PWM = 23
MB_IN1 = 15
MB_IN2 = 13
MB_PWM = 11
GPIO.setup(MA_IN1, GPIO.OUT, initial=GPIO.LOW)
GPIO.setup(MA_IN2, GPIO.OUT, initial=GPIO.LOW)
GPIO.setup(MA_PWM, GPIO.OUT, initial=GPIO.LOW)
GPIO.setup(MB_IN1, GPIO.OUT, initial=GPIO.LOW)
GPIO.setup(MB_IN2, GPIO.OUT, initial=GPIO.LOW)
GPIO.setup(MB_PWM, GPIO.OUT, initial=GPIO.LOW)
pwm_a = GPIO.PWM(MA_PWM, 100)
pwm_b = GPIO.PWM(MB_PWM, 100)
pwm_a.start(0)
pwm_b.start(0)
pwm_a.ChangeDutyCycle(80)
pwm_b.ChangeDutyCycle(80)
print("GPIO finish")

# def func_brake():
#     print("brake")
#     GPIO.output(MA_IN1, GPIO.HIGH)
#     GPIO.output(MA_IN2, GPIO.HIGH)
#     GPIO.output(MB_IN1, GPIO.HIGH)
#     GPIO.output(MB_IN2, GPIO.HIGH)

# def func_forward():
#     print("forward")
#     GPIO.output(MA_IN1, GPIO.LOW)
#     GPIO.output(MA_IN2, GPIO.HIGH)
#     GPIO.output(MB_IN1, GPIO.LOW)
#     GPIO.output(MB_IN2, GPIO.HIGH)


# 車輪を動かす関数
def move(left_speed, right_speed):
    print(f"左电机速度：{left_speed}, 右电机速度：{right_speed}")

    pwm_a.ChangeDutyCycle(abs(left_speed))
    pwm_b.ChangeDutyCycle(abs(right_speed))

    if left_speed > 0:
        GPIO.output(MA_IN1, GPIO.HIGH)
        GPIO.output(MA_IN2, GPIO.LOW)
    else:
        GPIO.output(MA_IN1, GPIO.LOW)
        GPIO.output(MA_IN2, GPIO.HIGH)

    if right_speed > 0:
        GPIO.output(MB_IN1, GPIO.HIGH)
        GPIO.output(MB_IN2, GPIO.LOW)
    else:
        GPIO.output(MB_IN1, GPIO.LOW)
        GPIO.output(MB_IN2, GPIO.HIGH)


# コマンドを受け取る関数
def receive_commands():
    try:
        while True:
            print("receiving")
            command = command_sock.recv_string()
            print(f"command {command}")
            if command == "quit":
                break
            left_speed, right_speed = map(int, command.split(","))
            move(left_speed, right_speed)
    except KeyboardInterrupt:
        print("Control thread quit")


# 画像をキャプチャして送信する関数
def capture_and_send():
    try:
        while True:
            # print("Capturing img...")
            img = picam2.capture_array()
            if img is None:
                print("No image captured")
                continue

            # 画像をJPEG形式に圧縮
            ret, jpeg_img = cv2.imencode(".jpg", img)
            if not ret:
                print("Failed to compress image")
                continue

            jpeg_data = jpeg_img.tobytes()
            height, width = img.shape[:2]
            ndim = img.ndim

            # printf"Image shape: {height}x{width}, Channels: {ndim}")

            # データをパック
            data = [
                struct.pack("i", height),
                struct.pack("i", width),
                struct.pack("i", ndim),
                jpeg_data,
            ]

            # データを送信
            image_sock.send_multipart(data)
            print("Sent image data")
            # recv = image_sock.recv_string()
            # print("Receiver response:", recv)

    except KeyboardInterrupt:
        print("Camera thread quit")

    time.sleep(0.05)


camera_thread = threading.Thread(target=capture_and_send)  # カメラスレッド
control_thread = threading.Thread(target=receive_commands)  # コントロールスレッド

# スレッドの開始
camera_thread.start()
print("camera thread start")
control_thread.start()
print("control thread start")

# スレッドの終了
camera_thread.join()
control_thread.join()

# 終了処理
print("Exiting...")
picam2.stop()
pwm_a.stop()
pwm_b.stop()
GPIO.cleanup()
