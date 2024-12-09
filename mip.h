#ifndef MIP_H
#define MIP_H

#include <map>
#include <string>
#include <vector>

class EulerAngles {
   private:
    double _phi;    // X軸周りの回転角
    double _theta;  // Y軸周りの回転角
    double _psi;    // Z軸周りの回転角

   public:
    EulerAngles(double p, double t, double s) : _phi(p), _theta(t), _psi(s) {}

    // アクセサメソッド
    double get_phi() const {
        return _phi;
    }
    double get_theta() const {
        return _theta;
    }
    double get_psi() const {
        return _psi;
    }

    // MIP画像生成メソッド
    static std::vector<unsigned char> generate_mip_image(const std::vector<unsigned char>& raw_data, int width,
                                                         int height, int depth, const EulerAngles& angles,
                                                         const std::map<std::string, double>& spacing);
};

#endif  // MIP_H
