import os
import sys
import torch
from torch.utils.data import DataLoader
from torchvision import transforms, datasets
from resnet18 import ResNet18
import pandas as pd
import argparse
import datetime
from roc import plot_roc

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('test_path', type=str, help='テスト用データのパス')
    parser.add_argument('model_path', type=str, help='モデルファイル名')
    parser.add_argument('output_path', type=str, help='出力パス')
    parser.add_argument('--gpu_id', '-g', type=int, default=0, help='GPU ID')
    parser.add_argument('--time_stamp', type=str, default=None, help='タイムスタンプ')
    args = parser.parse_args()

    if args.time_stamp is None:
        args.time_stamp = datetime.datetime.now().strftime('%Y%m%d%H%M%S')

    device = torch.device(f'cuda:{args.gpu_id}' if torch.cuda.is_available() else 'cpu')

    transform = transforms.Compose([
        transforms.Resize((224, 224)),
        transforms.ToTensor()
    ])
    test_dataset = datasets.ImageFolder(args.test_path, transform=transform)
    test_loader = DataLoader(test_dataset, batch_size=1, shuffle=False)

    sample_img, _ = test_dataset[0]
    in_channels = sample_img.shape[0]
    num_classes = len(test_dataset.classes)

    model = ResNet18(in_channels, num_classes).to(device)
    model.load_state_dict(torch.load(args.model_path, map_location=device))
    model.eval()

    results = []
    y_true = []
    y_score = []

    with torch.no_grad():
        for idx, (images, labels) in enumerate(test_loader):
            images, labels = images.to(device), labels.to(device)
            outputs = model(images)
            probs = torch.softmax(outputs, dim=1)
            pred = torch.argmax(probs, dim=1)
            results.append([idx, int(pred.item()), int(labels.item())])
            y_true.append(int(labels.item()))
            y_score.append(float(probs[0, 1].item()) if probs.shape[1] > 1 else float(probs[0, 0].item()))

    df = pd.DataFrame(results, columns=['index', 'pred', 'true'])
    df.to_csv(os.path.join(args.output_path, f"test_result_{args.time_stamp}.csv"), index=False)

    # ROC曲線保存
    roc_path = os.path.join(args.output_path, f"test_roc_{args.time_stamp}.png")
    plot_roc(y_true, y_score, save_path=roc_path)

if __name__ == "__main__":
    main()
