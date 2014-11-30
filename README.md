MaxLib
======

A set of terrain generation and visualization tools.

This project was initiated in 2011, my junior year in high school, as one of the first tools I tried to build. It used XNA 3.0 and its primary goal was to generate realistic looking world data. It used Perlin (or Simplex) turbulence to create a general terrain, followed by iterative applications of algorithms that normalized, and smoothed the terrain, simulated heat erosion, moisture erosion to create a realistic looking terrain, and simulated moisture propagation to generate realistic-looking climatic gradation, with defined biomes and intelligently generated rivers.

This project was also my first foray into 3D graphics — the visualization of the terrain involves primitive pseudo-normal mapping, which then is multiplied against a global light direction and then baked into the texture as shading. There is also a basic shadowcasting system where the system iteratively casts light rays in an attempt to determine what parts of the terrain are occluded. This was originally all performed in 2D, but towards the end of this project's life I began to render the texture onto a 3D mesh.

This project in its current form has been abandoned since 2012, though I have been considering porting it to some other system.
