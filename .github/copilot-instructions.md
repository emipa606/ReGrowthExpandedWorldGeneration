# GitHub Copilot Instructions for ReGrowth: Expanded World Generation (Continued)

## Mod Overview and Purpose

ReGrowth: Expanded World Generation is a standalone mod that allows players to customize the world generation of RimWorld. It provides extensive control over various aspects such as global coverage, river density, mountain density, sea level, axial tilt, road density, biomes, factions, and presets. The mod aims to give users the flexibility to create unique and tailored planets for their RimWorld games.

## Key Features and Systems

### Global Coverage
A slider that allows users to choose the percentage of the world that should be generated.

### River Density
A control to adjust the number of rivers in the generated world.

### Mountain Density
A setting to control the number of mountains in the world.

### Sea Level
Allows users to set the sea level to rise or fall as desired.

### Axial Tilt
A feature that works exactly like in Realistic Planets, allowing for extreme seasonal temperature shifts.

### Road Density
Controls the number of ancient and faction roads in the world.

### Biomes
Provides detailed controls over each biome, including vanilla and modded ones. Users can set the spawn value using multiplier and offset sliders.

### Factions
No changes have been made to faction settings; users can use the existing options provided by RimWorld.

### Presets
Enables users to save and load world generation presets, similar to EdB Prepare Carefully.

### Planet Preview
A preview feature to visualize the generated planet with the current settings.

## Coding Patterns and Conventions

### C# Code
- **Classes and Methods**: The C# code follows a structured approach with clear class and method names.
- **ModCompat.cs**: Contains harmony patches to ensure compatibility with other mods.
- **Dialog_PresetList.cs**: Handles the logic for preset list interactions and window contents.
- **Dialog_PresetList_Load.cs and Dialog_PresetList_Save.cs**: Manage the loading and saving of presets.

### XML Integration
- **About.xml**: Contains metadata about the mod, including dependencies.

### Harmony Patching
- **ModCompat.cs**: Contains methods for patching other mods to ensure compatibility.

## Suggestions for Copilot

### General Recommendations
- **Documentation**: Ensure that all code is well-documented, including method and class purposes, parameters, and return values.
- **Testing**: Encourage thorough testing of any changes or additions to ensure compatibility and functionality.
- **Code Review**: Implement a code review process to maintain code quality and consistency.

### Specific Recommendations
- **Compatibility**: Focus on maintaining compatibility with other popular mods like Realistic Planets and My Little Planet.
- **Performance**: Optimize performance, especially for the planet preview feature, to ensure smooth gameplay.
- **User Interface**: Ensure the UI scales correctly and is user-friendly, with intuitive controls and feedback.

### Example Scenarios

#### Scenario 1: Adding a New Biome Control
1. **XML Definition**: Define the new biome in the XML file.
2. **C# Implementation**: Create a new class or modify existing ones to handle the new biome control logic.
3. **Harmony Patching**: Ensure compatibility with other mods by using harmony patches.

#### Scenario 2: Enhancing Planet Preview Performance
1. **Optimization**: Optimize the preview generation algorithm to reduce processing time.
2. **Testing**: Test the changes in various scenarios to ensure performance improvements without affecting functionality.

#### Scenario 3: Adding a New Faction Setting
1. **XML Definition**: Define the new faction setting in the XML file.
2. **C# Implementation**: Create a new class or modify existing ones to handle the new faction setting logic.
3. **UI Integration**: Integrate the new setting into the user interface.

By following these guidelines, you can ensure that the ReGrowth: Expanded World Generation mod remains a powerful and flexible tool for RimWorld players, while also maintaining compatibility with other mods and ensuring a smooth and enjoyable gameplay experience.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

