# Ainalu

**A mystical card game inspired by Kazakh nomadic shamanic healing rituals.**  
Players perform sacred healing rituals across four seasonal loops: draw Disease cards representing pain and negative energy, then place Healing cards on a ritual grid. Manage the Shaman’s Hand to absorb and transform suffering, while balancing spirit power through color resonance, season cycles, cycle clashes, and toxic harmony—without letting power drop to zero.

**Playable WebGL Demo** ➜ [https://betapbo.itch.io/ainalu](https://betapbo.itch.io/ainalu)  
(Highly recommended to play it first! The page includes full gameplay trailer, animated GIFs, screenshots, and detailed rules.)

Submitted to GMTK Game Jam 2025.

## About This Repository
This repository contains the **Unity source code** (programming-focused) for Ainalu.  
The project emphasizes clean C# implementation of core gameplay mechanics.

## Repository Scope
This repository focuses on gameplay programming and system design.  
For clarity and review purposes, only core scripts, scenes, and project settings are included.  
The playable build is available on itch.io.


### My Contributions (Programming)
- Implemented full core gameplay loop:
  - Grid-based card placement system
  - Disease and Healing card drawing/handling
  - Shaman’s Hand management and spirit power tracking
  - Color resonance scoring (+2 to +4 bonuses)
  - Season cycle bonuses and progression across four loops
  - Cycle Clash penalties for conflicting colors (Red↔Green, Black↔White)
  - Toxic Harmony multipliers/penalties
- UI systems: Menus, in-game HUD, ritual feedback, and scoring display
- Ritual loop progression and difficulty scaling
- WebGL build optimizations and multi-platform support (HTML5/Windows/macOS)

### Technical Highlights
- Built in **Unity 6000.1.7f1**
- 100% C# with clean, commented, modular architecture
- Key systems located in `Assets/Scripts/` (card management, grid logic, scoring, season controller, etc.)
- Dependencies managed via `Packages/manifest.json` (e.g., DOTween, TextMeshPro)

## Project Structure
- Assets/Scripts/     # Core gameplay logic and systems (main focus)
- Assets/Scenes/      # Game scenes
- ProjectSettings/ # Unity project settings
- Packages/manifest.json     # Package dependencies
- .gitignore       # Ignores large/generated files
## How to Open (Optional – for reviewers)
1. Clone this repository
2. Open the folder in Unity Hub
3. Use **Unity 6000.1.7f1** (recommended for full compatibility)
4. Load the main scene from `Assets/Scenes/`
5. Press Play in the Editor

> Note: Media assets (audio, images, animations, videos) are omitted to keep the repository lightweight and focused on code.  
> The complete visual and audio experience is available in the itch.io demo.

## Contact & More
- GitHub: [@RuiqingCHEN](https://github.com/RuiqingCHEN)
- itch.io: [betapbo](https://betapbo.itch.io)
- Game Jam Entry: [GMTK 2025 Rating Page](https://itch.io/jam/gmtk-2025/rate/3783698)

Thanks for checking out the code! Feel free to explore the Scripts folder or reach out if you'd like to discuss the mechanics, architecture, or optimizations. 
