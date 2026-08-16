# Minity

A Unity extension framework that includes features such as object pooling, scene routing, UI manager, and behavior trees.

## **Setup**

Unity Editor -> Package Manger -> Add package from git URL...

```
# Minity Core
https://github.com/MorizeroDev/Minity.git

# Milease Core
https://github.com/MorizeroDev/Milease.git

# Color Tools
https://github.com/ParaParty/ParaPartyUtil.git?path=Colors
```

Or including these in `manifest.json`:

```
"dev.milthm.minity": "https://github.com/MorizeroDev/Minity.git",
"com.morizero.milease": "https://github.com/MorizeroDev/Milease.git",
"party.para.util.colors": "https://github.com/ParaParty/ParaPartyUtil.git?path=Colors",
```

## **Resource Mapping by Enum Values**

Using enum values as identifiers for various resources enhances the maintainability of your project. For instance, when you're working with scene routing or object pooling, you can associate enum values with specific resources during the initialization phase, and later reference these resources by using the enum values.  

Additionally, Minity does not rely on the integer data of enum values. Even if you have two enums with identical integer values, Minity can still differentiate between them.

## **Object Pool**

Minity provides features like object pooling and automatic returning. Object pools are commonly used in Unity development to reduce the overhead of frequent creation and destruction of game objects, thus improving performance. 


Minity will also batch return any excess objects created during peak usage based on the current usage situation.

First, you need to attach the `PoolableObject` component to the prefab of the object type you want to pool, and configure its parameters.  

For example, you can set the objects to automatically return to the pool after being active for a certain period, or wait for manual recycling.

Next, use the following method to register a poolable object prefab and define the lifecycle of the created objects:
```csharp
ObjectPool.EnsurePrefabRegistered(EnumValue, Prefab, BaseCount);
```

To retrieve an object managed by the object pool, use the following method:
```csharp
ObjectPool.Request(EnumValue);
```

## **Scene Router**

Have you encountered a situation during game development where you need to return to a previous scene, but due to a special game process, an intermediary scene is inserted between two scenes that originally had a parent-child relationship? This might break the "back" functionality, preventing it from returning properly to the previous scene. Alternatively, manually specifying the scene name for the return can be problematic if you need to change a scene’s name later on, and you realize that it's referenced by strings all over your project, making refactoring a complex task.

The scene router was created to solve these issues. It also wraps loading animations, making it easier to use them during scene transitions.  

First, we need to configure the scene router:

```csharp
private enum SceneIdentifier
{
    TitleScreen, MainMenu, StoryMenu, Story
}

[RuntimeInitializeOnLoadMethod]
public static void SetupSceneRouter()
{
    SceneRouter.Setup(new SceneRouterConfig()
    {
        SceneNodes = new[]
        {
            SceneRouter.Root(SceneIdentifier.TitleScreen, "Title"),
            SceneRouter.Node(SceneIdentifier.MainMenu, "main", "Main"),
            SceneRouter.Node(SceneIdentifier.StoryMenu, "main/storymenu", "StoryMenu"),
            SceneRouter.Node(SceneIdentifier.Story, "main/storymenu/story", "Story")
        }
    });
}
```
This way, if you need to jump from `StoryMenu` to something like `CGScreen`, and then to `Story` due to a special game process, the `Story` scene will correctly return to its parent `StoryMenu`.

Use the following method to switch scenes:
```csharp
SceneRouter.GoTo(SceneIdentifier.MainMenu);
```

To quickly return to the previous scene, use:
```csharp
SceneRouter.Back();
```

Both methods will return a `SceneRouterContext`, which allows you to pass data between scenes using a fluent interface:
```csharp
SceneRouter.GoTo(SceneIdentifier.MainMenu).Parameters(data);
```

You can retrieve the data in another scene like this:
```csharp
SceneRouter.FetchParameters<Data>();
```

## **UI Manager**

Quickly build easy-to-use, managed UIs by implementing abstract classes `ManagedUIReturnValueOnly<T, R>`, `ManagedUI<T, P>`, and `ManagedUI<T, P, R>`.

In the design of Minity, we consider that each UI can have "parameters" and "return values", and the results returned by the "UI" are passed to subsequent processing functions via callbacks. Of course, we also provide asynchronous functions for your choice.

For generic parameters:

- `T`: A Type ID for the UI. Generally, you should specify this as the specific derived class.
- `P`: The Parameter type for the UI.
- `R`: The Return Value type for the UI.

Let's assume we have an `InputBox`, which pops up a window for the player to enter a name, then returns the entered name.

We can use it like this:

```c#
public class InputBox : ManagedUI<InputBox, string, string> {}
```

If you feel that using just `string` isn't intuitive enough, you can further encapsulate it:

```c#
public class InputBoxRequest {
    public string Title;
    public string Prompt;
}

public class InputBoxResponse {
    public string PlayerName;
}

public class InputBox : ManagedUI<InputBox, InputBoxRequest, InputBoxResponse> {}
```

Next, we need to bind it with the prefab at the point of initializing the UI manager:

```c#
[RuntimeInitializeOnLoadMethod]
public static void SetupUI()
{
    UIManager.Setup(new []
    {
        // Both methods are acceptable
        UI.FromPrefab(prefab),
        UI.FromResources("path/to/your/prefab"),
    });
}
```

Now, we can use the UI directly through any of the following methods:

```c#
InputBox.Open("Please enter your name", (name) => Debug.Log($"The player name is {name}"));

var playerName = await InputBox.OpenAsync("Please enter your name");
```

## **Binding View**

The **Binding View** provides the functionality to bind `TextMeshPro` text components to data. For example, you can create a class `DemoBindingView` that inherits from the abstract class `BindingView`.

We use `Binding<T>` to declare data with binding functionality.

```c#
public class DemoBindingView : BindingView
{
    // Specify Format Culture
    protected override CultureInfo Formatter { get; } = CultureInfo.CurrentCulture;

    private Binding<DateTime> time;
    private Binding<string> userName;
    private Binding<float> score;
    
    // Custom Data Formatter
    private Binding<DateTime> date = new((v) => v.ToShortDateString());

    protected override void Initialize()
    {
        score.Value = 100.23333f;
        userName.Value = "Buger404";
        time.Value = DateTime.Now;
        date.Value = DateTime.Now;
    }

    public void MakeSomeChanges()
    {
        score.Value = Random.Range(0f, 100f);
        time.Value = DateTime.Now;
        date.Value = DateTime.Now;
        
        // Apply operations to all text components bound to the score data
        score.Do((t) => t.color = Color.red);
    }
}
```

Next, attach this component to your Canvas, and you can freely use the declared data within that Canvas. Whenever the data changes, the text will automatically update.

> **Note:** If a text component is bound to multiple data fields, modifying multiple data fields simultaneously will not cause multiple updates. Instead, changes are batched and processed at the end of the frame. Similarly, making multiple modifications to the same data field within a frame will not cause redundant updates. (In other words, while updates have a slight delay, there are no visual issues.)

You can set your text content to something like:

```
Hello, I am {{ userName }}, the current time is {{ time:HH:mm:ss }}, and my current score is: {{ score:F2 }}
```

After initialization, the text will dynamically update to:

```
Hello, I am Buger404, the current time is 11:45:14, and my current score is: 100.233
```

The format for binding data is: `{{ FieldName:FormatString }}`, where the format string is optional. For instance, `{{ time:HH:mm:ss }}` is equivalent to:

```c#
yourTextComponent.text = time.ToString("HH:mm:ss");
```

Additionally, you do not need to manually instantiate empty `Binding<T>` instances. They will be automatically managed by Minity.

## **Custom Loading Animations**

You can create custom loading animations by extending `LoadingAnimator` and assigning it to the scene router.  

For example, here’s a default black fade transition that uses `Milease`, a lightweight animation library designed for Unity UI development:

```csharp
public class BlackFade : LoadingAnimator
{
    public Image Panel;

    public override void AboutToLoad()
    {
        MilInstantAnimator.Start(
            	0.5f / Panel.MQuad(x => x.color, Color.clear, Color.black)
        	)
            .Then(
                new Action(ReadyToLoad).AsMileaseKeyEvent()
            )
            .UsingResetMode(RuntimeAnimationPart.AnimationResetMode.ResetToInitialState)
            .PlayImmediately();
    }

    public override void OnLoaded()
    {
        MilInstantAnimator.Start(
            	0.5f / Panel.MQuad(x => x.color, Color.black, Color.clear)
        	)
            .Then(
                new Action(FinishLoading).AsMileaseKeyEvent()
            )
            .UsingResetMode(RuntimeAnimationPart.AnimationResetMode.ResetToInitialState)
            .PlayImmediately();
    }
}
```
In `AboutToLoad()`, you need to cover the screen with the animation and call `ReadyToLoad()` at the end of the animation to notify the scene router to begin loading.  

During loading, you can get the loading progress via the `base.Progress` property to update the screen.  

Once the scene is fully loaded, the router will call `OnLoaded()`, where you should play the closing animation and call `FinishLoading()` to inform the router that everything is complete.

You can then associate these loading animation prefabs with enum values in the scene router configuration and use them during scene transitions.


## **EventBus**

Minity's EventBus is a simple, type-driven global event system for Unity. It helps you decouple your code and wire up callbacks both in code and (where supported) in the Inspector.

**How it works:**

- Define your event argument class by implementing `IEventArgs` (an empty marker interface).
- Use `Subscribe<T>(Action<T>)` to lazily register and attach a listener—no explicit registration needed.
- Use `Unsubscribe<T>(Action<T>)` to remove a listener—if no subscribers remain, the event is automatically unregistered.
- Publish events with `EventBus.Instance.Publish<T>(args)`.

**Quick Example:**

1. Define your event args:

```csharp
[System.Serializable]
public class TestEventArgs : Minity.Event.IEventArgs
{
    [SerializeField] private string message;
    [SerializeField] private int count;

    public string Message => message;
    public int Count => count;

    public TestEventArgs(string msg, int cnt)
    {
        message = msg;
        count = cnt;
    }
}
```

2. Subscribe in your MonoBehaviour (lazy registration):

```csharp
void Awake()
{
    EventBus.Instance.Subscribe<TestEventArgs>(OnTest);
}

void OnTest(TestEventArgs args)
{
    Debug.Log($"Received: {args.Message}, Count: {args.Count}");
}
```

3. Publish events from anywhere:

```csharp
EventBus.Instance.Publish(new TestEventArgs("Hello I'm 179", 114514));
```

4. Unsubscribe when done (auto-unregisters if no subscribers remain):

```csharp
EventBus.Instance.Unsubscribe<TestEventArgs>(OnTest);
```

**Editor Support:**

- In the Editor, you can query registered event types for debugging.
- The `EventArgsPublishWindow` lets you create and publish events for testing.

**Tips:**

- `Subscribe` handles lazy registration—just subscribe and the event type is automatically created if needed.
- `Unsubscribe` automatically cleans up events with no remaining subscribers, saving memory.
- Only one registration per event type is allowed. Duplicate registrations log an error.
- Publishing an unregistered event type logs a warning (no exception).
- Make your event args `[System.Serializable]` for Inspector and tooling support.

EventBus gives you a clean, Inspector-friendly, type-safe way to broadcast events and keep your systems decoupled.
