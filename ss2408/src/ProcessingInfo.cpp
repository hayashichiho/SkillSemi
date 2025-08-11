#include "ProcessingInfo.h"

void ProcessingInfo::showProcessingInfo() {
    std::cout << "Input: " << inputFilename << std::endl;
    if (windowProcessing) {
        std::cout << "WindowLevel: " << windowLevel << ", WindowWidth: " 
                  << windowWidth << std::endl;
    }
    std::cout << "ViewAngle: " << viewAnglePhi << " " << viewAngleTheta << " "
              << viewAnglePsi << std::endl;
}
