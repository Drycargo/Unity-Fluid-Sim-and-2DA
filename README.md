# Unity Grid-based Fluid Simulation and Two-Dimensionalization

[Video Demo](https://drive.google.com/file/d/1U0_PmLUaTz4S3j2Pl1J3N78aMHcy3pAe/view?usp=sharing)

This is a real-time grid-based (semi-Lagrangian) fluid simulation based on Jos Stam's stable fluid simulation method; it operates according to the Navier–Stokes equations, performing the complete procedure of force exertion - advection - projection - diffusion, with vortex affect included.

User interface is implemented to allow user to move around target object (e.g., the Earth model) across the fluid simulation plane, which "dissolves" the model entering itself and adds the melted fluid onto itself.

## Reference and Inspiration

This project is inspired by [Pancake Simulator](https://github.com/xdedss/PancakeSimulator), a Unity project that uses GPU accelerated particle system to simulate the Two-Dimensionalization-Attack in the science fiction *The Three Body Problem*. The "Pancake" is generated in the following manner:
- An orthogonal camera (CamP) is set up on the simulation plane, facing towards the objects falling into the plane;
- As objects fall into the plane, CamP captures the cross section of the objects and the plane;
- This cross section is used to generate new particles with corresponding colors; the particle velocity is determined by an accumulated "potential" of the pixel on which it is generated;

In the case of my project, the orthogonal camera is replaced by a perspective camera whose Near and Far planes are very close. Particle generation is replaced by coloring the pixels where the cross section falls on, while the potential calculation is preserved.