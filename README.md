# Predicting the Noticeability of Dynamic Virtual Elements in Virtual Reality

<img width="917" alt="Screenshot 2024-02-04 at 09 54 02" src="https://github.com/ZhipengLi-98/Predicting-Noticeability-in-VR/assets/26594068/1a49d407-0db1-4989-a6fc-1d12ae9334b6">

Authors: Zhipeng Li, Yi Fei Cheng, Yukang Yan, David Lindlbauer

Publication: ACM CHI, 2024

## Abstract

While Virtual Reality (VR) systems can present virtual elements such as notifications anywhere, designing them so they are not missed by or distracting to users is highly challenging for content creators. To address this challenge, we introduce a novel approach to predict the noticeability of virtual elements. It computes the visual saliency distribution of what users see, and analyzes the temporal changes of the distribution with respect to the dynamic virtual elements that are animated. The computed features serve as input for a long short-term memory (LSTM) model that predicts whether a virtual element will be noticed. Our approach is based on data collected from 24 users in different VR environments performing
tasks such as watching a video or typing. We evaluate our approach (n = 12), and show that it can predict the timing of when users notice a change to a virtual element within 2.56 sec compared to a ground truth, and demonstrate the versatility of our approach with a set of applications. We believe that our predictive approach opens the path for computational design tools that assist VR content creators in creating interfaces that automatically adapt virtual elements based on noticeability.

## Code

This repo contains the Unity user study platform and the machine learning model training code.

The Unity project demonstrates the user study environment, including the background, primary tasks, and visual effects. Dependencies: RockVR (for recording users' view).

The python code includes the computation of features and the training of LSTM and other traditional machine-learning models. Dependencies: Python 3, numpy, pandas, keras.

## Citation

To be added.
