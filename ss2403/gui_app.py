import tkinter as tk
import tkinter.messagebox
import tkinter.filedialog

from PIL import Image, ImageTk
import cv2
import os
from face_utils import FaceUtils


class GuiApplication(tk.Frame):
    def __init__(self, master=None):
        super().__init__(master)
        self.master.title("Face Detection Application")
        self.master.geometry("500x930")
        self.master.configure(bg="#ffffcc")
        self.face_utils = FaceUtils(self)
        self.create_widgets()
        self.master.bind("<Configure>", self.on_resize)

    def create_widgets(self):
        """ウィジェットを作成する関数"""
        self.canvas = tk.Canvas(self.master, bg="#ffe4e1", width=460, height=460)
        self.canvas.place(relx=0.5, rely=0.0, anchor="n")  # 上中央

        # 画像選択ボタン
        self.select_btn = tk.Button(self.master,
            text="Select Image", command=self.select_file, width=10, height=1, font=("Arial", 9), bg="#a9a9a9"
        )
        self.select_btn.place(relx=0.3, y=490, anchor="center")

        # 保存ボタン（画像選択ボタンの横に配置）
        self.save_btn = tk.Button(self.master,
            text="Save Results", command=self.save_results, width=10, height=1, font=("Arial", 9), bg="#a9a9a9"
        )
        self.save_btn.place(relx=0.7, y=490, anchor="center")

        window_width = self.master.winfo_width()
        self.slider_length = max(120, int(window_width * 0.5))  # 少し短め

        y_base = 540
        y_gap = 120

        # モザイク処理
        self.mosaic_label = tk.Label(self.master, bg="#ffffcc", text="Mosaic", font=("Arial", 9))
        self.mosaic_label.place(relx=0.5, y=y_base, anchor="center")
        self.mosaic_btn = tk.Scale(self.master, from_=0, to=100, orient=tk.HORIZONTAL, length=self.slider_length)
        self.mosaic_btn.place(relx=0.5, y=y_base+30, anchor="center")
        self.mosaic_apply_btn = tk.Button(self.master,
            text="Apply", command=self.apply_mosaic, width=7, height=1, font=("Arial", 9), bg="#a9a9a9"
        )
        self.mosaic_apply_btn.place(relx=0.5, y=y_base+70, anchor="center")

        # ぼかし処理
        self.blur_label = tk.Label(self.master, bg="#ffffcc", text="Blur", font=("Arial", 9))
        self.blur_label.place(relx=0.5, y=y_base+y_gap, anchor="center")
        self.blur_btn = tk.Scale(self.master, from_=0, to=100, orient=tk.HORIZONTAL, length=self.slider_length)
        self.blur_btn.place(relx=0.5, y=y_base+y_gap+30, anchor="center")
        self.blur_apply_btn = tk.Button(self.master,
            text="Apply", command=self.apply_blur, width=7, height=1, font=("Arial", 9), bg="#a9a9a9"
        )
        self.blur_apply_btn.place(relx=0.5, y=y_base+y_gap+70, anchor="center")

        # 色黒処理
        self.blown_skin_label = tk.Label(self.master, bg="#ffffcc", text="Blown Skin", font=("Arial", 9))
        self.blown_skin_label.place(relx=0.5, y=y_base+2*y_gap, anchor="center")
        self.blown_skin_btn = tk.Scale(self.master, from_=0, to=100, orient=tk.HORIZONTAL, length=self.slider_length)
        self.blown_skin_btn.place(relx=0.5, y=y_base+2*y_gap+30, anchor="center")
        self.blown_skin_apply_btn = tk.Button(self.master,
            text="Apply", command=self.apply_blown_skin, width=7, height=1, font=("Arial", 9), bg="#a9a9a9"
        )
        self.blown_skin_apply_btn.place(relx=0.5, y=y_base+2*y_gap+70, anchor="center")

        # 顔入れ替えボタン
        self.change_btn = tk.Button(self.master,
            text="Change Face", command=self.face_utils.change_face, width=10, height=1, font=("Arial", 9), bg="#a9a9a9"
        )
        self.change_btn.place(relx=0.5, y=y_base+3*y_gap, anchor="center")

    def on_resize(self, event):
        """ウィンドウサイズ変更時にスライダー長さを更新"""
        new_length = max(150, int(self.master.winfo_width() * 0.6))
        self.mosaic_btn.config(length=new_length)
        self.blur_btn.config(length=new_length)
        self.blown_skin_btn.config(length=new_length)

    def select_file(self):
        """画像ファイルを選択する"""
        file_name = tk.filedialog.askopenfilename(
            filetypes=[
                ("Image Files", "*.png"),
                ("Image Files", "*.jpg"),
                ("Image Files", "*.jpeg"),
                ("All Files", "*.*")
            ]
        )
        # ファイルが選択されなかった場合は何もしないで戻る
        if not file_name or not os.path.isfile(file_name):
            return
        self.face_utils.open_image_file(file_name)
        if self.face_utils.faces is not None:
            self.face_utils.dounding_box()

    def show_image(self, image):
        """画像をキャンバスに表示する"""
        if len(image.shape) == 3 and image.shape[2] == 3:
            # OpenCVのBGR形式をRGB形式に変換
            image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
        pil_image = Image.fromarray(image)

        # キャンバスサイズ取得
        canvas_width = self.canvas.winfo_width()
        canvas_height = self.canvas.winfo_height()

        # 画像サイズ取得
        image_width, image_height = pil_image.size

        # キャンバスに収まるように比率を保ってリサイズ
        scale = min(canvas_width / image_width, canvas_height / image_height)
        new_width = int(image_width * scale)
        new_height = int(image_height * scale)
        pil_image = pil_image.resize((new_width, new_height), Image.LANCZOS)

        # キャンバス中央に画像を配置
        self.photo_image = ImageTk.PhotoImage(image=pil_image)
        self.canvas.create_image(
            canvas_width // 2,
            canvas_height // 2,
            image=self.photo_image,
            anchor="center"
        )

    def save_results(self):
        """検出結果を保存する"""
        file_name = tk.simpledialog.askstring("Save As", "保存先のファイル名を入力してください:")
        if file_name:
            self.face_utils.save_results(file_name)
        else:
            tk.messagebox.showerror("Error", "ファイル名を入力してください")

    def apply_mosaic(self):
        """モザイク処理を適用する"""
        strength = self.mosaic_btn.get()
        self.face_utils.mosaic_face(strength)

    def apply_blur(self):
        """ぼかし処理を適用する"""
        strength = self.blur_btn.get()
        self.face_utils.blur_face(strength)

    def apply_blown_skin(self):
        """色黒処理を適用する"""
        strength = self.blown_skin_btn.get()
        self.face_utils.blown_skin(strength)
