#include "ProcessingInfoParser.h"
#include "MIPGenerator.h"
#include "Outputs.h"
#include <iostream>
#include <chrono>

int main() {
    ProcessingInfoParser parser;
    if (!parser.executeParsing()) {
        return 1;
    }
    ProcessingInfo processingInfo = parser.getProcessingInfo();
    ImageInfo3D imgInfo3D = parser.getImgInfo();
    MIPGenerator mipGenerator(processingInfo, imgInfo3D);

    auto startTime = std::chrono::high_resolution_clock::now();
    mipGenerator.renderMIPImage();
    auto endTime = std::chrono::high_resolution_clock::now();
    std::cout << std::chrono::duration<double, std::milli>(endTime - startTime).count() 
        << "[ms]" << std::endl;

    if (processingInfo.windowProcessing) {
        Outputs<unsigned char> outputs(mipGenerator.getMIPPixelNum(), processingInfo);
        outputs.executeOutput(mipGenerator.getMIPPixels());
    } else {
        Outputs<short> outputs(mipGenerator.getMIPPixelNum(), processingInfo);
        outputs.executeOutput(mipGenerator.getMIPPixels());
    }
    return 0;
}
