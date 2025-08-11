#ifndef PROCESSING_INFO_H
#define PROCESSING_INFO_H

#include <string>
#include <iostream>

class ProcessingInfo {
public:
    std::string inputFilename;
    bool windowProcessing = false;
    int windowLevel{};
    int windowWidth{};
    double viewAnglePhi{};
    double viewAngleTheta{};
    double viewAnglePsi{};
    void showProcessingInfo();
};

#endif
