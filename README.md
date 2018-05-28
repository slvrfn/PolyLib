
  
# PolyLib 
<p align="center">
  <img src="https://img.shields.io/badge/.net%20standard-2.0-blue.svg" alt=".Net Standard 2.0" title=".Net Standard 2.0"> <img src="https://img.shields.io/badge/Xamarin.Android-8-green.svg" alt="Xamarin.Android 8" title="Xamarin.Android 8"> <img src="https://img.shields.io/badge/Xamarin.iOS-11-lightgrey.svg" alt="Xamarin.iOS 11" title="Xamarin.iOS 11"> 
</p>
<br>

Sweep | Touch | Grow
--- | --- | ---
<img src="https://github.com/cameronwhite08/PolyLib/blob/master/gifs/sweep.gif?raw=true" alt="Demo of the Sweep Animation" title="Demo of the Sweep Animation"> | <img src="https://github.com/cameronwhite08/PolyLib/blob/master/gifs/touch.gif?raw=true" alt="Demo of the random Touch animation" title="Demo of the random Touch animation"> | <img src="https://github.com/cameronwhite08/PolyLib/blob/master/gifs/grow.gif?raw=true" alt="Demo of the Grow Animation" title="Demo of the Grow Animation">

## Usage
#### Create the view
>Android
```xml
<PolyLib.Views.Android.PolyLibView
	android:layout_width="match_parent"		<!-- Any size is possible -->
	android:layout_height="match_parent"	<!-- Same here -->
	android:id="@+id/triangulationView" />
```
>iOS

Add a `View` instance in your .storyboard or .xib (named polyView here), set `PolyLibView` class

#### Get reference to the view

>Android
```c#
PolyLibView polyView = FindViewById<PolyLibView> (Resource.Id.triangulationView);
```

>iOS
>
Connect `Outlet` in *ViewController.designer.cs*
```c#
partial class ViewController{
    [Outlet]
    PolyLib.Views.iOS.PolyLibView polyView { get; set; }
}
```
#### Get Reference to a Triangulation
Create standard `Triangulation`
```c#
var tri = new Triangulation(
	boundsWidth:1080,
	boundsHeight:720
	);
```
Create `Triangulation` with custom colors, provided to a random `SKShader`
```c#
var tri = new Triangulation(
	boundsWidth: 100,
	boundsHeight: 100,
	gradientColors: new SKColor[]
	);
```
Create `Triangulation` with a provided [`SKShader`](https://developer.xamarin.com/api/type/SkiaSharp.SKShader/)
```c#
var tri = new Triangulation(
	boundsWidth: 100,
	boundsHeight: 100,
	gradientShader: SKShader.Create...(...)
	);
```
__OR__

Get the `Triangulation` hosted by the current `PolyLibView`
```c#
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

### `PolyLib.Triangulation`

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
	- A `Triad` represents the 3 indices of a points in `List<Vertex> Points` that make up a  triangle in a `Triangulation`
- `public Dictionary<Vertex, HashSet<Triad>> PointToTriangleDic { get; }`
	- Copy of Dictionary which maps a `Vertex` to the `Triads` it is associated with

### `PolyLib.AnimationEngine`
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
- `public void UpdateRandomAnimTriangulations(List<Triangulation> triangulations)`
	- A list of user-provided `Triangulation`s that are randomly selected among and used as the source for any created animations.
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

## Important Notes
Each `PolyLibView` was created to not only display triangulations, but also animations on the triangulations. This was done by creating three separate views: `AnimationUpdateView`, `PolyLibView`, and `TriangulationView`. `PolyLibView` hosts the other two views sandwiched together with a  `TriangulationView` on the bottom, and a `AnimationUpdateView` on top. The `TriangulationView` is responsible for drawing entire static `Triangulation`s. An `AnimationUpdateView` is a transparent view which displays only updates frame-by-frame on top of the `Triangulation`. For performance reasons it is best to only draw the individual triangles updated in each frame of an animation, instead of the entire `Triangulation` each time. If desired, an `AnimationUpdateView` could be hosted anywhere for custom animation effects.

>An `AnimationUpdateView` demo

<center><img src="https://github.com/cameronwhite08/PolyLib/blob/master/gifs/animationUpdateView.gif?raw=true" alt="Demo of the Animation Update View" title="Demo of the Animation Update View" height="500"></center>


## Future plans
 - Support dynamically resizing the view
 - Support more platforms (Relatively easy to add a platform. Check out `PolyLib.Views.* `)
 - OpenGL support
###### <small>Does anyone want to pick up the ball here? </small>

## Libraries used

- [mono/SkiaSharp](https://github.com/mono/SkiaSharp)
Used to allow for cross-platform drawing
- [S-hull][sHullRef]
Responsible for creating the Delaunay triangulations. Chosen for its proven speed
- [Auburns/FastNoise_CSharp][fastNoiseRef]
Responsible for generating noise used in creating source points for triangulation
- [capesean/ColorBru][colorBruRef]
Binding to the ColorBrewer library developed by Cynthia Brewer
- [gridsum/DataflowEx](https://github.com/gridsum/DataflowEx)
Wraps animation loop in re-usable dataflow

[sHullRef]: http://www.s-hull.org/
[fastNoiseRef]: https://github.com/Auburns/FastNoise_CSharp
[colorBruRef]: https://github.com/capesean/ColorBru