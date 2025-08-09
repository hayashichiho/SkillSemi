import argparse
import os
import shutil
import sys
import tempfile
import zipfile

import numpy as np
import pydicom

def extract_zip(zip_file):
    """ZIPファイルを解凍し、一時ディレクトリのパスを返す"""
    if not zipfile.is_zipfile(zip_file):
        raise ValueError(f"{zip_file}は有効なZIPファイルではありません。")
    temp_dir = tempfile.mkdtemp()
    with zipfile.ZipFile(zip_file, "r") as zip_ref:
        zip_ref.extractall(temp_dir)
    return temp_dir

def get_instance_number(in_folder):
    """フォルダ内のDICOMファイルをInstance Number順に並べて返す"""
    dicom_files = []
    for root, _, files in os.walk(in_folder):
        for f in files:
            if f.lower().endswith(".dcm"):
                dicom_files.append(os.path.join(root, f))
    if not dicom_files:
        raise FileNotFoundError("指定フォルダにDICOMファイルが見つかりません。")
    instances = []
    for f in dicom_files:
        try:
            dcm_data = pydicom.dcmread(f)
            instance_number = int(getattr(dcm_data, "InstanceNumber", -1))
            if instance_number == -1:
                continue
            instances.append((f, instance_number))
        except Exception as e:
            print(f"ファイル{f}の読み込み失敗: {e}", file=sys.stderr)
    if not instances:
        raise ValueError("有効なDICOMファイルが見つかりません。")
    instances.sort(key=lambda x: x[1])
    return [x[0] for x in instances]

def resolution_z(dicom1, dicom2):
    """2枚のDICOM画像からZ方向解像度を算出"""
    try:
        slice_location1 = float(getattr(dicom1, "SliceLocation", 0.0))
        slice_location2 = float(getattr(dicom2, "SliceLocation", 0.0))
        spacing = abs(slice_location2 - slice_location1)
        return spacing if spacing > 0 else 1.0
    except Exception:
        print("SliceLocation取得失敗。デフォルト値1.0を使用。", file=sys.stderr)
        return 1.0

def convert_hu(dcm, pixel_array):
    """CT画像の場合、画素値をHU値に変換"""
    rescale_slope = float(getattr(dcm, "RescaleSlope", 1.0))
    rescale_intercept = float(getattr(dcm, "RescaleIntercept", 0.0))
    hu = rescale_slope * pixel_array + rescale_intercept
    return hu

def create_volume(sorted_files, wl, ww):
    """DICOMファイル群から3次元ボリュームデータを生成"""
    slices = []
    spacing_z = None
    modality = None
    spacing_x = spacing_y = None
    for idx, f in enumerate(sorted_files):
        try:
            ds = pydicom.dcmread(f)
            modality = getattr(ds, "Modality", None)
            img = ds.pixel_array.astype(np.float32)
            if modality == "CT":
                img = convert_hu(ds, img)
            slices.append(img)
            if idx == 0:
                spacing = getattr(ds, "PixelSpacing", [1.0, 1.0])
                spacing_x, spacing_y = map(float, spacing)
            if idx == 1 and spacing_z is None:
                dicom1 = pydicom.dcmread(sorted_files[0])
                dicom2 = pydicom.dcmread(sorted_files[1])
                spacing_z = resolution_z(dicom1, dicom2)
        except Exception as e:
            print(f"ファイル{f}処理中エラー: {e}", file=sys.stderr)
    if not slices:
        raise RuntimeError("ボリュームデータの作成に失敗しました。")
    volume = np.stack(slices, axis=0)
    if spacing_z is None:
        spacing_z = 1.0
    if modality == "CT":
        min_val = wl - (ww / 2)
        max_val = wl + (ww / 2)
    else:
        min_val = volume.min()
        max_val = volume.max()
    volume_normalized = (volume - min_val) / (max_val - min_val + 1e-8)
    volume_normalized = np.clip(volume_normalized, 0, 1)
    volume_scaled = (volume_normalized * 255).astype(np.uint8)
    metadata = {
        "DimSize": list(volume_scaled.shape),
        "ElementSpacing": [spacing_x, spacing_y, spacing_z],
        "ElementType": "MET_UCHAR",
    }
    return volume_scaled, metadata

def write_raw_mhd(volume, metadata, out_file):
    """ボリュームデータをMHD/RAW形式で保存"""
    raw_filename = out_file + ".raw"
    mhd_filename = out_file + ".mhd"
    volume.tofile(raw_filename)
    with open(mhd_filename, "w") as f:
        f.write("ObjectType = Image\n")
        f.write("NDims = 3\n")
        f.write(f"DimSize = {' '.join(map(str, metadata['DimSize']))}\n")
        f.write(f"ElementType = {metadata['ElementType']}\n")
        f.write(f"ElementSpacing = {' '.join(map(str, metadata['ElementSpacing']))}\n")
        f.write("ElementByteOrderMSB = False\n")
        f.write(f"ElementDataFile = {os.path.basename(raw_filename)}\n")
    print(f"RawデータとMHDヘッダーを {raw_filename} と {mhd_filename} に保存しました。")

def main():
    parser = argparse.ArgumentParser(
        description="DICOMフォルダまたはZIPから3次元ボリュームデータをMHD/RAW形式で保存"
    )
    parser.add_argument("in_folder", type=str, help="入力フォルダ名またはZIPファイル名")
    parser.add_argument("out_file", type=str, help="出力ファイル名（拡張子不要）")
    parser.add_argument("--wl", type=float, default=40.0, help="ウィンドウレベル（CT画像のみ）")
    parser.add_argument("--ww", type=float, default=400.0, help="ウィンドウ幅（CT画像のみ）")
    args = parser.parse_args()

    if not os.path.exists(args.in_folder):
        print(f"入力フォルダまたはファイル{args.in_folder}が存在しません。", file=sys.stderr)
        sys.exit(1)

    temp_dir = None
    extracted = False
    try:
        if os.path.isfile(args.in_folder) and args.in_folder.lower().endswith(".zip"):
            temp_dir = extract_zip(args.in_folder)
            extracted = True
        elif os.path.isdir(args.in_folder):
            temp_dir = args.in_folder
        else:
            print(f"入力{args.in_folder}はフォルダでもZIPファイルでもありません。", file=sys.stderr)
            sys.exit(1)

        sorted_files = get_instance_number(temp_dir)
        volume, metadata = create_volume(sorted_files, args.wl, args.ww)
        write_raw_mhd(volume, metadata, args.out_file)
    except Exception as e:
        print(f"エラー: {e}", file=sys.stderr)
        sys.exit(1)
    finally:
        if extracted and temp_dir and os.path.isdir(temp_dir):
            shutil.rmtree(temp_dir)

if __name__ == "__main__":
    main()
