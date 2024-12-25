"""
Test of 2-class classification model
Created on Fri Oct 14 2022
@author: ynomura
"""

import argparse
import os
import sys
from datetime import datetime as dt

import numpy as np
import roc
import torch
from resnet18 import WideResNet18
from torchvision import datasets, transforms


def do_test(test_data_path, model_file_name, output_path, gpu_id="0", time_stamp=""):
    if not os.path.isdir(test_data_path):
        print(f"Error: Path of test data ({test_data_path}) is not found.")
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
    roc_curve_file_name = f"{output_path}/test_roc_{time_stamp}.png"

    # Create dataset
    test_data_transform = transforms.Compose([transforms.ToTensor()])
    test_dataset = datasets.ImageFolder(
        root=test_data_path, transform=test_data_transform
    )
    test_loader = torch.utils.data.DataLoader(test_dataset, batch_size=1, num_workers=2)
    test_data_size = len(test_loader.dataset)

    print("Classes are: ", test_dataset.class_to_idx)
    class_labels = list(test_dataset.class_to_idx.keys())

    # load network
    in_channels = test_dataset[0][0].shape[0]
    class_num = len(test_dataset.classes)
    model = WideResNet18(3, 2)
    model.load_state_dict(torch.load(model_file_name, map_location=device))
    model = model.to(device)
    model.eval()

    auc = 0.0
    test_accuracy = 0
    probabilities = np.zeros(test_data_size)
    abnormal_labels = np.zeros(test_data_size)

    with torch.no_grad():
        for batch_idx, (test_data, test_labels) in enumerate(test_loader):
            abnormal_labels[batch_idx] = test_labels[0]  # 1:positive => 1
            data, labels = test_data.to(device), test_labels.to(device)

            outputs = model(data)
            probabilities[batch_idx] = outputs.cpu()[0, 1]

            pred = outputs.argmax(dim=1, keepdim=True)
            test_accuracy += pred.eq(labels.data.view_as(pred)).sum().item()

        auc, tpr, fpr, cutoff_threshold = roc.roc_analysis(
            probabilities, abnormal_labels, roc_curve_file_name
        )
        print(
            f"Sensitivity: {tpr:.3f}, Specificity: {1.0 - fpr:.3f} ({cutoff_threshold})"
        )

    return auc


def main():
    # Parse command line arguments by argparse
    parser = argparse.ArgumentParser(
        description="Sample code to test 2-class classification model in SS2022"
    )
    parser.add_argument("test_data_path", help="Path of test data")
    parser.add_argument("model_file_name", help="File name of trained model")
    parser.add_argument("output_path", help="Path of output data")
    parser.add_argument("-g", "--gpu_id", help="ID of GPU", default="0")
    parser.add_argument(
        "--time_stamp", help="Time stamp for saved data", type=str, default=""
    )

    args = parser.parse_args()

    auc = do_test(
        args.test_data_path,
        args.model_file_name,
        args.output_path,
        args.gpu_id,
        args.time_stamp,
    )

    print(f"Test AUC: {auc:.3f}")


if __name__ == "__main__":
    main()
