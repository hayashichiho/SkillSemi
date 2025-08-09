import argparse
import cv2
from PIL import Image

def parse_arguments():
    """コマンドライン引数を解析する"""
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
        help="cropping, paddingの基準位置。0: 中央（デフォルト）, 1: 左上, 2: 左下, 3: 右下, 4: 右上。",
    )
    return parser.parse_args()

def read_image(in_file):
    """指定されたファイルから画像を読み込む"""
    img = cv2.imread(in_file, cv2.IMREAD_GRAYSCALE)
    if img is None:
        raise FileNotFoundError(f"指定された入力ファイルが見つかりません: {in_file}")
    return img

def pad_image(image, pad_height, pad_width, position):
    """画像にパディングを追加する"""
    if position == 1:  # 左上
        top, bottom = 0, pad_height
        left, right = 0, pad_width
    elif position == 2:  # 左下
        top, bottom = pad_height, 0
        left, right = 0, pad_width
    elif position == 3:  # 右下
        top, bottom = pad_height, 0
        left, right = pad_width, 0
    elif position == 4:  # 右上
        top, bottom = 0, pad_height
        left, right = pad_width, 0
    else:  # 中央
        top = pad_height // 2
        bottom = pad_height - top
        left = pad_width // 2
        right = pad_width - left

    padded_image = cv2.copyMakeBorder(
        image, top, bottom, left, right,
        borderType=cv2.BORDER_CONSTANT, value=0
    )
    return padded_image

def crop_image(image, target_size, position):
    """画像をクロップする"""
    height, width = image.shape
    if height < target_size or width < target_size:
        raise ValueError("クロッピングサイズが画像サイズより大きいです。")
    if position == 1:  # 左上
        start_y, start_x = 0, 0
    elif position == 2:  # 左下
        start_y, start_x = height - target_size, 0
    elif position == 3:  # 右下
        start_y, start_x = height - target_size, width - target_size
    elif position == 4:  # 右上
        start_y, start_x = 0, width - target_size
    else:  # 中央
        start_y = max((height - target_size) // 2, 0)
        start_x = max((width - target_size) // 2, 0)
    cropped_image = image[start_y : start_y + target_size, start_x : start_x + target_size]
    return cropped_image

def main():
    args = parse_arguments()
    img = read_image(args.in_file)
    if args.point not in [0, 1, 2, 3, 4]:
        raise ValueError("無効な基準位置が指定されました。")
    if args.out_size <= 0:
        raise ValueError("無効な出力サイズが指定されました。")

    original_height, original_width = img.shape[:2]
    target_size = args.out_size

    resized_image = img.copy()
    operation = []

    # パディング
    pad_height = max(target_size - original_height, 0)
    pad_width = max(target_size - original_width, 0)
    if pad_height > 0 or pad_width > 0:
        resized_image = pad_image(resized_image, pad_height, pad_width, args.point)
        operation.append("padding")

    # クロッピング
    resized_height, resized_width = resized_image.shape[:2]
    if resized_height > target_size or resized_width > target_size:
        resized_image = crop_image(resized_image, target_size, args.point)
        operation.append("cropping")

    # 画像保存
    output_image = Image.fromarray(resized_image)
    output_image.save(args.out_file)
    print(f"画像を保存: {args.out_file} ({' and '.join(operation) if operation else 'no operation'})")

if __name__ == "__main__":
    main()
