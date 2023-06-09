import sys

def translate_spectro_main(file_path_for_mp3):
    import io
    import os
    import numpy
    import fnmatch
    import tensorflow as tf
    from tensorflow import keras
    import librosa
    from matplotlib import pyplot
    import soundfile as sf
    import pandas as pd

    def convert_mp3_to_wav(mp3_file_path, wav_file_path):
        data, samplerate = sf.read(mp3_file_path)
        sf.write(wav_file_path, data, samplerate)
        return wav_file_path
    def get_mfcc(file, wav_file_path, spectro_filepath):
        y, sr = librosa.load(wav_file_path, offset=0, duration=30)
        mfcc = numpy.array(librosa.feature.mfcc(y=y, sr=sr))

        pyplot.imshow(mfcc, interpolation='nearest', aspect='auto')
        file_name = os.path.splitext(file)[0]
        jpg_file_path = os.path.join(spectro_filepath, file_name + "_1.svg")
        pyplot.savefig(jpg_file_path, format="svg")
        pyplot.close()
        return mfcc
    def get_melspectrogram(file, wav_file_path, spectro_filepath):
        y, sr = librosa.load(wav_file_path, offset=0, duration=30)
        melspectrogram = numpy.array(librosa.feature.melspectrogram(y=y, sr=sr))
        
        pyplot.imshow(melspectrogram, interpolation='nearest', aspect='auto')
        file_name = os.path.splitext(file)[0]
        jpg_file_path = os.path.join(spectro_filepath, file_name + "_2.svg")
        pyplot.savefig(jpg_file_path, format="svg")
        pyplot.close()
        return melspectrogram
    def get_chroma_vector(file, wav_file_path, spectro_filepath):
        y, sr = librosa.load(wav_file_path)
        chroma = numpy.array(librosa.feature.chroma_stft(y=y, sr=sr))
        
        pyplot.imshow(chroma, interpolation='nearest', aspect='auto')
        file_name = os.path.splitext(file)[0]
        jpg_file_path = os.path.join(spectro_filepath, file_name + "_3.svg")
        pyplot.savefig(jpg_file_path, format="svg")
        pyplot.close()
        return chroma
    def get_tonnetz(file, wav_file_path, spectro_filepath):
        y, sr = librosa.load(wav_file_path)
        tonnetz = numpy.array(librosa.feature.tonnetz(y=y, sr=sr))
        
        pyplot.imshow(tonnetz, interpolation='nearest', aspect='auto')
        file_name = os.path.splitext(file)[0]
        jpg_file_path = os.path.join(spectro_filepath, file_name + "_4.svg")
        pyplot.savefig(jpg_file_path, format="svg")
        pyplot.close()
        return tonnetz    
    #обработка в wav файлы
    def processing_mp3(file_path_for_mp3, file_path_for_wav):
        files = os.listdir(file_path_for_mp3)
        files = fnmatch.filter(files, "*.mp3")
        for file in files:
            file_name = file[:file.rfind(".")]
            mp3_file_path = os.path.join(file_path_for_mp3, file_name+".mp3")
            wav_file_path = os.path.join(file_path_for_wav, file_name+".wav")
            convert_mp3_to_wav(mp3_file_path, wav_file_path)
    def translate_spectro(file_path_for_mp3):
        result = ""

        file_path_buf_wav = os.path.join(os.path.dirname(__file__), "..", "bin", "Debug", "net6.0-windows", "data", "buf_wav")
        file_path_spectro = os.path.join(os.path.dirname(__file__), "..", "bin", "Debug", "net6.0-windows", "data", "spectro_view")
        processing_mp3(file_path_for_mp3, file_path_buf_wav)

        y=[]
        files = os.listdir(file_path_buf_wav)
        files = fnmatch.filter(files, "*.wav")

        for file in files:
            file_path = os.path.join(file_path_buf_wav,file)

            get_mfcc(file, file_path, file_path_spectro)
            get_melspectrogram(file, file_path, file_path_spectro)
            get_chroma_vector(file, file_path, file_path_spectro)
            get_tonnetz(file, file_path, file_path_spectro)

            os.remove(file_path)

        return result

    try:
        # Convert the arguments to the desired types if needed
        file_path_for_mp3 = str (file_path_for_mp3)

        result = translate_spectro(file_path_for_mp3)
        return result
    except Exception as e:
        result = "Error: " + str(e)  
        return result

#Test
answer = translate_spectro_main(r"G:\My\Proga\MusicSearch\MusicSearch\MusicSearch\bin\Debug\net6.0-windows\data\songs_for_test")
print(answer)