import cv2
import tqdm
import pathlib
import numpy as np
import matplotlib.pyplot as plt
import pandas as pd

lstSide = ["front", "back", "left", "right"]

lstRet =[]
template = None
for f in tqdm.tqdm(list(pathlib.Path(f'./final/').rglob(f'*.png'))):
    

    imOrg = cv2.imread(str(f))
    imOrg = cv2.cvtColor(imOrg, cv2.COLOR_BGR2RGB)
    im = imOrg.sum(axis=2)
    im = im /im.max()
    im = 1-im
    im[im>0]=1
    lstRet.append([f.name.split('.')[0].lower(), imOrg, im])
    #print(f"./out/{f.split('/')[-1]}")
    #f = str(f)
    
    
    df = pd.DataFrame(lstRet)

df.columns = ["Name", "ImageOrg", "Image"]
df["Type"] = df.Name.apply(lambda x: x.split("_")[0])
df.head()

img_height, img_width = 3000, 2000
n_channels = 4
transparent_img = np.ones((img_height, img_width, n_channels), dtype=np.uint8)

transparent_img[:,:,3] = overlay / overlay.max()*255


eTemplate = df[df.Name == side].iloc[0]
template = eTemplate.Image


img_height, img_width = 3000, 2000
n_channels = 4
im1 = np.ones((img_height, img_width, n_channels), dtype=np.uint8) * 255

im1[:,:,:3] = image

im2 = np.ones((img_height, img_width, n_channels), dtype=np.uint8) * 255

#im2[:,:,0] = 0
im2[:,:,1] = 0
im2[:,:,2] = 0
im2[:,:,3] = overlay / overlay.max()*255

im1[im2[:,:,3] > 0] = im2[im2[:,:,3] > 0]
plt.imshow(im1)

plt.imshow()

plt.imshow(overlay / overlay.max())


lstRes = []
for side in lstSide:
    dfX = df[df.Type==side]

    eTemplate = dfX[dfX.Name == side].iloc[0]
    template = eTemplate.Image
    image = eTemplate.ImageOrg
    overlay = np.stack(dfX[dfX.Name != side].Image.values)
    overlay = overlay.sum(axis=0)
    overlay = overlay*template
    
    img_height, img_width = 3000, 2000
    n_channels = 4

    im2 = np.ones((img_height, img_width, n_channels), dtype=np.uint8) * 255
    im2[:,:,0] = 0
    im2[:,:,1] = 0
    #im2[:,:,2] = 0
    im2[:,:,3] = overlay / overlay.max()*255
    cv2.imwrite(f"overlay_{side}.png", im2)
    
    
    fig, ax = plt.subplots(1,4,figsize=(10,10))


    ax[0].imshow(image)
    im0 = ax[1].imshow(template)
    im1 = ax[2].imshow(overlay)
    ax[3].imshow(im2)

    plt.colorbar(im0, ax=ax[1])
    plt.colorbar(im1, ax=ax[2])
    #ax[1].set_colorbar()
    plt.show()
    
    if (side == 'right'):
        template = template[1000:]
    elif (side == 'front'):
        template = template[860:]
    elif (side == 'back'):
        template = template[1000:]
    elif (side == 'left'):
        template = template[1000:]
    else:
        break
    overlayOne = np.array(overlay)
    overlayOne[overlayOne > 1] = 1
    
    overlayAll = np.zeros(overlay.shape)    
    overlayAll[overlay > 11] = 1

    
    templateOne = np.array(template)
    templateOne[templateOne > 1] = 1
    
    lstRes.append([side, overlayOne.sum()/template.sum(), overlayAll.sum()/template.sum()])
    print()
    
    dfRes = pd.DataFrame(lstRes)
dfRes.columns = ['Side', 'One', 'All']
dfRes.One = dfRes.One*100
dfRes.All = dfRes.All*100
dfRes.round(1)
