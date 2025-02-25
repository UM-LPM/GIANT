# Game Modeling

Game modeling refers to the process of creating digital assets—characters, environments, props, and objects—that make up the visual world of a game. This process can involve both 2D and 3D modeling, depending on the game's art style and requirements.

- 🎨 2D Modeling: Focuses on pixel art, vector graphics, or painted sprites used in side-scrollers, platformers, and strategy games.

- 🏗 3D Modeling: Involves creating 3D objects with depth and perspective, commonly used in modern games with immersive environments.

Both 2D and 3D assets require careful consideration of art style, performance, and gameplay integration to ensure they fit seamlessly within the game world.

## 1. 2D Modeling for Games

### 1.1. What is 2D Modeling?

2D modeling refers to the creation of flat, two-dimensional assets that form the **visual elements of a game**. These can include **characters, backgrounds, UI elements, and objects**.

🕹 Common Game Genres Using 2D Models:

- **Platformers** (e.g., Hollow Knight, Celeste)
- **Top-down RPGs** (e.g., Stardew Valley, Pokémon)
- **Fighting Games** (e.g., Street Fighter, Guilty Gear)
- **Puzzle & Mobile Games** (e.g., Angry Birds, Candy Crush)

### 1.2. 2D Art Styles & Techniques

Different 2D games adopt various artistic approaches based on their gameplay and artistic direction.

#### 1.2.1. Pixel Art

🟣 **Definition**: A retro style where **characters and environments are built from small square pixels**.

🟣 **Used in**: Indie games, classic arcade-style games.

🟣 **Tools**: Aseprite, Pyxel Edit, Photoshop.

**Example:**

Super Mario Bros. (NES) uses **low-resolution pixel art**, while modern pixel art games like Celeste feature **highly detailed** animations.

#### **1.2.2. Vector Art**

🟢 **Definition**: Uses mathematical shapes (lines, curves) to create smooth, scalable artwork.

🟢 **Used in**: Flash-style games, mobile games, UI design.

🟢 **Tools**: Adobe Illustrator, Inkscape, Affinity Designer.

**Example:**

Angry Birds uses **vector-based characters**, allowing for smooth resizing without quality loss.

#### **1.2.3. Hand-Drawn & Painted Art**

🔵 **Definition**: Artists create detailed, high-resolution illustrations that are either scanned or drawn digitally.

🔵 **Used in**: Atmospheric, artistic games.

🔵 **Tools**: Photoshop, Procreate, Krita.

**Example:**

Hollow Knight features **hand-drawn environments** with fluidly animated characters.

### 1.3. 2D Animation for Games

2D models often need **animation** to bring characters and objects to life. The two primary methods include:

📌 **Frame-by-Frame Animation** – Individual frames are drawn for each movement (used in traditional animation).

📌 **Skeletal Animation (Rigging)** – A **digital skeleton** is applied to a sprite, allowing for smooth, reusable movements.

**Example:**

Cuphead uses **frame-by-frame animation**, while Hollow Knight relies on **skeletal animation** for fluid character motion.

### 1.4. Exporting & Optimizing 2D Assets for Games

Before importing 2D assets into a game engine, they must be **optimized** for performance.

✅ **Texture Atlases** – Combine multiple sprites into a single image to reduce memory usage.

✅ **Resolution Scaling** – Ensure assets remain crisp on different screen sizes.

✅ **Transparent Backgrounds** – Save characters as **PNG files** to keep them separate from backgrounds.

## 2. 3D Modeling for Games

### 2.1. What is 3D Modeling?

3D modeling is the process of **creating digital three-dimensional objects** that can be viewed from multiple angles. These models are built using polygons and are textured, rigged, and animated to be used in-game.

🕹 **Common Game Genres Using 3D Models:**

- **First-Person Shooters (FPS)** (e.g., Call of Duty, Halo)
- **RPGs** (e.g., The Witcher, Elden Ring)
- **Open-World Games** (e.g., GTA, Skyrim)
- **Fighting Games** (e.g., Tekken, Mortal Kombat)

### 2.2. The 3D Modeling Pipeline

#### 1️⃣ Blocking & Base Mesh Creation

- Roughly shapes the model using simple geometry.

#### 2️⃣ High-Poly & Low-Poly Modeling

- **High-Poly** models have intricate details.
- **Low-Poly** models are used in games to optimize performance.

#### 3️⃣ UV Mapping & Texturing

- **UV Mapping** flattens a 3D model for texturing.
- **Texturing** applies details like color, roughness, and reflections.

#### 4️⃣ Rigging & Animation

- Adding bones to **animate characters & objects**.

#### 5️⃣ Optimization

- Using **LODs (Level of Detail)** to adjust asset complexity based on distance.

### 2.3. Types of 3D Models

📌 **Character Models** – Main playable characters, NPCs, enemies.

📌 **Environment Models** – Buildings, trees, terrain.

📌 **Vehicle & Weapon Models** – Cars, guns, swords.

## 3. Comparing 2D vs 3D Modeling for Games

Both **2D and 3D modeling** have advantages and are suited for different types of games. Many modern games **blend both** by using **2D sprites in 3D environments** (e.g., Octopath Traveler).

| Feature | 2D Modeling | 3D Modeling |
| --- | --- | --- |
| **Art Style** | Pixel, Vector, Hand-Drawn | Realistic, Stylized, Cartoonish |
| **Tools** | Photoshop, Aseprite, Illustrator | Blender, Maya, ZBrush |
| **Animation** | Frame-by-Frame, Skeletal | Skeletal, Motion Capture |
| **Performance** | Generally lighter | Requires optimization (LODs) |
| **Best For** | Retro, indie, mobile games | Open-world, AAA games |

## 4. Tools Used in Game Modeling

| Tool | Purpose |
| --- | --- |
| **Blender** | 3D modeling, texturing, animation |
| **Maya** | 3D Modeling, character animation, rigging |
| **3ds Max** | Environment modeling |
| **ZBrush** | High-poly sculpting |
| **Substance Painter** | Texturing & material creation |
| **Photoshop** | 2D painting, texturing |
| **Aseprite** | Pixel art |
| **Spine** | 2D animation |

## 5. Summary
Modeling for games is a crucial part of game development, defining the visual identity and gameplay experience. Whether working with 2D sprites or 3D models, developers must optimize assets for performance, ensure stylistic consistency, and integrate them smoothly into game engines.
