import cv2
import numpy as np
import glob
import tqdm
import pathlib

from pathlib import Path
Path("./out/").mkdir(parents=True, exist_ok=True)
Path("./images/").mkdir(parents=True, exist_ok=True)

def warpImage (f):
    im = cv2.imread(f)
    w,h = im.shape[:2]
    qr = cv2.QRCodeDetector()
    ret_qr, points = qr.detect(im)
    if (ret_qr):
        text = qr.decode(im, points)[0]
        w, h  = 5000, 6000
        dist_points = np.array([[0.0,0], [1,0], [1,1], [0,1]], np.float32)*np.array([500, 500.], np.float32)+np.array([w/2, 500.], np.float32) #points + 100
        # https://medium.com/analytics-vidhya/opencv-perspective-transformation-9edffefb2143
        matrix = cv2.getPerspectiveTransform(points, dist_points)
        result = cv2.warpPerspective(im, matrix, (w, h))
        #plt.imshow(result) # Transformed Capture
        text = text.replace("thepaper", "")
        
        Path(f"./out/{f.split(pathlib.os.sep)[-2]}/").mkdir(parents=True, exist_ok=True)
        cv2.imwrite(f"./out/{f.split(pathlib.os.sep)[-2]}/{text}__{f.split(pathlib.os.sep)[-1]}", result)
        return None
    else:
        return f

lstRet =[]
for f in tqdm.tqdm(list(pathlib.Path('./images/').rglob('*.jpg'))):
    #print(f"./out/{f.split('/')[-1]}")
    lstRet.append(warpImage(str(f)))
