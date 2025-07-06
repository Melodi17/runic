# Runic
A templating program designed to bulk-create visuals from a dataset

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

## Template
The template file format is a json file, it describes how to bind the data source to a visual.

Example (remove comments before using)
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
Currently only csv files are supported.

Example (made to go with template file from above):
```csv
id,name,quote,color
1,Alice,"Sometimes the truth hurts more than a lie.",red
2,Bob,"Every piece of evidence tells a story.",blue
3,Clara,"You can trust your instincts, even in chaos.",green
4,David,"I saw it with my own eyes.",blue
5,Eve,"Lies are just truths with bad timing.",red
6,Frank,"I remember what she said that day.",green
7,Grace,"This clue changes everything.",blue
8,Hector,"He looked me in the eye and lied.",red
9,Ivy,"Don't ignore your gut feeling.",green
10,Jack,"Proof is stronger than memory.",blue
```