#include <iostream>
#include <filesystem>
#include <vector>

class Path {
public:
    Path(const std::string& path) : path_(path) {}

    /*現在の作業ディレクトリを取得*/
    static Path cwd() {
        return Path(std::filesystem::current_path().string());
    }

    /*親ディレクトリを取得*/
    Path parent() const {
        return Path(std::filesystem::path(path_).parent_path().string());
    }

    /*パスの名前部分を取得*/
    std::string name() const {
        return std::filesystem::path(path_).filename().string();
    }

    /*ファイルの拡張子を取得*/
    std::string suffix() const {
        return std::filesystem::path(path_).extension().string();
    }

    /*ファイルの拡張子を除いた名前を取得*/
    std::string stem() const {
        return std::filesystem::path(path_).stem().string();
    }

    /*子ディレクトリやファイル名を追加した新しいパスを作成*/
    Path joinpath(const std::string& other) const {
        return Path((std::filesystem::path(path_) / other).string());
    }

    /*パスの名前部分を指定された名前に変更*/
    Path with_name(const std::string& new_name) const {
        return Path(std::filesystem::path(path_).parent_path() / new_name);
    }

    /*ベース名を新しいものに変更し，元の拡張子を保持*/
    Path with_stem(const std::string& new_stem) const {
        return Path(std::filesystem::path(path_).parent_path() / (new_stem + suffix()));
    }

    /*拡張子を新しいものに変更*/
    Path with_suffix(const std::string& new_suffix) const {
        return Path(std::filesystem::path(path_).replace_extension(new_suffix).string());
    }

    /*パスが存在するかを確認*/
    bool exists() const {
        return std::filesystem::exists(path_);
    }

    /*パスがファイルかを確認*/
    bool is_file() const {
        return std::filesystem::is_regular_file(path_);
    }

    /*パスがディレクトリかを確認*/
    bool is_dir() const {
        return std::filesystem::is_directory(path_);
    }

    /*パスのステータスを取得*/
    std::filesystem::file_status stat() const {
        return std::filesystem::status(path_);
    }

    /*POSIX形式でパスを取得*/
    std::string as_posix() const {
        return std::filesystem::path(path_).generic_string();
    }

    /*ディレクトリ内のエントリを取得*/
    std::vector<Path> iterdir() const {
        std::vector<Path> entries;
        for (const auto& entry : std::filesystem::directory_iterator(path_)) {
            entries.emplace_back(entry.path().string());
        }
        return entries;
    }

private:
    std::string path_;
};

int main() {
    /*メイン関数*/
    Path p = Path::cwd();
    std::cout << "Current working directory: " << p.as_posix() << std::endl;

    Path parent = p.parent();
    std::cout << "Parent directory: " << parent.as_posix() << std::endl;

    std::cout << "Name: " << p.name() << std::endl;
    std::cout << "Suffix: " << p.suffix() << std::endl;
    std::cout << "Stem: " << p.stem() << std::endl;

    Path joined = p.joinpath("test.txt");
    std::cout << "Joined path: " << joined.as_posix() << std::endl;

    Path new_name = p.with_name("new_name.txt");
    std::cout << "With new name: " << new_name.as_posix() << std::endl;

    Path new_stem = p.with_stem("new_stem");
    std::cout << "With new stem: " << new_stem.as_posix() << std::endl;

    Path new_suffix = p.with_suffix(".md");
    std::cout << "With new suffix: " << new_suffix.as_posix() << std::endl;

    std::cout << "Exists: " << p.exists() << std::endl;
    std::cout << "Is file: " << p.is_file() << std::endl;
    std::cout << "Is directory: " << p.is_dir() << std::endl;

    std::vector<Path> entries = p.iterdir();
    std::cout << "Directory entries:" << std::endl;
    for (const auto& entry : entries) {
        std::cout << "  " << entry.as_posix() << std::endl;
    }

    return 0;
}