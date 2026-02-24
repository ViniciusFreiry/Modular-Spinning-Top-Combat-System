# 🌀 Modular Spinning Top Combat System

A modular spinning top combat system built in Unity 2022.3.21f1.

This project implements customizable spinning tops composed of interchangeable parts that dynamically affect attributes such as attack, defense, balance, and stamina.

---

## 📌 Overview

This system is based on competitive spinning tops (similar in concept to arena-based top battles).

Each spinning top is composed of:

- Top Core
- Attack Module
- Defense Module
- Base Module

Each module contributes to the final attributes of the spinning top, enabling different builds and combat strategies.

---

## 🛠 Engine Version

- Unity 2022.3.21f1 (LTS)
- C#
- ScriptableObjects for data-driven architecture

---

## ⚙️ Core Features

- Modular spinning top assembly system
- ScriptableObject-based module definitions
- Dynamic attribute scaling
- Collision-based combat resolution
- Stamina depletion system
- Balance system affecting stability and impact
- Customization interface with inventory swapping
- Procedural layered map progression

---

## 🧠 System Architecture

### 1️⃣ Data Layer (ScriptableObjects)

Each spinning top module is defined using ScriptableObjects containing:

- Attack value
- Defense value
- Balance modifier
- Visual prefab reference
- Metadata for classification

This approach ensures scalability and easy content expansion.

---

### 2️⃣ Runtime Assembly System

The spinning top dynamically composes:

- Visual hierarchy
- Aggregated attributes (calculated from modules)
- Runtime combat state

Attributes are recalculated whenever modules are swapped.

---

### 3️⃣ Combat System

Combat is driven by collision logic:

- When two spinning tops collide, attack and defense values are evaluated.
- Stamina is reduced based on impact strength.
- Balance influences stability and outcome consistency.

The system is deterministic and designed to be easily testable.

---

### 4️⃣ Customization Interface

The customization scene allows:

- Displaying up to 6 modules per category
- Draging a module to swap with the active one
- Real-time visual updates
- Immediate stat recalculation

The UI structure is designed for future expansion.

---

## 🔍 QA-Oriented Testing Notes

Tested scenarios include:

- Attribute recalculation after module swap
- Extreme stat difference collisions
- Zero stamina condition
- Rapid consecutive impacts
- Inventory stress testing
- Invalid module references
- Map progression restriction validation

### Known Limitations

- Combat logic is deterministic but not frame-locked.

---

## 🧪 Performance Considerations

- Data-driven design minimizes runtime allocation.
- Attribute recalculation occurs only on state change.
- Visual assembly avoids unnecessary instantiation.

---

## 🚀 Future Improvements

- Animation-driven impact feedback
- Expanded map generation logic
- Build preset save/load system

---

## 📂 Project Structure
```
Assets/
 ├── Scripts/
 │    ├── SpinningTop/
 │    ├── Combat/
 │    ├── Inventory/
 │    ├── Map/
 │    └── UI/
 ├── ScriptableObjects/
 ├── Prefabs/
 ├── Scenes/
ProjectSettings/
Packages/
```
---

## 🎯 Learning Goals

This project explores:

- Modular system design
- Data-driven architecture
- Scalable combat logic
- Runtime stat aggregation
- Customization-driven gameplay systems
- Extensible gameplay frameworks

---

## 📜 License

This project is for educational and portfolio purposes.