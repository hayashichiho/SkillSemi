#pragma once
#include <vector>
#include "euler_angles.h"

std::vector<unsigned char> generate_mip_image(
    const std::vector<unsigned char>& raw_data,
    int width, int height, int depth,
    const EulerAngles& angles = EulerAngles());