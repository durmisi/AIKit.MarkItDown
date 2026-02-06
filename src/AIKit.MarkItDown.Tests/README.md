# AIKit.MarkItDown Tests

## Structure

- `files/` - Test input files for various formats
- `scripts/` - Python scripts to generate test files
- `*.cs` - Test classes

## Test Files

The `files/` directory contains sample files for testing conversions:

- `pdf-test.pdf` - Existing PDF for basic tests
- `tst-text.txt` - Existing text file
- `test.docx` - Generated Word document with headings, lists, tables
- `test.xlsx` - Generated Excel with multiple sheets and data
- `test.pptx` - Generated PowerPoint with slides and tables
- `test.jpg` - Generated image with text overlay
- `test.wav` - Generated audio file
- `test.zip` - Generated archive with multiple files
- `test.epub` - Generated eBook with chapters
- `test.csv` - Generated CSV data
- `test.ipynb` - Generated Jupyter notebook
- `test.html` - Generated HTML page

## Generating Test Files

Run the scripts in `scripts/` to regenerate test files:

```bash
cd scripts
python generate_*.py
```

Or run all:

```bash
for script in generate_*.py; do python $script; done
```

## Running Tests

```bash
dotnet test
```

Individual format tests: `dotnet test --filter "TestConvertDocx"`
