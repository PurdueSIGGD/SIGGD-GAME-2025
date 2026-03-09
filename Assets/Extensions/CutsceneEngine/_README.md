# Cutscene Engine

A Timeline-integrated cutscene authoring system for Unity. Actors are bound to tracks, clips carry typed actions, and the whole pipeline supports real-time scrubbing, clip-length-driven interpolation, and zero-code parameterization from the Inspector.

---

## Table of Contents

1. [Overview](#overview)
2. [Quick Start — End to End](#quick-start--end-to-end)
3. [Core Concepts](#core-concepts)
   - [Actors and the ICutsceneActor Interface](#actors-and-the-icutsceneactor-interface)
   - [The [CutsceneAction] Attribute](#the-cutsceneaction-attribute)
   - [Action Execution Modes](#action-execution-modes)
   - [The CutsceneContext](#the-cutscenecontext)
   - [Built-in Actions](#built-in-actions)
4. [Editor Tooling](#editor-tooling)
   - [Cutscene Editor Window](#cutscene-editor-window)
   - [Clip Inspector](#clip-inspector)
   - [Validation](#validation)
5. [User Guide](#user-guide)
   - [Making an Actor](#making-an-actor)
   - [Setting Up a Scene](#setting-up-a-scene)
   - [Authoring Clips in Timeline](#authoring-clips-in-timeline)
   - [Using Built-in Actions](#using-built-in-actions)
   - [Writing Custom [CutsceneAction] Methods](#writing-custom-cutsceneaction-methods)
   - [Playing and Stopping Cutscenes at Runtime](#playing-and-stopping-cutscenes-at-runtime)
   - [Checking if a Cutscene is Active](#checking-if-a-cutscene-is-active)
6. [Best Practices](#best-practices)
7. [System Architecture](#system-architecture)
   - [Runtime Pipeline](#runtime-pipeline)
   - [Class Reference](#class-reference)
   - [The Motion System](#the-motion-system)
   - [The Animation System](#the-animation-system)
   - [The Camera System](#the-camera-system)
   - [Serialization of Actions and Parameters](#serialization-of-actions-and-parameters)
8. [Extending the System](#extending-the-system)
   - [Custom ICutsceneAction](#custom-icutsceneaction)
   - [Custom IMotionSystem](#custom-imotionsystem)
   - [Custom IAnimationSystem](#custom-ianimationsystem)
   - [Custom ICameraSystem](#custom-icamerasystem)

---

## Overview

The Cutscene Engine sits on top of Unity's **Timeline / Playables** system. You do not replace Timeline — you extend it. Every track in your Timeline is bound to an actor (any MonoBehaviour that implements `ICutsceneActor`). Every clip on that track carries one `ICutsceneAction` object, which is configured entirely through the Inspector.

Actions have three lifecycle hooks that mirror Timeline's own clip lifecycle:

| Hook | When it fires |
|---|---|
| `OnEnter` | Once, when the playhead enters the clip |
| `OnUpdate` | Every frame the clip is active, with `normalizedTime` (0 → 1) and `deltaTime` |
| `OnExit` | Once, when the playhead leaves the clip |

Because `OnUpdate` receives a `normalizedTime`, every continuous action — movement, rotation, camera moves — responds correctly to scrubbing the playhead in both edit mode and play mode.

---

## Quick Start — End to End

This walks through creating a simple cutscene where an enemy walks to a point, then the camera shakes.

### 1. Scene setup

1. Create a **GameObject** with a `PlayableDirector` component. Add a `CutsceneDirector` component to the same GameObject.
2. In the `CutsceneDirector` inspector, assign the `PlayableDirector` reference and, optionally, a `CinemachineCamera` for camera actions.
3. Create a **TimelineAsset** (`Create > Timeline`) and assign it to the `PlayableDirector`'s *Playable Asset* field.

### 2. Make an actor

Add `CutsceneActorProxy` (the built-in default actor) to any GameObject you want to animate, or write your own (see [Making an Actor](#making-an-actor)).

### 3. Add a track

Open the Timeline window. Right-click in the track area → **Add → Cutscene Action Track**. Drag the actor GameObject into the binding slot that appears on the left side of the track.

### 4. Add a clip

With the track selected, right-click in the clip area → **Add Cutscene Action Clip**, or use the **Cutscene Editor Window** (`Window → Cutscene Engine → Cutscene Editor`) to add one with a single click.

### 5. Configure the clip

Select the clip. In the Inspector you will see the **Cutscene Action Configuration** panel.

- Choose **Action Type** — for example *Move Actor*.
- Fields for that action appear immediately. For *Move Actor*, set the **Target** world position.

### 6. Play

Call `cutsceneDirector.Play()` at runtime (e.g. from a trigger or a button). The Timeline plays, the actor moves.

---

## Core Concepts

### Actors and the ICutsceneActor Interface

An actor is any `MonoBehaviour` that implements `ICutsceneActor`:

```csharp
public interface ICutsceneActor : ICutsceneOverridable
{
    Transform GetTransform();
}

public interface ICutsceneOverridable
{
    CutsceneActionAdapter GetCutsceneAdapter();
    void OnCutsceneEnter();
    void OnCutsceneExit();
}
```

- **`GetTransform()`** — Returns the actor's `Transform`. Used by all movement and rotation actions.
- **`GetCutsceneAdapter()`** — Returns a `CutsceneActionAdapter` built from `this`. The adapter scans the object for methods tagged with `[CutsceneAction]` and makes them available as named, invocable entries in the clip inspector.
- **`OnCutsceneEnter()`** / **`OnCutsceneExit()`** — Lifecycle hooks called by `CutsceneDirector` at the start and end of the whole cutscene. Use them to pause AI, disable input, reset physics, etc.

The built-in `CutsceneActorProxy` is a minimal MonoBehaviour that ships with two demo actions (`WaveHello` and `SaySomething`). It is a good reference for the minimum viable actor.

---

### The [CutsceneAction] Attribute

Mark any method on your actor with `[CutsceneAction]` to expose it in the clip inspector:

```csharp
// One-shot: fires once when the clip starts
[CutsceneAction("Play Attack")]
public void PlayAttack() { ... }

// Continuous: fires every frame, normalizedTime goes 0→1 over the clip
[CutsceneAction("Scale Over Time", CutsceneActionExecutionMode.OnUpdate)]
public void ScaleOverTime(float normalizedTime, float amount) { ... }

// Cleanup: fires once when the clip ends
[CutsceneAction("Reset Collider", CutsceneActionExecutionMode.OnExit)]
public void ResetCollider() { ... }
```

The **Display Name** string is what appears in the clip inspector's dropdown. Choose something readable.

---

### Action Execution Modes

| Mode | When | Signature |
|---|---|---|
| `OnEnter` (default) | Once at clip start | `void Method(params...)` |
| `OnUpdate` | Every frame | `void Method(float normalizedTime, params...)` — `normalizedTime` **must** be the first parameter |
| `OnExit` | Once at clip end | `void Method(params...)` |

For `OnUpdate` methods, the `normalizedTime` parameter is injected automatically by the system. It does **not** appear in the clip inspector's parameter list — only additional user parameters do.

---

### Action Execution Modes in Practice

**One-shot actions** (`OnEnter`) are appropriate for anything that only needs to trigger once: playing an animation state, spawning a particle, calling a dialogue system, enabling a component.

**Continuous actions** (`OnUpdate`) are appropriate for anything driven by clip duration: fading audio, blending weights, updating a custom shader property, interpolating a value. Use `normalizedTime` directly for interpolation — do not track elapsed time yourself.

**Cleanup actions** (`OnExit`) are appropriate for resetting state: re-enabling a collider, returning a flag, stopping a looping audio event.

---

### The CutsceneContext

Every action's `OnEnter`, `OnUpdate`, and `OnExit` receive a `CutsceneContext`. This gives actions access to three optional systems:

```csharp
public class CutsceneContext
{
    public IMotionSystem Motion;
    public IAnimationSystem Animation;
    public ICameraSystem Camera;
}
```

The context is built once per cutscene play by `CutsceneContextBuilder.Build(runner, cinemachineCamera)`. In edit mode (scrubbing without pressing Play), the context may be `null`. **All built-in actions guard against a null context**, and you should too.

The context is not injected into actor methods marked with `[CutsceneAction]` — those are called directly via reflection. The context is only available to `ICutsceneAction` implementations that receive it as a parameter.

---

### Built-in Actions

All built-in actions are `[Serializable]` classes implementing `CutsceneActionBase`. They appear in the **Action Type** dropdown in the clip inspector.

#### Move Actor
Translates the actor from a start position to a target position over the clip duration.

| Field | Description |
|---|---|
| `UseStartPositionFromActor` | If true (default), the start position is captured from the actor's transform when the clip begins. If false, use `StartPosition`. |
| `StartPosition` | Manual start position (only used when `UseStartPositionFromActor` is false). |
| `Target` | World-space destination. |

Supports scrubbing. Dragging the playhead moves the actor to the correct interpolated position in real time.

#### Rotate Actor
Rotates the actor from its current rotation to a target Euler rotation over the clip duration.

| Field | Description |
|---|---|
| `EulerRotation` | Target rotation in Euler angles (degrees). |

Supports scrubbing.

#### Play Animation
Triggers an Animator state by name on the actor's `Animator` component. One-shot (`OnEnter`).

| Field | Description |
|---|---|
| `AnimationId` | The Animator state name to play. |

#### Focus Camera
Points the Cinemachine camera at a target. One-shot (`OnEnter`).

| Field | Description |
|---|---|
| `FocusOnSelf` | If true, focuses on the actor executing this action. |
| `ExplicitTarget` | Target to focus on when `FocusOnSelf` is false. |

Requires a `CinemachineCamera` assigned to `CutsceneDirector`.

#### Move Camera
Moves the Cinemachine camera from its current position to a target position over the clip duration.

| Field | Description |
|---|---|
| `TargetPosition` | World-space destination. |
| `UseTargetObject` | If true, uses `TargetObject`'s live position as the target. |
| `TargetObject` | Target object reference (when `UseTargetObject` is true). |

Supports scrubbing.

#### Shake Camera
Triggers a `CinemachineImpulseSource` shake on the camera's GameObject. One-shot (`OnEnter`). A `CinemachineImpulseSource` component is added automatically if not present.

| Field | Description |
|---|---|
| `Intensity` | Shake strength (0–10). |
| `Duration` | Impulse duration in seconds. |

---

## Editor Tooling

### Cutscene Editor Window

Open via **Window → Cutscene Engine → Cutscene Editor**.

The window has three tabs:

**Scene Actors**
- Lists all `ICutsceneActor` MonoBehaviours in the currently open scene.
- Selecting an actor shows all of its `[CutsceneAction]` methods.
- Each method has an **Add to Timeline** button. Clicking it:
  1. Creates (or reuses) a `CutsceneActionTrack` bound to that actor.
  2. Creates a `CutsceneActionClip` on that track pre-configured with an `ActorMethodAction` pointing to the chosen method.
  3. Parameters are auto-generated based on the method signature.
- Use **Refresh** to re-scan the scene after adding new actors.

**Action Library**
- Shows all `CutsceneActionDefinition` assets in the project.
- These are reusable preset actions (ScriptableObject) created via `Create → Cutscene → Action Definition`.

**Settings**
- Placeholder for future settings (default durations, validation rules).

> **Director selection** — The top section of the window lets you pick a `PlayableDirector` from the scene. Once selected, **Open Timeline** focuses the Timeline window on that director, and **Play / Stop** drive `CutsceneDirector.Play()` / `Stop()` during play mode.

---

### Clip Inspector

When you select a `CutsceneActionClip` in the Timeline window, the Inspector transforms into the **Cutscene Action Configuration** panel.

**Bound Actor Info** — Shows the actor bound to the clip's track, and how many `[CutsceneAction]` methods are available on it. If no actor is bound, a warning is shown.

**Explicit Target** — Overrides the track binding for this individual clip. Useful when one clip in a sequence should target a different actor.

**Action Type** — Dropdown of all `ICutsceneAction` implementations found in the project. Changing the type creates a new instance and resets configuration.

**Action Configuration** — Varies by type:
- For `ActorMethodAction`: a dropdown of all `[CutsceneAction]` display names available on the bound actor, followed by auto-generated parameter fields matching the method signature.
- For all other types: all public fields are drawn using type-appropriate controls (IntField, FloatField, Vector3Field, ObjectField, EnumPopup, etc.).

**Validate Action** button — Runs `CutsceneValidationUtility` and shows a dialog with any errors or warnings.

---

### Validation

`CutsceneValidationUtility` provides static methods callable from any editor script:

```csharp
var result = CutsceneValidationUtility.ValidateClip(clip, boundActor);
Debug.Log(CutsceneValidationUtility.GenerateReport(result));
```

Checks performed include:
- Action is configured (not null)
- For `ActorMethodAction`: method name is set, actor has the method, parameter count and types match
- For movement actions: target is not zero (info only)
- `explicitTarget` implements `ICutsceneActor`

---

## User Guide

### Making an Actor

Implement `ICutsceneActor` on any `MonoBehaviour`. The minimum implementation:

```csharp
using Extensions.CutsceneEngine;
using UnityEngine;

public class BossActor : MonoBehaviour, ICutsceneActor
{
    private CutsceneActionAdapter _adapter;

    private void Awake()
    {
        _adapter = new CutsceneActionAdapter(this);
    }

    // ICutsceneActor
    public Transform GetTransform() => transform;
    public CutsceneActionAdapter GetCutsceneAdapter() => _adapter;

    public void OnCutsceneEnter()
    {
        // Disable AI, physics, player input here
        GetComponent<BossStateMachine>().enabled = false;
    }

    public void OnCutsceneExit()
    {
        // Re-enable everything
        GetComponent<BossStateMachine>().enabled = true;
    }

    // Cutscene-callable methods
    [CutsceneAction("Roar")]
    public void Roar()
    {
        GetComponent<Animator>().SetTrigger("Roar");
    }

    [CutsceneAction("Move Toward Player", CutsceneActionExecutionMode.OnUpdate)]
    public void MoveTowardPlayer(float normalizedTime, float speed)
    {
        // normalizedTime is 0→1 across the clip. Use it to drive interpolation,
        // or use deltaTime for frame-independent movement.
        var player = FindAnyObjectByType<PlayerController>();
        if (player == null) return;
        transform.position = Vector3.MoveTowards(
            transform.position,
            player.transform.position,
            speed * Time.deltaTime);
    }
}
```

> **Important:** Build the `CutsceneActionAdapter` in `Awake`, not lazily — the Timeline editor calls `GetCutsceneAdapter()` outside of play mode to populate dropdowns.

---

### Setting Up a Scene

1. Add a `PlayableDirector` to a GameObject. Assign a `TimelineAsset` to it.
2. Add a `CutsceneDirector` to the **same** GameObject. Assign the `PlayableDirector` and (optionally) a `CinemachineCamera`.
3. Place your actor GameObjects in the scene and add your actor component to each.

That is the complete scene setup. Nothing else needs to exist at start.

---

### Authoring Clips in Timeline

There are two workflows:

**Via the Cutscene Editor Window (recommended for actor methods)**
1. Open `Window → Cutscene Engine → Cutscene Editor`.
2. Select your `PlayableDirector` in the Director field.
3. Go to the **Scene Actors** tab, select an actor, and click **Add to Timeline** next to the method you want.
4. The clip is created and pre-configured — open Timeline to position and resize it.

**Via Timeline directly (recommended for built-in actions like Move Actor)**
1. Open Timeline.
2. Right-click on a track (or add a new `Cutscene Action Track` and bind your actor to it).
3. Right-click in the clip lane → **Add Cutscene Action Clip**.
4. Select the new clip. In the Inspector, choose the **Action Type** and fill in the fields.

---

### Using Built-in Actions

**Moving an actor**

Add a clip to the actor's track. Set Action Type to *Move Actor*. Set **Target** to the world position you want the actor to reach. Resize the clip to control how long the movement takes. Enable `UseStartPositionFromActor` to have the movement always start from wherever the actor is when the clip begins.

**Rotating an actor**

Add a clip. Set Action Type to *Rotate Actor*. Set **EulerRotation** to the target facing in degrees. The actor will slerp from its current rotation to this target over the clip's duration.

**Playing an animation**

Add a clip. Set Action Type to *Play Animation*. Set **AnimationId** to an Animator state name (e.g. `"Attack"`). The animation triggers once when the playhead enters the clip. Resize the clip to match the animation length for visual clarity.

**Camera focus**

Add a clip to any actor's track (or the camera actor's track). Set Action Type to *Focus Camera*. Enable **FocusOnSelf** to focus on the executing actor, or assign an **ExplicitTarget** for a different object. The Cinemachine camera's `Follow` and `LookAt` are set immediately on clip enter.

**Camera shake**

Add a clip. Set Action Type to *Shake Camera*. Set **Intensity** (0–10) and **Duration** (seconds). The `CinemachineImpulseSource` fires once on clip enter.

---

### Writing Custom [CutsceneAction] Methods

The attribute system lets you expose any method as a parameterized clip action. Guidelines:

**Do** — Use the update loop for continuous behavior:
```csharp
// Good: driven by normalizedTime, scrubbing-safe
[CutsceneAction("Fade Out", CutsceneActionExecutionMode.OnUpdate)]
public void FadeOut(float normalizedTime)
{
    canvasGroup.alpha = 1f - normalizedTime;
}
```

**Avoid** — Starting coroutines from `OnEnter`:
```csharp
// Problematic: coroutine runs independently of Timeline.
// Scrubbing, pausing, or stopping the Timeline will not stop the coroutine.
[CutsceneAction("Bad Fade")]
public void BadFade()
{
    StartCoroutine(FadeRoutine()); // Don't do this
}
```

If you genuinely need a coroutine (e.g. for a NavMesh path that cannot be driven by normalizedTime), track it in `OnCutsceneExit` and kill it:

```csharp
private Coroutine _moveRoutine;

[CutsceneAction("Walk To Point")]
public void WalkToPoint(Vector3 target)
{
    if (_moveRoutine != null) StopCoroutine(_moveRoutine);
    _moveRoutine = StartCoroutine(WalkRoutine(target));
}

public void OnCutsceneExit()
{
    if (_moveRoutine != null) { StopCoroutine(_moveRoutine); _moveRoutine = null; }
}
```

**Supported parameter types** in the Inspector:

| C# Type | Inspector Control |
|---|---|
| `int` | IntField |
| `float` | FloatField |
| `bool` | Toggle |
| `string` | TextField |
| `Vector3` | Vector3Field |
| `GameObject` | ObjectField |
| Any `enum` | EnumPopup (stored as int) |

Types not in this list will display "Unsupported type" in the inspector — they cannot be parameterized from the editor.

---

### Playing and Stopping Cutscenes at Runtime

```csharp
[SerializeField] private CutsceneDirector _director;

// Start playback
_director.Play();

// Stop immediately
_director.Stop();
```

`Play()` does the following in order:
1. Builds a `CutsceneContext` from the `CutsceneDirector`'s systems.
2. Scans the `PlayableDirector`'s track bindings and calls `OnCutsceneEnter()` on every bound `ICutsceneActor`.
3. Calls `CutsceneRuntime.BeginCutscene()` (sets `IsCutsceneActive = true`).
4. Starts the `PlayableDirector`.

`Stop()` does the reverse: stops the director, calls `OnCutsceneExit()` on all actors, clears the actor list, and calls `CutsceneRuntime.EndCutscene()`.

---

### Checking if a Cutscene is Active

```csharp
if (CutsceneRuntime.IsCutsceneActive)
{
    // Suppress player input, AI ticks, etc.
}
```

`CutsceneRuntime` is a simple static class. Poll it in `Update` or subscribe to any event-based system you use for game-wide state gating.

---

## Best Practices

**Prefer `OnUpdate` with `normalizedTime` over coroutines.** Timeline can be paused, scrubbed, and stopped at any time. Coroutines are unaware of Timeline's state. Any behavior driven by `normalizedTime` is automatically correct under all of these conditions.

**Always initialize the adapter in `Awake`.** `GetCutsceneAdapter()` is called by the editor outside of play mode. Lazy initialization (initializing on first call) can produce a null adapter in the editor if `Awake` has not yet run.

**Use `OnCutsceneEnter` / `OnCutsceneExit` for system pausing.** Do not scatter cutscene state checks across your gameplay code. Put all pause/resume logic inside the actor that owns it. The cutscene system calls these hooks automatically.

**Resize clips to match intent.** For `MoveActorAction` and `RotateActorAction`, the clip length *is* the animation duration. For `PlayAnimationAction`, align the clip length to the animation's length for readability, but the animation itself plays for its own duration regardless.

**Use `ExplicitTarget` sparingly.** It breaks the clean actor-per-track model. Reserve it for situations where a single clip genuinely needs to affect a different actor than the one bound to the track (e.g. a reaction shot where the camera quickly cuts to a bystander).

**Do not use `TransformMotionSystem` for cutscene movement.** `TransformMotionSystem` (used in the `CutsceneContext.Motion` field) runs coroutines and is intended for NavMesh agents or other motion controllers. For direct transform movement in a cutscene, use `MoveActorAction` or `RotateActorAction`, which are scrubbing-aware.

**Validate before shipping.** Use the **Validate Action** button in the clip inspector, or call `CutsceneValidationUtility.ValidateClip` in an Editor test, to catch misconfigured clips before they silently fail at runtime.

---

## System Architecture

### Runtime Pipeline

```
CutsceneDirector.Play()
    │
    ├─ CutsceneContextBuilder.Build()        ← Creates Motion/Animation/Camera systems
    ├─ actor.OnCutsceneEnter()               ← Per-actor lifecycle (foreach bound actor)
    ├─ CutsceneRuntime.BeginCutscene()       ← Sets IsCutsceneActive = true
    └─ PlayableDirector.Play()
           │
           └─ Per clip, per frame: CutscenePlayableBehaviour.ProcessFrame()
                   │
                   ├─ OnBehaviourPlay  → action.OnEnter(actor, context)
                   ├─ ProcessFrame     → action.OnUpdate(actor, context, normalizedTime, deltaTime)
                   └─ OnBehaviourPause → action.OnExit(actor, context)

CutsceneDirector.Stop()
    ├─ PlayableDirector.Stop()
    ├─ actor.OnCutsceneExit()                ← Per-actor cleanup
    └─ CutsceneRuntime.EndCutscene()         ← Sets IsCutsceneActive = false
```

### Class Reference

| Class / Interface | Responsibility |
|---|---|
| `ICutsceneActor` | Contract for anything that can participate in a cutscene |
| `ICutsceneOverridable` | Sub-interface: adapter, enter/exit hooks |
| `CutsceneActorProxy` | Built-in minimal actor with demo actions |
| `CutsceneActionAdapter` | Reflection wrapper — maps display names to `MethodInfo` |
| `CutsceneReflectionUtility` | Scans a type for `[CutsceneAction]` methods |
| `CutsceneActionAttribute` | Marks a method as cutscene-callable; carries display name and execution mode |
| `CutsceneActionExecutionMode` | Enum: `OnEnter`, `OnUpdate`, `OnExit` |
| `ICutsceneAction` | Contract for all action objects (OnEnter/OnUpdate/OnExit) |
| `CutsceneActionBase` | Abstract base with no-op default implementations |
| `ActorMethodAction` | Invokes a `[CutsceneAction]` method by name with serialized params |
| `MoveActorAction` | Lerps actor position over clip duration |
| `RotateActorAction` | Slerps actor rotation over clip duration |
| `PlayAnimationAction` | Plays an Animator state by name |
| `FocusCameraAction` | Sets Cinemachine Follow/LookAt targets |
| `MoveCameraAction` | Lerps Cinemachine camera position over clip duration |
| `ShakeCameraAction` | Fires a `CinemachineImpulseSource` shake |
| `CutsceneActionClip` | `PlayableAsset` — holds one `CutsceneActionReference` and an optional explicit target |
| `CutsceneActionTrack` | `TrackAsset` — binds to `MonoBehaviour`, accepts `CutsceneActionClip`s |
| `CutscenePlayableBehaviour` | `PlayableBehaviour` — drives OnEnter/OnUpdate/OnExit |
| `CutsceneActionReference` | `[SerializeReference]` wrapper around `ICutsceneAction` |
| `CutsceneActionDefinition` | ScriptableObject preset for a named action + parameter list |
| `SerializedCutsceneParameter` | Serializable union type for int/float/bool/string/Vector3/GameObject |
| `CutsceneContext` | Container for Motion, Animation, Camera systems |
| `CutsceneContextBuilder` | Static factory for `CutsceneContext` |
| `CutsceneDirector` | MonoBehaviour — owns play/stop and actor lifecycle |
| `CutsceneRuntime` | Static — `IsCutsceneActive` flag |
| `IMotionSystem` | Interface: `Move(actor, target, duration)`, `Rotate(actor, rotation, duration)` |
| `TransformMotionSystem` | Coroutine-based transform motion (for NavMesh actors) |
| `NavMeshMotionSystem` | NavMeshAgent-based motion |
| `IAnimationSystem` | Interface: `PlayAnimation(actor, id)` |
| `AnimatorAnimationSystem` | Calls `Animator.Play(id)` |
| `ICameraSystem` | Interface: `FocusOn`, `MoveTo`, `Shake` |
| `CinemachineCameraSystem` | Cinemachine-backed camera system |
| `ICutsceneAnimationProvider` | Optional interface for actors that provide Walk/Run/Idle hooks |

---

### The Motion System

`IMotionSystem` is part of `CutsceneContext` and is primarily intended for **actor-owned** motion that requires an external driver (e.g. a NavMesh agent). Built-in actions like `MoveActorAction` do **not** use it — they manipulate the transform directly in `OnUpdate` so that they are scrubbing-aware.

`TransformMotionSystem` runs coroutines and is useful for NavMesh actors that cannot be driven by normalized time (the path must run to completion). If you use it, accept that it will not respond to Timeline pausing or scrubbing.

`NavMeshMotionSystem` calls `NavMeshAgent.SetDestination`. Duration is ignored (the agent runs at its own speed).

---

### The Animation System

`IAnimationSystem.PlayAnimation(actor, id)` is a single call that triggers an animation by string ID. The built-in `AnimatorAnimationSystem` calls `Animator.Play(id)` on the actor's `Animator` component.

If your actor uses Animancer or a custom animation controller, implement `IAnimationSystem` and return it from `CutsceneContextBuilder.Build` (or pass it directly into a `CutsceneContext`).

---

### The Camera System

`ICameraSystem` exposes three operations:

- `FocusOn(Transform target)` — Sets Cinemachine Follow and LookAt.
- `MoveTo(Vector3 position, float duration)` — Moves the camera to a world position over time (coroutine-backed, not scrubbing-aware).
- `Shake(float intensity, float duration)` — Fires a `CinemachineImpulseSource`. A source component is added automatically to the camera GameObject at construction time.

`CinemachineCameraSystem` exposes `CameraTransform` (the Cinemachine camera's own transform) so that `MoveCameraAction` can cache the start position without falling back on `Camera.main`.

---

### Serialization of Actions and Parameters

`ICutsceneAction` is stored via `[SerializeReference]` inside `CutsceneActionReference`. This means Unity serializes the concrete type correctly, and the Inspector's **Action Type** dropdown can swap between types without data loss on the unchanged fields.

`SerializedCutsceneParameter` is a discriminated union — it holds all possible value types and exposes `GetValue()` which returns the correct one based on the `ParamType` enum. The Inspector auto-generates the correct field control based on the corresponding C# type in the method signature.

> **Note on `OnUpdate` parameters:** For methods with `CutsceneActionExecutionMode.OnUpdate`, the first parameter is `float normalizedTime`, which is injected by the system at runtime. It is skipped in the Inspector and does not count against the serialized `Parameters` array. Indices in `Parameters` correspond to the user-visible parameters only.

---

## Extending the System

### Custom ICutsceneAction

Inherit from `CutsceneActionBase` and mark the class `[Serializable]`. It will appear in the clip inspector's **Action Type** dropdown automatically.

```csharp
[Serializable]
public class FlashAction : CutsceneActionBase
{
    public Color FlashColor = Color.white;
    public int FlashCount = 3;

    public override void OnEnter(ICutsceneActor actor, CutsceneContext context)
    {
        // Kick off flash. Prefer driving it in OnUpdate via normalizedTime.
    }

    public override void OnUpdate(ICutsceneActor actor, CutsceneContext context, float normalizedTime, float deltaTime)
    {
        float phase = Mathf.PingPong(normalizedTime * FlashCount * 2f, 1f);
        var renderer = actor.GetTransform().GetComponent<Renderer>();
        if (renderer) renderer.material.color = Color.Lerp(Color.white, FlashColor, phase);
    }

    public override void OnExit(ICutsceneActor actor, CutsceneContext context)
    {
        var renderer = actor.GetTransform().GetComponent<Renderer>();
        if (renderer) renderer.material.color = Color.white;
    }
}
```

### Custom IMotionSystem

```csharp
public class RigidbodyMotionSystem : IMotionSystem
{
    public void Move(ICutsceneActor actor, Vector3 target, float duration)
    {
        var rb = actor.GetTransform().GetComponent<Rigidbody>();
        if (rb == null) return;
        Vector3 velocity = (target - actor.GetTransform().position) / duration;
        rb.linearVelocity = velocity;
    }

    public void Rotate(ICutsceneActor actor, Quaternion rotation, float duration)
    {
        actor.GetTransform().rotation = rotation; // instant snap
    }
}
```

Supply it when building the context:
```csharp
context.Motion = new RigidbodyMotionSystem();
```

### Custom IAnimationSystem

```csharp
public class AnimancerCutsceneAnimationSystem : IAnimationSystem
{
    public void PlayAnimation(ICutsceneActor actor, string id)
    {
        var animancer = actor.GetTransform().GetComponent<AnimancerComponent>();
        var clip = Resources.Load<AnimationClip>(id);
        if (animancer && clip) animancer.Play(clip);
    }
}
```

### Custom ICameraSystem

Implement all three methods. If you use a different virtual camera system (e.g. a custom follow camera), wrap it here and return it from `CutsceneContextBuilder.Build`.

```csharp
public class CustomCameraSystem : ICameraSystem
{
    private readonly MyCameraController _cam;

    public CustomCameraSystem(MyCameraController cam) { _cam = cam; }

    public void FocusOn(Transform target) => _cam.SetTarget(target);
    public void MoveTo(Vector3 position, float duration) => _cam.MoveTo(position, duration);
    public void Shake(float intensity, float duration) => _cam.Shake(intensity, duration);
}
```

