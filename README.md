# DiffCopy
A tool to copy only the changed and new files to a new folder, useful to create incremental update packages.

The program creates MD5 hashes of files and compares them to the ones of a baseline build to see if they were changed, and copies the changed or newly created files to a new folder.

[Download here](https://github.com/openbullet/DiffCopy/releases/download/1.0/DiffCopy.zip)

## Usage
```
dotnet ./DiffCopy.dll -s sourcedir -n newdir -o outputdir
```
Where

`-s` directory with baseline build

`-n` directory with latest release

`-o` output directory where only the changed or new files will be copied
