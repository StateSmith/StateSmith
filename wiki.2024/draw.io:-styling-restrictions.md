You can style the draw.io shapes however you want. StateSmith doesn't care **EXCEPT** for text nodes like below that specify the parent node's handlers.  

# Text Tab Is Safe
It is safe to change their font family, text color, size...

![style-restrictions-text-tab-2](https://user-images.githubusercontent.com/274012/229139784-11a52fe1-ca49-44be-a511-75292bb4c10b.gif)

# Style Tab Is Not Safe
Adding styling to the SHAPE of the text will cause the text node to look like a state in the draw.io XML and StateSmith will fail parsing it:

![style-restrictions-style-tab](https://user-images.githubusercontent.com/274012/229140638-480c66aa-532e-4b73-ac99-3acab41c8f7b.gif)

```
dotnet-script code_gen.csx

StateSmith Runner - Compiling file: `LightSm.drawio.svg` (no state machine name specified).Exception ArgumentException : Failed parsing node label
Parent path: `StateMachine{LightSm}.State{OFF}`
Label: `enter / window.alert("Light is OFF");`
Diagram id: `17`
Reason(s): no viable alternative at input ' /' at line 1 column 6. Offending symbol: `/`.  
           mismatched input '/' expecting <EOF> at line 1 column 6. Offending symbol: `/`. 
```

Those text nodes must specifically have have matching style elements (in any order): `fillColor=none;gradientColor=none;strokeColor=none;`.

Related: https://github.com/StateSmith/StateSmith/issues/191
