from sklearn import svm
import pandas as pd
import os
import sklearn.metrics as metrics
from matplotlib import pyplot as plt
from sklearn.linear_model import LogisticRegression
from sklearn.model_selection import train_test_split
import numpy as np
from sklearn.ensemble import AdaBoostClassifier, BaggingClassifier, GradientBoostingClassifier, RandomForestClassifier
from sklearn.neighbors import KNeighborsClassifier
from sklearn.ensemble import GradientBoostingClassifier
import random
import pickle

data_path = "./merge"
data_per_user = 1200

fig = plt.figure(figsize=(12, 6))

test_user_num = 1

save_files = "./{}_xgboost_emd_delta_results.pickle".format(test_user_num)
tprs = []
fprs = []
fout = open(save_files, "wb")
test_user_list = [[i] for i in os.listdir(data_path)]
# test_user_list = [random.sample(os.listdir(data_path), test_user_num) for i in range(24)]

aucs = []
for test_user in test_user_list:
    print(test_user)
    files = []
    test_files = []
    for user in os.listdir(data_path):
        if user in test_user:
            for condition in os.listdir(os.path.join(data_path, user)):
                try:
                    test_files.append(pd.read_csv(os.path.join(data_path, user, condition)))
                except:
                    continue
        else:
            for condition in os.listdir(os.path.join(data_path, user)):
                try:
                    files.append(pd.read_csv(os.path.join(data_path, user, condition)))
                except:
                    continue

    df = pd.concat(files)
    test_df = pd.concat(test_files)

    class_0 = df[df['label'] == 0]
    class_1 = df[df['label'] == 1]
    class_count_0, class_count_1 = df['label'].value_counts()
    class_0_over = class_0.sample(class_count_1, replace=True)
    class_1_over = class_1.sample(class_count_0, replace=True)

    # test = pd.concat([class_0, class_1_over], axis=0)
    balanced_df = pd.concat([class_0, class_1_over], axis=0)

    # fea = ["emd_ani_sal", "labDelta", "area", "center_x", "center_y"]
    # fea = ["labDelta", "area", "center_x", "center_y"]
    fea = ["emd_ani_sal", "labDelta"]
    # fea = ["emd_ani_sal"]

    X_train = balanced_df[fea]
    # X_train = test['emd_ani_gaze']
    y_train = balanced_df['label']
    y_train = y_train.astype('int')
    X_test = test_df[fea]
    # X_test = test_df['emd_ani_gaze']
    y_test = test_df['label']
    y_test = y_test.astype('int')
    X_train = X_train.values.reshape(-1, len(fea))
    X_test = X_test.values.reshape(-1, len(fea))
    y_train = y_train.values.reshape(-1, 1)
    y_test = y_test.values.reshape(-1, 1)

    # X_train, X_test, y_train, y_test = train_test_split(X_train, y_train, test_size=0.1)

    print(X_train.shape)
    print(y_train.shape)
    print(X_test.shape)
    print(y_test.shape)

    # clf = LogisticRegression()
    # clf = svm.SVC(probability=True)
    # clf = AdaBoostClassifier(n_estimators=100, random_state=0)
    # clf = RandomForestClassifier(n_estimators=100)
    clf = GradientBoostingClassifier(n_estimators=100, learning_rate=0.1, max_depth=1, random_state=0)
    clf.fit(X_train, y_train)
    print(clf.score(X_test, y_test))

    # y_pred = []
    # last_preds = []
    # for i in range(len(X_test)):
    #     cur_pred = clf.predict_proba([X_test[i]])[:, 1]
    #     # print(last_pred, cur_pred, last_pred * cur_pred)
    #     last_preds.append(cur_pred)
    #     if len(last_preds) > 5:
    #         last_preds.pop(0)
    #     y_pred.append(np.mean(np.array(last_preds)))
    #     # y_pred.append((cur_pred + cur_pred) / 2)
    #     # y_pred.append(last_pred * cur_pred)
    #     # last_pred = cur_pred

    probs = clf.predict_proba(X_test)
    preds = probs[:, 1]
    y_pred = []
    for pred_index in range(len(preds)):
        # print(pred_index, np.mean(np.array(preds[pred_index - 5 : pred_index] if pred_index > 4 else preds[: pred_index])))
        y_pred.append(np.mean(np.array(preds[pred_index - 2 : pred_index + 1] if pred_index > 2 else preds[: pred_index + 1])))
        # temp = np.sum(np.array(preds[pred_index - 4 : pred_index + 1] if pred_index > 1 else preds[: pred_index + 1]))
        # y_pred.append(1 if temp > 4 else 0)
    fpr, tpr, threshold = metrics.roc_curve(y_test, preds)
    roc_auc = metrics.auc(fpr, tpr)
    print(roc_auc)
    # fpr, tpr, threshold = metrics.roc_curve(y_test, y_pred)
    # roc_auc = metrics.auc(fpr, tpr)
    # print(roc_auc)
    aucs.append(roc_auc)

    fprs.append(fpr)
    tprs.append(tpr)
    continue

    # plt.plot(fpr, tpr, label = 'AUC = %0.2f' % roc_auc)
    # # plt.legend(loc = 'lower right', fontsize="small", bbox_to_anchor=(1.2, 0))
    # plt.plot([0, 1], [0, 1],'r--')
    # plt.xlim([0, 1])
    # plt.ylim([0, 1])
    # plt.ylabel('True Positive Rate')
    # plt.xlabel('False Positive Rate')
    # fig.suptitle("Average AUC = %0.2f" % np.mean(np.array(aucs)), fontsize=16)
    # fig.tight_layout()
    # # plt.show()
    # plt.savefig("./adaboost_smooth_pred_24_users_leave_{}_user_out_temp.jpg".format(test_user_num))
    # break
pickle.dump({"fpr": fprs, "tpr": tprs}, fout)
fout.close()