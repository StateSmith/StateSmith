using System.Text;

namespace StateSmith.Output.Sim;

/// <summary>
/// HTML渲染器
/// 负责生成状态机模拟器的HTML页面
/// </summary>
public class HtmlRenderer
{
    /// <summary>
    /// 渲染HTML模拟器页面
    /// </summary>
    /// <param name="stringBuilder">用于构建HTML内容的字符串构建器</param>
    /// <param name="smName">状态机名称</param>
    /// <param name="mocksCode">模拟代码</param>
    /// <param name="mermaidCode">Mermaid图表代码</param>
    /// <param name="jsCode">JavaScript状态机代码</param>
    /// <param name="diagramEventNamesArray">图表事件名称数组</param>
    /// <param name="stateEventsMapping">状态到可用事件的映射</param>
    public static void Render(StringBuilder stringBuilder, string smName, string mocksCode, string mermaidCode, string jsCode, string diagramEventNamesArray, string stateEventsMapping)
    {
        // 现在我们在StateSmith项目内部工作，需要限制自己使用dotnet 6功能。
        // 我们不能再使用"""原始字符串"""，所以下面进行手动字符串插值。
        // 另外，在下面的字符串中，我们必须使用`""`来转义双引号。我已经怀念原始字符串了...
        string htmlTemplate = @"<!-- 
  -- 此文件由StateSmith生成。
  -- 它作为如何在网页中使用生成的状态机的示例。
  -- 它也作为一个交互式控制台，您可以用来验证状态机的行为。
  --
  -- 使用{{smName}}.js通常如下所示：
  --   var sm = new {{smName}}();
  --   sm.start();
  --
  -- 然后使用sm.dispatchEvent()将事件分派给状态机。
  -->
<html>
  <head>
    <link rel='icon' type='image/png' href='https://statesmith.github.io/favicon.png'>
    <link rel='stylesheet' href='https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined'>
    <style>
      body {
        display: flex;
        flex-direction: row;
        margin: 0px;
      }

      /* 修复mermaid内容需要滚动条的问题 https://github.com/StateSmith/StateSmith/issues/288 */
      pre.mermaid {
        margin: 0px;
      }

      .wrapper {
        height: 100vh;
        width: 100vw;
        display: flex;
      }

      .pane {
        padding: 1em;
        min-width: 200px;
      }

      .titlebar-icon {
        font-family: 'Material Symbols Outlined', sans-serif;
        font-size: 16px;
        color: #777;
        border-radius: 5px;
      }

      .gutter {
        width: 10px;
        height: 100%;
        background: #ccc;
        position: absolute;
        top: 0;
        left: 0;
        cursor: col-resize;
      }

      .main {
        flex: 1;
        overflow: auto;
        padding: 10px;
      }

      .sidebar {
        width: 300px;
        padding-top: 0px;
        position: relative;
        background-color: #f0f0f0;
        border-left: 1px solid #ccc;
        display: flex;
        flex-direction: column;
      }

      #buttons {
        display: flex;
        flex-direction: column;
      }

      .titlebar {
        background-color: #ddd;
        border-bottom: 1px solid #ccc;
        font-weight: bold;
        padding: 5px;
        display: flex;
      }

      .console {
        border-collapse: collapse;
        margin-top: 10px;
        width: 100%;
      }

      table.console td.timestamp {
        display: none;
      }

      table.console.timestamps td.timestamp {
        display: table-cell;
      }

      table.console td {
          color: rgba(0, 0, 0, 0.7);
      }

      table.console td .dispatched {
          font-weight: bold;
          color: rgba(0, 0, 0, 1);
      }

      table.console tr:has(+tr td .dispatched) {
          border-bottom: 0px;
      }

      table.console tr:has(+tr td .dispatched) td {
          padding-bottom: 25px;
      }

      .console th {
        background-color: #f0f0f0;
        border-bottom: 1px solid #ccc;
        font-weight: normal;
        padding: 5px;
        text-align: left;
      }

      .console tbody {
        font-family: monospace;
      }

      .console tr {
        border-bottom: 1px solid #ccc;
      }

      .console td {
        padding: 5px;
      }
  
      .console td.timestamp {
        font-size: small;
      }

      .history {
        margin-top: 30px;       
        display: flex;
        overflow: auto;    
        flex-direction: column-reverse;
      }

      .console tr:last-child td {
        border-bottom: none;
      }

      .dispatched {
        font-weight: bold;
      }

      .dispatched > .trigger {
        border: 1px solid #000;
        border-radius: 4px;
        padding: 2px 10px 2px 10px;
      }

      button {
        margin: 5px;
      }

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

      button.event-button.hidden {
        display: none;
      }

      .dropbtn {
        border: none;
        cursor: pointer;
      }

      .dropbtn:hover, .dropbtn:focus {
        background-color: #f1f1f1;
      }

      .dropdown {
        position: relative;
        display: inline-block;
        margin-left: auto;
      }

      .dropdown-content {
        display: none;
        position: absolute;
        right: 0;
        background-color: #f1f1f1;
        min-width: 160px;
        overflow: auto;
        box-shadow: 0px 8px 16px 0px rgba(0,0,0,0.2);
        z-index: 1;
      }

      .dropdown-content .dropdown-item {
        padding: 12px 16px;
        font-weight: normal;
      }

      .show {display: block;}

      .transition.active {
        stroke: #fff5ad !important;
        stroke-width: 5px !important;
        filter: drop-shadow( 3px 3px 2px rgba(0, 0, 0, .7));
      }

      .statediagram-state.active > * {
        fill: #fff5ad !important;
        stroke-width: 2px !important;
      }

    </style>
  </head>

  <body>
    <div class=""wrapper"">
    <div class=""pane main"">
        <pre class=""mermaid"">
{{mermaidCode}}
        </pre>
    </div>

    <div class=""pane sidebar"">
        <div id=""buttons"">
            <div class=""titlebar"">Events            
              <div class='dropdown'>
                <span id='dropbtn' class='titlebar-icon dropbtn'>settings</span>
                <div id='myDropdown' class='dropdown-content'>
                  <div class='dropdown-item'>
                    <input type='checkbox' id='timestamps' name='timestamps' value='Timestamps'>
                    <label for='timestamps'>Timestamps</label>
                  </div>
                  <div class='dropdown-item'>
                    <input type='checkbox' id='hideIrrelevantEvents' name='hideIrrelevantEvents' value='Hide Unused'>
                    <label for='hideIrrelevantEvents'>Hide Unused</label>
                  </div>
                </div>
              </div>            
          </div>
        </div>

        <div class=""history"">
          <table class=""console"">
            <tbody>
            </tbody>
          </table>
        </div>

        <div class=""gutter""></div>
    </div>
    </div>

<script>
{{jsCode}}
</script>

    <script type=""module"">
        // 导入mermaid和svg-pan-zoom库
        import mermaid from 'https://cdn.jsdelivr.net/npm/mermaid@10/dist/mermaid.esm.min.mjs';
        import svgPanZoom from 'https://cdn.jsdelivr.net/npm/svg-pan-zoom@3.6.1/+esm' ;
        
        // 初始化mermaid，但不自动启动
        mermaid.initialize({ startOnLoad: false });
        await mermaid.run();

        // svg-pan-zoom不喜欢mermaid的viewbox属性
        document.querySelector('svg').removeAttribute('viewBox');
        document.querySelector('svg').setAttribute('width', '100%');
        document.querySelector('svg').setAttribute('height', '100%');
        document.querySelector('svg').style[""max-width""] = '';

        // 当我们缩放转换边缘时，不要缩放箭头
        document.querySelectorAll('g defs marker[id$=barbEnd]').forEach(marker => {
            marker.setAttribute('markerUnits', 'userSpaceOnUse');
        });

        // https://github.com/StateSmith/StateSmith/issues/404
        // https://github.com/StateSmith/StateSmith/issues/294
        // 将$initial_state重写为黑色圆圈
        document.querySelectorAll('g[data-id*=""(InitialState)""]').forEach(g=> {
          g.innerHTML = '<circle transform=""translate(0,3)"" height=""14"" width=""14"" r=""14"" class=""state - start""></circle>';
        })

        // 初始化svg平移和缩放功能
        var panZoom = window.panZoom = svgPanZoom(document.querySelector('svg'), {
            zoomEnabled: true,
            controlIconsEnabled: true,
            fit: true,
            center: true
        });

        // 图表事件名称数组
        const diagramEventNamesArray = {{diagramEventNamesArray}};

        // 状态到可用事件的映射
        const stateEventsMapping = {{stateEventsMapping}};

        // 获取页面元素引用
        const leftPane = document.querySelector("".main"");
        const rightPane = document.querySelector("".sidebar"");
        const gutter = document.querySelector("".gutter"");

        // 调整窗格大小的函数
        function resizer(e) {          
          window.addEventListener('mousemove', mousemove);
          window.addEventListener('mouseup', mouseup);          
          let prevX = e.x;
          const rightPanel = rightPane.getBoundingClientRect();
                    
          function mousemove(e) {
            let newX = prevX - e.x;
            rightPane.style.width = rightPanel.width + newX + 'px';
            window.panZoom.resize();
            window.panZoom.fit();
            window.panZoom.center();
          }
          
          function mouseup() {
            window.removeEventListener('mousemove', mousemove);
            window.removeEventListener('mouseup', mouseup);
            
          }                  
        }

        // 为调整器添加鼠标按下事件监听器
        gutter.addEventListener('mousedown', resizer);

        // 设置时间戳复选框的状态
        document.getElementById('timestamps').checked = document.querySelector('table.console').classList.contains('timestamps');
        document.getElementById('timestamps').addEventListener('change', function() {
          if(this.checked) {
            document.querySelector('table.console').classList.add('timestamps');
          } else {
            document.querySelector('table.console').classList.remove('timestamps');
          }
        });

        // 设置隐藏无关事件复选框的状态和事件监听器
        document.getElementById('hideIrrelevantEvents').addEventListener('change', function() {
          // 当复选框状态改变时，更新当前状态的事件按钮显示
          const currentStateName = {{smName}}.stateIdToString(sm.stateId);
          updateEventButtonStates(currentStateName);
        });

        // 为下拉菜单按钮添加点击事件监听器
        document.getElementById('dropbtn').addEventListener('click', myFunction);

        /* 当用户点击按钮时，在隐藏和显示下拉内容之间切换 */
        function myFunction() {
          document.getElementById('myDropdown').classList.toggle('show');
        }

        // 如果用户点击下拉菜单外部，则关闭下拉菜单
        window.onclick = function(event) {
          if (!event.target.matches('.dropbtn')) {
            var dropdowns = document.getElementsByClassName('dropdown-content');
            var i;
            for (i = 0; i < dropdowns.length; i++) {
              var openDropdown = dropdowns[i];
              if (openDropdown.classList.contains('show')) {
                openDropdown.classList.remove('show');
              }
            }
          }
        }

{{mocksCode}}

        // 将日期转换为HH:MM:SS.sss格式的字符串
        function formatTime(date) {
            return date.getHours().toString().padStart(2, '0') + ':' +
                date.getMinutes().toString().padStart(2, '0') + ':' +
                date.getSeconds().toString().padStart(2, '0') + '.' +
                date.getMilliseconds().toString().padStart(3, '0');
        }

        // 向历史记录表添加一行
        function addHistoryRow(time, event, html = false) {
            var row = document.createElement('tr');
            var timeCell = document.createElement('td');
            timeCell.innerText = formatTime(time);
            timeCell.classList.add('timestamp');
            var eventCell = document.createElement('td');

            if(html) {
              eventCell.innerHTML = event;
            } else {
              eventCell.innerText = event;
            }

            row.appendChild(timeCell);
            row.appendChild(eventCell);
            document.querySelector('tbody').appendChild(row);
        }

        // 创建状态机实例
        var sm = new {{smName}}();

        // 提示用户手动评估守卫条件
        sm.evaluateGuard = (vertexName, behaviorUml) => {
            return confirm(`Evaluate guard for\n${vertexName} behavior:\n${behaviorUml}.\n\nPress 'OK' to evaluate guard as true and 'Cancel' to evaluate it as false.`);
        }; 

        // 高亮显示的边缘集合
        const highlightedEdges = new Set();
        
        // 高亮显示边缘的函数
        function highlightEdge(edgeId) {
            var edge = document.getElementById(edgeId);
            if (edge) {
              edge.classList.add('active');
              highlightedEdges.add(edge);
            }
        }

        // 清除高亮显示边缘的函数
        function clearHighlightedEdges() {
            for (const edge of highlightedEdges) {
              edge.classList.remove('active');
              const showOldTraversal = false;
              if (showOldTraversal) {
                  // 显示边缘已被遍历。可选的，但很不错。
                  edge.style.stroke = 'green';
              }
            }
            highlightedEdges.clear();
        }

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

        // 模拟器使用跟踪器回调来执行状态高亮和日志记录等操作。
        // 在您自己的应用程序中使用{{smName}}.js时，您不需要此功能，
        // 尽管您可以选择实现跟踪器用于调试目的。
        sm.tracer = {
            // 进入状态时的回调
            enterState: (mermaidName) => {
                var e = document.querySelector('g[data-id=' + mermaidName + ']');
                if(e) {
                  e.classList.add('active');
                  panOnScreen(e);
                }
                sm.tracer.log('➡️ Entered ' + mermaidName);
                
                // 更新事件按钮状态
                updateEventButtonStates(mermaidName);
            },
            // 退出状态时的回调
            exitState: (mermaidName) => {
                document.querySelector('g[data-id=' + mermaidName + ']')?.classList.remove('active');
            },
            // 边缘转换时的回调
            edgeTransition: (edgeId) => {
                highlightEdge(edgeId);
            },
            // 日志记录回调
            log: (message, html=false) => {
                addHistoryRow(new Date(), message, html);
            }
        };

        // 为状态机事件分派连接按钮
        diagramEventNamesArray.forEach(diagramEventName => {
            var button = document.createElement('button');
            button.id = 'button_' + diagramEventName;
            button.className = 'event-button';
            button.innerText = diagramEventName;
            button.addEventListener('click', () => {
                // 只有在按钮启用时才处理点击事件
                if (!button.disabled) {
                    clearHighlightedEdges();
                    sm.tracer?.log('<span class=""dispatched""><span class=""trigger"">' + diagramEventName + '</span> DISPATCHED</span>', true);
                    const fsmEventName = diagramEventName.toUpperCase();
                    sm.dispatchEvent({{smName}}.EventId[fsmEventName]); 
                }
            });
            document.getElementById('buttons').appendChild(button);
        });

        // 记录启动日志并启动状态机
        sm.tracer?.log('<span class=""dispatched"">START</span>', true);
        sm.start();

        // 初始化事件按钮状态
        const initialStateName = {{smName}}.stateIdToString(sm.stateId);
        updateEventButtonStates(initialStateName);

        // 将元素平移到屏幕可见区域的函数
        function panOnScreen(element) {
          if(!element) return;

          var bounds = element.getBoundingClientRect();
          // 如果元素在屏幕左侧或上方，向右下平移
          if(bounds.x<0 || bounds.y<0) {
              var x = Math.max(0, -bounds.x + 20);
              var y = Math.max(0, -bounds.y + 20);
              window.panZoom.panBy({x: x, y: y});
          }
          var panebounds = document.querySelector('svg').getBoundingClientRect();
          // 如果元素在屏幕右侧或下方，向左上平移
          if(bounds.x>panebounds.width || bounds.y>panebounds.height) {
              var x = Math.min(0, panebounds.width - bounds.x - bounds.width - 20);
              var y = Math.min(0, panebounds.height - bounds.y - bounds.height - 20);
              window.panZoom.panBy({x: x, y: y});
          }
        }
    </script>


  </body>
</html>";

        // 替换HTML模板中的占位符
        htmlTemplate = htmlTemplate.Replace("{{mermaidCode}}", mermaidCode);
        htmlTemplate = htmlTemplate.Replace("{{jsCode}}", jsCode);
        htmlTemplate = htmlTemplate.Replace("{{mocksCode}}", mocksCode);
        htmlTemplate = htmlTemplate.Replace("{{smName}}", smName);
        htmlTemplate = htmlTemplate.Replace("{{diagramEventNamesArray}}", diagramEventNamesArray);
        htmlTemplate = htmlTemplate.Replace("{{stateEventsMapping}}", stateEventsMapping);
        stringBuilder.AppendLine(htmlTemplate);
    }
}
