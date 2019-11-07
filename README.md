# 图像变形程序

> 作者：高子靖
>
> 学号：2017010917
>
> 班级：自72
>
> Email: gaozj17@mails.tsinghua.edu.cn

## 项目简介

本项目为基于<font color="#666600">C#</font>的图像变形小程序，能够实现图像的扭曲变换，畸变校正，以及在给出关键点的图像对进行人脸变形。插值方法通过：<font color="#006666">最近邻插值</font>，<font color="#006666">双线性插值</font>和<font color="#006666">双三次插值</font>三种函数分别实现。

## 运行环境
- Windows 10 x64
- C# .NET Framework 4.6.1框架
- 使用WPF应用进行UI设计

## 库
- System.Drawing(Bitmap类作为图像读写接口)
- System.IO(实现文件读写)
- System.Windows.Media.Imaging
- Microsoft.Win32

## 操作说明

### 运行程序
- 双击`bin/Realease/`文件夹下的`Numercial_image1.exe`，便可以运行程序，获得如下所示的初始界面：



### 扭曲变换
