import os
import pandas as pd
import numpy as np
import sklearn.metrics as metrics
from matplotlib import pyplot as plt
from random import random
import argparse
from sklearn.model_selection import train_test_split
import random
import tensorflow as tf
from tensorflow.keras.metrics import AUC
import pickle
import visualkeras
print(visualkeras.__file__)

import keras
from keras.models import Sequential
from keras.layers import LSTM
from keras.layers import SimpleRNN
from keras.layers import Dropout
from keras.layers import Dense
from keras.layers import Conv1D, MaxPooling1D
from keras.models import load_model

parser = argparse.ArgumentParser()
parser.add_argument("-u", "--username", action="store")
parser.add_argument("-c", "--command", action="store")
parser.add_argument("-d", "--device", action="store")
parser.add_argument("-a", "--activation", action="store")
parser.add_argument("-i", "--initial", action="store")
args = parser.parse_args()

if args.device == "cpu":
    os.environ["CUDA_VISIBLE_DEVICES"] = "-1"

data_path = "./merge"

n_frames = 5
data_per_condition = 1200
trials = 1

test_user_num = 1

save_files = "./{}_lstm_results_test_emd4.pickle".format(test_user_num)
tprs = []
fprs = []
fout = open(save_files, "wb")
test_user_list = [[i] for i in os.listdir(data_path)]
# test_user_list = [random.sample(os.listdir(data_path), test_user_num) for i in range(24)]

Xs = {}
ys = {}
# fea = ["emd_ani_sal", "labDelta", "area", "center_x", "center_y"]
# fea = ["emd_ani_sal", "labDelta", "area"]
# fea = ["emd_ani_sal", "area"]
fea = ["emd_ani_sal"]

for user in os.listdir(data_path):
    t_x = []
    t_y = []
    temp = []
    for condition in os.listdir(os.path.join(data_path, user)):
        temp_x = []
        temp_y = []
        try:
            df = pd.read_csv(os.path.join(data_path, user, condition))
        except:
            continue
        # df = pd.read_csv("./data/{}/data_{}_{}_{}.csv".format(user, condition.split("_")[1], condition.split("_")[2], user))
        idx = df["index"].tolist()
        X = df[fea].values
        # X = df["emd_ani_gaze"].tolist()
        y = df["label"].tolist()

        if len(X) == 0:
            print(user, condition)
            continue

        for i in range(n_frames, len(idx)):
            if idx[i] - idx[i - n_frames] == n_frames:
                temp_x.append(X[i - n_frames: i])
                temp_y.append(y[i])
                t_x.append(X[i - n_frames: i])
                t_y.append(y[i])
        df = pd.DataFrame({"img": temp_x, "label": temp_y})
        class_0 = df[df['label'] == 0]
        class_1 = df[df['label'] == 1]
        class_count_0, class_count_1 = df['label'].value_counts()
        class_0_resample = class_0.sample(data_per_condition, replace=True)
        class_1_resample = class_1.sample(data_per_condition, replace=True)
        temp.append(class_0)
        temp.append(class_1_resample)

    # df = pd.DataFrame({"img": t_x, "label": t_y})
    # class_0 = df[df['label'] == 0]
    # class_1 = df[df['label'] == 1]
    # class_count_0, class_count_1 = df['label'].value_counts()
    # class_0_resample = class_0.sample(data_per_condition * 12, replace=True)
    # class_1_resample = class_1.sample(data_per_condition * 12, replace=True)

    # test = pd.concat([class_0_resample, class_1_resample], axis=0)
    
    test = pd.concat(temp, axis=0)

    X = test['img'].tolist()
    y = test['label'].tolist()

    X = np.array(X)
    y = np.array(y)

    Xs[user] = X
    ys[user] = y


def test_trials(test_user, trial_number):
    temp_x = []
    temp_y = []
    temp_x_train = []
    temp_y_train = []
    flag = False if trial_number == 0 else True
    trial_cnt = 0
    last_y = 0
    train_set = []
    for user in os.listdir(data_path):
        if user not in test_user:
            continue
        for condition in os.listdir(os.path.join(data_path, user)):
            temp_x_train = []
            temp_y_train = []
            trial_cnt = 0
            last_y = 0
            flag = False if trial_number == 0 else True
            # print(user, condition)
            try:
                df = pd.read_csv(os.path.join(data_path, user, condition))
            except:
                continue
            # df = pd.read_csv("./data/{}/data_{}_{}_{}.csv".format(user, condition.split("_")[1], condition.split("_")[2], user))
            idx = df["index"].tolist()
            X = df[fea].values
            # X = df["emd_ani_sal"].tolist()
            # X = df["emd_ani_gaze"].tolist()
            y = df["label"].tolist()

            if len(X) == 0:
                print(user, condition)
                continue

            for i in range(n_frames, len(idx)):
                if idx[i] - idx[i - n_frames] == n_frames:
                    if flag:
                        temp_x_train.append(X[i - n_frames: i])
                        temp_y_train.append(y[i])
                    else:
                        temp_x.append(X[i - n_frames: i])
                        temp_y.append(y[i])
                    if y[i] == 1 and last_y == 0:
                        trial_cnt += 1
                    if trial_cnt == trial_number:
                        flag = False
                    last_y = y[i]
            
            if len(temp_x_train) > 0:
                df = pd.DataFrame({"img": temp_x_train, "label": temp_y_train})
                class_0 = df[df['label'] == 0]
                class_1 = df[df['label'] == 1]
                class_count_0, class_count_1 = df['label'].value_counts()

                if len(class_0['img'].to_list()) * len(class_1['img'].to_list()) > 0:
                    class_0_resample = class_0.sample(int(data_per_condition / 1), replace=True)
                    class_1_resample = class_1.sample(class_count_0, replace=True)
                    train_set.append(class_0)
                    train_set.append(class_1_resample)
    
    if len(train_set) > 0:
        test = pd.concat(train_set, axis=0)

        return test['img'].tolist(), test['label'].tolist(), temp_x, temp_y
        return temp_x_train, temp_y_train, temp_x, temp_y
    else:
        return temp_x_train, temp_y_train, temp_x, temp_y

if args.command == "train":
    fig = plt.figure(figsize=(12, 6))
    aucs = []
    for test_user in test_user_list:
        # if os.path.isfile("./saved_model/{}_{}_{}.h5".format(test_user, args.activation, args.initial)):
        #     continue
        print(test_user)
        X_train = []
        y_train = []
        X_test = []
        y_test = []
        for user in os.listdir(data_path):
            if user in test_user and user in Xs.keys():
                X_test.extend(Xs[user])
                y_test.extend(ys[user])
                # X_train.extend(Xs[user])
                # y_train.extend(ys[user])
                continue
            elif user in Xs.keys():
                X_train.extend(Xs[user])
                y_train.extend(ys[user])
        
        # train_test_length = int(len(Xs[test_user]) / 10)
        # X_train.extend(Xs[test_user][:train_test_length])
        # y_train.extend(ys[test_user][:train_test_length])
        # X_test.extend(Xs[test_user][train_test_length:])
        # y_test.extend(ys[test_user][train_test_length:])

        if args.initial == "none":
            X_train_temp, y_train_temp, X_test, y_test = test_trials(test_user, 0)
        elif args.initial == "add":
            X_train_temp, y_train_temp, X_test, y_test = test_trials(test_user, trials)
        # X_train.extend(X_test)
        # y_train.extend(y_test)
        # X_test = []
        # y_test = []
        # for i in range(len(Xs[test_user])):
        #     if random() > 0.90:
        #         X_train.append(Xs[test_user][i])
        #         y_train.append(ys[test_user][i])
        #     else:
        #         X_test.append(Xs[test_user][i])
        #         y_test.append(ys[test_user][i])
        
        X_train = np.array(X_train).reshape(-1, n_frames, len(fea))
        y_train = np.array(y_train).reshape(-1, 1, 1)
        # X_train_temp = np.array(X_train_temp).reshape(-1, n_frames, len(fea))
        # y_train_temp = np.array(y_train_temp).reshape(-1, 1, 1)
        X_test = np.array(X_test).reshape(-1, n_frames, len(fea))
        y_test = np.array(y_test).reshape(-1, 1, 1)

        print(X_train.shape)
        print(y_train.shape)
        # print(X_train_temp.shape)
        # print(y_train_temp.shape)
        print(X_test.shape)
        print(y_test.shape)

        X_train, X_val, y_train, y_val = train_test_split(X_train, y_train, test_size=0.2)
        # tempX, X_val, tempy, y_val = train_test_split(X_test, y_test, test_size=0.5)

        model = Sequential()
        model.add(Conv1D(filters=32, kernel_size=3, padding='same', activation=args.activation))
        model.add(MaxPooling1D(pool_size=3, strides=1))
        model.add(LSTM(128, return_sequences=True, input_shape=(X_train.shape[1], X_train.shape[2])))
        model.add(Dropout(0.2))
        model.add(LSTM(32))
        model.add(Dropout(0.2))
        model.add(Dense(1, activation="sigmoid"))
        model.compile(optimizer='adam', loss='mean_squared_error', metrics=["accuracy", "AUC"]) 
        # opt = keras.optimizers.Adam(1e-4)
        # model.compile(optimizer=opt, loss='binary_crossentropy', metrics=["accuracy", "AUC"])
        history = model.fit(X_train, y_train, epochs=30, batch_size=64, validation_data=(X_val, y_val))
        # visualkeras.layered_view(model, to_file="lstm_arch.png", legend=True, show_dimension=True, scale_xy=2, scale_z=2)
        # exit()
        y_pred = model.predict(X_test).ravel()
        
        print(history.history)

        history_file = open("lstm_history.json", "w")

        import json
        json.dump(history.history, history_file)
        
        history_file.close() 
        
        plt.plot(history.history['loss'], label='train')
        plt.plot(history.history['val_loss'], label='test')
        plt.legend()
        plt.show()
        exit()

        y_test = y_test.flatten()
        fpr, tpr, threshold = metrics.roc_curve(y_test, y_pred)

        roc_auc = metrics.auc(fpr, tpr)

        aucs.append(roc_auc)

        print(test_user, roc_auc)

        print("Evaluate on test data")
        results = model.evaluate(X_test, y_test, batch_size=128)
        print("test loss, test acc:", results)

        tprs.append(tpr)
        fprs.append(fpr)
        continue

        # model.save("./lstm_{}_sigmoid.h5".format(n_frames))
        # exit()

        # model.save("./saved_model/{}_{}_{}.h5".format(test_user, args.activation, args.initial))
        # del model
        
        # plt.plot(fpr, tpr, label = '{} AUC = %0.2f'.format(test_user) % roc_auc)
        # # plt.legend(loc = 'lower right', fontsize="small", bbox_to_anchor=(1.2, 0))
        # plt.plot([0, 1], [0, 1],'r--')
        # plt.xlim([0, 1])
        # plt.ylim([0, 1])
        # plt.ylabel('True Positive Rate')
        # plt.xlabel('False Positive Rate')
        # # plt.show()
        # fig.suptitle("Average AUC = %0.2f" % np.mean(np.array(aucs)), fontsize=16)
        # fig.tight_layout()
        # plt.savefig("./lstm_results/leave_{}_users_out_balanced_{}_{}_new_data_merge_{}.jpg".format(test_user_num, args.activation, args.initial, n_frames))
        # plt.show()
pickle.dump({"fpr": fprs, "tpr": tprs}, fout)
fout.close()

if args.command == "test":
    fig = plt.figure(figsize=(12, 6))
    for test_user in os.listdir(data_path):
        model = load_model("./saved_model/{}_{}_{}.h5".format(test_user, args.activation, args.initial))
        X_train_temp, y_train_temp, X_test, y_test = test_trials(test_user, trials)
        X_train_temp = np.array(X_train_temp).reshape(-1, n_frames, 1)
        y_train_temp = np.array(y_train_temp).reshape(-1, 1, 1)
        X_test = np.array(X_test).reshape(-1, n_frames, 1)
        y_test = np.array(y_test).reshape(-1, 1, 1)

        print(X_train_temp.shape)
        print(y_train_temp.shape)
        print(X_test.shape)
        print(y_test.shape)

        y_pred = model.predict(X_test).ravel()
        y_test = y_test.flatten()
        fpr, tpr, threshold = metrics.roc_curve(y_test, y_pred)
        roc_auc = metrics.auc(fpr, tpr)

        print(test_user, roc_auc)

        print("Evaluate on test data")
        results = model.evaluate(X_test, y_test, batch_size=128)
        print("test loss, test acc:", results)
        
        if args.initial == "none":
            X_train_temp, X_val, y_train_temp, y_val = train_test_split(X_train_temp, y_train_temp, test_size=0.2)

            model.fit(X_train_temp, y_train_temp, epochs=5, batch_size=128, validation_data=(X_val, y_val))

            y_pred = model.predict(X_test).ravel()
            y_test = y_test.flatten()
            fpr, tpr, threshold = metrics.roc_curve(y_test, y_pred)
            roc_auc = metrics.auc(fpr, tpr)

            print(test_user, roc_auc)

            print("Evaluate on test data")
            results = model.evaluate(X_test, y_test, batch_size=128)
            print("test loss, test acc:", results)
        
        plt.plot(fpr, tpr, label = '{} AUC = %0.2f'.format(test_user) % roc_auc)
        plt.legend(loc = 'lower right', fontsize="small", bbox_to_anchor=(1.2, 0))
        plt.plot([0, 1], [0, 1],'r--')
        plt.xlim([0, 1])
        plt.ylim([0, 1])
        plt.ylabel('True Positive Rate')
        plt.xlabel('False Positive Rate')
        # plt.show()
    fig.tight_layout()
    plt.savefig("./lstm_results/leave_{}_trials_out_balanced_{}_{}_{}_test.jpg".format(trials, args.activation, args.initial, test_user))
        # plt.show()


# import visualkeras
# from PIL import ImageFont
# font = ImageFont.truetype("arial.ttf", 8)  # using comic sans is strictly prohibited!
# visualkeras.layered_view(model, to_file='lstm.png', legend=True, font=font).show() # write and show
    