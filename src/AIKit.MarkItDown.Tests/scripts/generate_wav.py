from pydub import AudioSegment
from pydub.generators import Sine

# Create a 5-second audio with a simple tone
tone = Sine(440).to_audio_segment(duration=5000)  # 440 Hz for 5 seconds
tone.export('test.wav', format='wav')
