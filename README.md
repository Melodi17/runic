# Runic
Runic is a command-line image templating tool for generating large batches of visuals
(e.g. cards, posters, or social assets) from structured data sources such as CSV files.

It is designed for repeatable, data-driven visual production without manual layout work.

## Features
- Bulk image generation from CSV data
- JSON-based template format
- Text and image components with bounding-box layout
- Variable substitution with recursive expansion
- Text alignment, casing, outlines, and font sizing
- Multiple image scaling modes (`zoom`, `fit`, `stretch`, `crop`)
- Debug mode for visual layout inspection

## Installation
### From source
```sh
git clone https://github.com/yourname/runic
cd runic
dotnet build -c Release

dotnet run -- -t template.json -d data.csv -o output/
```

### Install as a global tool
```sh
dotnet tool install -g runic
```

## Quick Start
```sh
runic -t template.json -d data.csv -o output/
```
This command generates one image per row in `data.csv` using `template.json`,
saving the results to the output directory.

---

## Usage
```sh
$ runic -t <template> -d <data-source> -o <output>
```

```
  -t, --template        Required. Path to the template file.
  -d, --data-source     Required. Path to the data source file.
  -o, --output          Required. Path to the output folder.
  -v, --verbose         (Default: false) Enable verbose output.
  -c, --clean-output    (Default: false) Clean the output folder before processing.
  -D, --debug           (Default: false) Enable debug mode for additional logging.
  --help                Display this help screen.
  --version             Display version information.
```

## Template Format
Templates are defined using JSON and describe how data entries are mapped to visual components.

### Top-Level Properties
- BaseImage (string, required)
  - Path to the base image. All output images are generated on top of a copy of this image.
- NameFormat (string, required)
  - Output filename format. \$id refers to the current entry’s field, and \$# refers to the numerical index.
- Variables (object, optional)
  - Constant variables accessible using @\<name>.
- Components (array, required)
  - A list of visual components to render.

### Example Template
> Remove comments before use
```jsonc
{
  "BaseImage": "base.png", // Required. Image path relative to template path, all images will be generated on top of a copy of this one.
  "NameFormat": "output_$id", // Filename of each entry. $# can be used for numerical index of the entry
  "Variables": { // These are constant variables that can be referred to using @<varname>
    "c_red": "#FF0000",
    "c_blue": "#00FF00",
    "c_green": "#0000FF",
  },
  "Components": [
    {
      "Type": "Text", // Image and Text components are supported
      "X": 100, // X,Y,W,H all control the bounding box that the text will be inside. Enable the debug flag to see this more clearly.
      "Y": 100,
      "Width": 450,
      "Height": 450,
      "Text": "$name", // Accessing the current entry's name value
      "Font": "Arial",
      "Size": 85,
      "Alignment": "top,left", // top|middle|bottom , left|center|right
      "Color": "#FF0000", // Currently only supports hex codes.
      "Case": "upper" // Transforms all the text to upper, supports upper|lower|title
    },
    {
      "Type": "Text",
      "X": 0,
      "Y": 1050,
      "Width": 450,
      "Height": 450,
      "Text": "\"$quote\"", // As you can see, other constant text can be inserted around the variable
      "Font": "Arial",
      "Size": 50,
      "Alignment": "bottom,left",
      "Color": "@c_$color", // Due to the recursive variable expansion, this will expand to @c_<color data entry> and then find the corresponding constant.
      "OutlineColor": "#000000",
      "OutlineWidth": 1
    },
    {
      "Type": "Image",
      "X": 0,
      "Y": 0,
      "Width": 900,
      "Height": 1500,
      "ImagePath": "Templates/debug_overlay.png", // This image path is relative to the CWD.
      "SizeMode": "zoom", // zoom|fit|stretch|crop
      "Visible": false // Prevents component from rendering
    }
  ]
}
```

## Data Sources
Currently, only CSV files are supported as data sources.

Each row represents one generated image, with column's values accessible via `$<column_name>` in the template.

### Example CSV
```csv
id,name,quote,color
1,Alice,"To be or not to be",red
2,Bob,"I think, therefore I am",blue
3,Charlie,"The only thing we have to fear is fear itself",green
4,Diana,"That's one small step for man ...",red
```

## Limitations & Future Work
- Only CSV data sources are supported currently.
- Limited image formats (PNG, JPEG).
- More component types (shapes, lines, etc.) could be added.

## Project Status
Runic is not in active development. It is a small utility project created to solve a specific need.

## Contributing
Contributions are welcome! Please open issues or pull requests on the GitHub repository.

## License
This project is licensed under the MIT License.