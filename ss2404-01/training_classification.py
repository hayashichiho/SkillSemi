import os
import torch
import torch.optim as optim
import torch.nn as nn
from torch.utils.data import DataLoader
from torchvision import transforms, datasets
from resnet18 import ResNet18
import pandas as pd
import argparse
import datetime

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('train_path', type=str, help='学習用画像フォルダ（例: input/train）')
    parser.add_argument('val_path', type=str, help='検証用画像フォルダ（例: input/val）')
    parser.add_argument('output_path', type=str, help='結果保存フォルダ（例: output/）')
    parser.add_argument('--gpu_id', '-g', type=int, default=0, help='GPU ID')
    parser.add_argument('--learning_rate', '-l', type=float, default=0.001, help='学習率')
    parser.add_argument('--batch_size', '-b', type=int, default=8, help='バッチサイズ')
    parser.add_argument('--max_epoch_num', '-m', type=int, default=50, help='最大エポック数')
    args = parser.parse_args()

    # 結果保存フォルダがなければ作成
    os.makedirs(args.output_path, exist_ok=True)

    # タイムスタンプ生成
    time_stamp = datetime.datetime.now().strftime('%Y%m%d%H%M%S')

    device = torch.device(f'cuda:{args.gpu_id}' if torch.cuda.is_available() else 'cpu')

    # データセットの準備
    transform = transforms.Compose([
        transforms.Resize((224, 224)),
        transforms.ToTensor()
    ])
    train_dataset = datasets.ImageFolder(args.train_path, transform=transform)
    val_dataset = datasets.ImageFolder(args.val_path, transform=transform)
    train_loader = DataLoader(train_dataset, batch_size=args.batch_size, shuffle=True)
    val_loader = DataLoader(val_dataset, batch_size=args.batch_size, shuffle=False)

    # 入力チャンネル数自動判定
    sample_img, _ = train_dataset[0]
    in_channels = sample_img.shape[0]
    num_classes = len(train_dataset.classes)

    # モデル・最適化・損失関数
    model = ResNet18(in_channels, num_classes).to(device)
    optimizer = optim.Adam(model.parameters(), lr=args.learning_rate)
    criterion = nn.CrossEntropyLoss()

    best_val_loss = float('inf')
    loss_log = []

    for epoch in range(args.max_epoch_num):
        # 学習
        model.train()
        train_loss = 0
        for images, labels in train_loader:
            images, labels = images.to(device), labels.to(device)
            optimizer.zero_grad()
            outputs = model(images)
            loss = criterion(outputs, labels)
            loss.backward()
            optimizer.step()
            train_loss += loss.item() * images.size(0)
        train_loss /= len(train_loader.dataset)

        # 検証
        model.eval()
        val_loss = 0
        with torch.no_grad():
            for images, labels in val_loader:
                images, labels = images.to(device), labels.to(device)
                outputs = model(images)
                loss = criterion(outputs, labels)
                val_loss += loss.item() * images.size(0)
        val_loss /= len(val_loader.dataset)

        print(f"Epoch {epoch+1}/{args.max_epoch_num} Train Loss: {train_loss:.4f} Val Loss: {val_loss:.4f}")
        loss_log.append([epoch+1, train_loss, val_loss])

        # ベストモデル保存
        if val_loss < best_val_loss:
            best_val_loss = val_loss
            torch.save(model.state_dict(), os.path.join(args.output_path, f"model_best_{time_stamp}.pth"))

    # ログ保存
    df = pd.DataFrame(loss_log, columns=['epoch', 'train_loss', 'val_loss'])
    df.to_csv(os.path.join(args.output_path, f"loss_log_{time_stamp}.csv"), index=False)

if __name__ == "__main__":
    main()
