import os
import cv2
import numpy as np
from tqdm import tqdm
from matplotlib import pyplot as plt
import pandas as pd
import colour

def compare_img(img1, img2):
    img1_lab = cv2.cvtColor(img1.astype(np.float32) / 255, cv2.COLOR_RGB2HSV)
    img2_lab = cv2.cvtColor(img2.astype(np.float32) / 255, cv2.COLOR_RGB2HSV)
    delta = colour.delta_E(img1_lab, img2_lab)
    return np.mean(delta)

def calculate_distance(i1, i2):
    return np.sum((i1 - i2) ** 2) / 1e8

def cal_feature(aug_path, gaze_path, sal_path, data_path, condition):
    try:
        cnt = len(os.listdir(aug_path))
    except:
        return
    idx = []
    labDelta = []
    area = []
    center_x = []
    center_y = []
    eucDist = []
    for img_index in tqdm(range(1, cnt)):
        try:
            last_aug_img = cv2.imread(os.path.join(aug_path, "frame{}.jpg".format(img_index - 1)))
            aug_img = cv2.imread(os.path.join(aug_path, "frame{}.jpg".format(img_index)))
            gaze_img = cv2.imread(os.path.join(gaze_path, "frame{}.jpg".format(img_index)), cv2.IMREAD_GRAYSCALE).astype(dtype=np.float32)
            sal_img = cv2.imread(os.path.join(sal_path, "%04d.png" % (img_index + 1)), cv2.IMREAD_GRAYSCALE).astype(dtype=np.float32)
        except:
            continue

        aug_gray = cv2.cvtColor(aug_img, cv2.COLOR_RGB2GRAY)
        ret, aug_binary = cv2.threshold(aug_gray, 20, 255, cv2.THRESH_BINARY)
        contours, hierarchy = cv2.findContours(aug_binary, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
        
        last_aug_gray = cv2.cvtColor(last_aug_img, cv2.COLOR_RGB2GRAY)
        ret, last_aug_binary = cv2.threshold(last_aug_gray, 20, 255, cv2.THRESH_BINARY)
        last_contours, last_hierarchy = cv2.findContours(last_aug_binary, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
        
        if len(contours) * len(last_contours) > 0:
            idx.append(img_index)
            labDelta.append(compare_img(aug_img, last_aug_img))
            max_area = -1
            max_c = []
            for c in contours:
                if cv2.contourArea(c) > max_area:
                    max_area = cv2.contourArea(c)
                    max_c = c
            area.append(max_area)
            eucDist.append(calculate_distance(sal_img, aug_gray))
            try:
                moments = cv2.moments(max_c)
                center = (int(moments['m10']/moments['m00']), int(moments['m01']/moments['m00']))
                center_x.append(center[0])
                center_y.append(center[1])
            except:
                # print(center_x[-1])
                # print(center_y[-1])
                center_x.append(max_c[0][0][0])
                center_y.append(max_c[0][0][1])
                # print(max_c[0][0][0], max_c[0][0][1])
    if not os.path.exists(data_path):
        os.makedirs(data_path)
    df = pd.DataFrame({"index": idx, "labDelta": labDelta, "area": area, "center_x": center_x, "center_y": center_y, "eucDist": eucDist})
    df.to_csv(os.path.join(data_path, condition) + ".csv")
    return

if __name__ == "__main__":
    imgs_path = "./formal/imgs"
    saliency_path = "./formal/saliency"
    for user in os.listdir(imgs_path):
        for condition in os.listdir(os.path.join(imgs_path, user)):
            # condition = "hyw_physicalhome2_typing_color"
            print(condition)
            aug_path = os.path.join(imgs_path, user, condition, condition + "_ani.mp4")
            gaze_path = os.path.join(imgs_path, user, condition, condition + "_gaze.mp4")
            sal_path = os.path.join(saliency_path, user, condition + "_all.mp4")
            data_path = os.path.join("./features", user)
            cal_feature(aug_path, gaze_path, sal_path, data_path, condition)
            # break
        # break