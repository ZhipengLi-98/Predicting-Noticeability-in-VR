import os
import cv2
import numpy as np
from tqdm import tqdm
from matplotlib import pyplot as plt
from scipy.stats import wasserstein_distance
import threading
from scipy.ndimage import gaussian_filter
import pandas as pd
# import colour

def get_signature_from_heatmap(hm):
    nr = hm.shape[0]
    nc = hm.shape[1]
    # print hm

    sig = np.zeros((nr * nc, 3), dtype=np.float32)
    for r in range(nr):
        for c in range(nc):
            idx = r * nc + c
            sig[idx, 0] = max(hm[r, c], 0)
            sig[idx, 1] = r
            sig[idx, 2] = c

    sum_hm = np.sum(sig[:, 0])

    if sum_hm < 0.0001:
        return None

    sig[:, 0] /= np.sum(sig[:, 0])
    # print sig
    return sig

# def compare_img(img1, img2):
#     img1_lab = cv2.cvtColor(img1.astype(np.float32) / 255, cv2.COLOR_RGB2HSV)
#     img2_lab = cv2.cvtColor(img2.astype(np.float32) / 255, cv2.COLOR_RGB2HSV)
#     delta = colour.delta_E(img1_lab, img2_lab)
#     return np.mean(delta)

def determine_img(img):
    img_hsv = cv2.cvtColor(img.astype(np.float32) / 255, cv2.COLOR_RGB2HSV)
    return np.mean(img_hsv[:, :, 0]), np.mean(img_hsv[:, :, 2])

def cal_emd(aug_path, gaze_path, sal_path, data_path, condition, latency):
    delay = int(latency / 1000 * 30)
    try:
        cnt = len(os.listdir(aug_path))
    except:
        return
    idx = []
    emd_ags = []
    emd_ass = []
    labels = []
    temp_idx = []
    temp_emd_ags = []
    temp_emd_ass = []
    one_cnt = 0
    last_contours_cnt = 0
    last_one_index = -1
    for img_index in tqdm(range(1, cnt)):
        img_index = 448
        try:
            last_aug_img = cv2.imread(os.path.join(aug_path, "frame{}.jpg".format(img_index - 1)))
            aug_img = cv2.imread(os.path.join(aug_path, "frame{}.jpg".format(img_index)))
            gaze_img = cv2.imread(os.path.join(gaze_path, "frame{}.jpg".format(img_index)), cv2.IMREAD_GRAYSCALE).astype(dtype=np.float32)
            sal_img = cv2.imread(os.path.join(sal_path, "%04d.png" % (img_index + 1)), cv2.IMREAD_GRAYSCALE).astype(dtype=np.float32)
        except:
            continue
        
        label = 0
        aug_gray = cv2.cvtColor(aug_img, cv2.COLOR_RGB2GRAY)
        ret, aug_binary = cv2.threshold(aug_gray, 20, 255, cv2.THRESH_BINARY)
        contours, hierarchy = cv2.findContours(aug_binary, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
        innerpoints = []
        outline = []
        if len(contours) == 0:
            # no augmentation
            last_contours_cnt = len(contours)
            continue
        else:
            if last_contours_cnt > 0:
                h0, v0 = determine_img(last_aug_img)
                h1, v1 = determine_img(aug_img)
                # print(img_index, determine_img(aug_img), determine_img(last_aug_img))
                if h1 < 2.5 and h0 - h1 > 0.25 and v1 / v0 < 0.7:
                    print(img_index, determine_img(aug_img), determine_img(last_aug_img))
                    if len(temp_idx) > delay:
                        one_cnt += 1
                        idx.extend(temp_idx[: len(temp_idx) - delay + 1])
                        emd_ass.extend(temp_emd_ass[: len(temp_idx) - delay + 1])
                        emd_ags.extend(temp_emd_ags[: len(temp_idx) - delay + 1])
                        labels.extend([0 for temp_i in range(len(temp_idx) - delay)])
                        labels.extend([1])
                        last_one_index = idx[-1]
                    temp_idx.clear()
                    temp_emd_ass.clear()
                    temp_emd_ags.clear()
                    continue
                elif h1 < 2.5 and v1 / v0 < 0.5:
                    print(img_index, determine_img(aug_img), determine_img(last_aug_img))
                    if len(temp_idx) > delay:
                        one_cnt += 1
                        idx.extend(temp_idx[: len(temp_idx) - delay + 1])
                        emd_ass.extend(temp_emd_ass[: len(temp_idx) - delay + 1])
                        emd_ags.extend(temp_emd_ags[: len(temp_idx) - delay + 1])
                        labels.extend([0 for temp_i in range(len(temp_idx) - delay)])
                        labels.extend([1])
                        last_one_index = idx[-1]
                    temp_idx.clear()
                    temp_emd_ass.clear()
                    temp_emd_ags.clear()
                    continue
                else:
                    # print(img_index, last_one_index)
                    if img_index - last_one_index < 30:
                        print(img_index, last_one_index)
                        continue
        last_contours_cnt = len(contours)
        
        aug_dis = gaussian_filter(aug_binary, sigma=5)
        aug_dis = (aug_dis - np.min(aug_dis)) / (np.max(aug_dis) - np.min(aug_dis)) * 255.0
        ret, binary_gaze = cv2.threshold(gaze_img, 20, 255, cv2.THRESH_BINARY)
        gaze_dis = gaussian_filter(binary_gaze, sigma=5)
        gaze_dis = (gaze_dis - np.min(gaze_dis)) / (np.max(gaze_dis) - np.min(gaze_dis)) * 255.0

        sal_img_resize = cv2.resize(sal_img, (48, 28), interpolation=cv2.INTER_LANCZOS4)
        aug_resize = cv2.resize(aug_dis, (48, 28), interpolation=cv2.INTER_LANCZOS4)
        gaze_resize = cv2.resize(gaze_dis, (48, 28), interpolation=cv2.INTER_LANCZOS4)
        

        # cv2.imshow("sal", sal_img_resize / 255.0)
        # cv2.waitKey(0)
        # cv2.imshow("aug", aug_resize / 255.0)
        # cv2.waitKey(0)
        # cv2.imshow("gaze", gaze_resize / 255.0)
        # cv2.waitKey(0)

        sal_flat = get_signature_from_heatmap(sal_img_resize)
        aug_flat = get_signature_from_heatmap(aug_resize)
        gaze_flat = get_signature_from_heatmap(gaze_resize)

        try:
            emd_aug_sal, lowerbound, flow_matrix = cv2.EMD(aug_flat, sal_flat, distType=cv2.DIST_L2, lowerBound=0)
            emd_aug_gaze, lowerbound, flow_matrix = cv2.EMD(aug_flat, gaze_flat, distType=cv2.DIST_L2, lowerBound=0)
            temp_idx.append(img_index)
            temp_emd_ass.append(emd_aug_sal)
            temp_emd_ags.append(emd_aug_gaze)
        except:
            continue

    if one_cnt < 5 or one_cnt > 15:
        print(aug_path)

    if not os.path.exists(data_path):
        os.makedirs(data_path)
    df = pd.DataFrame({"index": idx, "emd_ani_sal": emd_ass, "emd_ani_gaze": emd_ags, "label": labels})
    df.to_csv(os.path.join(data_path, condition) + ".csv")
    return idx, emd_ass, emd_ags, labels

if __name__ == "__main__":
    imgs_path = "./formal/imgs"
    saliency_path = "./formal/saliency"
    latency = 300
    for user in os.listdir(imgs_path):
        # user = "gww"
        # print(user)
        for condition in os.listdir(os.path.join(imgs_path, user)):
            # condition = "gww_physicalhome2_typing_color"
            # print(condition)
            aug_path = os.path.join(imgs_path, user, condition, condition + "_ani.mp4")
            gaze_path = os.path.join(imgs_path, user, condition, condition + "_gaze.mp4")
            sal_path = os.path.join(saliency_path, user, condition + "_all.mp4")
            data_path = os.path.join("./merge", user)
            cal_emd(aug_path, gaze_path, sal_path, data_path, condition, latency)
        #     break
        # break