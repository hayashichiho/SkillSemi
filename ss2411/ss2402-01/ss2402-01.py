import argparse
import os
import shutil
import sys
import tempfile
import zipfile

import numpy as np
import pydicom


def extract_zip(zip_file):
    """
    ZIPファイルを解凍し、解凍先のパスを返す
    """
    if not zipfile.is_zipfile(zip_file):
        print(f"{zip_file}は有効なZIPファイルではありません．")
        sys.exit(1)

    # ZIPファイルを解凍
    temp_dir = tempfile.mkdtemp()  # 一時ディレクトリを作成
    with zipfile.ZipFile(zip_file, "r") as zip_ref:
        zip_ref.extractall(temp_dir)
    return temp_dir


def get_instance_number(in_folder):
    """
    指定フォルダ内のDICOMファイルのリストを取得．
    各DICOMファイルからInstance Numberを取得．
    Instance Numberを昇順に並び替える．
    """

    # DICOMファイルの読み取り
    dicom_files = [
        os.path.join(in_folder, f)
        for f in os.listdir(in_folder)
        if os.path.isfile(os.path.join(in_folder, f)) and f.lower().endswith(".dcm")
    ]
    if not dicom_files:
        print("指定されたフォルダにDICOMファイルが見つかりません．")
        sys.exit(1)

    # Instance Numberの読み取り
    instances = []
    for f in dicom_files:
        try:
            # DICOMファイルを読み込む
            dcm_data = pydicom.dcmread(f)
            instance_number = int(dcm_data.InstanceNumber)
            instances.append((f, instance_number))

        except Exception as e:
            print(f"ファイル{f}の読み込みができませんでした．：{e}")
            continue
    if not instances:
        print("有効なDICOMファイルが見つかりません．")
        sys.exit(1)

    # Instance Numberでソート
    instances.sort(key=lambda x: x[1])
    sorted_files = [x[0] for x in instances]
    return sorted_files


def resolution_z(dicom1, dicom2):
    """
    2枚の連続画像を使用し，Z方向の解像度を求める．
    SliceLocationの差分（の絶対値）を求める．
    """
    try:
        slice_location1 = float(dicom1.SliceLocation)
        slice_location2 = float(dicom2.SliceLocation)
        spacing = abs(slice_location2 - slice_location1)
        return spacing
    except AttributeError:
        print(
            "SliceLocationが存在しないDICOMファイルが含まれています．デフォルトのZ方向解像度を使用します．"
        )
        return 1.0  # デフォルト値


def convert_hu(dcm, pixel_array):
    """
    モダリティがCTのとき，次式によりCT値を求める．
    （CT値）= RescaleSlope × （画素値）+ RescaleIntercept
    """
    rescale_slope = float(dcm.RescaleSlope) if "RescaleSlope" in dcm else 1.0
    rescale_intercept = (
        float(dcm.RescaleIntercept) if "RescaleIntercept" in dcm else 0.0
    )

    hu = rescale_slope * pixel_array + rescale_intercept
    return hu


def create_volume(sorted_files, wl, ww):
    """
    Instance Numberの昇順にDICOMファイルを読み，
    ボリュームデータ（3次元のNumpy array）を作成する．
    """
    slices = []
    spacing_z = None
    modality = None
    for idx, f in enumerate(sorted_files):
        try:
            ds = pydicom.dcmread(f)
            modality = ds.Modality
            img = ds.pixel_array.astype(np.float32)
            if ds.Modality == "CT":
                img = convert_hu(ds, img)
            slices.append(img)

            if idx == 0:
                spacing_x, spacing_y = map(float, ds.PixelSpacing)
            if idx == 1 and spacing_z is None:
                dicom1 = pydicom.dcmread(sorted_files[0])
                dicom2 = pydicom.dcmread(sorted_files[1])
                spacing_z = resolution_z(dicom1, dicom2)
        except Exception as e:
            print(f"ファイル{f}処理中にエラーが発生しました：{e}")
            continue
    if not slices:
        print("ボリュームデータの作成に失敗しました．")
        sys.exit(1)

    volume = np.stack(slices, axis=0)

    # spacing_zが求められなかった場合
    if spacing_z is None:
        spacing_z = 1.0  # デフォルト値

    # ボリュームデータを0-255に正規化
    if modality == "CT":
        min_val = wl - (ww / 2)
        max_val = wl + (ww / 2)
    else:
        min_val = volume.min()
        max_val = volume.max()

    # 正規化とクリッピング
    volume_normalized = (volume - min_val) / (max_val - min_val)
    volume_normalized = np.clip(volume_normalized, 0, 1)
    volume_scaled = (volume_normalized * 255).astype(np.uint8)

    # メタデータの設定
    metadata = {
        "DimSize": list(volume_scaled.shape),
        "ElementSpacing": [spacing_x, spacing_y, spacing_z],
        "ElementType": "MET_UCHAR",
    }
    return volume_scaled, metadata


def write_raw_mhd(volume, metadata, out_file):
    raw_filename = out_file + ".raw"
    mhd_filename = out_file + ".mhd"

    # rawファイルの書き出し
    volume.tofile(raw_filename)

    # mhdファイルの書き出し
    with open(mhd_filename, "w") as f:
        f.write("ObjectType = Image\n")
        f.write("NDims = 3\n")
        f.write(f"DimSize = {' '.join(map(str, metadata['DimSize']))}\n")
        f.write(f"ElementType = {metadata['ElementType']}\n")
        f.write(f"ElementSpacing = {' '.join(map(str, metadata['ElementSpacing']))}\n")
        f.write("ElementByteOrderMSB = False\n")
        f.write(f"ElementDataFile = {os.path.basename(raw_filename)}\n")
    print(f"RawデータとMHDヘッダーを {out_file}.raw と {out_file}.mhd に保存しました．")


def main():
    parser = argparse.ArgumentParser(
        description="指定したZIPファイル内またはフォルダ内のDICOMファイルを解凍・読み込み，3次元データを256階調で保存"
    )
    parser.add_argument("in_folder", type=str, help="入力フォルダ名またはZIPファイル名")
    parser.add_argument("out_file", type=str, help="出力ファイル名")
    parser.add_argument(
        "--wl", type=float, default=40.0, help="階調範囲の中心値 (Window Level)"
    )
    parser.add_argument(
        "--ww", type=float, default=400.0, help="階調範囲の幅 (Window Width)"
    )

    args = parser.parse_args()

    # 入力の検証
    if not os.path.exists(args.in_folder):
        print(f"入力フォルダまたはファイル{args.in_folder}が存在しません．")
        sys.exit(1)

    # ZIPファイルかどうかを判定
    if os.path.isfile(args.in_folder) and args.in_folder.lower().endswith(".zip"):
        temp_dir = extract_zip(args.in_folder)
        extracted = True
    elif os.path.isdir(args.in_folder):
        temp_dir = args.in_folder
        extracted = False
    else:
        print(f"入力{args.in_folder}はフォルダでもZIPファイルでもありません．")
        sys.exit(1)

    try:
        # Instance Numberでソート
        sorted_files = get_instance_number(temp_dir)

        # ボリュームデータの作成
        volume, metadata = create_volume(sorted_files, args.wl, args.ww)

        # RawとMHDの書き出し
        write_raw_mhd(volume, metadata, args.out_file)

    finally:
        # ZIPファイルを解凍した場合は一時ディレクトリを削除
        if "extracted" in locals() and extracted:
            shutil.rmtree(temp_dir)


if __name__ == "__main__":
    main()
