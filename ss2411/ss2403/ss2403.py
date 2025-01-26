import os
import tkinter as tk
import tkinter.filedialog
import tkinter.messagebox

import cv2
import numpy as np
from PIL import Image, ImageTk


class GuiApplication(tk.Frame):
    """
    顔認識アプリケーションのクラス
    """

    def __init__(self, master=None):
        super().__init__(master)

        self.master.title("Face Detection Application")  # メインウィンドウタイトル
        self.master.geometry("520x610")  # メインウィンドウサイズ
        self.master.configure(bg="#ffffcc")
        self.create_widgets()

        # 顔認識
        self.face_detect()

    def create_widgets(self):
        """
        メインウィンドウの中身の作成
        """
        # 顔画像の背景となる画面
        self.canvas = tk.Canvas(self.master, bg="#ffe4e1", width=520, height=520)
        self.canvas.place(x=0, y=0)

        # 画像選択ボタン
        self.select_btn = tk.Button(
            text="Select Image", command=self.select_file, width=10, bg="#a9a9a9"
        )
        self.select_btn.place(x=190, y=530)

        # 別ウィンドウを開くボタン
        self.open_sub_window_btn = tk.Button(
            text="Process and Save Image",
            command=lambda: self.open_sub_window(self.image),
            width=30,
            bg="#a9a9a9",
        )

    def open_sub_window(self, original_image):
        """
        別ウィンドウを開いてモザイクやぼかしの処理を行う
        """
        # 新しいウィンドウを作成
        process_window = tk.Toplevel(self.master)
        process_window.title("Process Image")  # ウィンドウのタイトル
        process_window.geometry("230x700")  # ウィンドウのサイズ
        process_window.configure(bg="#FFCC99")

        #  顔認識済画像情報保存
        self.save(process_window)

        #  モザイク
        self.mosaic(process_window)

        #  ぼかし
        self.blur(process_window)

        #  顔変更
        self.change(process_window, original_image)

        #  色黒処理
        self.blown_skin(process_window)

    def save(self, process_window):
        """
        顔認識済画像情報保存
        """
        #  顔認識済画像情報保存ラベル
        save_label = tk.Label(
            process_window, text="Save Detected Image as:", bg="#FFCC99"
        )
        save_label.place(x=30, y=10)

        #  顔認識済画像情報保存ファイル名
        self.entry_widget = tk.Entry(process_window)
        self.entry_widget.place(x=20, y=40)

        #  顔認識済画像情報保存ボタン
        self.save_btn = tk.Button(
            process_window,
            text="Save",
            command=self.save_results,
            width=10,
            bg="#c0c0c0",
        )
        self.save_btn.place(x=53, y=70)

    def mosaic(self, process_window):
        """
        モザイク適用
        """
        #  モザイクスライダーラベル
        mosaic_label = tk.Label(process_window, text="Mosaic Level:", bg="#FFCC99")
        mosaic_label.place(x=63, y=140)

        #  モザイクスライダー
        self.mosaic_btn = tk.Scale(
            process_window, from_=0, to=100, orient=tk.HORIZONTAL
        )
        self.mosaic_btn.place(x=60, y=170)

        #  モザイク適用ボタン
        self.mosaic_apply_btn = tk.Button(
            process_window,
            text="Apply Mosaic",
            command=self.mosaic_face,
            width=10,
            bg="#c0c0c0",
        )
        self.mosaic_apply_btn.place(x=52, y=220)

    def blur(self, process_window):
        """
        ぼかし適用
        """
        #  ぼかしスライダーラベル
        blur_label = tk.Label(process_window, text="Blur Level:", bg="#FFCC99")
        blur_label.place(x=75, y=290)

        #  ぼかしスライダー
        self.blur_btn = tk.Scale(process_window, from_=0, to=100, orient=tk.HORIZONTAL)
        self.blur_btn.place(x=60, y=320)

        #  ぼかし適用ボタン
        self.blur_apply_btn = tk.Button(
            process_window,
            text="Apply Blur",
            command=self.blur_face,
            width=10,
            bg="#c0c0c0",
        )
        self.blur_apply_btn.place(x=52, y=370)

    def change(self, process_window, original_image):
        """
        顔変更
        """
        #  色黒ラベル
        change_label = tk.Label(process_window, text="Select Face:", bg="#FFCC99")
        change_label.place(x=69, y=440)

        #  顔変更ボタン
        self.change_apply_btn = tk.Button(
            process_window,
            text="Change Face",
            command=lambda: self.change_face(original_image),
            width=10,
            bg="#c0c0c0",
        )
        self.change_apply_btn.place(x=52, y=470)

    def blown_skin(self, process_window):
        """
        肌を色黒にする
        """
        #  色黒ラベル
        blown_label = tk.Label(process_window, text="Blown Skin:", bg="#FFCC99")
        blown_label.place(x=68, y=540)

        #  色黒スライダー
        self.blown_btn = tk.Scale(process_window, from_=0, to=100, orient=tk.HORIZONTAL)
        self.blown_btn.place(x=60, y=570)

        # 色黒適用ボタン
        self.blown_apply_btn = tk.Button(
            process_window,
            text="Apply blown skin",
            command=lambda: self.apply_blown_skin(self.blown_btn.get()),
            width=15,
            bg="#c0c0c0",
        )
        self.blown_apply_btn.place(x=30, y=620)

    def face_detect(self):
        """
        モデルの認識
        """
        current_directory = os.path.abspath(os.path.dirname(__file__))
        weights_path = os.path.join(
            current_directory, "face_detection_yunet_2023mar.onnx"
        )

        if not os.path.exists(weights_path):
            tk.messagebox.showerror("Error", f"Model file not found: {weights_path}")
            exit()

        self.face_detector = cv2.FaceDetectorYN_create(weights_path, "", (0, 0))

    def select_file(self):
        """
        画像選択方法
        """
        # ファイルの選択
        file_name = tk.filedialog.askopenfilename(
            filetypes=[("Image Files", "*.png*.jpg*.jpeg")]
        )

        if os.path.isfile(file_name):
            # ファイルを開く
            self.open_image_file(file_name)

            #  バウンディングボックス表示
            if self.faces is not None:
                self.dounding_box(self.faces)

                #  別ウィンドウを開くボタンの配置
                self.open_sub_window_btn.place(x=100, y=570)

                # 別ウィンドウを開く
                self.open_sub_window(self.image)

    def open_image_file(self, file_name):
        """
        画像読み込み
        """
        # 画像を読み込む
        cv2_image = cv2.imread(file_name)

        if cv2_image is None:
            tk.messagebox.showerror("Error", "Failed to load the image.")
            return

        max_canvas_size = (512, 512)
        cv2_image = self.resize_image_to_fit(cv2_image, max_canvas_size)

        # 顔認識
        self.detect_faces(cv2_image)

    def resize_image_to_fit(self, image, max_size):
        """
        画像を最大サイズに収めるようにアスペクト比を維持してリサイズする
        """
        img_height, img_width = image.shape[:2]
        max_width, max_height = max_size

        scale = min(max_width / img_width, max_height / img_height)

        if scale < 1:
            new_size = (int(img_width * scale), int(img_height * scale))
            image = cv2.resize(image, new_size, interpolation=cv2.INTER_AREA)

        return image

    def detect_faces(self, cv2_image):
        """
        顔認識
        """
        # 画像が3チャンネル以外の場合は3チャンネルに変換する
        channels = 1 if len(cv2_image.shape) == 2 else cv2_image.shape[2]
        if channels == 1:
            cv2_image = cv2.cvtColor(cv2_image, cv2.COLOR_GRAY2BGR)
        elif channels == 4:
            cv2_image = cv2.cvtColor(cv2_image, cv2.COLOR_BGRA2BGR)

        self.image = cv2_image.copy()

        # 入力サイズ指定
        height, width, _ = self.image.shape
        self.face_detector.setInputSize((width, height))

        # 顔検出
        _, self.faces = self.face_detector.detect(self.image)
        if self.faces is None:
            tk.messagebox.showerror("Error", "can't detect the faces.")
            return

    def dounding_box(self, faces):
        """
        検出した画像のバウンディングボックスを描画
        """
        self.result_image = self.image.copy()  # 元画像をコピー
        for face in faces:
            box = list(map(int, face[:4]))
            cv2.rectangle(self.result_image, box, (0, 0, 255), 2)

        # 画像表示
        self.show_image(self.result_image)

        # 顔検出結果を保存
        self.detected_faces = faces

    def show_image(self, image):
        """
        画像を表示する．
        """
        # PIL画像にする
        pil_image = Image.fromarray(cv2.cvtColor(image, cv2.COLOR_BGR2RGB))

        self.photo_image = ImageTk.PhotoImage(image=pil_image)
        self.update()
        self.canvas.create_image(
            self.canvas.winfo_width() // 2,
            self.canvas.winfo_height() // 2,
            image=self.photo_image,
        )

    def save_results(self):
        """
        顔認識をした画像を保存
        """
        if hasattr(self, "result_image") and hasattr(self, "detected_faces"):
            # エントリーからファイル名を取得
            file_name = self.entry_widget.get()

            if file_name:
                # 画像を保存するパス
                save_path = file_name + ".png"
                cv2.imwrite(save_path, self.result_image)
                tk.messagebox.showinfo(
                    "Success", f"Detected Image saved as {save_path}"
                )

                # 顔検出結果を保存するパス
                coords_path = file_name + ".csv"
                self.write_csv(coords_path)
            else:
                tk.messagebox.showerror("Error", "Please enter a file name.")
        else:
            tk.messagebox.showerror("Error", "No detection results available.")

    def write_csv(self, coords_path):
        """
        顔認識済画像情報をcsvファイルに書き出す
        """
        with open(coords_path, "w") as f:
            for num, face in enumerate(self.detected_faces, 1):
                f.write(f"{num}人目\n")
                f.write(f"X座標 ： {face[0]}\n")
                f.write(f"Y座標 ： {face[1]}\n")
                f.write(f"幅 ： {face[2]}\n")
                f.write(f"高さ ： {face[3]}\n")
                f.write(f"右目X座標 ： {face[4]}\n")
                f.write(f"右目Y座標 ： {face[5]}\n")
                f.write(f"左目X座標 ： {face[6]}\n")
                f.write(f"左目Y座標 ： {face[7]}\n")
                f.write(f"鼻X座標 ： {face[8]}\n")
                f.write(f"鼻Y座標 ： {face[9]}\n")
                f.write(f"右口角X座標 ： {face[10]}\n")
                f.write(f"右口角Y座標 ： {face[11]}\n")
                f.write(f"左口角X座標 ： {face[12]}\n")
                f.write(f"左口角Y座標 ： {face[13]}\n")
                f.write(f"信頼度 ： {face[14]}\n")
            tk.messagebox.showinfo(
                "Success", f"Face coordinates saved as {coords_path}"
            )

    def apply_slider(self, strength, interpolation):
        """
        スライダーを用いるモザイク，ぼかし処理のための処理
        """
        # スライダーの値が0のときはバウンディングボックスだけ表示し、モザイク・ぼかし処理はしない
        if strength == 0:
            self.show_image(self.result_image)
            return  # モザイクをかけないのでここで終了

        # スライダーの値 // 15
        strength = strength // 15

        # 元画像をコピー
        image_copy = self.image.copy()

        if strength == 0:
            strength = 1

        # 顔領域に処理をかける
        for face in self.detected_faces:
            x, y, w, h = list(map(int, face[:4]))

            # 顔領域を切り出し
            face_region = image_copy[y : y + h, x : x + w]

            # 処理: 一度縮小してから拡大
            small = cv2.resize(
                face_region,
                (max(1, w // strength), max(1, h // strength)),
            )
            processed_face = cv2.resize(small, (w, h), interpolation=interpolation)

            # 処理をかけた顔領域を元の画像に適用
            image_copy[y : y + h, x : x + w] = processed_face

        # 処理後の画像を表示（バウンディングボックスは非表示）
        self.show_image(image_copy)

    def mosaic_face(self):
        """
        顔領域にモザイクをかける
        """
        if hasattr(self, "result_image") and hasattr(self, "detected_faces"):
            self.apply_slider(self.mosaic_btn.get(), Image.NEAREST)
        else:
            tk.messagebox.showerror("Error", "No faces detected for mosaic.")

    def blur_face(self):
        """
        顔領域にぼかしをかける
        """
        if hasattr(self, "result_image") and hasattr(self, "detected_faces"):
            self.apply_slider(self.blur_btn.get(), Image.BILINEAR)
        else:
            tk.messagebox.showerror("Error", "No faces detected for blur.")

    def change_face(self, original_image):
        """
        顔を選択した画像と変更する．
        """
        # 顔画像を選択
        selected_image = self.select_image_file()
        if selected_image is None:
            return

        # 選択した画像から顔検出を行う
        self.detect_faces(selected_image)
        selected_faces = self.faces

        if selected_faces is None:
            return

        # 元画像の顔領域を変更する
        original_image_copy = original_image.copy()  # 元画像をコピー
        self.detect_faces(original_image_copy)
        original_faces = self.faces

        for num in range(len(original_faces)):
            original_image_copy = self.replace_face(
                original_image_copy,
                selected_image,
                original_faces[num],
                selected_faces[0],
            )
        # 変更後の画像を別ウィンドウに表示
        self.display_face(original_image_copy)

    def select_image_file(self):
        """
        画像ファイルを選択し、読み込む．
        """
        file_name = tk.filedialog.askopenfilename(
            filetypes=[("Change Face Image Files", "*.png*.jpg*.jpeg")]
        )
        if os.path.isfile(file_name):
            selected_image = cv2.imread(file_name)
            if selected_image is None:
                tk.messagebox.showerror("Error", "Failed to load the selected image.")
                return None
            return selected_image

    def replace_face(
        self, original_image, selected_image, original_face, selected_face
    ):
        """
        元画像の顔領域を選択した画像の顔領域で置き換える．
        """
        original_image_copy = original_image.copy()

        # 元画像の顔領域
        x, y, w, h = list(map(int, original_face[:4]))

        # 選択した画像の顔領域
        selected_x, selected_y, selected_w, selected_h = list(
            map(int, selected_face[:4])
        )
        selected_face_region = selected_image[
            selected_y : selected_y + selected_h, selected_x : selected_x + selected_w
        ]

        # 縦横比を維持してリサイズ
        resized_face = self.resize_with_aspect_ratio(selected_face_region, (w, h))

        if resized_face.shape[:2] != (h, w):
            # 誤差を無視してリサイズ
            resized_face = cv2.resize(
                resized_face, (w, h), interpolation=cv2.INTER_LINEAR
            )

        # 元画像の顔領域にリサイズした顔を適用
        original_image_copy[y : y + h, x : x + w] = resized_face[:h, :w]

        return original_image_copy

    def resize_with_aspect_ratio(self, image, target_size):
        """
        縦横比を維持してリサイズし、クロッピングで余分な部分を取り除き、サイズを一致させる．
        """
        target_w, target_h = target_size
        h, w = image.shape[:2]

        # 縦横比を保ちながらリサイズ
        scale = max(
            target_w / w, target_h / h
        )  # クロッピングするので大きい方に合わせてリサイズ
        new_w, new_h = int(w * scale), int(h * scale)
        resized_image = cv2.resize(image, (new_w, new_h), interpolation=cv2.INTER_AREA)

        # 中央部分をクロッピングして指定サイズに合わせる
        x_center = new_w // 2
        y_center = new_h // 2

        x_start = x_center - target_w // 2
        y_start = y_center - target_h // 2

        # クロッピング
        cropped_image = resized_image[
            y_start : y_start + target_h, x_start : x_start + target_w
        ]

        return cropped_image

    def display_face(self, image):
        """
        別ウィンドウに変更後の顔画像を表示する．
        """
        face_window = tk.Toplevel(self.master)
        face_window.title("Changed Face Image")
        face_window.geometry("500x500")
        face_window.configure(bg="#FFCC99")

        canvas = tk.Canvas(face_window, width=500, height=500, bg="#ffffcc")
        canvas.pack()

        pil_image = Image.fromarray(cv2.cvtColor(image, cv2.COLOR_BGR2RGB))
        photo_image = ImageTk.PhotoImage(image=pil_image)

        canvas.create_image(250, 250, image=photo_image)
        canvas.photo_image = photo_image

    def apply_blown_skin(self, slider_value):
        """
        顔を一度だけ切り取り、色黒にする処理
        """
        original_image = np.array(self.image.copy(), dtype="uint8")

        # 一度だけ顔領域を切り取る
        if not hasattr(self, "mask_result"):
            bgdModel = np.zeros((1, 65), np.float64)
            fgdModel = np.zeros((1, 65), np.float64)

            # 顔領域の座標を取得
            x, y, w, h = list(map(int, self.faces[0][:4]))  # 1つ目の顔のみ処理

            rect = (x, y, w, h)
            mask = np.zeros(original_image.shape[:2], np.uint8)  # 顔用のマスク

            # GrabCutで顔領域を切り取る
            cv2.grabCut(
                original_image, mask, rect, bgdModel, fgdModel, 5, cv2.GC_INIT_WITH_RECT
            )

            # マスクから結果を作成して保存（再利用）
            self.mask_result = np.where((mask == 1) | (mask == 3), 1, 0).astype("uint8")

        # 顔領域だけに色黒処理を行う
        face_region = original_image * self.mask_result[:, :, np.newaxis]

        # HSV空間に変換して明度と彩度を調整
        hsv_image = cv2.cvtColor(face_region, cv2.COLOR_BGR2HSV)
        h, s, v = cv2.split(hsv_image)

        # スライダー値に基づいて明度と彩度を調整
        v = cv2.add(v, slider_value)  # 明度調整
        s = cv2.add(s, slider_value)  # 彩度調整

        # 調整後の顔を元画像に戻す
        adjusted_face = cv2.merge([h, s, v])
        adjusted_face_bgr = cv2.cvtColor(adjusted_face, cv2.COLOR_HSV2BGR)

        # 調整された顔領域を元画像に適用
        original_image[self.mask_result == 1] = adjusted_face_bgr[self.mask_result == 1]

        # 変更後の画像を表示
        self.display_face(original_image)


def main():
    root = tk.Tk()  # rootインスタンスを作成
    app = GuiApplication(master=root)  # GuiApplicationクラスのインスタンスを作成
    app.mainloop()  # アプリの待機，イベント処理


if __name__ == "__main__":
    main()
