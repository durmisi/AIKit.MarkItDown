from PIL import Image, ImageDraw, ImageFont

# Create image with text
img = Image.new('RGB', (400, 300), color='lightblue')
draw = ImageDraw.Draw(img)

# Try to use a font, fallback to default
try:
    font = ImageFont.truetype("arial.ttf", 20)
except:
    font = ImageFont.load_default()

draw.text((50, 50), "Sample Image", fill="black", font=font)
draw.text((50, 100), "This is a test image", fill="black", font=font)
draw.text((50, 150), "for Markdown conversion", fill="black", font=font)
draw.text((50, 200), "with OCR capabilities", fill="black", font=font)

img.save('test.jpg')
