import sys

def create_network_main(flag, file_path_for_mp3, file_path_for_wav, genres, count_epochs):
    import io
    import os
    import numpy
    import fnmatch
    import tensorflow as tf
    from tensorflow import keras
    import librosa
    from matplotlib import pyplot
    import soundfile as sf
    from tensorflow.python.framework.convert_to_constants import convert_variables_to_constants_v2
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
        file = file[:file.rfind(".")]
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
    def processing_mp3(file_path_for_mp3, file_path_for_wav, genres):
        file_path_buf = os.path.join(os.path.dirname(__file__), "..", "bin", "Debug", "net6.0-windows", "data", "buf_mp3_30s")
        
        for genre in genres:
            #выбор всех файлов mp3 по жанру 
            files = os.listdir(os.path.join(file_path_for_mp3, genre))
            files = fnmatch.filter(files, "*.mp3")
            for file in files:
                #поверка существования первого деления в wav
                check_filepath = os.path.join(file_path_for_wav, file + "_30s_1.mp3")
                if not os.path.exists(check_filepath):
                    #деление mp3 на части по 30s
                    mp3_file_path = os.path.join(file_path_for_mp3, genre, file)
                    split_mp3_by_30(file,mp3_file_path,file_path_buf)
                    files_30 = os.listdir(file_path_buf)
                    files_30 = fnmatch.filter(files_30, "*.mp3")
                    for file_30 in files_30:
                        filepath_30 = os.path.join(file_path_buf, file_30)
                        file_30_name = file_30[:file_30.rfind(".")]
                        wav_file_path = os.path.join(file_path_for_wav, genre, file_30_name+".wav")
                        #проверка существования каждого деления в wav
                        if not os.path.exists(wav_file_path):
                            #конвертация
                            convert_mp3_to_wav(filepath_30, wav_file_path)
                        #удаление деления из временной папки
                        os.remove(filepath_30)
        return
    #flag = true если дано file_path_for_mp3, иначе все wav файлы должны быть заполнены
    def create_network(flag, file_path_for_mp3, file_path_for_wav, genres, count_epochs):
        result = ""
        if not flag: processing_mp3(file_path_for_mp3, file_path_for_wav, genres)

        features = []
        labels = []
        songs_count = 0
        for genre in genres:
            print("Calculating features for genre : " + genre)
            result += "Calculating features for genre : " + genre + "\n"

            files = os.listdir(os.path.join(file_path_for_wav, genre))
            files = fnmatch.filter(files, "*.wav")
            for file in files:
                file_path = os.path.join(file_path_for_wav, genre, file)
                features.append(get_feature(file_path))
                label = genres.index(genre)
                labels.append(label)
                songs_count = songs_count+1
        # Shuffle the data
        permutations = numpy.random.permutation(songs_count)#количество песен на вход
        features = numpy.array(features)[permutations]
        labels = numpy.array(labels)[permutations]

        songs_count2 = round(songs_count * 0.2) #20% songs_count
        songs_count1 = songs_count - songs_count2*2 #60% songs_count
        songs_count2 = songs_count1 + songs_count2 #граница

        features_train = features[0:songs_count1]
        labels_train = labels[0:songs_count1]

        features_val = features[songs_count1:songs_count2]
        labels_val = labels[songs_count1:songs_count2]

        features_test = features[songs_count2:songs_count]
        labels_test = labels[songs_count2:songs_count]

        inputs = keras.Input(shape=(498), name="feature")
        x = keras.layers.Dense(300, activation="relu", name="dense_1")(inputs)
        x = keras.layers.Dense(200, activation="relu", name="dense_2")(x)
        outputs = keras.layers.Dense(len(genres), activation="softmax", name="predictions")(x)#len(genres) - количество жанров
        
        model = keras.Model(inputs=inputs, outputs=outputs)

        # Перехватываем стандартный вывод Python в буфер
        buffer = io.StringIO()
        sys.stdout = buffer

        model.compile(
        # Optimizer
        optimizer=keras.optimizers.RMSprop(),
        # Loss function to minimize
        loss=keras.losses.SparseCategoricalCrossentropy(),
        # List of metrics to monitor
        metrics=[keras.metrics.SparseCategoricalAccuracy()],
        )
        model.fit(x=features_train.tolist(),y=labels_train.tolist(),verbose=1,validation_data=(features_val.tolist() , labels_val.tolist()), epochs=count_epochs)
        score = model.evaluate(x=features_test.tolist(),y=labels_test.tolist(), verbose=0)
        # Добавляем содержимое буфера к result
        result += buffer.getvalue()
        # Восстанавливаем стандартный вывод Python
        sys.stdout = sys.__stdout__

        print('Accuracy : ' + str(score[1]*100) + '%')
        result += 'Accuracy : ' + str(score[1]*100) + '%\n'

        file_path_models = os.path.join(os.path.dirname(__file__), "..", "bin", "Debug", "net6.0-windows", "models")
        folder_list = os.listdir(file_path_models)
        #Фильтруем список по префиксу tensorflow_keras_spectrograms_genre
        folder_list = [f for f in folder_list if f.startswith("tensorflow_keras_spectrograms_genre")]
        folder_count = len([f for f in folder_list if os.path.isdir(f)])
        file_count = len(fnmatch.filter(folder_list, "*.h5"))
        count = max(folder_count, file_count)
        new_model_name = "tensorflow_keras_spectrograms_genre_" + str(count)
        result = new_model_name + ";" + result

        file_path_model = os.path.join(file_path_models, new_model_name)
        file_path_model_h5 = os.path.join(file_path_models, new_model_name + '.h5')

        #os.makedirs('file_path_model', exist_ok=True)

        model.save(file_path_model)
        model.save(file_path_model_h5, save_format='h5')

        return result

    try:
        flag = bool (flag)
        file_path_for_mp3 = str (file_path_for_mp3)
        file_path_for_wav = str (file_path_for_wav)
        genres = list (genres.split (","))
        count_epochs = int (count_epochs)

        result = create_network(flag, file_path_for_mp3, file_path_for_wav, genres, count_epochs)
        return result
    except Exception as e:
        return "Error: " + str(e)

if __name__ == "__main__":
    try:
        args = sys.argv[1:]
        result = create_network_main(*args)
        print(result)
    except Exception as e:
        print("An error occurred: ", e)     

#Test
#create_network_main(True, r"G:\My\Proga\MusicSearch\MusicSearch\MusicSearch\bin\Debug\net6.0-windows\data\songs_for_create_network", r"G:\My\Proga\MusicSearch\MusicSearch\MusicSearch\bin\Debug\net6.0-windows\data\songs_for_create_network_wav", ["Рок", "Метал", "Поп", "Джаз", "Блюз", "Ритм-н-блюз", "Хип-хоп", "Электронная", "Классика", "Народная"], 100)