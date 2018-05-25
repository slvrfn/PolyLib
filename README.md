

# PolyLib

Sweep | Touch | Grow
--- | --- | ---
<img src="https://github.com/cameronwhite08/PolyLib/blob/master/gifs/sweep.gif?raw=true" alt="Demo of the Sweep Animation" title="Demo of the Sweep Animation"> | <img src="https://github.com/cameronwhite08/PolyLib/blob/master/gifs/touch.gif?raw=true" alt="Demo of the random Touch animation" title="Demo of the random Touch animation"> | <img src="https://github.com/cameronwhite08/PolyLib/blob/master/gifs/grow.gif?raw=true" alt="Demo of the Grow Animation" title="Demo of the Grow Animation">

## Usage
#### Create the view
>Android
```xml
<LowPolyLibrary.Views.Android.LowPolyView
	android:layout_width="match_parent"		<!-- Any size is possible -->
	android:layout_height="match_parent"	<!-- Same here -->
	android:id="@+id/triangulationView" />
```
>iOS

Add a `View` instance in your .storyboard or .xib (named polyView here), set `LowPolyView` class

#### Get reference to the view

>Android
```c#
LowPolyView polyView = FindViewById<LowPolyView> (Resource.Id.triangulationView);
```

>iOS
>
Connect `Outlet` in *ViewController.designer.cs*
```c#
partial class ViewController{
    [Outlet]
    LowPolyLibrary.Views.iOS.LowPolyView polyView { get; set; }
}
```
#### Get Reference to a Triangulation
Create standard `Triangulation`
```
var tri = new Triangulation(
	boundsWidth:1080,
	boundsHeight:720
	);
```
Create `Triangulation` with custom colors, provided to a random `SKShader`
```
var tri = new Triangulation(
	boundsWidth: 100,
	boundsHeight: 100,
	gradientColors: new SKColor[]
	);
```
Create `Triangulation` with a provided [`SKShader`](https://developer.xamarin.com/api/type/SkiaSharp.SKShader/)
```
var tri = new Triangulation(
	boundsWidth: 100,
	boundsHeight: 100,
	gradientShader: SKShader.Create...(...)
	);
```
__OR__

Get the `Triangulation` hosted by the current `LowPolyView`
```
var tri = polyView.CurrentTriangulation;
```


#### Create animations
>Grow Animation
```c#
var growAnim = new Grow(
	triangulation: _polyView.CurrentTriangulation,
	numFrames: _numAnimFrames
	);
polyView.AddAnimation(growAnim);
```
> Sweep Animation
```C#
var sweepAnim = new Sweep(
	triangulation: _polyView.CurrentTriangulation,
	numFrames: _numAnimFrames
	);
polyView.AddAnimation(sweepAnim);
```
> Touch Animation
```c#
// new Touch animation at touch location, radius=250, 6 frames in the animation
var touchAnimation = new RandomTouch(
	triangulation: _polyView.CurrentTriangulation,
	numFrames: 12,
	x: touch.X,
	y: touch.Y,
	radius: 150
	);
polyView.AddAnimation(touchAnimation);
```
While two touch animations are provided to you, an abstraction was made in creating `PolyLib.Animation.Touch` to  handle all the logic of separating points, getting points in a touch area, etc. This allows more touch animations to be easily created by a user by only worrying about how each point is modified in a touch area in `Touch.DoPointDisplacement`.

## API

### `LowPolyLibrary.Triangulation`

>Note: Points are initially generated in a grid pattern, then offset by noise.

- `public float Seed { get; set; }`
	- Seed value used in determining noise for point generation
- `public float Frequency { get; set; }`
	- Frequency value used in determining noise for point generation
- `public float BleedY { get; set; }`
	- Y direction of how far points can be generated outside bounding rectangle defined by (Left, Top, Right, Bottom) : (0, 0, BoundsWidth, BoundsHeight)
- `public float BleedX { get; set; }`
	- X direction of how far points can be generated outside bounding rectangle defined by (Left, Top, Right, Bottom) : (0, 0, BoundsWidth, BoundsHeight)
- `public float CellSize { get; set; }`
	- How far apart points are initially generated in the grid
	- A grid cell has the dimensions: `CellSize`x`CellSize`
- `public float Variance { get; set; }`
	- How far a point can vary (X/Y directions independent) from its initial location in the grid.
	- Designed to be in (0,1]
		- Any number can be provided (0, infinity), although behavior is undefined outside of  (0,1]
	- Used internally as `CellSize * Variance`
- `public bool HideLines { get; set; }`
	- Whether or not to draw outer lines of triangles
- `public SKColor StrokeColor { get; set; }`
	- Color that will be used to draw outer lines of triangles
- `public SKShader GradientShader { get; set; }`
	- A [`SKShader`](https://developer.xamarin.com/api/type/SkiaSharp.SKShader/) used for the gradient
- `public List<Vertex> Points { get; }`
	- Copy of internal points used in a `Triangulation`
- `public List<Triad> TriangulatedPoints { get; }`
	- Copy of `Triad`(s) used in a `Triangulation`
	- A `Triad` represents the 3 indices of a triangle in a `Triangulation`
- `public Dictionary<Vertex, HashSet<Triad>> PointToTriangleDic { get; }`
	- Copy of Dictionary which maps a `Vertex` to the `Triads` it is associated with

### `LowPolyLibrary.AnimationEngine`
This comes pre-hosted in each `PolyLib.Views.*.AnimationUpdateView`
 - `public void AddAnimation(AnimationBase anim)`
	- Add an animation which derives from `AnimationBase` to be drawn
 - `public void UpdateAnimationFPS(int fps)`
	- Update engine to trigger frame draws at a different FPS
	- Defaulted to 24FPS (42ms) between each trigger (1sec/24FPS = ~42ms)
 - `public void DrawOnMe(SKSurface surface)`
	- Draw the current animation frame on some `SKSurface`
		- Typically used by views which host the `AnimationEngine`
 - `public void StartRandomAnimationsLoop(int msBetweenRandomAnim)`
	- Trigger a loop to start, which draws one of a set of provided Animations
 - `public void StopRandomAnimationsLoop()`
	- Stop the random animations from being drawn
 - `public void SetAnimCreatorsForRandomLoop(List<Func<Triangulation,AnimationBase>> animCreators)`
	- A list of user-defined `Func`s that given a `Triangulation`, returns some animation which derives from `AnimationBase` (Allows pre-defined or user-defined animations to be randomly selected)
>ex
 ```c#
   new Func<Triangulation, AnimationBase>((Triangulation arg) =>
   {
	   return new Sweep(arg,  12);
   })  
   ```

## What makes this library different?
This library does more than just generate Delaunay triangulations. I created this library because other similar libraries did not suit my needs, they just generated the basic triangulations. This library was created with the purpose of adding animations to these Delaunay triangulations. A few animations have been created already, but many *many* more interesting animations can be created following the template set by the presets. If you are *clever* this library can even be used in a to put animations on any set of points, not necessarily involving a `Triangulation`.

## Libraries used

- [S-hull][sHullRef]
Responsible for creating the Delaunay triangulations. Chosen for its proven speed
- [Auburns/FastNoise_CSharp][fastNoiseRef]
Responsible for generating noise used in creating source points for triangulation
- [capesean/ColorBru][colorBruRef]
Binding to the ColorBrewer library developed by Cynthia Brewer

[sHullRef]: http://www.s-hull.org/
[fastNoiseRef]: https://github.com/Auburns/FastNoise_CSharp
[colorBruRef]: https://github.com/capesean/ColorBru

## Alternate Usages
Describe how to use TriangulationView and AnimationUpdateView seperately with screenshots

## Future plans
 - Support dynamically resizing the view
 - Support more platforms (Relatively easy to add a platform. Check out `PolyLib.Views.* `)
 - OpenGL support
###### <small>Does anyone want to pick up the ball here? </small>

## Compatibility
Built with
- `.Net Standard` 2.0
- `Xamarin.Android` 8
- `Xamarin.iOS` 11
