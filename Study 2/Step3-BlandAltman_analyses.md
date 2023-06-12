
# Aims and content

The present document includes the analytical steps implemented to analyze the agreement between the heart rate HR and skin conductance measurements obtained with the *SensCon* wearable device and those simultaneously obtained from the corresponding gold standard methods.

Particularly, we implement a **Bland-Altman analysis** ([Altman & Bland, 1983; Bland & Altman, 1986, 1999](#ref)) following the procedures described by [Menghini et al. (2021)](#ref). Bland-Altman plots and related statistics are widely considered as the standard analytical tool for comparing two methods (i.e., a new method under assessment and a reference one) that provide quantitative measures. In addition to be unaffected by the factors potentially biasing correlation and t-tests (e.g., sample size, systematic bias, proportional bias), Bland-Altman analysis provide easily interpretable indices of **systematic bias** (i.e., the mean difference between the two methods) and **random error** (i.e., the limits of agreement) expressed in the original measurement unit.

Here, we remove all objects from the R global environment, and we set the basic options.
```{r  }
# removing all objets from the workspace
rm(list=ls())

# setting system time zone to GMT (for consistent temporal synchronization)
Sys.setenv(tz="GMT")

# setting seed for consistent bootstrap results between table and plots
set.seed(123)
```

The following R packages are used in this document (see [References](#ref) section):
```{r  }
# required packages
packages <- c("plyr","BlandAltmanLeh","ggplot2","ggExtra","mgsub","gridExtra")

# generate packages references
knitr::write_bib(c(.packages(), packages),"packagesProc.bib")

# # run to install missing packages
# xfun::pkg_attach2(packages, message = FALSE); rm(list=ls())
```

<br>

# 1. Data reading

First, we read and recode the aggregated `signal` data including the participants' mean value in each `Condition` (i.e., Seating, Standing",Walking,Selection, Nback, and Stroop) by each `System` (i.e., SensCon vs. Medical). We can see that the dataset is balanced by participant `PId`, `Condition`, and `System`, with 10 participants, 6 conditions, and 2 systems. 
```{r  }
# reading data
signals <- read.csv("data_R.csv")

# data structure
str(signals)

# categorical variables as factor
signals$PId <- as.factor(signals$PId) # participant's identifier
signals$Condition <- factor(signals$Condition, # task condition
                            levels=c("Seating","Standing","Walking","Selection","Nback","Stroop"))
signals$System <- as.factor(signals$System) # measurement system

# number of participants, conditions and systems
cat(nlevels(signals$PId),"participants,",nlevels(signals$Condition),"conditions,",nlevels(signals$System),"systems")

# summary of data
summary(signals)
```

<br>

Our aim is to compare the `PPGMean` (bpm), `EDA_Tonic` (mSiemens), and `SCRPeaks` (N/min) measurements between the two `Systems`s. Here, we can see that both `PPGMean` and `EDA_Tonic` are quite symmetrically distributed, whereas `SCRPeaks` shows a skewed distribution.
```{r  fig.width=12,fig.height=5}
# data distributions
par(mfrow=c(2,4))
for(Var in colnames(signals)[2:ncol(signals)]){
    if(is.factor(signals[,Var])){ plot(signals[,Var],main=Var,xlab="") 
    } else { hist(signals[,Var],main=Var,xlab="",breaks=15) }}
```

<br>

# 2. Data preparation

Here, we prepare the data for the following analysis. That is, we split the signal values columns by `System` levels and we create a wide-form dataset with one row for each participant.
```{r }
# splitting dataset
senscon <- signals[signals$System=="SensCon",]
colnames(senscon)[5:7] <- paste0(colnames(senscon)[5:7],"_SensCon")
medical <- signals[signals$System=="Medical",]
colnames(medical)[5:7] <- paste0(colnames(medical)[5:7],"_Medical")

# merging datasets
library(plyr)
signals <- join(senscon[,c(2:3,5:7)],medical[,c(2:3,5:7)],by=c("PId","Condition"))
signals # dataset used in the analyses
```

<br>

# 3. Data analysis

## 3.1. Data analysis functions

Here, we sightly modify the R functions provided by [Menghini et al., (2021)](#ref) and available at https://github.com/SRI-human-sleep/sleep-trackers-performance in order to adapt them to the current dataset.

<details><summary>`groupDiscr`</summary>
<p>

```{r warning=FALSE,message=FALSE}

groupDiscr <- function(data=NA,measures=c("TST_device","TST_ref"),size="mean",logTransf=FALSE,condition,
                       CI.type="classic",CI.level=.95,boot.type="basic",boot.R=10000,digits=2,warnings=TRUE,
                       meas.unit="bpm"){
  
  require(BlandAltmanLeh)
  
  # setting measure name and unit of measurement
  measure <- gsub("_Medical","",gsub("_SensCon","",measures[1]))
  if(warnings==TRUE){cat("\n\n-------------\n Measure:",measure,"- Condition:",condition,"\n-------------")}
  
  # packages and functions to be used with boostrap CI
  if(CI.type=="boot"){ require(boot)
    # functions to generate bootstrap CI for model parameters
    boot.reg <- function(data,formula,indices){ return(coef(lm(formula,data=data[indices,]))[2]) } # slope
    boot.int <- function(data,formula,indices){ return(coef(lm(formula,data=data[indices,]))[1]) } # intercept
    boot.res <- function(data,formula,indices){return(1.96*sd(resid(lm(formula,data=data[indices,]))))} # 1.96 SD of residuals
    if(warnings==TRUE){cat("\n\nComputing boostrap CI with method '",boot.type,"' ...",sep="")}
    } else if(CI.type!="classic") { stop("Error: CI.type can be either 'classic' or 'boot'") }
  
  # data to be used
  ba.stat <- bland.altman.stats(data[,measures[1]],data[,measures[2]],conf.int=CI.level)
  if(size=="reference"){
    ba <- data.frame(size=ba.stat$groups$group2,diffs=ba.stat$diffs)
    xlab <- paste("Reference-derived",measure)
    } else if(size=="mean"){
      ba <- data.frame(size=ba.stat$means,diffs=ba.stat$diffs)
      xlab <- paste("Mean",measure,"by device and reference")
    } else { stop("Error: size argument can be either 'reference' or 'mean'") }
  
  # basic output table
  out <- data.frame(measure=paste(measure," (",meas.unit,")",sep=""),
                    
                    # mean and standard deviation for ref and device
                    device = paste(round(mean(ba.stat$groups$group1,na.rm=TRUE),digits)," (",
                                   round(sd(ba.stat$groups$group1,na.rm=TRUE),digits),")",sep=""),
                    reference = paste(round(mean(ba.stat$groups$group2,na.rm=TRUE),digits)," (",
                                      round(sd(ba.stat$groups$group2,na.rm=TRUE),digits),")",sep=""),
                    
                    # CI type
                    CI.type=CI.type,
                    CI.level=CI.level)
  
  # ..........................................
  # 1. TESTING PROPORTIONAL BIAS
  # ..........................................
  m <- lm(diffs~size,ba)
  if(CI.type=="classic"){ CI <- confint(m,level=CI.level)[2,] 
  } else { CI <- boot.ci(boot(data=ba,statistic=boot.reg,formula=diffs~size,R=boot.R),
                         type=boot.type,conf=CI.level)[[4]][4:5] }
  prop.bias <- ifelse(CI[1] > 0 | CI[2] < 0, TRUE, FALSE)
  
  # updating output table
  out <- cbind(out,prop.bias=prop.bias)
  
  # ...........................................
  # 1.1. DIFFERENCES INDEPENDENT FROM SIZE
  # ...........................................
  if(prop.bias == FALSE){
    
    if(CI.type=="boot"){ # changing bias CI when CI.type="boot"
      ba.stat$CI.lines[3] <- boot.ci(boot(ba$diffs,function(dat,idx)mean(dat[idx],na.rm=TRUE),R=boot.R),
                                     type=boot.type,conf=CI.level)[[4]][4]
      ba.stat$CI.lines[4] <- boot.ci(boot(ba$diffs,function(dat,idx)mean(dat[idx],na.rm=TRUE),R=boot.R),
                                     type=boot.type,conf=CI.level)[[4]][5] }
    
    out <- cbind(out, # updating output table
                 # bias (SD) [CI]
                 bias=paste(round(ba.stat$mean.diffs,digits)," (",round(sd(ba.stat$diffs),digits),")",sep=""),
                 bias_CI=paste("[",round(ba.stat$CI.lines[3],digits),", ",round(ba.stat$CI.lines[4],digits),"]",sep=""))
    
    # ..........................................
    # 1.2. DIFFERENCES PROPORTIONAL TO SIZE
    # ..........................................
    } else { 
        
      b0 <- coef(m)[1]
      b1 <- coef(m)[2]
      
      # warning message
      if(warnings==TRUE){cat("\n\nWARNING: differences in ",measure," might be proportional to the size of measurement (coeff. = ",
          round(b1,2)," [",round(CI[1],2),", ",round(CI[2],2),"]",").",
          "\nBias and LOAs are represented as a function of the size of measurement.",sep="")}
      
      # intercept CI
      if(CI.type=="classic"){ CInt <- confint(m,level=CI.level)[1,]
      } else { CInt <- boot.ci(boot(data=ba,statistic=boot.int,formula=diffs~size,R=boot.R),
                               type=boot.type,conf=CI.level)[[4]][4:5] }
      
      out <- cbind(out, # updating output table
                   
                   # bias and CI following Bland & Altman (1999): D = b0 + b1 * size
                   bias=paste(round(b0,digits)," + ",round(b1,digits)," x ",ifelse(size=="mean","mean","ref"),sep=""),
                   bias_CI=paste("b0 = [",round(CInt[1],digits),", ",round(CInt[2],digits),
                                 "], b1 = [",round(CI[1],digits),", ",round(CI[2],digits),"]",sep="")) }
  
  # ..............................................
  # 2. LOAs ESTIMATION FROM ORIGINAL DATA
  # ..............................................
  if(logTransf == FALSE){
    
    # testing heteroscedasticity
    mRes <- lm(abs(resid(m))~size,ba)
    if(CI.type=="classic"){ CIRes <- confint(mRes,level=CI.level)[2,]
    } else { CIRes <- boot.ci(boot(data=ba,statistic=boot.reg,formula=abs(resid(m))~size,R=boot.R),
                              type=boot.type,conf=CI.level)[[4]][4:5] }
    heterosced <- ifelse(CIRes[1] > 0 | CIRes[2] < 0,TRUE,FALSE)
    
    # testing normality of differences
    shapiro <- shapiro.test(ba.stat$diffs)
    if(shapiro$p.value <= .05){ normality = FALSE
      if(warnings==TRUE){cat("\n\nWARNING: differences in ",measure,
          " might be not normally distributed (Shapiro-Wilk W = ",round(shapiro$statistic,3),", p = ",round(shapiro$p.value,3),
          ").","\nBootstrap CI (CI.type='boot') and log transformation (logTransf=TRUE) are recommended.",sep="")} 
    } else { normality = TRUE }
    
    # updating output table
    out <- cbind(out[,1:6],logTransf=FALSE,normality=normality,heterosced=heterosced,out[,7:ncol(out)])
    
    # ...............................................
    # 2.1. CONSTANT BIAS AND HOMOSCEDASTICITY
    # ............................................
      if(prop.bias==FALSE & heterosced==FALSE){
        
        if(CI.type=="boot"){ # changing LOAs CI when CI.type="boot"
          ba.stat$CI.lines[1] <- boot.ci(boot(ba$diffs-1.96*sd(ba.stat$diffs),
                                              function(dat,idx)mean(dat[idx],na.rm=TRUE),R=boot.R),
                                         type=boot.type,conf=CI.level)[[4]][4]
          ba.stat$CI.lines[2] <- boot.ci(boot(ba$diffs-1.96*sd(ba.stat$diffs),
                                              function(dat,idx)mean(dat[idx],na.rm=TRUE),R=boot.R),
                                         type=boot.type,conf=CI.level)[[4]][5]
          ba.stat$CI.lines[5] <- boot.ci(boot(ba$diffs+1.96*sd(ba.stat$diffs),
                                              function(dat,idx)mean(dat[idx],na.rm=TRUE),R=boot.R),
                                         type=boot.type,conf=CI.level)[[4]][4]
          ba.stat$CI.lines[6] <- boot.ci(boot(ba$diffs+1.96*sd(ba.stat$diffs),
                                              function(dat,idx)mean(dat[idx],na.rm=TRUE),R=boot.R),
                                         type=boot.type,conf=CI.level)[[4]][5] }
        
        out <- cbind(out, # updating output table
                     
                     # lower LOA and CI
                     LOA.lower = ba.stat$lower.limit,
                     LOA.lower_CI = paste("[",round(ba.stat$CI.lines[1],2),", ",round(ba.stat$CI.lines[2],digits),"]",sep=""),
                     
                     # upper LOA and CI
                     LOA.upper = ba.stat$upper.limit,
                     LOA.upper_CI = paste("[",round(ba.stat$CI.lines[5],2),", ",round(ba.stat$CI.lines[6],digits),"]",sep=""))
      
    # ...............................................
    # 2.2. PROPORTIONAL BIAS AND HOMOOSCEDASTICITY
    # ...............................................
    } else if(prop.bias==TRUE & heterosced==FALSE) { 
        
      # boostrapped CI in any case
      if(warnings==TRUE){cat("\n\nLOAs CI are computed using boostrap with method '",boot.type,"'.",sep="")}
      
      require(boot)
      
      boot.res <- function(data,formula,indices){return(1.96*sd(resid(lm(formula,data=data[indices,]))))} # 1.96 SD of residuals
      CI.sdRes <- boot.ci(boot(data=ba,statistic=boot.res,formula=diffs~size,R=boot.R),
                          type=boot.type,conf=CI.level)[[4]][4:5]
      
      out <- cbind(out, # updating output table
                   
                   # lower LOA and CI following Bland & Altman (1999): LOAs = bias - 1.96sd of the residuals
                   LOA.lower = paste("bias -",round(1.96*sd(resid(m)),digits)),
                   LOA.lower_CI = paste("bias - [",round(CI.sdRes[1],digits),", ",round(CI.sdRes[2],digits),"]",sep=""),
                   
                   # upper LOA and CI following Bland & Altman (1999): LOAs = bias + 1.96sd of the residuals
                   LOA.upper = paste("bias +",round(1.96*sd(resid(m)),digits)),
                   LOA.upper_CI =paste("bias + [",round(CI.sdRes[1],digits),", ",round(CI.sdRes[2],digits),"]",sep="")) 
      
      # ............................................
      # 2.3. HETEROSCEDASTICITY
      # ............................................
      } else if(heterosced==TRUE){
          
          c0 <- coef(mRes)[1]
          c1 <- coef(mRes)[2]
          
          # warning message
          if(warnings==TRUE){cat("\n\nWARNING: standard deviation of differences in ",measure,
              " might be proportional to the size of measurement (coeff. = ",
              round(c1,digits)," [",round(CIRes[1],digits),", ",round(CIRes[2],digits),"]",").",
              "\nLOAs range is represented as a function of the size of measurement.",sep="")}
          
          # intercept CI
          if(CI.type=="classic"){ CInt <- confint(mRes,level=CI.level)[1,]
          } else { CInt <- boot.ci(boot(data=ba,statistic=boot.int,formula=abs(resid(m))~size,R=boot.R),
                                   type=boot.type,conf=CI.level)[[4]][4:5] }
          
          out <- cbind(out, # updating output table
                       
                       # lower LOA and CI following Bland & Altman (1999): LOAs = meanDiff - 2.46(c0 + c1A)
                       LOA.lower = paste("bias - 2.46(",round(c0,digits)," + ",round(c1,digits)," x ",
                                         ifelse(size=="mean","mean","ref"),")",sep=""),
                       LOA.lower_CI = paste("c0 = [",round(CInt[1],digits),", ",round(CInt[2],digits),
                                            "], c1 = [",round(CIRes[1],digits),", ",round(CIRes[2],digits),"]",sep=""),
                       
                       # upper LOA and CI following Bland & Altman (1999): LOAs = meanDiff - 2.46(c0 + c1A)
                       LOA.upper = paste("bias + 2.46(",round(c0,digits)," + ",round(c1,digits)," x ",
                                         ifelse(size=="mean","mean","ref"),")",sep=""),
                       LOA.upper_CI = paste("c0 = [",round(CInt[1],digits),", ",round(CInt[2],digits),
                                            "], c1 = [",round(CIRes[1],digits),", ",round(CIRes[2],digits),"]",sep="")) }
    
  # ..............................................
  # 3. LOAs ESTIMATION FROM LOG-TRANSFORMED DATA
  # ..............................................
  } else {
    
    # log transformation of data (add little constant to avoid Inf values)
    if(warnings==TRUE){cat("\n\nLog transforming the data before computing LOAs...")}
    ba.stat$groups$LOGgroup1 <- log(ba.stat$groups$group1 + .0001)
    ba.stat$groups$LOGgroup2 <- log(ba.stat$groups$group2 + .0001)
    ba.stat$groups$LOGdiff <- ba.stat$groups$LOGgroup1 - ba.stat$groups$LOGgroup2
    if(size=="reference"){ baLog <- data.frame(size=ba.stat$groups$LOGgroup2,diffs=ba.stat$groups$LOGdiff)
    } else { baLog <- data.frame(size=(ba.stat$groups$LOGgroup1 + ba.stat$groups$LOGgroup2)/2,diffs=ba.stat$groups$LOGdiff) }
    
    # testing heteroscedasticity
    mRes <- lm(abs(resid(m))~size,baLog)
    if(CI.type=="classic"){ CIRes <- confint(mRes,level=CI.level)[2,]
    } else { CIRes <- boot.ci(boot(data=baLog,statistic=boot.reg,formula=abs(resid(m))~size,R=boot.R),
                              type=boot.type,conf=CI.level)[[4]][4:5] }
    heterosced <- ifelse(CIRes[1] > 0 | CIRes[2] < 0,TRUE,FALSE)
    
    # testing normality of differences
    shapiro <- shapiro.test(baLog$diffs)
    if(shapiro$p.value <= .05){ normality = FALSE
      if(warnings==TRUE){cat("\n\nWARNING: differences in log transformed ",measure,
          " might be not normally distributed (Shapiro-Wilk W = ",round(shapiro$statistic,3),", p = ",round(shapiro$p.value,3),
          ").","\nBootstrap CI (CI.type='boot') are recommended.",sep="")} } else { normality = TRUE }
    
    # updating output table
    out <- cbind(out[,1:6],logTransf=FALSE,normality=normality,heterosced=heterosced,out[,7:ncol(out)])
    
    # LOAs slope following Euser et al (2008) for antilog transformation: slope = 2 * (e^(1.96 SD) - 1)/(e^(1.96 SD) + 1)
    ANTILOGslope <- function(x){ 2 * (exp(1.96 * sd(x)) - 1) / (exp(1.96*sd(x)) + 1) }
    ba.stat$LOA.slope <- ANTILOGslope(baLog$diffs)
    
    # LOAs CI slopes 
    if(CI.type=="classic"){ # classic CI
      t1 <- qt((1 - CI.level)/2, df = ba.stat$based.on - 1) # t-value right
      t2 <- qt((CI.level + 1)/2, df = ba.stat$based.on - 1) # t-value left
      ba.stat$LOA.slope.CI.upper <- 2 * (exp(1.96 * sd(baLog$diffs) + t2 * sqrt(sd(baLog$diffs)^2 * 3/ba.stat$based.on)) - 1) /
        (exp(1.96*sd(baLog$diffs) + t2 * sqrt(sd(baLog$diffs)^2 * 3/ba.stat$based.on)) + 1)
      ba.stat$LOA.slope.CI.lower <- 2 * (exp(1.96 * sd(baLog$diffs) + t1 * sqrt(sd(baLog$diffs)^2 * 3/ba.stat$based.on)) - 1) /
        (exp(1.96*sd(baLog$diffs) + t1 * sqrt(sd(baLog$diffs)^2 * 3/ba.stat$based.on)) + 1)
      } else { # boostrap CI
        ba.stat$LOA.slope.CI.lower <- boot.ci(boot(baLog$diffs,function(dat,idx) ANTILOGslope(dat[idx]),R=boot.R),
                                              type=boot.type,conf=CI.level)[[4]][4]
        ba.stat$LOA.slope.CI.upper <- boot.ci(boot(baLog$diffs,function(dat,idx) ANTILOGslope(dat[idx]),R=boot.R),
                                              type=boot.type,conf=CI.level)[[4]][5] }
    
    # LOAs depending on mean and size (regardless if bias is proportional or not)
    out <- cbind(out, # updating output table
                 
                 # lower LOA and CI
                 LOA.lower = paste("bias - ",size," x ",round(ba.stat$LOA.slope,digits)),
                 LOA.lower_CI = paste("bias - ",size," x [",round(ba.stat$LOA.slope.CI.lower,digits),
                                      ", ",round(ba.stat$LOA.slope.CI.upper,digits),"]",sep=""),
                 
                 # upper LOA and CI
                 LOA.upper = paste("bias + ",size," x ",round(ba.stat$LOA.slope,digits)),
                 LOA.upper_CI = paste("bias + ",size," x [",round(ba.stat$LOA.slope.CI.lower,digits),
                                      ", ",round(ba.stat$LOA.slope.CI.upper,digits),"]",sep="")) }
  
  
  # rounding values
  nums <- vapply(out, is.numeric, FUN.VALUE = logical(1))
  out[,nums] <- round(out[,nums], digits = digits)
  out[,nums] <- as.character(out[,nums]) # to prevent NA values when combined with other cases
  row.names(out) <- NULL
  
  return(out)
  
}

```

</p>
</details>

computes the standard metrics to evaluate the discrepancy/agreement between device- and reference-derived measures, based on the definitions provided in the main article, in compliance with [Bland and Altman (1999)](#ref). As recommended by [Euser et al. (2008)](#ref), when a significant correlation is observed between differences and PSG-derived measures (proportional bias), the LOAs are indicated as a function of the PSG-derived value.

<details><summary>`BAplot`</summary>
<p>

```{r }

BAplot <- function(data=NA,measures=c("PPGMean_SensCon","PPGMean_Medical"),logTransf=FALSE,
                   xaxis="reference",CI.type="classic",CI.level=.95,boot.type="basic",boot.R=10000,
                   xlim=NA,ylim=NA,warnings=TRUE,condition){
  
  require(BlandAltmanLeh); require(ggplot2); require(ggExtra)
  
  # setting labels
  Measure <- gsub("_Medical","",gsub("_SensCon","",measures[1]))
  measure <- gsub("PPGMean","HR (bpm)",Measure)
  measure <- gsub("EDA_Tonic","SCL (stand. units)",measure)
  measure <- gsub("SCRPeaks","nsSCRs (peaks/min)",measure)
  if(warnings==TRUE){cat("\n\n--------\n Measure:",Measure,"- Condition:",condition,"\n------------")}
  
  # packages and functions to be used with boostrap CI
  if(CI.type=="boot"){ require(boot)
    # function to generate bootstrap CI for model parameters
    boot.reg <- function(data,formula,indices){ return(coef(lm(formula,data=data[indices,]))[2]) }
    # function for sampling and predicting Y values based on model
    boot.pred <- function(data,formula,tofit) { indices <- sample(1:nrow(data),replace = TRUE)
      return(predict(lm(formula,data=data[indices,]), newdata=data.frame(tofit))) }
    if(warnings==TRUE){cat("\n\nComputing boostrap CI with method '",boot.type,"' ...",sep="")}
    } else if(CI.type!="classic") { stop("Error: CI.type can be either 'classic' or 'boot'") }
  
  # data to be used
  ba.stat <- bland.altman.stats(data[,measures[1]],data[,measures[2]],conf.int=CI.level)
  if(xaxis=="reference"){
    ba <- data.frame(size=ba.stat$groups$group2,diffs=ba.stat$diffs)
    xlab <- paste("Reference",measure)
    } else if(xaxis=="mean"){
      ba <- data.frame(size=ba.stat$means,diffs=ba.stat$diffs)
      xlab <- paste("Mean",measure)
    } else { stop("Error: xaxis argument can be either 'reference' or 'mean'") }
  
  # range of values to be fitted for drawing the lines (i.e., from min to max of x-axis values, by .001)
  size <- seq(min(ba$size),max(ba$size),(max(ba$size)-min(ba$size))/((max(ba$size)-min(ba$size))*1000))
  if(length(size==1)){}
  
  # basic plot
  
  p <- ggplot(data=ba,aes(size,diffs))
  
  # ..........................................
  # 1. TESTING PROPORTIONAL BIAS
  # ..........................................
  m <- lm(diffs~size,ba)
  if(CI.type=="classic"){ CI <- confint(m,level=CI.level)[2,] 
  } else { 
      CI <- boot(data=ba,statistic=boot.reg,formula=diffs~size,R=boot.R)
      CI <- boot.ci(CI,type=boot.type,conf=CI.level)[[4]][4:5] }
  prop.bias <- ifelse(CI[1] > 0 | CI[2] < 0, TRUE, FALSE)
  
  # ...........................................
  # 1.1. DIFFERENCES INDEPENDENT FROM SIZE
  # ...........................................
  if(prop.bias == FALSE){ 
      
      if(CI.type=="boot"){ # changing bias CI when CI.type="boot"
        ba.stat$CI.lines[3] <- boot.ci(boot(ba$diffs,function(dat,idx)mean(dat[idx],na.rm=TRUE),R=boot.R),
                                       type=boot.type,conf=CI.level)[[4]][4]
        ba.stat$CI.lines[4] <- boot.ci(boot(ba$diffs,function(dat,idx)mean(dat[idx],na.rm=TRUE),R=boot.R),
                                       type=boot.type,conf=CI.level)[[4]][5] }
      
      p <- p + # adding lines to plot
        
        # bias and CI (i.e., mean diff)
        geom_line(aes(y=ba.stat$mean.diffs),colour="red",size=1.5) +
        geom_line(aes(y=ba.stat$CI.lines[3]),colour="red",linetype=2,size=1) +
        geom_line(aes(y=ba.stat$CI.lines[4]),colour="red",linetype=2,size=1)
      
      # ..........................................
      # 1.2. DIFFERENCES PROPORTIONAL TO SIZE
      # ..........................................
      } else {
        
        b0 <- coef(m)[1]
        b1 <- coef(m)[2]
        
        # warning message
        if(warnings==TRUE){cat("\n\nWARNING: differences in ",Measure," might be proportional to the size of measurement (coeff. = ",
            round(b1,2)," [",round(CI[1],2),", ",round(CI[2],2),"]",").",
            "\nBias and LOAs are plotted as a function of the size of measurement.",sep="")}
        
        # modeling bias following Bland & Altman (1999): D = b0 + b1 * size
        y.fit <- data.frame(size,y.bias=b0+b1*size) 
        
        # bias CI
        if(CI.type=="classic"){ # classic ci
          y.fit$y.biasCI.upr <- predict(m,newdata=data.frame(y.fit$size),interval="confidence",level=CI.level)[,3]
          y.fit$y.biasCI.lwr <- predict(m,newdata=data.frame(y.fit$size),interval="confidence",level=CI.level)[,2]
          } else { # boostrap CI 
            fitted <- t(replicate(boot.R,boot.pred(ba,"diffs~size",y.fit))) # sampling CIs
            y.fit$y.biasCI.upr <- apply(fitted,2,quantile,probs=c((1-CI.level)/2))
            y.fit$y.biasCI.lwr <- apply(fitted,2,quantile,probs=c(CI.level+(1-CI.level)/2)) }
        
        p <- p + # adding lines to plot
          
          # bias and CI (i.e., D = b0 + b1 * size)
          geom_line(data=y.fit,aes(y=y.bias),colour="red",size=1.5) +
          geom_line(data=y.fit,aes(y=y.biasCI.upr),colour="red",linetype=2,size=1) +
          geom_line(data=y.fit,aes(y=y.biasCI.lwr),colour="red",linetype=2,size=1) }
  
  # ..............................................
  # 2. LOAs ESTIMATION FROM ORIGINAL DATA
  # ..............................................
  if(logTransf == FALSE){
    
    # testing heteroscedasticity
    mRes <- lm(abs(resid(m))~size,ba)
    if(CI.type=="classic"){ CIRes <- confint(mRes,level=CI.level)[2,]
    } else { CIRes <- boot.ci(boot(data=ba,statistic=boot.reg,formula=abs(resid(m))~size,R=boot.R),
                              type=boot.type,conf=CI.level)[[4]][4:5] }
    heterosced <- ifelse(CIRes[1] > 0 | CIRes[2] < 0,TRUE,FALSE)
    
    # testing normality of differences
    shapiro <- shapiro.test(ba$diffs)
    if(shapiro$p.value <= .05){
       if(warnings==TRUE){cat("\n\nWARNING: differences in ",Measure,
           " might be not normally distributed (Shapiro-Wilk W = ",round(shapiro$statistic,3),", p = ",round(shapiro$p.value,3),
           ").","\nBootstrap CI (CI.type='boot') and log transformation (logTransf=TRUE) are recommended.",sep="")} }
     
    # ............................................
    # 2.1. CONSTANT BIAS AND HOMOSCEDASTICITY
    # ............................................
    if(prop.bias==FALSE & heterosced==FALSE){
        
        if(CI.type=="boot"){ # changing LOAs CI when CI.type="boot"
          ba.stat$CI.lines[1] <- boot.ci(boot(ba$diffs-1.96*sd(ba.stat$diffs),
                                              function(dat,idx)mean(dat[idx],na.rm=TRUE),R=boot.R),
                                         type=boot.type,conf=CI.level)[[4]][4]
          ba.stat$CI.lines[2] <- boot.ci(boot(ba$diffs-1.96*sd(ba.stat$diffs),
                                              function(dat,idx)mean(dat[idx],na.rm=TRUE),R=boot.R),
                                         type=boot.type,conf=CI.level)[[4]][5]
          ba.stat$CI.lines[5] <- boot.ci(boot(ba$diffs+1.96*sd(ba.stat$diffs),
                                              function(dat,idx)mean(dat[idx],na.rm=TRUE),R=boot.R),
                                         type=boot.type,conf=CI.level)[[4]][4]
          ba.stat$CI.lines[6] <- boot.ci(boot(ba$diffs+1.96*sd(ba.stat$diffs),
                                              function(dat,idx)mean(dat[idx],na.rm=TRUE),R=boot.R),
                                         type=boot.type,conf=CI.level)[[4]][5] }
        
        p <- p + # adding lines to plot
      
        # Upper LOA and CI (i.e., mean diff + 1.96 SD)
        geom_line(aes(y=ba.stat$upper.limit),colour="darkgray",size=1.3) +
        geom_line(aes(y=ba.stat$CI.lines[5]),colour="darkgray",linetype=2,size=1) +
        geom_line(aes(y=ba.stat$CI.lines[6]),colour="darkgray",linetype=2,size=1) +
        
        # Lower LOA and CI (i.e., mean diff - 1.96 SD)
        geom_line(aes(y=ba.stat$lower.limit),colour="darkgray",size=1.3) +
        geom_line(aes(y=ba.stat$CI.lines[1]),colour="darkgray",linetype=2,size=1) +
        geom_line(aes(y=ba.stat$CI.lines[2]),colour="darkgray",linetype=2,size=1)
        
        # ............................................
        # 2.2. PROPORTIONAL BIAS AND HOMOSCEDASTICITY
        # ............................................
        } else if(prop.bias==TRUE & heterosced==FALSE) { 
          
          # modeling LOAs following Bland & Altman (1999): LOAs = bias +- 1.96sd of the residuals
          y.fit$y.LOAu = b0+b1*size + 1.96*sd(resid(m))
          y.fit$y.LOAl = b0+b1*size - 1.96*sd(resid(m))
          
          # LOAs CI based on bias CI +- 1.96sd of the residuals
          if(warnings==TRUE){cat(" Note that LOAs CI are represented based on bias CI.")}
          y.fit$y.LOAu.upr = y.fit$y.biasCI.upr + 1.96*sd(resid(m))
          y.fit$y.LOAu.lwr = y.fit$y.biasCI.lwr + 1.96*sd(resid(m))
          y.fit$y.LOAl.upr = y.fit$y.biasCI.upr - 1.96*sd(resid(m))
          y.fit$y.LOAl.lwr = y.fit$y.biasCI.lwr - 1.96*sd(resid(m))
          
          # ............................................
          # 2.3. CONSTANT BIAS AND HOMOSCEDASTICITY
          # ............................................
          } else if(prop.bias==FALSE & heterosced==TRUE) {
            
            c0 <- coef(mRes)[1]
            c1 <- coef(mRes)[2]
  
            # warning message
            if(warnings==TRUE){cat("WARNING: SD of differences in ",Measure,
                " might be proportional to the size of measurement (coeff. = ",
                round(c1,2)," [",round(CIRes[1],2),", ",round(CIRes[2],2),"]",").",
                "\nLOAs range is plotted as a function of the size of measurement.",sep="")}
  
            # modeling LOAs following Bland & Altman (1999): LOAs = meanDiff +- 2.46(c0 + c1A)
            y.fit <- data.frame(size=size,
                                y.LOAu = ba.stat$mean.diffs + 2.46*(c0+c1*size),
                                y.LOAl = ba.stat$mean.diffs - 2.46*(c0+c1*size))
            
            # LOAs CI
            if(CI.type=="classic"){ # classic ci
              fitted <- predict(mRes,newdata=data.frame(y.fit$size),interval="confidence",level=CI.level) # based on mRes
              y.fit$y.LOAu.upr <- ba.stat$mean.diffs + 2.46*fitted[,3]
              y.fit$y.LOAu.lwr <- ba.stat$mean.diffs + 2.46*fitted[,2]
              y.fit$y.LOAl.upr <- ba.stat$mean.diffs - 2.46*fitted[,3]
              y.fit$y.LOAl.lwr <- ba.stat$mean.diffs - 2.46*fitted[,2]
              } else { # boostrap CI
                fitted <- t(replicate(boot.R,boot.pred(ba,"abs(resid(lm(diffs ~ size))) ~ size",y.fit)))
                y.fit$y.LOAu.upr <- ba.stat$mean.diffs + 2.46*apply(fitted,2,quantile,probs=c(CI.level+(1-CI.level)/2))
                y.fit$y.LOAu.lwr <- ba.stat$mean.diffs + 2.46*apply(fitted,2,quantile,probs=c((1-CI.level)/2))
                y.fit$y.LOAl.upr <- ba.stat$mean.diffs - 2.46*apply(fitted,2,quantile,probs=c(CI.level+(1-CI.level)/2))
                y.fit$y.LOAl.lwr <- ba.stat$mean.diffs - 2.46*apply(fitted,2,quantile,probs=c((1-CI.level)/2)) }
            
          # ............................................
          # 2.4. PROPORTIONAL BIAS AND HETEROSCEDASTICITY
          # ............................................ 
          } else if(prop.bias==TRUE & heterosced==TRUE) {
            
          c0 <- coef(mRes)[1]
          c1 <- coef(mRes)[2]
          
          # warning message
          if(warnings==TRUE){cat("\n\nWARNING: SD of differences in ",Measure,
              " might be proportional to the size of measurement (coeff. = ",
              round(c1,2)," [",round(CIRes[1],2),", ",round(CIRes[2],2),"]",").",
              "\nLOAs range is plotted as a function of the size of measurement.",sep="")}
          
          # modeling LOAs following Bland & Altman (1999): LOAs = b0 + b1 * size +- 2.46(c0 + c1A) 
          y.fit$y.LOAu = b0+b1*size + 2.46*(c0+c1*size)
          y.fit$y.LOAl = b0+b1*size - 2.46*(c0+c1*size)
          
          # LOAs CI
          if(CI.type=="classic"){ # classic ci
            fitted <- predict(mRes,newdata=data.frame(y.fit$size),interval="confidence",level=CI.level) # based on mRes
            y.fit$y.LOAu.upr <- b0+b1*size + 2.46*fitted[,3]
            y.fit$y.LOAu.lwr <- b0+b1*size + 2.46*fitted[,2]
            y.fit$y.LOAl.upr <- b0+b1*size - 2.46*fitted[,3]
            y.fit$y.LOAl.lwr <- b0+b1*size - 2.46*fitted[,2] 
            } else { # boostrap CI
              fitted <- t(replicate(boot.R,boot.pred(ba,"abs(resid(lm(diffs ~ size))) ~ size",y.fit)))
              y.fit$y.LOAu.upr <- b0+b1*size + 2.46*apply(fitted,2,quantile,probs=c(CI.level+(1-CI.level)/2))
              y.fit$y.LOAu.lwr <- b0+b1*size + 2.46*apply(fitted,2,quantile,probs=c((1-CI.level)/2))
              y.fit$y.LOAl.upr <- b0+b1*size - 2.46*apply(fitted,2,quantile,probs=c(CI.level+(1-CI.level)/2))
              y.fit$y.LOAl.lwr <- b0+b1*size - 2.46*apply(fitted,2,quantile,probs=c((1-CI.level)/2)) }}
    
    if(prop.bias==TRUE | heterosced==TRUE){
      
      p <- p + # adding lines to plot
          
          # Upper LOA and CI
          geom_line(data=y.fit,aes(y=y.LOAu),colour="darkgray",size=1.3) +
          geom_line(data=y.fit,aes(y=y.LOAu.upr),colour="darkgray",linetype=2,size=1) +
          geom_line(data=y.fit,aes(y=y.LOAu.lwr),colour="darkgray",linetype=2,size=1) +
          
          # Lower LOA and CI
          geom_line(data=y.fit,aes(y=y.LOAl),colour="darkgray",size=1.3) +
          geom_line(data=y.fit,aes(y=y.LOAl.upr),colour="darkgray",linetype=2,size=1) +
          geom_line(data=y.fit,aes(y=y.LOAl.lwr),colour="darkgray",linetype=2,size=1)
      
    }
    
  # ..............................................
  # 3. LOAs ESTIMATION FROM LOG-TRANSFORMED DATA
  # ..............................................
  } else {
      
    # log transformation of data (add little constant to avoid Inf values)
    if(warnings==TRUE){cat("\n\nLog transforming data ...")}
    ba.stat$groups$LOGgroup1 <- log(ba.stat$groups$group1 + .0001)
    ba.stat$groups$LOGgroup2 <- log(ba.stat$groups$group2 + .0001)
    ba.stat$groups$LOGdiff <- ba.stat$groups$LOGgroup1 - ba.stat$groups$LOGgroup2
    if(xaxis=="reference"){ baLog <- data.frame(size=ba.stat$groups$LOGgroup2,diffs=ba.stat$groups$LOGdiff)
    } else { baLog <- data.frame(size=(ba.stat$groups$LOGgroup1 + ba.stat$groups$LOGgroup2)/2,diffs=ba.stat$groups$LOGdiff) }
    
    # testing heteroscedasticity
    mRes <- lm(abs(resid(m))~size,baLog)
    if(CI.type=="classic"){ CIRes <- confint(mRes,level=CI.level)[2,]
    } else { CIRes <- boot.ci(boot(data=baLog,statistic=boot.reg,formula=abs(resid(m))~size,R=boot.R),
                              type=boot.type,conf=CI.level)[[4]][4:5] }
    heterosced <- ifelse(CIRes[1] > 0 | CIRes[2] < 0,TRUE,FALSE)
      
    # testing normality of differences
    shapiro <- shapiro.test(baLog$diffs)
    if(shapiro$p.value <= .05){
      if(warnings==TRUE){cat("\n\nWARNING: differences in log transformed ",Measure,
          " might be not normally distributed (Shapiro-Wilk W = ",round(shapiro$statistic,3),", p = ",round(shapiro$p.value,3),
          ").","\nBootstrap CI (CI.type='boot') are recommended.",sep="")} }
    
    # LOAs slope following Euser et al (2008) for antilog transformation: slope = 2 * (e^(1.96 SD) - 1)/(e^(1.96 SD) + 1)
    ANTILOGslope <- function(x){ 2 * (exp(1.96 * sd(x)) - 1) / (exp(1.96*sd(x)) + 1) }
    ba.stat$LOA.slope <- ANTILOGslope(baLog$diffs)
    
    # LOAs CI slopes 
    if(CI.type=="classic"){ # classic CI
      t1 <- qt((1 - CI.level)/2, df = ba.stat$based.on - 1) # t-value right
      t2 <- qt((CI.level + 1)/2, df = ba.stat$based.on - 1) # t-value left
      ba.stat$LOA.slope.CI.upper <- 2 * (exp(1.96 * sd(baLog$diffs) + t2 * sqrt(sd(baLog$diffs)^2 * 3/ba.stat$based.on)) - 1) /
        (exp(1.96*sd(baLog$diffs) + t2 * sqrt(sd(baLog$diffs)^2 * 3/ba.stat$based.on)) + 1)
      ba.stat$LOA.slope.CI.lower <- 2 * (exp(1.96 * sd(baLog$diffs) + t1 * sqrt(sd(baLog$diffs)^2 * 3/ba.stat$based.on)) - 1) /
        (exp(1.96*sd(baLog$diffs) + t1 * sqrt(sd(baLog$diffs)^2 * 3/ba.stat$based.on)) + 1)
      } else { # boostrap CI
        ba.stat$LOA.slope.CI.upper <- boot.ci(boot(baLog$diffs,
                                                   function(dat,idx) ANTILOGslope(dat[idx]),R=boot.R),
                                              type=boot.type,conf=CI.level)[[4]][4]
        ba.stat$LOA.slope.CI.lower <- boot.ci(boot(baLog$diffs,
                                                   function(dat,idx) ANTILOGslope(dat[idx]),R=boot.R),
                                              type=boot.type,conf=CI.level)[[4]][5] }
    
    # Recomputing LOAs and their CIs as a function of size multiplied by the computed slopes
    y.fit <- data.frame(size,
                        ANTLOGdiffs.upper = size * ba.stat$LOA.slope, # upper LOA
                        ANTLOGdiffs.upper.lower = size * ba.stat$LOA.slope.CI.lower,
                        ANTLOGdiffs.upper.upper = size * ba.stat$LOA.slope.CI.upper,
                        ANTLOGdiffs.lower = size * ((-1)*ba.stat$LOA.slope), # lower LOA
                        ANTLOGdiffs.lower.lower = size * ((-1)*ba.stat$LOA.slope.CI.lower),
                        ANTLOGdiffs.lower.upper = size * ((-1)*ba.stat$LOA.slope.CI.upper))
    
    # adding bias values based on prop.bias
    if(prop.bias==FALSE){ y.fit$y.bias <- rep(ba.stat$mean.diffs,nrow(y.fit)) } else { y.fit$y.bias <- b0+b1*y.fit$size }
    
    p <- p + # adding lines to plot
      
      # UPPER LIMIT (i.e., bias + 2 * (e^(1.96 SD) - 1)/(e^(1.96 SD) + 1))
      geom_line(data=y.fit,aes(y = y.bias + ANTLOGdiffs.upper),colour="darkgray",size=1.3) +
      geom_line(data=y.fit,aes(y = y.bias + ANTLOGdiffs.upper.upper),colour="darkgray",linetype=2,size=1) +
      geom_line(data=y.fit,aes(y = y.bias + ANTLOGdiffs.upper.lower),colour="darkgray",linetype=2,size=1) +
      
      # LOWER LIMIT (i.e., bias - 2 * (e^(1.96 SD) - 1)/(e^(1.96 SD) + 1))
      geom_line(data=y.fit,aes(y = y.bias + ANTLOGdiffs.lower),colour="darkgray",size=1.3) +
      geom_line(data=y.fit,aes(y = y.bias + ANTLOGdiffs.lower.upper),colour="darkgray",linetype=2,size=1) +
      geom_line(data=y.fit,aes(y = y.bias + ANTLOGdiffs.lower.lower),colour="darkgray",linetype=2,size=1)
    
    # ..........................................
    # 3.3. HETEROSCHEDASTICITY (only a warning)
    # ..........................................
    if(heterosced==TRUE){
      
      # warning message
      if(warnings==TRUE){cat("\n\nWARNING: standard deviation of differences in in log transformed ",Measure,
          " might be proportional to the size of measurement (coeff. = ",
          round(coef(mRes)[2],2)," [",round(CIRes[1],2),", ",round(CIRes[2],2),"]",").",sep="")} }}
  
  
  p <- p + # adding last graphical elements and plotting with marginal density distribution
    
    geom_point(size=4.5,shape=20) +
    xlab(xlab) + ylab(paste("SensCon - reference\ndiff. in ",measure,sep="")) +
    ggtitle(condition)+
    theme(axis.text = element_text(size=12,face="bold"),
          plot.title = element_text(hjust=0.5,vjust=-1,size=15,face="bold"),
          axis.title = element_text(size=12,face="bold",colour="black"))
  if(!is.na(ylim[1])){ p <- p + ylim(ylim) }
  if(!is.na(xlim[1])){ p <- p + xlim(xlim) }
  return(ggMarginal(p,fill="lightgray",colour="lightgray",size=4,margins="y"))
  
}

```

</p>
</details>

Visualizes the Bland-Altman plots with the same information printed by the function above.

<br>

## 3.2. Group discripancies

Here, we use the modified `groupDiscr` function above to generate a table reporting the statistics recommended by [Bland & Altman (1999)](#ref) to summarize the performance of the *SensCon* device in terms of systematic bias and limits of agreement.  
```{r }
# comparing mean heart rate in the Seating condition
out <- groupDiscr(data = signals[signals$Condition=="Seating",],condition="Seating", # selecting condition
                  measures=c("PPGMean_SensCon","PPGMean_Medical"),meas.unit="bpm", # selecting variables & measurement unit
                  size="mean", # mean values on the x axis
                  CI.type="boot", CI.level=.95) # 95% bootstrap CI
out

# creating output table
(out <- cbind(measure=out[,"measure"],condition="Seating",out[,c(2:3,10:15)]))

# filling output table
for(signal in c("PPGMean","EDA_Tonic","SCRPeaks")){
    for(condition in levels(signals$Condition)){ 
        if((condition=="Selection"&signal=="PPGMean")|(signal=="SCRPeaks" & condition=="Stroop")){
            logTransf <- TRUE } else { logTransf <- FALSE }
        if(signal=="PPGMean" & condition=="Seating"){ next 
        } else {
            if(signal=="PPGMean"){ mu <- "bpm" }else if(signal=="EDA_Tonic"){ mu <- "stand. units"  # measurement unit
            } else { mu <- "peaks/min" }
            new <- groupDiscr(data = signals[signals$Condition==condition,],CI.type="boot", CI.level=.95,condition=condition,
                              measures=paste0(signal,c("_SensCon","_Medical")),meas.unit=mu,logTransf=logTransf)
            out <- rbind(out,cbind(measure=new[,"measure"],condition=condition,new[,c(2:3,10:15)])) }}}

# saving and showing output table
library(knitr)
write.csv(out,"RESULTS/BAtable.csv",row.names=FALSE)
kable(out)
```

<br>

## 3.3. Bland-Altman plots

Here, we generate the Bland-Altman plots corresponding to the statistics showed above.
```{r warning=FALSE,message=FALSE}
library(gridExtra)
```

### 3.3.1. SCL

Here, we generate the Bland-Altman plots for `EDA_Tonic` measurements. The tonic EDA component showed proportional biases in most tasks, implying that the magnitude of the differences between systems was dependent on the size of the measurements. In the Seating, Standing, Selection, and Stroop tasks, \system{} showed the highest accuracy for intermediate SCL values, whereas it systematically underestimated lower SCL values and overestimated higher values, respectively. In contrast, only relatively high SCL values were measured without bias in the N-back task, showing underestimations for lower SCL values. The Walking task was the only condition showing uniform and nonsignificant bias, although we found wider LOAs for higher SCL values (i.e., heteroscedasticity). Heteroscedasticity was also detected in the N-back task but in the opposite direction, with higher random error for lower SCL measurements. Critically, in most tasks the computed LOAs were wider than the range of measurement, suggesting that \system{}-implied random error in SCL measurement might be too large, at least for extreme values.
```{r fig.width=12,fig.height=6}
p <- grid.arrange(BAplot(data=signals[signals$Condition=="Seating",],measures=c("EDA_Tonic_SensCon","EDA_Tonic_Medical"),
                    xaxis="mean", CI.type="boot", CI.level=.95,condition="Seating"),
             BAplot(data=signals[signals$Condition=="Standing",],measures=c("EDA_Tonic_SensCon","EDA_Tonic_Medical"),
                    xaxis="mean", CI.type="boot", CI.level=.95,condition="Standing"),
             BAplot(data=signals[signals$Condition=="Walking",],measures=c("EDA_Tonic_SensCon","EDA_Tonic_Medical"),
                    xaxis="mean", CI.type="boot", CI.level=.95,condition="Walking"),
             BAplot(data=signals[signals$Condition=="Selection",],measures=c("EDA_Tonic_SensCon","EDA_Tonic_Medical"),
                    xaxis="mean", CI.type="boot", CI.level=.95,condition="Selection"),
             BAplot(data=signals[signals$Condition=="Nback",],measures=c("EDA_Tonic_SensCon","EDA_Tonic_Medical"),
                    xaxis="mean", CI.type="boot", CI.level=.95,condition="Nback"),
             BAplot(data=signals[signals$Condition=="Stroop",],measures=c("EDA_Tonic_SensCon","EDA_Tonic_Medical"),
                    xaxis="mean", CI.type="boot", CI.level=.95,condition="Stroop"),nrow=2)
ggsave("RESULTS/BAplots_EDA_Tonic.tiff",plot=p,dpi=300,width=16,height=8)
```

<br>

### 3.3.2. nsSCRs

Here, we generate the Bland-Altman plots for `PPGMean` measurements. Based on the output above, we log-transform the measures collected in the `"Stroop"` task prior to data analysis for achieving better normality. We can note uniform and nonsignificant bias only in the N-back and the Stroop task, whereas negative proportional biases were found in all the remaining tasks. Specifically, in the Seating, Standing, Walking, and Selection tasks \system{} showed the highest accuracy for lower rates of nsSCRs, that is when participants showed no or only a few responses, whereas it tended to underestimate larger nsSCRs measurements compared to the medical-grade device, possibly indicating false negatives. Higher nsSCR rates were also associated with wider LOAs (i.e., positive heteroscedasticity) for the Standing, N-back, and Stroop tasks, with the latter requiring logarithmic transformation to improve data normality \cite{euser2008practical}. Again, LOAs were found to overcome the range of measurement for certain values, and particularly for higher nsSCR rates.
```{r fig.width=12,fig.height=6}
p <- grid.arrange(BAplot(data=signals[signals$Condition=="Seating",],measures=c("SCRPeaks_SensCon","SCRPeaks_Medical"),
                    xaxis="mean", CI.type="boot", CI.level=.95, condition="Seating"),
             BAplot(data=signals[signals$Condition=="Standing",],measures=c("SCRPeaks_SensCon","SCRPeaks_Medical"),
                    xaxis="mean", CI.type="boot", CI.level=.95, condition="Standing"),
             BAplot(data=signals[signals$Condition=="Walking",],measures=c("SCRPeaks_SensCon","SCRPeaks_Medical"),
                    xaxis="mean", CI.type="boot", CI.level=.95, condition="Walking"),
             BAplot(data=signals[signals$Condition=="Selection",],measures=c("SCRPeaks_SensCon","SCRPeaks_Medical"),
                    xaxis="mean", CI.type="boot", CI.level=.95, condition="Selection"),
             BAplot(data=signals[signals$Condition=="Nback",],measures=c("SCRPeaks_SensCon","SCRPeaks_Medical"),
                    xaxis="mean", CI.type="boot", CI.level=.95, condition="Nback"),
             BAplot(data=signals[signals$Condition=="Stroop",],measures=c("SCRPeaks_SensCon","SCRPeaks_Medical"),
                    xaxis="mean", CI.type="boot", CI.level=.95, condition="Stroop", logTransf=TRUE),nrow=2)
ggsave("RESULTS/BAplots_SCRPeaks.tiff",plot=p,dpi=300,width=16,height=8)
```

<br>

### 3.3.3. HR

Here, we generate the Bland-Altman plots for `PPGMean` measurements. Based on the output above, we log-transform the measures collected in the `"Selection"` task prior to data analysis for achieving better normality. We can note that uniform and not significant biases were found in all tasks, with negligible systematic differences between systems ranging from 0.2 to 7.56 bpm. However, the random component of measurement error was relatively high (i.e., around $\pm$20-to-30 bpm). LOAs were uniform in most tasks, whereas negative heteroscedasticity was found in the N-back task, showing wider LOAs for lower HR measurements. Slighter heteroscedasticity also characterized the Selection task, following the logarithmic transformation of the data to achieve normality
```{r fig.width=12,fig.height=6}
p <- grid.arrange(BAplot(data=signals[signals$Condition=="Seating",],measures=c("PPGMean_SensCon","PPGMean_Medical"),
                    xaxis="mean", CI.type="boot", CI.level=.95,condition="Seating"),
             BAplot(data=signals[signals$Condition=="Standing",],measures=c("PPGMean_SensCon","PPGMean_Medical"),
                    xaxis="mean", CI.type="boot", CI.level=.95,condition="Standing"),
             BAplot(data=signals[signals$Condition=="Walking",],measures=c("PPGMean_SensCon","PPGMean_Medical"),
                    xaxis="mean", CI.type="boot", CI.level=.95,condition="Walking"),
             BAplot(data=signals[signals$Condition=="Selection",],measures=c("PPGMean_SensCon","PPGMean_Medical"),
                    xaxis="mean", CI.type="boot", CI.level=.95,condition="Selection",logTransf=TRUE),
             BAplot(data=signals[signals$Condition=="Nback",],measures=c("PPGMean_SensCon","PPGMean_Medical"),
                    xaxis="mean", CI.type="boot", CI.level=.95,condition="Nback"),
             BAplot(data=signals[signals$Condition=="Stroop",],measures=c("PPGMean_SensCon","PPGMean_Medical"),
                    xaxis="mean", CI.type="boot", CI.level=.95,condition="Stroop"),nrow=2)
ggsave("RESULTS/BAplots_PPGMean.tiff",plot=p,dpi=300,width=16,height=8)
```

<br>

# References {#ref}

- Altman, D. G., & Bland, J. M. (1983). Measurement in Medicine: The Analysis of Method Comparison Studies. The Statistician, 32(3), 307. https://doi.org/10.2307/2987937

- Bland, J. M., & Altman, D. G. (1986). Statistical Methods for Assessing Agreement Between Two Methods of Clinical Measurement. The Lancet, 327(8476), 307–310. https://doi.org/10.1016/S0140-6736(86)90837-8

- Bland, J. M., & Altman, D. G. (1999). Measuring agreement in method comparison studies. Statistical Methods in Medical Research, 8(2), 135–160. https://doi.org/10.1177/096228029900800204

- Euser, A. M., Dekker, F. W., & le Cessie, S. (2008). A practical approach to Bland-Altman plots and variation coefficients for log transformed variables. Journal of Clinical Epidemiology, 61(10), 978–982. https://doi.org/10.1016/j.jclinepi.2007.11.003

- Menghini, L., Cellini, N., Goldstone, A., Baker, F. C., & de Zambotti, M. (2020). A standardized framework for testing the performance of sleep-tracking technology: Step-by-step guidelines and open-source code. Sleep, Accepted M. https://doi.org/10.1093/sleep/zsaa170

## R packages

