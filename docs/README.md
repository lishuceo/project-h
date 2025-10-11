# 📚 WasiCore API Documentation

> The complete API documentation and development guide for the **WasiCore Game Framework**, designed for modern game development and AI-assisted programming.

## ⚡ Quick Start

### 🤖 Core Documentation for AI Developers
- **[🚀 AI Developer Guide](AI_DEVELOPER_GUIDE.md)** - ⭐ **Condensed index guide for AI programming**
  - 📍 **Quick API Reference**: Essential APIs at a glance
  - 🎯 **Core Concepts**: The Data-Driven Trinity and Four Core Systems
  - 📚 **Pattern Library Index**: Links to 8 complete code patterns
  - 🔧 **Quick Task Guide**: Common tasks with solutions
- **[📋 AI Quick Rules](AI_QUICK_RULES.md)** - Essential rules that AI agents MUST follow
- **[💡 Code Patterns](patterns/)** - 8 complete programming patterns with full examples

### 📖 Essential Reading for Developers
- [🚀 5-Minute Quick Start](dev-guide/QuickStart.md) - Get started with WasiCore immediately.
- [📋 Framework Overview](dev-guide/FRAMEWORK_OVERVIEW.md) - Understand the design principles.
- [📖 Development Guides](dev-guide/) - Entry point to all development documentation.

## 📁 Directory Structure

```
wasicore-api-docs/
├── 🤖 AI_DEVELOPER_GUIDE.md      # Condensed index guide for AI programming
├── 📋 AI_QUICK_RULES.md          # Essential rules for AI agents
├── 💡 patterns/                   # Complete code patterns
│   ├── Pattern00_ReferenceExisting.md # 🌟 ALWAYS check existing examples first!
│   ├── Pattern01_SystemInit.md   # Game system initialization
│   ├── Pattern02_DataDriven.md   # Data-driven object creation
│   ├── Pattern03_FluentUI.md     # Fluent UI building
│   ├── Pattern04_Events.md       # Event-driven game logic
│   ├── Pattern05_Async.md        # Async programming (WebAssembly-safe)
│   ├── Pattern06_SceneCreation.md # Scene creation with shapes
│   ├── Pattern07_ErrorHandling.md # Error handling and debugging
│   └── Pattern08_Physics.md      # Physics system (client-only)
├── 📖 dev-guide/                  # Development Guides
│   ├── QuickStart.md             # Quick start tutorial
│   ├── ProjectStructure.md       # Project structure explanation
│   ├── AI_DEVELOPMENT_GUIDE.md   # AI Development Guide
│   ├── AI_FRIENDLY_UI_API.md     # AI-Friendly UI API
│   ├── Testing.md                # Testing guide
│   ├── CloudDataQuickStart.md    # Cloud data quick start
│   ├── EntityComponentDataPattern.md  # ECS pattern guide
│   ├── EntityVsActor.md          # Entity vs Actor concepts
│   ├── 💡 best-practices/         # Best Practices
│   │   ├── AsyncProgramming.md   # Best practices for asynchronous programming
│   │   ├── CloudDataBestPractices.md # Best practices for cloud data
│   │   └── CommonPitfalls.md     # Common pitfalls
│   ├── 🏗️ systems/                # System Architecture Documents (20 systems)
│   │   ├── GameDataSystem.md     # Data-Driven System
│   │   ├── UnitSystem.md         # Unit System
│   │   ├── AbilitySystem.md      # Ability System
│   │   ├── UIPropertySystem.md   # UI Property System
│   │   └── ...                   # Other 16 systems
│   └── FRAMEWORK_OVERVIEW.md     # Framework Architecture Overview
├── 📚 api-client-reference/        # Client API Reference (XML Documentation)
│   ├── GameCore.xml              # Game Core API
│   ├── GameUI.xml                # UI System API
│   └── ...                       # Other modules
└── 🖥️ api-server-reference/        # Server API Reference (XML Documentation)
    ├── GameCore.xml              # Game Core API
    ├── Events.xml                # Event System API
    └── ...                       # Other modules
```

## 🚀 Quick Navigation

### ⚡ Quick Start (Recommended)
- **[🤖 AI Developer Guide](AI_DEVELOPER_GUIDE.md) - One-stop AI programming guide** - ⭐ **Designed for AI programming, find APIs by intent**
- [🚀 Quick Start](dev-guide/QuickStart.md) - Get up and running with WasiCore in 5 minutes
- [📋 Framework Overview](dev-guide/FRAMEWORK_OVERVIEW.md) - Introduction to the overall architecture
- [📖 Project Structure](dev-guide/ProjectStructure.md) - Understand how the project is organized
  
### 📚 API Reference
- [📱 Client API Reference](api-client-reference/) - XML documentation for client-side APIs
- [🖥️ Server API Reference](api-server-reference/) - XML documentation for server-side APIs

### 🎯 Core Documentation
- [🏗️ System Architecture](dev-guide/systems/) - Detailed explanations of 20 systems
- [💡 Best Practices](dev-guide/best-practices/) - Development experience and tips

### 🤖 AI Development Zone
- [🤖 AI Development Guide](dev-guide/AI_DEVELOPMENT_GUIDE.md) - Guide to developing AI systems
- [🎨 AI-Friendly UI API](dev-guide/AI_FRIENDLY_UI_API.md) - Flow layout API design

### 🛠️ Development Resources
- [⚠️ FAQ & Common Pitfalls](dev-guide/best-practices/CommonPitfalls.md) - Avoid common development traps


## 🎯 Documentation Features

### Client/Server Separation

This documentation system is specifically designed for the WasiCore framework's client/server separated architecture:

- **Client API** - Contains classes and interfaces relevant to the client.
- **Server API** - Contains classes and interfaces relevant to the server.

### Build Configuration Support

Supports all build configurations of the framework:

#### Client Configurations
- `Client-Debug` - Client debug build (default for documentation generation)
- `Client-Release` - Client release build

#### Server Configurations
- `Server-Debug` - Server debug build (default for documentation generation)
- `Server-Release` - Server release build

## 📊 Project Statistics

- **📁 20 System Documents**: Covers all core systems of WasiCore
- **🤖 1 Condensed AI Guide**: Quick index guide with links to patterns (< 4KB)
- **💡 9 Code Pattern Files**: Complete programming patterns with examples (Pattern 0 is crucial!)
- **📋 2 Rule Documents**: AI Quick Rules and main guide
- **📚 XML API Reference**: Complete C# standard documentation format
- **🎯 Optimized for AI**: Condensed main guide prevents token overflow, patterns loaded on demand

---

*WasiCore API Documentation - Designed for modern game development and AI programming | For questions or suggestions, please contact the development team.*
