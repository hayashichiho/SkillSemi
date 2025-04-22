import threading
import time

import cv2
import RPi.GPIO as GPIO
import zmq
from picamera2 import Picamera2

#####################################
# ZMQ 設定
#####################################
image_conn_str = "tcp://192.168.23.177:5555"  # PC側で bind しているアドレス:5555
command_conn_str = "tcp://192.168.23.177:5556"  # PC側で bind しているアドレス:5556

ctx = zmq.Context()
image_sock = ctx.socket(zmq.PUSH)
image_sock.connect(image_conn_str)
image_sock.setsockopt(zmq.SNDHWM, 1)  # 送信バッファの最大サイズを設定

command_sock = ctx.socket(zmq.PULL)
command_sock.connect(command_conn_str)

print("ZMQ ソケットが作成され、接続されました。")

#####################################
# Picamera2 設定
#####################################
picam2 = Picamera2()
# プレビュー/ビデオモードを使用して、より高いフレームレートで連続キャプチャ
config = picam2.create_preview_configuration(main={"size": (640, 480)})
picam2.configure(config)
picam2.start()
print("カメラがプレビューモードで開始されました。")


#####################################
# GPIO 設定
#####################################
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

pwm_a = GPIO.PWM(MA_PWM, 100)  # 周波数100Hz
pwm_b = GPIO.PWM(MB_PWM, 100)
pwm_a.start(0)
pwm_b.start(0)

print("GPIO と PWM の設定が完了しました。")


#####################################
# モーター制御関数
#####################################
def move(left_speed, right_speed):
    """
    左右のモーターを回転させる
    left_speed, right_speed ∈ [-100, 100]
    """
    print(f"左モーター速度: {left_speed}, 右モーター速度: {right_speed}")

    pwm_a.ChangeDutyCycle(abs(left_speed))
    pwm_b.ChangeDutyCycle(abs(right_speed))

    if left_speed >= 0:
        GPIO.output(MA_IN1, GPIO.HIGH)
        GPIO.output(MA_IN2, GPIO.LOW)
    else:
        GPIO.output(MA_IN1, GPIO.LOW)
        GPIO.output(MA_IN2, GPIO.HIGH)

    if right_speed >= 0:
        GPIO.output(MB_IN1, GPIO.HIGH)
        GPIO.output(MB_IN2, GPIO.LOW)
    else:
        GPIO.output(MB_IN1, GPIO.LOW)
        GPIO.output(MB_IN2, GPIO.HIGH)


def stop_motors():
    """
    モーターを停止させる
    """
    pwm_a.ChangeDutyCycle(0)
    pwm_b.ChangeDutyCycle(0)
    # 必要に応じて IN1=IN2=HIGH にしてハードウェアブレーキをかける
    GPIO.output(MA_IN1, GPIO.LOW)
    GPIO.output(MA_IN2, GPIO.LOW)
    GPIO.output(MB_IN1, GPIO.LOW)
    GPIO.output(MB_IN2, GPIO.LOW)
    print("モーターが停止しました。")


#####################################
# コマンド受信スレッド
#####################################
def receive_commands():
    try:
        left_count = 0
        right_count = 0
        left_flag = False
        right_flag = False

        while True:
            command = command_sock.recv_string()
            print(f"[コマンド] => {command}")
            if command == "quit":
                # 相手から "quit" が送られてきた場合、モーターを停止してループを抜ける
                stop_motors()
                break

            left_speed, right_speed = map(int, command.split(","))
            if left_speed - right_speed > 15:
                right_speed = right_speed * 0.24
                left_speed = left_speed * 0.93
                left_count += 1
                print("左モーターの速度を調整")
            elif left_speed - right_speed > 12:
                right_speed = right_speed * 0.34
                left_speed = left_speed * 0.98
                left_count += 1
                print("左モーターの速度を調整")
            elif right_speed - left_speed > 15:
                left_speed = left_speed * 0.24
                right_speed = right_speed * 0.93
                right_count += 1
                print("左モーターの速度を調整")
            elif left_speed - right_speed < -12:
                left_speed = left_speed * 0.34
                right_speed = right_speed * 0.98
                right_count += 1
                print("右モーターの速度を調整")
            else:
                right_count = 0
                left_count = 0

            if left_count > 10:
                left_flag = True
            elif right_count > 10:
                right_flag = True

            if left_count == 0 and right_count == 0:
                if left_flag:
                    left_speed = left_speed * 2.2
                    left_flag = False
                elif right_flag:
                    right_speed = right_speed * 2.2
                    right_flag = False

            move(left_speed / 2, right_speed / 2)

    except KeyboardInterrupt:
        print("receive_commands: キーボード割り込み")
    finally:
        print("receive_commands: 終了中...")


#####################################
# カメラスレッド: 画像を連続キャプチャして送信
#####################################
def capture_and_send():
    try:
        while True:
            # 1フレームをキャプチャ
            img = picam2.capture_array()
            if img is None:
                print("画像がキャプチャされませんでした。")
                time.sleep(0.1)
                continue

            # JPEG形式に圧縮
            ret, jpeg_img = cv2.imencode(".jpg", img, [cv2.IMWRITE_JPEG_QUALITY, 50])
            if not ret:
                print("画像の圧縮に失敗しました。")
                continue

            jpeg_data = jpeg_img.tobytes()
            # JPEGデータのみを送信
            image_sock.send(jpeg_data)

            # フレームレートを制御 (例: 約10FPS)
            time.sleep(0.03)
    except KeyboardInterrupt:
        print("capture_and_send: キーボード割り込み")
    finally:
        print("capture_and_send: 終了中...")


#####################################
# メイン関数
#####################################
if __name__ == "__main__":
    camera_thread = threading.Thread(target=capture_and_send)
    control_thread = threading.Thread(target=receive_commands)

    camera_thread.start()
    print("camera_thread が開始されました。")
    control_thread.start()
    print("control_thread が開始されました。")

    try:
        camera_thread.join()
        control_thread.join()
    except KeyboardInterrupt:
        print("メイン: キーボード割り込みが発生しました。")
    finally:
        print("メイン: クリーンアップ中...")
        stop_motors()  # モーターを停止
        picam2.stop()  # カメラを停止

        pwm_a.stop()
        pwm_b.stop()
        GPIO.cleanup()
        ctx.term()

        print("すべて完了しました。終了します。")
