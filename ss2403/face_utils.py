import os
import cv2
import tkinter as tk
from PIL import Image, ImageTk
from face_detector import FaceDetector
from face_processing import FaceProcessing
import os

class FaceUtils:
    def __init__(self, gui_app):
        self.gui_app = gui_app
        self.detector = FaceDetector()
        self.faces = None
        self.image = None
        self.result_image = None

    def open_image_file(self, file_name):
        """画像ファイルを開いて顔検出を行う"""
        cv2_image = cv2.imread(file_name)
        if cv2_image is None:
            tk.messagebox.showerror("Error", "Failed to load the image.")
            return
        self.image = cv2_image
        self.faces = self.detector.detect(self.image)
        if self.faces is None:
            tk.messagebox.showerror("Error", "Can't detect the faces.")
            return

    def apply_face_processing(self, func, *args, **kwargs):
        """顔画像処理の共通メソッド"""
        if self.faces is None:
            tk.messagebox.showerror("Error", "No faces detected.")
            return
        self.result_image = func(self.image, self.faces, *args, **kwargs)
        self.gui_app.show_image(self.result_image)

    def dounding_box(self):
        """顔のバウンディングボックスを描画する"""
        self.apply_face_processing(FaceProcessing.draw_boxes)

    def mosaic_face(self, strength):
        """顔にモザイク処理を適用する"""
        self.apply_face_processing(FaceProcessing.mosaic, strength)

    def blur_face(self, strength):
        """顔にぼかし処理を適用する"""
        self.apply_face_processing(FaceProcessing.blur, strength)

    def change_face(self):
        """顔を別の画像に置き換える"""
        # もう一枚画像ファイルを選択
        file_name = tk.filedialog.askopenfilename(
            filetypes=[
                ("Image Files", "*.png"),
                ("Image Files", "*.jpg"),
                ("Image Files", "*.jpeg"),
                ("All Files", "*.*")
            ]
        )
        if not file_name or not os.path.isfile(file_name):
            return
        selected_image = cv2.imread(file_name)
        if selected_image is None:
            tk.messagebox.showerror("Error", "画像の読み込みに失敗しました")
            return
        selected_faces = self.detector.detect(selected_image)
        if selected_faces is None:
            tk.messagebox.showerror("Error", "選択した画像から顔を検出できませんでした")
            return
        # 顔交換処理
        self.result_image = FaceProcessing.change_face(self.image, self.faces, selected_image, selected_faces)
        self.gui_app.show_image(self.result_image)

    def blown_skin(self, strength):
        """色黒にする処理を適用する"""
        self.apply_face_processing(FaceProcessing.blown_skin, strength)

    def save_results(self, file_name):
        """処理結果を保存する"""
        result_dir = "results"
        os.makedirs(result_dir, exist_ok=True)
        if self.result_image is not None:
            cv2.imwrite(os.path.join(result_dir, file_name + ".png"), self.result_image)
            tk.messagebox.showinfo("Success", f"Detected Image saved as {file_name}.png")
            self.write_csv(os.path.join(result_dir, file_name + ".csv"))
        else:
            tk.messagebox.showerror("Error", "No detection results available.")

    def write_csv(self, coords_path):
        """顔の座標をCSVファイルに書き込む"""
        if self.faces is None:
            return
        with open(coords_path, "w") as f:
            for num, face in enumerate(self.faces, 1):
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
            tk.messagebox.showinfo("Success", f"Face coordinates saved as {coords_path}")
