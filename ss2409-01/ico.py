from PIL import Image


def convert_jpg_to_ico(jpg_file_path, ico_file_path):
    # 画像を読み込む
    img = Image.open(jpg_file_path)

    # 画像をアイコン形式に変換して保存
    img.save(ico_file_path, format="ICO", sizes=[(32, 32)])


# 使用例
if __name__ == "__main__":
    jpg_file_path = "/home/hayashi/SkillSemi2024/ss2409-01/list-pochacco.png"
    ico_file_path = "/home/hayashi/SkillSemi2024/ss2409-01/OIP_result.ico"
    convert_jpg_to_ico(jpg_file_path, ico_file_path)
    print("変換が完了しました。")
