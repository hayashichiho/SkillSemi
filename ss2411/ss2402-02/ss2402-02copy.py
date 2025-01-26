import argparse
import os

import numpy as np
from scipy import ndimage

AREA_THRESHOLD = 80  # ラベリングの閾値を小さく設定


class RAWFileProcess:
    def __init__(self, img_file, mhd_file, output_file_name):
        self.output_file_name = output_file_name
        self.get_info_from_mhd(mhd_file)  # mhdファイルから画素数などを取得
        self.load_raw_file(img_file)  # RAWファイルの読み込み

        print(f"Initial data shape: {self.img.shape}")
        print(f"Initial data range: {self.img.min()} to {self.img.max()}")

        print("全ピクセル数：", self.img.size)

        self.ImageProcess()  # 画像を処理する関数

        # データ型を変換して保存
        self.ElementType = "MET_UCHAR"
        self.ElementDataFile = self.output_file_name + ".raw"
        self.save_to_file()

    def get_info_from_mhd(self, file_name):
        """mhdファイルから情報を取得"""
        with open(file_name) as f:
            lines = [line.strip() for line in f.readlines()]

        def get_value(key):
            for line in lines:
                if line.startswith(key):
                    return line.split("=")[-1].strip()

        self.ObjectType = get_value("ObjectType")
        self.NDims = int(get_value("NDims"))
        self.DimSize = list(map(int, get_value("DimSize").split()))
        self.ElementType = get_value("ElementType")
        self.ElementSpacing = list(map(float, get_value("ElementSpacing").split()))
        self.ElementDataFile = get_value("ElementDataFile")

    def load_raw_file(self, img_file):
        """RAWファイルを読み込む"""
        width, height, slices = self.DimSize
        with open(img_file, "rb") as file:
            data = np.fromfile(file, dtype=np.int16)
            self.img = data.reshape((slices, height, width))

    def ImageProcess(self):
        """3D画像の処理"""
        # まず高さ方向の切り取り
        target_height = 512
        self.img = self.img[:, :target_height, :]
        print(f"After cropping data shape: {self.img.shape}")

        # CT値の範囲で二値化（骨や軟部組織を抽出）
        threshold_min = 50  # 軟部組織のCT値下限
        threshold_max = 1500  # 骨のCT値上限
        binary_img = ((self.img > threshold_min) & (self.img < threshold_max)).astype(
            np.uint8
        )

        # 3Dでのモルフォロジー処理
        kernel = np.ones((3, 3, 3))  # 3D構造要素
        binary_img = ndimage.binary_closing(binary_img, structure=kernel, iterations=2)
        binary_img = ndimage.binary_fill_holes(binary_img)

        # 3Dラベリング処理
        labeled_array, num_features = ndimage.label(
            binary_img, structure=np.ones((3, 3, 3))
        )

        # 体積（3D領域の大きさ）でフィルタリング
        volumes = np.bincount(labeled_array.ravel())[1:]
        volume_threshold = AREA_THRESHOLD * target_height  # 3D用に閾値を調整
        mask = np.isin(labeled_array, np.where(volumes >= volume_threshold)[0] + 1)

        self.img = mask.astype(np.uint8) * 255
        print(f"Processed data shape: {self.img.shape}")

    def process_threshold(self):
        """閾値を設定して2値画像を生成"""
        threshold_min = 50  # CT値の下限
        threshold_max = 700000000000  # CT値の上限
        return ((self.img > threshold_min) & (self.img < threshold_max)).astype(
            np.uint8
        )

    def process_morphology(self, img):
        """形態学的処理 (開閉処理)"""
        kernel_opening = np.ones((2, 2))  # 小さめのカーネルを設定
        img_dst = ndimage.binary_opening(img, structure=kernel_opening, iterations=1)
        kernel_closing = np.ones((3, 3))  # 閉処理用カーネル
        img_dst = ndimage.binary_closing(
            img_dst, structure=kernel_closing, iterations=1
        )
        img_dst = ndimage.binary_fill_holes(img_dst)
        return img_dst

    def process_labeling(self, img, neighborhood=8):
        """ラベリング処理で大きな領域を抽出"""
        struct_element = (
            np.ones((3, 3)) if neighborhood == 8 else [[0, 1, 0], [1, 1, 1], [0, 1, 0]]
        )
        label, number_label = ndimage.label(img, structure=struct_element)
        area = ndimage.sum(img, label, range(number_label + 1))
        print(f"Areas of labels: {area}")
        mask_area = area > AREA_THRESHOLD
        return mask_area[label]

    def save_to_file(self):
        """RAWとMHDファイルを保存"""
        if self.img.max() > 1:
            scaled_img = (
                (self.img - self.img.min()) / (self.img.max() - self.img.min()) * 255
            ).astype(np.uint8)
        else:
            scaled_img = (self.img * 255).astype(np.uint8)

        # RAWファイルを保存
        raw_file_path = self.output_file_name + ".raw"
        scaled_img.tofile(raw_file_path)

        # MHDファイルを保存
        mhd_file_path = self.output_file_name + ".mhd"
        with open(mhd_file_path, "w") as f:
            f.write(f"ObjectType = {self.ObjectType}\n")
            f.write(f"NDims = {self.NDims}\n")
            f.write(f"DimSize = {' '.join(map(str, self.DimSize))}\n")
            f.write("ElementType = MET_UCHAR\n")
            f.write(f"ElementSpacing = {' '.join(map(str, self.ElementSpacing))}\n")
            f.write("ElementByteOrderMSB = False\n")
            f.write(f"ElementDataFile = {os.path.basename(raw_file_path)}\n")

        print(f"Files saved: {raw_file_path}, {mhd_file_path}")


def main():
    parser = argparse.ArgumentParser(description="Process RAW and MHD files")
    parser.add_argument("img_file", help="Path to input RAW file")
    parser.add_argument("mhd_file", help="Path to input MHD file")
    parser.add_argument("output_file_name", help="Base name for output files")
    args = parser.parse_args()

    RAWFileProcess(args.img_file, args.mhd_file, args.output_file_name)


if __name__ == "__main__":
    main()
