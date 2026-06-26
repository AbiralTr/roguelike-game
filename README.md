# roguelike-game

A passion project game developed between three friends — Griffin McCallum-Fite, Riley Sackett, and Abiral Tuladhar.

- **Genre:** 2D pixel-art platformer / roguelike / metroidvania
- **Setting:** Sci-fi

## Tech Stack

| Layer | Choice |
|---|---|
| Engine | Unity (LTS) |
| Language | C# |
| Render Pipeline | Universal Render Pipeline (URP) — 2D Renderer |
| Pixel art tooling | Aseprite |
| Version control | Git + GitHub, Git LFS for binaries |

**Unity version:** `6000.3.18f1 LTS`

## Getting Started

### Prerequisites

- Unity Hub + Unity installed
- Git
- [Git LFS](https://git-lfs.com/) (`git lfs install`, once per machine)

### Setup

```bash
git clone https://github.com/AbiralTr/roguelike-game.git
cd roguelike-game
git lfs pull
```

Open the project folder from Unity Hub (Add/Open → select the cloned `roguelike-game` folder).

### Unity smart-merge driver (one-time, per machine)

Avoids broken merges when two people edit the same scene/prefab:

```bash
git config merge.unityyamlmerge.name "Unity SmartMerge"
git config merge.unityyamlmerge.driver "'<path-to-UnityYAMLMerge-binary>' merge -p %O %B %A %A"
```

- Windows: `.../Editor/Data/Tools/UnityYAMLMerge.exe`
- macOS: `.../Unity.app/Contents/Tools/UnityYAMLMerge`

## Workflow

- Branch per feature off `main`, open a PR when ready
- PRs require approval before merging

## License

MIT — see [LICENSE](LICENSE).