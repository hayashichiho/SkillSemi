#include "ProcessingInfoParser.h"
#include <fstream>
#include <regex>
#include <iostream>
#include <stdexcept>
#include <limits>

#define PI_180 0.017453292519943

bool ProcessingInfoParser::executeParsing() {
    try {
        parseText();
    } catch (const std::exception& error) {
        std::cout << error.what() << std::endl;
        return false;
    }
    _processingInfo.showProcessingInfo();
    try {
        parseMHD();
    } catch (const std::exception& error) {
        std::cout << error.what() << std::endl;
        return false;
    }
    _imgInfo.showImgInfo();
    return true;
}

ProcessingInfo ProcessingInfoParser::getProcessingInfo() {
    return _processingInfo;
}

ImageInfo3D ProcessingInfoParser::getImgInfo() {
    return _imgInfo;
}

void ProcessingInfoParser::parseText() {
    std::ifstream textFileStream("ProcessingParameter.txt");
    if (!textFileStream) {
        throw std::runtime_error("ProcessingParameter.txtが開けませんでした");
    }
    std::string line;
    TextErrorChecker errorChecker;
    while (std::getline(textFileStream, line)) {
        std::smatch match;
        if (std::regex_match(line, match, std::regex("^Input = (.+)$"))) {
            _processingInfo.inputFilename = match[1];
            errorChecker.input = true;
            continue;
        }
        if (std::regex_match(line, match, std::regex("^WindowProcessing = (True|False)$"))) {
            if (match[1] == "True") {
                _processingInfo.windowProcessing = true;
            }
            errorChecker.windowProcessing = true;
            continue;
        }
        if (std::regex_match(line, match, std::regex("^WindowLevel = ([-0-9]+)$"))) {
            try {
                _processingInfo.windowLevel = std::stoi(match[1]);
            } catch (...) {
                throw std::runtime_error("WindowLevelの数値変換に失敗しました");
            }
            errorChecker.windowLevel = true;
            continue;
        }

        if (std::regex_match(line, match, std::regex("^WindowWidth = ([0-9]+)$"))) {
            try {
                _processingInfo.windowWidth = std::stoi(match[1]);
            } catch (...) {
                throw std::runtime_error("WindowWidthの数値変換に失敗しました");
            }
            errorChecker.windowWidth = true;
            continue;
        }
        if (std::regex_match(line, match, std::regex("^ViewAngle = ([-0-9\\.]+) ([-0-9\\.]+) ([-0-9\\.]+)$"))) {
            try {
                _processingInfo.viewAnglePhi = std::stod(match[1]);
                _processingInfo.viewAngleTheta = std::stod(match[2]);
                _processingInfo.viewAnglePsi = std::stod(match[3]);
            } catch (...) {
                throw std::runtime_error("ViewAngleの数値変換に失敗しました");
            }
            errorChecker.viewAngle = true;
            continue;
        }
        else continue;
    }
    if (!errorChecker.input)
        throw std::runtime_error("Inputの読み取りに失敗しました");
    if (!errorChecker.windowProcessing)
        throw std::runtime_error("WindowProcessingの読み取りに失敗しました");
    if (!errorChecker.windowLevel)
        throw std::runtime_error("WindowLevelの読み取りに失敗しました");
    if (!errorChecker.windowWidth)
        throw std::runtime_error("WindowWidthの読み取りに失敗しました");
    if (!errorChecker.viewAngle)
        throw std::runtime_error("ViewAngleの読み取りに失敗しました");
}

void ProcessingInfoParser::parseMHD() {
    std::ifstream MHDFileStream(_processingInfo.inputFilename + ".mhd");
    if (!MHDFileStream) {
        throw std::runtime_error("MHDファイルが開けませんでした");
    }
    MHDErrorChecker errorChecker;
    std::string line;
    while (std::getline(MHDFileStream, line)) {
        std::smatch match;
        if (std::regex_match(line, match, std::regex("^\\s*DimSize\\s*=\\s*(\\d+)\\s+(\\d+)\\s+(\\d+)\\s*$"))) {
            try {
                _imgInfo._dimSizeX = std::stoi(match[1]);
                _imgInfo._dimSizeY = std::stoi(match[2]);
                _imgInfo._dimSizeZ = std::stoi(match[3]);
            } catch (...) {
                throw std::runtime_error("DimSizeの数値変換に失敗しました");
            }
            errorChecker.dimSize = true;
            continue;
        }
        if (std::regex_match(line, match, std::regex("^\\s*ElementSpacing\\s*=\\s*([0-9\\.]+)\\s+([0-9\\.]+)\\s+([0-9\\.]+)\\s*$"))) {
            try {
                _imgInfo._elementSpacingX = std::stod(match[1]);
                _imgInfo._elementSpacingY = std::stod(match[2]);
                _imgInfo._elementSpacingZ = std::stod(match[3]);
            } catch (...) {
                throw std::runtime_error("ElementSpacingの数値変換に失敗しました");
            }
            errorChecker.elementSpacing = true;
            continue;
        }
        if (std::regex_match(line, match, std::regex("^\\s*ElementType\\s*=\\s*([A-Z_]+)\\s*$"))) {
            _imgInfo._elementType = match[1];
            errorChecker.elementType = true;
            continue;
        }
        if (std::regex_match(line, match, std::regex("^\\s*ElementDataFile\\s*=\\s*(.+)\\s*$"))) {
            _imgInfo._elementDataFile = match[1];
            errorChecker.elementDataFile = true;
            continue;
        }
        else continue;
    }
    if (!errorChecker.dimSize)
        throw std::runtime_error("DimSizeの読み取りに失敗しました");
    if (!errorChecker.elementSpacing)
        throw std::runtime_error("ElementSpacingの読み取りに失敗しました");
    if (!errorChecker.elementType)
        throw std::runtime_error("ElementTypeの読み取りに失敗しました");
    if (!errorChecker.elementDataFile)
        throw std::runtime_error("ElementDataTypeの読み取りに失敗しました");
}
