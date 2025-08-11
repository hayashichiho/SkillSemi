#ifndef TRICUBIC_PROCESSING_H
#define TRICUBIC_PROCESSING_H

#include <vector>

class TricubicProcessing {
public:
    static std::vector<short> perform_isotropic_resampling(
        const std::vector<short>& input_data, int width, int height, int depth,
        float new_width, float new_height, float new_depth, float a, int interpolation_method);
};

#endif
