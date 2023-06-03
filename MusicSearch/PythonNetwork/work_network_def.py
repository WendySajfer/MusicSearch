import sys

def search_genre_main(file_path_for_model, file_path_for_mp3, genres):
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
    def get_mfcc(wav_file_path):
        y, sr = librosa.load(wav_file_path, offset=0, duration=30)
        mfcc = numpy.array(librosa.feature.mfcc(y=y, sr=sr))
        return mfcc
    def get_melspectrogram(wav_file_path):
        y, sr = librosa.load(wav_file_path, offset=0, duration=30)
        melspectrogram = numpy.array(librosa.feature.melspectrogram(y=y, sr=sr))
        return melspectrogram
    def get_chroma_vector(wav_file_path):
        y, sr = librosa.load(wav_file_path)
        chroma = numpy.array(librosa.feature.chroma_stft(y=y, sr=sr))
        return chroma
    def get_tonnetz(wav_file_path):
        y, sr = librosa.load(wav_file_path)
        tonnetz = numpy.array(librosa.feature.tonnetz(y=y, sr=sr))
        return tonnetz
    def get_feature(file_path):
        # Extracting MFCC feature
        mfcc = get_mfcc(file_path)
        mfcc_mean = mfcc.mean(axis=1)
        mfcc_min = mfcc.min(axis=1)
        mfcc_max = mfcc.max(axis=1)
        mfcc_feature = numpy.concatenate( (mfcc_mean, mfcc_min, mfcc_max) )

        # Extracting Mel Spectrogram feature
        melspectrogram = get_melspectrogram(file_path)
        melspectrogram_mean = melspectrogram.mean(axis=1)
        melspectrogram_min = melspectrogram.min(axis=1)
        melspectrogram_max = melspectrogram.max(axis=1)
        melspectrogram_feature = numpy.concatenate( (melspectrogram_mean, melspectrogram_min, melspectrogram_max) )

        # Extracting chroma vector feature
        chroma = get_chroma_vector(file_path)
        chroma_mean = chroma.mean(axis=1)
        chroma_min = chroma.min(axis=1)
        chroma_max = chroma.max(axis=1)
        chroma_feature = numpy.concatenate( (chroma_mean, chroma_min, chroma_max) )

        # Extracting tonnetz feature
        tntz = get_tonnetz(file_path)
        tntz_mean = tntz.mean(axis=1)
        tntz_min = tntz.min(axis=1)
        tntz_max = tntz.max(axis=1)
        tntz_feature = numpy.concatenate( (tntz_mean, tntz_min, tntz_max) ) 

        feature = numpy.concatenate( (chroma_feature, melspectrogram_feature, mfcc_feature, tntz_feature) )
        return feature

    def split_mp3_by_30(file, file_path_for_mp3, file_path_buf):
        segment_length = 30
        # Открываем файл mp3
        data, sr = sf.read(file_path_for_mp3)
        duration = len(data) / sr
        splits = int(duration // 30)
        offset = int((duration - splits * 30) / 2)
        # Разбить аудиофайл на части по 30 секунд и сохранить их с новыми именами во временную папку
        for i in range(splits):
            start = (offset + i * 30) * sr
            end = start + 30 * sr
            segment = data[start:end]
            sf.write (os.path.join(file_path_buf, file + "_30s_" + str(i + 1) + ".mp3"), segment, sr, format="mp3")       
    #обработка в wav файлы
    def processing_song_mp3(file_name, file_path_for_mp3, file_path_for_wav):
        file_path_buf = os.path.join(os.path.dirname(__file__), "..", "bin", "Debug", "net6.0-windows", "data", "buf_mp3_30s")

        #деление mp3 на части по 30s
        split_mp3_by_30(file_name,file_path_for_mp3,file_path_buf)
        files_30 = os.listdir(file_path_buf)
        files_30 = fnmatch.filter(files_30,file_name + "*.mp3")
        for file_30 in files_30:
            filepath_30 = os.path.join(file_path_buf, file_30)
            file_30_name = file_30[:file_30.rfind(".")]
            wav_file_path = os.path.join(file_path_for_wav, file_30_name+".wav")
            #конвертация
            convert_mp3_to_wav(filepath_30, wav_file_path)
            #удаление деления из временной папки
            os.remove(filepath_30)
        return

    def search_genre(file_path_for_model, file_path_for_mp3, genres):
        result = ""

        file_name = os.path.basename(file_path_for_mp3)
        file_name = os.path.splitext(file_name)[0]

        file_path_buf_wav = os.path.join(os.path.dirname(__file__), "..", "bin", "Debug", "net6.0-windows", "data", "buf_wav")
        processing_song_mp3(file_name, file_path_for_mp3, file_path_buf_wav)

        model = tf.keras.models.load_model(file_path_for_model)

        y=[]
        files = os.listdir(file_path_buf_wav)
        files = fnmatch.filter(files,file_name + "*.wav")

        part = 1
        for file in files:
            file_path = os.path.join(file_path_buf_wav,file)
            feature = get_feature(file_path)
            prediction = model.predict(feature.reshape(1,498))
            y.append(prediction)
            max_index = numpy.argmax(prediction)
            max_genre = genres[max_index]

            result_stat = []
            for i in range(len(genres)):
                result_stat.append((genres[i], prediction[0][i]))

            result += f"Часть {part}\nЖанр: {max_genre}\nСтатистика: {str(result_stat)}\n"
            part += 1
            #удаление wav файлов из временной папки
            os.remove(file_path)

        y = numpy.array(y)
        # Считаем среднее по всем предсказаниям по жанрам
        mean_prediction = numpy.mean(y, axis=0)
        max_index = numpy.argmax(mean_prediction)
        max_genre = genres[max_index]

        result_stat = []
        for i in range(len(genres)):
            result_stat.append((genres[i], mean_prediction[0][i]))

        result = f"Общий результат.\nЖанр: {max_genre}\nСтатистика: {str(result_stat)}\n" + result
        
        data = []
        data.append([file_name] + list(mean_prediction[0]))
        for i in range(len(y)):
            data.append([f"Часть {i+1}"] + list(y[i][0]))
        df = pd.DataFrame(data, columns=["Название"] + genres)
        df.to_excel("result.xlsx", index=False)

        return result

    try:
        # Convert the arguments to the desired types if needed
        file_path_for_model = str (file_path_for_model)
        file_path_for_mp3 = str (file_path_for_mp3)
        genres = list (genres.split (","))

        result = search_genre(file_path_for_model, file_path_for_mp3, genres)
        return result
    except Exception as e:
        result = "Error: " + str(e)  
        return result

if __name__ == "__main__":
    try:
        args = sys.argv[1:]
        result = search_genre_main(*args)
        print(result)
    except Exception as e:
        print("An error occurred: ", e) 

#Test
#answer = search_genre_main(r"G:\My\Proga\MusicSearch\MusicSearch\MusicSearch\bin\Debug\net6.0-windows\models\tensorflow_keras_spectrograms_genre_0",r"G:\My\Proga\MusicSearch\MusicSearch\MusicSearch\bin\Debug\net6.0-windows\data\songs_for_test\Like That.mp3", ["Рок", "Метал", "Поп", "Джаз", "Блюз", "Ритм-н-блюз", "Хип-хоп", "Электронная", "Классика", "Народная"])
#print(answer)