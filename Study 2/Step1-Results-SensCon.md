```python
import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
import os
import neurokit2 as nk
import scipy
from os.path import exists
import tqdm

def con_sorter_Social(column):
    """Sort function"""
    sorter = ['Standing', 'Seating', "Walking", 'Selection', 'Nback', 'Stroop']
    correspondence = {team: order for order, team in enumerate(sorter)}
    return column.map(correspondence)


#https://github.com/hrtlacek/SNR/blob/main/SNR.ipynb
def signalPower(x):
    return np.mean(x**2)

def SNR(signal, noise):
    powS = signalPower(signal)
    powN = signalPower(noise)
    return 10*np.log10((powS-powN)/powN)

colorDic = {"yellow" : "#FFAD33", "green" : "#198D6D",  "purple": "#683b96", "red" : "#FF523F", "blue" : "#6599FF"}
colors = list(colorDic.values())
```


```python
directory = "./Data"
#lst = []
#for file in os.listdir(directory):
#    filename = os.fsdecode(file)
#    if filename.endswith("-simple-state.csv"): 
#        #print(os.path.join(directory, filename))
#        lst.append(os.path.join(directory, filename))
#        continue
#    else:
#        continue
#lst = sorted(lst)

lst=[]
for pid in [2, 3, 5, 6, 7, 8, 9, 10, 11, 12]:
    for i in range (1,7):
        filename = f"ID{pid}{i}-simple-state.csv"
        lst.append(os.path.join(directory, filename))
```


```python
def readRawMPPG (p):
    dfMPPG = pd.read_csv(f"{p}-MEDA.csv")
    dfMPPG = dfMPPG[dfMPPG.Time != "Time"]
    dfMPPG = dfMPPG.astype(float)
    dfMPPG = dfMPPG.groupby("TimeLsl").mean().reset_index()
    dfMPPG = dfMPPG.rename(columns={'Heartrate':'Value'})
    dfMPPG = dfMPPG[['Time', 'TimeLsl', 'Value']]
    dfMPPG = dfMPPG[~dfMPPG.Value.isna()].copy()
    return dfMPPG

def readRawMEDA (p):
    dfMEDA = pd.read_csv(f"{p}-MEDA.csv")
    dfMEDA = dfMEDA[dfMEDA.Time != "Time"]
    dfMEDA = dfMEDA.astype(float)
    dfMEDA = dfMEDA.groupby("TimeLsl").mean().reset_index()
    dfMEDA = dfMEDA.rename(columns={'EDA':'Value'})
    dfMEDA = dfMEDA[['Time', 'TimeLsl', 'Value']]
    dfMEDA = dfMEDA[~dfMEDA.Value.isna()].copy()
    return dfMEDA

def readRawSPPG(p):
    dfSPPG = pd.read_csv(f"{p}-PPG.csv")
    dfSPPG = dfSPPG[dfSPPG.Time != "Time"]
    dfSPPG = dfSPPG.astype(float)
    #dfSPPG = dfSPPG[(dfSPPG.Value > 0)].copy()
    dfSPPG = dfSPPG.rename(columns={'PPG':'Value'})
    dfSPPG = dfSPPG[~dfSPPG.Value.isna()].copy()
    dfSPPG = dfSPPG.groupby("TimeLsl").mean().reset_index()
    return dfSPPG

def readRawSEDA(p):
    dfSEDA = pd.read_csv(f"{p}-EDA.csv")
    dfSEDA = dfSEDA.astype(float)
    dfSEDA = dfSEDA.groupby("TimeLsl").mean().reset_index()
    dfSEDA = dfSEDA[~dfSEDA.Value.isna()].copy()
    #dfSEDA = dfSEDA.rename(columns={'Value':'EDA'})
    return dfSEDA

```


```python
def readMPPG (p, timeLslStart, timeLslEnd, sr=250):
    dfMPPG = readRawMPPG(p)
    if(len(dfMPPG) < 10):
        print((f"Empty file {p}-MEDA.csv"))
        return None
    
    dfMPPG = dfMPPG[(dfMPPG.TimeLsl > timeLslStart) & (dfMPPG.TimeLsl < timeLslEnd)].copy()
    
    dfMPPG["TimeRel"] = dfMPPG.TimeLsl - timeLslStart
    dfMPPG.TimeRel = dfMPPG.TimeRel.round(3)
    dfMPPG = dfMPPG.groupby("TimeRel").mean().reset_index()
    dfMPPG = dfMPPG.sort_values('TimeRel')
    
    #dfMPPG.Value = dfMPPG.Value.rolling(3).mean()
    #dfMPPG = dfMPPG[~dfMPPG.Value.isna()]
   
    #ppgSR = 1/dfSPPG.TimeLsl.diff().mean() 
    #PPGsignal, info=nk.ppg_process(dfSPPG.Value, sampling_rate=ppgSR)
    #dfSPPG["sPPGRate"] = PPGsignal.PPG_Rate.values
    
    cs = scipy.interpolate.CubicSpline(dfMPPG.TimeRel.values, dfMPPG.Value.values)
    xs = np.arange(dfMPPG.TimeRel.min(), dfMPPG.TimeRel.max(), 1/250)
    
    dfResample = pd.DataFrame(np.array([xs, cs(xs)]).T)
    dfResample.columns = ["TimeRel", "Value"]
    dfResample
    ppgSR = 1/dfResample.TimeRel.diff().mean()
    PPGsignal, info=nk.ppg_process(dfResample.Value, sampling_rate=ppgSR)
    dfResample["mPPGRate"] = PPGsignal.PPG_Rate.values
    #dfResample["sPPGRate"].mean()
    dfResample = dfResample.sort_values('TimeRel')
    return dfMPPG, dfResample

```


```python

    
def readMEDA (p, timeLslStart, timeLslEnd, sr=250):
    dfMEDA = readRawMEDA(p)
    
    if(len(dfMEDA) < 10):
        print((f"Empty file {p}-MEDA.csv"))
        return None
    
    dfMEDA = dfMEDA[(dfMEDA.TimeLsl > timeLslStart) & (dfMEDA.TimeLsl < timeLslEnd)]
    dfMEDA.Value = (dfMEDA.Value / 1000) / 25
    
    dfMEDA["TimeRel"] = dfMEDA.TimeLsl - timeLslStart
    dfMEDA.TimeRel = dfMEDA.TimeRel.round(3)
    dfMEDA = dfMEDA.groupby("TimeRel").mean().reset_index()
    dfMEDA = dfMEDA.sort_values('TimeRel')

    if (len(dfMEDA) > 15):
        
        
        sr = 250 #1/dfMEDA.Time.diff().mean()
        #dfMEDA["Value2"] = dfMEDA.EDA#.rolling(20).mean()
        #dfMEDA = dfMEDA[~dfMEDA["Value2"].isna()]
        #dfMEDA["EDAClean"] =  nk.eda_clean(dfMEDA.Value2.values, sampling_rate=sr)
        #dfMEDA = dfMEDA[dfMEDA.EDAClean < dfMEDA.EDAClean.mean() + dfMEDA.EDAClean.std()*3]
        dfMEDA.Value  = dfMEDA.Value - dfMEDA.Value.mean()
        mSignal, info=nk.eda_process(dfMEDA.Value, sampling_rate=sr, method='neurokit')
        dfMEDA["EDARaw"] = mSignal.EDA_Raw.values

#        dfMEDA["EDARaw"] = dfMEDA["EDARaw"].rolling(20).mean()
        dfMEDA["SCRPeaks"] = mSignal.SCR_Peaks.values

        dfMEDA["EDA_Tonic"] = mSignal.EDA_Tonic.values
        
        
        
        dfMEDA = dfMEDA.rename(columns = {'Value':'mEDAClean','EDARaw':'mEDARaw','SCRPeaks':'mSCRPeaks','EDA_Tonic':'mEDA_Tonic'})
        return dfMEDA

    else:
        return None
    
```


```python

    
def readSPPG (p, timeLslStart, timeLslEnd):
    dfSPPG = readRawSPPG(p)
    
    if(len(dfSPPG) < 10):
        print((f"Empty file {p}-PPG.csv"))
        return None
    
    dfSPPG = dfSPPG[(dfSPPG.TimeLsl > timeLslStart) & (dfSPPG.TimeLsl < timeLslEnd)]
    dfSPPG["TimeRel"] = dfSPPG.TimeLsl - timeLslStart
    dfSPPG.TimeRel = dfSPPG.TimeRel.round(3)
    dfSPPG = dfSPPG.groupby("TimeRel").mean().reset_index()
    dfSPPG = dfSPPG.sort_values('TimeRel')
    
    #dfSPPG.Value = dfSPPG.Value.rolling(3).mean()
    #dfSPPG = dfSPPG[~dfSPPG.Value.isna()]
    #ppgSR = 1/dfSPPG.TimeLsl.diff().mean() 
    #PPGsignal, info=nk.ppg_process(dfSPPG.Value, sampling_rate=ppgSR)
    #dfSPPG["sPPGRate"] = PPGsignal.PPG_Rate.values
    
    cs = scipy.interpolate.CubicSpline(dfSPPG.TimeRel.values, dfSPPG.Value.values)
    xs = np.arange(dfSPPG.TimeRel.min(), dfSPPG.TimeRel.max(), 1/250)
    
    dfResample = pd.DataFrame(np.array([xs, cs(xs)]).T)
    dfResample.columns = ["TimeRel", "Value"]
    dfResample
    ppgSR = 1/dfResample.TimeRel.diff().mean()
    PPGsignal, info=nk.ppg_process(dfResample.Value, sampling_rate=ppgSR)
    dfResample["sPPGRate"] = PPGsignal.PPG_Rate.values
    #dfResample["sPPGRate"].mean()
    dfResample = dfResample.sort_values('TimeRel')
    return dfSPPG, dfResample
```


```python


def readSEDA(p, timeLslStart, timeLslEnd):
    dfSEDA = readRawSEDA(p)
    if(len(dfSEDA) < 10):
        print((f"Empty file {p}-EDA.csv"))
        return None
    
    dfSEDA = dfSEDA[(dfSEDA.TimeLsl > timeLslStart) & (dfSEDA.TimeLsl < timeLslEnd)]
    
    dfSEDA["TimeRel"] = dfSEDA.TimeLsl - timeLslStart
    dfSEDA.TimeRel = dfSEDA.TimeRel.round(3)
    dfSEDA = dfSEDA.groupby("TimeRel").mean().reset_index()
    dfSEDA = dfSEDA.sort_values('TimeRel')
    
    dfSEDA.Value = (1/dfSEDA.Value)*500000 
    
    edaMissing = False
    
    if (len(dfSEDA) > 10):
        dfSEDA.Value  = (dfSEDA.Value - dfSEDA.Value.mean()) / dfSEDA.Value.std()
        
        cs = scipy.interpolate.CubicSpline(dfSEDA.TimeRel.values, dfSEDA.Value.values)
        xs = np.arange(dfSEDA.TimeRel.min(), dfSEDA.TimeRel.max(), 1/250)
        dfResample = pd.DataFrame(np.array([xs, cs(xs)]).T)
        dfResample.columns = ["TimeRel", "Value"]
        dfResample
        edaSR = 1/dfResample.TimeRel.diff().mean()
        #PPGsignal, info=nk.ppg_process(dfResample.Value, sampling_rate=ppgSR)
        mSignal, info=nk.eda_process(dfResample.Value.values, sampling_rate=edaSR, method='neurokit')
        #dfResample["sPPGRate"] = PPGsignal.PPG_Rate.values
        #dfResample["sPPGRate"].mean()
        dfResample = dfResample.sort_values('TimeRel')
        
        
        
        dfResample["EDARaw"] = mSignal.EDA_Raw.values

        #        dfMEDA["EDARaw"] = dfMEDA["EDARaw"].rolling(20).mean()
        dfResample["SCRPeaks"] = mSignal.SCR_Peaks.values
        dfResample["EDA_Tonic"] = mSignal.EDA_Tonic.values

        dfResample = dfResample.sort_values('TimeRel')
        dfResample = dfResample.rename(columns = {'Value':'sEDAClean','EDARaw':'sEDARaw','SCRPeaks':'sSCRPeaks','EDA_Tonic':'sEDA_Tonic'})
        return dfSEDA, dfResample
    else:
        return None
```


```python
def getStartTimes(p):
    dfP = pd.read_csv(f'{p}-simple-state.csv')
    dfSPPG = readRawSPPG(p)
    dfMPPG = readRawMPPG(p)
    dfSEDA  =readRawSEDA(p)

    timeStart = max(dfSPPG.Time.min(), dfMPPG.Time.min(), dfSEDA.Time.min(), dfP[dfP.Status=="start"].Time.values[0])
    timeEnd = min(dfSPPG.Time.max(), dfMPPG.Time.max(), dfSEDA.Time.max(), dfP[dfP.Status=="end"].Time.values[0])


    dfSPPG = dfSPPG[(dfSPPG.Time > timeStart) & (dfSPPG.Time < timeEnd)]
    dfMPPG = dfMPPG[(dfMPPG.Time > timeStart) & (dfMPPG.Time < timeEnd)]
    dfSEDA = dfSEDA[(dfSEDA.Time > timeStart) & (dfSEDA.Time < timeEnd)]


    timeLslStart = max(dfSPPG.TimeLsl.min(), dfMPPG.TimeLsl.min(), dfSEDA.TimeLsl.min())
    timeLslEnd = min(dfSPPG.TimeLsl.max(), dfMPPG.TimeLsl.max(), dfSEDA.TimeLsl.max())
        
    return (np.ceil(timeStart), np.floor(timeEnd)), (np.ceil(timeLslStart), np.floor(timeLslEnd))
```


```python
lstData = []
for f in tqdm.tqdm(lst, total=len(lst)):
    
    if(exists(f)):
        data = {}
        snrEDA = np.nan
        p = f.split("/")[-1].split("-")[0]
        
        dfP = pd.read_csv(f)
        (timeStart, timeEnd), (timeLslStart, timeLslEnd) = getStartTimes(p)
        
        dfSPPGRaw, dfSPPG = readSPPG(p, timeLslStart, timeLslEnd)
        _, dfMPPG = readMPPG(p, timeLslStart, timeLslEnd)
        
        if (type(dfSPPG) != type(None)) and (type(dfMPPG) != type(None)):
            ppgMissing = False

            dfPPGCompare = pd.merge_asof(dfSPPG[["TimeRel", "sPPGRate"]],
                                         dfMPPG[["TimeRel", "mPPGRate"]],
                                         on="TimeRel", tolerance=1)
            dfPPGCompare = dfPPGCompare[~dfPPGCompare["mPPGRate"].isna()]

            ppgHRcorr = scipy.stats.pearsonr(dfPPGCompare.mPPGRate.values, dfPPGCompare.sPPGRate.values)
            ppgspear = scipy.stats.spearmanr(dfPPGCompare.mPPGRate.values, dfPPGCompare.sPPGRate.values)

            data["ppgPearson"] = ppgHRcorr[0]
            data["ppgPearsonP"] = ppgHRcorr[1]
            data["ppgSpearman"] = ppgspear[0]
            data["ppgSpearmanP"] = ppgspear[1]
            data["sPPGCoverage"] = len(dfSPPG.TimeRel.round().unique()) # If there is a sample every second?
            data["mPPGCoverage"] = len(dfMPPG.drop_duplicates(["TimeRel", "mPPGRate"]).TimeRel.round().unique())
            data["mPPGMean"] = dfMPPG["mPPGRate"].mean()
            data["mPPGStd"] = dfMPPG["mPPGRate"].std()
            data["sPPGMean"] = dfSPPG["sPPGRate"].mean() + 7.950812
            data["sPPGStd"] = dfSPPG["sPPGRate"].std()
            dfSPPGDiff = dfSPPG.TimeRel.diff()
            if (len(dfSPPGDiff[dfSPPGDiff > 5]) >0):
                print(f"gap {p}")
                
                
            data["srSPPGMean"] = 1/dfSPPGRaw.TimeRel.diff().mean()
            data["srMPPGMean"] = 1/dfMPPG.TimeRel.diff().mean()
            data["dfsPPG"] = dfSPPG
            data["dfmPPG"] = dfMPPG
        else:
            ppgMissing = True
            print(f"{p} – {dfP.Condition.iloc[0]} – no clean PPG data")

        
        dfSEDARaw, dfSEDA = readSEDA(p, timeLslStart, timeLslEnd)
        dfMEDA = readMEDA(p, timeLslStart, timeLslEnd)

        if (type(dfSEDA) != type(None)) and (type(dfMEDA) != type(None)):
            
            dfEDACompare = pd.merge_asof( dfSEDA[["TimeRel", "sEDAClean",  "sEDA_Tonic", "sSCRPeaks"]], 
                             dfMEDA[["TimeRel", "mEDAClean", "mEDA_Tonic", "mSCRPeaks"]], 
                             left_on='TimeRel', right_on='TimeRel',
                             allow_exact_matches=True,
                             direction='nearest',
                             tolerance=.1)
            dfEDACompare = dfEDACompare[~dfEDACompare.mEDAClean.isna()]
            #dfEDACompare = pd.merge_asof(
            #                             dfSEDA[["TimeRel", "sEDAClean",  "sEDA_Tonic", "sSCRPeaks"]], 
            #                             on="TimeRel",
            #                             allow_exact_matches=True,
            #                             direction='nearest',
            #                             tolerance=1)
            #dfEDACompare = dfEDACompare[~dfEDACompare.sEDA_Tonic.isna()]

            eda = scipy.stats.pearsonr(dfEDACompare.mEDA_Tonic.values, dfEDACompare.sEDA_Tonic.values)
            edaspear = scipy.stats.spearmanr(dfEDACompare.mEDA_Tonic.values, dfEDACompare.sEDA_Tonic.values)
            data["edaPearson"] = eda[0]
            data["edaPearsonP"] = eda[1]
            data["edaSpearman"] = edaspear[0]
            data["edaSpearmanP"] = edaspear[1]

            data["mEDA_Tonic"] = dfEDACompare.mEDA_Tonic.median()
            data["mEDA_TonicStd"] = dfEDACompare.mEDA_Tonic.std()
            data["sEDA_Tonic"] = dfEDACompare.sEDA_Tonic.median()
            data["sEDA_TonicStd"] = dfEDACompare.sEDA_Tonic.std()
            data["mSCRPeaks"] = dfEDACompare.mSCRPeaks.sum() / dfEDACompare.mSCRPeaks.count() *60
            data["sSCRPeaks"] = dfEDACompare.sSCRPeaks.sum() / dfEDACompare.mSCRPeaks.count() *60

            #data["edaSNR"] = SNR(dfEDACompare.mEDARaw.values, dfEDACompare.sEDARaw.values)
            edaMissing = False

            data["sEDACoverage"] = len(dfSEDA.TimeRel.round().unique())
            data["mEDACoverage"]= len(dfMEDA.TimeRel.round().unique())
            #data["mEDASampleFrequency"] = 1 / dfMEDA.TimeLsl.diff().mean()
            #data["sEDASampleFrequency"] = 1 / dfSEDA.TimeLsl.diff().mean()
            dfSEDADiff = dfSEDA.TimeRel.diff()
            if (len(dfSEDADiff[dfSEDADiff > 5]) >0):
                print(f"gap {p}")
            dfSPPGDiff = dfSPPG.TimeRel.diff()
            if (len(dfSPPGDiff[dfSPPGDiff > 5]) >0):
                print(f"gap {p}")

            data["srSEDAMean"] = 1/dfSEDARaw.TimeLsl.diff().mean()
            data["srMEDAMean"] = 1/dfMEDA.TimeLsl.diff().mean()

        else:
            edaMissing = True
            print(f"{p} – {dfP.Condition.iloc[0]} – no clean EDA data")

        if (len(dfMEDA.TimeLsl.unique()) != len(dfMEDA)):
            print(f"Check file MEDA {p}")

        if (len(dfSEDARaw.TimeLsl.unique()) != len(dfSEDARaw)):
            print(f"Check file SEDA {p}")

        if (len(dfSPPGRaw.TimeLsl.unique()) != len(dfSPPGRaw)):
            print(f"Check file SPPG {p}")

        data["Condition"] = dfP.Condition.iloc[0]
        data["PId"] = int(str(dfP.PId.iloc[0])[:-1])
        data["Duration"] = timeLslEnd - timeLslStart+1 #timeLslEnd - timeLslStart 
        data["edaMissing"] = edaMissing
        data["ppgMissing"] = ppgMissing
        data["dfEDACompare"] = dfEDACompare
        data["dfPPGCompare"] = dfPPGCompare

        lstData.append(data)
    else:
        print(f"{f} does not exist")

df = pd.DataFrame(lstData)
df.Condition = df.Condition.str.title()
df = df.sort_values(["Condition", "PId"])
df["sEDACoverage"] = df.sEDACoverage / df.Duration.round() * 100
df["sPPGCoverage"] = df.sPPGCoverage / df.Duration.round() * 100
df["mEDACoverage"] = df.mEDACoverage / df.Duration.round() * 100
df["mPPGCoverage"] = df.mPPGCoverage / df.Duration.round() * 100


df['PPGDiff'] = df.sPPGMean - df.mPPGMean
df['EDA_TonicDiff'] = df.mEDA_Tonic - df.sEDA_Tonic
df['EDA_PhasicDiff'] = df.mSCRPeaks - df.sSCRPeaks

df.to_pickle("./data_clean.pkl")
df.to_csv('./data_diff_corr.csv')

dfX = df[['PId', 'Condition', 'sPPGMean', 'mPPGMean', 'mEDA_Tonic', 'sEDA_Tonic', 'sSCRPeaks', 'mSCRPeaks','sPPGCoverage', 'mPPGCoverage','sEDACoverage','mEDACoverage']]
dfX = dfX.drop_duplicates(['PId', 'Condition'])
lstC = []
for c in dfX.columns:
    if 's' == c[0]:
        c = c[1:]+'-SensCon'
    elif 'm' == c[0]:
        c = c[1:]+'-Medical'
    lstC.append(c)
dfX.columns = lstC
dfX = pd.wide_to_long(dfX, ['PPGMean', 'EDA_Tonic', 'SCRPeaks', 'PPGCoverage', 'EDACoverage'], i=['PId', 'Condition'], j='System', sep='-', suffix=r'\w+').reset_index()
dfX.to_csv('./data_R.csv')
dfX



df.head()
```

    100%|██████████████████████████████████████████████████████████████████████████████████| 60/60 [00:21<00:00,  2.75it/s]
    




<div>
<style scoped>
    .dataframe tbody tr th:only-of-type {
        vertical-align: middle;
    }

    .dataframe tbody tr th {
        vertical-align: top;
    }

    .dataframe thead th {
        text-align: right;
    }
</style>
<table border="1" class="dataframe">
  <thead>
    <tr style="text-align: right;">
      <th></th>
      <th>ppgPearson</th>
      <th>ppgPearsonP</th>
      <th>ppgSpearman</th>
      <th>ppgSpearmanP</th>
      <th>sPPGCoverage</th>
      <th>mPPGCoverage</th>
      <th>mPPGMean</th>
      <th>mPPGStd</th>
      <th>sPPGMean</th>
      <th>sPPGStd</th>
      <th>...</th>
      <th>Condition</th>
      <th>PId</th>
      <th>Duration</th>
      <th>edaMissing</th>
      <th>ppgMissing</th>
      <th>dfEDACompare</th>
      <th>dfPPGCompare</th>
      <th>PPGDiff</th>
      <th>EDA_TonicDiff</th>
      <th>EDA_PhasicDiff</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <th>4</th>
      <td>-0.149283</td>
      <td>2.622711e-221</td>
      <td>-0.131950</td>
      <td>6.212419e-173</td>
      <td>100.0</td>
      <td>100.0</td>
      <td>86.511704</td>
      <td>7.637884</td>
      <td>98.176558</td>
      <td>28.425923</td>
      <td>...</td>
      <td>Nback</td>
      <td>2</td>
      <td>180.0</td>
      <td>False</td>
      <td>False</td>
      <td>TimeRel  sEDAClean  sEDA_Tonic  sSCRPea...</td>
      <td>TimeRel    sPPGRate   mPPGRate
0       ...</td>
      <td>11.664854</td>
      <td>0.481300</td>
      <td>0.088507</td>
    </tr>
    <tr>
      <th>10</th>
      <td>0.013898</td>
      <td>3.556807e-03</td>
      <td>0.087728</td>
      <td>6.816062e-76</td>
      <td>100.0</td>
      <td>100.0</td>
      <td>89.436072</td>
      <td>9.795370</td>
      <td>68.584574</td>
      <td>29.546710</td>
      <td>...</td>
      <td>Nback</td>
      <td>3</td>
      <td>177.0</td>
      <td>False</td>
      <td>False</td>
      <td>TimeRel  sEDAClean  sEDA_Tonic  sSCRPea...</td>
      <td>TimeRel   sPPGRate   mPPGRate
0        ...</td>
      <td>-20.851498</td>
      <td>-0.049482</td>
      <td>0.013637</td>
    </tr>
    <tr>
      <th>16</th>
      <td>0.153578</td>
      <td>3.923828e-234</td>
      <td>0.157064</td>
      <td>6.433115e-245</td>
      <td>100.0</td>
      <td>100.0</td>
      <td>95.764928</td>
      <td>11.423062</td>
      <td>93.342237</td>
      <td>25.168384</td>
      <td>...</td>
      <td>Nback</td>
      <td>5</td>
      <td>180.0</td>
      <td>False</td>
      <td>False</td>
      <td>TimeRel  sEDAClean  sEDA_Tonic  sSCRPea...</td>
      <td>TimeRel    sPPGRate    mPPGRate
0      ...</td>
      <td>-2.422691</td>
      <td>0.222136</td>
      <td>-0.005366</td>
    </tr>
    <tr>
      <th>22</th>
      <td>0.173890</td>
      <td>6.611468e-299</td>
      <td>0.159105</td>
      <td>5.777962e-250</td>
      <td>100.0</td>
      <td>100.0</td>
      <td>102.878928</td>
      <td>10.787725</td>
      <td>95.834389</td>
      <td>26.869025</td>
      <td>...</td>
      <td>Nback</td>
      <td>6</td>
      <td>179.0</td>
      <td>False</td>
      <td>False</td>
      <td>TimeRel  sEDAClean  sEDA_Tonic  sSCRPea...</td>
      <td>TimeRel   sPPGRate    mPPGRate
0       ...</td>
      <td>-7.044539</td>
      <td>0.101210</td>
      <td>0.014838</td>
    </tr>
    <tr>
      <th>28</th>
      <td>0.135243</td>
      <td>1.579847e-180</td>
      <td>0.201944</td>
      <td>0.000000e+00</td>
      <td>100.0</td>
      <td>100.0</td>
      <td>91.973020</td>
      <td>8.815548</td>
      <td>94.094393</td>
      <td>25.019364</td>
      <td>...</td>
      <td>Nback</td>
      <td>7</td>
      <td>179.0</td>
      <td>False</td>
      <td>False</td>
      <td>TimeRel  sEDAClean  sEDA_Tonic  sSCRPea...</td>
      <td>TimeRel   sPPGRate   mPPGRate
0        ...</td>
      <td>2.121373</td>
      <td>-0.005159</td>
      <td>-0.041824</td>
    </tr>
  </tbody>
</table>
<p>5 rows × 38 columns</p>
</div>


