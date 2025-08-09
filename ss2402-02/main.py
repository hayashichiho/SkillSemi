import argparse
import numpy as np
import SimpleITK as sitk
from scipy import ndimage
import matplotlib.pyplot as plt

def auto_threshold(slice_img):
    """二値化処理の実施"""
    thresh = -700
    return (slice_img > thresh).astype(np.uint8)

def process_slice(slice_img):
    """各断面画像の体幹抽出処理"""
    # 1. 閾値処理
    bin_img = auto_threshold(slice_img)

    # 2. モルフォロジー処理（開閉＋穴埋め）
    structure = np.ones((5, 5), dtype=np.uint8)
    bin_img = ndimage.binary_opening(bin_img, structure=structure, iterations=2)
    bin_img = ndimage.binary_closing(bin_img, structure=structure, iterations=2)
    bin_img = ndimage.binary_fill_holes(bin_img)

    # 3. ラベリング（8近傍）
    labeled, num = ndimage.label(bin_img, structure=np.ones((3, 3), dtype=np.uint8))
    if num == 0:
        return np.zeros_like(slice_img, dtype=np.uint8)
    areas = ndimage.sum(bin_img, labeled, range(1, num + 1))
    max_label = np.argmax(areas) + 1
    mask = (labeled == max_label)
    # 最大領域のみ白（1）、それ以外は黒（0）
    print("mask数:", np.sum(mask))
    return mask.astype(np.uint8)

def extract_trunk(volume):
    """各スライスごとに2Dラベリング最大領域のみ抽出"""
    trunk_mask = np.zeros_like(volume, dtype=np.uint8)
    for z in range(volume.shape[0]):
        trunk_mask[z] = process_slice(volume[z])
    return trunk_mask

def main():
    parser = argparse.ArgumentParser(description="CT/MR画像から体幹領域抽出（NIfTI形式）")
    parser.add_argument("input_nifti", help="入力NIfTIファイル（CT/MR画像）")
    parser.add_argument("output_nifti", help="出力NIfTIファイル（体幹マスク）")
    args = parser.parse_args()

    # NIfTI画像読み込み
    img = sitk.ReadImage(args.input_nifti)
    arr = sitk.GetArrayFromImage(img)  # shape: (z, y, x)

    # 体幹抽出（各断面で最大領域のみ白）
    trunk_mask = extract_trunk(arr)
    print(f"体幹領域画素数: {np.sum(trunk_mask)}")

    # NIfTI画像として保存（np.uint8, 体幹=255, その他=0）
    mask_img = sitk.GetImageFromArray((trunk_mask * 255).astype(np.uint8))
    mask_img.CopyInformation(img)
    sitk.WriteImage(mask_img, args.output_nifti)
    print(f"Saved: {args.output_nifti}")

if __name__ == "__main__":
    main()
