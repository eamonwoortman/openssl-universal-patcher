# OpenSSL Universal Patcher

OpenSSL Universal Patcher is a command-line tool designed to patch 64-bit Windows binary executables that are linked to an older version of OpenSSL (1.1.0). This tool helps to resolve crashes in 10th and 11th gen Intel CPUs and improve the overall stability and security of the affected binaries. More information about the crash can be found in this [Intel support article](https://www.intel.com/content/www/us/en/support/articles/000060819/software/software-applications.html).

*⚠️NOTE: This patcher was created for a specific Unity 5.4.0f3 game, and although it aims to be an universal patcher, it may not work on your binary. Having that said, I have tested it on an Unreal 4.21 game build and it successfully patched that as well.*

[!["Buy Me A Coffee"](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/eamonw)

## Table of Contents

- [Installation](#installation)
- [Usage](#usage)
- [License](#license)

## Installation

To install OpenSSL Universal Patcher, simply download the latest release from the [releases](https://github.com/eamonwoortman/openssl-universal-patcher/releases) page and extract the `OpenSSLUniversalPatcher.exe` file to your desired location.

## Usage

To use OpenSSL Universal Patcher, open a command prompt and navigate to the directory containing the `OpenSSLUniversalPatcher.exe` file. Then, execute the following command:

```
OpenSSLUniversalPatcher.exe <oldfile> <newfile>
```

- `<oldfile>`: The path to the binary file to be patched (linked to OpenSSL 1.1.0).
- `<newfile>`: The path where the patched binary file will be saved.

For example, to patch a file called `old-binary.exe` and save the patched file as `new-binary.exe`, the command would look like:

```
OpenSSLUniversalPatcher.exe old-binary.exe new-binary.exe
```

## License

OpenSSL Universal Patcher is released under the [MIT License](LICENSE). By using this tool, you agree to the terms and conditions specified in the license.
