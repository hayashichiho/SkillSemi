#ifndef PROCESSING_INFO_PARSER_H
#define PROCESSING_INFO_PARSER_H

#include "ProcessingInfo.h"
#include "ImageInfo3D.h"

struct TextErrorChecker {
    bool input = false;
    bool windowProcessing = false;
    bool windowLevel = false;
    bool windowWidth = false;
    bool viewAngle = false;
};

struct MHDErrorChecker {
    bool dimSize = false;
    bool elementSpacing = false;
    bool elementType = false;
    bool elementDataFile = false;
};

class ProcessingInfoParser {
private:
    ProcessingInfo _processingInfo;
    ImageInfo3D _imgInfo;
    void parseText();
    void parseMHD();
public:
    bool executeParsing();
    ProcessingInfo getProcessingInfo();
    ImageInfo3D getImgInfo();
};

#endif
