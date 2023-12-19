# Unity Virtual Projector

*This is an experimental project.*

The initial goal was to develop a tool that could be used to trace perspective murals (Trompe-l’œil) without the need of a physical projector.

Instead, a calibrated camera could used to obtain the Euclidean transformation between the viewer point and the mural surface with a single picture.

Using this information, the ray-plane-collisions for the desired shape into each of the mural planes is computed to obtain the distorted shape points on plane coordinates.

Working on `Unity_5.6.7` and `Unity_2021`.


## Table of Contents

- [Unity Virtual Projector](#unity-virtual-projector)
  - [Table of Contents](#table-of-contents)
  - [About](#about)
  - [TODOs](#todos)

## About

See more info at:

https://bonzolabs.com/multi-plane-virtual-projector/


## TODOs

- Remove scale from plane/cube transforms
- Handle shape plane discontinuity
- Implement export mechanism to get an output file with the plane points ready for tracing in planes (handle shape discontinuity)
- Handle SVG files for the line renderer (some code already exists)
- Find a way to input Camera Extrinsics into the scene.
