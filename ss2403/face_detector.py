import os
import cv2
import tkinter as tk

class FaceDetector:
    def __init__(self):
        current_directory = os.path.abspath(os.path.dirname(__file__))
        weights_path = os.path.join(current_directory, "face_detection_yunet_2023mar.onnx")
        if not os.path.exists(weights_path):
            tk.messagebox.showerror("Error", f"Model file not found: {weights_path}")
            exit()
        self.detector = cv2.FaceDetectorYN_create(weights_path, "", (0, 0))

    def detect(self, image):
        height, width = image.shape[:2]
        self.detector.setInputSize((width, height))
        _, faces = self.detector.detect(image)
        return faces
