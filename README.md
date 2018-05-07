# PolyLib
Library for creating (dynamically) animated Delaunay Triangulations

## Demo each default animation

## Show how to generate each animation

```
LowPolyView polyView = FindViewById<LowPolyView> (Resource.Id...);

polyView.GenerateNewTriangulation(boundsWidth, boundsHeight, variance, cellSize, this)
```
```
// 12 frames in the animation
var growAnim = new Grow(polyView.CurrentTriangulation, 12);
polyView.AddAnimation(growAnim);
```
```
// 12 frames in the animation
var sweepAnim = new Sweep(polyView.CurrentTriangulation, 12);
polyView.AddAnimation(sweepAnim);
```
```
// new Touch animation at touch location, radius=250, 6 frames in the animation
var touchAnimation = new RandomTouch(polyView.CurrentTriangulation, 6, touch.X, touch.Y, 250);
polyView.AddAnimation(touchAnimation);
```

Show what each parameter does

## What makes this library different?
Explain that this does more than create triangulations, but allows you to put animations on top of the triangulations. Even allows for creation of custom animations

## Libraries and tools used

### Alternate Usages
Describe how to use TriangulationView and AnimationUpdateView seperately with screenshots


#### Find where to place in this doc:
framerate
