"""
Training of 2-class classification model
Created on Fri Oct 14 2022
@author: ynomura
"""

import argparse
import os
import random as rn
import sys
from datetime import datetime as dt

import numpy as np
import roc
import torch
import torch.nn as nn
import torch.optim as optim
from resnet50 import ResNet50
from torchvision import datasets, transforms


def training(
    training_data_path,
    validation_data_path,
    output_path,
    learning_rate=0.0001,
    beta_1=0.99,
    batch_size=16,
    max_epoch_num=50,
    gpu_id="0",
    time_stamp="",
):
    # Fix seed
    seed_num = 234567
    os.environ["PYTHONHASHSEED"] = "0"
    rn.seed(seed_num)
    np.random.seed(seed_num)
    torch.manual_seed(seed_num)
    torch.backends.cudnn.deterministic = True

    if not os.path.isdir(training_data_path):
        print(f"Error: {training_data_path} is not found.")
        sys.exit(1)

    if not os.path.isdir(validation_data_path):
        print(f"Error: {validation_data_path} is not found.")
        sys.exit(1)

    # Automatic creation of output folder
    if not os.path.isdir(output_path):
        print(f"Path of output data ({output_path}) is created automatically.")
        os.makedirs(output_path)

    # Set ID of CUDA device
    device = f"cuda:{gpu_id}"
    print(f"Device: {device}")

    if time_stamp == "":
        time_stamp = dt.now().strftime("%Y%m%d%H%M%S")

    loss_log_file_name = f"{output_path}/loss_log_{time_stamp}.csv"
    model_file_name = f"{output_path}/model_best_{time_stamp}.pth"
    roc_curve_file_name = f"{output_path}/validation_roc_{time_stamp}.png"

    # Create dataset
    training_data_transform = transforms.Compose([transforms.ToTensor()])
    training_dataset = datasets.ImageFolder(
        root=training_data_path, transform=training_data_transform
    )
    training_loader = torch.utils.data.DataLoader(
        training_dataset, batch_size=batch_size, shuffle=True, num_workers=2
    )

    validation_data_transform = transforms.Compose([transforms.ToTensor()])
    validation_dataset = datasets.ImageFolder(
        root=validation_data_path, transform=validation_data_transform
    )
    validation_loader = torch.utils.data.DataLoader(
        validation_dataset, batch_size=1, num_workers=2
    )
    validation_data_size = len(validation_loader.dataset)
    print("Classes are: ", validation_dataset.class_to_idx)

    # Load network
    in_channels = validation_dataset[0][0].shape[0]
    class_num = len(validation_dataset.classes)

    model = ResNet50(in_channels, class_num, k=2)
    model = model.to(device)

    optimizer = optim.Adam(model.parameters(), lr=learning_rate, betas=(beta_1, 0.999))
    criterion = nn.CrossEntropyLoss()

    best_validation_loss = float("inf")
    best_validation_auc = 0.0

    for epoch in range(max_epoch_num):
        training_loss = 0
        valdation_loss = 0
        probabilities = np.zeros(validation_data_size)
        abnormal_labels = np.zeros(validation_data_size)

        # training
        model.train()

        training_data_transform = transforms.Compose(
            [
                transforms.Resize(224),
                transforms.RandomRotation(5),
                transforms.RandomHorizontalFlip(0.5),
                transforms.ToTensor(),
                transforms.Normalize([0.485, 0.456, 0.406], [0.229, 0.224, 0.225]),
            ]
        )

        training_dataset = datasets.ImageFolder(
            root=training_data_path, transform=training_data_transform
        )
        training_loader = torch.utils.data.DataLoader(
            training_dataset, batch_size=batch_size, shuffle=True, num_workers=2
        )

        for batch_idx, (data, labels) in enumerate(training_loader):
            data, labels = data.to(device), labels.to(device)

            optimizer.zero_grad()
            outputs = model(data)

            loss = criterion(outputs, labels)
            training_loss += loss.item()

            loss.backward()
            optimizer.step()

        avg_training_loss = training_loss / (batch_idx + 1)

        # validation
        model.eval()
        validation_accuracy = 0

        validation_data_transform = transforms.Compose(
            [
                transforms.Resize(224),
                transforms.ToTensor(),
                transforms.Normalize([0.485, 0.456, 0.406], [0.229, 0.224, 0.225]),
            ]
        )

        validation_dataset = datasets.ImageFolder(
            root=validation_data_path, transform=validation_data_transform
        )
        validation_loader = torch.utils.data.DataLoader(
            validation_dataset, batch_size=batch_size, shuffle=True, num_workers=2
        )

        with torch.no_grad():
            for batch_idx, (validation_data, validation_labels) in enumerate(
                validation_loader
            ):
                # 1:positive => 1
                abnormal_labels[batch_idx] = validation_labels[0]
                data, labels = validation_data.to(device), validation_labels.to(device)

                outputs = model(data)

                probabilities[batch_idx] = outputs.cpu()[0, 1]

                loss = criterion(outputs, labels)
                valdation_loss += loss.item()

                prediction = outputs.argmax(dim=1, keepdim=True)
                validation_accuracy += (
                    prediction.eq(labels.data.view_as(prediction)).sum().item()
                )

        avg_validation_loss = valdation_loss / (batch_idx + 1)

        if best_validation_loss > avg_validation_loss:
            best_validation_loss = avg_validation_loss
            torch.save(model.state_dict(), model_file_name)
            best_validation_auc = roc.roc_analysis(
                probabilities, abnormal_labels, roc_curve_file_name
            )
            saved_str = " ==> model saved"
        else:
            saved_str = ""

        print(
            "epoch %d: training_loss:%.4f validation_loss:%.4f validation_accuracy=%.3f%s"
            % (
                epoch + 1,
                avg_training_loss,
                avg_validation_loss,
                100.0 * validation_accuracy / len(validation_loader.dataset),
                saved_str,
            )
        )

        with open(loss_log_file_name, "a") as fp:
            fp.write(f"{epoch+1},{avg_training_loss},{avg_validation_loss}\n")

    return best_validation_auc


def main():
    # Parse command line arguments by argparse
    parser = argparse.ArgumentParser(
        description="Sample code to train 2-class classification model in SS2022"
    )
    parser.add_argument("training_data_path", help="Path of training data")
    parser.add_argument("validation_data_path", help="Path of validation data")
    parser.add_argument("output_path", help="Path of output data")
    parser.add_argument("-g", "--gpu_id", help="GPU ID", type=str, default="0")
    parser.add_argument(
        "-l", "--learning_rate", help="Learning rate", type=float, default=0.001
    )
    parser.add_argument("--beta_1", help="Beta_1 for Adam", type=float, default=0.99)
    parser.add_argument("-b", "--batch_size", help="Batch size", type=int, default=8)
    parser.add_argument(
        "-m",
        "--max_epoch_num",
        help="Maximum number of training epochs",
        type=int,
        default=50,
    )
    parser.add_argument(
        "--time_stamp", help="Time stamp for saved data", type=str, default=""
    )

    args = parser.parse_args()

    results = training(
        args.training_data_path,
        args.validation_data_path,
        args.output_path,
        args.learning_rate,
        args.beta_1,
        args.batch_size,
        args.max_epoch_num,
        args.gpu_id,
        args.time_stamp,
    )

    # print(f"AUC value of validation data: {results:.3f}")


if __name__ == "__main__":
    main()
