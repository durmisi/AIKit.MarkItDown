from ebooklib import epub

book = epub.EpubBook()
book.set_title('Sample EPUB Book')
book.set_language('en')
book.add_author('Test Author')

# Chapter 1
chapter1 = epub.EpubHtml(title='Introduction', file_name='chap1.xhtml', lang='en')
chapter1.content = '<h1>Introduction</h1><p>This is the introduction to the sample EPUB book.</p><p>It contains multiple chapters to test conversion.</p>'

# Chapter 2
chapter2 = epub.EpubHtml(title='Chapter 1: The Beginning', file_name='chap2.xhtml', lang='en')
chapter2.content = '<h1>Chapter 1: The Beginning</h1><p>This is the first chapter.</p><ul><li>Point 1</li><li>Point 2</li></ul>'

# Chapter 3
chapter3 = epub.EpubHtml(title='Chapter 2: The Middle', file_name='chap3.xhtml', lang='en')
chapter3.content = '<h1>Chapter 2: The Middle</h1><p>This is the second chapter.</p><table border="1"><tr><th>Name</th><th>Value</th></tr><tr><td>Item 1</td><td>100</td></tr></table>'

# Chapter 4
chapter4 = epub.EpubHtml(title='Conclusion', file_name='chap4.xhtml', lang='en')
chapter4.content = '<h1>Conclusion</h1><p>This concludes the sample EPUB book.</p>'

book.add_item(chapter1)
book.add_item(chapter2)
book.add_item(chapter3)
book.add_item(chapter4)

book.toc = (
    epub.Link('chap1.xhtml', 'Introduction', 'intro'),
    epub.Link('chap2.xhtml', 'Chapter 1', 'chap1'),
    epub.Link('chap3.xhtml', 'Chapter 2', 'chap2'),
    epub.Link('chap4.xhtml', 'Conclusion', 'conclusion')
)

book.add_item(epub.EpubNcx())
book.add_item(epub.EpubNav())

book.spine = ['nav', chapter1, chapter2, chapter3, chapter4]

epub.write_epub('test.epub', book)
