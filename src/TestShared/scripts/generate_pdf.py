from reportlab.pdfgen import canvas
from reportlab.lib.pagesizes import letter
from reportlab.lib.styles import getSampleStyleSheet
from reportlab.platypus import SimpleDocTemplate, Paragraph, Spacer, Table, TableStyle
from reportlab.lib import colors

# Create PDF with more content
doc = SimpleDocTemplate('test.pdf', pagesize=letter)
styles = getSampleStyleSheet()

story = []

# Title
story.append(Paragraph("Sample PDF Document", styles['Title']))
story.append(Spacer(1, 12))

# Introduction
story.append(Paragraph("This is a sample PDF document created with ReportLab to test Markdown conversion.", styles['Normal']))
story.append(Spacer(1, 12))

# Section
story.append(Paragraph("Features", styles['Heading2']))
story.append(Paragraph("This document includes various elements:", styles['Normal']))
story.append(Paragraph("• Text paragraphs", styles['Normal']))
story.append(Paragraph("• Headings", styles['Normal']))
story.append(Paragraph("• Tables", styles['Normal']))
story.append(Spacer(1, 12))

# Table
data = [
    ['Name', 'Age', 'City'],
    ['John', '30', 'New York'],
    ['Jane', '25', 'London'],
    ['Bob', '35', 'Paris']
]

table = Table(data)
table.setStyle(TableStyle([
    ('BACKGROUND', (0, 0), (-1, 0), colors.grey),
    ('TEXTCOLOR', (0, 0), (-1, 0), colors.whitesmoke),
    ('ALIGN', (0, 0), (-1, -1), 'CENTER'),
    ('FONTNAME', (0, 0), (-1, 0), 'Helvetica-Bold'),
    ('FONTSIZE', (0, 0), (-1, 0), 14),
    ('BOTTOMPADDING', (0, 0), (-1, 0), 12),
    ('BACKGROUND', (0, 1), (-1, -1), colors.beige),
    ('GRID', (0, 0), (-1, -1), 1, colors.black)
]))

story.append(table)
story.append(Spacer(1, 12))

# Conclusion
story.append(Paragraph("Conclusion", styles['Heading2']))
story.append(Paragraph("This concludes the sample PDF document for testing purposes.", styles['Normal']))

doc.build(story)
