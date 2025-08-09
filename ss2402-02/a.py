import argparse
import SimpleITK as sitk
import numpy as np
import matplotlib.pyplot as plt

def main():
    parser = argparse.ArgumentParser(description="NIfTI画像の画素値ヒストグラム表示")
    parser.add_argument("input_nifti", help="入力NIfTIファイル（CT/MR画像）")
    args = parser.parse_args()

    # NIfTI画像読み込み
    img = sitk.ReadImage(args.input_nifti)
    arr = sitk.GetArrayFromImage(img)  # shape: (z, y, x)

    # ヒストグラム表示
    plt.figure(figsize=(8, 5))
    plt.hist(arr.flatten(), bins=100, color='blue', alpha=0.7)
    plt.xlabel("Pixel Value")
    plt.ylabel("Frequency")
    plt.title("Histogram of NIfTI Image")
    plt.grid(True)
    plt.show()

    # 画素値の統計情報表示
    print(f"Mean: {np.mean(arr):.2f}")
    print(f"Median: {np.median(arr):.2f}")
    print(f"Std Dev: {np.std(arr):.2f}")

if __name__ == "__main__":
    main()
