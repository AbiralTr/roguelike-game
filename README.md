# roguelike-game

A passion project game developed between three friends — Griffin McCallum-Fite, Riley Sackett, and Abiral Tuladhar.

- **Genre:** 2D pixel-art platformer / roguelike / metroidvania
- **Setting:** Sci-fi

## Tech Stack

| Layer | Choice |
|---|---|
| Engine | Godot 4.7 |
| Language | GDScript |
| Pixel art tooling | Aseprite |
| Version control | Git + GitHub |

## Getting Started

### Prerequisites

- Godot 4.7 (or later 4.x)
- Git

### Setup

```bash
git clone https://github.com/AbiralTr/roguelike-game.git
cd roguelike-game
```

Open Godot, choose **Import**, and select the cloned `roguelike-game` folder (`project.godot` lives at the repo root). Run the game with F5, or open `scenes/Main.tscn` and run just that scene with F6.

## Controls

| Action | Key/Button |
|---|---|
| Move | A / D |
| Jump | W |
| Dash | Space |
| Melee attack | Left click |
| Ranged attack | Right click |
| Interact / pick up weapon | E |
| Pause | Escape |
| Stat menu | Tab |

## Project Layout

```
project.godot        Godot project file (repo root is the project root)
scenes/               .tscn scenes, including scenes/UI for menus/HUD
scripts/              game logic (.gd), scripts/ui and scripts/weapons subfolders
resources/            .tres data assets (stats, weapons, sprite animation sets)
assets/sprites/       game art
tools/                standalone dev scripts (run via Godot's --script flag)
```

## Workflow

- Branch per feature off `main`, open a PR when ready
- PRs require approval before merging

## History

Originally prototyped in Unity; rebuilt in Godot starting August 2026.
