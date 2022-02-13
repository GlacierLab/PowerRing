# PowerRing
聚能环？！让有限的供电功率换取更好的游戏性能！

## 工作逻辑
若`显卡功耗`持续大于`触发功耗`且`CPU占用`小于`释放占用`超过`检测时间`，则压制CPU功耗  
若压制功耗期间`CPU占用`大于`触发占用`，则立即解除CPU功耗压制  
若`显卡功耗`持续小于`释放功耗`超过`检测时间`，则解除CPU功耗压制  

## 适合的设备
使用`Nvidia`独显的散热或供电存在瓶颈的设备，如大部分笔记本  
没有独显的设备自己用`Throttle Stop`设置平衡就行了  
`AMD YES`平台为什么不用`Smart Shift`呢？  
请不要在散热供电充裕的台式机上使用本项目，会负优化  

## 使用方法
从[Release](https://github.com/GlacierLab/PowerRing/releases)下载解压后运行`Resona.exe`按照指引操作即可  

## 运行环境
本项目需要`.NET 6.0`，如果你没有运行时，请[点击这里](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-6.0.2-windows-x64-installer)下载安装  

## 开源依赖
`本项目由以下开源项目驱动`  
[.NET](https://github.com/dotnet)  
[LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor)  

## 闭源依赖
[ThrottleStop](https://www.techpowerup.com/download/techpowerup-throttlestop/)  
本程序依赖于`ThrottleStop`限制功耗  


## 工程名称为什么叫Resona
[大藏里想奈(Resona Ookura)](https://zh.moegirl.org.cn/%E5%A4%A7%E8%97%8F%E9%87%8C%E6%83%B3%E5%A5%88):`喜欢玩网游，并在网游中是站在顶峰的人，例如全服务器仅有五把的EX-咖喱棒都被她买断了。（根据露娜的出价，一把至少是200W日元）`