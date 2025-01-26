import torch.nn as nn


class WideResidualBlock(nn.Module):
    def __init__(self, in_channels, out_channels, is_first_resblock=False):
        super(WideResidualBlock, self).__init__()
        self.is_ch_changed = in_channels != out_channels

        if self.is_ch_changed:
            if is_first_resblock:
                stride = 1
            else:
                stride = 2
            self.shortcut = nn.Conv2d(in_channels, out_channels, 1, stride=stride)
        else:
            stride = 1

        self.bn1 = nn.BatchNorm2d(in_channels)
        self.relu = nn.ReLU(inplace=True)
        self.conv1 = nn.Conv2d(
            in_channels, out_channels, kernel_size=3, padding=1, stride=stride
        )
        self.bn2 = nn.BatchNorm2d(out_channels)
        self.conv2 = nn.Conv2d(
            out_channels, out_channels, kernel_size=3, padding=1, stride=1
        )
        self.drop_out = nn.Dropout(0.5)

    def forward(self, x):
        shortcut = x
        out = self.bn1(x)
        out = self.relu(out)
        out = self.conv1(out)
        out = self.bn2(out)
        out = self.relu(out)
        out = self.drop_out(out)
        out = self.conv2(out)
        if self.is_ch_changed:
            shortcut = self.shortcut(x)

        out += shortcut
        return out


class WideResNet18(nn.Module):
    def __init__(self, in_channels=3, out_channels=10, k=4):
        super(WideResNet18, self).__init__()

        # 入力層
        self.conv1 = nn.Conv2d(in_channels, 64 * k, kernel_size=7, stride=2, padding=3)
        self.bn1 = nn.BatchNorm2d(64 * k)
        self.relu = nn.ReLU(inplace=True)
        self.maxpool = nn.MaxPool2d(kernel_size=3, stride=2, padding=1)

        # 残差ブロック層
        self.res_block1 = WideResidualBlock(64 * k, 64 * k)
        self.res_block2 = WideResidualBlock(64 * k, 64 * k)

        self.res_block3 = WideResidualBlock(64 * k, 128 * k)
        self.res_block4 = WideResidualBlock(128 * k, 128 * k)

        self.res_block5 = WideResidualBlock(128 * k, 256 * k)
        self.res_block6 = WideResidualBlock(256 * k, 256 * k)

        self.res_block7 = WideResidualBlock(256 * k, 512 * k)
        self.res_block8 = WideResidualBlock(512 * k, 512 * k)

        # 最後のプーリングと出力層
        self.avg_pool = nn.AdaptiveAvgPool2d((1, 1))
        self.fc = nn.Linear(512 * k, out_channels)
        self.drop_out_fc = nn.Dropout(0.5)

    def forward(self, x):
        out = self.conv1(x)
        out = self.bn1(out)
        out = self.relu(out)
        out = self.maxpool(out)

        # 各ブロックを順に通過
        N = 3
        out = self.res_block1(out)
        for _ in range(N):
            out = self.res_block2(out)
            out = self.drop_out_fc(out)
            out = self.res_block2(out)

        out = self.res_block3(out)
        for _ in range(N):
            out = self.res_block4(out)
            out = self.drop_out_fc(out)
            out = self.res_block4(out)

        out = self.res_block5(out)
        for _ in range(N):
            out = self.res_block6(out)
            out = self.drop_out_fc(out)
            out = self.res_block6(out)

        out = self.res_block7(out)
        for _ in range(N):
            out = self.res_block8(out)
            out = self.drop_out_fc(out)
            out = self.res_block8(out)

        # プーリングと最終出力
        out = self.avg_pool(out)
        out = out.view(out.size(0), -1)
        out = self.fc(out)
        out = self.drop_out_fc(out)

        return out
