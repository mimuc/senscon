# SensCon: Embedding Physiological Sensing into Virtual Reality Controllers

This repository contains Unity Project and PPG and EDA data logging system for the Mobile HCI '23 full paper "SensCon: Embedding Physiological Sensing into Virtual Reality Controllers"

![](study1_senscon.PNG)

# Abstract 
Virtual reality experiences increasingly use physiological data for virtual environment adaptations to evaluate user experience and immersion. Previous research required complex medical-grade equipment to collect physiological data, limiting real-world applicability. To overcome this, we present SensCon for skin conductance and heart rate data acquisition. To identify the optimal sensor location in the controller, we conducted a first study investigating users' controller grasp behavior. In a second study, we evaluated the performance of SensCon against medical-grade devices in six scenarios regarding user experience and signal quality. Users subjectively preferred SensCon in terms of usability and user experience. Moreover, the signal quality evaluation showed satisfactory accuracy across static, dynamic, and cognitive scenarios. Therefore, SensCon reduces the complexity of capturing and adapting the environment via real-time physiological data. By open-sourcing SensCon, we enable researchers and practitioners to adapt their virtual reality environment effortlessly. Finally, we discuss possible use cases for virtual reality-embedded physiological sensing.


DOI: https://doi.org/10.1145/3604270

## Full Paper

The paper is available in [chiossi2023senscon.pdf](./chiossi2023senscon.pdf).

# Application 
- The [./Senscon/](./Senscon) provides Unity scene with the different tasks used for the validation of Senscon. 
- The [./Arduino/](./Arduino) provides the Arduino  code to create the LSL stream from the Arduino Sensors.
- The [./Server/](./Server) provides Python code to receive the LSL stream from the Arduino Sensors.
- The [./Schematics/](./Schematics) provides the SensCon schematics for both EDA and PPG sensors. 

## Materials
We integrated the PPG and EDA sensors into two HTC Vive controllers. 
- For the communication and local sensor control unit, we used an [ESP 8266 D1 Mini](www.openhacks.com/uploadsproductos/tutorial_nb.pdf) microcontroller offering both WiFi and Bluetooth connectivity. 
- We use a [Groove galvanic skin response (GSR) sensor](https://wiki.seeedstudio.com/Grove-GSR_Sensor/) to measure EDA with a sample rate of 192 Hz.
- We use a [Pulse Sensor](https://pulsesensor.com/) for PPG sensing with a sample rate of 50 Hz.

The controllers have built-in batteries with 960mAh at 3.85V. However, this is under the minimum required 4V for the microcontroller. Thus,  we recommend to add a power bank connected to the ESP flash port instead. 

## Contribute

The easiest way to contribute is to provide feedback! We would love to hear what you think. Please write to [francescochiossi93@gmail.com](mailto:francescochiossi93@gmail.com) and [info@sven-mayer.com](mailto:info@sven-mayer.com) for closer communication.

## Licenses and Citation

Copyright &copy; 2023. [LMU Munich Media Informatics Group](https://www.medien.ifi.lmu.de). All rights reserved.

- The [dataset](./dataset) itself, available in [Creative Commons Public Domain Dedication (CC-0)](https://creativecommons.org/share-your-work/public-domain/cc0/), represented the results from consented anonymous participants and was collected by Francesco Chiossi. 
- The [./Study1/Step01_Image_preprocessing.py](./Study1/Step01_Image_preprocessing.py) provides the code for image perspective transformation.
- The [./Study1/Step02_AreaCalc.py](./Study1/Step02_AreaCalc.py) provides the code for replicating the results from Study 1.
- The [./Study 2/Step1-Results-SensCon.md](./Study%202/Step1-Results-SensCon.md) provides the code for reading the LSL output from SensCon.
- The [./Study 2/Step2-Plots.md](./Study%202/Step2-Plots.md) provides the code for preprocessing and replicating the results and figures from Study 2 in the paper.
- The [./Study 2/Step3-BlandAltman_analyses.md](./Study%202/Step3-BlandAltman_analyses.md) provides the commented code for Bland-Altman analysis.

The contained "source code" (i.e., Python and R scripts and Jupyter Notebooks) of this work is made available under the terms of [GNU GPLv3](./LICENSE). They are fully available also in the [Open Science Framework](https://osf.org/share-your-work/public-domain/cc0/).

## Citing the Paper and Application

Below are the BibTex entries to cite the paper and data set.


```
@inproceedings{chiossi2023senscon,
author = {Chiossi, Francesco and Kosch, Thomas and Menghini, Luca, And Villa, Steeven and Mayer, Sven},
title = {{SensCon: Embedding Physiological Sensing into Virtual Reality Controllers}},
year = {2023},
publisher = {Association for Computing Machinery},
address = {New York, NY, USA},
url = {https://doi.org/10.1145/3491102.3517593},
doi = {10.1145/3604270},
volume = {6},
number = {MHCI},
pages = {1–32},
numpages = {32},
keywords = {Virtual Reality, Controller, Physiological Computing, Physiological Interaction, Embedded Systems},
}
```


```
@misc{chiossi2023sensconapp,
 author={Chiossi, Francesco and Kosch, Thomas and Menghini, Luca and Vila, Steeven and Mayer, Sven},
  title = {{SensCon: Embedding Physiological Sensing into Virtual Reality Controllers}},
  year = {2023},
  publisher = {OSF},
  journal = {Open Science Framework},
  doi = {10.17605/OSF.IO/H9MJS},
}

```
