# Generate an original, multi-resolution Windows ICO from the selected
# Companion Beacon WPF vector. No downloaded art, fonts or external programs.
# Build runs on Windows in STA mode before .NET assembles the application icon.
param(
    [string]$OutputPath = ""
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $root "desktop\PetPing.Windows\Assets\PetPing.ico"
}
if ([System.Threading.Thread]::CurrentThread.GetApartmentState() -ne 'STA') {
    throw "Brand icon generator must run from a single-threaded apartment (PowerShell -STA)."
}
Add-Type -AssemblyName PresentationFramework, PresentationCore, WindowsBase
$source = Join-Path $root "desktop\PetPing.Windows\BrandMark.xaml"
# Compiled WPF classes require x:Class. The dynamically loaded markup must
# omit ONLY that compiler directive; all geometric elements remain identical.
$markup = Get-Content -LiteralPath $source -Raw -Encoding UTF8
$markup = [regex]::Replace($markup, '\s+x:Class="[^"]+"', '')
$mark = [System.Windows.Markup.XamlReader]::Parse($markup)
if ($mark -isnot [System.Windows.Controls.UserControl]) {
    throw "Expected a vector WPF UserControl, got a different brand asset."
}
$sizes = @(16, 24, 32, 48, 64, 128, 256)
$images = [System.Collections.Generic.List[byte[]]]::new()
foreach ($size in $sizes) {
    $mark.Width = $size
    $mark.Height = $size
    $mark.Measure([System.Windows.Size]::new($size, $size))
    $mark.Arrange([System.Windows.Rect]::new(0, 0, $size, $size))
    $mark.UpdateLayout()
    $target = [System.Windows.Media.Imaging.RenderTargetBitmap]::new(
        $size, $size, 96, 96,
        [System.Windows.Media.PixelFormats]::Pbgra32)
    $target.Render($mark)
    $encoder = [System.Windows.Media.Imaging.PngBitmapEncoder]::new()
    $encoder.Frames.Add([System.Windows.Media.Imaging.BitmapFrame]::Create($target))
    $png = [System.IO.MemoryStream]::new()
    try {
        $encoder.Save($png)
        $bytes = $png.ToArray()
        if ($bytes.Length -lt 100) { throw "Unexpectedly empty $size-pixel logo." }
        $images.Add($bytes)
    }
    finally { $png.Dispose() }
}
$directory = Split-Path -Parent $OutputPath
[System.IO.Directory]::CreateDirectory($directory) | Out-Null
$stream = [System.IO.File]::Create($OutputPath)
try {
    $writer = [System.IO.BinaryWriter]::new($stream)
    try {
        $writer.Write([uint16]0)
        $writer.Write([uint16]1)
        $writer.Write([uint16]$sizes.Count)
        $offset = 6 + 16 * $sizes.Count
        for ($i = 0; $i -lt $sizes.Count; $i++) {
            # In ICO, byte 0 means 256px.
            $dimension = if ($sizes[$i] -eq 256) { 0 } else { $sizes[$i] }
            $writer.Write([byte]$dimension)
            $writer.Write([byte]$dimension)
            $writer.Write([byte]0)
            $writer.Write([byte]0)
            $writer.Write([uint16]1)
            $writer.Write([uint16]32)
            $writer.Write([uint32]$images[$i].Length)
            $writer.Write([uint32]$offset)
            $offset += $images[$i].Length
        }
        foreach ($data in $images) { $writer.Write([byte[]]$data) }
        $writer.Flush()
    }
    finally { $writer.Dispose() }
}
finally { $stream.Dispose() }
$iconFile = [System.IO.FileInfo]::new($OutputPath)
if ($iconFile.Length -lt 1000) { throw "ICO generation unexpectedly produced a tiny file." }
Write-Host "PASS: Companion Beacon ICO, $($sizes.Count) PNG frames (16 to 256px), $($iconFile.Length) bytes: $OutputPath"
