# 📖 WasiCore Development Guide

> 🗺️ **Development Guide Navigation** - Quickly find development documents and system architecture explanations.

## 🚀 Quick Start

### ⚡ Get Started Now (Recommended)
- **[🤖 AI Developer Guide](../AI_DEVELOPER_GUIDE.md)** - ⭐ **A one-stop guide for AI programming**
  - **Intent-Based API Finder**: Quickly locate functionalities based on your development intent.
  - **Core Architecture Concepts**: Understand the design philosophy behind WasiCore.
  - **Complete Code Patterns**: 7 major programming patterns ready to copy and use.

### Essential Reading for Newcomers
- [⚡ Quick Start](QuickStart.md) - Get up and running with the WasiCore framework in 5 minutes.
- [📋 Framework Overview](FRAMEWORK_OVERVIEW.md) - Understand the overall architectural design.
- [📖 Project Structure](ProjectStructure.md) - Learn how the project is organized.
- [⚠️ Common Pitfalls](best-practices/CommonPitfalls.md) - Avoid common development traps.

### 🤖 AI Development Zone
- [🤖 AI Development Guide](AI_DEVELOPMENT_GUIDE.md) - A guide to developing AI systems.
- [🎨 AI-Friendly UI API](AI_FRIENDLY_UI_API.md) - The flow layout API.

## 🎯 Core Documentation

### System Architecture (20 Systems)
> Dive deep into the various subsystems of WasiCore.

#### 🏗️ Core Systems
- [🎯 GameData System](systems/GameDataSystem.md) - **(Must Read)** The core of the framework's data-driven architecture.
- [🎮 Unit System](systems/UnitSystem.md) - **(Must Read)** Game unit management and synchronization.
- [⚡ Ability System](systems/AbilitySystem.md) - Ability configuration and casting mechanics.
- [🌟 Buff System](systems/BuffSystem.md) - Temporary effects and state management.
- [🎭 Actor System](systems/ActorSystem.md) - Visual representation layer objects.

#### 🎨 UI and Interaction
- [🖱️ Order System](systems/OrderSystem.md) - Player actions and AI behavior commands.
- [🎨 UI Property System](systems/UIPropertySystem.md) - UI data binding mechanism.
- [🔧 Trigger System](systems/TriggerSystem.md) - Event handling mechanism.

#### 📊 Data and Communication
- [📦 Item System](systems/ItemSystem.md) - Equipment and consumable management.
- [☁️ Cloud Data System](systems/CloudDataSystem.md) - Data synchronization and persistence.
- [💬 Messaging System](systems/MessagingSystem.md) - Network communication mechanism.
- [📝 Logging System](systems/LoggingSystem.md) - Unified logging.

#### 🧠 AI and Behavior
- [🤖 AI System](systems/AISystem.md) - NPC behavior and decision trees.
- [🤔 Thinker System](systems/ThinkerSystem.md) - AI thinking simulation.
- [📋 Unit Property System](systems/UnitPropertySystem.md) - Property management mechanism.

#### 🎬 Presentation and Effects
- [🎥 Model Animation System](systems/ModelAnimationSystem.md) - 3D animation control.
- [💥 Effect System](systems/EffectSystem.md) - Special effects and particle systems.
- [🎵 Audio System](systems/AudioSystem.md) - Sound effects and music management.
- [🏃 Player System](systems/PlayerSystem.md) - Player state management.

#### 🔧 Development Resources
- [⚠️ Development Best Practices](best-practices/CommonPitfalls.md) - Common issues and solutions.

### API Reference
- [🤖 AI Developer Guide](../AI_DEVELOPER_GUIDE.md) - The one-stop guide for AI programming.
- 🔗 [Client API Reference](../api-client-reference/) - Client-side API in XML format.
- 🔗 [Server API Reference](../api-server-reference/) - Server-side API in XML format.

## 💡 Best Practices

### Development Guides
- [🔄 Asynchronous Programming](best-practices/AsyncProgramming.md) - **(Important)** Async programming in a WebAssembly environment.
- [☁️ Cloud Data Best Practices](best-practices/CloudDataBestPractices.md) - Data synchronization and management.
- [⚠️ Common Pitfalls](best-practices/CommonPitfalls.md) - Avoid common development traps.

### Design Patterns
- [🧩 Entity-Component-Data Pattern](EntityComponentDataPattern.md) - Application of the ECS architecture.
- [🎭 Entity vs. Actor](EntityVsActor.md) - Understand the difference between the two object models.

### Specific Guides
- [☁️ Cloud Data Quick Start](CloudDataQuickStart.md) - Introduction to the cloud data feature.
- [🧪 Testing Guide](Testing.md) - Unit and integration testing.

## 📋 Navigation Tips

### 🔍 How do I quickly find the information I need?

1.  **AI Developers**: Go directly to the [🤖 AI Developer Guide](../AI_DEVELOPER_GUIDE.md) - ⭐ **The most efficient way to find what you need.**
2.  **Newcomers**: Read in this order → Quick Start → Framework Overview → Choose a system that interests you.
3.  **Specific Functionality**: Look at the corresponding system document.
4.  **Best Practices**: Check the `best-practices` directory.
5.  **Troubleshooting**: First, check `CommonPitfalls`, then look at the specific system documentation.

### 📚 Document Categories

- **Root directory documents**: Getting started tutorials and topic-specific guides.
- **systems/**: Detailed technical documents for each subsystem.
- **best-practices/**: Development best practices and lessons learned.


---

💡 **Tip for Newcomers**: It's recommended to read the [Quick Start](QuickStart.md) → [Framework Overview](FRAMEWORK_OVERVIEW.md) → and then choose a relevant system document.
