# Custom 2D Physics Engine: Verlet & SAT

A custom 2D physics simulation engine implemented in Unity using **C#**. This project bypasses Unity's built-in `Rigidbody2D` and `Collider2D` systems to demonstrate a fundamental understanding of vector mathematics, numerical integration, and computational geometry.

![Demo GIF](Media/gameplay_demo.gif)

## ⚙️ Core Physics Architecture

### 1. Verlet Integration (Soft Body Physics)
Implemented a stable **Verlet Integration** scheme for rope simulation, chosen for its stability in handling constraints compared to Euler integration.
- **Constraint Solving:** Iterative distance constraint relaxation to simulate tension.
- **Dynamic Topology:** Supports runtime modification of constraints. The rope can be **cut** dynamically at any segment, splitting the physics chain in real-time.

### 2. Separating Axis Theorem (SAT)
Implemented a custom collision detection system from scratch handling Convex Polygons and Circles.
- **Collision Detection:** Projects shape vertices onto normal axes to detect overlaps (Dot Product).
- **Edge-Case Handling:** Implements dynamic axis generation (Closest Vertex) to correctly resolve **Circle-to-Box (Voronoi Region)** collisions.
- **Collision Resolution:** Calculates the **Minimum Translation Vector** to separate overlapping bodies and applies impulse-based friction and bounciness.

### 3. Interaction Mechanics
- **Projectile Physics:** Custom arrow ballistics that interact with the physics engine.
- **Rope Cutting:** Raycast/Segment intersection logic to detect arrow impacts on specific rope constraints.

## Controls
- **Left Mouse (Drag & Release):** Aim and fire an arrow.
- **Objective:** Shoot arrows to cut the ropes and interact with suspended objects.

## Optimization Roadmap (Work in Progress)
*Current version represents a functional prototype focusing on mathematical correctness.* The following architectural optimizations are planned for the next iteration to improve performance and stability:

- [ ] **Physics Sub-stepping (CCD):** Implement a sub-stepping loop (dividing `FixedUpdate` into smaller time steps) to resolve **"tunneling"** artifacts where high-velocity arrows pass through colliders.
- [ ] **Data-Oriented Refactor:** Replace individual `GameObject` nodes for rope segments with a lightweight `struct` or array-based approach to eliminate Transform overhead.
- [ ] **Rendering Optimization:** Implement a single `LineRenderer` per rope (batching) instead of one per segment to drastically reduce Draw Calls.
- [ ] **GC Allocation Fix:** Optimize `GetWorldVertices` and SAT loop calculations to use cached arrays, eliminating per-frame garbage collection spikes.
- [ ] **Broad-Phase Collision:** Implement a Spatial Hash Grid or Quadtree to optimize the collision loop from O(N²) to O(N log N).

## Tech Stack
- **Engine:** Unity 6+
- **Language:** C#
- **Key Concepts:** Vector Math, Numerical Integration, SAT, Computational Geometry.

## How to Run
1. Clone the repository.
2. Open the project in Unity Hub.
3. Open `Assets/_Scenes/DemoScene.unity`.
4. Press Play to start the simulation.

## License
This project is open-source and available under the MIT License.