# Milutools

**Mil**thm **u**nity **tools**, a collection of commonly used Unity features, such as object pool, scene router, sound player, and more.

## Setup

Unity Editor -> Package Manger -> Add package from git URL...

```
# Milutools Core
https://github.com/MorizeroDev/Milutools.git

# Milease Core
https://github.com/MorizeroDev/Milease.git

# Color Tools
https://github.com/ParaParty/ParaPartyUtil.git?path=Colors
```

Or including these in `manifest.json`:

```
"com.morizero.milutools": "https://github.com/MorizeroDev/Milutools.git",
"com.morizero.milease": "https://github.com/MorizeroDev/Milease.git",
"party.para.util.colors": "https://github.com/ParaParty/ParaPartyUtil.git?path=Colors",
```

## **Resource Mapping by Enum Values**

Using enum values as identifiers for various resources enhances the maintainability of your project. For instance, when you're working with scene routing or object pooling, you can associate enum values with specific resources during the initialization phase, and later reference these resources by using the enum values.  

Additionally, Milutools does not rely on the integer data of enum values. Even if you have two enums with identical integer values, Milutools can still differentiate between them.

## **Object Pool**

Milutools provides features like object pooling and automatic recycling. Object pools are commonly used in Unity development to reduce the overhead of frequent creation and destruction of game objects, thus improving performance. 


Milutools will also batch recycle any excess objects created during peak usage based on the current usage situation.

First, you need to attach the `RecycableObject` component to the prefab of the object type you want to pool, and configure its parameters.  

For example, you can set the objects to automatically return to the pool after being active for a certain period, or wait for manual recycling.

Next, use the following method to register a recyclable object prefab and define the lifecycle of the created objects:
```csharp
RecyclePool.EnsurePrefabRegistered(EnumValue, Prefab, BaseCount);
```

To retrieve an object managed by the object pool, use the following method:
```csharp
RecyclePool.Request(EnumValue);
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

## **Custom Loading Animations**

You can create custom loading animations by extending `LoadingAnimator` and assigning it to the scene router.  

For example, here’s a default black fade transition that uses `Milease`, a lightweight animation library designed for Unity UI development:

```csharp
public class BlackFade : LoadingAnimator
{
    public Image Panel;

    public override void AboutToLoad()
    {
        Panel.Milease(UMN.Color, Color.clear, Color.black, 0.5f)
            .Then(
                new Action(ReadyToLoad).AsMileaseKeyEvent()
            )
            .UsingResetMode(RuntimeAnimationPart.AnimationResetMode.ResetToInitialState)
            .PlayImmediately();
    }

    public override void OnLoaded()
    {
        Panel.Milease(UMN.Color, Color.black, Color.clear, 0.5f)
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
