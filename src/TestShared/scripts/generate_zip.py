import zipfile

with zipfile.ZipFile('test.zip', 'w') as zf:
    zf.writestr('sample.txt', 'This is text content inside the ZIP file.')
    zf.writestr('data.csv', 'Name,Value\nJohn,100\nJane,200')
    zf.writestr('notes.md', '# Notes\n\nThis is a markdown file inside ZIP.\n\n- Item 1\n- Item 2')
    zf.writestr('subdir/file.txt', 'File in subdirectory')
