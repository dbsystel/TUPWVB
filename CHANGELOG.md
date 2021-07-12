# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.6] - 2021-05-12

### Changed
- Changed exception for invalid characters in Base32 encodings to FormatException.

## [2.0.5] - 2021-05-11

### Changed
- Clearer structure of getting encryption data from string.

## [2.0.4] - 2021-01-04

### Changed
- Corrected naming of some methods and improved error handling.

## [2.0.3] - 2021-01-04

### Changed
- Fixed some error messages.

## [2.0.2] - 2020-12-16

### Changed
- Made usage of SyncLock for disposal consistent.
- Added a maximum file size check for FileAndKeyEncryption.
- Changed some message creation methods.

## [2.0.1] - 2020-12-11

### Changed
- Put "IsValid" methods where they belong.
- Corrected tests for changed exception.

## [2.0.0] - 2020-12-10

### Changed
- Correct handling of diposed instances of FileAndKeyEncryption and SplitKeyEncryption.
- Throw ObjectDisposedException instead of InvalidOperationException if an instance of ProtectedByteArray or ShuffledByteArray is disposed of.

## [1.3.1] - 2020-12-10

### Changed
- Made hashing simpler and 2.5 times faster.
- Added missing test for source bytes being null.

## [1.3.0] - 2020-11-12

### Changed
- New format `6` introduced. It uses a custom form of Base32 encoding for the encrypted string.
- Documented new format in `README.md`.
- Changed `README.md` to use one line per sentence.
