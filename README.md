# SensCon: Embedding Physiological Sensing into Virtual Reality Controllers

This repository contains Unity Project and PPG and EDA data logging system for the Mobile HCI '23 full paper "SensCon: Embedding Physiological Sensing into Virtual Reality Controllers"

![](study1_senscon.PNG)

# Abstract 
Virtual reality experiences increasingly use physiological data for virtual environment adaptations to evaluate user experience and immersion. Previous research required complex medical-grade equipment to collect physiological data, limiting real-world applicability. To overcome this, we present SensCon for skin conductance and heart rate data acquisition. To identify the optimal sensor location in the controller, we conducted a first study investigating users' controller grasp behavior. In a second study, we evaluated the performance of SensCon against medical-grade devices in six scenarios regarding user experience and signal quality. Users subjectively preferred SensCon in terms of usability and user experience. Moreover, the signal quality evaluation showed satisfactory accuracy across static, dynamic, and cognitive scenarios. Therefore, SensCon reduces the complexity of capturing and adapting the environment via real-time physiological data. By open-sourcing SensCon, we enable researchers and practitioners to adapt their virtual reality environment effortlessly. Finally, we discuss possible use cases for virtual reality-embedded physiological sensing.


DOI: https://doi.org/10.1145/3491102.3517593

## Full Paper

The paper is available in [chiossi2023senscon.pdf](./chiossi2023senscon.pdf).

# Application 


## Contribute

The easiest way to contribute is to provide feedback! We would love to hear what you think. Please write to [francescochiossi93@gmail.com](mailto:francescochiossi93@gmail.com) and [info@sven-mayer.com](mailto:info@sven-mayer.com) for closer communication.

## Licenses and Citation

Copyright &copy; 2023. [LMU Munich Media Informatics Group](https://www.medien.ifi.lmu.de). All rights reserved.

- The [dataset](./dataset) itself, available in [Creative Commons Public Domain Dedication (CC-0)](https://creativecommons.org/share-your-work/public-domain/cc0/), represented the results from consented anonymous participants and was collected by Francesco Chiossi. 
- The [./Study1/Step01_Image_preprocessing.py](./Study1/Step01_Image_preprocessing.py) provides the code for image perspective transformation.
- The [./Study1/Step02_AreaCalc.py](./Study1/Step02_AreaCalc.py) provides the code for replicating the results from Study 1.
- The [./Study%202/Step1-Results-SensCon.md](./Study 2/Step1-Results-SensCon.md) provides the code for reading the LSL output from SensCon.
- The [./Study%202/Step2-Plots.md](./Study 2/Step2-Plots.md) provides the code for preprocessing and replicating the results and figures from Study 2 in the paper.
- The [./Study%202/Step3-BlandAltman_analyses.md](./Study 2/Step3-BlandAltman_analyses.md) provides the commented code for Bland-Altman analysis.

The contained "source code" (i.e., Python and R scripts and Jupyter Notebooks) of this work is made available under the terms of [GNU GPLv3](./LICENSE). They are fully available also in the [Open Science Framework](https://osf.org/share-your-work/public-domain/cc0/).

## Citing the Paper and Application

Below are the BibTex entries to cite the paper and data set.


```
@inproceedings{10.1145/3491102.3517593,
author = {Chiossi, Francesco and Kosch, Thomas and Menghini, Luca, And Villa, Steeven and Mayer, Sven},
title = {{SensCon: Embedding Physiological Sensing into Virtual Reality Controllers}},
year = {2023},
publisher = {Association for Computing Machinery},
address = {New York, NY, USA},
url = {https://doi.org/10.1145/3491102.3517593},
doi = {10.1145/3491102.3517593},
booktitle = {Proceedings of the 2022 CHI Conference on Human Factors in Computing Systems},
pages = {1–20},
numpages = {20},
keywords = {Augmented Reality, Proxemics, Personal space, Virtual agents, Human-Agent Interaction, Perception, HCI},
location = {New Orleans, LA, USA},
series = {CHI ’22}
}
```


```
@misc{chiossi2023sensconapp,
  author = {Chiossi, Francesco and Kosch, Thomas and Menghini, Luca, And Villa, Steeven and Mayer, Sven},
  title = {{SensCon: Embedding Physiological Sensing into Virtual Reality Controllers}},
  year = {2023},
  publisher = {GitHub},
  journal = {GitHub repository},
  howpublished = {\url{https://github.com/mimuc/Senscon}}
}
```
