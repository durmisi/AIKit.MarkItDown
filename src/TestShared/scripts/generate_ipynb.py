import nbformat as nbf

nb = nbf.v4.new_notebook()
nb.cells = [
    nbf.v4.new_markdown_cell('# Sample Jupyter Notebook\n\nThis notebook demonstrates various cell types for testing Markdown conversion.'),
    nbf.v4.new_code_cell('print("Hello, World!")'),
    nbf.v4.new_markdown_cell('## Data Analysis\n\nHere is some sample code:'),
    nbf.v4.new_code_cell('import pandas as pd\ndata = {"Name": ["John", "Jane"], "Age": [30, 25]}\ndf = pd.DataFrame(data)\nprint(df)'),
    nbf.v4.new_markdown_cell('### Results\n\nThe code above creates a DataFrame and prints it.'),
    nbf.v4.new_code_cell('# This is a comment\nx = 5\ny = 10\nprint(f"Sum: {x + y}")'),
    nbf.v4.new_markdown_cell('## Conclusion\n\nThis notebook tests notebook-to-Markdown conversion.')
]

nbf.write(nb, 'test.ipynb')
