import cv2
import numpy as np
from PIL import Image

class FaceProcessing:
    @staticmethod
    def draw_boxes(image, faces):
        """顔のバウンディングボックスを描画する"""
        result = image.copy()
        for face in faces:
            box = list(map(int, face[:4]))
            cv2.rectangle(result, box, (0, 0, 255), 2)
        return result

    @staticmethod
    def mosaic(image, faces, strength):
        """顔にモザイク処理を適用する"""
        strength = max(strength // 15, 1)
        result = image.copy()
        for face in faces:
            x, y, w, h = list(map(int, face[:4]))
            region = result[y:y+h, x:x+w]
            small = cv2.resize(region, (max(1, w//strength), max(1, h//strength)))
            mosaic_face = cv2.resize(small, (w, h), interpolation=Image.NEAREST)
            result[y:y+h, x:x+w] = mosaic_face
        return result

    @staticmethod
    def blur(image, faces, strength):
        """顔にぼかし処理を適用する"""
        strength = max(strength // 15, 1)
        result = image.copy()
        for face in faces:
            x, y, w, h = list(map(int, face[:4]))
            region = result[y:y+h, x:x+w]
            small = cv2.resize(region, (max(1, w//strength), max(1, h//strength)))
            blur_face = cv2.resize(small, (w, h), interpolation=Image.BILINEAR)
            result[y:y+h, x:x+w] = blur_face
        return result

    @staticmethod
    def change_face(image, faces, selected_image=None, selected_faces=None):
        """
        画像の顔領域を、選択した画像の顔領域で置き換える
        """
        if selected_image is None or selected_faces is None:
            return image  # 入れ替え画像がなければ何もしない

        result = image.copy()

        # 元画像の各顔領域に、選択画像の1人目の顔を貼り付け
        sel_face = selected_faces[0]
        sx, sy, sw, sh = list(map(int, sel_face[:4]))
        selected_face_region = selected_image[sy:sy+sh, sx:sx+sw]

        for orig_face in faces:
            x, y, w, h = list(map(int, orig_face[:4]))
            # 選択画像の顔領域を元画像の顔サイズにリサイズ
            resized_face = cv2.resize(selected_face_region, (w, h), interpolation=cv2.INTER_LINEAR)
            # 元画像の顔領域に貼り付け
            result[y:y+h, x:x+w] = resized_face

        return result

    @staticmethod
    def blown_skin(image, faces, strength):
        """色黒にする処理を適用する"""
        result = image.copy()
        face = faces[0]  # 1人目の顔
        bgdModel = np.zeros((1, 65), np.float64)
        fgdModel = np.zeros((1, 65), np.float64)
        x, y, w, h = list(map(int, face[:4]))
        rect = (x, y, w, h)
        mask = np.zeros(result.shape[:2], np.uint8)
        cv2.grabCut(result, mask, rect, bgdModel, fgdModel, 5, cv2.GC_INIT_WITH_RECT)
        mask_result = np.where((mask == 1) | (mask == 3), 1, 0).astype("uint8")
        face_region = result * mask_result[:, :, np.newaxis]
        hsv_image = cv2.cvtColor(face_region, cv2.COLOR_BGR2HSV)
        h, s, v = cv2.split(hsv_image)
        v = cv2.add(v, strength)
        s = cv2.add(s, strength)
        adjusted_face = cv2.merge([h, s, v])
        adjusted_face_bgr = cv2.cvtColor(adjusted_face, cv2.COLOR_HSV2BGR)
        result[mask_result == 1] = adjusted_face_bgr[mask_result == 1]
        return result
