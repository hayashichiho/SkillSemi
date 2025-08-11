#include "Outputs.h"

template <class OutputType>
Outputs<OutputType>::Outputs(int pixelNum, ProcessingInfo &processingInfo) {
    _processingInfo = processingInfo;
    _newPixels = new OutputType[pixelNum];
    _pixelNum = pixelNum;
}

template <class OutputType>
Outputs<OutputType>::~Outputs() {
    delete[] _newPixels;
}

template <class OutputType>
void Outputs<OutputType>::windowProcessor(short* originalPixels) {
    for (int i = 0; i < _pixelNum; i++) {
        if (originalPixels[i] > _processingInfo.windowLevel + _processingInfo.windowWidth * 0.5) {
            _newPixels[i] = std::numeric_limits<unsigned char>::max();
            continue;
        } else if (originalPixels[i] < _processingInfo.windowLevel - _processingInfo.windowWidth * 0.5) {
            _newPixels[i] = std::numeric_limits<unsigned char>::min();
            continue;
        } else {
            _newPixels[i] = (originalPixels[i] - (_processingInfo.windowLevel - _processingInfo.windowWidth / 2))
                * (std::numeric_limits<unsigned char>::max() - std::numeric_limits<unsigned char>::min())
                / _processingInfo.windowWidth;
        }
    }
}

template <class OutputType>
void Outputs<OutputType>::outputMHD(std::string elementType) {
    _imgInfo._elementType = elementType;
    _imgInfo._dimSizeX = (int)std::sqrt(_pixelNum);
    _imgInfo._dimSizeY = (int)std::sqrt(_pixelNum);
    _imgInfo._elementDataFile = _processingInfo.inputFilename + "_mip.raw";
    std::string outputMHDFilename = _processingInfo.inputFilename + "_mip.mhd";
    std::ofstream fstream(outputMHDFilename);
    if (!fstream) {
        throw std::runtime_error("出力用MHDファイルが開けませんでした");
    } else {
        fstream << "ObjectType = Image\nNDims = 2\nDimSize = " << _imgInfo._dimSizeX << ' '
                << _imgInfo._dimSizeY << "\nElementType = " << _imgInfo._elementType
                << "\nElementSpacing = " << _imgInfo._elementSpacingX << " " << _imgInfo._elementSpacingY
                << "\nElementByteOrderMSB = False\nElementDataFile = " << _imgInfo._elementDataFile;
    }
}

template <class OutputType>
void Outputs<OutputType>::outputMIP() {
    FILE* fp = fopen(_imgInfo._elementDataFile.c_str(), "wb");
    if (!fp) {
        throw std::runtime_error("出力用rawファイルが開けませんでした");
    } else {
        fwrite(_newPixels, sizeof(OutputType), _pixelNum, fp);
        fclose(fp);
    }
}

template <class OutputType>
void Outputs<OutputType>::executeOutput(short* originalPixels) {
    if (!_processingInfo.windowProcessing) {
        for (int i = 0; i < _pixelNum; i++) {
            _newPixels[i] = (OutputType)originalPixels[i];
        }
        try {
            outputMHD("MET_SHORT");
        } catch (const std::runtime_error& error) {
            std::cout << error.what() << std::endl;
            return;
        }
    } else {
        windowProcessor(originalPixels);
        try {
            outputMHD("MET_UCHAR");
        } catch (const std::runtime_error& error) {
            std::cout << error.what() << std::endl;
            return;
        }
    }
    try {
        outputMIP();
    } catch (const std::runtime_error& error) {
        std::cout << error.what() << std::endl;
        return;
    }
    _imgInfo.showImgInfo();
}

// テンプレートの明示的インスタンス化
template class Outputs<unsigned char>;
template class Outputs<short>;
