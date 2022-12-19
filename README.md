# TurtleRace


### 版本
- Unity2020.3.38f1c1
- VS2022 Professional
- Proto 3.0.0-win32
- ILRuntime 2.0.2
- wampserver 3.2.4.2
- .NetCore 3.1.0

### 环境配置
- 1. 
### 接口说明

- 1. 主工程向HotFix发消息。

```C#
appdomain.Invoke("HotFix.Main", "Init", gameObject, GameManager.present);
appdomain.Invoke("HotFix.Main", "Init", null, GameManager.present); //static方法
```

