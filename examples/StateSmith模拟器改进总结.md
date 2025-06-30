# StateSmith Web模拟器改进总结

## 问题描述

在原始的StateSmith Web模拟器中，右侧侧边栏的Events区域显示了所有可能的事件，但这会导致以下问题：
1. 当事件过多时，用户难以选择正确的事件
2. 从一个状态转移到另一个状态通常只使用少量特定事件
3. 无关的事件会干扰用户的操作体验

## 解决方案

实现了基于当前状态的事件动态显示功能，根据当前状态对无关事件进行淡化处理，让可用事件更加明显。

增加了"Hide Unused"复选框功能，允许用户选择完全隐藏无关事件，使界面更加简洁。

## 具体修改

### 1. SimWebGenerator.cs 修改

#### 1.1 添加状态事件映射数据结构
```csharp
/// <summary>
/// 状态到可用事件的映射
/// 键是状态名称，值是该状态可以处理的事件名称集合
/// </summary>
Dictionary<string, HashSet<string>> stateToAvailableEvents = new(StringComparer.OrdinalIgnoreCase);
```

#### 1.2 修改CollectDiagramNames方法
增强了该方法以收集每个状态的可用事件：
- 收集直接在该状态定义的事件
- 收集从父状态继承的事件
- 建立状态名称到可用事件的映射关系

#### 1.3 添加OrganizeStateEventsIntoJsObject方法
```csharp
/// <summary>
/// 将状态到可用事件的映射转换为JavaScript对象格式
/// </summary>
/// <returns>JavaScript对象格式的字符串</returns>
private string OrganizeStateEventsIntoJsObject()
```

#### 1.4 修改Generate方法
- 生成状态事件映射的JavaScript对象
- 将映射传递给HTML渲染器

### 2. HtmlRenderer.cs 修改

#### 2.1 修改Render方法签名
添加了`stateEventsMapping`参数：
```csharp
public static void Render(StringBuilder stringBuilder, string smName, string mocksCode, 
    string mermaidCode, string jsCode, string diagramEventNamesArray, string stateEventsMapping)
```

#### 2.2 添加CSS样式
为事件按钮添加了启用/禁用状态的视觉样式：
```css
button.event-button {
    transition: opacity 0.3s ease, background-color 0.3s ease;
}

button.event-button.disabled {
    opacity: 0.4;
    background-color: #f0f0f0;
    color: #999;
    cursor: not-allowed;
}

button.event-button.enabled {
    opacity: 1;
    background-color: #007bff;
    color: white;
    cursor: pointer;
}

button.event-button.enabled:hover {
    background-color: #0056b3;
}

/* 隐藏无关事件的样式 */
button.event-button.hidden {
    display: none;
}
```

#### 2.3 添加JavaScript功能

##### 状态事件映射变量
```javascript
// 状态到可用事件的映射
const stateEventsMapping = {{stateEventsMapping}};
```

##### updateEventButtonStates函数
```javascript
// 更新事件按钮状态的函数
function updateEventButtonStates(currentStateName) {
    const availableEvents = stateEventsMapping[currentStateName] || [];
    const hideIrrelevantEvents = document.getElementById('hideIrrelevantEvents').checked;
    
    diagramEventNamesArray.forEach(eventName => {
        const button = document.getElementById('button_' + eventName);
        if (button) {
            // do事件始终保持启用状态，因为它是状态机运行的核心事件
            const isDoEvent = eventName.toLowerCase() === 'do';
            const isAvailable = isDoEvent || availableEvents.includes(eventName);
            
            // 清除所有状态类
            button.classList.remove('enabled', 'disabled', 'hidden');
            
            if (hideIrrelevantEvents && !isAvailable) {
                // 如果选中了隐藏无关事件且该事件不可用，则隐藏按钮
                button.classList.add('hidden');
                button.disabled = true;
            } else {
                // 否则显示按钮并设置相应状态
                button.classList.add(isAvailable ? 'enabled' : 'disabled');
                button.disabled = !isAvailable;
            }
        }
    });
}
```

##### 新增设置选项
在设置下拉菜单中添加了第二个复选框：
```html
<div class='dropdown-item'>
  <input type='checkbox' id='hideIrrelevantEvents' name='hideIrrelevantEvents' value='Hide Unused'>
  <label for='hideIrrelevantEvents'>Hide Unused</label>
</div>
```

##### 新增事件监听器
```javascript
// 设置隐藏无关事件复选框的状态和事件监听器
document.getElementById('hideIrrelevantEvents').addEventListener('change', function() {
  // 当复选框状态改变时，更新当前状态的事件按钮显示
  const currentStateName = {{smName}}.stateIdToString(sm.stateId);
  updateEventButtonStates(currentStateName);
});
```

##### 修改事件按钮创建
- 为事件按钮添加`event-button` CSS类
- 添加禁用状态下的点击保护

##### 修改状态进入回调
在`enterState`回调中添加事件按钮状态更新：
```javascript
enterState: (mermaidName) => {
    // ... 原有代码 ...
    
    // 更新事件按钮状态
    updateEventButtonStates(mermaidName);
},
```

##### 添加初始化代码
```javascript
// 初始化事件按钮状态
const initialStateName = {{smName}}.stateIdToString(sm.stateId);
updateEventButtonStates(initialStateName);
```

## 功能特性

### 1. 智能事件过滤
- 根据当前状态动态显示可用事件
- 无关事件被淡化处理（透明度降低，颜色变灰）
- 可用事件突出显示（蓝色背景，完全不透明）

### 2. 新增：隐藏无关事件功能
- **Hide Unused复选框**：用户可以选择完全隐藏无关事件
- **两种显示模式**：
  - 默认模式（复选框未选中）：无关事件淡化显示
  - 简洁模式（复选框选中）：无关事件完全隐藏


### 3. do事件特殊处理
- `do`事件始终保持启用状态


### 4. 交互保护
- 禁用的按钮无法被点击


## 测试验证

使用`test-misc/c99-test/electromagnet.puml`进行了测试验证：
- 成功生成包含状态事件映射的模拟器
- 事件按钮根据当前状态正确更新
- do事件始终保持可用状态
- Hide Unused复选框功能正常工作
- 两种显示模式切换正常
- 视觉效果符合预期

## 使用方法

使用CLI工具生成模拟器：
```bash
cd src
dotnet run --project StateSmith.Cli --framework net9.0 -- run your_state_machine.puml
```

生成的`.sim.html`文件将包含新的智能事件过滤功能。

### 操作说明
1. 打开生成的模拟器HTML文件
2. 点击右上角的"settings"图标
3. 可以看到两个复选框选项：
   - **Timestamps**：控制是否显示时间戳
   - **Hide Unused**：控制是否隐藏无关事件
4. 选中"Hide Unused"复选框可完全隐藏当前状态下不可用的事件

