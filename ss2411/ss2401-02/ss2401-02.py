<<<<<<< HEAD
import argparse

import cv2
import matplotlib.pyplot as plt
from PIL import Image


def parse_arguments():
    parser = argparse.ArgumentParser(
        description="グレースケール画像のサイズを変更する。出力サイズが大きければpaddingを、小さければcroppingをする。"
    )
    parser.add_argument(
        "-in", "--in_file", type=str, required=True, help="入力画像ファイル名。"
    )
    parser.add_argument(
        "-out", "--out_file", type=str, required=True, help="出力画像ファイル名。"
    )
    parser.add_argument(
        "-size",
        "--out_size",
        type=int,
        required=True,
        help="出力画像のサイズ（幅と高さが同じ正方形）。",
    )
    parser.add_argument(
        "-p",
        "--point",
        type=int,
        default=0,
        choices=[0, 1, 2, 3, 4],
        help="cropping, paddingの基準位置。0: 中央（デフォルト, 1: 左上, 2: 左下, 3: 右下, 4: 右上。",
    )
    return parser.parse_args()


def read_image(in_file):
    img = cv2.imread(in_file, cv2.IMREAD_GRAYSCALE)
    if img is None:
        raise FileNotFoundError(f"指定された入力ファイルが見つかりません: {in_file}")
    return img


def pad_image(image, pad_height, pad_width, position):
    # 基準位置に合わせて，余白のサイズを決定
    if position == 1:  # 基準位置が左上の場合
        top = 0
        bottom = pad_height
        left = 0
        right = pad_width
    elif position == 2:  # 基準位置が左下の場合
        top = pad_height
        bottom = 0
        left = 0
        right = pad_width
    elif position == 3:  # 基準位置が右下の場合
        top = pad_height
        bottom = 0
        left = pad_width
        right = 0
    elif position == 4:  # 基準位置が右上の場合
        top = 0
        bottom = pad_height
        left = pad_width
        right = 0
    else:  # 基準位置が中央の場合 または デフォルトの場合
        top = pad_height // 2
        bottom = pad_height - top
        left = pad_width // 2
        right = pad_width - left

    padded_image = cv2.copyMakeBorder(
        image,
        top,
        bottom,
        left,
        right,
        borderType=cv2.BORDER_CONSTANT,
        value=0,  # 余白の色（黒）
    )
    return padded_image


def crop_image(image, target_size, position):
    height, width = image.shape

    if position == 1:  # 基準位置が左上の場合
        start_y = 0
        start_x = 0
    elif position == 2:  # 基準位置が左下の場合
        start_y = height - target_size
        start_x = 0
    elif position == 3:  # 基準位置が右下の場合
        start_y = height - target_size
        start_x = width - target_size
    elif position == 4:  # 基準位置が右上の場合
        start_y = 0
        start_x = width - target_size
    else:  # 基準位置が中央の場合 または デフォルトの場合
        start_y = (height - target_size) // 2
        start_x = (width - target_size) // 2

    cropped_image = image[
        start_y : start_y + target_size, start_x : start_x + target_size
    ]
    return cropped_image


def main():
    args = parse_arguments()
    img = read_image(args.in_file)  # 画像読み込み

    original_height, original_width = img.shape[:2]  # 元の画像サイズ
    target_size = args.out_size  # 出力画像サイズ

    resized_image = img.copy()
    operation = "no operation"

    # paddingが必要な場合、両方の次元に対してパディングを行う
    pad_height = max(target_size - original_height, 0)
    pad_width = max(target_size - original_width, 0)

    if pad_height > 0 or pad_width > 0:
        resized_image = pad_image(resized_image, pad_height, pad_width, args.point)
        operation = "padding"

    # padding後の画像サイズ
    resized_height, resized_width = resized_image.shape[:2]

    # croppingが必要な場合、両方の次元に対してクロッピングを行う
    if resized_height > target_size or resized_width > target_size:
        resized_image = crop_image(resized_image, target_size, args.point)
        operation = (
            "cropping" if operation == "no operation" else "padding and cropping"
        )

    # 画像を保存
    output_image = Image.fromarray(resized_image)
    output_image.save(args.out_file)
    print(f"画像を保存: {args.out_file}({operation})")


if __name__ == "__main__":
    main()
=======
import argparse

import cv2
from PIL import Image


def parse_arguments():
    parser = argparse.ArgumentParser(
        description="グレースケール画像のサイズを変更する。出力サイズが大きければpaddingを、小さければcroppingをする。"
    )
    parser.add_argument(
        "-in", "--in_file", type=str, required=True, help="入力画像ファイル名。"
    )
    parser.add_argument(
        "-out", "--out_file", type=str, required=True, help="出力画像ファイル名。"
    )
    parser.add_argument(
        "-size",
        "--out_size",
        type=int,
        required=True,
        help="出力画像のサイズ（幅と高さが同じ正方形）。",
    )
    parser.add_argument(
        "-p",
        "--point",
        type=int,
        default=0,
        choices=[0, 1, 2, 3, 4],
        help="cropping, paddingの基準位置。0: 中央（デフォルト, 1: 左上, 2: 左下, 3: 右下, 4: 右上。",
    )
    return parser.parse_args()


def read_image(in_file):
    img = cv2.imread(in_file, cv2.IMREAD_GRAYSCALE)
    if img is None:
        raise FileNotFoundError(f"指定された入力ファイルが見つかりません: {in_file}")
    return img


def pad_image(image, pad_height, pad_width, position):
    # 基準位置に合わせて，余白のサイズを決定
    if position == 1:  # 基準位置が左上の場合
        top = 0
        bottom = pad_height
        left = 0
        right = pad_width
    elif position == 2:  # 基準位置が左下の場合
        top = pad_height
        bottom = 0
        left = 0
        right = pad_width
    elif position == 3:  # 基準位置が右下の場合
        top = pad_height
        bottom = 0
        left = pad_width
        right = 0
    elif position == 4:  # 基準位置が右上の場合
        top = 0
        bottom = pad_height
        left = pad_width
        right = 0
    else:  # 基準位置が中央の場合 または デフォルトの場合
        top = pad_height // 2
        bottom = pad_height - top
        left = pad_width // 2
        right = pad_width - left

    padded_image = cv2.copyMakeBorder(
        image,
        top,
        bottom,
        left,
        right,
        borderType=cv2.BORDER_CONSTANT,
        value=0,  # 余白の色（黒）
    )
    return padded_image


def crop_image(image, target_size, position):
    height, width = image.shape

    if position == 1:  # 基準位置が左上の場合
        start_y = 0
        start_x = 0
    elif position == 2:  # 基準位置が左下の場合
        start_y = height - target_size
        start_x = 0
    elif position == 3:  # 基準位置が右下の場合
        start_y = height - target_size
        start_x = width - target_size
    elif position == 4:  # 基準位置が右上の場合
        start_y = 0
        start_x = width - target_size
    else:  # 基準位置が中央の場合 または デフォルトの場合
        start_y = (height - target_size) // 2
        start_x = (width - target_size) // 2

    cropped_image = image[
        start_y : start_y + target_size, start_x : start_x + target_size
    ]
    return cropped_image


def main():
    args = parse_arguments()
    img = read_image(args.in_file)  # 画像読み込み

    original_height, original_width = img.shape[:2]  # 元の画像サイズ
    target_size = args.out_size  # 出力画像サイズ

    resized_image = img.copy()
    operation = "no operation"

    # paddingが必要な場合、両方の次元に対してパディングを行う
    pad_height = max(target_size - original_height, 0)
    pad_width = max(target_size - original_width, 0)

    if pad_height > 0 or pad_width > 0:
        resized_image = pad_image(resized_image, pad_height, pad_width, args.point)
        operation = "padding"

    # padding後の画像サイズ
    resized_height, resized_width = resized_image.shape[:2]

    # croppingが必要な場合、両方の次元に対してクロッピングを行う
    if resized_height > target_size or resized_width > target_size:
        resized_image = crop_image(resized_image, target_size, args.point)
        operation = (
            "cropping" if operation == "no operation" else "padding and cropping"
        )

    # 画像を保存
    output_image = Image.fromarray(resized_image)
    output_image.save(args.out_file)
    print(f"画像を保存: {args.out_file}({operation})")


if __name__ == "__main__":
    main()
>>>>>>> 0148407 (出力の方法を変えました．)
