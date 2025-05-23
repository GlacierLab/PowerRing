# 聚能环PowerRing 2.x
聚能环？！让有限的供电功率换取更好的游戏性能！  
针对显卡瓶颈的游戏提高20%甚至更高的帧率  
[![](https://get.microsoft.com/images/en-us%20dark.svg)](https://apps.microsoft.com/detail/9N8BLRHH6JTV?mode=direct)  

## 工作逻辑
根据`刷新频率`自动持续监测  
若`显卡功耗`持续大于`触发功耗`且`CPU占用`小于`释放占用`超过`切换阈值`，则压制CPU功耗  
若压制功耗期间`CPU占用`大于`触发占用`，则立即解除CPU功耗压制  
若`显卡功耗`持续小于`释放功耗`超过`切换阈值`，则解除CPU功耗压制  
压制功耗方法为调整睿频策略  

## 适合的设备
散热或供电存在瓶颈的设备，如大部分笔记本  
请不要在散热供电充裕的台式机上使用本项目，会负优化  

## 为什么这玩意效果这么好
这就不得不提到`Nvidia`搞得`Dynamic Boost`了，这项技术可以在CPU功耗较低时动态的提高显卡的功耗墙以改善显卡性能  
但是我们都知道`Intel`的睿频会在轻负载下也跑到很高的频率，而高频能耗比一塌糊涂，导致轻负载下功耗也下不去  
所以我们需要在CPU没有性能瓶颈时适当限制CPU功耗，让显卡分配尽可能多的功耗来改善性能  
总的来说，本项目虽然不能压榨出更多的性能，但是可以让你更好的分配现有的性能  

## 使用方法
从[Release](https://github.com/GlacierLab/PowerRing/releases)下载解压后运行`Resona.exe`按照指引操作即可  

## 运行环境
本项目普通发布版本需要`.NET 9.0`，如果你没有运行时，请[点击这里](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)下载安装  

## 版本说明
普通发布版本体积较小，支持管理员启动模式，但需要安装.Net运行时  
MSIX版本体积较大，不支持管理员启动模式，但不需要安装运行时，更适合批量部署  

## 开源依赖
`本项目由以下开源项目驱动`  
[.NET](https://github.com/dotnet)  
[LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor)  
[小赖字体](https://github.com/lxgw/kose-font)  
[fontsubset](https://github.com/821869798/fontsubset) 本项目的构建流程包含了一份该项目的执行程序  

## 致谢
https://www.sakya.it/wordpress/enable-disable-cpu-turbo-boost-from-command-line/  

## 工程名称为什么叫Resona
[大藏里想奈(Resona Ookura)](https://zh.moegirl.org.cn/%E5%A4%A7%E8%97%8F%E9%87%8C%E6%83%B3%E5%A5%88):`喜欢玩网游，并在网游中是站在顶峰的人，例如全服务器仅有五把的EX-咖喱棒都被她买断了。（根据露娜的出价，一把至少是200W日元）`

## 许可
GPLv3 或 [琴梨梨通用许可](https://github.com/qinlili23333/QinliliUniversalLicense)  
本项目的首次发布日期为12/02/2022  

## 免责声明
本项目依赖的数据监控若在管理员模式下启动需要加载LibreHardwareMonitor的Ring0驱动。虽然在测试中这对游戏反作弊没有影响，但不排除极少数游戏反作弊排异反应的可能性  
琴梨梨亦无法保证此驱动绝对安全，尽管琴梨梨已尽可能对此库的源代码进行了安全审计，但若因为此驱动存在漏洞被利用，造成的后果琴梨梨概不负责  
若你不希望加载此驱动，请不要使用管理员身份启动  

## 特别说明
尽管本项目能提高游戏性能，但本项目并不能真正解决硬件问题，只是在软件层面上进行了功率分配优化  
本项目不能代替硬件修复和升级，如果你的硬件存在硅脂老化等各类问题，你仍然应该首先考虑解决硬件问题，你亦不应该因为本项目而选择购买电子垃圾  