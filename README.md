# image-resize

A command-line tool that batch-resizes images in a directory to a target width or height, preserving the aspect ratio, and saves them as WebP.

## Requirements

- **.NET 10 SDK or later** — this is a file-based C# app using `#:package` directives, which require **C# 14** (shipped with .NET 10). No `.csproj` file is needed.

## How it works

Point the tool at a folder of images and specify either a target width or a target height. It reads every supported image in that folder (non-recursively), scales it down while preserving the original aspect ratio, and writes the result as a `.webp` file into a `resized/` subfolder alongside the originals.

## Usage

```bash
dotnet run Program.cs <inputPath> --width <pixels>
dotnet run Program.cs <inputPath> --height <pixels>
```

| Argument    | Short | Description                                                |
| ----------- | ----- | ---------------------------------------------------------- |
| `inputPath` | —     | Path to the folder containing the source images (required) |
| `--width`   | `-w`  | Target width in pixels; height is calculated automatically |
| `--height`  | `-h`  | Target height in pixels; width is calculated automatically |

Exactly one of `--width` or `--height` must be provided.

### Examples

Resize all images to 800 px wide:

```bash
dotnet run Program.cs "./photos" --width 800
```

Resize all images to 600 px tall:

```bash
dotnet run Program.cs "./photos" --height 600
```

## Output

Resized images are saved to `<inputPath>/resized/` with their original filename but with the extension changed to `.webp`. The encoder uses a quality setting of **80**.

## Supported input formats

- `.jpg` / `.jpeg` / `.jfif`
- `.png`
- `.webp`

## Limitations

- **No upscaling.** If the target dimension is larger than the source image's corresponding dimension, that file is skipped with an error message and processing continues.
- **Non-recursive.** Only the top-level files in the specified folder are processed; subdirectories are ignored.
- **Single dimension at a time.** You cannot specify both `--width` and `--height` simultaneously.
- **Output format is always WebP.** There is no option to keep the original format.
- **Quality is fixed at 80.** WebP encoder quality is not configurable from the command line.

## Dependencies

Managed automatically via C# 14 file-level package directives — no manual restore step required:

| Package                | Version |
| ---------------------- | ------- |
| `SixLabors.ImageSharp` | 3.1.12  |
| `CommandLineParser`    | 2.9.1   |
