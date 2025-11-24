# GAM531 Assignment 9: 3D Object Collision Detection

**Author:** Divyesh Bhanvadiya <br>
**Technology:** C# with OpenTK and OpenGL


## Gameplay
- Navigate a maze (3x3) of rooms
- 'E' to open/close doors
- Find a golden cube, walk towards it to WIN!!!

## Program Results
![](https://github.com/Divyesh-Bhanvadiya/GAM531-Assignment-9-3DRoomMazeWithCollisionDetection/blob/main/Program%20Results%20(with%20Text).gif)

## 🎯 Project Overview

This project implements a 3D maze navigation game featuring:
- First-person player controller with collision detection
- Procedurally generated 3×3 room grid (9 rooms total)
- Interactive doors that open/close on demand
- Random obstacles (pillars and cubes) in each room
- Goal-seeking gameplay (find the golden cube to win)



## 🎮 Controls

| Input | Action |
|-------|--------|
| **W** | Move Forward |
| **A** | Move Left |
| **S** | Move Backward |
| **D** | Move Right |
| **Mouse** | Look Around (First-Person) |
| **E** | Open/Close Nearby Door (1m range) |
| **ESC** | Exit Game |


### **Scene Layout:**
- 3×3 grid of rooms (each 3m × 3m × 2.5m tall)
- Player starts in center room (0, 0)
- Goal spawns randomly in one of the 8 outer rooms
- Each room (except center) contains:
  - 50% chance of a pillar (0.5m × 2.5m × 0.5m)
  - 0-2 random cubes (0.3m-0.6m sizes)



## 🚀 Running the Project

1. Clone repo from GitHub OR Download zip from GitHub
```bash
git clone https://github.com/Divyesh-Bhanvadiya/GAM531-Assignment-9-3DRoomMazeWithCollisionDetection.git
```
2. Open in your choice of IDE
3. Build and run
4. Navigate the maze, open doors with **E**, find the golden cube to win!



## 📊 Technical Specifications

- **Framework:** OpenTK 4.9+
- **Rendering:** Flat shading (no lighting) without textures
- **Collision:** AABB with axis-separated resolution
- **Player Speed:** 3 m/s
- **Player Size:** 0.2m × 1.5m × 0.2m (W×H×D)
- **Interaction Range:** 1 meter (doors, goal)

## 🔧 Collision Detection Method

### **AABB (Axis-Aligned Bounding Box)**

The project uses **AABB collision detection** for its simplicity, efficiency, and suitability for axis-aligned objects.

#### **Why AABB?**
- **Performance:** simple min/max comparisons
- **Simplicity:** No complex math (angles, rotations, or dot products)
- **Perfect fit:** All objects in the scene are axis-aligned (walls, doors, pillars, cubes)
- **Predictable:** Easy to debug and visualize

#### **AABB Structure**
Each collider stores:
- **Center:** Position of the bounding box center
- **Size:** Dimensions (width, height, depth)
- **Min:** Minimum corner (Center - Size/2)
- **Max:** Maximum corner (Center + Size/2)

#### **Intersection Test**
Two AABBs intersect if they overlap on **all three axes** (X, Y, Z):

```csharp
public bool Intersects(AABBCollider other)
{
    return (this.Min.X <= other.Max.X && this.Max.X >= other.Min.X) &&
           (this.Min.Y <= other.Max.Y && this.Max.Y >= other.Min.Y) &&
           (this.Min.Z <= other.Max.Z && this.Max.Z >= other.Min.Z);
}
```

If any axis does **not** overlap, the AABBs do not collide.



## 🎮 Collision and Movement Integration

### **Collision Resolution Strategy: Axis-Separated Testing**

The player's movement is resolved using a **two-step axis-separated collision check** to enable smooth sliding along walls.

#### **Step-by-Step Process:**

1. **Calculate Desired Movement**
   - User presses WASD → Calculate velocity vector
   - Velocity is projected onto the XZ plane (no vertical movement)
   - Speed: 3 meters/second

2. **Test X-Axis Movement First**
   ```csharp
   nextPos.X += velocity.X;
   Collider.UpdatePosition(nextPos);
   
   // Check collision on X-axis
   if (collision detected)
       nextPos.X = currentPos.X; // Revert X movement
   ```

3. **Test Z-Axis Movement Second**
   ```csharp
   nextPos.Z += velocity.Z;
   Collider.UpdatePosition(nextPos);
   
   // Check collision on Z-axis
   if (collision detected)
       nextPos.Z = currentPos.Z; // Revert Z movement
   ```

4. **Apply Final Position**
   - Player moves to `nextPos` with collision-resolved coordinates
   - Camera follows player at eye height (0.65m above feet)

#### **Why Separate Axes?**

This approach enables **wall sliding**:
- If moving diagonally into a wall, the blocked axis stops but the free axis continues
- Example: Walking northeast into a wall → X-axis blocked, Z-axis continues → Player slides north along the wall
- Feels natural and prevents getting "stuck" on corners

#### **Open Door Handling**

Doors have an `IsOpen` flag. During collision checks:
```csharp
var door = obj.GetComponent<Door>();
if (door != null && door.IsOpen)
    continue; // Skip collision check for open doors
```

When closed, doors block movement. When open (slid into floor), player walks through freely.



## 🚧 Challenges Encountered and Solutions

### **Challenge 1: Wall Generation Overlaps**

**Problem:**  
Initial wall generation logic (rendering isolated rooms) created overlapping wall segments at room boundaries, causing:
- Z-fighting (flickering walls)
- Visual artifacts

**Solution:**  
Simplified wall generation:
- Created 4 single outer walls instead of multiple segments
- Explicitly positioned inner walls between specific room pairs
- Removed complex nested loops that caused overlaps

---

### **Challenge 2: Door Interaction After Opening**

**Problem:**  
When doors slid down into the floor (Y -= 2m), the interaction range check used the door's **current position** (underground). Players couldn't close doors because they were too far away.

**Solution:**  
- Stored the door's `ClosedPosition` separately
- Distance check uses `ClosedPosition` instead of current position
- Players can now interact with doors regardless of open/closed state

**Code Fix:**
```csharp
Vector3 doorPosition = door.ClosedPosition != Vector3.Zero 
    ? door.ClosedPosition 
    : obj.Transform.Position;

float distance = (doorPosition - Transform.Position).Length;
```



## 🏗️ Architecture

### **Component-Based Design**

The project uses a modular component system:

- **GameObject:** Container for game entities (walls, doors, player)
- **Transform:** Position, rotation, scale
- **Components:**
  - `Renderer` - Handles VAO/VBO/EBO and rendering
  - `AABBCollider` - Collision detection
  - `Door` - Door-specific logic (open/close state)

**Advantages:**
- Easy to add/remove functionality
- Clean separation of concerns
- Reusable components across different object types




