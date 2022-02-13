# 聚能环PowerRing
聚能环？！让有限的供电功率换取更好的游戏性能！  
针对显卡瓶颈的游戏提高20%甚至更高的帧率  

## 工作逻辑
若`显卡功耗`持续大于`触发功耗`且`CPU占用`小于`释放占用`超过`检测时间`，则压制CPU功耗  
若压制功耗期间`CPU占用`大于`触发占用`，则立即解除CPU功耗压制  
若`显卡功耗`持续小于`释放功耗`超过`检测时间`，则解除CPU功耗压制  

## 适合的设备
使用`Nvidia`独显的散热或供电存在瓶颈的设备，如大部分笔记本  
没有独显的设备自己用`Throttle Stop`设置平衡就行了  
`AMD YES`平台为什么不用`Smart Shift`呢？  
请不要在散热供电充裕的台式机上使用本项目，会负优化  

## 为什么这玩意效果这么好
这就不得不提到`Nvidia`搞得`Dynamic Boost`了，这项技术可以在CPU功耗较低时动态的提高显卡的功耗墙以改善显卡性能  
但是我们都知道`Intel`的睿频会在轻负载下也跑到很高的频率，而高频能耗比一塌糊涂，导致轻负载下功耗也下不去  
所以我们需要在CPU没有性能瓶颈时适当限制CPU功耗，让显卡分配尽可能多的功耗来改善性能  

## 使用方法
从[Release](https://github.com/GlacierLab/PowerRing/releases)下载解压后运行`Resona.exe`按照指引操作即可  

## 运行环境
本项目需要`.NET 6.0`，如果你没有运行时，请[点击这里](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-6.0.2-windows-x64-installer)下载安装  

## 为什么本项目需要管理员权限
废话，没管理员权限改你妈功耗墙啊  

## 托盘出现了一大堆温度图标
这些是`Throttle Stop`的图标，由于`Windows`杀进程不会立即清除托盘图标，虽然本项目自动清理过期的进程，但仍然会遗留大量图标。不过不用担心，鼠标滑过你就会发现图标全消失了。  

## 开源依赖
`本项目由以下开源项目驱动`  
[.NET](https://github.com/dotnet)  
[LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor)  

## 闭源依赖
[ThrottleStop](https://www.techpowerup.com/download/techpowerup-throttlestop/)  
本程序依赖于`ThrottleStop`限制功耗  


## 工程名称为什么叫Resona
[大藏里想奈(Resona Ookura)](https://zh.moegirl.org.cn/%E5%A4%A7%E8%97%8F%E9%87%8C%E6%83%B3%E5%A5%88):`喜欢玩网游，并在网游中是站在顶峰的人，例如全服务器仅有五把的EX-咖喱棒都被她买断了。（根据露娜的出价，一把至少是200W日元）`