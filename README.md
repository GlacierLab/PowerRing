# PowerRing
聚能环？！让有限的供电功率换取更好的游戏性能！

## 工作逻辑
若`显卡功耗`持续大于`触发功耗`且`CPU占用`小于`释放占用`超过`检测时间`，则压制CPU功耗  
若压制功耗期间`CPU占用`大于`触发占用`，则立即解除CPU功耗压制  
若`显卡功耗`持续小于`释放功耗`超过`检测时间`，则解除CPU功耗压制  

## 开源依赖
`本项目由以下开源项目驱动`  
[.NET](https://github.com/dotnet)  
[LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor)  


## 工程名称为什么叫Resona
[大藏里想奈(Resona Ookura)](https://zh.moegirl.org.cn/%E5%A4%A7%E8%97%8F%E9%87%8C%E6%83%B3%E5%A5%88):`喜欢玩网游，并在网游中是站在顶峰的人，例如全服务器仅有五把的EX-咖喱棒都被她买断了。（根据露娜的出价，一把至少是200W日元）`