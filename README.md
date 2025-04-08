# Kyra Simulation

**Kyra** is an experimental terminal-based artificial life simulation.  
It's a simple ASCII world where digital organisms try to survive, replicate, and die with meaning. Or at least with color.

![Simulation Preview](screenshots/kyra-grid-01.png)

---

## 🧠 What Is This?

Kyra simulates primitive digital organisms navigating a 2D world. Each Kyra has simple instincts: gather energy, replicate when strong, and leave behind tombs when they die.

This is an early-stage project—Kyra is still learning to crawl. It doesn’t aim to replicate biology, but plays with ideas of self-replication, decay, and emergence.

---

## ✨ Features (so far)

- Simple AI: Kyras move, replicate, and perish based on energy
- Tombs retain the total lifetime energy gathered by the deceased
- Other Kyras can scavenge tombs for survival
- Fully energy-populated grid (or random placement)
- Live terminal interface with colored rendering
- Logs printed in a secondary terminal window (on Windows) or to file

---

## 🖼 Screenshots

| Simulation | Log View | Lineage Tree |
|------------|-----------|---------------|
| ![Grid](screenshots/kyra-grid-01.png) | ![Logs](screenshots/kyra-logs.png) | ![Tree](screenshots/kyra-tree.png) |

---

## 📦 Run It

bash
dotnet run

---

## 💡 Inspiration

Kyra started as a curiosity: could digital life emerge from very simple rules? Inspired by cellular automata and artificial life experiments, this simulation explores the tension between order and entropy, inheritance and extinction, movement and meaning.

---

## 🧰 Tech Stack

- **Language:** C# (.NET 7+)
- **Environment:** Terminal (cross-platform)
- **Rendering:** ASCII-based, color-coded with `Console.ForegroundColor`
- **Logging:** Real-time logs to `log.txt`, viewed in external terminal (Windows) or tail (Linux/macOS)

---

## 🚧 Development Status

This is an evolving playground for digital life ideas. The simulation currently lacks:
- Long-term memory
- Mutation
- Adaptation
- Pathfinding
- Environmental change

It is expected to grow into something richer... or collapse beautifully.

---

## 🌱 Coming Soon / Ideas

- **Mutation Logic:** Energy transfer inefficiencies, ID corruption
- **Memory Systems:** Carrying ancestral data
- **Evolution Tree Visualizer:** Render lineage growth more visually
- **Environmental Challenges:** Scarcity, seasons, etc.
- **Sound Integration:** Kyra cries, ambient death hums (very optional)

---

## 💬 Feedback & Contributions

This project is still in its early stages. Feedback, pull requests, and philosophical debates are all welcome.