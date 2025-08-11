#ifndef OUTPUTS_H
#define OUTPUTS_H

#include "ProcessingInfo.h"
#include "ImageInfo2D.h"
#include <stdexcept>
#include <iostream>
#include <fstream>
#include <limits>
#include <cmath>

template <class OutputType>
class Outputs {
private:
    ProcessingInfo _processingInfo;
    ImageInfo2D _imgInfo;
    OutputType* _newPixels;
    int _pixelNum;
    void windowProcessor(short* originalPixels);
    void outputMIP();
public:
    Outputs(int pixelNum, ProcessingInfo &processingInfo);
    ~Outputs();
    void outputMHD(std::string elementType);
    void executeOutput(short* originalPixels);
};

#endif
