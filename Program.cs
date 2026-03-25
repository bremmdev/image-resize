#:package SixLabors.ImageSharp@3.1.12
#:package CommandLineParser@2.9.1

#pragma warning disable IDE0005
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using CommandLine;
using SixLabors.ImageSharp.Formats.Webp;
#pragma warning restore IDE0005

/** 
    This program is a command line tool that resizes images in a directory non-recursively to a specified width or height.
    It uses the SixLabors.ImageSharp library to load and save images.
    It uses the CommandLineParser library to parse command line arguments.
    It uses the WebpEncoder library to encode images as Webp.
**/

Parser.Default.ParseArguments<CLIArguments>(args).WithParsed(options =>
{
    try
    {
        CLIArgumentsValidator.Validate(options);
        IEnumerable<string> imageFiles = CLIArgumentsValidator.GetImageFiles(options.InputPath);

        foreach (string file in imageFiles)
        {
            try
            {
                using Image image = Image.Load(file);
                var (width, height) = ImageResizer.GetDimensions(
                    file,
                    image.Width,
                    image.Height,
                    options.Width,
                    options.Height);

                ImageResizer.ResizeAndSaveAsWebp(options.InputPath, file, image, width, height);
            }
            catch (Exception e) when (e is ArgumentException || e is IOException)
            {
                Console.Error.WriteLine($"Skipping {file}: {e.Message}");
            }
        }
    }
    catch (ArgumentException e)
    {
        Console.Error.WriteLine($"Error: {e.Message}");
    }
});

class ImageResizer
{
    public static (int width, int height) GetDimensions(string file, int currentWidth, int currentHeight, int? targetWidth, int? targetHeight)
    {
        double currentAspectRatio = (double)currentWidth / currentHeight;

        if (targetWidth is not null)
        {
            if (targetWidth > currentWidth)
            {
                throw new ArgumentException($"Target width {targetWidth} is greater than current width {currentWidth} for file {file}.");
            }
            targetHeight = (int)(targetWidth / currentAspectRatio);
        }
        else if (targetHeight is not null)
        {
            if (targetHeight > currentHeight)
            {
                throw new ArgumentException($"Target height {targetHeight} is greater than current height {currentHeight} for file {file}.");
            }
            targetWidth = (int)(targetHeight * currentAspectRatio);
        }
        return (targetWidth ?? 0, targetHeight ?? 0);
    }

    public static void ResizeAndSaveAsWebp(string inputPath, string file, Image image, int targetWidth, int targetHeight)
    {
        string outputPath = Path.Combine(inputPath, "resized", Path.GetFileName(file));
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        image.Mutate(x => x.Resize(targetWidth, targetHeight));
        image.Save(Path.ChangeExtension(outputPath, ".webp"), new WebpEncoder { Quality = 80 });
        Console.WriteLine($"Resized {file} to {targetWidth}x{targetHeight}");
    }
}

class CLIArgumentsValidator
{
    static readonly string[] ALLOWED_IMAGE_EXTENSIONS = [".jfif", ".jpg", ".jpeg", ".png", ".webp"];
    public static void Validate(CLIArguments options)
    {
        if (File.Exists(options.InputPath))
            throw new ArgumentException($"'{options.InputPath}' is a file, not a directory.");
        if (!Directory.Exists(options.InputPath) || Directory.GetFiles(options.InputPath).Length == 0)
            throw new ArgumentException($"Directory '{options.InputPath}' does not exist or is empty.");
        if (options.Width is <= 0)
            throw new ArgumentException("Width must be a positive value.");

        if (options.Height is <= 0)
            throw new ArgumentException("Height must be a positive value.");
    }

    public static IEnumerable<string> GetImageFiles(string inputPath)
    {
        List<string> files = Directory.GetFiles(inputPath).Where(file => ALLOWED_IMAGE_EXTENSIONS.Contains(Path.GetExtension(file).ToLower())).ToList();
        if (files.Count == 0)
            throw new ArgumentException($"No image files found in directory '{inputPath}'.");
        return files;
    }

}

// First positional argument is the path to the input folder
// Exactly one of --width or --height must be provided.
// CommandLineParser enforces this via different SetName values.
class CLIArguments
{
    [Value(0, MetaName = "inputPath", Required = true, HelpText = "Path to the input folder")]
    public string InputPath { get; set; } = string.Empty;

    [Option('w', "width", SetName = "width", Required = true,
        HelpText = "Target width in pixels (aspect ratio preserved)")]
    public int? Width { get; set; }

    [Option('h', "height", SetName = "height", Required = true,
        HelpText = "Target height in pixels (aspect ratio preserved)")]
    public int? Height { get; set; }
}